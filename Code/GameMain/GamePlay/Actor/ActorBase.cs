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

        public int Id { get; }

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
        protected IActorSkill m_ActorSkill;
        protected IAnimController m_AnimController;
        protected IAIPathFinding m_ActorPathFinding;
        protected IBuffCtrl m_BuffCtrl;
        protected ICommandReceiver m_CommandReceiver;

        protected int m_CurSkillId = 0;
        protected uint m_AIMark = 0;



        protected ActorBase(int entityId,int id, GameObject go, ActorType type, ActorBattleCampType camp,
            CharacterController cc, Animator anim)
        {
            if (id == 0 || go == null || cc == null || anim == null)
            {
                throw new GameFrameworkException("Construct Actor Fail.");
            }

            m_ActorSkill = new ActorSkill(this);

            Id = id;
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

        public void GotoEmptyFsm()
        {
            ChangeState<ActorEmptyFsm>();
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
                new ActorWalkFsm(), 
                new ActorTurnFsm(), 
                new ActorSkillFsm(), 
                new ActorDeadFsm(), 
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
            m_CommandReceiver.AddCommand<MoveCommand>(CommandType.Moveto, CheckRunTo);
        }

        #region Command
        //空闲
        protected virtual CommandReplyType CheckIdle(IdleCommand cmd)
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

        //寻路至
        protected virtual CommandReplyType CheckRunTo(MoveCommand cmd)
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
            //TODO
            //if (m_Vehicle.GetActorPathFinding().CanReachPosition(cmd.DestPosition) == false)
            //{
            //    ShowWarning("300001");
            //    return CommandReplyType.NO;
            //}
            m_ActorAI.ChangeAIMode(AIModeType.Auto);
            ChangeState<ActorRunFsm>(cmd);
            return CommandReplyType.YES; ;
        }

        //使用技能
        protected virtual CommandReplyType CheckUseSkill(UseSkillCommand cmd)
        {
            if (CacheTransform == null)
            {
                return CommandReplyType.NO;
            }
            if (CannotControlSelf())
            {
                ShowWarning("100012");
                return CommandReplyType.NO;
            }
            if (CurFsmStateType == ActorFsmStateType.FSM_SKILL)
            {
                return CommandReplyType.NO;
            }
            if (GetActorState(ActorStateType.IsRide))
            {
                ShowWarning("100011");
                return CommandReplyType.NO;
            }

            SkillTree skill = m_ActorSkill.GetSkill(cmd.SkillPos);
            if (skill == null) return CommandReplyType.NO;
            if (skill.IsInCD()) return CommandReplyType.NO;
            switch (skill.CostType)
            {
                case SkillCostType.Hp:
                {
                    bool success = UseHp(skill.CostNum);
                    if (!success)
                    {
                        return CommandReplyType.NO;
                    }
                }
                    break;
                case SkillCostType.Mp:
                {
                    bool success = UseMp(skill.CostNum);
                    if (!success)
                    {
                        return CommandReplyType.NO;
                    }
                }
                    break;
            }
            cmd.LastTime = skill.StateTime;

            ChangeState<ActorSkillFsm>(cmd);
            return CommandReplyType.YES;
        }

        //死亡
        protected virtual CommandReplyType CheckDead(DeadCommand cmd)
        {
            m_ActorSkill.Clear();
            if (GetActorState(ActorStateType.IsRide))
            {
                OnEndRide();
            }

            ChangeState<ActorDeadFsm>(cmd);
            return CommandReplyType.YES;
        }

        //转向
        protected virtual CommandReplyType CheckTurnTo(TurnToCommand cmd)
        {
            if (GetAIFeature(AIFeatureType.CanTurn) == false)
            {
                return CommandReplyType.NO;
            }
            if (CannotControlSelf())
            {
                return CommandReplyType.NO;
            }

            ChangeState<ActorTurnFsm>(cmd);
            return CommandReplyType.YES;
        }

        //强制移动
        protected virtual CommandReplyType CheckMoveTo(MoveCommand cmd)
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
                m_ActorAI.ChangeAIMode(AIModeType.Hand);

                ChangeState<ActorRunFsm>(cmd);
                return CommandReplyType.YES;
            }
            return CommandReplyType.NO;
        }

        //交谈
        protected virtual CommandReplyType CheckTalk(TalkCommand cmd)
        {
            ChangeState<ActorTalkFsm>(cmd);
            return CommandReplyType.YES;
        }

        //冰冻
        protected virtual CommandReplyType CheckFrost(FrostCommand cmd)
        {
            if (CannotControlSelf())
            {
                return CommandReplyType.NO;
            }
            if (GetActorState(ActorStateType.IsDivine) == true)
            {
                return CommandReplyType.NO;
            }
            m_ActorSkill.Clear();

            ChangeState<ActorFrostFsm>(cmd);
            return CommandReplyType.YES;
        }

        //昏迷
        protected virtual CommandReplyType CheckStun(StunCommand cmd)
        {
            if (CannotControlSelf())
            {
                return CommandReplyType.NO;
            }
            if (GetActorState(ActorStateType.IsDivine))
            {
                return CommandReplyType.NO;
            }
            m_ActorSkill.Clear();

            ChangeState<ActorStunFsm>(cmd);
            return CommandReplyType.YES;
        }

        ////麻痹
        //protected virtual CommandReplyType CheckPalsy(MBCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    mActorSkill.Clear();
        //    SendStateMessage(ActorFsmStateType.FSM_PARALY, cmd);
        //    return CommandReplyType.YES;
        //}

        ////睡眠
        //protected virtual CommandReplyType CheckSleep(SPCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    mActorSkill.Clear();
        //    SendStateMessage(ActorFsmStateType.FSM_SLEEP, cmd);
        //    return CommandReplyType.YES;
        //}

        ////致盲
        //protected virtual CommandReplyType CheckBlind(ZMCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    mActorSkill.Clear();
        //    SendStateMessage(ActorFsmStateType.FSM_BLIND, cmd);
        //    return CommandReplyType.YES;
        //}

        ////恐惧
        //protected virtual CommandReplyType CheckFear(FRCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    mActorSkill.Clear();
        //    SendStateMessage(ActorFsmStateType.FSM_FEAR, cmd);
        //    return CommandReplyType.YES;
        //}

        ////定身
        //protected virtual CommandReplyType CheckFixBody(FBCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    SendStateMessage(ActorFsmStateType.FSM_FIXBODY, cmd);
        //    return CommandReplyType.YES;
        //}

        ////受击
        //protected virtual CommandReplyType CheckWound(WDCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    mActorSkill.Clear();
        //    cmd.LastTime = mActorAction.GetAnimLength("hit");
        //    SendStateMessage(ActorFsmStateType.FSM_WOUND, cmd);
        //    return CommandReplyType.YES;
        //}

        ////击退
        //protected virtual CommandReplyType CheckBeatBack(BBCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    mActorSkill.Clear();
        //    SendStateMessage(ActorFsmStateType.FSM_BEATBACK, cmd);
        //    return CommandReplyType.YES;
        //}

        ////击飞
        //protected virtual CommandReplyType CheckBeatFly(BFCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    mActorSkill.Clear();
        //    cmd.LastTime = mActorAction.GetAnimLength("fly");
        //    SendStateMessage(ActorFsmStateType.FSM_BEATFLY, cmd);
        //    return CommandReplyType.YES;
        //}

        ////击倒
        //protected virtual CommandReplyType CheckBeatDown(BDCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    mActorSkill.Clear();
        //    cmd.LastTime = mActorAction.GetAnimLength("down");
        //    SendStateMessage(ActorFsmStateType.FSM_BEATDOWN, cmd);
        //    return CommandReplyType.YES;
        //}

        ////浮空
        //protected virtual CommandReplyType CheckFly(FLCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    mActorSkill.Clear();
        //    SendStateMessage(ActorFsmStateType.FSM_FLOATING, cmd);
        //    return CommandReplyType.YES;
        //}

        ////被勾取
        //protected virtual CommandReplyType CheckHook(HKCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    mActorSkill.Clear();
        //    SendStateMessage(ActorFsmStateType.FSM_HOOK, cmd);
        //    return CommandReplyType.YES;
        //}

        ////被抓取
        //protected virtual CommandReplyType CheckGrab(GBCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    mActorSkill.Clear();
        //    SendStateMessage(ActorFsmStateType.FSM_GRAB, cmd);
        //    return CommandReplyType.YES;
        //}

        ////变形
        //protected virtual CommandReplyType CheckVariation(VACommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Divine) == true)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    mActorSkill.Clear();
        //    SendStateMessage(ActorFsmStateType.FSM_VARIATION, cmd);
        //    return CommandReplyType.YES;
        //}

        ////骑乘
        //protected virtual CommandReplyType CheckRide(ERCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (Launcher.Instance.GetCurrSceneType() != ESceneType.City)
        //    {
        //        ShowWarning("300002");
        //        return CommandReplyType.NO;
        //    }
        //    if (m_ActorFsm == ActorFsmStateType.FSM_RUN || m_ActorFsm == ActorFsmStateType.FSM_WALK || m_ActorFsm == ActorFsmStateType.FSM_SKILL)
        //    {
        //        ShowWarning("300003");
        //        return CommandReplyType.NO;
        //    }
        //    if (mActorCard.GetMountID() == 0)
        //    {
        //        ShowWarning("300004");
        //        return CommandReplyType.NO;
        //    }
        //    OnBeginRide();
        //    return CommandReplyType.YES;
        //}

        //private CommandReplyType CheckReborn(RBCommand cmd)
        //{
        //    cmd.LastTime = mActorAction.GetAnimLength("fuhuo");
        //    SendStateMessage(ActorFsmStateType.FSM_REBORN, cmd);
        //    return CommandReplyType.YES;
        //}

        ////跳跃
        //protected virtual CommandReplyType CheckJump(JPCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (GetActorEffect(EActorEffect.Is_Ride) == true)
        //    {
        //        ShowWarning("100011");
        //        return CommandReplyType.NO;
        //    }
        //    SendStateMessage(ActorFsmStateType.FSM_JUMP);
        //    return CommandReplyType.YES;
        //}

        ////隐身
        //protected virtual CommandReplyType CheckSteal(YSCommand cmd)
        //{
        //    if (m_ActorFsm != ActorFsmStateType.FSM_IDLE ||
        //       m_ActorFsm != ActorFsmStateType.FSM_RUN ||
        //       m_ActorFsm != ActorFsmStateType.FSM_WALK)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    this.OnBeginStealth(cmd.LastTime);
        //    return CommandReplyType.YES;
        //}

        ////交互
        //private CommandReplyType CheckInterActive(ITCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (m_ActorFsm == ActorFsmStateType.FSM_DEAD)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (cmd.AnimName == "idle")
        //    {
        //        cmd.LastTime = 1;
        //    }
        //    else
        //    {
        //        cmd.LastTime = mActorAction.GetAnimLength(cmd.AnimName);
        //    }
        //    SendStateMessage(ActorFsmStateType.FSM_INTERACTIVE, cmd);
        //    return CommandReplyType.YES;
        //}

        //private CommandReplyType CheckMine(CJCommand cmd)
        //{
        //    if (CannotControlSelf())
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    if (m_ActorFsm == ActorFsmStateType.FSM_DEAD)
        //    {
        //        return CommandReplyType.NO;
        //    }
        //    cmd.LastTime = mActorAction.GetAnimLength("miss");
        //    SendStateMessage(ActorFsmStateType.FSM_INTERACTIVE, cmd);
        //    return CommandReplyType.YES;
        //}

        #endregion 

        #region Action

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
        /// 休闲
        /// </summary>
        public virtual void OnIdle()
        {
            StopPathFinding();
            this.m_AnimController.Play("idle", null, true);
        }

        /// <summary>
        /// 交谈
        /// </summary>
        public virtual void OnTalk(TalkCommand ev)
        {
            this.m_AnimController.Play("talk", null, true);
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
        /// 使用技能
        /// </summary>
        public virtual void OnUseSkill(UseSkillCommand ev)
        {
            StopPathFinding();
            LookAtEnemy();
            m_ActorSkill.UseSkill(ev.SkillPos);
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
        /// 击退
        /// </summary>
        public virtual void OnBeatBack(BeatBackCommand ev)
        {
            StopPathFinding();
        }

        /// <summary>
        /// 击倒
        /// </summary>
        public virtual void OnBeatDown()
        {
            StopPathFinding();
            m_AnimController.Play("down", GotoEmptyFsm, false);
        }

        /// <summary>
        /// 击飞
        /// </summary>
        public virtual void OnBeatFly()
        {
            StopPathFinding();
            m_AnimController.Play("fly", GotoEmptyFsm, false);
        }

        /// <summary>
        /// 受伤
        /// </summary>
        public virtual void OnWound()
        {
            StopPathFinding();
            m_AnimController.Play("hit", GotoEmptyFsm, false);
        }

        /// <summary>
        /// 行走
        /// </summary>
        public virtual void OnWalk()
        {
            StopPathFinding();
            m_AnimController.Play("walk", GotoEmptyFsm, true);
        }

        /// <summary>
        /// 晕眩
        /// </summary>
        public virtual void OnStun(float lastTime)
        {
            StopPathFinding();
            m_AnimController.Play("yun", GotoEmptyFsm, true, 1, lastTime);
        }

        /// <summary>
        /// 跳
        /// </summary>
        public void OnJump()
        {
            StopPathFinding();
            m_AnimController.Play("jump", GotoEmptyFsm, false);
        }

        /// <summary>
        /// 重生
        /// </summary>
        public void OnReborn()
        {
            AddHp(Attrbute.GetValue(ActorAttributeType.MaxHp), false);
            AddMp(Attrbute.GetValue(ActorAttributeType.MaxMp), false);
            m_CharacterController.enabled = true;
            m_AnimController.Play("fuhuo", GotoEmptyFsm, false);
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
        /// 采矿
        /// </summary>
        /// <param name="ev"></param>
        public void OnCollectMine(CollectMineCommand ev)
        {
            StopPathFinding();
            Action callback = delegate ()
            {
                GotoEmptyFsm();
                ev.OnFinish?.Invoke();
            };
            m_AnimController.Play("miss", callback, false);
        }

        /// <summary>
        /// 交互
        /// </summary>
        /// <param name="ev"></param>
        public void OnInteractive(InteractiveCommand ev)
        {
            StopPathFinding();
            Action callback = delegate ()
            {
                GotoEmptyFsm();
                ev.OnFinish?.Invoke();
            };
            m_AnimController.Play(ev.AnimName, callback, false);
        }

        protected void OnBeginStealth(float lifeTime)
        {
            SetActorState(ActorStateType.IsStealth, true);
            OnFadeOut();
        }

        protected void OnEndStealth()
        {
            SetActorState(ActorStateType.IsStealth, false);
            OnFadeIn();
        }

        protected void OnFadeOut()
        {
            SetAlphaVertexColorOff(0.1f);
           // mActorBuff.SetAllParticleEnabled(false);
           //TODO
        }

        protected void OnFadeIn()
        {
            SetAlphaVertexColorOn(0.1f);
           // mActorBuff.SetAllParticleEnabled(true);

        }

        protected void OnDeadEnd()
        {
            m_ActorAI.Clear();
            GameEntry.Entity.HideEntity(EntityId);
        }
        #endregion

    }
}
