using UnityEngine;
using UnityEngine.UI;

public class DeveloperMenu : MonoBehaviour
{
    public Image doublePowerupSpawn;
    public Image ToggleAdsImage;
    public Text ToggleAdsText;
    
    // Start is called before the first frame update
    void Start()
    {
        SetDoublePowerupSpawnUIState();
        SetToggleAdsUIState();
    }

    public void ToggleDoublePowerupSpawn()
    {
        GameController.Instance.DoublePowerupSpawn = !GameController.Instance.DoublePowerupSpawn;
        SetDoublePowerupSpawnUIState();
    }

    void SetDoublePowerupSpawnUIState()
    {
        doublePowerupSpawn.color = GameController.Instance.DoublePowerupSpawn ? Color.yellow : Color.white;
    }

    void SetToggleAdsUIState()
    {
        ToggleAdsImage.color = RemoteConfigController.Instance.AdsEnabled ? Color.white : Color.red;
        ToggleAdsText.text = RemoteConfigController.Instance.AdsEnabled ? "DISABLE ADS" : "ENABLE ADS";
    }
    
    public void AddCoins()
    {
        CurrencyManager.Instance.AddCoinBalance(500);
    }

    public void ShowMediationTestSuite()
    {
        AdController.Instance.MediationTestSuiteShow();
    }

    public void ToggleAds()
    {
        RemoteConfigController.Instance.AdsEnabled = !RemoteConfigController.Instance.AdsEnabled;
        SetToggleAdsUIState();
    }
}
