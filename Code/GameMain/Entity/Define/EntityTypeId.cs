namespace GameMain
{
    /// <summary>
    /// 实体编号(由服务端产生，单机版先在这定义)
    /// </summary>
    public enum EntityTypeId
    {
        /// <summary>
        /// 未定义
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// 剑士角色
        /// </summary>
        PlayerWarrior = 1,

        /// <summary>
        /// 法师角色
        /// </summary>
        PlayerMaster = 2,

        /// <summary>
        /// 射手角色
        /// </summary>
        PlayerShooter = 3,
    }
}
