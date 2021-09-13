using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubmarineWeapons : MonoBehaviour {

#region Public Fields

#endregion

#region Private Serializable Fields
    [SerializeField] private GameObject torpedo01;
    [SerializeField] private Transform torpedoSpawnPointRight, torpedoSpawnPointLeft;
#endregion

#region Private Fields

#endregion

#region MonoBehaviour CallBacks
    void Awake() {

    }

    void Start() {

    }

    void Update() {
        if(GameManager.Instance.gameState != GameManager.GameStates.Playing) {
            return;
        }

        if(Input.GetButtonDown("Fire1")) {
            Instantiate(torpedo01, torpedoSpawnPointRight.position, torpedoSpawnPointRight.rotation, null);
        }

    }
#endregion

#region Private Methods

#endregion

#region Public Methods

#endregion
}