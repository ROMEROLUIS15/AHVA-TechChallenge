# Pruebas E2E (Playwright)

Pruebas de extremo a extremo que conducen la **app real** en un navegador (no un servidor
de pruebas): login, interacción y verificación de lo que vería un usuario.

## Requisitos

- La app corriendo en `https://localhost:5443` (`.\run.ps1` o `dotnet run --project src/Ceplan.Web`).
- El usuario semilla disponible y **desbloqueado** (DNI `07079879` / `Ceplan2025$`).
- Node.js y Playwright instalados.

## Ejecutar

```bash
cd tests/e2e
npm install
npx playwright install chromium   # solo la primera vez (descarga el navegador)
npm run test:avatar
```

Variables opcionales: `BASE_URL`, `SEED_USER`, `SEED_PASSWORD`.

## Qué cubre `avatar-upload.e2e.js`

El flujo completo de la foto de perfil (extra): inicia sesión, sube una imagen generada al
vuelo mediante el formulario del avatar y comprueba que:

- el avatar pasa a mostrar la imagen (`.avatar-img`),
- la ruta apunta al área de avatares (`/uploads/avatars/...`),
- la imagen se sirve con `HTTP 200` y `content-type` de imagen,
- aparece el aviso "Foto de perfil actualizada".

Sale con código `0` si todo pasa, `1` si alguna comprobación falla.
