using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    private Rigidbody2D _rigidbody;
    private GameObject _owner;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
        {
            Debug.LogError("Mermi prefab'ında Rigidbody2D bileşeni eksik!");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _rigidbody.linearVelocity = transform.up * _speed;
        Debug.Log($"Mermi başlatıldı: Hız={_rigidbody.linearVelocity}, Sahip={_owner?.GetComponent<PlayerTank>()?.GetPlayerId()}");
    }

    public void SetOwner(GameObject owner)
    {
        _owner = owner;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Harita sınırıyla çarpışma
        if (other.CompareTag("Border"))
        {
            Destroy(gameObject);
            Debug.Log($"Mermi sınırla çarpıştı ve yok edildi: {transform.position}");
            return;
        }

        // Tankla çarpışma
        if (_owner == null) return;
        PlayerTank hitTank = other.GetComponent<PlayerTank>();
        if (hitTank != null && hitTank.gameObject != _owner)
        {
            TankHealth tankHealth = hitTank.GetComponent<TankHealth>();
            if (tankHealth != null)
            {
                tankHealth.TakeDamage(10);
                string message = $"DAMAGE|{hitTank.GetPlayerId()}|{tankHealth.GetHealth()}";
                if (NetworkManager.instance.isServer)
                    NetworkManager.instance.Broadcast(message);
                else
                    NetworkManager.instance.SendMessageToServer(message);
                Debug.Log($"DAMAGE mesajı gönderildi: {message}");
            }
            Destroy(gameObject);
        }
    }
}