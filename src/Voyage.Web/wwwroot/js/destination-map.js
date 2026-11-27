(() => {
    'use strict';

    const element = document.querySelector('[data-map]');

    if (!element || typeof L === 'undefined') {
        return;
    }

    const position = [Number(element.dataset.lat), Number(element.dataset.lng)];

    if (position.some(Number.isNaN)) {
        return;
    }

    const map = L.map(element, { scrollWheelZoom: false }).setView(position, 11);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    const icon = L.divIcon({
        className: 'v-map-marker',
        html: '<i class="bi bi-geo-alt-fill"></i>',
        iconSize: [36, 36],
        iconAnchor: [18, 34],
        popupAnchor: [0, -30]
    });

    const label = document.createElement('strong');
    label.textContent = element.dataset.name;

    L.marker(position, { icon, title: element.dataset.name })
        .addTo(map)
        .bindPopup(label);

    map.on('focus', () => map.scrollWheelZoom.enable());
    map.on('blur', () => map.scrollWheelZoom.disable());
})();
