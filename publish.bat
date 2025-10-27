@echo off

rmdir /s "publish"
dotnet publish "DotnetGUI\DotnetGUI.csproj" -c Release -o "publish\bin"
dotnet publish "ExecuteShell\ExecuteShell.csproj" -c Release -p:PublishSinggleFile=true -o "publish"