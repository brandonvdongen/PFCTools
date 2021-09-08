using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PFCTools2.Installer.PseudoParser {
    public static partial class PseudoParserDeprecated {
        [Flags]
        private enum LexType {
            Mismatch = 0,
            Action = 1 << 1,
            Comp = 1 << 2,
            Operator = 1 << 3,
            String = 1 << 4,
            Float = 1 << 5,
            Int = 1 << 6,
            Bool = 1 << 7,
            Value = 1 << 8,
            Optional = 1 << 9,
            Comment = 1 << 10,
            Unkown = 1 << 11,
            Skip = 1 << 12,
        }
    }
}