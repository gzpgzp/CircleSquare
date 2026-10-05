using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Adventure.Character
{
    /// <summary>
    /// 状态机调试工具 - 仅在Editor模式下生效
    /// 支持快捷键切换状态，Inspector显示当前状态
    /// </summary>
    public class StateMachineDebugger : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private CharacterStateMachine stateMachine;
        [SerializeField] private CharacterModelLoader modelLoader;

        [Header("快捷键配置")]
        [Tooltip("切换到Idle状态的快捷键")]
        [SerializeField] private KeyCode idleKey = KeyCode.Alpha1;
        [Tooltip("切换到Moving状态的快捷键")]
        [SerializeField] private KeyCode movingKey = KeyCode.Alpha2;
        [Tooltip("切换到Dancing状态的快捷键")]
        [SerializeField] private KeyCode dancingKey = KeyCode.Alpha3;
        [Tooltip("切换到Fighting状态的快捷键")]
        [SerializeField] private KeyCode fightingKey = KeyCode.Alpha4;

        [Header("调试选项")]
        [Tooltip("是否在Game视图显示调试信息")]
        [SerializeField] private bool showDebugInfo = true;
        [Tooltip("是否启用快捷键")]
        [SerializeField] private bool enableHotkeys = true;

        [Header("Moving调试参数")]
        [Tooltip("切换到Moving时，移动的距离")]
        [SerializeField] private float debugMoveDistance = 3f;

        [Header("方向快捷键")]
        [SerializeField] private KeyCode moveUpKey = KeyCode.UpArrow;
        [SerializeField] private KeyCode moveDownKey = KeyCode.DownArrow;
        [SerializeField] private KeyCode moveLeftKey = KeyCode.LeftArrow;
        [SerializeField] private KeyCode moveRightKey = KeyCode.RightArrow;

        /// <summary>当前状态（只读显示）</summary>
        public string CurrentStateName => stateMachine != null ? stateMachine.CurrentStateType.ToString() : "None";

        private void Awake()
        {
            if (stateMachine == null)
                stateMachine = GetComponent<CharacterStateMachine>();
            if (modelLoader == null)
                modelLoader = GetComponent<CharacterModelLoader>();
        }

        private void Update()
        {
            if (stateMachine == null) return;

#if UNITY_EDITOR
            // 快捷键切换状态
            if (enableHotkeys)
            {
                if (Input.GetKeyDown(idleKey))
                    ChangeStateDebug(CharacterStateType.Idle);
                else if (Input.GetKeyDown(dancingKey))
                    ChangeStateDebug(CharacterStateType.Dancing);
                else if (Input.GetKeyDown(fightingKey))
                    ChangeStateDebug(CharacterStateType.Fighting);
                
                // 方向键直接移动（固定四向）
                if (Input.GetKeyDown(moveUpKey))
                    MoveInDirection(Vector3.up);
                else if (Input.GetKeyDown(moveDownKey))
                    MoveInDirection(Vector3.down);
                else if (Input.GetKeyDown(moveLeftKey))
                    MoveInDirection(Vector3.left);
                else if (Input.GetKeyDown(moveRightKey))
                    MoveInDirection(Vector3.right);
            }
#endif
        }

        private void OnGUI()
        {
            if (!showDebugInfo || stateMachine == null) return;

#if UNITY_EDITOR
            GUILayout.BeginArea(new Rect(10, 10, 280, 150));
            GUILayout.Label("=== 状态机调试 ===");
            GUILayout.Label($"当前状态: {stateMachine.CurrentStateType}");
            GUILayout.Label($"状态切换:");
            GUILayout.Label($"  [{idleKey}] Idle  [{dancingKey}] Dancing  [{fightingKey}] Fighting");
            GUILayout.Label($"方向移动:");
            GUILayout.Label($"  [{moveUpKey}] 上  [{moveDownKey}] 下  [{moveLeftKey}] 左  [{moveRightKey}] 右");
            GUILayout.EndArea();
#endif
        }

        /// <summary>
        /// 切换状态（带调试日志）
        /// </summary>
        public void ChangeStateDebug(CharacterStateType state)
        {
            if (stateMachine == null)
            {
                Debug.LogWarning("[Debugger] 没有CharacterStateMachine组件！");
                return;
            }

            Debug.Log($"[Debugger] 手动切换状态 -> {state}");
            stateMachine.ChangeState(state);
        }

        /// <summary>
        /// 向指定方向移动（固定四向）
        /// </summary>
        public void MoveInDirection(Vector3 direction)
        {
            CharacterMover mover = stateMachine?.Mover;
            if (mover == null)
            {
                Debug.LogWarning("[Debugger] 没有CharacterMover组件！");
                return;
            }

            // 固定四向移动
            Vector3 currentPos = transform.position;
            Vector3 targetPos = currentPos + direction.normalized * debugMoveDistance;

            Debug.Log($"[Debugger] 移动: {direction} -> {targetPos}");
            mover.MoveTo(targetPos);
        }

        /// <summary>
        /// 切换角色模型
        /// </summary>
        public void SwitchModel(string modelPath)
        {
            if (modelLoader == null)
            {
                Debug.LogWarning("[Debugger] 没有CharacterModelLoader组件！");
                return;
            }

            Debug.Log($"[Debugger] 切换模型 -> {modelPath}");
            modelLoader.SwitchModel(modelPath);
        }

        /// <summary>
        /// 当前模型ID
        /// </summary>
        public string CurrentModelId => modelLoader != null ? modelLoader.CurrentModelId : "None";

        // Inspector按钮（Editor专用）
#if UNITY_EDITOR
        [CustomEditor(typeof(StateMachineDebugger))]
        public class StateMachineDebuggerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                StateMachineDebugger debugger = (StateMachineDebugger)target;

                if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox("调试按钮仅在运行模式下可用", MessageType.Info);
                    return;
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("=== 状态切换 ===", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Idle"))
                    debugger.ChangeStateDebug(CharacterStateType.Idle);
                if (GUILayout.Button("Dancing"))
                    debugger.ChangeStateDebug(CharacterStateType.Dancing);
                if (GUILayout.Button("Fighting"))
                    debugger.ChangeStateDebug(CharacterStateType.Fighting);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("=== 方向移动 ===", EditorStyles.boldLabel);

                // 方向按钮布局（模拟方向键）
                EditorGUILayout.BeginVertical();
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("↑ 上", GUILayout.Width(60)))
                    debugger.MoveInDirection(Vector3.up);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("← 左", GUILayout.Width(60)))
                    debugger.MoveInDirection(Vector3.left);
                GUILayout.Space(10);
                if (GUILayout.Button("→ 右", GUILayout.Width(60)))
                    debugger.MoveInDirection(Vector3.right);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("↓ 下", GUILayout.Width(60)))
                    debugger.MoveInDirection(Vector3.down);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                // 模型切换
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("=== 模型切换 ===", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox($"当前模型: {debugger.CurrentModelId}", MessageType.None);

                // 从Resources/Characters加载示例模型
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("角色A"))
                    debugger.SwitchModel("Characters/CharacterA");
                if (GUILayout.Button("角色B"))
                    debugger.SwitchModel("Characters/CharacterB");
                if (GUILayout.Button("角色C"))
                    debugger.SwitchModel("Characters/CharacterC");
                EditorGUILayout.EndHorizontal();

                // 刷新Inspector
                Repaint();
            }
        }
#endif
    }
}
