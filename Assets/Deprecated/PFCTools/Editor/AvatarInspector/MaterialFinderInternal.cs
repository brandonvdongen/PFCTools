using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PFCTools.Utils;

namespace PFCTools.AvatarInspector {

    public class MaterialFinderInternal {


        Vector2 scrollPos;
        string search = "search";
        bool showSettings = false;
        bool showRenderers = true;
        bool showParticleSystems = false;
        bool showTrailRenderers = false;
        bool showLineRenderers = false;

        bool showShader = false;
        bool showMaterialPreview = true;

        public readonly Dictionary<Material, MaterialData> _materialCache;
        Dictionary<Material, MaterialData> filteredMaterials;

        [SerializeField] public static GameObject Target;

        public MaterialFinderInternal(Dictionary<Material, MaterialData> materialCache) {
            _materialCache = materialCache;
        }

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
                PFCGUI.HorizontalLine();
            }
            if (_materialCache.Count > 1) {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                drawMaterialList();
                EditorGUILayout.EndScrollView();
            }


        }

        private void drawMaterialList() {
            int totalMaterialCOunt = 0;

            filteredMaterials = new Dictionary<Material, MaterialData>();
            foreach (KeyValuePair<Material, MaterialData> kvp in _materialCache) {

                if (showRenderers) {
                    if(kvp.Value.types.Contains(typeof(MeshRenderer)) || kvp.Value.types.Contains(typeof(SkinnedMeshRenderer))){
                        filteredMaterials.Add(kvp.Key,kvp.Value);
                    }
                }
                else if (showParticleSystems && kvp.Value.types.Contains(typeof(ParticleSystemRenderer))) {
                    filteredMaterials.Add(kvp.Key, kvp.Value);
                }
                else if (showLineRenderers && kvp.Value.types.Contains(typeof(LineRenderer))) {
                    filteredMaterials.Add(kvp.Key, kvp.Value);
                }
                else if (showTrailRenderers && kvp.Value.types.Contains(typeof(TrailRenderer))) {
                    filteredMaterials.Add(kvp.Key, kvp.Value);
                }
            }

                foreach (KeyValuePair<Material, MaterialData> kvp in filteredMaterials) {
                if (search != "search" && (!kvp.Key.name.ToLower().Contains(search.ToLower()))) continue;
                totalMaterialCOunt += kvp.Value.count;
            }
            GUILayout.Label(string.Format("Found {0} materials", totalMaterialCOunt));
            foreach (KeyValuePair<Material, MaterialData> kvp in filteredMaterials) {
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

    }
}