using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHP = 100;
    private int currentHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentHP -= amount;
        currentHP = Mathf.Max(0, currentHP);

        Debug.Log($"Player took {amount} damage. HP: {currentHP}/{maxHP}");

        if (currentHP == 0)
        {
            Debug.Log("Player died.");
            Destroy(gameObject);
        }
    }
}
