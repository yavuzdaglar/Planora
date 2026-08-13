// Planora calendar.js
var API = 'http://localhost:5210/';

function navigateCalendar(dir) {
    var url = new URL(window.location.href);
    var date = url.searchParams.get('date') || new Date().toISOString().slice(0, 10);
    var d = new Date(date + 'T00:00:00');
    if (dir === 'prev') d.setDate(d.getDate() - 1);
    if (dir === 'next') d.setDate(d.getDate() + 1);
    if (dir === 'today') d = new Date();
    url.searchParams.set('date', d.toISOString().slice(0, 10));
    window.location.href = url.toString();
}

/* ---------- Saat sütunu zoom (hafta / gün) ---------- */
var ZOOM_KEY = 'planoraZoom';
var ZOOM_LEVELS = [24, 36, 48, 60, 90, 120, 150];

function getZoomIndex() {
    var z = parseInt(localStorage.getItem(ZOOM_KEY), 10);
    var i = ZOOM_LEVELS.indexOf(z);
    return i === -1 ? 3 : i;
}
function currentZoom() { return ZOOM_LEVELS[getZoomIndex()]; }
function zoomContainer() {
    return document.getElementById('weekGrid') || document.getElementById('dayTimeline');
}

function renderTimeLabels(container, zoom) {
    var cols = container.querySelectorAll('.time-col');
    if (!cols.length) return;
    var haveHalf = zoom >= 90;
    var haveQuarter = zoom >= 130;
    function pad(n) { return n < 10 ? '0' + n : '' + n; }
    function add(col, text, hPx, sub) {
        var l = document.createElement('div');
        l.className = 'time-label' + (sub ? ' sub' : '');
        l.style.height = hPx + 'px';
        l.textContent = text;
        col.appendChild(l);
    }
    cols.forEach(function (col, ci) {
        col.innerHTML = '';
        var base = cols.length === 2 ? ci * 12 : 0;
        var count = cols.length === 2 ? 12 : 24;
        for (var h = base; h < base + count; h++) {
            if (haveQuarter) {
                add(col, pad(h) + ':00', zoom / 4, false);
                add(col, pad(h) + ':15', zoom / 4, true);
                add(col, pad(h) + ':30', zoom / 4, true);
                add(col, pad(h) + ':45', zoom / 4, true);
            } else if (haveHalf) {
                add(col, pad(h) + ':00', zoom / 2, false);
                add(col, pad(h) + ':30', zoom / 2, true);
            } else {
                add(col, pad(h) + ':00', zoom, false);
            }
        }
    });
}

function positionBlocks(container, zoom) {
    container.querySelectorAll('[data-top-min]').forEach(function (b) {
        var base = b.closest('.day-half.late') ? 720 : 0;
        var top = (parseFloat(b.dataset.topMin) - base) / 60 * zoom;
        var height = Math.max(parseFloat(b.dataset.durMin) / 60 * zoom, 24);
        b.style.top = top + 'px';
        b.style.height = height + 'px';
    });
}

function applyZoom() {
    var container = zoomContainer();
    if (!container) return;
    var zoom = currentZoom();
    container.style.setProperty('--pph', zoom + 'px');
    var haveHalf = zoom >= 90;
    var haveQuarter = zoom >= 130;
    if (haveQuarter) container.style.setProperty('--sub-period', (zoom / 4) + 'px');
    else if (haveHalf) container.style.setProperty('--sub-period', (zoom / 2) + 'px');
    else container.style.removeProperty('--sub-period');
    container.classList.toggle('sub-lines', haveHalf);
    renderTimeLabels(container, zoom);
    positionBlocks(container, zoom);
    var lbl = document.getElementById('zoomLabel');
    if (lbl) lbl.textContent = zoom + ' px';
}

function initZoomControls() {
    var outBtn = document.getElementById('zoomOutBtn');
    var inBtn = document.getElementById('zoomInBtn');
    if (!outBtn && !inBtn) return;
    outBtn.addEventListener('click', function () {
        var i = getZoomIndex();
        if (i > 0) { localStorage.setItem(ZOOM_KEY, ZOOM_LEVELS[i - 1]); applyZoom(); }
    });
    inBtn.addEventListener('click', function () {
        var i = getZoomIndex();
        if (i < ZOOM_LEVELS.length - 1) { localStorage.setItem(ZOOM_KEY, ZOOM_LEVELS[i + 1]); applyZoom(); }
    });
}

document.addEventListener('DOMContentLoaded', function () {
    initZoomControls();
    applyZoom();
});

