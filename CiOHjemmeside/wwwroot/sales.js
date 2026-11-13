// Holder skaermen taendt paa salgs-iPad'en (Screen Wake Lock API, Safari 16.4+).
// Laasen slippes automatisk naar fanen skjules, saa den genanmodes naar man vender tilbage.
window.cioSales = (function () {
    let lock = null;
    let wanted = false;

    async function request() {
        if (!('wakeLock' in navigator) || document.visibilityState !== 'visible') return;
        try {
            lock = await navigator.wakeLock.request('screen');
        } catch (e) {
            // Ikke understoettet / afvist (fx strømsparetilstand) - salget virker stadig.
        }
    }

    document.addEventListener('visibilitychange', function () {
        if (wanted && document.visibilityState === 'visible') request();
    });

    return {
        keepAwake: function () {
            wanted = true;
            return request();
        }
    };
})();
