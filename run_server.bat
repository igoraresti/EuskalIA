@echo off
echo Starting Euskal IA Server...
cd server\EuskalIA.Server
dotnet run --launch-profile http
pause
