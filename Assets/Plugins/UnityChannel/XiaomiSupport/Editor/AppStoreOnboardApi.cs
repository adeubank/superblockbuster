﻿#if UNITY_5_6_OR_NEWER && !UNITY_5_6_0
using System;
using System.Text;
using AppStoreModel;
using UnityEngine;
using UnityEngine.Networking;

namespace AppStoresSupport
{
    public class AppStoreOnboardApi
    {
        public const string oauthClientId = "channel_editor";
        public const string oauthClientSecret = "B63AFB324DE3D12A13827340019D1EE3";
        public const string oauthRedirectUri = "https://id.unity.com";
        public const string url = "https://api.unity.com";
        public const string xiaomiAppType = "xiaomi";
        public const string tokenExpiredInfo = "Expired Access Token";
        public static TokenInfo tokenInfo = new TokenInfo();
        public static string userId;
        public static string orgId;
        public static string updateRev;
        public static bool loaded;

        public static UnityWebRequest asyncRequest(string method, string url, string api, string token,
            object postObject)
        {
            var request = new UnityWebRequest(url + api, method);

            if (postObject != null)
            {
                var postData = JsonUtility.ToJson(postObject);
                var postDataBytes = Encoding.UTF8.GetBytes(postData);
                request.uploadHandler = new UploadHandlerRaw(postDataBytes);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            // set content-type header
            request.SetRequestHeader("Content-Type", "application/json");
            // set auth header
            if (token != null) request.SetRequestHeader("Authorization", "Bearer " + token);
#if UNITY_2017_2_OR_NEWER
            request.SendWebRequest();
#else
			request.Send ();
#endif
            return request;
        }

        public static UnityWebRequest RefreshToken()
        {
            var req = new TokenRequest();
            req.client_id = oauthClientId;
            req.client_secret = oauthClientSecret;
            req.grant_type = "refresh_token";
            req.refresh_token = tokenInfo.refresh_token;
            return asyncRequest(UnityWebRequest.kHttpVerbPOST, url, "/v1/oauth2/token", null, req);
        }

        public static UnityWebRequest GetAccessToken(string authCode)
        {
            var req = new TokenRequest();
            req.code = authCode;
            req.client_id = oauthClientId;
            req.client_secret = oauthClientSecret;
            req.grant_type = "authorization_code";
            req.redirect_uri = oauthRedirectUri;
            return asyncRequest(UnityWebRequest.kHttpVerbPOST, url, "/v1/oauth2/token", null, req);
        }

        public static UnityWebRequest GetUserId()
        {
            var token = tokenInfo.access_token;
            var api = "/v1/oauth2/tokeninfo?access_token=" + token;
            return asyncRequest(UnityWebRequest.kHttpVerbGET, url, api, token, null);
        }

        public static UnityWebRequest GetOrgId(string projectGuid)
        {
            var api = "/v1/core/api/projects/" + projectGuid;
            var token = tokenInfo.access_token;
            return asyncRequest(UnityWebRequest.kHttpVerbGET, url, api, token, null);
        }

        public static UnityWebRequest GetOrgRoles()
        {
            var api = "/v1/organizations/" + orgId + "/roles?userId=" + userId;
            var token = tokenInfo.access_token;
            return asyncRequest(UnityWebRequest.kHttpVerbGET, url, api, token, null);
        }

        public static UnityWebRequest GetUnityClientInfo(string projectGuid)
        {
            var api = "/v1/oauth2/user-clients?projectGuid=" + projectGuid;
            var token = tokenInfo.access_token;
            return asyncRequest(UnityWebRequest.kHttpVerbGET, url, api, token, null);
        }

        public static UnityWebRequest GenerateUnityClient(string projectGuid, UnityClientInfo unityClientInfo,
            XiaomiSettings xiaomi, string callbackUrl)
        {
            return generateOrUpdateUnityClient(projectGuid, UnityWebRequest.kHttpVerbPOST, unityClientInfo, xiaomi,
                callbackUrl);
        }

        public static UnityWebRequest UpdateUnityClient(string projectGuid, UnityClientInfo unityClientInfo,
            XiaomiSettings xiaomi, string callbackUrl)
        {
            return generateOrUpdateUnityClient(projectGuid, UnityWebRequest.kHttpVerbPUT, unityClientInfo, xiaomi,
                callbackUrl);
        }

        private static UnityWebRequest generateOrUpdateUnityClient(string projectGuid, string method,
            UnityClientInfo unityClientInfo, XiaomiSettings xiaomi, string callbackUrl)
        {
            // TODO read xiaomi info from user input
            var channel = new UnityChannel();
            channel.xiaomi = xiaomi;
            channel.projectGuid = projectGuid;
            channel.callbackUrl = callbackUrl;

            // set necessary client post data
            var client = new UnityClient();
            client.client_name = projectGuid;
            client.scopes.Add("identity");
            client.channel = channel;

            string api = null;
            if (method.Equals(UnityWebRequest.kHttpVerbPOST, StringComparison.InvariantCultureIgnoreCase))
            {
                api = "/v1/oauth2/user-clients";
            }
            else if (method.Equals(UnityWebRequest.kHttpVerbPUT, StringComparison.InvariantCultureIgnoreCase))
            {
                // if client is not generated or loaded, directly ignore update
                if (unityClientInfo.ClientId == null)
                {
                    Debug.LogError("Please get/generate Unity Client first.");
                    loaded = false;
                    return null;
                }

                if (updateRev == null)
                {
                    Debug.LogError("Please get/generate Unity Client first.");
                    loaded = false;
                    return null;
                }

                client.rev = updateRev;
                if (orgId == null)
                {
                    Debug.LogError("Please get/generate Unity Client first.");
                    loaded = false;
                    return null;
                }

                client.owner = orgId;
                client.ownerType = "ORGANIZATION";
                api = "/v1/oauth2/user-clients/" + unityClientInfo.ClientId;
            }
            else
            {
                return null;
            }

            var token = tokenInfo.access_token;
            return asyncRequest(method, url, api, token, client);
        }

        public static UnityWebRequest UpdateUnityClientSecret(string clientId)
        {
            if (clientId == null) return null;
            var token = tokenInfo.access_token;
            return asyncRequest(UnityWebRequest.kHttpVerbPUT, url,
                "/v1/oauth2/user-clients/channel-secret?clientId=" + clientId, token, null);
        }
    }
}
#endif