using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PFCTools2.Installer.PseudoParser {


    public struct StateOffset {
        public Vector3 position;
        public Vector3 reverse;
        public StateOffset (float x, float y, float z = 0) {
            position = new Vector3(x * 210, -y * 50,z);
            reverse = new Vector3(x / 210, -y / 50,z);
        }
    }

    public struct SmallStateOffset {
        public Vector3 position;
        public Vector3 reverse;
        public SmallStateOffset(float x, float y, float z = 0) {
            position = new Vector3(20 + (x * 210), 5 + (-y * 50), z);
            reverse = new Vector3(20 + (x / 210), 5 + (-y / 50), z);
        }
    }
     
}