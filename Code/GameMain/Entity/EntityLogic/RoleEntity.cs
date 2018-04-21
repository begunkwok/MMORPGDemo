using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GameMain
{
    /// <summary>
    /// 角色基类
    /// </summary>
    public class RoleBase : EntityBase
    {
        public ActorBase Actor { get; protected set; }

        protected CharacterController m_CharacterController;
        protected Animator m_Animator;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            m_Animator = gameObject.GetComponent<Animator>();
            m_CharacterController = gameObject.GetOrAddComponent<CharacterController>();
        }

    }
}
