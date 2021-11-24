using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MonetizationManager))]
public class AdmobMonetizationManager : MonoBehaviour
{
    public AdmobBannerAdSetting bannerAd;
    public AdmobInterstitialAdSetting interstitialAd;
    public AdmobRewardedVideoAdSetting[] productRewardedVideoAds;
    public AdmobRewardedVideoAdSetting[] currencyRewardedVideoAds;
    public BaseAdmobRewardedVideoAdSetting overridePlacementRewardedAd;

    public void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(OnInit);
        if (bannerAd != null)
        {
            bannerAd.Init();
            if (bannerAd.showOnStart)
                bannerAd.ShowAd();
        }

        if (interstitialAd != null)
        {
            interstitialAd.Init();
            if (interstitialAd.showOnStart)
                interstitialAd.ShowAd();
        }

        if (productRewardedVideoAds != null && productRewardedVideoAds.Length > 0)
        {
            foreach (var ad in productRewardedVideoAds)
            {
                ad.Init();
                ad.onRewarded = (reward) =>
                {
                    for (var i = 0; i < (int)reward.Amount; ++i)
                    {
                        MonetizationManager.Save.AddPurchasedItem(reward.Type);
                    }
                };
                if (ad.showOnStart)
                    ad.ShowAd();
            }
        }

        if (currencyRewardedVideoAds != null && currencyRewardedVideoAds.Length > 0)
        {
            foreach (var ad in currencyRewardedVideoAds)
            {
                ad.Init();
                ad.onRewarded = (reward) =>
                {
                    MonetizationManager.Save.AddCurrency(reward.Type, (int)reward.Amount);
                };
                if (ad.showOnStart)
                    ad.ShowAd();
            }
        }
    }

    private void OnInit(InitializationStatus status)
    {

    }

    public void ShowBannerAd()
    {
        if (bannerAd != null)
            bannerAd.ShowAd();
    }

    public void HideBannerAd()
    {
        if (bannerAd != null)
            bannerAd.HideAd();
    }

    public void ShowInterstitialAd()
    {
        if (interstitialAd != null)
            interstitialAd.ShowAd();
    }

    public void HideInterstitialAd()
    {
        if (interstitialAd != null)
            interstitialAd.HideAd();
    }

    public void ShowProductRewardedVideoAd()
    {
        if (productRewardedVideoAds != null && productRewardedVideoAds.Length > 0)
            productRewardedVideoAds[Random.Range(0, productRewardedVideoAds.Length)].ShowAd();
    }

    public void ShowCurrencyRewardedVideoAd()
    {
        if (currencyRewardedVideoAds != null && currencyRewardedVideoAds.Length > 0)
            currencyRewardedVideoAds[Random.Range(0, currencyRewardedVideoAds.Length)].ShowAd();
    }

    public void ShowOverridePlacementRewardedAd(string placementId)
    {
        if (overridePlacementRewardedAd != null)
        {
            overridePlacementRewardedAd.ShowAd();
            overridePlacementRewardedAd.onRewarded = (reward) =>
            {
                System.Action<MonetizationManager.RemakeShowResult> showResultHandler;
                if (MonetizationManager.ShowResultCallbacks.TryGetValue(placementId, out showResultHandler) &&
                    showResultHandler != null)
                    showResultHandler.Invoke(MonetizationManager.RemakeShowResult.Finished);
            };
        }
    }
}

[System.Serializable]
public abstract class BaseAdmobAdSetting
{
    [Header("UnitID Settings")]
    public string androidUnitId = "";
    public string iosUnitId = "";

    [Header("General")]
    public bool showOnStart;
    public string[] testDevices;

    [Header("Events")]
    public UnityEvent onAdLoaded;
    public UnityEvent onAdFailedToLoad;
    public UnityEvent onAdOpening;
    public UnityEvent onAdClosed;
    public UnityEvent onShowAdNotLoaded;

    protected bool willShowOnLoaded;

