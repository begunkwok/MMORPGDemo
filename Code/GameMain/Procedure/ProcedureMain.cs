using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    public class ProcedureMain : ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get
            {
                return false;
            }
        }

        private ProcedureOwner m_ProcedureOwner;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnInit(procedureOwner);
            m_ProcedureOwner = procedureOwner;

            GameEntry.UI.OpenUIForm(UIFormId.ControllerForm);

            CreatePlayer();
        }


        private void CreatePlayer()
        {
            if(m_ProcedureOwner== null)
                return;

            int playerId = m_ProcedureOwner.GetData<VarInt>(Constant.ProcedureData.PlayerId);
            DBPlayer dbPlayer = GameEntry.Database.GetDBRow<DBPlayer>(playerId);

            GameEntry.Entity.ShowPlayer(new PlayerEntityData(dbPlayer.Id, dbPlayer.EntityTypeId));
            GameEntry.Camera.SwitchCameraBehaviour(CameraBehaviourType.LockLook);
     
        }

    }
}
