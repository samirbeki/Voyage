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

    map.attributionControl.setPrefix('<a href="https://leafletjs.com" target="_blank" rel="noopener noreferrer">Leaflet</a>');

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

    const label = document.createElement('div');
    const name = document.createElement('strong');
    name.textContent = element.dataset.name;
    label.append(name);

    if (element.dataset.country) {
        const country = document.createElement('div');
        country.className = 'v-map-popup-country';
        country.textContent = element.dataset.country;
        label.append(country);
    }

    L.marker(position, { icon, title: element.dataset.name })
        .addTo(map)
        .bindPopup(label);

    map.on('focus', () => map.scrollWheelZoom.enable());
    map.on('blur', () => map.scrollWheelZoom.disable());
})();
