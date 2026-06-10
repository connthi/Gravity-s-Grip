using UnityEngine;

/// <summary>
/// Sets scene fog and plays an ambient audio loop.
/// Drop one instance anywhere in the scene.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AmbientAtmosphere : MonoBehaviour
{
    [SerializeField] private Color     fogColor      = new Color(0.12f, 0.1f, 0.08f);
    [SerializeField] private float     fogDensity    = 0.015f;
    [SerializeField] private AudioClip ambientLoop;
    [SerializeField] private float     ambientVolume = 0.35f;

    private void Start()
    {
        RenderSettings.fog        = true;
        RenderSettings.fogMode    = FogMode.Exponential;
        RenderSettings.fogColor   = fogColor;
        RenderSettings.fogDensity = fogDensity;

        if (ambientLoop == null) return;

        var src            = GetComponent<AudioSource>();
        src.clip           = ambientLoop;
        src.loop           = true;
        src.volume         = ambientVolume;
        src.spatialBlend   = 0f;
        src.playOnAwake    = false;
        src.Play();
    }
}