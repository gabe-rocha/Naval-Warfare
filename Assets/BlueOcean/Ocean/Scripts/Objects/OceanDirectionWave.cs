using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ocean
{
    public class OceanDirectionWave : MonoBehaviour
    {
        public float XScale = 10;
        public float ZScale = 10;

        public float WaveStrength = 1;
        public float FoamAlpha = 1;

        public Texture2D ShapeTexture;
        public Texture2D FoamTexture;

        private GameObject waveInput;
        private GameObject foamInput;
        // Start is called before the first frame update
        void OnEnable()
        {
            waveInput = GameObject.CreatePrimitive(PrimitiveType.Quad);
            waveInput.transform.SetParent(transform);
            waveInput.transform.localPosition = Vector3.zero;
            waveInput.transform.Rotate(new Vector3(90, 0, 0), Space.Self);
            waveInput.AddComponent<RegisterAnimWavesInput>();
            waveInput.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("BlueOcean/Inputs/Animated Waves/Add From Texture"));
            waveInput.GetComponent<Renderer>().sharedMaterial.mainTexture = ShapeTexture;
            waveInput.GetComponent<Renderer>().enabled = false;

            foamInput = GameObject.CreatePrimitive(PrimitiveType.Plane);
            foamInput.transform.SetParent(transform);
            foamInput.transform.localPosition = Vector3.zero;
            foamInput.AddComponent<RenderAlphaOnSurface>();
            foamInput.GetComponent<Renderer>().sharedMaterial = new Material(Shader.Find("BlueOcean/Ocean Surface Alpha"));
            foamInput.GetComponent<Renderer>().sharedMaterial.mainTexture = FoamTexture;

            ExpandBoundBox();
        }

        void UpdateScale()
        {
            if (waveInput != null)
            {
                waveInput.transform.localScale = new Vector3(XScale * 10, ZScale * 10, 0);
            }

            if (foamInput != null)
            {
                foamInput.transform.localScale = new Vector3(XScale, 0, ZScale);
            }
        }

        void ExpandBoundBox()
        {
            if (foamInput != null)
            {
                foamInput.GetComponent<Renderer>().bounds.Expand(new Vector3(0, 1000, 0));
                foamInput.GetComponent<Renderer>().bounds.Expand(new Vector3(0, -1000, 0));
            }
        }

        void UpdateParameters()
        {
            if (waveInput != null)
            {
                waveInput.GetComponent<Renderer>().sharedMaterial.SetFloat("_Strength", WaveStrength);
            }

            if (foamInput != null)
            {
                foamInput.GetComponent<Renderer>().sharedMaterial.SetFloat("_Alpha", FoamAlpha);
            }
            
        }

        // Update is called once per frame
        void Update()
        {
            UpdateScale();
            UpdateParameters();
        }
    }
}

