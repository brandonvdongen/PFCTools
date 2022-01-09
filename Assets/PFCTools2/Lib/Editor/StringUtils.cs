using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace PFCTools2.Utils {
    public class StringUtils {
        internal static string parseQuotes(string text) {
            if (text.Contains(" ")) {
                return "\"" + text + "\"";
            }
            else {
                return text;
            }
        }
        internal static string parseComparator(AnimatorConditionMode mode) {
            if (mode == AnimatorConditionMode.Equals)return "==";
            if (mode == AnimatorConditionMode.NotEqual) return "!=";
            if (mode == AnimatorConditionMode.Greater) return ">";
            if (mode == AnimatorConditionMode.Less) return "<";
            if (mode == AnimatorConditionMode.If) return "== true";
            if (mode == AnimatorConditionMode.IfNot) return "== false";
            return "==";
        }
    }
}