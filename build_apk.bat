@echo off
setlocal

echo --- Preparando Build de APK (Producion) ---

if not exist "app\src\config.prod.ts" (
    echo [ERROR] No se encuentra app\src\config.prod.ts
    echo Por favor, copia app\src\config.prod.ts.template a app\src\config.prod.ts y configura tu IP real.
    exit /b 1
)

echo --- Intercambiando configuracion a Produccion ---
rename "app\src\config.ts" "config.dev.ts.tmp"
copy /y "app\src\config.prod.ts" "app\src\config.ts"

echo --- Iniciando EAS Build (Android APK) ---
cd app
call eas build -p android --profile preview

echo --- Restaurando configuracion de Desarrollo ---
cd ..
del "app\src\config.ts"
rename "app\src\config.dev.ts.tmp" "config.ts"

echo --- Proceso finalizado. Revisa el enlace de EAS para descargar la APK ---
pause
