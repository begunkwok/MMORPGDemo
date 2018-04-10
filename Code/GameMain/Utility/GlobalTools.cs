using UnityEngine;
using UnityEngine.AI;

namespace GameMain
{
    public class GlobalTools
    {
        public static void SetTag(GameObject go, string tag)
        {
            if (go != null)
            {
                go.tag = tag;
            }    
        }

        public static void SetLayer(GameObject go, int layer)
        {
            go.layer = layer;

            Transform t = go.transform;

            for (int i = 0, imax = t.childCount; i < imax; ++i)
            {
                Transform child = t.GetChild(i);
                SetLayer(child.gameObject, layer);
            }
        }

        public static Transform GetBone(Transform trans, string boneName)
        {
            if (string.IsNullOrEmpty(boneName))
            {
                return null;
            }
            Transform[] tran = trans.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in tran)
            {
                if (t.name == boneName)
                {
                    return t;
                }

            };
            return null;
        }

        public static bool GetValueFromBitMark(uint value, int index)
        {
            return (value >> index) % 2 == 1;
        }

        public static Vector3 NavSamplePosition(Vector3 srcPosition)
        {
            Vector3 dstPosition = srcPosition;
            NavMeshHit meshHit = new NavMeshHit();
            int layer = 1 << NavMesh.GetAreaFromName("Walkable");
            if (NavMesh.SamplePosition(srcPosition, out meshHit, 100, layer))
            {
                dstPosition = meshHit.position;
            }
            return dstPosition;
        }

        public static float GetHorizontalDistance(Vector3 a, Vector3 b)
        {
            Vector3 v1 = a;
            v1.y = 0;
            Vector3 v2 = b;
            v2.y = 0;
            return Vector3.Distance(v1, v2);
        }

        public static bool IsTrigger(float ratio)
        {
            float r = UnityEngine.Random.Range(0, 1f);
            return r < ratio ? true : false;
        }
    }
}
