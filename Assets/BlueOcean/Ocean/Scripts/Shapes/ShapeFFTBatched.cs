using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Ocean
{
[ExecuteAlways]
public class ShapeFFTBatched : MonoBehaviour, IFloatingOrigin
{
    public class FFTBatch : ILodDataInput
    {
        public FFTBatch(ShapeFFTBatched fft, int batchIndex, MeshRenderer rend)
        {
            _fft = fft;
            _batchIndex = batchIndex;
          
            _material = new PropertyWrapperMaterial(new Material(rend.sharedMaterial ?? rend.material));

            _rend = rend;

            // Enabled stays true, because we don't sort the waves into buckets until Draw time, so we don't know if something should
            // be drawn in advance.
            Enabled = true;
        }

        PropertyWrapperMaterial _material;
        

        MeshRenderer _rend;

        ShapeFFTBatched _fft;
        int _batchIndex = -1;
            
        public float Wavelength => 0;
        public int BatchIdx => _batchIndex;
        public bool Enabled { get; set; }

        public void Draw(CommandBuffer buf, float weight, int isTransition, int lodIdx)
        {
            if (!_rend) return;

            float time = Time.time;
            var lodScale = OceanRenderer.Instance.CalcLodScale(lodIdx);
            var camOrthSize = 2 * lodScale;
            _fft.UpdateFFTChain(buf, _batchIndex, 2 * camOrthSize, time);
            _fft.BindData(_material, _batchIndex);
            if (weight > 0f)
            {
                buf.DrawRenderer(_rend, _material.material);
            }
        }
    }

    bool _init = false;

    FFTBatch[] _batches = null;   

    private ComputeShader _shaderSpectrum;
    private PropertyWrapperCompute _shaderSpectrumWrapper;
    private int _kernelSpectrumInit;
    private int _kernelSpectrumUpdate;

    private ComputeShader _shaderFFT;
    private int _kernelFFTX = 0;
    private int _kernelFFTY = 1;



    // Spectrum
    private RenderTexture _bufferSpectrumH0;
    private RenderTexture _bufferSpectrumH;
    private RenderTexture _bufferSpectrumDx;
    private RenderTexture _bufferSpectrumDy;
    //final
    private RenderTexture _bufferHDxDyFinal;

    // FFT
    private RenderTexture _bufferFFTTemp;
    private Texture2D _texButterfly;
    private Texture2D _texBitReverse;

    // Combine
    private RenderTexture _bufferDisplacement;
        
    readonly int sp_Choppiness = Shader.PropertyToID("choppiness");
    readonly int sp_DepthFade = Shader.PropertyToID("depthFade");
    readonly int sp_BaseScattering = Shader.PropertyToID("baseSSS");
    readonly int sp_DisplacementScatteringMul = Shader.PropertyToID("dispSSSMul");
    readonly int sp_Tex_InputHDxDy = Shader.PropertyToID("inputHDxDy");

    GameObject _renderProxy;

    private void OnEnable()
    {
        _init = false;
    }

    void OnDisable()
    {
        if (_bufferFFTTemp)  _bufferFFTTemp.Release();
        if (_bufferSpectrumH0) _bufferSpectrumH0.Release();
        if (_bufferSpectrumH) _bufferSpectrumH.Release();
        if (_bufferSpectrumDx) _bufferSpectrumDx.Release();
        if (_bufferSpectrumDy) _bufferSpectrumDy.Release();
        if (_bufferHDxDyFinal) _bufferHDxDyFinal.Release();
        if (_bufferDisplacement) _bufferDisplacement.Release();
#if UNITY_EDITOR
        if (_texButterfly) DestroyImmediate(_texButterfly);
        if (_texBitReverse) DestroyImmediate(_texBitReverse);
#else
        if (_texButterfly) Destroy(_texButterfly);
        if (_texBitReverse) Destroy(_texBitReverse);
#endif

        }

        // Use this for initialization
        void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        InitBatches();
    }

    public void SetOrigin(Vector3 newOrigin)
    {

    }

    void InitBatches()
    {
        if (!OceanRenderer.Instance) return;
        if (_init) return;

        _init = true;

        int _fftSize = OceanRenderer.Instance.LodDataResolution;

        _shaderSpectrum = ComputeShaderHelpers.LoadShader("FFTSpectrum");
        if (_shaderSpectrum == null)
        {
            enabled = false;
            return;
        }

        _shaderSpectrumWrapper = new PropertyWrapperCompute();

        _kernelSpectrumInit = _shaderSpectrum.FindKernel("SpectrumInit");
        _kernelSpectrumUpdate = _shaderSpectrum.FindKernel("SpectrumUpdate");

        _shaderFFT = ComputeShaderHelpers.LoadShader("FFTCompute");
        if (_shaderFFT == null)
        {
            enabled = false;
            return;
        }
        // Kernel offset
        {
            int baseLog2Size = Mathf.RoundToInt(Mathf.Log(256, 2));
            int log2Size = Mathf.RoundToInt(Mathf.Log(_fftSize, 2));
            _kernelFFTX = (log2Size - baseLog2Size) * 2;
            _kernelFFTY = _kernelFFTX + 1;
        }

        _bufferFFTTemp = new RenderTexture(_fftSize, _fftSize, 0, RenderTextureFormat.ARGBFloat);
        _bufferFFTTemp.enableRandomWrite = true;
        _bufferFFTTemp.Create();

        _bufferSpectrumH0 = new RenderTexture(_fftSize, _fftSize, 0, RenderTextureFormat.ARGBFloat);
        _bufferSpectrumH0.enableRandomWrite = true;
        _bufferSpectrumH0.Create();

        _bufferSpectrumH = CreateSpectrumUAV(_fftSize);
        _bufferSpectrumDx = CreateSpectrumUAV(_fftSize);
        _bufferSpectrumDy = CreateSpectrumUAV(_fftSize);
        _bufferHDxDyFinal = Create3FinalTexture(_fftSize);
        _bufferDisplacement = CreateCombinedTexture(_fftSize);

        // Butterfly
        {
            int log2Size = Mathf.RoundToInt(Mathf.Log(_fftSize, 2));

            var butterflyData = new Vector2[_fftSize * log2Size];

            int offset = 1, numIterations = _fftSize >> 1;
            for (int rowIndex = 0; rowIndex < log2Size; rowIndex++)
            {
                int rowOffset = rowIndex * _fftSize;

                // Weights
                {
                    int start = 0, end = 2 * offset;
                    for (int iteration = 0; iteration < numIterations; iteration++)
                    {
                        float bigK = 0.0f;
                        for (int K = start; K < end; K += 2)
                        {
                            float phase = 2.0f * Mathf.PI * bigK * numIterations / _fftSize;
                            float cos = Mathf.Cos(phase);
                            float sin = Mathf.Sin(phase);

                            butterflyData[rowOffset + K / 2].x = cos;
                            butterflyData[rowOffset + K / 2].y = -sin;

                            butterflyData[rowOffset + K / 2 + offset].x = -cos;
                            butterflyData[rowOffset + K / 2 + offset].y = sin;

                            bigK += 1.0f;
                        }
                        start += 4 * offset;
                        end = start + 2 * offset;
                    }
                }

                numIterations >>= 1;
                offset <<= 1;
            }

            var butterflyBytes = new byte[butterflyData.Length * sizeof(ushort) * 2];
            for (uint i = 0; i < butterflyData.Length; i++)
            {
                uint byteOffset = i * sizeof(ushort) * 2;
                HalfHelper.SingleToHalf(butterflyData[i].x, butterflyBytes, byteOffset);
                HalfHelper.SingleToHalf(butterflyData[i].y, butterflyBytes, byteOffset + sizeof(ushort));
            }

            _texButterfly = new Texture2D(_fftSize, log2Size, TextureFormat.RGHalf, false);
            _texButterfly.LoadRawTextureData(butterflyBytes);
            _texButterfly.Apply(false, true);
        }

        {
            int log2Size = Mathf.RoundToInt(Mathf.Log(_fftSize, 2));
            var bitReverseBytes = new byte[_fftSize * sizeof(uint)];
            for (uint i = 0; i < _fftSize; i++)
            {
                float reversed = (float)(HalfHelper.Reverse(i) >> (32 - log2Size));
                uint byteOffset = i * sizeof(uint);
                HalfHelper.SingleToBytes(reversed, bitReverseBytes, byteOffset);

                //bitReverseBytes[i].r = reversed;
            }
            _texBitReverse = new Texture2D(_fftSize, 1, TextureFormat.RFloat, false);
            _texBitReverse.LoadRawTextureData(bitReverseBytes);
            _texBitReverse.Apply(false, true);
        }


            var registered = RegisterLodDataInputBase.GetRegistrar(typeof(LodDataMgrAnimWaves));

#if UNITY_EDITOR
        // Unregister after switching modes in the editor.
        if (_batches != null)
        {
            foreach (var batch in _batches)
            {
                registered.Remove(batch);
            }
        }
#endif
        initRenderProxy();
        Debug.Assert(_renderProxy, "_renderProxy is null!");
        MeshRenderer rend = _renderProxy.GetComponent<MeshRenderer>();
        var lodCount = OceanRenderer.Instance.CurrentLodCount;
        _batches = new FFTBatch[lodCount];
        for (int i = 0; i < _batches.Length; i++)
        {
            _batches[i] = new FFTBatch(this, i, rend);
        }

        foreach (var batch in _batches)
        {
            registered.Add(0, batch);
        }
    }

    void initRenderProxy()
        {
            // Get the wave
            MeshRenderer rend = null;
            // Create render proxy only if we don't already have one.
            if (_renderProxy == null)
            {
                // Create a proxy MeshRenderer to feed the rendering
                _renderProxy = GameObject.CreatePrimitive(PrimitiveType.Quad);
#if UNITY_EDITOR
                DestroyImmediate(_renderProxy.GetComponent<Collider>());
#else
            Destroy(_renderProxy.GetComponent<Collider>());
#endif
                _renderProxy.hideFlags = HideFlags.HideAndDontSave;
                _renderProxy.transform.parent = transform;
                rend = _renderProxy.GetComponent<MeshRenderer>();
                rend.enabled = false;
                var combineShader = Shader.Find("Hidden/BlueOcean/Inputs/Animated Waves/FFT Batch Global");
                Debug.Assert(combineShader, "Could not load fft combine shader, make sure it is packaged in the build.");
                if (combineShader == null)
                {
                    enabled = false;
                    return;
                }

                rend.material = new Material(combineShader);
            }
        }

    RenderTexture CreateSpectrumUAV(int size)
    {
        var uav = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat);
        uav.enableRandomWrite = true;
        uav.Create();
        return uav;
    }

    RenderTexture CreateFinalTexture(int size)
    {
        var texture = new RenderTexture(size, size, 0, RenderTextureFormat.RFloat);
        texture.enableRandomWrite = true;
        texture.Create();
        return texture;
    }

    RenderTexture Create3FinalTexture(int size)
    {
        var texture = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat);
        texture.enableRandomWrite = true;
        texture.Create();
        return texture;
    }

    RenderTexture CreateCombinedTexture(int size)
    {
        var texture = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat);
        texture.enableRandomWrite = true;
        texture.useMipMap = true;
        texture.autoGenerateMips = true;
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.Create();
        return texture;
    }

    void SpectrumInit(CommandBuffer buf, float domainSize)
    {
        int _fftSize = OceanRenderer.Instance.LodDataResolution;
        buf.SetComputeIntParam(_shaderSpectrum, "size", _fftSize);
        buf.SetComputeFloatParam(_shaderSpectrum, "domainSize", domainSize);
        buf.SetComputeFloatParam(_shaderSpectrum, "gravity", OceanRenderer.Instance.Gravity);
        buf.SetComputeFloatParam(_shaderSpectrum, "windDirection", OceanRenderer.Instance.WindDirection);

        //if (_windSpeed <= 0) _windSpeed = 0.001f;
        buf.SetComputeFloatParam(_shaderSpectrum, "windSpeed", OceanRenderer.Instance.WindSpeed);

        buf.SetComputeTextureParam(_shaderSpectrum, _kernelSpectrumInit, "outputH0", _bufferSpectrumH0);

        buf.DispatchCompute(_shaderSpectrum, _kernelSpectrumInit, _fftSize / 8, _fftSize / 8, 1);
    }

    void SpectrumUpdate(CommandBuffer buf, float time)
    {
        int _fftSize = OceanRenderer.Instance.LodDataResolution;
        buf.SetComputeFloatParam(_shaderSpectrum, "time", time);
        buf.SetComputeFloatParam(_shaderSpectrum, "windDirection", OceanRenderer.Instance.WindDirection);

        buf.SetComputeTextureParam(_shaderSpectrum, _kernelSpectrumUpdate, "inputH0", _bufferSpectrumH0);
        buf.SetComputeTextureParam(_shaderSpectrum, _kernelSpectrumUpdate, "outputH", _bufferSpectrumH);
        buf.SetComputeTextureParam(_shaderSpectrum, _kernelSpectrumUpdate, "outputDx", _bufferSpectrumDx);
        buf.SetComputeTextureParam(_shaderSpectrum, _kernelSpectrumUpdate, "outputDy", _bufferSpectrumDy);

        buf.DispatchCompute(_shaderSpectrum, _kernelSpectrumUpdate, _fftSize / 8, _fftSize / 8, 1);
    }

    //slot: 0:h 1:dx 2:dy
    void FFT(CommandBuffer buf, RenderTexture spectrum, int slot, RenderTexture output)
    {
        int _fftSize = OceanRenderer.Instance.LodDataResolution;
        buf.SetComputeIntParam(_shaderFFT, "slot", slot);

        buf.SetComputeTextureParam(_shaderFFT, _kernelFFTX, "inputSpectrum", spectrum);
        buf.SetComputeTextureParam(_shaderFFT, _kernelFFTX, "inputButterfly", _texButterfly);
        buf.SetComputeTextureParam(_shaderFFT, _kernelFFTX, "inputBitReverse", _texBitReverse);
        buf.SetComputeTextureParam(_shaderFFT, _kernelFFTX, "output", _bufferFFTTemp);
        buf.DispatchCompute(_shaderFFT, _kernelFFTX, 1, _fftSize, 1);
        buf.SetComputeTextureParam(_shaderFFT, _kernelFFTY, "inputSpectrum", _bufferFFTTemp);
        buf.SetComputeTextureParam(_shaderFFT, _kernelFFTY, "inputButterfly", _texButterfly);
        buf.SetComputeTextureParam(_shaderFFT, _kernelFFTY, "inputBitReverse", _texBitReverse);
        buf.SetComputeTextureParam(_shaderFFT, _kernelFFTY, "output", output);
        buf.DispatchCompute(_shaderFFT, _kernelFFTY, _fftSize, 1, 1);
    }

    public void UpdateFFTChain(CommandBuffer buf, int lod, float domainSize, float time)
    {
        SpectrumInit(buf, domainSize);
        SpectrumUpdate(buf, time);
            
        FFT(buf, _bufferSpectrumH, 0, _bufferHDxDyFinal);
        FFT(buf, _bufferSpectrumDx, 1, _bufferHDxDyFinal);
        FFT(buf, _bufferSpectrumDy, 2, _bufferHDxDyFinal);
        }

    public void BindData(IPropertyWrapper properties, int lod)
    {
        if (OceanRenderer.Instance._lodDataSeaDepths != null)
        {
            OceanRenderer.Instance._lodDataSeaDepths.BindResultData(properties);
        }
        else
        {
            LodDataMgrSeaFloorDepth.BindNull(properties);
        }
        OceanRenderer.Instance._lodDataAnimWaves.BindLodData(properties);

        properties.SetInt(LodDataMgr.sp_LD_SliceIndex, lod);
        properties.SetFloat(sp_Choppiness, OceanRenderer.Instance._deepChoppiness);
        properties.SetFloat(sp_DepthFade, OceanRenderer.Instance._fadeDepth);
        properties.SetFloat(sp_BaseScattering, OceanRenderer.Instance._baseScattering);
        properties.SetFloat(sp_DisplacementScatteringMul, OceanRenderer.Instance._displacementScatteringMul);

        properties.SetTexture(sp_Tex_InputHDxDy, _bufferHDxDyFinal);
        }
    }
}
