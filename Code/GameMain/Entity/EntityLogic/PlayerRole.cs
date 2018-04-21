using GameFramework;
using UnityEngine;

namespace GameMain
{
    /// <summary>
    /// 玩家
    /// </summary>
    public class PlayerRole : RoleBase
    {
        private RoleEntityData m_PlayerEntityData;


        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            m_PlayerEntityData = userData as RoleEntityData;
            if (m_PlayerEntityData == null)
            {
                Log.Error("playerEntityData is null");
                return;
            }

            //创建Actor
            int actorId = m_PlayerEntityData.Id;
            int actorEntityId = m_PlayerEntityData.TypeId;
            ActorType actorType = m_PlayerEntityData.ActorType;
            BattleCampType campType = m_PlayerEntityData.CampType;
            Actor = new ActorPlayer(actorId, actorEntityId, gameObject, actorType, campType, m_CharacterController,
                m_Animator);
            Actor.Init();

            //设置自身,与跟随相机到场景出身点
            Vector3 spawnPos = GameEntry.Scene.GetCurSceneSpawnPos();
            CachedTransform.position = spawnPos;
            GameEntry.Camera.SetCameraRigPos(spawnPos);

            AddEventListener();
        }


        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            Actor?.Step();
        }

        protected override void OnHide(object userData)
        {
            base.OnHide(userData);

            RemoveEventListener();
            Actor?.Destory();
            Actor = null;
        }

        private void AddEventListener()
        {
            GameEntry.Input.OnAxisInput += OnPlayerMove;
            GameEntry.Input.OnAxisInputEnd += OnPlayerIdle;
        }

        private void RemoveEventListener()
        {
            GameEntry.Input.OnAxisInput -= OnPlayerMove;
            GameEntry.Input.OnAxisInputEnd -= OnPlayerIdle;
        }

        private void OnPlayerMove(Vector2 delta)
        {
            Actor.ExecuteCommand(new MoveCommand(delta));
        }

        private void OnPlayerIdle()
        {
            Actor.ExecuteCommand(new IdleCommand());
        }
    }
}
