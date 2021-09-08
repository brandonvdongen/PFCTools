using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace PFCTools2.Installer.PseudoParser {
    public class AnimatorLayerContext {

        public Dictionary<string, AnimatorState> states = new Dictionary<string, AnimatorState>();
        public AnimatorControllerLayer layer;

        public AnimatorLayerContext(AnimatorControllerLayer layer) {
            foreach (ChildAnimatorState childState in layer.stateMachine.states) {
                AnimatorState state = childState.state;
                states.Add(state.name, state);
                
            }
            this.layer = layer;
        }

        public AnimatorState GetState(string name) {
            if (states.ContainsKey(name)) {
                return states[name];
            }
            else {
                AnimatorState newState = layer.stateMachine.AddState(name);
                states.Add(name, newState);
                return newState;

            }

        }

    }
}