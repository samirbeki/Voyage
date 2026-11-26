(() => {
    'use strict';

    document.querySelectorAll('[data-copy-target]').forEach(button => {
        const input = document.querySelector(button.dataset.copyTarget);

        if (!input) {
            return;
        }

        const originalHtml = button.innerHTML;

        button.addEventListener('click', async () => {
            try {
                await navigator.clipboard.writeText(input.value);
            } catch {
                input.select();
                document.execCommand('copy');
            }

            button.innerHTML = '<i class="bi bi-check2"></i> Copied';

            setTimeout(() => {
                button.innerHTML = originalHtml;
            }, 2000);
        });
    });
})();
