document.addEventListener('DOMContentLoaded', function() {
    const cards = document.querySelectorAll('.md-typeset .grid.cards > ul > li');
    
    cards.forEach(card => {
        const link = card.querySelector('a');
        if (link) {
            card.style.cursor = 'pointer';
            card.addEventListener('click', function(e) {
                if (e.target.tagName !== 'A') {
                    window.location.href = link.href;
                }
            });
            card.setAttribute('tabindex', '0');
            card.addEventListener('keydown', function(e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    window.location.href = link.href;
                }
            });
        }
    });
});