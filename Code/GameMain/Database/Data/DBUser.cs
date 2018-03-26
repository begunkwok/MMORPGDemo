using System;
using GameFramework;

namespace GameMain
{
    public class DBUser : IDBRow
    {
        public DBUser() : this(0, "")
        {

        }

        public DBUser(int id, string userId)
        {
            this.Id = id;
            this.UserId = userId;

            m_Items = new[]
{
                "Id",
                "UserId",
                "Account",
                "Password",
                "Player",
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

        public string Account { get; set; } = "";

        public string Password { get; set; } = "";

        public int Player
        {
            get;
            set;
        }=0;

        private string[] m_Items;
        private string[] m_Values;
        private string[] m_SelectKeys;
        private string[] m_SelectValues;


        public void Insert()
        {
            UpdateValues();
            GameEntry.Database.InsertInto("DBUser", m_Values);
        }

        public void Load()
        {
            var dr = GameEntry.Database.SelectWhereEqual("DBUser", m_Items, m_SelectKeys, m_SelectValues);

            if (!dr.HasRows)
            {
                Log.Error("Can no find data. id:{0},userId:{1}", Id, UserId);
                return;
            }

            while (dr.Read())
            {
                Account = dr.GetString(dr.GetOrdinal("Account"));
                Password = dr.GetString(dr.GetOrdinal("Password"));
                Player = int.Parse(dr.GetString(dr.GetOrdinal("Player")));
            }
        }

        public void Save()
        {
            UpdateValues();
            GameEntry.Database.UpdateInto("DBUser", m_Items, m_Values, m_SelectKeys, m_SelectValues);
        }

        public void Delete()
        {
            UpdateValues();
            GameEntry.Database.Delete("DBUser", m_Items, m_Values);
        }

        private void UpdateValues()
        {
            m_Values = new[]
            {
                $"'{Id}'",
                $"'{UserId}'",
                $"'{Account}'",
                $"'{Password}'",
                $"'{Player}'",
            };

            m_SelectValues = new[]
            {
                $"'{Id}'",
                $"'{UserId}'",
            };
        }
    }
}
