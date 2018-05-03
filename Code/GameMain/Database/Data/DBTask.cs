using GameFramework;

namespace GameMain
{
    [DatabaseRow]
    public class DBTask : IDBRow
    {
        public DBTask() : this(0, 0)
        {

        }

        public DBTask(int id, int userId)
        {
            this.Id = id;
            this.UserId = userId;

            m_Items = new[]
            {
                "Id",
                "UserId",
                "ThreadTaskID",
                "ThreadTaskStep",
                "BranchTaskID",
                "BranchTaskStep",
            };

            m_SelectKeys = new[]
            {
                "Id",
                "UserId",
            };

            UpdateValues();
        }

        public int Id { get; }
        public int UserId { get; }
        public int ThreadTaskID   { get; set; }
        public int ThreadTaskStep { get; set; }
        public int BranchTaskID   { get; set; }
        public int BranchTaskStep { get; set; }

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
                ThreadTaskID   = int.Parse(dr.GetString(dr.GetOrdinal("ThreadTaskID")));
                ThreadTaskStep = int.Parse(dr.GetString(dr.GetOrdinal("ThreadTaskStep")));
                BranchTaskID   = int.Parse(dr.GetString(dr.GetOrdinal("BranchTaskID")));
                BranchTaskStep = int.Parse(dr.GetString(dr.GetOrdinal("BranchTaskStep")));
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
                $"'{UserId}'"
            };

            m_SelectValues = new[]
            {
                $"'{Id}'",
                $"'{UserId}'",
            };
        }
    }
}
