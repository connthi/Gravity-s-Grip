using System.Collections;
using UnityEngine;

/// <summary>
/// FireSimulation — attach to any 3D GameObject.
/// Creates a particle-based fire effect with dynamic flickering light.
/// No external assets required; everything is generated at runtime.
/// </summary>
[AddComponentMenu("Effects/Fire Simulation")]
public class FireSimulation : MonoBehaviour
{
    // ─────────────────────────────────────────────
    //  Inspector Settings
    // ─────────────────────────────────────────────

    [Header("Particle Settings")]
    [Tooltip("Max number of fire particles alive at once.")]
    public int maxParticles = 300;

    [Tooltip("How many particles are emitted per second.")]
    public float emissionRate = 80f;

    [Tooltip("Base lifetime of each particle (seconds).")]
    public float particleLifetime = 1.2f;

    [Tooltip("How fast particles rise.")]
    public float riseSpeed = 2.5f;

    [Tooltip("Random spread radius around the object's origin.")]
    public float spreadRadius = 0.4f;

    [Tooltip("Start size of particles.")]
    public float startSize = 0.35f;

    [Tooltip("End size of particles (shrinks toward top of flame).")]
    public float endSize = 0.05f;

    [Header("Color Gradient")]
    [Tooltip("Inner core color (hottest).")]
    public Color coreColor = new Color(1f, 1f, 0.6f, 1f);

    [Tooltip("Mid-flame color.")]
    public Color midColor = new Color(1f, 0.45f, 0.05f, 0.9f);

    [Tooltip("Outer tip color (coolest).")]
    public Color tipColor = new Color(0.6f, 0.05f, 0f, 0f);

    [Header("Light Settings")]
    [Tooltip("Enable a dynamic point light that flickers.")]
    public bool enableLight = true;

    [Tooltip("Base intensity of the fire light.")]
    public float lightIntensity = 2.5f;

    [Tooltip("How much the intensity flickers (+/- this value).")]
    public float flickerAmount = 0.8f;

    [Tooltip("Speed of the flicker noise.")]
    public float flickerSpeed = 8f;

    [Tooltip("Light range in world units.")]
    public float lightRange = 6f;

    [Tooltip("Color of the light cast onto surroundings.")]
    public Color lightColor = new Color(1f, 0.5f, 0.15f);

    [Header("Smoke Settings")]
    [Tooltip("Enable rising smoke above the flame.")]
    public bool enableSmoke = true;

    [Tooltip("Emission rate for smoke puffs.")]
    public float smokeEmissionRate = 12f;

    [Tooltip("How high above the object smoke spawns.")]
    public float smokeSpawnHeight = 0.8f;

    [Tooltip("How large smoke puffs grow.")]
    public float smokeMaxSize = 1.2f;

    [Tooltip("Smoke color (typically dark grey).")]
    public Color smokeColor = new Color(0.72f, 0.68f, 0.63f, 0.28f);

    [Header("Ember Settings")]
    [Tooltip("Enable small rising ember particles.")]
    public bool enableEmbers = true;

    [Tooltip("Emission rate for embers.")]
    public float emberEmissionRate = 10f;

    // ─────────────────────────────────────────────
    //  Private State
    // ─────────────────────────────────────────────

    private ParticleSystem firePS;
    private ParticleSystem smokePS;
    private ParticleSystem emberPS;
    private Light fireLight;
    private float flickerOffset;

    // ─────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────

    void Start()
    {
        flickerOffset = Random.Range(0f, 100f);

        BuildFireParticles();

        if (enableSmoke)
            BuildSmokeParticles();

        if (enableEmbers)
            BuildEmberParticles();

        if (enableLight)
            BuildLight();
    }

    void Update()
    {
        if (enableLight && fireLight != null)
            UpdateFlicker();
    }

    // ─────────────────────────────────────────────
    //  Fire Particle System
    // ─────────────────────────────────────────────

