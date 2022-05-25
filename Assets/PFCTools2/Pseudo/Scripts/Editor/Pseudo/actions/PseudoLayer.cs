using PFCTools2.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace PFCTools2.Installer.PseudoParser
{
    public class PseudoLayer : PseudoAction
    {

        public override string ActionKey => "layer";
        public override ControllerContext Process(ControllerContext context, TokenStream tokenStream, AvatarDefinition currentAvatar)
        {
            Token token = tokenStream.Next();
            if ((token.type & TokenType.String) != TokenType.String)
            {
                tokenStream.Exception();
            }
            else
            {
                AnimatorControllerLayer newLayer = new AnimatorControllerLayer();
                newLayer.name = context.Controller.MakeUniqueLayerName(token.value);
                newLayer.stateMachine = new AnimatorStateMachine();
                newLayer.stateMachine.name = newLayer.name;
                newLayer.stateMachine.hideFlags = HideFlags.HideInHierarchy;
                newLayer.defaultWeight = 1;
                if (AssetDatabase.GetAssetPath(context.Controller) != "")
                {
                    AssetDatabase.AddObjectToAsset(newLayer.stateMachine, AssetDatabase.GetAssetPath(context.Controller));
                }

                context.Controller.AddLayer(newLayer);
                context.layers.Add(new AnimatorLayerContext(newLayer));
                context.activeLayer = context.Controller.layers.Length - 1;
            }
            return context;
        }

        public override ControllerContext Remove(ControllerContext context, TokenStream tokenStream, AvatarDefinition currentAvatar)
        {
            while (!tokenStream.EOF() && tokenStream.Peek().type != TokenType.Action)
            {
                Token token = tokenStream.Next();

                if (token.IsType(TokenType.String))
                {
                    int lastLayerIndex = -1;
                    for (int i = 0; i < context.Controller.layers.Length; i++)
                    {
                        if (context.Controller.layers[i].name.Contains(token.value))
                        {
                            Debug.Log($"Found: {context.Controller.layers[i].name}");
                            lastLayerIndex = i;
                        }
                    }
                    if (lastLayerIndex != -1)
                    {
                        context.Controller.RemoveLayer(lastLayerIndex);
                        Debug.Log("Deleting");
                    }

                    //tokenStream.Exception();
                }
            }
            return context;
        }
    }
}