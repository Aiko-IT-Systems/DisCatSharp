import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { describe, expect, it } from "vitest";

async function workflow(name: string): Promise<string> {
  return readFile(resolve(`../../.github/workflows/${name}`), "utf8");
}

describe("documentation release workflow", () => {
  it("serializes production runs and activates D1 only after the matching site marker is live", async () => {
    const source = await workflow("documentation.yml");
    const stage = source.indexOf("npm run sync -- ../obj/search/search-index.json");
    const publish = source.indexOf("    documentation:");
    const activationJob = source.indexOf("    activate-search:");
    const waitForDocs = source.indexOf("npm run wait:docs -- ../obj/search/search-index.json");
    const activate = source.indexOf("npm run sync:activate -- ../obj/search/search-index.json");
    const expectedBuildSmoke = source.indexOf("EXPECTED_BUILD_SHA: ${{ github.sha }}");

    expect(source).toContain("group: documentation-production");
    expect(source).toContain("cancel-in-progress: false");
    expect(source).toContain("needs: documentation");
    expect([stage, publish, activationJob, waitForDocs, activate, expectedBuildSmoke]).not.toContain(-1);
    expect(stage).toBeLessThan(publish);
    expect(publish).toBeLessThan(activationJob);
    expect(activationJob).toBeLessThan(waitForDocs);
    expect(waitForDocs).toBeLessThan(activate);
    expect(activate).toBeLessThan(expectedBuildSmoke);
    expect(source).toContain("DCS_DOCS_BASE_URL: https://discatsharp-docs.pages.dev");
  });

  it("writes the release marker in both production and preview builds", async () => {
    for (const name of ["documentation.yml", "documentation_test.yml"]) {
      expect(await workflow(name)).toContain("npm run marker -- ../obj/search/search-index.json ../_site/search-build.json");
    }
  });
});
