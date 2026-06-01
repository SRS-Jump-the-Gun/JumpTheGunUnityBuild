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
    [SerializeField] private Image nextPanelImage;      // for crossfade
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
        screenFade.alpha = 1f;
        StartCoroutine(BeginCutscene());
    }

    private void Update()
    {
        if (!waitingForInput) return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                SkipTypewriter();
            }
            else
            {
                AdvanceDialogue();
            }
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

        // crossfade image
        yield return CrossfadePanel(data.panelImage);

        // start Ken Burns on the visible image
        if (kenBurnsCoroutine != null) StopCoroutine(kenBurnsCoroutine);
        kenBurnsCoroutine = StartCoroutine(KenBurns(panelImage, data));

        // audio
        if (data.ambientSound != null && sfxSource != null)
        {
            sfxSource.clip = data.ambientSound;
            sfxSource.loop = true;
            sfxSource.Play();
        }

        // show dialogue if any
        if (data.dialogueLines != null && data.dialogueLines.Length > 0)
        {
            currentLineIndex = 0;
            dialogueBox.SetActive(true);
            speakerNameText.text = data.speakerName;
            yield return RunTypewriter(data.dialogueLines[0]);
        }
        else
        {
            // no dialogue — just wait for input to move on
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
        dialogueText.text = panels[currentPanelIndex].dialogueLines[currentLineIndex];
        isTyping = false;
        ShowContinuePrompt(true);
    }

    private void AdvanceDialogue()
    {
        CutscenePanelData data = panels[currentPanelIndex];

        currentLineIndex++;

        if (data.dialogueLines != null && currentLineIndex < data.dialogueLines.Length)
        {
            // more lines on this panel
            StartCoroutine(RunTypewriter(data.dialogueLines[currentLineIndex]));
        }
        else
        {
            // move to next panel
            waitingForInput = false;
            ShowContinuePrompt(false);
            currentPanelIndex++;
            StartCoroutine(LoadPanel(currentPanelIndex));
        }
    }

    private IEnumerator CrossfadePanel(Sprite newSprite)
    {
        if (panelImage.sprite == null)
        {
            panelImage.sprite = newSprite;
            panelImage.color = Color.white;
            yield break;
        }

        // set next panel underneath, fade out current on top via screen fade
        nextPanelImage.sprite = newSprite;
        nextPanelImage.color = Color.white;

        yield return FadeScreen(0f, 1f, panelFadeDuration * 0.5f);
        panelImage.sprite = newSprite;
        yield return FadeScreen(1f, 0f, panelFadeDuration * 0.5f);
    }

    private IEnumerator KenBurns(Image img, CutscenePanelData data)
    {
        float elapsed = 0f;
        RectTransform rt = img.rectTransform;

        Vector2 startPos = data.panStart;
        Vector2 endPos = data.panEnd;
        float startScale = data.zoomStart;
        float endScale = data.zoomEnd;

        while (elapsed < data.panDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / data.panDuration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, smooth);
            float scale = Mathf.Lerp(startScale, endScale, smooth);
            rt.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }
    }

    private IEnumerator EndCutscene()
    {
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

    private void ShowContinuePrompt(bool show)
    {
        continuePrompt.SetActive(show);
    }
}
