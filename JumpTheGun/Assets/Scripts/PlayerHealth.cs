using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHP = 100;
    private int currentHP;
    [SerializeField] protected TMP_Text hpText;

    private void Awake()
    {
        currentHP = maxHP;
        hpText.text = currentHP.ToString();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentHP -= amount;
        currentHP = Mathf.Max(0, currentHP);
        hpText.text = currentHP.ToString();

        Debug.Log($"Player took {amount} damage. HP: {currentHP}/{maxHP}");

        if (currentHP == 0)
        {
            Debug.Log("Player died.");
            Destroy(gameObject);
        }
    }
}
