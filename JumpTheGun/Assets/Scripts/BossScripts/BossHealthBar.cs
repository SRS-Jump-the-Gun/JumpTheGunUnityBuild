using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the boss health bar UI.
/// Attach to a Canvas child that contains:
///   - A Slider (or Image with Image.Type = Filled) for the bar
///   - A TMP_Text for the boss name
///   - A TMP_Text for the current phase label
///
/// In the Inspector, wire up the BossEnemy reference and the UI elements.
/// The Canvas can start hidden and is activated by BossManager when the fight begins.
/// </summary>
public class BossHealthBar : MonoBehaviour
{
    [Header("Boss Reference")]
    [SerializeField] private BossEnemy boss;
    [SerializeField] private string bossDisplayName = "THE BOSS";

    [Header("UI Elements")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image  fillImage;       // Optional: the slider fill image for color changes
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text phaseLabel;

    [Header("Phase Colors")]
    [SerializeField] private Color phase1Color = new Color(0.9f, 0.4f, 0.1f);
    [SerializeField] private Color phase2Color = new Color(0.8f, 0.1f, 0.8f);
    [SerializeField] private Color phase3Color = new Color(0.9f, 0.05f, 0.05f);

    private void OnEnable()
    {
        if (boss == null)
        {
            Debug.LogWarning("BossHealthBar: Boss reference is not assigned in the Inspector!", this);
            return;
        }

        boss.OnHealthChanged += HandleHealthChanged;
        boss.OnPhaseChanged  += HandlePhaseChanged;
        boss.OnBossDefeated  += HandleBossDefeated;

        if (bossNameText != null)
            bossNameText.text = bossDisplayName;
        else
            Debug.LogWarning("BossHealthBar: Boss Name Text is not assigned in the Inspector!", this);

        if (healthSlider != null)
            healthSlider.value = 1f;
        else
            Debug.LogWarning("BossHealthBar: Health Slider is not assigned in the Inspector!", this);

        if (phaseLabel == null)
            Debug.LogWarning("BossHealthBar: Phase Label is not assigned in the Inspector!", this);

        HandlePhaseChanged(BossEnemy.BossPhase.Phase1_Projectile);
    }

    private void OnDisable()
    {
        if (boss == null) return;

        boss.OnHealthChanged -= HandleHealthChanged;
        boss.OnPhaseChanged  -= HandlePhaseChanged;
        boss.OnBossDefeated  -= HandleBossDefeated;
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (healthSlider != null)
            healthSlider.value = (float)current / max;
    }

    private void HandlePhaseChanged(BossEnemy.BossPhase phase)
    {
        Color c = phase switch
        {
            BossEnemy.BossPhase.Phase1_Projectile => phase1Color,
            BossEnemy.BossPhase.Phase2_Sniper     => phase2Color,
            _                                      => phase3Color,
        };

        if (fillImage  != null) fillImage.color = c;

        if (phaseLabel != null)
        {
            phaseLabel.text = phase switch
            {
                BossEnemy.BossPhase.Phase1_Projectile => "ARMED",
                BossEnemy.BossPhase.Phase2_Sniper     => "FOCUSED",
                _                                      => "ENRAGED",
            };
            phaseLabel.color = c;
        }
    }

    private void HandleBossDefeated()
    {
        gameObject.SetActive(false);
    }
}
