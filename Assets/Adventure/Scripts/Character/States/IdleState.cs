using UnityEngine;
using System.Collections.Generic;
using Adventure.Location;

namespace Adventure.Character
{
    /// <summary>
    /// 待机状态 - 角色静止不动
    /// 可以触发多种随机行为：跳舞、战斗、随机移动、移动到建筑
    /// </summary>
    public class IdleState : CharacterState
    {
        [Tooltip("随机行为触发的最小等待时间（秒）")]
        private float minIdleTime = 3f;

        [Tooltip("随机行为触发的最大等待时间（秒）")]
        private float maxIdleTime = 8f;

        [Tooltip("触发跳舞行为的概率（0-1）")]
        private float danceProbability = 0.3f;

        [Tooltip("触发战斗行为的概率（0-1），0表示不会自动触发")]
        private float fightProbability = 0f;

        [Tooltip("触发随机移动的概率（0-1），0表示不会自动触发")]
        private float randomMoveProbability = 0.2f;

        [Tooltip("触发移动到建筑的概率（0-1），0表示不会自动触发")]
        private float moveToBuildingProbability = 0.1f;

        [Tooltip("随机移动时，距离角色的最大距离")]
        private float randomMoveRadius = 5f;

        private float currentIdleTime; // 本次进入Idle后随机生成的等待时间
        private float idleTimer;
        private bool hasTriggeredAction;

        public IdleState()
        {
            StateType = CharacterStateType.Idle;
        }

        public override void Enter()
        {
            base.Enter();
            
            // 随机生成本次的等待时间
            currentIdleTime = Random.Range(minIdleTime, maxIdleTime);
            idleTimer = 0;
            hasTriggeredAction = false;

            // 确保动画是Idle
            if (animatorCtrl != null)
            {
                animatorCtrl.SetDragging(false);
            }
        }

        public override void Execute()
        {
            // 如果角色正在移动，不计时
            if (mover != null && mover.isMoving)
            {
                idleTimer = 0;
                return;
            }

            // 如果还没触发过行为
            if (!hasTriggeredAction && currentIdleTime > 0)
            {
                idleTimer += Time.deltaTime;

                if (idleTimer >= currentIdleTime)
                {
                    hasTriggeredAction = true;
                    TryTriggerRandomAction();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            idleTimer = 0;
        }

        /// <summary>
        /// 根据概率权重随机触发行为
        /// </summary>
        private void TryTriggerRandomAction()
        {
            float totalProbability = danceProbability + fightProbability + randomMoveProbability + moveToBuildingProbability;
            
            // 没有任何行为概率，不触发
            if (totalProbability <= 0) return;

            float random = Random.Range(0, totalProbability);
            float cumulative = 0;

            // 跳舞
            cumulative += danceProbability;
            if (random < cumulative)
            {
                ChangeState(CharacterStateType.Dancing);
                return;
            }

            // 战斗
            cumulative += fightProbability;
            if (random < cumulative)
            {
                ChangeState(CharacterStateType.Fighting);
                return;
            }

            // 随机移动
            cumulative += randomMoveProbability;
            if (random < cumulative)
            {
                ExecuteRandomMove();
                return;
            }

            // 移动到建筑
            cumulative += moveToBuildingProbability;
            if (random < cumulative)
            {
                ExecuteMoveToBuilding();
                return;
            }
        }

        /// <summary>
        /// 执行随机移动 - L型路径（先横走再竖走），限制在相机可见范围内
        /// </summary>
        private void ExecuteRandomMove()
        {
            if (mover == null) return;

            Vector3 currentPos = stateMachine.transform.position;
            
            // 获取相机可见范围
            Vector2 minBound = GetCameraMinBound();
            Vector2 maxBound = GetCameraMaxBound();
            
            // 计算角色可移动范围（相机范围与角色移动半径的交集）
            float minX = Mathf.Max(minBound.x, currentPos.x - randomMoveRadius);
            float maxX = Mathf.Min(maxBound.x, currentPos.x + randomMoveRadius);
            float minY = Mathf.Max(minBound.y, currentPos.y - randomMoveRadius);
            float maxY = Mathf.Min(maxBound.y, currentPos.y + randomMoveRadius);

            // 检查范围是否有效
            if (minX >= maxX || minY >= maxY)
            {
                Debug.Log("[IdleState] 没有可移动空间，跳过随机移动");
                return;
            }

            // 在有效范围内随机选点
            float targetX = Random.Range(minX, maxX);
            float targetY = Random.Range(minY, maxY);
            Vector3 targetPos = new Vector3(targetX, targetY, currentPos.z);

            // 计算中间点（先横走到目标X，再竖走到目标Y）
            Vector3 midPoint = new Vector3(targetPos.x, currentPos.y, currentPos.z);

            Debug.Log($"[IdleState] L型移动: {currentPos} -> {midPoint} -> {targetPos}");

            // 第一段：横走到中间点
            mover.MoveTo(midPoint, () =>
            {
                // 第二段：竖走到目标点
                mover.MoveTo(targetPos);
            });
        }

        /// <summary>
        /// 获取主相机可见区域的最小边界（世界坐标）
        /// </summary>
        private Vector2 GetCameraMinBound()
        {
            Camera cam = Camera.main;
            if (cam == null) return new Vector2(-10, -10); // 默认值

            // 相机左下角的世界坐标
            Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, -cam.transform.position.z));
            return new Vector2(bottomLeft.x, bottomLeft.y);
        }