    public string UnitId
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return androidUnitId;
                case RuntimePlatform.IPhonePlayer:
                    return iosUnitId;
            }
            return "unexpected_platform";
        }
    }

    public virtual void HandleOnAdLoaded(object sender, System.EventArgs args)
    {
        if (onAdLoaded != null)
            onAdLoaded.Invoke();
        if (willShowOnLoaded)
        {
            willShowOnLoaded = false;
            ShowAd();
        }
    }

    public virtual void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        if (onAdFailedToLoad != null)
            onAdFailedToLoad.Invoke();
    }

    public virtual void HandleOnAdOpened(object sender, System.EventArgs args)
    {
        if (onAdOpening != null)
            onAdOpening.Invoke();
    }

    public virtual void HandleOnAdClosed(object sender, System.EventArgs args)
    {
        if (onAdClosed != null)
            onAdClosed.Invoke();
    }

    public abstract void Init();
    public abstract void ShowAd();
    public abstract void HideAd();
}

public enum EAdSize
{
    BANNER,
    IAB_BANNER,
    MEDIUM_RECTANGLE,
    FULL_WIDTH,
    LEADERBOARD,
    SMART_BANNER,
    CUSTOM,
}

public enum EAdPosition
{
    BOTTOM,
    BOTTOM_LEFT,
    BOTTOM_RIGHT,
    CENTER,
    TOP,
    TOP_LEFT,
    TOP_RIGHT,
    CUSTOM,
}

[System.Serializable]
public class AdmobRewardedVideoEvent : UnityEvent<string> { }

[System.Serializable]
public class AdmobBannerAdSetting : BaseAdmobAdSetting
{
    [Header("Banner Settings")]
    public EAdSize adSize;
    public int customWidth;
    public int customHeight;
    public EAdPosition adPosition;
    public int customPositionX;
    public int customPositionY;

    private AdSize size;
    private AdPosition position;
    private BannerView bannerView;
    public bool IsInit { get; protected set; }

    public override void Init()
    {
        if (!IsInit)
        {
            IsInit = true;

            switch (adSize)
            {
                case EAdSize.BANNER:
                    size = AdSize.Banner;
                    break;
                case EAdSize.IAB_BANNER:
                    size = AdSize.IABBanner;
                    break;
                case EAdSize.MEDIUM_RECTANGLE:
                    size = AdSize.MediumRectangle;
                    break;
                case EAdSize.LEADERBOARD:
                    size = AdSize.Leaderboard;
                    break;
                case EAdSize.SMART_BANNER:
                    size = AdSize.SmartBanner;
                    break;
                case EAdSize.CUSTOM:
                    size = new AdSize(customWidth, customHeight);
                    break;
            }

            switch (adPosition)
            {
                case EAdPosition.BOTTOM:
                    position = AdPosition.Bottom;
                    break;
                case EAdPosition.BOTTOM_LEFT:
                    position = AdPosition.BottomLeft;
                    break;
                case EAdPosition.BOTTOM_RIGHT:
                    position = AdPosition.BottomRight;
                    break;
                case EAdPosition.CENTER:
                    position = AdPosition.Center;
                    break;
                case EAdPosition.TOP:
                    position = AdPosition.Top;
                    break;
                case EAdPosition.TOP_LEFT:
                    position = AdPosition.TopLeft;
                    break;
                case EAdPosition.TOP_RIGHT:
                    position = AdPosition.TopRight;
                    break;
            }

            if (adPosition == EAdPosition.CUSTOM)
                bannerView = new BannerView(UnitId, size, customPositionX, customPositionY);
            else
                bannerView = new BannerView(UnitId, size, position);

            // Called when an ad request has successfully loaded.
            bannerView.OnAdLoaded += HandleOnAdLoaded;
            // Called when an ad request failed to load.
            bannerView.OnAdFailedToLoad += HandleOnAdFailedToLoad;
            // Called when an ad is clicked.
            bannerView.OnAdOpening += HandleOnAdOpened;
            // Called when the user returned from the app after an ad click.
            bannerView.OnAdClosed += HandleOnAdClosed;
        }

        // Create an empty ad request.
        var builder = new AdRequest.Builder();
        var request = builder.Build();
        // Load the banner with the request.
        bannerView.LoadAd(request);
    }

    public override void ShowAd()
    {
        Init();
        bannerView.Show();
    }

    public override void HideAd()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }
        IsInit = false;
    }
}

[System.Serializable]
public class AdmobInterstitialAdSetting : BaseAdmobAdSetting
{
    private InterstitialAd interstitial;
    public bool IsInit { get; protected set; }

