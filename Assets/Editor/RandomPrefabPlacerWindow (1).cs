using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Tool per l'Editor di Unity: piazza prefab casuali su un Terrain.
/// IMPORTANTE: questo file deve stare dentro una cartella chiamata "Editor"
/// (es. Assets/Editor/RandomPrefabPlacerWindow.cs), altrimenti la build fallisce.
///
/// Apri la finestra da: Tools > Random Prefab Placer
/// Scorciatoia rapida in Editor (senza aprire la finestra): premi "G"
/// mentre il focus non è su un campo di testo (vedi menu Tools > Random Prefab Placer > Genera).
/// </summary>
public class RandomPrefabPlacerWindow : EditorWindow
{
    // GameObject "terreno": può essere un Plane, una Mesh custom, ecc.
    // Deve avere un Collider (MeshCollider o BoxCollider) per il raycast.
    private static GameObject groundObject;
    private static List<GameObject> prefabs = new List<GameObject>();
    private static int numberOfObjects = 10;
    private static bool randomYRotation = true;
    private static bool alignToSurfaceNormal = false;
    private static bool randomScale = false;
    private static Vector2 scaleRange = new Vector2(0.8f, 1.2f);
    private static bool clearPreviousBeforeSpawn = false;
    private static Transform spawnParent;
    private static float raycastHeight = 500f; // da dove parte il raycast verso il basso

    private SerializedObject serializedWindow;
    private Vector2 scroll;

    [MenuItem("Tools/Random Prefab Placer/Apri finestra")]
    public static void ShowWindow()
    {
        var window = GetWindow<RandomPrefabPlacerWindow>("Prefab Placer");
        window.minSize = new Vector2(320, 400);
    }

    // Scorciatoia da tastiera nell'Editor: premi "G" (senza modificatori)
    // Funziona quando il focus è sulla Scene View o sull'Hierarchy, non su un campo di testo.
    [MenuItem("Tools/Random Prefab Placer/Genera _g")]
    private static void GenerateShortcut()
    {
        PlaceRandomPrefabs();
    }

    // Valida la scorciatoia: disabilitata se non c'è ancora nulla di configurato
    [MenuItem("Tools/Random Prefab Placer/Genera _g", true)]
    private static bool ValidateGenerateShortcut()
    {
        return groundObject != null && prefabs.Count > 0;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Random Prefab Placer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Configura Terrain e Prefab, poi premi 'Genera' qui sotto oppure usa " +
            "il tasto scorciatoia 'G' (Tools > Random Prefab Placer > Genera).",
            MessageType.Info);

        EditorGUILayout.Space();
        groundObject = (GameObject)EditorGUILayout.ObjectField("Ground (GameObject)", groundObject, typeof(GameObject), true);
        if (groundObject == null)
        {
            EditorGUILayout.HelpBox("Trascina qui il GameObject che rappresenta il terreno (es. il tuo Plane).", MessageType.Warning);
        }
        else if (groundObject.GetComponent<Collider>() == null)
        {
            EditorGUILayout.HelpBox(
                "Questo GameObject non ha un Collider (MeshCollider o BoxCollider). " +
                "Il raycast non funzionerà senza un Collider!",
                MessageType.Error);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Prefab da piazzare", EditorStyles.boldLabel);

        int newCount = Mathf.Max(0, EditorGUILayout.IntField("Numero prefab diversi", prefabs.Count));
        while (newCount > prefabs.Count) prefabs.Add(null);
        while (newCount < prefabs.Count) prefabs.RemoveAt(prefabs.Count - 1);

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(150));
        for (int i = 0; i < prefabs.Count; i++)
        {
            prefabs[i] = (GameObject)EditorGUILayout.ObjectField($"Prefab {i + 1}", prefabs[i], typeof(GameObject), false);
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Impostazioni spawn", EditorStyles.boldLabel);
        numberOfObjects = EditorGUILayout.IntSlider("Quantità da generare", numberOfObjects, 1, 500);
        randomYRotation = EditorGUILayout.Toggle("Rotazione Y casuale", randomYRotation);
        alignToSurfaceNormal = EditorGUILayout.Toggle("Allinea alla normale della superficie", alignToSurfaceNormal);
        randomScale = EditorGUILayout.Toggle("Scala casuale", randomScale);
        if (randomScale)
        {
            EditorGUI.indentLevel++;
            float min = scaleRange.x, max = scaleRange.y;
            EditorGUILayout.MinMaxSlider("Range scala", ref min, ref max, 0.1f, 3f);
            scaleRange = new Vector2(min, max);
            EditorGUILayout.LabelField($"Min: {scaleRange.x:F2}   Max: {scaleRange.y:F2}");
            EditorGUI.indentLevel--;
        }
        clearPreviousBeforeSpawn = EditorGUILayout.Toggle("Cancella i precedenti prima di generare", clearPreviousBeforeSpawn);
        spawnParent = (Transform)EditorGUILayout.ObjectField("Parent (opzionale)", spawnParent, typeof(Transform), true);

        EditorGUILayout.Space();
        GUI.enabled = groundObject != null && prefabs.Count > 0;
        if (GUILayout.Button("Genera prefab sul terreno", GUILayout.Height(35)))
        {
            PlaceRandomPrefabs();
        }
        GUI.enabled = true;

        if (GUILayout.Button("Rimuovi tutti i prefab generati con questo tool"))
        {
            ClearSpawned();
        }
    }

