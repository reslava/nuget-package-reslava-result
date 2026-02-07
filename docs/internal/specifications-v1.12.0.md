# REslava.Result v1.12.0 Specifications

## Project Overview
**Type**: NuGet package library with source generators  
**Target Frameworks**: .NET 10 (core library), .NET Standard 2.0 (source generators)  
**Language**: C# 14  
**Documentation**: Markdown with Mermaid diagrams  

## Core Libraries

### REslava.Result (Core Library)
**Purpose**: Complete result pattern implementation with CRTP for error handling

**Key Components:**
- **Reasons**: Error context and metadata
- **Results**: Success/failure result containers
- **Factories**: Result creation utilities
- **Extensions**: Map, Bind, Tap, LINQ operations
- **AdvancedPatterns**: OneOf discriminated unions (OneOf2, OneOf3, OneOf4)
- **ValidationRules**: Input validation framework

**Architecture**: CRTP (Curiously Recurring Template Pattern) for type-safe result chaining

### REslava.Result.SourceGenerator (Source Generators)
**Purpose**: ASP.NET Core integration with zero boilerplate

**Core Architecture:**
- **Core Infrastructure**: SOLID principles implementation
  - CodeGeneration framework
  - Configuration management
  - Infrastructure utilities
- **Interfaces**: Generator contracts
- **Utilities**: Common helper functions

**Generators:**
1. **ResultToIResult**: Converts Result<T> to ASP.NET Core IResult
2. **OneOf2ToIResult**: Converts OneOf<T1,T2> to IResult with intelligent HTTP mapping
3. **OneOf3ToIResult**: Converts OneOf<T1,T2,T3> to IResult with intelligent HTTP mapping
4. **OneOf4ToIResult**: Converts OneOf<T1,T2,T3,T4> to IResult with intelligent HTTP mapping
5. **SmartEndpoints**: Automatic endpoint generation for controllers

**SmartEndpoints Features:**
- Attribute-based endpoint generation
- Automatic HTTP method mapping
- Intelligent error handling
- Extension method generation for route registration

## Testing Strategy
**Framework**: MSTest  
**Test Projects:**
- REslava.Result.Tests: Core library unit tests
- REslava.Result.SourceGenerators.Tests: Source generator tests

**Coverage Areas:**
- Result pattern operations
- OneOf discriminated unions
- Source generator code generation
- ASP.NET Core integration
- Error handling scenarios

## Sample Applications

### Console Sample
**Project**: REslava.Result.Samples.Console  
**Purpose**: Demonstrate core library features without web dependencies

### ASP.NET Samples
**MinimalApi.Net10.Reference**: Pure ASP.NET Core Minimal API implementation for comparison

**MinimalApi.Net10.REslavaResult**: Full integration showcase
- All source generators active
- Zero boilerplate fat APIs
- SmartEndpoints demonstration
- Comprehensive error handling

## Documentation Structure
```
docs/
├── api/                          # Public API documentation
│   └── advanced-patterns/        # Advanced pattern docs
├── architecture/                 # Architecture concepts
├── github/                       # Release notes
└── internal/                     # Internal documentation (gitignored)
```

## Key Features v1.12.0

### Result Pattern
- Type-safe error handling with CRTP
- Fluent chaining with Map, Bind, Tap
- LINQ integration
- Validation rules framework

### OneOf Pattern
- Discriminated unions (2-4 types)
- Intelligent HTTP status mapping
- Error type detection
- Automatic serialization

### Source Generators
- Compile-time code generation
- Zero runtime overhead
- Intelligent error mapping
- Automatic endpoint generation

### SmartEndpoints
- Attribute-driven development
- Automatic route registration
- Built-in error handling
- Extension method generation

## Technical Specifications

### Dependencies
- .NET 10 for core library
- .NET Standard 2.0 for source generators
- ASP.NET Core for web integration
- Microsoft.CodeAnalysis for source generation

### Performance
- Compile-time generation (zero runtime overhead)
- Memory-efficient result types
- Optimized error handling
- Minimal allocation patterns

### Compatibility
- .NET 10+ applications
- ASP.NET Core 10+
- Visual Studio 2022+
- .NET CLI tooling

## Integration Points
- ASP.NET Core Minimal APIs
- MVC Controllers
- Web API applications
- Console applications
- Background services
