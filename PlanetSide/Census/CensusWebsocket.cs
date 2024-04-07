using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using Websocket.Client;

namespace PlanetSide
{
    public class CensusWebsocket : IDisposable
    {
        public readonly string Name;
       
        private readonly ICensusStreamClient _client;
        private readonly ILogger<CensusWebsocket> _logger;
        private readonly CensusStreamSubscription _subscription;
        private List<Func<SocketResponse, bool>> _onMessageCallbacks = new List<Func<SocketResponse, bool>>();

        public CensusWebsocket(
            string name,
            ICensusStreamClient censusStreamClient,
            ILogger<CensusWebsocket> logger,
            CensusStreamSubscription subscription,
            params Func<SocketResponse, bool>[] onMessageCallbacks)
        {
            Name = name;
            _client = censusStreamClient;
            _logger = logger;
            _subscription = subscription;
            _onMessageCallbacks.AddRange(onMessageCallbacks);

            _client.OnConnect(OnConnect)
                .OnMessage(OnMessage)
                .OnDisconnect(OnDisconnect);
        }

        public Task Connect()
        {
            return _client.ConnectAsync();
        }
        public Task Disconnect()
        {
            return _client.DisconnectAsync();
        }

        public Task OnApplicationShutdown(CancellationToken cancellationToken)
        {
            return _client.DisconnectAsync();
        }

        public Task OnApplicationStartup(CancellationToken cancellationToken)
        {
            return _client.ConnectAsync();
        }

        private Task OnConnect(ReconnectionType type)
        {
            if (type == ReconnectionType.Initial)
            {
                _logger.LogInformation("Websocket Client Connected!!");
            }
            else
            {
                _logger.LogInformation("Websocket Client Reconnected!!");
            }

            _client.Subscribe(_subscription);

            return Task.CompletedTask;
        }

        private async Task OnMessage(string message)
        {
            if (message == null)
            {
                return;
            }

            try
            {
                var response = new SocketResponse()
                {
                    SocketName = Name,
                    Message = JsonDocument.Parse(message)
                };

                if (!response.Message.RootElement.TryGetProperty("payload", out var payload))
                    return;

                for (int i = 0; i < _onMessageCallbacks.Count; ++i)
                {
                    if(_onMessageCallbacks[i].Invoke(response))
                        _onMessageCallbacks.RemoveAt(i--);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(91097, "Failed to parse message: {0}", e);
            }
        }

        private Task OnDisconnect(DisconnectionInfo info)
        {
            _logger.LogInformation($"Websocket Client Disconnected: {info.Type}");

            return Task.CompletedTask;
        }

        public void AddCallback(Func<SocketResponse, bool> callback) => _onMessageCallbacks.Add(callback);
        public void RemoveCallback(int index)
        {
            _onMessageCallbacks.RemoveAt(index);
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }

    public class SocketResponse
    {
        public string SocketName { get; set; }
        public JsonDocument Message { get; set; }
    }
}
