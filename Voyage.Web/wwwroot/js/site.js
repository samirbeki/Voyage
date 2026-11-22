document.addEventListener('submit', function (e) {
    const form = e.target;

    if (form.dataset.submitting === 'true') {
        e.preventDefault();
        return;
    }

    setTimeout(function () {
        if (e.defaultPrevented) {
            return;
        }

        form.dataset.submitting = 'true';

        form.querySelectorAll('button[type="submit"], input[type="submit"]')
            .forEach(function (button) {
                button.disabled = true;
            });
    }, 0);
});

window.addEventListener('pageshow', function () {
    document.querySelectorAll('form[data-submitting]').forEach(function (form) {
        delete form.dataset.submitting;

        form.querySelectorAll('button[type="submit"], input[type="submit"]')
            .forEach(function (button) {
                button.disabled = false;
            });
    });
});
