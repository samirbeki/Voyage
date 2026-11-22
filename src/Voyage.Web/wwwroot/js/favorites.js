(() => {
    'use strict';

    const statusRegion = document.getElementById('client-status');
    let statusTimer;

    function showStatus(message, isError) {
        if (!statusRegion || !message) {
            return;
        }

        const alert = document.createElement('div');
        alert.className = `alert ${isError ? 'alert-danger' : 'alert-success'}`;
        alert.setAttribute('role', isError ? 'alert' : 'status');
        alert.textContent = message;

        statusRegion.replaceChildren(alert);

        clearTimeout(statusTimer);

        statusTimer = setTimeout(() => {
            statusRegion.replaceChildren();
        }, 4000);
    }

    function updateButtons(destinationId, isFavorite) {
        document.querySelectorAll(
            `.favorite-form[data-destination-id="${destinationId}"]`
        ).forEach(form => {
            const name = form.dataset.destinationName;
            const label = isFavorite
                ? `Remove ${name} from favorites`
                : `Add ${name} to favorites`;

            form.action = isFavorite
                ? form.dataset.removeUrl
                : form.dataset.addUrl;

            const button = form.querySelector('.favorite-btn');

            button.classList.toggle('is-favorite', isFavorite);
            button.setAttribute('aria-pressed', String(isFavorite));
            button.setAttribute('aria-label', label);
            button.title = label;
            button.querySelector('.favorite-icon').textContent =
                isFavorite ? '♥' : '♡';

            const text = button.querySelector('.favorite-label');

            if (text) {
                text.textContent = isFavorite ? 'Saved' : 'Save';
            }
        });
    }

    function updateCount(destinationId, countText) {
        document.querySelectorAll(
            `[data-favorite-count-for="${destinationId}"]`
        ).forEach(element => {
            element.textContent = countText;
        });
    }

    document.addEventListener('submit', async event => {
        const form = event.target.closest('.favorite-form');

        if (!form || form.closest('[data-favorites-list]')) {
            return;
        }

        event.preventDefault();

        const button = form.querySelector('.favorite-btn');

        if (button.disabled) {
            return;
        }

        button.disabled = true;
        button.setAttribute('aria-busy', 'true');

        try {
            const response = await fetch(form.action, {
                method: 'POST',
                body: new FormData(form),
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.redirected) {
                window.location.href = response.url;
                return;
            }

            if (response.status === 429) {
                showStatus(
                    'Too many requests. Please wait a moment and try again.',
                    true
                );
                return;
            }

            if (!response.ok && response.status !== 409) {
                form.submit();
                return;
            }

            const data = await response.json();

            updateButtons(form.dataset.destinationId, data.isFavorite);
            updateCount(form.dataset.destinationId, data.countText);
            showStatus(data.message, !data.success);
        } catch {
            form.submit();
        } finally {
            button.disabled = false;
            button.removeAttribute('aria-busy');
        }
    });
})();