using System;
using System.Net;
using System.Text;
using System.Net.WebSockets;


namespace PlanetSideWebSocket
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static void Simple(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseUrls("http://localhost:56854");

            var app = builder.Build();

            app.UseWebSockets();
            app.Map("/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using var ws = await context.WebSockets.AcceptWebSocketAsync();
                    var bytes = Encoding.UTF8.GetBytes("Successfully connected!");
                    var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                    if (ws.State == WebSocketState.Open)
                    {
                        await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
                else context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            });

            app.Run();
        }
    }
}
