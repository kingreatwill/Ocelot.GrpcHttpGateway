# Ocelot.GrpcHttpGateway
grpc service gateway used ocelot



## What functions does he have?

* load .dll files
* load proto files

## Getting Started

* cd samples\OcelotGateway and dotnet run
* cd samples\Examples.GrpcServer  and dotnet run
* copy Examples.GrpcModels.dll to samples\OcelotGateway\bin\Debug\netcoreapp2.1\plugins
* curl http://localhost:5000/grpc/PLAYERSEARCH/SEARCHTEAM
* curl http://localhost:5000/grpc/PLAYERSEARCH/SearchPlayer_ServerStream
