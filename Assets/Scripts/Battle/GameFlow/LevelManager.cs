using GameFramework;

namespace Battle.GameFlow
{
    /// <summary>
    /// 关卡管理器。
    /// 持有一个 ILevelSetup 策略对象，由 GameManager 根据 GameMode 注入。
    /// 自身不包含任何硬编码的角色/地图创建逻辑。
    /// </summary>
    public class LevelManager
    {
        private ILevelSetup setup;

        /// <summary>由 GameManager 调用，注入具体策略并立即创建关卡</summary>
        public void CreateLevel(GameContext ctx, CharacterManager charMgr, ILevelSetup levelSetup)
        {
            setup = levelSetup;
            setup.CreateLevel(ctx, charMgr);
        }

        public void RestartLevel()
        {
            setup?.Restart();
        }

        public void Tick(float dt)
        {
            setup?.Tick(dt);
        }
    }
}
