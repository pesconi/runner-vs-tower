using System;
using JungleDegree.StudioKit.Swarm;
using UnityEngine;

namespace Claybreak
{
    /// <summary>
    /// Measurement players only (RT-6t.11): <c>adb shell am start ... --ez chunks true</c> forces the fallback draw route of
    /// the instanced crowd, props and units in flight (blocks of matrices, as on OpenGL ES devices without structured
    /// buffers in the vertex shader) before the first scene loads; <c>--ez chunks false</c> forces the buffer route. With
    /// <c>-e unity -force-gles</c> on the same command the device runs OpenGL ES. Release builds compile it to nothing.
    /// </summary>
    public static class DrawRouteExtra
    {
#if (DEVELOPMENT_BUILD || RELEASE_MEASUREMENT) && UNITY_ANDROID && !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Apply()
        {
            try
            {
                using AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");
                if (intent.Call<bool>("hasExtra", "chunks"))
                {
                    ToonInstanceRouting.Forced = intent.Call<bool>("getBooleanExtra", "chunks", true)
                        ? ToonInstanceRoute.Chunks
                        : ToonInstanceRoute.Buffer;
                }
                else
                {
                    ToonInstanceRouting.Apply();
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[ToonRoute] Could not read the chunks extra: " + exception.Message);
            }
        }
#endif
    }
}
