using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fpslimiter : MonoBehaviour
{
    [Range(30,90)]
    public int target = 30;

    void Awake() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = target;
    }

    void Update() {
        if (Application.targetFrameRate != target)
            Application.targetFrameRate = target;
    }
}
