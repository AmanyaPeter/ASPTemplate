(() => {
  const body = document.body;
  document.querySelectorAll("[data-sidebar-open]").forEach(x =>
    x.addEventListener("click", () => body.classList.add("sidebar-open"))
  );
  document.querySelectorAll("[data-sidebar-close], .prototype-nav a").forEach(x =>
    x.addEventListener("click", () => body.classList.remove("sidebar-open"))
  );
  document.querySelectorAll("[data-alert-close]").forEach(x =>
    x.addEventListener("click", () => x.closest(".prototype-alert")?.remove())
  );
  document.querySelectorAll("[data-confirm]").forEach(x => x.addEventListener("click", event => {
    if (!window.confirm(x.dataset.confirm || "Are you sure?")) event.preventDefault();
  }));
  document.querySelectorAll("form[data-loading]").forEach(form =>
    form.addEventListener("submit", () => {
      const button = form.querySelector("button[type=submit]");
      if (button) {
        button.disabled = true;
        button.dataset.previousText = button.innerHTML;
        button.innerHTML = "Please wait…";
      }
    })
  );
  document.querySelectorAll("[data-lucide]").forEach(icon => {
    icon.classList.add("ti", `ti-${icon.dataset.lucide}`);
  });

  const notificationBadges = [...document.querySelectorAll("[data-notification-count]")];
  if (notificationBadges.length) {
    let unreadCount = Number.parseInt(notificationBadges[0].textContent || "0", 10) || 0;

    const showNotificationToast = () => {
      document.querySelector(".prototype-live-notification")?.remove();
      const toast = document.createElement("a");
      toast.className = "prototype-live-notification";
      toast.href = "/Notification";
      toast.setAttribute("role", "status");
      toast.innerHTML = '<i class="ti ti-bell-ringing"></i><span><strong>New notification</strong><small>Open your notification centre</small></span>';
      document.body.appendChild(toast);
      window.setTimeout(() => toast.remove(), 7000);
    };

    const updateNotificationCount = (nextCount, announce) => {
      notificationBadges.forEach(badge => {
        badge.textContent = String(nextCount);
        badge.classList.toggle("d-none", nextCount === 0);
      });
      document.querySelector(".prototype-notification-link")?.setAttribute(
        "aria-label",
        `Notifications, ${nextCount} unread`
      );
      if (announce && nextCount > unreadCount) showNotificationToast();
      unreadCount = nextCount;
    };

    const refreshNotificationCount = async () => {
      try {
        const response = await fetch("/Notification/UnreadCount", {
          cache: "no-store",
          headers: { "X-Requested-With": "XMLHttpRequest" }
        });
        if (!response.ok) return;
        const payload = await response.json();
        updateNotificationCount(Number(payload.count) || 0, true);
      } catch {
        // A later refresh will retry without interrupting the current page.
      }
    };

    window.setInterval(refreshNotificationCount, 15000);
    window.addEventListener("focus", refreshNotificationCount);
  }
})();
