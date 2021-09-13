using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class CameraController : MonoBehaviour {

#region Public Fields

#endregion

#region Private Serializable Fields

#endregion

#region Private Fields
    private Animator animCameraStateDriven;
#endregion

#region MonoBehaviour CallBacks
    void Awake() {
        animCameraStateDriven = GetComponent<Animator>();
    }

    void Start() {

    }

    void Update() {
        if(GameManager.Instance.gameState != GameManager.GameStates.Playing) {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Alpha1)) {
            animCameraStateDriven.SetTrigger("Player FreeLook");
        } else if(Input.GetKeyDown(KeyCode.Alpha2)) {
            animCameraStateDriven.SetTrigger("Camera2");
        } else if(Input.GetKeyDown(KeyCode.Alpha3)) {
            animCameraStateDriven.SetTrigger("Camera3");
        }
    }
#endregion

#region Private Methods

#endregion

#region Public Methods

#endregion
}