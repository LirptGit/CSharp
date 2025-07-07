using UnityEngine;
using System.Collections;
using System.IO;
using UnityEditor;

// 要求游戏对象必须附加MeshFilter组件
[RequireComponent(typeof(MeshFilter))]
public class MeshCombiner : MonoBehaviour
{
    [Tooltip("保存合并网格的路径（相对于Assets文件夹）")]
    public string savePath = "Assets/CombinedMeshes";
    
    [Tooltip("合并后网格的基础名称")]
    public string meshName = "CombinedMesh";

    // 在Unity编辑器的上下文菜单中添加选项
    [ContextMenu("Combine and Save Mesh")]
    public void CombineAndSaveMesh()
    {
        // 1. 收集所有MeshFilter组件
        // GetComponentsInChildren会返回自身和所有子物体上的MeshFilter组件
        // 参数true表示包含非激活的子物体
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>(true);
        
        // 调试输出：打印所有找到的网格过滤器名称
        Debug.Log($"Found {meshFilters.Length} mesh filters:");
        foreach (MeshFilter meshFilter in meshFilters)
        {
            Debug.Log($"{meshFilter.gameObject.name}");
        }

        // 2. 准备合并数据
        // CombineInstance数组用于存储要合并的网格信息
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        // 遍历所有MeshFilter
        for (int i = 0; i < meshFilters.Length; i++)
        {
            // 跳过没有有效网格的MeshFilter
            if (meshFilters[i].sharedMesh == null)
            {
                Debug.LogWarning($"Skipping {meshFilters[i].gameObject.name} - no mesh found");
                continue;
            }

            // 设置合并实例的数据：
            // mesh - 要合并的网格
            // transform - 网格的变换矩阵（从局部空间到世界空间）
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            
            Debug.Log($"Adding mesh from {meshFilters[i].gameObject.name} to combine");
        }

        // 3. 创建新网格并执行合并
        Mesh combinedMesh = new Mesh();
        
        // CombineMeshes方法将所有网格合并到一个网格中
        // 参数说明：
        // combine - 包含要合并的网格数据的数组
        // mergeSubMeshes - 设为true会合并所有子网格为一个
        // useMatrices - 设为true会应用变换矩阵
        // true - 表示合并后网格应该可写入
        combinedMesh.CombineMeshes(combine, true, true, true);
        
        Debug.Log($"Successfully combined {combine.Length} meshes");

        // 4. 确保保存目录存在
        if (!Directory.Exists(savePath))
        {
            Debug.Log($"Creating directory: {savePath}");
            Directory.CreateDirectory(savePath);
        }

        // 5. 生成唯一文件名
        string fileName = meshName;
        string path = Path.Combine(savePath, fileName + ".asset");
        int counter = 1;
        
        // 如果文件名已存在，添加数字后缀直到找到可用的文件名
        while (File.Exists(path))
        {
            fileName = meshName + "_" + counter;
            path = Path.Combine(savePath, fileName + ".asset");
            counter++;
        }
        
        Debug.Log($"Saving combined mesh to: {path}");

        // 6. 保存网格（仅在编辑器模式下可用）
#if UNITY_EDITOR
        // 创建网格资源
        AssetDatabase.CreateAsset(combinedMesh, path);
        // 保存所有资产
        AssetDatabase.SaveAssets();
        // 刷新资源数据库
        AssetDatabase.Refresh();

        Debug.Log("Mesh saved successfully!");
#else
        Debug.LogWarning("Mesh saving is only available in Editor mode");
#endif
    }
}
