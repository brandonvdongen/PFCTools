using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace PFCTools2.Installer.PseudoParser {
    public class ControllerContext {

        public AnimatorController Controller { get; private set; }
        public int activeLayer = 0;
        public List<AnimatorLayerContext> layers = new List<AnimatorLayerContext>();
        public Dictionary<string, AnimatorControllerParameter> parameters = new Dictionary<string, AnimatorControllerParameter>();

        public ControllerContext(string path) {
            string directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileNameWithoutExtension(path);
            this.Controller = AnimatorController.CreateAnimatorControllerAtPath(directory + "/" + fileName + ".controller");
            this.Controller.RemoveLayer(0);
            RefreshControllerData();
        }
        public ControllerContext(AnimatorController Controller) {
            this.Controller = Controller;
            RefreshControllerData();
        }

        private void RefreshControllerData() {
            layers.Clear();
            parameters.Clear();
            foreach (var layer in Controller.layers) {
                layers.Add(new AnimatorLayerContext(layer));
            }
            foreach (var param in Controller.parameters) {
                parameters.Add(param.name, param);
            }
        }
        public AnimatorControllerParameter GetParameter(string name, AnimatorControllerParameterType type) {
            if (parameters.ContainsKey(name)) {
                return parameters[name];
            }
            else {
                AnimatorControllerParameter newParameter = new AnimatorControllerParameter() {  type = type, name = name };
                Controller.AddParameter(newParameter);
                parameters.Add(name, newParameter);
                return newParameter;
            }

        }
        public AnimatorControllerParameterType GetParameterType(string name) {
            if (parameters.ContainsKey(name)) {
                return parameters[name].type;
            }
            return new AnimatorControllerParameterType();
        }
    }
}