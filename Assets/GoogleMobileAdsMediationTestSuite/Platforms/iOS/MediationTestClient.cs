// <copyright company="Google" file="MediationTestClient.cs"> Copyright (C) 2017 Google, Inc. </copyright>
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

using System;
using System.Runtime.InteropServices;
using GoogleMobileAds.Api;
using GoogleMobileAds.iOS;
using GoogleMobileAdsMediationTestSuite.Common;

#if UNITY_IOS

namespace GoogleMobileAdsMediationTestSuite.iOS
{
    /// <summary>
    /// iOS implementation of Mediation Test Client interface <see cref="IMediationTestClient"/>
    /// </summary>
    public class MediationTestClient : IMediationTestClient
    {
        private static MediationTestClient instance = new MediationTestClient();
        private IntPtr nativeClientPtr;
        private IntPtr mediationClientPtr;

        private MediationTestClient(){
            this.mediationClientPtr = (IntPtr)GCHandle.Alloc(this);
        }

        public void Dispose()
        {
            Externs.GADUMRelease(this.nativeClientPtr);
            ((GCHandle)this.mediationClientPtr).Free();
        }

        ~MediationTestClient()
        {
            this.Dispose();
        }

        public event EventHandler<EventArgs> OnMediationTestSuiteDismissed;

        public AdRequest AdRequest
        {
            set
            {
                Externs.GADUMSetAdRequest(Utils.BuildAdRequest(value));
            }
        }
        internal delegate void GADUMediationTestSuiteDidDismissScreenCallback(IntPtr mediationClient);

        public static MediationTestClient Instance
        {
            get
            {
                return instance;
            }
        }

        // This property should be used when setting the nativeClientPtr.
        private IntPtr NativeClientPtr
        {
            get
            {
                return this.nativeClientPtr;
            }

            set
            {
                Externs.GADUMRelease(this.nativeClientPtr);
                this.nativeClientPtr = value;
            }
        }

        public void Show(string appId)
        {
            this.NativeClientPtr = Externs.GADUShowMediationTestSuiteWithAppID(appId, this.mediationClientPtr);
            Externs.GADUMSetMediationClientCallback(this.NativeClientPtr, MediationTestSuiteDidDismissScreenCallback);
        }

        public void Show()
        {
            this.NativeClientPtr = Externs.GADUShowMediationTestSuite(this.mediationClientPtr);
            Externs.GADUMSetMediationClientCallback(this.NativeClientPtr, MediationTestSuiteDidDismissScreenCallback);
        }

        [MonoPInvokeCallback(typeof(GADUMediationTestSuiteDidDismissScreenCallback))]
        private static void MediationTestSuiteDidDismissScreenCallback(IntPtr mediationClient)
        {
            MediationTestClient client = IntPtrToMediationTestClient(mediationClient);
            if (client.OnMediationTestSuiteDismissed != null)
            {
                client.OnMediationTestSuiteDismissed(client, EventArgs.Empty);
            }
        }

        private static MediationTestClient IntPtrToMediationTestClient(IntPtr mediationClient)
        {
            GCHandle handle = (GCHandle)mediationClient;
            return handle.Target as MediationTestClient;
        }

    }
}

#endif