/* ---------- Blok tik: tamamla / geri al (blok silinmez, kalır) ---------- */
document.addEventListener('click', function (e) {
    var tick = e.target.closest('.block-tick');
    if (!tick) return;
    e.preventDefault();
    e.stopPropagation();
    var block = tick.closest('[data-id]');
    if (!block) return;
    var done = block.classList.contains('done');
    fetch(API + 'api/blocks/' + block.dataset.id + '/status', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(done ? 0 : 2)
    }).then(function (res) {
        if (!res.ok) throw new Error('HTTP ' + res.status);
        if (done) {
            block.classList.remove('done');
            showToast('Blok beklemeye alındı', 'info');
        } else {
            block.classList.add('done');
            showToast('Blok tamamlandı ✓', 'success');
        }
    }).catch(function (err) {
        showToast('Durum güncellenemedi: ' + err.message, 'error');
    });
});

/* ---------- Blok sil: yanındaki ✕ (üstten popup onayı) ---------- */
document.addEventListener('click', function (e) {
    var del = e.target.closest('.block-delete');
    if (!del) return;
    e.preventDefault();
    e.stopPropagation();
    var block = del.closest('[data-id]');
    if (!block) return;
    confirmAction('«' + block.dataset.title + '» silinsin mi?', function () {
        fetch(API + 'api/blocks/' + block.dataset.id, { method: 'DELETE' })
            .then(function (res) {
                if (!res.ok) throw new Error('HTTP ' + res.status);
                block.remove();
                showToast('Blok silindi', 'success');
            })
            .catch(function (err) {
                showToast('Silinemedi: ' + err.message, 'error');
            });
    });
});

/* ---------- Blok detay: çift tık ---------- */
function openBlockDetails(el) {
    var modal = document.getElementById('blockDetailModal');
    if (!modal) return;
    document.getElementById('detailTitle').textContent = el.dataset.title || 'Blok';
    document.getElementById('detailTime').textContent = (el.dataset.date || '') + '  ' + (el.dataset.start || '') + ' – ' + (el.dataset.end || '');
    var pri = ['Düşük', 'Orta', 'Yüksek', 'Acil'];
    document.getElementById('detailPriority').textContent = pri[parseInt(el.dataset.priority, 10)] || '—';
    var st = ['Bekliyor', 'Devam ediyor', 'Tamamlandı', 'İptal'];
    document.getElementById('detailStatus').textContent = st[parseInt(el.dataset.status, 10)] || '—';
    document.getElementById('detailDesc').textContent = el.dataset.desc || '—';
    modal.style.display = 'flex';
}

window.closeBlockDetailModal = function () {
    var m = document.getElementById('blockDetailModal');
    if (m) m.style.display = 'none';
};

document.addEventListener('DOMContentLoaded', function () {
    var m = document.getElementById('blockDetailModal');
    if (m) m.addEventListener('click', function (e) { if (e.target === m) closeBlockDetailModal(); });
});

document.addEventListener('dblclick', function (e) {
    var el = e.target.closest('.week-block, .day-block, .block-pill');
    if (!el) return;
    e.preventDefault();
    e.stopPropagation();
    openBlockDetails(el);
});

document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') closeBlockDetailModal();
});

/* ---------- Blok taşıma: sürükle-bırak ---------- */
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.week-block, .day-block, .block-pill').forEach(function (el) {
        el.draggable = true;
        el.addEventListener('dragstart', function (e) {
            el.classList.add('dragging');
            window._dragData = {
                moveId: el.dataset.id,
                durationMinutes: parseFloat(el.dataset.durMin) || 30,
                color: cssColorToHex(el.style.background) || null
            };
            e.dataTransfer.setData('text/plain', JSON.stringify({ moveId: el.dataset.id }));
            e.dataTransfer.effectAllowed = 'move';
        });
        el.addEventListener('dragend', function () { el.classList.remove('dragging'); });
    });
});

async function moveBlock(id, date, hour, minute) {
    try {
        var res = await fetch(API + 'api/blocks/' + id);
        if (!res.ok) throw new Error('HTTP ' + res.status);
        var b = await res.json();
        var parts = (b.startTime || '09:00').split(':');
        var startHour = (hour !== null && hour !== undefined) ? hour : parseInt(parts[0], 10);
        var startMin = (hour !== null && hour !== undefined) ? (minute || 0) : parseInt(parts[1], 10);
        var start = new Date(2000, 0, 1, startHour, startMin, 0);
        var end = new Date(start.getTime() + b.durationMinutes * 60000);
        function fmt(d) { return ('0' + d.getHours()).slice(-2) + ':' + ('0' + d.getMinutes()).slice(-2) + ':00'; }
        var payload = {
            id: b.id, title: b.title, description: b.description, notes: b.notes,
            date: date, startTime: fmt(start), endTime: fmt(end),
            priority: b.priority, repeat: b.repeat, status: b.status,
            color: b.color, isAiCreated: b.isAiCreated, reminderMinutes: b.reminderMinutes,
            userId: b.userId
        };
        var upd = await fetch(API + 'api/blocks', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        if (!upd.ok) throw new Error(await upd.text());
        showToast('Blok taşındı ✓', 'success');
        window.location.reload();
    } catch (err) {
        showToast('Taşınamadı: ' + err.message, 'error');
    }
}