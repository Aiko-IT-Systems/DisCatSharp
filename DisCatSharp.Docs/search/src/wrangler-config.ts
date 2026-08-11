export interface WranglerD1Binding {
  binding?: string;
  database_name?: string;
  database_id?: string;
  [key: string]: unknown;
}

export interface WranglerConfiguration {
  d1_databases?: WranglerD1Binding[];
  [key: string]: unknown;
}

export function bindProductionDatabase(configuration: WranglerConfiguration, databaseName: string, databaseId: string): WranglerConfiguration {
  if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/iu.test(databaseId)) {
    throw new Error("Cloudflare returned an invalid D1 database UUID.");
  }

  const result = structuredClone(configuration);
  const matches = result.d1_databases?.filter((binding) => binding.binding === "DB" && binding.database_name === databaseName) ?? [];
  if (matches.length !== 1) {
    throw new Error(`Expected exactly one DB binding for D1 database '${databaseName}', found ${matches.length}.`);
  }
  matches[0]!.database_id = databaseId;
  return result;
}
