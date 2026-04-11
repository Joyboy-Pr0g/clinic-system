(function () {
    document.querySelectorAll("[data-maps-link-fetch]").forEach(function (btn) {
        btn.addEventListener("click", function () {
            var url = btn.getAttribute("data-maps-link-fetch");
            if (!url) return;
            var idle = btn.querySelector(".btn-maps-track__text");
            var prev = idle ? idle.textContent : "";
            btn.disabled = true;
            if (idle) idle.textContent = "جاري الجلب…";
            fetch(url, { credentials: "same-origin", headers: { Accept: "application/json" } })
                .then(function (r) {
                    if (r.status === 404) throw new Error("غير موجود");
                    return r.json();
                })
                .then(function (j) {
                    btn.disabled = false;
                    if (idle) idle.textContent = prev;
                    if (j.url) {
                        window.open(j.url, "_blank", "noopener,noreferrer");
                    } else {
                        alert(j.error || "تعذر فتح الخرائط");
                    }
                })
                .catch(function () {
                    btn.disabled = false;
                    if (idle) idle.textContent = prev;
                    alert("تعذر الاتصال بالخادم.");
                });
        });
    });
})();
