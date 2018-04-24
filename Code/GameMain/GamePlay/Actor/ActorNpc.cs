using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameMain
{
    public class ActorNpc : ActorBase
    {
        public ActorNpc(int entityId, int id, GameObject go, ActorType type, BattleCampType camp, CharacterController cc, Animator anim) : base(entityId, id, go, type, camp, cc, anim)
        {

        }

        protected override void InitAi()
        {
            
        }
    }
}
