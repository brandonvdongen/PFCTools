using PFCTools2.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace PFCTools2.Installer.PseudoParser
{


    public partial class Pseudo
    {

        [MenuItem("Help/Clear Progress Bar")]
        public static void ClearProgressBar()
        {
            EditorUtility.ClearProgressBar();
        }


        public static List<PseudoAction> EnabledActions = new List<PseudoAction> { new PseudoLayer(), new PseudoEntry(), new PseudoTransition(), new PseudoState(), new PseudoAny(), new PseudoEntry(), new PseudoExit(), new PseudoParameter() };
        public static List<PseudoAction> EnabledVRCActions = new List<PseudoAction> { new PseudoVRCParameterDriver() };
        public static List<PseudoAction> getEnabledActions()
        {
            List<PseudoAction> availableActions = new List<PseudoAction>();
            availableActions.AddRange(EnabledActions);
            availableActions.AddRange(EnabledVRCActions);
            return availableActions;
        }
        public static void Parse(TextAsset asset, AvatarDefinition currentAvatar, AnimatorController controller = null)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            Parse(Lexxer(asset), path, currentAvatar, controller);
        }
        public static void Parse(List<Token> tokenList, string path, AvatarDefinition currentAvatar, AnimatorController controller = null)
        {
            TokenStream Tokens = new TokenStream(tokenList);
            ControllerContext controllerContext;
            if (controller)
            {
                controllerContext = new ControllerContext(controller);
            }
            else
            {
                controllerContext = new ControllerContext(path);
            }
            while (!Tokens.EOF())
            {
                Token token = Tokens.Next();
                if ((token.type & TokenType.Action) == TokenType.Mismatch)
                {
                    Tokens.Exception();
                }

                bool foundAction = false;
                foreach (PseudoAction action in getEnabledActions())
                {
                    if (token.value == action.ActionKey)
                    {
                        controllerContext = action.Process(controllerContext, Tokens, currentAvatar);
                        foundAction = true;
                        break;
                    }
                }
                if (!foundAction)
                {
                    Tokens.Exception();
                }

                if (controller)
                {
                    EditorUtility.SetDirty(controller);
                }
            }

        }

        public static void Remove(TextAsset asset, AvatarDefinition currentAvatar, AnimatorController controller)
        {
            string path = AssetDatabase.GetAssetPath(asset);
            Remove(Lexxer(asset), path, currentAvatar, controller);
        }
        public static void Remove(List<Token> tokenList, string path, AvatarDefinition currentAvatar, AnimatorController controller)
        {
            TokenStream Tokens = new TokenStream(tokenList);
            ControllerContext controllerContext;
            if (controller == null)
            {
                throw new Exception("No controller was given, aborting.");
            }
            else
            {
                controllerContext = new ControllerContext(controller);
            }
            while (!Tokens.EOF())
            {
                Token token = Tokens.Next();
                if ((token.type & TokenType.Action) == TokenType.Mismatch)
                {
                    Tokens.Exception();
                }

                bool foundAction = false;
                foreach (PseudoAction action in getEnabledActions())
                {
                    if (token.value == action.ActionKey)
                    {
                        controllerContext = action.Remove(controllerContext, Tokens, currentAvatar);
                        foundAction = true;
                        break;
                    }
                }
                if (!foundAction)
                {
                    Tokens.Exception();
                }

                if (controller)
                {
                    EditorUtility.SetDirty(controller);
                }
            }

        }

        /*
        public static void RemoveLayers(TextAsset asset, AvatarDefinition currentAvatar, AnimatorController controller) {
            string path = AssetDatabase.GetAssetPath(asset);
            RemoveLayers(Lexxer(asset), path, currentAvatar, controller);
        }
        public static void RemoveLayers(List<Token> tokenList, string path, AvatarDefinition currentAvatar, AnimatorController controller) {
            string layerKey = new PseudoLayer().ActionKey;
            string parameterKey = new PseudoParameter().ActionKey;

            TokenStream Tokens = new TokenStream(tokenList);
            while (!Tokens.EOF()) {
                Token token = Tokens.Next();
                if (token.value == layerKey) {
                    string layerName = Tokens.Next().value;
                    Debug.Log("Found layer: "+ layerName);

                    for (var i = 0; i < controller.layers.Length; i++) {
                        if(controller.layers[i].name == layerName) {
                            controller.RemoveLayer(i);
                            EditorUtility.SetDirty(controller);
                            break;
                        }
                    }

                }
                else if(token.value == parameterKey) {
                    string parameterName = Tokens.Next().value;
                    Debug.Log("Found parameter: " + parameterName + " in animator, removing...");

                    for(var i = 0; i < controller.parameters.Length; i++) {
                        if(controller.parameters[i].name == parameterName) {
                            controller.RemoveParameter(i);
                            EditorUtility.SetDirty(controller);

                            List<VRCExpressionParameters.Parameter> parameters = new List<VRCExpressionParameters.Parameter>(currentAvatar.ExpressionParameters.parameters);
                            VRCExpressionParameters.Parameter param = currentAvatar.ExpressionParameters.FindParameter(parameterName);
                            if (param != null) {
                                Debug.Log("Found Parameter: " + parameterName + " in expression parameters, removing...");
                                parameters.Remove(param);
                                currentAvatar.ExpressionParameters.parameters = parameters.ToArray();
                                EditorUtility.SetDirty(currentAvatar.ExpressionParameters);
                            }
                            break;
                        }
                        
                    }
                }
                
                
                

            }

        }
        */

    }
}