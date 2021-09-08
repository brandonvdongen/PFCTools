using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PFCTools2.Utils {

    public static class FileHelper {

        public static TextAsset CreateNewTextFile(string content) {
            return CreateNewTextFile("Assets/newFile.txt", content);
        }

        public static TextAsset CreateNewTextFile(Object pathReference, string fileName, string content) {
            string path = AssetDatabase.GetAssetPath(pathReference);
            string directory = Path.GetDirectoryName(path);
            string newPath = AssetDatabase.GenerateUniqueAssetPath(directory + "/" + fileName);
            return CreateNewTextFile(newPath, content);
        }

        public static TextAsset CreateNewTextFile(string path, string content) {
            string newPath = AssetDatabase.GenerateUniqueAssetPath(path);
            File.WriteAllText(newPath, "");
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<TextAsset>(newPath);
        }
    }
}