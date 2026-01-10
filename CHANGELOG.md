# Changelog

All notable changes to this project will be documented in this file. See [commit-and-tag-version](https://github.com/absolute-version/commit-and-tag-version) for commit guidelines.

## [0.4.0](https://github.com/reslava/nuget-package-reslava-result/compare/v0.3.1...v0.4.0) (2026-01-10)


### ✨ Features

* **result:** complete core, factory, convert from methods, delete ok(string) method ([523ef63](https://github.com/reslava/nuget-package-reslava-result/commit/523ef6348d28e47cdd96a6db4db8b53c7e499564))


### ♻️ Code Refactoring

* **result:** use concrete interface ([65d2770](https://github.com/reslava/nuget-package-reslava-result/commit/65d2770faa3e0e87ec1e61e9ed0568dd0f3301ee))

## [0.3.1](https://github.com/reslava/nuget-package-reslava-result/compare/v0.3.0...v0.3.1) (2026-01-10)


### 📚 Documentation

* **quick-start:** update ([f40915e](https://github.com/reslava/nuget-package-reslava-result/commit/f40915e73f9035bff6b34882759a8fda0ae44bb1))
* **quick-start:** update ([83c49ca](https://github.com/reslava/nuget-package-reslava-result/commit/83c49ca6b38d282cf38499b8155659abf12ad419))
* **quick-start:** update tests docs branch strategies ([b29c210](https://github.com/reslava/nuget-package-reslava-result/commit/b29c210c3cea2d6702990b8cd542128fe0c58fda))


### ✅ Tests

* **result:** map ([b0a0a4a](https://github.com/reslava/nuget-package-reslava-result/commit/b0a0a4acdc72ed47155bf3224864f6b9f46f3e78))

## [0.3.0](https://github.com/reslava/nuget-package-reslava-result/compare/v0.2.0...v0.3.0) (2026-01-09)


### ✨ Features

* add custom error for cheking ([51e8df3](https://github.com/reslava/nuget-package-reslava-result/commit/51e8df3fac405fcd9e260d3ca883cdd0dcde553f))
* **commit-and-tag-version:** uninstall deprecated standard-version, update QUICK-START ([f24da39](https://github.com/reslava/nuget-package-reslava-result/commit/f24da3928e6c7a65aa0eb073808b8513196698e9))
* **result:** implement Bind, Match ([a4ab370](https://github.com/reslava/nuget-package-reslava-result/commit/a4ab370797ba8b18b7dd2b7f6df0ebfeafdd9f29))
* **result:** implement Map ([322fc7b](https://github.com/reslava/nuget-package-reslava-result/commit/322fc7b4453eae6a72cfd98b08444a4a2fd51963))


### 🐛 Bug Fixes

* code style ([5e1a537](https://github.com/reslava/nuget-package-reslava-result/commit/5e1a5373c32ece601b96a644fc330dae924183fa))


### 📚 Documentation

* **pull_request_template:** add PULL_REQUEST_TEMPLATE.md ([0272e89](https://github.com/reslava/nuget-package-reslava-result/commit/0272e89fd7d45b27b6b80edc6ee9e1a3934bd3f0))
* **workflow:** add dev branch workflow and release documentation ([1db34c8](https://github.com/reslava/nuget-package-reslava-result/commit/1db34c8f82689befeda6b42c26dcc500adb40346))


### 🔧 Build System

* add code analysis and suppress CS1591 warnings ([6ce1db2](https://github.com/reslava/nuget-package-reslava-result/commit/6ce1db216059cb7c91b96add9394b079794db422))
* simplify config using built-in .NET support ([9931ca3](https://github.com/reslava/nuget-package-reslava-result/commit/9931ca3afb7689a596db7a1249b39e5503ef4e21))

## [0.2.0](https://github.com/reslava/nuget-package-reslava-result/compare/v0.1.0...v0.2.0) (2026-01-07)


### 🔧 Build System

* fix .versionrc.json to properly update .csproj ([1de1805](https://github.com/reslava/nuget-package-reslava-result/commit/1de1805065d0a9ce3574ee6eef36b286fd48cb16))

## [0.1.0](https://github.com/reslava/nuget-package-reslava-result/compare/v0.0.1...v0.1.0) (2026-01-07)


### ✨ Features

* add custom error for checking ([7de3e23](https://github.com/reslava/nuget-package-reslava-result/commit/7de3e2322a4a0c67646c3ffbf1d0279d9df7de61))
* **core:** add base interface IResult.cs (missed) ([104a540](https://github.com/reslava/nuget-package-reslava-result/commit/104a540262ea149dcc16bc28466424cdb01c3ea1))


### 📚 Documentation

* **changelog:** add CHANGELOG for v0.0.1 ([1636ce6](https://github.com/reslava/nuget-package-reslava-result/commit/1636ce68e080ebb39a099027cb890106bdf9f9a9))
* **workflow:** add dev branch workflow and release documentation ([9f2a7cc](https://github.com/reslava/nuget-package-reslava-result/commit/9f2a7ccb8121be90b62d0741370adc1731e79c0b))


### 🔧 Build System

* add Version tag to .csproj for automated versioning ([6cabf56](https://github.com/reslava/nuget-package-reslava-result/commit/6cabf56732fe856d22e87107e2e586ba50b58288))

## [0.0.1] - 2026-01-07

### Added

- Initial implementation with clean history
- Conventional commits enabled
- Core Result pattern implementation
- Comprehensive test suite
- Full documentation
- Windows-compatible configuration

### Commits

- chore: initial project setup with automation
- feat(core): add base interfaces for Result pattern
- feat(reasons): implement Reason base classes using CRTP
- feat(result): implement Result and Result<T> classes
- feat(factory): add Ok and Fail factory methods
- test(result): add unit tests for Result creation
- build: add .NET project configuration
- docs: add comprehensive documentation

[0.0.1]: https://github.com/reslava/nuget-package-reslava-result/releases/tag/v0.0.1
