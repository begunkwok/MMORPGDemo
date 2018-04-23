using System;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    /// <summary>
    /// 主城流程
    /// </summary>
    public class ProcedureMainCity : ProcedureBase
    {
        public override bool UseNativeDialog => false;

        private ProcedureOwner m_ProcedureOwner;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);

            m_ProcedureOwner = procedureOwner;


            GameEntry.Event.Subscribe(EnterLevelEventArgs.EventId, OnEnterLevel);

            GameEntry.UI.OpenUIForm(UIFormId.ControllerForm);
            GameEntry.UI.OpenUIForm(UIFormId.HomeForm);

            CreatePlayer();
            GameEntry.Level.CreateEnemy(50001, TransformParam.Default);
            GameEntry.Level.EnterLevel(1001, SceneId.MainCity);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            GameEntry.Event.Unsubscribe(EnterLevelEventArgs.EventId, OnEnterLevel);
        }

        private void CreatePlayer()
        {
            if (m_ProcedureOwner == null)
                return;

            int playerId = m_ProcedureOwner.GetData<VarInt>(Constant.ProcedureData.PlayerId);
            DBPlayer dbPlayer = GameEntry.Database.GetDBRow<DBPlayer>(playerId);

            TransformParam tParam = new TransformParam()
            {
                Position = Vector3.zero,
                EulerAngles = Vector3.zero,
                Scale = Vector3.one
            };

            GameEntry.Level.CreatePlayer(dbPlayer.Id);
            GameEntry.Level.CreateEnemy(50001, tParam);
        }

        private void OnEnterLevel(object sender, GameEventArgs e)
        {
            EnterLevelEventArgs ne = e as EnterLevelEventArgs;
            m_ProcedureOwner.SetData<VarInt>(Constant.ProcedureData.NextSceneId, (int) ne.SceneId);
            m_ProcedureOwner.SetData<VarInt>(Constant.ProcedureData.NextLevelId, ne.LevelId);

            ChangeState<ProcedureChangeScene>(m_ProcedureOwner);
        }

     }
}
