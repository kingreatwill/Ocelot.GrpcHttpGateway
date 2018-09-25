
set PROTOC_DIR=..\..\grpc\
set MODEL_DIR=..\Models\
set SERVICE_DIR=..\Services\

%PROTOC_DIR%protoc PlayerSearch.proto --csharp_out %MODEL_DIR% --grpc_out %SERVICE_DIR% --plugin=protoc-gen-grpc=%PROTOC_DIR%grpc_csharp_plugin.exe

pause

