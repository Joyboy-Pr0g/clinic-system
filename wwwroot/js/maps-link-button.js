(function () {
    document.querySelectorAll("[data-maps-link-fetch]").forEach(function (btn) {
        btn.addEventListener("click", function () {
            var url = btn.getAttribute("data-maps-link-fetch");
            if (!url) return;
            var idle = btn.querySelector(".btn-maps-track__text");
            var prev = idle ? idle.textContent : "";
            btn.disabled = true;
            if (idle) idle.textContent = "جاري تحديث موقعك…";

            // أرسل موقع المريض أولاً
            sendPatientLocation().then(function () {
                // ثم اجلب رابط الخرائط
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
            }).catch(function () {
                // إذا فشل إرسال الموقع، استمر في جلب الرابط
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
    });

    function sendPatientLocation() {
        return new Promise(function (resolve, reject) {
            if (!navigator.geolocation) {
                reject(new Error("Geolocation not supported"));
                return;
            }
            var form = document.getElementById("live-loc-antiforgery");
            var tokenInput = form && form.querySelector("input[name=\"__RequestVerificationToken\"]");
            if (!tokenInput) {
                reject(new Error("No token"));
                return;
            }
            navigator.geolocation.getCurrentPosition(
                function (pos) {
                    var fd = new FormData();
                    fd.append("lat", String(pos.coords.latitude));
                    fd.append("lng", String(pos.coords.longitude));
                    fd.append("__RequestVerificationToken", tokenInput.value);
                    fetch(window.__saveLiveLocationUrl, { method: "POST", body: fd, credentials: "same-origin" })
                        .then(function (r) {
                            if (r.ok) {
                                resolve();
                            } else {
                                reject(new Error("Failed to send location"));
                            }
                        })
                        .catch(reject);
                },
                reject,
                { enableHighAccuracy: false, maximumAge: 120000, timeout: 15000 }
            );
        });
    }
})();