    public override void Init()
    {
        if (!IsInit)
        {
            IsInit = true;

            interstitial = new InterstitialAd(UnitId);

            // Called when an ad request has successfully loaded.
            interstitial.OnAdLoaded += HandleOnAdLoaded;
            // Called when an ad request failed to load.
            interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
            // Called when an ad is shown.
            interstitial.OnAdOpening += HandleOnAdOpened;
            // Called when the ad is closed.
            interstitial.OnAdClosed += HandleOnAdClosed;
        }

        // Create an empty ad request.
        var builder = new AdRequest.Builder();
        var request = builder.Build();
        // Load the banner with the request.
        interstitial.LoadAd(request);
    }

    public override void ShowAd()
    {
        Init();
        if (interstitial != null && interstitial.IsLoaded())
        {
            interstitial.Show();
        }
        else
        {
            willShowOnLoaded = true;
            onShowAdNotLoaded.Invoke();
        }
    }

    public override void HideAd()
    {
        if (interstitial != null)
        {
            interstitial.Destroy();
            interstitial = null;
        }
        IsInit = false;
    }

    public override void HandleOnAdClosed(object sender, System.EventArgs args)
    {
        base.HandleOnAdClosed(sender, args);
        HideAd();
    }
}

[System.Serializable]
public class BaseAdmobRewardedVideoAdSetting : BaseAdmobAdSetting
{
    public static bool IsInitEvent;
    public System.Action<Reward> onRewarded;
    private RewardedAd rewardedAd;
    public bool IsInit { get; protected set; }

    public override void Init()
    {
        if (!IsInit)
        {
            IsInit = true;

            rewardedAd = new RewardedAd(UnitId);

            // Called when an ad request has successfully loaded.
            rewardedAd.OnAdLoaded += HandleOnAdLoaded;
            // Called when an ad request failed to load.
            rewardedAd.OnAdFailedToLoad += HandleRewardBasedVideoFailedToLoad;
            // Called when an ad is shown.
            rewardedAd.OnAdOpening += HandleRewardBasedVideoStarted;
            // Called when an ad request failed to show.
            rewardedAd.OnAdFailedToShow += HandleRewardBasedVideoFailedToShow;
            // Called when the user should be rewarded for interacting with the ad.
            rewardedAd.OnUserEarnedReward += HandleRewardBasedVideoRewarded;
            // Called when the ad is closed.
            rewardedAd.OnAdClosed += HandleOnAdClosed;
        }

        // Create an empty ad request.
        var builder = new AdRequest.Builder();
        var request = builder.Build();
        // Load the banner with the request.
        rewardedAd.LoadAd(request);
    }

    public override void ShowAd()
    {
        Init();
        if (rewardedAd.IsLoaded())
        {
            rewardedAd.Show();
        }
        else
        {
            willShowOnLoaded = true;
            onShowAdNotLoaded.Invoke();
        }
    }

    public override void HideAd()
    {
    }

    public override void HandleOnAdClosed(object sender, System.EventArgs args)
    {
        base.HandleOnAdClosed(sender, args);
        rewardedAd = null;
        IsInit = false;
    }

    public virtual void HandleRewardBasedVideoStarted(object sender, System.EventArgs args)
    {
    }

    public virtual void HandleRewardBasedVideoFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
    }

    public virtual void HandleRewardBasedVideoFailedToShow(object sender, AdErrorEventArgs args)
    {
    }

    public virtual void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        if (onRewarded != null)
            onRewarded.Invoke(args);
    }
}

[System.Serializable]
public class AdmobRewardedVideoAdSetting : BaseAdmobRewardedVideoAdSetting
{
    [Header("Events")]
    public UnityEvent onAdStarted;
    public AdmobRewardedVideoEvent onAdRewarded;

    [Header("Event Text Formats")]
    [Tooltip("Rewarded Format: {0} = Product Name, {1} = Amount")]
    public string rewardedFormat = "You received {1} {0}(s)";

    public override void HandleRewardBasedVideoStarted(object sender, System.EventArgs args)
    {
        base.HandleRewardBasedVideoStarted(sender, args);

        if (onAdStarted != null)
            onAdStarted.Invoke();
    }

    public override void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        base.HandleRewardBasedVideoRewarded(sender, args);

        if (onAdRewarded != null)
            onAdRewarded.Invoke(string.Format(rewardedFormat, args.Type, args.Amount));
    }
}