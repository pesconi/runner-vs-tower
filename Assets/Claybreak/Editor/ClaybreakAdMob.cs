using System;
using System.IO;
using System.Text.RegularExpressions;
using JungleDegree.StudioKit.Ads;
using UnityEditor;
using UnityEngine;

namespace Claybreak.Editor
{
    /// <summary>
    /// The game's AdMob IDs (D35 = A): the AdMob app ID and the rewarded unit of the studio's business account live
    /// OUTSIDE every repository, in <c>admob-ids.json</c> (fields <c>androidAppId</c>, <c>rewardedAdUnitId</c>), and go
    /// into the assets only for the duration of one game build (<see cref="Scope"/>), then the committed values come
    /// back: Google's sample app ID and an empty unit, so the Editor, the tests and the measurement builds show
    /// Google's test ads. The monetization validator refuses any real ID left in the committed assets.
    /// <para>Where the file is read from: the environment variable <see cref="EnvironmentVariable"/>, or
    /// <see cref="DefaultPath"/> (<c>..\_privado\runner-vs-tower\admob-ids.json</c> beside the repositories).
    /// The interstitial has no unit here: it stays off (D6).</para>
    /// </summary>
    public static class ClaybreakAdMob
    {
        public const string EnvironmentVariable = "CLAYBREAK_ADMOB_IDS";
        public const string AdsSettingsPath = "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";

        private static readonly Regex AppIdPattern = new Regex(@"^ca-app-pub-[0-9]{16}~[0-9]{10}$", RegexOptions.CultureInvariant);

