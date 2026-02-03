@echo off
echo --- Iniciando Euskal IA Full Stack ---

echo --- Limpiando procesos previos ---
taskkill /F /IM dotnet.exe /T 2>nul
taskkill /F /FI "WINDOWTITLE eq Metro" /T 2>nul

echo --- Iniciando Servidor .NET (Backend) ---
start "Servidor EuskalIA" cmd /k "cd server\EuskalIA.Server && dotnet run"

echo --- Esperando al backend... ---
timeout /t 5

echo --- Iniciando App Web (Frontend) ---
start "Frontend EuskalIA" cmd /k "cd app && npm run web"

echo --- Todo listo. Revisa las nuevas ventanas de comandos ---
