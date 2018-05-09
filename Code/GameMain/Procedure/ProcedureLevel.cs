using System;
using GameFramework;
using GameFramework.Event;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    /// <summary>
    /// 关卡流程
    /// </summary>
    public class ProcedureLevel : ProcedureBase
    {
        public override bool UseNativeDialog => false;

        private ProcedureOwner m_ProcedureOwner;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);

            m_ProcedureOwner = procedureOwner;

            int levelId = m_ProcedureOwner.GetData<VarInt>(Constant.ProcedureData.NextLevelId);
            int sceneId = m_ProcedureOwner.GetData<VarInt>(Constant.ProcedureData.NextSceneId);

            CreatePlayer();
            GameEntry.Level.EnterLevel(levelId, (SceneId) sceneId);

            GameEntry.Event.Subscribe(BackCityEventArgs.EventId,OnBackCity);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);

            GameEntry.Event.Unsubscribe(BackCityEventArgs.EventId, OnBackCity);          
        }

        private void OnBackCity(object sender, GameEventArgs e)
        {
            m_ProcedureOwner.SetData<VarInt>(Constant.ProcedureData.NextSceneId, (int)SceneId.MainCity);
            ChangeState<ProcedureChangeScene>(m_ProcedureOwner);
        }

        private void CreatePlayer()
        {
            if (m_ProcedureOwner == null)
                return;

            int playerId = m_ProcedureOwner.GetData<VarInt>(Constant.ProcedureData.PlayerId);

            DBPlayer dbPlayer = GameEntry.Database.GetDBRow<DBPlayer>(playerId);

            GameEntry.Level.CreatePlayer(dbPlayer.Id);
            RefreshHeroInfoEventArgs args = ReferencePool.Acquire<RefreshHeroInfoEventArgs>().FillName(dbPlayer.Name);
            GameEntry.Event.Fire(this, args);

        }
    }
}
