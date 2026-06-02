using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbientAtmosphere : MonoBehaviour
{
    public Color fogColor = new Color(0.12f, 0.1f, 0.08f, 1f);
    public float fogDensity = 0.015f;
    public AudioClip ambientLoop;
    public float ambientVolume = 0.35f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = ambientVolume;
        audioSource.clip = ambientLoop;
    }

    private void Start()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;

        if (ambientLoop != null)
        {
            audioSource.Play();
        }
    }
}
