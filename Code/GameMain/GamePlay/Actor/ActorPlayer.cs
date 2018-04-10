using System;
using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;

namespace GameMain
{
    public class EquipAvatar
    {
        public GameObject[] Models = new GameObject[2];
    }

    /// <summary>
    /// 玩家
    /// </summary>
    public class ActorPlayer : ActorBase
    {
        protected ActorBase m_Partner1;
        protected ActorBase m_Partner2;
        protected ActorBase m_Partner3;
        protected ActorBase m_Pet;
        protected ActorBase m_Mount;
        protected ActorBase m_Vehicle;

        protected Dictionary<int, EquipAvatar> mEquipAvatars = new Dictionary<int, EquipAvatar>();

        public ActorPartnerSortType Sort { get; set; }

        public ActorBase Partner1
        {
            get { return m_Partner1; }
            set
            {
                m_Partner1 = value;
                m_Partner1?.SetHost(this);
            }
        }

        public ActorBase Partner2
        {
            get { return m_Partner2; }
            set
            {
                m_Partner2 = value;
                m_Partner2?.SetHost(this);
            }
        }

        public ActorBase Mount
        {
            get { return m_Mount; }
            set
            {
                m_Mount = value;
                m_Mount?.SetHost(this);
            }
        }

        public ActorBase Pet
        {
            get { return m_Pet; }
            set
            {
                m_Pet = value;
                m_Pet?.SetHost(this);
            }
        }

        public ActorBase Vehicle
        {
            get { return m_Vehicle; }
            set
            {
                m_Vehicle = value;
                m_Vehicle?.SetHost(this);
            }
        }


        public ActorPlayer(int entityId, GameObject go, ActorType type, ActorBattleCampType camp, CharacterController cc,Animator anim) : base(entityId, go, type, camp, cc,anim)
        {

        }

        public override void Init()
        {
            base.Init();
            GameEntry.Event.Subscribe(ChangeEquipEventArgs.EventId,ChangeEquipAvatar);

        }

        public Vector3 GetPartnerPosBySort(ActorPartnerSortType sortType)
        {
            switch (sortType)
            {
                case ActorPartnerSortType.Middle:
                    return Pos + new Vector3(0, 0, 2);
                case ActorPartnerSortType.Left:
                    return Pos + new Vector3(-2, 0, 0);
                case ActorPartnerSortType.Right:
                    return Pos + new Vector3(2, 0, 0);
                default:
                    return Pos;
            }
        }

        public override void OnBeginRide()
        {
            this.StopPathFinding();
            this.LoadMount();
            m_Mount.SetHost(this);
            m_AnimController.Play("qicheng", null, true);
            m_CharacterController.enabled = false;
            this.SetActorState(ActorStateType.IsRide, true);
        }

        public override void OnEndRide()
        {
            //CacheTransform.parent = ZTLevel.Instance.GetHolder(EMapHolder.Role).transform;
            CacheTransform.localPosition = GlobalTools.NavSamplePosition(Pos);
            m_CharacterController.enabled = true;
            if (m_Mount != null)
            {
                //ZTLevel.Instance.DelActor(mMount);
                m_Mount = null;
            }
            m_Vehicle = this;
            this.SetActorState(ActorStateType.IsRide, false);
            ChangeState<ActorIdleFsm>();
        }

        public override void MoveTo(Vector3 destPosition)
        {
            if (m_Vehicle != this)
            {
                m_Vehicle.MoveTo(destPosition);
                m_AnimController.Play("qicheng_run", null, true);
            }
            else
            {
                base.MoveTo(destPosition);
            }
        }

        public override void StopPathFinding()
        {
            if (m_Vehicle != null)
            {
                m_Vehicle.StopPathFinding();
            }
            else
            {
                base.StopPathFinding();
            }
        }

        public override void OnForceToMove(MoveCommand ev)
        {
            if (m_Vehicle != null)
            {
                m_Vehicle.OnForceToMove(ev);
                m_AnimController.Play("qicheng_run", null, true);
            }
            else
            {
                base.OnForceToMove(ev);
            }
        }

        public override void OnPursue(AutoMoveCommand ev)
        {
            if (m_Vehicle != null && m_Vehicle != this)
            {
                m_Vehicle.OnPursue(ev);
                m_AnimController.Play("qicheng_run", null, true);
            }
            else
            {
                base.OnPursue(ev);
            }
        }

        public override void OnIdle()
        {
            if (m_Vehicle != null && m_Vehicle != this)
            {
                m_Vehicle.OnIdle();
                m_AnimController.Play("qicheng", null, true);
            }
            else
            {
                base.OnIdle();
            }
        }

        public override void UpdateCurAttribute(bool init = false)
        {
            base.UpdateCurAttribute(init);

            //ZTEvent.FireEvent(EventID.UPDATE_AVATAR_ATTR);
        }

