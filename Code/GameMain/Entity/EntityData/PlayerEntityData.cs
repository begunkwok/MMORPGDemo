namespace GameMain
{
    public class RoleEntityData : EntityData
    {
        public ActorType ActorType { get; }
        public BattleCampType CampType { get; }

        public RoleEntityData(int entityId, int typeId, ActorType actorType, BattleCampType campType) : base(entityId, typeId)
        {
            ActorType = actorType;
            CampType = campType;
        }
    }
}
