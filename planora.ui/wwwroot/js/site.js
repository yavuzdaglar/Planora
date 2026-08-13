// Planora site.js
document.addEventListener('DOMContentLoaded', function () {
    initThemeToggle();
    initQuickCommand();
    initRailToggle();
    initAiToggle();
    initBlockBuilder();
    initDraftDnD();
});

/* ---------- Toast bildirimleri (üstten kuyruk) ---------- */
window.showToast = function (message, type) {
    var wrap = document.getElementById('toastWrap');
    if (!wrap) {
        wrap = document.createElement('div');
        wrap.id = 'toastWrap';
        wrap.className = 'toast-wrap';
        document.body.appendChild(wrap);
    }
    var t = document.createElement('div');
    t.className = 'toast ' + (type || 'info');
    t.textContent = message;
    wrap.appendChild(t);
    setTimeout(function () {
        t.classList.add('out');
        setTimeout(function () { t.remove(); }, 300);
    }, 3500);
};

/* ---------- Üstten popup onayı (tarayıcı confirm yerine) ---------- */
window.confirmAction = function (message, onOk) {
    var wrap = document.getElementById('confirmWrap');
    if (!wrap) {
        wrap = document.createElement('div');
        wrap.id = 'confirmWrap';
        wrap.className = 'confirm-wrap';
        document.body.appendChild(wrap);
    }
    wrap.innerHTML =
        '<div class="confirm-pop"><p>' + message + '</p>' +
        '<div class="confirm-actions">' +
        '<button type="button" class="btn btn-outline" data-ok="no">Vazgeç</button>' +
        '<button type="button" class="btn btn-danger" data-ok="yes">Evet</button>' +
        '</div></div>';
    wrap.classList.add('show');
    function close(result) {
        wrap.classList.remove('show');
        wrap.innerHTML = '';
        if (result && onOk) onOk();
    }
    var noBtn = wrap.querySelector('[data-ok="no"]');
    var yesBtn = wrap.querySelector('[data-ok="yes"]');
    if (noBtn) noBtn.addEventListener('click', function () { close(false); });
    if (yesBtn) yesBtn.addEventListener('click', function () { close(true); });
    wrap.addEventListener('click', function (e) { if (e.target === wrap) close(false); });
};

/* ---------- Tema ---------- */
function initThemeToggle() {
    var btn = document.querySelector('.theme-toggle');
    if (!btn) return;
    btn.addEventListener('click', function () {
        document.body.classList.toggle('dark');
        this.textContent = document.body.classList.contains('dark') ? '☀️' : '🌙';
    });
}

/* ---------- Ctrl+K ---------- */
function initQuickCommand() {
    var input = document.getElementById('quickCommandInput');
    if (!input) return;
    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
            e.preventDefault();
            input.focus();
        }
    });
    input.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            var cmd = input.value.trim();
            if (!cmd) return;
            input.value = '';
            if (window.handleQuickCommand) window.handleQuickCommand(cmd);
        }
    });
}

/* ---------- Sol panel toggle: aç/kapat ---------- */
function initRailToggle() {
    var rail = document.getElementById('railToggle');
    var openBtn = document.getElementById('railOpenBtn');
    var panel = document.getElementById('mainRail');
    if (!panel) return;
    if (rail) rail.addEventListener('click', function () { panel.classList.add('closed'); });
    if (openBtn) openBtn.addEventListener('click', function () { panel.classList.remove('closed'); });
}

/* ---------- AI paneli aç/kapat ---------- */
function initAiToggle() {
    var openBtn = document.getElementById('aiToggleBtn');
    var closeBtn = document.getElementById('aiCloseBtn');
    var panel = document.getElementById('aiPanel');
    if (!panel) return;
    if (openBtn) openBtn.addEventListener('click', function () { panel.classList.add('open'); });
    if (closeBtn) closeBtn.addEventListener('click', function () { panel.classList.remove('open'); });
}

window.openAiPanel = function () {
    var panel = document.getElementById('aiPanel');
    if (panel) panel.classList.add('open');
};

/* ---------- Blok oluşturucu ---------- */
var builderState = { color: '#22c55e', durationMinutes: 30 };
var drafts = [];

