using System;
using GameFramework;
using GameFramework.DataTable;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public static class EntityExtension
    {
        // 关于 EntityId 的约定：
        // 0 为无效
        // 正值用于和服务器通信的实体（如玩家角色、NPC、怪等，服务器只产生正值）
        // 负值用于本地生成的临时实体（如特效、FakeObject等）
        private static int s_SerialId = 0;
        private static int s_TempSerialId = 0;

        /// <summary>
        /// 生成实体序列ID（由服务器产生，目前先本地产生）
        /// </summary>
        public static int GenerateSerialId(this EntityComponent entityComponent)
        {
            return ++s_SerialId;
        }

        /// <summary>
        /// 生成临时实体序列ID（负值）
        /// </summary>
        public static int GenerateTempSerialId(this EntityComponent entityComponent)
        {
            return --s_TempSerialId;
        }

        public static EntityBase GetGameEntity(this EntityComponent entityComponent, int entityId)
        {
            UnityGameFramework.Runtime.Entity entity = entityComponent.GetEntity(entityId);
            if (entity == null)
            {
                return null;
            }

            return (EntityBase) entity.Logic;
        }

        public static void HideEntity(this EntityComponent entityComponent, EntityBase entity)
        {
            entityComponent.HideEntity(entity.Entity);
        }

        public static void AttachEntity(this EntityComponent entityComponent, EntityBase entity, int ownerId,
            string parentTransformPath = null, object userData = null)
        {
            entityComponent.AttachEntity(entity.Entity, ownerId, parentTransformPath, userData);
        }

        public static void ShowEntity(this EntityComponent entityComponent, Type logicType, EntityData data)
        {
            if (data == null)
            {
                Log.Warning("Data is invalid.");
                return;
            }

            if (entityComponent.HasEntity(data.Id))
            {
                Log.Warning(string.Format("Entity {0} is exist.", data.Id));
                return;
            }

            IDataTable<DREntity> dtEntity = GameEntry.DataTable.GetDataTable<DREntity>();
            DREntity drEntity = dtEntity.GetDataRow(data.TypeId);
            if (drEntity == null)
            {
                Log.Warning("Can not load entity id '{0}' from data table.", data.TypeId.ToString());
                return;
            }

            entityComponent.ShowEntity(data.Id, logicType, AssetUtility.GetEntityAsset(drEntity.AssetName),
                drEntity.Group, data);         
        }

        public static void ShowActorEntity(this EntityComponent entityComponent, Type logicType, EntityData data)
        {
            if (data == null)
            {
                Log.Warning("Data is invalid.");
                return;
            }

            if (entityComponent.HasEntity(data.Id))
            {
                Log.Warning(string.Format("Entity {0} is exist.", data.Id));
                return;
            }

            IDataTable<DRActorEntity> dtActorEntity = GameEntry.DataTable.GetDataTable<DRActorEntity>();
            DRActorEntity drActorEntity = dtActorEntity.GetDataRow(data.TypeId);
            if (drActorEntity == null)
            {
                Log.Warning("Can not load entity id '{0}' from data table.", data.TypeId.ToString());
                return;
            }
            entityComponent.ShowEntity(data.Id, logicType, AssetUtility.GetEntityAsset(drActorEntity.ModelAsset),
                drActorEntity.Group, data);
        }

        //-----------------简化调用函数----------------
        public static void ShowEffect(this EntityComponent entityComponent, EffectData data)
        {
            entityComponent.ShowEntity(typeof (Effect), data);
        }

        public static void ShowPoseRole(this EntityComponent entityComponent,PoseRoleData data)
        {
            entityComponent.ShowEntity(typeof (PoseRole), data);
        }

        public static void ShowPlayer(this EntityComponent entityComponent, PlayerEntityData data)
        {
            entityComponent.ShowActorEntity(typeof (PlayerRole), data);

        }

        public static EntityBase GetEntity(this EntityComponent entityComponent, int serialId)
        {
            Entity playerEntity = entityComponent.GetEntity(serialId);
            EntityBase entity = (EntityBase) playerEntity?.Logic;
            return entity;
        }

    }
}
