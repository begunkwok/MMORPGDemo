using System;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Fsm;
using UnityEngine;

namespace GameMain
{
    /// <summary>
    /// 行动者基类
    /// </summary>
    public abstract class ActorBase : IActor
    {
        #region Property

        public Transform CacheTransform { get; }

        public Transform BornParam { get; }

        public GameObject EntityGo { get; }

        public int EntityId { get; }

        public ActorType ActorType { get; }

        public ActorBattleCampType Camp { get; }

        public IAttribute Attrbute { get; protected set; }

        public ActorBase Target
        {
            get
            {
                if (m_Target == null)
                {
                    Log.Error("Target is null");
                    return null;
                }

                return m_Target;
            }
        }

        public ActorFsmStateType CurFsmStateType
        {
            get
            {
                if (m_ActorFsm == null)
                {
                    return ActorFsmStateType.FSM_BORN;
                }
                ActorFsmStateBase state = m_ActorFsm.CurrentState as ActorFsmStateBase;

                if (state != null)
                    return state.StateType;
                else
                    return ActorFsmStateType.FSM_BORN;
            }
        }

        public Vector3 Dir => CacheTransform.forward;

        public Vector3 Euler => CacheTransform.localEulerAngles;

        public Vector3 Pos => CacheTransform.position;

        public float Height => m_CharacterController.height * CacheTransform.localScale.x;

        public float Radius => m_CharacterController.radius * CacheTransform.localScale.x;

        public bool IsDead => CurFsmStateType == ActorFsmStateType.FSM_DEAD;

        #endregion

        protected Dictionary<AIFeatureType, bool> m_AIFeatures = new Dictionary<AIFeatureType, bool>();
        protected Dictionary<ActorStateType, bool> m_ActorStates = new Dictionary<ActorStateType, bool>();

        protected List<ActorBase> m_Enemys = new List<ActorBase>();
        protected List<ActorBase> m_Allys = new List<ActorBase>();
        protected List<ActorBase> m_Targets = new List<ActorBase>();

        protected CharacterController m_CharacterController;
        protected Animator m_Animator;
        protected Transform[] m_Hands;

        protected ActorBase m_Target;
        protected ActorBase m_Host;
        protected IFsm<ActorBase> m_ActorFsm;
        protected IActorAI m_ActorAI;
        protected IAnimController m_AnimController;
        protected IAIPathFinding m_ActorPathFinding;
        protected IBuffCtrl m_BuffCtrl;
        protected ICommandReceiver m_CommandReceiver;

        protected int m_CurSkillId = 0;
        protected uint m_AIMark = 0;



        protected ActorBase(int entityId, GameObject go, ActorType type, ActorBattleCampType camp,
            CharacterController cc, Animator anim)
        {
            if (entityId == 0 || go == null || cc == null || anim == null)
            {
                throw new GameFrameworkException("Construct Actor Fail.");
            }

            EntityId = entityId;
            ActorType = type;
            Camp = camp;
            EntityGo = go;
            CacheTransform = go.transform;
            BornParam = go.transform;
            m_CharacterController = cc;
            m_Animator = anim;
        }


        public virtual void Init()
        {
            InitAttribute();
            InitLayer();
            InitCommands();
            InitAnim();
            InitFeature();
            InitState();
            InitFsm();
            InitAI();

            CreateBoard();
        }

        public virtual void Step()
        {
            if (CacheTransform == null || m_ActorFsm == null || m_ActorAI == null || m_ActorPathFinding == null)
            {
                return;
            }

            m_ActorAI.Step();
            m_ActorPathFinding.Step();
            m_AnimController.Step();
        }

        public virtual void Pause(bool isPause)
        {
            m_AnimController.SetEnable(!isPause);
            if(!isPause)
                ChangeState<ActorIdleFsm>();
        }

        public virtual void Destory()
        {
            m_ActorAI.Clear();         
        }

        public virtual void UpdateCurAttribute(bool init = false)
        {
            //TODO
        }


        protected virtual void UpdateHealth()
        {
            //TODO
        }

        protected virtual void UpdatePower()
        {
            //TODO
        }

