using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

namespace PFCTools2.Installer.PseudoParser {
    public class AnimatorLayerContext {

        public Dictionary<string, AnimatorState> states = new Dictionary<string, AnimatorState>();
        public AnimatorControllerLayer layer;
        public AnimatorState lastInteractedState;

        public AnimatorLayerContext(AnimatorControllerLayer layer) {
            foreach (ChildAnimatorState childState in layer.stateMachine.states) {
                states.Add(childState.state.name, childState.state);
            }
            this.layer = layer;
        }

        public AnimatorState GetState(string name) {
            if (states.ContainsKey(name)) {
                lastInteractedState = states[name];
                return states[name];
            }
            else {
                AnimatorState newState = layer.stateMachine.AddState(name);
                states.Add(name, newState);
                lastInteractedState = newState;
                return newState;

            }

        }
        public ChildAnimatorState GetChildState(AnimatorState state) {
            foreach (ChildAnimatorState childState in layer.stateMachine.states) {
                if (childState.state == state) {
                    return childState;
                }
            }

            throw new Exception("If you see this something went *REALLY* wrong, as it should be physically impossible to see this error. good luck o7");
        }
        public ChildAnimatorState GetChildState(string name) {
            AnimatorState state = GetState(name);
            return GetChildState(state);
        }
        public void SetStatePos(AnimatorState state, float x, float y) {
            ChildAnimatorState[] childStates = layer.stateMachine.states;
            for(int i = 0; i < childStates.Length; i++) {
                ChildAnimatorState childState = childStates[i];
                if(childState.state == state) {
                    childState.position = new StateOffset(x, y).position;
                    childStates[i] = childState;
                }   
            }
            layer.stateMachine.states = childStates;
        }

    }
}