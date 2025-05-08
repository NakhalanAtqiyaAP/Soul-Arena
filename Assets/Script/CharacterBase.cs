using UnityEngine;
using System;

public abstract class CharacterBase : MonoBehaviour
{
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public event Action<float> OnHealthChanged;

    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    public virtual void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"{gameObject.name} took {damage} damage. Current health: {currentHealth}");
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected abstract void Die();
}