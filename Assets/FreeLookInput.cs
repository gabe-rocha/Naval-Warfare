using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CinemachineFreeLook))]
public class FreeLookInput : MonoBehaviour {

#region Public Fields

#endregion

#region Private Serializable Fields

#endregion

#region Private Fields
    private CinemachineFreeLook freeLookCamera;

    private string XAxisName = "Mouse X";
    private string YAxisName = "Mouse Y";

#endregion

#region MonoBehaviour CallBacks
    void Awake() {
        freeLookCamera = GetComponent<CinemachineFreeLook>();
    }

    void Start() {
        freeLookCamera.m_XAxis.m_InputAxisName = "";
        freeLookCamera.m_YAxis.m_InputAxisName = "";
    }

    void Update() {
        if(GameManager.Instance.gameState != GameManager.GameStates.Playing) {
            return;
        }

        if(Input.GetMouseButton(1)) {
            freeLookCamera.m_XAxis.m_InputAxisValue = Input.GetAxis(XAxisName);
            freeLookCamera.m_YAxis.m_InputAxisValue = Input.GetAxis(YAxisName);
        } else {
            freeLookCamera.m_XAxis.m_InputAxisValue = 0;
            freeLookCamera.m_YAxis.m_InputAxisValue = 0;
        }
    }
#endregion

#region Private Methods

#endregion

#region Public Methods

#endregion
}