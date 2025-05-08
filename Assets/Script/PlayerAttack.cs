using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{
    public float damage = 8f;
    public float hitCooldown = 0.5f;
    public string targetTag = "Player";
    private Dictionary<GameObject, float> lastHitTimes = new Dictionary<GameObject, float>();
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(targetTag))
        {
            HeroKnight hero = other.GetComponent<HeroKnight>();
            Bandit bandit = other.GetComponent<Bandit>();

            if (hero != null)
            {
                hero.TakeDamage(damage);
            }
            else if (bandit != null)
            {
                bandit.TakeDamage(damage);
            }
        }
    }
    void OnTriggerStay2D(Collider2D other)
    {
        // Skip if hitting self or not a player
        if (other.gameObject == transform.root.gameObject ||
           (!other.CompareTag("Player1") && !other.CompareTag("Player2")))
            return;

        GameObject target = other.gameObject;

        // Check if we've hit this target recently
        if (lastHitTimes.ContainsKey(target))
        {
            if (Time.time - lastHitTimes[target] < hitCooldown)
                return;
            lastHitTimes[target] = Time.time;
        }
        else
        {
            lastHitTimes.Add(target, Time.time);
        }

        // Apply damage based on target type
        if (other.CompareTag("Player1"))
        {
            HeroKnight hero = target.GetComponent<HeroKnight>();
            if (hero != null)
            {
                Debug.Log($"Dealing {damage} damage to Player1");
                hero.TakeDamage(damage);
            }
        }
        else if (other.CompareTag("Player2"))
        {
            Bandit bandit = target.GetComponent<Bandit>();
            if (bandit != null)
            {
                Debug.Log($"Dealing {damage} damage to Player2");
                bandit.TakeDamage(damage);
            }
        }
    }

    void OnDisable()
    {
        lastHitTimes.Clear();
    }
}