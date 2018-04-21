using System;
using System.Collections.Generic;
using GameFramework.Event;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Level")]
    public class LevelComponent : GameFrameworkComponent
    {
        public int MapID;
        public SceneId CurSceneId { get; private set; }
        public SceneType CurSceneType { get; private set; }
        public float Delay;
        public string MapName = string.Empty;
        public string MapPath = string.Empty;
        public MapConfig Config { get; private set; }

        private readonly List<LevelTask> m_OnLoadNewSceneTasks = new List<LevelTask>();
        private readonly Dictionary<MapHolderType, LevelElement> m_Holders = new Dictionary<MapHolderType, LevelElement>();
        private static int _guidStart = 100001;

        public int GetGUID()
        {
            _guidStart++;
            return _guidStart;
        }

        public void Init()
        {
            GameEntry.Event.Subscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);

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

        public void EnterWorld(int mapID)
        {
            this.MapID = mapID;
            this.Init();
            string assetName = AssetUtility.GetSkillScriptAsset(mapID.ToString());
            Config = new MapConfig();
            Config.Load(assetName);

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
            this.OnSceneStart();
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

        public PlayerRole CreatePlayer(int id, TransformParam param)
        {
            PlayerRole player = AddRole<PlayerRole>(id, ActorType.Player, BattleCampType.Ally, param);
            LevelData.Player = player;
            GameEntry.Camera.SwitchCameraBehaviour(CameraBehaviourType.LockLook);
            return player;
        }

        public EnemyRole CreateEnemy(int id, TransformParam param)
        {
            EnemyRole enemy = AddRole<EnemyRole>(id, ActorType.Monster, BattleCampType.Enemy, param);
            return enemy;
        }

        public Barrier CreateBarrier(int id, TransformParam param = null)
        {
            int entityId = GameEntry.Entity.GenerateSerialId();
            BarrierData barrierData = new BarrierData(entityId,id);
            Barrier barrier = GameEntry.Entity.ShowEntity<Barrier>(barrierData);

            if (param != null)
            {
                barrier.CachedTransform.position = param.Position;
                barrier.CachedTransform.eulerAngles = param.EulerAngles;
                barrier.CachedTransform.localScale = param.Scale;
            }

            return barrier;
        }

        public Mine CreateMine(int id, TransformParam param = null)
        {
            int entityId = GameEntry.Entity.GenerateSerialId();
            MineData mineData = new MineData(entityId, id);
            Mine mine = GameEntry.Entity.ShowEntity<Mine>(mineData);

            if (param != null)
            {
                mine.CachedTransform.position = param.Position;
                mine.CachedTransform.eulerAngles = param.EulerAngles;
                mine.CachedTransform.localScale = param.Scale;
            }

            return mine;
        }

        public void AddPartner(PlayerRole host, int pos, int id)
        {
            if (host?.Actor == null)
                return;

            if (id <= 0)
                return;

            PartnerSortType sort = (PartnerSortType)pos;
            ActorPlayer actorPlayer = host.Actor as ActorPlayer;
            if (actorPlayer == null)
            {
                return;
            }

            Vector3 pPos = actorPlayer.GetPartnerPosBySort(sort);

            TransformParam param = TransformParam.Create(pPos, host.CachedTransform.eulerAngles, Vector3.one * 1.5f);

            ActorBase partner = AddRole<RoleBase>(id, ActorType.Partner, actorPlayer.Camp, param).Actor;
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

        public bool GetPortalByDestMapID(int id, ref Vector3 pos)
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

        public T AddRole<T>(int id, ActorType type, BattleCampType camp, Vector3 pos, Vector3 angle) where T : RoleBase
        {
            return AddRole<T>(id, type, camp, TransformParam.Create(pos, angle));
        }

        public T AddRole<T>(int id, ActorType type, BattleCampType camp, Vector3 pos, Vector3 angle, Vector3 scale) where T : RoleBase
        {
            return AddRole<T>(id, type, camp, TransformParam.Create(pos, angle, scale));
        }

        public T AddRole<T>(int entityTypeId, ActorType type, BattleCampType camp, TransformParam param)
            where T : RoleBase
        {
            int entityId = GameEntry.Entity.GenerateSerialId();
            RoleEntityData roleData = new RoleEntityData(entityId, entityTypeId, type, camp);
            RoleBase role = GameEntry.Entity.ShowRole<T>(roleData);

            if (role != null)
            {
                param.Position = GlobalTools.NavSamplePosition(param.Position);

                if (role.CachedTransform != null)
                {
                    //TODO 设置父节点
                    //role.CachedTransform.parent = GetHolder(MapHolderType.Role).transform;
                    LevelData.AllRoles.Add(role);
                    LevelData.AllActors.Add(role.Actor);
                    LevelData.CampActors[camp].Add(role);
                }
            }

            return role as T;
        }

        public void OnPlayerDead()
        {
            List<RoleBase> pList = LevelData.GetRolesByActorType(ActorType.Monster);
            for (int i = 0; i < pList.Count; i++)
            {
                pList[i].Actor.SetTarget(null);
            }
            if (CurSceneType == SceneType.City)
            {
                //TODO 打开重生界面
                //ZTUIManager.Instance.OpenWindow(WindowID.UI_REBORN);
            }
            else
            {
                this.OnBattleEnd();
            }
        }

        public bool DelRole(RoleBase role)
        {
            if (role != null)
            {
                LevelData.AllRoles.Remove(role);
                LevelData.CampActors[role.Actor.Camp].Remove(role);
                GameEntry.Entity.HideEntity(role.Id);
            }
            return false;
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
                        OnSceneEnd();
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
        
        public void OnSceneStart()
        {
            CurSceneType = GameEntry.Scene.GetSceneTypeBySceneId(CurSceneId);

            int id = GameEntry.Database.GetPlayerId();

            if (Config.A == null)
            {
                return;
            }
            TransformParam pTransParam = TransformParam.Create(Config.A.TransParam.Position,
                Config.A.TransParam.EulerAngles);

            CreatePlayer(id, pTransParam);

            //TODO 创建同伴
            //AddPartner(LevelData.MainPlayer, 1, LevelData.MainPlayer.GetActorCard().Partners[0]);
            //AddPartner(LevelData.MainPlayer, 2, LevelData.MainPlayer.GetActorCard().Partners[1]);

            for (int i = 0; i < Config.Npcs.Count; i++)
            {
                MapNpc data = Config.Npcs[i];

                //TODO 创建npc
                //AddActor(data.Id, EActorType.NPC, EBattleCamp.C, data.Position, data.Euler, data.Scale);
            }
            if (CurSceneType == SceneType.Battle)
            {
                OnBattleStart();
            }

            for (int i = 0; i < m_OnLoadNewSceneTasks.Count; i++)
            {
                LevelTask task = m_OnLoadNewSceneTasks[i];
                if (task.callback == null)
                {
                    Invoke(task.callback.Method.Name, task.delay);
                }
            }
        }

        public void OnSceneEnd()
        {
            if (CurSceneType == SceneType.Battle)
            {
                OnBattleEnd();
            }
        }

        public void OnBattleStart()
        {
            //TODO: 战斗场景开始
            //DBCopy db = ZTConfig.Instance.GetDBCopy(LevelData.CopyID);
            //if (db == null)
            //{
            //    return;
            //}
            //LevelData.StrTime = Time.realtimeSinceStartup;
        }

        public void OnBattleEnd()
        {
            //TODO: 战斗结束
            //DBCopy db = ZTConfig.Instance.GetDBCopy(LevelData.CopyID);
            //if (db == null)
            //{
            //    return;
            //}
            //LevelData.EndTime = Time.realtimeSinceStartup - LevelData.StrTime;
            //LevelData.Win = !LevelData.MainPlayer.IsDead();
            //LevelData.CalcResult();
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

        public void Clear()
        {
            CancelInvoke();
            m_OnLoadNewSceneTasks.Clear();
            for (int i = LevelData.AllActors.Count - 1; i >= 0; i--)
            {
                RoleBase role = LevelData.AllRoles[i];
                LevelData.AllActors.RemoveAt(i);
                GameEntry.Entity.HideEntity(role.Id);
            }
            LevelData.Player = null;
            foreach (KeyValuePair<MapHolderType, LevelElement> current in m_Holders)
            {
                current.Value.transform.DestroyChildren();
            }
            foreach (var current in LevelData.CampActors)
            {
                current.Value.Clear();
            }

            GameEntry.Event.Unsubscribe(LoadSceneSuccessEventArgs.EventId, OnLoadSceneSuccess);
        }

        private void OnLoadSceneSuccess(object sender, GameEventArgs e)
        {
            LoadSceneSuccessEventArgs ne = e as LoadSceneSuccessEventArgs;
            if (ne != null)
                CurSceneId = (SceneId)ne.UserData;
        }
    }
}
