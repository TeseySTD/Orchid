window.utils = {
    resizeObserve: (element, dotNetReference) => {
        let debounceTimer;
        let throttleTimer;

        const resizeObserver = new ResizeObserver((entries) => {
            if (!throttleTimer) {
                throttleTimer = requestAnimationFrame(() => {
                    // Call the C# method 'OnResize'
                    dotNetReference.invokeMethodAsync('OnResize');
                    throttleTimer = null;
                });
            }

            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
                // Call the C# method 'OnResizeEnd' after debounce
                dotNetReference.invokeMethodAsync('OnResizeEnd');
            }, 500); 
        });

        resizeObserver.observe(element);

        element._resizeObserver = resizeObserver;
        element._resizeCleanup = () => {
             clearTimeout(debounceTimer);
             cancelAnimationFrame(throttleTimer);
        };
    },

    resizeUnobserve: (element) => {
        if (element) {
            if (element._resizeCleanup) {
                element._resizeCleanup();
                delete element._resizeCleanup;
            }
            if (element._resizeObserver) {
                element._resizeObserver.disconnect();
                delete element._resizeObserver;
            }
        }
    }
};
