# Apollo's Architecture Overview

## Environment Variables

To set up your local development environment for Apollo, you'll need to configure a few environment variables. These variables will help the application connect to necessary services and APIs.

- `Apollo__AI__ModelId`: The identifier for the AI model you wish to use.
- `Apollo__AI__Endpoint`: The endpoint URL for the AI service.
- `Apollo__AI__ApiKey`: The API key for authenticating requests to

## Coding Practices

### General

- Use DTOs for records exchanged between services to ensure clear contracts and separation of concerns.
