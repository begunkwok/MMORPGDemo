using System.Threading;
using UnityEditor;
using UnityEngine;

namespace GameMain
{

    /// <summary>
    /// 关卡组件，编辑部分         //TODO: 后续再完善编辑器功能
    /// </summary>
    public partial class LevelComponent
    {
        //是否是编辑模式
        public static bool IsEditorMode = false;

        private static LevelComponent instance = null;
        private static object lockObject = new object();

        public static LevelComponent Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {

                        instance = GameObject.FindObjectOfType(typeof (LevelComponent)) as LevelComponent;
                        if (instance == null)
                        {
                            instance =
                                new GameObject(typeof (LevelComponent).Name, typeof (LevelComponent))
                                    .GetComponent<LevelComponent>();
                        }
                    }
                }

                return instance;
            }
        }

        //编辑资源
        public static string BarrierAsset = "Barrier_01";
        public static string PortalAsset = "Portal_01";
        public static string MineAsset = "Mine/Mine_01";

        public static GameObject CreateLevelEditorObject(MapHolderType type)
        {
            GameObject levelGo = null;
            string assetPath = ""; 
            switch (type)
            {
                case MapHolderType.Barrier:
                    assetPath = AssetUtility.GetLevelObjectAsset(BarrierAsset);
                    break;
                case MapHolderType.Portal:
                    assetPath = AssetUtility.GetLevelObjectAsset(PortalAsset);
                    break;
                case MapHolderType.MineGroup:
                    assetPath = AssetUtility.GetLevelObjectAsset(MineAsset);
                    break;
            }

            Object asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            GameObject portalPrefab = asset as GameObject;
            levelGo = Instantiate(portalPrefab);
            return levelGo;
        }

    }
}
