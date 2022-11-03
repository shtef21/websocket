using System.Net.WebSockets;
using System.Linq;

namespace WebSocket.Core
{
    public class WebSocketHandler
    {

        public static async Task HandleWebSocket(HttpContext context)
        {
            // From here we will handle the web socket request
            using var socket = await context.WebSockets.AcceptWebSocketAsync();

            Console.WriteLine(" -> A new client connected!");
            
            // 1. Extract useful information from HttpContext
            string requestRoute = context.Request.Path.ToString();
            string username = context.Request.Query["username"];

            // Initialize containers for reading
            bool connectionAlive = true;
            List<byte> webSocketPayload = new List<byte>(1024 * 4); // 4 KB initial capacity
            byte[] tempMessage = new byte[1024 * 4]; // Message reader

            // 2. Connection loop
            while (connectionAlive)
            {
                // Empty the container
                webSocketPayload.Clear();

                // Message handler
                WebSocketReceiveResult? webSocketResponse;

                // Read message in a loop until fully read
                do
                {
                    // Wait until client sends message
                    webSocketResponse = await socket.ReceiveAsync(tempMessage, CancellationToken.None);

                    // Save bytes
                    webSocketPayload.AddRange(new ArraySegment<byte>(tempMessage, 0, webSocketResponse.Count));
                }
                while (webSocketResponse.EndOfMessage == false);
                
                // Process the message
                if (webSocketResponse.MessageType == WebSocketMessageType.Text)
                {
                    // 3. Convert textual message from bytes to string
                    string message = System.Text.Encoding.UTF8.GetString(webSocketPayload.ToArray());

                    Console.WriteLine("Client says {0}", message);
                }
                else if (webSocketResponse.MessageType == WebSocketMessageType.Close)
                {
                    // 4. Close the connection
                    connectionAlive = false;
                }
            }

            Console.WriteLine(" -> A client disconnected.");
        }

    }
}
