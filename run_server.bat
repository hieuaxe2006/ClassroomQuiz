@echo off
cd /d "%~dp0"
dotnet run --project "Server\ClassroomQuizServer.csproj" --no-launch-profile -- %*
