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

    private bool built;

    private void Start()
    {
        if (built)
            return;

        BuildDungeon();
        built = true;
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
}
