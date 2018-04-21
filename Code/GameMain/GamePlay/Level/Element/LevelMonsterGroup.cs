using UnityEngine;
using System.Collections.Generic;

namespace GameMain
{
    public class LevelMonsterGroup : LevelElement
    {
        private HashSet<int> mMonsterGUIDSet = new HashSet<int>();

        public int   MonsterID;
        public float RebornCD = 3;
        public int   MaxCount = 4;

        public LevelRegion Region
        {
            get; set;
        }

        public int RegionID
        {
            get { return Region == null ? 0 : Region.Id; }
        }

        public override void SetName()
        {
            transform.name = "Monster_Group_" + Id;
        }

        public override void Import(XmlObject pData,bool pBuild)
        {
            MapMonsterGroup data = pData as MapMonsterGroup;
            Id = data.Id;
            RebornCD = data.RebornCD;
            MaxCount = data.MaxCount;
            MonsterID = data.MonsterID;
            HolderRegion pHolder = GameEntry.Level.GetHolder(MapHolderType.Region) as HolderRegion;
            this.Region = pHolder.FindElement(data.RegionID);
   
            this.Build();
            this.SetName();
        }

        public override XmlObject Export()
        {
            MapMonsterGroup data = new MapMonsterGroup();
            data.Id = Id;
            data.RegionID = RegionID;
            data.RebornCD = RebornCD;
            data.MaxCount = MaxCount;
            data.MonsterID = MonsterID;
            return data;
        }

        public override void Init()
        {
            //TODO 击杀怪物事件
            //ZTEvent.AddHandler<int,int>(EventID.RECV_KILL_MONSTER, OnKillMonster);
            //for(int i=0;i<MaxCount;i++)
            //{
            //    CreateMonster();
            //}
        }

        private void CreateMonster()
        {
            if(Region==null)
            {
                Debug.LogError("找不到区域" + Region.Id);
                return;
            }
            Vector3 pos = GlobalTools.RandomOnCircle(10)+Region.Position;
            Vector3 angle = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
            EnemyRole monster = GameEntry.Level.CreateEnemy(MonsterID, TransformParam.Create(pos, angle));

            mMonsterGUIDSet.Add(monster.Id);
        }

        private void OnKillMonster(int guid,int id)
        {
            if (!this.mMonsterGUIDSet.Contains(guid))
            {
                return;
            }
            mMonsterGUIDSet.Remove(guid);
            if(mMonsterGUIDSet.Count<MaxCount)
            {
                Invoke("CreateMonster", RebornCD);
            }
        }

        void OnDestroy()
        {
            CancelInvoke();
            //TODO 移除击杀怪物时间
            //ZTEvent.RemoveHandler<int,int>(EventID.RECV_KILL_MONSTER, OnKillMonster);
        }
    }
}
