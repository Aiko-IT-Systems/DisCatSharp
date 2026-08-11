(function ($) {
	"use strict";

	$(function () {
		if (!$.fn.featherlight) {
			return;
		}

		$(".catpunk-article img")
			.not("#logo, .catpunk-actions img, [data-disable-featherlight]")
			.each(function () {
				const image = $(this);
				const source = image.attr("src");

				if (!source || image.closest("a").length > 0) {
					return;
				}

				const description = image.attr("alt") || "image";
				image
					.css("cursor", "zoom-in")
					.attr({
						"aria-haspopup": "dialog",
						"aria-label": `Open ${description} at full size`,
						role: "button",
						tabindex: "0"
					})
					.featherlight(source)
					.on("keydown.dcsFeatherlight", function (event) {
						if (event.key === "Enter" || event.key === " ") {
							event.preventDefault();
							image.trigger("click");
						}
					});
			});
	});
})(window.jQuery);
