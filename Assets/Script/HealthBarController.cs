using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBarController : MonoBehaviour
{
    private Slider healthSlider;
    [SerializeField] private CharacterBase character;

    void Awake()
    {
        healthSlider = GetComponent<Slider>();
        if (character == null)
            character = GetComponentInParent<CharacterBase>();

        if (character == null)
            Debug.LogError("Character reference not set in HealthBarController");
    }

    void Start()
    {
        if (character != null)
        {
            healthSlider.maxValue = character.MaxHealth;
            healthSlider.value = character.CurrentHealth;
            character.OnHealthChanged += UpdateHealthBar;
            Debug.Log($"HealthBar initialized for {character.gameObject.name}");
        }
    }

    void OnDestroy()
    {
        if (character != null)
            character.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(float currentHealth)
    {
        healthSlider.value = currentHealth;
        Debug.Log($"HealthBar updated: {currentHealth}");
    }
}