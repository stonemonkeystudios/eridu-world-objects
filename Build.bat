dotnet build eridu-world-object-host/eridu-world-object-host.csproj --runtime linux-x64 -c Release --self-contained true
tar.exe -a -c -f out.zip eridu-world-object-host/bin/Release/net6.0/linux-x64
copy "eridu-world-object-host/bin/Release/net6.0/out.zip" "release/eridu-world-object-host-linux-x64.zip" /Y