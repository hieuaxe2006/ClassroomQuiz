@echo off
cd /d "%~dp0"
dotnet build "Server\ClassroomQuizServer.csproj" --nologo -v minimal