        public virtual int Attack(IActor defender, int value)
        {
            if (defender == null || value <= 0)
            {
                return 0;
            }
            float v = value - defender.Attrbute.GetValue(ActorAttributeType.Defense)*0.2f;
            v = v*Constant.Define.DamageRatio;
            if (v <= 0)
            {
                v = UnityEngine.Random.Range(3, 7);
            }

            float cRate = this.Attrbute.GetValue(ActorAttributeType.Crit)*0.01f;
            float bRate = this.Attrbute.GetValue(ActorAttributeType.CritDamage)*0.01f;
            bool critical = GlobalTools.IsTrigger(cRate);
            if (critical)
            {
                v = (v*(1 + bRate));
            }
            int dmg = Mathf.FloorToInt(UnityEngine.Random.Range(0.85f, 1.08f)*v);
            defender.TakeDamage(this, dmg, critical);
            return dmg;
        }

        public virtual int SuckBlood(IActor defender, int value, float suckRate)
        {
            if (defender == null || value <= 0)
            {
                return 0;
            }
            int v = Attack(defender, value);
            if (suckRate < 0)
            {
                suckRate = 0;
            }
            int suckValue = Mathf.FloorToInt(v * suckRate);
            AddHp(suckValue, true);
            return suckValue;
        }

        public virtual void TakeDamage(IActor attacker, int damage, bool strike)
        {
            ShowFlyword(strike ? FlyWordType.CritHurt : FlyWordType.Hurt, damage);

            int curHp = Attrbute.GetValue(ActorAttributeType.Hp);

            if (curHp > damage)
            {
                Attrbute.UpdateValue(ActorAttributeType.Hp, curHp - damage);
            }
            else
            {
                Attrbute.UpdateValue(ActorAttributeType.Hp, 0);
            }
            UpdateHealth();
            if (curHp <= 0)
            {
                ExecuteCommand(new DeadCommand(ActorDeadType.Normal));
            }
        }

        public virtual void SetTarget(ActorBase actor)
        {
            if (actor == null)
            {
                m_Target = null;
                return;
            }
            if (m_Target == actor)
            {
                return;
            }
            m_Target = actor;
            CacheTransform.LookAt(m_Target.CacheTransform);
        }

        public virtual void SetHost(ActorBase actor)
        {
            m_Host = actor;
        }

        public virtual void ChangeState<T>(ICommand command = null) where T : ActorFsmStateBase
        {
            if (m_ActorFsm == null)
            {
                Log.Error("Please set ActorFsm first");
                return;
            }

            if (!m_ActorFsm.HasState<T>())
            {
                Log.Error("Can no find state" + typeof(T));
                return;
            }

            ActorFsmStateBase state = m_ActorFsm.GetState<T>();

            if (CurFsmStateType == ActorFsmStateType.FSM_DEAD && state.StateType != ActorFsmStateType.FSM_REBORN)
            {
                return;
            }

            m_ActorFsm.GetState<T>().SetCommand(command);
            state.ChangeState<T>();
        }

        public virtual void TranslateTo(Vector3 destPosition, bool idle)
        {
            if (CacheTransform == null)
            {
                return;
            }
            CacheTransform.position = destPosition;
            if (idle)
            {
                ChangeState<ActorEmptyFsm>();
            }
        }

        public virtual void MoveTo(Vector3 destPosition)
        {
            this.SetActorState(ActorStateType.IsAutoToMove, true);
            m_ActorPathFinding.SetDestPosition(destPosition);
        }

        public virtual void StopPathFinding()
        {
            //this.SetActorState(ActorStateType.IsAutoToMove, false);
            //m_ActorPathFinding.StopPathFinding();
        }

        public virtual void SetAlphaVertexColorOff(float time)
        {
            throw new NotImplementedException();
            //TODO:
        }

        public virtual void SetAlphaVertexColorOn(float time)
        {
            throw new NotImplementedException();
            //TODO:
        }

