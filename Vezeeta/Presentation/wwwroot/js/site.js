(() => {
  const sidebarToggle = document.querySelector("[data-sidebar-toggle]");
  if (sidebarToggle) {
    sidebarToggle.addEventListener("click", () => {
      document.body.classList.toggle("sidebar-open");
    });
  }

  document.addEventListener("click", (event) => {
    if (
      document.body.classList.contains("sidebar-open") &&
      !event.target.closest(".sidebar") &&
      !event.target.closest("[data-sidebar-toggle]")
    ) {
      document.body.classList.remove("sidebar-open");
    }
  });

  const themeToggle = document.querySelector("[data-theme-toggle]");
  const savedTheme = localStorage.getItem("vezeeta-theme");
  if (savedTheme) {
    document.documentElement.dataset.theme = savedTheme;
  }

  if (themeToggle) {
    themeToggle.addEventListener("click", () => {
      const nextTheme = document.documentElement.dataset.theme === "dark" ? "light" : "dark";
      if (nextTheme === "dark") {
        document.documentElement.dataset.theme = "dark";
      } else {
        delete document.documentElement.dataset.theme;
      }
      localStorage.setItem("vezeeta-theme", nextTheme);
    });
  }

  document.querySelectorAll(".table").forEach((table) => {
    if (!table.parentElement.classList.contains("table-responsive")) {
      const wrapper = document.createElement("div");
      wrapper.className = "table-responsive";
      table.parentNode.insertBefore(wrapper, table);
      wrapper.appendChild(table);
    }
  });
})();
