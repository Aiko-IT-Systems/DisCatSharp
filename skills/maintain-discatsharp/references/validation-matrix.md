# Validation matrix

Choose checks from the changed behavior rather than running commands mechanically.

| Change | Minimum validation |
| --- | --- |
| Core or shared API | Focused tests plus affected project build across `net9.0`, `net10.0`, and `net11.0` |
| Application commands | ApplicationCommands tests; trace parsing, validation, localization copies, equality, and synchronization |
| Gateway event or cache | Event-handler/Copilot regression test; account for concurrent handlers and verify raw payload mapping |
| Configuration or hosting | Configuration/Hosting tests and copy/reconstruction paths |
| Analyzer/code fix | Analyzer package tests with positive, negative, and fix-output cases |
| Documentation | DocFX build and relevant link/search validation |
| Search Worker | .NET indexer tests plus `npm ci`, typecheck, build, and Worker tests in `DisCatSharp.Docs/search` |
| Release workflow | Release helper unit tests, skill validation, and `actionlint`; never publish during PR validation |

Always finish with `git diff --check`, `git status --short`, and a focused diff review. `git diff --stat` does not include untracked files.
