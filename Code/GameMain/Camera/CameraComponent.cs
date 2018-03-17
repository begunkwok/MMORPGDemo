using System;
using System.Collections.Generic;
using GameFramework;
using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public class CameraComponent : GameFrameworkComponent
    {
        private Dictionary<CameraEffectType, CameraEffectBase> m_CameraEffects = new Dictionary<CameraEffectType, CameraEffectBase>();


        public Camera MainCamera { get; set; }
        public Camera UICamera { get; set; }

        private CameraEffectType curEffectType = CameraEffectType.ScreenFade;
        private float fadeTime = 1.5f;

        public void Init()
        {
            CreateMainCamera();

            InitCameraEffect();
        }

        /// <summary>
        /// 创建相机
        /// </summary>
        /// <param name="name">相机名字</param>
        /// <returns></returns>
        public Camera CreateCamera(string name)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go.AddComponent<Camera>();
        }

        /// <summary>
        /// 重载主相机
        /// </summary>
        public void ResetMainCamera()
        {
            if (MainCamera == null)
            {
                return;
            }
            MainCamera.fieldOfView = 60;
            MainCamera.renderingPath = RenderingPath.Forward;
            MainCamera.depth = Constant.Depth.MainCameraDepth;

            MainCamera.transform.position = Vector3.zero;
            MainCamera.transform.rotation = Quaternion.identity;
        }

        /// <summary>
        /// 显示相机特效
        /// </summary>
        /// <param name="effectType">特效类型</param>
        public void ShowEffect(CameraEffectType effectType)
        {
            if (effectType == CameraEffectType.Deafault)
            {
                HideAllEffect();
                return;
            }

            CameraEffectBase cameraEffect;
            m_CameraEffects.TryGetValue(effectType, out cameraEffect);
            if (cameraEffect == null)
            {
                Log.Error("Can no find cameraEffect by type:", effectType);
                return;
            }

            cameraEffect.Show();
        }

        /// <summary>
        /// 隐藏相机特效
        /// </summary>
        /// <param name="effectType">特效类型</param>
        public void HideEffect(CameraEffectType effectType)
        {
            CameraEffectBase cameraEffect;
            m_CameraEffects.TryGetValue(effectType, out cameraEffect);
            if (cameraEffect == null)
            {
                Log.Error("Can no find cameraEffect by type:", effectType);
                return;
            }

            cameraEffect.Hide();
        }

        /// <summary>
        /// 隐藏所有相机特效
        /// </summary>
        public void HideAllEffect()
        {
            foreach (KeyValuePair<CameraEffectType, CameraEffectBase> effect in m_CameraEffects)
            {
                effect.Value.Hide();
            }
        }

        /// <summary>
        /// 淡入相机特效
        /// </summary>
        /// <param name="effectType">特效类型</param>
        /// <param name="fadeTime">淡入时间</param>
        /// <param name="callback">回调</param>
        public void FadeInEffect(CameraEffectType effectType, float fadeTime, Action callback = null)
        {
            CameraEffectBase cameraEffect;
            m_CameraEffects.TryGetValue(effectType, out cameraEffect);
            if (cameraEffect == null)
            {
                Log.Error("Can no find cameraEffect by type:", effectType);
                return;
            }

            cameraEffect.FadeIn(fadeTime, callback);
        }

        /// <summary>
        /// 淡出相机特效
        /// </summary>
        /// <param name="effectType">特效类型</param>
        /// <param name="fadeTime">淡出时间</param>
        /// <param name="callback">回调</param>
        public void FadeOutEffect(CameraEffectType effectType, float fadeTime, Action callback = null)
        {
            CameraEffectBase cameraEffect;
            m_CameraEffects.TryGetValue(effectType, out cameraEffect);
            if (cameraEffect == null)
            {
                Log.Error("Can no find cameraEffect by type:", effectType);
                return;
            }

            cameraEffect.FadeOut(fadeTime, callback);
        }

        private void InitCameraEffect()
        {
            CameraEffectBase blurMovieEffect = MainCamera.gameObject.GetOrAddComponent<BlurMovieEffect>();
            m_CameraEffects.Add(CameraEffectType.BlurMovie, blurMovieEffect);

            CameraEffectBase blurRadialEffect = MainCamera.gameObject.GetOrAddComponent<BlurRadialEffect>();
            m_CameraEffects.Add(CameraEffectType.BlurRadial, blurRadialEffect);

            CameraEffectBase waterDropEffect = MainCamera.gameObject.GetOrAddComponent<WaterDropEffect>();
            m_CameraEffects.Add(CameraEffectType.WaterDrop, waterDropEffect);

            CameraEffectBase screenGray = MainCamera.gameObject.GetOrAddComponent<ScreenGrayEffect>();
            m_CameraEffects.Add(CameraEffectType.ScreenGray, screenGray);

            CameraEffectBase oilPaint = MainCamera.gameObject.GetOrAddComponent<OilPaintEffect>();
            m_CameraEffects.Add(CameraEffectType.OilPaint, oilPaint);

            CameraEffectBase screenFade = MainCamera.gameObject.GetOrAddComponent<ScreenFadeEffect>();
            m_CameraEffects.Add(CameraEffectType.ScreenFade, screenFade);

        }

        private void CreateMainCamera()
        {
            MainCamera = Camera.main;
            if (MainCamera == null)
            {
                GameObject cameraGo = new GameObject("MainCamera");
                MainCamera = cameraGo.AddComponent<Camera>();
                GlobalTools.SetTag(cameraGo,Tags.MainCamera);
                MainCamera.gameObject.GetOrAddComponent<AudioListener>();
            }

            ResetMainCamera();
        }


        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                ShowEffect(curEffectType);
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                HideEffect(curEffectType);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                FadeInEffect(curEffectType, fadeTime, () =>
                {
                    Debug.Log("Fade in");
                });
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                FadeOutEffect(curEffectType, fadeTime, () => Debug.Log("Fade out"));
            }
        }

    }
}
