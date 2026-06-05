using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Drives the Level 2 boss fight sequence.
///
/// Workflow:
///   1. Place a trigger collider on the same GameObject — when the player walks in, the fight starts.
///   2. The boss GameObject starts disabled. This script enables it on fight start.
///   3. Boss-intro text appears briefly, then fades.
///   4. BossHealthBar UI is shown.
///   5. When the boss dies, victory flow plays: open doors, show "victory" text, optionally load next scene.
///
/// Setup:
///   - Assign bossObject (the BossEnemy's root GameObject, set inactive in scene)
///   - Assign bossHealthBarCanvas (the Canvas with BossHealthBar, set inactive)
///   - Assign doorsToOpen[] — GameObjects to deactivate on boss death
///   - Assign bossIntroText (optional TMP_Text for "THE BOSS" splash)
///   - Assign victoryText (optional TMP_Text shown after boss dies)
///   - Assign nextSceneName if you want to auto-load after victory
/// </summary>
public class BossManager : MonoBehaviour
{
    [Header("Boss")]
    [SerializeField] private GameObject bossObject;

    [Header("UI")]
    [SerializeField] private GameObject bossHealthBarCanvas;
    [SerializeField] private TMP_Text   bossIntroText;
    [SerializeField] private TMP_Text   victoryText;

    [Header("Scene Flow")]
    [SerializeField] private GameObject[] doorsToOpen;
    [SerializeField] private GameObject[] rewardsToActivate;
    [Tooltip("Leave empty to stay in the level after defeating the boss.")]
    [SerializeField] private string nextSceneName = "";
    [SerializeField] private float  victoryHoldSeconds = 3f;

    [Header("Music")]
    [SerializeField] private AudioClip bossMusicClip;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField] private AudioSource musicSource;

    private bool fightStarted;

    private void Awake()
    {
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    // ─────────────────────────────────────────
    //  Fight Trigger
    // ─────────────────────────────────────────
    private void OnTriggerEnter(Collider other)
    {
        if (fightStarted || !other.CompareTag("Player")) return;
        fightStarted = true;

        // Disable the trigger so it doesn't fire again
        GetComponent<Collider>().enabled = false;

        StartCoroutine(StartFight());
    }

    // ─────────────────────────────────────────
    //  Fight Start
    // ─────────────────────────────────────────
    private IEnumerator StartFight()
    {
        // Show boss intro splash
        if (bossIntroText != null)
        {
            bossIntroText.gameObject.SetActive(true);
            bossIntroText.text = "THE BOSS";
            yield return new WaitForSeconds(2f);
            bossIntroText.gameObject.SetActive(false);
        }

        // Show health bar first so BossHealthBar.OnEnable() subscribes before
        // BossEnemy.Start() fires the initial OnHealthChanged event
        if (bossHealthBarCanvas != null)
            bossHealthBarCanvas.SetActive(true);

        // Activate boss
        if (bossObject != null)
        {
            bossObject.SetActive(true);

            BossEnemy boss = bossObject.GetComponent<BossEnemy>();
            if (boss != null)
                boss.OnBossDefeated += StartVictorySequence;
        }

        // Start boss music
        if (musicSource != null && bossMusicClip != null)
        {
            musicSource.clip = bossMusicClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    // ─────────────────────────────────────────
    //  Victory Sequence
    // ─────────────────────────────────────────
    private void StartVictorySequence()
    {
        StartCoroutine(VictoryFlow());
    }

    private IEnumerator VictoryFlow()
    {
        // Stop boss music
        if (musicSource != null) musicSource.Stop();

        if (victoryClip != null && musicSource != null)
        {
            musicSource.loop = false;
            musicSource.clip = victoryClip;
            musicSource.Play();
        }

        // Open doors / give rewards
        if (doorsToOpen != null)
            foreach (var door in doorsToOpen)
                if (door != null) door.SetActive(false);

        if (rewardsToActivate != null)
            foreach (var reward in rewardsToActivate)
                if (reward != null) reward.SetActive(true);

        // Show victory text
        if (victoryText != null)
        {
            victoryText.gameObject.SetActive(true);
            victoryText.text = "BOSS DEFEATED";
        }

        yield return new WaitForSeconds(victoryHoldSeconds);

        if (victoryText != null)
            victoryText.gameObject.SetActive(false);

        // Optionally load next scene
        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
