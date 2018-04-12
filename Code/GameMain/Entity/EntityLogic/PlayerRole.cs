using GameFramework;
using UnityEngine;

namespace GameMain
{
    /// <summary>
    /// 玩家实体
    /// </summary>
    public class PlayerRole : EntityBase
    {
        public ActorPlayer PlayerActor { get; protected set; }

        private PlayerEntityData m_PlayerEntityData;
        private CharacterController m_CharacterController;
        private Animator m_Animator;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            m_PlayerEntityData = userData as PlayerEntityData;
            if (m_PlayerEntityData == null)
            {
                Log.Error("playerEntityData is null");
                return;
            }

            m_Animator = gameObject.GetComponent<Animator>();
            m_CharacterController = gameObject.GetOrAddComponent<CharacterController>();
            
            PlayerActor = new ActorPlayer(m_PlayerEntityData.Id,m_PlayerEntityData.TypeId,gameObject,ActorType.Player, ActorBattleCampType.Ally, m_CharacterController, m_Animator);
            PlayerActor.Init();

            //设置自身,与跟随相机到场景出身点
            Vector3 spawnPos = GameEntry.Scene.GetCurSceneSpawnPos();
            CachedTransform.position = spawnPos;
            GameEntry.Camera.SetCameraRigPos(spawnPos);

            AddEventListener();
        }


        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            PlayerActor?.Step();
        }

        protected override void OnHide(object userData)
        {
            base.OnHide(userData);

            RemoveEventListener();
            PlayerActor.Destory();
            PlayerActor = null;
        }

        private void AddEventListener()
        {
            GameEntry.Input.OnInputAxis += OnPlayerMove;
            GameEntry.Input.OnEndInputAxis += OnPlayerIdle;
        }

        private void RemoveEventListener()
        {
            GameEntry.Input.OnInputAxis -= OnPlayerMove;
            GameEntry.Input.OnEndInputAxis -= OnPlayerIdle;
        }

        private void OnPlayerMove(Vector2 delta)
        {
            PlayerActor.ExecuteCommand(new MoveCommand(delta));
        }

        private void OnPlayerIdle()
        {
            PlayerActor.ExecuteCommand(new IdleCommand());
        }
    }
}
