// === Bootstrap Alert 全域工具 ===
// type: 'success' | 'danger' | 'warning' | 'info'
window.showBootstrapAlert = function (type, message) {
    let container = document.getElementById('globalAlertContainer');

    // 頁面若沒有放 container，則動態建立一個
    if (!container) {
        container = document.createElement('div');
        container.id = 'globalAlertContainer';
        container.className =
            "position-fixed top-0 start-50 translate-middle-x p-3";
        container.style.cssText = "z-index:1080;min-width:320px;max-width:480px;";
        document.body.appendChild(container);
    }

    const wrapper = document.createElement('div');
    wrapper.innerHTML = `
        <div class="alert alert-${type} alert-dismissible fade show shadow"
             role="alert" style="margin-bottom: 0.5rem;">
            ${message}
            <button type="button" class="btn-close"
                    data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

    const alertEl = wrapper.firstElementChild;
    container.appendChild(alertEl);

    // 自動在 4 秒後關閉
    setTimeout(() => {
        if (!alertEl) return;
        const bsAlert = bootstrap.Alert.getOrCreateInstance(alertEl);
        bsAlert.close();
    }, 4000);
};

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}