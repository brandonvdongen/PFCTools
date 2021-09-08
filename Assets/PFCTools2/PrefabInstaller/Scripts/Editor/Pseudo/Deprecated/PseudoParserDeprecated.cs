using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Text.RegularExpressions;

namespace PFCTools2.Installer.PseudoParser {
    public static partial class PseudoParserDeprecated {

        private class TypeQueues {
            public static LexType[] None = new[] { LexType.Action };
            public static LexType[] Layer = new[] { LexType.String };
            public static LexType[] Entry = new[] { LexType.String };
            public static LexType[] Transition = new[] { LexType.String, LexType.Operator, LexType.String, LexType.Operator };
            public static LexType[] Condtion = new[] { LexType.String, LexType.Comp, LexType.Value };
        }
        private class LexEntry {
            public string value;
            public LexType type;
            public int line;
            public LexEntry(string part, LexType type, int line) {
                this.value = part;
                this.type = type;
                this.line = line;
            }
        }


        private class ParseStream {
            int index = 0;
            List<LexEntry> _input;
            public ParseStream(TextAsset file) {
                //_input = LexFile(file);
            }
            public LexEntry Next() {
                return _input[index++];
            }
            public LexEntry Peek() {
                return _input[index + 1];
            }
            public bool EOF() {
                return Peek() == null;
            }
            public void Exception() {
                throw new Exception("Unexpected " + _input[index].type + " \"" + _input[index].value + "\", on Line : " + _input[index].line);
            }

        }


       

        public static void Parse(TextAsset file) {

            List<LexEntry> layers = new List<LexEntry>();
            List<LexEntry> states = new List<LexEntry>();
            List<LexEntry> entry = new List<LexEntry>();
            List<LexEntry> transitions = new List<LexEntry>();


            var parseStream = new ParseStream(file);
            while (!parseStream.EOF()) {
                LexType expected = LexType.Action;
                LexEntry part = parseStream.Next();
                if ((part.type & expected) == LexType.Mismatch) {
                    parseStream.Exception();
                }

                if(part.value.ToLower() == "layer") {
                    Queue<LexType> queue = new Queue<LexType>(TypeQueues.Layer);
                    List<LexEntry> commandLine = new List<LexEntry>();
                    while (queue.Count > 0) {
                        expected = queue.Dequeue();
                        part = parseStream.Next();
                        if ((part.type & expected) == LexType.Mismatch) {
                            parseStream.Exception();
                        }
                        else {

                        }
                    }

                }
                if(part.value.ToLower() == "entry") {}
                if(part.value.ToLower() == "transition") {}
                
            }

        }
    }
}
/*
    AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(directory + "/" + file.name + "_controller.controller");
    Dictionary<string, AnimatorState> states = new Dictionary<string, AnimatorState>();
    Dictionary<string, AnimatorControllerParameter> parameters = new Dictionary<string, AnimatorControllerParameter>();

    foreach (AnimatorControllerParameter param in controller.parameters) {
        parameters.Add(param.name, param);
    }

    AnimatorStateMachine currentLayer = controller.layers[0].stateMachine;
    AnimatorStateTransition lastTransition = null;

    AnimatorState getState(string name) {
        if (states.ContainsKey(name)) {
            return states[name];
        }
        else {
            AnimatorState newState = currentLayer.AddState(name);
            states.Add(name, newState);
            return newState;
        }
    }


    ParseStream<LexEntry> parseQueue = new ParseStream<LexEntry>(lex);
    LexType expected = LexType.Action;
    while(!parseQueue.EOF()) {
        var part = parseQueue.Next();
        if ((part.type & expected) == LexType.Mismatch) {
        }
    }



    /*
    while (partQueue.Count > 0) {
        Queue<LexType> expectedTypeQueue = new Queue<LexType>(TypeQueues.None);
        Queue<LexEntry> commandLine = new Queue<LexEntry>();
        while (expectedTypeQueue.Count > 0) {

            LexEntry part = partQueue.Dequeue();
            LexType expected = expectedTypeQueue.Dequeue();

            //Debug.Log(part.value);
            if ((part.type & expected) == LexType.Mismatch) {
                throw new Exception("Unexpected " + part.type + " \"" + part.value + "\", Expected " + expected + " on Line : " + lineDirectory[part] + " in " + filePath);
            }

            else if((part.type & LexType.Action) != LexType.Mismatch) {
                if (part.value.ToLower() == "layer") { expectedTypeQueue = new Queue<LexType>(TypeQueues.Layer); }

                }
            else {
                commandLine.Enqueue(part);
            }
        }
        foreach(var line in commandLine) {
            Debug.Log(line.value);
        }
        /*else if (part.value.ToLower() == "layer") {
            controller.AddLayer(part.value);
            currentLayer = controller.layers[controller.layers.Length - 1].stateMachine;
        }
        else if (part.value.ToLower() == "entry") { }
        else if (part.value.ToLower() == "transition") { }

    }
}
    */
