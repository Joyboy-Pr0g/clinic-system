(function () {
    var root = document.getElementById("appt-realtime-root");
    if (!root) return;

    var apptId = parseInt(root.dataset.apptId || "0", 10);
    var userId = root.dataset.userId || "";
    if (!apptId || !userId) return;

    var hasSignalR = typeof signalR !== "undefined";
    var patientPush = root.dataset.patientPush === "true";
    var nursePush = root.dataset.nursePush === "true";
    var listenNurse = root.dataset.listenNurse === "true";
    var listenPatient = root.dataset.listenPatient === "true";

    var connection = hasSignalR
        ? new signalR.HubConnectionBuilder()
            .withUrl("/hubs/appointment")
            .withAutomaticReconnect()
            .build()
        : null;

    var seenChat = new Set();

    function appendChatRow(logEl, m, me) {
        var mine = m.isMine === true || m.senderUserId === me;
        var row = document.createElement("div");
        row.className = "appointment-chat-msg" + (mine ? " appointment-chat-msg--mine" : "");
        var who = document.createElement("div");
        who.className = "appointment-chat-msg__who";
        who.textContent = mine ? "أنت" : (m.senderName || "");
        row.appendChild(who);

        var type = (m.messageType || "text").toLowerCase();
        var url = m.attachmentUrl || "";
        function fileNameFromUrl(u) {
            try {
                var clean = (u || "").split("?")[0].split("#")[0];
                var parts = clean.split("/");
                return decodeURIComponent(parts[parts.length - 1] || "attachment");
            } catch (e) {
                return "attachment";
            }
        }

        if (type === "image" && url) {
            var img = document.createElement("img");
            img.className = "appointment-chat-msg__attach-img";
            img.src = url;
            img.alt = "";
            img.loading = "lazy";
            row.appendChild(img);
        } else if (type === "video" && url) {
            var vid = document.createElement("video");
            vid.className = "appointment-chat-msg__attach-video";
            vid.controls = true;
            vid.preload = "metadata";
            vid.playsInline = true;
            vid.src = url;
            row.appendChild(vid);
        } else if (type === "audio" && url) {
            var aud = document.createElement("audio");
            aud.className = "appointment-chat-msg__attach-audio";
            aud.controls = true;
            aud.preload = "metadata";
            aud.src = url;
            row.appendChild(aud);
        } else if (type === "file" && url) {
            var link = document.createElement("a");
            link.href = url;
            link.target = "_blank";
            link.rel = "noopener noreferrer";
            link.className = "appointment-chat-msg__attach-file";
            link.textContent = "📎 " + fileNameFromUrl(url);
            row.appendChild(link);
        }

        if (m.body) {
            var body = document.createElement("div");
            body.className = "appointment-chat-msg__body";
            body.textContent = m.body;
            row.appendChild(body);
        }

        logEl.appendChild(row);
        logEl.scrollTop = logEl.scrollHeight;
    }

    function wireChat() {
        var panel = document.querySelector(".appointment-chat-panel");
        if (!panel) return;
        var logEl = panel.querySelector("[data-chat-log]");
        var form = panel.querySelector("[data-chat-form]");
        var input = form && form.querySelector("input[name=\"body\"], textarea[name=\"body\"]");
        var fileInput = form && form.querySelector("[data-chat-file], [data-chat-media-picker]");
        var tokenInput = form && form.querySelector("input[name=\"__RequestVerificationToken\"]");
        if (!logEl || !form || !input || !tokenInput) return;

        window.addEventListener("appt-chat-reload", function () {
            loadHistory();
        });

        var baseUrl = "/appointment-chat/" + apptId + "/messages";

        function loadHistory() {
            return fetch(baseUrl, { credentials: "same-origin" })
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    (data.messages || []).forEach(function (m) {
                        if (seenChat.has(m.appointmentMessageId)) return;
                        seenChat.add(m.appointmentMessageId);
                        appendChatRow(logEl, m, userId);
                    });
                })
                .catch(function () { });
        }

        if (connection) {
            connection.on("ReceiveChatMessage", function (m) {
                if (!m || seenChat.has(m.appointmentMessageId)) return;
                seenChat.add(m.appointmentMessageId);
                appendChatRow(logEl, m, userId);
            });
        }

        form.addEventListener("submit", function (e) {
            e.preventDefault();
            var text = (input.value || "").trim();
            var file = fileInput && fileInput.files && fileInput.files[0];
            var pendingVoice = form.querySelector("[data-voice-actions]");
            if (pendingVoice && !pendingVoice.hidden) {
                alert("أرسل التسجيل الصوتي أو ارفضه من شريط المعاينة أولاً.");
                return;
            }
            var pendingMedia = form.querySelector("[data-media-preview]");
            if (pendingMedia && !pendingMedia.hidden) {
                alert("أرسل المرفق أو ارفضه من شريط المعاينة أولاً.");
                return;
            }
            if (!text && !file) {
                alert("اكتب رسالة أو استخدم الكاميرا / المعرض / الميكروفون أسفل الصندوق.");
                return;
            }
            var fd = new FormData();
            fd.append("body", text);
            if (file) fd.append("file", file);
            fd.append("__RequestVerificationToken", tokenInput.value);
            fetch(baseUrl, { method: "POST", body: fd, credentials: "same-origin" })
                .then(function (r) {
                    if (!r.ok) return r.json().then(function (j) { throw new Error(j.error || "فشل الإرسال"); });
                    return r.json();
                })
                .then(function () {
                    input.value = "";
                    if (fileInput) fileInput.value = "";
                    form.querySelectorAll("input[type=\"file\"]").forEach(function (el) { el.value = ""; });
                    return loadHistory();
                })
                .catch(function (err) { alert(err.message || "تعذر إرسال الرسالة"); });
        });

        loadHistory();
    }

    if (connection) {
        connection.on("NurseLocationUpdated", function (lat, lng) {
            if (!listenNurse) return;
            window.dispatchEvent(new CustomEvent("appt:nurse-loc", { detail: { lat: lat, lng: lng } }));
        });

        connection.on("PatientLocationUpdated", function (lat, lng) {
            if (!listenPatient) return;
            window.dispatchEvent(new CustomEvent("appt:patient-loc", { detail: { lat: lat, lng: lng } }));
        });
    }

    function wirePush() {
        if (!connection) return;
        var timerN = null;
        var timerP = null;
        function startN(cb) {
            if (timerN) clearInterval(timerN);
            timerN = setInterval(cb, 55000);
            cb();
        }
        function stopN() {
            if (timerN) { clearInterval(timerN); timerN = null; }
        }
        function startP(cb) {
            if (timerP) clearInterval(timerP);
            timerP = setInterval(cb, 55000);
            cb();
        }
        function stopP() {
            if (timerP) { clearInterval(timerP); timerP = null; }
        }

        if (nursePush) {
            var cbN = document.getElementById("shareLiveNurse");
            if (cbN) {
                cbN.addEventListener("change", function () {
                    if (!cbN.checked) { stopN(); return; }
                    if (!navigator.geolocation) return;
                    startN(function () {
                        navigator.geolocation.getCurrentPosition(function (pos) {
                            connection.invoke("PushNurseLocation", apptId, pos.coords.latitude, pos.coords.longitude).catch(function () { });
                        }, function () { }, { enableHighAccuracy: true, maximumAge: 30000, timeout: 15000 });
                    });
                });
            }
        }

        if (patientPush) {
            var cbP = document.getElementById("shareLivePatient");
            if (cbP) {
                cbP.addEventListener("change", function () {
                    if (!cbP.checked) { stopP(); return; }
                    if (!navigator.geolocation) return;
                    startP(function () {
                        navigator.geolocation.getCurrentPosition(function (pos) {
                            connection.invoke("PushPatientLocation", apptId, pos.coords.latitude, pos.coords.longitude).catch(function () { });
                        }, function () { }, { enableHighAccuracy: true, maximumAge: 30000, timeout: 15000 });
                    });
                });
            }
        }
    }

    wireChat();

    if (connection) {
        connection.start()
            .then(function () { return connection.invoke("JoinAppointment", apptId); })
            .then(function () { wirePush(); })
            .catch(function () { /* الدردشة تعمل بالـ HTTP حتى لو فشل الـ Hub */ });
    }
})();
