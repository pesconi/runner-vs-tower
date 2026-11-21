using System;
using UnityEngine;

namespace Claybreak.Lab
{
    /// <summary>
    /// What the toon lab measures. In measurement players (RELEASE_MEASUREMENT or development, Android) it comes from
    /// the launch extras: <c>adb shell am start ... --ez toonlab true --ei units N [--ei msaa 1|2|4] [--ez hdr true|false]
    /// [--ei fps 60]</c>. Values left out keep the project settings.
    /// </summary>
    public struct ToonLabSettings
    {
        public const int KeepMsaa = 0;
        public const int KeepHdr = -1;

        public bool Requested;
        public int Units;
        public int Msaa;
        public int Hdr;
        public int FrameRate;

        public static ToonLabSettings Defaults(int units)
        {
            return new ToonLabSettings { Units = units, Msaa = KeepMsaa, Hdr = KeepHdr, FrameRate = 0 };
        }

        /// <summary>Reads the launch extras; false outside measurement players on Android or when they cannot be read.</summary>
        public static bool TryReadLaunchExtras(int defaultUnits, out ToonLabSettings settings)
        {
            settings = Defaults(defaultUnits);
#if (DEVELOPMENT_BUILD || RELEASE_MEASUREMENT) && UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");
                settings.Requested = intent.Call<bool>("getBooleanExtra", "toonlab", false);
                settings.Units = intent.Call<int>("getIntExtra", "units", defaultUnits);
                settings.Msaa = intent.Call<int>("getIntExtra", "msaa", KeepMsaa);
                settings.Hdr = intent.Call<bool>("hasExtra", "hdr") ? (intent.Call<bool>("getBooleanExtra", "hdr", false) ? 1 : 0) : KeepHdr;
                settings.FrameRate = intent.Call<int>("getIntExtra", "fps", 0);
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[ToonLab] Could not read the launch extras: " + exception.Message);
                return false;
            }
#else
            return false;
#endif
        }
    }
}
