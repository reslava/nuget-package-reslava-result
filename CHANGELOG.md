# Changelog

All notable changes to this project will be documented in this file. See [commit-and-tag-version](https://github.com/absolute-version/commit-and-tag-version) for commit guidelines.

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
