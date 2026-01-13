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
    
    _getPointData: (x, y) => {
        if (typeof document.caretPositionFromPoint === 'function') {
            const pos = document.caretPositionFromPoint(x, y);
            if (pos) {
                return {node: pos.offsetNode, offset: pos.offset};
            }
        }

        if (typeof document.caretRangeFromPoint === 'function') {
            const range = document.caretRangeFromPoint(x, y);
            if (range) {
                return {node: range.startContainer, offset: range.startOffset};
            }
        }

        return null;
    },

    getCurrentLocator: (element) => {
        if (!element) return null;

        const containerRect = element.getBoundingClientRect();
        const scrollLeft = element.scrollLeft;
        const viewStart = scrollLeft; 
        
        const walker = document.createTreeWalker(
            element,
            NodeFilter.SHOW_TEXT | NodeFilter.SHOW_ELEMENT,
            {
                acceptNode: (node) => {
                    if (node.nodeType === Node.TEXT_NODE) {
                        return node.textContent.trim().length > 0 
                            ? NodeFilter.FILTER_ACCEPT 
                            : NodeFilter.FILTER_SKIP;
                    }
                    const tagName = node.tagName ? node.tagName.toUpperCase() : '';
                    if (tagName === 'IMG' || tagName === 'SVG') {

                        return NodeFilter.FILTER_ACCEPT;
                    }
                    // div, p, span, etc - skip but go inside
                    return NodeFilter.FILTER_SKIP;
                }
            }
        );

        let node = walker.nextNode();

        while (node) {
            let rect;
            
            if (node.nodeType === Node.TEXT_NODE) {
                const range = document.createRange();
                range.selectNodeContents(node);
                rect = range.getBoundingClientRect();
            } else {
                rect = node.getBoundingClientRect();
            }

            const nodeAbsLeft = (rect.left - containerRect.left) + scrollLeft;
            const nodeAbsRight = nodeAbsLeft + rect.width;

            // Skip what is already scrolled 
            if (nodeAbsRight <= viewStart) {
                node = walker.nextNode();
                continue;
            }

            const path = window.orchidReader._generateNodePath(element, node);

            // If element is cut
            if (nodeAbsLeft < viewStart) {
                // Picture logic - don't take the symbol offset, just take the beginning
                if (node.nodeType !== Node.TEXT_NODE) {
                    return `${path}:0`;
                }

                // Text logic 
                const text = node.textContent;
                const hiddenWidth = viewStart - nodeAbsLeft;
                const ratio = hiddenWidth / rect.width;
                const offset = Math.floor(text.length * ratio);
                const safeOffset = Math.max(0, Math.min(offset, text.length - 1));
                
                return `${path}:${safeOffset}`;
            }

            // If element starts of the current page
            if (nodeAbsLeft >= viewStart) {
                return `${path}:0`;
            }
            
            node = walker.nextNode();
        }

        return null;
    },

    scrollToLocator: (element, locator) => {
        if (!element || !locator) return 0;

        const parts = locator.split(':');
        const path = parts[0];
        const offset = parseInt(parts[1], 10);

        const node = window.orchidReader._getNodeFromPath(element, path);
        if (!node) {
            console.warn("Node not found for locator:", locator);
            return 0;
        }

        const range = document.createRange();
        try {
            if (node.nodeType === Node.TEXT_NODE) {
                const safeOffset = Math.min(offset, node.length);
                range.setStart(node, safeOffset);
                range.collapse(true);
            } else {
                range.selectNode(node);
            }
        } catch (e) {
            console.error("Error setting range:", e);
            return 0;
        }

        const rect = range.getBoundingClientRect();
        const containerRect = element.getBoundingClientRect();

        const absoluteLeft = (rect.left - containerRect.left) + element.scrollLeft;
        const pageWidth = containerRect.width;

        const pageIndex = Math.floor(absoluteLeft / pageWidth);

        window.orchidReader.scrollToPage(element, pageIndex);

        return pageIndex;
    },

    scrollToPage: (element, pageIndex) => {
        if (!element) return;
        const width = element.getBoundingClientRect().width;
        const currentPage = element.scrollLeft / width;

        console.debug("Try to scroll to page:", pageIndex);
        console.debug("Current page:", currentPage);
        if (currentPage === pageIndex) return;
        
        element.scrollTo({
            left: width * pageIndex,
            behavior: 'instant'
        });
        console.debug("Scrolled to page:", pageIndex);
    },

    _generateNodePath: (root, node) => {
        const path = [];
        let current = node;
        while (current && current !== root) {
            const parent = current.parentNode;
            if (!parent) break;

            const index = Array.prototype.indexOf.call(parent.childNodes, current);
            path.unshift(index);
            current = parent;
        }
        return path.join('/');
    },

    _getNodeFromPath: (root, pathString) => {
        if (!pathString) return null;
        const indices = pathString.split('/').map(Number);
        let current = root;

        for (const index of indices) {
            if (!current || !current.childNodes || current.childNodes.length <= index) {
                return null;
            }
            current = current.childNodes[index];
        }
        return current;
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
