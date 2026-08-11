export interface ReleaseStatement {
  sql: string;
  params?: string[];
}

export interface StagedReleaseState {
  complete: number;
  source_commit: string;
  generated_at: string;
}

export function isSameRelease(state: StagedReleaseState | undefined, sourceCommit: string, generatedAt: string): boolean {
  return state?.source_commit === sourceCommit && state.generated_at === generatedAt;
}

export function createActivationStatements(): ReleaseStatement[] {
  return [
    {
      sql: `INSERT INTO symbols (record_id, uid, name, display_name, qualified_name, full_name, kind, namespace, module, parent_uid, summary, signature, content, url,
            source_path, source_start_line, source_end_line, related_json, content_hash)
            SELECT record_id, uid, name, display_name, qualified_name, full_name, kind, namespace, module, parent_uid, summary, signature, content, url,
              source_path, source_start_line, source_end_line, related_json, content_hash FROM staged_symbols WHERE true
            ON CONFLICT(uid) DO UPDATE SET record_id=excluded.record_id, name=excluded.name, display_name=excluded.display_name,
            qualified_name=excluded.qualified_name, full_name=excluded.full_name, kind=excluded.kind, namespace=excluded.namespace,
            module=excluded.module, parent_uid=excluded.parent_uid, summary=excluded.summary, signature=excluded.signature,
            content=excluded.content, url=excluded.url, source_path=excluded.source_path, source_start_line=excluded.source_start_line,
            source_end_line=excluded.source_end_line, related_json=excluded.related_json, content_hash=excluded.content_hash`,
    },
    {
      sql: `INSERT INTO documents (record_id, document_key, family, kind, title, description, content, url, module, source_path, content_hash)
            SELECT record_id, document_key, family, kind, title, description, content, url, module, source_path, content_hash
            FROM staged_documents WHERE true
            ON CONFLICT(document_key) DO UPDATE SET record_id=excluded.record_id, family=excluded.family, kind=excluded.kind,
            title=excluded.title, description=excluded.description, content=excluded.content, url=excluded.url, module=excluded.module,
            source_path=excluded.source_path, content_hash=excluded.content_hash`,
    },
    {
      sql: `INSERT INTO source_chunks (record_id, path, language, start_line, end_line, content, content_hash)
            SELECT record_id, path, language, start_line, end_line, content, content_hash FROM staged_source_chunks WHERE true
            ON CONFLICT(path, start_line, end_line) DO UPDATE SET record_id=excluded.record_id, language=excluded.language,
            end_line=excluded.end_line, content=excluded.content, content_hash=excluded.content_hash`,
    },
    { sql: "DELETE FROM symbols WHERE record_id IN (SELECT record_id FROM staged_deletions WHERE table_name = 'symbols')" },
    { sql: "DELETE FROM documents WHERE record_id IN (SELECT record_id FROM staged_deletions WHERE table_name = 'documents')" },
    { sql: "DELETE FROM source_chunks WHERE record_id IN (SELECT record_id FROM staged_deletions WHERE table_name = 'source_chunks')" },
    {
      sql: `INSERT INTO sync_state (id, schema_version, source_commit, generated_at, completed_at, ready, symbol_count, document_count,
            source_chunk_count, modules_json, types_json)
            SELECT 1, schema_version, source_commit, generated_at, completed_at, 1, symbol_count, document_count,
              source_chunk_count, modules_json, types_json FROM staged_sync_state WHERE id = 1 AND complete = 1
            ON CONFLICT(id) DO UPDATE SET schema_version=excluded.schema_version, source_commit=excluded.source_commit,
            generated_at=excluded.generated_at, completed_at=excluded.completed_at, ready=1, symbol_count=excluded.symbol_count,
            document_count=excluded.document_count, source_chunk_count=excluded.source_chunk_count,
            modules_json=excluded.modules_json, types_json=excluded.types_json`,
    },
    { sql: "DELETE FROM staged_symbols" },
    { sql: "DELETE FROM staged_documents" },
    { sql: "DELETE FROM staged_source_chunks" },
    { sql: "DELETE FROM staged_deletions" },
    { sql: "DELETE FROM staged_sync_state" },
  ];
}
