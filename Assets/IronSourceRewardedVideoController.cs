using UnityEngine;

public class IronSourceRewardedVideoController : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
        IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
        IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
        IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
        IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
        IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
        IronSourceEvents.onRewardedVideoAdClickedEvent += RewardedVideoAdClickedEvent;
    }

    public static bool IsRewardedVideoAvailable()
    {
        return IronSource.Agent.isRewardedVideoAvailable();
    }

    public static void ShowRewardedVideoButtonClicked()
    {
        Debug.Log("unity-script: ShowRewardedVideoButtonClicked");
        if (IsRewardedVideoAvailable())
            IronSource.Agent.showRewardedVideo();
        else
            Debug.Log("unity-script: IronSource.Agent.isRewardedVideoAvailable - False");
    }


    /************* RewardedVideo Delegates *************/
    private void RewardedVideoAvailabilityChangedEvent(bool canShowAd)
    {
        Debug.Log("unity-script: I got RewardedVideoAvailabilityChangedEvent, value = " + canShowAd);
    }

    private void RewardedVideoAdOpenedEvent()
    {
        Debug.Log("unity-script: I got RewardedVideoAdOpenedEvent");
    }

    private void RewardedVideoAdRewardedEvent(IronSourcePlacement ssp)
    {
        Debug.Log("unity-script: I got RewardedVideoAdRewardedEvent, amount = " + ssp.getRewardAmount() + " name = " + ssp.getRewardName());
        CurrencyManager.Instance.AddCoinBalance(ssp.getRewardAmount());
    }

    private void RewardedVideoAdClosedEvent()
    {
        Debug.Log("unity-script: I got RewardedVideoAdClosedEvent");
    }

    private void RewardedVideoAdStartedEvent()
    {
        Debug.Log("unity-script: I got RewardedVideoAdStartedEvent");
    }

    private void RewardedVideoAdEndedEvent()
    {
        Debug.Log("unity-script: I got RewardedVideoAdEndedEvent");
    }

    private void RewardedVideoAdShowFailedEvent(IronSourceError error)
    {
        Debug.Log("unity-script: I got RewardedVideoAdShowFailedEvent, code :  " + error.getCode() + ", description : " + error.getDescription());
    }

    private void RewardedVideoAdClickedEvent(IronSourcePlacement ssp)
    {
        Debug.Log("unity-script: I got RewardedVideoAdClickedEvent, name = " + ssp.getRewardName());
    }
}