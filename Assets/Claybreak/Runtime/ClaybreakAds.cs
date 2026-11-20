using System;
using JungleDegree.StudioKit.Ads;
using UnityEngine;

namespace Claybreak
{
    /// <summary>What the flow needs from ads, so tests can stand in for the SDK.</summary>
    public interface IAdGateway
    {
        /// <summary>A rewarded ad is loaded: an offer may be shown (never offer what cannot play).</summary>
        bool RewardedReady { get; }

        /// <summary>UMP requires a privacy options entry point (EEA/UK and similar regions).</summary>
        bool PrivacyOptionsRequired { get; }

        /// <summary>Rewarded readiness or the privacy requirement changed.</summary>
        event Action Changed;

        /// <summary>Plays a rewarded ad; the callback says whether the reward was earned (always called).</summary>
        void ShowRewarded(Action<bool> earned);

        /// <summary>A level was finished (interstitial pacing).</summary>
        void LevelFinished();

        /// <summary>Shows the interstitial when it is on and due; <paramref name="done"/> is always called.</summary>
        void ShowInterstitialIfDue(Action done);

        void ShowPrivacyOptions();
    }

    /// <summary>
    /// The scene's ads: the kit's rewarded and interstitial services behind <see cref="IAdGateway"/>, set up from
    /// <see cref="ClaybreakMonetization"/>. Consent (UMP) and the SDK start inside the kit's
    /// <c>MobileAdsGate</c>; nothing is requested before consent allows it.
    /// </summary>
    public sealed class ClaybreakAds : MonoBehaviour, IAdGateway
    {
        [SerializeField] private ClaybreakMonetization monetization;
        [SerializeField] private RewardedAdService rewarded;
        [SerializeField] private InterstitialAdService interstitial;

        public event Action Changed;

        public bool HasParts => monetization != null && rewarded != null && interstitial != null;
        public bool RewardedReady => rewarded != null && rewarded.Status == RewardedAdStatus.Ready;
        public bool PrivacyOptionsRequired => rewarded != null && rewarded.PrivacyOptionsRequired;

        private void Awake()
        {
            if (!HasParts)
            {
                Debug.LogError("ClaybreakAds is missing a part (rebuild the scene).", this);
                enabled = false;
                return;
            }

            interstitial.Configure(monetization.InterstitialOn, monetization.InterstitialPacing);
            rewarded.StatusChanged += OnRewardedStatus;
            rewarded.PrivacyOptionsRequirementChanged += OnPrivacyRequirement;
        }

        private void OnDestroy()
        {
            if (rewarded != null)
            {
                rewarded.StatusChanged -= OnRewardedStatus;
                rewarded.PrivacyOptionsRequirementChanged -= OnPrivacyRequirement;
            }
        }

        public void ShowRewarded(Action<bool> earned)
        {
            rewarded.ShowRewardedAd(result => earned?.Invoke(result == RewardedAdResult.Earned));
        }

        public void LevelFinished()
        {
            interstitial.RecordLevel();
        }

        public void ShowInterstitialIfDue(Action done)
        {
            interstitial.ShowIfDue(_ => done?.Invoke());
        }

        public void ShowPrivacyOptions()
        {
            rewarded.ShowPrivacyOptionsForm();
        }

        private void OnRewardedStatus(RewardedAdStatus status)
        {
            Changed?.Invoke();
        }

        private void OnPrivacyRequirement(bool required)
        {
            Changed?.Invoke();
        }
    }
}
