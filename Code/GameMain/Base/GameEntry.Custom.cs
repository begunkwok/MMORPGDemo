namespace GameMain
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        private static void InitCustomComponents()
        {
            AppConfig = UnityGameFramework.Runtime.GameEntry.GetComponent<AppConfigComponent>();
            FairyGui = UnityGameFramework.Runtime.GameEntry.GetComponent<FairyGuiComponent>();
            Lua = UnityGameFramework.Runtime.GameEntry.GetComponent<LuaComponent>();
            Camera = UnityGameFramework.Runtime.GameEntry.GetComponent<CameraComponent>();
            Input = UnityGameFramework.Runtime.GameEntry.GetComponent<InputComponent>();

            FairyGui.Init();
            Lua.Init();
            Camera.Init();
            Input.Init();
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

    }
}
