(function () {
    function extractChapterInfo(/** @type {Element} */ row) {
        let seriesLink = row.querySelector("td:nth-of-type(2) a");
        let chapterCell = row.querySelector("td:last-of-type");

        let chapterLink = chapterCell.querySelector("a");
        let readAt = null;
        if (chapterLink && chapterLink.textContent) {
            let lastNodeIndex = chapterCell.childNodes.length - 1;
            readAt = chapterCell.childNodes[lastNodeIndex].textContent;
        }

        return {
            series: seriesLink.textContent,
            lastReadChapterUrl: chapterLink && chapterLink.href,
            lastReadDate: readAt
        };
    }

    let lastReadChapters = [...document.querySelectorAll("h3.maintitle + .ipb_table tr:nth-of-type(2n+1)")];
    let lastReadInfos = lastReadChapters.map(extractChapterInfo);

    return JSON.stringify(lastReadInfos);
})();