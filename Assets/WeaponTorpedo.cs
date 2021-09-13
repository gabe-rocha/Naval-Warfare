using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponTorpedo : MonoBehaviour {

#region Public Fields

#endregion

#region Private Serializable Fields
    [SerializeField] private float speed = 30f;
    [SerializeField] private GameObject explosionPrefab;
#endregion

#region Private Fields
    private Transform target;
    private Vector3 targetPositionRecalc;
    private bool isTargetAcquired;
#endregion

#region MonoBehaviour CallBacks
    void Awake() {

    }

    void Start() {
        AcquireTarget();
    }

    void Update() {
        if(GameManager.Instance.gameState != GameManager.GameStates.Playing) {
            return;
        }

        if(isTargetAcquired && target != null) {
            transform.position = Vector3.MoveTowards(transform.position, targetPositionRecalc, speed * Time.deltaTime);
        } else {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        transform.LookAt(targetPositionRecalc);

    }

    private void OnCollisionEnter(Collision other) {
        if(other.gameObject.CompareTag("Enemy")) {
            // Instantiate(explosionPrefab, other.GetContact(0).point, Quaternion.identity, null);
            Instantiate(explosionPrefab, transform.position, Quaternion.identity, null);
            Destroy(gameObject);
        }
    }
#endregion

#region Private Methods
    private void AcquireTarget() {
        // if(Physics.CapsuleCast(transform.position, transform.position + (transform.forward * 10f), 50000f, transform.forward, out RaycastHit hit)) {
        //     if(hit.collider.CompareTag("Enemy")) {
        //         isTargetAcquired = true;
        //         target = hit.collider.gameObject.transform;
        //         targetPositionRecalc = target.position - (target.up * 5f);
        //         Debug.Log("Target Acquired!");
        //     } else {
        //         isTargetAcquired = false;
        //     }
        // }
        target = GameObject.FindGameObjectsWithTag("Enemy")[0].transform;
        targetPositionRecalc = target.position - (target.up * 5f);
        isTargetAcquired = true;

    }
#endregion

#region Public Methods

#endregion
}