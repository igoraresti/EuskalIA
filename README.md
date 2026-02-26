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

## 🌟 Características Principales

EuskalIA no es solo un clon de aplicaciones tradicionales de idiomas. Es un ecosistema completo preparado para Producción:

- **🔐 Social Authentication Segura:** Flujos OAuth integrados para Google y Facebook. Registro automático y generación de alias ("Nicknames") aleatorios respetuosos con la privacidad en un solo clic.
- **🌍 Internacionalización (i18n):** Interfaz disponible y traducida dinámicamente en **Español, Euskera, Inglés, Francés y Polaco**.
- **🧠 Generación de Ejercicios por IA:** Integrado con modelos generativos para adaptar la educación dinámicamente y no depender de un banco de preguntas estático. (Framework listo vía `MockAIService`).
- **🏆 Sistema de Gamificación (Leaderboards):** Ranking Global y Personal. Gana puntos de experiencia (XP), mantén rachas de días seguidos ("Streaks") y gana *Txanponak* (monedas virtuales).
- **🛡️ Panel de Administración (Admin Dashboard):** Exclusivo para usuarios con rol `Admin`. Interfaz web integrada para ver métricas, listar usuarios y activar/desactivar cuentas conflictivas.
- **🗑️ Gestión de Privacidad (Deactivation Flow):** Flujo completo automatizado donde un usuario puede solicitar la desactivación de su cuenta. Tras confirmarlo por Email (con token seguro + PIN), desaparece de forma segura sin romper la base de datos (Soft Delete).

---

## 📱 Descripción de Pantallas y Experiencia de Usuario

La arquitectura Frontend React Native (Expo) está dividida en pantallas fluidas y nativas:

| Pantalla | Descripción |
| :--- | :--- |
| **Onboarding** | La puerta de entrada. Presenta el logo de forma atractiva con animaciones suaves usando `react-native-reanimated`. |
| **Login / Registro** | Experiencia unificada con validaciones, control de vista de contraseñas, olvidé mi contraseña y botones nativos para Google y Facebook. El registro permite seleccionar el idioma materno desde el inicio. |
| **Home (Dashboard)** | El núcleo del progreso. Muestra en una cabecera flotante las estadísticas (Racha de fuego 🔥, Monedas 🪙 y Bandera de idioma 🇪🇸). Debajo, un listado de las Lecciones del curso disponibles. |
| **Lesson (Quiz UI)** | Pantalla inmersiva para contestar ejercicios. Incluye barras de progreso (`ProgressBar`), opciones interactivas y modales de éxito/fracaso con el característico estilete gamificado. |
| **Leaderboard** | Un podio visual donde competir con el resto del mundo clasificando por mayor cantidad de XP ganada al resolver lecciones. |
| **Profile** | Un rincón personal. Permite visualizar el avatar, consultar cuándo te uniste, cambiar tu `Username/Nickname`, re-seleccionar tu idioma (aplicación en tiempo real de traducciones) y un botón de peligro para solicitar la desactivación de cuenta. |
| **Admin Panel** | *Sólo accesible por el Rol Admin, oculto al resto*. Muestra estadísticas críticas del servidor (usuarios totales, número de lecciones) y una tabla paginada de todos los jugadores con poder absoluto de "baneo temporal" (Toggle Active). |

---

## 💻 Arquitectura y Stack Tecnológico

El repositorio es un **Monorepo** que alberga ambas partes fundamentales:

### 1. Servidor API (`/server`)
- **Framework:** `ASP.NET Core 9.0 Web API`.
- **OR/M:** `Entity Framework Core`
- **Autenticación:** Sistema de claims basado en `JWT Bearer`. Separación de roles (User/Admin).
- **Base de Datos:** SQLite automatizado. En el primer arranque crea el archivo `euskalia.db` e inyecta usuarios de prueba (incluido un usuario Admin root `igoraresti`).
- **Testeo:** Suite completa usando `xUnit` y un proveedor en memoria In-Memory DB.

### 2. Cliente Móvil y Web (`/app`)
- **Framework:** `React Native` gestionado nativamente a través de `Expo SDK`.
- **Páginas / Navegación:** `React Navigation` (Stack & Bottom Tabs).
- **Comunicaciones HTTP:** Cliente `Axios`, encapsulado en una clase `apiService` centralizada.
- **Persistencia:** `AsyncStorage` para control mágico de sesiones y tokens.
- **Estilos:** Creados artesanalmente pero simulando estándares modernos de Tailwind/Bootstrap mediante Theme Colors en un `config.ts` único.
- **Testeo:** React Native Testing Library / Jest.

---

## 🚀 Despliegue e Instalación

EuskalIA está codificado y arquitectado con mentalidad *Cloud-Ready*. Desde el uso de variables de entorno `.env` encriptadas hasta proxys inversos dinámicos.

> [!TIP]  
> 📖 Para una guía Exhaustiva, Paso A Paso y "De Cero a Héroe" sobre cómo desplegar este repositorio en un Windows Server con HTTPS usando Nginx/Caddy y DNS gratuitos, por favor consulta nuestro manual oficial:
>
> 👉 **[📓 DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md)**

### Desarrollo Local (Quick-Start)

Si solo quieres trastear con el código en tu ordenador:

**1. Arrancar el Backend:**
```bash
cd server/EuskalIA.Server
dotnet run
# Escuchará en: http://localhost:5235
```

**2. Arrancar el Frontend:**
Abre una terminal nueva:
```bash
cd app
npm install
npm run web  # Versión de navegador
# O bien:
npx expo start # Escanea el QR con tu móvil y la app "Expo Go"
```

**(O alternativamente, en Mac o Linux, simplemente tira de consola en la raíz y ejecuta `./start.sh` para hacer ambos procesos de golpe).*

---
<div align="center">
  <i>Diseñado y Programado a la vieja usanza del buen código C# y la magia de React.</i><br/>
  <b>Desarrollado por @igoraresti</b>
</div>