        /// <summary>
        /// 获取主相机可见区域的最大边界（世界坐标）
        /// </summary>
        private Vector2 GetCameraMaxBound()
        {
            Camera cam = Camera.main;
            if (cam == null) return new Vector2(10, 10); // 默认值

            // 相机右上角的世界坐标
            Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, -cam.transform.position.z));
            return new Vector2(topRight.x, topRight.y);
        }

        /// <summary>
        /// 执行移动到建筑 - 随机选择一个场景中的LocationPoint并移动
        /// </summary>
        private void ExecuteMoveToBuilding()
        {
            if (mover == null) return;

            // 获取场景中所有的LocationPoint
            LocationPoint[] locations = Object.FindObjectsOfType<LocationPoint>();
            
            if (locations == null || locations.Length == 0)
            {
                Debug.Log("[IdleState] 场景中没有建筑，降级为随机移动");
                ExecuteRandomMove();
                return;
            }

            // 随机选一个建筑
            LocationPoint target = locations[Random.Range(0, locations.Length)];
            if (target == null)
            {
                ExecuteRandomMove();
                return;
            }

            Vector3 targetPos = target.GetPosition();
            Debug.Log($"[IdleState] 移动到建筑: {target.LocationName} ({targetPos})");
            mover.MoveTo(targetPos);
        }

        #region 配置方法

        /// <summary>
        /// 设置随机等待时间范围
        /// </summary>
        public void SetIdleTimeRange(float min, float max)
        {
            minIdleTime = Mathf.Max(0, min);
            maxIdleTime = Mathf.Max(minIdleTime, max);
        }

        /// <summary>
        /// 设置触发跳舞的概率
        /// </summary>
        public void SetDanceProbability(float probability)
        {
            danceProbability = Mathf.Clamp01(probability);
        }

        /// <summary>
        /// 设置触发战斗的概率
        /// </summary>
        public void SetFightProbability(float probability)
        {
            fightProbability = Mathf.Clamp01(probability);
        }

        /// <summary>
        /// 设置触发随机移动的概率
        /// </summary>
        public void SetRandomMoveProbability(float probability)
        {
            randomMoveProbability = Mathf.Clamp01(probability);
        }

        /// <summary>
        /// 设置触发移动到建筑的概率
        /// </summary>
        public void SetMoveToBuildingProbability(float probability)
        {
            moveToBuildingProbability = Mathf.Clamp01(probability);
        }

        /// <summary>
        /// 设置随机移动半径
        /// </summary>
        public void SetRandomMoveRadius(float radius)
        {
            randomMoveRadius = Mathf.Max(0, radius);
        }

        #endregion
    }
}
