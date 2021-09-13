using UnityEngine;

namespace Ocean
{
    [ExecuteAlways]
    public class RegisterShoreFoamInput : RegisterLodDataInput<LodDataMgrFoam>
    {
        public override bool Enabled => true;

        public override float Wavelength => 0f;

        public override int BatchIdx => 0;

        protected override Color GizmoColor => new Color(1f, 1f, 1f, 0.5f);

        protected override string ShaderPrefix => "BlueOcean/Inputs/Foam";

        Renderer _shoreFoamRender;

        public RenderTexture _depthTexture;

        protected override void OnEnable()
        {
            base.OnEnable();

            var rend = GetComponent<Renderer>();
            rend.sharedMaterial = new Material(Shader.Find("BlueOcean/Inputs/Foam/OceanShoreFoam"));
            _shoreFoamRender = rend;
        }

        protected override void Update()
        {
            base.Update();

            if (OceanRenderer.Instance == null)
            {
                return;
            }

            if (_shoreFoamRender != null)
            {
                _shoreFoamRender.sharedMaterial.SetFloat("_GlobalTime", Time.time);
                _shoreFoamRender.sharedMaterial.SetTexture("_DepthTex", _depthTexture);
            }

        }
    }
}
