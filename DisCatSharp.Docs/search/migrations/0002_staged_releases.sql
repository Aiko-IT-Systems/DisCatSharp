CREATE TABLE staged_sync_state (
    id INTEGER PRIMARY KEY CHECK (id = 1),
    schema_version INTEGER NOT NULL,
    source_commit TEXT NOT NULL,
    generated_at TEXT NOT NULL,
    completed_at TEXT,
    complete INTEGER NOT NULL DEFAULT 0 CHECK (complete IN (0, 1)),
    symbol_count INTEGER NOT NULL,
    document_count INTEGER NOT NULL,
    source_chunk_count INTEGER NOT NULL,
    modules_json TEXT NOT NULL,
    types_json TEXT NOT NULL
);

CREATE TABLE staged_symbols (
    record_id TEXT PRIMARY KEY,
    uid TEXT NOT NULL,
    name TEXT NOT NULL,
    display_name TEXT NOT NULL,
    qualified_name TEXT NOT NULL,
    full_name TEXT NOT NULL,
    kind TEXT NOT NULL,
    namespace TEXT,
    module TEXT,
    parent_uid TEXT,
    summary TEXT NOT NULL,
    signature TEXT NOT NULL,
    content TEXT NOT NULL,
    url TEXT NOT NULL,
    source_path TEXT,
    source_start_line INTEGER,
    source_end_line INTEGER,
    related_json TEXT NOT NULL,
    content_hash TEXT NOT NULL
);

CREATE TABLE staged_documents (
    record_id TEXT PRIMARY KEY,
    document_key TEXT NOT NULL,
    family TEXT NOT NULL,
    kind TEXT NOT NULL,
    title TEXT NOT NULL,
    description TEXT NOT NULL,
    content TEXT NOT NULL,
    url TEXT NOT NULL,
    module TEXT,
    source_path TEXT NOT NULL,
    content_hash TEXT NOT NULL
);

CREATE TABLE staged_source_chunks (
    record_id TEXT PRIMARY KEY,
    path TEXT NOT NULL,
    language TEXT NOT NULL,
    start_line INTEGER NOT NULL,
    end_line INTEGER NOT NULL,
    content TEXT NOT NULL,
    content_hash TEXT NOT NULL
);

CREATE TABLE staged_deletions (
    table_name TEXT NOT NULL CHECK (table_name IN ('symbols', 'documents', 'source_chunks')),
    record_id TEXT NOT NULL,
    PRIMARY KEY (table_name, record_id)
);
