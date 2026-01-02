window.utils = {
    resizeObserve: (element, dotNetReference) => {
        const resizeObserver = new ResizeObserver((entries) => {
            for (let entry of entries) {
                // Call the C# method 'OnResize'
                dotNetReference.invokeMethodAsync('OnResize');
            }
        });

        resizeObserver.observe(element);

        // Store the observer on the element to disconnect later
        element._resizeObserver = resizeObserver;
    },
    resizeUnobserve: (element) => {
        if (element && element._resizeObserver) {
            element._resizeObserver.disconnect();
            delete element._resizeObserver;
        }
    }
}