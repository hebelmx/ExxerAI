# AGENTS.md

## Project Overview

- **Repository**: IndTraceV2025
- **Language**: C# (.NET 9+)
- **Testing Frameworks**: XUnit NSubstitute and Shouldly
- **Target Architecture**: Modular, well-encapsulated components with traceability and extensibility in industrial contexts

## Branch Policy

- All AI-assisted changes must be made on the `work` branch.
- Agents must check out `work` before starting any work.
- Pull Requests must target `work`.
- Direct commits or pull requests to `main`, `dev`, or other branches are prohibited unless explicitly stated.


## Code Style

- Follow **Microsoft C# conventions** with the following emphasis:
  - Explicit visibility modifiers (e.g., `public`, `private`)
  - Use **PascalCase** for classes, methods, and public properties
  - Use **camelCase** for local variables and private fields
  - - Use **PascalCase** for field if private
  - Prefer expression-bodied members for short accessors
  
- All methods should include:
  - XML-style **summary comments**
  - Proper **error handling** using structured exception types
  - Avoid trow exception prefer use Return.Fail o Return<T>.Fail with the cause of the fail
- Prefer `readonly` modifiers where applicable
- Use primary constructor with applicable
- When needed leave a private constructor empty to use for json or efcore
- LINQ is desirable but favor clarity over conciseness

## File Structure

- `/Src`: Core application logic
- `/Tests`: Unit and integration test projects
- `/Tools`: Scripts, code generation, and utilities
- `.cr`, `.vs`: IDE metadata and copilot snapshots (excluded from agent operations)

## Testing & Coverage

- Use `dotnet test` with Shouldly and NSubstitute
- Ensure at least **85% unit test coverage**
- All public methods must be unit tested
- Mocks must use `Substitute.For<T>()`, and assertions must use `.ShouldBe(...)`
- Unit test class are progresive on the first test till the last test we are atesting more and more behavior of the class 
- Use many assertions as posible on a methods on the last methods of the class
- Always Unit test Manual mappers on edge case and on happy path


## Linting and Formatting

- Follow `.editorconfig` where present
- Prefer tabs or 4-space indentation consistently
- Run `dotnet format` before PRs

## PR and Commit Guidelines

- PR Titles:
  - `[Fix]` for bug fixes
  - `[Feat]` for new features
  - `[Refactor]` for internal changes
- Include:
  - Linked issues
  - Testing strategy
  - Checklist of impacted modules
- CI must pass before merge

## CI/CD Notes

- Build using `dotnet build`
- Test using `dotnet test --collect:"Code Coverage"`
- Validate no regressions on UI behavior if impacted

## Special Considerations

- Maintain high-quality documentation in code
- Use region tags (`#region`, `#endregion`) to organize large files logically
- Place all enums, DTOs, and models in dedicated subfolders
- Async methods must be **suffixed with `Async`**

##📝 Code Annotation Policy
- All code changes and suggestions made by Codex agents must be annotated using the following formats:

✅ For Code Changes:
//[Fix] 
//CODEX
//Date: DD/MM/YYYY 
//Reason: <Concise explanation of the change>

--Example:

//[Fix] 
//CODEX
//Date: June/8/2025 
//Reason: Replaced obsolete API call with recommended method

💡 For Suggestions
//[TODO] 
//CODEX
//Date: DD/MM/YYYY 
//Reason: <Clear rationale or intended improvement>


--Example:

//[TODO] 
//CODEX
//Date: 08/06/2025 
//Reason: Improve error handling to cover network timeouts

