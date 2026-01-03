window.orchidReader = {
    getPreciseWidth: (element) => {
        if (!element) return 0;
        return element.getBoundingClientRect().width;
    },

    getPreciseHeight: (element) => {
        if (!element) return 0;
        return element.getBoundingClientRect().height;
    },

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

    measureHiddenChapter: (element, htmlContent, cssContent) => {
        const sandbox = document.createElement('div');
        const containerWidth = window.orchidReader.getPreciseWidth(element);
        const containerHeight = window.orchidReader.getPreciseHeight(element);
        sandbox.style.position = 'absolute';
        sandbox.style.visibility = 'hidden';
        sandbox.style.top = '-10000px';
        sandbox.style.left = '-10000px';
        sandbox.style.width = `${containerWidth}px`;
        sandbox.style.height = `${containerHeight}px`;

        sandbox.style.columnWidth = `${containerWidth}px`;
        sandbox.style.columnGap = '0';
        sandbox.style.columnFill = 'auto';
        sandbox.style.overflow = 'hidden';
        sandbox.style.wordBreak = 'break-word';
        sandbox.className = "chapter-content"

        const styleTag = document.createElement('style');
        styleTag.textContent = cssContent;
        sandbox.appendChild(styleTag);

        const contentDiv = document.createElement('div');
        contentDiv.innerHTML = htmlContent;
        sandbox.appendChild(contentDiv);

        document.body.appendChild(sandbox);

        const pageCount = Math.ceil(sandbox.scrollWidth / containerWidth);

        document.body.removeChild(sandbox);

        return pageCount;
    },
};