        /// <summary>The default place of the IDs: outside the repository, next to the games' folders.</summary>
        public static string DefaultPath => Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "_privado",
            "runner-vs-tower", "admob-ids.json"));

        [Serializable]
        private sealed class IdsFile
        {
            public string androidAppId;
            public string rewardedAdUnitId;
        }

        /// <summary>The two IDs of one game build.</summary>
        public readonly struct Ids
        {
            public readonly string AppId;
            public readonly string RewardedUnitId;

            public Ids(string appId, string rewardedUnitId)
            {
                AppId = appId;
                RewardedUnitId = rewardedUnitId;
            }
        }

        /// <summary>The file this build reads (environment variable first, then the default place).</summary>
        public static string ConfigPath()
        {
            string fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariable);
            return string.IsNullOrEmpty(fromEnvironment) ? DefaultPath : Path.GetFullPath(fromEnvironment);
        }

        /// <summary>
        /// Null when <paramref name="appId"/> and <paramref name="rewardedUnitId"/> are a production pair (app with
        /// <c>~</c>, unit with <c>/</c>, both of the same <c>pub-</c> account, not Google's sample account); otherwise
        /// what is wrong, without repeating the values.
        /// </summary>
        public static string Problem(string appId, string rewardedUnitId)
        {
            if (string.IsNullOrEmpty(appId) || !AppIdPattern.IsMatch(appId))
            {
                return "androidAppId is not an AdMob app ID (ca-app-pub-<16 digits>~<10 digits>)";
            }

            if (!AdUnitSettings.IsWellFormedUnitId(rewardedUnitId))
            {
                return "rewardedAdUnitId is not an AdMob ad unit ID (ca-app-pub-<16 digits>/<10 digits>)";
            }

            if (appId.StartsWith(AdUnitSettings.GoogleSamplePublisher + "~", StringComparison.Ordinal)
                || AdUnitSettings.IsGoogleTestUnit(rewardedUnitId))
            {
                return "the IDs are Google's test IDs, not the studio's";
            }

            if (appId.Split('~')[0] != rewardedUnitId.Split('/')[0])
            {
                return "androidAppId and rewardedAdUnitId belong to different AdMob accounts";
            }

            return null;
        }

        /// <summary>Reads and checks the IDs file, or throws saying what to do.</summary>
        public static Ids Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new InvalidOperationException("[ClaybreakAdMob] The game build needs the AdMob IDs in " + path + " (D35: outside the "
                    + "repository; fields androidAppId and rewardedAdUnitId; docs/store/passo-a-passo-console.md, step 4).");
            }

            if (ClaybreakBuild.InsideARepository(path))
            {
                throw new InvalidOperationException("[ClaybreakAdMob] admob-ids.json must live outside every git repository (D35).");
            }

            IdsFile file;
            try
            {
                file = JsonUtility.FromJson<IdsFile>(File.ReadAllText(path));
            }
            catch (ArgumentException)
            {
                throw new InvalidOperationException("[ClaybreakAdMob] " + path + " is not valid JSON.");
            }

            string appId = file?.androidAppId?.Trim();
            string unit = file?.rewardedAdUnitId?.Trim();
            string problem = Problem(appId, unit);
            if (problem != null)
            {
                throw new InvalidOperationException("[ClaybreakAdMob] " + path + ": " + problem + ".");
            }

            return new Ids(appId, unit);
        }

        /// <summary>
        /// Puts the production IDs into the AdMob plug-in settings and the game's ad units for one build and restores
        /// the committed values when disposed (also when the build fails).
        /// </summary>
        public sealed class Scope : IDisposable
        {
            private const string AppIdField = "adMobAndroidAppId";
            private const string RewardedField = "androidRewardedUnitId";

            public Scope(Ids ids)
            {
                // The committed values are fixed (Google's sample app, no unit), so the restore never depends on what was
                // there before: a build killed half way cannot turn real IDs into "the previous values".
                if (Read(AdsSettingsPath, AppIdField) != ProjectConfiguration.SampleAdMobAppId
                    || Read(MonetizationContent.AdUnitsPath, RewardedField).Length != 0)
                {
                    throw new InvalidOperationException("[ClaybreakAdMob] " + AdsSettingsPath + " or " + MonetizationContent.AdUnitsPath
                        + " does not hold the committed values (an earlier build stopped half way?): restore them with git first (D35).");
                }

                try
                {
                    Write(AdsSettingsPath, AppIdField, ids.AppId);
                    Write(MonetizationContent.AdUnitsPath, RewardedField, ids.RewardedUnitId);
                    AssetDatabase.SaveAssets();
                }
                catch
                {
                    Dispose();
                    throw;
                }

                Debug.Log("[ClaybreakAdMob] Production AdMob app and rewarded unit applied for this build (from outside the repository).");
            }

            // The assets are loaded again by path: the build unloads objects (the plug-in's settings among them), so a
            // reference kept from before the build may be dead here. Each restore runs even if the other one fails.
            public void Dispose()
            {
                try
                {
                    Write(AdsSettingsPath, AppIdField, ProjectConfiguration.SampleAdMobAppId);
                }
                finally
                {
                    try
                    {
                        Write(MonetizationContent.AdUnitsPath, RewardedField, string.Empty);
                    }
                    finally
                    {
                        AssetDatabase.SaveAssets();
                    }
                }

                Debug.Log("[ClaybreakAdMob] Committed AdMob values restored (Google's sample app, no unit).");
            }

            private static SerializedObject Open(string path, string property, out SerializedProperty value)
            {
                ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (asset == null)
                {
                    throw new InvalidOperationException("[ClaybreakAdMob] " + path + " is missing (rebuild the scene).");
                }

                var so = new SerializedObject(asset);
                value = so.FindProperty(property);
                if (value == null)
                {
                    throw new InvalidOperationException("[ClaybreakAdMob] " + path + " has no field " + property + ".");
                }

                return so;
            }

            private static string Read(string path, string property)
            {
                Open(path, property, out SerializedProperty value);
                return value.stringValue;
            }

            private static void Write(string path, string property, string text)
            {
                SerializedObject so = Open(path, property, out SerializedProperty value);
                value.stringValue = text;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(so.targetObject);
            }
        }
    }
}
