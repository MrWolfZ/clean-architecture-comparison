# Baseline

This is a simple implementation of our application using basic architectural patterns. We have a single _web_ project that contains all the logic for the application. The web application uses a classical layering approach using the three layers _models and services_, _persistence_, and _controllers and data transfer objects (DTOs)_.

Our _models_ are basic mutable C# objects. These models are designed to map to relational database tables (although this example does not use a real database).

In the _persistence_ layer we use the **repository pattern**. For our task lists we have two implementations of repositories: `file system` (for running the application) and `in-memory` (for testing).

In the _controller_ layer we have the controllers that provide the entry points into the application.

We also have _unit tests_ for our repositories and API endpoints. Test tests for the API endpoints are integration tests that test the whole system without needing databases or external systems to be present. This is achieved by replacing repositories with their in-memory variants and by mocking adapters for external systems.

The code is fairly simple so we encourage you to take a look to familiarize yourself with it.
