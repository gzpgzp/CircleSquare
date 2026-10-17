using System;
using System.Collections.Generic;

namespace BattleSystemCore.Logic.Projectiles
{
    /// <summary>
    /// 投射物静态定义注册表：projectileId→ProjectileData（照BuffLibrary/SkillLibrary模式）。
    /// 配置化链路（Luban Loader→Library→战斗运行时）的缓存层；
    /// SpawnBullet发射轨道经此解析弹体定义。同id不同定义视为配置错误（尽早炸）；
    /// 同引用幂等，供自检双跑、Demo重启安全复用。
    /// </summary>
    public static class ProjectileLibrary
    {
        private static readonly Dictionary<uint, ProjectileData> projectiles =
            new Dictionary<uint, ProjectileData>();

        /// <summary>注册投射物定义。同id不同定义抛异常，同引用幂等</summary>
        public static void Register(ProjectileData data)
        {
            if (data == null)
            {
                return;
            }

            if (projectiles.TryGetValue(data.projectileId, out var existing))
            {
                if (!ReferenceEquals(existing, data))
                {
                    throw new ArgumentException(
                        $"ProjectileLibrary: projectileId {data.projectileId}（{data.name}）重复注册为不同定义" +
                        $"（已注册：{existing.name}）");
                }

                return;
            }

            projectiles.Add(data.projectileId, data);
        }

        /// <summary>按id取投射物定义。未注册返回null（发射轨道自行空转）</summary>
        public static ProjectileData Get(uint projectileId)
        {
            projectiles.TryGetValue(projectileId, out var data);
            return data;
        }

        /// <summary>清空注册表（重载配置/重置测试环境）</summary>
        public static void Clear()
        {
            projectiles.Clear();
        }
    }
}
