#!/bin/bash

echo "--- Preparando Build de APK (Producción) ---"

if [ ! -f "app/src/config.prod.ts" ]; then
    echo "[ERROR] No se encuentra app/src/config.prod.ts"
    echo "Por favor, copia app/src/config.prod.ts.template a app/src/config.prod.ts y configura tu IP real."
    exit 1
fi

echo "--- Intercambiando configuración a Producción ---"
mv app/src/config.ts app/src/config.dev.ts.tmp
cp app/src/config.prod.ts app/src/config.ts

echo "--- Iniciando EAS Build (Android APK) ---"
cd app
eas build -p android --profile preview

echo "--- Restaurando configuración de Desarrollo ---"
cd ..
rm app/src/config.ts
mv app/src/config.dev.ts.tmp app/src/config.ts

echo "--- Proceso finalizado ---"
