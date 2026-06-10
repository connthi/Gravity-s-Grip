using TMPro;
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
            tracker.SetRequiredToWin(puzzlesToWin);
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
        CreateTreasureRoom(root.transform);
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
        const float W = 20f, D = 22f;   // D unified with puzzle room so N/S walls align
        var room = NewRoom("IntroRoom", parent, Vector3.zero);
        BuildRoomShell(room, W, D, omitEast: true);

        CreateTorchStation(room, new Vector3(-8f, 0.45f, 0f), "StartTorch", lit: true);

        CreatePlatform(room, new Vector3(-5f, 1.5f, -4f), new Vector3(3f, 0.3f, 3f), "Platform_A");
        CreatePlatform(room, new Vector3(2f,  2.2f,  4f), new Vector3(3f, 0.3f, 3f), "Platform_B");
        CreatePlatform(room, new Vector3(5f,  1.0f, -6f), new Vector3(3f, 0.3f, 3f), "Platform_C");

        // Gravity volumes — colour-coded: blue=right, yellow=up, purple=forward, green=down(reset)
        CreateGravityVolume(room, "GravVol_Right",   new Vector3(-4f, 0f,  3f), Vector3.right);
        CreateGravityVolume(room, "GravVol_Up",      new Vector3( 3f, 0f,  5f), Vector3.up);
        CreateGravityVolume(room, "GravVol_Forward", new Vector3( 0f, 0f, -5f), Vector3.forward);
        CreateGravityVolume(room, "GravVol_Down",    new Vector3( 4f, 0f, -2f), Vector3.down);

        // Floor-level green reset tiles along every wall base.
        // When stuck on a wall, walk toward the original floor (y≈0) to find these.
        CreateGravityVolume(room, "Reset_NearW", new Vector3(-8.5f, 0f,  0f), Vector3.down);
        CreateGravityVolume(room, "Reset_NearN", new Vector3(  0f,  0f,  9f), Vector3.down);
        CreateGravityVolume(room, "Reset_NearS", new Vector3(  0f,  0f, -9f), Vector3.down);
        // Invisible ceiling trigger — catches upward-gravity fall immediately.
        MakeCeilingSafetyTrigger(room, W, D);

        // East wall with doorway; door slides open on approach
        var door = CreateDoor(room, new Vector3(W * 0.5f - 0.3f, 1.2f, 0f), Quaternion.identity, "IntroDoor");
        BuildWallWithDoorway(room, "WallE", W * 0.5f, D, isXAxis: true);
        CreateProximitySwitch(room, new Vector3(W * 0.5f - 2.5f, 1.1f, 0f), door);
    }

    // Generic: builds a wall (east or north) with a centred doorway opening split into 3 boxes.
    // isXAxis=true → wall at x=wallPos facing Z; isXAxis=false → wall at z=wallPos facing X.
    private void BuildWallWithDoorway(Transform parent, string prefix, float wallPos, float span, bool isXAxis)
    {
        const float doorwayW = 2.8f;
        const float doorwayH = 2.6f;

        float sideDepth = (span - doorwayW) * 0.5f;
        float sideOff   = doorwayW * 0.5f + sideDepth * 0.5f;
        float headerH   = wallHeight - doorwayH;

        Vector3 LPos, RPos, HPos, LSize, RSize, HSize;
        if (isXAxis)
        {
            LPos  = new Vector3(wallPos, wallHeight * 0.5f, -sideOff);
            RPos  = new Vector3(wallPos, wallHeight * 0.5f,  sideOff);
            HPos  = new Vector3(wallPos, doorwayH + headerH * 0.5f, 0f);
            LSize = new Vector3(wallThickness, wallHeight, sideDepth);
            RSize = LSize;
            HSize = new Vector3(wallThickness, headerH, doorwayW + wallThickness);
        }
        else
        {
            LPos  = new Vector3(-sideOff, wallHeight * 0.5f, wallPos);
            RPos  = new Vector3( sideOff, wallHeight * 0.5f, wallPos);
            HPos  = new Vector3(0f, doorwayH + headerH * 0.5f, wallPos);
            LSize = new Vector3(sideDepth, wallHeight, wallThickness);
            RSize = LSize;
            HSize = new Vector3(doorwayW + wallThickness, headerH, wallThickness);
        }

        CreateBox(parent, prefix + "_Left",   LPos, LSize, stoneColor);
        CreateBox(parent, prefix + "_Right",  RPos, RSize, stoneColor);
        CreateBox(parent, prefix + "_Header", HPos, HSize, stoneColor);
    }

    // Invisible trigger that spans the ceiling — resets gravity to down when the player
    // hits the ceiling (catches the "gravity up" case where no floor tile is reachable).
    private void MakeCeilingSafetyTrigger(Transform parent, float w, float d)
    {
        var go = new GameObject("Safety_Ceil");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0f, wallHeight - 0.4f, 0f);
        var col = go.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(w - 1f, 0.8f, d - 1f);
        var gp = go.AddComponent<GravityPanel>();
        gp.SetDirection(Vector3.down);
    }

    // ── Room 2: Puzzle Room ───────────────────────────────────────────────────

    private void CreateGravityCubeRoom(Transform parent)
    {
        const float W = 24f, D = 22f;
        var room = NewRoom("PuzzleRoom", parent, new Vector3(22f, 0f, 0f));
        // omitWest: Room 1 already built the shared wall. omitEast: we build the exit doorway below.
        BuildRoomShell(room, W, D, omitWest: true, omitEast: true);
        BuildWallWithDoorway(room, "WallE", W * 0.5f, D, isXAxis: true);

        CreateGravityCube(room, new Vector3(2f, 0.5f, -4f));

        // Six gravity directions — colour-coded so the player can learn them visually
        CreateGravityVolume(room, "GravVol_Up",      new Vector3(-7f, 0f, -5f), Vector3.up);
        CreateGravityVolume(room, "GravVol_Down",    new Vector3( 4f, 0f,  5f), Vector3.down);
        CreateGravityVolume(room, "GravVol_Right",   new Vector3(-9f, 0f,  4f), Vector3.right);
        CreateGravityVolume(room, "GravVol_Left",    new Vector3( 9f, 0f, -3f), Vector3.left);
        CreateGravityVolume(room, "GravVol_Forward", new Vector3(-4f, 0f,  0f), Vector3.forward);
        CreateGravityVolume(room, "GravVol_Back",    new Vector3( 5f, 0f, -6f), Vector3.back);

        // Floor-level green reset tiles along every wall base + ceiling safety trigger.
        CreateGravityVolume(room, "Reset_NearN", new Vector3(  0f,  0f,  9f),  Vector3.down);
        CreateGravityVolume(room, "Reset_NearS", new Vector3(  0f,  0f, -9f),  Vector3.down);
        CreateGravityVolume(room, "Reset_NearE", new Vector3(10.5f, 0f,  0f),  Vector3.down);
        MakeCeilingSafetyTrigger(room, W, D);

        CreatePlatform(room, new Vector3(4f,  wallHeight - 0.4f,  4f), new Vector3(4f, 0.3f, 4f), "CeilingPlatform");
        CreatePlatform(room, new Vector3(-6f, 1.1f,  7f),              new Vector3(4f, 0.3f, 4f), "MidPlatform_A");
        CreatePlatform(room, new Vector3(5f,  2.5f, -7f),              new Vector3(3f, 0.3f, 3f), "MidPlatform_B");
        CreatePlatform(room, new Vector3(-9f, 1.0f, -3f),              new Vector3(3f, 0.3f, 5f), "LandingPlatform");

        CreateTorchStation(room, new Vector3(-9f, 0.45f, 0f), "PuzzleTorch", lit: false);

        // Exit door sits in the east doorway; torch switch opens it
        var exitDoor = CreateDoor(room, new Vector3(W * 0.5f - 0.3f, 1.2f, 0f), Quaternion.identity, "ExitDoor");
        CreateDoorSwitch(room, new Vector3(8f, 1.1f, -2f), exitDoor,
            "Light the Way", "Carry a lit torch to the switch near the east wall to open the exit.");

        // No exit trigger — the treasure room itself is the reward.
    }

    // ── Room 3: Treasure Room ─────────────────────────────────────────────────

    private void CreateTreasureRoom(Transform parent)
    {
        // Sits directly east of Room 2 (Room 2 east wall = world x 34).
        // offset x=39 + W/2=5 puts the west wall at world x=34, matching the exit doorway.
        const float W = 10f, D = 14f;
        var room = NewRoom("TreasureRoom", parent, new Vector3(39f, 0f, 0f));

        Color wallGold  = new Color(0.22f, 0.18f, 0.06f);  // dark warm stone
        Color goldBright= new Color(1.00f, 0.82f, 0.12f);
        Color goldDark  = new Color(0.72f, 0.50f, 0.04f);
        Color goldShiny = new Color(1.00f, 0.94f, 0.45f);
        Color gemRed    = new Color(0.85f, 0.10f, 0.10f);
        Color gemBlue   = new Color(0.10f, 0.30f, 0.90f);

        // Walls / shell — warm dark-gold stone
        CreateBox(room, "Floor",   new Vector3(0f, 0f, 0f),             new Vector3(W, 0.2f, D), wallGold);
        CreateBox(room, "Ceiling", new Vector3(0f, wallHeight, 0f),     new Vector3(W + wallThickness, 0.2f, D + wallThickness), wallGold);
        CreateBox(room, "WallN",   new Vector3(0f, wallHeight*0.5f,  D*0.5f), new Vector3(W + wallThickness, wallHeight, wallThickness), wallGold);
        CreateBox(room, "WallS",   new Vector3(0f, wallHeight*0.5f, -D*0.5f), new Vector3(W + wallThickness, wallHeight, wallThickness), wallGold);
        CreateBox(room, "WallE",   new Vector3(W*0.5f, wallHeight*0.5f, 0f),  new Vector3(wallThickness, wallHeight, D), wallGold);
        // West wall omitted — open to Room 2

        // Gold floor overlay
        CreateBox(room, "GoldFloor", new Vector3(0f, 0.11f, 0f), new Vector3(W - 0.5f, 0.04f, D - 0.5f), goldDark);

        // ── Central treasure mound ────────────────────────────────────────────
        CreateBox(room, "Mound_Base", new Vector3(1.5f, 0.35f, 0f), new Vector3(4f, 0.7f, 3.5f), goldDark);
        CreateBox(room, "Mound_Mid",  new Vector3(1.5f, 0.85f, 0f), new Vector3(2.8f, 0.5f, 2.5f), goldBright);
        CreateBox(room, "Mound_Top",  new Vector3(1.5f, 1.20f, 0f), new Vector3(1.5f, 0.4f, 1.5f), goldShiny);
        CreateBox(room, "Mound_Peak", new Vector3(1.5f, 1.55f, 0f), new Vector3(0.7f, 0.3f, 0.7f), goldShiny);

        // ── Chests ────────────────────────────────────────────────────────────
        // Left chest (open)
        CreateBox(room, "ChestL_Body", new Vector3(-2f, 0.30f, -3.5f), new Vector3(1.6f, 0.6f, 1.0f), goldDark);
        CreateBox(room, "ChestL_Lid",  new Vector3(-2f, 0.68f, -3.5f), new Vector3(1.6f, 0.28f, 1.0f), goldBright);
        CreateBox(room, "ChestL_Rim",  new Vector3(-2f, 0.50f, -3.5f), new Vector3(1.7f, 0.08f, 1.1f), goldShiny);
        CreateBox(room, "ChestL_Gems", new Vector3(-2f, 0.65f, -3.5f), new Vector3(0.4f, 0.15f, 0.4f), gemRed);

        // Right chest (open)
        CreateBox(room, "ChestR_Body", new Vector3(-2f, 0.30f,  3.5f), new Vector3(1.6f, 0.6f, 1.0f), goldDark);
        CreateBox(room, "ChestR_Lid",  new Vector3(-2f, 0.68f,  3.5f), new Vector3(1.6f, 0.28f, 1.0f), goldBright);
        CreateBox(room, "ChestR_Rim",  new Vector3(-2f, 0.50f,  3.5f), new Vector3(1.7f, 0.08f, 1.1f), goldShiny);
        CreateBox(room, "ChestR_Gems", new Vector3(-2f, 0.65f,  3.5f), new Vector3(0.4f, 0.15f, 0.4f), gemBlue);

        // ── Scattered coins / nuggets ─────────────────────────────────────────
        CreateBox(room, "Coins_A", new Vector3( 0.0f, 0.13f,  2.0f), new Vector3(1.2f, 0.05f, 0.8f), goldShiny);
        CreateBox(room, "Coins_B", new Vector3(-1.5f, 0.13f, -1.5f), new Vector3(0.9f, 0.05f, 0.6f), goldShiny);
        CreateBox(room, "Coins_C", new Vector3( 3.0f, 0.13f,  1.0f), new Vector3(0.7f, 0.05f, 0.5f), goldBright);
        CreateBox(room, "Coins_D", new Vector3(-2.5f, 0.13f,  1.0f), new Vector3(1.0f, 0.05f, 0.7f), goldShiny);
        CreateBox(room, "Nugget_A",new Vector3( 0.5f, 0.22f, -2.5f), new Vector3(0.5f, 0.20f, 0.4f), goldBright);
        CreateBox(room, "Nugget_B",new Vector3( 3.5f, 0.22f, -1.5f), new Vector3(0.4f, 0.20f, 0.4f), goldShiny);

        // ── Gold pillars ──────────────────────────────────────────────────────
        float ph = wallHeight;
        CreateBox(room, "Pillar_NW", new Vector3(-4f, ph*0.5f,  5.5f), new Vector3(0.6f, ph, 0.6f), goldDark);
        CreateBox(room, "Pillar_SW", new Vector3(-4f, ph*0.5f, -5.5f), new Vector3(0.6f, ph, 0.6f), goldDark);
        CreateBox(room, "Pillar_NE", new Vector3( 4f, ph*0.5f,  5.5f), new Vector3(0.6f, ph, 0.6f), goldDark);
        CreateBox(room, "Pillar_SE", new Vector3( 4f, ph*0.5f, -5.5f), new Vector3(0.6f, ph, 0.6f), goldDark);
        // Pillar caps
        CreateBox(room, "Cap_NW", new Vector3(-4f, ph + 0.15f,  5.5f), new Vector3(0.9f, 0.3f, 0.9f), goldShiny);
        CreateBox(room, "Cap_SW", new Vector3(-4f, ph + 0.15f, -5.5f), new Vector3(0.9f, 0.3f, 0.9f), goldShiny);
        CreateBox(room, "Cap_NE", new Vector3( 4f, ph + 0.15f,  5.5f), new Vector3(0.9f, 0.3f, 0.9f), goldShiny);
        CreateBox(room, "Cap_SE", new Vector3( 4f, ph + 0.15f, -5.5f), new Vector3(0.9f, 0.3f, 0.9f), goldShiny);

        // ── Warm golden point light ───────────────────────────────────────────
        var lg = new GameObject("TreasureLight");
        lg.transform.SetParent(room, false);
        lg.transform.localPosition = new Vector3(1.5f, 2.5f, 0f);
        var l = lg.AddComponent<Light>();
        l.type      = LightType.Point;
        l.color     = new Color(1f, 0.88f, 0.35f);
        l.intensity = 4f;
        l.range     = 18f;
        l.shadows   = LightShadows.Soft;
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
        var doorGO = CreateBox(parent, doorName, pos, new Vector3(0.4f, 2.4f, 2.8f), doorColor);
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

    // Colour each gravity direction so players can learn them visually.
    private static Color GravityTileColor(Vector3 dir)
    {
        if (dir == Vector3.down)    return new Color(0.10f, 0.75f, 0.25f); // green  — reset/floor
        if (dir == Vector3.up)      return new Color(0.90f, 0.80f, 0.10f); // yellow — ceiling
        if (dir == Vector3.right)   return new Color(0.15f, 0.40f, 0.90f); // blue   — right wall
        if (dir == Vector3.left)    return new Color(0.80f, 0.20f, 0.20f); // red    — left wall
        if (dir == Vector3.forward) return new Color(0.70f, 0.20f, 0.90f); // purple — front wall
        if (dir == Vector3.back)    return new Color(0.90f, 0.55f, 0.10f); // orange — back wall
        return new Color(0.12f, 0.44f, 0.74f);
    }

    // Floor-plate gravity marker: a flat coloured tile on the ground plus a tall invisible trigger above it.
    private void CreateGravityVolume(Transform parent, string volName, Vector3 floorPos, Vector3 gravityDir)
    {
        const float tileSize = 3f;

        // Flat visible tile sitting ON TOP of the floor (floor top surface is at y=0.1)
        CreateBox(parent, volName + "_Tile",
            floorPos + new Vector3(0f, 0.15f, 0f),
            new Vector3(tileSize, 0.1f, tileSize), GravityTileColor(gravityDir));

        // Tall invisible trigger spanning floor-to-ceiling above the tile
        float trigH = wallHeight - 0.2f;
        var trigger = new GameObject(volName + "_Trigger");
        trigger.transform.SetParent(parent, false);
        trigger.transform.localPosition = floorPos + new Vector3(0f, 0.2f + trigH * 0.5f, 0f);

        var col   = trigger.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size  = new Vector3(tileSize, trigH, tileSize);

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

    private void BuildHUD()
    {
        var canvas = new GameObject("HUD_Canvas").AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        BuildCrosshair(canvas.transform);

        // Pass null for hudPanel — there is no HUD panel to hide, and passing
        // canvas.gameObject would disable the whole canvas (including win/pause panels).
        var ui = canvas.gameObject.AddComponent<UIManager>();
        ui.Inject(null, null, null, null, null, null,
            BuildWinPanel(canvas.transform),
            BuildPausePanel(canvas.transform));
    }

    private static GameObject MakePanel(Transform parent, string name,
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

    // Cached TMP font — loaded once and reused. Assets survive PlayMode re-runs so no staleness.
    private static TMP_FontAsset _hudFont;

    private static TMP_FontAsset GetHudFont()
    {
        if (_hudFont != null) return _hudFont;
        // TMP Essential Resources install path (standard Unity project layout).
        _hudFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (_hudFont == null)
            _hudFont = TMP_Settings.defaultFontAsset;
        return _hudFont;
    }

    private static TMP_Text MakeTMP(Transform parent, string content, float topOffset,
        float fontSize, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject("T");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(8f, -topOffset);
        rt.sizeDelta        = new Vector2(280f, fontSize + 8f);

        var t = go.AddComponent<TextMeshProUGUI>();
        var font = GetHudFont();
        if (font != null) t.font = font;
        t.text           = content;
        t.fontSize       = fontSize;
        t.color          = color;
        t.alignment      = align;
        t.raycastTarget  = false;
        t.overflowMode   = TextOverflowModes.Overflow;
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
    }

    private static GameObject BuildWinPanel(Transform canvasParent)
    {
        var win = new GameObject("WinPanel");
        win.transform.SetParent(canvasParent, false);
        var rt = win.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        win.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.78f);
        MakeCentredTMP(win.transform, "YOU ESCAPED!",       48, new Color(1f, 0.85f, 0.1f),     60f);
        MakeCentredTMP(win.transform, "Press Esc to quit",  20, new Color(0.75f, 0.75f, 0.75f), 10f);
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
        MakeCentredTMP(pause.transform, "PAUSED",                              40, Color.white,                      160f);
        MakeCentredTMP(pause.transform, "WASD - Move",                         18, new Color(0.85f, 0.85f, 0.85f),   90f);
        MakeCentredTMP(pause.transform, "Mouse - Look",                        18, new Color(0.85f, 0.85f, 0.85f),   62f);
        MakeCentredTMP(pause.transform, "Space - Jump",                        18, new Color(0.85f, 0.85f, 0.85f),   34f);
        MakeCentredTMP(pause.transform, "E - Pickup   Q - Drop   F - Toggle",  18, new Color(0.85f, 0.85f, 0.85f),   6f);
        MakeCentredTMP(pause.transform, "LMB - Throw",                         18, new Color(0.85f, 0.85f, 0.85f),  -22f);
        MakeCentredTMP(pause.transform, "Press Esc to resume",                 22, new Color(1f, 0.85f, 0.2f),      -80f);
        pause.SetActive(false);
        return pause;
    }

    private static TMP_Text MakeCentredTMP(Transform parent, string content, float fontSize, Color color, float yFromCentre)
    {
        var go = new GameObject("T");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, yFromCentre);
        rt.sizeDelta = new Vector2(800f, fontSize + 12f);

        var t = go.AddComponent<TextMeshProUGUI>();
        var font = GetHudFont();
        if (font != null) t.font = font;
        t.text          = content;
        t.fontSize      = fontSize;
        t.color         = color;
        t.alignment     = TextAlignmentOptions.Center;
        t.raycastTarget = false;
        t.overflowMode  = TextOverflowModes.Overflow;
        return t;
    }
}