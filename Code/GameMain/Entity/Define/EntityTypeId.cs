namespace GameMain
{
    /// <summary>
    /// 实体类型编号
    /// </summary>
    public enum EntityTypeId
    {
        /// <summary>
        /// 未定义
        /// </summary>
        Undefined = 0,

#region 角色相关实体

        /// <summary>
        /// 职业选择剑士
        /// </summary>
        PoseWarrior = 10001,

        /// <summary>
        /// 职业选择法师
        /// </summary>
        PoseMaster = 10002,

        /// <summary>
        /// 职业选择射手
        /// </summary>
        PoseShooter = 10003,

        /// <summary>
        /// 剑士角色
        /// </summary>
        PlayerWarrior = 10004,

        /// <summary>
        /// 法师角色
        /// </summary>
        PlayerMaster = 10005,

        /// <summary>
        /// 射手角色
        /// </summary>
        PlayerShooter = 10006,

        #endregion

#region 技能特效实体
        /// <summary>
        /// 职业选择剑士大剑特效
        /// </summary>
        SwordTrail = 31001,

        /// <summary>
        /// 剑士打击地面特效
        /// </summary>
        GroundExplode = 31002,



        /// <summary>
        /// 冰爆特效
        /// </summary>
        IceExplode = 32001,

        /// <summary>
        /// 冰晶特效
        /// </summary>
        IceCrys = 32002,



        /// <summary>
        /// 冰环特效
        /// </summary>
        IceCircle = 33001,

        /// <summary>
        /// 冰凤凰
        /// </summary>
        IcePhenix = 33002,
#endregion


    }
}
