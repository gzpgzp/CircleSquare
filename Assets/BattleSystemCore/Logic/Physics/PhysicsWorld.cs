namespace BattleSystemCore.Logic.Physics
{
    using BattleSystemCore.Logic.Actors;

    /// <summary>
    /// 战斗物理世界：tick驱动的单位间推挤分离与竞技场边界钳制。
    /// 无内部状态（配对记忆等），位置修正直接写Actor.pos——状态哈希天然覆盖，无需扩哈希字段。
    /// 命中判定不在本类（见HitTest纯函数库）；空间网格BroadPhase等unit上百再引入（当前O(N²)足够）。
    /// 时序：由BattleSystem.UpdateLogicPartB在全部移动（意图移动+位移轨道）之后调用，
    /// 保证每tick结束位置互不重叠；位移效果轨道推进的位置可能被推挤轻微修正（冲锋顶开路人，可接受的物理感）。
    /// </summary>
    public class PhysicsWorld
    {
        /// <summary>竞技场中心（milli）</summary>
        public Vec3 arenaCenter;

        /// <summary>竞技场半径（milli，水平圆）。≤0表示无边界钳制</summary>
        public int arenaRadius;

        public void Init(Vec3 center, int radiusMilli)
        {
            arenaCenter = center;
            arenaRadius = radiusMilli;
        }

        /// <summary>每tick末执行：先推挤分离，再边界钳制。顺序确定（依赖actors列表创建序）</summary>
        public void Update(int currTick)
        {
            ResolveOverlaps();
            ClampArena();
        }

        /// <summary>
        /// 单位推挤：两两检查水平重叠，沿连线各退重叠量的一半。
        /// 确定性：对(i&lt;j)按创建序遍历，分离量整除（余数留待下一tick收敛，无随机性）；
        /// 完全重合（dist=0）时以uid大小决定分离方向（稳定伪方向，uid小的向-x退）。
        /// bodyRadius≤0或已死亡的actor不参与。
        /// </summary>
        private void ResolveOverlaps()
        {
            var battle = BattleSystem.Current;
            if (battle == null)
            {
                return;
            }

            var actors = battle.actorManager.Actors;
            for (int i = 0; i < actors.Count; i++)
            {
                var a = actors[i];
                if (!IsPushable(a))
                {
                    continue;
                }

                for (int j = i + 1; j < actors.Count; j++)
                {
                    var b = actors[j];
                    if (!IsPushable(b))
                    {
                        continue;
                    }

                    int rSum = a.bodyRadius + b.bodyRadius;
                    if (rSum <= 0)
                    {
                        continue;
                    }

                    long dx = b.pos.x - a.pos.x;
                    long dz = b.pos.z - a.pos.z;
                    long distSq = dx * dx + dz * dz;
                    long rSumSq = (long)rSum * rSum;
                    if (distSq >= rSumSq)
                    {
                        continue;
                    }

                    if (distSq == 0)
                    {
                        // 完全重合：uid小的向-x退、大的向+x退，各退一半（确定且对称）
                        int half = rSum / 2;
                        if (a.actorContext.UID < b.actorContext.UID)
                        {
                            a.pos.x -= half;
                            b.pos.x += half;
                        }
                        else
                        {
                            a.pos.x += half;
                            b.pos.x -= half;
                        }

                        continue;
                    }

                    int dist = FixMath.ISqrt(distSq);
                    int overlap = rSum - dist;
                    int push = overlap / 2;
                    if (push <= 0)
                    {
                        continue;
                    }

                    // 各沿连线方向退push（位移=差值×push/dist，直接比例避免再归一化）
                    a.pos.x -= (int)(dx * push / dist);
                    a.pos.z -= (int)(dz * push / dist);
                    b.pos.x += (int)(dx * push / dist);
                    b.pos.z += (int)(dz * push / dist);
                }
            }
        }

        /// <summary>边界钳制：水平超出竞技场半径的点沿径向拉回圆周（y不动）。arenaRadius≤0时关闭</summary>
        private void ClampArena()
        {
            if (arenaRadius <= 0)
            {
                return;
            }

            var battle = BattleSystem.Current;
            var actors = battle.actorManager.Actors;
            long radiusSq = (long)arenaRadius * arenaRadius;
            for (int i = 0; i < actors.Count; i++)
            {
                var actor = actors[i];
                long dx = actor.pos.x - arenaCenter.x;
                long dz = actor.pos.z - arenaCenter.z;
                long distSq = dx * dx + dz * dz;
                if (distSq <= radiusSq)
                {
                    continue;
                }

                int dist = FixMath.ISqrt(distSq);
                if (dist == 0)
                {
                    continue;
                }

                actor.pos.x = arenaCenter.x + (int)(dx * arenaRadius / dist);
                actor.pos.z = arenaCenter.z + (int)(dz * arenaRadius / dist);
            }
        }

        private static bool IsPushable(Actor actor)
        {
            return actor != null
                && actor.bodyRadius > 0
                && actor.actorContext.hp > 0;
        }
    }
}
