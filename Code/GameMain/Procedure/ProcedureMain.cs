using UnityEngine;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    public class ProcedureMain : ProcedureBase
    {
        public override bool UseNativeDialog => false;

        private ProcedureOwner m_ProcedureOwner;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            m_ProcedureOwner = procedureOwner;

            GameEntry.UI.OpenUIForm(UIFormId.ControllerForm);
            GameEntry.UI.OpenUIForm(UIFormId.HomeForm);

            CreatePlayer();
        }


        private void CreatePlayer()
        {
            if(m_ProcedureOwner== null)
                return;

            int playerId = m_ProcedureOwner.GetData<VarInt>(Constant.ProcedureData.PlayerId);
            DBPlayer dbPlayer = GameEntry.Database.GetDBRow<DBPlayer>(playerId);

            TransformParam tParam = new TransformParam()
            {
                Position = Vector3.zero,
                EulerAngles = Vector3.zero,
                Scale = Vector3.one
            };

            GameEntry.Level.CreatePlayer(dbPlayer.Id, tParam);
            GameEntry.Level.CreateEnemy(50001, tParam);
        }

    }
}
