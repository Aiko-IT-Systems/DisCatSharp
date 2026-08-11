export interface ExistingHashRow {
  record_id: string;
  content_hash: string;
}

export interface HashedRecord {
  id: string;
  contentHash: string;
}

export interface HashDelta<T extends HashedRecord> {
  changedRecords: T[];
  staleIds: string[];
  unchanged: number;
}

export function toD1Scalar(value: number): string {
  if (!Number.isFinite(value)) throw new Error("Cannot serialize a non-finite D1 parameter.");
  return String(value);
}

export function toD1JsonScalar(value: string | number | null): string {
  if (typeof value === "number" && !Number.isFinite(value)) throw new Error("Cannot serialize a non-finite D1 parameter.");
  return JSON.stringify(value);
}

export function diffHashes<T extends HashedRecord>(existingRows: readonly ExistingHashRow[], records: readonly T[]): HashDelta<T> {
  const existing = new Map(existingRows.map((row) => [String(row.record_id), String(row.content_hash)]));
  const desired = new Map<string, string>();
  for (const record of records) {
    if (desired.has(record.id)) throw new Error(`Search artifact contains duplicate record ID '${record.id}'.`);
    desired.set(record.id, record.contentHash);
  }

  const changedRecords = records.filter((record) => existing.get(record.id) !== record.contentHash);
  const staleIds = [...existing.keys()].filter((id) => !desired.has(id)).sort((left, right) => left.localeCompare(right));
  return { changedRecords, staleIds, unchanged: records.length - changedRecords.length };
}
