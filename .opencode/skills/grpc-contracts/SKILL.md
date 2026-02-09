---
name: grpc-contracts
description: gRPC and protobuf-net contract conventions for the Apollo project
---

## Overview

Apollo uses protobuf-net (code-first gRPC) rather than `.proto` files. Contracts are defined as C# classes/records with attributes. The shared contracts live in `Apollo.GRPC`.

## Contract Attributes

Use `[DataContract]` and `[DataMember]` attributes with **explicit `Order`** for all gRPC message types:

```csharp
[DataContract]
public sealed class PersonResponse
{
    [DataMember(Order = 1)]
    public required PersonId Id { get; init; }

    [DataMember(Order = 2)]
    public required string Name { get; init; }

    [DataMember(Order = 3)]
    public string? Email { get; init; }
}
```

## Required Fields

- Use `required` properties with `init` setters for required gRPC fields.
- Use nullable types (`string?`, `DateTimeOffset?`) for optional fields.

## Order Rules

- `Order` values must be explicit and unique within a type.
- Start from 1 and increment sequentially.
- Never reuse or change an `Order` value once deployed -- this breaks wire compatibility.
- When adding new fields, use the next available `Order` number.

## Service Contracts

gRPC service interfaces are defined with `[Service]` and `[Operation]` attributes:

```csharp
[Service]
public interface IPersonService
{
    [Operation]
    Task<PersonResponse> GetByIdAsync(GetPersonRequest request, CallContext context = default);
}
```

## Project Structure

- **`Apollo.GRPC`** - Shared contracts (DTOs, service interfaces), clients, and interceptors. This is a library, not a host.
- Service implementations live in `Apollo.Service`.
- Clients are consumed by `Apollo.API` and `Apollo.Discord`.

## Testing gRPC

- Test service implementations via their handler/application layer, not through the gRPC transport.
- For integration tests, use `WebApplicationFactory` to spin up the host.
- Verify serialization round-trips for complex types.