        public virtual bool CannotControlSelf()
        {
            switch (CurFsmStateType)
            {
                case ActorFsmStateType.FSM_STUN:
                case ActorFsmStateType.FSM_FROST:
                case ActorFsmStateType.FSM_FEAR:
                case ActorFsmStateType.FSM_BEATFLY:
                case ActorFsmStateType.FSM_BEATDOWN:
                case ActorFsmStateType.FSM_BEATBACK:
                case ActorFsmStateType.FSM_DROP:
                case ActorFsmStateType.FSM_DEAD:
                case ActorFsmStateType.FSM_FLOATING:
                case ActorFsmStateType.FSM_HOOK:
                case ActorFsmStateType.FSM_VARIATION:
                case ActorFsmStateType.FSM_WOUND:
                case ActorFsmStateType.FSM_GRAB:
                case ActorFsmStateType.FSM_SLEEP:
                case ActorFsmStateType.FSM_PARALY:
                case ActorFsmStateType.FSM_BLIND:
                case ActorFsmStateType.FSM_JUMP:
                case ActorFsmStateType.FSM_REBORN:
                    return true;
                default:
                    return false;
            }
        }

        public virtual bool CanActManully()
        {
            switch (CurFsmStateType)
            {
                case ActorFsmStateType.FSM_IDLE:
                case ActorFsmStateType.FSM_RUN:
                case ActorFsmStateType.FSM_WALK:
                case ActorFsmStateType.FSM_SKILL:
                    return true;
                default:
                    return false;
            }
        }

        public virtual void Clear()
        {
            RemoveBoard();
            RemoveEffect();
        }



        public Transform[] GetHands()
        {
            if (m_Hands == null && CacheTransform != null)
            {
                m_Hands = new Transform[2];
                m_Hands[0] = GlobalTools.GetBone(CacheTransform, "Bip01 L Hand");
                m_Hands[1] = GlobalTools.GetBone(CacheTransform, "Bip01 R Hand");
            }
            return m_Hands;
        }

        public Vector3 GetBind(ActorBindPosType bindType, Vector3 offset)
        {
            switch (bindType)
            {
                case ActorBindPosType.Body:
                    return Pos + new Vector3(0, Height * 0.5f, 0) + offset;
                case ActorBindPosType.Head:
                    return Pos + new Vector3(0, Height, 0) + offset;
                case ActorBindPosType.Foot:
                    return Pos + offset;
                default:
                    return Pos;
            }
        }

        public IAnimController GetAnimController()
        {
            return m_AnimController;
        }

        public IAIPathFinding GetActorPathFinding()
        {
            return m_ActorPathFinding;
        }

        public void ApplyAnimator(bool enable)
        {
            if (m_AnimController == null)
            {
                Log.Error("Please set AnimController first.");
                return;
            }

            m_AnimController.SetEnable(enable);
        }

        public void ApplyRootMotion(bool enable)
        {
            if (m_AnimController == null)
            {
                Log.Error("Please set AnimController first.");
                return;
            }

            m_AnimController.SetRootMotionEnable(enable);
        }

        public void ApplyCharacterCtrl(bool enabled)
        {
            if (m_CharacterController == null)
            {
                Log.Error("CharacterController is null.");
                return;
            }
            m_CharacterController.enabled = enabled;
        }

        public void ChangeModel(int id)
        {
            //TODO
        }

        public void SetActorState(ActorStateType type, bool flag)
        {
            m_ActorStates[type] = flag;
        }

        public bool GetActorState(ActorStateType type)
        {
            bool flag;
            m_ActorStates.TryGetValue(type, out flag);
            return flag;
        }

        public bool GetAIFeature(AIFeatureType type)
        {
            bool flag;
            m_AIFeatures.TryGetValue(type, out flag);
            return flag;
        }

        public CommandReplyType ExecuteCommand<T>(T command) where T : ICommand
        {
            if (!m_CommandReceiver.HasCommand(command.CommandType))
            {
                Log.Error("Can no find Command");
                return CommandReplyType.NO;
            }

            return m_CommandReceiver.ExecuteCommand(command);
        }

        public bool CheckActorState(ActorStateType type)
        {
            bool flag;
            m_ActorStates.TryGetValue(type, out flag);
            return flag;
        }

        public void AddHp(int hp, bool showFlyword)
        {
            if (IsDead)
            {
                return;
            }
            int newHp = Attrbute.GetValue(ActorAttributeType.Hp);
            if (newHp + hp > Attrbute.GetValue(ActorAttributeType.MaxHp))
            {
                newHp = Attrbute.GetValue(ActorAttributeType.MaxHp);
            }
            else
            {
                newHp += hp;
            }
            Attrbute.UpdateValue(ActorAttributeType.Hp, newHp);
            if (showFlyword)
            {
                ShowFlyword(FlyWordType.HpHeal, hp);
            }
            this.UpdateHealth();
        }

