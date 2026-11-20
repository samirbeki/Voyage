(() => {
    'use strict';

    const list = document.querySelector('[data-packing-list]');

    if (!list) {
        return;
    }

    function updateItem(itemId, isPacked) {
        const item = list.querySelector(
            `[data-item-id="${itemId}"]`
        );

        if (!item) {
            return;
        }

        item.classList.toggle('is-packed', isPacked);

        const button = item.querySelector('.packing-toggle-form button');

        if (!button) {
            return;
        }

        const name = button.dataset.itemName;

        button.setAttribute('aria-pressed', String(isPacked));
        button.setAttribute(
            'aria-label',
            isPacked
                ? `Mark ${name} as not packed`
                : `Mark ${name} as packed`
        );

        const check = button.querySelector('.packing-check');

        if (check) {
            check.textContent = isPacked ? '☑' : '☐';
        }
    }

    function updateGroupCount(form) {
        const group = form.closest('.packing-group');

        if (!group) {
            return;
        }

        const items = Array.from(
            group.querySelectorAll('[data-item-id]')
        );

        const packed = items.filter(item =>
            item.classList.contains('is-packed')
        ).length;

        const counter = group.querySelector('[data-group-count]');

        if (counter) {
            counter.textContent = `${packed}/${items.length}`;
        }
    }

    function updateProgress(data) {
        const progress = document.querySelector('[data-packing-progress]');

        if (!progress) {
            return;
        }

        progress.dataset.total = data.total;
        progress.dataset.packed = data.packed;
        progress.dataset.percent = data.percent;

        const text = progress.querySelector('[data-progress-text]');
        const percent = progress.querySelector('[data-progress-percent]');
        const bar = progress.querySelector('[data-progress-bar]');
        const complete = progress.querySelector('[data-progress-complete]');
        const progressBar = progress.querySelector('.progress');

        if (text) {
            text.textContent = `${data.packed} of ${data.total} packed`;
        }

        if (percent) {
            percent.textContent = data.percent;
        }

        if (bar) {
            bar.style.width = `${data.percent}%`;
            bar.classList.toggle('bg-success', data.isComplete);
        }

        if (progressBar) {
            progressBar.setAttribute('aria-valuenow', data.percent);
        }

        if (complete) {
            complete.hidden = !data.isComplete;
        }
    }

    list.addEventListener('submit', async event => {
        const form = event.target.closest('.packing-toggle-form');

        if (!form) {
            return;
        }

        event.preventDefault();

        const button = form.querySelector('button[type="submit"]');

        if (!button || button.disabled) {
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

            if (!response.ok) {
                form.submit();
                return;
            }

            const data = await response.json();

            updateItem(data.itemId, data.isPacked);
            updateGroupCount(form);
            updateProgress(data);
        } catch {
            form.submit();
        } finally {
            button.disabled = false;
            button.removeAttribute('aria-busy');
        }
    });
})();