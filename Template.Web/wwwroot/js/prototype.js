(() => {
  const body = document.body;
  document.querySelectorAll("[data-sidebar-open]").forEach(x => x.addEventListener("click", () => body.classList.add("sidebar-open")));
  document.querySelectorAll("[data-sidebar-close], .prototype-nav a").forEach(x => x.addEventListener("click", () => body.classList.remove("sidebar-open")));
  document.querySelectorAll("[data-alert-close]").forEach(x => x.addEventListener("click", () => x.closest(".prototype-alert")?.remove()));
  document.querySelectorAll("[data-confirm]").forEach(x => x.addEventListener("click", e => {
    if (!window.confirm(x.dataset.confirm || "Are you sure?")) e.preventDefault();
  }));
  document.querySelectorAll("form[data-loading]").forEach(form => form.addEventListener("submit", () => {
    const button = form.querySelector("button[type=submit]");
    if (button) { button.disabled = true; button.dataset.previousText = button.innerHTML; button.innerHTML = "Please wait…"; }
  }));
  document.querySelectorAll("[data-lucide]").forEach(icon => {
    icon.classList.add("ti", `ti-${icon.dataset.lucide}`);
  });
})();
