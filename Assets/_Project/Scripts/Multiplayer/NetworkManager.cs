// =============================================================================
// NetworkManager.cs
// =============================================================================
// PURPOSE:
//   Netcode for GameObjects wrapper. Handles host/client start, scene mgmt,
//   and player registration. Pairs with the Node.js authoritative server.
//
// ENGINE: Unity 6, Netcode for GameObjects 2.x
// =============================================================================

using System;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using ShadowEmpire.Core;

namespace ShadowEmpire.Multiplayer
{
    [RequireComponent(typeof(NetworkManager))]
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        [Header("Connection")]
        public string defaultAddress = "127.0.0.1";
        public ushort defaultPort = 7777;

        public event Action OnConnected;
        public event Action<string> OnDisconnected;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            var nm = GetComponent<NetworkManager>();
            nm.OnClientConnectedCallback += id => { if (id == nm.LocalClientId) OnConnected?.Invoke(); };
            nm.OnClientDisconnectCallback += id => { if (id == nm.LocalClientId) OnDisconnected?.Invoke("disconnected"); };
        }

        // -------- API --------
        public void StartHost()
        {
            Configure();
            GetComponent<NetworkManager>().StartHost();
            Debug.Log("[Network] Hosting.");
        }

        public void StartClient(string address = null, ushort port = 0)
        {
            Configure(address, port);
            GetComponent<NetworkManager>().StartClient();
            Debug.Log($"[Network] Connecting to {address ?? defaultAddress}:{port}");
        }

        public void Disconnect()
        {
            var nm = GetComponent<NetworkManager>();
            if (nm.IsHost || nm.IsClient) nm.Shutdown();
        }

        private void Configure(string address = null, ushort port = 0)
        {
            var utp = GetComponent<NetworkManager>().NetworkConfig.NetworkTransport as UnityTransport;
            if (utp == null) return;
            utp.SetConnectionData(address ?? defaultAddress, port == 0 ? defaultPort : port);
        }
    }
}