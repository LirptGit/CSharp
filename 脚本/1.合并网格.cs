using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
public class MeshCombiner : MonoBehaviour
{
    public string savePath = "Assets/CombinedMeshes";
    public string meshName = "CombinedMesh";

    [ContextMenu("Combine and Save Mesh")]
    public void CombineAndSaveMesh()
    {
        // 获取所有MeshFilter组件（包括子物体）
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters) 
        {
            Debug.Log($"{meshFilter.gameObject.name}");
        }

        // 准备合并数据
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            // 跳过没有网格的MeshFilter
            if (meshFilters[i].sharedMesh == null) continue;

            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        // 创建新网格并合并
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        // 确保保存目录存在
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // 生成唯一文件名
        string fileName = meshName;
        string path = Path.Combine(savePath, fileName + ".asset");
        int counter = 1;
        while (File.Exists(path))
        {
            fileName = meshName + "_" + counter;
            path = Path.Combine(savePath, fileName + ".asset");
            counter++;
        }

        // 保存网格
#if UNITY_EDITOR
        AssetDatabase.CreateAsset(combinedMesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Mesh saved to: " + path);
#else
        Debug.LogWarning("Mesh saving is only available in Editor mode");
#endif
    }
}