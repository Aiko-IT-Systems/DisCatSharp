ALTER TABLE symbols ADD COLUMN corpus TEXT NOT NULL DEFAULT 'main';
ALTER TABLE symbols ADD COLUMN repository TEXT NOT NULL DEFAULT 'Aiko-IT-Systems/DisCatSharp';
ALTER TABLE symbols ADD COLUMN canonical_uid TEXT;
ALTER TABLE documents ADD COLUMN corpus TEXT NOT NULL DEFAULT 'main';
ALTER TABLE documents ADD COLUMN repository TEXT NOT NULL DEFAULT 'Aiko-IT-Systems/DisCatSharp';
ALTER TABLE source_chunks ADD COLUMN corpus TEXT NOT NULL DEFAULT 'main';
ALTER TABLE source_chunks ADD COLUMN repository TEXT NOT NULL DEFAULT 'Aiko-IT-Systems/DisCatSharp';

ALTER TABLE staged_symbols ADD COLUMN corpus TEXT NOT NULL DEFAULT 'main';
ALTER TABLE staged_symbols ADD COLUMN repository TEXT NOT NULL DEFAULT 'Aiko-IT-Systems/DisCatSharp';
ALTER TABLE staged_symbols ADD COLUMN canonical_uid TEXT;
ALTER TABLE staged_documents ADD COLUMN corpus TEXT NOT NULL DEFAULT 'main';
ALTER TABLE staged_documents ADD COLUMN repository TEXT NOT NULL DEFAULT 'Aiko-IT-Systems/DisCatSharp';
ALTER TABLE staged_source_chunks ADD COLUMN corpus TEXT NOT NULL DEFAULT 'main';
ALTER TABLE staged_source_chunks ADD COLUMN repository TEXT NOT NULL DEFAULT 'Aiko-IT-Systems/DisCatSharp';
ALTER TABLE staged_deletions ADD COLUMN corpus TEXT NOT NULL DEFAULT 'main';

CREATE TABLE corpus_sync_state (
    corpus TEXT PRIMARY KEY,
    repository TEXT NOT NULL,
    site_base_url TEXT,
    schema_version INTEGER NOT NULL,
    source_commit TEXT NOT NULL,
    generated_at TEXT NOT NULL,
    completed_at TEXT,
    ready INTEGER NOT NULL DEFAULT 0 CHECK (ready IN (0, 1)),
    symbol_count INTEGER NOT NULL DEFAULT 0,
    document_count INTEGER NOT NULL DEFAULT 0,
    source_chunk_count INTEGER NOT NULL DEFAULT 0,
    modules_json TEXT NOT NULL DEFAULT '[]',
    types_json TEXT NOT NULL DEFAULT '[]'
);

INSERT INTO corpus_sync_state (
    corpus, repository, site_base_url, schema_version, source_commit, generated_at, completed_at, ready,
    symbol_count, document_count, source_chunk_count, modules_json, types_json
)
SELECT 'main', 'Aiko-IT-Systems/DisCatSharp', 'https://docs.dcs.aitsys.dev', schema_version, source_commit, generated_at,
    completed_at, ready, symbol_count, document_count, source_chunk_count, modules_json, types_json
FROM sync_state
WHERE id = 1;

CREATE TABLE staged_corpus_sync_state (
    corpus TEXT PRIMARY KEY,
    repository TEXT NOT NULL,
    site_base_url TEXT,
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

CREATE INDEX symbols_corpus ON symbols(corpus);
UPDATE symbols SET canonical_uid = uid WHERE canonical_uid IS NULL;
UPDATE staged_symbols SET canonical_uid = uid WHERE canonical_uid IS NULL;
CREATE INDEX symbols_corpus_canonical_uid_nocase ON symbols(corpus, canonical_uid COLLATE NOCASE);
CREATE INDEX documents_corpus ON documents(corpus);
CREATE INDEX source_chunks_corpus_path_range ON source_chunks(corpus, path, start_line, end_line);
CREATE INDEX staged_symbols_corpus ON staged_symbols(corpus);
CREATE INDEX staged_documents_corpus ON staged_documents(corpus);
CREATE INDEX staged_source_chunks_corpus ON staged_source_chunks(corpus);
CREATE INDEX staged_deletions_corpus ON staged_deletions(corpus);
