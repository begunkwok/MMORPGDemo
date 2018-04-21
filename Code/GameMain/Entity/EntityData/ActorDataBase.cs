namespace GameMain
{
    /// <summary>
    /// 行动者数据基类
    /// </summary>
    public class ActorDataBase : EntityData
    {
        protected ActorAttribute m_Attribute;

        public ActorDataBase(int entityId, int typeId) : base(entityId, typeId)
        {
            m_Attribute = new ActorAttribute();
        }
    }
}
