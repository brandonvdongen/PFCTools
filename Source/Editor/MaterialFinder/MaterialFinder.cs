using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PFCTools.EditorTools {

    [InitializeOnLoad]
    public class MaterialFinder : EditorWindow {

        Vector2 scrollPos;
        string search = "search";
        bool showAvatarSelector = false;
        bool showSettings = false;
        bool showRenderers = true;
        bool showParticleSystems = false;
        bool showTrailRenderers = false;
        bool showLineRenderers = false;

        bool showShader = false;
        bool showMaterialPreview = true;

        GameObject[] knownAvatars;

        Dictionary<Material, MaterialData> materialCache = new Dictionary<Material, MaterialData>();
        [SerializeField] public static GameObject Target;




        [MenuItem("PFCTools/Material Finder")]
        public static EditorWindow ShowWindow() {
            EditorWindow window = EditorWindow.GetWindow(typeof(MaterialFinder), false, "Material Finder");
            window.minSize = new Vector2(200, 50);
            return window;
        }

        private void OnGUI() {

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();

            if (VRCSDK.installed) {

                if (GUILayout.Button("Select Avatar", EditorStyles.toolbarDropDown)) {
                    if (showAvatarSelector == false) {
                        knownAvatars = VRCSDK.GetAvatars();
                    }
                    showAvatarSelector = !showAvatarSelector;
                    //UpdateMaterialCache();

                }
            }
            if (GUILayout.Button("Select from scene", EditorStyles.toolbarButton)) {
                Target = Selection.activeGameObject;
                UpdateMaterialCache();
            }
            EditorGUILayout.EndHorizontal();

            if (showAvatarSelector) {
                if (knownAvatars.Length == 1) {
                    Target = knownAvatars[0];
                    UpdateMaterialCache();
                    drawMaterialList();
                    showAvatarSelector = false;

                }
                else {
                    foreach (var avatar in knownAvatars) {
                        EditorGUILayout.BeginHorizontal();
                        //GUILayout.FlexibleSpace();
                        if (GUILayout.Button(avatar.name, EditorStyles.miniButton)) {
                            Target = avatar;
                            showAvatarSelector = false;
                            UpdateMaterialCache();
                            drawMaterialList();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                UIUtils.HorizontalLine();
            }
            else {


                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Target:");
                GUILayout.FlexibleSpace();
                Target = (GameObject)EditorGUILayout.ObjectField("", Target, typeof(GameObject), true, GUILayout.MinWidth(100));
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck()) { UpdateMaterialCache(); }
                UIUtils.HorizontalLine();
            }
            if (Target != null) {

                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                showSettings = GUILayout.Toggle(showSettings, "Options", EditorStyles.toolbarButton);
                EditorGUI.BeginChangeCheck();
                search = EditorGUILayout.TextField("", search, EditorStyles.toolbarTextField);
                if (EditorGUI.EndChangeCheck()) {
                    if (string.IsNullOrWhiteSpace(search)) {
                        search = "search";
                    }
                }
                GUILayout.EndHorizontal();

                if (showSettings) {
                    GUILayout.Label("Settings:");
                    showShader = GUILayout.Toggle(showShader, "Show shader file");
                    showMaterialPreview = GUILayout.Toggle(showMaterialPreview, "Show material previews");
                    UIUtils.HorizontalLine();
                    GUILayout.Label("Sources:");
                    EditorGUI.BeginChangeCheck();
                    showRenderers = GUILayout.Toggle(showRenderers, "Mesh Renderers");
                    showParticleSystems = GUILayout.Toggle(showParticleSystems, "Particle Systems");
                    showTrailRenderers = GUILayout.Toggle(showTrailRenderers, "Trail Renderers");
                    showLineRenderers = GUILayout.Toggle(showLineRenderers, "Line Renderers");
                    if (EditorGUI.EndChangeCheck()) {
                        UpdateMaterialCache();
                    }
                    UIUtils.HorizontalLine();
                }
                if (materialCache.Count > 1) {
                    scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                    drawMaterialList();
                    EditorGUILayout.EndScrollView();
                }
            }


        }


        private void UpdateMaterialCache() {
            if (Target == null) return;
            materialCache = new Dictionary<Material, MaterialData>();
            if (showRenderers) {
                Renderer[] renderers = Target.GetComponentsInChildren<MeshRenderer>(true);
                renderers = renderers.Concat<Renderer>(Target.GetComponentsInChildren<SkinnedMeshRenderer>(true)).ToArray();
                foreach (Renderer renderer in renderers) {
                    if (renderer == null) continue;
                    cacheMaterials(renderer.sharedMaterials, renderer.gameObject);

                }
            }
            if (showParticleSystems) {
                ParticleSystemRenderer[] particleSystems = Target.GetComponentsInChildren<ParticleSystemRenderer>(true);
                foreach (ParticleSystemRenderer particleSystem in particleSystems) {
                    cacheMaterials(particleSystem.sharedMaterials, particleSystem.gameObject);
                }
            }
            if (showTrailRenderers) {
                TrailRenderer[] trailrenderers = Target.GetComponentsInChildren<TrailRenderer>(true);
                foreach (TrailRenderer trailrenderer in trailrenderers) {
                    cacheMaterials(trailrenderer.sharedMaterials, trailrenderer.gameObject);
                }
            }
            if (showLineRenderers) {
                LineRenderer[] lineRenderers = Target.GetComponentsInChildren<LineRenderer>(true);
                foreach (LineRenderer lineRenderer in lineRenderers) {
                    cacheMaterials(lineRenderer.sharedMaterials, lineRenderer.gameObject);
                }
            }
        }

        private void cacheMaterials(Material[] materials, GameObject source) {
            foreach (Material material in materials) {
                if (material == null) continue;
                if (!materialCache.ContainsKey(material)) {
                    MaterialData materialData = new MaterialData();
                    materialData.material = material;
                    materialData.count = 1;
                    materialData.renderers = new List<GameObject>();
                    materialData.renderers.Add(source);
                    //materialData.renderers.Add(renderer as Renderer);
                    materialCache.Add(material, materialData);
                }
                else {
                    MaterialData materialData = materialCache[material];
                    materialData.count = materialData.count + 1;
                    materialData.renderers.Add(source);
                    materialCache[material] = materialData;
                }

            }
        }

        private void drawMaterialList() {
            int totalMaterialCOunt = 0;
            foreach (KeyValuePair<Material, MaterialData> kvp in materialCache) {
                if (search != "search" && (!kvp.Key.name.ToLower().Contains(search.ToLower()))) continue;
                totalMaterialCOunt += kvp.Value.count;
            }
            GUILayout.Label(string.Format("Found {0} materials", totalMaterialCOunt));
            foreach (KeyValuePair<Material, MaterialData> kvp in materialCache) {
                Material material = kvp.Key;
                if (search != "search" && (!material.name.ToLower().Contains(search.ToLower()))) continue;
                MaterialData materialData = kvp.Value;

                UIUtils.HorizontalLine();
                EditorGUILayout.BeginHorizontal();
                if (!showMaterialPreview) {
                    if (GUILayout.Button(material.name + (materialData.count > 1 ? " (x" + materialData.count + ")" : ""), EditorStyles.miniButtonLeft)) {
                        Selection.activeObject = material;
                    }
                }
                else {
                    GUILayout.Label(material.name + (materialData.count > 1 ? "(" + materialData.count + " Instances)" : ""), GUILayout.MinWidth(50));
                    GUILayout.FlexibleSpace();
                }



                if (GUILayout.Button((materialData.count > 1 ? "Select Sources" : "Select Source"), (!showMaterialPreview ? EditorStyles.miniButtonRight : GUI.skin.button),GUILayout.MaxWidth(100))) {
                    Selection.objects = materialData.renderers.ToArray();
                }
                EditorGUILayout.EndHorizontal();

                if (showMaterialPreview) {
                    if (GUILayout.Button(AssetPreview.GetAssetPreview(material), EditorStyles.miniButton, GUILayout.MaxHeight(100))) {
                        Selection.activeObject = material;
                    }
                }
                if (showShader) {
                    EditorGUILayout.ObjectField("", material.shader, typeof(Shader), false);
                }
            }
        }

        private struct MaterialData {
            public Material material;
            public int count;
            public List<GameObject> renderers;
        }
    }


}