# Clean Architecture Comparison

This repository contains a comparison of multiple styles for implementing a [clean architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) in .NET (using [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0) and [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-5.0)).

> Note that _clean architecture_ is arguably just the latest of a long list of names (including [hexagonal / ports and adapters](https://en.wikipedia.org/wiki/Hexagonal_architecture_(software)) and [onion](https://medium.com/@shivendraodean/software-architecture-the-onion-architecture-1b235bec1dec) among others) that describe the same fundamental concept for defining your architecture: [dependency inversion](https://en.wikipedia.org/wiki/Dependency_inversion_principle). Therefore this comparison is also somewhat applicable to those styles, although the naming of concepts slightly differs.

The scenario we are using for showcasing the different styles is a simple task list management application.

Our domain model has the following properties:

- there are _task lists_ that are owned by _users_
- a _task list_ has a _name_ and contains _task list entries_
- a _task list entry_ has a description and can be marked as _done_
- there are _premium_ and non-_premium_ users

The following are our application's business rules:

- a _task list_'s name must be unique per owner
- non-_premium_ users are limited to at most one _task list_ with at most 5 _entries_

In addition to our business rules, our application has a few more behaviors:

- the system gathers statistics about changes that are made to the _task lists_ it manages
- whenever a _task list_ is created or changed the system publishes change notification to other systems

For the sake of simplicity our system contains no user interface, but only APIs. It also does not have any authentication or authorization, i.e. _users_ only exist as business objects.

## Baseline

To be able to compare the different styles it is useful to have a [baseline implementation](baseline#readme) of our application to compare against. This baseline is a simple implementation using basic architectural patterns and is something that most readers should be familiar with from textbooks or their studies.

## Domain-Driven Design

While not strictly necessary for a clean architecture, [domain-driven design](https://en.wikipedia.org/wiki/Domain-driven_design) (_DDD_) is a natural fit for the separation a clean architecture aims to achieve. Therefore, as a [first step](ddd#readme) on our journey towards clean architecture we refactor the _baseline_ to use concepts of _domain-driven design_ (i.e. aggregates, entities, domain events etc.).

## Core

Each compared style is a standalone application that has some common cross-cutting concerns (e.g. API documentation, testing infrastructure, base classes). To prevent having the code for these concerns duplicated in each application, we have a number of [core](core#readme) projects that provide this common code.

## Compared Styles

Below you can find a list of the various styles we are comparing. We recommend you to go through the list in order since some styles re-use concepts from other styles. We also encourage you to clone the repository and look at the code in your favorite IDE for easier navigation.

- (work-in-progress) [basic](basic#readme): in this style we simply split the _DDD_ example into layers according to clean architecture (i.e. _domain_, _application_, _infrastructure_, and _web_)
- (work-in-progress) [command query separation](cqs#readme) (_CQS_): an extension of the _basic_ style that models operations as _commands_ and _queries_
- (work-in-progress) [mediator](mediatr#readme): a variant of _CQS_ that uses the [mediator pattern](https://en.wikipedia.org/wiki/Mediator_pattern) for handling _commands_, _queries_, and _domain events_ (using the [MediatR](https://github.com/jbogard/MediatR) library)
- (work-in-progress) [decorator](decorator#readme): a variant of _CQS_ that uses the [decorator pattern](https://en.wikipedia.org/wiki/Decorator_pattern) for handling cross-cutting concerns (e.g. logging and validation) of our _command_ and _query_ handlers
- (work-in-progress) [proxy](proxy#readme): a variant of _CQS_ that uses the [proxy pattern](https://en.wikipedia.org/wiki/Proxy_pattern) for handling cross-cutting concerns of our _command_ and _query_ handlers (using the [Castle](https://github.com/castleproject/Core) library)
- (work-in-progress) [functional](functional#readme): a variant of _CQS_ that uses concepts of [functional programming](https://en.wikipedia.org/wiki/Functional_programming) instead of [object-oriented programming](https://en.wikipedia.org/wiki/Object-oriented_programming) (using the [language-ext](https://github.com/louthy/language-ext) library)

## Open Points

- add `README` for `ddd`
- add `README` for `basic`
- adjust `basic` based on `ddd`
- adjust `cqs` based on `basic`
  - split write and read repositories
- rename `TaskListsController` to `TaskListsApi` in CQS
- change inspection settings to not mark un-instantiated classes as suggestions but as hints
- change inspection settings to ignore files whose name ends on `Dto`, `Request`, `Response` etc.
- create `mediatr`
- create `cqs-decorator`
- create `cqs-proxy`
