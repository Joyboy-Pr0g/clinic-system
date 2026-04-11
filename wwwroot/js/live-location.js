(function () {
    var url = window.__saveLiveLocationUrl;
    if (!url || !navigator.geolocation) return;

    var key = "homecareLiveLocV1";
    try {
        if (sessionStorage.getItem(key)) return;
    } catch (e) { /* private mode */ }

    var form = document.getElementById("live-loc-antiforgery");
    var tokenInput = form && form.querySelector("input[name=\"__RequestVerificationToken\"]");
    if (!tokenInput) return;

    navigator.geolocation.getCurrentPosition(
        function (pos) {
            var fd = new FormData();
            fd.append("lat", String(pos.coords.latitude));
            fd.append("lng", String(pos.coords.longitude));
            fd.append("__RequestVerificationToken", tokenInput.value);
            fetch(url, { method: "POST", body: fd, credentials: "same-origin" })
                .then(function (r) {
                    if (r.ok) {
                        try { sessionStorage.setItem(key, "1"); } catch (e) { }
                    }
                })
                .catch(function () { });
        },
        function () { },
        { enableHighAccuracy: false, maximumAge: 120000, timeout: 15000 }
    );
})();
