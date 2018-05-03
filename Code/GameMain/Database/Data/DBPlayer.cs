using System;
using GameFramework;

namespace GameMain
{
    [DatabaseRow]
    public class DBPlayer : IDBRow
    {
        public DBPlayer() : this(0, 0)
        {

        }

        public DBPlayer(int id, int userId)
        {
            this.Id = id;
            this.UserId = userId;

            m_Items = new[]
            {
                "Id",
                "UserId",
                "EntityTypeId",
                "Name",
                "Level",
                "Exp",
                "VipLevel",
                "MountID",
                "RelicID",
                "PetID",
                "Partner1ID",
                "Partner2ID",
                "Partner3ID",
                "TalentMask",
            };

            m_SelectKeys = new[]
            {
                "Id",
                "UserId",
            };

            UpdateValues();
        }

        public int Id { get;}
        public int UserId { get;}
        public int EntityTypeId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Exp { get; set; }
        public int VipLevel { get; set; }
        public int MountId { get; set; }
        public int RelicId { get; set; }
        public int PetId { get; set; }
        public int Partner1Id { get; set; }
        public int Partner2Id { get; set; }
        public int Partner3Id { get; set; }
        public int TalentMask{ get; set; }


        private string[] m_Items;
        private string[] m_Values;
        private string[] m_SelectKeys;
        private string[] m_SelectValues;


        public void Insert()
        {
            UpdateValues();
            GameEntry.Database.InsertInto("DBPlayer", m_Values);
        }
        
        public void Load()
        {
            var dr = GameEntry.Database.SelectWhereEqual("DBPlayer", m_Items, m_SelectKeys, m_SelectValues);

            if (!dr.HasRows)
            {
                Log.Error("Can no find data. id:{0},userId:{1}", Id, UserId);
                return;
            }

            while (dr.Read())
            {
                EntityTypeId = int.Parse(dr.GetString(dr.GetOrdinal("EntityTypeId")));
                Name         = dr.GetString(dr.GetOrdinal("Name"));
                Level        = int.Parse(dr.GetString(dr.GetOrdinal("Level")));
                Exp          = int.Parse(dr.GetString(dr.GetOrdinal("Exp")));
                VipLevel     = int.Parse(dr.GetString(dr.GetOrdinal("VipLevel")));
                MountId      = int.Parse(dr.GetString(dr.GetOrdinal("MountId")));
                RelicId      = int.Parse(dr.GetString(dr.GetOrdinal("RelicId")));
                PetId        = int.Parse(dr.GetString(dr.GetOrdinal("PetId")));
                Partner1Id   = int.Parse(dr.GetString(dr.GetOrdinal("Partner1Id")));
                Partner2Id   = int.Parse(dr.GetString(dr.GetOrdinal("Partner2Id")));
                Partner3Id   = int.Parse(dr.GetString(dr.GetOrdinal("Partner3Id")));
                TalentMask   = int.Parse(dr.GetString(dr.GetOrdinal("TalentMask")));
            }
        }

        public void Save()
        {
            UpdateValues();
            GameEntry.Database.UpdateInto("DBPlayer", m_Items, m_Values, m_SelectKeys, m_SelectValues);
        }

        public void Delete()
        {
            UpdateValues();
            GameEntry.Database.Delete("DBPlayer", m_Items, m_Values);
        }

        private void UpdateValues()
        {
            m_Values = new[]
            {
                $"'{Id}'",
                $"'{UserId}'",
                $"'{EntityTypeId}'",
                $"'{Name}'",
                $"'{Level}'",
                $"'{Exp}'",
                $"'{VipLevel}'",
                $"'{MountId}'",
                $"'{RelicId}'",
                $"'{PetId}'",
                $"'{Partner1Id}'",
                $"'{Partner2Id}'",
                $"'{Partner3Id}'",
                $"'{TalentMask}'",
            };

            m_SelectValues = new[]
            {
                $"'{Id}'",
                $"'{UserId}'",
            };
        }
    }
}
