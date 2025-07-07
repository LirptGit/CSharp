using UnityEngine;
using UnityEditor;
using System.IO;

public class SubmeshCombiner : MonoBehaviour
{
    [Tooltip("����ϲ������·���������Assets�ļ��У�")]
    public string savePath = "Assets/CombinedMeshes";

    [Tooltip("�ϲ�������Ļ�������")]
    public string meshName = "CombinedSubmeshes";

    [Tooltip("Ҫ�ϲ��������Ŀ��MeshFilter���")]
    public MeshFilter targetMeshFilter;

    // ��Unity�༭���������Ĳ˵������ѡ��
    [ContextMenu("Combine Submeshes")]
    public void CombineSubmeshes()
    {
        // 1. ���������Ч��
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

        // ��ȡԴ����
        Mesh sourceMesh = targetMeshFilter.sharedMesh;

        Debug.Log($"Starting submesh combination. Source mesh has {sourceMesh.subMeshCount} submeshes.");

        // 2. ׼���ϲ�����
        // ����CombineInstance���飬ÿ���������Ӧһ��ʵ��
        CombineInstance[] combine = new CombineInstance[sourceMesh.subMeshCount];

        for (int i = 0; i < sourceMesh.subMeshCount; i++)
        {
            combine[i] = new CombineInstance()
            {
                mesh = sourceMesh,           // ʹ����ͬ��Դ����
                subMeshIndex = i,             // ָ������������
                transform = targetMeshFilter.transform.localToWorldMatrix // Ӧ�ñ任����
            };

            Debug.Log($"Added submesh {i} to combine array");
        }

        // 3. ����������ִ�кϲ�
        Mesh combinedMesh = new Mesh();

        // CombineMeshes����˵����
        // combine - ����Ҫ�ϲ����������ݵ�����
        // mergeSubMeshes - ��Ϊtrue�Ὣ����������ϲ�Ϊһ����һ������
        // useMatrices - ��Ϊtrue��Ӧ�ñ任����
        combinedMesh.CombineMeshes(combine, true, false);

        Debug.Log($"Successfully combined {sourceMesh.subMeshCount} submeshes into one");

        // 4. �Ż��ϲ��������
        // ���¼���߽磨������ײ������׶��ü���
        combinedMesh.RecalculateBounds();
        Debug.Log("Recalculated mesh bounds");

        // ���¼��㷨�ߣ�ȷ��������ȷ��
        combinedMesh.RecalculateNormals();
        Debug.Log("Recalculated mesh normals");

        // ���¼������ߣ����ڷ�����ͼ��
        combinedMesh.RecalculateTangents();
        Debug.Log("Recalculated mesh tangents");

        // 5. ��������
        SaveMesh(combinedMesh);
    }

    /// <summary>
    /// ��������ָ��·��
    /// </summary>
    /// <param name="mesh">Ҫ���������</param>
    void SaveMesh(Mesh mesh)
    {
        // ȷ������Ŀ¼����
        if (!Directory.Exists(savePath))
        {
            Debug.Log($"Creating directory: {savePath}");
            Directory.CreateDirectory(savePath);
        }

        // ����Ψһ�ļ���
        string path = Path.Combine(savePath, meshName + ".asset");
        int counter = 1;

        // �����ļ�����ͻ
        while (File.Exists(path))
        {
            path = Path.Combine(savePath, meshName + "_" + counter + ".asset");
            counter++;
        }

        Debug.Log($"Saving mesh to: {path}");

        // ���ڱ༭��ģʽ�±���
#if UNITY_EDITOR
        // ����������Դ
        AssetDatabase.CreateAsset(mesh, path);
        // ���������ʲ�
        AssetDatabase.SaveAssets();
        // ˢ����Դ���ݿ�
        AssetDatabase.Refresh();

        Debug.Log("Mesh saved successfully!");
#else
        Debug.LogWarning("Mesh saving is only available in Editor mode");
#endif
    }
}