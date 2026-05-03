#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 编辑器工具：一键生成 SquareBattle_Environment 预制体。
/// 菜单：Tools → SquareBattle → Create Environment Prefab
///
/// 生成结构：
///   SquareBattle_Environment
///   ├── Background      (纯色背景 Quad，无碰撞)
///   ├── Wall_Top        (Tag=Wall, BoxCollider2D)
///   ├── Wall_Bottom     (Tag=Wall, BoxCollider2D)
///   ├── Wall_Left       (Tag=Wall, BoxCollider2D)
///   └── Wall_Right      (Tag=Wall, BoxCollider2D)
/// </summary>
public static class SquareBattleMapCreator
{
    // ── 场地参数（可按需修改）────────────────────────────────────
    private const float AreaHalfW  = 9f;    // 半宽（总宽 18 单位）
    private const float AreaHalfH  = 5f;    // 半高（总高 10 单位）
    private const float WallThick  = 0.5f;  // 墙壁厚度

    // 预制体保存路径（Resources 目录下，LoadAndInstantiate 可直接使用）
    private const string SavePath  = "Assets/Resources/Prefabs/Maps/SquareBattle_Environment.prefab";

    [MenuItem("Tools/SquareBattle/Create Environment Prefab")]
    public static void CreateEnvironmentPrefab()
    {
        // ── 根节点 ───────────────────────────────────────────────
        var root = new GameObject("SquareBattle_Environment");

        // ── 背景 ────────────────────────────────────────────────
        var bg = CreateBackground(root.transform);

        // ── 四面墙 ──────────────────────────────────────────────
        CreateWall(root.transform, "Wall_Top",
            new Vector3(0f,  AreaHalfH + WallThick * 0.5f, 0f),
            new Vector2(AreaHalfW * 2f + WallThick * 2f, WallThick));

        CreateWall(root.transform, "Wall_Bottom",
            new Vector3(0f, -AreaHalfH - WallThick * 0.5f, 0f),
            new Vector2(AreaHalfW * 2f + WallThick * 2f, WallThick));

        CreateWall(root.transform, "Wall_Left",
            new Vector3(-AreaHalfW - WallThick * 0.5f, 0f, 0f),
            new Vector2(WallThick, AreaHalfH * 2f));

        CreateWall(root.transform, "Wall_Right",
            new Vector3( AreaHalfW + WallThick * 0.5f, 0f, 0f),
            new Vector2(WallThick, AreaHalfH * 2f));

        // ── 保存为预制体 ─────────────────────────────────────────
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, SavePath);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Debug.Log($"[SquareBattleMapCreator] 预制体已生成：{SavePath}");
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
    }

    // ── 背景：纯色 Sprite（深蓝灰）────────────────────────────────
    private static GameObject CreateBackground(Transform parent)
    {
        var go = new GameObject("Background");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite   = CreateWhiteSquareSprite();
        sr.color    = new Color(0.12f, 0.14f, 0.18f);   // 深蓝灰
        sr.sortingOrder = -10;

        go.transform.localScale = new Vector3(AreaHalfW * 2f, AreaHalfH * 2f, 1f);
        return go;
    }

    // ── 墙壁：Tag=Wall + BoxCollider2D + SpriteRenderer ───────────
    private static GameObject CreateWall(Transform parent, string wallName,
        Vector3 localPos, Vector2 size)
    {
        var go = new GameObject(wallName);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;

        // Tag（需要提前在 TagManager 里存在 "Wall"）
        go.tag = "Wall";

        // 碰撞体
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;

        // 外观
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = CreateWhiteSquareSprite();
        sr.color        = new Color(0.25f, 0.28f, 0.35f);  // 稍亮的墙色
        sr.sortingOrder = -5;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        return go;
    }

    // ── 生成 1×1 白色 Sprite（运行时不依赖任何贴图资源）────────────
    private static Sprite CreateWhiteSquareSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f);
    }
}
#endif
