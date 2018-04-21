using UnityEngine;

namespace GameMain
{
    public class LevelMine:LevelElement
    {
        public float RebornCD = 5;
        public int DropItemCount = 1;
        private int m_MineGUID = 0;

        private Mine m_Mine;

        public override void Import(XmlObject pData, bool pBuild)
        {
            MapMine data = pData as MapMine;
            if (data != null)
            {
                Id = data.Id;
                Position = data.Position;
                Euler = data.EulerAngles;
                RebornCD = data.RebornCD;
                DropItemCount = data.DropItemCount;
            }
            this.Build();
            this.SetName();
        }

        public override XmlObject Export()
        {
            MapMine data = new MapMine
            {
                Id = Id,
                Position = Position,
                EulerAngles = Euler,
                RebornCD = RebornCD,
                DropItemCount = DropItemCount
            };
            return data;
        }

        public override void SetName()
        {
            gameObject.name = "Mine_" + Id.ToString();
        }

        public override void Build()
        {
            transform.DestroyChildren();
            m_Mine = GameEntry.Level.CreateMine(Id);
            if(m_Mine==null)
            {
                return;
            }

            m_Mine.CachedTransform.parent = transform;
            m_Mine.CachedTransform.localPosition = Vector3.zero;
            m_Mine.CachedTransform.localScale = Vector3.one;
            m_Mine.CachedTransform.localRotation = Quaternion.identity;
        }
    }
}