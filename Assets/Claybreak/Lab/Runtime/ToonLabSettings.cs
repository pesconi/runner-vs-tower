using System;
using UnityEngine;

namespace Claybreak.Lab
{
    /// <summary>
    /// What the toon lab measures. In measurement players (RELEASE_MEASUREMENT or development, Android) it comes from
    /// the launch extras: <c>adb shell am start ... --ez toonlab true --ei units N [--ei dying D] [--ei shape 0|1]
    /// [--ez eyes true|false] [--ei msaa 1|2|4] [--ez hdr true|false] [--ei fps 60]</c>. Values left out keep the scene or
    /// project settings (shape 0 = Bolinha, 1 = Pintinho; dying 0 = no deaths). With D &gt; 0 the lab keeps the N live
    /// units while it throws out the outer ring and pops it back, fast enough to keep D units dying at all times (RT-6t.5).
    /// RT-6v3.2: <c>--ei vinyl N</c> rehearses a vinyl direction of decision D36 (0 = StudioKit/ToonLit, 1..3 = directions A..C
    /// of <see cref="VinylDirection.All"/> on the lab shader), both with the smoothed Bibopzi of the concept screens;
    /// <c>--ez post true|false</c> turns the direction's post stack on or off (default off).
    /// </summary>
    public struct ToonLabSettings
    {
        public const int KeepMsaa = 0;
        public const int KeepHdr = -1;
        public const int KeepShape = -1;
        public const int KeepEyes = -1;
        public const int KeepVinyl = -1;

        public bool Requested;
        public int Units;
        public int Dying;
        public int Msaa;
        public int Hdr;
        public int Shape;
        public int Eyes;
        public int FrameRate;
        public int Vinyl;
        public bool Post;

        public static ToonLabSettings Defaults(int units)
        {
            return new ToonLabSettings { Units = units, Dying = 0, Msaa = KeepMsaa, Hdr = KeepHdr, Shape = KeepShape, Eyes = KeepEyes, FrameRate = 0, Vinyl = KeepVinyl, Post = false };
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
                settings.Dying = intent.Call<int>("getIntExtra", "dying", 0);
                settings.Msaa = intent.Call<int>("getIntExtra", "msaa", KeepMsaa);
                settings.Hdr = intent.Call<bool>("hasExtra", "hdr") ? (intent.Call<bool>("getBooleanExtra", "hdr", false) ? 1 : 0) : KeepHdr;
                settings.Shape = intent.Call<int>("getIntExtra", "shape", KeepShape);
                settings.Eyes = intent.Call<bool>("hasExtra", "eyes") ? (intent.Call<bool>("getBooleanExtra", "eyes", true) ? 1 : 0) : KeepEyes;
                settings.FrameRate = intent.Call<int>("getIntExtra", "fps", 0);
                settings.Vinyl = intent.Call<int>("getIntExtra", "vinyl", KeepVinyl);
                settings.Post = intent.Call<bool>("getBooleanExtra", "post", false);
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
