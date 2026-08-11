PRAGMA foreign_keys = ON;

CREATE TABLE sync_state (
    id INTEGER PRIMARY KEY CHECK (id = 1),
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

CREATE TABLE symbols (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    record_id TEXT NOT NULL UNIQUE,
    uid TEXT NOT NULL UNIQUE,
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
    related_json TEXT NOT NULL DEFAULT '[]',
    content_hash TEXT NOT NULL
);

CREATE UNIQUE INDEX symbols_uid_nocase ON symbols(uid COLLATE NOCASE);
CREATE INDEX symbols_name_nocase ON symbols(name COLLATE NOCASE);
CREATE INDEX symbols_qualified_name_nocase ON symbols(qualified_name COLLATE NOCASE);
CREATE INDEX symbols_full_name_nocase ON symbols(full_name COLLATE NOCASE);

CREATE TABLE documents (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    record_id TEXT NOT NULL UNIQUE,
    document_key TEXT NOT NULL UNIQUE,
    family TEXT NOT NULL CHECK (family = 'conceptual'),
    kind TEXT NOT NULL,
    title TEXT NOT NULL,
    description TEXT NOT NULL,
    content TEXT NOT NULL,
    url TEXT NOT NULL UNIQUE,
    module TEXT,
    source_path TEXT NOT NULL,
    content_hash TEXT NOT NULL
);

CREATE INDEX documents_title_nocase ON documents(title COLLATE NOCASE);
CREATE INDEX documents_key_nocase ON documents(document_key COLLATE NOCASE);
CREATE INDEX documents_family_kind ON documents(family, kind);

CREATE TABLE source_chunks (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    record_id TEXT NOT NULL UNIQUE,
    path TEXT NOT NULL,
    language TEXT NOT NULL,
    start_line INTEGER NOT NULL CHECK (start_line > 0),
    end_line INTEGER NOT NULL CHECK (end_line >= start_line),
    content TEXT NOT NULL,
    content_hash TEXT NOT NULL,
    UNIQUE(path, start_line, end_line)
);

CREATE INDEX source_chunks_path_range ON source_chunks(path, start_line, end_line);

CREATE VIRTUAL TABLE symbols_fts USING fts5(
    uid,
    name,
    display_name,
    qualified_name,
    full_name,
    summary,
    signature,
    content,
    content = 'symbols',
    content_rowid = 'id',
    tokenize = 'unicode61 remove_diacritics 2',
    prefix = '2 3'
);

CREATE TRIGGER symbols_fts_insert AFTER INSERT ON symbols BEGIN
    INSERT INTO symbols_fts(rowid, uid, name, display_name, qualified_name, full_name, summary, signature, content)
    VALUES (new.id, new.uid, new.name, new.display_name, new.qualified_name, new.full_name, new.summary, new.signature, new.content);
END;

CREATE TRIGGER symbols_fts_delete AFTER DELETE ON symbols BEGIN
    INSERT INTO symbols_fts(symbols_fts, rowid, uid, name, display_name, qualified_name, full_name, summary, signature, content)
    VALUES ('delete', old.id, old.uid, old.name, old.display_name, old.qualified_name, old.full_name, old.summary, old.signature, old.content);
END;

CREATE TRIGGER symbols_fts_update AFTER UPDATE ON symbols BEGIN
    INSERT INTO symbols_fts(symbols_fts, rowid, uid, name, display_name, qualified_name, full_name, summary, signature, content)
    VALUES ('delete', old.id, old.uid, old.name, old.display_name, old.qualified_name, old.full_name, old.summary, old.signature, old.content);
    INSERT INTO symbols_fts(rowid, uid, name, display_name, qualified_name, full_name, summary, signature, content)
    VALUES (new.id, new.uid, new.name, new.display_name, new.qualified_name, new.full_name, new.summary, new.signature, new.content);
END;

CREATE VIRTUAL TABLE documents_fts USING fts5(
    document_key,
    title,
    description,
    content,
    content = 'documents',
    content_rowid = 'id',
    tokenize = 'unicode61 remove_diacritics 2',
    prefix = '2 3'
);

CREATE TRIGGER documents_fts_insert AFTER INSERT ON documents BEGIN
    INSERT INTO documents_fts(rowid, document_key, title, description, content)
    VALUES (new.id, new.document_key, new.title, new.description, new.content);
END;

CREATE TRIGGER documents_fts_delete AFTER DELETE ON documents BEGIN
    INSERT INTO documents_fts(documents_fts, rowid, document_key, title, description, content)
    VALUES ('delete', old.id, old.document_key, old.title, old.description, old.content);
END;

CREATE TRIGGER documents_fts_update AFTER UPDATE ON documents BEGIN
    INSERT INTO documents_fts(documents_fts, rowid, document_key, title, description, content)
    VALUES ('delete', old.id, old.document_key, old.title, old.description, old.content);
    INSERT INTO documents_fts(rowid, document_key, title, description, content)
    VALUES (new.id, new.document_key, new.title, new.description, new.content);
END;
