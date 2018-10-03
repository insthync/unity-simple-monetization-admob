using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class UIWatchAdmobRewardedVideoAdsButton : MonoBehaviour
{
    public enum WatchAdsButtonType
    {
        RewardCurrency,
        RewardProduct,
        Custom
    }
    public WatchAdsButtonType buttonType;
    public AdmobRewardedVideoAdSetting setting;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);

        setting.Init();

        switch (buttonType)
        {
            case WatchAdsButtonType.RewardCurrency:
                setting.onRewarded = (reward) =>
                {
                    MonetizationManager.Save.AddCurrency(reward.Type, (int)reward.Amount);
                };
                break;
            case WatchAdsButtonType.RewardProduct:
                setting.onRewarded = (reward) =>
                {
                    for (var i = 0; i < (int)reward.Amount; ++i)
                    {
                        MonetizationManager.Save.AddPurchasedItem(reward.Type);
                    }
                };
                break;
            default:
                break;
        }
    }

    private void OnClick()
    {
        setting.ShowAd();
    }
}
