# ExxerAI Project Context (as of latest restart)

## 1. Project State
- **Build:** Clean, compiles successfully via CLI and Visual Studio (after cache clear).
- **Tests:** All integration tests pass; test output and artifacts are isolated from source.
- **Artifacts:** All `bin/`, `obj/`, `TestResults/`, and Stryker outputs are redirected to `artifacts/` at the solution root.

## 2. Key Architectural Decisions
- **Hexagonal Architecture:** Clear separation between Application, Domain, Infrastructure, and Integration layers.
- **EnumModel Unification:** All code now uses the canonical `ExxerAI.Domain.Enums.EnumModel` for strongly-typed enums. Duplicate/legacy enum models have been removed or excluded.
- **Test Layering:** Integration tests use only the correct abstractions and DTOs; obsolete or direct infrastructure references have been removed.

## 3. Build & Artifact Isolation
- **Directory.Build.props:**
  - Redirects all build outputs to `artifacts/bin/` and `artifacts/obj/`.
  - Test results go to `artifacts/TestResults/`.
- **Stryker:**
  - Configured via `stryker-config.json` to output to `artifacts/StrykerOutput/`.
- **.gitignore:**
  - Ensures `artifacts/` is not tracked in source control.

## 4. Recent Major Changes
- Unified EnumModel usage across all layers and tests.
- Removed duplicate/legacy enum model implementations.
- Created/updated `Directory.Build.props` for artifact isolation.
- Confirmed all integration tests pass and build is clean.

## 5. Next Steps After Restart
- Reopen solution in Visual Studio (or CLI).
- Confirm clean build and test run.
- Continue with new development, test coverage, or architectural improvements as needed.

---
**This file is your up-to-date context checkpoint. Use it as a baseline for future work.** 