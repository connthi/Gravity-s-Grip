using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Single authoritative scene builder. Replaces both DungeonBuilder and
/// DungeonLevelBuilder. Add this component to one GameObject in the scene
/// and it builds everything at runtime start. No [RuntimeInitializeOnLoad]
/// auto-spawning — whoever owns the scene sets this up explicitly.
/// </summary>
[DisallowMultipleComponent]
public class LevelBuilder : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Dimensions")]
    [SerializeField] private float wallHeight    = 3.5f;
    [SerializeField] private float wallThickness = 0.5f;

    [Header("Colors")]
    [SerializeField] private Color stoneColor     = new Color(0.16f, 0.16f, 0.16f);
    [SerializeField] private Color panelColor     = new Color(0.12f, 0.44f, 0.74f);
    [SerializeField] private Color doorColor      = new Color(0.08f, 0.08f, 0.08f);
    [SerializeField] private Color cubeColor      = new Color(0.28f, 0.16f, 0.08f);
    [SerializeField] private Color torchStandColor= new Color(0.12f, 0.08f, 0.04f);

    [Header("Puzzle settings")]
    [SerializeField] private int puzzlesToWin = 1;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────

    private void Start()
    {
        EnsureTag("PuzzleBlock");
        EnsureSingletons();
        BuildScene();
    }

    private static void EnsureTag(string tag)
    {
#if UNITY_EDITOR
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProp   = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
#endif
    }

    // ── Bootstrap Singletons ──────────────────────────────────────────────────

    private void EnsureSingletons()
    {
        // GameManager
        if (GameManager.Instance == null)
            new GameObject("GameManager").AddComponent<GameManager>();

        // Player (only if one doesn't exist yet)
        if (FindAnyObjectByType<PlayerController>() == null)
            CreatePlayer();

        // ObjectiveTracker
        if (ObjectiveTracker.Instance == null)
        {
            var go      = new GameObject("ObjectiveTracker");
            var tracker = go.AddComponent<ObjectiveTracker>();
            // We need to set requiredToWin via serialized field; use
            // a helper shim since it's [SerializeField] private.
            // Alternatively set it in the Inspector on this LevelBuilder.
        }

        // UIManager
        if (UIManager.Instance == null)
            BuildHUD();
    }

    private void CreatePlayer()
    {
        var go = new GameObject("Player");
        go.transform.position = new Vector3(-10f, 1.1f, 0f);

        var cc    = go.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0f, 0.9f, 0f);

        go.AddComponent<PlayerController>();
        go.AddComponent<PlayerGrabber>();
    }

    // ── Scene Construction ────────────────────────────────────────────────────

    private void BuildScene()
    {
        var root = new GameObject("Level");

        CreateLighting(root.transform);
        CreateIntroRoom(root.transform);
        CreateGravityCubeRoom(root.transform);
    }

    // ── Lighting ──────────────────────────────────────────────────────────────

    private void CreateLighting(Transform parent)
    {
        var go = new GameObject("Directional Light");
        go.transform.SetParent(parent, false);
        go.transform.rotation = Quaternion.Euler(50f, 30f, 0f);

        var l       = go.AddComponent<Light>();
        l.type      = LightType.Directional;
        l.color     = new Color(1f, 0.96f, 0.84f);
        l.intensity = 0.6f;
        l.shadows   = LightShadows.Soft;

        RenderSettings.ambientMode  = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.06f, 0.06f, 0.06f);
    }

    // ── Room 1: Introduction ──────────────────────────────────────────────────

    private void CreateIntroRoom(Transform parent)
    {
        const float W = 20f, D = 18f;
        var room = NewRoom("IntroRoom", parent, Vector3.zero);
        // East wall IS built here; the door opening punches through it visually via PuzzleDoor sliding up
        BuildRoomShell(room, W, D);

        // Torch near spawn — auto-lights when player walks close (autoPickupRadius)
        CreateTorchStation(room, new Vector3(-8f, 0.45f, 0f), "StartTorch", lit: true);

        // Platforms to explore
        CreatePlatform(room, new Vector3(-5f, 1.5f, -4f), new Vector3(3f, 0.3f, 3f), "Platform_A");
        CreatePlatform(room, new Vector3(2f,  2.2f,  4f), new Vector3(3f, 0.3f, 3f), "Platform_B");
        CreatePlatform(room, new Vector3(5f,  1.0f, -6f), new Vector3(3f, 0.3f, 3f), "Platform_C");

        // Walk-through gravity volumes (columns the player walks into, not thin walls)
        CreateGravityVolume(room, "GravVol_Right", new Vector3(-2f, wallHeight * 0.5f, 2f),
            new Vector3(2.5f, wallHeight, 2.5f), Vector3.right);
        CreateGravityVolume(room, "GravVol_Down",  new Vector3(3f,  wallHeight * 0.5f, -3f),
            new Vector3(2.5f, wallHeight, 2.5f), Vector3.down);

        // Door to puzzle room — closed, opens when player walks into the trigger zone in front of it
        var door = CreateDoor(room, new Vector3(W * 0.5f - 0.3f, 1.2f, 0f),
            Quaternion.Euler(0f, 90f, 0f), "IntroDoor");
        CreateProximitySwitch(room, new Vector3(W * 0.5f - 2f, 1.1f, 0f), door);
    }

    // ── Room 2: Puzzle Room ───────────────────────────────────────────────────

    private void CreateGravityCubeRoom(Transform parent)
    {
        const float W = 24f, D = 22f;
        // Offset so the shared wall between rooms aligns: room1 east wall = room2 west wall = x=10
        var room = NewRoom("PuzzleRoom", parent, new Vector3(22f, 0f, 0f));
        // Omit west wall — shared with intro room (intro room's east wall serves as the divider)
        BuildRoomShell(room, W, D, omitWest: true);

        CreateGravityCube(room, new Vector3(2f, 0.5f, -4f));

        // Walk-through gravity volumes
        CreateGravityVolume(room, "GravVol_Up",    new Vector3(-7f, wallHeight * 0.5f, -6f),
            new Vector3(3f, wallHeight, 3f), Vector3.up);
        CreateGravityVolume(room, "GravVol_Down",  new Vector3(4f,  wallHeight * 0.5f,  4f),
            new Vector3(3f, wallHeight, 3f), Vector3.down);
        CreateGravityVolume(room, "GravVol_Right", new Vector3(-9f, wallHeight * 0.5f,  3f),
            new Vector3(3f, wallHeight, 3f), Vector3.right);

        // Platforms reachable after gravity flips
        CreatePlatform(room, new Vector3(4f,   wallHeight - 0.4f,  4f), new Vector3(4f, 0.3f, 4f), "CeilingPlatform");
        CreatePlatform(room, new Vector3(-6f,  1.1f,  7f),              new Vector3(4f, 0.3f, 4f), "MidPlatform_A");
        CreatePlatform(room, new Vector3(5f,   2.5f, -7f),              new Vector3(3f, 0.3f, 3f), "MidPlatform_B");
        CreatePlatform(room, new Vector3(-9f,  1.0f, -3f),              new Vector3(3f, 0.3f, 5f), "LandingPlatform");

        // Second torch
        CreateTorchStation(room, new Vector3(-9f, 0.45f, 0f), "PuzzleTorch", lit: false);

        // Locked exit door
        var exitDoor = CreateDoor(room, new Vector3(W * 0.5f - 0.3f, 1.2f, 0f),
            Quaternion.Euler(0f, 90f, 0f), "ExitDoor");

        // Torch switch near exit door — bring lit torch to open
        CreateDoorSwitch(room, new Vector3(8f, 1.1f, -2f), exitDoor,
            "Light the Way", "Bring a lit torch to the glowing switch near the east wall.");

        CreateExitTrigger(room, new Vector3(W * 0.5f + 1f, 1f, 0f));
    }

    // ── Room Shell ────────────────────────────────────────────────────────────

    private void BuildRoomShell(Transform parent, float w, float d,
        bool omitEast = false, bool omitWest = false)
    {
        CreateBox(parent, "Floor",   new Vector3(0f, 0f, 0f),               new Vector3(w, 0.2f, d), stoneColor);
        CreateBox(parent, "Ceiling", new Vector3(0f, wallHeight, 0f),       new Vector3(w + wallThickness, 0.2f, d + wallThickness), stoneColor);
        CreateBox(parent, "WallN",   new Vector3(0f, wallHeight / 2f,  d / 2f), new Vector3(w + wallThickness, wallHeight, wallThickness), stoneColor);
        CreateBox(parent, "WallS",   new Vector3(0f, wallHeight / 2f, -d / 2f), new Vector3(w + wallThickness, wallHeight, wallThickness), stoneColor);
        if (!omitEast)
            CreateBox(parent, "WallE", new Vector3(w / 2f, wallHeight / 2f, 0f), new Vector3(wallThickness, wallHeight, d), stoneColor);
        if (!omitWest)
            CreateBox(parent, "WallW", new Vector3(-w / 2f, wallHeight / 2f, 0f), new Vector3(wallThickness, wallHeight, d), stoneColor);
    }

    // ── Object Factories ──────────────────────────────────────────────────────

    private PuzzleDoor CreateDoor(Transform parent, Vector3 pos, Quaternion rot, string doorName)
    {
        var doorGO = CreateBox(parent, doorName, pos, new Vector3(0.4f, 2.4f, 2.2f), doorColor);
        doorGO.transform.localRotation = rot;

        var closed = new GameObject(doorName + "_Closed");
        closed.transform.SetParent(parent, false);
        closed.transform.localPosition = pos;
        closed.transform.localRotation = rot;

        var open = new GameObject(doorName + "_Open");
        open.transform.SetParent(parent, false);
        open.transform.localPosition = pos + new Vector3(0f, 2.5f, 0f); // slides up
        open.transform.localRotation = rot;

        var door           = doorGO.AddComponent<PuzzleDoor>();
        // PuzzleDoor reads these via Inspector fields set here in code.
        SetPuzzleDoorTargets(door, closed.transform, open.transform);
        return door;
    }

    // PuzzleDoor has [SerializeField] fields; use a helper to set them.
    private static void SetPuzzleDoorTargets(PuzzleDoor door, Transform closed, Transform open)
    {
        // Because the fields are SerializeField we set them via reflection-free
        // public shim methods exposed on PuzzleDoor.
        door.SetTargets(closed, open);
    }

    private void CreateDoorSwitch(Transform parent, Vector3 pos, PuzzleDoor door,
        string switchName, string objectiveDesc)
    {
        var go = new GameObject(switchName);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;

        var col  = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size      = new Vector3(1.6f, 2.2f, 1.6f);

        var ds        = go.AddComponent<DoorSwitch>();
        ds.SetDoor(door);

        var obj             = go.AddComponent<PuzzleObjective>();
        obj.SetContent(switchName, objectiveDesc);
        ds.SetObjective(obj);
    }

    // Visible thin panel on a wall (kept for legacy use if needed)
    private void CreateGravityPanel(Transform parent, string panelName, Vector3 pos,
        Quaternion rot, Vector3 gravityDir)
    {
        var go = CreateBox(parent, panelName, pos, new Vector3(2.2f, 2.2f, 0.2f), panelColor);
        go.transform.localRotation = rot;
        var col       = go.GetComponent<BoxCollider>();
        col.isTrigger = true;
        var gp = go.AddComponent<GravityPanel>();
        gp.SetDirection(gravityDir);
    }

    // Walk-through gravity volume: visible colored column with a trigger the player walks into.
    private void CreateGravityVolume(Transform parent, string volName, Vector3 pos,
        Vector3 size, Vector3 gravityDir)
    {
        // Visible marker (semi-transparent blue box)
        var visual = CreateBox(parent, volName + "_Visual", pos, size * 0.9f, panelColor);

        // Invisible trigger volume (slightly larger so you don't have to be pixel-perfect)
        var trigger = new GameObject(volName + "_Trigger");
        trigger.transform.SetParent(parent, false);
        trigger.transform.localPosition = pos;

        var col       = trigger.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size      = size;

        var gp = trigger.AddComponent<GravityPanel>();
        gp.SetDirection(gravityDir);
    }

    // Proximity switch: player walks into zone and the door opens, no torch needed.
    private void CreateProximitySwitch(Transform parent, Vector3 pos, PuzzleDoor door)
    {
        var go = new GameObject("ProximitySwitch");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;

        var col       = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size      = new Vector3(3f, 2.2f, 3f);

        var ds = go.AddComponent<DoorSwitch>();
        ds.SetDoor(door);
        ds.SetRequireLitTorch(false);
    }

    private void CreateGravityCube(Transform parent, Vector3 pos)
    {
        var go = CreateBox(parent, "GravityCube", pos, Vector3.one, cubeColor);
        go.tag = "PuzzleBlock";

        var rb         = go.AddComponent<Rigidbody>();
        rb.useGravity  = false;
        rb.mass        = 2f;

        var gao                = go.AddComponent<GravityAffectedObject>();
    }

    private void CreatePlatform(Transform parent, Vector3 pos, Vector3 size, string platName)
        => CreateBox(parent, platName, pos, size, stoneColor * 0.9f);

    private void CreateExitTrigger(Transform parent, Vector3 pos)
    {
        var go = new GameObject("ExitTrigger");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;

        var col       = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size      = new Vector3(2f, 2f, 2f);

        go.AddComponent<PuzzleExitTrigger>();
    }

    private GameObject CreateTorchStation(Transform parent, Vector3 pos, string torchName, bool lit)
    {
        // Stand
        var stand = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stand.name = torchName + "_Stand";
        stand.transform.SetParent(parent, false);
        stand.transform.localPosition = pos;
        stand.transform.localScale    = new Vector3(0.3f, 0.25f, 0.3f);
        ApplyColor(stand, torchStandColor);

        // Torch body
        var torch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        torch.name = torchName;
        torch.transform.SetParent(parent, false);
        torch.transform.localPosition = pos + new Vector3(0f, 0.5f, 0f);
        torch.transform.localScale    = new Vector3(0.2f, 0.6f, 0.2f);
        ApplyColor(torch, new Color(0.18f, 0.12f, 0.08f));

        var col       = torch.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius    = 0.35f;

        var pickup         = torch.AddComponent<TorchPickup>();
        var tl             = torch.AddComponent<TorchLight>();
        var fire           = torch.AddComponent<FireSimulation>();

        return torch;
    }

    // ── Primitive Helpers ─────────────────────────────────────────────────────

    private Transform NewRoom(string roomName, Transform parent, Vector3 offset)
    {
        var go = new GameObject(roomName);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = offset;
        return go.transform;
    }

    private GameObject CreateBox(Transform parent, string boxName, Vector3 localPos,
        Vector3 localScale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = boxName;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localScale    = localScale;
        ApplyColor(go, color);
        return go;
    }

    private static void ApplyColor(GameObject go, Color color)
    {
        var r = go.GetComponent<Renderer>();
        if (r == null) return;
        var mat = new Material(Shader.Find("Standard")) { color = color };
        if (mat.HasProperty("_Metallic"))    mat.SetFloat("_Metallic", 0f);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.25f);
        r.material = mat;
    }

    // ── HUD Builder ───────────────────────────────────────────────────────────

    private static Font _hudFont;
    private static Font HudFont
    {
        get
        {
            if (_hudFont != null) return _hudFont;
            _hudFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_hudFont == null) _hudFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (_hudFont == null) _hudFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
            return _hudFont;
        }
    }

    private void BuildHUD()
    {
        var canvas = new GameObject("HUD_Canvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // ── Objective panel (top-left) ────────────────────────────────────────
        var hud = MakeCornerPanel(canvas.transform, "HUD_Panel",
            new Vector2(0f, 1f), new Vector2(12f, -12f), new Vector2(280f, 148f),
            new Color(0f, 0f, 0f, 0.6f));

        var objTitle    = MakeLeftText(hud.transform, "—",                         8, 16, Color.white);
        MakeSeparator(hud.transform, 28f);
        var objDesc     = MakeLeftText(hud.transform, "",                         32, 13, new Color(0.85f, 0.85f, 0.85f));
        var progress    = MakeLeftText(hud.transform, $"Puzzles: 0/{puzzlesToWin}", 80, 13, new Color(0.55f, 1f, 0.55f));
        var torchStatus = MakeLeftText(hud.transform, "Torch: none",             104, 13, new Color(1f, 0.85f, 0.45f));
        var hint        = MakeLeftText(hud.transform, "",                        128, 12, new Color(0.7f, 0.8f, 1f));

        // ── Controls reminder (bottom-left) ───────────────────────────────────
        var ctrl = MakeCornerPanel(canvas.transform, "Controls_Panel",
            new Vector2(0f, 0f), new Vector2(12f, 12f), new Vector2(230f, 140f),
            new Color(0f, 0f, 0f, 0.55f));
        MakeLeftText(ctrl.transform, "CONTROLS",            8,  11, new Color(1f, 0.75f, 0.2f));
        MakeSeparator(ctrl.transform, 22f);
        MakeLeftText(ctrl.transform, "WASD - Move",        26,  13, Color.white);
        MakeLeftText(ctrl.transform, "Mouse - Look",       44,  13, Color.white);
        MakeLeftText(ctrl.transform, "Space - Jump",       62,  13, Color.white);
        MakeLeftText(ctrl.transform, "E - Pickup  Q - Drop", 80, 13, Color.white);
        MakeLeftText(ctrl.transform, "F - Torch  LMB - Throw", 98, 13, Color.white);
        MakeLeftText(ctrl.transform, "Esc - Pause",       116,  13, Color.white);

        // ── Crosshair ─────────────────────────────────────────────────────────
        BuildCrosshair(canvas.transform);

        var ui = hud.AddComponent<UIManager>();
        ui.Inject(hud, objTitle, objDesc, progress, torchStatus, hint,
            BuildWinPanel(canvas.transform),
            BuildPausePanel(canvas.transform));
    }

    private static GameObject MakeCornerPanel(Transform parent, string name,
        Vector2 anchor, Vector2 offset, Vector2 size, Color bg)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        float signY = (anchor.y < 0.5f) ? 1f : -1f;
        rt.anchoredPosition = new Vector2(offset.x, offset.y * signY);
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = bg;
        return go;
    }

    private static Text MakeLeftText(Transform parent, string content, float topOffset, int fontSize, Color color)
    {
        var go = new GameObject("T");
        go.transform.SetParent(parent, false);
        // Simple top-left anchor — no stretch, no offsetMin/Max conflicts.
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(8f, -topOffset);
        rt.sizeDelta        = new Vector2(260f, fontSize + 8f);

        var t = go.AddComponent<Text>();
        t.text               = content;
        t.fontSize           = fontSize;
        t.alignment          = TextAnchor.UpperLeft;
        t.color              = color;
        t.font               = HudFont;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        t.raycastTarget      = false;
        go.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 1f);
        return t;
    }

    private static void MakeSeparator(Transform parent, float topOffset)
    {
        var go = new GameObject("Sep");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(8f, -topOffset);
        rt.sizeDelta        = new Vector2(260f, 1f);
        go.AddComponent<Image>().color = new Color(1f, 0.75f, 0.2f, 0.7f);
    }

    private static void BuildCrosshair(Transform canvasParent)
    {
        var go = new GameObject("Crosshair");
        go.transform.SetParent(canvasParent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(16f, 16f);

        MakeCrosshairBar(go.transform, new Vector2(16f, 2f));
        MakeCrosshairBar(go.transform, new Vector2(2f,  16f));
    }

    private static void MakeCrosshairBar(Transform parent, Vector2 size)
    {
        var bar = new GameObject("Bar");
        bar.transform.SetParent(parent, false);
        var rt = bar.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
        bar.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.9f);
        bar.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 0.7f);
    }

    private static GameObject BuildWinPanel(Transform canvasParent)
    {
        var win = new GameObject("WinPanel");
        win.transform.SetParent(canvasParent, false);
        var rt = win.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        win.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);
        MakeCentredText(win.transform, "YOU ESCAPED!",      48, new Color(1f, 0.85f, 0.1f),          60f);
        MakeCentredText(win.transform, "Press Esc to quit", 18, new Color(0.75f, 0.75f, 0.75f),      20f);
        win.SetActive(false);
        return win;
    }

    private static GameObject BuildPausePanel(Transform canvasParent)
    {
        var pause = new GameObject("PausePanel");
        pause.transform.SetParent(canvasParent, false);
        var rt = pause.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        pause.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.72f);
        MakeCentredText(pause.transform, "PAUSED",                    40, Color.white,                      160f);
        MakeCentredText(pause.transform, "WASD - Move",               18, new Color(0.85f, 0.85f, 0.85f),   90f);
        MakeCentredText(pause.transform, "Mouse - Look",              18, new Color(0.85f, 0.85f, 0.85f),   62f);
        MakeCentredText(pause.transform, "Space - Jump",              18, new Color(0.85f, 0.85f, 0.85f),   34f);
        MakeCentredText(pause.transform, "E - Pickup   Q - Drop   F - Toggle", 18, new Color(0.85f, 0.85f, 0.85f), 6f);
        MakeCentredText(pause.transform, "LMB - Throw",               18, new Color(0.85f, 0.85f, 0.85f),  -22f);
        MakeCentredText(pause.transform, "Press Esc to resume",       20, new Color(1f, 0.85f, 0.2f),      -80f);
        pause.SetActive(false);
        return pause;
    }

    private static Text MakeCentredText(Transform parent, string content, int fontSize, Color color, float yFromCentre)
    {
        var go = new GameObject("T");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, yFromCentre);
        rt.sizeDelta = new Vector2(800f, fontSize + 10f);

        var t = go.AddComponent<Text>();
        t.text               = content;
        t.fontSize           = fontSize;
        t.alignment          = TextAnchor.MiddleCenter;
        t.color              = color;
        t.font               = HudFont;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow   = VerticalWrapMode.Overflow;
        t.raycastTarget      = false;
        go.AddComponent<Shadow>().effectColor = new Color(0f, 0f, 0f, 1f);
        return t;
    }

    private static GameObject MakePanel(Transform parent, string panelName,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Color bgColor)
    {
        var go = new GameObject(panelName);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot     = anchorMin;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        go.AddComponent<Image>().color = bgColor;
        return go;
    }
}