        public void AddMp(int mp, bool showFlyword)
        {
            if (IsDead)
            {
                return;
            }
            int newMp = Attrbute.GetValue(ActorAttributeType.Mp);
            if (newMp + mp > Attrbute.GetValue(ActorAttributeType.MaxMp))
            {
                newMp = Attrbute.GetValue(ActorAttributeType.MaxMp);
            }
            else
            {
                newMp += mp;
            }
            Attrbute.UpdateValue(ActorAttributeType.Mp, newMp);
            if (showFlyword)
            {
                ShowFlyword(FlyWordType.MpHeal, mp);
            }
            this.UpdatePower();
        }

        public bool UseHp(int use)
        {
            int hp = Attrbute.GetValue(ActorAttributeType.Hp);
            if (hp > use)
            {
                Attrbute.UpdateValue(ActorAttributeType.Hp, hp - use);
                UpdatePower();
                return true;
            }

            return false;
        }

        public bool UseMp(int use)
        {
            int mp = Attrbute.GetValue(ActorAttributeType.Mp);
            if (mp > use)
            {
                Attrbute.UpdateValue(ActorAttributeType.Mp, mp - use);
                UpdatePower();
                return true;
            }

            return false;
        }

        public bool IsFullHp()
        {
            int hp = Attrbute.GetValue(ActorAttributeType.Hp);
            int maxHp = Attrbute.GetValue(ActorAttributeType.MaxHp);
            return hp >= maxHp;
        }

        public bool IsFullMp()
        {
            int mp = Attrbute.GetValue(ActorAttributeType.Mp);
            int maxMp = Attrbute.GetValue(ActorAttributeType.MaxMp);
            return mp >= maxMp;
        }





        protected bool IsEnemy(ActorBase actor)
        {
            if (actor == null)
            {
                Log.Error("Actor is null.");
                return false;
            }

            return GetCamp(actor) == ActorBattleCampType.Enemy;
        }

        protected bool IsAlly(ActorBase actor)
        {
            if (actor == null)
            {
                Log.Error("Actor is null.");
                return false;
            }

            return GetCamp(actor) == ActorBattleCampType.Ally;
        }

        protected List<ActorBase> GetAllEnemy()
        {
            m_Enemys.Clear();
            FindActorsByCamp(ActorBattleCampType.Enemy, ref m_Enemys, true);
            return m_Enemys;
        }

        protected List<ActorBase> GetAllAlly()
        {
            m_Allys.Clear();
            FindActorsByCamp(ActorBattleCampType.Ally,ref m_Allys);
            return m_Allys;
        }

        protected void FindActorsByCamp(ActorBattleCampType actorCamp, ref List<ActorBase> list, bool ignoreStealth = false)
        {
            //TODO 从关卡获取敌人
        }

        protected ActorBattleCampType GetCamp(ActorBase actor)
        {
            if (actor == null)
            {
                Log.Error("Actor is null.");
                return ActorBattleCampType.None;
            }

            return actor.Camp;
        }

        protected ActorBase GetNearestEnemy(float radius = 100)
        {
            List<ActorBase> actors = GetAllEnemy();
            ActorBase nearest = null;
            float min = radius;
            for (int i = 0; i < actors.Count; i++)
            {
                float dist = GlobalTools.GetHorizontalDistance(actors[i].CacheTransform.position, this.CacheTransform.position);
                if (dist < min)
                {
                    min = dist;
                    nearest = actors[i];
                }
            }
            return nearest;
        }

        protected void LookAtEnemy()
        {
            if (m_Target == null || m_Target.IsDead || !IsEnemy(m_Target) || m_Target.CheckActorState(ActorStateType.IsStealth))
            {
                m_Target = null;
            }

            ActorBase enemy = GetNearestEnemy(m_ActorAI.WaringDist);
            this.SetTarget(enemy);
            if (m_Target != null)
            {
                CacheTransform.LookAt(new Vector3(m_Target.Pos.x, Pos.y, m_Target.Pos.z));
            }
        }

