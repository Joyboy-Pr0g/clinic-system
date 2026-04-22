(function () {
    console.log('appointment-chat-rich loaded');
    var panel = document.querySelector(".appointment-chat-panel");
    if (!panel) return;

    var apptId = panel.getAttribute("data-chat-appt");
    var userRole = panel.getAttribute("data-user-role");
    if (!apptId) return;

    var form = panel.querySelector("[data-chat-form]");
    var tokenInput = form && form.querySelector("input[name=\"__RequestVerificationToken\"]");
    var bodyInput = form && form.querySelector("textarea[name=\"body\"], input[name=\"body\"]");
    var mediaPicker = form && form.querySelector("[data-chat-media-picker]");
    var micBtn = form && form.querySelector("[data-chat-record-mic]");
    var recBar = form && form.querySelector("[data-recording-bar]");
    var recLabel = form && form.querySelector("[data-recording-label]");
    var voiceActions = form && form.querySelector("[data-voice-actions]");
    var voicePreview = form && form.querySelector("[data-voice-preview]");
    var voiceCancel = form && form.querySelector("[data-voice-cancel]");
    var voiceSend = form && form.querySelector("[data-voice-send]");
    var mediaPreview = form && form.querySelector("[data-media-preview]");
    var mediaSlot = form && form.querySelector("[data-media-preview-slot]");
    var mediaName = form && form.querySelector("[data-media-preview-name]");
    var mediaCancel = form && form.querySelector("[data-media-preview-cancel]");
    var mediaSend = form && form.querySelector("[data-media-preview-send]");

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

    function isMediaPreviewActive() {
        return mediaPreview && !mediaPreview.hidden;
    }

    var pendingMediaFile = null;
    var pendingMediaObjectUrl = null;

    function clearMediaPreview() {
        if (pendingMediaObjectUrl) {
            try { URL.revokeObjectURL(pendingMediaObjectUrl); } catch (e) { }
            pendingMediaObjectUrl = null;
        }
        pendingMediaFile = null;
        if (mediaSlot) mediaSlot.innerHTML = "";
        if (mediaName) {
            mediaName.textContent = "";
            mediaName.hidden = true;
        }
        if (mediaPreview) mediaPreview.hidden = true;
        if (mediaPicker) mediaPicker.value = "";
    }

    function showMediaPreview(file) {
        if (!file || !mediaPreview || !mediaSlot) return;
        if (mediaRecorder && mediaRecorder.state === "recording") {
            alert("أوقف التسجيل أولاً.");
            if (mediaPicker) mediaPicker.value = "";
            return;
        }
        if (voiceActions && !voiceActions.hidden) {
            alert("أرسل التسجيل الصوتي أو ارفضه أولاً.");
            if (mediaPicker) mediaPicker.value = "";
            return;
        }
        clearVoicePreview();
        clearMediaPreview();
        pendingMediaFile = file;
        var t = file.type || "";
        var n = file.name || "";
        if (t.indexOf("image/") === 0) {
            pendingMediaObjectUrl = URL.createObjectURL(file);
            var img = document.createElement("img");
            img.src = pendingMediaObjectUrl;
            img.alt = "";
            mediaSlot.appendChild(img);
        } else if (t.indexOf("video/") === 0) {
            pendingMediaObjectUrl = URL.createObjectURL(file);
            var vid = document.createElement("video");
            vid.src = pendingMediaObjectUrl;
            vid.controls = true;
            vid.playsInline = true;
            vid.muted = true;
            mediaSlot.appendChild(vid);
        } else if (t.indexOf("audio/") === 0) {
            pendingMediaObjectUrl = URL.createObjectURL(file);
            var aud = document.createElement("audio");
            aud.src = pendingMediaObjectUrl;
            aud.controls = true;
            mediaSlot.appendChild(aud);
        } else if (mediaName) {
            mediaName.textContent = n || "مرفق";
            mediaName.hidden = false;
        }
        mediaPreview.hidden = false;
        if (mediaPicker) mediaPicker.value = "";
    }

    function onFileChosen(input) {
        if (!input || !input.files || !input.files[0]) return;
        showMediaPreview(input.files[0]);
    }

    if (mediaPicker) mediaPicker.addEventListener("change", function () { onFileChosen(mediaPicker); });

    if (mediaCancel) {
        mediaCancel.addEventListener("click", function () {
            clearMediaPreview();
        });
    }

    if (mediaSend) {
        mediaSend.addEventListener("click", function () {
            if (!pendingMediaFile) return;
            var cap = bodyInput ? bodyInput.value : "";
            postMedia(pendingMediaFile, cap)
                .then(function () {
                    clearMediaPreview();
                    afterSend();
                })
                .catch(function (err) { alert(err.message || "تعذر الإرسال"); });
        });
    }

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

    /** إيقاف التسجيل وعرض معاينة (إرسال / رفض) */
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
            clearMediaPreview();
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
        console.log('chat: startRecording called');
        if (isMediaPreviewActive()) {
            alert("أرسل المرفق أو ارفضه أولاً.");
            return;
        }
        if (voiceActions && !voiceActions.hidden) {
            alert("أرسل التسجيل الحالي أو ارفضه أولاً.");
            return;
        }
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            alert("المتصفح لا يدعم تسجيل الصوت من هنا.");
            return;
        }
        if (!window.isSecureContext) {
            alert("تسجيل الصوت يحتاج اتصال آمن (HTTPS) أو تشغيل الموقع على localhost.");
            return;
        }
        if (!window.MediaRecorder) {
            alert("المتصفح لا يدعم تسجيل الصوت. جرب متصفح آخر مثل Chrome أو Firefox.");
            return;
        }
        clearVoicePreview();
        navigator.mediaDevices.getUserMedia({ audio: true }).then(function (stream) {
            console.log('chat: getUserMedia granted, creating MediaRecorder');
            chunks = [];
            var mime = "audio/webm";
            try {
                if (MediaRecorder.isTypeSupported && MediaRecorder.isTypeSupported("audio/webm;codecs=opus"))
                    mime = "audio/webm;codecs=opus";
                else if (MediaRecorder.isTypeSupported && MediaRecorder.isTypeSupported("audio/mp4"))
                    mime = "audio/mp4";
            } catch (e) { console.warn('chat: isTypeSupported check failed', e); }
            var mr;
            try {
                mr = new MediaRecorder(stream, { mimeType: mime });
            } catch (e) {
                console.warn('chat: MediaRecorder with mime failed, trying default', e);
                try { mr = new MediaRecorder(stream); } catch (err) {
                    console.error('chat: MediaRecorder not available', err);
                    stream.getTracks().forEach(function (t) { t.stop(); });
                    alert('تعذر بدء التسجيل — المتصفح لا يدعم MediaRecorder.');
                    return;
                }
            }
            mr._stopStream = function () { stream.getTracks().forEach(function (t) { t.stop(); }); };
            mr.ondataavailable = function (e) { if (e.data && e.data.size) { chunks.push(e.data); console.log('chat: chunk received', e.data.size); } };
            mr.onstart = function () { console.log('chat: recorder started'); };
            mr.onstop = function () { console.log('chat: recorder stopped'); };
            try { mr.start(200); } catch (e) {
                console.error('chat: recorder.start failed', e);
                stream.getTracks().forEach(function (t) { t.stop(); });
                alert('تعذر بدء التسجيل.');
                return;
            }
            mediaRecorder = mr;
            if (recBar) recBar.hidden = false;
            if (micBtn) {
                micBtn.classList.add("chat-ig-btn--recording");
                micBtn.setAttribute("aria-pressed", "true");
            }
            if (recLabel) recLabel.textContent = "جاري التسجيل… اضغط «صوت» مرة أخرى للإيقاف ثم اختر إرسال أو رفض";
            recordTimer = setTimeout(function () { if (mediaRecorder && mediaRecorder.state === "recording") stopRecordingForPreview(); }, maxMs);
        }).catch(function (err) {
            console.error('chat: getUserMedia error', err);
            alert("لم يُسمح بالوصول للميكروفون أو حدث خطأ: " + (err && err.message ? err.message : 'غير معروف'));
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

    /** Long-press on message field to start recording (tap still types normally). */
    var longPressTimer = null;
    var longPressMs = 550;
    if (bodyInput) {
        bodyInput.addEventListener("pointerdown", function () {
            if (longPressTimer) clearTimeout(longPressTimer);
            longPressTimer = setTimeout(function () {
                longPressTimer = null;
                if (voiceActions && !voiceActions.hidden) return;
                if (mediaRecorder && mediaRecorder.state === "recording") return;
                if (isMediaPreviewActive()) return;
                startRecording();
            }, longPressMs);
        });
        function cancelLongPress() {
            if (longPressTimer) {
                clearTimeout(longPressTimer);
                longPressTimer = null;
            }
        }
        bodyInput.addEventListener("pointerup", cancelLongPress);
        bodyInput.addEventListener("pointercancel", cancelLongPress);
        bodyInput.addEventListener("pointerleave", cancelLongPress);
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
                closeCameraModal();
                showMediaPreview(file);
            }, "image/jpeg", 0.88);
        });
    }

    // مشاركة الموقع
    var shareLocationBtn = form && form.querySelector("[data-share-location]");
    if (shareLocationBtn) {
        shareLocationBtn.addEventListener("click", function () {
            if (!navigator.geolocation) {
                alert("المتصفح لا يدعم مشاركة الموقع.");
                return;
            }
            if (isMediaPreviewActive()) {
                alert("أرسل المرفق أو ارفضه أولاً.");
                return;
            }
            if (voiceActions && !voiceActions.hidden) {
                alert("أرسل التسجيل الصوتي أو ارفضه أولاً.");
                return;
            }
            shareLocationBtn.disabled = true;
            shareLocationBtn.textContent = "جاري الحصول على الموقع…";
            navigator.geolocation.getCurrentPosition(
                function (pos) {
                    var lat = pos.coords.latitude;
                    var lng = pos.coords.longitude;
                    var body = "موقعي الحالي: https://www.google.com/maps/search/?api=1&query=" + lat + "," + lng;
                    var fd = new FormData();
                    fd.append("body", body);
                    fd.append("__RequestVerificationToken", tokenInput.value);
                    fetch(baseUrl, { method: "POST", body: fd, credentials: "same-origin" })
                        .then(function (r) {
                            if (!r.ok) return r.json().then(function (j) { throw new Error(j.error || "فشل الإرسال"); });
                            return r.json();
                        })
                        .then(function () {
                            afterSend();
                        })
                        .catch(function (err) { alert(err.message || "تعذر مشاركة الموقع"); })
                        .finally(function () {
                            shareLocationBtn.disabled = false;
                            shareLocationBtn.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path><circle cx="12" cy="10" r="3"></circle></svg>';
                        });
                },
                function (err) {
                    alert("تعذر الحصول على الموقع: " + (err.message || "غير معروف"));
                    shareLocationBtn.disabled = false;
                    shareLocationBtn.innerHTML = '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z"></path><circle cx="12" cy="10" r="3"></circle></svg>';
                },
                { enableHighAccuracy: false, maximumAge: 120000, timeout: 15000 }
            );
        });
    }
    // تتبع الموقع
    var trackLocationBtn = form && form.querySelector("[data-track-location]");
    if (trackLocationBtn) {
        trackLocationBtn.addEventListener("click", function () {
            var url;
            if (userRole === "Patient") {
                url = "/Patient/AppointmentMapsLink?id=" + apptId;
            } else if (userRole === "Nurse") {
                url = "/Nurse/AppointmentMapsLink?id=" + apptId;
            } else {
                alert("دور المستخدم غير معروف.");
                return;
            }
            fetch(url, { credentials: "same-origin", headers: { Accept: "application/json" } })
                .then(function (r) {
                    if (r.status === 404) throw new Error("غير موجود");
                    return r.json();
                })
                .then(function (j) {
                    if (j.url) {
                        window.open(j.url, "_blank", "noopener,noreferrer");
                    } else {
                        alert(j.error || "تعذر فتح الخرائط");
                    }
                })
                .catch(function () {
                    alert("تعذر الاتصال بالخادم.");
                });
        });
    }
})();
