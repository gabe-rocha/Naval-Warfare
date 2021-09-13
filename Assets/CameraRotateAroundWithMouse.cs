using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraRotateAroundWithMouse : MonoBehaviour {

#region Public Fields

#endregion

#region Private Serializable Fields

#endregion

#region Private Fields
    private CinemachineVirtualCamera vcam;
    private Transform lookAtTarget;
#endregion

#region MonoBehaviour CallBacks
    void Awake() {
        vcam = GetComponent<CinemachineVirtualCamera>();
        if(vcam == null) {
            Debug.LogError($"{name} is missing component CinemachineVirtualCamera");
        }

        lookAtTarget = vcam.LookAt;

    }

    void Start() {

    }

    void Update() {
        if(GameManager.Instance.gameState != GameManager.GameStates.Playing) {
            return;
        }

        if(Input.GetMouseButton(1)) { }

    }
#endregion

#region Private Methods

#endregion

#region Public Methods

#endregion
}