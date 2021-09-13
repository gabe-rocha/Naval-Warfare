using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ocean
{
    /// <summary>
    /// Registers a shore wave input to the wave shape. Attach this GameObjects that represent shore.
    /// </summary>
    [ExecuteAlways]
    public class RegisterShoreWaveInput : RegisterLodDataInput<LodDataMgrAnimWaves>
    {
        public override bool Enabled => true;

        public override float Wavelength => 0;

        //-1 represent shore wave, temp
        public override int BatchIdx => -1;

        public RenderTexture _depthTexture;

        public readonly static Color s_gizmoColor = new Color(0f, 1f, 0f, 0.5f);
        protected override Color GizmoColor => s_gizmoColor;

        protected override string ShaderPrefix => "BlueOcean/Inputs/Shore";

        Renderer _shoreRender;

        protected override void OnEnable()
        {
            base.OnEnable();

            var rend = GetComponent<Renderer>();
            rend.sharedMaterial = new Material(Shader.Find("BlueOcean/Inputs/Shore/OceanShoreWaves"));
            _shoreRender = rend;
            
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void Update()
        {
            base.Update();

            if (OceanRenderer.Instance == null)
            {
                return;
            }

            //OceanRenderer.Instance.ReportMaxDisplacementFromShape(_maxDisplacementHorizontal, maxDispVert, 0f);

            if (_shoreRender != null)
            {
                _shoreRender.sharedMaterial.SetFloat("_GlobalTime", Time.time);
                //_shoreRender.sharedMaterial.SetTexture("_DepthTex", _depthTexture);
                _shoreRender.sharedMaterial.SetFloat("_DepthMax", OceanRenderer.Instance._fadeDepth);

                var gerstnerSetting = OceanRenderer.Instance._simSettingsShoreWaves;
                if (gerstnerSetting != null)
                {
                    _shoreRender.sharedMaterial.SetVectorArray("waveData", gerstnerSetting.waves);
                    _shoreRender.sharedMaterial.SetFloat("_WaveCount", gerstnerSetting.waves.Length);
                }
                else
                {
                    _shoreRender.sharedMaterial.SetFloat("_WaveCount", 0);
                }
                
                _shoreRender.sharedMaterial.SetFloat("_WindDirection", OceanRenderer.Instance.WindDirection);
            }

        }

    }

}
