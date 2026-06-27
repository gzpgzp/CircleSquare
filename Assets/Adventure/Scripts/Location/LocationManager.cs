using Adventure.Character;
using Adventure.UI;
using NormalUI;
using Tools.Dialogs;
using Tools.Singletons;
using UnityEngine;

namespace Adventure.Location
{
    /// <summary>
    /// 地点管理器 - 处理地点点击后的功能路由
    /// </summary>
    public class LocationManager : Singleton<LocationManager>
    {
        /// <summary>
        /// 地点被点击时的统一入口
        /// </summary>
        public void OnLocationClicked(LocationPoint location)
        {
            switch ((LocationType)location.LocationType)
            {
                case LocationType.AdventurerGuild:
                    OpenAdventurerGuild();
                    break;
                case LocationType.BattleArea:
                    OpenBattleArea(location);
                    break;
                case LocationType.QuestBoard:
                    OpenQuestBoard();
                    break;
                case LocationType.Shop:
                    OpenShop();
                    break;
                case LocationType.Blacksmith:
                    OpenBlacksmith();
                    break;
                case LocationType.Tavern:
                    OpenTavern();
                    break;
                default:
                    Debug.LogWarning($"[LocationManager] Unhandled location type: {location.LocationType}");
                    break;
            }
        }

        /// <summary>
        /// 冒险者公会 - 角色列表/编队
        /// </summary>
        private void OpenAdventurerGuild()
        {
            Debug.Log("[LocationManager] Opening Adventurer Guild - Character List");
            OpenDialogEvent.Trigger("CharacterListDialog", new BaseUIDialogContext());
        }

        /// <summary>
        /// 战斗区域 - 显示敌人信息，开始/继续挂机
        /// </summary>
        private void OpenBattleArea(LocationPoint location)
        {
            Debug.Log($"[LocationManager] Opening Battle Area: {location.LocationName}");
            OpenDialogEvent.Trigger("BattleAreaDialog", new BattleAreaDialogContext
            {
                areaName = location.LocationName,
                areaDescription = location.Description
            });
        }

        /// <summary>
        /// 任务公告板 - 打开任务列表
        /// </summary>
        private void OpenQuestBoard()
        {
            Debug.Log("[LocationManager] Opening Quest Board");
            OpenDialogEvent.Trigger("QuestListDialog", new QuestListDialogContext());
        }

        /// <summary>
        /// 商店
        /// </summary>
        private void OpenShop()
        {
            Debug.Log("[LocationManager] Opening Shop (TODO)");
            // TODO: 实现商店弹窗
        }

        /// <summary>
        /// 铁匠铺
        /// </summary>
        private void OpenBlacksmith()
        {
            Debug.Log("[LocationManager] Opening Blacksmith (TODO)");
            // TODO: 实现装备强化弹窗
        }

        /// <summary>
        /// 酒馆
        /// </summary>
        private void OpenTavern()
        {
            Debug.Log("[LocationManager] Opening Tavern (TODO)");
            // TODO: 实现招募弹窗
        }

        /// <summary>
        /// 地点被右键点击时的统一入口
        /// </summary>
        public void OnLocationRightClicked(LocationPoint location)
        {
            Debug.Log($"[LocationManager] Right clicked: {location.LocationName} (Type:{location.LocationType})");

            // 打开地点信息/设置弹窗
            OpenDialogEvent.Trigger("LocationInfoDialog", new LocationInfoDialogContext
            {
                locationType = location.LocationType,
                locationName = location.LocationName,
                description = location.Description
            });
        }
    }

    /// <summary>
    /// 地点信息弹窗上下文（右键触发）
    /// </summary>
    public class LocationInfoDialogContext : BaseUIDialogContext
    {
        public int locationType;
        public string locationName;
        public string description;
    }

    /// <summary>
    /// 战斗区域弹窗上下文
    /// </summary>
    public class BattleAreaDialogContext : BaseUIDialogContext
    {
        public string areaName;
        public string areaDescription;
    }
}
