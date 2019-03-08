#if UNITY_5_6_OR_NEWER && !UNITY_5_6_0
using System;
using System.Collections.Generic;
using AppStoreModel;
using UnityEditor;
using UnityEditor.Connect;
using UnityEngine;
using UnityEngine.Networking;

namespace AppStoresSupport
{
    [CustomEditor(typeof(AppStoreSettings))]
    public class AppStoreSettingsEditor : Editor
    {
        private const string STEP_GET_CLIENT = "get_client";
        private const string STEP_UPDATE_CLIENT = "update_client";
        private const string STEP_UPDATE_CLIENT_SECRET = "update_client_secret";
        private string appId_last;
        private string appKey_last;
        private bool appSecret_hidden = true;
        private string appSecret_in_memory;
        private string appSecret_last;
        private string callbackUrl_in_memory;

        private string callbackUrl_last;

        private string clientSecret_in_memory;

        private bool isOperationRunning;
        private bool ownerAuthed;

        private readonly Queue<ReqStruct> requestQueue = new Queue<ReqStruct>();

        private SerializedProperty unityClientID;
        private UnityClientInfo unityClientInfo;
        private SerializedProperty unityClientKey;
        private SerializedProperty unityClientRSAPublicKey;
        private XiaomiSettings xiaomi;

        private SerializedProperty xiaomiAppID;
        private SerializedProperty xiaomiAppKey;
        private SerializedProperty xiaomiIsTestMode;

