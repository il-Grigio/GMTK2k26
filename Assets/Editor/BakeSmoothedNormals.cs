// BakeSmoothedNormals.cs
//
// Genera normali "smussate" (media di tutte le normali dei vertici che
// condividono la stessa posizione nello spazio locale) e le salva nel
// canale UV3 (TEXCOORD2) della mesh. Il toon shader (ToonShader4Level)
// le usa - se il toggle "Usa Normali Smussate per Outline" e' attivo -
// per estrudere l'outline, evitando che si spacchi in corrispondenza
// degli edge "hard" tipici dei modelli lowpoli.
//
// USO:
// 1. Metti questo file in una cartella "Editor" del progetto
//    (es. Assets/Editor/BakeSmoothedNormals.cs).
// 2. Seleziona nella Hierarchy o nel Project uno o piu' oggetti/mesh.
// 3. Menu in alto: Tools > Toon Shader > Bake Smoothed Normals (UV3)
// 4. Sul materiale, attiva il toggle "Usa Normali Smussate per Outline (UV3)".
//
// Nota: lo script duplica e salva un nuovo asset .asset della mesh
// modificata accanto all'originale (non sovrascrive l'FBX importato),
// e lo assegna automaticamente ai MeshFilter/SkinnedMeshRenderer
// selezionati nella scena.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BakeSmoothedNormals
{
    [MenuItem("Tools/Toon Shader/Bake Smoothed Normals (UV3)")]
    private static void BakeSelected()
    {
        if (Selection.gameObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("Bake Smoothed Normals",
                "Seleziona prima uno o piu' GameObject con MeshFilter o SkinnedMeshRenderer.", "OK");
            return;
        }

        int count = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                Mesh baked = BakeMesh(mf.sharedMesh, go.name);
                mf.mesh = baked;
                count++;
                continue;
            }

            SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
            if (smr != null && smr.sharedMesh != null)
            {
                Mesh baked = BakeMesh(smr.sharedMesh, go.name);
                smr.sharedMesh = baked;
                count++;
            }
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Bake Smoothed Normals",
            $"Normali smussate generate e salvate in UV3 per {count} mesh.", "OK");
    }

    private static Mesh BakeMesh(Mesh source, string ownerName)
    {
        // Duplica la mesh cosi' non modifichiamo l'asset FBX originale
        Mesh mesh = Object.Instantiate(source);
        mesh.name = source.name + "_SmoothedOutline";

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        if (normals == null || normals.Length != vertices.Length)
        {
            mesh.RecalculateNormals();
            normals = mesh.normals;
        }

        // Raggruppa i vertici per posizione (con una tolleranza per gli
        // errori in virgola mobile) e media le normali di ogni gruppo.
        var groups = new Dictionary<Vector3Int, List<int>>();
        const float precision = 10000f; // ~0.0001 unita' di tolleranza

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 p = vertices[i];
            Vector3Int key = new Vector3Int(
                Mathf.RoundToInt(p.x * precision),
                Mathf.RoundToInt(p.y * precision),
                Mathf.RoundToInt(p.z * precision));

            if (!groups.TryGetValue(key, out List<int> list))
            {
                list = new List<int>();
                groups[key] = list;
            }
            list.Add(i);
        }

        Vector3[] smoothed = new Vector3[vertices.Length];
        foreach (var kvp in groups)
        {
            Vector3 avg = Vector3.zero;
            foreach (int idx in kvp.Value)
                avg += normals[idx];
            avg.Normalize();

            foreach (int idx in kvp.Value)
                smoothed[idx] = avg;
        }

        // Unity supporta UV a 3 componenti tramite SetUVs; usiamo il
        // canale 2 (UV3 / TEXCOORD2), come atteso dallo shader.
        List<Vector3> uv3 = new List<Vector3>(smoothed);
        mesh.SetUVs(2, uv3);

        // Salva la mesh come asset persistente accanto a un file di
        // supporto, cosi' il riferimento non si perde chiudendo Unity.
        string folder = "Assets/GeneratedSmoothedMeshes";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "GeneratedSmoothedMeshes");

        string safeName = ownerName.Replace(" ", "_");
        string path = $"{folder}/{safeName}_{mesh.name}.asset";
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        AssetDatabase.CreateAsset(mesh, path);

        return mesh;
    }
}
