---
description: Format, build, and test the full solution
agent: build
---
Run the full verification pipeline and report any issues at each step:

1. `dotnet format Apollo.sln` - Ensure code formatting is correct
2. `dotnet build Apollo.sln` - Ensure the solution compiles
3. `dotnet test Apollo.sln --verbosity normal` - Ensure all tests pass

If any step fails, stop and analyze the failure before proceeding. Suggest fixes for any issues found.
