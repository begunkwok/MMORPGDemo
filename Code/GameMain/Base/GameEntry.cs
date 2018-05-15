using FairyGUI;
using UnityEngine;

namespace GameMain
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry : MonoBehaviour
    {
        private void Awake()
        {
            InitBuiltinComponents();
        }

        private void Start()
        {
            InitCustomComponents();
        }

        private void OnDestroy()
        {
            ClearCustomComponents();
        }
    }
}
