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

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done", $"Panel assets created in {OutputFolder}.\nRun 'Tools > Cutscene > Assign Sprites to Panels' next.", "OK");
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

        var dialogueLines = new DialogueLine[lines.Length];
        for (int i = 0; i < lines.Length; i++)
            dialogueLines[i] = new DialogueLine { speakerName = speaker, text = lines[i] };

        var panel = ScriptableObject.CreateInstance<CutscenePanelData>();
        panel.dialogueLines = dialogueLines;
        panel.panStart = panStart;
        panel.panEnd = panEnd;
        panel.zoomStart = zoomStart;
        panel.zoomEnd = zoomEnd;
        panel.panDuration = duration;

        AssetDatabase.CreateAsset(panel, path);
    }
}
