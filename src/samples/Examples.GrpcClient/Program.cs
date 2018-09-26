using Examples.GrpcModels;
using Grpc.Core;
using System;
using System.Threading;

namespace Examples.GrpcClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
            var client = new PlayerSearch.PlayerSearchClient(channel);

            Console.WriteLine("Help:1,NoStream;2,ServerStream;3,ClientStream;4,DuplexStream");
            var rk = Console.ReadLine();
            while (rk != "exit")
            {
                if (rk == "1")
                {
                    var s1 = client.SearchTeam(new TeamSearchRequest
                    {
                        Country = "ct",
                        Name = "names"
                    });
                    Console.WriteLine(s1);
                }
                else if (rk == "2")
                {
                    var s2 = client.SearchPlayer_ServerStream(new PlayerSearchRequest
                    {
                        ExpectedDataCount = 10,
                        PlayerName = "pn",
                        Position = "1",
                        TeamName = "tn"
                    });
                    while (s2.ResponseStream.MoveNext(default(CancellationToken)).Result)
                    {
                        Console.WriteLine(s2.ResponseStream.Current);
                    }
                }
                else if (rk == "3")
                {
                    var s2 = client.SearchPlayer_ClientStream();

                    using (var call = client.SearchPlayer_ClientStream())
                    {
                        call.RequestStream.WriteAsync(new PlayerSearchRequest
                        {
                            ExpectedDataCount = 10,
                            PlayerName = "pn",
                            Position = "1",
                            TeamName = "tn"
                        }).Wait();
                        call.RequestStream.WriteAsync(new PlayerSearchRequest
                        {
                            ExpectedDataCount = 10,
                            PlayerName = "pn",
                            Position = "1",
                            TeamName = "tn"
                        }).Wait();
                        call.RequestStream.CompleteAsync().Wait();
                        Console.WriteLine(call.ResponseAsync.Result);
                    }
                }
                else if (rk == "4")
                {
                    using (var call = client.SearchPlayer_DuplexStream())
                    {
                        call.RequestStream.WriteAsync(new PlayerSearchRequest
                        {
                            ExpectedDataCount = 10,
                            PlayerName = "pn",
                            Position = "1",
                            TeamName = "tn"
                        }).Wait();
                        call.RequestStream.WriteAsync(new PlayerSearchRequest
                        {
                            ExpectedDataCount = 10,
                            PlayerName = "pn",
                            Position = "1",
                            TeamName = "tn"
                        }).Wait();
                        call.RequestStream.CompleteAsync().Wait();
                        while (call.ResponseStream.MoveNext(default(CancellationToken)).Result)
                        {
                            Console.WriteLine(call.ResponseStream.Current);
                        }
                        // Console.WriteLine(s2);
                    }
                }
                //else
                {
                    Console.WriteLine("Help:1,NoStream;2,ServerStream;3,ClientStream;4,DuplexStream");
                }
                rk = Console.ReadLine();
            }
        }
    }
}