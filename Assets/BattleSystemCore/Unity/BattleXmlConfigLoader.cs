using System;
using System.IO;
using BattleSystemCore.Logic.Buffs;
using BattleSystemCore.Logic.Projectiles;
using BattleSystemCore.Logic.Skills;
using UnityEngine;

namespace BattleSystemCore.Unity
{
    /// <summary>
    /// 战斗配置装载器：StreamingAssets/BattleCore/battlecore.xml → 运行时对象 →
    /// BuffLibrary/SkillLibrary/ProjectileLibrary。
    /// 配置源只在此处出现一次，战斗运行时对XML零感知（换配置源=换本类，核心不动）。
    /// 解析与校验失败抛异常（配置错误尽早炸）；内部Clear+Register全量，可重复调用即热重载。
    /// </summary>
    public static class BattleXmlConfigLoader
    {
        /// <summary>配置文件相对 StreamingAssets 的路径</summary>
        public const string RelativePath = "BattleCore/battlecore.xml";

        /// <summary>配置文件绝对路径（编辑器与桌面平台可直接 File IO）</summary>
        public static string FilePath => Path.Combine(Application.streamingAssetsPath, RelativePath);

        /// <summary>从XML读取全部战斗配置并装载进三个Library</summary>
        public static void Load()
        {
            string path = FilePath;
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"BattleXmlConfigLoader: 找不到战斗配置文件 {path}", path);
            }

            BattleConfigData data = BattleXmlCodec.Parse(File.ReadAllText(path));

            var errors = BattleXmlCodec.Validate(data);
            if (errors.Count > 0)
            {
                throw new InvalidDataException(
                    $"BattleXmlConfigLoader: {path} 校验失败（{errors.Count} 项）：\n" +
                    string.Join("\n", errors));
            }

            BuffLibrary.Clear();
            for (int i = 0; i < data.buffs.Count; i++)
            {
                BuffLibrary.Register(data.buffs[i]);
            }

            SkillLibrary.Clear();
            for (int i = 0; i < data.skills.Count; i++)
            {
                SkillLibrary.Register(data.skills[i]);
            }

            ProjectileLibrary.Clear();
            for (int i = 0; i < data.projectiles.Count; i++)
            {
                ProjectileLibrary.Register(data.projectiles[i]);
            }
        }
    }
}
