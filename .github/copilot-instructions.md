# Copilot Instructions

## C# Coding Conventions
- Use camelCase (e.g. `session`, `undoButton`, `board`) for local variables.
- Use local variables without underscore prefixes and keep code clean, idiomatic, and extensible with explanatory comments when implementing features.

## Test Naming Conventions
- Name tests using the pattern `MethodName_StateUnderTest_ExpectedOutcome`.

## Game Rules
- When implementing a game rule from `GameRules.md`, add corresponding unit tests that cover both the positive case (rule applies) and the negative case (rule does not apply). Name those tests using the Test Naming Conventions above.
