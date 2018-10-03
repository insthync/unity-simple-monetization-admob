using GoogleMobileAds.Api;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MonetizationManager))]
public class AdmobMonetizationManager : MonoBehaviour
{
    [Header("AppID Settings")]
    public string androidAppId = "";
    public string iosAppId = "";

    public AdmobBannerAdSetting bannerAd;
    public AdmobInterstitialAdSetting interstitialAd;
    public AdmobRewardedVideoAdSetting[] productRewardedVideoAds;
    public AdmobRewardedVideoAdSetting[] currencyRewardedVideoAds;

    public string AppId
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    return androidAppId;
                case RuntimePlatform.IPhonePlayer:
                    return iosAppId;
            }
            return "unexpected_platform";
        }
    }

    public void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(AppId);

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
}

[System.Serializable]
public abstract class BaseAdmobAdSetting
{
    [Header("UnitID Settings")]
    public string androidUnitId = "";
    public string iosUnitId = "";

    [Header("General")]
    public bool showOnStart;

    [Header("Events")]
    public UnityEvent onAdLoaded;
    public UnityEvent onAdFailedToLoad;
    public UnityEvent onAdOpening;
    public UnityEvent onAdClosed;
    public UnityEvent onShowAdNotLoaded;

    public bool IsInit { get; protected set; }

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

    public void HandleOnAdLoaded(object sender, System.EventArgs args)
    {
        if (onAdLoaded != null)
            onAdLoaded.Invoke();
    }

    public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
    {
        if (onAdFailedToLoad != null)
            onAdFailedToLoad.Invoke();
    }

    public void HandleOnAdOpened(object sender, System.EventArgs args)
    {
        if (onAdOpening != null)
            onAdOpening.Invoke();
    }

    public void HandleOnAdClosed(object sender, System.EventArgs args)
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

    public override void Init()
    {
        if (IsInit)
            return;

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

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
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
    }
}

[System.Serializable]
public class AdmobInterstitialAdSetting : BaseAdmobAdSetting
{
    private InterstitialAd interstitial;

    public override void Init()
    {
        if (IsInit)
            return;

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

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        interstitial.LoadAd(request);
    }

    public override void ShowAd()
    {
        Init();
        if (interstitial != null && interstitial.IsLoaded())
            interstitial.Show();
        else
            onShowAdNotLoaded.Invoke();
    }

    public override void HideAd()
    {
        if (interstitial != null)
        {
            interstitial.Destroy();
            interstitial = null;
        }
    }
}

[System.Serializable]
public class AdmobRewardedVideoAdSetting : BaseAdmobAdSetting
{
    [Header("Events")]
    public UnityEvent onAdStarted;
    public AdmobRewardedVideoEvent onAdRewarded;

    [Header("Event Text Formats")]
    [Tooltip("Rewarded Format: {0} = Product Name, {1} = Amount")]
    public string rewardedFormat = "You received {1} {0}(s)";

    public System.Action<Reward> onRewarded;
    private RewardBasedVideoAd rewardBasedVideo;

    public override void Init()
    {
        if (IsInit)
            return;

        IsInit = true;

        rewardBasedVideo = RewardBasedVideoAd.Instance;

        // Called when an ad request has successfully loaded.
        rewardBasedVideo.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        rewardBasedVideo.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        rewardBasedVideo.OnAdOpening += HandleOnAdOpened;
        // Called when the ad is closed.
        rewardBasedVideo.OnAdClosed += HandleOnAdClosed;
        // Called when the ad starts to play.
        rewardBasedVideo.OnAdStarted += HandleRewardBasedVideoStarted;
        // Called when the user should be rewarded for watching a video.
        rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded video ad with the request.
        rewardBasedVideo.LoadAd(request, UnitId);
    }

    public override void ShowAd()
    {
        Init();
        if (rewardBasedVideo != null && rewardBasedVideo.IsLoaded())
            rewardBasedVideo.Show();
        else
            onShowAdNotLoaded.Invoke();
    }

    public override void HideAd()
    {
    }

    public void HandleRewardBasedVideoStarted(object sender, System.EventArgs args)
    {
        if (onAdStarted != null)
            onAdStarted.Invoke();
    }

    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        if (onRewarded != null)
            onRewarded.Invoke(args);

        if (onAdRewarded != null)
            onAdRewarded.Invoke(string.Format(rewardedFormat, args.Type, args.Amount));
    }
}