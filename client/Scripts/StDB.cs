using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using SpacetimeDB;
using Logger = StDBCraft.Scripts.Utils.Logger;

namespace StDBCraft.Scripts;

public static class StDb
{
    private static readonly Logger Logger = new(typeof(StDb));
    private static readonly SpacetimeDBClient Client = SpacetimeDBClient.instance;
    public static event Action OnReady;

    public static void Connect()
    {
        AuthToken.Init("stdbcraft");

        Client.onConnect += OnConnect;
        Client.onDisconnect += OnDisconnect;
        Client.onConnectError += OnConnectError;
        Client.onIdentityReceived += OnIdentityReceived;
        Client.onSubscriptionApplied += OnSubscriptionApplied;

        Client.Connect(AuthToken.Token ?? "", "localhost:3000", "stdbmc");
    }

    public static void Update()
    {
        Client.Update();
    }

    private static void OnIdentityReceived(string authToken, Identity identity, Address address)
    {
        AuthToken.SaveToken(authToken);
    }

    private static void OnConnect()
    {
        Logger.Info("Connected, subscribing to tables");
        Client.Subscribe(new List<string> { "SELECT * FROM *" });
    }

    private static void OnSubscriptionApplied()
    {
        Logger.Info("Subscription process completed");
        OnReady?.Invoke();
    }

    private static void OnDisconnect(WebSocketCloseStatus? status, WebSocketError? error)
    {
        Logger.Info($"Disconnected for {status}, error {error}");
    }

    private static void OnConnectError(WebSocketError? error, string message)
    {
        Logger.Info($"Connection failed for for {error}, error {message}");
    }
}