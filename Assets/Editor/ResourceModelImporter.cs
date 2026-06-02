#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ResourceModelImporter
{
    static ResourceModelImporter()
    {
        EditorApplication.delayCall += CreateResourcePrefabs;
    }

    private static void CreateResourcePrefabs()
    {
        CreatePrefabForResource("Assets/Resources/wallA.fbx", "Assets/Resources/WallA.prefab");
        CreatePrefabForResource("Assets/Resources/wallB.fbx", "Assets/Resources/WallB.prefab");
    }

    private static void CreatePrefabForResource(string sourceAssetPath, string prefabPath)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(sourceAssetPath);
        if (asset == null)
            return;

        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (existingPrefab != null)
            return;

        string prefabDir = System.IO.Path.GetDirectoryName(prefabPath);
        if (!System.IO.Directory.Exists(prefabDir))
            System.IO.Directory.CreateDirectory(prefabDir);

        PrefabUtility.SaveAsPrefabAsset(asset, prefabPath);
        Debug.Log($"Created Resource prefab: {prefabPath}");

        // Make sure the generated prefab has colliders on its mesh parts so geometry is solid.
        GameObject prefabContents = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabContents != null)
        {
            AddMeshColliders(prefabContents);
            PrefabUtility.SaveAsPrefabAsset(prefabContents, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefabContents);
        }
    }

    private static void AddMeshColliders(GameObject root)
    {
        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
        foreach (MeshFilter filter in meshFilters)
        {
            if (filter.sharedMesh == null)
                continue;

            Collider existingCollider = filter.GetComponent<Collider>();
            if (existingCollider == null)
            {
                MeshCollider collider = filter.gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = filter.sharedMesh;
                collider.convex = false;
            }
        }
    }
}
#endif