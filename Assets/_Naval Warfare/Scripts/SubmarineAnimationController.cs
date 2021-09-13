using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SubmarineController))]
public class SubmarineAnimationController : MonoBehaviour {

#region Public Fields

#endregion

#region Private Serializable Fields
    [SerializeField] private Animator animatorPropeller, animatorRudderTurn, animatorRudderDive;
#endregion

#region Private Fields
    private SubmarineController submarineController;
    private bool isTurningRight, isTurningLeft;
#endregion

#region MonoBehaviour CallBacks
    void Awake() {
        submarineController = GetComponent<SubmarineController>();
        if(submarineController == null) {
            Debug.LogError($"{name} missing component SubmarineController");
        }
    }

    void Start() {

    }

    void Update() {
        if(GameManager.Instance.gameState != GameManager.GameStates.Playing) {
            return;
        }

        AnimateTurnRudder();
        AnimateDiveRudder();
        AnimatePropeller();
    }

    private void AnimateTurnRudder() {
        var hor = Input.GetAxis("Horizontal");
        if(hor < 0 && !isTurningLeft) {
            Debug.Log("Rudder Left");
            animatorRudderTurn.SetTrigger("Turn Left");
            isTurningLeft = true;
            isTurningRight = false;
        } else if(hor > 0 && !isTurningRight) {
            Debug.Log("Rudder Right");
            animatorRudderTurn.SetTrigger("Turn Right");
            isTurningRight = true;
            isTurningLeft = false;
        } else if(hor == 0 && (isTurningLeft || isTurningRight)) {
            Debug.Log("Rudder Middle");
            animatorRudderTurn.SetTrigger("Idle");
            isTurningRight = false;
            isTurningLeft = false;
        }
    }

    private static void AnimateDiveRudder() {
        var ver = Input.GetAxis("Vertical");
    }
    private void AnimatePropeller() {
        animatorPropeller.SetFloat("VelocityZ", submarineController.RigidBody.velocity.z);
    }
#endregion

#region Private Methods

#endregion

#region Public Methods

#endregion
}