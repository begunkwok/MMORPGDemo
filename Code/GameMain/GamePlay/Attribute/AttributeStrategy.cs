﻿using System;
using System.Collections.Generic;
using GameFramework;

namespace GameMain
{
    /// <summary>
    /// 默认属性计算策略
    /// </summary>
    public class AttributeStrategy : IAttributeStrategy
    {
        public void CalcHeroLevel(Dictionary<PropertyType, int> curData, int level)
        {
            DRHeroLevel drHero = GameEntry.DataTable.GetDataTable<DRHeroLevel>().GetDataRow(level);
            if (drHero == null)
            {
                return;
            }

            curData[PropertyType.LHP] += drHero.LHP;
            curData[PropertyType.ATK] += drHero.ATK;
            curData[PropertyType.DEF] += drHero.DEF;
            curData[PropertyType.CRI] += drHero.CRI;
            curData[PropertyType.BUR] += drHero.BUR;
            curData[PropertyType.LMP] += drHero.LMP;
            curData[PropertyType.VAM] += drHero.VAM;
            curData[PropertyType.HIT] += drHero.HIT;
            curData[PropertyType.DOG] += drHero.DOG;
            curData[PropertyType.BAF] += drHero.BAF;
        }

        public void CalcAllDressEquip(Dictionary<PropertyType, int> curData, int ownerId)
        {
            throw new NotImplementedException();
        }

        public void CalcAllDressGem(Dictionary<PropertyType, int> curData, int ownerId)
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
    }
}
