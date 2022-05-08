using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace PFCTools2.CleanupTools {
    public class FolderCleaner {
        [MenuItem("PFCTools2/Cleanup/Remove All Empty Folders")]
        public static void removeAll() {
            removeEmptyFolders();
        }

        public static void removeEmptyFolders(string rootPath = "assets") {
            int lockout = 10000000;
            List<string> deletable = new List<string>();
            List<FolderInfo> folders = new List<FolderInfo>(); 

            expandToFolderBranchEnds(folders, rootPath);

            //folders.Sort((a, b) => { return a.path.Split('/').Length.CompareTo(b.path.Split('/').Length); });

            try {
                float i = 1;
                foreach (FolderInfo folder in folders) {
                    bool cancel = EditorUtility.DisplayCancelableProgressBar("Deleting Empty Folders", folder.path, i / folders.Count);

                    if (folder.IsEmpty) {
                        //AssetDatabase.MoveAssetToTrash(childInfo.path);
                        Directory.Delete(folder.path);
                        File.Delete(folder.path + ".meta");
                    }

                    FolderInfo _folderInfo = folder;
                    string pname = _folderInfo.parent == null ? "null" : _folderInfo.parent.Name;
                    while (_folderInfo.parent != null) {
                        _folderInfo = _folderInfo.parent;
                        Debug.Log($"check:{_folderInfo.path}");
                        if (_folderInfo.IsEmpty) {
                            //AssetDatabase.MoveAssetToTrash(childInfo.path);
                            Directory.Delete(_folderInfo.path);
                            File.Delete(_folderInfo.path + ".meta");
                            Debug.Log($"Delete:{_folderInfo.path}");
                        }


                        //Debug.Log($"{childInfo.name} > {pname}");
                        lockout--;
                        if (lockout <= 0) {
                            Debug.LogError("You fucked up bruh");
                            break;
                        }
                        if (cancel) break;
                    }
                    if (cancel) break;
                    i++;
                }
            }
            catch {
                EditorUtility.ClearProgressBar();
            }
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }
        private static List<FolderInfo> expandToFolderBranchEnds(List<FolderInfo> folders, string path, FolderInfo _parent = null) {
            foreach (string folder in AssetDatabase.GetSubFolders(path)) {
                FolderInfo info = new FolderInfo(folder) { parent = _parent };
                if (info.HasSubFolders) {
                    expandToFolderBranchEnds(folders, info.path, info);
                }
                else {
                    folders.Add(info);
                }
            }
            return folders;
        }

        private class FolderInfo {
            public FolderInfo parent;
            public string path;
            public string Name { get { return Path.GetFileName(path); } }
            public bool HasSubFolders { get { return Directory.GetDirectories(path).Length > 0; } }
            public bool HasFiles { get { return Directory.GetFiles(path).Length > 0; } }
            public bool IsEmpty { get { return !HasSubFolders && !HasFiles; } }

            public FolderInfo(string path) {
                this.path = path;
            }
        }
    }
}