        protected void CreateBoard()
        {
            //TODO 创建头上ui
        }

        protected void RemoveBoard()
        {
            //TODO 移除头上ui
        }

        protected void RemoveEffect()
        {
            //TODO
        }

        protected void ShowFlyword(FlyWordType type, int value)
        {
            if (IsDead)
            {
                return;
            }

            //TODO:显示飞字
        }

        protected void ShowWarning(string localKey)
        {
            if (m_ActorAI.AIMode == AIModeType.Hand)
            {
                //TODO:显示警告
            }
        }

        protected void InitAttribute(bool init = false)
        {
            //TODO
            Attrbute = null;
        }

        protected void InitFeature()
        {
            for (int i = 0; i < Enum.GetNames(typeof(AIFeatureType)).Length; i++)
            {
                AIFeatureType type = (AIFeatureType)i;
                bool flag = GlobalTools.GetValueFromBitMark(m_AIMark, i);
                this.m_AIFeatures[type] = flag;
            }
        }

        protected void InitLayer()
        {
            switch (ActorType)
            {
                case ActorType.Player:
                    GlobalTools.SetLayer(EntityGo, Constant.Layer.PlayerId);
                    break;
                case ActorType.Monster:
                    GlobalTools.SetLayer(EntityGo, Constant.Layer.MonsterId);
                    break;
                case ActorType.Npc:
                    GlobalTools.SetLayer(EntityGo, Constant.Layer.NpcId);
                    break;
                case ActorType.Pet:
                    GlobalTools.SetLayer(EntityGo, Constant.Layer.PetId);
                    break;
                case ActorType.Mount:
                    GlobalTools.SetLayer(EntityGo, Constant.Layer.MountId);
                    break;
                case ActorType.Partner:
                    GlobalTools.SetLayer(EntityGo, Constant.Layer.PartnerId);
                    break;
            }
        }

        protected void InitState()
        {
            this.m_AIFeatures[AIFeatureType.CanMove] = true;
            this.m_AIFeatures[AIFeatureType.CanTurn] = true;

            this.m_ActorStates[ActorStateType.IsRide] = false;
            this.m_ActorStates[ActorStateType.IsSilent] = false;
            this.m_ActorStates[ActorStateType.IsDivine] = false;
            this.m_ActorStates[ActorStateType.IsStory] = false;
            this.m_ActorStates[ActorStateType.IsTask] = false;
            this.m_ActorStates[ActorStateType.IsAutoToMove] = false;
            this.m_ActorStates[ActorStateType.IsStealth] = false;

            this.ApplyAnimator(true);
        }

        protected void InitAnim()
        {
            if (m_Animator == null)
            {
                Log.Error("Animator is null.");
                return;
            }

            m_Animator.enabled = true;
            m_Animator.applyRootMotion = true;
            m_Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
            m_AnimController = new AnimController(m_Animator);
        }

        protected void InitFsm()
        {
            FsmState<ActorBase>[] states =
            {
                new ActorEmptyFsm(),
                new ActorIdleFsm(),
                new ActorRunFsm(), 
            };
            m_ActorFsm = GameEntry.Fsm.CreateFsm(this, states);
            m_ActorFsm.Start<ActorIdleFsm>();
        }

        protected void InitAI()
        {
            //TODO;
            m_ActorPathFinding = new AIPathFinding(this);
            m_ActorAI = new ActorFsmAI(this, AIModeType.Hand, 10f,10f,10f,0.2f);
            m_ActorAI.Start();
        }

        protected void InitCommands()
        {
            m_CommandReceiver = new CommandReceiver();
            m_CommandReceiver.AddCommand<IdleCommand>(CommandType.Idle, CheckIdle);
            m_CommandReceiver.AddCommand<MoveCommand>(CommandType.Moveto, CheckMoveTo);
        }

        #region Command
        //空闲
        protected virtual CommandReplyType CheckIdle(ICommand cmd)
        {
            if (CacheTransform == null)
            {
                return CommandReplyType.NO;
            }
            if (CannotControlSelf())
            {
                return CommandReplyType.NO;
            }
            if (CurFsmStateType == ActorFsmStateType.FSM_SKILL)
            {
                return CommandReplyType.NO;
            }

            ChangeState<ActorIdleFsm>(cmd);
            return CommandReplyType.YES;
        }

