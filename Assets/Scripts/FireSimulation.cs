using System.Collections;
using UnityEngine;

/// <summary>
/// Runtime particle-based fire effect with flickering light.
/// Call ResumeFire() / PauseFire() / Extinguish() to control it.
/// TorchPickup owns when to call these — FireSimulation just renders.
/// </summary>
[AddComponentMenu("Effects/Fire Simulation")]
public class FireSimulation : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Fire Particles")]
    [SerializeField] private int   maxParticles    = 300;
    [SerializeField] private float emissionRate    = 80f;
    [SerializeField] private float particleLife    = 1.2f;
    [SerializeField] private float riseSpeed       = 2.5f;
    [SerializeField] private float spreadRadius    = 0.4f;
    [SerializeField] private float startSize       = 0.35f;
    [SerializeField] private float endSize         = 0.05f;

    [Header("Colors")]
    [SerializeField] private Color coreColor = new Color(1f, 1f, 0.6f, 1f);
    [SerializeField] private Color midColor  = new Color(1f, 0.45f, 0.05f, 0.9f);
    [SerializeField] private Color tipColor  = new Color(0.6f, 0.05f, 0f, 0f);

    [Header("Light")]
    [SerializeField] private bool  enableLight    = true;
    [SerializeField] private float lightIntensity = 2.5f;
    [SerializeField] private float flickerAmt     = 0.8f;
    [SerializeField] private float flickerSpeed   = 8f;
    [SerializeField] private float lightRange     = 6f;
    [SerializeField] private Color lightColor     = new Color(1f, 0.5f, 0.15f);

    [Header("Smoke")]
    [SerializeField] private bool  enableSmoke       = true;
    [SerializeField] private float smokeRate         = 12f;
    [SerializeField] private float smokeSpawnHeight  = 0.8f;
    [SerializeField] private float smokeMaxSize      = 1.2f;
    [SerializeField] private Color smokeColor        = new Color(0.72f, 0.68f, 0.63f, 0.28f);

    [Header("Embers")]
    [SerializeField] private bool  enableEmbers  = true;
    [SerializeField] private float emberRate     = 10f;

    // ── Private ───────────────────────────────────────────────────────────────

    private ParticleSystem _firePS;
    private ParticleSystem _smokePS;
    private ParticleSystem _emberPS;
    private Light          _light;
    private float          _flickerOffset;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        _flickerOffset = Random.Range(0f, 100f);
        _firePS  = BuildFireParticles();
        if (enableSmoke)  _smokePS = BuildSmokeParticles();
        if (enableEmbers) _emberPS = BuildEmberParticles();
        if (enableLight)  _light   = BuildLight();
    }

    private void Update()
    {
        if (enableLight && _light != null) FlickerLight();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void ResumeFire()
    {
        _firePS?.Play();
        _smokePS?.Play();
        _emberPS?.Play();
        if (_light) _light.enabled = true;
    }

    public void PauseFire()
    {
        _firePS?.Pause();
        _smokePS?.Pause();
        _emberPS?.Pause();
        if (_light) _light.enabled = false;
    }

    public void Extinguish(float duration = 2f) => StartCoroutine(ExtinguishRoutine(duration));

    // ── Light ─────────────────────────────────────────────────────────────────

    private void FlickerLight()
    {
        float t  = Time.time * flickerSpeed + _flickerOffset;
        float n  = (Mathf.PerlinNoise(t, 0f)
                  + Mathf.PerlinNoise(t * 2.3f, 10f) * 0.5f
                  + Mathf.PerlinNoise(t * 5.1f, 20f) * 0.25f) / 1.75f;

        _light.intensity = lightIntensity + (n - 0.5f) * 2f * flickerAmt;
        _light.range     = lightRange     + (n - 0.5f) * 0.5f;
        _light.color     = Color.Lerp(new Color(1f, 0.35f, 0.1f), new Color(1f, 0.75f, 0.3f), n);
    }

    private Light BuildLight()
    {
        var go = new GameObject("Fire_Light");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        var l       = go.AddComponent<Light>();
        l.type      = LightType.Point;
        l.color     = lightColor;
        l.intensity = lightIntensity;
        l.range     = lightRange;
        l.shadows   = LightShadows.Soft;
        return l;
    }

    // ── Particle Builders ─────────────────────────────────────────────────────

    private ParticleSystem BuildFireParticles()
    {
        var go  = new GameObject("Fire_Particles");
        go.transform.SetParent(transform, false);
        go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        var ps   = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop              = true;
        main.startLifetime     = new ParticleSystem.MinMaxCurve(particleLife * 0.7f, particleLife);
        main.startSpeed        = new ParticleSystem.MinMaxCurve(riseSpeed * 0.8f, riseSpeed * 1.2f);
        main.startSize         = new ParticleSystem.MinMaxCurve(startSize * 0.6f, startSize * 1.2f);
        main.startColor        = coreColor;
        main.maxParticles      = maxParticles;
        main.simulationSpace   = ParticleSystemSimulationSpace.World;
        main.gravityModifier   = -0.3f;

        var em = ps.emission; em.rateOverTime = emissionRate;

        var sh = ps.shape;
        sh.enabled   = true;
        sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle     = 12f;
        sh.radius    = spreadRadius;

        var col  = ps.colorOverLifetime;
        col.enabled = true;
        col.color   = new ParticleSystem.MinMaxGradient(MakeFireGradient());

        var sz   = ps.sizeOverLifetime;
        sz.enabled = true;
        sz.size    = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.4f, 0.8f), new Keyframe(1f, endSize / startSize)));

        var noise = ps.noise;
        noise.enabled     = true;
        noise.strength    = 0.3f;
        noise.frequency   = 1.2f;
        noise.scrollSpeed = 0.5f;
        noise.quality     = ParticleSystemNoiseQuality.Medium;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.renderMode  = ParticleSystemRenderMode.Billboard;
        rend.material    = MakeAdditiveMaterial();
        rend.sortingOrder = 1;

        ps.Play();
        return ps;
    }

    private ParticleSystem BuildSmokeParticles()
    {
        var go = new GameObject("Smoke_Particles");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = new Vector3(0f, smokeSpawnHeight, 0f);
        go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        var ps   = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop            = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(2.5f, 4.5f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
        main.startSize       = new ParticleSystem.MinMaxCurve(smokeMaxSize * 0.25f, smokeMaxSize * 0.55f);
        main.startColor      = smokeColor;
        main.maxParticles    = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.05f;

        var em = ps.emission; em.rateOverTime = smokeRate;

        var sh = ps.shape;
        sh.enabled   = true;
        sh.shapeType = ParticleSystemShapeType.Cone;
        sh.angle     = 20f;
        sh.radius    = spreadRadius * 0.6f;

        var col  = ps.colorOverLifetime;
        col.enabled = true;
        col.color   = new ParticleSystem.MinMaxGradient(MakeSmokeGradient());

        var sz = ps.sizeOverLifetime;
        sz.enabled = true;
        sz.size    = new ParticleSystem.MinMaxCurve(smokeMaxSize, new AnimationCurve(
            new Keyframe(0f, 0.15f), new Keyframe(0.3f, 0.65f), new Keyframe(1f, 1f)));

        var noise = ps.noise;
        noise.enabled     = true;
        noise.strength    = 0.5f;
        noise.frequency   = 0.4f;
        noise.scrollSpeed = 0.2f;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.renderMode  = ParticleSystemRenderMode.Billboard;
        rend.material    = MakeAlphaMaterial();
        rend.sortingOrder = 0;

        ps.Play();
        return ps;
    }

    private ParticleSystem BuildEmberParticles()
    {
        var go = new GameObject("Ember_Particles");
        go.transform.SetParent(transform, false);
        go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        var ps   = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop            = true;
        main.startLifetime   = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        main.startSpeed      = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        main.startSize       = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
        main.startColor      = new Color(1f, 0.6f, 0.1f, 1f);
        main.maxParticles    = 80;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.1f;

        var em = ps.emission; em.rateOverTime = emberRate;

        var sh = ps.shape;
        sh.enabled   = true;
        sh.shapeType = ParticleSystemShapeType.Circle;
        sh.radius    = spreadRadius * 0.5f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color   = new ParticleSystem.MinMaxGradient(MakeEmberGradient());

        var noise = ps.noise;
        noise.enabled     = true;
        noise.strength    = 0.8f;
        noise.frequency   = 0.8f;
        noise.scrollSpeed = 1f;

        var rend = ps.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Billboard;
        rend.material   = MakeAdditiveMaterial();

        ps.Play();
        return ps;
    }

    // ── Gradient Helpers ──────────────────────────────────────────────────────

    private Gradient MakeFireGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(coreColor, 0f),
                    new GradientColorKey(midColor,  0.4f),
                    new GradientColorKey(tipColor,  1f) },
            new[] { new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.9f, 0.3f),
                    new GradientAlphaKey(0f, 1f) });
        return g;
    }

    private Gradient MakeSmokeGradient()
    {
        float peak = Mathf.Clamp(smokeColor.a, 0.01f, 1f);
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(smokeColor, 0f),
                    new GradientColorKey(smokeColor, 0.35f),
                    new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f,        0f),
                    new GradientAlphaKey(peak,       0.12f),
                    new GradientAlphaKey(peak * 0.6f, 0.55f),
                    new GradientAlphaKey(0f,         1f) });
        return g;
    }

    private static Gradient MakeEmberGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f),
                    new GradientColorKey(new Color(0.8f, 0.1f, 0f),  0.5f),
                    new GradientColorKey(new Color(0.1f, 0.1f, 0.1f), 1f) },
            new[] { new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.5f),
                    new GradientAlphaKey(0f, 1f) });
        return g;
    }

    // ── Material Helpers ──────────────────────────────────────────────────────

    private static Material MakeAdditiveMaterial()
    {
        var mat = new Material(Shader.Find("Particles/Standard Unlit")
                   ?? Shader.Find("Legacy Shaders/Particles/Additive"));
        return mat;
    }

    private static Material MakeAlphaMaterial()
    {
        var mat = new Material(Shader.Find("Sprites/Default"));
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.renderQueue = 3000;
        return mat;
    }

    // ── Extinguish Coroutine ──────────────────────────────────────────────────

    private IEnumerator ExtinguishRoutine(float duration)
    {
        float elapsed        = 0f;
        float startIntensity = lightIntensity;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / duration;

            SetEmissionRate(_firePS,  Mathf.Lerp(emissionRate, 0f, t));
            SetEmissionRate(_emberPS, Mathf.Lerp(emberRate,    0f, t));
            SetEmissionRate(_smokePS, Mathf.Lerp(smokeRate * 1.4f, 0f, Mathf.Clamp01(t * 1.5f - 0.3f)));

            if (_light) _light.intensity = Mathf.Lerp(startIntensity, 0f, t);
            yield return null;
        }

        PauseFire();
    }

    private static void SetEmissionRate(ParticleSystem ps, float rate)
    {
        if (ps == null) return;
        var em = ps.emission;
        var r  = em.rateOverTime;
        r.constant     = rate;
        em.rateOverTime = r;
    }
}