﻿(function () {
	function extractKeyValue(/** @type {Element} */ row) {
		let [keyCell, valueCell] = row.querySelectorAll("td");

		let key = keyCell.textContent.substring(0, keyCell.textContent.length - 1);
		let value = valueCell.textContent;

		if (key === "Alt Names") {
			value = JSON.stringify([...valueCell.querySelectorAll("span")].map(x => x.textContent.trim()));
		}

		if (key === "Author" || key === "Artist") {
			value = JSON.stringify([...valueCell.querySelectorAll("a")].map(x => x.textContent.trim()));
		}

		if (key === "Genres") {
			value = JSON.stringify([...valueCell.querySelectorAll(":scope > a span")].map(x => x.textContent.trim()));
		}

		if (key === "Description") {
			value = valueCell.innerHTML;
		}

		return {
			key: key,
			value: value
		};
	}

	let metaDataTableEntries = [...document.querySelectorAll(".ipsBox_withphoto + br + .ipsBox table tr")]
		.filter(x => x.querySelectorAll("td").length === 2);
	let metaData = metaDataTableEntries.map(extractKeyValue);

	function extractChapterInfo(/** @type {Element} */ row) {
		let cells = row.querySelectorAll("td");
		let [titleCell, languageCell, groupsCell, contributorCell, dateCell] = cells;

		let groupInfos = [...groupsCell.querySelectorAll("a")].map(x => ({ name: x.textContent, url: x.href}));

		return {
			title: titleCell.textContent.trim(),
			language: languageCell.querySelector("div").title,
			groups: groupInfos,
			contributor: contributorCell.textContent.trim(),
			date: dateCell.textContent.trim(),
			url: titleCell.querySelector("a").href
		};
	}

	let chapterTableEntries = [...document.querySelectorAll(".chapters_list tr.chapter_row")];
	let chapterInfos = chapterTableEntries.map(extractChapterInfo);

	let imageElement = /** @type {HTMLImageElement} */ (document.querySelector(".ipsBox_withphoto + br + .ipsBox > div > div > img"));

	let seriesInfo = {
		metaData: metaData,
		chapters: chapterInfos,
		image: imageElement && imageElement.src
	};

	return JSON.stringify(seriesInfo);
})();