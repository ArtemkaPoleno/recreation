(() => {
  const header = document.querySelector(".site-header");
  const toggle = document.querySelector("[data-nav-toggle]");
  if (!header || !toggle) return;

  const setOpen = (open) => {
    header.classList.toggle("is-open", open);
    toggle.setAttribute("aria-expanded", open ? "true" : "false");
  };

  toggle.addEventListener("click", () => {
    setOpen(!header.classList.contains("is-open"));
  });

  header.querySelectorAll("a").forEach((link) => {
    link.addEventListener("click", () => setOpen(false));
  });
})();
