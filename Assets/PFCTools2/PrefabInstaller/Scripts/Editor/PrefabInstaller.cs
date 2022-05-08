using PFCTools2.Installer.PseudoParser;
using PFCTools2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;
using VRC.SDK3.Avatars.Components;

namespace PFCTools2.Installer.Core
{

    [CustomEditor(typeof(PrefabTemplate), true)]
    public class PrefabInstaller : Editor
    {

        //Operating params
        private bool loaded = false;
        private bool isValid = false;
        private AvatarDefinition currentAvatar;
        private InstallerMode mode = InstallerMode.Intall;

        //UI Elements
        private VisualElement avatarListContainer;
        private VisualElement configWindow;
        private VisualElement customizerWindow;
        private VisualElement debugWindow;
        private Button InstallBtn;
        private VisualElement ErrorList;
        private PrefabTemplate template;
        private List<ValidatorResponse> validatorLog = new List<ValidatorResponse>();

        private void OnEnable()
        {
            template = target as PrefabTemplate;
            template.onConfigChange += onConfigChange;
        }

        private void OnDisable()
        {
            template.onConfigChange -= onConfigChange;
        }

        public void onConfigChange()
        {
            if (loaded)
            {
                ValidateAvatar();
                UpdateUI();
            }
        }

        public override VisualElement CreateInspectorGUI()
        {


            VisualElement root = new VisualElement();
            StyleSheet styleSheet = Resources.Load<StyleSheet>("PFCTools2/PrefabInstaller/BaseStyle");
            root.styleSheets.Add(styleSheet);

            //Build AvatarList Container
            avatarListContainer = new VisualElement();
            avatarListContainer.AddToClassList("AvatarListContainer");
            ListView emptyAvatarList = new ListView() { itemHeight = 16 };
            avatarListContainer.Add(emptyAvatarList);

            //Build Refresh Button
            Button refreshBtn = new Button() { text = "Select Avatar to Install prefab On" };
            void refresh()
            {
                VRCAvatarDescriptor[] descriptors = FindObjectsOfType<VRCAvatarDescriptor>();
                //Array.Reverse(descriptors);
                Func<VisualElement> makeItem = () => new Label();
                Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = descriptors[i].gameObject.name;
                ListView avatarList = new ListView(descriptors, 16, makeItem, bindItem);
                avatarList.onSelectionChanged += obj => { currentAvatar = new AvatarDefinition((obj[0] as VRCAvatarDescriptor)); onConfigChange(); };
                avatarListContainer.Clear();
                avatarListContainer.Add(avatarList);
            };
            refreshBtn.clicked += refresh;
            refresh();

            //Fetch ConfigUI
            configWindow = template.PrefabConfigUI();
            //Fetch CustomizerUI
            customizerWindow = template.CustomizerUI();

            //Create Instal Button
            InstallBtn = new Button() { text = "Install Prefab" };
            InstallBtn.SetEnabled(false);
            InstallBtn.clicked += ProcessPrefab;

            //Create Error List
            ErrorList = new ScrollView();
            ErrorList.AddToClassList("ErrorList");

            //Create Debug
            debugWindow = new VisualElement();
            Button createPseudoBtn = new Button() { text = "New Text Asset" };
            createPseudoBtn.clicked += () => { FileHelper.CreateNewTextFile(template, "newFile.txt", ""); };
            ObjectField pseudoField = new ObjectField() { objectType = typeof(TextAsset) };
            ObjectField controllerField = new ObjectField() { objectType = typeof(AnimatorController) };
            Button testPseudoBtn = new Button() { text = "Generate Animator" };
            testPseudoBtn.clicked += () =>
            {
                if (controllerField.value == null)
                {
                    Pseudo.Parse(pseudoField.value as TextAsset, currentAvatar);
                }
                else
                {
                    Pseudo.Parse(pseudoField.value as TextAsset, currentAvatar, controllerField.value as AnimatorController);
                }
            };
            Button exportStateDataBtn = new Button() { text = "Export State Data" };
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

            //Build UI
            root.Add(refreshBtn);
            root.Add(avatarListContainer);
            SerializedObject SO = new SerializedObject(target);
            if (configWindow != null)
            {
                root.Add(configWindow);
                configWindow.AddToClassList("configWindow");
                configWindow.Bind(SO);
            }
            if (customizerWindow != null)
            {
                root.Add(customizerWindow);
                customizerWindow.AddToClassList("customizerWindow");
                customizerWindow.Bind(SO);
            }
            root.Add(InstallBtn);
            root.Add(ErrorList);
            root.Add(debugWindow);
            loaded = true;
            onConfigChange();
            return root;
        }

