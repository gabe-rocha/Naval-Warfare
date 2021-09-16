using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuDirector : MonoBehaviour {

#region Public Fields

#endregion

#region Private Serializable Fields
    [SerializeField] Transform transformSubmarine;
    [SerializeField] float submarineForwardSpeed = 5f, submarineSubmergeSpeed = 3f, submergeInSeconds = 5f;
    [SerializeField] List<ParticleSystem> listParticlesSubmerge;
    [SerializeField] Animator animStateDrivenCameras;
    [SerializeField] float secondsToCutTo1 = 15, secondsToCutTo2 = 15, secondsToCutTo3 = 15;
#endregion

#region Private Fields
    private Vector3 submarineInitialPosition;
#endregion

#region MonoBehaviour CallBacks
    void Awake() {

    }

    void Start() {
        submarineInitialPosition = transformSubmarine.transform.position;
        StartCoroutine(AaaandAction());
    }

    void Update() {
        MoveSubmarineForward();
    }
#endregion

#region Private Methods 
    private IEnumerator AaaandAction() {
        while (true) {
            transformSubmarine.position = submarineInitialPosition;
            animStateDrivenCameras.SetTrigger("Camera1");
            yield return new WaitForSeconds(secondsToCutTo2);

            animStateDrivenCameras.SetTrigger("Camera2");
            yield return new WaitForSeconds(secondsToCutTo3 * 0.25f);

            StartCoroutine(Submerge());

            yield return new WaitForSeconds(secondsToCutTo3 * 0.75f);
            animStateDrivenCameras.SetTrigger("Camera3");
            yield return new WaitForSeconds(secondsToCutTo1);
        }
    }

    private IEnumerator Submerge() {

        yield return StartCoroutine(PlayParticlesAndSubmergeALittle());

        foreach (var partSyst in listParticlesSubmerge) {
            partSyst.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        var startTime = Time.time;
        while (Time.time < startTime + submergeInSeconds * 0.5) {
            transformSubmarine.Translate(Vector3.down * submarineSubmergeSpeed * 2f * Time.deltaTime, Space.Self);
            yield return null;
        }

        yield return null;
    }
    private void MoveSubmarineForward() {
        transformSubmarine.Translate(Vector3.forward * submarineForwardSpeed * Time.deltaTime, Space.Self);
    }

    private IEnumerator PlayParticlesAndSubmergeALittle() {
        foreach (var partSyst in listParticlesSubmerge) {
            partSyst.Play();
        }

        float startTime = Time.time;
        while (Time.time < startTime + submergeInSeconds * 0.35f) {
            transformSubmarine.Translate(Vector3.down * submarineSubmergeSpeed / 2f * Time.deltaTime, Space.Self);
            yield return null;
        }
        yield return null;
    }
#endregion

#region Public Methods

#endregion
}