using UnityEngine;

[System.Serializable]
public struct DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)] public string text;
    public AudioClip voiceLine;
}

[CreateAssetMenu(fileName = "CutscenePanel", menuName = "Cutscene/Panel Data")]
public class CutscenePanelData : ScriptableObject
{
    [Header("Visuals")]
    public Sprite panelImage;

    [Header("Ken Burns Effect")]
    public Vector2 panStart = Vector2.zero;
    public Vector2 panEnd = Vector2.zero;
    [Range(1f, 1.5f)] public float zoomStart = 1f;
    [Range(1f, 1.5f)] public float zoomEnd = 1.05f;
    public float panDuration = 6f;

    [Header("Dialogue")]
    public Color textColor = Color.white;
    public DialogueLine[] dialogueLines;

    [Header("Audio")]
    public AudioClip ambientSound;
}
