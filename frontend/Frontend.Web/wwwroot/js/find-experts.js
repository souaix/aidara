(function () {
    const list = document.getElementById('find-cat-list');
    const container = document.getElementById('tiles-container');
    if (!list || !container) return;

    list.addEventListener('click', async (e) => {
        const a = e.target.closest('a.find-link');
        if (!a) return;
        e.preventDefault();

        // 切換 active
        [...list.querySelectorAll('a.find-link')].forEach(x => x.classList.remove('active'));
        a.classList.add('active');

        const cat = a.dataset.cat || 'interior';
        // 顯示載入中
        container.innerHTML = `<div class="text-center p-4 text-muted">載入中…</div>`;
        try {
            const resp = await fetch(`/Browse/CategoryTiles?cat=${encodeURIComponent(cat)}`, { cache: 'no-store' });
            container.innerHTML = await resp.text();
        } catch (err) {
            container.innerHTML = `<div class="alert alert-danger">讀取失敗，請稍後重試。</div>`;
            console.error(err);
        }
    });
})();
