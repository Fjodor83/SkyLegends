// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(() => {
  const navbarWrap = document.querySelector(".premium-navbar-wrap");
  if (!navbarWrap) {
    return;
  }

  const navbar = navbarWrap.querySelector(".premium-navbar");
  const burgerButton = navbarWrap.querySelector(".premium-burger");
  const mobilePanel = navbarWrap.querySelector(".premium-mobile-panel");
  const desktopMediaQuery = window.matchMedia("(min-width: 768px)");

  const setNavbarHeight = () => {
    const navbarHeight = Math.ceil(navbarWrap.getBoundingClientRect().height);
    document.documentElement.style.setProperty("--navbar-height", `${navbarHeight}px`);
  };

  const toggleScrolledState = () => {
    navbarWrap.classList.toggle("is-scrolled", window.scrollY > 8);
  };

  const closeMobileMenu = () => {
    if (!burgerButton || !mobilePanel) {
      return;
    }

    navbarWrap.classList.remove("menu-open");
    burgerButton.setAttribute("aria-expanded", "false");
    setNavbarHeight();
  };

  const toggleMobileMenu = () => {
    if (!burgerButton || !mobilePanel) {
      return;
    }

    const isOpening = !navbarWrap.classList.contains("menu-open");
    navbarWrap.classList.toggle("menu-open", isOpening);
    burgerButton.setAttribute("aria-expanded", String(isOpening));
    setNavbarHeight();
  };

  if (navbar && burgerButton && mobilePanel) {
    burgerButton.addEventListener("click", (event) => {
      event.stopPropagation();
      toggleMobileMenu();
    });

    document.addEventListener("click", (event) => {
      if (!navbarWrap.classList.contains("menu-open")) {
        return;
      }

      const target = event.target;
      if (!(target instanceof Node)) {
        return;
      }

      if (!navbar.contains(target)) {
        closeMobileMenu();
      }
    });

    mobilePanel.addEventListener("click", (event) => {
      const target = event.target;
      if (!(target instanceof Element)) {
        return;
      }

      if (target.closest("a, button")) {
        closeMobileMenu();
      }
    });

    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        closeMobileMenu();
      }
    });

    const onDesktopChange = (event) => {
      if (event.matches) {
        closeMobileMenu();
      }
    };

    if (typeof desktopMediaQuery.addEventListener === "function") {
      desktopMediaQuery.addEventListener("change", onDesktopChange);
    } else if (typeof desktopMediaQuery.addListener === "function") {
      desktopMediaQuery.addListener(onDesktopChange);
    }
  }

  setNavbarHeight();
  toggleScrolledState();

  window.addEventListener("load", setNavbarHeight);
  window.addEventListener("resize", () => {
    if (desktopMediaQuery.matches) {
      closeMobileMenu();
    }

    setNavbarHeight();
  });
  window.addEventListener("scroll", toggleScrolledState, { passive: true });

  if (typeof ResizeObserver !== "undefined") {
    const observer = new ResizeObserver(setNavbarHeight);
    observer.observe(navbarWrap);
  }
})();

// Login Modal Logic
(() => {
  const loginModal = document.getElementById("login-modal");
  const loginTrigger = document.getElementById("login-modal-trigger");
  const loginClose = document.getElementById("login-modal-close");
  const loginOverlay = document.getElementById("login-modal-overlay");

  if (!loginModal || !loginTrigger) return;

  const openModal = () => {
    loginModal.classList.remove("hidden");
    // Focus email input if exists
    const emailInput = loginModal.querySelector("input[name='Input.Email']");
    if (emailInput) setTimeout(() => emailInput.focus(), 100);
    document.body.style.overflow = "hidden"; // Prevent scrolling
  };

  const closeModal = () => {
    loginModal.classList.add("hidden");
    document.body.style.overflow = ""; // Restore scrolling
  };

  loginTrigger.addEventListener("click", (e) => {
    e.preventDefault();
    openModal();
  });

  if (loginClose) {
    loginClose.addEventListener("click", closeModal);
  }

  if (loginOverlay) {
    loginOverlay.addEventListener("click", closeModal);
  }

  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape" && !loginModal.classList.contains("hidden")) {
      closeModal();
    }
  });
})();

// Cart Badge Updater
(() => {
  const badges = document.querySelectorAll(".cart-badge-sync");
  if (badges.length === 0) return;

  const updateBadge = async () => {
    try {
      const res = await fetch("/Cart/Count");
      const data = await res.json();
      badges.forEach(badge => {
        if (data.count > 0) {
          badge.textContent = data.count;
          badge.style.display = "flex";
        } else {
          badge.style.display = "none";
        }
      });
    } catch (e) {
      // silently fail
    }
  };

  updateBadge();
})();
