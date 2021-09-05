using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using PFCTools.Utils;
using VRC.SDK3.Avatars.Components;
using UnityEngine.Animations;

namespace PFCTools2.Installer.Core {

    [CustomEditor(typeof(PrefabTemplate), true)]
    public class PrefabInstaller : Editor {

        VRCAvatarDescriptor _selectedAvatar;
        VRCAvatarDescriptor ActiveDescriptor { get { return _selectedAvatar; } set { _selectedAvatar = value; ValidateAvatar(); } }
        bool avatarSelector;
        VisualElement avatarListContainer;
        VisualElement ErrorList;
        Button InstallBtn;
        PrefabTemplate template;

        List<validatorResponse> validatorLog = new List<validatorResponse>();

        private void OnEnable() {
            template = target as PrefabTemplate;
            template.onConfigChange += onPrefabConfigChange;
        }

        private void OnDisable() {
            template.onConfigChange -= onPrefabConfigChange;
        }

        public void onPrefabConfigChange() {
            ValidateAvatar();
        }

        public override VisualElement CreateInspectorGUI() {


            VisualElement root = new VisualElement();
            StyleSheet styleSheet = Resources.Load<StyleSheet>("PFCTools2/PrefabInstaller/BaseStyle");
            root.styleSheets.Add(styleSheet);


            avatarListContainer = new VisualElement();
            avatarListContainer.AddToClassList("AvatarListContainer");
            ListView emptyAvatarList = new ListView() { itemHeight = 16 };
            avatarListContainer.Add(emptyAvatarList);

            Button refreshBtn = new Button() { text = "Select Avatar to Install prefab On" };
            void refresh() {
                VRCAvatarDescriptor[] descriptors = FindObjectsOfType<VRCAvatarDescriptor>();
                Array.Reverse(descriptors);
                Func<VisualElement> makeItem = () => new Label();
                Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = descriptors[i].gameObject.name;
                ListView avatarList = new ListView(descriptors, 16, makeItem, bindItem);
                avatarList.onSelectionChanged += obj => { ActiveDescriptor = (obj[0] as VRCAvatarDescriptor); };
                avatarListContainer.Clear();
                avatarListContainer.Add(avatarList);
            };
            refreshBtn.clicked += refresh;
            refresh();

            VisualElement config = template.PrefabConfigUI();

            InstallBtn = new Button() { text = "Install Prefab" };
            InstallBtn.SetEnabled(false);
            InstallBtn.clicked += BuildPrefab;

            ErrorList = new ScrollView();
            ErrorList.AddToClassList("ErrorList");

            root.Add(refreshBtn);
            root.Add(avatarListContainer);
            if (config != null) {
                root.Add(config);
                config.Bind(new SerializedObject(target));
            }
            root.Add(InstallBtn);
            root.Add(ErrorList);
            ValidateAvatar();
            return root;
        }


        private bool ValidateAvatar() {
            List<validatorResponse> log = new List<validatorResponse>();

            if (ErrorList != null) ErrorList.Clear();
            if (ActiveDescriptor != null) {
                Animator animator = ActiveDescriptor.GetComponent<Animator>();
                if (animator == null) { log.Add(new validatorResponse("No animator found", "The selected object seems to not have an animator attached to it, make sure your avatar has a animator!", true)); }
                if (animator == null || (animator != null && !animator.isHuman)) { log.Add(new validatorResponse("Avatar is not human", "It appears the selected avatar is not humanoid, the dice prefab can currently not be installed on non-humanoid avatars yet, sorry for the inconveninece", true)); }
                if (ActiveDescriptor.expressionParameters == null) { log.Add(new validatorResponse("No expression parameters found", "This avatar seems to not have a expression parameter asset assigned in the descriptor.", true)); }
                if (ActiveDescriptor.expressionsMenu == null) { log.Add(new validatorResponse("No expression menu found", "This avatar seems to not have a expression menu asset assigned in the descriptor.", true)); }
            }

            template.ValidateConfig(log);

            bool isValid = true;
            foreach (validatorResponse response in log) {
                VisualElement notif = createNotification(response.name, response.desc);
                notif.AddToClassList(response.preventInstall ? "error" : "warning");
                if (ErrorList != null) ErrorList.Add(notif);
                if (response.preventInstall) isValid = false;
            }
            if (ActiveDescriptor == null) isValid = false;
            if (InstallBtn != null) InstallBtn.SetEnabled(isValid);
            return isValid;

        }


        private VisualElement createNotification(string name, string description) {
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

        private void BuildPrefab() {
            Animator animator = ActiveDescriptor.GetComponent<Animator>();
            List<string> metaTags = template.getConstraintMetaTags();
            GameObject Prefab = PrefabUtility.InstantiatePrefab(template.Prefab) as GameObject;
            Prefab.transform.parent = ActiveDescriptor.transform;

            List<ConstraintAssigner> assigners = new List<ConstraintAssigner>(Prefab.GetComponentsInChildren<ConstraintAssigner>());
            foreach (ConstraintAssigner assigner in assigners) {
                foreach (HumanBoneEntry hbe in assigner.Sources) {
                    IConstraint constraint = assigner.TargetConstraint as IConstraint;
                    Transform bone = animator.GetBoneTransform(hbe.targetBone);
                    if (assigner.Mode == ConstraintAssignerMode.All) {
                        constraint.AddSource(new ConstraintSource() { sourceTransform = bone, weight = hbe.weight });
                    }
                    else if(assigner.Mode == ConstraintAssignerMode.Meta) {
                        List<string> entryTags = new List<string>(hbe.Meta.Split(char.Parse(",")));
                        bool tagFound = false;
                        foreach(string tag in entryTags) {
                            if (metaTags.Contains(tag)) {
                                tagFound = true;
                            }
                        }
                        if (tagFound) {
                            constraint.AddSource(new ConstraintSource() { sourceTransform = bone, weight = hbe.weight });
                        }
                    }
                }
                DestroyImmediate(assigner);
            }
        }

    }
    public struct validatorResponse {
        public string name;
        public string desc;
        public bool preventInstall;
        public validatorResponse(string name, string desc, bool preventInstall) {
            this.name = name;
            this.desc = desc;
            this.preventInstall = preventInstall;
        }

    }
}