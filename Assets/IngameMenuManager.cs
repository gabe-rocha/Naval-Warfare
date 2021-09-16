using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IngameMenuManager : MonoBehaviour {

#region Public Fields

#endregion

#region Private Serializable Fields
    [SerializeField] GameObject canvasInGameMenu;
#endregion

#region Private Fields
    bool isShowingMenu = false;
#endregion

#region MonoBehaviour CallBacks
    void Awake() {

    }

    void Start() {
        HideInGameMenu();
    }

    void Update() {
        if(GameManager.Instance.gameState != GameManager.GameStates.Playing) {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Escape)) {
            if(isShowingMenu) {
                HideInGameMenu();
            } else {
                ShowInGameMenu();
            }
        }
    }

    private void ShowInGameMenu() {
        canvasInGameMenu.SetActive(true);
        Time.timeScale = 0f;
        isShowingMenu = true;
    }

    private void HideInGameMenu() {
        canvasInGameMenu.SetActive(false);
        Time.timeScale = 1f;
        isShowingMenu = false;
    }
#endregion

#region Private Methods

#endregion

#region Public Methods
    public void OnButtonResumePressed() {
        HideInGameMenu();
    }
    public void OnButtonRestartPressed() {
        HideInGameMenu();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }
    public void OnButtonMissionsPressed() {
        HideInGameMenu();
        SceneManager.LoadScene("Mission Generator");

    }
    public void OnButtonOptionsPressed() {
        HideInGameMenu();

    }
    public void OnButtonQuitPressed() {
        HideInGameMenu();
        SceneManager.LoadScene("Main Menu");

    }
#endregion
}