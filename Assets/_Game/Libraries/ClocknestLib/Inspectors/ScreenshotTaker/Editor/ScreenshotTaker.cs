using UnityEditor;
using UnityEngine;

namespace ClocknestGames.Library.Editor
{
    [ExecuteInEditMode]
    public class ScreenshotTaker : EditorWindow
    {
        public Camera CameraToUse;
        public Camera[] AdditionalCameras;

        protected int _resWidth = Screen.width * 4;
        protected int _resHeight = Screen.height * 4;
        protected int _scale = 1;
        protected string _savePath = "";
        protected bool _showPreview = true;
        protected RenderTexture _renderTexture;
        protected bool _isTransparent = false;
        protected bool _takeHiResShot = false;
        protected string _lastScreenshot = "";

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Clocknest Games/Tools/Screenshot Taker")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow editorWindow = EditorWindow.GetWindow(typeof(ScreenshotTaker));
            editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.Show();
            editorWindow.titleContent.text = "Screenshot Taker";
        }

        // C#
        public static EditorWindow GetMainGameView()
        {
            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            System.Object Res = GetMainGameView.Invoke(null, null);
            return (EditorWindow)Res;
        }


        void OnGUI()
        {
            SerializedObject serializedObj = new SerializedObject(this);
            SerializedProperty serializedAdditionalCameras = serializedObj.FindProperty("AdditionalCameras");

            EditorGUILayout.LabelField("Resolution", EditorStyles.boldLabel);
            _resWidth = EditorGUILayout.IntField("Width", _resWidth);
            _resHeight = EditorGUILayout.IntField("Height", _resHeight);

            EditorGUILayout.Space();

            _scale = EditorGUILayout.IntSlider("Scale", _scale, 1, 15);

            EditorGUILayout.HelpBox("The default mode of screenshot is crop - so choose a proper width and height. The scale is a factor " +
                "to multiply or enlarge the renders without loosing quality.", MessageType.None);

            EditorGUILayout.Space();

            GUILayout.Label("Save Path", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(_savePath, GUILayout.ExpandWidth(false));
            if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
                _savePath = EditorUtility.SaveFolderPanel("Path to Save Images", _savePath, Application.dataPath);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("Choose the folder in which to save the screenshots ", MessageType.None);
            EditorGUILayout.Space();

            GUILayout.Label("Select Camera", EditorStyles.boldLabel);

            CameraToUse = EditorGUILayout.ObjectField(CameraToUse, typeof(Camera), true, null) as Camera;
            if (CameraToUse == null)
            {
                CameraToUse = Camera.main;
            }

            EditorGUILayout.PropertyField(serializedAdditionalCameras, true);

            serializedObj.ApplyModifiedProperties();

            _isTransparent = EditorGUILayout.Toggle("Transparent Background", _isTransparent);

            EditorGUILayout.HelpBox("Choose the camera of which to capture the render. You can make the background transparent using the transparency option.", MessageType.None);

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Default Options", EditorStyles.boldLabel);

            if (GUILayout.Button("Set To Screen Size"))
            {
                _resHeight = (int)Handles.GetMainGameViewSize().y;
                _resWidth = (int)Handles.GetMainGameViewSize().x;
            }

            if (GUILayout.Button("Default Size"))
            {
                _resHeight = 1440;
                _resWidth = 2560;
                _scale = 1;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Screenshot will be taken at " + _resWidth * _scale + " x " + _resHeight * _scale + " px", EditorStyles.boldLabel);

            if (GUILayout.Button("Take Screenshot", GUILayout.MinHeight(60)))
            {
                if (_savePath == "")
                {
                    _savePath = EditorUtility.SaveFolderPanel("Path to Save Images", _savePath, Application.dataPath);
                    TakeHiResShot();
                }
                else
                {
                    TakeHiResShot();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Open Last Screenshot", GUILayout.MinHeight(40)))
            {
                if (_lastScreenshot != "")
                {
                    Application.OpenURL("file://" + _lastScreenshot);
                }
            }

            if (GUILayout.Button("Open Folder", GUILayout.MinHeight(40)))
            {
                Application.OpenURL("file://" + _savePath);
            }

            EditorGUILayout.EndHorizontal();

            if (_takeHiResShot)
            {
                int resWidthN = _resWidth * _scale;
                int resHeightN = _resHeight * _scale;
                RenderTexture rt = new RenderTexture(resWidthN, resHeightN, 24);
                CameraToUse.targetTexture = rt;
                if (AdditionalCameras != null)
                {
                    foreach (var camera in AdditionalCameras)
                    {
                        if (camera != null)
                            camera.targetTexture = rt;
                    }
                }

                TextureFormat tFormat;
                if (_isTransparent)
                    tFormat = TextureFormat.ARGB32;
                else
                    tFormat = TextureFormat.RGB24;

                Texture2D screenShot = new Texture2D(resWidthN, resHeightN, tFormat, false);
                CameraToUse.Render();
                if (AdditionalCameras != null)
                {
                    foreach (var camera in AdditionalCameras)
                    {
                        if (camera != null)
                            camera.Render();
                    }
                }
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
                CameraToUse.targetTexture = null;
                if (AdditionalCameras != null)
                {
                    foreach (var camera in AdditionalCameras)
                    {
                        if (camera != null)
                            camera.targetTexture = null;
                    }
                }
                RenderTexture.active = null;
                byte[] bytes = screenShot.EncodeToPNG();
                string filename = ScreenShotName(resWidthN, resHeightN);

                System.IO.File.WriteAllBytes(filename, bytes);
                Debug.Log(string.Format("Took screenshot to: {0}", filename));
                Application.OpenURL(filename);
                _takeHiResShot = false;
            }
        }

        public string ScreenShotName(int width, int height)
        {
            string strPath = "";
            strPath = string.Format("{0}/screen_{1}x{2}_{3}.png",
                                 _savePath,
                                 width, height,
                                           System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            _lastScreenshot = strPath;
            return strPath;
        }

        public void TakeHiResShot()
        {
            Debug.Log("Taking Screenshot...");
            _takeHiResShot = true;
        }

    }
}