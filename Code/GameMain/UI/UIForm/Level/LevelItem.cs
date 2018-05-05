using FairyGUI;
using FairyGUI.Utils;
using GameFramework;
using UnityEngine;

namespace GameMain
{
    public class LevelItem : GButton
    {
        public int LevelId { get; private set; }
        public SceneId SceneId { get; private set; }
        public string LevelName { get; private set; }
        public bool IsLock { get; private set; }

        private Controller m_Ctrl = null;
        private GLoader m_Icon = null;
        private GTextField m_Title = null;
        private GTextField m_Level = null;
        private GComponent m_Star01 = null;
        private GComponent m_Star02 = null;
        private GComponent m_Star03 = null;

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_Ctrl = GetController("bgCtrl");
            m_Icon = GetChild("icon").asLoader;
            m_Title = GetChild("title").asTextField;
            m_Level = GetChild("tf_Level").asTextField;
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
            SceneId = (SceneId) levelData.Scene;

            DBPlayer player = GameEntry.Database.GetDBRow<DBPlayer>(GameEntry.Database.GetPlayerId());
            if (levelData.LevelRequest <= player.Level)
            {
                IsLock = false;
                m_Ctrl.selectedIndex = levelData.Icon;
                m_Level.color = Color.green;
            }
            else
            {
                IsLock = true;
                m_Ctrl.selectedIndex = 0;
                m_Level.color = Color.red;
            }
            m_Title.text = levelData.Name;
            m_Level.text = "LV." + levelData.LevelRequest;
        }

    }
}
