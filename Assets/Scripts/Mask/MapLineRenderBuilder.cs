using System.Collections.Generic;
using UnityEngine;

namespace Mask
{
    [ExecuteAlways] // 编辑器模式可用
    public class MapLineRendererBuilder : MonoBehaviour
    {
        [Header("Line Renderer")]
        public LineRenderer lineRenderer;

        [Header("Map Points (Drag & Drop in Scene)")]
        public List<Transform> mapPoints = new List<Transform>();

        [Header("Settings")]
        public bool loop = false; // 是否闭合地图

        [Header("Edge Collider (Optional)")]
        public bool addEdgeCollider = true;

        /// <summary>
        /// 生成地图（点击按钮调用）
        /// </summary>
        [ContextMenu("Generate Map Line")]
        public void GenerateMapLine()
        {
            if (lineRenderer == null)
            {
                Debug.LogWarning("请先拖入 LineRenderer!");
                return;
            }

            if (mapPoints.Count == 0)
            {
                Debug.LogWarning("请先拖入 Map Points!");
                return;
            }

            // 设置 LineRenderer 点
            int count = mapPoints.Count;
            lineRenderer.positionCount = loop ? count + 1 : count;

            Vector2[] colliderPoints = new Vector2[loop ? count + 1 : count];

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = mapPoints[i].position;
                lineRenderer.SetPosition(i, pos);
                colliderPoints[i] = pos;
            }

            if (loop)
            {
                lineRenderer.SetPosition(count, mapPoints[0].position);
                colliderPoints[count] = mapPoints[0].position;
            }

            // EdgeCollider2D
            if (addEdgeCollider)
            {
                EdgeCollider2D edge = GetComponent<EdgeCollider2D>();
                if (edge == null) edge = gameObject.AddComponent<EdgeCollider2D>();
                edge.points = colliderPoints;
            }

            Debug.Log("Map Line Generated!");
        }
    }
}