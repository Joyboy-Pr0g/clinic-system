(function () {
  document.querySelectorAll("[data-nav-toggle]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      var nav = document.querySelector("[data-main-nav]");
      if (nav) nav.classList.toggle("is-open");
    });
  });

  document.querySelectorAll("[data-dropdown-toggle]").forEach(function (btn) {
    btn.addEventListener("click", function (e) {
      e.stopPropagation();
      var wrap = btn.closest(".nav-user");
      var menu = wrap && wrap.querySelector("[data-dropdown]");
      if (menu) menu.classList.toggle("is-open");
    });
  });
  document.addEventListener("click", function () {
    document.querySelectorAll("[data-dropdown].is-open").forEach(function (m) {
      m.classList.remove("is-open");
    });
  });

  document.querySelectorAll("[data-sidebar-open]").forEach(function (b) {
    b.addEventListener("click", function () {
      var s = document.querySelector("[data-dash-sidebar]");
      if (s) s.classList.add("is-open");
    });
  });
  document.querySelectorAll("[data-sidebar-close]").forEach(function (b) {
    b.addEventListener("click", function () {
      var s = document.querySelector("[data-dash-sidebar]");
      if (s) s.classList.remove("is-open");
    });
  });

  window.AppToast = function (message, type) {
    var root = document.getElementById("toast-root");
    if (!root) return;
    var el = document.createElement("div");
    el.className = "toast " + (type || "success");
    el.textContent = message;
    root.appendChild(el);
    setTimeout(function () {
      el.remove();
    }, 4500);
  };

  var tempSuccess = document.body.dataset.tempSuccess;
  var tempError = document.body.dataset.tempError;
  if (tempSuccess) AppToast(tempSuccess, "success");
  if (tempError) AppToast(tempError, "error");

  document.querySelectorAll("form[data-loading]").forEach(function (form) {
    form.addEventListener("submit", function () {
      var isValid = true;

      // Respect unobtrusive/jQuery validation when available.
      if (window.jQuery && window.jQuery.validator) {
        isValid = window.jQuery(form).valid();
      } else if (typeof form.checkValidity === "function") {
        isValid = form.checkValidity();
      }

      if (!isValid) return;

      var btn = form.querySelector('button[type="submit"]');
      if (btn) {
        btn.disabled = true;
        btn.dataset.html = btn.innerHTML;
        btn.innerHTML = '<span class="spinner"></span> جاري الإرسال...';
      }
    });
  });

  window.confirmModal = function (message, onConfirm) {
    var backdrop = document.getElementById("global-modal");
    if (!backdrop) {
      backdrop = document.createElement("div");
      backdrop.id = "global-modal";
      backdrop.className = "modal-backdrop";
      backdrop.innerHTML =
        '<div class="modal-box"><p class="modal-msg"></p><div class="modal-actions">' +
        '<button type="button" class="btn btn-primary" data-ok>تأكيد</button>' +
        '<button type="button" class="btn btn-ghost" data-cancel>إلغاء</button></div></div>';
      document.body.appendChild(backdrop);
    }
    backdrop.querySelector(".modal-msg").textContent = message;
    backdrop.classList.add("is-open");
    var ok = backdrop.querySelector("[data-ok]");
    var cancel = backdrop.querySelector("[data-cancel]");
    function close() {
      backdrop.classList.remove("is-open");
      ok.replaceWith(ok.cloneNode(true));
      cancel.replaceWith(cancel.cloneNode(true));
    }
    ok.addEventListener("click", function () {
      close();
      if (onConfirm) onConfirm();
    });
    cancel.addEventListener("click", close);
    backdrop.addEventListener("click", function (e) {
      if (e.target === backdrop) close();
    });
  };

  /** Full-width hero slider: [data-hero-slider] with .hero-slider__track and .hero-slide */
  document.querySelectorAll("[data-hero-slider]").forEach(function (root) {
    var track = root.querySelector(".hero-slider__track");
    var slides = root.querySelectorAll(".hero-slide");
    var dots = root.querySelectorAll(".hero-slider__dot");
    var prev = root.querySelector("[data-hero-prev]");
    var next = root.querySelector("[data-hero-next]");
    if (!track || slides.length === 0) return;

    var i = 0;
    var n = slides.length;
    var autoplayMs = parseInt(root.getAttribute("data-autoplay") || "6000", 10);
    var timer = null;

    function go(idx) {
      i = ((idx % n) + n) % n;
      track.style.transform = "translateX(" + -i * 100 + "%)";
      dots.forEach(function (d, di) {
        d.classList.toggle("is-active", di === i);
      });
    }

    function schedule() {
      if (timer) clearInterval(timer);
      if (autoplayMs > 0) timer = setInterval(function () { go(i + 1); }, autoplayMs);
    }

    if (prev) prev.addEventListener("click", function () { go(i - 1); schedule(); });
    if (next) next.addEventListener("click", function () { go(i + 1); schedule(); });
    dots.forEach(function (d, di) {
      d.addEventListener("click", function () { go(di); schedule(); });
    });

    go(0);
    schedule();

    document.addEventListener("visibilitychange", function () {
      if (document.hidden) {
        if (timer) clearInterval(timer);
      } else schedule();
    });
  });
})();
