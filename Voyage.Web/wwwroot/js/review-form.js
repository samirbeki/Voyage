(() => {
    'use strict';

    document.querySelectorAll('.review-form').forEach(form => {
        const inputs = form.querySelectorAll('.review-star-input');
        const labels = form.querySelectorAll('.review-star-label');
        const stars = form.querySelector('.review-stars');

        if (!stars) {
            return;
        }

        function updateStars(selectedRating) {
            labels.forEach((label, index) => {
                label.classList.toggle('is-active', index < selectedRating);
            });
        }

        inputs.forEach((input, index) => {
            input.addEventListener('change', () => updateStars(Number(input.value)));
            labels[index].addEventListener('mouseenter', () => updateStars(Number(input.value)));
        });

        stars.addEventListener('mouseleave', () => {
            const selected = form.querySelector('.review-star-input:checked');
            updateStars(selected ? Number(selected.value) : 0);
        });
    });
})();
