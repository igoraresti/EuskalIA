@echo off
setlocal

echo --- Preparando Build Local de APK (Produccion) ---

if not exist "app\src\config.prod.ts" (
    echo [ERROR] No se encuentra app\src\config.prod.ts
    exit /b 1
)

echo --- Intercambiando configuracion a Produccion ---
rename "app\src\config.ts" "config.dev.ts.tmp"
copy /y "app\src\config.prod.ts" "app\src\config.ts"

echo --- Generando carpeta Android (Prebuild) ---
cd app
call npx expo prebuild --platform android

echo --- Compilando APK con Gradle ---
cd android
call gradlew assembleRelease

echo --- Restaurando configuracion de Desarrollo ---
cd ..\..
if exist "app\src\config.dev.ts.tmp" (
    del "app\src\config.ts"
    rename "app\src\config.dev.ts.tmp" "config.ts"
)

echo --- Proceso finalizado ---
if exist "app\android\app\build\outputs\apk\release\app-release.apk" (
    copy "app\android\app\build\outputs\apk\release\app-release.apk" "EuskalIA_Production.apk"
    echo [EXITO] APK generada en la raiz: EuskalIA_Production.apk
) else (
    echo [ERROR] No se pudo encontrar la APK generada. Revisa los errores de compilacion arriba.
)
pause
