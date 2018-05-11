using GameFramework;
using UnityEngine;

namespace GameMain
{

    public class MountRole : RoleEntityBase
    {
        private RoleEntityData m_EnemyEntityData;

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            CachedTransform.rotation = Quaternion.identity;

            m_EnemyEntityData = userData as RoleEntityData;
            if (m_EnemyEntityData == null)
            {
                Log.Error("playerEntityData is null");
                return;
            }

            ActorType actorType = m_EnemyEntityData.ActorType;
            BattleCampType campType = m_EnemyEntityData.CampType;
            Actor = new ActorMount(this, actorType, campType, m_CharacterController,
                m_Animator);
            Actor.Init();
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if (CachedTransform.eulerAngles.x != 0|| CachedTransform.eulerAngles.z != 0)
            {
                CachedTransform.eulerAngles = Vector3.zero;
            }
        }
    }
}
