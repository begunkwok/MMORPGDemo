using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain
{
    public static class SceneComponentExtension
    {
        private static Vector3 CurSceneSpawnPos = Vector3.zero;

        public static void SetCurSceneSpawnPos(this SceneComponent sceneComponent, Vector3 spawnPos)
        {
            CurSceneSpawnPos = spawnPos;
        }

        public static Vector3 GetCurSceneSpawnPos(this SceneComponent sceneComponent)
        {
            return CurSceneSpawnPos;
        }
    }
}