function initBlockBuilder() {
    // Renk seçimi
    var colors = document.querySelectorAll('#builderColors .color-option');
    colors.forEach(function (el) {
        el.addEventListener('click', function () {
            colors.forEach(function (o) { o.classList.remove('selected'); });
            el.classList.add('selected');
            builderState.color = el.dataset.color;
        });
    });
    if (colors.length) colors[0].classList.add('selected');

    // Süre (manuel dakika girişi)
    var durationInput = document.getElementById('builderDuration');
    if (durationInput) {
        durationInput.addEventListener('input', function () {
            var v = parseInt(this.value, 10);
            builderState.durationMinutes = (isNaN(v) || v <= 0) ? 30 : v;
        });
    }

    // Açıklama: içeriğe göre büyüsün, çok büyüyünce yanda bar
    var desc = document.getElementById('builderDesc');
    if (desc) {
        function growDesc() {
            desc.style.height = 'auto';
            desc.style.height = Math.min(desc.scrollHeight, 96) + 'px';
            desc.style.overflowY = desc.scrollHeight > 96 ? 'auto' : 'hidden';
        }
        desc.addEventListener('input', growDesc);
        growDesc();
    }

    // Üret
    var btn = document.getElementById('buildBlockBtn');
    if (btn) btn.addEventListener('click', buildDraft);
}

function buildDraft() {
    var title = (document.getElementById('builderTitle').value || '').trim();
    if (!title) { showToast('Blok için başlık girin.', 'error'); return; }
    var desc = document.getElementById('builderDesc').value.trim();
    var durationInput = document.getElementById('builderDuration');
    var minutes = durationInput ? parseInt(durationInput.value, 10) : 30;
    if (isNaN(minutes) || minutes <= 0) minutes = 30;
    var draft = {
        id: Date.now(),
        title: title,
        description: desc,
        color: builderState.color,
        durationMinutes: minutes
    };
    drafts.push(draft);
    document.getElementById('builderTitle').value = '';
    var d = document.getElementById('builderDesc');
    d.value = '';
    d.style.height = 'auto';
    renderDrafts();
}

function renderDrafts() {
    var list = document.getElementById('draftList');
    if (!list) return;
    if (!drafts.length) {
        list.innerHTML = '<div class="draft-hint">Blok üretince buraya gelir, takvime sürükle.</div>';
        return;
    }
    list.innerHTML = '';
    drafts.forEach(function (d) {
        var el = document.createElement('div');
        el.className = 'draft-block';
        el.draggable = true;
        el.dataset.draftId = d.id;
        el.innerHTML =
            '<span class="draft-color" style="background:' + d.color + '"></span>' +
            '<span class="draft-info"><span class="draft-title">' + escapeHtml(d.title) + '</span>' +
            '<span class="draft-meta">' + d.durationMinutes + ' dk</span></span>' +
            '<button type="button" class="draft-remove" title="Sil">✕</button>';
        var remove = el.querySelector('.draft-remove');
        remove.addEventListener('click', function (e) {
            e.stopPropagation();
            drafts = drafts.filter(function (x) { return x.id !== d.id; });
            renderDrafts();
        });
        el.addEventListener('dragstart', function (e) {
            el.classList.add('dragging');
            window._dragData = { draft: d, durationMinutes: d.durationMinutes, color: d.color };
            e.dataTransfer.setData('text/plain', JSON.stringify(d));
            e.dataTransfer.effectAllowed = 'move';
        });
        el.addEventListener('dragend', function () { el.classList.remove('dragging'); });
        list.appendChild(el);
    });
}

