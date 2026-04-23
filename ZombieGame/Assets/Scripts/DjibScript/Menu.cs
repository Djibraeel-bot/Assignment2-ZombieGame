using UnityEngine;
using TMPro;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using Unity.Netcode;

public class Menu : MonoBehaviour
{
  [Header("UI Elements")]
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private TMP_InputField portInput;
    [Header("Defaults")]
    [SerializeField] private string defaultIP = "127.0.0.1";
    [SerializeField] private ushort defaultPort = 7777;
    [SerializeField] private UnityTransport transport;
    [SerializeField] private NetworkManager networkManager;
  public  void StartHost()
    {
        ushort port = GetPort();
        transport.SetConnectionData("0.0.0.0", port); //listen on all interfaces
        //set connection data first
        networkManager.StartHost(); 
        // host loads game scene for host
        networkManager.SceneManager.LoadScene("GameScene", LoadSceneMode.Single);
    }

    public void JoinGame()
    {
        string ip = GetIP();
        ushort port = GetPort();

        transport.SetConnectionData(ip, port);
        networkManager.StartClient();
    }

    public void StartServerOnly()
    {
        ushort port = GetPort();
        transport.SetConnectionData("0.0.0.0", port); //listen on all interfaces
        networkManager.StartServer();
    }

    private string GetIP()
    {
        if (!ipInput || string.IsNullOrWhiteSpace(ipInput.text))
        {
            return defaultIP;
        }
        return ipInput.text;
    }
    private ushort GetPort()
    {
        if (!portInput || !ushort.TryParse(portInput.text, out ushort port))
        {
            return defaultPort;
        }
        return port;
    }


    
}
