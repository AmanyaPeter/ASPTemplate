(() => {
  const body = document.body;
  const iconShapes = {
    default: '<circle cx="12" cy="12" r="9"/><path d="M12 8v4m0 4h.01"/>',
    eye: '<path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6Z"/><circle cx="12" cy="12" r="2.5"/>',
    download: '<path d="M12 3v11m-4-4 4 4 4-4"/><path d="M5 18v2h14v-2"/>',
    upload: '<path d="M12 15V4m-4 4 4-4 4 4"/><path d="M5 18v2h14v-2"/>',
    file: '<path d="M6 3h8l4 4v14H6Z"/><path d="M14 3v5h5M9 12h6m-6 4h6"/>',
    paperclip: '<path d="m8 12 6.7-6.7a3 3 0 0 1 4.3 4.2l-8.5 8.6a5 5 0 0 1-7-7l8-8"/><path d="m7 14 7.5-7.5"/>',
    books: '<path d="M4 5h5v14H4zM9 7h5v12H9zM15 5l4-1 3 14-4 1z"/>',
    list: '<path d="M9 6h11M9 12h11M9 18h11"/><circle cx="4.5" cy="6" r=".8"/><circle cx="4.5" cy="12" r=".8"/><circle cx="4.5" cy="18" r=".8"/>',
    search: '<circle cx="10.5" cy="10.5" r="6.5"/><path d="m16 16 5 5"/>',
    filter: '<path d="M4 6h16M7 12h10M10 18h4"/>',
    trash: '<path d="M4 7h16M9 7V4h6v3m3 0-1 14H7L6 7M10 11v6m4-6v6"/>',
    edit: '<path d="M4 20h4l11-11-4-4L4 16v4Z"/><path d="m13.5 6.5 4 4"/>',
    plus: '<path d="M12 5v14M5 12h14"/>',
    close: '<path d="m6 6 12 12M18 6 6 18"/>',
    check: '<path d="m5 12 4 4L19 6"/>',
    alert: '<path d="M12 3 2.5 20h19L12 3Z"/><path d="M12 9v4m0 3h.01"/>',
    info: '<circle cx="12" cy="12" r="9"/><path d="M12 11v6m0-10h.01"/>',
    calendar: '<rect x="3" y="5" width="18" height="16" rx="2"/><path d="M7 3v4m10-4v4M3 10h18"/>',
    user: '<circle cx="12" cy="8" r="3.5"/><path d="M5 21a7 7 0 0 1 14 0"/>',
    users: '<circle cx="9" cy="8" r="3"/><path d="M3 20a6 6 0 0 1 12 0M16 6a3 3 0 0 1 0 5m1 3a5 5 0 0 1 4 5"/>',
    building: '<path d="M4 21V5l8-3 8 3v16M8 8h2m4 0h2M8 12h2m4 0h2M8 16h2m4 0h2M2 21h20"/>',
    database: '<ellipse cx="12" cy="5" rx="8" ry="3"/><path d="M4 5v6c0 1.7 3.6 3 8 3s8-1.3 8-3V5M4 11v6c0 1.7 3.6 3 8 3s8-1.3 8-3v-6"/>',
    category: '<rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="7" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/><circle cx="17.5" cy="17.5" r="3.5"/>',
    bell: '<path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4"/>',
    clock: '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',
    send: '<path d="m3 11 18-8-8 18-2-8-8-2Z"/><path d="m11 13 5-5"/>',
    save: '<path d="M5 3h12l3 3v15H4V3Z"/><path d="M8 3v6h8V3M8 21v-7h8v7"/>',
    arrowRight: '<path d="M5 12h14m-5-5 5 5-5 5"/>',
    arrowLeft: '<path d="M19 12H5m5-5-5 5 5 5"/>',
    arrowUpRight: '<path d="M7 17 17 7M8 7h9v9"/>',
    chevron: '<path d="m7 9 5 5 5-5"/>',
    inbox: '<path d="M4 4h16l2 11v5H2v-5L4 4Z"/><path d="M2 15h6l2 3h4l2-3h6"/>',
    help: '<circle cx="12" cy="12" r="9"/><path d="M9.5 9a2.6 2.6 0 1 1 4 2.2c-1 .7-1.5 1.1-1.5 2.3M12 17h.01"/>',
    lock: '<rect x="5" y="10" width="14" height="11" rx="2"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/>',
    key: '<circle cx="8" cy="15" r="4"/><path d="m11 12 8-8m-3 3 3 3"/>',
    shield: '<path d="M12 3 20 6v5c0 5-3.5 8.5-8 10-4.5-1.5-8-5-8-10V6l8-3Z"/>',
    history: '<path d="M3 12a9 9 0 1 0 3-6.7L3 8M3 4v4h4M12 7v5l3 2"/>',
    settings: '<circle cx="12" cy="12" r="3"/><path d="M12 2v3m0 14v3M2 12h3m14 0h3M5 5l2 2m10 10 2 2M19 5l-2 2M7 17l-2 2"/>',
    chart: '<path d="M4 20V10h4v10m2 0V4h4v16m2 0v-7h4v7M3 20h18"/>',
    bulb: '<path d="M9 17h6m-5 4h4"/><path d="M8.2 14.2A6 6 0 1 1 15.8 14.2c-.7.6-.8 1.2-.8 2.8H9c0-1.6-.1-2.2-.8-2.8Z"/>'
  };
  const iconAliases = {
    "view-360": "eye", "list-details": "list", list: "list", "layout-dashboard": "category",
    "layout-kanban": "category", "message-circle": "send", circle: "default",
    report: "chart", "chart-bar": "chart",
    "file-text": "file", "file-spreadsheet": "file", "file-type-pdf": "file", forms: "file",
    "book-open": "books", "books-off": "books", "cloud-upload": "upload",
    "category-plus": "plus", "user-plus": "plus", "adjustments-horizontal": "filter", adjustments: "filter",
    "circle-check": "check", checks: "check", "check-circle": "check", "user-check": "check", "shield-check": "check",
    "alert-circle": "alert", "alert-triangle": "alert", "clock-exclamation": "clock",
    "calendar-event": "calendar", "building-bank": "building", "folder-tree": "category",
    mail: "bell", "bell-off": "bell", "bell-ringing": "bell", "device-floppy": "save", folder: "save",
    "arrow-right": "arrowRight", "arrow-left": "arrowLeft", "arrow-up-right": "arrowUpRight",
    "chevron-down": "chevron", "info-circle": "info", "help-circle": "help", lifebuoy: "help",
    "lock-open": "lock", "user-off": "user", "users-off": "users", "user-search": "search",
    "category-off": "category", "history-off": "history", "file-search": "search",
    "circle-minus": "close", x: "close", "bulb-filled": "bulb", lightbulb: "bulb"
  };

  const renderAppIcons = (root = document) => {
    root.querySelectorAll("i[data-lucide], i.ti").forEach(icon => {
      if (icon.dataset.appIconRendered === "true") return;
      const iconClass = [...icon.classList].find(name => name.startsWith("ti-"));
      const requestedName = icon.dataset.lucide || iconClass?.slice(3) || "default";
      const shapeName = iconAliases[requestedName] || requestedName;
      const shape = iconShapes[shapeName] || iconShapes.default;
      icon.classList.remove("ti");
      if (iconClass) icon.classList.remove(iconClass);
      icon.classList.add("app-icon-host");
      icon.dataset.appIconRendered = "true";
      icon.innerHTML = `<svg class="app-icon" viewBox="0 0 24 24" aria-hidden="true" focusable="false">${shape}</svg>`;
    });
  };
  window.renderAppIcons = renderAppIcons;
  window.lucide = { createIcons: () => renderAppIcons() };
  renderAppIcons();

  document.querySelectorAll("[data-sidebar-open]").forEach(x =>
    x.addEventListener("click", () => body.classList.add("sidebar-open"))
  );
  document.querySelectorAll("[data-sidebar-close], .prototype-nav a").forEach(x =>
    x.addEventListener("click", () => body.classList.remove("sidebar-open"))
  );
  document.querySelectorAll("[data-alert-close]").forEach(x =>
    x.addEventListener("click", () => x.closest(".prototype-alert")?.remove())
  );
  document.querySelectorAll("[data-popup]").forEach(popup => {
    const closePopup = () => popup.remove();
    popup.querySelectorAll("[data-popup-close]").forEach(button =>
      button.addEventListener("click", closePopup)
    );
    popup.addEventListener("click", event => {
      if (event.target === popup) closePopup();
    });
    document.addEventListener("keydown", event => {
      if (event.key === "Escape" && popup.isConnected) closePopup();
    });
  });
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
      renderAppIcons(toast);
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
