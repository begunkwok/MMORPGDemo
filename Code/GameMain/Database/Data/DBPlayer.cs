using System;
using GameFramework;

namespace GameMain
{
    [DatabaseRow]
    public class DBPlayer : IDBRow
    {
        public DBPlayer() : this(0, "")
        {

        }

        public DBPlayer(int id, string userId)
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
            };

            m_SelectKeys = new[]
            {
                "Id",
                "UserId",
            };

            UpdateValues();
        }

        public int Id { get; } = 0;

        public string UserId { get; } = "";

        public int EntityTypeId { get; set; } = 0;

        public string Name { get; set; } = "";

        public int Level { get; set; } = 0;

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
                Name = dr.GetString(dr.GetOrdinal("Name"));
                Level = int.Parse(dr.GetString(dr.GetOrdinal("Level")));
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
            };

            m_SelectValues = new[]
            {
                $"'{Id}'",
                $"'{UserId}'",
            };
        }
    }
}
