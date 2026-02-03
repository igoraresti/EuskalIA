#!/bin/bash

echo "--- Preparando Build Local de APK (Producción) ---"

if [ ! -f "app/src/config.prod.ts" ]; then
    echo "[ERROR] No se encuentra app/src/config.prod.ts"
    exit 1
fi

echo "--- Intercambiando configuración a Producción ---"
mv app/src/config.ts app/src/config.dev.ts.tmp
cp app/src/config.prod.ts app/src/config.ts

echo "--- Generando carpeta Android (Prebuild) ---"
cd app
npx expo prebuild --platform android

echo "--- Compilando APK con Gradle ---"
cd android
./gradlew assembleRelease

echo "--- Restaurando configuración de Desarrollo ---"
cd ../..
if [ -f "app/src/config.dev.ts.tmp" ]; then
    rm app/src/config.ts
    mv app/src/config.dev.ts.tmp app/src/config.ts
fi

echo "--- Proceso finalizado ---"
if [ -f "app/android/app/build/outputs/apk/release/app-release.apk" ]; then
    cp app/android/app/build/outputs/apk/release/app-release.apk EuskalIA_Production.apk
    echo "[ÉXITO] APK generada en la raíz: EuskalIA_Production.apk"
else
    echo "[ERROR] No se pudo encontrar la APK generada. Revisa los errores de compilación arriba."
fi
