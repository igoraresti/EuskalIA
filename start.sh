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

echo "--- Asegurando Base de Datos (Docker SQL Server) ---"
./server/start-db.sh
if [ $? -ne 0 ]; then
  echo "Error al levantar la base de datos. Abortando."
  exit 1
fi

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
