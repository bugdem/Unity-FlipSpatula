using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ElephantSdkManager.Model;
using ElephantSdkManager.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace ElephantSdkManager
{
    public class ElephantSdkManager : EditorWindow
    {
        private const string AssetsPathPrefix = "Assets/";
        private const string DownloadDirectory = AssetsPathPrefix + "ElephantSdkManager";

        private List<Sdk> _sdkList;

        private EditorCoroutines.EditorCoroutine _editorCoroutine;
        private EditorCoroutines.EditorCoroutine _editorCoroutineSelfUpdate;
        private UnityWebRequest _downloader;
        private string _activity;

        private string _selfUpdateStatus;
        private bool _canUpdateSelf = false;
        private Sdk selfUpdateSdk;
        public string requiredSdks = "";

        private GUIStyle _labelStyle;
        private GUIStyle _headerStyle;
        private readonly GUILayoutOption _fieldWidth = GUILayout.Width(60);

        private Vector2 _scrollPos;

        [MenuItem("Elephant/Manage Core SDK")]
        public static void ShowSdkManager()
        {
            var win = GetWindow<ElephantSdkManager>("Manage SDKs");
            win.titleContent = new GUIContent("Elephant Packages");
            win.Focus();
            win.CancelOperation();
            win.OnEnable();
        }

        public void Awake()
        {
            _labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            _headerStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 18
            };

            CancelOperation();
        }

        public void OnEnable()
        {
            _editorCoroutine = this.StartCoroutine(FetchManifest());
            _editorCoroutineSelfUpdate = this.StartCoroutine(CheckSelfUpdate());
        }

        void OnDisable()
        {
            CancelOperation();
        }

        public void OnGUI()
        {
            var stillWorking = _editorCoroutine != null || _downloader != null;

            if (_sdkList != null && _sdkList.Count > 0)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                using (var s = new EditorGUILayout.ScrollViewScope(_scrollPos, false, false))
                {
                    _scrollPos = s.scrollPosition;

                    var groupedSdkList = _sdkList
                        .GroupBy(item => item.type)
                        .Select(group => group.ToList())
                        .ToList();

                   foreach (var groupSdk in groupedSdkList)
                   {
                       switch (groupSdk[0].type)
                       {
                           case "internal":
                               PopulateGroupSdks(groupSdk, "Elephant SDKs");
                               break;
                           case "mopub":
                               PopulateGroupSdks(groupSdk, "MoPub");
                               break;
                           case "network":
                               PopulateGroupSdks(groupSdk, "Networks");
                               break;
                           default:
                               PopulateGroupSdks(groupSdk, "Elephant SDKs");
                               break;
                       }
                   }
                }
            }

            // Indicate async operation in progress.
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
            {
                EditorGUILayout.LabelField(stillWorking ? _activity : " ");
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(10);
                if (!stillWorking)
                {
                    if (GUILayout.Button("Done", _fieldWidth))
                        Close();
                }
                else
                {
                    if (GUILayout.Button("Cancel", _fieldWidth))
                        CancelOperation();
                }
            }
            GUILayout.Space(10);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(_selfUpdateStatus);

                if (selfUpdateSdk != null)
                {
                    EditorGUILayout.LabelField(selfUpdateSdk.version ?? "--");
                    GUI.enabled = _canUpdateSelf;
                    if (GUILayout.Button(new GUIContent
                    {
                        text = "Update",
                    }, _fieldWidth))
                        this.StartCoroutine(DownloadSdkManager(selfUpdateSdk));
                    GUI.enabled = true;
                }
            }
            
            GUILayout.Space(10);
        }

        private void PopulateGroupSdks(List<Sdk> groupedSdkList, string groupTitle)
        {
            GUILayout.Space(5);
            EditorGUILayout.LabelField(groupTitle, _labelStyle, GUILayout.Height(20));
            
            if (groupTitle.Equals("Elephant SDKs"))
            {
                groupedSdkList = groupedSdkList.OrderBy(sdk => sdk.sdkName).ToList();
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                SdkHeaders();
                foreach (var sdk in groupedSdkList)
                {
                    SdkRow(sdk);
                }
            }
        }

        private void SdkHeaders()
        {
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Package", _headerStyle);
                GUILayout.Button("Current Version", _headerStyle);
                GUILayout.Space(3);
                GUILayout.Button("Latest Version", _headerStyle);
                GUILayout.Space(3);
                GUILayout.Button("Action", _headerStyle, _fieldWidth);
                GUILayout.Button(" ", _headerStyle, GUILayout.Width(1));
                GUILayout.Space(5);
            }

            GUILayout.Space(4);
        }

        private void SdkRow(Sdk sdkInfo, Func<bool, bool> customButton = null)
        {
            var sdkName = sdkInfo.sdkName;
            var latestVersion = sdkInfo.version.Replace("v", string.Empty);
            var cur = sdkInfo.currentVersion;
            cur = !string.IsNullOrEmpty(cur) ? sdkInfo.currentVersion.Replace("v", string.Empty) : "";
            var isInst = !string.IsNullOrEmpty(cur);
            var canInst = !string.IsNullOrEmpty(latestVersion) &&
                          (!isInst || VersionUtils.CompareVersions(cur, latestVersion) < 0);

            var stillWorking = _editorCoroutine != null || _downloader != null;

            GUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(false)))
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField(new GUIContent {text = sdkName});
                GUILayout.Button(new GUIContent
                {
                    text = string.IsNullOrEmpty(cur) ? "--" : cur,
                }, canInst ? EditorStyles.boldLabel : EditorStyles.label);
                GUILayout.Space(3);
                GUILayout.Button(new GUIContent
                {
                    text = latestVersion ?? "--",
                }, canInst ? EditorStyles.boldLabel : EditorStyles.label);
                GUILayout.Space(3);
                if (customButton == null || !customButton(canInst))
                {
                    GUI.enabled = !stillWorking && (canInst) && !VersionUtils.IsEqualVersion(cur, latestVersion);
                    if (GUILayout.Button(new GUIContent
                    {
                        text = isInst ? "Upgrade" : "Install",
                    }, _fieldWidth))
                    {
                        if (!IsInstallAvailable(sdkInfo))
                        {
                            EditorUtility.DisplayDialog("Warning!", "Please install or upgrade required SDKs first:\n" + requiredSdks, "OK");
                            return;
                        }
                        this.StartCoroutine(DownloadSDK(sdkInfo));
                    }
                    GUI.enabled = true;
                }
                
                if (!canInst && sdkName.Equals("Rollic Ads"))
                {
                    var isChinaLoaded = EditorPrefs.GetBool(RegionSwitch.KeyIsRollicAdsChinaInstalled, false);
                    if (GUILayout.Button(new GUIContent
                    {
                        text = isChinaLoaded ? "WW" : "China",
                    }, _fieldWidth))
                    {
                        RegionSwitch.SwitchRegions(!isChinaLoaded);
                    }
                }

                GUILayout.Space(5);
            }

            GUILayout.Space(4);
        }
        
        private IEnumerator CheckSelfUpdate()
        {
            yield return null;
            _selfUpdateStatus = "Checking for new versions of SDK Manager";

            var unityWebRequest = new UnityWebRequest(ManifestSource.SelfUpdateURL)
            {
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = 10,
            };

            if (!string.IsNullOrEmpty(unityWebRequest.error))
            {
                Debug.LogError(unityWebRequest.error);
            }

            yield return unityWebRequest.SendWebRequest();

            var responseJson = unityWebRequest.downloadHandler.text;

            if (string.IsNullOrEmpty(responseJson))
            {
                Debug.LogError("Unable to retrieve SDK Manager manifest");

                yield break;
            }

            unityWebRequest.Dispose();

            selfUpdateSdk = JsonUtility.FromJson<Sdk>(responseJson);
            
            if (selfUpdateSdk == null) yield break;
            
            var currentVersion = ElephantSdkManagerVersion.SDK_VERSION.Replace("v", string.Empty);
            var latestVersion = selfUpdateSdk.version.Replace("v", string.Empty);

            if (VersionUtils.CompareVersions(currentVersion, latestVersion) < 0
                && !VersionUtils.IsEqualVersion(currentVersion, latestVersion))
            {
                _selfUpdateStatus = "New Version Available";
                _canUpdateSelf = true;
            }
            else
            {
                _selfUpdateStatus = "Up To Date";
                _canUpdateSelf = false;
            }
            
        }
        
        private IEnumerator FetchManifest()
        {
            yield return null;
            _activity = "Downloading SDK version manifest...";
            
            string elephantSettingsPage = Application.dataPath + "/Resources/ElephantSettings.asset";
            string gameId = "";
            if (File.Exists(elephantSettingsPage))
            {
                string[] lines = File.ReadAllLines(elephantSettingsPage);
                foreach (var line in lines)
                {
                    if (line.Contains("GameID"))
                    {
                        gameId = line.Replace("  GameID: ", "");
                    }
                }
            }
            
            var unityWebRequest = new UnityWebRequest(ManifestSource.ManifestURL + gameId +  "&version=" + ElephantSdkManagerVersion.SDK_VERSION)
            {
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = 10,
            };

            if (!string.IsNullOrEmpty(unityWebRequest.error))
            {
                Debug.LogError(unityWebRequest.error);
            }

            yield return unityWebRequest.SendWebRequest();

            var responseJson = unityWebRequest.downloadHandler.text;

            if (string.IsNullOrEmpty(responseJson))
            {
                Debug.LogError("Unable to retrieve SDK version manifest.  Showing installed SDKs only.");

                yield break;
            }

            unityWebRequest.Dispose();

            var manifest = JsonUtility.FromJson<Manifest>(responseJson);
            _sdkList = manifest.sdkList;

            CheckVersions();
        }

        private void CheckVersions()
        {
            if (!Directory.Exists(AssetsPathPrefix + "Elephant"))
            {
                Sdk elephantSdk = _sdkList.Find(sdk => sdk.sdkName.Equals("Elephant"));
                elephantSdk.currentVersion = "";
                
                Assembly assemblyForAds = Assembly.GetExecutingAssembly();
                foreach (var type in assemblyForAds.GetTypes())
                {
                    if (type.FullName == null) return;

                    if (type.FullName.Equals("RollicGames.Advertisements.AdsSdkVersion"))
                    {
                        var fieldInfo = type.GetField("SDK_VERSION",
                            BindingFlags.NonPublic | BindingFlags.Static);
                        var adsSDK = _sdkList.Find(sdk => sdk.sdkName.Equals("Rollic Ads"));
                        if (!(fieldInfo is null)) adsSDK.currentVersion = fieldInfo.GetValue(null).ToString();
                    }
                }
                _editorCoroutine = null;
                Repaint();
                return;
            }
            
            Assembly myAssembly = Assembly.GetExecutingAssembly();
            foreach (var type in myAssembly.GetTypes())
            {
                if (type.FullName == null) return;

                if (type.FullName.Equals("ElephantSDK.ElephantVersion"))
                {
                    FieldInfo fieldInfo = type.GetField("SDK_VERSION",
                        BindingFlags.NonPublic | BindingFlags.Static);
                    Sdk elephantSdk = _sdkList.Find(sdk => sdk.sdkName.Equals("Elephant"));
                    if (!(fieldInfo is null)) elephantSdk.currentVersion = fieldInfo.GetValue(null).ToString();
                }

                if (type.FullName.Equals("RollicGames.Advertisements.AdsSdkVersion"))
                {
                    var fieldInfo = type.GetField("SDK_VERSION",
                        BindingFlags.NonPublic | BindingFlags.Static);
                    var adsSDK = _sdkList.Find(sdk => sdk.sdkName.Equals("Rollic Ads"));
                    if (!(fieldInfo is null)) adsSDK.currentVersion = fieldInfo.GetValue(null).ToString();
                }
                
                var mopubSdk = _sdkList.Find(sdk => sdk.sdkName.Equals("MoPub"));
                if (mopubSdk != null)
                {
                    string mopubPath = Application.dataPath + "/Mopub/Scripts/Mopub.cs";
                    if (File.Exists(mopubPath))
                    {
                        string[] lines = File.ReadAllLines(mopubPath);
                        foreach (var line in lines)
                        {
                            if (line.Contains("public const string MoPubSdkVersion"))
                            {
                                Regex regex = new Regex("\"(.*?)\"");
    
                                var matches = regex.Matches(line);
                                
                                if (matches.Count > 0)
                                {
                                    var mopubVersion = matches[0].Value;
                                    mopubVersion = mopubVersion.Replace("\"", "");
                                    mopubSdk.currentVersion = mopubVersion;
                                }
                            }
                        }
                    }
                }
            }
            
            var types = myAssembly.GetTypes();
            var packageConfigTypes = 
                types.Where(packageConfigType => packageConfigType.Name.Contains("PackageConfig")).ToList();
            if (packageConfigTypes.Count > 0)
            {
                foreach (var packageConfigType in packageConfigTypes)
                {
                    var version = "";
                    var name = "";
                    var versionMethodInfo = packageConfigType.GetMethod("get_Version");
                    if (versionMethodInfo != null)
                    {
                        var classInstance = Activator.CreateInstance(packageConfigType, null);
                        version = (string) versionMethodInfo.Invoke(classInstance, null);
                    }
                    var nameMethodInfo = packageConfigType.GetMethod("get_Name");
                    if (nameMethodInfo != null)
                    {
                        var classInstance = Activator.CreateInstance(packageConfigType, null);
                        name = (string) nameMethodInfo.Invoke(classInstance, null);
                    }
                    var networkSdk = _sdkList.Find(sdk => sdk.sdkName.Equals(name));
                    if (networkSdk != null)
                    {
                        networkSdk.currentVersion = version;
                    }
                }
            }

            _editorCoroutine = null;
            Repaint();
        }

        private IEnumerator DownloadSdkManager(Sdk sdkInfo)
        {
            var path = Path.Combine(AssetsPathPrefix, sdkInfo.sdkName + "new");
            _activity = $"Downloading {sdkInfo.sdkName}...";

            // Start the async download job.
            _downloader = new UnityWebRequest(sdkInfo.downloadUrl)
            {
                downloadHandler = new DownloadHandlerFile(path),
                timeout = 60, // seconds
            };
            _downloader.SendWebRequest();
            
            while (!_downloader.isDone)
            {
                yield return null;
                var progress = Mathf.FloorToInt(_downloader.downloadProgress * 100);
                if (EditorUtility.DisplayCancelableProgressBar("Elephant SDK Manager", _activity, progress))
                    _downloader.Abort();
            }
            
            EditorUtility.ClearProgressBar();
            
            if (string.IsNullOrEmpty(_downloader.error))
            {
                if (Directory.Exists(AssetsPathPrefix + sdkInfo.sdkName))
                {
                    FileUtil.DeleteFileOrDirectory(AssetsPathPrefix + sdkInfo.sdkName);
                }

                AssetDatabase.ImportPackage(path, true);
                FileUtil.DeleteFileOrDirectory(path);
            }
            
            _downloader.Dispose();
            _downloader = null;
            _editorCoroutine = null;

            yield return null;
        }

        private IEnumerator DownloadSDK(Sdk sdkInfo)
        {
            var path = Path.Combine(DownloadDirectory, sdkInfo.sdkName);
            _activity = $"Downloading {sdkInfo.sdkName}...";

            // Start the async download job.
            _downloader = new UnityWebRequest(sdkInfo.downloadUrl)
            {
                downloadHandler = new DownloadHandlerFile(path),
                timeout = 60, // seconds
            };
            _downloader.SendWebRequest();

            // Pause until download done/cancelled/fails, keeping progress bar up to date.
            while (!_downloader.isDone)
            {
                yield return null;
                var progress = Mathf.FloorToInt(_downloader.downloadProgress * 100);
                if (EditorUtility.DisplayCancelableProgressBar("Elephant SDK Manager", _activity, progress))
                    _downloader.Abort();
            }

            EditorUtility.ClearProgressBar();

            if (string.IsNullOrEmpty(_downloader.error))
            {
                if (Directory.Exists(AssetsPathPrefix + sdkInfo.sdkName))
                {
                    if (!string.Equals(sdkInfo.sdkName, "MoPub"))
                    {
                        FileUtil.DeleteFileOrDirectory(AssetsPathPrefix + sdkInfo.sdkName);
                    }
                }

                if (sdkInfo.sdkName.Equals("Rollic Ads"))
                {
                    if (Directory.Exists(AssetsPathPrefix + "RollicGames"))
                    {
                        FileUtil.DeleteFileOrDirectory(AssetsPathPrefix + "RollicGames");
                    }
                }
                
                AssetDatabase.ImportPackage(path, true);
                FileUtil.DeleteFileOrDirectory(path);
            }

            _downloader.Dispose();
            _downloader = null;
            _editorCoroutine = null;

            yield return null;
        }

        private bool IsInstallAvailable(Sdk dependentSdk)
        {
            requiredSdks = "";
            if (_sdkList == null || _sdkList.Count == 0)
            {
                return false;
            }

            if (string.IsNullOrEmpty(dependentSdk.depends_on)) return true;

            var requiredSDK = _sdkList.Find(sdk => sdk.sdkName.Equals(dependentSdk.depends_on));

            if (string.IsNullOrEmpty(requiredSDK?.currentVersion))
            {
                requiredSdks = requiredSDK?.sdkName;
                return false;
            }

            if (VersionUtils.CompareVersions(requiredSDK.currentVersion.Replace("v", string.Empty),
                dependentSdk.depending_sdk_version) >= 0)
            {
                return true;
            }

            requiredSdks = requiredSDK.sdkName;
            return false;

        }

        private void CancelOperation()
        {
            // Stop any async action taking place.
            if (_downloader != null)
            {
                _downloader.Abort(); // The coroutine should resume and clean up.
                return;
            }

            if (_editorCoroutine != null)
                this.StopCoroutine(_editorCoroutine.routine);
            
            if (_editorCoroutineSelfUpdate != null)
                this.StopCoroutine(_editorCoroutineSelfUpdate.routine);

            _editorCoroutineSelfUpdate = null;
            _editorCoroutine = null;
            _downloader = null;
        }
    }
}