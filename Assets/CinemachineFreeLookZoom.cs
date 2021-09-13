using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[SaveDuringPlay]
[RequireComponent(typeof(CinemachineFreeLook))]
public class CinemachineFreeLookZoom : MonoBehaviour {

#region Public Fields
    public CinemachineFreeLook.Orbit[] originalOrbits = new CinemachineFreeLook.Orbit[0];

    [Tooltip("The minimum scale for the orbits")]
    [Range(0.01f, 1f)]
    public float minScale = 0.5f;

    [Tooltip("The maximum scale for the orbits")]
    [Range(1F, 10f)]
    public float maxScale = 1;

    [Tooltip("The Vertical axis.  Value is 0..1.  How much to scale the orbits")]
    [AxisStateProperty]
    public AxisState zAxis = new AxisState(0, 1, false, true, 50f, 0.1f, 0.1f, "Mouse ScrollWheel", true);
#endregion

#region Private Serializable Fields

#endregion

#region Private Fields
    private CinemachineFreeLook freelook;

#endregion

#region MonoBehaviour CallBacks
    void OnValidate() {
        minScale = Mathf.Max(0.01f, minScale);
        maxScale = Mathf.Max(minScale, maxScale);
    }
    void Awake() {
        freelook = GetComponentInChildren<CinemachineFreeLook>();
        var scale = Mathf.Lerp(minScale, maxScale, zAxis.Value);
        if(freelook != null && originalOrbits.Length == 0) {
            //Updates the input axis.
            zAxis.Update(Time.deltaTime);
            //Lerps the scale multiplier based on the axis value
        }
        // If we have reference to any orbits, set the freelook camera orbits to match
        for (int i = 0; i < Mathf.Min(originalOrbits.Length, freelook.m_Orbits.Length); i++) {
            freelook.m_Orbits[i].m_Height = originalOrbits[i].m_Height * scale;
            freelook.m_Orbits[i].m_Radius = originalOrbits[i].m_Radius * scale;
        }
    }

    void Start() {

    }

    void Update() {
        if(freelook != null) {
            //If we don't have the correct number of orbits cached, copy the orbits from the freelook camera into
            //a locally stored array with the starting values. This is so our scale multiplier references the initial value
            //instead of the current one.
            if(originalOrbits.Length != freelook.m_Orbits.Length) {
                originalOrbits = new CinemachineFreeLook.Orbit[freelook.m_Orbits.Length];
                Array.Copy(freelook.m_Orbits, originalOrbits, freelook.m_Orbits.Length);
            }
            //Update the axis value
            zAxis.Update(Time.deltaTime);
            //Lerp the scale multiplier baysed on the zAxis value
            float scale = Mathf.Lerp(minScale, maxScale, zAxis.Value);
            //Update the free look camera orbits to match the scaled version of the original
            for (int i = 0; i < Mathf.Min(originalOrbits.Length, freelook.m_Orbits.Length); i++) {
                freelook.m_Orbits[i].m_Height = originalOrbits[i].m_Height * scale;
                freelook.m_Orbits[i].m_Radius = originalOrbits[i].m_Radius * scale;
            }
        }
    }
#endregion

#region Private Methods

#endregion

#region Public Methods

#endregion
}