#!/bin/bash

# Euskal IA Unified Start Script (Mac/Linux)

# Load centralized environment variables
if [ -f .env ]; then
  echo "--- Cargando variables de entorno (.env) ---"
  set -a
  source .env
  set +a
fi

# Verify Node version
echo "Using Node from: $(which node)"
node --version

# Kill processes on backend HTTPS port 7229, HTTP port 5235, and frontend port 8081
lsof -ti:7229 | xargs kill -9 2>/dev/null
lsof -ti:5235 | xargs kill -9 2>/dev/null
lsof -ti:8081 | xargs kill -9 2>/dev/null

echo "--- Iniciando Servidor .NET (Backend) ---"
cd server/EuskalIA.Server
dotnet run --launch-profile https &
BACKEND_PID=$!

echo "--- Esperando a que el servidor esté listo... ---"
sleep 5

echo "--- Iniciando App Web (Frontend) ---"
cd ../../app
export EXPO_HTTPS=1
npm run web

# Cleanup backend on exit
trap "kill $BACKEND_PID" EXIT
