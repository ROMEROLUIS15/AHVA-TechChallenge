// Interacciones de cliente del portal de acceso CEPLAN (TypeScript, sin dependencias).
// Se compila a wwwroot/js/app.js con `tsc` (ver tsconfig.json).
(function () {
    "use strict";

    /** Mostrar/ocultar la contraseña (icono de ojo). */
    function initPasswordToggle(): void {
        const toggles = document.querySelectorAll<HTMLButtonElement>("[data-toggle-password]");
        toggles.forEach((btn) => {
            btn.addEventListener("click", () => {
                const wrapper = btn.closest(".input-icon");
                const input = wrapper?.querySelector<HTMLInputElement>("[data-password-input]");
                if (!input) {
                    return;
                }
                const willShow = input.type === "password";
                input.type = willShow ? "text" : "password";
                btn.classList.toggle("revealed", willShow);
                btn.setAttribute("aria-pressed", String(willShow));
            });
        });
    }

    /** Validación en vivo del documento y habilitación del botón "Ingresar". */
    function initLoginValidation(): void {
        const form = document.getElementById("loginForm");
        if (!form) {
            return;
        }
        const docInput = form.querySelector<HTMLInputElement>("[data-doc-input]");
        const passInput = form.querySelector<HTMLInputElement>("[data-password-input]");
        const submit = form.querySelector<HTMLButtonElement>("[data-login-submit]");
        const docTypes = form.querySelectorAll<HTMLInputElement>("[data-doc-type]");
        if (!docInput || !passInput || !submit) {
            return;
        }

        const currentDocType = (): string => {
            const checked = Array.from(docTypes).find((r) => r.checked);
            return checked ? checked.value : "Dni";
        };

        const isDocValid = (): boolean => {
            const value = docInput.value.trim();
            if (currentDocType() === "Dni") {
                return /^\d{8}$/.test(value); // DNI: exactamente 8 dígitos
            }
            return value.length >= 6 && value.length <= 20; // CE
        };

        const refresh = (): void => {
            submit.disabled = !(isDocValid() && passInput.value.length > 0);
        };

        docInput.addEventListener("input", refresh);
        passInput.addEventListener("input", refresh);
        docTypes.forEach((r) => r.addEventListener("change", refresh));
        refresh();
    }

    /** Cambio de tabs en la página de perfil. */
    function initProfileTabs(): void {
        const tabs = document.querySelectorAll<HTMLButtonElement>("[data-tab-target]");
        if (tabs.length === 0) {
            return;
        }
        tabs.forEach((tab) => {
            tab.addEventListener("click", () => {
                const target = tab.getAttribute("data-tab-target");
                tabs.forEach((t) => t.classList.remove("active"));
                tab.classList.add("active");
                document.querySelectorAll<HTMLElement>("[data-tab-panel]").forEach((panel) => {
                    panel.classList.toggle("active", panel.getAttribute("data-tab-panel") === target);
                });
            });
        });
    }

    /** Timeout de sesión por inactividad: aviso con cuenta regresiva, extender o expirar. */
    function initSessionTimeout(): void {
        const shell = document.querySelector<HTMLElement>(".app-shell[data-session-inactivity]");
        const modal = document.getElementById("sessionModal");
        if (!shell || !modal) {
            return;
        }

        const inactivityMs = parseInt(shell.dataset.sessionInactivity || "0", 10) * 1000;
        const countdownSecs = parseInt(shell.dataset.sessionCountdown || "0", 10);
        const keepAliveUrl = shell.dataset.keepaliveUrl || "";
        const expireUrl = shell.dataset.expireUrl || "";
        if (inactivityMs <= 0 || countdownSecs <= 0 || expireUrl === "") {
            return;
        }

        const modalRoot: HTMLElement = modal;
        const remainingEl = modalRoot.querySelector<HTMLElement>("[data-session-remaining]");
        const extendBtn = modalRoot.querySelector<HTMLButtonElement>("[data-session-extend]");
        let inactivityTimer = 0;
        let countdownTimer = 0;
        let remaining = countdownSecs;

        const startInactivity = (): void => {
            window.clearTimeout(inactivityTimer);
            inactivityTimer = window.setTimeout(showWarning, inactivityMs);
        };

        function showWarning(): void {
            remaining = countdownSecs;
            if (remainingEl) {
                remainingEl.textContent = String(remaining);
            }
            modalRoot.hidden = false;
            countdownTimer = window.setInterval(() => {
                remaining -= 1;
                if (remainingEl) {
                    remainingEl.textContent = String(Math.max(remaining, 0));
                }
                if (remaining <= 0) {
                    window.clearInterval(countdownTimer);
                    window.location.href = expireUrl;
                }
            }, 1000);
        }

        const extend = (): void => {
            window.clearInterval(countdownTimer);
            modalRoot.hidden = true;
            if (keepAliveUrl !== "") {
                fetch(keepAliveUrl, { credentials: "same-origin" }).catch(() => undefined);
            }
            startInactivity();
        };

        ["mousemove", "keydown", "click", "scroll"].forEach((evt) => {
            document.addEventListener(evt, () => {
                if (!modalRoot.hidden) {
                    return;
                }
                startInactivity();
            }, { passive: true });
        });

        extendBtn?.addEventListener("click", extend);
        startInactivity();
    }

    /** Auto-cierre de los toasts de aviso (p. ej. sesión expirada) tras unos segundos. */
    function initToasts(): void {
        const toasts = document.querySelectorAll<HTMLElement>(".flash-toasts .alert-ceplan");
        toasts.forEach((toast) => {
            window.setTimeout(() => {
                toast.classList.add("is-hiding");
                window.setTimeout(() => toast.remove(), 400);
            }, 6000);
        });
    }

    document.addEventListener("DOMContentLoaded", () => {
        initPasswordToggle();
        initLoginValidation();
        initProfileTabs();
        initSessionTimeout();
        initToasts();
    });
})();
