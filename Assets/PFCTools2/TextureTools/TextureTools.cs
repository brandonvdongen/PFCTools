using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PFCTools2.TextureTools
{
    public class TextureToolsMenu : EditorWindow
    {
        private readonly string OUTPUT_FOLDER = "Assets/TextureTools";

        private Button btn_split;
        private Button btn_join;
        private ObjectField input_TextureRGBA;
        private ObjectField input_TextureR;
        private ObjectField input_TextureG;
        private ObjectField input_TextureB;
        private ObjectField input_TextureA;

        private Label selectionLabel;

        [MenuItem("PFCTools2/TextureTools")]
        public static void OpenWindow()
        {
            TextureToolsMenu wnd = GetWindow<TextureToolsMenu>();
            wnd.titleContent = new GUIContent("TextureTools");
            wnd.minSize = new Vector2(321, 180);
            wnd.maxSize = new Vector2(321, 180);
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            ObjectField ob = new ObjectField("Target:") { objectType = typeof(GameObject) };

            input_TextureRGBA = new ObjectField() { label = "Texture To Split", objectType = typeof(Texture2D) };
            input_TextureRGBA.RegisterValueChangedCallback(t =>
            {
                OnSplitValueChange();
            });


            btn_split = new Button() { text = "Split Texture" };
            btn_split.clicked += () =>
            {
                SplitInputTexture(input_TextureRGBA.value as Texture2D);
            };

            input_TextureR = new ObjectField() { label = "Texture R", objectType = typeof(Texture2D) };
            input_TextureG = new ObjectField() { label = "Texture G", objectType = typeof(Texture2D) };
            input_TextureB = new ObjectField() { label = "Texture B", objectType = typeof(Texture2D) };
            input_TextureA = new ObjectField() { label = "Texture A", objectType = typeof(Texture2D) };
            btn_join = new Button() { text = "Join Textures" };
            btn_join.clicked += () =>
            {
                JoinInputTextures(input_TextureR.value as Texture2D, input_TextureG.value as Texture2D, input_TextureB.value as Texture2D, input_TextureA.value as Texture2D);
            };

            root.Add(selectionLabel);

            root.Add(input_TextureRGBA);
            root.Add(btn_split);

            root.Add(input_TextureR);
            root.Add(input_TextureG);
            root.Add(input_TextureB);
            root.Add(input_TextureA);
            root.Add(btn_join);

            bool isGameObject = Selection.activeObject is GameObject;
            btn_split.SetEnabled(ValidTexture(input_TextureRGBA.value as Texture2D));

        }

        private void JoinInputTextures(Texture2D R, Texture2D G, Texture2D B, Texture2D A)
        {
            Directory.CreateDirectory($"{OUTPUT_FOLDER}/{R.name}_combined/");
            bool RisReadable = R.isReadable;
            bool GisReadable = G.isReadable;
            bool BisReadable = B.isReadable;
            bool AisReadable = A.isReadable;
            SetTextureImporterFormat(R, true);
            SetTextureImporterFormat(G, true);
            SetTextureImporterFormat(B, true);
            SetTextureImporterFormat(A, true);


            Texture2D Joined_RGBA = new Texture2D(R.width, R.height);


            int x = R.width;
            int y = R.height;

            Color[] Reds = GetColorsByChannel(R.GetPixels(0, 0, R.width, R.height), ChannelPass.Red);
            Color[] Greens = GetColorsByChannel(G.GetPixels(0, 0, G.width, G.height), ChannelPass.Green);
            Color[] Blues = GetColorsByChannel(B.GetPixels(0, 0, B.width, B.height), ChannelPass.Blue);
            Color[] Alpha = GetColorsByChannel(A.GetPixels(0, 0, A.width, A.height), ChannelPass.Alpha);

            Color[] Colors = new Color[Reds.Length];
            for (int i = 0; i < Reds.Length; i++)
            {
                Colors[i] = new Color(Reds[i].r, Greens[i].g, Blues[i].b, Alpha[i].a);
            }
            Joined_RGBA.SetPixels(0, 0, R.width, R.height, Colors);
            Joined_RGBA.Apply();

            File.WriteAllBytes($"{OUTPUT_FOLDER}/combined.png", Joined_RGBA.EncodeToPNG());
            AssetDatabase.Refresh();
        }

        private void SplitInputTexture(Texture2D ToSplit)
        {
            Directory.CreateDirectory($"{OUTPUT_FOLDER}/{ToSplit.name}/");
            bool isReadable = ToSplit.isReadable;
            if (!isReadable)
            {
                SetTextureImporterFormat(ToSplit, true);
            }


            //Generate Image

            Texture2D Split_R = new Texture2D(ToSplit.width, ToSplit.height);
            Texture2D Split_G = new Texture2D(ToSplit.width, ToSplit.height);
            Texture2D Split_B = new Texture2D(ToSplit.width, ToSplit.height);
            Texture2D Split_A = new Texture2D(ToSplit.width, ToSplit.height);

            int x = ToSplit.width;
            int y = ToSplit.height;


            Color[] colors = ToSplit.GetPixels(0, 0, x, y);

            Split_R.SetPixels(0, 0, x, y, GetColorsByChannel(colors, ChannelPass.Red));
            Split_R.Apply();

            Split_G.SetPixels(0, 0, x, y, GetColorsByChannel(colors, ChannelPass.Green));
            Split_G.Apply();

            Split_B.SetPixels(0, 0, x, y, GetColorsByChannel(colors, ChannelPass.Blue));
            Split_B.Apply();

            Split_A.SetPixels(0, 0, x, y, GetColorsByChannel(colors, ChannelPass.Alpha));
            Split_A.Apply();


            if (!isReadable)
            {
                SetTextureImporterFormat(ToSplit, false);
            }

            File.WriteAllBytes($"{OUTPUT_FOLDER}/{ToSplit.name}/{ToSplit.name}_r.png", Split_R.EncodeToPNG());
            File.WriteAllBytes($"{OUTPUT_FOLDER}/{ToSplit.name}/{ToSplit.name}_g.png", Split_G.EncodeToPNG());
            File.WriteAllBytes($"{OUTPUT_FOLDER}/{ToSplit.name}/{ToSplit.name}_b.png", Split_B.EncodeToPNG());
            File.WriteAllBytes($"{OUTPUT_FOLDER}/{ToSplit.name}/{ToSplit.name}_a.png", Split_A.EncodeToPNG());

            AssetDatabase.Refresh();
        }

        public static void SetTextureImporterFormat(Texture2D texture, bool isReadable)
        {
            if (null == texture)
            {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter tImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (tImporter != null)
            {
                tImporter.textureType = TextureImporterType.Default;

                tImporter.isReadable = isReadable;

                tImporter.textureCompression = TextureImporterCompression.Uncompressed;

                AssetDatabase.ImportAsset(assetPath);
                AssetDatabase.Refresh();
            }
        }
        private bool ValidTexture(Texture2D texture)
        {
            return texture != null;
        }

        private void OnSplitValueChange()
        {
            if (btn_split != null)
            {
                btn_split.SetEnabled(ValidTexture(input_TextureRGBA.value as Texture2D));
            }
        }
        private Color[] GetColorsByChannel(Color[] colors, ChannelPass pass)
        {
            Color[] output = new Color[colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                float r = colors[i].r;
                float g = colors[i].g;
                float b = colors[i].b;
                float a = colors[i].a;

                switch (pass)
                {
                    case ChannelPass.Red:
                        output[i] = new Color(r, r, r, 1);
                        break;
                    case ChannelPass.Green:
                        output[i] = new Color(g, g, g, 1);
                        break;
                    case ChannelPass.Blue:
                        output[i] = new Color(b, b, b, 1);
                        break;
                    case ChannelPass.Alpha:
                        output[i] = new Color(a, a, a, 1);
                        break;
                }


            }

            return output;
        }
        private enum ChannelPass
        {
            Red,
            Green,
            Blue,
            Alpha
        }
    }
}
