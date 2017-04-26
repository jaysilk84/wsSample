using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace WsSample.Web.Handlers
{
    public class WebSocketHandler
    {
        public const int BufferSize = 4096;
        private readonly WebSocket _socket;

        public WebSocketHandler(WebSocket socket)
        {
            this._socket = socket;
        }

        private async Task Echo()
        {
            var buffer = new byte[BufferSize];
            var seg = new ArraySegment<byte>(buffer);
            var jaredSays = System.Text.Encoding.UTF8.GetBytes("Jared hates ");

            while (_socket.State == WebSocketState.Open)
            {
                var incoming = await _socket.ReceiveAsync(seg, CancellationToken.None);
                var outgoing = new ArraySegment<byte>(jaredSays.Concat(buffer).ToArray(), 0, incoming.Count + jaredSays.Length);
                await _socket.SendAsync(outgoing, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private static async Task AcceptSocketFactory(HttpContext context, Func<Task> f)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return;

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            var h = new WebSocketHandler(socket);
            await h.Echo();
        }

        public static void Map(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Use(AcceptSocketFactory);
        }
    }
}
