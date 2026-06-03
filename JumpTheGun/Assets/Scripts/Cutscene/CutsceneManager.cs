using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CutscenePanelData[] panels;
    [SerializeField] private string nextSceneName = "L1_jose";

    [Header("UI References")]
    [SerializeField] private Image panelImage;
    // nextPanelImage sits BEHIND panelImage in the Canvas hierarchy (listed first in Inspector).
    // During a dissolve, panelImage fades out, revealing nextPanelImage underneath.
    [SerializeField] private Image nextPanelImage;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private GameObject continuePrompt;
    [SerializeField] private CanvasGroup screenFade;

    [Header("Timing")]
    [SerializeField] private float typewriterSpeed = 0.04f;
    [SerializeField] private float panelFadeDuration = 0.6f;
    [SerializeField] private float introFadeDuration = 1.2f;

    [Header("Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource voiceSource;

    private int currentPanelIndex = 0;
    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool waitingForInput = false;
    private Coroutine typewriterCoroutine;
    private Coroutine kenBurnsCoroutine;

    private void Start()
    {
        continuePrompt.SetActive(false);
        dialogueBox.SetActive(false);
        nextPanelImage.color = new Color(1f, 1f, 1f, 0f);
        screenFade.alpha = 1f;
        StartCoroutine(BeginCutscene());
    }

    private void Update()
    {
        if (!waitingForInput) return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
                SkipTypewriter();
            else
                AdvanceDialogue();
        }
    }

    private IEnumerator BeginCutscene()
    {
        yield return FadeScreen(1f, 0f, introFadeDuration);
        yield return LoadPanel(0);
    }

    private IEnumerator LoadPanel(int index)
    {
        if (index >= panels.Length)
        {
            yield return EndCutscene();
            yield break;
        }

        CutscenePanelData data = panels[index];

        yield return CrossfadePanel(data.panelImage);

        if (kenBurnsCoroutine != null) StopCoroutine(kenBurnsCoroutine);
        kenBurnsCoroutine = StartCoroutine(KenBurns(panelImage, data));

        if (sfxSource != null)
        {
            if (data.ambientSound != null)
            {
                sfxSource.clip = data.ambientSound;
                sfxSource.loop = true;
                sfxSource.Play();
            }
            else
            {
                sfxSource.Stop();
            }
        }

        if (data.dialogueLines != null && data.dialogueLines.Length > 0)
        {
            currentLineIndex = 0;
            dialogueBox.SetActive(true);
            dialogueText.color = data.textColor;
            ShowSpeaker(data.dialogueLines[0].speakerName);
            PlayVoiceLine(data.dialogueLines[0].voiceLine);
            yield return RunTypewriter(data.dialogueLines[0].text);
        }
        else
        {
            dialogueBox.SetActive(false);
            ShowContinuePrompt(true);
            waitingForInput = true;
        }
    }

    private IEnumerator RunTypewriter(string line)
    {
        waitingForInput = true;
        ShowContinuePrompt(false);
        dialogueText.text = "";
        isTyping = true;

        typewriterCoroutine = StartCoroutine(TypewriterRoutine(line));
        yield return typewriterCoroutine;

        isTyping = false;
        ShowContinuePrompt(true);
    }

    private IEnumerator TypewriterRoutine(string line)
    {
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    private void SkipTypewriter()
    {
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        dialogueText.text = panels[currentPanelIndex].dialogueLines[currentLineIndex].text;
        isTyping = false;
        ShowContinuePrompt(true);
    }

    private void AdvanceDialogue()
    {
        CutscenePanelData data = panels[currentPanelIndex];
        currentLineIndex++;

        if (data.dialogueLines != null && currentLineIndex < data.dialogueLines.Length)
        {
            ShowSpeaker(data.dialogueLines[currentLineIndex].speakerName);
            PlayVoiceLine(data.dialogueLines[currentLineIndex].voiceLine);
            StartCoroutine(RunTypewriter(data.dialogueLines[currentLineIndex].text));
        }
        else
        {
            waitingForInput = false;
            ShowContinuePrompt(false);
            currentPanelIndex++;
            StartCoroutine(LoadPanel(currentPanelIndex));
        }
    }

    // Dissolve: panelImage (front) fades out, revealing nextPanelImage (behind) with the new sprite.
    // After the dissolve, the new sprite is moved back to panelImage so Ken Burns can animate it.
    private IEnumerator CrossfadePanel(Sprite newSprite)
    {
        if (panelImage.sprite == null)
        {
            panelImage.sprite = newSprite;
            panelImage.color = Color.white;
            ResetRect(panelImage.rectTransform);
            yield break;
        }

        nextPanelImage.sprite = newSprite;
        nextPanelImage.color = Color.white;
        ResetRect(nextPanelImage.rectTransform);

        float elapsed = 0f;
        while (elapsed < panelFadeDuration)
        {
            elapsed += Time.deltaTime;
            panelImage.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01(elapsed / panelFadeDuration));
            yield return null;
        }

        panelImage.sprite = newSprite;
        panelImage.color = Color.white;
        ResetRect(panelImage.rectTransform);
        nextPanelImage.color = new Color(1f, 1f, 1f, 0f);
    }

    private void ResetRect(RectTransform rt)
    {
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    private IEnumerator KenBurns(Image img, CutscenePanelData data)
    {
        float elapsed = 0f;
        RectTransform rt = img.rectTransform;

        while (elapsed < data.panDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / data.panDuration);

            rt.anchoredPosition = Vector2.Lerp(data.panStart, data.panEnd, t);
            float scale = Mathf.Lerp(data.zoomStart, data.zoomEnd, t);
            rt.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }
    }

    private IEnumerator EndCutscene()
    {
        if (kenBurnsCoroutine != null) StopCoroutine(kenBurnsCoroutine);
        nextPanelImage.color = new Color(1f, 1f, 1f, 0f);
        dialogueBox.SetActive(false);
        continuePrompt.SetActive(false);
        yield return FadeScreen(0f, 1f, introFadeDuration);
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator FadeScreen(float from, float to, float duration)
    {
        float elapsed = 0f;
        screenFade.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            screenFade.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        screenFade.alpha = to;
    }

    private void PlayVoiceLine(AudioClip clip)
    {
        if (voiceSource == null) return;
        voiceSource.Stop();
        if (clip != null) voiceSource.PlayOneShot(clip);
    }

    private void ShowSpeaker(string name)
    {
        bool hasSpeaker = !string.IsNullOrEmpty(name);
        speakerNameText.gameObject.SetActive(hasSpeaker);
        if (hasSpeaker) speakerNameText.text = name;
    }

    private void ShowContinuePrompt(bool show)
    {
        continuePrompt.SetActive(show);
    }
}
