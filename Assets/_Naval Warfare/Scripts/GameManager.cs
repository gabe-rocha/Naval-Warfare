using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

#region Public Fields
    public static GameManager Instance { get => instance; set => instance = value; }
    public enum GameStates {
        Initializing,
        Loading,
        Playing
    }
    public GameStates gameState = GameStates.Initializing;
#endregion

#region Private Serializable Fields
#endregion

#region Private Fields
    private static GameManager instance;
#endregion

#region MonoBehaviour CallBacks

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            Application.targetFrameRate = 60;

        } else {
            Destroy(this);
        }
    }

    private void Start() {
        gameState = GameStates.Loading;
        //...

        gameState = GameStates.Playing;

        EventManager.Instance.TriggerEvent(EventManager.Events.OnGameManagerReady);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            EventManager.Instance.TriggerEvent(EventManager.Events.OnLevelStarted);
        }
    }
#endregion

#region Private Methods
#endregion

#region Public Methods
#endregion
}