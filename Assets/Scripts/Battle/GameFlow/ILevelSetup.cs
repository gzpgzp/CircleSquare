using GameFramework;

namespace Battle.GameFlow
{
    /// <summary>
    /// 关卡初始化策略接口。
    /// 每种游戏模式实现一个，由 LevelManager 持有并驱动。
    /// </summary>
    public interface ILevelSetup
    {
        /// <summary>创建地图、角色等初始内容</summary>
        void CreateLevel(GameContext ctx, CharacterManager charMgr);

        /// <summary>每帧驱动（可选，如关卡计时、刷怪逻辑等）</summary>
        void Tick(float dt);

        /// <summary>重置/重玩</summary>
        void Restart();
    }
}
