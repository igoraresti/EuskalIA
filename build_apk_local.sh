#!/bin/bash

# =================================================================
# EuskalLingo APK Build Script (Local)
# Usage: ./build_apk_local.sh [dev|prod]
# =================================================================

ENV=${1:-dev}
CONFIG_FILE="app/src/config.ts"
PROD_CONFIG="app/src/config.prod.ts"
MANIFEST="app/android/app/src/main/AndroidManifest.xml"

echo "--- Preparando Build Local de APK (Entorno: $ENV) ---"

# 1. Verificación de dependencias básicas
command -v node >/dev/null 2>&1 || { echo "[ERROR] Node.js no está instalado."; exit 1; }
command -v dotnet >/dev/null 2>&1 || { echo "[ERROR] .NET SDK no está instalado."; exit 1; }

# 2. Gestión de configuración (config.ts)
if [ "$ENV" == "prod" ]; then
    if [ ! -f "$PROD_CONFIG" ]; then
        echo "[ERROR] No se encuentra $PROD_CONFIG"
        exit 1
    fi
    echo "--- Configurando para Producción (HTTPS) ---"
    mv "$CONFIG_FILE" "${CONFIG_FILE}.dev.tmp"
    cp "$PROD_CONFIG" "$CONFIG_FILE"
else
    echo "--- Configurando para Desarrollo (HTTP) ---"
fi

# 3. Generar carpeta Android si no existe o actualizarla
echo "--- Ejecutando Expo Prebuild ---"
cd app
npx expo prebuild --platform android --no-install
cd ..

# 4. Parchear AndroidManifest.xml según el entorno
echo "--- Ajustando Manifest para $ENV ---"
if [ "$ENV" == "dev" ]; then
    # Habilitar HTTP (Cleartext) para desarrollo
    if ! grep -q "android:usesCleartextTraffic=\"true\"" "$MANIFEST"; then
        sed -i '' 's/<application/<application android:usesCleartextTraffic="true"/' "$MANIFEST"
    fi
    # Asegurar networkSecurityConfig para IPs locales
    if ! grep -q "android:networkSecurityConfig=\"@xml/network_security_config\"" "$MANIFEST"; then
        sed -i '' 's/<application/<application android:networkSecurityConfig="@xml/network_security_config"/' "$MANIFEST"
    fi
else
    # Deshabilitar HTTP (Cleartext) para producción (Solo HTTPS)
    sed -i '' 's/android:usesCleartextTraffic="true"//' "$MANIFEST"
    sed -i '' 's/android:networkSecurityConfig="@xml\/network_security_config"//' "$MANIFEST"
fi

# 5. Compilación con Gradle
echo "--- Compilando APK con Gradle ---"
cd app/android
./gradlew assembleRelease
cd ../..

# 6. Restaurar configuración de Desarrollo si era Prod
if [ "$ENV" == "prod" ] && [ -f "${CONFIG_FILE}.dev.tmp" ]; then
    echo "--- Restaurando configuración de Desarrollo ---"
    rm "$CONFIG_FILE"
    mv "${CONFIG_FILE}.dev.tmp" "$CONFIG_FILE"
fi

# 7. Resultado final
echo "--- Proceso finalizado ---"
APK_PATH="app/android/app/build/outputs/apk/release/app-release.apk"
if [ -f "$APK_PATH" ]; then
    cp "$APK_PATH" EuskalLingo.apk
    echo "[ÉXITO] APK generada en la raíz: EuskalLingo.apk ($ENV)"
else
    echo "[ERROR] No se pudo encontrar la APK generada. Revisa los errores de compilación."
    exit 1
fi
