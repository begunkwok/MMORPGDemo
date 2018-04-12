namespace GameMain
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        private static void InitCustomComponents()
        {
            Database = UnityGameFramework.Runtime.GameEntry.GetComponent<DatabaseComponent>();
            FairyGui = UnityGameFramework.Runtime.GameEntry.GetComponent<FairyGuiComponent>();
            AppConfig = UnityGameFramework.Runtime.GameEntry.GetComponent<AppConfigComponent>();
            Lua = UnityGameFramework.Runtime.GameEntry.GetComponent<LuaComponent>();
            Camera = UnityGameFramework.Runtime.GameEntry.GetComponent<CameraComponent>();
            Input = UnityGameFramework.Runtime.GameEntry.GetComponent<InputComponent>();
            BT = UnityGameFramework.Runtime.GameEntry.GetComponent<BehaviorTreeComponent>();
            Timer = UnityGameFramework.Runtime.GameEntry.GetComponent<TimerComponent>();

            Database.Init();
            FairyGui.Init();
            Lua.Init();
            Camera.Init();
            Input.Init();
        }

        public static DatabaseComponent Database
        {
            get;
            private set;
        }

        public static FairyGuiComponent FairyGui
        {
            get;
            private set;
        }

        public static AppConfigComponent AppConfig
        {
            get;
            private set;
        }

        public static LuaComponent Lua
        {
            get;
            private set;
        }

        public static CameraComponent Camera
        {
            get;
            private set;
        }

        public static InputComponent Input
        {
            get;
            private set;
        }

        public static BehaviorTreeComponent BT
        {
            get; private set;
        }

        public static TimerComponent Timer { get; protected set; }

    }
}
