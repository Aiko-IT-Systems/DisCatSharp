#!/usr/bin/env node

import { createHash } from "node:crypto";
import { appendFileSync, readFileSync, readdirSync, realpathSync, statSync, writeFileSync } from "node:fs";
import { basename, join } from "node:path";
import process from "node:process";
import { fileURLToPath } from "node:url";

export const FAMILIES = Object.freeze({
  DisCatSharp: Object.freeze({
    packageId: "DisCatSharp",
    tagPrefix: "v",
    title: "DisCatSharp",
  }),
  "DisCatSharp.Attributes": Object.freeze({
    packageId: "DisCatSharp.Attributes",
    tagPrefix: "attributes-v",
    title: "DisCatSharp.Attributes",
  }),
  "DisCatSharp.Analyzer": Object.freeze({
    packageId: "DisCatSharp.Analyzer",
    tagPrefix: "analyzer-v",
    title: "DisCatSharp.Analyzer",
  }),
});

export function validateInputs({ family, prerelease, confirmFullRelease, suffix }) {
  if (!FAMILIES[family])
    throw new Error(`Unsupported package family: ${family}`);

  if (prerelease) {
    if (!suffix?.trim())
      throw new Error("A version suffix is required for a prerelease.");
  } else {
    if (suffix?.trim())
      throw new Error("A stable release cannot have a version suffix.");
    if (!confirmFullRelease)
      throw new Error("A stable release requires confirm_full_release.");
  }
}

export function parsePrimaryPackageVersion(fileName, family) {
  const config = FAMILIES[family];
  if (!config)
    throw new Error(`Unsupported package family: ${family}`);

  const id = escapeRegExp(config.packageId);
  const match = fileName.match(new RegExp(`^${id}\\.(\\d+\\.\\d+\\.\\d+(?:\\.\\d+)?(?:-[0-9A-Za-z.-]+)?)\\.nupkg$`));
  return match?.[1] ?? null;
}

export function resolveMetadata({ artifactDirectory, family, prerelease, suffix }) {
  const files = readdirSync(artifactDirectory)
    .filter((name) => statSync(join(artifactDirectory, name)).isFile());
  const candidates = files
    .map((name) => ({ name, version: parsePrimaryPackageVersion(name, family) }))
    .filter((candidate) => candidate.version !== null);

  if (candidates.length !== 1)
    throw new Error(`Expected exactly one primary ${family} package, found ${candidates.length}.`);

  const version = candidates[0].version;
  const hasPrereleaseSuffix = version.includes("-");
  if (prerelease !== hasPrereleaseSuffix)
    throw new Error(`Built package version ${version} does not match the requested release channel.`);
  if (prerelease && !version.toLowerCase().endsWith(`-${suffix.toLowerCase()}`))
    throw new Error(`Built package version ${version} does not end with the requested suffix ${suffix}.`);

  const config = FAMILIES[family];
  return {
    version,
    tag: `${config.tagPrefix}${version}`,
    title: `${config.title} v${version}`,
    primaryPackage: candidates[0].name,
  };
}

export function selectPreviousTag({ releases, family, prerelease, currentTag }) {
  const flattened = releases.flat?.(Infinity) ?? releases;
  const config = FAMILIES[family];
  if (!config)
    throw new Error(`Unsupported package family: ${family}`);

  const sameFamily = flattened
    .filter((release) => !release.isDraft && release.tagName !== currentTag)
    .filter((release) => tagBelongsToFamily(release.tagName, family))
    .filter((release) => prerelease || !release.isPrerelease)
    .sort(newestFirst);

  if (sameFamily.length > 0)
    return sameFamily[0].tagName;

  if (family !== "DisCatSharp") {
    const mainStable = flattened
      .filter((release) => !release.isDraft && !release.isPrerelease)
      .filter((release) => tagBelongsToFamily(release.tagName, "DisCatSharp"))
      .sort(newestFirst);
    return mainStable[0]?.tagName ?? null;
  }

  return null;
}

