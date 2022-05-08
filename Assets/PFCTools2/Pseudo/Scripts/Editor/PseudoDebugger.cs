using PFCTools2.Installer.PseudoParser;
using PFCTools2.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;
namespace PFCTools2.Installer.PseudoDebugger
{
    public class PseudoDebugger : EditorWindow
    {

        [MenuItem("PFCTools2/Pseudo/Debugger")]
        public static void ShowWindow()
        {
            // Opens the window, otherwise focuses it if it’s already open.
            PseudoDebugger window = GetWindow<PseudoDebugger>();

            // Adds a title to the window.
            window.titleContent = new GUIContent("Pseudo Debugger");

            // Sets a minimum size to the window.
            window.minSize = new Vector2(50, 50);
        }

        private Button btn_createPseudoFile;
        private ObjectField fld_Controller;
        private List<AnimatorController> controllers = new List<AnimatorController>();
        private VisualElement controllerListView;

        private void CreateGUI()
        {

            VisualElement root = rootVisualElement;
            StyleSheet styleSheet = Resources.Load<StyleSheet>("PFCTools2/PrefabInstaller/BaseStyle");
            root.styleSheets.Add(styleSheet);

            VisualElement debugWindow = new VisualElement();
            Button createPseudoBtn = new Button() { text = "New Text Asset" };
            createPseudoBtn.clicked += () => { FileHelper.CreateNewTextFile("Assets/Empty.txt", ""); };
            ObjectField pseudoField = new ObjectField() { objectType = typeof(TextAsset) };
            ObjectField controllerField = new ObjectField() { objectType = typeof(AnimatorController) };
            Button testPseudoBtn = new Button() { text = "Parse Pseudo To Animator" };
            testPseudoBtn.clicked += () =>
            {
                if (controllerField.value == null)
                {
                    Pseudo.Parse(pseudoField.value as TextAsset, null);
                }
                else
                {
                    Pseudo.Parse(pseudoField.value as TextAsset, null, controllerField.value as AnimatorController);
                }
            };
            Button exportStateDataBtn = new Button() { text = "Dump Animator to Pseudo" };
            exportStateDataBtn.clicked += () =>
            {
                if (controllerField.value != null)
                {
                    PseudoExporter.Export(controllerField.value as AnimatorController);
                }
            };

            debugWindow.Add(createPseudoBtn);
            debugWindow.Add(pseudoField);
            debugWindow.Add(controllerField);
            debugWindow.Add(testPseudoBtn);
            debugWindow.Add(exportStateDataBtn);


            root.Add(debugWindow);

        }
        private void UpdateDescriptor(ChangeEvent<UnityEngine.Object> evt)
        {

            VRCAvatarDescriptor descriptor = evt.newValue as VRCAvatarDescriptor;

            if (descriptor != null)
            {
                controllers = new List<AnimatorController>();
                foreach (VRCAvatarDescriptor.CustomAnimLayer con in descriptor.baseAnimationLayers)
                {
                    if (con.animatorController != null)
                    {
                        controllers.Add(con.animatorController as AnimatorController);
                        Debug.Log(con.animatorController.name);
                    }
                }

                Func<VisualElement> makeItem = (() => new Label());
                Action<VisualElement, int> bindItem = (e, i) => { if (controllers.Count < 1) { return; } (e as Label).text = controllers[i].name; };
                ListView list = new ListView() { itemHeight = 20 };
            }
            else
            {
                controllers = new List<AnimatorController>();
                controllers.Clear();
            }



        }
    }
}