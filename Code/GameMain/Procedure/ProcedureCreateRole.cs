using GameFramework;
using GameFramework.DataTable;
using System.Collections.Generic;
using UnityGameFramework.Runtime;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace GameMain
{
    public class ProcedureCreateRole : ProcedureBase
    {
        public override bool UseNativeDialog
        {
            get
            {
                return false;
            }
        }

        private ProcedureOwner m_ProcedureOwner = null;

        private PoseRoleData m_WarriorData = null;
        private PoseRoleData m_MasterData = null;
        private PoseRoleData m_ShooterData = null;

        private int m_SelectRoleTypeId = 0;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            m_ProcedureOwner = procedureOwner;

            //初始化数据
            InitData();

            //显示第一个职业
            GameEntry.Entity.ShowPoseRole(m_WarriorData);
            m_SelectRoleTypeId = (int)EntityTypeId.PlayerWarrior;

            //打开创建角色界面
            RoleCreateFormParams data = new RoleCreateFormParams();
            IDataTable<DRRoleName> nameDt = GameEntry.DataTable.GetDataTable<DRRoleName>() ;
            DRRoleName[] allNames = nameDt.GetAllDataRows();
            Queue<string> namesQueue = new Queue<string>();
            for (int i = 0; i < allNames.Length; i++)
            {
                namesQueue.Enqueue(allNames[i].RoleName);
            }
            data.RandomNameQueue = namesQueue;
            data.OnClickRoleType = OnClickRoleType;
            data.OnClickCreateRole = OnClickCreateRole;
            GameEntry.UI.OpenUIForm(UIFormId.CreateRoleForm, data);
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            GameEntry.UI.CloseUIForm(UIFormId.CreateRoleForm);
        }

        private void InitData()
        {
            m_WarriorData = new PoseRoleData(GameEntry.Entity.GenerateTempSerialId(), (int)EntityTypeId.PoseWarrior, ProfessionType.Warrior);
            m_MasterData = new PoseRoleData(GameEntry.Entity.GenerateTempSerialId(), (int)EntityTypeId.PoseMaster, ProfessionType.Master);
            m_ShooterData = new PoseRoleData(GameEntry.Entity.GenerateTempSerialId(), (int)EntityTypeId.PoseShooter, ProfessionType.Shoooter);
        }

        private void OnClickRoleType(int type)
        {
            ProfessionType pfType = (ProfessionType)type;
            switch (pfType)
            {
                case ProfessionType.Warrior:
                    GameEntry.Entity.ShowPoseRole(m_WarriorData);
                    m_SelectRoleTypeId = (int)EntityTypeId.PlayerWarrior;

                    if (GameEntry.Entity.HasEntity(m_MasterData.Id))
                        GameEntry.Entity.HideEntity(m_MasterData.Id);
                    if (GameEntry.Entity.HasEntity(m_ShooterData.Id))
                        GameEntry.Entity.HideEntity(m_ShooterData.Id);
                    break;
                case ProfessionType.Master:
                    GameEntry.Entity.ShowPoseRole(m_MasterData);
                    m_SelectRoleTypeId = (int)EntityTypeId.PlayerMaster;

                    if (GameEntry.Entity.HasEntity(m_WarriorData.Id))
                        GameEntry.Entity.HideEntity(m_WarriorData.Id);
                    if (GameEntry.Entity.HasEntity(m_ShooterData.Id))
                        GameEntry.Entity.HideEntity(m_ShooterData.Id);
                    break;
                case ProfessionType.Shoooter:
                    GameEntry.Entity.ShowPoseRole(m_ShooterData);
                    m_SelectRoleTypeId = (int)EntityTypeId.PlayerShooter;

                    if (GameEntry.Entity.HasEntity(m_MasterData.Id))
                        GameEntry.Entity.HideEntity(m_MasterData.Id);
                    if (GameEntry.Entity.HasEntity(m_WarriorData.Id))
                        GameEntry.Entity.HideEntity(m_WarriorData.Id);
                    break;
            }
        }

        private void OnClickCreateRole(string roleName)
        {
            Log.Info("create role ,name : " + roleName);

            if (m_ProcedureOwner != null)
            {
                //创建角色数据
                string userId = m_ProcedureOwner.GetData<VarString>(Constant.ProcedureData.UserId);
                int playerId = GameEntry.Entity.GenerateSerialId();
                DBPlayer dbPlayer = new DBPlayer(playerId, userId);
                dbPlayer.EntityTypeId = m_SelectRoleTypeId;
                dbPlayer.Name = roleName;
                dbPlayer.Level = 1;
                dbPlayer.Insert();
                GameEntry.Database.AddDBRow<DBPlayer>(dbPlayer.Id, dbPlayer);

                DBUser dbUser = GameEntry.Database.GetDBRow<DBUser>(int.Parse(userId));
                dbUser.Player = dbPlayer.Id;

                m_ProcedureOwner.SetData<VarInt>(Constant.ProcedureData.PlayerId, dbUser.Player);
                m_ProcedureOwner.SetData<VarInt>(Constant.ProcedureData.NextSceneId, (int) SceneId.MainCity);
                ChangeState<ProcedureChangeScene>(m_ProcedureOwner);
            }
        }

    }

}
