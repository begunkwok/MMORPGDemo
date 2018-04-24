using GameFramework;

namespace GameMain
{

    public class NpcRole : RoleBase
    {
        private RoleEntityData m_EnemyEntityData;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            m_EnemyEntityData = userData as RoleEntityData;
            if (m_EnemyEntityData == null)
            {
                Log.Error("playerEntityData is null");
                return;
            }

            int actorId = m_EnemyEntityData.Id;
            int actorEntityId = m_EnemyEntityData.TypeId;
            ActorType actorType = m_EnemyEntityData.ActorType;
            BattleCampType campType = m_EnemyEntityData.CampType;
            Actor = new ActorNpc(actorId, actorEntityId, gameObject, actorType, campType, m_CharacterController,
                m_Animator);
            Actor.Init();
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            Actor?.Step();
        }

        protected override void OnHide(object userData)
        {
            base.OnHide(userData);


            Actor?.Destory();
            Actor = null;
        }
    }
}
