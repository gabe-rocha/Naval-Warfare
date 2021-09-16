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
    [SerializeField] float lifeTimeInSecs = 60f;
#endregion

#region Private Fields
    private Transform target;
    private Vector3 targetPositionRecalc;
    private bool isTargetAcquired;
    private float launchTime;
#endregion

#region MonoBehaviour CallBacks
    void Awake() {

    }

    void Start() {
        launchTime = Time.time;
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

        if(Vector3.Distance(transform.position, targetPositionRecalc) < 1f) {
            Explode();
        }

        transform.LookAt(targetPositionRecalc);

        if(Time.time > launchTime + lifeTimeInSecs) {
            Explode();
        }

    }

    private void OnCollisionEnter(Collision other) {
        if(other.gameObject.CompareTag("Enemy")) {
            // Instantiate(explosionPrefab, other.GetContact(0).point, Quaternion.identity, null);
            Explode();
        }
    }

    private void Explode() {
        Instantiate(explosionPrefab, transform.position + (transform.forward * 5f), Quaternion.identity, null);
        Destroy(gameObject);
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
        targetPositionRecalc = target.position + (target.forward * UnityEngine.Random.Range(-50f, 50f)); //random horizontal
        isTargetAcquired = true;

    }
#endregion

#region Public Methods

#endregion
}