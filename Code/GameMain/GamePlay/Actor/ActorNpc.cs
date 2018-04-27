using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameMain
{
    public class ActorNpc : ActorBase
    {
        public ActorNpc(RoleBase entity, ActorType type, BattleCampType camp, CharacterController cc, Animator anim) : base(entity, type, camp, cc, anim)
        {

        }

        protected override void InitAi()
        {
            
        }
    }
}
