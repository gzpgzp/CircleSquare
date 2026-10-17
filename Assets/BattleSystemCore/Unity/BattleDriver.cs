using BattleSystemCore;
using UnityEngine;

namespace BattleSystemCore.Unity
{
    /// <summary>
    /// 战斗核心的Unity宿主驱动器：以固定逻辑帧率驱动纯C#的BattleSystem。
    /// Time.deltaTime只决定"何时推进"，不参与任何战斗逻辑，因此不影响确定性。
    /// 需要向战斗注入指令（施放技能等）时，通过System访问actionManager，在AdvanceTick之外调用。
    /// </summary>
    public class BattleDriver : MonoBehaviour
    {
        [Tooltip("逻辑帧率（每秒tick数）")]
        [SerializeField] private int ticksPerSecond = 60;

        [Tooltip("单渲染帧最多追进的逻辑帧数，防止低帧率时追帧死循环")]
        [SerializeField] private int maxTicksPerFrame = 5;

        private BattleSystem battleSystem;
        private float accumulator;

        /// <summary>本驱动器持有的战斗实例</summary>
        public BattleSystem System => battleSystem;

        private void Start()
        {
            battleSystem = new BattleSystem();
            battleSystem.Init();
        }

        private void Update()
        {
            if (battleSystem == null)
            {
                return;
            }

            accumulator += Time.deltaTime;
            float tickInterval = 1f / ticksPerSecond;
            int steps = 0;
            while (accumulator >= tickInterval && steps < maxTicksPerFrame)
            {
                battleSystem.AdvanceTick();
                accumulator -= tickInterval;
                steps++;
            }

            // 达到追帧上限说明积压过多，丢弃剩余时间避免追帧螺旋
            if (steps == maxTicksPerFrame)
            {
                accumulator = 0f;
            }
        }
    }
}
