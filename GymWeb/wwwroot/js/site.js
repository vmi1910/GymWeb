// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// ==== Toast notifications (thay cho alert-success/alert-danger mặc định) ====
(function () {
    function hideToast(el) {
        el.classList.add('is-hiding');
        setTimeout(function () { el.remove(); }, 300);
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('.app-toast').forEach(function (toast) {
            var timer = setTimeout(function () { hideToast(toast); }, 4500);

            var closeBtn = toast.querySelector('.toast-close');
            if (closeBtn) {
                closeBtn.addEventListener('click', function () {
                    clearTimeout(timer);
                    hideToast(toast);
                });
            }
        });
    });
})();

// ==== Confirm modal (thay cho window.confirm() mặc định của trình duyệt) ====
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        var modalEl = document.getElementById('appConfirmModal');
        if (!modalEl || typeof bootstrap === 'undefined') return;

        var modal = new bootstrap.Modal(modalEl);
        var titleEl = document.getElementById('appConfirmTitle');
        var messageEl = document.getElementById('appConfirmMessage');
        var okBtn = document.getElementById('appConfirmOkBtn');
        var pendingForm = null;

        document.querySelectorAll('form[data-confirm]').forEach(function (form) {
            form.addEventListener('submit', function (e) {
                if (form.dataset.confirmed === 'true') return;
                e.preventDefault();
                pendingForm = form;
                titleEl.textContent = form.dataset.confirmTitle || 'Xác nhận thao tác';
                messageEl.textContent = form.dataset.confirm;
                modal.show();
            });
        });

        okBtn.addEventListener('click', function () {
            modal.hide();
            if (pendingForm) {
                pendingForm.dataset.confirmed = 'true';
                pendingForm.submit();
                pendingForm = null;
            }
        });
    });
})();
