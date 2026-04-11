(function () {
    var panel = document.querySelector(".appointment-chat-panel");
    if (!panel) return;

    var apptId = panel.getAttribute("data-chat-appt");
    if (!apptId) return;

    var form = panel.querySelector("[data-chat-form]");
    var tokenInput = form && form.querySelector("input[name=\"__RequestVerificationToken\"]");
    var bodyInput = form && form.querySelector("input[name=\"body\"]");
    var mediaPicker = form && form.querySelector("[data-chat-media-picker]");
    var micBtn = form && form.querySelector("[data-chat-record-mic]");
    var recBar = form && form.querySelector("[data-recording-bar]");
    var recLabel = form && form.querySelector("[data-recording-label]");
    var voiceActions = form && form.querySelector("[data-voice-actions]");
    var voicePreview = form && form.querySelector("[data-voice-preview]");
    var voiceCancel = form && form.querySelector("[data-voice-cancel]");
    var voiceSend = form && form.querySelector("[data-voice-send]");

    if (!form || !tokenInput) return;

    var baseUrl = "/appointment-chat/" + apptId + "/messages";

    function postMedia(file, caption) {
        var fd = new FormData();
        fd.append("body", (caption || "").trim());
        fd.append("file", file);
        fd.append("__RequestVerificationToken", tokenInput.value);
        return fetch(baseUrl, { method: "POST", body: fd, credentials: "same-origin" }).then(function (r) {
            if (!r.ok) return r.json().then(function (j) { throw new Error(j.error || "فشل الإرسال"); });
            return r.json();
        });
    }

    function afterSend() {
        window.dispatchEvent(new CustomEvent("appt-chat-reload"));
        if (bodyInput) bodyInput.value = "";
        if (mediaPicker) mediaPicker.value = "";
        form.querySelectorAll("input[type=\"file\"]").forEach(function (el) { el.value = ""; });
    }

    function onFileChosen(input) {
        if (!input || !input.files || !input.files[0]) return;
        var f = input.files[0];
        var cap = bodyInput ? bodyInput.value : "";
        postMedia(f, cap)
            .then(function () { afterSend(); })
            .catch(function (e) { alert(e.message || "تعذر الإرسال"); input.value = ""; });
    }

    if (mediaPicker) mediaPicker.addEventListener("change", function () { onFileChosen(mediaPicker); });

    var mediaRecorder = null;
    var chunks = [];
    var recordTimer = null;
    var maxMs = 90000;
    var pendingVoiceFile = null;
    var pendingVoiceObjectUrl = null;

    function clearVoicePreview() {
        if (pendingVoiceObjectUrl) {
            try { URL.revokeObjectURL(pendingVoiceObjectUrl); } catch (e) { }
            pendingVoiceObjectUrl = null;
        }
        pendingVoiceFile = null;
        if (voicePreview) {
            voicePreview.removeAttribute("src");
            try { voicePreview.load(); } catch (e) { }
        }
        if (voiceActions) voiceActions.hidden = true;
    }

    /** إيقاف التسجيل وعرض معاينة (إرسال / إلغاء) */
    function stopRecordingForPreview() {
        if (recordTimer) {
            clearTimeout(recordTimer);
            recordTimer = null;
        }
        if (recBar) recBar.hidden = true;
        if (micBtn) {
            micBtn.classList.remove("chat-ig-btn--recording");
            micBtn.setAttribute("aria-pressed", "false");
        }
        var mr = mediaRecorder;
        mediaRecorder = null;
        if (!mr || mr.state === "inactive") {
            chunks = [];
            return;
        }
        var stopStream = mr._stopStream;
        mr.onstop = function () {
            if (typeof stopStream === "function") stopStream();
            var blob = new Blob(chunks, { type: chunks[0] ? chunks[0].type : "audio/webm" });
            chunks = [];
            if (blob.size < 500) {
                alert("التسجيل قصير جداً.");
                return;
            }
            var ext = blob.type.indexOf("mp4") >= 0 || blob.type.indexOf("m4a") >= 0 ? "m4a" : "webm";
            if (pendingVoiceObjectUrl) {
                try { URL.revokeObjectURL(pendingVoiceObjectUrl); } catch (e) { }
                pendingVoiceObjectUrl = null;
            }
            pendingVoiceFile = new File([blob], "voice-" + Date.now() + "." + ext, { type: blob.type || "audio/webm" });
            pendingVoiceObjectUrl = URL.createObjectURL(blob);
            if (voicePreview) voicePreview.src = pendingVoiceObjectUrl;
            if (voiceActions) voiceActions.hidden = false;
        };
        try {
            mr.stop();
        } catch (e) {
            chunks = [];
            if (typeof stopStream === "function") stopStream();
        }
    }

    function startRecording() {
        if (voiceActions && !voiceActions.hidden) {
            alert("أرسل التسجيل الحالي أو ألغِه أولاً.");
            return;
        }
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            alert("المتصفح لا يدعم تسجيل الصوت من هنا.");
            return;
        }
        clearVoicePreview();
        navigator.mediaDevices.getUserMedia({ audio: true }).then(function (stream) {
            chunks = [];
            var mime = "audio/webm";
            if (window.MediaRecorder && MediaRecorder.isTypeSupported && MediaRecorder.isTypeSupported("audio/webm;codecs=opus"))
                mime = "audio/webm;codecs=opus";
            else if (window.MediaRecorder && MediaRecorder.isTypeSupported && MediaRecorder.isTypeSupported("audio/mp4"))
                mime = "audio/mp4";
            var mr;
            try {
                mr = new MediaRecorder(stream, { mimeType: mime });
            } catch (e) {
                mr = new MediaRecorder(stream);
            }
            mr._stopStream = function () {
                stream.getTracks().forEach(function (t) { t.stop(); });
            };
            mr.ondataavailable = function (e) {
                if (e.data && e.data.size) chunks.push(e.data);
            };
            mr.start(200);
            mediaRecorder = mr;
            if (recBar) recBar.hidden = false;
            if (micBtn) {
                micBtn.classList.add("chat-ig-btn--recording");
                micBtn.setAttribute("aria-pressed", "true");
            }
            if (recLabel) recLabel.textContent = "جاري التسجيل… اضغط «صوت» مرة أخرى للإيقاف ثم اختر إرسال أو إلغاء";
            recordTimer = setTimeout(function () {
                if (mediaRecorder && mediaRecorder.state === "recording") stopRecordingForPreview();
            }, maxMs);
        }).catch(function () {
            alert("لم يُسمح بالوصول للميكروفون.");
        });
    }

    if (micBtn) {
        micBtn.addEventListener("click", function (e) {
            e.preventDefault();
            if (mediaRecorder && mediaRecorder.state === "recording") {
                stopRecordingForPreview();
                return;
            }
            startRecording();
        });
    }

    if (voiceCancel) {
        voiceCancel.addEventListener("click", function () {
            clearVoicePreview();
        });
    }

    if (voiceSend) {
        voiceSend.addEventListener("click", function () {
            if (!pendingVoiceFile) return;
            var cap = bodyInput ? bodyInput.value : "";
            postMedia(pendingVoiceFile, cap)
                .then(function () {
                    clearVoicePreview();
                    afterSend();
                })
                .catch(function (err) { alert(err.message || "تعذر إرسال التسجيل"); });
        });
    }

    /* كاميرا مباشرة داخل الصفحة (معاينة + التقاط) */
    var modal = panel.querySelector("[data-chat-camera-modal]");
    var openLiveBtn = form.querySelector("[data-chat-open-live-camera]");
    var videoEl = modal && modal.querySelector("[data-chat-camera-video]");
    var canvasEl = modal && modal.querySelector("[data-chat-camera-canvas]");
    var snapBtn = modal && modal.querySelector("[data-chat-camera-snap]");
    var liveStream = null;

    function stopLiveStream() {
        if (liveStream) {
            liveStream.getTracks().forEach(function (t) { t.stop(); });
            liveStream = null;
        }
        if (videoEl) videoEl.srcObject = null;
    }

    function closeCameraModal() {
        if (!modal) return;
        stopLiveStream();
        modal.hidden = true;
    }

    function openCameraModal() {
        if (!modal || !videoEl || !navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            alert("المتصفح لا يدعم فتح الكاميرا من هنا.");
            return;
        }
        var constraints = { video: { facingMode: { ideal: "environment" }, width: { ideal: 1280 }, height: { ideal: 720 } }, audio: false };
        navigator.mediaDevices.getUserMedia(constraints).then(function (stream) {
            liveStream = stream;
            videoEl.srcObject = stream;
            modal.hidden = false;
        }).catch(function () {
            navigator.mediaDevices.getUserMedia({ video: true, audio: false }).then(function (stream) {
                liveStream = stream;
                videoEl.srcObject = stream;
                modal.hidden = false;
            }).catch(function () {
                alert("تعذر فتح الكاميرا. تحقق من الأذونات.");
            });
        });
    }

    if (openLiveBtn) openLiveBtn.addEventListener("click", function () { openCameraModal(); });
    if (modal) {
        modal.querySelectorAll("[data-chat-camera-close]").forEach(function (el) {
            el.addEventListener("click", closeCameraModal);
        });
    }

    if (snapBtn && canvasEl && videoEl) {
        snapBtn.addEventListener("click", function () {
            if (!liveStream || !videoEl.videoWidth) {
                alert("انتظر ظهور الصورة من الكاميرا.");
                return;
            }
            var w = videoEl.videoWidth;
            var h = videoEl.videoHeight;
            canvasEl.width = w;
            canvasEl.height = h;
            var ctx = canvasEl.getContext("2d");
            if (!ctx) return;
            ctx.drawImage(videoEl, 0, 0, w, h);
            canvasEl.toBlob(function (blob) {
                if (!blob) {
                    alert("تعذر إنشاء الصورة.");
                    return;
                }
                var file = new File([blob], "camera-" + Date.now() + ".jpg", { type: "image/jpeg" });
                var cap = bodyInput ? bodyInput.value : "";
                postMedia(file, cap)
                    .then(function () {
                        closeCameraModal();
                        afterSend();
                    })
                    .catch(function (e) { alert(e.message || "تعذر الإرسال"); });
            }, "image/jpeg", 0.88);
        });
    }
})();
