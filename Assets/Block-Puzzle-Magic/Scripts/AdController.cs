using GoogleMobileAds.Api;
using UnityEngine;

public class AdController : MonoBehaviour
{
    private bool _adsInitialized;

    public bool AdsInitialized()
    {
        return _adsInitialized;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize the Google Mobile Ads SDK.
        // Do not call other ads until ads are initialized when using admob mediation
        MobileAds.Initialize(initStatus => { _adsInitialized = true; });
    }
}