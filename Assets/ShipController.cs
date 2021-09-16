using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipController : MonoBehaviour {

#region Public Fields

#endregion

#region Private Serializable Fields

#endregion

#region Private Fields
    Animator animShipActions;
#endregion

#region MonoBehaviour CallBacks
    void Awake() {
        animShipActions = GetComponent<Animator>();
        if(animShipActions == null) {
            Debug.LogError($"{name} is missing a component");
        }

    }

    void Start() {

    }

    void Update() {

    }
    private void OnCollisionEnter(Collision other) {
        if(other.gameObject.CompareTag("Torpedo")) {
            animShipActions.SetTrigger("Sink");
        }
    }
#endregion

#region Private Methods

#endregion

#region Public Methods

#endregion
}