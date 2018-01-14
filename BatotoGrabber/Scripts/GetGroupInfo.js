﻿(function () {
    function extractKeyValue(/** @type {Element} */ row) {
        let keyCell = row.querySelector("td:first-of-type");
        let valueCell = row.querySelector("td:last-of-type");

        let key = keyCell.textContent.substring(0, keyCell.textContent.length - 1);
        let value = valueCell.textContent;

        if (key === "Description") {
            value = valueCell.innerHTML;
        }

        return {
            key: key,
            value: value
        };
    }

    let metaDataTableEntries = [...document.querySelectorAll(".ipsBox_withphoto + br + .ipsBox table tr")]
        .filter(x => x.querySelectorAll("td").length == 2);
    let metaData = metaDataTableEntries.map(extractKeyValue);

    return JSON.stringify(metaData);
})();