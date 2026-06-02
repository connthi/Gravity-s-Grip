using UnityEngine;

[RequireComponent(typeof(Light))]
public class TorchLight : MonoBehaviour
{
    public float intensity = 2f;
    public float range = 6f;
    public Color lightColor = new Color(1f, 0.85f, 0.6f);

    private Light torchLight;

    void Awake()
    {
        torchLight = GetComponent<Light>();
        torchLight.type = LightType.Point;
        torchLight.intensity = intensity;
        torchLight.range = range;
        torchLight.color = lightColor;
        torchLight.shadows = LightShadows.Soft;
    }
}
