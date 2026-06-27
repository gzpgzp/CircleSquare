using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// 调试工具窗口 - 通过菜单 Tools/Debug Tools 打开
    /// </summary>
    public class DebugToolsWindow : EditorWindow
    {
        [MenuItem("Tools/Debug Tools")]
        public static void ShowWindow()
        {
            GetWindow<DebugToolsWindow>("调试工具");
        }

        private void OnGUI()
        {
            GUILayout.Label("存档管理", EditorStyles.boldLabel);
            GUILayout.Space(5);

            // 清除所有存档
            GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
            if (GUILayout.Button("清除所有本地数据", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("确认清除",
                        "将清除以下数据：\n• SaveManager 文件存档\n• PlayerPrefs 所有数据\n\n此操作不可撤销！",
                        "确认清除", "取消"))
                {
                    ClearAllData();
                }
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            // 单独清除文件存档
            if (GUILayout.Button("仅清除文件存档 (save.json)", GUILayout.Height(25)))
            {
                ClearFileSave();
            }

            // 单独清除 PlayerPrefs
            if (GUILayout.Button("仅清除 PlayerPrefs", GUILayout.Height(25)))
            {
                ClearPlayerPrefs();
            }

            GUILayout.Space(15);
            GUILayout.Label("存档路径", EditorStyles.boldLabel);
            GUILayout.Space(5);

            string savePath = Application.persistentDataPath + "/Saves/save.json";
            EditorGUILayout.TextField("Save File:", savePath);

            bool exists = File.Exists(savePath);
            EditorGUILayout.LabelField("状态:", exists ? "存档存在 ✓" : "无存档");

            GUILayout.Space(5);
            if (GUILayout.Button("打开存档目录"))
            {
                string folder = Application.persistentDataPath + "/Saves";
                if (Directory.Exists(folder))
                    EditorUtility.RevealInFinder(folder);
                else
                    EditorUtility.RevealInFinder(Application.persistentDataPath);
            }
        }

        private static void ClearAllData()
        {
            ClearFileSave();
            ClearPlayerPrefs();
            Debug.Log("[DebugTools] ✅ 所有本地数据已清除！");
        }

        private static void ClearFileSave()
        {
            string savePath = Application.persistentDataPath + "/Saves/save.json";
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("[DebugTools] 文件存档已删除: " + savePath);
            }
            else
            {
                Debug.Log("[DebugTools] 无文件存档需要删除");
            }
        }

        private static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[DebugTools] PlayerPrefs 已清除");
        }
    }
}

