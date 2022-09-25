using UnityEditor;
using UnityEngine.UIElements;

namespace PFCTools2.Installer.Core
{

    [CustomEditor(typeof(PrefabTemplate), true)]
    public class PrefabTemplateEditor : Editor
    {
        private PrefabTemplate template;
        public override VisualElement CreateInspectorGUI()
        {
            template = target as PrefabTemplate;
            Button OpenInstaller = new Button() { text = $"Open {template.PrefabName} Installer" };
            OpenInstaller.clicked += () => { PrefabTemplate.OpenAssetEditor(template); };

            return OpenInstaller;
        }
    }

}