using UnityEngine;

[RequireComponent(typeof(Light))]
public class TorchLight : MonoBehaviour
{
    public bool startLit = true;
    public float intensity = 2f;
    public float range = 6f;
    public float flickerSpeed = 10f;
    public float flickerAmount = 0.35f;
    public Color lightColor = new Color(1f, 0.85f, 0.6f);

    private Light torchLight;
    private float baseIntensity;
    public bool IsLit { get; private set; }

    private void Awake()
    {
        torchLight = GetComponent<Light>();
        torchLight.type = LightType.Point;
        torchLight.color = lightColor;
        torchLight.range = range;
        baseIntensity = intensity;
        torchLight.intensity = intensity;
        torchLight.shadows = LightShadows.Soft;
        SetLit(startLit);
    }

    private void Update()
    {
        if (!IsLit)
            return;

        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        float flicker = (noise - 0.5f) * flickerAmount;
        torchLight.intensity = Mathf.Max(0f, baseIntensity + flicker);
        torchLight.color = Color.Lerp(new Color(1f, 0.55f, 0.1f), lightColor, 0.6f + noise * 0.4f);
    }

    public void SetLit(bool lit)
    {
        IsLit = lit;
        if (torchLight == null)
            return;

        torchLight.enabled = lit;
        torchLight.intensity = lit ? baseIntensity : 0f;
    }
}
