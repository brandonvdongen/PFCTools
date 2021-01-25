using UnityEngine;
using UnityEditor;

namespace PFCTools.EditorTools {
    public class UIUtils {

        public static void HorizontalLine() { 
        Rect rect = EditorGUILayout.GetControlRect(false, 0);
            rect.height = 0.5f;
            rect.width = rect.width + (rect.position.x*2+1);
            rect.position = new Vector2(0, rect.position.y+1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }
    }
}