        private void OnEnable()
        {
            // For unity client settings.
            unityClientID = serializedObject.FindProperty("UnityClientID");
            unityClientKey = serializedObject.FindProperty("UnityClientKey");
            unityClientRSAPublicKey = serializedObject.FindProperty("UnityClientRSAPublicKey");

            // For Xiaomi settings.
            var xiaomiAppStoreSetting = serializedObject.FindProperty("XiaomiAppStoreSetting");
            xiaomiAppID = xiaomiAppStoreSetting.FindPropertyRelative("AppID");
            xiaomiAppKey = xiaomiAppStoreSetting.FindPropertyRelative("AppKey");
            xiaomiIsTestMode = xiaomiAppStoreSetting.FindPropertyRelative("IsTestMode");

            EditorApplication.update += CheckUpdate;
            InitializeSecrets();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(isOperationRunning);

            // Unity project id.
            EditorGUILayout.LabelField(new GUIContent("Unity Project ID"));
            GUILayout.BeginVertical();
            GUILayout.Space(2);
            GUILayout.EndVertical();

            using (new EditorGUILayout.VerticalScope("OL Box",
                GUILayout.Height(AppStoreStyles.kUnityProjectIDBoxHeight)))
            {
                GUILayout.FlexibleSpace();

                var unityProjectID = Application.cloudProjectId;
                if (string.IsNullOrEmpty(unityProjectID))
                {
                    EditorGUILayout.LabelField(new GUIContent(AppStoreStyles.kNoUnityProjectIDErrorMessage));
                    GUILayout.FlexibleSpace();
                    return;
                }

                EditorGUILayout.LabelField(new GUIContent(Application.cloudProjectId));
                GUILayout.FlexibleSpace();
            }

            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.EndVertical();

            // Unity client settings.
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Unity Client Settings"));
            var clientNotExists = string.IsNullOrEmpty(unityClientID.stringValue);
            var buttonLableString = "Generate Unity Client";
            var target = STEP_GET_CLIENT;
            if (!clientNotExists)
            {
                if (string.IsNullOrEmpty(clientSecret_in_memory) || !AppStoreOnboardApi.loaded)
                {
                    buttonLableString = "Load Unity Client";
                }
                else
                {
                    buttonLableString = "Update Client Secret";
                    target = STEP_UPDATE_CLIENT_SECRET;
                }
            }

            if (GUILayout.Button(buttonLableString, GUILayout.Width(AppStoreStyles.kUnityClientIDButtonWidth)))
            {
                isOperationRunning = true;
                Debug.Log(buttonLableString + "...");
                if (target == STEP_UPDATE_CLIENT_SECRET) clientSecret_in_memory = null;
                callApiAsync(target);

                serializedObject.ApplyModifiedProperties();
                Repaint();
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Space(2);
            GUILayout.EndVertical();

            using (new EditorGUILayout.VerticalScope("OL Box", GUILayout.Height(AppStoreStyles.kUnityClientBoxHeight)))
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                if (string.IsNullOrEmpty(unityClientID.stringValue))
                {
                    EditorGUILayout.LabelField("Client ID", GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                    EditorGUILayout.LabelField("None");
                }
                else
                {
                    EditorGUILayout.LabelField("Client ID", GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                    EditorGUILayout.LabelField(strPrefix(unityClientID.stringValue),
                        GUILayout.Width(AppStoreStyles.kClientLabelWidthShort));
                    if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(AppStoreStyles.kClientLabelHeight)))
                    {
                        var te = new TextEditor();
                        te.text = unityClientID.stringValue;
                        te.SelectAll();
                        te.Copy();
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                if (string.IsNullOrEmpty(unityClientKey.stringValue))
                {
                    EditorGUILayout.LabelField("Client Key", GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                    EditorGUILayout.LabelField("None");
                }
                else
                {
                    EditorGUILayout.LabelField("Client Key", GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                    EditorGUILayout.LabelField(strPrefix(unityClientKey.stringValue),
                        GUILayout.Width(AppStoreStyles.kClientLabelWidthShort));
                    if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(AppStoreStyles.kClientLabelHeight)))
                    {
                        var te = new TextEditor();
                        te.text = unityClientKey.stringValue;
                        te.SelectAll();
                        te.Copy();
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                if (string.IsNullOrEmpty(unityClientRSAPublicKey.stringValue))
                {
                    EditorGUILayout.LabelField("Client RSA Public Key",
                        GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                    EditorGUILayout.LabelField("None");
                }
                else
                {
                    EditorGUILayout.LabelField("Client RSA Public Key",
                        GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                    EditorGUILayout.LabelField(strPrefix(unityClientRSAPublicKey.stringValue),
                        GUILayout.Width(AppStoreStyles.kClientLabelWidthShort));
                    if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(AppStoreStyles.kClientLabelHeight)))
                    {
                        var te = new TextEditor();
                        te.text = unityClientRSAPublicKey.stringValue;
                        te.SelectAll();
                        te.Copy();
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginHorizontal();
                if (string.IsNullOrEmpty(clientSecret_in_memory))
                {
                    EditorGUILayout.LabelField("Client Secret", GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                    EditorGUILayout.LabelField("None");
                }
                else
                {
                    EditorGUILayout.LabelField("Client Secret", GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                    EditorGUILayout.LabelField(strPrefix(clientSecret_in_memory),
                        GUILayout.Width(AppStoreStyles.kClientLabelWidthShort));
                    if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(AppStoreStyles.kClientLabelHeight)))
                    {
                        var te = new TextEditor();
                        te.text = clientSecret_in_memory;
                        te.SelectAll();
                        te.Copy();
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Callback URL", GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                callbackUrl_in_memory =
                    EditorGUILayout.TextField(string.IsNullOrEmpty(callbackUrl_in_memory) ? "" : callbackUrl_in_memory);
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }

            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.EndVertical();

            // Xiaomi application settings.
            EditorGUILayout.LabelField(new GUIContent("Xiaomi App Settings"));

            GUILayout.BeginVertical();
            GUILayout.Space(2);
            GUILayout.EndVertical();

            using (new EditorGUILayout.VerticalScope("OL Box", GUILayout.Height(AppStoreStyles.kXiaomiBoxHeight)))
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.Label("App ID", GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                EditorGUILayout.PropertyField(xiaomiAppID, GUIContent.none);
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.Label("App Key", GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                EditorGUILayout.PropertyField(xiaomiAppKey, GUIContent.none);
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.Label("App Secret", GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                if (appSecret_hidden)
                    GUILayout.Label("App Secret is Hidden");
                else
                    appSecret_in_memory =
                        EditorGUILayout.TextField(string.IsNullOrEmpty(appSecret_in_memory) ? "" : appSecret_in_memory);
                var hiddenButtonLabel = appSecret_hidden ? "Show" : "Hide";
                if (GUILayout.Button(hiddenButtonLabel, GUILayout.Width(AppStoreStyles.kClientLabelWidthShort),
                    GUILayout.Height(AppStoreStyles.kClientLabelHeightShort))) appSecret_hidden = !appSecret_hidden;
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Test Mode", GUILayout.Width(AppStoreStyles.kClientLabelWidth));
                EditorGUILayout.PropertyField(xiaomiIsTestMode, GUIContent.none);
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }

            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.EndVertical();

            // Save the settings.
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save All Settings", GUILayout.Width(AppStoreStyles.kSaveButtonWidth)))
            {
                isOperationRunning = true;
                Debug.Log("Saving...");
                if (clientNotExists)
                {
                    Debug.LogError("Please get/generate Unity Client first.");
                }
                else
                {
                    if (callbackUrl_last != callbackUrl_in_memory ||
                        appId_last != xiaomiAppID.stringValue ||
                        appKey_last != xiaomiAppKey.stringValue ||
                        appSecret_last != appSecret_in_memory)
                    {
                        callApiAsync(STEP_UPDATE_CLIENT);
                    }
                    else
                    {
                        isOperationRunning = false;
                        Debug.Log("Unity Client Refreshed. Finished: " + STEP_UPDATE_CLIENT);
                    }
                }

                serializedObject.ApplyModifiedProperties();
                Repaint();
                AssetDatabase.SaveAssets();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
            Repaint();

            EditorGUI.EndDisabledGroup();
        }

        private string strPrefix(string str)
        {
            var preIndex = str.Length < 5 ? str.Length : 5;
            return str.Substring(0, preIndex) + "...";
        }

        private void callApiAsync(string targetStep)
        {
            if (targetStep == STEP_GET_CLIENT) AppStoreOnboardApi.tokenInfo.access_token = null;
            if (AppStoreOnboardApi.tokenInfo.access_token == null)
            {
                UnityOAuth.GetAuthorizationCodeAsync(AppStoreOnboardApi.oauthClientId, response =>
                {
                    if (response.AuthCode != null)
                    {
                        var authcode = response.AuthCode;
                        var request = AppStoreOnboardApi.GetAccessToken(authcode);
                        var tokenInfoResp = new TokenInfo();
                        var reqStruct = new ReqStruct();
                        reqStruct.request = request;
                        reqStruct.resp = tokenInfoResp;
                        reqStruct.targetStep = targetStep;
                        requestQueue.Enqueue(reqStruct);
                    }
                    else
                    {
                        Debug.Log("Failed: " + response.Exception.ToString());
                        isOperationRunning = false;
                    }
                });
            }
            else
            {
                var request = AppStoreOnboardApi.GetUserId();
                var userIdResp = new UserIdResponse();
                var reqStruct = new ReqStruct();
                reqStruct.request = request;
                reqStruct.resp = userIdResp;
                reqStruct.targetStep = targetStep;
                requestQueue.Enqueue(reqStruct);
            }
        }

        private void OnDestroy()
        {
            EditorApplication.update -= CheckUpdate;
        }

        private void CheckUpdate()
        {
            CheckRequestUpdate();
        }

        private void InitializeSecrets()
        {
            // No need to initialize for invalid client settings.
            if (string.IsNullOrEmpty(unityClientID.stringValue)) return;

            if (!string.IsNullOrEmpty(clientSecret_in_memory)) return;

            // Start secret initialization. 
            isOperationRunning = true;
            Debug.Log("Loading existed client info...");
            callApiAsync(STEP_GET_CLIENT);
        }

        private void CheckRequestUpdate()
        {
            if (requestQueue.Count <= 0) return;

            var reqStruct = requestQueue.Dequeue();
            var request = reqStruct.request;
            var resp = reqStruct.resp;

            if (request != null && request.isDone)
            {
                if (request.error != null)
                {
                    if (request.responseCode == 404)
                    {
                        Debug.LogError("Resouce not found.");
                        isOperationRunning = false;
                    }
                    else if (request.responseCode == 403)
                    {
                        Debug.LogError("Permision denied.");
                        isOperationRunning = false;
                    }
                    else
                    {
                        Debug.LogError(request.error);
                        isOperationRunning = false;
                    }
                }
                else
                {
                    if (request.downloadHandler.text.Contains(AppStoreOnboardApi.tokenExpiredInfo))
                    {
                        var newRequest = AppStoreOnboardApi.RefreshToken();
                        var tokenInfoResp = new TokenInfo();
                        var newReqStruct = new ReqStruct();
                        newReqStruct.request = newRequest;
                        newReqStruct.resp = tokenInfoResp;
                        newReqStruct.targetStep = reqStruct.targetStep;
                        requestQueue.Enqueue(newReqStruct);
                    }
                    else
                    {
                        if (resp.GetType() == typeof(TokenInfo))
                        {
                            resp = JsonUtility.FromJson<TokenInfo>(request.downloadHandler.text);
                            AppStoreOnboardApi.tokenInfo.access_token = ((TokenInfo) resp).access_token;
                            if (AppStoreOnboardApi.tokenInfo.refresh_token == null ||
                                AppStoreOnboardApi.tokenInfo.refresh_token == "")
                                AppStoreOnboardApi.tokenInfo.refresh_token = ((TokenInfo) resp).refresh_token;
                            var newRequest = AppStoreOnboardApi.GetUserId();
                            var userIdResp = new UserIdResponse();
                            var newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = userIdResp;
                            newReqStruct.targetStep = reqStruct.targetStep;
                            requestQueue.Enqueue(newReqStruct);
                        }
                        else if (resp.GetType() == typeof(UserIdResponse))
                        {
                            resp = JsonUtility.FromJson<UserIdResponse>(request.downloadHandler.text);
                            AppStoreOnboardApi.userId = ((UserIdResponse) resp).sub;
                            var newRequest = AppStoreOnboardApi.GetOrgId(Application.cloudProjectId);
                            var orgIdResp = new OrgIdResponse();
                            var newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = orgIdResp;
                            newReqStruct.targetStep = reqStruct.targetStep;
                            requestQueue.Enqueue(newReqStruct);
                        }
                        else if (resp.GetType() == typeof(OrgIdResponse))
                        {
                            resp = JsonUtility.FromJson<OrgIdResponse>(request.downloadHandler.text);
                            AppStoreOnboardApi.orgId = ((OrgIdResponse) resp).org_foreign_key;
                            var newRequest = AppStoreOnboardApi.GetOrgRoles();
                            var orgRoleResp = new OrgRoleResponse();
                            var newReqStruct = new ReqStruct();
                            newReqStruct.request = newRequest;
                            newReqStruct.resp = orgRoleResp;
                            newReqStruct.targetStep = reqStruct.targetStep;
                            requestQueue.Enqueue(newReqStruct);
                        }
                        else if (resp.GetType() == typeof(OrgRoleResponse))
                        {
                            resp = JsonUtility.FromJson<OrgRoleResponse>(request.downloadHandler.text);
                            if (resp == null)
                            {
                                Debug.LogError("Permision denied.");
                                isOperationRunning = false;
                            }

                            var roles = ((OrgRoleResponse) resp).roles;
                            if (roles.Contains("owner"))
                            {
                                ownerAuthed = true;
                                if (reqStruct.targetStep == STEP_GET_CLIENT)
                                {
                                    var newRequest = AppStoreOnboardApi.GetUnityClientInfo(Application.cloudProjectId);
                                    var clientRespWrapper = new UnityClientResponseWrapper();
                                    var newReqStruct = new ReqStruct();
                                    newReqStruct.request = newRequest;
                                    newReqStruct.resp = clientRespWrapper;
                                    newReqStruct.targetStep = reqStruct.targetStep;
                                    requestQueue.Enqueue(newReqStruct);
                                }
                                else if (reqStruct.targetStep == STEP_UPDATE_CLIENT)
                                {
                                    var unityClientInfo = new UnityClientInfo();
                                    unityClientInfo.ClientId = unityClientID.stringValue;
                                    var callbackUrl = callbackUrl_in_memory;
                                    // read xiaomi from user input
                                    var xiaomi = new XiaomiSettings();
                                    xiaomi.appId = xiaomiAppID.stringValue;
                                    xiaomi.appKey = xiaomiAppKey.stringValue;
                                    xiaomi.appSecret = appSecret_in_memory;
                                    var newRequest = AppStoreOnboardApi.UpdateUnityClient(Application.cloudProjectId,
                                        unityClientInfo, xiaomi, callbackUrl);
                                    var clientResp = new UnityClientResponse();
                                    var newReqStruct = new ReqStruct();
                                    newReqStruct.request = newRequest;
                                    newReqStruct.resp = clientResp;
                                    newReqStruct.targetStep = reqStruct.targetStep;
                                    requestQueue.Enqueue(newReqStruct);
                                }
                                else if (reqStruct.targetStep == STEP_UPDATE_CLIENT_SECRET)
                                {
                                    var clientId = unityClientID.stringValue;
                                    var newRequest = AppStoreOnboardApi.UpdateUnityClientSecret(clientId);
                                    var clientResp = new UnityClientResponse();
                                    var newReqStruct = new ReqStruct();
                                    newReqStruct.request = newRequest;
                                    newReqStruct.resp = clientResp;
                                    newReqStruct.targetStep = reqStruct.targetStep;
                                    requestQueue.Enqueue(newReqStruct);
                                }
                            }
                            else if (roles.Contains("user") || roles.Contains("manager"))
                            {
                                ownerAuthed = false;
                                if (reqStruct.targetStep == STEP_GET_CLIENT)
                                {
                                    var newRequest = AppStoreOnboardApi.GetUnityClientInfo(Application.cloudProjectId);
                                    var clientRespWrapper = new UnityClientResponseWrapper();
                                    var newReqStruct = new ReqStruct();
                                    newReqStruct.request = newRequest;
                                    newReqStruct.resp = clientRespWrapper;
                                    newReqStruct.targetStep = reqStruct.targetStep;
                                    requestQueue.Enqueue(newReqStruct);
                                }
                                else
                                {
                                    Debug.LogError("Permision denied.");
                                    isOperationRunning = false;
                                }
                            }
                            else
                            {
                                Debug.LogError("Permision denied.");
                                isOperationRunning = false;
                            }
                        }
                        else if (resp.GetType() == typeof(UnityClientResponseWrapper))
                        {
                            var raw = "{ \"array\": " + request.downloadHandler.text + "}";
                            resp = JsonUtility.FromJson<UnityClientResponseWrapper>(raw);
                            // only one element in the list
                            if (((UnityClientResponseWrapper) resp).array.Length > 0)
                            {
                                var unityClientResp = ((UnityClientResponseWrapper) resp).array[0];
                                unityClientID.stringValue = unityClientResp.client_id;
                                unityClientKey.stringValue = unityClientResp.client_secret;
                                unityClientRSAPublicKey.stringValue = unityClientResp.channel.publicRSAKey;
                                clientSecret_in_memory = unityClientResp.channel.channelSecret;
                                callbackUrl_in_memory = unityClientResp.channel.callbackUrl;
                                callbackUrl_last = callbackUrl_in_memory;
                                foreach (var thirdPartySetting in unityClientResp.channel.thirdPartySettings)
                                    if (thirdPartySetting.appType.Equals(AppStoreOnboardApi.xiaomiAppType,
                                        StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        xiaomiAppID.stringValue = thirdPartySetting.appId;
                                        xiaomiAppKey.stringValue = thirdPartySetting.appKey;
                                        appSecret_in_memory = thirdPartySetting.appSecret;
                                        appId_last = xiaomiAppID.stringValue;
                                        appKey_last = xiaomiAppKey.stringValue;
                                        appSecret_last = appSecret_in_memory;
                                    }

                                AppStoreOnboardApi.updateRev = unityClientResp.rev;
                                Debug.Log("Unity Client Refreshed. Finished: " + reqStruct.targetStep);
                                AppStoreOnboardApi.loaded = true;
                                isOperationRunning = false;
                                serializedObject.ApplyModifiedProperties();
                                Repaint();
                                AssetDatabase.SaveAssets();
                            }
                            else
                            {
                                // no client found, generate one.
                                if (ownerAuthed)
                                {
                                    var unityClientInfo = new UnityClientInfo();
                                    var callbackUrl = callbackUrl_in_memory;
                                    // read xiaomi from user input
                                    var xiaomi = new XiaomiSettings();
                                    xiaomi.appId = xiaomiAppID.stringValue;
                                    xiaomi.appKey = xiaomiAppKey.stringValue;
                                    xiaomi.appSecret = appSecret_in_memory;
                                    var newRequest = AppStoreOnboardApi.GenerateUnityClient(Application.cloudProjectId,
                                        unityClientInfo, xiaomi, callbackUrl);
                                    var clientResp = new UnityClientResponse();
                                    var newReqStruct = new ReqStruct();
                                    newReqStruct.request = newRequest;
                                    newReqStruct.resp = clientResp;
                                    newReqStruct.targetStep = reqStruct.targetStep;
                                    requestQueue.Enqueue(newReqStruct);
                                }
                                else
                                {
                                    Debug.LogError("Permision denied.");
                                    isOperationRunning = false;
                                }
                            }
                        }
                        else if (resp.GetType() == typeof(UnityClientResponse))
                        {
                            resp = JsonUtility.FromJson<UnityClientResponse>(request.downloadHandler.text);
                            unityClientID.stringValue = ((UnityClientResponse) resp).client_id;
                            unityClientKey.stringValue = ((UnityClientResponse) resp).client_secret;
                            unityClientRSAPublicKey.stringValue = ((UnityClientResponse) resp).channel.publicRSAKey;
                            clientSecret_in_memory = ((UnityClientResponse) resp).channel.channelSecret;
                            callbackUrl_in_memory = ((UnityClientResponse) resp).channel.callbackUrl;
                            callbackUrl_last = callbackUrl_in_memory;
                            foreach (var thirdPartySetting in ((UnityClientResponse) resp).channel.thirdPartySettings)
                                if (thirdPartySetting.appType.Equals(AppStoreOnboardApi.xiaomiAppType,
                                    StringComparison.InvariantCultureIgnoreCase))
                                {
                                    xiaomiAppID.stringValue = thirdPartySetting.appId;
                                    xiaomiAppKey.stringValue = thirdPartySetting.appKey;
                                    appSecret_in_memory = thirdPartySetting.appSecret;
                                    appId_last = xiaomiAppID.stringValue;
                                    appKey_last = xiaomiAppKey.stringValue;
                                    appSecret_last = appSecret_in_memory;
                                }

                            AppStoreOnboardApi.updateRev = ((UnityClientResponse) resp).rev;
                            Debug.Log("Unity Client Refreshed. Finished: " + reqStruct.targetStep);
                            AppStoreOnboardApi.loaded = true;
                            isOperationRunning = false;
                            serializedObject.ApplyModifiedProperties();
                            Repaint();
                            AssetDatabase.SaveAssets();
                        }
                    }
                }
            }
            else
            {
                requestQueue.Enqueue(reqStruct);
            }
        }

        private struct ReqStruct
        {
            public string currentStep;
            public string targetStep;
            public UnityWebRequest request;
            public GeneralResponse resp;
        }

        private class AppStoreStyles
        {
            public const string kNoUnityProjectIDErrorMessage =
                "Unity Project ID doesn't exist, please go to Window/Services to create one.";

            public const int kUnityProjectIDBoxHeight = 24;
            public const int kUnityClientBoxHeight = 110;
            public const int kXiaomiBoxHeight = 90;

            public const int kUnityClientIDButtonWidth = 160;
            public const int kSaveButtonWidth = 120;

            public const int kClientLabelWidth = 140;
            public const int kClientLabelHeight = 16;
            public const int kClientLabelWidthShort = 50;
            public const int kClientLabelHeightShort = 15;
        }
    }
}
#endif