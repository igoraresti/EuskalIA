<div align="center">
  <img src="app/assets/icon.png" width="150" alt="EuskalIA Logo">
  
  <h1>EuskalIA</h1>
  <p><strong>Aprende Euskera con IA - Descarga, Juega, Progresa</strong></p>
  <p>Una plataforma moderna y gamificada inspirada en Duolingo, potenciada por Inteligencia Artificial generativa y un robusto backend .NET 9.</p>

  [![React Native](https://img.shields.io/badge/React_Native-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)](https://reactnative.dev/)
  [![Expo](https://img.shields.io/badge/EXPO-000020?style=for-the-badge&logo=expo&logoColor=white)](https://expo.dev/)
  [![.NET](https://img.shields.io/badge/.NET_10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
  [![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/sql-server/)
  [![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)](https://www.docker.com/)
</div>

---

## 📖 Sobre EuskalIA

EuskalIA es una aplicación de aprendizaje de euskera que utiliza Inteligencia Artificial para generar ejercicios personalizados. Combina técnicas modernas como RAG (Retrieval-Augmented Generation) para basar el aprendizaje en materiales reales y algoritmos de repaso espaciado (SRS) para optimizar la retención.

## 🚀 Inicio Rápido (Quick Start)

Si tienes instalado **Docker**, **Node.js** y **.NET 9**, puedes poner en marcha todo el ecosistema con un solo comando:

```bash
# 1. Clonar el repositorio e instalar dependencias del frontend
npm install --prefix app

# 2. Configurar variables de entorno (Ver sección 'Configuración')
cp app/.env.example app/.env # Si existe un ejemplo, o créalo manualmente

# 3. Lanzar todo (Base de Datos + API + App Web)
chmod +x start.sh
./start.sh
```

---

## 📋 Prerrequisitos

Para contribuir o ejecutar el proyecto localmente, asegúrate de tener:

- **.NET 9 SDK**: Para el backend.
- **Node.js (v18+)**: Para el frontend (Expo).
- **Docker Desktop**: Para levantar SQL Server rápidamente.
- **Android Studio / Xcode**: (Opcional) Si quieres probar en emuladores nativos.
- **ngrok o Dev Tunnels**: (Recomendado) Para probar la API desde un dispositivo físico.

---

## 📂 Estructura del Proyecto

Este repositorio es un **monorepo** organizado de la siguiente manera:

```text
.
├── app/                # Frontend en React Native (Expo)
│   ├── src/            # Código fuente (Screens, Components, Services)
│   └── assets/         # Imágenes, iconos y recursos
├── server/             # Backend en ASP.NET Core 9
│   ├── EuskalIA.Server/# Proyecto de API Web
│   ├── EuskalIA.Tests/ # Suite de tests xUnit
│   └── db-init/        # Scripts de inicialización de SQL Server
├── start.sh            # Script unificado para desarrollo
├── build_apk_local.sh  # Script para generar APK de Android
└── README.md           # Esta guía
```

---

## ⚙️ Configuración

### Variables de Entorno (.env)
El frontend requiere conocer la URL de la API y las claves de IA. Crea un archivo `app/.env`:

```env
# URL de la API (Usa tu IP local o túnel HTTPS)
EXPO_PUBLIC_API_URL=https://tu-url-de-ngrok.app/api

# Configuración de Google Gemini (IA)
GeminiSettings__ApiKey=TU_API_KEY_AQUI
GeminiSettings__Model=gemini-1.5-flash
```

### Base de Datos
El sistema usa SQL Server 2022. Puedes gestionarlo fácilmente con Docker:
```bash
./server/start-db.sh
```
*Configuración por defecto:* Usuario `sa`, Password `YourStrong!Pass123`, Puerto `1433`.

---

## 🛠️ Guía de Desarrollo Detallada

### Backend (.NET 9)
Ubicado en `/server`. Utiliza Entity Framework Core y una arquitectura orientada a servicios.
- **Ejecutar API:** `cd server/EuskalIA.Server && dotnet run --launch-profile https`
- **Generar Migración:** `dotnet ef migrations add NombreMigracion --project server/EuskalIA.Server`

### Frontend (Expo)
Ubicado en `/app`. Es una aplicación universal (iOS, Android, Web).
- **Ejecutar Web:** `npm run web --prefix app`
- **Ejecutar Expo Go:** `npm start --prefix app`

---

## 🧪 Testing

Mantenemos la calidad del código mediante tests automatizados:

- **Backend (xUnit):**
  ```bash
  cd server && dotnet test
  ```
- **Frontend (Jest):**
  ```bash
  cd app && npm test
  ```

---

## 📱 Generación de APK (Android)

Para generar una APK local para pruebas en dispositivos físicos:
```bash
# Para desarrollo (Permite tráfico HTTP)
./build_apk_local.sh dev

# Para producción (Solo HTTPS)
./build_apk_local.sh prod
```
El archivo resultante aparecerá en la raíz como `EuskalLingo.apk`.

---

## 🛡️ Características Principales (Resumen)

- **IA RAG:** Generación de ejercicios basada en PDFs pedagógicos.
- **SRS (Algoritmo SM-2):** Repaso espaciado inteligente.
- **Gamificación:** Rachas, XP, Logros y Rankings.
- **i18n:** Traducido a 5 idiomas.
- **Admin Panel:** Gestión de usuarios y métricas para administradores.
- **Privacidad:** Flujo de desactivación de cuenta conforme a estándares.

---

## ⚖️ Licencia

Este proyecto está bajo la **Licencia MIT**. Ver el archivo README original para el texto completo del copyright de Igor Aresti.

<div align="center">
  <i>Desarrollado con ❤️ para el aprendizaje del Euskera.</i><br/>
  <b>@igoraresti</b>
</div>
