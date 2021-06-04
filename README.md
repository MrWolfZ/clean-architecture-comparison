# Clean Architecture Comparison

This repository contains a comparison of multiple styles for implementing a [clean architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) in .NET (using [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0) and [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-5.0)).

> Note that _clean architecture_ is arguably just the latest of a long list of names (including [hexagonal / ports and adapters](https://en.wikipedia.org/wiki/Hexagonal_architecture_(software)) and [onion](https://medium.com/@shivendraodean/software-architecture-the-onion-architecture-1b235bec1dec) among others) that describe the same fundamental concept for defining your architecture: [dependency inversion](https://en.wikipedia.org/wiki/Dependency_inversion_principle). Therefore this comparison is also somewhat applicable to those styles, although the naming of concepts slightly differs.

The scenario we are using for showcasing the different styles is a simple task list management application with the following properties:

- the system manages _task lists_ that are owned by _users_
- a _task list_ has a unique-per-owner _name_ and contains _task list entries_
- a _task list entry_ has a description and can be marked as _done_
- there are _premium_ and non-_premium_ users
- non-_premium_ users are limited to at most one _task list_ with at most 5 items
- the system gathers statistics about changes that are made to the _task lists_ it manages

For the sake of simplicity our system contains no user interface, but only APIs. It also does not have any authentication or authorization, i.e. _users_ only exist as business objects.

## Baseline

To be able to compare the different styles it is useful to have a [baseline implementation](baseline#readme) of our application to compare against. This baseline is a simple implementation using basic architectural patterns and is something that most readers should be familiar with from textbooks or their studies.

## Core

Each compared style is a standalone application that has some common cross-cutting concerns (e.g. API documentation, testing infrastructure, base classes). To prevent having the code for these concerns duplicated in each application, we have a number of [core](core#readme) projects that provide this common code.

## Compared Styles

Below you can find a list of the various styles we are comparing. We recommend you to go through the list in order since some styles re-use concepts from other styles. We also encourage you to clone the repository and look at the code in your favorite IDE for easier navigation.

- (work-in-progress) [basic](basic#readme): in this style we simply split the baseline into layers according to clean architecture (i.e. _domain_, _application_, _infrastructure_, and _web_)
- (work-in-progress) [domain-driven design](ddd#readme) (_DDD_): an extension of the _basic_ style that uses concepts of [domain-driven design](https://en.wikipedia.org/wiki/Domain-driven_design) (i.e. aggregates, entities, domain events etc.)
- (work-in-progress) [command query separation](cqs#readme) (_CQS_): an extension of the _DDD_ style that models operations as _commands_ and _queries_
- (work-in-progress) [mediator](mediatr#readme): a variant of _CQS_ that uses the [mediator pattern](https://en.wikipedia.org/wiki/Mediator_pattern) for handling _commands_, _queries_, and _domain events_ (using the [MediatR](https://github.com/jbogard/MediatR) library)
- (work-in-progress) [decorator](decorator#readme): a variant of _CQS_ that uses the [decorator pattern](https://en.wikipedia.org/wiki/Decorator_pattern) for handling cross-cutting concerns (e.g. logging and validation) of our _command_ and _query_ handlers
- (work-in-progress) [proxy](proxy#readme): a variant of _CQS_ that uses the [proxy pattern](https://en.wikipedia.org/wiki/Proxy_pattern) for handling cross-cutting concerns of our _command_ and _query_ handlers (using the [Castle](https://github.com/castleproject/Core) library)
- (work-in-progress) [functional](functional#readme): a variant of _CQS_ that uses concepts of [functional programming](https://en.wikipedia.org/wiki/Functional_programming) instead of [object-oriented programming](https://en.wikipedia.org/wiki/Object-oriented_programming) (using the [language-ext](https://github.com/louthy/language-ext) library)

## Open Points

- add `README` for `basic`
- adjust `ddd` based on `basic`
- adjust `cqs` based on `ddd`
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
- create `mediatr`
- create `cqs-decorator`
- create `cqs-proxy`