export function evaluateReleaseState(release, { tag, targetCommit }) {
  if (release === null)
    return "create";
  if (release.tag_name !== tag)
    throw new Error(`Existing release tag ${release.tag_name} does not match ${tag}.`);
  if (release.target_commitish !== targetCommit)
    throw new Error(`Release ${tag} targets ${release.target_commitish}, expected ${targetCommit}.`);
  return release.draft ? "resume" : "verify";
}

export function createChecksums(artifactDirectory) {
  const checksumName = "SHA256SUMS";
  const files = readdirSync(artifactDirectory)
    .filter((name) => name !== checksumName)
    .filter((name) => statSync(join(artifactDirectory, name)).isFile())
    .sort((left, right) => left.localeCompare(right, "en"));

  if (files.length === 0)
    throw new Error("No release artifacts were found for checksum generation.");

  const lines = files.map((name) => {
    const digest = createHash("sha256").update(readFileSync(join(artifactDirectory, name))).digest("hex");
    return `${digest}  ${name}`;
  });
  writeFileSync(join(artifactDirectory, checksumName), `${lines.join("\n")}\n`, "utf8");
  return files;
}

function newestFirst(left, right) {
  return Date.parse(right.publishedAt ?? 0) - Date.parse(left.publishedAt ?? 0);
}

function tagBelongsToFamily(tag, family) {
  if (family === "DisCatSharp")
    return /^v\d/.test(tag);
  return tag.startsWith(FAMILIES[family].tagPrefix);
}

function escapeRegExp(value) {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function parseArguments(arguments_) {
  const values = {};
  for (let index = 0; index < arguments_.length; index += 2) {
    const key = arguments_[index];
    const value = arguments_[index + 1];
    if (!key?.startsWith("--") || value === undefined)
      throw new Error(`Invalid argument list near ${key ?? "<end>"}.`);
    values[key.slice(2)] = value;
  }
  return values;
}

function boolean(value, name) {
  if (value === "true")
    return true;
  if (value === "false")
    return false;
  throw new Error(`${name} must be true or false.`);
}

function writeOutputs(outputs) {
  const text = Object.entries(outputs).map(([key, value]) => `${key}=${value ?? ""}`).join("\n");
  if (process.env.GITHUB_OUTPUT)
    appendFileSync(process.env.GITHUB_OUTPUT, `${text}\n`, "utf8");
  process.stdout.write(`${JSON.stringify(outputs, null, 2)}\n`);
}

async function main() {
  const [command, ...arguments_] = process.argv.slice(2);
  const args = parseArguments(arguments_);

  switch (command) {
    case "validate-inputs": {
      validateInputs({
        family: args.family,
        prerelease: boolean(args.prerelease, "prerelease"),
        confirmFullRelease: boolean(args.confirm, "confirm"),
        suffix: args.suffix,
      });
      break;
    }
    case "metadata": {
      const outputs = resolveMetadata({
        artifactDirectory: args.artifacts,
        family: args.family,
        prerelease: boolean(args.prerelease, "prerelease"),
        suffix: args.suffix,
      });
      createChecksums(args.artifacts);
      writeOutputs(outputs);
      break;
    }
    case "previous-tag": {
      const releases = JSON.parse(readFileSync(args.releases, "utf8"));
      const previousTag = selectPreviousTag({
        releases,
        family: args.family,
        prerelease: boolean(args.prerelease, "prerelease"),
        currentTag: args.tag,
      });
      writeOutputs({ previous_tag: previousTag });
      break;
    }
    case "release-state": {
      const release = JSON.parse(readFileSync(args.release, "utf8"));
      const state = evaluateReleaseState(release, { tag: args.tag, targetCommit: args.commit });
      writeOutputs({ release_state: state });
      break;
    }
    default:
      throw new Error(`Unknown command: ${command ?? "<none>"}`);
  }
}

if (process.argv[1] && realpathSync(fileURLToPath(import.meta.url)) === realpathSync(process.argv[1])) {
  main().catch((error) => {
    process.stderr.write(`${error.message}\n`);
    process.exitCode = 1;
  });
}
