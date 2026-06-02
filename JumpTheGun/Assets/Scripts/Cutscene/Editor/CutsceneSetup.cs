using UnityEditor;
using UnityEngine;

public static class CutsceneSetup
{
    private const string OutputFolder = "Assets/Cutscene/Panels";
    private const string SpritesFolder = "Assets/Art/panels";

    [MenuItem("Tools/Cutscene/Create Panel Assets")]
    public static void CreatePanelAssets()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Cutscene"))
            AssetDatabase.CreateFolder("Assets", "Cutscene");
        if (!AssetDatabase.IsValidFolder(OutputFolder))
            AssetDatabase.CreateFolder("Assets/Cutscene", "Panels");

        Make("Panel_01_NormalNight",
            speaker: "",
            lines: new[] { "The city never truly sleeps." },
            panStart: new Vector2(-30f, 0f), panEnd: new Vector2(30f, 0f),
            zoomStart: 1.0f, zoomEnd: 1.06f, duration: 6f);

        Make("Panel_02_TheCall",
            speaker: "Dispatch",
            lines: new[] {
                "Shots fired on the east side.",
                "We have a report of a young girl— taken by force."
            },
            panStart: new Vector2(0f, -15f), panEnd: new Vector2(0f, 15f),
            zoomStart: 1.05f, zoomEnd: 1.1f, duration: 9f);

        Make("Panel_03_Recognition",
            speaker: "Jake",
            lines: new[] {
                "…That's my daughter's street.",
                "That's my daughter."
            },
            panStart: Vector2.zero, panEnd: Vector2.zero,
            zoomStart: 1.0f, zoomEnd: 1.2f, duration: 7f);

        Make("Panel_04_NoPeace",
            speaker: "Jake",
            lines: new[] {
                "I spent years trying to leave all this behind.",
                "Looks like it followed me home."
            },
            panStart: new Vector2(-20f, 0f), panEnd: new Vector2(20f, 0f),
            zoomStart: 1.05f, zoomEnd: 1.0f, duration: 8f);

        Make("Panel_05_ArmedAndReady",
            speaker: "",
            lines: new string[0],
            panStart: Vector2.zero, panEnd: Vector2.zero,
            zoomStart: 1.0f, zoomEnd: 1.25f, duration: 4f);

        Make("Panel_06_TheHunt",
            speaker: "Jake",
            lines: new[] { "They picked the wrong family." },
            panStart: new Vector2(15f, 0f), panEnd: new Vector2(-15f, 0f),
            zoomStart: 1.1f, zoomEnd: 1.0f, duration: 5f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done", $"Panel assets created in {OutputFolder}.\nRun 'Tools > Cutscene > Assign Sprites to Panels' next.", "OK");
    }

    [MenuItem("Tools/Cutscene/Assign Sprites to Panels")]
    public static void AssignSprites()
    {
        // Maps panel asset name -> sprite filename (without extension)
        var mapping = new[]
        {
            ("Panel_01_NormalNight",    "kitchen"),
            ("Panel_02_TheCall",        "breakingin"),
            ("Panel_03_Recognition",    "dad-scared"),
            ("Panel_04_NoPeace",        "daughtertaken"),
            ("Panel_05_ArmedAndReady",  "dadkick"),
            ("Panel_06_TheHunt",        "upangledaughterlaying"),
        };

        // Force-reimport all sprites so Unity's cache reflects the Sprite texture type
        foreach (var (_, spriteName) in mapping)
            AssetDatabase.ImportAsset($"{SpritesFolder}/{spriteName}.png", ImportAssetOptions.ForceUpdate);

        AssetDatabase.Refresh();

        int assigned = 0;
        var errors = new System.Text.StringBuilder();

        foreach (var (panelName, spriteName) in mapping)
        {
            string panelPath  = $"{OutputFolder}/{panelName}.asset";
            string spritePath = $"{SpritesFolder}/{spriteName}.png";

            var panel = AssetDatabase.LoadAssetAtPath<CutscenePanelData>(panelPath);
            if (panel == null)
            {
                errors.AppendLine($"Panel not found: {panelName}");
                continue;
            }

            // LoadAllAssetsAtPath finds the Sprite sub-asset even if LoadAssetAtPath<Sprite> returns null
            Sprite sprite = null;
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(spritePath))
            {
                if (obj is Sprite s) { sprite = s; break; }
            }

            if (sprite == null)
            {
                errors.AppendLine($"Sprite not found or not imported as Sprite type: {spriteName}.png");
                continue;
            }

            panel.panelImage = sprite;
            EditorUtility.SetDirty(panel);
            assigned++;
            Debug.Log($"[CutsceneSetup] Assigned {spriteName} → {panelName}");
        }

        AssetDatabase.SaveAssets();

        string msg = $"Assigned {assigned}/6 sprites.";
        if (errors.Length > 0) msg += $"\n\nWarnings:\n{errors}";
        Debug.Log($"[CutsceneSetup] {msg}");
        EditorUtility.DisplayDialog("Assign Sprites", msg, "OK");
    }

    private static void Make(string assetName, string speaker, string[] lines,
        Vector2 panStart, Vector2 panEnd, float zoomStart, float zoomEnd, float duration)
    {
        string path = $"{OutputFolder}/{assetName}.asset";

        if (AssetDatabase.LoadAssetAtPath<CutscenePanelData>(path) != null)
        {
            Debug.Log($"[CutsceneSetup] Skipping existing asset: {path}");
            return;
        }

        var panel = ScriptableObject.CreateInstance<CutscenePanelData>();
        panel.speakerName = speaker;
        panel.dialogueLines = lines;
        panel.panStart = panStart;
        panel.panEnd = panEnd;
        panel.zoomStart = zoomStart;
        panel.zoomEnd = zoomEnd;
        panel.panDuration = duration;

        AssetDatabase.CreateAsset(panel, path);
    }
}
