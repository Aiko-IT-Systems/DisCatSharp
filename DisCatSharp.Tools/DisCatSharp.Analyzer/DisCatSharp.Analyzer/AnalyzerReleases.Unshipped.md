### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DCS1301 | Usage    | Warning  | DisCatSharp client used with sync `using`; prefer `await using` for async disposal
DCS1302 | Usage    | Warning  | Sync `Dispose()` called on DisCatSharp client; prefer `await DisposeAsync()`
