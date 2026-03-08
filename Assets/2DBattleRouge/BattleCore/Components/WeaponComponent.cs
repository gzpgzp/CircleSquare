using System;
using System.Collections.Generic;
using Tools.GameObjectPools;
using Tools.ResourcesTool;
using UnityEngine;

namespace Rouge.BattleCore.Components
{
    public class WeaponComponent : BaseComponent
    {
        // view层
        [SerializeField] private Transform weaponTr;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private Transform bulletTr;
        
        private Sprite weaponSprite;
        private string bulletName;
        
        // 
        
        
        public virtual void Init()
        {
            string spritePath = "";
            weaponSprite = MyResourcesManager.Instance.Load<Sprite>(spritePath);

            string bulletPath = "";
            string bulletName = "";
            GameObjectPool.Instance.Load(bulletName, bulletPath);
        }
        
        public virtual void OnEquip()
        {
            sprite.sprite = weaponSprite;
        }

        public virtual void OnAttack()
        {
            var bullet = GameObjectPool.Instance.CreateGameObject(bulletName);
            bullet.transform.position = bulletTr.position;
            bullet.transform.localScale = Vector3.zero;
            
        }
        
    }
}