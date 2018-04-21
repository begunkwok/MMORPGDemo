using System;
using System.Collections.Generic;
using DG.Tweening;
using GameFramework;
using UnityEngine;

namespace GameMain
{
    public class BuffAttrData
    {
        public AttributeType AttrType { get; }
        public DataValueType ValueType { get; }
        public int Value { get; }

        public BuffAttrData( AttributeType attr, DataValueType valueType, int value)
        {
            this.AttrType = attr;
            this.ValueType = valueType;
            this.Value = value;
        }
    }

    /// <summary>
    /// buff基类
    /// </summary>
    public class BuffBase : IBuff
    {
        public int Id { get; private set; }
        public DRBuff Data { get; private set; }
        public int CurOverlayNum { get; private set; }
        public bool IsDead { get; private set; }
        public List<BuffAttrData> AttrList { get; private set; }

        protected float m_StartTimer;
        protected float m_IntervalTimer;
        protected ActorBase m_Owner;
        protected ActorBase m_Caster;
        protected EffectBase m_Effect;
        protected DRBuffAttr m_BuffAttr;

        public BuffBase(int id, ActorBase owner, ActorBase caster)
        {
            this.Id = id;
            this.Data = GameEntry.DataTable.GetDataTable<DRBuff>()?.GetDataRow(id);
            if (Data == null)
            {
                Log.Error("Buff Data is null.");
                return;
            }

            this.m_Owner = owner;
            this.m_Caster = caster;
            if (Data.ResultAttrID != 0)
            {
                m_BuffAttr = GameEntry.DataTable.GetDataTable<DRBuffAttr>()?.GetDataRow(Data.ResultAttrID);
            }

            this.Enter();
        }


        public void Enter()
        {
            if (IsDead)
                return;

            Refresh();
            ChangeModel();
            AddEffect();
        }

        public void Update()
        {
            if (m_Owner.IsDead || IsDead)
            {
                return;
            }

            if (Data.LifeTime > 0 && Time.realtimeSinceStartup - m_StartTimer > Data.LifeTime)
            {
                IsDead = true;
            }

            if (!IsDotOrHot())
            {
                return;
            }

            if (m_BuffAttr != null && m_IntervalTimer > Data.ResultInterval)
            {
                TriggerIntervalEvent();
                m_IntervalTimer = 0;
            }
            else
            {
                m_IntervalTimer += Time.deltaTime;
            }
        }

        public void Exit()
        {
            float size = Data.ChangeModelScale;
            if (size > 0)
            {
                m_Owner.CachedTransform.DOScale(size, 1);
            }
            m_Effect?.Clear();
        }

        public float GetLeftTime()
        {
            if (Data.LifeTime < 0)
            {
                return Data.LifeTime;
            }
            float sec = Time.realtimeSinceStartup - m_StartTimer;
            return sec >= Data.LifeTime ? 0 : Data.LifeTime - sec;
        }

        public void Overlay()
        {
            if (CurOverlayNum < Data.MaxOverlayNum)
            {
                CurOverlayNum++;
            }
            Refresh();
        }

        public void Refresh()
        {
            if (m_Owner.IsDead)
            {
                return;
            }
            m_Effect?.Reset();
            m_StartTimer = Time.realtimeSinceStartup;
        }

        public void SetEffectEnable(bool enable)
        {
            if (m_Effect?.CachedTransform == null)
            {
                return;
            }
            GameEntry.Entity.HideEntity(m_Effect.Id);
        }

        private bool IsDotOrHot()
        {
            return (BattleActType)Data.Result == BattleActType.Lddattr ||
                   (BattleActType)Data.Result == BattleActType.Lubattr;
        }

        private void TriggerIntervalEvent()
        {
            if (m_BuffAttr == null)
            {
                return;
            }

            if (m_BuffAttr.Attr1 != 0)
            {
                BuffAttrData attrData1 = new BuffAttrData((AttributeType) m_BuffAttr.Attr1,
                    (DataValueType) m_BuffAttr.ValueType1,
                    m_BuffAttr.Value1);
                AttrList.Add(attrData1);
                TriggerIntervalEventByBuffAttr(attrData1);
            }

            if (m_BuffAttr.Attr2 != 0)
            {
                BuffAttrData attrData2 = new BuffAttrData((AttributeType)m_BuffAttr.Attr2,
                    (DataValueType)m_BuffAttr.ValueType2,
                    m_BuffAttr.Value2);
                AttrList.Add(attrData2);
                TriggerIntervalEventByBuffAttr(attrData2);
            }

            if (m_BuffAttr.Attr3 != 0)
            {
                BuffAttrData attrData3 = new BuffAttrData((AttributeType)m_BuffAttr.Attr3,
                    (DataValueType)m_BuffAttr.ValueType3,
                    m_BuffAttr.Value3);
                AttrList.Add(attrData3);
                TriggerIntervalEventByBuffAttr(attrData3);
            }
        }

        private void TriggerIntervalEventByBuffAttr(BuffAttrData data)
        {
            int current = m_Owner.Attrbute.GetValue(data.AttrType);
            int changeValue = 0;

            switch (data.ValueType)
            {
                case DataValueType.Fix:
                {
                    changeValue = data.Value;
                }
                    break;
                case DataValueType.Per:
                {
                    changeValue = Mathf.FloorToInt((data.Value / 10000f + 1)*current);
                }
                    break;
                case DataValueType.Com:
                {
                    if (data.AttrType == AttributeType.Hp)
                    {
                        int maxHp = m_Owner.Attrbute.GetValue(AttributeType.MaxHp);
                        changeValue = Mathf.FloorToInt((data.Value / 10000f)*maxHp);
                    }
                    else
                    {
                        int maxMp = m_Owner.Attrbute.GetValue(AttributeType.MaxMp);
                        changeValue = Mathf.FloorToInt((data.Value / 10000f)*maxMp);
                    }
                }
                    break;
            }


            switch ((BattleActType)Data.Result)
            {
                case BattleActType.Lddattr:
                    {
                        if (data.AttrType == AttributeType.Mp)
                        {
                            m_Owner.AddMp(changeValue, true);
                        }
                        else if (data.AttrType == AttributeType.Hp)
                        {
                            m_Owner.AddHp(changeValue, true);
                        }
                    }
                    break;
                case BattleActType.Lubattr:
                    {
                        if (data.AttrType == AttributeType.Mp)
                        {
                            m_Owner.UseMp(changeValue);
                        }
                        else if (data.AttrType == AttributeType.Hp)
                        {
                            m_Owner.TakeDamage(m_Caster, changeValue);
                        }
                    }
                    break;
            }
        }

        private void ChangeModel()
        {
            if (Data.ChangeModelScale > 0)
            {
                Vector3 to = m_Owner.CachedTransform.localScale * Data.ChangeModelScale;
                m_Owner.CachedTransform.DOScale(to, 1);
            }
        }

        private void AddEffect()
        {
            if (Data.EffectID == 0)
            {
                return;
            }

            int entityId = GameEntry.Entity.GenerateTempSerialId();
            EffectData effectdata = new EffectData(entityId, Data.EffectID);
            effectdata.BindType = (EffectBindType)Data.EffectBind;
            effectdata.DeadType = (FlyObjDeadType)Data.DestroyType;
            effectdata.FlyType = FlyObjFlyType.Stay;
            effectdata.KeepTime = Data.LifeTime;
            effectdata.Owner = m_Owner;
            effectdata.Parent = m_Owner.CachedTransform;
            effectdata.SetParent = true;
            m_Effect = GameEntry.Entity.ShowEffect(effectdata);
        }
    }
}
