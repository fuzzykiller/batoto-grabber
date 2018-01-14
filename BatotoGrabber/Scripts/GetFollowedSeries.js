(function () {
	let links = /** @type {HTMLAnchorElement[]} */ ([...document.querySelectorAll("#categories ~ h3.maintitle ~ div a")]);

	let series = links.map(x => ({ name: x.textContent.trim(), url: x.href }));
	//window.grabberService.postFollowedSeries(JSON.stringify(series));

	return JSON.stringify(series);
})();