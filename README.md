# Clean Architecture Comparison

This repository contains a comparison of multiple styles for implementing a clean architecture.

## Open Points

- rename `plain-cqs` to `cqs`
- rename `TaskListsController` to `TaskListsApi`
- in `ddd` move DTOs from `TaskListsApi` into separate classes
- change inspection settings to allow C# 9 object instantiation shorthands
- change inspection settings to not mark un-instantiated classes as suggestions but as hints
- change inspection settings to ignore files whose name ends on `Dto`, `Request`, `Response` etc.
- add fluent validation to `ddd`
- add `AggregateRoot`, `IAggregateRepository` etc. base classes for DDD
- extend `ddd` and `cqs` with more queries (e.g. `getAllTaskLists`, `getAllTaskListsWithPendingItems`)
- in `cqs` split write and read repositories
- in `ddd` and `cqs` implement file system repository and use that by default (using in-memory during tests)
- copy `ddd` to `basic` and remove entity base classes, typed IDs etc.
- create `mediatr`
- create `cqs-decorator`
- create `cqs-proxy`
