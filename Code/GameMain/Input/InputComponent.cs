using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Input")]
    public class InputComponent : GameFrameworkComponent
    {
        [SerializeField]
        private CrossPlatformInputManager.ActiveInputMethod InputType = CrossPlatformInputManager.ActiveInputMethod.Hardware;

        public void Init()
        {
            SwitchActiveInput();
        }

        /// <summary>
        /// 注册虚拟轴
        /// </summary>
        /// <param name="axis">虚拟轴</param>
        public void RegisterVirtualAxis(VirtualAxisBase axis)
        {
            if (axis == null)
            {
                Log.Error("Can no register null axis.");
                return;
            }
            CrossPlatformInputManager.RegisterVirtualAxis(axis);
        }

        /// <summary>
        /// 注册虚拟按钮
        /// </summary>
        /// <param name="button">虚拟按钮</param>
        public void RegisterVirtualButton(VirtualButton button)
        {
            if (button == null)
            {
                Log.Error("Can no register null button.");
                return;
            }
            CrossPlatformInputManager.RegisterVirtualButton(button);
        }

        /// <summary>
        /// 取消注册虚拟轴
        /// </summary>
        /// <param name="name">虚拟轴名称</param>
        public void UnRegisterVirtualAxis(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Log.Error("name is invlid.");
                return;
            }
            CrossPlatformInputManager.UnRegisterVirtualAxis(name);
        }

        /// <summary>
        /// 取消注册虚拟按钮
        /// </summary>
        /// <param name="name">虚拟按钮名称</param>
        public void UnRegisterVirtualButton(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Log.Error("name is invlid.");
                return;
            }
            CrossPlatformInputManager.UnRegisterVirtualButton(name);
        }

        /// <summary>
        /// 获取虚拟轴
        /// </summary>
        /// <param name="name">虚拟轴</param>
        /// <returns></returns>
        public VirtualAxisBase GetVirtualAxis(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Log.Error("name is invlid.");
                return null;
            }
            return CrossPlatformInputManager.VirtualAxisReference(name) as VirtualAxisBase;
        }

        /// <summary>
        /// 获取轴系数
        /// </summary>
        public float GetAxis(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Log.Error("name is invlid.");
                return 0;
            }

            return CrossPlatformInputManager.GetAxis(name);
        }

        /// <summary>
        /// 获取原始轴系数
        /// </summary>
        public float GetAxisRaw(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Log.Error("name is invlid.");
                return 0;
            }

            return CrossPlatformInputManager.GetAxisRaw(name);
        }

        /// <summary>
        /// 获取按钮
        /// </summary>
        public static bool GetButton(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Log.Error("name is invlid.");
                return false;
            }

            return CrossPlatformInputManager.GetButton(name);
        }

        /// <summary>
        /// 获取按钮按下
        /// </summary>
        public static bool GetButtonDown(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Log.Error("name is invlid.");
                return false;
            }

            return CrossPlatformInputManager.GetButtonDown(name);
        }

        /// <summary>
        /// 获取按钮抬起
        /// </summary>
        public static bool GetButtonUp(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Log.Error("name is invlid.");
                return false;
            }

            return CrossPlatformInputManager.GetButtonUp(name);
        }

        private void SwitchActiveInput()
        {
#if UNITY_EDITOR
            CrossPlatformInputManager.SwitchActiveInputMethod(InputType);
#else
            if (Application.isMobilePlatform)
            {
                CrossPlatformInputManager.SwitchActiveInputMethod(CrossPlatformInputManager.ActiveInputMethod.Touch);
            }
            else
            {
                CrossPlatformInputManager.SwitchActiveInputMethod(CrossPlatformInputManager.ActiveInputMethod.Hardware);
            }
#endif
        }

    }
}
