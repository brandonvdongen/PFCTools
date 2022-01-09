using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;
#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
#endif

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoParameter : PseudoAction {
        public override string ActionKey => "parameter";
        public override ControllerContext Process(ControllerContext Context, TokenStream Tokens) {
            Token nametoken = Tokens.Next(TokenType.String);
            Token valueToken = Tokens.Next(TokenType.String);
            if (valueToken.value == "float") {
                Context.GetParameter(nametoken.value, AnimatorControllerParameterType.Float);
            }
            else if (valueToken.value == "int") {
                Context.GetParameter(nametoken.value, AnimatorControllerParameterType.Int);
            }
            else if (valueToken.value == "bool") {
                Context.GetParameter(nametoken.value, AnimatorControllerParameterType.Bool);
            }

                return Context;
        }
    }
}