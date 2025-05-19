using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.Concurrent;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    private UdpClient udpClient;
    private Dictionary<string, IPEndPoint> players = new Dictionary<string, IPEndPoint>();
    private bool _isServer = false;
    private string playerId;

    [SerializeField] private int port = 9050;
    [SerializeField] private GameObject playerPrefab;
    private readonly ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        while (mainThreadActions.TryDequeue(out Action action))
        {
            action?.Invoke();
        }
    }

    public void StartServer()
    {
        _isServer = true;
        playerId = Guid.NewGuid().ToString();
        try
        {
            udpClient = new UdpClient(port);
            udpClient.BeginReceive(ReceiveCallback, null);
            players.Add(playerId, new IPEndPoint(IPAddress.Any, port));
            Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f), 0);
            SpawnPlayer(playerId, spawnPos);
            Broadcast($"SPAWN|{playerId}|{spawnPos.x}|{spawnPos.y}");
            Debug.Log($"Server started on port: {port}, playerId: {playerId}, players: {players.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Server could not started: {e.Message}");
        }
    }
    public event Action<string> OnClientConnectionError;

    public void StartClient(string serverIp)
    {
        _isServer = false;
        playerId = Guid.NewGuid().ToString();
        try
        {
            udpClient = new UdpClient();
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), port);
            players.Add(playerId, serverEndPoint);
            udpClient.Connect(serverIp, port);
            SendMessageToServer($"CONNECT|{playerId}");
            udpClient.BeginReceive(ReceiveCallback, null);
            Debug.Log($"Connected to server: {serverIp}, playerId: {playerId}");
        }
        catch (System.Exception e)
        { 
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient = null;
            }
            mainThreadActions.Enqueue(() => OnClientConnectionError?.Invoke(e.Message));
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            if (udpClient == null)
                return;
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = udpClient.EndReceive(ar, ref sender);
            string message = Encoding.UTF8.GetString(data);
            ProcessMessage(message, sender);
            udpClient.BeginReceive(ReceiveCallback, null);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Message error: {e.Message}");
        }
    }

    private void ProcessMessage(string message, IPEndPoint sender)
    {
        string[] parts = message.Split('|');
        if (parts.Length < 1) return;
        string command = parts[0];
        
        try
        {
            if (command == "CONNECT" && _isServer)
            {
                string newPlayerId = parts[1];
                if (!players.ContainsKey(newPlayerId))
                {
                    players.Add(newPlayerId, sender);
                    Debug.Log($"Client added: {newPlayerId}, IP: {sender}, Toplam oyuncu: {players.Count}");
                    mainThreadActions.Enqueue(() =>
                    {
                        Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-5f, 5f), 0);
                        SpawnPlayer(newPlayerId, spawnPos);
                        Debug.Log($"Client spawned: {newPlayerId} at {spawnPos}");
                        Broadcast($"SPAWN|{newPlayerId}|{spawnPos.x}|{spawnPos.y}");
                        foreach (var player in players)
                        {
                            if (player.Key != newPlayerId)
                            {
                                Vector3 pos = FindPlayerPosition(player.Key);
                                SendMessageToPlayer($"SPAWN|{player.Key}|{pos.x}|{pos.y}", sender);
                            }
                        }
                    });
                }
            }
            else if (command == "SPAWN")
            {
                string pid = parts[1];
                Vector3 pos = new Vector3(float.Parse(parts[2]), float.Parse(parts[3]), 0);
                mainThreadActions.Enqueue(() =>
                {
                    Debug.Log($"SPAWN message recieved pid={pid}, pos={pos}");
                    PlayerTank existingTank = FindObjectsByType<PlayerTank>(FindObjectsSortMode.None)
                        .FirstOrDefault(p => p.GetPlayerId() == pid);
                    if (existingTank == null)
                    {
                        SpawnPlayer(pid, pos);
                    }
                    else
                    {
                        Debug.Log($"Tank exists pid={pid}, pos={existingTank.transform.position}");
                    }
                });
            }
            else if (command == "MOVE")
            {
                string pid = parts[1];
                float x = float.Parse(parts[2]);
                float y = float.Parse(parts[3]);
                float rot = float.Parse(parts[4]);

                mainThreadActions.Enqueue(() =>
                {
                    UpdatePlayerState(pid, new Vector3(x, y, 0), rot);
                });
            }
            else if (command == "SHOOT")
            {
                string pid = parts[1];
                Vector3 pos = new Vector3(float.Parse(parts[2]), float.Parse(parts[3]), 0);
                Quaternion rot = Quaternion.Euler(0, 0, float.Parse(parts[4]));

                mainThreadActions.Enqueue(() =>
                {
                    SpawnBullet(pid, pos, rot);
                });
            }
            else if (command == "DAMAGE")
            {
                string pid = parts[1];
                int health = int.Parse(parts[2]);

                mainThreadActions.Enqueue(() =>
                {
                    UpdateHealth(pid, health);


                    Debug.Log("CheckGameOver");
                    CheckGameOver();

                });
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Mesaj işleme hatası: {message}, hata: {e.Message}");
        }
    }

    public void SendMessageToServer(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length);
            Debug.Log($"Sunucuya gönderildi: {message}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Mesaj gönderme hatası: {e.Message}");
        }
    }

    public void SendMessageToPlayer(string message, IPEndPoint target)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            udpClient.Send(data, data.Length, target);
            Debug.Log($"Message sent: {message} to {target}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Message error: {message} to {target}, error: {e.Message}");
        }
    }

    public void Broadcast(string message)
    {
        if (_isServer)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            foreach (var player in players)
            {
                if (player.Key != playerId)
                {
                    try
                    {
                        udpClient.Send(data, data.Length, player.Value);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Broadcast error: {message} to {player.Key}, error: {e.Message}");
                    }
                }
            }
        }
    }

    private void SpawnPlayer(string pid, Vector3 position)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("playerPrefab missing!");
            return;
        }
        GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
        PlayerTank playerTank = player.GetComponent<PlayerTank>();
        if (playerTank == null)
        {
            Debug.LogError("PlayerTank is missing in player prefab!");
            Destroy(player);
            return;
        }
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement is missing in player prefab!");
            Destroy(player);
            return;
        }
        PlayerShoot playerShoot = player.GetComponent<PlayerShoot>();
        if (playerShoot == null)
        {
            Debug.LogError("PlayerShoot is missing in player prefab!");
            Destroy(player);
            return;
        }
        playerTank.SetPlayerId(pid);
        playerMovement.SetEnabled(pid == playerId);
        playerShoot.SetCanShoot(pid == playerId);
        Debug.Log($"Tank spawned: {pid} at {position}, Movement enabled: {playerMovement.enabled}, CanShoot: {playerShoot._canShoot}");
    }

    private void SpawnBullet(string pid, Vector3 position, Quaternion rotation)
    {
        PlayerShoot playerShoot = FindFirstObjectByType<PlayerShoot>();
        if (playerShoot == null || playerShoot.bulletPrefab == null)
        {
            Debug.LogWarning("PlayerShoot or bulletPrefab is missing");
            return;
        }
        GameObject bullet = Instantiate(playerShoot.bulletPrefab, position, rotation);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript == null)
        {
            Debug.LogWarning("Bullet prefab is missing");
            Destroy(bullet);
            return;
        }
        PlayerTank ownerTank = FindObjectsByType<PlayerTank>(FindObjectsSortMode.None).FirstOrDefault(p => p.GetPlayerId() == pid);
        if (ownerTank != null)
        {
            bulletScript.SetOwner(ownerTank.gameObject);
        }
        else
        {
            Debug.LogWarning($"no PlayerTank: {pid}");
        }
    }

    private void UpdatePlayerState(string pid, Vector3 position, float rotation)
    {
        PlayerTank tank = FindObjectsByType<PlayerTank>(FindObjectsSortMode.None).FirstOrDefault(p => p.GetPlayerId() == pid);
        if (tank != null)
        {
            tank.transform.position = position;
            tank.transform.rotation = Quaternion.Euler(0, 0, rotation);
        }
        else
        {
            Debug.LogWarning($"no tank: {pid}");
        }
    }

    private void UpdateHealth(string pid, int health)
    {
        PlayerTank tank = FindObjectsByType<PlayerTank>(FindObjectsSortMode.None).FirstOrDefault(p => p.GetPlayerId() == pid);
        if (tank != null)
        {
            TankHealth tankHealth = tank.GetComponent<TankHealth>();
            if (tankHealth != null)
            {
                tankHealth.SetHealth(health);
            }
        }
        else
        {
            Debug.LogWarning($"no tank: {pid}");
        }
    }

    private Vector3 FindPlayerPosition(string pid)
    {
        PlayerTank tank = FindObjectsByType<PlayerTank>(FindObjectsSortMode.None).FirstOrDefault(p => p.GetPlayerId() == pid);
        return tank != null ? tank.transform.position : Vector3.zero;
    }

    private void CheckGameOver()
    {
        // Sahnede canlı tankları bul
        var aliveTanks = FindObjectsByType<PlayerTank>(FindObjectsSortMode.None)
            .Where(t => t.GetComponent<TankHealth>()?.GetHealth() > 0)
            .ToList();

        if (aliveTanks.Count == 1)
        {
            string winnerId = aliveTanks[0].GetPlayerId();
            Debug.Log($"GAME OVER! Kazanan: {winnerId}");
            Broadcast($"GAME_OVER|{winnerId}");
        }
    }

    public bool isServer { get { return _isServer; } }

    void OnDestroy()
    {
        udpClient?.Close();
        Debug.Log("NetworkManager destroyed. UDP client closed.");
    }
}