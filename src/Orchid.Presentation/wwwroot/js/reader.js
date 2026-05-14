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
                                const center = absLeft + (r.width / 2);

                                if (center < pageRight) strictlyAfter = false;
                                if (center >= pageLeft && center < pageRight) {
                                    intersects = true;
                                }
                            }

                            if (intersects) {
                                eNode = node;
                                if (node.nodeType === Node.TEXT_NODE) {
                                    const startSearchOff = (node === sNode) ? sOff : 0;
                                    eOff = window.orchidReader._findOffsetAtX(node, pageRight, containerRect.left, startSearchOff);
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

                            const nodePosition = sNode.compareDocumentPosition(eNode);
                            if (nodePosition & Node.DOCUMENT_POSITION_PRECEDING) {
                                eNode = sNode;
                                eOff = sNode.nodeType === Node.TEXT_NODE ? sNode.textContent.length : 1;
                            } else if (sNode === eNode && eOff < sOff) {
                                eOff = sOff;
                            }

                            if (eNode.nodeType === Node.TEXT_NODE) {
                                range.setEnd(eNode, eOff);
                            } else {
                                range.setEndAfter(eNode);
                            }

                            const fragment = range.cloneContents();

                            const wrappedFragment = window.orchidReader._wrapWithContext(fragment, range, element);

                            // Strip top margins to prevent text push-down on fractured elements
                            if (i > 0) {
                                let curr = wrappedFragment.nodeType === Node.DOCUMENT_FRAGMENT_NODE ? wrappedFragment.firstElementChild : wrappedFragment;
                                while (curr && curr.nodeType === Node.ELEMENT_NODE) {
                                    curr.style.setProperty('margin-top', '0px', 'important');
                                    curr.style.setProperty('padding-top', '0px', 'important');
                                    curr.style.setProperty('text-indent', '0px', 'important');
                                    curr = curr.firstElementChild;
                                }
                            }

                            // Strip bottom margins to prevent premature overflow 
                            if (i < count - 1) {
                                let curr = wrappedFragment.nodeType === Node.DOCUMENT_FRAGMENT_NODE ? wrappedFragment.lastElementChild : wrappedFragment;
                                while (curr && curr.nodeType === Node.ELEMENT_NODE) {
                                    curr.style.setProperty('margin-bottom', '0px', 'important');
                                    curr.style.setProperty('padding-bottom', '0px', 'important');
                                    curr = curr.lastElementChild;
                                }
                            }

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

                        if (eNode.nodeType === Node.TEXT_NODE && eOff >= eNode.textContent.length) {
                            walker.currentNode = eNode;
                            let next = walker.nextNode();
                            if (!next) break;
                            lastEndNode = next;
                            lastEndOffset = 0;
                        } else if (eNode.nodeType !== Node.TEXT_NODE) {
                            walker.currentNode = eNode;
                            let next = walker.nextNode();
                            if (!next) break;
                            lastEndNode = next;
                            lastEndOffset = 0;
                        } else {
                            lastEndNode = eNode;
                            lastEndOffset = eOff;
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

    _isNodeVisibleInRange: (node, left, right, containerLeft) => {
        const rects = node.nodeType === Node.TEXT_NODE ? window.orchidReader._getTextRects(node) : node.getClientRects();
        for (const r of Array.from(rects)) {
            const center = r.left - containerLeft + (r.width / 2);
            if (center >= left && center < right) return true;
        }
        return false;
    },

    _getTextRects: (node) => {
        const range = document.createRange();
        range.selectNodeContents(node);
        return range.getClientRects();
    },

    _findOffsetAtX: (node, pageRight, containerLeft, startOffset = 0) => {
        const text = node.textContent;
        const range = document.createRange();
        
        for (let i = startOffset; i < text.length; i++) {
            range.setStart(node, i);
            range.setEnd(node, i + 1);
            const rects = range.getClientRects();
            
            if (rects.length === 0) continue; 
            
            const rect = rects[0];
            const center = rect.left - containerLeft + (rect.width / 2);
            
            if (center >= pageRight) {
                return i;
            }
        }
        return text.length;
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
