using UnityEngine;

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

    }
}
