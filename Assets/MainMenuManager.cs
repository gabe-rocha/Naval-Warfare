using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {

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
    public void OnButtonMissionPressed() {
        SceneManager.LoadScene("Mission Generator");

    }
    public void OnButtonOptionsPressed() {

    }
    public void OnButtonCreditsPressed() {

    }
    public void OnButtonQuitPressed() {
        Application.Quit();

    }
#endregion
}