/* ---------- Sürükle-bırak hedefleri (takvim hücreleri) ---------- */
function initDraftDnD() {
    var previewTimer = null;
    var targets = document.querySelectorAll('.day-cell, .time-slot, .timeline-slot');

    // Fare konumundan tam dakikayı hesapla (izgara ile birebir)
    function computeMinutes(e, t) {
        var line = t.closest('.day-line') || t.closest('.day-col');
        if (!line) return null;
        var rect = line.getBoundingClientRect();
        var isHalf = !!line.closest('.day-half');
        var span = isHalf ? 720 : 1440;
        var base = isHalf && line.closest('.day-half.late') ? 720 : 0;
        var frac = Math.min(Math.max((e.clientY - rect.top) / rect.height, 0), 1);
        var minutes = base + frac * span;
        minutes = Math.round(minutes / 5) * 5;
        return Math.min(minutes, 1435);
    }

    function showPreview(t, minutes) {
        var data = window._dragData;
        if (!data) return;
        var line = t.closest('.day-line') || t.closest('.day-col');
        if (!line) return;
        var p = line.querySelector('.drop-preview');
        if (!p) { p = document.createElement('div'); p.className = 'drop-preview'; line.appendChild(p); }
        var isHalf = !!line.closest('.day-half');
        var base = isHalf && line.closest('.day-half.late') ? 720 : 0;
        var pph = parseFloat(getComputedStyle(line).getPropertyValue('--pph')) || 60;
        var dur = data.durationMinutes || 30;
        var color = cssColorToHex(data.color) || '#4f46e5';
        p.style.top = ((minutes - base) / 60 * pph) + 'px';
        p.style.height = Math.max(dur / 60 * pph, 20) + 'px';
        p.style.background = color + '2e';
        p.style.borderColor = color;
    }

    function hidePreview(t) {
        var line = t && (t.closest('.day-line') || t.closest('.day-col'));
        if (!line) return;
        var p = line.querySelector('.drop-preview');
        if (p) p.remove();
    }

    targets.forEach(function (t) {
        t.addEventListener('dragover', function (e) {
            if (!window._dragData) return;
            e.preventDefault();
            e.dataTransfer.dropEffect = 'move';
            if (t.dataset.hour !== undefined && t.dataset.hour !== '') {
                t.classList.remove('drop-target');
                clearTimeout(previewTimer);
                previewTimer = setTimeout(function () { showPreview(t, computeMinutes(e, t)); }, 25);
            } else {
                hidePreview(t);
                t.classList.add('drop-target');
            }
        });
        t.addEventListener('dragleave', function () {
            t.classList.remove('drop-target');
            clearTimeout(previewTimer);
            hidePreview(t);
        });
        t.addEventListener('drop', function (e) {
            e.preventDefault();
            e.stopPropagation();
            t.classList.remove('drop-target');
            clearTimeout(previewTimer);
            hidePreview(t);
            var data = window._dragData;
            var raw = e.dataTransfer.getData('text/plain');
            if (!data && raw) {
                try {
                    var d = JSON.parse(raw);
                    data = d.moveId ? { moveId: d.moveId, durationMinutes: 0 } : { draft: d, durationMinutes: d.durationMinutes, color: d.color };
                } catch (err) { return; }
            }
            if (!data) return;
            var date = t.dataset.date ||
                (t.closest('.day-cell') ? t.closest('.day-cell').getAttribute('data-date') : null) ||
                new Date().toISOString().slice(0, 10);
            var minutes = (t.dataset.hour !== undefined && t.dataset.hour !== '') ? computeMinutes(e, t) : null;
            var hour = 9, minute = 0;
            if (minutes !== null) { hour = Math.floor(minutes / 60); minute = minutes % 60; }
            // Mevcut blok taşıma
            if (data.moveId) {
                moveBlock(data.moveId, date, minutes === null ? null : hour, minutes === null ? null : minute);
                return;
            }
            // Hafta görünümünde YENİ blok oluşturma yok (sadece taşıma)
            if (t.closest('.week-grid')) {
                showToast('Hafta görünümünde yeni blok oluşturulamaz — Ay veya Gün görünümünü kullan.', 'info');
                return;
            }
            createBlockFromDraft(data.draft, date, hour, minute);
        });
    });

    document.addEventListener('dragend', function () {
        window._dragData = null;
        document.querySelectorAll('.drop-preview').forEach(function (p) { p.remove(); });
        document.querySelectorAll('.drop-target').forEach(function (t) { t.classList.remove('drop-target'); });
    });
}

function cssColorToHex(color) {
    if (!color) return null;
    if (color[0] === '#') return color;
    var m = /rgba?\((\d+),\s*(\d+),\s*(\d+)/.exec(color);
    if (!m) return null;
    var hex = '#';
    for (var i = 1; i <= 3; i++) hex += ('0' + parseInt(m[i], 10).toString(16)).slice(-2);
    return hex;
}

function createBlockFromDraft(draft, date, startHour, startMinute) {
    var start = new Date(2000, 0, 1, startHour, startMinute, 0);
    var end = new Date(start.getTime() + draft.durationMinutes * 60000);
    function fmt(d) { return ('0' + d.getHours()).slice(-2) + ':' + ('0' + d.getMinutes()).slice(-2) + ':00'; }
    var payload = {
        title: draft.title,
        description: draft.description || '',
        notes: '',
        date: date,
        startTime: fmt(start),
        endTime: fmt(end),
        priority: 1,
        repeat: 0,
        status: 0,
        color: draft.color,
        isAiCreated: false,
        reminderMinutes: null,
        userId: parseInt(document.getElementById('blockUserId') ? document.getElementById('blockUserId').value : 5)
    };
    fetch('http://localhost:5210/api/blocks', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    }).then(function (res) {
        if (!res.ok) {
            return res.text().then(function (t) { throw new Error(t || ('HTTP ' + res.status)); });
        }
        drafts = drafts.filter(function (x) { return x.id !== draft.id; });
        renderDrafts();
        window.location.reload();
    }).catch(function (err) {
        showToast(err.message || 'Blok eklenemedi', 'error');
    });
}

function escapeHtml(s) {
    var d = document.createElement('div');
    d.textContent = s;
    return d.innerHTML;
}

/* ---------- Koyu tema ---------- */
document.addEventListener('DOMContentLoaded', function () {
    var style = document.createElement('style');
    style.textContent = 'body.dark { --bg:#111318; --surface:#1a1d24; --border:#2a2e37; --text:#e5e7eb; --text-secondary:#9ca3af; }';
    document.head.appendChild(style);
});