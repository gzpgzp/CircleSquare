using UnityEngine;

namespace BattleSystemCore.Unity
{
    /// <summary>
    /// 技能形状攻击的贴地指示圈（纯表现，自生自灭）：
    /// 圆=LineRenderer圆环；扇形=三角扇面片（+z为平分线）；矩形=面片+描边框。
    /// 显示hold秒后向施法点收缩淡出（Opaque材质不做alpha，用缩放表达消散）。
    /// 由AttackShapeVTrack创建（也可作纯特效构建件，见BattleFxLibrary），milli→units换算在此完成。
    /// </summary>
    public class SkillCastIndicator : MonoBehaviour
    {
        private const float MilliToUnits = 1f / 1000f;
        private const float GroundY = 0.02f;
        private const int CircleSegments = 48;
        private const int FanSegments = 24;

        private float hold;
        private float fade;
        private float timer;

        /// <summary>构建指示圈。origin/forward为milli坐标；shapeType=1圆/2扇/3矩形</summary>
        public void Init(int shapeType, int radiusMilli, int minDot, int halfX, int halfZ,
            Vector2 originMilli, Vector2 forwardMilli, Color tint, float holdSeconds, float fadeSeconds)
        {
            hold = holdSeconds;
            fade = fadeSeconds;

            transform.position = new Vector3(originMilli.x * MilliToUnits, GroundY, originMilli.y * MilliToUnits);
            var forward = new Vector3(forwardMilli.x, 0f, forwardMilli.y);
            if (forward.sqrMagnitude > 1e-10f)
            {
                transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
            }

            switch (shapeType)
            {
                case 2:
                    BuildSector(radiusMilli * MilliToUnits, minDot, tint);
                    break;

                case 3:
                    BuildBox(halfX * MilliToUnits, halfZ * MilliToUnits, tint);
                    break;

                default:
                    BuildRing(radiusMilli * MilliToUnits, tint);
                    break;
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < hold)
            {
                return;
            }

            float t = (timer - hold) / fade;
            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // 向施法点收缩消散（几何均以本地原点为中心）
            float scale = Mathf.Max(0.15f, 1f - t);
            transform.localScale = Vector3.one * scale;
        }

        private void BuildRing(float radius, Color tint)
        {
            var ring = gameObject.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = CircleSegments;
            ring.startWidth = 0.06f;
            ring.endWidth = 0.06f;
            ring.material = MakeUnlit(tint);
            for (int i = 0; i < CircleSegments; i++)
            {
                float angle = i / (float)CircleSegments * Mathf.PI * 2f;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }
        }

        private void BuildSector(float radius, int minDot, Color tint)
        {
            // minDot为半角余弦万分比 → 半角弧度
            float halfAngle = Mathf.Acos(Mathf.Clamp(minDot / 10000f, -1f, 1f));
            var mesh = new Mesh();
            var vertices = new Vector3[FanSegments + 2];
            var triangles = new int[FanSegments * 3];
            vertices[0] = Vector3.zero;
            for (int i = 0; i <= FanSegments; i++)
            {
                float angle = Mathf.Lerp(-halfAngle, halfAngle, i / (float)FanSegments);
                vertices[i + 1] = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
            }

            for (int i = 0; i < FanSegments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 2;
                triangles[i * 3 + 2] = i + 1;
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            var filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var fillRenderer = gameObject.AddComponent<MeshRenderer>();
            fillRenderer.material = MakeUnlit(tint);
        }

        private void BuildBox(float halfX, float halfZ, Color tint)
        {
            var mesh = new Mesh();
            mesh.vertices = new[]
            {
                new Vector3(-halfX, 0f, -halfZ),
                new Vector3(halfX, 0f, -halfZ),
                new Vector3(halfX, 0f, halfZ),
                new Vector3(-halfX, 0f, halfZ),
            };
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            var filter = gameObject.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            var fillRenderer = gameObject.AddComponent<MeshRenderer>();
            fillRenderer.material = MakeUnlit(tint);

            var ring = gameObject.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = 4;
            ring.startWidth = 0.05f;
            ring.endWidth = 0.05f;
            ring.material = MakeUnlit(tint);
            ring.SetPosition(0, new Vector3(-halfX, 0.001f, -halfZ));
            ring.SetPosition(1, new Vector3(halfX, 0.001f, -halfZ));
            ring.SetPosition(2, new Vector3(halfX, 0.001f, halfZ));
            ring.SetPosition(3, new Vector3(-halfX, 0.001f, halfZ));
        }

        /// <summary>URP兼容的Opaque Unlit材质（Sprites/Default等内置shader在URP下会粉红）。视图层共用</summary>
        public static Material MakeUnlit(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            var mat = new Material(shader);
            mat.color = color;
            return mat;
        }
    }
}
