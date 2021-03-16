using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PFCTools.Utils;

namespace PFCTools.AvatarInspector {

    public class MaterialFinderInternal {


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

        public void drawMaterialFinder() {

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            showSettings = GUILayout.Toggle(showSettings, EditorGUIUtility.IconContent("_Popup"), EditorStyles.toolbarButton, GUILayout.MaxWidth(30));
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            search = EditorGUILayout.TextField("", search, GUI.skin.FindStyle("ToolbarSeachTextField"));
            if (EditorGUI.EndChangeCheck()) {
                if (string.IsNullOrWhiteSpace(search)) {
                    search = "";
                }
            }
            if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSeachCancelButton"))) {
                GUI.FocusControl(null);
                search = "";
            }
            GUILayout.EndHorizontal();

            if (showSettings) {
                GUILayout.Label("Settings:");
                showShader = GUILayout.Toggle(showShader, "Show shader file");
                showMaterialPreview = GUILayout.Toggle(showMaterialPreview, "Show material previews");
                PFCGUI.HorizontalLine();
                GUILayout.Label("Sources:");
                EditorGUI.BeginChangeCheck();
                showRenderers = GUILayout.Toggle(showRenderers, "Mesh Renderers");
                showParticleSystems = GUILayout.Toggle(showParticleSystems, "Particle Systems");
                showTrailRenderers = GUILayout.Toggle(showTrailRenderers, "Trail Renderers");
                showLineRenderers = GUILayout.Toggle(showLineRenderers, "Line Renderers");
                if (EditorGUI.EndChangeCheck()) {
                    UpdateMaterialCache();
                }
                PFCGUI.HorizontalLine();
            }
            if (Target != null) {
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

                PFCGUI.HorizontalLine();
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



                if (GUILayout.Button((materialData.count > 1 ? "Select Sources" : "Select Source"), (!showMaterialPreview ? EditorStyles.miniButtonRight : GUI.skin.button), GUILayout.MaxWidth(100))) {
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