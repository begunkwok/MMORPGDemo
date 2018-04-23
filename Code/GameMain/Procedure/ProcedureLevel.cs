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
            GameEntry.Level.EnterLevel(levelId, (SceneId) sceneId);
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);


        }

     
    }
}
