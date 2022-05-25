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
    public class PrefabInstallerWindow : EditorWindow
    {
        //Operating params
        private bool loaded = false;
        private bool isValid = false;
        private bool isFocused = false;
        private AvatarDefinition currentAvatar;
        private InstallerMode mode = InstallerMode.Intall;

        //UI Elements
        private VisualElement avatarListContainer;
        private VisualElement configContainer;
        private VisualElement customizerContainer;
        private Button InstallBtn;
        private PrefabTemplate template;

        //Debug Info
        private ScrollView ErrorList;
        private static List<ValidatorResponse> validatorLog = new List<ValidatorResponse>();
        private GameObject gizmoDelegate;

#if PFCTOOLSDEBUG
        [MenuItem("PFCTools2/Debug/ClearWindow/PrefabInstallerWindow")]
        public static void ClearWindows()
        {

            while (EditorWindow.HasOpenInstances<PrefabInstallerWindow>())
            {
                PrefabInstallerWindow window = (PrefabInstallerWindow)EditorWindow.GetWindow(typeof(PrefabInstallerWindow), false, "Prefab Installer");
                if (window != null)
                {
                    window.Close();
                }
            }
        }
#endif

        public static void OpenWindow(PrefabTemplate target)
        {
            // Get existing open window or if none, make a new one:
            //PrefabInstallerWindow window = (PrefabInstallerWindow)EditorWindow.GetWindow(typeof(PrefabInstallerWindow), false, "Prefab Installer");
            PrefabInstallerWindow window = CreateInstance<PrefabInstallerWindow>();
            window.template = target;
            window.titleContent = new GUIContent(target.PrefabName + " Installer");
            window.Show();
            VRCAvatarDescriptor[] descriptors = FindObjectsOfType<VRCAvatarDescriptor>();
            window.currentAvatar = new AvatarDefinition(descriptors[0]);
            window.OnConfigChange();
            window.OnTargetChange();
        }

        private void OnEnable()
        {
            PrefabTemplate.OnConfigChange += OnConfigChange;
            PrefabTemplate.canDrawGizmos = true;
        }

        private void OnDisable()
        {
            PrefabTemplate.OnConfigChange -= OnConfigChange;
            PrefabTemplate.canDrawGizmos = false;
        }

        private void OnConfigChange()
        {
            if (loaded)
            {
                ValidateAvatar();
                UpdateUI();
            }
        }
        private void OnTargetChange()
        {
            SerializedObject SO = new SerializedObject(template);
            //Setup Config Window
            configContainer.Clear();
            VisualElement configUI = template.PrefabConfigUI();
            configContainer.Add(configUI);
            configUI.Bind(SO);

            //Setup Customizer Window
            customizerContainer.Clear();
            VisualElement customizerUI = template.CustomizerUI();
            customizerContainer.Add(customizerUI);
            customizerUI.Bind(SO);
        }

        public void CreateGUI()
        {


            VisualElement root = rootVisualElement;
            StyleSheet styleSheet = Resources.Load<StyleSheet>("PFCTools2/PrefabInstaller/BaseStyle");
            root.styleSheets.Add(styleSheet);
            root.AddToClassList("root");

            //Build AvatarList Container
            avatarListContainer = new VisualElement();
            avatarListContainer.AddToClassList("AvatarListContainer");
            ListView emptyAvatarList = new ListView() { itemHeight = 16 };
            avatarListContainer.Add(emptyAvatarList);

            //Build Refresh Button
            Button refreshBtn = new Button() { text = "Refresh Avatar List" };
            void refresh()
            {
                VRCAvatarDescriptor[] descriptors = FindObjectsOfType<VRCAvatarDescriptor>();
                //Array.Reverse(descriptors);
                Func<VisualElement> makeItem = () => new Label();
                Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = descriptors[i].gameObject.name;
                ListView avatarList = new ListView(descriptors, 16, makeItem, bindItem);
                avatarList.onSelectionChanged += obj => { currentAvatar = new AvatarDefinition((obj[0] as VRCAvatarDescriptor)); OnConfigChange(); OnTargetChange(); };
                avatarListContainer.Clear();
                avatarListContainer.Add(avatarList);
            };
            refreshBtn.clicked += refresh;
            refresh();

            //Fetch ConfigUI
            configContainer = new VisualElement();
            //Fetch CustomizerUI
            customizerContainer = new VisualElement();

            //Create Instal Button
            InstallBtn = new Button() { text = "Install Prefab" };
            InstallBtn.SetEnabled(false);
            InstallBtn.clicked += ProcessPrefab;

            //Create Error List
            ErrorList = new ScrollView();
            ErrorList.AddToClassList("ErrorList");
            ErrorList.Add(new Label("test") { text = "test" });

            //Build UI
            root.Add(refreshBtn);
            root.Add(avatarListContainer);
            if (configContainer != null)
            {
                root.Add(configContainer);
                configContainer.AddToClassList("configWindow");
            }
            if (customizerContainer != null)
            {
                root.Add(customizerContainer);
                customizerContainer.AddToClassList("customizerWindow");

            }
            root.Add(InstallBtn);
            root.Add(ErrorList);

            OnConfigChange();
            loaded = true;
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
                configContainer.style.display = DisplayStyle.Flex;
                customizerContainer.style.display = DisplayStyle.None;
                InstallBtn.SetEnabled(isValid);
            }
            if (mode == InstallerMode.Modify)
            {
                InstallBtn.text = "Remove Prefab";
                configContainer.style.display = DisplayStyle.None;
                customizerContainer.style.display = DisplayStyle.Flex;
                InstallBtn.SetEnabled(true);
            }
            ErrorList.Clear();
            foreach (ValidatorResponse response in validatorLog)
            {
                VisualElement notif = createNotification(response.name, response.desc);

                if (response.responseType == ValidatorResponseType.error) { notif.AddToClassList("error"); }
                if (response.responseType == ValidatorResponseType.warning) { notif.AddToClassList("warning"); }

                ErrorList.Add(notif);

            }
        }

        private bool ValidateAvatar()
        {
            validatorLog.Clear();

            if (currentAvatar != null)
            {
                if (!currentAvatar.HasAnimator) { validatorLog.Add(new ValidatorResponse("No animator found", "The selected object seems to not have an animator attached to it, make sure your avatar has a animator!", ValidatorResponseType.error)); }
                if (!currentAvatar.HasParameters) { validatorLog.Add(new ValidatorResponse("No expression parameters found", "This avatar seems to not have a expression parameter asset assigned in the descriptor. The installer will create a new parameter asset to use if you decide to continue.", ValidatorResponseType.warning)); }
                if (!currentAvatar.HasMenu) { validatorLog.Add(new ValidatorResponse("No expression menu found", "This avatar seems to not have a expression menu asset assigned in the descriptor. The installer will create a new menu asset to use if you decide to continue.", ValidatorResponseType.warning)); }

                foreach (string path in template.RequiredResourcePaths)
                {
                    if (Resources.LoadAll(template.PrefabName + "/" + path).Length == 0) { validatorLog.Add(new ValidatorResponse("Missing Resources", $"It appears you moved the {path} folder out of it's parent Resources folder.\nFor the installer to function properly the {path} folder has to be inside any Resources folder with the following path:\n\nResources/{template.PrefabName}/{path}", ValidatorResponseType.error)); }
                }


            }
            else
            {
                return false;
            }

            if (template.IsInstalledOn(currentAvatar))
            {
                mode = InstallerMode.Modify;
                validatorLog.Add(new ValidatorResponse("Existing Install Found", "An existing installation of this prefab has been found on the selected avatar.", ValidatorResponseType.notice));
            }
            else
            {
                mode = InstallerMode.Intall;
            }

            template.Validate(validatorLog, mode);

            isValid = true;
            foreach (ValidatorResponse log in validatorLog)
            {
                if (log.responseType == ValidatorResponseType.error)
                {
                    isValid = false;
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
                List<string> metaTags = template.GetMetaTags();
                Dictionary<string, MetaData> metaData = template.GetMetaData();
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
                    if (assigner.Mode == PositionAssignerMode.MetaData)
                    {
                        MetaData<Vector3> data = metaData[assigner.MetaDataKey] as MetaData<Vector3>;
                        Debug.Log(data.Value);
                        assigner.transform.position = data.Value;
                    }
                    else
                    {
                        foreach (PositionOffsetEntry poe in assigner.Offsets)
                        {
                            bool tagFound = false;
                            if (assigner.Mode == PositionAssignerMode.MetaTags)
                            {
                                List<string> entryTags = new List<string>(poe.Meta.Split(char.Parse(",")));
                                foreach (string tag in entryTags)
                                {
                                    if (metaTags.Contains(tag))
                                    {
                                        tagFound = true;
                                    }
                                }
                            }
                            if (tagFound || assigner.Mode == PositionAssignerMode.All)
                            {
                                Vector3 centroid = Vector3.zero;
                                float massTotal = 0;
                                foreach (sourceData target in poe.targets)
                                {
                                    Vector3 start = centroid;
                                    centroid += currentAvatar.Animator.GetBoneTransform(target.bone).position * target.weight;
                                    Debug.DrawLine(start, centroid, Color.blue, 1, false);
                                    massTotal += (target.weight);
                                }
                                centroid /= massTotal;
                                assigner.transform.position = centroid + poe.offset;
                            }

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
                        if (!AssetDatabase.IsValidFolder($"{directory}/Backups"))
                        {
                            AssetDatabase.CreateFolder($"{directory}", "Backups");
                        }
                        string newPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/Backups/{filename}Backup({DateTime.Now:MM-dd-yy:H:mm:ss}){extension}");
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
                            if (!AssetDatabase.IsValidFolder($"{directory}/Backups"))
                            {
                                AssetDatabase.CreateFolder($"{directory}", "Backups");
                            }
                            string newPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/Backups/{filename}Backup({DateTime.Now:MM-dd-yy:H:mm:ss}){extension}");
                            AssetDatabase.CopyAsset(path, newPath);
                            foreach (TextAsset pseudoFile in template.FXLayers)
                            {
                                Pseudo.Remove(pseudoFile, currentAvatar, controller);
                            }
                        }

                    }

                }

            }
            OnConfigChange();
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