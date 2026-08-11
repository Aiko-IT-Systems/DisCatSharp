import { beforeEach, describe, expect, it, vi } from "vitest";
import { JSDOM } from "jsdom";
import { initializeDocumentationSearch } from "../../dcs/public/search.js";

function mount() {
	const dom = new JSDOM("<!doctype html><html><head></head><body></body></html>", { url: "https://docs.dcs.aitsys.dev/" });
	globalThis.window = dom.window;
	globalThis.document = dom.window.document;
	globalThis.Element = dom.window.Element;
	globalThis.Event = dom.window.Event;
	globalThis.KeyboardEvent = dom.window.KeyboardEvent;
	document.head.innerHTML = '<meta name="dcs-search-endpoint" content="/_search">';
	document.body.innerHTML = `
		<form id="dcs-search"><input id="dcs-search-query" disabled></form>
		<section id="dcs-search-results" hidden>
			<button data-search-type="">All</button>
			<button data-search-type="conceptual">Articles</button>
			<button data-search-type="changelog">Changelogs</button>
			<p id="dcs-search-status"></p>
			<div id="dcs-search-list"></div>
		</section>`;
	Element.prototype.scrollIntoView = vi.fn();
	return initializeDocumentationSearch(document);
}

function ok(results = []) {
	return Promise.resolve({ ok: true, json: () => Promise.resolve({ results }) });
}

