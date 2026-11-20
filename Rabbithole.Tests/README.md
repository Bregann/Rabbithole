# Rabbithole Test Infrastructure

Same setup as Orbit: **NUnit**, **Moq** and **Testcontainers** (PostgreSQL). Docker must be running.

- `Infrastructure/TestContainerSetup.cs`: one Postgres container for the whole run.
- `Infrastructure/DatabaseIntegrationTestBase.cs`: a fresh, migrated database for every test. Override `CustomSetUp()` / `CustomTearDown()`.
- `Infrastructure/TestDatabaseSeedHelper.cs`: small seed methods. Seed only what the test needs.
- `Infrastructure/MockFactory.cs`: mocks for external dependencies only. **Never mock the database.**

Conventions:
- `<Service>IntegrationTests` for anything touching the database; `<Thing>Tests` for pure logic.
- Folders mirror the Domain: `Services/<Feature>/`, `Helpers/`.
- Names: `Method_Scenario_ExpectedResult`. Arrange / Act / Assert. `Assert.That(...)`.
- Cover the happy path **and** the failure cases for every method.

```bash
dotnet test
dotnet test --filter "FullyQualifiedName~StatusServiceTests"
```
