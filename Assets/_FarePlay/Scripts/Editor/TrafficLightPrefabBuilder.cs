using UnityEditor;
using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// EDITOR ONLY. Menu: Fare Play > Build Traffic Light Prefab.
    /// Builds Assets/_FarePlay/Prefabs/TrafficLight.prefab out of primitives, already wired up:
    ///   TrafficLight (root, sits on the road centre at the stop line; +Z = the way traffic drives)
    ///     Post        pole + housing + 3 lamps on the right-hand side, outside the wall
    ///     StopLine    TrafficStopLine trigger across the road + a painted white line
    /// Run it again to rebuild the prefab (instances in scenes keep their position and offset).
    /// </summary>
    public static class TrafficLightPrefabBuilder
    {
        const string PrefabPath   = "Assets/_FarePlay/Prefabs/TrafficLight.prefab";
        const string HousingPath  = "Assets/_FarePlay/Prefabs/TrafficLightHousing.mat";
        const string PaintPath    = "Assets/_FarePlay/Prefabs/StopLinePaint.mat";

        [MenuItem("Fare Play/Build Traffic Light Prefab")]
        static void Build()
        {
            Material housingMat = GetOrCreateMaterial(HousingPath, new Color(0.12f, 0.12f, 0.12f));
            Material paintMat   = GetOrCreateMaterial(PaintPath, Color.white);

            var root = new GameObject("TrafficLight");
            var light = root.AddComponent<TrafficLight>();

            // Post: outside the right-hand wall (road is 14 m wide, walls at +-7.25).
            var post = new GameObject("Post");
            post.transform.SetParent(root.transform, false);
            post.transform.localPosition = new Vector3(8.5f, 0f, 0f);

            MakePart(PrimitiveType.Cylinder, "Pole",    post.transform, new Vector3(0f, 2.5f, 0f),   new Vector3(0.3f, 2.5f, 0.3f), housingMat);
            MakePart(PrimitiveType.Cube,     "Housing", post.transform, new Vector3(0f, 5.6f, 0f),   new Vector3(0.9f, 2.6f, 0.7f), housingMat);
            // Lamps face the oncoming bus (-Z side of the housing).
            Renderer red    = MakePart(PrimitiveType.Sphere, "Lamp_Red",    post.transform, new Vector3(0f, 6.4f, -0.35f), Vector3.one * 0.6f, null);
            Renderer yellow = MakePart(PrimitiveType.Sphere, "Lamp_Yellow", post.transform, new Vector3(0f, 5.6f, -0.35f), Vector3.one * 0.6f, null);
            Renderer green  = MakePart(PrimitiveType.Sphere, "Lamp_Green",  post.transform, new Vector3(0f, 4.8f, -0.35f), Vector3.one * 0.6f, null);

            var so = new SerializedObject(light);
            so.FindProperty("redLamp").objectReferenceValue = red;
            so.FindProperty("yellowLamp").objectReferenceValue = yellow;
            so.FindProperty("greenLamp").objectReferenceValue = green;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Stop line: the trigger, plus a painted line just above the road pieces.
            var stopLine = new GameObject("StopLine");
            stopLine.transform.SetParent(root.transform, false);
            var box = stopLine.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(14f, 4f, 1f);
            box.center = new Vector3(0f, 2f, 0f);
            stopLine.AddComponent<TrafficStopLine>();
            MakePart(PrimitiveType.Cube, "Paint", stopLine.transform, new Vector3(0f, 0.06f, 0f), new Vector3(14f, 0.02f, 0.5f), paintMat);

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Debug.Log($"[Fare Play] Built {PrefabPath}");
        }

        /// <summary>A primitive with no collider (decoration only), so the bus can't hit it.</summary>
        static Renderer MakePart(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            var renderer = part.GetComponent<Renderer>();
            if (material != null) renderer.sharedMaterial = material;
            return renderer;
        }

        static Material GetOrCreateMaterial(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            material = new Material(shader) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
