namespace GameMain
{
    public interface IDBRow
    {
        /// <summary>
        /// 编号
        /// </summary>
        int Id { get;}

        /// <summary>
        /// 用户编号
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// 插入数据到数据库
        /// </summary>
        void Insert();

        /// <summary>
        /// 从数据库获取数据
        /// </summary>
        void Load();

        /// <summary>
        /// 保存数据到数据库
        /// </summary>
        void Save();

        /// <summary>
        /// 删除数据
        /// </summary>
        void Delete();
    }
}
