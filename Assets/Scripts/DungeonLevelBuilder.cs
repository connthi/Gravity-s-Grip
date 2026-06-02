using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DungeonLevelBuilder : MonoBehaviour
{
    public bool buildOnStart = true;
    public bool createUI = true;
    public float wallHeight = 3.5f;
    public float wallThickness = 0.5f;
    public Color stoneColor = new Color(0.16f, 0.16f, 0.16f);
    public Color panelColor = new Color(0.12f, 0.44f, 0.74f);
    public Color doorColor = new Color(0.08f, 0.08f, 0.08f);
    public Color cubeColor = new Color(0.28f, 0.16f, 0.08f);
    public Color torchStandColor = new Color(0.12f, 0.08f, 0.04f);

    private bool built;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureSceneBootstrapper()
    {
        if (FindAnyObjectByType<DungeonLevelBuilder>() == null)
        {
            GameObject bootstrapper = new GameObject("SceneBootstrapper");
            DungeonLevelBuilder builder = bootstrapper.AddComponent<DungeonLevelBuilder>();
            builder.buildOnStart = true;
            builder.createUI = true;
        }
    }

    private void Start()
    {
        if (buildOnStart)
        {
            BuildDungeon();
        }
    }

    [ContextMenu("Build Dungeon Layout")]
    public void BuildDungeon()
    {
        if (built)
            return;

        CleanupExistingDungeon();

        GameObject root = new GameObject("GeneratedDungeon");
        root.transform.SetParent(transform, false);

        CreateLighting(root.transform);
        CreatePlayerStart(root.transform);
        PuzzleManager manager = CreatePuzzleManager(root.transform);
        if (createUI && manager != null)
        {
            UIManager uiManager = CreateUI(root.transform);
            manager.uiManager = uiManager;
            // Ensure any existing player controllers get a reference to the HUD
            PlayerController existingPlayer = FindAnyObjectByType<PlayerController>();
            if (existingPlayer != null)
                existingPlayer.uiManager = uiManager;
        }
        CreateIntroductoryRoom(root.transform);
        CreateGravityCubeRoom(root.transform);

        built = true;
    }

    private void CleanupExistingDungeon()
    {
        GameObject existing = GameObject.Find("GeneratedDungeon");
        if (existing != null)
        {
            if (Application.isPlaying)
                Destroy(existing);
            else
                DestroyImmediate(existing);
        }
    }

    private void CreateLighting(Transform parent)
    {
        GameObject lightGO = new GameObject("Directional Light");
        lightGO.transform.SetParent(parent, false);
        lightGO.transform.position = new Vector3(0f, 6f, -4f);
        lightGO.transform.rotation = Quaternion.Euler(50f, 30f, 0f);

        Light directional = lightGO.AddComponent<Light>();
        directional.type = LightType.Directional;
        directional.color = new Color(1f, 0.96f, 0.84f);
        // Slightly darker, dungeon-y lighting
        directional.intensity = 0.6f;
        directional.shadows = LightShadows.Soft;

        GameObject fillGO = new GameObject("Ambient Fill Light");
        fillGO.transform.SetParent(parent, false);
        fillGO.transform.position = new Vector3(0f, 2f, 0f);

        Light fill = fillGO.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.range = 10f;
        fill.intensity = 0.15f;
        fill.color = new Color(0.8f, 0.9f, 1f);

        // Reduce global ambient to make rooms darker
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.06f, 0.06f, 0.06f);
    }

    private void CreatePlayerStart(Transform parent)
    {
        if (FindAnyObjectByType<PlayerController>() != null)
            return;

        GameObject playerGO = new GameObject("Player");
        playerGO.transform.SetParent(parent, false);
        playerGO.transform.position = new Vector3(-6f, 1.1f, 0f);

        PlayerController player = playerGO.AddComponent<PlayerController>();
        player.moveSpeed = 4f;
        player.lookSensitivity = 2f;
        player.jumpForce = 5f;

        CharacterController controller = playerGO.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.3f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        controller.stepOffset = 0.3f;
        controller.slopeLimit = 45f;
    }

    private PuzzleManager CreatePuzzleManager(Transform parent)
    {
        if (FindAnyObjectByType<PuzzleManager>() != null)
            return FindAnyObjectByType<PuzzleManager>();

        GameObject managerGO = new GameObject("PuzzleManager");
        managerGO.transform.SetParent(parent, false);
        PuzzleManager manager = managerGO.AddComponent<PuzzleManager>();
        manager.requiredPuzzlesToWin = 2;
        return manager;
    }

    private void CreateIntroductoryRoom(Transform parent)
    {
        Vector3 roomOffset = new Vector3(0f, 0f, 0f);
        float roomWidth = 8f;
        float roomDepth = 8f;

        GameObject room = new GameObject("IntroRoom");
        room.transform.SetParent(parent, false);
        room.transform.localPosition = roomOffset;

        CreateRoomShell(room.transform, roomWidth, roomDepth, "IntroRoomShell");

        // Nudge the torch slightly inward so it doesn't spawn inside the wall
        Vector3 torchPos = new Vector3(-3.5f, 0.45f, 1f);
        CreateTorchStation(room.transform, torchPos, "Start Torch", true);

        Vector3 panel90Position = new Vector3(-1.5f, 1.5f, 3.5f);
        CreateGravityPanel(room.transform, "GravityPanel_90deg", panel90Position, Quaternion.Euler(0f, 180f, 0f), Vector3.right, "90°");

        Vector3 panel180Position = new Vector3(2.2f, 1.5f, 3.5f);
        CreateGravityPanel(room.transform, "GravityPanel_180deg", panel180Position, Quaternion.Euler(0f, 180f, 0f), Vector3.down, "180°");

        CreateGrate(room.transform, new Vector3(1.4f, 0.05f, 2f), new Vector3(2.4f, 0.05f, 2.4f));

        PuzzleDoor door = CreateDoor(room.transform, new Vector3(4f, 1.2f, 0f), Quaternion.Euler(0f, 90f, 0f), "IntroDoor");
        CreateDoorSwitch(room.transform, new Vector3(1.5f, 1.1f, 3.4f), door, "Light the photocell with your torch.");
    }

    private void CreateGravityCubeRoom(Transform parent)
    {
        Vector3 roomOffset = new Vector3(14.5f, 0f, 0f);
        float roomWidth = 10f;
        float roomDepth = 9f;

        GameObject room = new GameObject("GravityCubeRoom");
        room.transform.SetParent(parent, false);
        room.transform.localPosition = roomOffset;

        CreateRoomShell(room.transform, roomWidth, roomDepth, "CubeRoomShell");

        Vector3 cubeStart = new Vector3(1.8f, 0.5f, -2.5f);
        CreateGravityCube(room.transform, cubeStart);

        Vector3 wallPanel90 = new Vector3(-3.8f, 1.4f, -1.2f);
        CreateGravityPanel(room.transform, "CubeRoomPanel_90", wallPanel90, Quaternion.Euler(0f, 90f, 0f), Vector3.up, "90°");

        Vector3 ceilingPanel = new Vector3(0.5f, 3.1f, 1.8f);
        CreateGravityPanel(room.transform, "CubeRoomPanel_180", ceilingPanel, Quaternion.Euler(90f, 0f, 0f), Vector3.down, "180°");

        CreatePlatform(room.transform, new Vector3(0.5f, 2.6f, 1.8f), new Vector3(2.4f, 0.3f, 2.4f), "CeilingPlatform");
        CreatePlatform(room.transform, new Vector3(-2.4f, 1.1f, 3f), new Vector3(1.8f, 0.3f, 3.4f), "LandingPlatform");

        PuzzleDoor door = CreateDoor(room.transform, new Vector3(10f, 1.2f, 0f), Quaternion.Euler(0f, 90f, 0f), "ExitDoor");
        CreateDoorSwitch(room.transform, new Vector3(6.2f, 1.1f, -1.4f), door, "Use your torch to open the exit.");
        // Create an exit trigger in front of the door that only allows leaving when puzzles complete
        GameObject exitTrigger = new GameObject("ExitTrigger");
        exitTrigger.transform.SetParent(room.transform, false);
        exitTrigger.transform.localPosition = new Vector3(9.2f, 0.5f, 0f);
        BoxCollider exitCol = exitTrigger.AddComponent<BoxCollider>();
        exitCol.isTrigger = true;
        exitCol.size = new Vector3(2f, 2f, 2f);
        PuzzleExitTrigger pet = exitTrigger.AddComponent<PuzzleExitTrigger>();
    }

    private void CreateRoomShell(Transform parent, float width, float depth, string name)
    {
        CreateFloor(parent, new Vector3(0f, 0f, 0f), new Vector3(width, 0.2f, depth));
        CreateWall(parent, new Vector3(0f, wallHeight * 0.5f, depth * 0.5f), new Vector3(width + wallThickness, wallHeight, wallThickness));
        CreateWall(parent, new Vector3(0f, wallHeight * 0.5f, -depth * 0.5f), new Vector3(width + wallThickness, wallHeight, wallThickness));
        CreateWall(parent, new Vector3(width * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, depth));
        CreateWall(parent, new Vector3(-width * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, depth));
        // Add a ceiling to close the room and improve visuals
        CreateCeiling(parent, width, depth);
    }

    private void CreateCeiling(Transform parent, float width, float depth)
    {
        // Try to use an imported ceiling model from Resources (WallA), fall back to cube primitive
        GameObject ceilingPrefab = LoadModelResource("WallA", "wallA", "ceiling");
        GameObject ceiling;
        if (ceilingPrefab != null)
        {
            ceiling = Instantiate(ceilingPrefab, parent);
            ceiling.name = "Ceiling";
            ceiling.transform.localPosition = new Vector3(0f, wallHeight, 0f);
            ceiling.transform.localRotation = Quaternion.identity;
            ceiling.transform.localScale = new Vector3(width + wallThickness, 0.2f, depth + wallThickness);
            ApplyColor(ceiling, stoneColor);
        }
        else
        {
            ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceiling.name = "Ceiling";
            ceiling.transform.SetParent(parent, false);
            // Place the ceiling at the top of the walls
            ceiling.transform.localPosition = new Vector3(0f, wallHeight, 0f);
            ceiling.transform.localScale = new Vector3(width + wallThickness, 0.2f, depth + wallThickness);
            ApplyColor(ceiling, stoneColor);
        }
    }

    private void CreateFloor(Transform parent, Vector3 center, Vector3 scale)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(parent, false);
        floor.transform.localPosition = center;
        floor.transform.localScale = scale;
        ApplyColor(floor, stoneColor);
    }

    private void CreateWall(Transform parent, Vector3 center, Vector3 scale)
    {
        // Try to use an imported wall model from Resources (WallB), fall back to cube primitive
        GameObject wallPrefab = LoadModelResource("WallB", "wallB", "wall");
        GameObject wall;
        if (wallPrefab != null)
        {
            wall = Instantiate(wallPrefab, parent);
            wall.name = "Wall";
            wall.transform.localPosition = center;
            wall.transform.localRotation = Quaternion.identity;
            wall.transform.localScale = scale;
            ApplyColor(wall, stoneColor);
        }
        else
        {
            wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "Wall";
            wall.transform.SetParent(parent, false);
            wall.transform.localPosition = center;
            wall.transform.localScale = scale;
            ApplyColor(wall, stoneColor);
        }
    }

    private GameObject LoadModelResource(params string[] names)
    {
        foreach (string n in names)
        {
            if (string.IsNullOrEmpty(n))
                continue;

            GameObject prefab = Resources.Load<GameObject>(n);
            if (prefab != null)
                return prefab;
        }

        return null;
    }

    private void CreatePlatform(Transform parent, Vector3 center, Vector3 scale, string name)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = name;
        platform.transform.SetParent(parent, false);
        platform.transform.localPosition = center;
        platform.transform.localScale = scale;
        ApplyColor(platform, stoneColor * 0.9f);
    }

    private PuzzleDoor CreateDoor(Transform parent, Vector3 position, Quaternion rotation, string name)
    {
        GameObject doorGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        doorGO.name = name;
        doorGO.transform.SetParent(parent, false);
        doorGO.transform.localPosition = position;
        doorGO.transform.localRotation = rotation;
        doorGO.transform.localScale = new Vector3(0.4f, 2.4f, 2.2f);
        ApplyColor(doorGO, doorColor);

        GameObject closed = new GameObject(name + "_Closed");
        closed.transform.SetParent(parent, false);
        closed.transform.localPosition = position;
        closed.transform.localRotation = rotation;

        GameObject open = new GameObject(name + "_Open");
        open.transform.SetParent(parent, false);
        open.transform.localPosition = position + new Vector3(1.5f, 0f, 0f);
        open.transform.localRotation = rotation;

        PuzzleDoor door = doorGO.AddComponent<PuzzleDoor>();
        door.closedPosition = closed.transform;
        door.openPosition = open.transform;
        door.openSpeed = 2.5f;

        return door;
    }

    private void CreateDoorSwitch(Transform parent, Vector3 position, PuzzleDoor door, string objectiveText)
    {
        GameObject trigger = new GameObject("DoorSwitch");
        trigger.transform.SetParent(parent, false);
        trigger.transform.localPosition = position;

        BoxCollider collider = trigger.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = new Vector3(1.6f, 2.2f, 1.6f);

        DoorSwitch switchComponent = trigger.AddComponent<DoorSwitch>();
        switchComponent.targetDoor = door;
        switchComponent.requireLitTorch = true;
        switchComponent.allowPlayerCarry = true;

        PuzzleObjective objective = trigger.AddComponent<PuzzleObjective>();
        objective.title = "Activate the switch";
        objective.description = objectiveText;
    }

    private void CreateGravityPanel(Transform parent, string name, Vector3 position, Quaternion rotation, Vector3 gravityDirection, string label)
    {
        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = name;
        panel.transform.SetParent(parent, false);
        panel.transform.localPosition = position;
        panel.transform.localRotation = rotation;
        panel.transform.localScale = new Vector3(2.2f, 2.2f, 0.2f);
        ApplyColor(panel, panelColor);

        BoxCollider collider = panel.GetComponent<BoxCollider>();
        if (collider != null)
            collider.isTrigger = true;

        GravityPanel gravityPanel = panel.AddComponent<GravityPanel>();
        gravityPanel.gravityDirection = gravityDirection;
        gravityPanel.affectPlayer = true;
        gravityPanel.affectPhysicsObjects = true;
    }

    private void CreateGrate(Transform parent, Vector3 position, Vector3 size)
    {
        GameObject grate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grate.name = "Grate";
        grate.transform.SetParent(parent, false);
        grate.transform.localPosition = position;
        grate.transform.localScale = size;
        ApplyColor(grate, stoneColor * 0.75f);
    }

    private GameObject CreateTorchStation(Transform parent, Vector3 position, string name, bool lit)
    {
        GameObject stand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stand.name = name + " Stand";
        stand.transform.SetParent(parent, false);
        stand.transform.localPosition = position;
        stand.transform.localScale = new Vector3(0.3f, 0.25f, 0.3f);
        ApplyColor(stand, torchStandColor);

        GameObject torch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        torch.name = name;
        torch.transform.SetParent(parent, false);
        torch.transform.localPosition = position + new Vector3(0f, 0.5f, 0f);
        torch.transform.localScale = new Vector3(0.2f, 0.6f, 0.2f);

        SphereCollider collider = torch.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.35f;

        TorchPickup pickup = torch.AddComponent<TorchPickup>();
        pickup.startLit = lit;
        pickup.autoIgniteOnPickup = true;
        pickup.pickupRange = 3f;
        pickup.burnRate = 0.5f;
        pickup.maxFuel = 120f;

        torch.AddComponent<TorchLight>();
        torch.AddComponent<FireSimulation>();

        ApplyColor(torch, new Color(0.18f, 0.12f, 0.08f));

        return torch;
    }

    private void CreateGravityCube(Transform parent, Vector3 position)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "GravityCube";
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = position;
        cube.transform.localScale = new Vector3(1f, 1f, 1f);

        Rigidbody rb = cube.AddComponent<Rigidbody>();
        rb.mass = 2f;
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.None;

        GravityAffectedObject gravityObject = cube.AddComponent<GravityAffectedObject>();
        gravityObject.gravityDirection = Vector3.down;
        gravityObject.gravityStrength = 9.81f;

        ApplyColor(cube, cubeColor);
    }

    private UIManager CreateUI(Transform parent)
    {
        GameObject canvasGO = new GameObject("HUD Canvas");
        canvasGO.transform.SetParent(parent, false);

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        GameObject hudPanel = new GameObject("HUD Panel");
        hudPanel.transform.SetParent(canvasGO.transform, false);

        RectTransform hudRect = hudPanel.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 1f);
        hudRect.anchorMax = new Vector2(0f, 1f);
        hudRect.pivot = new Vector2(0f, 1f);
        hudRect.anchoredPosition = new Vector2(10f, -10f);
        hudRect.sizeDelta = new Vector2(380f, 140f);

        Image hudImage = hudPanel.AddComponent<Image>();
        hudImage.color = new Color(0f, 0f, 0f, 0.45f);

        UIManager uiManager = hudPanel.AddComponent<UIManager>();
        uiManager.hudPanel = hudPanel;

        uiManager.objectiveTitleText = CreateUIText(hudPanel.transform, "Objective", new Vector2(10f, -10f), 20, TextAnchor.UpperLeft);
        uiManager.objectiveDescriptionText = CreateUIText(hudPanel.transform, "Description", new Vector2(10f, -35f), 16, TextAnchor.UpperLeft);
        uiManager.progressText = CreateUIText(hudPanel.transform, "Puzzles: 0/0", new Vector2(10f, -75f), 16, TextAnchor.UpperLeft);
        uiManager.torchStatusText = CreateUIText(hudPanel.transform, "Torch: Out", new Vector2(10f, -100f), 16, TextAnchor.UpperLeft);
        uiManager.hintText = CreateUIText(hudPanel.transform, "Hint: Use your torch to power doors.", new Vector2(10f, -122f), 14, TextAnchor.UpperLeft);

        if (uiManager.objectiveTitleText == null || uiManager.objectiveDescriptionText == null || uiManager.progressText == null || uiManager.torchStatusText == null || uiManager.hintText == null)
        {
            Debug.LogWarning("DungeonLevelBuilder: UI text creation failed. Check that Unity UI is available.");
        }

        GameObject winPanel = new GameObject("Win Panel");
        winPanel.transform.SetParent(canvasGO.transform, false);

        RectTransform winRect = winPanel.AddComponent<RectTransform>();
        winRect.anchorMin = new Vector2(0.5f, 0.5f);
        winRect.anchorMax = new Vector2(0.5f, 0.5f);
        winRect.pivot = new Vector2(0.5f, 0.5f);
        winRect.anchoredPosition = Vector2.zero;
        winRect.sizeDelta = new Vector2(600f, 220f);

        Image winImage = winPanel.AddComponent<Image>();
        winImage.color = new Color(0f, 0f, 0f, 0.75f);

        Text winText = CreateUIText(winPanel.transform, "Puzzle Complete!", new Vector2(0f, 0f), 32, TextAnchor.MiddleCenter);
        winText.color = Color.yellow;
        uiManager.winPanel = winPanel;
        winPanel.SetActive(false);

        return uiManager;
    }

    private Text CreateUIText(Transform parent, string text, Vector2 position, int fontSize, TextAnchor anchor)
    {
        GameObject textGO = new GameObject("HUD_Text_" + text);
        textGO.transform.SetParent(parent, false);
        // ensure this text is rendered above the background panel
        textGO.transform.SetAsLastSibling();

        RectTransform rect = textGO.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(380f, 32f);

        Text uiText = textGO.AddComponent<Text>();
        uiText.text = text;
        uiText.fontSize = fontSize;
        uiText.alignment = anchor;
        uiText.color = Color.white;
        // Use LegacyRuntime.ttf (valid built-in) and fall back to OS fonts if missing
        uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (uiText.font == null)
            uiText.font = Font.CreateDynamicFontFromOSFont("Arial", fontSize);
        if (uiText.font == null)
            uiText.font = Font.CreateDynamicFontFromOSFont("Helvetica", fontSize);

        uiText.enabled = true;
        uiText.fontStyle = FontStyle.Normal;

        uiText.horizontalOverflow = HorizontalWrapMode.Wrap;
        uiText.verticalOverflow = VerticalWrapMode.Truncate;
        uiText.raycastTarget = false;
        uiText.resizeTextForBestFit = false;

        return uiText;
    }

    private void CreateLabel(Transform parent, string labelText, Vector3 localPosition, float size)
    {
        // Intentional no-op: world labels were creating too much visual clutter.
    }

    private void ApplyColor(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
            return;

        Material material = new Material(Shader.Find("Standard"));
        material.color = color;
        // Reduce metallic/smoothness to avoid blotchy/specular artifacts
        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", 0f);
        if (material.HasProperty("_Glossiness"))
            material.SetFloat("_Glossiness", 0.25f);
        renderer.material = material;
    }
}
