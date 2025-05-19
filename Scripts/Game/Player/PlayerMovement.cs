using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 180f;
    private Rigidbody2D _rigidbody;
    private Vector2 _moveInput;
    private PlayerInput _playerInput;
    private PlayerTank _playerTank;
    private Vector3 _lastPosition;
    private float _lastRotation;

    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerTank = GetComponent<PlayerTank>();
        _playerInput = GetComponent<PlayerInput>();
        enabled = false; // Başlangıçta devre dışı
        Debug.Log($"PlayerMovement Awake: Tank={_playerTank?.GetPlayerId()}, enabled={enabled}");
    }

    void FixedUpdate()
    {
        if (!enabled) return;

        if (_playerInput.actions["Move"].IsPressed())
        {
            _moveInput = _playerInput.actions["Move"].ReadValue<Vector2>();
            Vector2 movement = _moveInput.normalized * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(_rigidbody.position + movement);
            if (_moveInput != Vector2.zero)
            {
                float angle = Mathf.Atan2(_moveInput.y, _moveInput.x) * Mathf.Rad2Deg - 90f;
                _rigidbody.rotation = Mathf.LerpAngle(_rigidbody.rotation, angle, _rotationSpeed * Time.fixedDeltaTime);
            }

            // Sadece pozisyon veya rotasyon değiştiğinde mesaj gönder
            if (Vector3.Distance(transform.position, _lastPosition) > 0.01f || Mathf.Abs(_rigidbody.rotation - _lastRotation) > 0.1f)
            {
                SendMovementData();
                _lastPosition = transform.position;
                _lastRotation = _rigidbody.rotation;
            }
        }
    }

    private void SendMovementData()
    {
        if (_playerTank == null) return;
        string message = $"MOVE|{_playerTank.GetPlayerId()}|{transform.position.x}|{transform.position.y}|{_rigidbody.rotation}";
        if (NetworkManager.instance.isServer)
            NetworkManager.instance.Broadcast(message);
        else
            NetworkManager.instance.SendMessageToServer(message);
        //Debug.Log($"MOVE sent: {message}");
    }

    public void SetEnabled(bool isEnabled)
    {
        enabled = isEnabled;
        Debug.Log($"PlayerMovement SetEnabled: Tank={_playerTank?.GetPlayerId()}, enabled={enabled}");
    }
}