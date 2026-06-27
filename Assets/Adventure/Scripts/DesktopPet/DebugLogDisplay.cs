using UnityEngine;

namespace Adventure.DesktopPet
{
    /// <summary>
    /// 调试日志显示 - 在Build版本中显示日志到屏幕上
    /// </summary>
    public class DebugLogDisplay : MonoBehaviour
    {
        [SerializeField] private Vector2 logPosition = new Vector2(10, 10);
        [SerializeField] private int maxLines = 20;

        private System.Collections.Generic.List<string> logMessages = new System.Collections.Generic.List<string>();

        private void Awake()
        {
            // 注册日志回调
            Application.logMessageReceived += HandleLog;
            
            // 测试日志
            Debug.Log("[DebugLogDisplay] 日志系统已启动");
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            logMessages.Add(logString);
            
            // 限制日志行数
            if (logMessages.Count > maxLines)
            {
                logMessages.RemoveAt(0);
            }
        }

        private void OnGUI()
        {
            GUI.color = Color.white;
            float y = logPosition.y;
            
            foreach (string msg in logMessages)
            {
                GUI.Label(new Rect(logPosition.x, y, 800, 20), msg);
                y += 20;
            }
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= HandleLog;
        }
    }
}