        private void UpdateUI()
        {
            if (InstallBtn == null)
            {
                return;
            }

            if (mode == InstallerMode.Intall)
            {
                InstallBtn.text = "Install Prefab";
                configWindow.style.display = DisplayStyle.Flex;
                customizerWindow.style.display = DisplayStyle.None;
                InstallBtn.SetEnabled(isValid);
            }
            if (mode == InstallerMode.Modify)
            {
                InstallBtn.text = "Remove Prefab";
                configWindow.style.display = DisplayStyle.None;
                customizerWindow.style.display = DisplayStyle.Flex;
                InstallBtn.SetEnabled(true);
            }
            if (template.debug)
            {
                debugWindow.style.display = DisplayStyle.Flex;
            }
            else
            {
                debugWindow.style.display = DisplayStyle.None;
            }
        }

        private bool ValidateAvatar()
        {
            List<ValidatorResponse> log = new List<ValidatorResponse>();
            if (ErrorList != null)
            {
                ErrorList.Clear();
            }

            if (currentAvatar != null)
            {
                if (!currentAvatar.HasAnimator) { log.Add(new ValidatorResponse("No animator found", "The selected object seems to not have an animator attached to it, make sure your avatar has a animator!", ValidatorResponseType.error)); }
                if (!currentAvatar.HasParameters) { log.Add(new ValidatorResponse("No expression parameters found", "This avatar seems to not have a expression parameter asset assigned in the descriptor. The installer will create a new parameter asset to use if you decide to continue.", ValidatorResponseType.warning)); }
                if (!currentAvatar.HasMenu) { log.Add(new ValidatorResponse("No expression menu found", "This avatar seems to not have a expression menu asset assigned in the descriptor. The installer will create a new menu asset to use if you decide to continue.", ValidatorResponseType.warning)); }
            }
            else
            {
                return false;
            }

            if (template.IsInstalledOn(currentAvatar))
            {
                mode = InstallerMode.Modify;
                log.Add(new ValidatorResponse("Existing Install Found", "An existing installation of this prefab has been found on the selected avatar.", ValidatorResponseType.notice));
            }
            else
            {
                mode = InstallerMode.Intall;
            }

            template.Validate(log, mode);

            isValid = true;
            foreach (ValidatorResponse response in log)
            {
                VisualElement notif = createNotification(response.name, response.desc);

                if (response.responseType == ValidatorResponseType.error) { notif.AddToClassList("error"); isValid = false; }
                if (response.responseType == ValidatorResponseType.warning)
                {
                    notif.AddToClassList("warning");
                }

                if (ErrorList != null)
                {
                    ErrorList.Add(notif);
                }
            }
            return isValid;

        }


        private VisualElement createNotification(string name, string description)
        {
            VisualElement note = new VisualElement();
            note.AddToClassList("notification");
            Label label = new Label(name);
            label.AddToClassList("title");
            TextElement text = new TextElement() { text = description };
            text.AddToClassList("description");

            note.Add(label);
            note.Add(text);
            return note;
        }

