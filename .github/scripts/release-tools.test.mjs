import assert from "node:assert/strict";
import { mkdtempSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { join } from "node:path";
import test from "node:test";

import {
  createChecksums,
  evaluateReleaseState,
  parsePrimaryPackageVersion,
  resolveMetadata,
  selectPreviousTag,
  validateInputs,
} from "./release-tools.mjs";

test("validates stable, nightly, beta, and RC inputs", () => {
  assert.doesNotThrow(() => validateInputs({ family: "DisCatSharp", prerelease: false, confirmFullRelease: true, suffix: "" }));
  for (const suffix of ["nightly-015", "beta.2", "rc-1"])
    assert.doesNotThrow(() => validateInputs({ family: "DisCatSharp", prerelease: true, confirmFullRelease: false, suffix }));
  assert.throws(() => validateInputs({ family: "DisCatSharp", prerelease: true, confirmFullRelease: false, suffix: "" }), /suffix is required/);
  assert.throws(() => validateInputs({ family: "DisCatSharp", prerelease: false, confirmFullRelease: true, suffix: "nightly-015" }), /cannot have/);
  assert.throws(() => validateInputs({ family: "DisCatSharp", prerelease: false, confirmFullRelease: false, suffix: "" }), /confirm_full_release/);
});

test("extracts only each family's primary package version", () => {
  assert.equal(parsePrimaryPackageVersion("DisCatSharp.10.7.1-nightly-015.nupkg", "DisCatSharp"), "10.7.1-nightly-015");
  assert.equal(parsePrimaryPackageVersion("DisCatSharp.ApplicationCommands.10.7.1.nupkg", "DisCatSharp"), null);
  assert.equal(parsePrimaryPackageVersion("DisCatSharp.Attributes.2026.3.26-beta.2.nupkg", "DisCatSharp.Attributes"), "2026.3.26-beta.2");
  assert.equal(parsePrimaryPackageVersion("DisCatSharp.Analyzer.1.0.6.1.nupkg", "DisCatSharp.Analyzer"), "1.0.6.1");
});

test("derives all tag namespaces from built artifacts", () => {
  const cases = [
    ["DisCatSharp", "DisCatSharp.10.7.1-nightly-015.nupkg", true, "nightly-015", "v10.7.1-nightly-015"],
    ["DisCatSharp", "DisCatSharp.10.7.1-beta.2.nupkg", true, "beta.2", "v10.7.1-beta.2"],
    ["DisCatSharp.Attributes", "DisCatSharp.Attributes.2026.3.26.nupkg", false, "", "attributes-v2026.3.26"],
    ["DisCatSharp.Analyzer", "DisCatSharp.Analyzer.1.0.6.1-rc.1.nupkg", true, "rc.1", "analyzer-v1.0.6.1-rc.1"],
  ];

  for (const [family, file, prerelease, suffix, tag] of cases) {
    withTempDirectory((directory) => {
      writeFileSync(join(directory, file), "package");
      assert.equal(resolveMetadata({ artifactDirectory: directory, family, prerelease, suffix }).tag, tag);
    });
  }
});

test("stable notes ignore nightlies while prerelease notes use the latest family release", () => {
  const releases = [
    release("v10.7.1-nightly-014", true, "2026-04-26"),
    release("v10.7.1-nightly-013", true, "2026-04-21"),
    release("v10.7.0", false, "2026-03-20"),
    release("v10.6.7", false, "2025-02-08"),
  ];
  assert.equal(selectPreviousTag({ releases, family: "DisCatSharp", prerelease: true, currentTag: "v10.7.1-nightly-015" }), "v10.7.1-nightly-014");
  assert.equal(selectPreviousTag({ releases, family: "DisCatSharp", prerelease: false, currentTag: "v10.7.1" }), "v10.7.0");
});

test("first Attributes and Analyzer release falls back to the latest stable main release", () => {
  const releases = [
    release("v10.7.1-nightly-014", true, "2026-04-26"),
    release("v10.7.0", false, "2026-03-20"),
  ];
  assert.equal(selectPreviousTag({ releases, family: "DisCatSharp.Attributes", prerelease: true, currentTag: "attributes-v2026.3.26-nightly-001" }), "v10.7.0");
  assert.equal(selectPreviousTag({ releases, family: "DisCatSharp.Analyzer", prerelease: false, currentTag: "analyzer-v1.0.6.1" }), "v10.7.0");
});

test("draft state is resumable and published state is verification-only", () => {
  const expected = { tag: "v10.7.1", targetCommit: "abc123" };
  assert.equal(evaluateReleaseState(null, expected), "create");
  assert.equal(evaluateReleaseState({ tag_name: expected.tag, target_commitish: expected.targetCommit, draft: true }, expected), "resume");
  assert.equal(evaluateReleaseState({ tag_name: expected.tag, target_commitish: expected.targetCommit, draft: false }, expected), "verify");
  assert.throws(() => evaluateReleaseState({ tag_name: "v10.7.0", target_commitish: expected.targetCommit, draft: true }, expected), /does not match/);
  assert.throws(() => evaluateReleaseState({ tag_name: expected.tag, target_commitish: "different", draft: true }, expected), /targets different/);
});

test("writes deterministic checksums for every release asset", () => {
  withTempDirectory((directory) => {
    writeFileSync(join(directory, "b.nupkg"), "b");
    writeFileSync(join(directory, "a.snupkg"), "a");
    writeFileSync(join(directory, "RELEASENOTES.md"), "notes");
    createChecksums(directory);
    const checksums = readFileSync(join(directory, "SHA256SUMS"), "utf8").trim().split("\n");
    assert.match(checksums[0], /  a\.snupkg$/);
    assert.match(checksums[1], /  b\.nupkg$/);
    assert.match(checksums[2], /  RELEASENOTES\.md$/);
  });
});

function release(tagName, isPrerelease, publishedAt) {
  return { tagName, isPrerelease, isDraft: false, publishedAt };
}

function withTempDirectory(operation) {
  const directory = mkdtempSync(join(tmpdir(), "dcs-release-test-"));
  try {
    operation(directory);
  } finally {
    rmSync(directory, { recursive: true, force: true });
  }
}