        //强制移动
        protected virtual CommandReplyType CheckMoveTo(ICommand cmd)
        {
            if (CannotControlSelf())
            {
                return CommandReplyType.NO;
            }
            if (CurFsmStateType == ActorFsmStateType.FSM_SKILL)
            {
                return CommandReplyType.NO;
            }
            if (GetAIFeature(AIFeatureType.CanMove) == false)
            {
                return CommandReplyType.NO;
            }
            if (this is ActorPlayer)
            {
                this.m_ActorAI.ChangeAIMode(AIModeType.Hand);
                ChangeState<ActorRunFsm>(cmd);
                return CommandReplyType.YES;
            }
            return CommandReplyType.NO;
        }

        #endregion 

        #region Action

        /// <summary>
        /// 休闲
        /// </summary>
        public virtual void OnIdle()
        {
            StopPathFinding();
            this.m_AnimController.Play("idle", null, true);
        }

        /// <summary>
        /// 死亡
        /// </summary>
        public virtual void OnDead(DeadCommand ev)
        {
            StopPathFinding();
            this.m_AnimController.Play("die");
            Attrbute.UpdateValue(ActorAttributeType.Hp, 0);
            Attrbute.UpdateValue(ActorAttributeType.Mp, 0);
            this.Clear();
            this.ApplyCharacterCtrl(false);
            this.m_ActorAI.Clear();
            //DBEntiny db = ZTConfig.Instance.GetDBEntiny(Id);
            //if ((this is ActorMainPlayer) == false)
            //{
            //    LevelData.AllActors.Remove(this);
            //}
            //switch (ActorType)
            //{
            //    case EActorType.PLAYER:
            //        if (this is ActorMainPlayer)
            //        {
            //            ZTLevel.Instance.OnMainPlayerDead();
            //        }
            //        break;
            //    case EActorType.MONSTER:
            //        ZTEvent.FireEvent(EventID.RECV_KILL_MONSTER, GUID, Id);
            //        ZTTimer.Instance.Register(1.5f, OnDeadEnd);
            //        break;
            //}
        }

        /// <summary>
        /// 转向
        /// </summary>
        public virtual void OnTurnTo(TurnToCommand ev)
        {
            Vector3 pos = new Vector3(ev.LookDirection.x, CacheTransform.position.y, ev.LookDirection.z);
            CacheTransform.LookAt(pos);
        }

        /// <summary>
        /// 强制移动
        /// </summary>
        public virtual void OnForceToMove(MoveCommand ev)
        {
            StopPathFinding();
            Vector3 delta = ev.Delta;
            int speed = 5; //Attrbute.GetValue(ActorAttributeType.Speed);

            CacheTransform.LookAt(new Vector3(CacheTransform.position.x + delta.x, CacheTransform.position.y,
            CacheTransform.position.z + delta.y));
            m_CharacterController.SimpleMove(delta*speed);
            this.m_AnimController.Play("run", null, true);
        }

        /// <summary>
        /// 自动寻路
        /// </summary>
        public virtual void OnPursue(AutoMoveCommand ev)
        {
            this.m_ActorPathFinding.SetOnFinished(ev.Callback);
            MoveTo(ev.DestPosition);
            this.m_AnimController.Play("run", null, true);
        }

        /// <summary>
        /// 到达目标地
        /// </summary>
        public virtual void OnArrive()
        {
            ChangeState<ActorEmptyFsm>();
            if (m_Host != null && m_Host.GetActorState(ActorStateType.IsRide))
            {
                m_Host.OnArrive();
            }
        }

        /// <summary>
        /// 开始骑坐骑
        /// </summary>
        public virtual void OnBeginRide()
        {

        }

        /// <summary>
        /// 结束骑坐骑
        /// </summary>
        public virtual void OnEndRide()
        {

        }

        /// <summary>
        /// 交谈
        /// </summary>
        public virtual void OnTalk(TalkCommand ev)
        {
            this.m_AnimController.Play("talk", null, true);
        }



        #endregion

    }
}
