using UnityEngine;
using UnityEditor;
using System.IO;

namespace PFCTools2.Utils {
    public class ScriptableObjectUtils {

		public static T CreateAsset<T>(string name) where T : ScriptableObject {
			return CreateAsset<T>(name, "");
        }
			public static T CreateAsset<T>(string name, string path) where T : ScriptableObject {
			T asset = ScriptableObject.CreateInstance<T>();

			if (path == "") {
				path = "Assets";
			}
			//else if (Path.GetExtension(path) != "") {
			//path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			//}
			Debug.Log(path);
			Directory.CreateDirectory(path);
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".asset");
			AssetDatabase.CreateAsset(asset, assetPathAndName);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = asset;
			return asset;
		}

	}
}