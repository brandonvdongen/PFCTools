using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFCTools2.SpriteGen
{
    public class SpriteGenMenu : EditorWindow
    {
        private readonly string OUTPUT_FOLDER = "Assets/SpriteGen";
        private readonly int[] sides = new int[] { 4, 8, 16, 32, 64 };

        private bool validSelection { get { return Selection.activeObject is GameObject; } }

        private Button btn_export;
        private IntegerField input_Radial;
        private IntegerField input_Angle;
        private FloatField input_cameraSize;
        private IntegerField input_Pixels;
        private Label selectionLabel;

        [MenuItem("PFCTools/Deprecated/Avatar Sprite Generator")]
        public static void OpenWindow()
        {
            SpriteGenMenu wnd = GetWindow<SpriteGenMenu>();
            wnd.titleContent = new GUIContent("Avatar Sprite Generator");
            wnd.minSize = new Vector2(321, 180);
            wnd.maxSize = new Vector2(321, 180);
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            ObjectField ob = new ObjectField("Target:") { objectType = typeof(GameObject) };

            input_Radial = new IntegerField() { label = "Radial Shots", value = 4 };
            input_Angle = new IntegerField() { label = "Angle Shots", value = 3 };
            input_cameraSize = new FloatField() { label = "Size", value = 1 };
            input_Pixels = new IntegerField() { label = "Pixel Density", value = 256 };


            btn_export = new Button() { text = "Generate Sprite Map" };
            btn_export.clicked += () =>
            {
                ExportSpriteMap();
            };

            selectionLabel = new Label(validSelection ? "Selection: " + Selection.activeObject.name : "No Selection");
            root.Add(selectionLabel);

            root.Add(input_Radial);
            root.Add(input_Angle);
            root.Add(input_cameraSize);
            root.Add(input_Pixels);
            root.Add(btn_export);
            bool isGameObject = Selection.activeObject is GameObject;
            btn_export.SetEnabled(validSelection);

        }

        private void ExportSpriteMap()
        {
            Directory.CreateDirectory($"{OUTPUT_FOLDER}/{Selection.activeGameObject.name}/");
            Renderer[] renderers = Selection.activeGameObject.GetComponentsInChildren<Renderer>(false);

            Vector3 pos = Vector3.zero;

            Bounds bounds = new Bounds();

            bounds.size = Vector3.zero; // reset
            foreach (Renderer renderer in renderers)
            {
                if (renderer.gameObject.activeInHierarchy)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }


            GameObject camera_rig = new GameObject() { name = "SpriteGen Rig" };
            GameObject camera_radial = new GameObject() { name = "Radial" };
            GameObject camera_angle = new GameObject() { name = "Angle" };
            GameObject cameraGO = new GameObject() { name = "Camera" };

            camera_radial.transform.parent = camera_rig.transform;
            camera_angle.transform.parent = camera_radial.transform;
            cameraGO.transform.parent = camera_angle.transform;
            cameraGO.transform.localPosition = Vector3.back * input_cameraSize.value;

            camera_rig.transform.position = bounds.center;

            Camera camera = cameraGO.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = input_cameraSize.value;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0, 0, 0, 0);

            int pixels = input_Pixels.value;

            //Generate Image
            RenderTexture rt = new RenderTexture(pixels, pixels, 24);
            camera.targetTexture = rt;
            RenderTexture.active = rt;

            float inputAngle = input_Angle.value - 1;
            float inputRadial = input_Radial.value - 1;

            Texture2D outputImage = new Texture2D((int)(pixels * (inputRadial + 1)), (int)(pixels * (inputAngle + 1)), TextureFormat.ARGB32, false);
            float angleShots = inputAngle;
            while (angleShots >= 0)
            {
                camera_angle.transform.localRotation = Quaternion.Euler(Mathf.Lerp(-90, 90, angleShots / inputAngle), 0, 0);

                float radialShots = inputRadial;
                while (radialShots >= 0)
                {
                    camera_radial.transform.localRotation = Quaternion.Euler(0, Mathf.Lerp(180, -180, radialShots / inputRadial), 0);
                    camera.Render();
                    outputImage.ReadPixels(new Rect(0, 0, pixels, pixels), (int)(pixels * radialShots), (int)(pixels * angleShots));
                    radialShots--;
                }
                angleShots--;
            }

            byte[] bytes = outputImage.EncodeToPNG();
            string uniqPath = $"{OUTPUT_FOLDER}/{Selection.activeGameObject.name}/spritesheet.png";
            File.WriteAllBytes(uniqPath, bytes);

            RenderTexture.active = null;
            camera.targetTexture = null;
            DestroyImmediate(rt);
            DestroyImmediate(camera_rig);
            AssetDatabase.Refresh();
        }

        private void OnSelectionChange()
        {
            if (btn_export != null)
            {
                btn_export.SetEnabled(validSelection);
            }
            if (selectionLabel != null)
            {
                selectionLabel.text = validSelection ? "Selection: " + Selection.activeObject.name : "No Selection";
            }
        }
    }
}
