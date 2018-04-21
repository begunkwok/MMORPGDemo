using GameFramework;
using UnityEngine;

namespace GameMain
{
    public class LevelBarrier : LevelElement
    {
        public  float Width=14;
        private Transform   m_Body;
        private BoxCollider m_Collider;
        private Vector3 m_Size;

        public override void Build()
        {
            if (Width < Constant.Define.BarrierWidth)
            {
               Width = Constant.Define.BarrierWidth;
            }
            int count = Mathf.CeilToInt(Width / Constant.Define.BarrierWidth);
            m_Size.x = count * Constant.Define.BarrierWidth;
            m_Size.y = 4;
            m_Size.z = 1.5f;

            m_Body = transform.Find("Body");
            if (m_Body == null)
            {
                m_Body = new GameObject("Body").transform;
                m_Body.parent = transform;
                m_Body.transform.localPosition = Vector3.zero;
                m_Body.localEulerAngles = Vector3.zero;
            }
            else
            {
                m_Body.DestroyChildren();
            }
            float halfCount = count * 0.5f;
            for (int i = 0; i < count; i++)
            {
                Barrier barrier = GameEntry.Level.CreateBarrier(Constant.Define.BarrierTypeId);
                if (barrier == null)
                {
                    Log.Error("Create barrier failure.");
                    return;
                }

                Vector3 localPosition = Vector3.right * (i - halfCount + 0.5f) * Constant.Define.BarrierWidth;
                localPosition.z = m_Size.z * 0.5f;
                barrier.CachedTransform.localPosition = localPosition;
                barrier.CachedTransform.SetParent(m_Body, false);
            }

            m_Collider = gameObject.GetOrAddComponent<BoxCollider>();
            m_Collider.size = m_Size;
            m_Collider.center = new Vector3(0, m_Size.y * 0.5f, m_Size.z * 0.5f);
            GlobalTools.SetLayer(gameObject, Constant.Define.BarrierTypeId);

        }

        public override void SetName()
        {
            gameObject.name = "Barrier_" + Id.ToString();
        }

        public override XmlObject Export()
        {
            MapBarrier data = new MapBarrier();
            data.Id = Id;
            data.Width = Width;
            data.TransParam = new MapTransform();
            data.TransParam.Position = Position;
            data.TransParam.Scale = Scale;
            data.TransParam.EulerAngles = Euler;
            return data;
        }

        public override void Import(XmlObject pData,bool pBuild)
        {
            MapBarrier data = pData as MapBarrier;
            if (data != null)
            {
                Id = data.Id;
                Width = data.Width;
                Position = data.TransParam.Position;
                Scale = data.TransParam.Scale;
                Euler = data.TransParam.EulerAngles;
            }

            this.Build();
            this.SetName();
        }
    }
}