using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using Common;
using Common.Models;
using Common.Models.Api;
using Common.Networking;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace GameClient // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        private static HttpClient httpClient = new HttpClient() { BaseAddress = new Uri("http://localhost:5060") };
        private static AuthDetails _authDetails = null;
        private static ConcurrentDictionary<ResourceType, long> resources = new  ConcurrentDictionary<ResourceType, long>();
        private static ClientWebSocket _client;
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private static Task _receivingTask = null;
        private static object _lock = new object();
        static async Task Main(string[] args)
        {
            /*
             * Commands:
             * login {udid}
             * connect
             * disconnect
             * print
             * update resourceType value
             * send resourceType value playerId
             * exit
             */


            while (true)
            {
                try
                {
                    Console.WriteLine();
                    var commandLine = Console.ReadLine();
                    var segments = new Queue<string>(commandLine.Split(" ").Where(s => !s.IsNullOrEmpty() && s != " "));
                    var initialCommand = segments.Dequeue();
                    
                    switch (initialCommand)
                    {
                        case "login" :
                            var deviceID = segments.Dequeue();
                            var loginResponse = await Login(deviceID, _cts.Token);
                            var token = new JwtSecurityTokenHandler().ReadJwtToken(loginResponse.Token);
                            _authDetails = new AuthDetails()
                            {
                                Token = loginResponse.Token,
                                PlayerId = long.Parse(token.Claims.First(c => c.Type == "UserId").Value),
                                UdId = deviceID
                            };
                            Console.WriteLine($"Logged in.");
                            Console.WriteLine($"Connection established: {loginResponse.ConnectionEsteblished}");
                            break;
                        case "connect" :
                            //todo: manage connection 
                            _cts.CancelAfter(TimeSpan.FromSeconds(1200));
                            if (_authDetails == null)
                            {
                                throw new InvalidOperationException("Cannot establish connection. Login first.");
                            }
                            await EstablishWebsocketConnection(_authDetails.Token, _cts.Token);
                            Console.WriteLine($"Connection established.");
                            break;
                        case "disconnect" :
                            throw new NotImplementedException();
                            break;
                        case "print" : 
                            resources.ToList().ForEach(r => Console.WriteLine($"{r.Key}: {r.Value}"));
                            break;
                        case "update" :
                            var updateResType = Enum.Parse<ResourceType>(segments.Dequeue(), true);
                            var updateResValue = long.Parse(segments.Dequeue());
                            await _client.SendMessage(new GameDto(new UpdateResourcesOperationRequest()
                            {
                                ResourceType = updateResType,
                                Value = updateResValue
                            }), _cts.Token);
                            break;
                        case "send" : 
                            var sendResType = Enum.Parse<ResourceType>(segments.Dequeue(), true);
                            var sendResValue = long.Parse(segments.Dequeue());
                            var sendToPlayerId = long.Parse(segments.Dequeue());
                            await _client.SendMessage(new GameDto(new SendGiftOperationRequest()
                            {
                                ResourceType = sendResType,
                                Value = sendResValue,
                                FriendPlayerId = sendToPlayerId
                            }), _cts.Token);
                            break;
                        case "exit" :
                            throw new NotImplementedException();
                            break;
                        default: 
                            Console.WriteLine();
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        private static async Task<LoginResponseModel> Login(string udid, CancellationToken cancellationToken)
        {
            var response = await httpClient.PostAsJsonAsync(@"/api/auth/login", new { UdId = udid}, cancellationToken);
            var result = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<LoginResponseModel>(result);
        }

        private static async Task EstablishWebsocketConnection(string accessToken, CancellationToken token)
        {
            //todo: jwt token
            Console.WriteLine("establishing connection..");
            _client = new ClientWebSocket();
            var serviceUri = new Uri($"ws://localhost:5060/game?access_token={accessToken}");
            await _client.ConnectAsync(serviceUri, token);
           
            ReceiveMessages(_cts.Token);
            _receivingTask =
                Task.Run(() => ReceiveMessages(_cts.Token).Wait(token), _cts.Token);
        }

        private static async Task ReceiveMessages(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _client.State == WebSocketState.Open)
            {
                try
                {
                    var result = await _client.ReceiveGameDto(_cts.Token);
                    if (result is { Message: { } } && !result.Value.Response.CloseStatus.HasValue)
                    {
                        //todo: rely on enum values instead of type names
                        if(result.Value.Message.DataTypeName == typeof(GiftEvent).FullName)
                        {
                            var giftEvent = JsonConvert.DeserializeObject<GiftEvent>(result.Value.Message.Data);
                            resources[giftEvent.ResourceType] = giftEvent.NewBalanceValue;
                            Console.WriteLine($"Gift received: {giftEvent.ResourceType} +{giftEvent.Value} from {giftEvent.GiftSender}");
                            Console.WriteLine($"New balance: {giftEvent.ResourceType}: {giftEvent.NewBalanceValue}");
                        }
                        if (result.Value.Message.DataTypeName == typeof(ResourceBalanceResponse).FullName)
                        {
                            var resourceBalance = JsonConvert.DeserializeObject<ResourceBalanceResponse>(result.Value.Message.Data);
                            resources[resourceBalance.ResourceType] = resourceBalance.NewBalanceValue;
                            Console.WriteLine($"New balance: {resourceBalance.ResourceType}: {resourceBalance.NewBalanceValue}");
                        }
                    }
                    else
                    {
                        await _client.CloseAsync(result.Value.Response.CloseStatus.Value, result.Value.Response.CloseStatusDescription, CancellationToken.None);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    
                }
            }
        }

        public class AuthDetails
        {
            public string UdId { get; set; }
            public long PlayerId { get; set; }
            public string Token { get; set; }
        }
    }
}