// AfterQuake - Emergency Response Platform
(function () {
    // Cookie Consent banner
    if (!localStorage.getItem('cookieConsent')) {
        var banner = document.createElement('div');
        banner.id = 'cookie-consent';
        banner.className = 'fixed bottom-0 left-0 right-0 bg-gray-900 text-white p-4 text-center z-50';
        banner.innerHTML = '<p class="text-sm">Usamos cookies necesarias para el funcionamiento de la plataforma. <button onclick="acceptCookies()" class="ml-2 bg-red-600 px-4 py-1 rounded font-bold text-sm">Aceptar</button></p>';
        document.body.appendChild(banner);
    }

    window.acceptCookies = function () {
        localStorage.setItem('cookieConsent', 'true');
        var el = document.getElementById('cookie-consent');
        if (el) el.remove();
    };

    // Auto-dismiss alerts
    document.querySelectorAll('.alert-auto-dismiss').forEach(function (el) {
        setTimeout(function () { el.style.transition = 'opacity 0.5s'; el.style.opacity = '0'; setTimeout(function () { el.remove(); }, 500); }, 5000);
    });

    // SOS button confirmation
    document.querySelectorAll('.btn-emergency').forEach(function (btn) {
        btn.addEventListener('click', function (e) {
            if (!confirm('¿Estás seguro de enviar esta alerta de emergencia? Solo para situaciones críticas.')) {
                e.preventDefault();
            }
        });
    });

    // Service Worker registration
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.register('/service-worker.js').catch(function () { });
    }

    // Auto-refresh active emergency counter every 30s
    var counter = document.getElementById('active-emergencies-count');
    if (counter) {
        setInterval(function () {
            fetch('/api/dashboard/active-count').then(function (r) { return r.json(); }).then(function (data) {
                if (data.count !== undefined) counter.textContent = data.count;
            }).catch(function () { });
        }, 30000);
    }
})();
