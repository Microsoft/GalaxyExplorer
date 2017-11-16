using UnityEngine;

namespace GalaxyExplorer
{
    public class CameraRigs : MonoBehaviour
    {
        void Awake()
        {
            if (MyAppPlatformManager.Platform == MyAppPlatformManager.PlatformId.ImmersiveHMD)
            {
                var cameras = GetComponentsInChildren<Camera>();
                for (int i = 0; i < cameras.Length; i++)
                {
                    cameras[i].nearClipPlane = 0.05f;
                }
            }
        }
    }
}