# Domain-Driven Design

This file is a _work-in-progress_.

- strongly typed IDs
- rich domain model
  - mutating access only through aggregate root
- immutability
- domain invariants and exceptions
- domain events for decoupling
  - publish-after-commit vs commit-after-publish
  - mention transactionality and outbox pattern (e.g. storing events on DB for later processing)
- difference between an integration and a domain event
  - also mention push/pull pattern
