using System;
using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Level")]
    public partial class LevelComponent : GameFrameworkComponent,ICustomComponent
    {
        public int LevelID;
        public string MapName = string.Empty;
        public string MapPath = string.Empty;
        public LevelType CurLevelType { get; private set; }
        public SceneId CurSceneId { get; private set; }
        public SceneType CurSceneType { get; private set; }
        public MapConfig Config { get; private set; }
        public PlayerRole Player { get; private set; }
        public float CurTime => Time.realtimeSinceStartup - m_StartTime;

        private List<LevelTask> m_OnLoadNewSceneTasks = new List<LevelTask>();
        private Dictionary<MapHolderType, LevelElement> m_Holders = new Dictionary<MapHolderType, LevelElement>();
        private List<RoleEntityBase> m_AllRoles = new List<RoleEntityBase>();
        private Dictionary<BattleCampType, List<RoleEntityBase>> m_CampActors = new Dictionary<BattleCampType, List<RoleEntityBase>>
        {
            {BattleCampType.Ally, new List<RoleEntityBase>()},
            {BattleCampType.Enemy, new List<RoleEntityBase>()},
            {BattleCampType.Neutral, new List<RoleEntityBase>()},
            {BattleCampType.Other, new List<RoleEntityBase>()}
        };

        private float m_StartTime;
        private float m_EndTime;
        private bool m_Victory;

        public void Init()
        {
            m_StartTime = 0;
            m_EndTime = 0;
            m_Victory = false;

            InitHolder();       
        }

        public void Update()
        {
            FlyWordManager.Instance.Step();
        }

        public void Clear()
        {
            CancelInvoke();

            m_OnLoadNewSceneTasks.Clear();

            GameEntry.BT.Clear();

            for (int i = m_AllRoles.Count - 1; i >= 0; i--)
            {
                RoleEntityBase role = m_AllRoles[i];
                m_AllRoles.RemoveAt(i);
                GameEntry.Entity.HideEntity(role.Id);
            }

            Player = null;

            foreach (KeyValuePair<MapHolderType, LevelElement> current in m_Holders)
            {
                current.Value.transform.DestroyChildren();
            }

            foreach (var current in m_CampActors)
            {
                current.Value.Clear();
            }

            FlyWordManager.Instance.Clear();
        }

        public void InitHolder()
        {
            if (transform.childCount > 0)
            {
                return;
            }

            AddHolder<HolderBorn>(MapHolderType.Born);
            AddHolder<HolderMonsterGroup>(MapHolderType.MonsterGroup);
            AddHolder<HolderWaveSet>(MapHolderType.WaveSet);
            AddHolder<HolderBarrier>(MapHolderType.Barrier);
            AddHolder<HolderRegion>(MapHolderType.Region);
            AddHolder<HolderPortal>(MapHolderType.Portal);
            AddHolder<HolderNpc>(MapHolderType.Npc);
            AddHolder<HolderMineGroup>(MapHolderType.MineGroup);
            AddHolder<HolderRole>(MapHolderType.Role);

            foreach (KeyValuePair<MapHolderType, LevelElement> current in m_Holders)
            {
                Transform trans = current.Value.transform;
                trans.parent = transform;
                trans.localPosition = Vector3.zero;
                trans.localRotation = Quaternion.identity;
                trans.localScale = Vector3.one;
            }
        }

        public void EnterLevel(int levelId,SceneId sceneId)
        {
            this.LevelID = levelId;
            this.CurSceneId = sceneId;

            m_StartTime = 0;
            m_EndTime = 0;
            m_Victory = false;

            GameEntry.Event.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
            GameEntry.Event.Subscribe(OnPlayerDeadEventArgs.EventId, OnPlayerDead);
            GameEntry.Event.Subscribe(PassLevelEventArgs.EventId,OnPassLevel);

            string assetName = AssetUtility.GetLevelConfigAsset(levelId.ToString());
            Config = new MapConfig();
            Config.Load(assetName);

            this.OnLevelStart();
        }

        public void LeaveCurrentLevel()
        {
            GameEntry.Event.Unsubscribe(OnPlayerDeadEventArgs.EventId, OnPlayerDead);
            GameEntry.Event.Unsubscribe(PassLevelEventArgs.EventId, OnPassLevel);
            GameEntry.Event.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);

            OnLevelEnd();
            Clear();
        }

        public LevelElement GetHolder(MapHolderType type)
        {
            LevelElement holder = null;
            m_Holders.TryGetValue(type, out holder);
            return holder;
        }

        public void AddHolder<T>(MapHolderType type) where T : LevelElement
        {
            LevelElement holder = null;
            m_Holders.TryGetValue(type, out holder);
            if (holder == null)
            {
                holder = new GameObject(typeof(T).Name).AddComponent<T>();
                m_Holders[type] = holder;
            }
        }

        public PlayerRole CreatePlayer(int id)
        {
            Player = AddRole<PlayerRole>(id, ActorType.Player, BattleCampType.Ally, TransformParam.Default);
            Player = Player;
            GameEntry.Camera.SwitchCameraBehaviour(CameraBehaviourType.LockLook);
            return Player;
        }

        public EnemyRole CreateEnemy(int id, TransformParam param)
        {
            EnemyRole enemy = AddRole<EnemyRole>(id, ActorType.Monster, BattleCampType.Enemy, param);
            return enemy;
        }

        public LevelObject CreateLevelObject(int id, TransformParam param = null)
        {
            int entityId = GameEntry.Entity.GenerateSerialId();
            LevelObjectData levelObjectData = new LevelObjectData(entityId, id);
            LevelObject levelObject = GameEntry.Entity.ShowEntity<LevelObject>(levelObjectData);

            if (param != null && levelObject != null)
            {
                levelObject.CachedTransform.position = param.Position;
                levelObject.CachedTransform.eulerAngles = param.EulerAngles;
                levelObject.CachedTransform.localScale = param.Scale;
            }

            return levelObject;
        }

        public void AddPartner(PlayerRole host, int pos, int id)
        {
            if (id <= 0)
                return;

            if (host?.Actor == null)
                return;
            
            PartnerSortType sort = (PartnerSortType)pos;
            ActorPlayer actorPlayer = host.Actor as ActorPlayer;
            if (actorPlayer == null)
            {
                return;
            }

            Vector3 pPos = actorPlayer.GetPartnerPosBySort(sort);

            TransformParam param = TransformParam.Create(pPos, host.CachedTransform.eulerAngles, Vector3.one * 1.5f);

            ActorBase partner = AddRole<RoleEntityBase>(id, ActorType.Partner, actorPlayer.Camp, param).Actor;
            if (partner == null)
            {
                return;
            }

            switch (sort)
            {
                case PartnerSortType.Left:
                    actorPlayer.Partner1 = partner;
                    break;
                case PartnerSortType.Right:
                    actorPlayer.Partner2 = partner;
                    break;
            }
            partner.Sort = sort;
        }

        public bool GetPortalByDestMapId(int id, ref Vector3 pos)
        {
            HolderPortal pHolder = (HolderPortal)GetHolder(MapHolderType.Portal);
            for (int i = 0; i < pHolder.Elements.Count; i++)
            {
                LevelPortal p = pHolder.Elements[i];
                if (p.DestMapID == id && p.Region != null)
                {
                    pos = p.Region.Position;
                    return true;
                }
            }
            return false;
        }

        public T AddRole<T>(int id, ActorType type, BattleCampType camp, Vector3 pos, Vector3 angle) where T : RoleEntityBase
        {
            return AddRole<T>(id, type, camp, TransformParam.Create(pos, angle));
        }

        public T AddRole<T>(int id, ActorType type, BattleCampType camp, Vector3 pos, Vector3 angle, Vector3 scale) where T : RoleEntityBase
        {
            return AddRole<T>(id, type, camp, TransformParam.Create(pos, angle, scale));
        }

        public T AddRole<T>(int entityTypeId, ActorType type, BattleCampType camp, TransformParam param)
            where T : RoleEntityBase
        {
            int entityId = GameEntry.Entity.GenerateSerialId();
            RoleEntityData roleData = new RoleEntityData(entityId, entityTypeId, type, camp);
            RoleEntityBase role = GameEntry.Entity.ShowRole<T>(roleData);

            if (role != null)
            {
                param.Position = GlobalTools.NavSamplePosition(param.Position);

                if (role.CachedTransform != null)
                {
                    m_AllRoles.Add(role);
                    m_CampActors[camp].Add(role);
                    role.CachedTransform.position = param.Position;
                    role.CachedTransform.eulerAngles = param.EulerAngles;
                    role.CachedTransform.localScale = param.Scale;
                }
            }

            return role as T;
        }

        public bool DelRole(RoleEntityBase role)
        {
            if (role?.Actor == null)
                return false;

            m_AllRoles.Remove(role);
            m_CampActors[role.Actor.Camp].Remove(role);
            GameEntry.Entity.HideEntity(role.Id);
            return true;
        }

        public void CreateMapEvent<T, S>(MapEvent pData, LevelRegion pRegion, LevelContainerBase<T> pHolder, List<S> pElemDataList) where T : LevelElement where S : MapElement
        {
            T tElement = null;
            if (pData.Active)
            {
                for (int i = 0; i < pElemDataList.Count; i++)
                {
                    if (pElemDataList[i].Id == pData.Id)
                    {
                        tElement = pHolder.AddElement();
                        tElement.Import(pElemDataList[i], false);
                        if (tElement is LevelWaveSet)
                        {
                            LevelWaveSet waveset = tElement as LevelWaveSet;
                            waveset.Region = pRegion;
                        }
                        tElement.Init();
                    }
                }
            }
            else
            {
                tElement = pHolder.FindElement(pData.Id);
                if (tElement != null)
                {
                    DestroyImmediate(tElement.gameObject);
                }
            }
        }

        public void StartMapEvent(MapEvent pData, LevelRegion pRegion)
        {
            switch (pData.Type)
            {
                case MapTriggerType.Barrier:
                    {
                        HolderBarrier pHolder = GetHolder(MapHolderType.Barrier) as HolderBarrier;
                        CreateMapEvent<LevelBarrier, MapBarrier>(pData, pRegion, pHolder, Config.Barriers);
                    }
                    break;
                case MapTriggerType.Region:
                    {
                        HolderRegion pHolder = GetHolder(MapHolderType.Region) as HolderRegion;
                        CreateMapEvent<LevelRegion, MapRegion>(pData, pRegion, pHolder, Config.Regions);
                    }
                    break;
                case MapTriggerType.Portal:
                    {
                        HolderPortal pHolder = GetHolder(MapHolderType.Portal) as HolderPortal;
                        CreateMapEvent<LevelPortal, MapPortal>(pData, pRegion, pHolder, Config.Portals);
                    }
                    break;
                case MapTriggerType.Waveset:
                    {
                        HolderWaveSet pHolder = GetHolder(MapHolderType.WaveSet) as HolderWaveSet;
                        CreateMapEvent<LevelWaveSet, MapWaveSet>(pData, pRegion, pHolder, Config.WaveSets);
                    }
                    break;
                case MapTriggerType.Result:
                    {
                        OnLevelEnd();
                    }
                    break;
                case MapTriggerType.Monstegroup:
                    {
                        HolderMonsterGroup pHolder = GetHolder(MapHolderType.MonsterGroup) as HolderMonsterGroup;
                        CreateMapEvent<LevelMonsterGroup, MapMonsterGroup>(pData, pRegion, pHolder, Config.MonsterGroups);
                    }
                    break;
                case MapTriggerType.Minegroup:
                    {
                        HolderMineGroup pHolder = GetHolder(MapHolderType.MineGroup) as HolderMineGroup;
                        CreateMapEvent<LevelMineGroup, MapMineGroup>(pData, pRegion, pHolder, Config.MineGroups);
                    }
                    break;

            }
        }
        
        public void RegisterLoadSceneCallback(Action pCallback, float pDelay = 0)
        {
            m_OnLoadNewSceneTasks.Add(new LevelTask() { callback = pCallback, delay = pDelay });
        }

        public void UnRegisterLoadSceneCallback(Action callback)
        {
            for (int i = 0; i < m_OnLoadNewSceneTasks.Count; i++)
            {
                if (m_OnLoadNewSceneTasks[i].callback == callback)
                {
                    m_OnLoadNewSceneTasks.RemoveAt(i);
                    break;
                }
            }
        }

        public List<RoleEntityBase> GetRolesByActorType(ActorType pType)
        {
            List<RoleEntityBase> pList = new List<RoleEntityBase>();
            for (int i = 0; i < m_AllRoles.Count; i++)
            {
                if (m_AllRoles[i].Actor.ActorType == pType)
                {
                    pList.Add(m_AllRoles[i]);
                }
            }
            return pList;
        }

        public List<ActorBase> GetAllRoleActor()
        {
            List<ActorBase> all = new List<ActorBase>();
            for (int i = 0; i < m_AllRoles.Count; i++)
            {
                all.Add(m_AllRoles[i].Actor);
            }

            return all;
        }

        public void FindActorsByCamp(BattleCampType actorCamp, ref List<ActorBase> list, bool ignoreStealth = false)
        {
            for (int i = 0; i < m_AllRoles.Count; i++)
            {
                ActorBase actor = m_AllRoles[i].Actor;
                if (actor.Camp == actorCamp && actor.IsDead == false)
                {
                    if (ignoreStealth == false)
                    {
                        list.Add(actor);
                    }
                    else
                    {
                        if (actor.GetActorState(ActorStateType.IsStealth) == false)
                        {
                            list.Add(actor);
                        }
                    }
                }
            }
        }

        private void OnLevelStart()
        {
            GameEntry.Camera.HideAllEffect();

            CurSceneType = GameEntry.Scene.GetSceneTypeBySceneId(CurSceneId);

            if (CurSceneType == SceneType.Battle)
            {
                OnBattleStart();
            }

            InitPlayer();

            InitNpc();

            InitLevelObject();

            for (int i = 0; i < m_OnLoadNewSceneTasks.Count; i++)
            {
                LevelTask task = m_OnLoadNewSceneTasks[i];
                if (task.callback == null)
                {
                    if (task.callback != null)
                        Invoke(task.callback.Method.Name, task.delay);
                }
            }
        }

        private void OnLevelEnd()
        {
            if (CurSceneType == SceneType.Battle)
            {
                OnBattleEnd();
            }
        }

        private void OnBattleStart()
        {
            if (LevelID <= 0)
            {
                Log.Error("CopyId is invalid.");
                return;
            }

            if(!GameEntry.DataTable.GetDataTable<DRLevel>().HasDataRow(LevelID))
            {
                Log.Error("the copy is no exist.");
                return;
            }

            m_StartTime = Time.realtimeSinceStartup;

            HomeFormData data = new HomeFormData();
            data.SceneType = SceneType.Battle;
            GameEntry.UI.OpenUIForm(UIFormId.HomeForm, data);
            GameEntry.UI.OpenUIForm(UIFormId.ControllerForm);
            GameEntry.UI.OpenUIForm(UIFormId.ComboForm);
        }

        private void OnBattleEnd()
        {
            if (LevelID <= 0)
            {
                Log.Error("LevelID is invalid.");
                return;
            }

            GameEntry.UI.CloseUIForm(UIFormId.ComboForm);
            FlyWordManager.Instance.Clear();
            GameEntry.UI.CloseUIForm(UIFormId.ControllerForm);
            GameEntry.UI.CloseUIForm(UIFormId.HomeForm);
           
            m_EndTime = Time.realtimeSinceStartup - m_StartTime;

            m_Victory = !Player.Actor.IsDead;
            CalcResult();
        }

        private void InitPlayer()
        {
            if (Config.Ally == null)
            {
                return;
            }

            if (Player != null)
            {
                TransformParam param = new TransformParam
                {
                    Position = Config.Ally.TransParam.Position,
                    EulerAngles = Config.Ally.TransParam.EulerAngles,
                    Scale = Config.Ally.TransParam.Scale
                };

                Player.Actor.SetBornParam(param);
                Player.UpdateTransform(param);

                AddPartner(Player, 1, Player.Actor.ActorCard.Partners[0]);
                AddPartner(Player, 2, Player.Actor.ActorCard.Partners[1]);
            }
        }

        private void InitNpc()
        {
            for (int i = 0; i < Config.Npcs.Count; i++)
            {
                MapNpc data = Config.Npcs[i];

                AddRole<NpcRole>(data.Id, ActorType.Npc, BattleCampType.Ally, data.Position, data.Euler, data.Scale);
            }
        }

        private void InitLevelObject()
        {
            //触发区域
            for (int i = 0; i < Config.Regions.Count; i++)
            {
                MapRegion data = Config.Regions[i];
                if (data.StartActive)
                {
                    LevelElement pHolder = GetHolder(MapHolderType.Region);
                    GameObject go = pHolder.gameObject.AddChild();
                    LevelRegion pRegion = go.AddComponent<LevelRegion>();
                    pRegion.Import(data, false);
                    pRegion.Init();
                }
            }

            //障碍物
            for (int i = 0; i < Config.Barriers.Count; i++)
            {
                MapBarrier data = Config.Barriers[i];
                LevelElement pHolder = GetHolder(MapHolderType.Barrier);
                GameObject go = pHolder.gameObject.AddChild();
                LevelBarrier pBarrier = go.AddComponent<LevelBarrier>();
                pBarrier.Import(data, false);
                pBarrier.Init();
            }

            //传送门
            for (int i = 0; i < Config.Portals.Count; i++)
            {
                MapPortal data = Config.Portals[i];
                LevelElement pHolder = GetHolder(MapHolderType.Portal);
                GameObject go = pHolder.gameObject.AddChild();
                LevelPortal pPortal = go.AddComponent<LevelPortal>();
                pPortal.Import(data, false);
                pPortal.Init();
            }
        }

        private void CalcResult()
        {
            LevelResultFormData data = new LevelResultFormData();
            if (m_Victory == false)
            {
                data.Victory = false;
                data.StarCount = 0;
                data.GlodAward = 0;
                data.ExpAward = 0;
                GameEntry.UI.OpenUIForm(UIFormId.LevelResultForm, data);
                return;
            }

            DRLevel drLevel = GameEntry.DataTable.GetDataTable<DRLevel>().GetDataRow(LevelID);
            if (drLevel == null)
            {
                Log.Error("the level dr data is no exist.");
                return;
            }

            int starCount = CalcStar(drLevel); 
            int money = drLevel.GetMoneyRatio;
            int exp = drLevel.GetExpRatio;


            data.Victory = true;
            data.StarCount = starCount;
            data.GlodAward = money;
            data.ExpAward = exp;
            GameEntry.UI.OpenUIForm(UIFormId.LevelResultForm, data);
        }

        private int CalcStar(DRLevel copy)
        {
            int star = 0;
            bool[] passContents = new bool[3];
            if (m_Victory == false)
            {
                return 0;
            }

            StarConditionType[] starConditions = new StarConditionType[3];
            starConditions[0] = (StarConditionType)copy.StarCondition1;
            starConditions[1] = (StarConditionType)copy.StarCondition2;
            starConditions[2] = (StarConditionType)copy.StarCondition3;

            int[] starValues = new int[3];
            starValues[0] = copy.StarValue1;
            starValues[1] = copy.StarValue2;
            starValues[2] = copy.StarValue3;

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
                                float ratio = curHealth / (maxHealth * 1f);
                                if (ratio >= v / 100f)
                                {
                                    star++;
                                    passContents[i] = true;
                                }
                            }
                        }
                        break;
                    case StarConditionType.Pass:
                        {
                            if (m_Victory)
                            {
                                star++;
                                passContents[i] = true;
                            }
                        }
                        break;
                    case StarConditionType.TimeLimit:
                        {
                            if (CurTime < v)
                            {
                                star++;
                                passContents[i] = true;
                            }
                        }
                        break;
                }
            }
            return star;
        }

        private void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            LoadSceneSuccessEventArgs ne = e as LoadSceneSuccessEventArgs;
            if (ne != null)
                CurSceneId = (SceneId)ne.UserData;
        }

        private void OnPlayerDead(object sender, GameEventArgs e)
        {
            List<RoleEntityBase> pList = GetRolesByActorType(ActorType.Monster);
            for (int i = 0; i < pList.Count; i++)
            {
                pList[i].Actor.SetTarget(null);
            }

            this.LeaveCurrentLevel();

            this.Player = null;

            GameEntry.Camera.ShowEffect(CameraEffectType.ScreenGray);
        }

        private void OnPassLevel(object sender, GameEventArgs e)
        {
            LeaveCurrentLevel();
        }
    }
}
