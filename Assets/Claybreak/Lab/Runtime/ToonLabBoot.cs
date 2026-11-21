using UnityEngine;
using UnityEngine.SceneManagement;

namespace Claybreak.Lab
{
    /// <summary>
    /// Measurement players only: <c>--ez toonlab true</c> switches from the game scene to the toon lab scene before the
    /// game starts (the game objects are deactivated first, so the game never runs its Start).
    /// </summary>
    public static class ToonLabBoot
    {
#if (DEVELOPMENT_BUILD || RELEASE_MEASUREMENT) && UNITY_ANDROID && !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (!ToonLabSettings.TryReadLaunchExtras(0, out ToonLabSettings settings) || !settings.Requested)
            {
                return;
            }

            Scene active = SceneManager.GetActiveScene();
            if (active.name == ToonLab.SceneName)
            {
                return;
            }

            foreach (GameObject root in active.GetRootGameObjects())
            {
                root.SetActive(false);
            }

            Debug.Log("[ToonLab] toonlab extra: loading " + ToonLab.SceneName);
            SceneManager.LoadScene(ToonLab.SceneName, LoadSceneMode.Single);
        }
#endif
    }
}
