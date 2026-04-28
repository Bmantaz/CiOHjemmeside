window.cioCookie = {
    getConsent: function () {
        try {
            return localStorage.getItem('cio-cookie-consent');
        } catch (e) {
            return null;
        }
    },
    setConsent: function (value) {
        try {
            localStorage.setItem('cio-cookie-consent', value);
        } catch (e) { }
    },
    onAccept: function () {
        // Placeholder: load analytics or other non-essential scripts here.
        console.log('EPK: cookie consent accepted.');
    }
};
