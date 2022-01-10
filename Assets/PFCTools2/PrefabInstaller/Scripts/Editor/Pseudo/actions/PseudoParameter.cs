using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEditor;
using PFCTools2.Utils;
#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

namespace PFCTools2.Installer.PseudoParser {
    public class PseudoParameter : PseudoAction {
        public override string ActionKey => "parameter";
        public override ControllerContext Process(ControllerContext context, TokenStream tokenStream, AvatarDefinition currentAvatar) {
            Token nametoken = tokenStream.Next(TokenType.String);
            Token valueToken = tokenStream.Next(TokenType.String);
            Token syncToken = tokenStream.Peek();

            Debug.Log(currentAvatar.ExpressionParameters.name);

            List<VRCExpressionParameters.Parameter> parameters = new List<VRCExpressionParameters.Parameter>();
            if (currentAvatar != null) parameters = new List<VRCExpressionParameters.Parameter>(currentAvatar.ExpressionParameters.parameters);
            VRCExpressionParameters.Parameter newParam = new VRCExpressionParameters.Parameter();

            if (valueToken.value == "float") {
                context.GetParameter(nametoken.value, AnimatorControllerParameterType.Float);
                newParam.valueType = VRCExpressionParameters.ValueType.Float;
            }
            else if (valueToken.value == "int") {
                context.GetParameter(nametoken.value, AnimatorControllerParameterType.Int);
                newParam.valueType = VRCExpressionParameters.ValueType.Int;
            }
            else if (valueToken.value == "bool") {
                context.GetParameter(nametoken.value, AnimatorControllerParameterType.Bool);
                newParam.valueType = VRCExpressionParameters.ValueType.Bool;
            }
            if (syncToken.value == "sync" && currentAvatar != null) {
                tokenStream.Next();
                if (currentAvatar.ExpressionParameters.FindParameter(nametoken.value) == null) {
                    newParam.name = nametoken.value;
                    parameters.Add(newParam);
                    currentAvatar.ExpressionParameters.parameters = parameters.ToArray();
                    EditorUtility.SetDirty(currentAvatar.ExpressionParameters);
                }
            }
            return context;
        }

        public override ControllerContext Remove(ControllerContext context, TokenStream tokenStream, AvatarDefinition currentAvatar) {
            Token nameToken = tokenStream.Next(TokenType.String);//nameToken
            Token valueToken = tokenStream.Next(TokenType.String);//ValueToken
            Token syncToken = tokenStream.Peek();

            AnimatorControllerParameter foundParameter = null;
            foreach (AnimatorControllerParameter parameter in context.Controller.parameters) {
                if(parameter.name == nameToken.value) {
                    foundParameter = parameter;
                    break;
                }
            }
            if(foundParameter != null) {
                context.Controller.RemoveParameter(foundParameter);
            }
            

            if (syncToken.value == "sync") {
                tokenStream.Next();

                VRCExpressionParameters.Parameter parameter = currentAvatar.ExpressionParameters.FindParameter(nameToken.value);
                if(parameter != null) { 

                    List<VRCExpressionParameters.Parameter> parameters = new List<VRCExpressionParameters.Parameter>(currentAvatar.ExpressionParameters.parameters);
                    parameters.Remove(parameter);
                    currentAvatar.ExpressionParameters.parameters = parameters.ToArray();
                    EditorUtility.SetDirty(currentAvatar.ExpressionParameters);
                }

            }
            return context;
        }
    }
}