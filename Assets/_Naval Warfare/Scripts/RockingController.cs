using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RockingController : MonoBehaviour {

#region Private Serializable Fields
    [Tooltip("How many degrees the vessel can rotate on the X Axis when rocking")]
    [SerializeField][Range(0, 90)] private float rockingRotationXLimit = 45f;
    [Tooltip("How fast should this rotation occur?")]
    [SerializeField][Range(0, 10)] private float rockingRotationXSpeed = 5f;

    [Tooltip("How many degrees the vessel can rotate on the Z Axis when rocking")]
    [SerializeField][Range(0, 90)] private float rockingRotationZLimit = 45f;
    [Tooltip("How fast should this rotation occur?")]
    [SerializeField][Range(0, 10)] private float rockingRotationZSpeed = 5f;
    [Tooltip("How many meters the vessel floats Up/Down while rocking")]
    [SerializeField][Range(0, 5)] private float rockingPositionYLimit = 1f;
    [Tooltip("How fast should this change in position occur?")]
    [SerializeField][Range(0, 10)] private float rockingPositionYSpeed = 5f;
#endregion

#region Private Fields
    private bool isRocking;
    private float targetDegreeX, targetDegreeZ, targetPositionY;
    // private float rotationXSpeed, rotationZSpeed, positionYSpeed;
#endregion

#region MonoBehaviour CallBacks
    void Awake() {

    }

    void Start() {
        GetNewTargetDegreeX();
        //rotationXSpeed = UnityEngine.Random.Range(-rockingRotationXMaxSpeed, rockingRotationXMaxSpeed);
    }

    void Update() {
        if (isRocking) {
            //Rocking Rotation X

            transform.Rotate(Vector3.right * rockingRotationXSpeed * Time.deltaTime);
            if (transform.rotation.eulerAngles.x >= targetDegreeX) {
                GetNewTargetDegreeX();
            }

        } else {
            //go back to rotation 0?
        }
    }

    private void GetNewTargetDegreeX() {
        var randomRangeRotationX = UnityEngine.Random.Range(-rockingRotationXLimit, rockingRotationXLimit);
        targetDegreeX = transform.rotation.eulerAngles.x + randomRangeRotationX;
        targetDegreeX = Mathf.Clamp(targetDegreeX, -rockingRotationXLimit, rockingRotationXLimit);

        if (randomRangeRotationX < 0) {
            // rockingRotationXSpeed = rockingRotationXSpeed * Mathf.LerpAngle()
        } else {

        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("Rocking Collider")) {
            isRocking = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Rocking Collider")) {
            isRocking = false;
        }
    }
#endregion

#region Private Methods

#endregion

#region Public Methods

#endregion
}