(() => {
    const App = {
        init() {
            this.setupMobileMenu();
            this.setupReveal();
            this.setupContactFormValidation();
        },

        setupMobileMenu() {
            const toggle = document.querySelector("[data-menu-toggle]");
            const menu = document.getElementById("mobile-menu");
            if (!toggle || !menu) {
                return;
            }

            toggle.addEventListener("click", () => {
                const isOpen = menu.classList.toggle("is-open");
                toggle.setAttribute("aria-expanded", String(isOpen));
            });
        },

        setupReveal() {
            const items = document.querySelectorAll("[data-reveal]");
            if (items.length === 0) {
                return;
            }

            if (!("IntersectionObserver" in window)) {
                items.forEach((item) => item.classList.add("is-visible"));
                return;
            }

            const observer = new IntersectionObserver((entries, currentObserver) => {
                entries.forEach((entry) => {
                    if (!entry.isIntersecting) {
                        return;
                    }

                    entry.target.classList.add("is-visible");
                    currentObserver.unobserve(entry.target);
                });
            }, { threshold: 0.15, rootMargin: "0px 0px -25px 0px" });

            items.forEach((item, index) => {
                item.style.transitionDelay = `${Math.min(index, 5) * 80}ms`;
                observer.observe(item);
            });
        },

        setupContactFormValidation() {
            const form = document.querySelector("[data-contact-form]");
            if (!form) {
                return;
            }

            const status = form.querySelector("[data-form-status]");
            const fields = Array.from(form.querySelectorAll("[data-validate]"));

            const setFieldError = (input, message) => {
                const field = input.closest(".field");
                if (!field) {
                    return;
                }

                const messageEl = field.querySelector("[data-field-message]");
                field.classList.add("is-invalid");
                input.setAttribute("aria-invalid", "true");
                if (messageEl) {
                    messageEl.textContent = message;
                }
            };

            const clearFieldError = (input) => {
                const field = input.closest(".field");
                if (!field) {
                    return;
                }

                const messageEl = field.querySelector("[data-field-message]");
                field.classList.remove("is-invalid");
                input.removeAttribute("aria-invalid");
                if (messageEl) {
                    messageEl.textContent = "";
                }
            };

            const validateField = (input) => {
                const value = input.value.trim();
                if (!value) {
                    setFieldError(input, "Bu alan zorunludur.");
                    return false;
                }

                if (input.type === "email") {
                    const ok = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
                    if (!ok) {
                        setFieldError(input, "Geçerli bir e-posta giriniz.");
                        return false;
                    }
                }

                clearFieldError(input);
                return true;
            };

            form.addEventListener("input", (event) => {
                const target = event.target;
                if (!(target instanceof HTMLInputElement || target instanceof HTMLTextAreaElement)) {
                    return;
                }
                if (!target.hasAttribute("data-validate")) {
                    return;
                }

                validateField(target);
            });

            form.addEventListener("submit", (event) => {
                let valid = true;
                fields.forEach((field) => {
                    if (!validateField(field)) {
                        valid = false;
                    }
                });

                if (!status) {
                    return;
                }

                if (!valid) {
                    event.preventDefault();
                    status.textContent = "Formu göndermeden önce işaretli alanları düzeltin.";
                    status.style.color = "#dc2626";
                    return;
                }

                status.textContent = "Form gönderiliyor...";
                status.style.color = "#0f766e";
            });
        }
    };

    document.addEventListener("DOMContentLoaded", () => App.init());
})();
