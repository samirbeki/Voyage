(() => {
    'use strict';

    const source = document.querySelector('[data-weather-url]');
    const targets = document.querySelectorAll('[data-weather-target]');

    if (!source || targets.length === 0) {
        return;
    }

    const removeTargets = () => targets.forEach(target => target.remove());

    fetch(source.dataset.weatherUrl, { headers: { 'X-Requested-With': 'XMLHttpRequest' } })
        .then(response => response.status === 200 ? response.text() : '')
        .then(html => {
            if (!html) {
                removeTargets();
                return;
            }

            const template = document.createElement('template');
            template.innerHTML = html;

            targets.forEach(target => {
                const part = template.content.querySelector(
                    `[data-weather-part="${target.dataset.weatherTarget}"]`);

                if (part) {
                    target.replaceChildren(...part.childNodes);
                } else {
                    target.remove();
                }
            });
        })
        .catch(removeTargets);
})();
