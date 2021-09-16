using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubmarineController : MonoBehaviour {

#region Private Serializable Fields
    [SerializeField] private float accelerationVelocity = 1f;
    [SerializeField] private float maxSpeedForward = 10f;
    [SerializeField] private float decelerationVelocity = 0.5f;
    [SerializeField] private float maxSpeedBackward = 5f;
    [SerializeField] private float accelerationAngularVelocityY = 0.5f; //Left/right
    [SerializeField] private float maxAngularVelocityY = 0.1f;
    [SerializeField] private float accelerationAngularVelocityX = 0.1f; //nose up, nose down
    [SerializeField] private float maxAngularVelocityX = 0.1f;
    [SerializeField] private float maxSubmersionSpeed = 0.5f;
    [SerializeField] private float accelerationSurmersion = 1f;

    [SerializeField] private Transform propeller, turnRudder, diveRudder;
    [SerializeField] List<ParticleSystem> listParticlesSubmerge;
#endregion

#region Private Fields
    private Rigidbody rigidBody;
    public Rigidbody RigidBody { get => rigidBody; private set => rigidBody = value; }

    private float horInput, verInput, submergeInput, diveInput;
    private bool isSubmersionParticlesPlaying;

#endregion

#region MonoBehaviour CallBacks
    void Awake() {
        RigidBody = GetComponent<Rigidbody>();
    }

    void Start() {

    }

    void Update() {
        if(GameManager.Instance.gameState != GameManager.GameStates.Playing) {
            return;
        }

        horInput = Input.GetAxis("Horizontal"); // A D
        verInput = Input.GetAxis("Vertical"); // W S
        if(Input.GetKey(KeyCode.Q)) {
            submergeInput = -1f;
        } else if(Input.GetKey(KeyCode.E)) {
            submergeInput = 1f;
        } else {
            submergeInput = 0f;
        }

        MoveForwardBackwards();
        MoveUpDown();
        Rotate();
        ParticleEffectsSubmersion();

    }
#endregion

#region Private Methods
    private void MoveForwardBackwards() {
        if(verInput > 0 && RigidBody.velocity.z < maxSpeedForward) {
            RigidBody.AddForce(transform.forward * accelerationVelocity * verInput, ForceMode.Acceleration); //Move Forward
        } else if(verInput < 0 && RigidBody.velocity.z > maxSpeedBackward) {
            RigidBody.AddForce(transform.forward * decelerationVelocity * verInput, ForceMode.Acceleration); //Move Backwards
        }
    }

    private void MoveUpDown() {
        if(submergeInput > 0 && RigidBody.velocity.y < maxSubmersionSpeed) {
            RigidBody.AddForce(transform.up * accelerationSurmersion * submergeInput, ForceMode.Acceleration); //Move Forward
        } else if(submergeInput < 0 && RigidBody.velocity.y > -maxSubmersionSpeed) {
            RigidBody.AddForce(transform.up * accelerationSurmersion * submergeInput, ForceMode.Acceleration); //Move Backwards
        }
    }

    private void Rotate() {
        if(horInput > 0 && RigidBody.angularVelocity.y < maxAngularVelocityY) {
            // transform.Rotate(Vector3.up * accelerationAngularVelocityY * horInput * Time.deltaTime);
            RigidBody.AddForceAtPosition(-turnRudder.transform.right * accelerationAngularVelocityY * horInput * Time.deltaTime, turnRudder.position, ForceMode.Acceleration); //Rotate Clockwise
        } else if(horInput < 0 && RigidBody.angularVelocity.y > -maxAngularVelocityY) {
            // transform.Rotate(Vector3.up * accelerationAngularVelocityY * horInput * Time.deltaTime);
            RigidBody.AddForceAtPosition(turnRudder.transform.right * accelerationAngularVelocityY * horInput * -1f * Time.deltaTime, turnRudder.position, ForceMode.Acceleration); //Rotate Clockwise
        }
        var rotation = transform.rotation;
        rotation.x = 0;
        rotation.z = 0;
        transform.rotation = rotation;
    }

    private void ParticleEffectsSubmersion() {
        if(submergeInput < 0 && !isSubmersionParticlesPlaying && transform.position.y > -4.44f) {
            foreach (var partSyst in listParticlesSubmerge) {
                partSyst.Play();
            }
            isSubmersionParticlesPlaying = true;
        } else if(submergeInput < 0 && isSubmersionParticlesPlaying && transform.position.y <= -4.44f) {
            foreach (var partSyst in listParticlesSubmerge) {
                partSyst.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            isSubmersionParticlesPlaying = false;
        } else if(submergeInput >= 0 && isSubmersionParticlesPlaying) {
            foreach (var partSyst in listParticlesSubmerge) {
                partSyst.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
            isSubmersionParticlesPlaying = false;
        }
    }
#endregion

#region Public Methods

#endregion

#if UNITY_EDITOR
    void OnGUI() {
        GUI.skin.label.fontSize = Screen.width / 75;
        GUI.skin.label.normal.textColor = Color.magenta;
        GUILayout.Label($"Velocity: {RigidBody.velocity}");
        GUILayout.Label($"Ang Velocity: {RigidBody.angularVelocity}");
    }
#endif
}