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
    }
}
#endif