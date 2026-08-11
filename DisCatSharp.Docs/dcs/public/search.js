const DEBOUNCE_MS = 350;
const MIN_QUERY_LENGTH = 2;

export function initializeDocumentationSearch(root = document) {
	const form = root.querySelector("#dcs-search");
	const input = root.querySelector("#dcs-search-query");
	const container = root.querySelector("#dcs-search-results");
	const list = root.querySelector("#dcs-search-list");
	const status = root.querySelector("#dcs-search-status");
	const endpoint = root.querySelector('meta[name="dcs-search-endpoint"]')?.content || "/_search";
	if (!form || !input || !container || !list || !status) return null;

	let timer;
	let controller;
	let sequence = 0;
	let activeIndex = -1;
	let activeType = "";
	let results = [];
	input.disabled = false;

	const setSearchVisible = (visible) => {
		container.hidden = !visible;
		document.body.toggleAttribute("data-dcs-search", visible);
		input.setAttribute("aria-expanded", String(visible));
	};

	const setActiveResult = (index) => {
		const options = [...list.querySelectorAll('[role="option"]')];
		for (const option of options) option.setAttribute("aria-selected", "false");
		if (options.length === 0) {
			activeIndex = -1;
			input.removeAttribute("aria-activedescendant");
			return;
		}
		activeIndex = Math.max(0, Math.min(index, options.length - 1));
		const active = options[activeIndex];
		active.setAttribute("aria-selected", "true");
		input.setAttribute("aria-activedescendant", active.id);
		active.scrollIntoView({ block: "nearest" });
	};

	const render = (items) => {
		results = items;
		activeIndex = -1;
		input.removeAttribute("aria-activedescendant");
		list.replaceChildren();
		if (items.length === 0) {
			status.textContent = "No documentation matched your search.";
			return;
		}

		status.textContent = `${items.length} result${items.length === 1 ? "" : "s"}`;
		items.forEach((item, index) => {
			const link = document.createElement("a");
			link.id = `dcs-search-result-${index}`;
			link.className = "dcs-search-result";
			link.href = item.url;
			link.setAttribute("role", "option");
			link.setAttribute("aria-selected", "false");

			const heading = document.createElement("span");
			heading.className = "dcs-search-result-title";
			heading.textContent = item.title;
			const badge = document.createElement("span");
			badge.className = "dcs-search-result-badge";
			badge.textContent = item.type;
			const summary = document.createElement("span");
			summary.className = "dcs-search-result-summary";
			summary.textContent = item.summary || item.module || "DisCatSharp documentation";
			link.append(heading, badge, summary);
			list.append(link);
		});
	};

	const showFailure = () => {
		results = [];
		list.replaceChildren();
		status.textContent = "Search is taking a catnap. Please try again.";
		const retry = document.createElement("button");
		retry.type = "button";
		retry.className = "dcs-search-retry";
		retry.textContent = "Retry";
		retry.addEventListener("click", () => runSearch());
		list.append(retry);
	};

	const runSearch = async () => {
		const query = input.value.trim();
		if (query.length < MIN_QUERY_LENGTH) {
			controller?.abort();
			sequence++;
			list.replaceChildren();
			status.textContent = "";
			setSearchVisible(false);
			return;
		}

		controller?.abort();
		controller = new AbortController();
		const requestSequence = ++sequence;
		setSearchVisible(true);
		status.textContent = "Searching…";
		list.replaceChildren();
		try {
			const url = new URL(endpoint, window.location.href);
			url.searchParams.set("q", query);
			if (activeType) url.searchParams.set("type", activeType);
			const response = await fetch(url, { signal: controller.signal, headers: { Accept: "application/json" } });
			if (!response.ok) throw new Error(`Search failed with ${response.status}`);
			const payload = await response.json();
			if (requestSequence !== sequence) return;
			render(Array.isArray(payload.results) ? payload.results : []);
		} catch (error) {
			if (error?.name === "AbortError" || requestSequence !== sequence) return;
			showFailure();
		}
	};

	const schedule = () => {
		clearTimeout(timer);
		timer = setTimeout(runSearch, DEBOUNCE_MS);
	};

	form.addEventListener("submit", (event) => {
		event.preventDefault();
		clearTimeout(timer);
		runSearch();
	});
	input.addEventListener("input", schedule);
	input.addEventListener("keydown", (event) => {
		if (event.key === "ArrowDown") {
			event.preventDefault();
			setActiveResult(activeIndex + 1);
		} else if (event.key === "ArrowUp") {
			event.preventDefault();
			setActiveResult(activeIndex <= 0 ? results.length - 1 : activeIndex - 1);
		} else if (event.key === "Enter" && activeIndex >= 0) {
			event.preventDefault();
			list.querySelectorAll('[role="option"]')[activeIndex]?.click();
		} else if (event.key === "Escape") {
			input.value = "";
			runSearch();
			input.focus();
		}
	});

	for (const button of container.querySelectorAll("[data-search-type]")) {
		button.addEventListener("click", () => {
			activeType = button.dataset.searchType || "";
			for (const candidate of container.querySelectorAll("[data-search-type]")) {
				const selected = candidate === button;
				candidate.classList.toggle("active", selected);
				candidate.setAttribute("aria-pressed", String(selected));
			}
			runSearch();
		});
	}

	return { runSearch, get activeType() { return activeType; } };
}

if (typeof document !== "undefined") {
	if (document.readyState === "loading") {
		document.addEventListener("DOMContentLoaded", () => initializeDocumentationSearch(), { once: true });
	} else {
		initializeDocumentationSearch();
	}
}
