document.addEventListener("DOMContentLoaded", () => {
    // 1. Sidebar Toggle
    const sidebarToggle = document.querySelector("[data-sidebar-toggle]");
    const body = document.body;
    
    // Create overlay if it doesn't exist
    if (!document.querySelector('.sidebar-overlay')) {
        const overlay = document.createElement('div');
        overlay.className = 'sidebar-overlay';
        body.appendChild(overlay);
        
        overlay.addEventListener("click", () => {
            body.classList.remove("sidebar-open");
        });
    }

    if (sidebarToggle) {
        sidebarToggle.addEventListener("click", () => {
            body.classList.toggle("sidebar-open");
        });
    }

    // 2. Theme Toggle
    const themeToggles = document.querySelectorAll("[data-theme-toggle]");
    const savedTheme = localStorage.getItem("Taybat-theme");
    const html = document.documentElement;

    if (savedTheme === "dark") {
        html.setAttribute("data-theme", "dark");
        updateThemeIcons("dark");
    }

    themeToggles.forEach(toggle => {
        toggle.addEventListener("click", () => {
            const currentTheme = html.getAttribute("data-theme");
            const newTheme = currentTheme === "dark" ? "light" : "dark";
            
            if (newTheme === "dark") {
                html.setAttribute("data-theme", "dark");
                localStorage.setItem("Taybat-theme", "dark");
            } else {
                html.removeAttribute("data-theme");
                localStorage.setItem("Taybat-theme", "light");
            }
            
            updateThemeIcons(newTheme);
        });
    });

    function updateThemeIcons(theme) {
        themeToggles.forEach(toggle => {
            const icon = toggle.querySelector("i");
            if (icon) {
                if (theme === "dark") {
                    icon.classList.remove("bi-moon", "fa-moon");
                    icon.classList.add("bi-sun", "fa-sun");
                } else {
                    icon.classList.remove("bi-sun", "fa-sun");
                    icon.classList.add("bi-moon", "fa-moon");
                }
            }
        });
    }

    // Initialize icons
    updateThemeIcons(savedTheme || "light");

    // 3. Responsive Tables
    document.querySelectorAll(".table").forEach((table) => {
        if (!table.parentElement.classList.contains("table-responsive")) {
            const wrapper = document.createElement("div");
            wrapper.className = "table-responsive shadow-sm border-0 rounded-4";
            table.parentNode.insertBefore(wrapper, table);
            wrapper.appendChild(table);
            table.classList.add("mb-0"); // Ensure no bottom margin inside wrapper
        }
    });

    // 4. Scroll Animations using Intersection Observer
    const animatedElements = document.querySelectorAll('.animate-in, .metric-card, .doctor-card, .clinic-card');
    
    if ('IntersectionObserver' in window) {
        const observerOptions = {
            root: null,
            rootMargin: '0px',
            threshold: 0.1
        };

        const observer = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    // Add animation class if it doesn't already have it
                    if (!entry.target.classList.contains('animate-in')) {
                        entry.target.classList.add('animate-in');
                    }
                    // Stop observing once animated
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        animatedElements.forEach(el => {
            // Remove animate-in initially so it can be added when scrolled into view
            if (!el.classList.contains('hero-panel')) { // Don't delay hero panel
                el.style.opacity = '0'; // Hide initially
                observer.observe(el);
            }
        });
    }

    // 5. Navbar Scroll Effect
    const navbar = document.querySelector('.app-navbar');
    if (navbar) {
        window.addEventListener('scroll', () => {
            if (window.scrollY > 10) {
                navbar.classList.add('scrolled');
            } else {
                navbar.classList.remove('scrolled');
            }
        }, { passive: true });
    }

    // 6. Metric Counter Animation
    const metricValues = document.querySelectorAll('.metric-value');
    if ('IntersectionObserver' in window && metricValues.length > 0) {
        const counterObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const target = entry.target;
                    const text = target.innerText;
                    // Check if it's a number or contains numbers (e.g. "EGP 1,000")
                    const match = text.match(/[\d,.]+/);
                    
                    if (match && !target.hasAttribute('data-animated')) {
                        target.setAttribute('data-animated', 'true');
                        const numberStr = match[0].replace(/,/g, '');
                        const finalNum = parseFloat(numberStr);
                        
                        if (!isNaN(finalNum)) {
                            animateValue(target, 0, finalNum, 1000, text.replace(match[0], '{num}'));
                        }
                    }
                    observer.unobserve(target);
                }
            });
        });

        metricValues.forEach(el => counterObserver.observe(el));
    }

    function animateValue(obj, start, end, duration, template) {
        let startTimestamp = null;
        const step = (timestamp) => {
            if (!startTimestamp) startTimestamp = timestamp;
            const progress = Math.min((timestamp - startTimestamp) / duration, 1);
            // Easing out cubic
            const easeOut = 1 - Math.pow(1 - progress, 3);
            const current = progress === 1 ? end : start + (end - start) * easeOut;
            
            // Format number
            let formatted;
            if (Number.isInteger(end)) {
                formatted = Math.floor(current).toLocaleString();
            } else {
                formatted = current.toFixed(2).toLocaleString();
            }
            
            obj.innerHTML = template.replace('{num}', formatted);
            if (progress < 1) {
                window.requestAnimationFrame(step);
            }
        };
        window.requestAnimationFrame(step);
    }
});
