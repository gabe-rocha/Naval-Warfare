using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionGenerator : MonoBehaviour {

#region Public Fields

#endregion

#region Private Serializable Fields

#endregion

#region Private Fields

#endregion

#region MonoBehaviour CallBacks
    void Awake() {

    }

    void Start() {

    }

    void Update() {

    }
#endregion

#region Private Methods

#endregion

#region Public Methods
    public void OnButtonPlayPressed() {
        SceneManager.LoadScene("Sandbox");
    }

    public void OnButtonCancelPressed() {
        SceneManager.LoadScene("Main Menu");
    }
#endregion
}