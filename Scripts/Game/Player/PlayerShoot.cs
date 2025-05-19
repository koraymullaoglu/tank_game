using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] public GameObject bulletPrefab;
    [SerializeField] private Transform _gunOffset;
    private PlayerTank _playerTank;
    private PlayerInput _playerInput;
    public bool _canShoot = false;

    void Awake()
    {
        _playerTank = GetComponent<PlayerTank>();
        _playerInput = GetComponent<PlayerInput>();
        Debug.Log($"PlayerShoot Awake: Tank={_playerTank?.GetPlayerId()}, canShoot={_canShoot}");
    }

    public void SetCanShoot(bool canShoot)
    {
        _canShoot = canShoot;
        Debug.Log($"PlayerShoot SetCanShoot: Tank={_playerTank?.GetPlayerId()}, canShoot={_canShoot}");
    }

    void Update()
    {
        if (!_canShoot) return;

        if (_playerInput.actions["Attack"].WasPressedThisFrame())
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || _gunOffset == null)
        {
            Debug.LogWarning("bulletPrefab veya gunOffset atanmamış!");
            return;
        }
        GameObject bullet = Instantiate(bulletPrefab, _gunOffset.position, _gunOffset.rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetOwner(gameObject);
        }
        string message = $"SHOOT|{_playerTank.GetPlayerId()}|{_gunOffset.position.x}|{_gunOffset.position.y}|{_gunOffset.rotation.eulerAngles.z}";
        if (NetworkManager.instance.isServer)
            NetworkManager.instance.Broadcast(message);
        else
            NetworkManager.instance.SendMessageToServer(message);
        Debug.Log($"SHOOT mesajı gönderildi: {message}");
    }
}