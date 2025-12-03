# Implement Caching Layer

- Create an interface to interact with the users in the datastore inside of Apollo.Core
- Create a mock implementation inside of Apollo.Database to allow us to pull users without needing to access the database currently
- Create an interface to expose access to the ApolloCache inside of Apollo.Core
- Create a service to interact with the Redis cache inside of Apollo.Cache
- Create a user validation service interface inside Apollo.Core
- Create a user validation service implementation inside of Apollo.Application
- Create method in new user validation service that checks the cache to see if the user has already been approved/denied access. If so, it returns that result. Otherwise, it gets the user from the database updates the cache appropriately before returning if the user has access
- Update `IncomingMessageHandler.HandleAsync` to check if the user has access

## Execution Plan

### Plan: User Access Validation with Redis Caching

Implement a caching layer that validates user access by checking Redis cache first, then falling back to the database. This optimizes the message handling flow by avoiding repeated database queries for user authorization.

### Steps

1. **Define core interfaces** in [Apollo.Core](../../src/Apollo.Core): Create `IUserDataAccess` for database operations and populate the existing `IUserCache` interface with methods for getting, setting, and invalidating user access cache entries.

2. **Create mock user data access** in [Apollo.Database](../../src/Apollo.Database): Implement `MockUserDataAccess` that returns hardcoded user access data without database queries, to be used until real implementation is needed.

3. **Implement Redis cache service** in [Apollo.Cache](../../src/Apollo.Cache): Create `UserCache` class implementing `IUserCache` using StackExchange.Redis, with cache key pattern `user:access:{username}` and 5-minute TTL expiration for security.

4. **Create user validation service** in [Apollo.Application](../../src/Apollo.Application): Implement `UserValidationService` with cache-first logic (check Redis → query data access → update cache → return result) using FluentResults pattern with fail-closed error handling.

5. **Wire up dependency injection**: Update ServiceCollectionExtensions in Cache, Database, and Application projects, register Redis connection in [Apollo.Discord](../../src/Apollo.Discord), and add necessary project references.

6. **Integrate validation into message handler**: Update [IncomingMessageHandler.HandleAsync](../../src/Apollo.Discord/Handlers/IncomingMessageHandler.cs#L18) to call validation service and return early if user lacks access, logging denied attempts appropriately.
