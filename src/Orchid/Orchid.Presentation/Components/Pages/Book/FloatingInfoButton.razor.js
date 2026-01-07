let timer;
let events = ['mousemove', 'mousedown', 'touchstart', 'keydown', 'scroll'];

export function init(element, timeoutMs) {
    if (!element) return;

    const show = () => {
        element.style.opacity = '1';
        element.style.pointerEvents = 'auto';
        
        clearTimeout(timer);
        
        timer = setTimeout(() => {
            element.style.opacity = '0';
            element.style.pointerEvents = 'none';
        }, timeoutMs);
    };

    element._activityHandler = show;

    events.forEach(event => window.addEventListener(event, show));
    
    show();
}

export function dispose(element) {
    if (element && element._activityHandler) {
        events.forEach(event => window.removeEventListener(event, element._activityHandler));
        clearTimeout(timer);
        delete element._activityHandler;
    }
}
