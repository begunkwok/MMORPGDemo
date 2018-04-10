namespace GameMain
{
    /// <summary>
    /// Actor属性类型
    /// </summary>
    public enum ActorAttributeType
    {
        Unkonwn     = 0,   //未知
        Hp          = 1,   //生命值
        MaxHp       = 2,   //最大生命值  
        HpRecover   = 3,   //生命恢复
        Mp          = 4,   //魔法值
        MaxMp       = 5,   //最大魔法值
        MpRecover   = 6,   //魔法恢复
        Attack      = 7,   //攻击力
        Defense     = 8,   //防御力
        Speed       = 9,   //速度
        Crit        = 10,  //爆击
        CritDamage  = 11,  //爆击伤害
        SuckBlood   = 12,  //吸血
        Dodge       = 13,  //闪避
        Hit         = 14,  //命中
        Absorb      = 15,  //伤害吸收
        Reflex      = 16,  //伤害反弹
    }
}
