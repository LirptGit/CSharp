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
        // ��ȡ����MeshFilter��������������壩
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters) 
        {
            Debug.Log($"{meshFilter.gameObject.name}");
        }

        // ׼���ϲ�����
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        for (int i = 0; i < meshFilters.Length; i++)
        {
            // ����û�������MeshFilter
            if (meshFilters[i].sharedMesh == null) continue;

            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
        }

        // ���������񲢺ϲ�
        Mesh combinedMesh = new Mesh();
        combinedMesh.CombineMeshes(combine);

        // ȷ������Ŀ¼����
        if (!Directory.Exists(savePath))
        {
            Directory.CreateDirectory(savePath);
        }

        // ����Ψһ�ļ���
        string fileName = meshName;
        string path = Path.Combine(savePath, fileName + ".asset");
        int counter = 1;
        while (File.Exists(path))
        {
            fileName = meshName + "_" + counter;
            path = Path.Combine(savePath, fileName + ".asset");
            counter++;
        }

        // ��������
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