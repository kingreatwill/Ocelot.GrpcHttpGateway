# Ocelot.GrpcHttpGateway
grpc service gateway used ocelot


> master not available  ,see https://github.com/kingreatwill/Ocelot.GrpcHttpGateway/tree/v0.0.1

## What functions does he have?

* load .dll files
* load proto files
* support header
* support unary,client streaming, server streaming, duplex streaming

## Getting Started

* cd samples\OcelotGateway and dotnet run
* cd samples\Examples.GrpcServer  and dotnet run
* copy Examples.GrpcModels.dll to samples\OcelotGateway\bin\Debug\netcoreapp2.1\plugins
* curl http://localhost:5000/grpc/PLAYERSEARCH/SEARCHTEAM
* curl http://localhost:5000/grpc/PLAYERSEARCH/SearchPlayer_ServerStream
