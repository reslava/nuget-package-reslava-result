# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) guideline.

## [1.11.0] - 2025-02-05

### 🎯 Added
- **SmartEndpoints Generator** - Complete Zero Boilerplate API generation
  - Automatic route generation with parameter awareness
  - Intelligent HTTP method detection (GET/POST/PUT/DELETE)
  - Route prefix support via `[AutoGenerateEndpoints(RoutePrefix = "...")]`
  - Full integration with existing OneOf2/OneOf3 extensions
  - Comprehensive error handling with automatic HTTP status mapping

### 🔄 Changed
- **Route Inference** - Enhanced to include `{id}` parameters when needed
- **OneOf Integration** - SmartEndpoints now uses existing OneOf extensions
- **Generated Code** - Cleaned up debug code and production-ready

### 🧪 Fixed
- **SmartEndpoints Warnings** - Resolved null reference warnings
- **Route Generation** - Fixed parameter-aware route inference
- **Test Coverage** - Added comprehensive MSTest suite for SmartEndpoints

### ⚠️ Breaking Changes
- **SmartEndpoints Route Inference** - Generated routes now properly include `{id}` parameters
  - Routes may change from `/api/User` to `/api/User/{id}` for methods with ID parameters
  - This improves route correctness and is a recommended update

### 📚 Documentation
- Updated README with comprehensive SmartEndpoints examples
- Added breaking changes documentation
- Enhanced troubleshooting section

---

## [1.10.3] - 2025-02-05

### 🎯 Added
- **OneOf2ToIResult Generator** - Two-type error handling
- **OneOf3ToIResult Generator** - Three-type error handling
- **Intelligent HTTP Mapping** - Automatic error type detection
- **Comprehensive Error Coverage** - All common error scenarios

### 🔄 Changed
- **Error Detection** - Smart error type identification
- **HTTP Status Mapping** - Automatic response code generation

---

## [1.10.2] - 2025-02-05

### 🎯 Added
- **ResultToIResult Generator** - Basic Result<T> conversion
- **HTTP Status Mapping** - Intelligent error response generation
- **ProblemDetails Support** - Structured error responses

### 🔄 Changed
- **Core Library** - Enhanced error handling capabilities

---

## [1.10.1] - 2025-02-05

### 🎯 Added
- **Initial Release** - Core Result types
- **Error Handling** - Basic error type definitions
- **HTTP Integration** - ASP.NET Core IResult support

### 🔄 Changed
- **Initial Setup** - Project structure and packaging

---

## [1.10.0] - 2025-02-05

### 🎯 Added
- **Framework Foundation** - Railway-oriented programming patterns
- **Result Types** - Success, Error, ValidationError types
- **Basic HTTP Integration** - IResult conversion methods

### 🔄 Changed
- **Initial Setup** - Project structure and packaging
