using Newtonsoft.Json.Linq;
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
        _socket = new SocketIOClient.SocketIO("http://10.32.242.69:6430", new SocketIOClient.SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        _socket.OnConnected += (s, e) =>
        {
            Console.WriteLine("Socket connected!");
        };

        // รับ event trigger-queue
        //_socket.On("trigger-queue", response =>
        //{
        //    try
        //    {
        //        var data = response.GetValue<string>();
        //        OnTriggerReceived?.Invoke(data);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error parsing data: " + ex.Message);
        //    }
        //});

        //_socket.On("trigger-queue", response =>
        //{
        //    try
        //    {
        //        // ดึงค่ามาเป็น string JSON
        //        string raw = response.GetValue<string>();

        //        // ส่งต่อให้ WPF
        //        OnTriggerReceived?.Invoke(raw);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Error in trigger-queue: " + ex.Message);
        //    }
        //});

        _socket.On("trigger-queue", response =>
        {
            try
            {
                var arr = response.GetValue<object[]>();   // System.Text.Json deserialize
                string raw = System.Text.Json.JsonSerializer.Serialize(arr); // แปลงกลับเป็น string JSON

                Console.WriteLine("RAW trigger-queue: " + raw);

                // Parse ด้วย Newtonsoft
                JArray jarr = JArray.Parse(raw);

                OnTriggerReceived?.Invoke(jarr.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing trigger-queue: " + ex.Message);
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
