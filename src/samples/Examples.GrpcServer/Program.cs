using Examples.GrpcModels;
using Grpc.Core;
using System;

namespace Examples.GrpcServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var Port = 50051;
            Server server = new Server
            {
                Services = {
                    PlayerSearch.BindService(new PlayerSearchServiceImpl()),
                    //BuiltHelloDemoSrv.BindService(new HelloworldImpl()).Intercept(
                    //        new ServerCallContextInterceptor(ctx =>
                    //            {
                    //                Console.WriteLine(ctx);
                    //            }
                    //        )
                    //),
                },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) },
            };
            var s = PlayerSearch.Descriptor.FullName;
            var dsfs = PlayerSearchReflection.Descriptor;

            Console.WriteLine("PlayerSearch server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();
            server.ShutdownAsync().Wait();
            Console.ReadLine();
        }
    }
}