using System;
using System.Collections.Generic;
using GameFramework;

namespace GameMain
{
    /// <summary>
    /// 默认属性计算策略
    /// </summary>
    public class DefaultCalcStrategy : ICalcStrategy
    {
        public void CalcAllDressEquip(Dictionary<PropertyType, int> curData)
        {
            throw new NotImplementedException();
        }

        public void CalcAllDressGem(Dictionary<PropertyType, int> curData)
        {
            throw new NotImplementedException();
        }

        public void CalcEquipAdvance(Dictionary<PropertyType, int> curData, int id, int advanceLevel)
        {
            throw new NotImplementedException();
        }

        public void CalcEquipStar(Dictionary<PropertyType, int> curData, int id, int starLevel)
        {
            throw new NotImplementedException();
        }

        public void CalcEquipStrength(Dictionary<PropertyType, int> curData, int id, int strengthLevel)
        {
            throw new NotImplementedException();
        }

        public int CalcFinalDamage(IAttribute attackerAttr, IAttribute defenseAttr)
        {
            throw new NotImplementedException();
        }

        public int CalcGemStrenthLevel(Dictionary<PropertyType, int> curData, int id, int strengthLevel)
        {
            throw new NotImplementedException();
        }

        public void CalcPartner(Dictionary<PropertyType, int> curData, int id, int partnerLevel)
        {
            throw new NotImplementedException();
        }

        public void CalcPartnerAdvance(Dictionary<PropertyType, int> curData, int id, int advanceLevel)
        {
            throw new NotImplementedException();
        }

        public void CalcPlayer(Dictionary<PropertyType, int> curData, int level)
        {
            throw new NotImplementedException();
        }
    }
}
