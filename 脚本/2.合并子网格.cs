using UnityEngine;
using UnityEditor;
using System.IO;

public class SubmeshCombiner : MonoBehaviour
{
    [Tooltip("保存合并网格的路径（相对于Assets文件夹）")]
    public string savePath = "Assets/CombinedMeshes";

    [Tooltip("合并后网格的基础名称")]
    public string meshName = "CombinedSubmeshes";

    [Tooltip("要合并子网格的目标MeshFilter组件")]
    public MeshFilter targetMeshFilter;

    // 在Unity编辑器的上下文菜单中添加选项
    [ContextMenu("Combine Submeshes")]
    public void CombineSubmeshes()
    {
        // 1. 检查输入有效性
        if (targetMeshFilter == null)
        {
            Debug.LogError("Target MeshFilter is not assigned!");
            return;
        }

        if (targetMeshFilter.sharedMesh == null)
        {
            Debug.LogError("The assigned MeshFilter has no mesh!");
            return;
        }

        // 获取源网格
        Mesh sourceMesh = targetMeshFilter.sharedMesh;

        Debug.Log($"Starting submesh combination. Source mesh has {sourceMesh.subMeshCount} submeshes.");

        // 2. 准备合并数据
        // 创建CombineInstance数组，每个子网格对应一个实例
        CombineInstance[] combine = new CombineInstance[sourceMesh.subMeshCount];

        for (int i = 0; i < sourceMesh.subMeshCount; i++)
        {
            combine[i] = new CombineInstance()
            {
                mesh = sourceMesh,           // 使用相同的源网格
                subMeshIndex = i,             // 指定子网格索引
                transform = targetMeshFilter.transform.localToWorldMatrix // 应用变换矩阵
            };

            Debug.Log($"Added submesh {i} to combine array");
        }

        // 3. 创建新网格并执行合并
        Mesh combinedMesh = new Mesh();

        // CombineMeshes参数说明：
        // combine - 包含要合并的网格数据的数组
        // mergeSubMeshes - 设为true会将所有子网格合并为一个单一子网格
        // useMatrices - 设为true会应用变换矩阵
        combinedMesh.CombineMeshes(combine, true, false);

        Debug.Log($"Successfully combined {sourceMesh.subMeshCount} submeshes into one");

        // 4. 优化合并后的网格
        // 重新计算边界（用于碰撞检测和视锥体裁剪）
        combinedMesh.RecalculateBounds();
        Debug.Log("Recalculated mesh bounds");

        // 重新计算法线（确保光照正确）
        combinedMesh.RecalculateNormals();
        Debug.Log("Recalculated mesh normals");

        // 重新计算切线（用于法线贴图）
        combinedMesh.RecalculateTangents();
        Debug.Log("Recalculated mesh tangents");

        // 5. 保存网格
        SaveMesh(combinedMesh);
    }

    /// <summary>
    /// 保存网格到指定路径
    /// </summary>
    /// <param name="mesh">要保存的网格</param>
    void SaveMesh(Mesh mesh)
    {
        // 确保保存目录存在
        if (!Directory.Exists(savePath))
        {
            Debug.Log($"Creating directory: {savePath}");
            Directory.CreateDirectory(savePath);
        }

        // 生成唯一文件名
        string path = Path.Combine(savePath, meshName + ".asset");
        int counter = 1;

        // 处理文件名冲突
        while (File.Exists(path))
        {
            path = Path.Combine(savePath, meshName + "_" + counter + ".asset");
            counter++;
        }

        Debug.Log($"Saving mesh to: {path}");

        // 仅在编辑器模式下保存
#if UNITY_EDITOR
        // 创建网格资源
        AssetDatabase.CreateAsset(mesh, path);
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