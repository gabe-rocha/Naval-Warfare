using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraWaterEffects : MonoBehaviour {

#region Public Fields

#endregion

#region Private Serializable Fields
    [SerializeField] private Material skyboxAboveWater, skyboxUnderWater;
    [SerializeField] Image imgUnderwaterFogginess;

#endregion

#region Private Fields
#endregion

#region MonoBehaviour CallBacks
    void Awake() {

    }

    void Start() {

    }

    void Update() {
        if(Camera.main.transform.position.y < 1f) {
            RenderSettings.skybox = skyboxUnderWater;
            imgUnderwaterFogginess.enabled = true;
        } else {
            RenderSettings.skybox = skyboxAboveWater;
            imgUnderwaterFogginess.enabled = false;
        }
    }
#endregion

#region Private Methods

#endregion

#region Public Methods

#endregion
}