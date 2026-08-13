// Planora ai.js
var AI_API = 'http://localhost:5210/';
var aiUserId = parseInt(document.getElementById('aiUserId')?.value || '1');
var currentPlan = null;
var TR_DAYS = { pazartesi: 1, pzt: 1, monday: 1, mon: 1, salı: 2, sali: 2, sal: 2, tuesday: 2, tue: 2, çarşamba: 3, carsamba: 3, çar: 3, car: 3, wednesday: 3, wed: 3, perşembe: 4, persembe: 4, per: 4, thursday: 4, thu: 4, cuma: 5, cum: 5, friday: 5, fri: 5, cumartesi: 6, cmt: 6, saturday: 6, sat: 6, pazar: 7, paz: 7, sunday: 7, sun: 7 };

document.addEventListener('DOMContentLoaded', function () {
    var sendBtn = document.getElementById('aiSendBtn');
    var prompt = document.getElementById('aiPrompt');
    if (sendBtn) sendBtn.addEventListener('click', function () { handlePrompt(); });
    if (prompt) prompt.addEventListener('keydown', function (e) { if (e.key === 'Enter' && !e.shiftKey) { e.preventDefault(); handlePrompt(); } });
    var applyBtn = document.getElementById('aiApplyBtn');
    if (applyBtn) applyBtn.addEventListener('click', applyPlan);
});

function addUserMsg(text) {
    var chat = document.getElementById('aiChat');
    var el = document.createElement('div');
    el.className = 'user-msg';
    el.textContent = text;
    chat.appendChild(el);
    chat.scrollTop = chat.scrollHeight;
}

function addAiMsg(html) {
    var chat = document.getElementById('aiChat');
    var el = document.createElement('div');
    el.className = 'ai-msg';
    el.innerHTML = html;
    chat.appendChild(el);
    chat.scrollTop = chat.scrollHeight;
}

