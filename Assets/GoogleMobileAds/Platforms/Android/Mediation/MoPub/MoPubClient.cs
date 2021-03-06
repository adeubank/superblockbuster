// Copyright 2018 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if UNITY_ANDROID

using GoogleMobileAds.Common.Mediation.MoPub;
using UnityEngine;

namespace GoogleMobileAds.Android.Mediation.MoPub
{
    public class MoPubClient : IMoPubClient
    {
        private static string adUnitId;
        private static MoPubClient instance = new MoPubClient();

        private MoPubClient()
        {
        }

        public static MoPubClient Instance
        {
            get { return instance; }
        }

        public void Initialize(string moPubAdUnitID)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            AndroidJavaObject moPubSdkConfigurationBuilder =
                new AndroidJavaObject("com.mopub.common.SdkConfiguration$Builder", moPubAdUnitID);
            AndroidJavaObject moPubSdkConfiguration =
                moPubSdkConfigurationBuilder.Call<AndroidJavaObject>("build");

            AndroidJavaClass moPub = new AndroidJavaClass("com.mopub.common.MoPub");

            // MoPub requires its SDK to be initialized on the Main Thread.
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() => { moPub.CallStatic("initializeSdk", currentActivity, moPubSdkConfiguration, null); }));
        }

        public bool IsInitialized()
        {
            AndroidJavaClass moPub = new AndroidJavaClass("com.mopub.common.MoPub");
            return moPub.CallStatic<bool>("isSdkInitialized");
        }
    }
}

#endif