    private static readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private static void PlaceRandomPrefabs()
    {
        if (groundObject == null)
        {
            Debug.LogWarning("Random Prefab Placer: nessun Ground GameObject assegnato.");
            return;
        }

        Collider groundCollider = groundObject.GetComponent<Collider>();
        if (groundCollider == null)
        {
            Debug.LogWarning("Random Prefab Placer: il Ground non ha un Collider, impossibile fare il raycast.");
            return;
        }

        var validPrefabs = prefabs.FindAll(p => p != null);
        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning("Random Prefab Placer: nessun prefab valido nella lista.");
            return;
        }

        if (clearPreviousBeforeSpawn)
        {
            ClearSpawned();
        }

        // IMPORTANTE: uso i bounds LOCALI della mesh (rettangolo pulito, indipendente
        // da rotazione/scala) invece dei bounds world del Collider.
        // Se usassi i bounds world (AABB) su un oggetto ruotato, il box sarebbe più
        // grande della mesh reale: tanti punti casuali cadrebbero fuori dalla mesh,
        // il raycast fallirebbe e verrebbero scartati -> risultato: oggetti raggruppati
        // solo dove il box si sovrappone alla mesh vera, con zone scoperte altrove.
        Bounds localBounds;
        MeshFilter meshFilter = groundObject.GetComponent<MeshFilter>();
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            localBounds = meshFilter.sharedMesh.bounds;
        }
        else
        {
            // Fallback: nessuna mesh (es. terreno fatto da primitive/Collider puro).
            // Uso i bounds world come approssimazione (funziona bene se non è ruotato).
            Debug.LogWarning("Random Prefab Placer: nessun MeshFilter trovato su Ground, uso i bounds world come fallback (potrebbe non essere preciso se l'oggetto è ruotato).");
            Bounds wb = groundCollider.bounds;
            localBounds = new Bounds(groundObject.transform.InverseTransformPoint(wb.center), wb.size);
        }

        Transform groundTransform = groundObject.transform;

        Undo.SetCurrentGroupName("Genera Prefab Casuali");
        int undoGroup = Undo.GetCurrentGroup();

        int placed = 0;
        int attempts = 0;
        int maxAttempts = numberOfObjects * 20; // margine se qualche raycast fallisce

        // Punto più alto del ground, in world space, per far partire il raycast da sopra
        float worldTopY = groundCollider.bounds.max.y;

        while (placed < numberOfObjects && attempts < maxAttempts)
        {
            attempts++;

            // Punto casuale in spazio LOCALE della mesh (X e Z, ignoro Y locale)
            float localX = Random.Range(localBounds.min.x, localBounds.max.x);
            float localZ = Random.Range(localBounds.min.z, localBounds.max.z);
            Vector3 localPoint = new Vector3(localX, localBounds.center.y, localZ);

            // Trasformo in world space per sapere dove piazzare il raycast
            Vector3 worldPoint = groundTransform.TransformPoint(localPoint);

            Vector3 rayOrigin = new Vector3(worldPoint.x, worldTopY + raycastHeight, worldPoint.z);

            if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f))
                continue; // niente sotto in questo punto, riprovo

            // Mi assicuro che il raycast abbia colpito proprio il ground (evita di spawnare sopra ad altri oggetti)
            if (hit.collider != groundCollider)
                continue;

            Vector3 spawnPos = hit.point;

            GameObject prefabToSpawn = validPrefabs[Random.Range(0, validPrefabs.Count)];

            Quaternion rotation;
            if (alignToSurfaceNormal)
            {
                // Allinea l'oggetto alla normale della superficie, con rotazione Y casuale opzionale
                Quaternion normalRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                if (randomYRotation)
                    normalRotation *= Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                rotation = normalRotation;
            }
            else
            {
                rotation = randomYRotation
                    ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                    : Quaternion.identity;
            }

            // Instantiate che mantiene il collegamento al prefab (PrefabUtility)
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
            instance.transform.SetPositionAndRotation(spawnPos, rotation);
            if (spawnParent != null)
                instance.transform.SetParent(spawnParent, true);

            if (randomScale)
            {
                float s = Random.Range(scaleRange.x, scaleRange.y);
                instance.transform.localScale *= s;
            }

            Undo.RegisterCreatedObjectUndo(instance, "Genera Prefab Casuali");
            spawnedObjects.Add(instance);
            placed++;
        }

        Undo.CollapseUndoOperations(undoGroup);

        if (placed < numberOfObjects)
        {
            Debug.LogWarning($"Random Prefab Placer: generati solo {placed}/{numberOfObjects} prefab (troppi raycast falliti, prova ad aumentare 'Raycast Height' o controlla la mesh del Ground).");
        }
        else
        {
            Debug.Log($"Random Prefab Placer: generati {placed}/{numberOfObjects} prefab su '{groundObject.name}'.");
        }
    }

    private static void ClearSpawned()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
                Undo.DestroyObjectImmediate(spawnedObjects[i]);
        }
        spawnedObjects.Clear();
    }
}