function parseNaturalLanguage(text) {
    var lower = text.toLowerCase();
    // Basit doğal dil parser — hafta planı çıkarmak için
    var request = {
        userId: aiUserId,
        startDate: mondayOfWeek().toISOString().slice(0, 10),
        numberOfDays: 7,
        tasks: [],
        fixedBlocks: [],
        freeDays: []
    };

    // Boş günler (Pazarı boş bırak)
    for (var k in TR_DAYS) {
        if (lower.includes(k + 'ı boş') || lower.includes(k + "'ı boş") || lower.includes(k + 'u tamamen') || lower.includes(k + ' boş bırak') || lower.includes('keep ' + k + ' free')) {
            var day = TR_DAYS[k];
            if (request.freeDays.indexOf(day) === -1) request.freeDays.push(day);
        }
    }

    // Sabit bloklar: spor/gym [günler] saat
    var sport = /(?:spor|gym|fitness)\s*(?:için\s*)?(\w+)(?:,|\s)*(\w+)?(?:,|\s)*(\w+)?.*?\b(\d{1,2})(?::(\d{2}))?/i.exec(lower);
    var fixedDays = [];
    for (var k2 in TR_DAYS) {
        if (text.includes(k2) || lower.includes(k2)) {
            // gün adı fixed blok bağlamında
        }
    }
    if (/(spor|gym|fitness)/.test(lower)) {
        var gymDays = [];
        var gw = lower.split(/\s+/);
        for (var i = 0; i < gw.length; i++) {
            if (TR_DAYS[gw[i]] !== undefined && i < gw.length - 1 && !/(analiz|çalışma|study|net|proje|project|okul|toplantı|meeting)/.test(gw[i + 1])) {
                // gün adı task bağlamında değilse gym günü
            }
        }
        // Pzt-Çar-Cuma gibi "Pzt Çar Cuma" arayışı: komşu günleri yakala
        var m = lower.match(/(\w+)[-\s,]+\s*(\w+)[-\s,]+\s*(\w+)/);
        var hourMatch = lower.match(/(\d{1,2})[:.]?(\d{2})?\s*('da|'de|saat|at)?/);
        // Basit: Pzt-Çar-Cuma 18:00
        var list = lower.split(/[,\s]+/);
        for (var j = 0; j < list.length; j++) {
            var key = list[j].replace(/[',]/g, '').toLowerCase();
            if (TR_DAYS[key]) gymDays.push(TR_DAYS[key]);
        }
        var h = parseInt((/(\d{1,2}):(\d{2})/.exec(lower) || [])[1]) || 18;
        var mm = parseInt((/(\d{1,2}):(\d{2})/.exec(lower) || [])[2]) || 0;
        if (gymDays.length) {
            request.fixedBlocks.push({
                title: 'Spor',
                description: '',
                startTime: h + ':00:00',
                endTime: (h + 1) + ':00:00',
                priority: 2,
                color: '#f97316',
                days: gymDays
            });
        }
    }

    // Görevler: "X saat çalışma / çalışmam lazım", "1 saat net proje"
    // Analiz / study
    var studyMatch = lower.match(/(?:analiz|study|ders|çalışma)\s*(?:sınavım|var|lazım|çalışmam)?.*?(\d+)\s*saat/i) || lower.match(/(\d+)\s*saat\s*(?:çalısma|çalışma|study|analiz|ders)/i);
    if (/(analiz|study|ders|okul)/.test(lower)) {
        var studyHours = studyMatch ? parseInt(studyMatch[1]) : 2;
        request.tasks.push({
            title: 'Analiz Çalışma',
            description: '',
            durationMinutes: studyHours * 60,
            priority: 2,
            color: '#3b82f6',
            days: [1, 2, 3, 4, 5]
        });
    }
    // .NET / project
    if (/(\.net|project|proje)/.test(lower)) {
        request.tasks.push({
            title: '.NET Proje',
            description: '',
            durationMinutes: 60,
            priority: 1,
            color: '#8b5cf6',
            days: [1, 2, 3, 4, 5, 6, 7]
        });
    }

    return request;
}

function mondayOfWeek() {
    var now = new Date();
    var day = (now.getDay() + 6) % 7;
    var monday = new Date(now);
    monday.setDate(now.getDate() - day);
    return monday;
}

async function handlePrompt() {
    var prompt = document.getElementById('aiPrompt');
    var text = prompt.value.trim();
    if (!text) return;

    addUserMsg(text);
    prompt.value = '';

    var request;
    try {
        request = parseNaturalLanguage(text);
    } catch (e) {
        addAiMsg('İsteğini çözümleyemedim: ' + e.message);
        return;
    }

    addAiMsg('✦ Plan üretiliyor...');

    try {
        var res = await fetch(AI_API + 'api/ai/plan', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(request)
        });
        var plan = await res.json();
        currentPlan = plan;
        renderPlan(plan);
        addAiMsg('<p><b>' + (plan.message || 'Plan hazır.') + '</b></p>');
    } catch (err) {
        addAiMsg('API yanıt vermedi: ' + err.message);
        showToast('API yanıt vermedi: ' + err.message, 'error');
    }
}

function renderPlan(plan) {
    var list = document.getElementById('aiPlanList');
    var summary = document.getElementById('aiSummary');
    var conflicts = document.getElementById('aiConflicts');
    var applyBtn = document.getElementById('aiApplyBtn');

    list.innerHTML = '';
    if (summary) summary.style.display = 'block';

    document.querySelector('.ai-plan-panel .empty-state')?.remove();

    // Özet
    if (summary) {
        var chips = [];
        for (var k in plan.summary) {
            chips.push('<span class="chip">✓ ' + k + ' — ' + plan.summary[k] + ' blok</span>');
        }
        summary.innerHTML = '<div class="msg">Planora ' + (plan.proposedBlocks ? plan.proposedBlocks.length : 0) + ' blok oluşturdu.</div><div class="chips">' + chips.join('') + '</div>';
    }

    // Bloklar — gün bazlı
    if (plan.proposedBlocks && plan.proposedBlocks.length) {
        var byDay = {};
        plan.proposedBlocks.forEach(function (b) {
            var key = b.date.slice(0, 10);
            if (!byDay[key]) byDay[key] = [];
            byDay[key].push(b);
        });
        Object.keys(byDay).sort().forEach(function (d) {
            var dateObj = new Date(d + 'T00:00:00');
            var dayLabel = dateObj.toLocaleDateString('tr-TR', { weekday: 'long', day: 'numeric', month: 'long' });
            var sec = document.createElement('div');
            sec.className = 'ai-plan-day';
            sec.innerHTML = '<h4>' + dayLabel + '</h4>';
            byDay[d].forEach(function (b) {
                var color = b.color || '#4f46e5';
                sec.innerHTML += '<div class="ai-plan-block"><span class="dot" style="background:' + color + '"></span><span class="time">' + (b.startTime || '').slice(0, 5) + ' – ' + (b.endTime || '').slice(0, 5) + '</span><strong>' + b.title + '</strong></div>';
            });
            list.appendChild(sec);
        });
    } else {
        list.innerHTML = '<div class="empty-state"><p>Plan üretilemedi.</p><span>' + (plan.message || '') + '</span></div>';
    }

    // Çakışmalar
    if (conflicts) {
        if (plan.conflicts && plan.conflicts.length) {
            conflicts.style.display = 'block';
            conflicts.innerHTML = plan.conflicts.map(function (c) { return '<div class="ai-conflict">⚠️ ' + c.message + '</div>'; }).join('');
        } else {
            conflicts.style.display = 'none';
        }
    }

    if (applyBtn) applyBtn.style.display = plan.proposedBlocks && plan.proposedBlocks.length ? 'inline-flex' : 'none';
}

async function applyPlan() {
    if (!currentPlan || !currentPlan.proposedBlocks || !currentPlan.proposedBlocks.length) return;
    try {
        var res = await fetch(AI_API + 'api/ai/apply', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId: aiUserId, proposedBlocks: currentPlan.proposedBlocks })
        });
        var result = await res.json();
        showToast(result.message || 'Plan uygulandı.', 'success');
        window.location.href = '/calendar';
    } catch (err) {
        showToast('Uygulanamadı: ' + err.message, 'error');
    }
}

// Ctrl+K hızlı komut desteği (kalıcı görev) — site.js handleQuickCommand
window.handleQuickCommand = async function (cmd) {
    var prompt = document.getElementById('aiPrompt');
    if (prompt) { prompt.value = cmd; prompt.focus(); return; }
    // komut doğrudan gönder
    addUserMsg(cmd);
    try {
        var res = await fetch(AI_API + 'api/ai/command', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ userId: aiUserId, command: cmd, startDate: new Date().toISOString().slice(0, 10) })
        });
        var result = await res.json();
        addAiMsg('<p>' + (result.message || '') + '</p>');
    } catch (err) {
        showToast('Komut işlenemedi: ' + err.message, 'error');
    }
};