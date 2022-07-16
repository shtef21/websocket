using System.Net.WebSockets;
using System.Linq;

namespace WebSocket.Core
{
    public class WebSocketHandler
    {

        public static async Task HandleWebSocket(HttpContext context, System.Net.WebSockets.WebSocket socket)
        {
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
        }

        public static async Task HandleWebSocket2 (HttpContext context, System.Net.WebSockets.WebSocket socket)
        {
            // Extract useful information from HttpContext
            string requestRoute = context.Request.Path.ToString();

            // Define loop variables
            bool connectionAlive = true;
            List<byte> webSocketPayload = new List<byte>(1024 * 4); // 4 KB initial capacity
            byte[] tempMessage = new byte[1024 * 4]; // Message reader

            // Request loop
            try
            {
                while(connectionAlive)
                {
                    if (webSocketPayload.Capacity > 1024 * 1024 * 5)
                    {
                        // Max allowed capacity is set to 4 MB
                        webSocketPayload.Capacity = 1024 * 1024 * 4;
                    }

                    // Web socket message class
                    WebSocketReceiveResult? webSocketResponse;

                    do
                    {
                        // Wait until client sends message
                        webSocketResponse = await socket.ReceiveAsync(tempMessage, CancellationToken.None);

                        // Save read bytes
                        webSocketPayload.AddRange(new ArraySegment<byte>(tempMessage, 0, webSocketResponse.Count));

                        // Repeat until message fully read
                    }
                    while (webSocketResponse.EndOfMessage == false);

                    // Now, handle the message

                    if (webSocketResponse.MessageType == WebSocketMessageType.Text)
                    {
                        // Convert bytes to a string
                        string message = System.Text.Encoding.UTF8.GetString(webSocketPayload.ToArray());

                        Console.WriteLine("Client says {0}", message);
                    }
                    else if (webSocketResponse.MessageType == WebSocketMessageType.Close)
                    {
                        // Client requested socket close
                        connectionAlive = false;
                    }

                }
            }
            catch (WebSocketException ex)
            {
                if (ex.WebSocketErrorCode == WebSocketError.InvalidState)
                {
                    // Cannot operate on a closed socket
                }
                Console.WriteLine("Error in web socket: {0}", ex.Message);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Unknown error in websocket: {0}", ex.Message);
            }

        }

    }
}
