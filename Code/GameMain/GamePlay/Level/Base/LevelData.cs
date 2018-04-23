using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameMain
{
    public class LevelData
    {
        public static List<RoleBase> AllRoles = new List<RoleBase>();
        public static List<ActorBase> AllActors = new List<ActorBase>();

        public static Dictionary<BattleCampType, List<RoleBase>> CampActors = new Dictionary
            <BattleCampType, List<RoleBase>>()
        {
            {BattleCampType.Ally, new List<RoleBase>()},
            {BattleCampType.Enemy, new List<RoleBase>()},
            {BattleCampType.Neutral, new List<RoleBase>()},
            {BattleCampType.Other, new List<RoleBase>()}
        };

        public static int Chapter;
        public static int SceneID;
        public static int CopyID;
        public static CopyType CopyType;
        public static float StrTime;
        public static float EndTime;
        public static bool Win;

        public static PlayerRole Player = null;

        public static List<RoleBase> GetRolesByActorType(ActorType pType)
        {
            List<RoleBase> pList = new List<RoleBase>();
            for (int i = 0; i < AllRoles.Count; i++)
            {
                if (AllRoles[i].Actor.ActorType == pType)
                {
                    pList.Add(AllRoles[i]);
                }
            }
            return pList;
        }

        public static float CurTime
        {
            get { return Time.realtimeSinceStartup - StrTime; }
        }

        public static Action Call { get; set; }

        public static int Star { get; private set; }

        public static bool[] PassContents { get; private set; }

        public static void CalcResult()
        {
            DRCopy drCopy = GameEntry.DataTable.GetDataTable<DRCopy>().GetDataRow(CopyID);
            if (drCopy == null)
            {
                return;
            }

            Star = CalcStar(drCopy);
            if (Call != null)
            {
                Call();
            }

            //TODO 通过副本
          //  ZSRaid.Instance.TryPassCopy(CopyType, Chapter, CopyID, Star);
        }

        public static void Reset()
        {
            CopyID = 0;
            Win = false;
            Call = null;
        }
        
        public static void ShowResult()
        {
            DRCopy drCopy = GameEntry.DataTable.GetDataTable<DRCopy>().GetDataRow(CopyID);
            if (drCopy == null)
            {
                return;
            }
            switch ((CopyType)drCopy.CopyType)
            {
                case CopyType.Easy:
                case CopyType.World:
                case CopyType.Elite:
                case CopyType.Daily:
                {
                        //TODO 显示副本通过UI
                    //ZTUIManager.Instance.OpenWindow(WindowID.UI_MAINRESULT);
                    //UIMainResult window = (UIMainResult) ZTUIManager.Instance.GetWindow(WindowID.UI_MAINRESULT);
                    //window.ShowView();
                }
                    break;
            }
        }

        private static int CalcStar(DRCopy copy)
        {
            int star = 0;
            PassContents = new bool[3] {false, false, false};
            if (Win == false)
            {
                return 0;
            }

            StarConditionType[] starConditions = new StarConditionType[3];
            starConditions[1] = (StarConditionType)copy.StarCondition1;
            starConditions[2] = (StarConditionType)copy.StarCondition2;
            starConditions[3] = (StarConditionType)copy.StarCondition3;

            int [] starValues = new int[3];
            starValues[1] = copy.StarValue1;
            starValues[2] = copy.StarValue2;
            starValues[3] = copy.StarValue3;

            for (int i = 0; i < starConditions.Length; i++)
            {
                StarConditionType type = starConditions[i];
                int v = starValues[i];
                switch (type)
                {
                    case StarConditionType.Health:
                    {
                        if (Player != null)
                        {
                            int maxHealth = Player.Actor.Attrbute.GetValue(AttributeType.MaxHp);
                            int curHealth = Player.Actor.Attrbute.GetValue(AttributeType.Hp);
                            float ratio = curHealth/(maxHealth*1f);
                            if (ratio >= v/100f)
                            {
                                star++;
                                PassContents[i] = true;
                            }
                        }
                    }
                        break;
                    case StarConditionType.Pass:
                    {
                        star++;
                        PassContents[i] = true;
                    }
                        break;
                    case StarConditionType.TimeLimit:
                    {
                        if (CurTime < v)
                        {
                            star++;
                            PassContents[i] = true;
                        }
                    }
                        break;
                }
            }
            return star;
        }
    }
}
