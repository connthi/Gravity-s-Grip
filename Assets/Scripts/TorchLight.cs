using UnityEngine;

/// <summary>
/// Manages the flickering point light on a torch.
/// SetLit(bool) is the only public API — TorchPickup calls it.
/// </summary>
[RequireComponent(typeof(Light))]
public class TorchLight : MonoBehaviour
{
    [SerializeField] private float  intensity    = 2f;
    [SerializeField] private float  range        = 6f;
    [SerializeField] private float  flickerSpeed = 10f;
    [SerializeField] private float  flickerAmt   = 0.35f;
    [SerializeField] private Color  color        = new Color(1f, 0.85f, 0.6f);

    private Light _light;
    private float _baseIntensity;

    public bool IsLit { get; private set; }

    private void Awake()
    {
        _light          = GetComponent<Light>();
        _light.type     = LightType.Point;
        _light.color    = color;
        _light.range    = range;
        _light.shadows  = LightShadows.Soft;
        _baseIntensity  = intensity;
    }

    private void Update()
    {
        if (!IsLit) return;

        float noise     = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        _light.intensity = Mathf.Max(0f, _baseIntensity + (noise - 0.5f) * flickerAmt);
        _light.color     = Color.Lerp(new Color(1f, 0.55f, 0.1f), color, 0.6f + noise * 0.4f);
    }

    public void SetLit(bool lit)
    {
        IsLit             = lit;
        _light.enabled    = lit;
        _light.intensity  = lit ? _baseIntensity : 0f;
    }
}