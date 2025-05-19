using UnityEngine;

public class TankHealth : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 30; // 3 mermi (her biri 10 hasar)
    private int _currentHealth;
    private PlayerTank _playerTank;

    void Awake()
    {
        _playerTank = GetComponent<PlayerTank>();
        _currentHealth = _maxHealth;
        Debug.Log($"TankHealth başlatıldı: Tank={_playerTank?.GetPlayerId()}, Can={_currentHealth}");
    }

    public void TakeDamage(int damage)
    {
        if (_currentHealth <= 0) return;
        _currentHealth = Mathf.Max(0, _currentHealth - damage);
        Debug.Log($"Tank hasar aldı: Tank={_playerTank?.GetPlayerId()}, Hasar={damage}, Kalan Can={_currentHealth}");
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void SetHealth(int health)
    {
        _currentHealth = health;
        Debug.Log($"Tank canı güncellendi: Tank={_playerTank?.GetPlayerId()}, Can={_currentHealth}");
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public int GetHealth()
    {
        return _currentHealth;
    }

    private void Die()
    {
        Debug.Log($"Tank öldü: Tank={_playerTank?.GetPlayerId()}");
        Destroy(gameObject);
    }
}