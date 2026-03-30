<div align="center">
  <img src="app/assets/icon.png" width="150" alt="EuskalIA Logo">
  
  <h1>EuskalIA</h1>
  <p><strong>Aprende Euskera con IA - Descarga, Juega, Progresa</strong></p>
  <p>Una plataforma moderna y gamificada inspirada en Duolingo, potenciada por Inteligencia Artificial generativa y un robusto backend .NET.</p>

  [![React Native](https://img.shields.io/badge/React_Native-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)](https://reactnative.dev/)
  [![Expo](https://img.shields.io/badge/EXPO-000020?style=for-the-badge&logo=expo&logoColor=white)](https://expo.dev/)
  [![.NET](https://img.shields.io/badge/.NET_9-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
  [![SQLite](https://img.shields.io/badge/SQLite-07405E?style=for-the-badge&logo=sqlite&logoColor=white)](https://sqlite.org/)
</div>

<br/>

## 👤 Parte Funcional

EuskalIA no es solo un clon de aplicaciones tradicionales de idiomas. Es un ecosistema completo preparado para Producción:

### 🌟 Características Principales

- **🔐 Social Authentication Segura:** Flujos OAuth integrados para Google y Facebook. Registro automático y generación de alias ("Nicknames") aleatorios respetuosos con la privacidad en un solo clic.
- **🌍 Internacionalización (i18n):** Interfaz disponible y traducida dinámicamente en **Español, Euskera, Inglés, Francés y Polaco**.
- **🧠 Generación de Ejercicios por IA:** Integrado con modelos generativos para adaptar la educación dinámicamente y no depender de un banco de preguntas estático. (Framework listo vía `MockAIService`).
- **📅 Repaso Espaciado (SRS):** Sistema inteligente basado en el algoritmo SM-2 que programa repasos automáticos según el nivel de dominio del usuario, maximizando la retención a largo plazo.
- **🔔 Notificaciones Push:** Recordatorios diarios personalizados y localizados (según el idioma del usuario) para realizar repasos pendientes y mantener rachas, integrados mediante Expo Push API.
- **🏆 Gamificación Avanzada:** Sistema de **Rachas (Streaks)** dinámico que premia la constancia diaria y un **Cuadro de Logros** con medallas desbloqueables (Madrugador, Erudito, etc.) para motivar el progreso.
- **📊 Análisis de Errores:** Dashboard inteligente que identifica los temas donde el usuario comete más fallos (ej: declinaciones, verbos) basándose en el historial de los últimos 30 días, permitiendo un refuerzo dirigido.
- **📊 Sistema de Clasificación (Leaderboards):** Ranking Global y Personal donde competir por el mayor XP acumulado.
- **🛡️ Panel de Administración (Admin Dashboard):** Exclusivo para usuarios con rol `Admin`. Interfaz web integrada para ver métricas, listar usuarios y activar/desactivar cuentas conflictivas.
- **🗑️ Gestión de Privacidad (Deactivation Flow):** Flujo completo automatizado donde un usuario puede solicitar la desactivación de su cuenta. Tras confirmarlo por Email (con token seguro + PIN), desaparece de forma segura sin romper la base de datos (Soft Delete).

### 📱 Experiencia de Usuario y Pantallas

La arquitectura Frontend React Native (Expo) está dividida en pantallas fluidas y nativas:

| Pantalla | Descripción |
| :--- | :--- |
| **Onboarding** | La puerta de entrada. Presenta el logo de forma atractiva con animaciones suaves usando `react-native-reanimated`. |
| **Login / Registro** | Experiencia unificada con validaciones, control de vista de contraseñas, olvidé mi contraseña y botones nativos para Google y Facebook. El registro permite seleccionar el idioma materno desde el inicio. |
| **Home (Dashboard)** | El núcleo del progreso. Muestra estadísticas (Racha 🔥, XP ⭐) y un **Dashboard de Debilidades** con los temas a reforzar. Debajo, el listado de niveles (A1-B1). |
| **Lesson (Quiz UI)** | Pantalla inmersiva para contestar ejercicios. Incluye barras de progreso (`ProgressBar`), opciones interactivas y modales de éxito/fracaso con el característico estilete gamificado. |
| **Review (SRS)** | Sesión de refuerzo personalizada activada por el sistema de repaso espaciado. Enfocada en temas con menor dominio detectado por el algoritmo. |
| **Leaderboard** | Un podio visual donde competir con el resto del mundo clasificando por mayor cantidad de XP ganada al resolver lecciones. |
| **Profile** | Un rincón personal. Permite visualizar el avatar, consultar cuándo te uniste, cambiar tu `Username/Nickname`, consultar tus **Logros y Medallas** y un botón de peligro para solicitar la desactivación de cuenta. |
| **Admin Panel** | *Sólo accesible por el Rol Admin, oculto al resto*. Muestra estadísticas críticas del servidor (usuarios totales, número de lecciones) y una tabla paginada de todos los jugadores con poder absoluto de "baneo temporal" (Toggle Active). |

---

## 💻 Parte Técnica

El repositorio es un **Monorepo** que alberga ambas partes fundamentales de la arquitectura:

### 1. Servidor API (`/server`)
- **Framework:** `ASP.NET Core 10.0 Web API`.
- **OR/M:** `Entity Framework Core`
- **Lógica de Negocio:** Servicios desacoplados para Auth, AI, Email y **SRS (Algoritmo SM-2)**.
- **Autenticación:** Sistema de claims basado en `JWT Bearer`. Separación de roles (User/Admin).
- **Base de Datos:** SQLite automatizado. En el primer arranque crea el archivo `euskalia.db` e inyecta usuarios de prueba.
- **Testeo:** Suite robusta con **44 tests automáticos** usando `xUnit` y un proveedor en memoria In-Memory DB.

### 2. Cliente Móvil y Web (`/app`)
- **Framework:** `React Native` gestionado nativamente a través de `Expo SDK`.
- **Páginas / Navegación:** `React Navigation` (Stack & Bottom Tabs).
- **Comunicaciones HTTP:** Cliente `Axios`, encapsulado en una clase `apiService` centralizada.
- **Persistencia:** `AsyncStorage` para control mágico de sesiones y tokens.
- **Estilos:** Creados artesanalmente pero simulando estándares modernos de Tailwind/Bootstrap mediante Theme Colors en un `config.ts` único.
- **Testeo:** React Native Testing Library / Jest.

---

## 🛠️ Parte del Desarrollador

Esta sección está destinada al equipo de desarrollo para la configuración, despliegue local y depuración.

### 🔒 Configuración de HTTPS en Desarrollo (Expo + .NET 10)

Esta guía resume los pasos necesarios para habilitar HTTPS durante el desarrollo local entre la API y Expo, de forma que se puedan probar características en dispositivos reales de iOS y Android salvando los problemas de "Certificado no confiable".

#### 1. Backend (.NET 10)
**Generar y confiar en Certificados:**
En macOS o Windows, abre una terminal y ejecuta:
```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

**Forzar el uso del puerto HTTPS (Kestrel y launchSettings):**
El proyecto usa de forma predeterminada un perfil que escucha en el puerto HTTPS `7229`. Puedes arrancar el backend forzando este perfil (`start.sh` ya lo hace de forma automática):
```bash
dotnet run --launch-profile https
```
*(El archivo `Program.cs` del proyecto ya cuenta con las políticas de CORS pertinentes y `app.UseHttpsRedirection()` agregadas correctamente).*

#### 2. Conectividad y Dispositivos Físicos (ngrok / Dev Tunnels)
El acceso a `localhost` no funciona desde dispositivos físicos (se interpone la falta de un certificado de autoridad - CA). Para solucionarlo de la mejor forma, se recomienda exponer la API mediante un túnel seguro con certificado universal.

**Opción A: ngrok**
```bash
ngrok http http://localhost:5235
```

**Opción B: Dev Tunnels (De Visual Studio)**
```bash
devtunnel host -p 5235 --allow-anonymous
```
Ambos métodos devolverán una URL HTTPS válida y pública de forma temporal (ejemplo: `https://abcd-12.ngrok-free.app`).

#### 3. Frontend (Expo)
Para hacer que Axios llame a esta nueva URL en vez de a `localhost`, debes crear un archivo `.env` en el directorio cliente (`/app`) inyectando tu URL del túnel.

1. Crea / edita el archivo `app/.env`:
   ```env
   EXPO_PUBLIC_API_URL=https://abcd-12.ngrok-free.app/api
   ```
2. Reinicia la caché de tu bundler en Expo:
   ```bash
   npx expo start --clear
   ```
*(El archivo `config.ts` ha sido integrado para recibir automáticamente esta variable de entorno de existir).*

---

### 📱 Instalación (APK) y Depuración en Android

Si tienes el archivo `EuskalLingo.apk` empaquetado y deseas instalarlo o depurarlo en un dispositivo conectado, utiliza el ADB de Android SDK:

**Instalar APK:**
```bash
adb install -r EuskalLingo.apk
```

**Visualizar Logs y Errores (Logcat):**
Para ver los logs de la aplicación filtrando por errores críticos y contexto de JavaScript:
1. Limpiar logs previos:
   ```bash
   adb logcat -c
   ```
2. Ver logs de errores en tiempo real y componentes de React:
   ```bash
   adb logcat "*:E" | grep -iE "euskal|react|fatal|javascript"
   ```

---

## ⚖️ Licencia

Este proyecto está bajo la **Licencia MIT**.

Copyright (c) 2026 Igor Aresti

Se concede permiso por la presente, de forma gratuita, a cualquier persona que obtenga una copia de este software y de los archivos de documentación asociados (el "Software"), para utilizar el Software sin restricción, incluyendo sin limitación los derechos de uso, copia, modificación, fusión, publicación, distribución, sublicencia y/o venta de copias del Software, y para permitir a las personas a las que se les proporcione el Software a hacer lo mismo, sujeto a las siguientes condiciones:

**El aviso de copyright anterior y este aviso de permiso se incluirán en todas las copias o partes sustanciales del Software.**

---

<div align="center">
  <i>Diseñado y Programado a la vieja usanza del buen código C# y la magia de React.</i><br/>
  <b>Desarrollado por @igoraresti</b>
</div>
