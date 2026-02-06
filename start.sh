#!/bin/bash

# Euskal IA Unified Start Script (Mac/Linux)

# Add node_env to PATH
export PATH="$(pwd)/node_env/bin:$PATH"

# Verify Node version
echo "Using Node from: $(which node)"
node --version

echo "--- Limpiando puertos ---"
# Kill processes on backend port 5235 and frontend port 8081
lsof -ti:5235 | xargs kill -9 2>/dev/null
lsof -ti:8081 | xargs kill -9 2>/dev/null

echo "--- Iniciando Servidor .NET (Backend) ---"
cd server/EuskalIA.Server
dotnet run --launch-profile http &
BACKEND_PID=$!

echo "--- Esperando a que el servidor esté listo... ---"
sleep 5

echo "--- Iniciando App Web (Frontend) ---"
cd ../../app
npm run web

# Cleanup backend on exit
trap "kill $BACKEND_PID" EXIT
