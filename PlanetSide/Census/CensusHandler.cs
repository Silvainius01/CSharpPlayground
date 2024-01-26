using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using DaybreakGames.Census;
using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DaybreakGames.Census.Operators;

namespace PlanetSide
{
    public class CensusHandler : IDisposable
    {
        public readonly CensusOptions CensusOptions;

        CensusClient client;
        Dictionary<string, CensusWebsocket> websockets = new Dictionary<string, CensusWebsocket>();

        public CensusHandler()
        {
            CensusOptions = new CensusOptions()
            {
                CensusServiceId = "Silvainius"
            };

            client = CreateClient();
        }

        public async Task ConnectClientAsync(string key)
        {
            if (websockets.ContainsKey(key))
                await websockets[key].Connect();
        }

        //public async Task ConnectClientsAsync()
        //{
        //    foreach (var kvp in websockets)
        //        await kvp.Value.Connect();
        //}

        public CensusWebsocket AddSubscription(string key, CensusStreamSubscription subscription)
        {
            if (!websockets.ContainsKey(key))
            {
                CensusWebsocket socket = new CensusWebsocket(
                    key,
                    CreateStreamClient(),
                    Program.LoggerFactory.CreateLogger<CensusWebsocket>(),
                    subscription//,
                                //(msg, logger) => logger.LogInformation($"{key} Received Message: {msg.RootElement.ToString()}")
                );

                websockets.Add(key, socket);
                return socket;
            }
            return null;
        }
        public bool AddActionToSubscription(string key, Func<SocketResponse, bool> action)
        {
            if (!websockets.ContainsKey(key))
                return false;
            websockets[key].AddCallback(action);
            return true;
        }

        public async Task DisconnectSocketAsync(string key)
        {
            if (websockets.ContainsKey(key))
            {
                await websockets[key].Disconnect();
                websockets.Remove(key);
            }
        }

        public CensusQuery GetClientQuery(string service)
            => client.CreateQuery(service);

        public JsonElement GetCharacter(string id)
        {
            var task = GetCharacterQuery(id).GetAsync();
            task.Wait();
            return task.Result;
        }
        public CensusQuery GetCharacterQuery(string id)
        {
            var q = GetClientQuery("character");
            q.Where("character_id").Equals(id);
            return q;
        }

        public IEnumerable<JsonElement> GetCharacters(params string[] characterIds)
        {
            var task = GetCharactersQuery(characterIds).GetListAsync();
            task.Wait();
            return task.Result;
        }
        public CensusQuery GetCharactersQuery(params string[] characterIds)
        {
            var q = GetClientQuery("character");
            q.Where("character_id").Equals(characterIds);
            return q;
        }

        private CensusClient CreateClient()
        {
            return new CensusClient(
                Options.Create(CensusOptions),
                Program.LoggerFactory.CreateLogger<CensusClient>());
        }
        private CensusStreamClient CreateStreamClient()
        {
            return new CensusStreamClient(
                Options.Create(CensusOptions),
                Program.LoggerFactory.CreateLogger<CensusStreamClient>());
        }
        public void Dispose()
        {
            foreach (var socket in websockets.Values)
                socket?.Dispose();
        }
    }
}