        private void ProcessPrefab()
        {
            if (mode == InstallerMode.Intall)
            {
                List<string> metaTags = template.getConstraintMetaTags();
                GameObject Prefab = PrefabUtility.InstantiatePrefab(template.Prefab) as GameObject;
                Prefab.transform.parent = currentAvatar.transform;

                List<ConstraintAssigner> constraintAssigner = new List<ConstraintAssigner>(Prefab.GetComponentsInChildren<ConstraintAssigner>());
                foreach (ConstraintAssigner assigner in constraintAssigner)
                {
                    foreach (HumanBoneEntry hbe in assigner.Sources)
                    {
                        IConstraint constraint = assigner.TargetConstraint as IConstraint;
                        Transform bone = currentAvatar.Animator.GetBoneTransform(hbe.targetBone);
                        if (assigner.Mode == ConstraintAssignerMode.All)
                        {
                            constraint.AddSource(new ConstraintSource() { sourceTransform = bone, weight = hbe.weight });
                        }
                        else if (assigner.Mode == ConstraintAssignerMode.Meta)
                        {
                            List<string> entryTags = new List<string>(hbe.Meta.Split(char.Parse(",")));
                            bool tagFound = false;
                            foreach (string tag in entryTags)
                            {
                                if (metaTags.Contains(tag))
                                {
                                    tagFound = true;
                                }
                            }
                            if (tagFound)
                            {
                                constraint.AddSource(new ConstraintSource() { sourceTransform = bone, weight = hbe.weight });
                            }
                        }
                    }
                    DestroyImmediate(assigner);
                }
                List<PositionAssigner> positionAssigners = new List<PositionAssigner>(Prefab.GetComponentsInChildren<PositionAssigner>());
                foreach (PositionAssigner assigner in positionAssigners)
                {
                    foreach (PositionOffsetEntry poe in assigner.Offsets)
                    {
                        List<string> entryTags = new List<string>(poe.Meta.Split(char.Parse(",")));
                        bool tagFound = false;
                        foreach (string tag in entryTags)
                        {
                            if (metaTags.Contains(tag))
                            {
                                tagFound = true;
                            }
                        }
                        if (tagFound)
                        {
                            assigner.transform.position = poe.offset;
                        }
                    }
                    DestroyImmediate(assigner);

                }
                AnimatorController controller = currentAvatar.GetLayer(VRCAvatarDescriptor.AnimLayerType.FX);
                if (EditorUtility.DisplayDialog("Animator Layer Import", "Would you like to import the prefab's Animation layers into your avatar's animator?\n(A backup of your animator will be made just in case)", "Import Animation Layers", "Skip Import"))
                {
                    if (template.FXLayers.Length > 0 && controller != null)
                    {

                        string path = AssetDatabase.GetAssetPath(controller);
                        string directory = Path.GetDirectoryName(path);
                        string filename = Path.GetFileNameWithoutExtension(path);
                        string extension = Path.GetExtension(path);
                        string newPath = AssetDatabase.GenerateUniqueAssetPath(directory + "\\Backups\\" + filename + "Backup" + extension);
                        //Debug.Log(newPath);
                        AssetDatabase.CopyAsset(path, newPath);
                        foreach (TextAsset pseudoFile in template.FXLayers)
                        {
                            Pseudo.Parse(pseudoFile, currentAvatar, currentAvatar.GetLayer(VRCAvatarDescriptor.AnimLayerType.FX));
                        }
                    }
                }
            }
            else if (mode == InstallerMode.Modify)
            {
                GameObject prefab = template.GetInstalledPrefab(currentAvatar);
                template.BeforePrefabRemove();
                DestroyImmediate(prefab);
                template.AfterPrefabRemove();
                AnimatorController controller = currentAvatar.GetLayer(VRCAvatarDescriptor.AnimLayerType.FX);
                if (controller)
                {
                    if (EditorUtility.DisplayDialog("Delete Imported Layers", "Would you like to Delete the prefab's animation layers from your avatar's animator?\n(A backup of your animator will be made just in case)", "Delete Layers", "Keep Layers"))
                    {
                        if (template.FXLayers.Length > 0 && controller != null)
                        {
                            string path = AssetDatabase.GetAssetPath(controller);
                            string directory = Path.GetDirectoryName(path);
                            string filename = Path.GetFileNameWithoutExtension(path);
                            string extension = Path.GetExtension(path);
                            string newPath = AssetDatabase.GenerateUniqueAssetPath(directory + "\\" + filename + "Backup" + extension);
                            AssetDatabase.CopyAsset(path, newPath);
                            foreach (TextAsset pseudoFile in template.FXLayers)
                            {
                                Pseudo.Remove(pseudoFile, currentAvatar, controller);
                            }
                        }

                    }

                }

            }
            onConfigChange();
        }

    }
    public struct ValidatorResponse
    {
        public string name;
        public string desc;
        public ValidatorResponseType responseType;
        public ValidatorResponse(string name, string desc, ValidatorResponseType responseType)
        {
            this.name = name;
            this.desc = desc;
            this.responseType = responseType;
        }

    }
    public enum ValidatorResponseType
    {
        error,
        warning,
        notice
    }
    public enum InstallerMode
    {
        Intall,
        Modify
    }
}