/*
for (int i = 0; i < lex.Count; i++) {
    LexEntry entry = lex[i];
    //if (expectedTypeQueue.Count <= 0) { expectedTypeQueue = new Queue<LexType>(TypeQueues.None); parsing = ParseMode.None; };
    string part = entry.part;
    LexType partType = entry.type;
    //Debug.Log("ExpectedQueue:" + string.Join(",", expectedTypeQueue));
    Debug.Log("Lex:" + entry.part + "|[" + entry.type + "]");
    if(expectedTypeQueue.Count > 0)expected = expectedTypeQueue.Dequeue();
    if ((partType & expected) == 0) {
        throw new Exception("Unexpected " + partType + " \"" + part + "\", Expected " + expected + " on Line : " + lineDirectory[entry] + " in " + filePath);
    }

    if (parsing == ParseMode.None) {
        if (partType == LexType.Action) {
            if (part.ToLower() == "layer") { parsing = ParseMode.Layer; expectedTypeQueue = new Queue<LexType>(TypeQueues.Layer); }
            if (part.ToLower() == "entry") { parsing = ParseMode.Entry; expectedTypeQueue = new Queue<LexType>(TypeQueues.Entry); }
            if (part.ToLower() == "transition") { parsing = ParseMode.Transition; expectedTypeQueue = new Queue<LexType>(TypeQueues.Transition); }
            continue;
        }
    }

    if(parsing == ParseMode.Layer) {
        controller.AddLayer(part);
        currentLayer = controller.layers[controller.layers.Length - 1].stateMachine;
    }
    else if(parsing == ParseMode.Entry) {
        currentLayer.defaultState = getState(part);
    }
    else if(parsing == ParseMode.Transition) {


    }
}        
*/
/*
if (parts.Count <= 0) continue;
if (parts[0] == "layer") {
    states = new Dictionary<string, AnimatorState>();
    AnimatorControllerLayer newLayer = new AnimatorControllerLayer();
    newLayer.stateMachine = new AnimatorStateMachine();
    newLayer.name = parts[1];
    newLayer.defaultWeight = 1;

    controller.AddLayer(newLayer);

    currentLayer = newLayer.stateMachine;

}
else if (parts[0] == "entry") {
    AnimatorState state;
    if (states.ContainsKey(parts[1])) state = states[parts[0]];
    else {
        state = currentLayer.AddState(parts[1]);
        states.Add(parts[1], state);
    }
    currentLayer.defaultState = state;
}
else if (parts[0] == "goto") {
    string startStateName = parts[1];
    string endStateName = parts[3];
    lastTransition = getState(string.Join(" ", startStateName)).AddTransition(getState(string.Join(" ", endStateName)));
    lastTransition.duration = 0;

}
else if (parts[0] == "when") {
    int i = 1;
    while (i < parts.Count) {
        AnimatorControllerParameterType paramType = AnimatorControllerParameterType.Bool;
        if (parts[i] == "int") paramType = AnimatorControllerParameterType.Int;
        if (parts[i] == "float") paramType = AnimatorControllerParameterType.Float;

        string paramName = parts[i + 1];
        string paramCondition = parts[i + 2];
        string paramValue = parts[i + 3];

        if (!parameters.ContainsKey(paramName)) {
            controller.AddParameter(paramName, paramType);
            parameters.Add(paramName, controller.parameters[controller.parameters.Length - 1]);
        }

        if (paramType == AnimatorControllerParameterType.Bool) {
            if (paramCondition == "==") {
                if (paramValue.ToLower() == "true") lastTransition.AddCondition(AnimatorConditionMode.If, 0, paramName);
                if (paramValue.ToLower() == "false") lastTransition.AddCondition(AnimatorConditionMode.IfNot, 0, paramName);
            }
        }
        else {
            Debug.Log(paramValue);
            AnimatorConditionMode conditionMode = AnimatorConditionMode.Greater;
            if (paramCondition == "<") conditionMode = AnimatorConditionMode.Less;
            if (paramCondition == "==") conditionMode = AnimatorConditionMode.Equals;
            if (paramCondition == "!=") conditionMode = AnimatorConditionMode.NotEqual;
            lastTransition.AddCondition(conditionMode, float.Parse(paramValue), paramName);
        }
        i += 5;
    }
}
else if (parts[0] == "duration") {
    if (parts[1] == "fixed") {
        lastTransition.hasFixedDuration = true;
        lastTransition.duration = float.Parse(parts[2]);
    }
    else {
        lastTransition.duration = float.Parse(parts[1]);
    }
}
else if (parts[0] == "exit") {
    lastTransition.exitTime = float.Parse(parts[1]);
    lastTransition.hasExitTime = true;
}
*/