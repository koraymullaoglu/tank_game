using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button joinClientButton;
    [SerializeField] private TMP_InputField ipInputField;
    [SerializeField] private TextMeshProUGUI errorText;
    [SerializeField] private Canvas uiCanvas; // Canvas referansı

    void Start()
    {
        // if (startServerButton == null)
        // {
        //     Debug.LogError("StartServerButton atanmamış!");
        //     return;
        // }
        // if (joinClientButton == null)
        // {
        //     Debug.LogError("JoinClientButton atanmamış!");
        //     return;
        // }
        // if (ipInputField == null)
        // {
        //     Debug.LogError("IPInputField atanmamış!");
        //     return;
        // }
        // if (errorText == null)
        // {
        //     Debug.LogError("ErrorText atanmamış!");
        //     return;
        // }
        // if (uiCanvas == null)
        // {
        //     Debug.LogError("UICanvas atanmamış!");
        //     return;
        // }

        startServerButton.onClick.AddListener(OnStartServer);
        joinClientButton.onClick.AddListener(OnJoinClient);
        errorText.text = "";
        if (NetworkManager.instance != null)
        {
            NetworkManager.instance.OnClientConnectionError += OnClientConnectionError;
        }
    }

    private void OnStartServer()
    {
        if (NetworkManager.instance == null)
        {
            Debug.LogError("NetworkManager instance null! Sahneye NetworkManager ekleyin.");
            errorText.text = "NetworkManager not found!";
            return;
        }
        NetworkManager.instance.StartServer();
        uiCanvas.gameObject.SetActive(false); 
        errorText.text = "";
    }

    private void OnJoinClient()
    {
        if (NetworkManager.instance == null)
        {
            Debug.LogError("NetworkManager instance null! Sahneye NetworkManager ekleyin.");
            errorText.text = "NetworkManager not found!";
            return;
        }
        string ip = ipInputField.text;
        if (string.IsNullOrEmpty(ip))
        {
            errorText.text = "Lütfen Server IP Adresini Girin!";
            return;
        }
        NetworkManager.instance.StartClient(ip);
        uiCanvas.gameObject.SetActive(false);
        errorText.text = "";
    }
    private void OnClientConnectionError(string message)
    {
        errorText.text = "Connection Error: " + message;
        uiCanvas.gameObject.SetActive(true);
    }
}