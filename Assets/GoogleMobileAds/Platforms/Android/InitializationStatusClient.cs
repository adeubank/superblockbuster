// Copyright (C) 2018 Google, Inc.
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

using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;

namespace GoogleMobileAds.Android
{
    internal class InitializationStatusClient : IInitializationStatusClient
    {
        private readonly AndroidJavaObject statusMap;
        private AndroidJavaObject status;

        public InitializationStatusClient(AndroidJavaObject status)
        {
            this.status = status;
            statusMap = status.Call<AndroidJavaObject>("getAdapterStatusMap");
        }

        public AdapterStatus getAdapterStatusForClassName(string className)
        {
            var map = statusMap;
            var adapterStatus = map.Call<AndroidJavaObject>("get", className);

            if (adapterStatus == null) return null;

            var description = adapterStatus.Call<string>("getDescription");
            var latency = adapterStatus.Call<int>("getLatency");
            var state = new AndroidJavaClass(Utils.UnityAdapterStatusEnumName);
            var readyEnum = state.GetStatic<AndroidJavaObject>("READY");
            var adapterLoadState = adapterStatus.Call<AndroidJavaObject>("getInitializationState");
            var adapterState =
                adapterLoadState.Call<bool>("equals", readyEnum) ? AdapterState.Ready : AdapterState.NotReady;
            return new AdapterStatus(adapterState, description, latency);
        }

        public Dictionary<string, AdapterStatus> getAdapterStatusMap()
        {
            var map = new Dictionary<string, AdapterStatus>();
            var keys = getKeys();
            foreach (var key in keys) map.Add(key, getAdapterStatusForClassName(key));
            return map;
        }

        private string[] getKeys()
        {
            var map = statusMap;
            var keySet = map.Call<AndroidJavaObject>("keySet");
            var arrayClass = new AndroidJavaClass("java.lang.reflect.Array");
            var arrayObject = arrayClass.CallStatic<AndroidJavaObject>("newInstance",
                new AndroidJavaClass("java.lang.String"),
                map.Call<int>("size"));
            return keySet.Call<string[]>("toArray", arrayObject);
        }
    }
}

#endif