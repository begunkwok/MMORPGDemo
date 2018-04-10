using System;
using System.Collections.Generic;

namespace GameMain
{
    /// <summary>
    /// 属性接口
    /// </summary>
    public interface IAttribute
    {
        /// <summary>
        /// 初始化属性值
        /// </summary>
        void InitAttribute();

        /// <summary>
        /// 获取属性值
        /// </summary>
        int GetValue(ActorAttributeType type);

        /// <summary>
        /// 更新属性值
        /// </summary>
        void UpdateValue(ActorAttributeType type, int value);

        /// <summary>
        /// 获取最终承受的伤害
        /// </summary>
        /// <param name="attackerAttrbute">攻击者属性</param>
        int GetFinalDamage(IAttribute attackerAttrbute);
    }
}
