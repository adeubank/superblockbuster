using UnityEngine;

public class IronSourceAppController : MonoBehaviour
{
#if UNITY_IPHONE
    public const string AppKey = "ac47fee5";
#endif
#if UNITY_ANDROID
    public const string AppKey = "a9f966c5";
#endif

    // Start is called before the first frame update
    private void Start()
    {
        if (AppKey == "")
        {
            Debug.Log("Invalid platform. No AppKey found.");
            return;
        }

        if (Debug.isDebugBuild)
        {
            Debug.Log("unity-script: IronSourceAppController Start called");

            var id = IronSource.Agent.getAdvertiserId();
            Debug.Log("unity-script: IronSource.Agent.getAdvertiserId : " + id);

            Debug.Log("unity-script: IronSource.Agent.validateIntegration");
            IronSource.Agent.validateIntegration();

            Debug.Log("unity-script: unity version" + IronSource.unityVersion());
        }

        // SDK init
        Debug.Log("unity-script: IronSource.Agent.init");
        IronSource.Agent.init(AppKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
        IronSource.Agent.shouldTrackNetworkState(true);

        //Set User ID For Server To Server Integration
        //// IronSource.Agent.setUserId ("UserId");
    }

    private void OnApplicationPause(bool isPaused)
    {
        Debug.Log("unity-script: OnApplicationPause = " + isPaused);
        IronSource.Agent.onApplicationPause(isPaused);
    }
}