using UnityEngine;

[DisallowMultipleComponent]
public class DungeonBuilder : MonoBehaviour
{
    public int width = 14;
    public int depth = 14;
    public float wallHeight = 3.2f;
    public float wallThickness = 0.6f;
    public Color wallColor = new Color(0.18f, 0.18f, 0.18f);
    public Color floorColor = new Color(0.12f, 0.12f, 0.12f);
    public bool createDefaultLight = true;
    public bool spawnDefaultTorch = true;
    public bool showInstructions = true;

    private bool built;

    private void Start()
    {
        if (built)
            return;

        EnsurePlayerController();
        EnsureMainCamera();

        if (createDefaultLight)
            EnsureDirectionalLight();

        if (spawnDefaultTorch)
            EnsureTorchObject();

        BuildDungeon();
        built = true;
    }

    private void EnsurePlayerController()
    {
        if (!TryGetComponent<PlayerController>(out var controller))
        {
            controller = gameObject.AddComponent<PlayerController>();
        }

        if (controller.torchHolder == null)
        {
            GameObject torchHolder = new GameObject("TorchHolder");
            torchHolder.transform.SetParent(transform, false);
            torchHolder.transform.localPosition = new Vector3(0.2f, 0.4f, 0.5f);
            torchHolder.transform.localRotation = Quaternion.identity;
            controller.torchHolder = torchHolder.transform;
        }
    }

    private void EnsureMainCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            mainCam = cameraGO.AddComponent<Camera>();
            mainCam.clearFlags = CameraClearFlags.Skybox;
            mainCam.fieldOfView = 60f;
            cameraGO.transform.SetParent(transform, false);
            cameraGO.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            cameraGO.transform.localRotation = Quaternion.identity;
        }
        else if (mainCam.transform.parent != transform)
        {
            mainCam.transform.SetParent(transform, false);
            mainCam.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            mainCam.transform.localRotation = Quaternion.identity;
        }
    }

    private void EnsureDirectionalLight()
    {
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light existingLight in allLights)
        {
            if (existingLight != null && existingLight.type == LightType.Directional)
                return;
        }

        GameObject lightGO = new GameObject("Directional Light");
        lightGO.transform.SetParent(transform, false);
        lightGO.transform.localPosition = new Vector3(0f, 6f, -3f);
        lightGO.transform.localRotation = Quaternion.Euler(50f, 30f, 0f);

        Light directional = lightGO.AddComponent<Light>();
        directional.type = LightType.Directional;
        directional.color = new Color(1f, 0.95686275f, 0.8392157f);
        directional.intensity = 1f;
        directional.shadows = LightShadows.Soft;
    }

    private void EnsureTorchObject()
    {
        TorchPickup existingTorch = FindObjectOfType<TorchPickup>();
        if (existingTorch != null)
            return;

        GameObject torch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        torch.name = "DefaultTorch";
        torch.transform.SetParent(transform.parent, false);
        torch.transform.position = transform.position + transform.forward * 2f + Vector3.up * 0.5f;
        torch.transform.localScale = new Vector3(0.2f, 0.6f, 0.2f);

        Collider col = torch.GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;

        torch.AddComponent<TorchPickup>();
        torch.AddComponent<TorchLight>();
        torch.AddComponent<FireSimulation>();

        Rigidbody rb = torch.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.mass = 0.5f;
    }

    private void BuildDungeon()
    {
        CreateFloor();
        CreateWalls();
    }

    private void CreateFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "DungeonFloor";
        floor.transform.SetParent(transform, false);
        floor.transform.localPosition = new Vector3(0f, -0.1f, 0f);
        floor.transform.localScale = new Vector3(width, 0.2f, depth);
        SetStoneMaterial(floor, floorColor);
    }

    private void CreateWalls()
    {
        CreateWall("DungeonWall_North", new Vector3(0f, wallHeight * 0.5f, depth * 0.5f), new Vector3(width + wallThickness, wallHeight, wallThickness));
        CreateWall("DungeonWall_South", new Vector3(0f, wallHeight * 0.5f, -depth * 0.5f), new Vector3(width + wallThickness, wallHeight, wallThickness));
        CreateWall("DungeonWall_East", new Vector3(width * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, depth));
        CreateWall("DungeonWall_West", new Vector3(-width * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, depth));
    }

    private void CreateWall(string name, Vector3 localPosition, Vector3 localScale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(transform, false);
        wall.transform.localPosition = localPosition;
        wall.transform.localScale = localScale;
        SetStoneMaterial(wall, wallColor);
    }

    private void SetStoneMaterial(GameObject target, Color baseColor)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
            return;

        Material material = new Material(Shader.Find("Standard"));
        if (material == null)
        {
            material = new Material(Shader.Find("Diffuse"));
        }

        material.color = baseColor;
        renderer.material = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;
    }

    private void OnGUI()
    {
        if (!showInstructions)
            return;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 18;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 600, 30), "WASD = move, mouse = look, E = pickup, Q = drop, F = toggle torch", style);
        GUI.Label(new Rect(10, 35, 600, 30), "If the scene is still dark, try walking toward the lit torch in front of you.", style);
    }
}
