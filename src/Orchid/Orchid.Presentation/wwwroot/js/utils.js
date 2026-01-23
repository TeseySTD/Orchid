window.utils = {
    getPaginationContext: (element) => {
        if (!element) return null;

        const style = window.getComputedStyle(element);
        const rect = element.getBoundingClientRect();

        const parsePx = (val) => {
            const floatVal = parseFloat(val);
            return isNaN(floatVal) ? 0 : floatVal;
        };

        let lh = parsePx(style.lineHeight);
        if (style.lineHeight === 'normal') {
            lh = parsePx(style.fontSize) * 1.2;
        }

        return {
            width: rect.width,
            height: rect.height,
            fontSize: parsePx(style.fontSize),
            fontFamily: style.fontFamily.replace(/['"]/g, ''),
            lineHeight: lh
        };
    },

    pageContextObserve: (element, dotNetReference) => {
        if (!element) return;

        let lastContext = window.utils.getPaginationContext(element);
        let lastClass = element.getAttribute('class');
        let debounceTimer;
        let throttleTimer;
        let isFirstResizeObserverCall = true;

        const triggerChange = () => {
            if (!hasContextChanged()) return;

            if (!throttleTimer) {
                throttleTimer = requestAnimationFrame(() => {
                    dotNetReference.invokeMethodAsync('OnPageContextChange');
                    throttleTimer = null;
                });
            }

            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                dotNetReference.invokeMethodAsync('OnPageContextChangeEnd');
            }, 500);
        };

        const hasContextChanged = () => {
            const newContext = window.utils.getPaginationContext(element);
            const newClass = element.getAttribute('class');
            if (!lastContext || !newContext) return false;

            const isDifferent =
                newContext.width !== lastContext.width ||
                newContext.height !== lastContext.height ||
                newContext.fontSize !== lastContext.fontSize ||
                newContext.lineHeight !== lastContext.lineHeight ||
                newContext.fontFamily !== lastContext.fontFamily ||
                lastClass !== newClass;

            if (isDifferent) {
                lastContext = newContext;
                lastClass = newClass; 
                return true;
            }
            return false;
        };

        const resizeObserver = new ResizeObserver(() => {
            if (isFirstResizeObserverCall) {
                isFirstResizeObserverCall = false;
                return;
            }
            triggerChange();
        });
        resizeObserver.observe(element);

        const mutationObserver = new MutationObserver(() => {
            triggerChange();
        });
        mutationObserver.observe(element, {
            attributes: true,
            attributeFilter: ['style', 'class']
        });

        element._pageContextCleanup = () => {
            clearTimeout(debounceTimer);
            cancelAnimationFrame(throttleTimer);
            resizeObserver.disconnect();
            mutationObserver.disconnect();
        };
    },

    pageContextUnobserve: (element) => {
        if (element && element._pageContextCleanup) {
            element._pageContextCleanup();
            delete element._pageContextCleanup;
        }
    },
    
    cleanupElementContent: (element) => {
        if (!element) return;

        console.log("Cleaning element content:", element, "...");

        const images = element.querySelectorAll('img');
        images.forEach(img => {
            img.src = ''; // Tear the connection with data in memory 
            img.remove();
        });

        element.innerHTML = '';
        console.log("Element content is cleaned");
    }
};
