window.orchidReader = {
    pageWidthTolerance: 20,

    getPageCount: async (element) => {
        if (!element) return 0;

        // Wait until all fonts will be ready
        await document.fonts.ready;

        // Wait until all pics will load
        const images = Array.from(element.querySelectorAll('img'));
        if (images.length > 0) {
            await Promise.all(images.map(img => {
                if (img.complete) return Promise.resolve();
                return new Promise(resolve => {
                    img.onload = () => resolve();
                    img.onerror = () => resolve();
                });
            }));
        }

        return new Promise((resolve) => {
            // First rAF - wait until current frame will be completed
            requestAnimationFrame(() => {
                // Second rAF - wait for next frame
                requestAnimationFrame(() => {
                    const width = element.getBoundingClientRect().width;
                    if (width === 0) {
                        resolve(0);
                        return;
                    }

                    const scrollWidth = element.scrollWidth;

                    const count = Math.ceil((scrollWidth - window.orchidReader.pageWidthTolerance) / width);

                    resolve(Math.max(1, count));
                });
            });
        });
    },

    scrollToPage: (element, pageIndex) => {
        if (!element) return;
        const width = element.getBoundingClientRect().width;

        element.scrollTo({
            left: width * pageIndex,
            behavior: 'instant'
        });
    },

    _sandbox: null,

    measureHiddenChapter: async (element, htmlContent) => {
        if (!element) {
            console.log("Chapter content element not found.");
            return 0;
        }
        if (!window.orchidReader._sandbox) {
            console.log("Make new sandbox")
            const sandbox = element.cloneNode(false);

            sandbox.style.cssText = `
                opacity: 0;
                z-index: -1000;
                pointer-events: none;
                position: absolute; 
                top: 0;
                left: 0;
                visibility: hidden; 
                margin: 0;
                padding: 0;
            `;
            sandbox.removeAttribute('id');

            element.parentElement.appendChild(sandbox);
            window.orchidReader._sandbox = sandbox;
            console.log("Created new sandbox");
        }

        const sandbox = window.orchidReader._sandbox;
        const rect = element.getBoundingClientRect();
        sandbox.style.width = `${rect.width}px`;
        sandbox.style.height = `${rect.height}px`;
        sandbox.innerHTML = htmlContent;
        return await window.orchidReader.getPageCount(sandbox);
    },

    cleanupSandbox: () => {
        if (window.orchidReader._sandbox) {
            window.orchidReader._sandbox.remove();
            window.orchidReader._sandbox = null;
        }
    }
};
