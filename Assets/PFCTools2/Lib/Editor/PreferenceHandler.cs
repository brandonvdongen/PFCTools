using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PFCTools2.Utils {

    [InitializeOnLoad]
    public abstract class PreferenceHandler {
        public static Dictionary<string, PreferenceHandler> Preferences = new Dictionary<string, PreferenceHandler>();

        internal const string PREF_PREFIX = "PFCTOOLS2PREF";
        public string path = "pfc.unassigned";
        public string name = "pfc.unnamed";

        public PreferenceHandler(string SettingName, string SettingPath) {
            this.path = SettingPath;
            this.name = SettingName;
            Preferences.Add(SettingPath,this);
        }
    }
    public class BoolPreferenceHandler : PreferenceHandler {

        public bool cachedValue = false;
        public bool defaultValue = false;

        public BoolPreferenceHandler(string SettingName, string SettingPath, bool defaultValue = true) : base(SettingName, SettingPath) {
            this.defaultValue = defaultValue;
        }

        public bool IsEnabled {
            get { bool val = EditorPrefs.GetBool(PREF_PREFIX + path, defaultValue); cachedValue = val; return val; }
            set { EditorPrefs.SetBool(PREF_PREFIX+path, value); cachedValue = value; }
        }

        public void Toggle() {
            IsEnabled = !IsEnabled;
        }
    }
}