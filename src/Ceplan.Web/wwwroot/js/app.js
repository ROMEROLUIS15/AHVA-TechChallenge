"use strict";
// Interacciones de cliente del portal de acceso CEPLAN (TypeScript, sin dependencias).
// Se compila a wwwroot/js/app.js con `tsc` (ver tsconfig.json).
(function () {
    "use strict";
    /** Mostrar/ocultar la contraseña (icono de ojo). */
    function initPasswordToggle() {
        const toggles = document.querySelectorAll("[data-toggle-password]");
        toggles.forEach((btn) => {
            btn.addEventListener("click", () => {
                const wrapper = btn.closest(".input-icon");
                const input = wrapper === null || wrapper === void 0 ? void 0 : wrapper.querySelector("[data-password-input]");
                if (!input) {
                    return;
                }
                const willShow = input.type === "password";
                input.type = willShow ? "text" : "password";
                btn.setAttribute("aria-pressed", String(willShow));
            });
        });
    }
    /** Validación en vivo del documento y habilitación del botón "Ingresar". */
    function initLoginValidation() {
        const form = document.getElementById("loginForm");
        if (!form) {
            return;
        }
        const docInput = form.querySelector("[data-doc-input]");
        const passInput = form.querySelector("[data-password-input]");
        const submit = form.querySelector("[data-login-submit]");
        const docTypes = form.querySelectorAll("[data-doc-type]");
        if (!docInput || !passInput || !submit) {
            return;
        }
        const currentDocType = () => {
            const checked = Array.from(docTypes).find((r) => r.checked);
            return checked ? checked.value : "Dni";
        };
        const isDocValid = () => {
            const value = docInput.value.trim();
            if (currentDocType() === "Dni") {
                return /^\d{8}$/.test(value); // DNI: exactamente 8 dígitos
            }
            return value.length >= 6 && value.length <= 20; // CE
        };
        const refresh = () => {
            submit.disabled = !(isDocValid() && passInput.value.length > 0);
        };
        docInput.addEventListener("input", refresh);
        passInput.addEventListener("input", refresh);
        docTypes.forEach((r) => r.addEventListener("change", refresh));
        refresh();
    }
    /** Cambio de tabs en la página de perfil. */
    function initProfileTabs() {
        const tabs = document.querySelectorAll("[data-tab-target]");
        if (tabs.length === 0) {
            return;
        }
        tabs.forEach((tab) => {
            tab.addEventListener("click", () => {
                const target = tab.getAttribute("data-tab-target");
                tabs.forEach((t) => t.classList.remove("active"));
                tab.classList.add("active");
                document.querySelectorAll("[data-tab-panel]").forEach((panel) => {
                    panel.classList.toggle("active", panel.getAttribute("data-tab-panel") === target);
                });
            });
        });
    }
    document.addEventListener("DOMContentLoaded", () => {
        initPasswordToggle();
        initLoginValidation();
        initProfileTabs();
    });
})();
