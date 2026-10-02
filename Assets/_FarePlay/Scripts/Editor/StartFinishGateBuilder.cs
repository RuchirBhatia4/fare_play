using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FarePlay
{
    /// <summary>
    /// EDITOR ONLY. Menu: Fare Play > Build Start + Finish Gates.
    /// Adds a visible gate to every StartLine and FinishLine in the open scene, built from primitives:
    /// a black-and-white chequered strip on the road, two posts outside the walls and an overhead
    /// banner reading START (green) or FINISH (red) on both sides. Decoration only (no colliders).
    /// Safe to run again: it replaces each line's old "Gate" child.
    /// </summary>
    public static class StartFinishGateBuilder
    {
        const string Folder = "Assets/_FarePlay/Prefabs/";
        const float RoadWidth = 14f;

        [MenuItem("Fare Play/Build Start + Finish Gates")]
        static void Build()
        {
            Material black  = GetOrCreateMaterial("GateCheckerBlack", new Color(0.08f, 0.08f, 0.08f));
            Material white  = GetOrCreateMaterial("GateCheckerWhite", Color.white);
            Material post   = GetOrCreateMaterial("GatePost", new Color(0.25f, 0.25f, 0.28f));
            Material green  = GetOrCreateMaterial("GateBannerStart", new Color(0.1f, 0.6f, 0.25f));
            Material red    = GetOrCreateMaterial("GateBannerFinish", new Color(0.75f, 0.12f, 0.12f));

            int built = 0;
            foreach (StartLine line in Object.FindObjectsByType<StartLine>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                BuildGate(line.transform, "START", green, black, white, post);
                built++;
            }
            foreach (FinishLine line in Object.FindObjectsByType<FinishLine>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                BuildGate(line.transform, "FINISH", red, black, white, post);
                built++;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"[Fare Play] Built {built} gate(s). Save the scene to keep them.");
        }

        static void BuildGate(Transform line, string label, Material banner, Material black, Material white, Material post)
        {
            Transform old = line.Find("Gate");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

            var gate = new GameObject("Gate");
            Undo.RegisterCreatedObjectUndo(gate, "Build gate");
            gate.transform.SetParent(line, false);
            // Undo the line's own scale (e.g. 1.2 wide) so everything below is in plain metres.
            Vector3 s = line.lossyScale;
            gate.transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);

            // Chequered strip across the road: 1 m squares, 2 rows, just above the road surface.
            int columns = Mathf.RoundToInt(RoadWidth);
            for (int i = 0; i < columns; i++)
                for (int j = 0; j < 2; j++)
                    Part(PrimitiveType.Cube, $"Checker_{i}_{j}", gate.transform,
                         new Vector3(-RoadWidth / 2f + 0.5f + i, 0.06f, -0.5f + j), new Vector3(1f, 0.02f, 1f),
                         (i + j) % 2 == 0 ? black : white);

            // Posts just outside the walls (walls are at +-7.25 m), and the banner across the top.
            Part(PrimitiveType.Cylinder, "Post_Left",  gate.transform, new Vector3(-7.9f, 3f, 0f), new Vector3(0.35f, 3f, 0.35f), post);
            Part(PrimitiveType.Cylinder, "Post_Right", gate.transform, new Vector3( 7.9f, 3f, 0f), new Vector3(0.35f, 3f, 0.35f), post);
            Part(PrimitiveType.Cube, "Banner", gate.transform, new Vector3(0f, 6.3f, 0f), new Vector3(16.2f, 1.6f, 0.3f), banner);

            // The word on both faces, so it reads correctly from either direction.
            Label(gate.transform, label, -0.17f, 0f);
            Label(gate.transform, label,  0.17f, 180f);
        }

        static void Label(Transform parent, string text, float z, float yRotation)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, 6.3f, z);
            go.transform.localRotation = Quaternion.Euler(0f, yRotation, 0f);

            var tmp = go.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 4f;
            tmp.fontSizeMax = 14f;
            tmp.rectTransform.sizeDelta = new Vector2(15f, 1.4f);
        }

        /// <summary>A primitive with no collider (decoration only), so the bus can't hit it.</summary>
        static void Part(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
        }

        static Material GetOrCreateMaterial(string name, Color color)
        {
            string path = Folder + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }
    }
}
