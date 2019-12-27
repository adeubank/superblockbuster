using UnityEditor;
using UnityEngine;

namespace GoogleMobileAds.Editor
{
    internal class GoogleMobileAdsSettings : ScriptableObject
    {
        private const string MobileAdsSettingsDir = "Assets/GoogleMobileAds";

        private const string MobileAdsSettingsResDir = "Assets/GoogleMobileAds/Resources";

        private const string MobileAdsSettingsFile =
            "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";

        private static GoogleMobileAdsSettings instance;

        [SerializeField] private string adMobAndroidAppId = string.Empty;

        [SerializeField] private string adMobIOSAppId = string.Empty;

        [SerializeField] private bool delayAppMeasurementInit;

        [SerializeField] private bool isAdManagerEnabled;

        [SerializeField] private bool isAdMobEnabled;

        public bool IsAdManagerEnabled
        {
            get => Instance.isAdManagerEnabled;

            set => Instance.isAdManagerEnabled = value;
        }

        public bool IsAdMobEnabled
        {
            get => Instance.isAdMobEnabled;

            set => Instance.isAdMobEnabled = value;
        }

        public string AdMobAndroidAppId
        {
            get => Instance.adMobAndroidAppId;

            set => Instance.adMobAndroidAppId = value;
        }

        public string AdMobIOSAppId
        {
            get => Instance.adMobIOSAppId;

            set => Instance.adMobIOSAppId = value;
        }

        public bool DelayAppMeasurementInit
        {
            get => Instance.delayAppMeasurementInit;

            set => Instance.delayAppMeasurementInit = value;
        }

        public static GoogleMobileAdsSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    if (!AssetDatabase.IsValidFolder(MobileAdsSettingsResDir)) AssetDatabase.CreateFolder(MobileAdsSettingsDir, "Resources");

                    instance = (GoogleMobileAdsSettings) AssetDatabase.LoadAssetAtPath(
                        MobileAdsSettingsFile, typeof(GoogleMobileAdsSettings));

                    if (instance == null)
                    {
                        instance = CreateInstance<GoogleMobileAdsSettings>();
                        AssetDatabase.CreateAsset(instance, MobileAdsSettingsFile);
                    }
                }

                return instance;
            }
        }

        internal void WriteSettingsToFile()
        {
            AssetDatabase.SaveAssets();
        }
    }
}