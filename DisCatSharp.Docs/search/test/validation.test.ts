import { describe, expect, it } from "vitest";
import { normalizeLimit, normalizeSearchQuery, normalizeTypes, toFtsQuery, validateSourceRequest } from "../src/validation";

describe("search validation", () => {
  it("normalizes symbol-style queries", () => {
    expect(normalizeSearchQuery("  DiscordGuild.GetMemberAsync() ")).toBe("DiscordGuild.GetMemberAsync");
    expect(toFtsQuery("DiscordGuild.GetMemberAsync")).toBe('"DiscordGuild" AND "GetMemberAsync"*');
  });

  it("rejects empty, punctuation-only, and oversized queries", () => {
    expect(() => normalizeSearchQuery(" ")).toThrow(/at least two/u);
    expect(() => normalizeSearchQuery("::")).toThrow(/letter/u);
    expect(() => normalizeSearchQuery("x".repeat(201))).toThrow(/at most 200/u);
  });

  it("deduplicates filters and enforces result limits", () => {
    expect(normalizeTypes(["Method", "method", "conceptual"])).toEqual(["method", "conceptual"]);
    expect(normalizeLimit(undefined)).toBe(12);
    expect(() => normalizeLimit(51)).toThrow(/between 1 and 50/u);
  });
});

describe("source validation", () => {
  it("accepts bounded repository-relative source ranges", () => {
    expect(validateSourceRequest({ path: "DisCatSharp/Entities/Guild.cs", startLine: 10, endLine: 30 })).toEqual({
      path: "DisCatSharp/Entities/Guild.cs",
      startLine: 10,
      endLine: 30,
    });
  });

  it.each(["../secret", "/etc/passwd", "C:/secret", "folder\\secret", "folder//secret"])("rejects unsafe path %s", (path) => {
    expect(() => validateSourceRequest({ path, startLine: 1, endLine: 2 })).toThrow(/relative paths/u);
  });

  it("rejects reversed and oversized ranges", () => {
    expect(() => validateSourceRequest({ path: "file.cs", startLine: 5, endLine: 4 })).toThrow(/endLine/u);
    expect(() => validateSourceRequest({ path: "file.cs", startLine: 1, endLine: 201 })).toThrow(/200 lines/u);
  });
});
