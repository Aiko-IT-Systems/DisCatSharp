## Release 1.0.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
DCS0001 | Usage    | Info     | Experimental APIs, [Documentation](https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0001)
DCS0002 | Usage    | Error    | Deprecated APIs, [Documentation](https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0002)
DCS0101 | Usage    | Warning  | Discord experimental APIs, [Documentation](https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0101)
DCS0102 | Usage    | Error    | Discord deprecated APIs, [Documentation](https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0102)
DCS0103 | Usage    | Warning  | Discord unreleased APIs, [Documentation](https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0103)
DCS0200 | Usage    | Info     | Requires feature APIs, [Documentation](https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0200)
DCS0201 | Usage    | Warning  | Requires override APIs, [Documentation](https://docs.dcs.aitsys.dev/vs/analyzer/dcs/0201)

## Release 1.0.1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
DCS2101 | Usage    | Error    | Application-command checks-failed migration prototype; raised to error because legacy checks-failed handlers can break consumers if left on errored events, [Documentation](https://docs.dcs.aitsys.dev/vs/analyzer/dcs/2101)

## Release 1.0.2.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
DCS1101 | Usage    | Warning  | Presence access migration guidance, [Documentation](https://docs.dcs.aitsys.dev/vs/analyzer/dcs/1101)

## Release 1.0.3.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|--------------------
DCS1102 | Usage    | Warning  | Ban parameter renamed from 'deleteMessageDays' to 'deleteMessageSeconds'; named argument callers can be auto-fixed, [Documentation](https://docs.dcs.aitsys.dev/vs/analyzer/dcs/1102)

## Release 1.0.5.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DCS1103 | Usage    | Error    | DiscordClient.Presences property removed
DCS1201 | Usage    | Warning  | Configuration property moved to nested config

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DCS1101 | Usage    | Warning  | Removed; superseded by DCS1103

## Release 1.0.6.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DCS1301 | Usage    | Warning  | DisCatSharp client used with sync `using`; prefer `await using` for async disposal, [Documentation](https://docs.dcs.aitsys.dev/vs/analyzer/dcs/1301)
DCS1302 | Usage    | Warning  | Sync `Dispose()` called on DisCatSharp client; prefer `await DisposeAsync()`, [Documentation](https://docs.dcs.aitsys.dev/vs/analyzer/dcs/1302)
