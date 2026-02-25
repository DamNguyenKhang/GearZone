/**
 * Toast Notification System for GearZone
 * Usage: toast.success('Message'), toast.error('Message'), etc.
 */
class ToastManager {
    constructor() {
        this.containerId = 'toast-container';
        this._ensureContainerExists();
    }

    _ensureContainerExists() {
        if (!document.getElementById(this.containerId)) {
            const container = document.createElement('div');
            container.id = this.containerId;
            container.className = 'fixed top-6 right-6 z-[9999] flex flex-col gap-4 max-w-md w-full sm:w-80';
            document.body.appendChild(container);
        }
    }

    show(message, type = 'info', duration = 5000) {
        const container = document.getElementById(this.containerId);
        
        const toast = document.createElement('div');
        // Improved styles: tinted background, better shadows, and padding
        toast.className = `transform translate-x-full opacity-0 transition-all duration-500 cubic-bezier(0.16, 1, 0.3, 1) 
                           flex items-center p-4 pr-3 border-l-4 rounded-xl shadow-2xl backdrop-blur-sm
                           ${this._getTypeStyles(type)}`;
        
        const icon = this._getIcon(type);
        
        toast.innerHTML = `
            <div class="flex-shrink-0 mr-3.5">
                <div class="p-1.5 rounded-full ${this._getIconBg(type)}">
                    <span class="material-symbols-outlined text-[22px] block ${this._getIconColor(type)}">${icon}</span>
                </div>
            </div>
            <div class="flex-grow mr-4 text-[14.5px] font-semibold leading-snug">
                ${message}
            </div>
            <button class="flex-shrink-0 p-1.5 rounded-lg hover:bg-black/5 dark:hover:bg-white/10 text-gray-400 hover:text-gray-600 transition-all active:scale-95">
                <span class="material-symbols-outlined text-[18px] block">close</span>
            </button>
        `;

        container.appendChild(toast);

        // Trigger animate in
        requestAnimationFrame(() => {
            toast.classList.remove('translate-x-full', 'opacity-0');
            toast.classList.add('translate-x-0', 'opacity-100');
        });

        const closeBtn = toast.querySelector('button');
        const removeToast = () => {
            if (!toast.parentNode) return;
            toast.classList.remove('translate-x-0', 'opacity-100');
            toast.classList.add('translate-x-full', 'opacity-0');
            toast.style.marginTop = `-${toast.offsetHeight}px`;
            toast.style.marginBottom = '0';
            toast.style.pointerEvents = 'none';
            setTimeout(() => toast.remove(), 500);
        };

        closeBtn.onclick = removeToast;

        if (duration > 0) {
            setTimeout(removeToast, duration);
        }
    }

    success(message) { this.show(message, 'success'); }
    error(message) { this.show(message, 'error', 7000); }
    warning(message) { this.show(message, 'warning'); }
    info(message) { this.show(message, 'info'); }

    _getTypeStyles(type) {
        const styles = {
            success: 'bg-emerald-50 border-emerald-500 text-emerald-900',
            error: 'bg-rose-50 border-rose-500 text-rose-900',
            warning: 'bg-amber-50 border-amber-500 text-amber-900',
            info: 'bg-sky-50 border-sky-500 text-sky-900'
        };
        return styles[type] || styles.info;
    }

    _getIcon(type) {
        const icons = {
            success: 'check_circle',
            error: 'error',
            warning: 'warning',
            info: 'info'
        };
        return icons[type] || icons.info;
    }

    _getIconColor(type) {
        const colors = {
            success: 'text-emerald-500',
            error: 'text-rose-500',
            warning: 'text-amber-500',
            info: 'text-sky-500'
        };
        return colors[type] || colors.info;
    }

    _getIconBg(type) {
        const bgs = {
            success: 'bg-emerald-100/50',
            error: 'bg-rose-100/50',
            warning: 'bg-amber-100/50',
            info: 'bg-sky-100/50'
        };
        return bgs[type] || bgs.info;
    }
}

const toast = new ToastManager();
window.toast = toast;
