using System;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    public class ProcedureLogin : ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get
            {
                return false;
            }
        }

        private ProcedureOwner m_ProcedureOwner;

        private bool m_LoginSuccess = false;
        private bool m_GetPlayerSuccess = false;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_ProcedureOwner = procedureOwner;

            LoginFormParams data = new LoginFormParams();
            data.Version = GameEntry.Base.GameVersion;
            data.OnClickLogin = OnLoginClick;
            data.OnClickRegister = OnRegisterClick;
            GameEntry.UI.OpenUIForm(UIFormId.LoginForm, data);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (m_LoginSuccess)
            {
                if (m_GetPlayerSuccess)
                {
                    procedureOwner.SetData<VarInt>(Constant.ProcedureData.NextSceneId, (int)SceneId.MainCity);
                    ChangeState<ProcedureChangeScene>(procedureOwner);
                }
                else
                {
                    procedureOwner.SetData<VarInt>(Constant.ProcedureData.NextSceneId, (int)SceneId.CreateRole);
                    ChangeState<ProcedureChangeScene>(procedureOwner);
                }
            }

        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.UI.CloseUIForm(UIFormId.LoginForm);
        }

        private void OnLoginClick(string account,string password)
        {
            if (GameEntry.Database.TryLogin(account,password))
            {
                m_ProcedureOwner.SetData<VarString>(Constant.ProcedureData.UserId, account);
                m_LoginSuccess = true;

                IDBTable<DBUser> dtUser = GameEntry.Database.GetDBTable<DBUser>();
                DBUser drUser = dtUser.GetDBRow(int.Parse(account));
                if (drUser == null || drUser.Player == 0)
                {
                    m_GetPlayerSuccess = false;
                }
                else
                {
                    m_ProcedureOwner.SetData<VarInt>(Constant.ProcedureData.PlayerId, drUser.Player);
                    m_GetPlayerSuccess = true;
                }
            }
            else
            {
                Log.Error("账号密码不存在！");
            }
        }

        private void OnRegisterClick(string account, string password,Action callback)
        {
            if (GameEntry.Database.TryRegister(account, password))
            {
                callback?.Invoke();
            }
            else
            {
                Log.Error("账号已存在！");
            }
            
        }

    }
}
