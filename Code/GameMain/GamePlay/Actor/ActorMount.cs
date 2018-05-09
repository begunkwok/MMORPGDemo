using UnityEngine;

namespace GameMain
{
    public class ActorMount : ActorBase
    {
        public ActorMount(RoleEntityBase entity, ActorType type, BattleCampType camp, CharacterController cc, Animator anim) : base(entity, type, camp, cc, anim)
        {

        }

        protected override void InitAi()
        {
            m_ActorPathFinding = new AIPathFinding(this);
        }
    }
}
