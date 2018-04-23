using FairyGUI;
using GameFramework;

namespace GameMain
{
    public class HomeForm : FairyGuiForm
    {
        private GComponent m_HeadPanel = null;
        private Controller m_Ctrl = null;

        private GButton m_RoleButton  = null;
        private GButton m_BagButton   = null;
        private GButton m_SkillButton = null;
        private GButton m_TaskButton  = null;
        private GButton m_MountButton = null;

        private GButton m_AiButton        = null;
        private GButton m_Skill01Button   = null;
        private GButton m_Skill02Button   = null;
        private GButton m_Skill03Button   = null;
        private GButton m_Skill04Button   = null;
        private GButton m_Skill05Button = null;


        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            m_HeadPanel = UI.GetChild("HeadPanel").asCom;
            m_Ctrl = UI.GetController("HomeCtrl");

            m_RoleButton = UI.GetChild("btn_Role").asButton;
            m_BagButton = UI.GetChild("btn_Bag").asButton;
            m_SkillButton = UI.GetChild("btn_Skill").asButton;
            m_TaskButton = UI.GetChild("btn_Task").asButton;
            m_MountButton = UI.GetChild("btn_Mount").asButton;

            m_AiButton = UI.GetChild("btn_Ai").asButton;
            m_Skill01Button = UI.GetChild("btn_Skill01").asButton;
            m_Skill02Button = UI.GetChild("btn_Skill02").asButton;
            m_Skill03Button = UI.GetChild("btn_Skill03").asButton;
            m_Skill04Button = UI.GetChild("btn_Skill04").asButton;
            m_Skill05Button = UI.GetChild("btn_Skill05").asButton;
        }

        protected override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            AddListener();
        }

        protected override void OnClose(object userData)
        {
            base.OnClose(userData);

            RemoveListener();
        }

        private void AddListener()
        {
            m_Skill01Button.onClick.Add(() => OnBattleSkillClick(SkillPosType.Skill_0));
            m_Skill02Button.onClick.Add(() => OnBattleSkillClick(SkillPosType.Skill_1));
            m_Skill03Button.onClick.Add(() => OnBattleSkillClick(SkillPosType.Skill_2));
            m_Skill04Button.onClick.Add(() => OnBattleSkillClick(SkillPosType.Skill_3));
            m_Skill05Button.onClick.Add(() => OnBattleSkillClick(SkillPosType.Skill_4));
        }

        private void RemoveListener()
        {
            m_Skill01Button.onClick.Remove(() => OnBattleSkillClick(SkillPosType.Skill_0));
            m_Skill02Button.onClick.Remove(() => OnBattleSkillClick(SkillPosType.Skill_1));
            m_Skill03Button.onClick.Remove(() => OnBattleSkillClick(SkillPosType.Skill_2));
            m_Skill04Button.onClick.Remove(() => OnBattleSkillClick(SkillPosType.Skill_3));
            m_Skill05Button.onClick.Remove(() => OnBattleSkillClick(SkillPosType.Skill_4));
        }


        private void OnBattleSkillClick(SkillPosType skillPos)
        {
            SkillKeyDownEventArgs skillEventArgs = ReferencePool.Acquire<SkillKeyDownEventArgs>();
            skillEventArgs.Fill(skillPos);
           // GameEntry.Event.Fire(skillEventArgs.Id, skillEventArgs);


            EnterLevelEventArgs enterLevelEventArgs = new EnterLevelEventArgs();
            enterLevelEventArgs.Fill(4, SceneId.Level_01);
            GameEntry.Event.Fire(enterLevelEventArgs.LevelId, enterLevelEventArgs);
        }



    }
}
