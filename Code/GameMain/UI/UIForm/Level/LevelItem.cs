using FairyGUI;
using FairyGUI.Utils;
using GameFramework;

namespace GameMain
{
    public class LevelItem : GButton
    {
        public int LevelId { get; private set; }
        public SceneId SceneId { get; private set; }
        public string LevelName { get; private set; }

        private Controller m_Ctrl = null;
        private GLoader m_Icon = null;
        private GTextField m_Title = null;
        private GComponent m_Star01 = null;
        private GComponent m_Star02 = null;
        private GComponent m_Star03 = null;

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_Ctrl = GetController("bgCtrl");
            m_Icon = GetChild("icon").asLoader;
            m_Title = GetChild("title").asTextField;
            m_Star01 = GetChild("star01").asCom;
            m_Star02 = GetChild("star02").asCom;
            m_Star03 = GetChild("star03").asCom;
        }

        public void Init(DRLevel levelData)
        {
            if (levelData == null)
            {
                Log.Error("data is invalid.");
                return;
            }
            LevelId = levelData.Id;
            LevelName = levelData.Name;
            SceneId = (SceneId)levelData.Scene;

            m_Ctrl.selectedIndex = levelData.Icon;
            m_Title.text = levelData.Name;
        }

    }
}
