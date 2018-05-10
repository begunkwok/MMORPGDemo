using System;
using System.Collections.Generic;
using GameFramework;
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
        protected MountRole m_Mount;
        protected ActorBase m_Vehicle;

        protected Dictionary<int, EquipAvatar> mEquipAvatars = new Dictionary<int, EquipAvatar>();
        protected DBPlayer m_PlayerData;

        private Transform m_OriginalParent;

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

        public MountRole Mount
        {
            get { return m_Mount; }
            set
            {
                m_Mount = value;
                m_Mount.Actor.SetHost(this);
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

        public bool BattleState
        {
            get;
            set;
        }


        public ActorPlayer(RoleEntityBase entity, ActorType type, BattleCampType camp, CharacterController cc,Animator anim) : base(entity, type, camp, cc,anim)
        {
            m_PlayerData = GameEntry.Database.GetDBRow<DBPlayer>(Id);
            if (m_PlayerData == null)
            {
                throw new Exception("Get DBPlayer Failure.");
            }
        }

        public override void Init()
        {
            base.Init();
            GameEntry.Event.Subscribe(ChangeEquipEventArgs.EventId,ChangeEquipAvatar);
            GameEntry.Event.Subscribe(SkillKeyDownEventArgs.EventId,TryUseSkill);
            GameEntry.Event.Subscribe(KillMonsterEventArgs.EventId,OnKillMonster);
            GameEntry.Event.Subscribe(ChangeAiModeEventArgs.EventId,OnChangeAiMode);
            GameEntry.Event.Subscribe(ChangeVehicleEventArgs.EventId,OnChangeVehicle);
            GameEntry.Event.Subscribe(ChangePartnerEventArgs.EventId,OnChangePartner);

            BroadcastHeroInfo();
            m_OriginalParent = CachedTransform.parent;
            m_Vehicle = this;
            BattleState = false;
        }

        public override void Clear()
        {
            base.Clear();

            GameEntry.Event.Unsubscribe(ChangeEquipEventArgs.EventId, ChangeEquipAvatar);
            GameEntry.Event.Unsubscribe(SkillKeyDownEventArgs.EventId, TryUseSkill);
            GameEntry.Event.Unsubscribe(KillMonsterEventArgs.EventId, OnKillMonster);
            GameEntry.Event.Unsubscribe(ChangeAiModeEventArgs.EventId, OnChangeAiMode);
            GameEntry.Event.Unsubscribe(ChangeVehicleEventArgs.EventId, OnChangeVehicle);
            GameEntry.Event.Unsubscribe(ChangePartnerEventArgs.EventId, OnChangePartner);

            for (int i = 0; i < 8; i++)
            {
                RemoveEquip(i);
            }

            this.CachedTransform.parent = m_OriginalParent;
        }

        public override void Step()
        {
            base.Step();

            if (!IsDead && BattleState)
            {
                RefreshBuffEventArgs args = ReferencePool.Acquire<RefreshBuffEventArgs>().Fill(this);
                GameEntry.Event.Fire(this, args);
            }
        }

        protected override void InitAi()
        {
            m_ActorPathFinding = new AIPathFinding(this);
            float atkDist = m_ActorData.AiAtkDist;
            float followDist = m_ActorData.AiFollowDist;
            float waringDist = m_ActorData.AiWaringDist;
            float findEnemyInterval = m_ActorData.FindEnemyInterval;
            m_ActorAI = new ActorFsmAI(this, AIModeType.Hand, atkDist, followDist, waringDist, findEnemyInterval);
            m_ActorAI.Start();
        }

        public override int Attack(IActor defender, int value)
        {
            OnPlayerAttackEventArgs args = ReferencePool.Acquire<OnPlayerAttackEventArgs>().Fill(defender);
            GameEntry.Event.Fire(this, args);
            return base.Attack(defender, value);
        }

        protected override void CreateBoard()
        {
            BoardFormData data = new BoardFormData
            {
                OwnerId = EntityId,
                ActorType = ActorType,
                CacheTransform = CachedTransform,
                Name = m_PlayerData.Name,
                Level = m_ActorData.Level,
                Height = Height
            };
            BoardFormManager.Instance.Create(data);
        }

        protected override void InitAttribute(bool init = false)
        {
            m_BaseAttribute = new ActorAttribute();
            m_CurAttribute = new ActorAttribute();
            Dictionary<PropertyType, int> propertys = AttributeTools.GetPlayerPropertys(m_PlayerData);
            m_BaseAttribute.CopyFrom(propertys);
            UpdateCurAttribute(init);
        }

        public override void UpdateCurAttribute(bool init = false)
        {
            base.UpdateCurAttribute(init);

            if (BattleState)
            {
                RefreshBuffEventArgs args = ReferencePool.Acquire<RefreshBuffEventArgs>().Fill(this);
                GameEntry.Event.Fire(this, args);
            }
        }

        protected override void UpdateHealth()
        {
            int maxHp = Attrbute.GetValue(AttributeType.MaxHp);
            int curHp = Attrbute.GetValue(AttributeType.Hp);
            RefreshHeroInfoEventArgs args = ReferencePool.Acquire<RefreshHeroInfoEventArgs>().FillHp(curHp, maxHp);
            GameEntry.Event.Fire(this, args);
        }

        protected override void UpdatePower()
        {
            base.UpdatePower();
            int maxMp = Attrbute.GetValue(AttributeType.MaxMp);
            int curMp = Attrbute.GetValue(AttributeType.Mp);
            RefreshHeroInfoEventArgs args = ReferencePool.Acquire<RefreshHeroInfoEventArgs>().FillMp(curMp, maxMp);
            GameEntry.Event.Fire(this, args);
        }

        public override void OnBeginRide()
        {
            this.StopPathFinding();
            this.LoadMount();
            m_Mount.Actor.SetHost(this);
            m_AnimController.Play("qicheng", null, true);
            m_CharacterController.enabled = false;
            this.SetActorState(ActorStateType.IsRide, true);
            m_Mount.CachedTransform.localRotation = Quaternion.identity;
        }

        public override void OnEndRide()
        {
            this.CachedTransform.parent = m_OriginalParent;
            CachedTransform.localPosition = GlobalTools.NavSamplePosition(Pos);
            m_CharacterController.enabled = true;
            if (m_Mount != null)
            {
                GameEntry.Level.DelRole(m_Mount);
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
            if (m_Vehicle != null && m_Vehicle != this)
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
            if (m_Vehicle != null && m_Vehicle != this) 
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

        public override void OnDead(DeadCommand ev)
        {
            base.OnDead(ev);

            OnPlayerDeadEventArgs args = ReferencePool.Acquire<OnPlayerDeadEventArgs>().Fill(ev.Type);
            GameEntry.Event.Fire(this, args);
        }



        public Vector3 GetPartnerPosBySort(PartnerSortType sortType)
        {
            switch (sortType)
            {
                case PartnerSortType.Middle:
                    return Pos + new Vector3(0, 0, 2);
                case PartnerSortType.Left:
                    return Pos + new Vector3(-2, 0, 0);
                case PartnerSortType.Right:
                    return Pos + new Vector3(2, 0, 0);
                default:
                    return Pos;
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


        #region Command Overide

        protected override CommandReplyType CheckRunTo(AutoMoveCommand cmd)
        {
            if (m_Vehicle.GetActorPathFinding().CanReachPosition(cmd.DestPosition) == false)
            {
                ShowWarning("300001");
                return CommandReplyType.NO;
            }

            return base.CheckRunTo(cmd);
        }

        #endregion

        private void LoadMount()
        {
            int mountID = UnityEngine.Random.Range(100001, 100003);

            TransformParam param= TransformParam.Create(CachedTransform.position, CachedTransform.eulerAngles);
            Mount = GameEntry.Level.AddRole<MountRole>(mountID, ActorType.Mount, BattleCampType.Neutral, param);
            m_Vehicle = m_Mount.Actor;

            var mountActor = Mount.Actor as ActorMount;
            Transform ridePoint = mountActor?.GetRidePoint();
            if (ridePoint != null)
            {
                CachedTransform.parent = ridePoint;
                CachedTransform.localPosition = Vector3.zero;
                CachedTransform.localRotation = Quaternion.identity;
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

        private void OnKillMonster(object sender, GameEventArgs e)
        {
            KillMonsterEventArgs ne = e as KillMonsterEventArgs;
            DRActorEntity drActorEntity = GameEntry.DataTable.GetDataTable<DRActorEntity>().GetDataRow(ne.MonsterId);
            if (drActorEntity.KillExp <= 0)
            {
                return;
            }

            int maxLevle = GameEntry.DataTable.GetDataTable<DRHeroLevel>().Count;

            if (drActorEntity.KillExp > 0)
            {
                TryAddExp(drActorEntity.KillExp);
            }
        }

        private void OnChangeAiMode(object sender, GameEventArgs e)
        {
            ChangeAiModeEventArgs ne = e as ChangeAiModeEventArgs;
            if (ne == null)
                return;

            this.m_ActorAI.ChangeAIMode(ne.AiMode);
        }

        private void TryUseSkill(object sender, GameEventArgs e)
        {
            SkillKeyDownEventArgs ne = e as SkillKeyDownEventArgs;
            if (ne == null)
            {
                Log.Error("EventArgs is invalid");
                return;
            }

            this.ExecuteCommand(new UseSkillCommand(ne.SkillPos));
        }

        private void ChangeEquipAvatar(object sender, GameEventArgs e)
        {
            ChangeEquipEventArgs ne = e as ChangeEquipEventArgs;
            int targetPos = ne.EquipPos;

        }

        private void OnChangeVehicle(object sender, GameEventArgs e)
        {
            if (m_Vehicle != this)
            {
                OnEndRide();
                return;
            }

            ExecuteCommand(new RideCommand());
        }

        private void OnChangePartner(object sender, GameEventArgs e)
        {
            ChangePartnerEventArgs ne = e as ChangePartnerEventArgs;
            if (ne == null)
                return;

            m_PlayerData.Partner1Id = ne.Partner01ID;
            m_PlayerData.Partner2Id = ne.Partner02ID;

            GameEntry.Level.AddPartner(this, 2, ne.Partner01ID);
            GameEntry.Level.AddPartner(this, 3, ne.Partner02ID);
        }

        private void TryAddExp(int exp)
        {
            if (exp <= 0)
                return;

            var dt = GameEntry.DataTable.GetDataTable<DRHeroLevel>();

            int maxLevel = dt.Count;
            if (m_PlayerData.Level == maxLevel)
            {
                ShowWarning("100005");
                return;
            }

            DRHeroLevel curLevelData = dt.GetDataRow(m_PlayerData.Level);
            if (curLevelData == null)
            {
                Log.Error("Can no get level{0} data.", m_PlayerData.Level);
                return;
            }

            int maxExp = curLevelData.RequireExp;
            m_PlayerData.Exp += exp;
            int offsetExp = m_PlayerData.Exp - maxExp;
            if (offsetExp > 0)
            {
                m_PlayerData.Exp = offsetExp;
                m_PlayerData.Level++;
                OnUpgradeLevel();
            }

            int curExp = m_PlayerData.Exp;

            RefreshHeroInfoEventArgs args = ReferencePool.Acquire<RefreshHeroInfoEventArgs>().FillExp(curExp, maxExp);
            GameEntry.Event.Fire(this, args);
        }

        private void OnUpgradeLevel()
        {
            this.ActorCard.SetLevel();

            RefreshBoardEventArgs eventArgs = ReferencePool.Acquire<RefreshBoardEventArgs>();
            int maxHp = Attrbute.GetValue(AttributeType.MaxHp);
            int curHp = Attrbute.GetValue(AttributeType.Hp);
            eventArgs.Fill(EntityId, maxHp, curHp, ActorCard.Level);
            GameEntry.Event.Fire(this, eventArgs);

            RefreshHeroInfoEventArgs args = ReferencePool.Acquire<RefreshHeroInfoEventArgs>().FillLevel(ActorCard.Level);
            GameEntry.Event.Fire(this, args);

            int id = GameEntry.Entity.GenerateTempSerialId();
            EffectData data = new EffectData(id, Constant.Define.LevelUpEffect);
            data.Owner = this;
            data.KeepTime = 3;
            data.BindType = EffectBindType.OwnFoot;
            data.DeadType = FlyObjDeadType.UntilLifeTimeEnd;
            data.Parent = CachedTransform;
            data.SetParent = true;
            GameEntry.Entity.ShowEffect(data);
        }

        private void OnChangeFightValue()
        {
            base.InitAttribute();
        }

        private void BroadcastHeroInfo()
        {
            int maxExp = GameEntry.DataTable.GetDataTable<DRHeroLevel>().GetDataRow(m_PlayerData.Level).RequireExp;
            RefreshHeroInfoEventArgs args = ReferencePool.Acquire<RefreshHeroInfoEventArgs>().Fill(m_PlayerData.Name, m_PlayerData.Level, m_PlayerData.Exp, maxExp, m_CurAttribute);
            GameEntry.Event.Fire(this, args);
        }
    }
}
