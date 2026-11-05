// Independent navigation behavior; uses the existing Bootstrap collapse API.
(() => {
    const navigation = document.getElementById('portfolio-navigation');
    const toggle = document.querySelector('[data-bs-target="#portfolio-navigation"]');
    if (!navigation || !toggle || !window.bootstrap) return;

    navigation.addEventListener('click', (event) => {
        const link = event.target.closest('a[href^="#"]');
        if (!link || toggle.getAttribute('aria-expanded') !== 'true') return;
        const section = document.getElementById(link.getAttribute('href').slice(1));
        navigation.addEventListener('hidden.bs.collapse', () => {
            if (section) section.scrollIntoView({ block: 'start', behavior: 'instant' });
        }, { once: true });
        bootstrap.Collapse.getOrCreateInstance(navigation, { toggle: false }).hide();
    });
})();