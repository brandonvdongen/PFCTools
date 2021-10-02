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

        public static TextAsset CreateNewTextFile(Object pathReference, string fileName, string content, bool replace = false) {
            string path = AssetDatabase.GetAssetPath(pathReference);
            string directory = Path.GetDirectoryName(path);
            string newPath = directory + "/" + fileName;
            return CreateNewTextFile(newPath, content, replace);
        }

        public static TextAsset CreateNewTextFile(string path, string content, bool replace = false) {
            string newPath;
            if (replace) {
                newPath = path;
            }
            else {
                newPath = AssetDatabase.GenerateUniqueAssetPath(path);
            }
            File.WriteAllText(newPath, content);
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<TextAsset>(newPath);
        }
    }
}