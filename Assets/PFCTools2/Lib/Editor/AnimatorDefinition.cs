using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace PFCTools2.Utils {
    public class AnimatorDefinition {

        public List<parameterDefinition> parameters = new List<parameterDefinition>();
        public List<layerDefinition> layers = new List<layerDefinition>();

        public AnimatorDefinition(AnimatorController controller) {
            
            foreach(AnimatorControllerParameter param in controller.parameters){
                parameterDefinition def = new parameterDefinition(param);
                parameters.Add(def);
            }

            foreach(var p in parameters) {
                Debug.Log(p.name);
            }

        }
    }

    public class layerDefinition {
    }

    public class parameterDefinition {
        public string name;
        public AnimatorControllerParameterType type;
        public bool defaultBool;
        public int defaultInt;
        public float defaultFloat;

        public parameterDefinition(AnimatorControllerParameter param) {
            name = param.name;
            type = param.type;
            defaultBool = param.defaultBool;
            defaultInt = param.defaultInt;
            defaultFloat = param.defaultFloat;
        }
    }
}