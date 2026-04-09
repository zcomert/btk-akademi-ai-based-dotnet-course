(() => {
    const App = {
        init() {
            this.setupMobileMenu();
            this.setupCarousel();
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

        setupCarousel() {
            const root = document.querySelector("[data-carousel]");
            if (!root) {
                return;
            }

            const slides = Array.from(root.querySelectorAll("[data-carousel-slide]"));
            const prevButton = root.querySelector("[data-carousel-prev]");
            const nextButton = root.querySelector("[data-carousel-next]");
            const currentEl = root.querySelector("[data-carousel-current]");

            if (slides.length <= 1) {
                return;
            }

            let currentIndex = slides.findIndex((slide) => slide.classList.contains("is-active"));
            if (currentIndex < 0) {
                currentIndex = 0;
            }

            let autoTimer = 0;
            const intervalMs = 5600;

            const render = () => {
                slides.forEach((slide, index) => {
                    const isActive = index === currentIndex;
                    slide.classList.toggle("is-active", isActive);
                    slide.setAttribute("aria-hidden", String(!isActive));
                });

                if (currentEl) {
                    currentEl.textContent = String(currentIndex + 1).padStart(2, "0");
                }
            };

            const step = (delta) => {
                currentIndex = (currentIndex + delta + slides.length) % slides.length;
                render();
            };

            const restartAuto = () => {
                window.clearInterval(autoTimer);
                autoTimer = window.setInterval(() => step(1), intervalMs);
            };

            prevButton?.addEventListener("click", () => {
                step(-1);
                restartAuto();
            });

            nextButton?.addEventListener("click", () => {
                step(1);
                restartAuto();
            });

            root.addEventListener("mouseenter", () => window.clearInterval(autoTimer));
            root.addEventListener("mouseleave", restartAuto);

            render();
            restartAuto();
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
                        setFieldError(input, "Gecerli bir e-posta giriniz.");
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
                    status.textContent = "Formu gondermeden once isaretli alanlari duzeltin.";
                    status.style.color = "#b91c1c";
                    return;
                }

                status.textContent = "Form gonderiliyor...";
                status.style.color = "#009668";
            });
        }
    };

    document.addEventListener("DOMContentLoaded", () => App.init());
})();