        public override void Destory()
        {
            base.Destory();

            GameEntry.Event.Unsubscribe(ChangeEquipEventArgs.EventId, ChangeEquipAvatar);
        }

        public override void Clear()
        {
            base.Clear();

            for (int i = 0; i < 8; i++)
            {
                RemoveEquip(i);
            }
        }


        public EquipAvatar GetEquipModelsByPos(int pos)
        {
            EquipAvatar pModel = null;
            mEquipAvatars.TryGetValue(pos, out pModel);
            if (pModel == null)
            {
                pModel = new EquipAvatar();
                mEquipAvatars.Add(pos, pModel);
            }
            return pModel;
        }

        public void ChangeEquipAvatar(object sender, GameEventArgs e)
        {
            ChangeEquipEventArgs ne = e as ChangeEquipEventArgs;
            int targetPos = ne.EquipPos;

            //RemoveEquip(pTargetPos - 1);
            //XEquip equip = DataManager.Instance.GetDressEquipByPos(pTargetPos);
            //if (equip != null)
            //{
            //    ChangeEquip(pTargetPos - 1, equip.Id);
            //}
        }

        public void ChangeEquip(int pDressPos, int pEquipID)
        {
            //DBItem itemDB = ZTConfig.Instance.GetDBItem(pEquipID);
            //string[] modelPaths = { itemDB.Model_R, itemDB.Model_L };
            //EquipAvatar pModel = GetEquipModelsByPos(pDressPos);
            //for (int i = 0; i < 2; i++)
            //{
            //    string path = modelPaths[i];
            //    string bone = Define.EQUIP_BONES[pDressPos, i];
            //    if (string.IsNullOrEmpty(path)) { continue; }
            //    Transform boneTrans = GTTools.GetBone(CacheTransform, bone);
            //    if (boneTrans == null) { continue; }
            //    pModel.Models[i] = ZTResource.Instance.Load<GameObject>(path, true);
            //    if (pModel.Models[i] != null)
            //    {
            //        GameObject model = pModel.Models[i];
            //        model.transform.parent = boneTrans;
            //        NGUITools.SetLayer(pModel.Models[i], Obj.layer);
            //        model.transform.localPosition = Vector3.zero;
            //        model.transform.localEulerAngles = Vector3.zero;
            //        model.transform.localScale = Vector3.one;
            //    }
            //}
        }

        public void RemoveEquip(int pos)
        {
            EquipAvatar pModel = GetEquipModelsByPos(pos);
            for (int i = 0; i < pModel.Models.Length; i++)
            {
                if (pModel.Models[i] != null)
                {
                    pModel.Models[i].SetActive(false);
                    GameObject.Destroy(pModel.Models[i]);
                }
            }
        }



        private void LoadMount()
        {
            //Transform param = Transform.Create(CacheTransform.position, CacheTransform.eulerAngles);
            //Mount = ZTLevel.Instance.AddActor(mActorCard.GetMountID(), ActorType.Mount, ActorBattleCampType.Neutral, param);
            m_Vehicle = m_Mount;
            Transform ridePoint = GetRidePoint();
            if (ridePoint != null)
            {
                CacheTransform.parent = ridePoint;
                CacheTransform.localPosition = Vector3.zero;
                CacheTransform.localRotation = Quaternion.identity;
            }
        }

        private void OnChangePartner(int pos, int id)
        {
            //mActorCard.SetPartnerByPos(pos, id);
            //switch (pos)
            //{
            //    case 1:
            //        ZTLevel.Instance.DelActor(this.Partner1);
            //        break;
            //    case 2:
            //        ZTLevel.Instance.DelActor(this.Partner2);
            //        break;
            //}
            //ZTLevel.Instance.AddPartner(this, pos, id);
        }

        private void OnKillMonster(int guid, int id)
        {
            //DBEntiny db = ZTConfig.Instance.GetDBEntiny(id);
            //if (db.Exp > 0)
            //{
            //    ZSRole.Instance.TryAddHeroExp(db.Exp);
            //}
        }

        private void OnUpgradeLevel()
        {
            //this.GetActorCard().SetLevel();
            //ZTBoard.Instance.Refresh(this);
            //EffectData data = new EffectData();
            //data.Owner = this;
            //data.Id = Define.EFFECT_UPGRADE_ID;
            //data.LastTime = 3;
            //data.Bind = EEffectBind.OwnFoot;
            //data.Dead = EFlyObjDeadType.UntilLifeTimeEnd;
            //data.Parent = CacheTransform;
            //data.SetParent = true;
            //EffectBase effect = ZTEffect.Instance.CreateEffect(data);
        }

        private void OnChangeFightValue()
        {
            base.InitAttribute();
        }

        private Transform GetRidePoint()
        {
            return GlobalTools.GetBone(CacheTransform, "Bone026");
        }

    }
}
