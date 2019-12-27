// Copyright (C) 2017 Google, Inc.
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

using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace GoogleMobileAds.Android
{
    public class MobileAdsClient : AndroidJavaProxy, IMobileAdsClient
    {
        private Action<InitializationStatus> initCompleteAction;

        private MobileAdsClient() : base(Utils.OnInitializationCompleteListenerClassName)
        {
        }

        public static MobileAdsClient Instance { get; } = new MobileAdsClient();

        public void Initialize(string appId)
        {
            var playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
            var activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            var mobileAdsClass = new AndroidJavaClass(Utils.MobileAdsClassName);
            mobileAdsClass.CallStatic("initialize", activity, appId);
        }

        public void Initialize(Action<InitializationStatus> initCompleteAction)
        {
            this.initCompleteAction = initCompleteAction;

            var playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
            var activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            var mobileAdsClass = new AndroidJavaClass(Utils.MobileAdsClassName);
            mobileAdsClass.CallStatic("initialize", activity, this);
        }

        public void SetApplicationVolume(float volume)
        {
            var mobileAdsClass = new AndroidJavaClass(Utils.MobileAdsClassName);
            mobileAdsClass.CallStatic("setAppVolume", volume);
        }

        public void SetApplicationMuted(bool muted)
        {
            var mobileAdsClass = new AndroidJavaClass(Utils.MobileAdsClassName);
            mobileAdsClass.CallStatic("setAppMuted", muted);
        }

        public void SetiOSAppPauseOnBackground(bool pause)
        {
            // Do nothing on Android. Default behavior is to pause when app is backgrounded.
        }

        public float GetDeviceScale()
        {
            var playerClass = new AndroidJavaClass(Utils.UnityActivityClassName);
            var activity =
                playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            var resources = activity.Call<AndroidJavaObject>("getResources");
            var metrics = resources.Call<AndroidJavaObject>("getDisplayMetrics");
            return metrics.Get<float>("density");
        }

        #region Callbacks from OnInitializationCompleteListener.

        public void onInitializationComplete(AndroidJavaObject initStatus)
        {
            if (initCompleteAction != null)
            {
                var status = new InitializationStatus(new InitializationStatusClient(initStatus));
                initCompleteAction(status);
            }
        }

        #endregion
    }
}

#endif