(function () {
	function extractKeyValue(/** @type {Element} */ row) {
		let keyCell = row.querySelector("td:first-of-type");
		let valueCell = row.querySelector("td:last-of-type");

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
		.filter(x => x.querySelectorAll("td").length == 2);
	let metaData = metaDataTableEntries.map(extractKeyValue);

	function extractChapterInfo(/** @type {Element} */ row) {
		let cells = row.querySelectorAll("td");
		let titleCell = cells[0];
		let languageCell = cells[1];
		let groupsCell = cells[2];
		let contributorCell = cells[3];
		let dateCell = cells[4];

		let groupInfos = [...groupsCell.querySelectorAll("a")].map(x => ({ name: x.textContent, url: x.href}));

		return {
			title: titleCell.textContent.trim(),
			language: languageCell.querySelector("div").title,
			groups: groupInfos,
			contributorCell: contributorCell.textContent.trim(),
			date: dateCell.textContent.trim()
		};
	}

	let chapterTableEntries = [...document.querySelectorAll(".chapters_list tr.chapter_row")];
	let chapterInfos = chapterTableEntries.map(extractChapterInfo);

	let imageElement = /** @type {HTMLImageElement} */ (document.querySelector(".ipsBox_withphoto ~ .ipsBox > div > div > img"));

	let seriesInfo = {
		metaData: metaData,
		chapters: chapterInfos,
		image: imageElement.src
	};

	return JSON.stringify(seriesInfo);
})();