    void BuildFireParticles()
    {
        GameObject fireGO = new GameObject("Fire_Particles");
        fireGO.transform.SetParent(transform, false);

        firePS = fireGO.AddComponent<ParticleSystem>();
        fireGO.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        var main = firePS.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(particleLifetime * 0.7f, particleLifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(riseSpeed * 0.8f, riseSpeed * 1.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(startSize * 0.6f, startSize * 1.2f);
        main.startColor = coreColor;
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.3f;

        var emission = firePS.emission;
        emission.rateOverTime = emissionRate;

        var shape = firePS.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = spreadRadius;

        var col = firePS.colorOverLifetime;
        col.enabled = true;

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(coreColor, 0.0f),
                new GradientColorKey(midColor, 0.4f),
                new GradientColorKey(tipColor, 1.0f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(0.9f, 0.3f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        var sizeOL = firePS.sizeOverLifetime;
        sizeOL.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.4f, 0.8f),
            new Keyframe(1f, endSize / startSize)
        );
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var noise = firePS.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 1.2f;
        noise.scrollSpeed = 0.5f;
        noise.quality = ParticleSystemNoiseQuality.Medium;

        var renderer = firePS.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader")
        {
            mat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        }
        mat.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
        renderer.material = mat;
        renderer.sortingOrder = 1;

        firePS.Play();
    }

    // ─────────────────────────────────────────────
    //  Smoke Particle System
    // ─────────────────────────────────────────────

    void BuildSmokeParticles()
    {
        GameObject smokeGO = new GameObject("Smoke_Particles");
        smokeGO.transform.SetParent(transform, false);
        smokeGO.transform.localPosition = new Vector3(0f, smokeSpawnHeight, 0f);
        smokeGO.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        smokePS = smokeGO.AddComponent<ParticleSystem>();

        var main = smokePS.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2.5f, 4.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
        main.startSize = new ParticleSystem.MinMaxCurve(smokeMaxSize * 0.25f, smokeMaxSize * 0.55f);
        main.startColor = smokeColor;
        main.maxParticles = 60;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.05f;

        main.startRotation3D = true;
        main.startRotationX = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startRotationY = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startRotationZ = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);

        var emission = smokePS.emission;
        emission.rateOverTime = smokeEmissionRate;

        var shape = smokePS.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f;
        shape.radius = spreadRadius * 0.6f;

        Color smokeWarm = new Color(
            Mathf.Clamp01(smokeColor.r * 1.2f),
            Mathf.Clamp01(smokeColor.g * 1.05f),
            Mathf.Clamp01(smokeColor.b * 0.75f)
        );
        Color smokeLight = new Color(
            Mathf.Lerp(smokeColor.r, 1f, 0.45f),
            Mathf.Lerp(smokeColor.g, 1f, 0.45f),
            Mathf.Lerp(smokeColor.b, 1f, 0.45f)
        );
        float peakA = Mathf.Clamp(smokeColor.a, 0.01f, 1f);

        var col = smokePS.colorOverLifetime;
        col.enabled = true;
        Gradient sGrad = new Gradient();
        sGrad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(smokeWarm, 0.00f),
                new GradientColorKey(smokeColor, 0.35f),
                new GradientColorKey(smokeLight, 1.00f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f, 0.00f),
                new GradientAlphaKey(peakA, 0.12f),
                new GradientAlphaKey(peakA * 0.6f, 0.55f),
                new GradientAlphaKey(0f, 1.00f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(sGrad);

        var sizeOL = smokePS.sizeOverLifetime;
        sizeOL.enabled = true;
        sizeOL.size = new ParticleSystem.MinMaxCurve(smokeMaxSize,
            new AnimationCurve(
                new Keyframe(0f, 0.15f),
                new Keyframe(0.3f, 0.65f),
                new Keyframe(1f, 1.00f)
            ));

        var rotOL = smokePS.rotationOverLifetime;
        rotOL.enabled = true;
        rotOL.separateAxes = true;
        rotOL.x = new ParticleSystem.MinMaxCurve(-25f * Mathf.Deg2Rad, 25f * Mathf.Deg2Rad);
        rotOL.y = new ParticleSystem.MinMaxCurve(-20f * Mathf.Deg2Rad, 20f * Mathf.Deg2Rad);
        rotOL.z = new ParticleSystem.MinMaxCurve(-15f * Mathf.Deg2Rad, 15f * Mathf.Deg2Rad);

        var noise = smokePS.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        noise.frequency = 0.4f;
        noise.scrollSpeed = 0.2f;
        noise.quality = ParticleSystemNoiseQuality.Medium;

        var rend = smokePS.GetComponent<ParticleSystemRenderer>();
        rend.renderMode = ParticleSystemRenderMode.Mesh;
        rend.mesh = Resources.GetBuiltinResource(typeof(Mesh), "Cube.fbx") as Mesh;
        rend.sortingOrder = 0;

        Material sMat = new Material(Shader.Find("Sprites/Default"));
        sMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        sMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        sMat.renderQueue = 3000;
        rend.material = sMat;

        smokePS.Play();
    }

    // ─────────────────────────────────────────────
    //  Ember Particle System
    // ─────────────────────────────────────────────

    void BuildEmberParticles()
    {
        GameObject emberGO = new GameObject("Ember_Particles");
        emberGO.transform.SetParent(transform, false);

        emberPS = emberGO.AddComponent<ParticleSystem>();
        emberGO.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        var main = emberPS.main;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.08f);
        main.startColor = new Color(1f, 0.6f, 0.1f, 1f);
        main.maxParticles = 80;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.gravityModifier = -0.1f;

        var emission = emberPS.emission;
        emission.rateOverTime = emberEmissionRate;

        var shape = emberPS.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = spreadRadius * 0.5f;

        var col = emberPS.colorOverLifetime;
        col.enabled = true;
        Gradient eGrad = new Gradient();
        eGrad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f),
                new GradientColorKey(new Color(0.8f, 0.1f, 0f), 0.5f),
                new GradientColorKey(new Color(0.1f, 0.1f, 0.1f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(eGrad);

        var noise = emberPS.noise;
        noise.enabled = true;
        noise.strength = 0.8f;
        noise.frequency = 0.8f;
        noise.scrollSpeed = 1f;

        var renderer = emberPS.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        Material eMat = new Material(Shader.Find("Particles/Standard Unlit"));
        if (eMat.shader == null || eMat.shader.name == "Hidden/InternalErrorShader")
        {
            eMat = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
        }
        renderer.material = eMat;

        emberPS.Play();
    }

    // ─────────────────────────────────────────────
    //  Dynamic Light
    // ─────────────────────────────────────────────

    void BuildLight()
    {
        GameObject lightGO = new GameObject("Fire_Light");
        lightGO.transform.SetParent(transform, false);
        lightGO.transform.localPosition = new Vector3(0f, 0.5f, 0f);

        fireLight = lightGO.AddComponent<Light>();
        fireLight.type = LightType.Point;
        fireLight.color = lightColor;
        fireLight.intensity = lightIntensity;
        fireLight.range = lightRange;
        fireLight.shadows = LightShadows.Soft;
    }

    void UpdateFlicker()
    {
        float t = Time.time * flickerSpeed + flickerOffset;
        float n1 = Mathf.PerlinNoise(t, 0f);
        float n2 = Mathf.PerlinNoise(t * 2.3f, 10f) * 0.5f;
        float n3 = Mathf.PerlinNoise(t * 5.1f, 20f) * 0.25f;
        float combined = (n1 + n2 + n3) / 1.75f;

        fireLight.intensity = lightIntensity + (combined - 0.5f) * 2f * flickerAmount;
        fireLight.range = lightRange + (combined - 0.5f) * 0.5f;
        fireLight.color = Color.Lerp(
            new Color(1f, 0.35f, 0.1f),
            new Color(1f, 0.75f, 0.3f),
            combined
        );
    }

    // ─────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────

    /// <summary>Pause all fire effects.</summary>
    public void PauseFire()
    {
        if (firePS != null) firePS.Pause();
        if (smokePS != null) smokePS.Pause();
        if (emberPS != null) emberPS.Pause();
        if (fireLight != null) fireLight.enabled = false;
    }

    /// <summary>Resume all fire effects.</summary>
    public void ResumeFire()
    {
        if (firePS != null) firePS.Play();
        if (smokePS != null) smokePS.Play();
        if (emberPS != null) emberPS.Play();
        if (fireLight != null) fireLight.enabled = true;
    }

    /// <summary>Gradually extinguish the fire over a given duration.</summary>
    public void Extinguish(float duration = 2f)
    {
        StartCoroutine(ExtinguishRoutine(duration));
    }

    private IEnumerator ExtinguishRoutine(float duration)
    {
        float elapsed = 0f;
        float startRate = emissionRate;
        float startIntensity = lightIntensity;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            var emission = firePS.emission;
            var rate = emission.rateOverTime;
            rate.constant = Mathf.Lerp(startRate, 0f, t);
            emission.rateOverTime = rate;

            if (enableEmbers && emberPS != null)
            {
                var eEmission = emberPS.emission;
                var eRate = eEmission.rateOverTime;
                eRate.constant = Mathf.Lerp(emberEmissionRate, 0f, t);
                eEmission.rateOverTime = eRate;
            }

            if (enableSmoke && smokePS != null)
            {
                float smokeT = Mathf.Clamp01(t * 1.5f - 0.3f);
                var sEmission = smokePS.emission;
                var sRate = sEmission.rateOverTime;
                sRate.constant = Mathf.Lerp(smokeEmissionRate * 1.4f, 0f, smokeT);
                sEmission.rateOverTime = sRate;
            }

            if (enableLight && fireLight)
                fireLight.intensity = Mathf.Lerp(startIntensity, 0f, t);

            yield return null;
        }

        if (firePS != null) firePS.Stop();
        if (smokePS != null) smokePS.Stop();
        if (emberPS != null) emberPS.Stop();
        if (fireLight != null) fireLight.enabled = false;
    }
}
