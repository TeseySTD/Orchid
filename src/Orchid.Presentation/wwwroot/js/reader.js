window.orchidReader = {
    pageWidthTolerance: 20,

    getPages: async (element) => {
        if (!element) return new Blob(["[]"], {type: "application/json"});

        // Wait until all fonts will be ready
        await document.fonts.ready;

        // Wait until all pics will load
        const images = Array.from(element.querySelectorAll('img'));
        if (images.length > 0) {
            await Promise.all(images.map(img => {
                if (img.complete) return Promise.resolve();
                return new Promise(resolve => {
                    img.onload = img.onerror = () => resolve();
                });
            }));
        }

        return new Promise((resolve) => {
            requestAnimationFrame(() => {
                requestAnimationFrame(() => {
                    const containerRect = element.getBoundingClientRect();
                    const viewWidth = containerRect.width;
                    const style = window.getComputedStyle(element);
                    const columnGap = parseFloat(style.columnGap) || 0;
                    const columnStep = viewWidth + columnGap;

                    const pages = [];
                    let lastEndNode = null;
                    let lastEndOffset = 0;

                    const walker = document.createTreeWalker(
                        element,
                        NodeFilter.SHOW_TEXT | NodeFilter.SHOW_ELEMENT,
                        {
                            acceptNode: (node) => {
                                if (node.nodeType === Node.TEXT_NODE) {
                                    return node.textContent.trim().length > 0 ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_SKIP;
                                }
                                const tag = node.tagName?.toUpperCase();
                                if (tag === 'IMG' || tag === 'SVG' || tag === 'IMAGE') return NodeFilter.FILTER_ACCEPT;
                                return NodeFilter.FILTER_ACCEPT;
                            }
                        }
                    );

                    const count = Math.max(1, Math.ceil((element.scrollWidth + columnGap) / columnStep));

                    for (let i = 0; i < count; i++) {
                        const pageLeft = columnStep * i;
                        const pageRight = pageLeft + viewWidth;

                        let sNode = lastEndNode;
                        let sOff = lastEndOffset;

                        if (!sNode) {
                            walker.currentNode = element;
                            let firstNode = walker.nextNode();
                            while (firstNode) {
                                if (window.orchidReader._isNodeVisibleInRange(firstNode, pageLeft, pageRight, containerRect.left)) {
                                    sNode = firstNode;
                                    sOff = 0;
                                    break;
                                }
                                firstNode = walker.nextNode();
                            }
                        }

                        if (!sNode) break;

                        let eNode = sNode;
                        let eOff = sNode.nodeType === Node.TEXT_NODE ? sNode.textContent.length : 1;

                        walker.currentNode = sNode;
                        let node = sNode;
                        while (node) {
                            const rects = node.nodeType === Node.TEXT_NODE
                                ? window.orchidReader._getTextRects(node)
                                : node.getClientRects();

                            let intersects = false;
                            let strictlyAfter = true;

                            for (const r of Array.from(rects)) {
                                const absLeft = r.left - containerRect.left;
                                const absRight = absLeft + r.width;

                                if (absLeft < pageRight - 1) strictlyAfter = false;
                                if (absRight > pageLeft + 1 && absLeft < pageRight - 1) {
                                    intersects = true;
                                }
                            }

                            if (intersects) {
                                eNode = node;
                                if (node.nodeType === Node.TEXT_NODE) {
                                    const rawOff = window.orchidReader._findOffsetAtX(node, pageRight, containerRect.left);
                                    eOff = window.orchidReader._snapToWordBoundary(node, rawOff);
                                } else {
                                    eOff = 1;
                                }
                            }

                            if (strictlyAfter) break;
                            node = walker.nextNode();
                        }
                        
                        const startLocator = window.orchidReader._generateNodePath(element, sNode) + ":" + sOff;
                        let pageHtml = "";
                        try {
                            const range = document.createRange();

                            // For non text nodes set start before to catch it
                            if (sNode.nodeType === Node.TEXT_NODE) {
                                range.setStart(sNode, sOff);
                            } else {
                                range.setStartBefore(sNode);
                            }

                            if (eNode.nodeType === Node.TEXT_NODE) {
                                range.setEnd(eNode, eOff);
                            } else {
                                range.setEndAfter(eNode);
                            }

                            const fragment = range.cloneContents();

                            const wrappedFragment = window.orchidReader._wrapWithContext(fragment, range, element);

                            const div = document.createElement('div');
                            div.appendChild(wrappedFragment);
                            pageHtml = div.innerHTML;
                        } catch (e) {
                            console.error(`Page ${i} error:`, e);
                        }

                        pages.push({
                            html: pageHtml,
                            locator: startLocator,
                        });

                        lastEndNode = eNode;
                        lastEndOffset = eOff;

                        if (eNode.nodeType === Node.TEXT_NODE && eOff >= eNode.textContent.length) {
                            walker.currentNode = eNode;
                            if (!walker.nextNode()) break;
                        } else if (eNode.nodeType !== Node.TEXT_NODE) {
                            walker.currentNode = eNode;
                            let next = walker.nextNode();
                            if (!next) break;
                            lastEndNode = next;
                            lastEndOffset = 0;
                        }
                    }

                    resolve(new Blob([JSON.stringify(pages)], {type: "application/json"}));
                });
            });
        });
    },

    _wrapWithContext: (fragment, range, root) => {
        let ancestor = range.commonAncestorContainer;
        if (ancestor.nodeType === Node.TEXT_NODE) ancestor = ancestor.parentNode;

        let current = ancestor;
        let lastWrapper = fragment;

        while (current && current !== root) {
            const wrapper = current.cloneNode(false);
            wrapper.appendChild(lastWrapper);
            lastWrapper = wrapper;
            current = current.parentNode;
        }
        return lastWrapper;
    },


    _snapToWordBoundary: (node, offset) => {
        const text = node.textContent;
        if (offset <= 0 || offset >= text.length) return offset;
        const isBoundary = (char) => /\s|[\u2000-\u200B\u202F\u205F\u00A0]/.test(char);
        if (isBoundary(text[offset]) || isBoundary(text[offset - 1])) return offset;
        const textBefore = text.substring(0, offset);
        const lastSpace = Math.max(textBefore.lastIndexOf(' '), textBefore.lastIndexOf('\n'), textBefore.lastIndexOf('\t'));
        return lastSpace !== -1 ? lastSpace + 1 : offset;
    },

    _isNodeVisibleInRange: (node, left, right, containerLeft) => {
        const rects = node.nodeType === Node.TEXT_NODE ? window.orchidReader._getTextRects(node) : node.getClientRects();
        for (const r of Array.from(rects)) {
            const rLeft = r.left - containerLeft;
            if (rLeft < right - 1 && (rLeft + r.width) > left + 1) return true;
        }
        return false;
    },

    _getTextRects: (node) => {
        const range = document.createRange();
        range.selectNodeContents(node);
        return range.getClientRects();
    },

    _findOffsetAtX: (node, targetAbsX, containerLeft) => {
        const text = node.textContent;
        let low = 0, high = text.length;
        const range = document.createRange();
        while (low < high) {
            let mid = Math.floor((low + high) / 2);
            range.setStart(node, mid);
            range.setEnd(node, mid + 1);
            if (range.getBoundingClientRect().left - containerLeft < targetAbsX) low = mid + 1;
            else high = mid;
        }
        return low;
    },

    _getPointData: (x, y) => {
        if (typeof document.caretPositionFromPoint === 'function') {
            const pos = document.caretPositionFromPoint(x, y);
            if (pos) return {node: pos.offsetNode, offset: pos.offset};
        }

        if (typeof document.caretRangeFromPoint === 'function') {
            const range = document.caretRangeFromPoint(x, y);
            if (range) return {node: range.startContainer, offset: range.startOffset};
        }

        const el = document.elementFromPoint(x, y);
        if (el) return {node: el, offset: 0};

        return null;
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

    measureHiddenChapter: async (element, contentStreamReference) => {
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

        try {
            const stream = await contentStreamReference.stream();
            sandbox.innerHTML = await new Response(stream).text();

            return await window.orchidReader.getPages(sandbox);
        } catch (error) {
            console.error("Error reading chapter stream:", error);
            return new Blob(["[]"], {type: "application/json"});
        }
    },


    cleanupSandbox: () => {
        if (window.orchidReader._sandbox) {
            window.orchidReader._sandbox.remove();
            window.orchidReader._sandbox = null;
        }
    }
};
