using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace FitnessTrainerPlatform.Services;

/// <summary>
/// TCP-based inter-process chat relay. First instance starts the server;
/// additional instances connect as clients. Messages broadcast to all peers.
/// </summary>
public class ChatIpcService : IDisposable
{
    private const int Port = 9876;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private TcpListener? _listener;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;
    private readonly List<TcpClient> _clients = new();
    private readonly object _clientLock = new();
    private bool _isServer;

    public event Action<ChatIpcMessage>? MessageReceived;

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(IPAddress.Loopback, Port);
            _stream = _client.GetStream();
            _ = Task.Run(() => ListenLoop(_client, _cts.Token));
        }
        catch
        {
            _isServer = true;
            _listener = new TcpListener(IPAddress.Loopback, Port);
            _listener.Start();
            _ = Task.Run(AcceptLoop);
        }
    }

    private async Task AcceptLoop()
    {
        while (_cts is { IsCancellationRequested: false })
        {
            try
            {
                var client = await _listener!.AcceptTcpClientAsync();
                lock (_clientLock) _clients.Add(client);
                _ = Task.Run(() => ListenLoop(client, _cts!.Token));
            }
            catch
            {
                break;
            }
        }
    }

    private async Task ListenLoop(TcpClient client, CancellationToken token)
    {
        using var stream = client.GetStream();
        var buffer = new byte[8192];
        var sb = new StringBuilder();

        while (!token.IsCancellationRequested && client.Connected)
        {
            int read;
            try
            {
                read = await stream.ReadAsync(buffer, token);
            }
            catch
            {
                break;
            }

            if (read == 0) break;

            sb.Append(Encoding.UTF8.GetString(buffer, 0, read));
            var content = sb.ToString();
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            sb.Clear();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    var msg = JsonSerializer.Deserialize<ChatIpcMessage>(line, JsonOptions);
                    if (msg != null)
                    {
                        if (_isServer)
                            _ = RelayToOtherClientsAsync(client, line + "\n");
                        MessageReceived?.Invoke(msg);
                    }
                }
                catch
                {
                    sb.Append(line);
                }
            }
        }

        lock (_clientLock) _clients.Remove(client);
    }

    private async Task RelayToOtherClientsAsync(TcpClient sender, string payload)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        List<TcpClient> copy;
        lock (_clientLock) copy = _clients.Where(c => c != sender).ToList();
        foreach (var c in copy)
        {
            try
            {
                if (c.Connected)
                    await c.GetStream().WriteAsync(bytes);
            }
            catch { /* ignore */ }
        }
    }

    public async Task BroadcastAsync(ChatIpcMessage message)
    {
        var json = JsonSerializer.Serialize(message, JsonOptions) + "\n";
        var bytes = Encoding.UTF8.GetBytes(json);

        if (_isServer)
        {
            List<TcpClient> copy;
            lock (_clientLock) copy = _clients.ToList();
            foreach (var c in copy)
            {
                try
                {
                    if (c.Connected)
                        await c.GetStream().WriteAsync(bytes);
                }
                catch { /* ignore disconnected clients */ }
            }
        }
        else if (_stream != null)
        {
            await _stream.WriteAsync(bytes);
        }

        MessageReceived?.Invoke(message);
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _stream?.Dispose();
        _client?.Dispose();
        _listener?.Stop();
        lock (_clientLock)
        {
            foreach (var c in _clients) c.Dispose();
            _clients.Clear();
        }
        _cts?.Dispose();
    }
}

public class ChatIpcMessage
{
    public string TutelageId { get; set; } = string.Empty;
    public string SenderUserId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
