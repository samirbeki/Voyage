(() => {
    'use strict';

    const container = document.getElementById('destination-weather');

    if (!container) {
        return;
    }

    fetch(container.dataset.weatherUrl, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
        .then(response => response.status === 200 ? response.text() : '')
        .then(html => {
            if (html) {
                container.innerHTML = html;
            } else {
                container.remove();
            }
        })
        .catch(() => container.remove());
})();
