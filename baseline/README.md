# Baseline

This is a simple implementation of our application using basic architectural patterns. We have a single _web_ project that contains all the logic for the application. The web application uses a classical layering approach using the three layers _model_, _data_, and _controllers_.

Our _models_ are basic mutable C# objects.

In the _data_ layer we use the **repository pattern**. For our task lists we have two implementations of repositories: `file system` (for running the application) and `in-memory` (for testing).

In the _controller_ layer we have the controllers that provide the entry points into the application.

We also have _unit tests_ for each layer.

The code is fairly simple so we encourage you to take a look to familiarize with it.
