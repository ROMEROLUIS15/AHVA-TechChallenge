// E2E: carga de foto de perfil, conduciendo la app real en un navegador (Playwright).
//
// Requisitos: la app corriendo (https://localhost:5443) y Playwright instalado.
// Uso:  node avatar-upload.e2e.js   (ver README.md)
//
// Verifica el flujo de extremo a extremo: login -> subir imagen -> el avatar se
// actualiza, el archivo se sirve (HTTP 200) y aparece el aviso de éxito.

const { chromium } = require("playwright");
const fs = require("fs");
const path = require("path");
const os = require("os");

const BASE = process.env.BASE_URL || "https://localhost:5443";
const USER = process.env.SEED_USER || "07079879";
const PASS = process.env.SEED_PASSWORD || "Ceplan2025$";

let failures = 0;
function check(name, condition) {
  const ok = Boolean(condition);
  console.log(`${ok ? "PASS" : "FAIL"}  ${name}`);
  if (!ok) failures++;
}

(async () => {
  const browser = await chromium.launch();
  const context = await browser.newContext({ ignoreHTTPSErrors: true, viewport: { width: 1280, height: 800 } });
  const page = await context.newPage();

  // Generar una imagen de prueba temporal.
  await page.setContent("<canvas id='c' width='200' height='200'></canvas>");
  const dataUrl = await page.evaluate(() => {
    const c = document.getElementById("c"), x = c.getContext("2d");
    x.fillStyle = "#2c5fc4"; x.fillRect(0, 0, 200, 200);
    x.fillStyle = "#fff"; x.font = "bold 90px sans-serif"; x.textAlign = "center"; x.textBaseline = "middle";
    x.fillText("JC", 100, 105);
    return c.toDataURL("image/png");
  });
  const imgPath = path.join(os.tmpdir(), `e2e-avatar-${Date.now()}.png`);
  fs.writeFileSync(imgPath, Buffer.from(dataUrl.split(",")[1], "base64"));

  // 1) Login
  await page.goto(`${BASE}/Account/Login`, { waitUntil: "networkidle" });
  await page.fill("[data-doc-input]", USER);
  await page.fill("[data-password-input]", PASS);
  await page.click("[data-login-submit]");
  await page.waitForURL(/\/Profile/i, { timeout: 15000 });
  check("login lleva al perfil", /\/Profile/i.test(page.url()));

  // 2) Subir la foto (el input hidden dispara el auto-envío del formulario).
  await page.setInputFiles("[data-avatar-input]", imgPath);
  await page.waitForLoadState("networkidle");
  await page.waitForTimeout(500);

  // 3) Verificaciones
  const img = await page.$(".avatar-img");
  const src = img ? await img.getAttribute("src") : null;
  check("el avatar muestra una imagen", Boolean(src));
  check("la ruta es del área de avatares", src && src.startsWith("/uploads/avatars/"));

  if (src) {
    const resp = await page.request.get(BASE + src, { ignoreHTTPSErrors: true });
    check("la imagen se sirve (HTTP 200)", resp.status() === 200);
    check("el content-type es imagen", (resp.headers()["content-type"] || "").startsWith("image/"));
  }

  const body = await page.innerText("body");
  check("aparece el aviso de éxito", /Foto de perfil actualizada/.test(body));

  fs.unlinkSync(imgPath);
  await browser.close();

  console.log(failures === 0 ? "\nE2E OK: todas las comprobaciones pasaron." : `\nE2E FALLÓ: ${failures} comprobación(es).`);
  process.exit(failures === 0 ? 0 : 1);
})().catch((e) => { console.error("E2E ERROR:", e.message); process.exit(1); });
