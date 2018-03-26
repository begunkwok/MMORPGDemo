using System;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public class PlayerRole : RoleEntity
    {
        private PlayerController m_PlayerController;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_PlayerController = gameObject.GetOrAddComponent<PlayerController>();

            //设置自身,与跟随相机到场景出身点
            Vector3 spawnPos = GameEntry.Scene.GetCurSceneSpawnPos();
            CachedTransform.position = spawnPos;
            GameEntry.Camera.SetCameraRigPos(spawnPos);
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

        }

        protected override void OnHide(object userData)
        {
            base.OnHide(userData);
        }


    }
}
