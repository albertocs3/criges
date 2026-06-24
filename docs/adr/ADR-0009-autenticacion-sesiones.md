# ADR-0009: Usar access token breve, refresh token opaco y sesión persistida

## Estado

Aceptada.

## Contexto

El sistema requiere usuario y contraseña, sesión única, bloqueo por intentos fallidos, caducidad por inactividad y revocación al cambiar contraseña, rol o permisos.

## Decisión

La autenticación usará:

- Contraseña con hash PBKDF2 versionado.
- Access token JWT de duración breve.
- Refresh token opaco, rotatorio y almacenado solo como hash.
- Sesión persistida en base de datos.

## Alternativas consideradas

- Cookies de servidor: posibles, pero menos naturales para cliente WPF con API.
- JWT largo sin sesión persistida: simplifica, pero dificulta revocación y sesión única.
- Tokens almacenados en claro: descartado por seguridad.

## Consecuencias

- Cada petición validará token, sesión activa, usuario activo y versión de seguridad.
- El refresh token se protegerá en el cliente con DPAPI.
- El servidor podrá revocar sesiones de forma inmediata.
- El flujo de login y refresh requiere pruebas de concurrencia.

