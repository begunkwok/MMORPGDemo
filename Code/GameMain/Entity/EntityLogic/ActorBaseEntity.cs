using System;
using GameFramework;
using GameFramework.Fsm;
using UnityEngine;

namespace GameMain
{
    /// <summary>
    /// 行动者实体基类
    /// </summary>
    public abstract class ActorBaseEntity : EntityBase
    {

        protected CharacterController m_CharacterCtrl;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

        }           

    }
}
