using SocketIOClient;
using System;
using System.Threading.Tasks;

public class RxService : IDisposable
{
    private readonly SocketIOClient.SocketIO _socket;

    // Event ให้ subscribe ใน UI
    public event Action<string> OnTriggerReceived;

    public RxService()
    {
        _socket = new SocketIOClient.SocketIO("http://127.0.0.1:6426", new SocketIOClient.SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        _socket.OnConnected += (s, e) =>
        {
            Console.WriteLine("Socket connected!");
        };

        // รับ event trigger-queue
        _socket.On("trigger-queue", response =>
        {
            try
            {
                var data = response.GetValue<string>();
                OnTriggerReceived?.Invoke(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing data: " + ex.Message);
            }
        });

        ConnectAsync();
    }

    private async Task ConnectAsync()
    {
        try
        {
            await _socket.ConnectAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Socket connect error: " + ex.Message);
        }
    }

    public void Dispose()
    {
        _socket?.Dispose();
    }
}