describe("documentation search UI", () => {
	beforeEach(() => {
		vi.useRealTimers();
		vi.restoreAllMocks();
	});

	it("maps the Articles chip to the conceptual family alias", async () => {
		const fetchMock = vi.spyOn(globalThis, "fetch").mockImplementation(() => ok());
		const search = mount();
		document.querySelector("#dcs-search-query").value = "voice setup";
		document.querySelector('[data-search-type="conceptual"]').click();
		await vi.waitFor(() => expect(fetchMock).toHaveBeenCalledOnce());
		const url = new URL(fetchMock.mock.calls[0][0]);
		expect(url.searchParams.get("type")).toBe("conceptual");
		expect(search.activeType).toBe("conceptual");
	});

	it("maps the Changelogs chip to the literal changelog kind", async () => {
		const fetchMock = vi.spyOn(globalThis, "fetch").mockImplementation(() => ok());
		const search = mount();
		document.querySelector("#dcs-search-query").value = "release notes";
		document.querySelector('[data-search-type="changelog"]').click();
		await vi.waitFor(() => expect(fetchMock).toHaveBeenCalledOnce());
		const url = new URL(fetchMock.mock.calls[0][0]);
		expect(url.searchParams.get("type")).toBe("changelog");
		expect(search.activeType).toBe("changelog");
	});

	it("renders a stable document ID using kind only as display metadata", async () => {
		vi.spyOn(globalThis, "fetch").mockImplementation(() => ok([
			{ id: "document:release-notes", family: "conceptual", type: "changelog", title: "Release Notes", summary: "Changes", url: "/release-notes.html", module: null, score: 400 },
		]));
		const search = mount();
		document.querySelector("#dcs-search-query").value = "release notes";
		await search.runSearch();

		const result = document.querySelector(".dcs-search-result");
		expect(result.getAttribute("href")).toBe("/release-notes.html");
		expect(result.querySelector(".dcs-search-result-title").textContent).toBe("Release Notes");
		expect(result.querySelector(".dcs-search-result-badge").textContent).toBe("changelog");
	});

	it("debounces input and never requests fewer than two characters", async () => {
		vi.useFakeTimers();
		const fetchMock = vi.spyOn(globalThis, "fetch").mockImplementation(() => ok());
		mount();
		const input = document.querySelector("#dcs-search-query");
		input.value = "D";
		input.dispatchEvent(new Event("input"));
        await vi.advanceTimersByTimeAsync(350);
		expect(fetchMock).not.toHaveBeenCalled();
		input.value = "DiscordGuild";
		input.dispatchEvent(new Event("input"));
		input.dispatchEvent(new Event("input"));
        await vi.advanceTimersByTimeAsync(349);
		expect(fetchMock).not.toHaveBeenCalled();
		await vi.advanceTimersByTimeAsync(1);
		expect(fetchMock).toHaveBeenCalledOnce();
	});

	it("ignores a stale response and exposes keyboard selection through ARIA", async () => {
		let resolveFirst;
		const first = new Promise((resolve) => { resolveFirst = resolve; });
		const fetchMock = vi.spyOn(globalThis, "fetch")
			.mockImplementationOnce(() => first)
			.mockImplementationOnce(() => ok([{ id: "symbol:New", type: "class", title: "New", summary: "new", url: "/new", module: "DisCatSharp", score: 1 }]));
		const search = mount();
		const input = document.querySelector("#dcs-search-query");
		input.value = "old query";
		const oldRequest = search.runSearch();
		input.value = "new query";
		await search.runSearch();
		resolveFirst(await ok([{ id: "symbol:Old", type: "class", title: "Old", summary: "old", url: "/old", module: "DisCatSharp", score: 2 }]));
		await oldRequest;
		expect(fetchMock).toHaveBeenCalledTimes(2);
		expect(document.querySelector("#dcs-search-list").textContent).toContain("New");
		expect(document.querySelector("#dcs-search-list").textContent).not.toContain("Old");
		input.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowDown", bubbles: true }));
		expect(input.getAttribute("aria-activedescendant")).toBe("dcs-search-result-0");
		expect(document.querySelector("#dcs-search-result-0").getAttribute("aria-selected")).toBe("true");
	});

	it("renders loading, empty, and retryable failure states", async () => {
		let resolveSearch;
		const pendingResponse = new Promise((resolve) => { resolveSearch = resolve; });
		const fetchMock = vi.spyOn(globalThis, "fetch")
			.mockImplementationOnce(() => pendingResponse)
			.mockResolvedValueOnce({ ok: false, status: 500 })
			.mockImplementationOnce(() => ok());
		const search = mount();
		const input = document.querySelector("#dcs-search-query");
		input.value = "nothing";
		const pendingSearch = search.runSearch();
		expect(document.querySelector("#dcs-search-status").textContent).toBe("Searching…");
		resolveSearch(await ok());
		await pendingSearch;
		expect(document.querySelector("#dcs-search-status").textContent).toContain("No documentation matched");

		await search.runSearch();
		expect(document.querySelector("#dcs-search-status").textContent).toContain("catnap");
		const retry = document.querySelector(".dcs-search-retry");
		expect(retry).not.toBeNull();
		retry.click();
		await vi.waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(3));
		await vi.waitFor(() => expect(document.querySelector("#dcs-search-status").textContent).toContain("No documentation matched"));
	});

	it("supports Arrow Up, Enter, and Escape keyboard behavior", async () => {
		vi.spyOn(globalThis, "fetch").mockImplementation(() => ok([
			{ id: "symbol:One", type: "class", title: "One", summary: "one", url: "/one", module: "DisCatSharp", score: 2 },
			{ id: "symbol:Two", type: "class", title: "Two", summary: "two", url: "/two", module: "DisCatSharp", score: 1 },
		]));
		const search = mount();
		const input = document.querySelector("#dcs-search-query");
		input.value = "symbols";
		await search.runSearch();
		input.dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowUp", bubbles: true }));
		expect(input.getAttribute("aria-activedescendant")).toBe("dcs-search-result-1");
		const selected = document.querySelector("#dcs-search-result-1");
		selected.click = vi.fn();
		input.dispatchEvent(new KeyboardEvent("keydown", { key: "Enter", bubbles: true }));
		expect(selected.click).toHaveBeenCalledOnce();

		input.dispatchEvent(new KeyboardEvent("keydown", { key: "Escape", bubbles: true }));
		expect(input.value).toBe("");
		expect(document.querySelector("#dcs-search-results").hidden).toBe(true);
		expect(input.getAttribute("aria-expanded")).toBe("false");
	});
});
