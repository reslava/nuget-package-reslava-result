# REslava.Result Project Naming Convention

## 📋 Naming Convention Strategy

### 🎯 Core Principle
- **Default**: `projectname.csproj` → Uses REslava.Result only
- **External**: `projectname.external-libraryname.csproj` → Uses external library called `libraryname`

---

## 📋 Project Structure

### ✅ Default Projects (Self-Contained)
| Project | Location | OneOf Implementation | Dependencies |
|---------|----------|---------------------|-------------|
| **OneOfTest.Api** | `samples/OneOfTest.Api/` | REslava.Result OneOf | REslava.Result only |
| **OneOfApi.IntegrationTests** | `SourceGenerator/Tests/OneOfApi.IntegrationTests/` | REslava.Result OneOf | REslava.Result only |

### 🔶 External Projects (Explicit)
| Project | Location | OneOf Implementation | Dependencies |
|---------|----------|---------------------|-------------|
| **OneOfTest.Api.external-oneof** | `samples/OneOfTest.Api.external-oneof/` | External OneOf 3.0.26 | OneOf 3.0.26 |
| **OneOfApi.IntegrationTests.external-oneof** | `SourceGenerator/Tests/OneOfApi.IntegrationTests.external-oneof/` | External OneOf 3.0.26 | OneOf 3.0.26 |

---

## 📋 Naming Examples

### ✅ Default (REslava.Result Only)
- `BasicWebApi.csproj` → Uses REslava.Result OneOf
- `AdvancedWebApi.csproj` → Uses REslava.Result OneOf
- `ConsoleApp.csproj` → Uses REslava.Result OneOf

### 🔶 External (Explicit Library)
- `BasicWebApi.external-oneof.csproj` → Uses external OneOf library
- `AdvancedWebApi.external-newtonsoft.csproj` → Uses external Newtonsoft library
- `ConsoleApp.external-automapper.csproj` → Uses external AutoMapper library

---

## 📋 Benefits of This Convention

### ✅ Clear Implementation Identification
- **No suffix** = REslava.Result implementation
- **`.external-libraryname`** = External library implementation
- **No ambiguity** about dependencies

### ✅ User Choice
- **Default**: Self-contained REslava.Result
- **External**: Specific external library
- **Migration**: Clear upgrade path

### ✅ Development Clarity
- **Team knows** implementation from project name
- **Dependencies clear** from naming
- **Testing focused** on correct implementation

---

## 📋 Usage Guidelines

### ✅ For New Users
- Use **default projects** (no suffix)
- **REslava.Result only** implementation
- **No external dependencies**

### ✅ For External Dependencies
- Use **`.external-libraryname`** suffix
- **Clear indication** of external library
- **Explicit dependency** management

### ✅ For Migration
- **Start with external** version
- **Migrate to default** version
- **Clear naming** prevents confusion

---

## 📋 Project Directory Structure

```
samples/
├── OneOfTest.Api/                          # Default - REslava.Result OneOf
└── OneOfTest.Api.external-oneof/           # External - OneOf 3.0.26

SourceGenerator/Tests/
├── OneOfApi.IntegrationTests/               # Default - REslava.Result OneOf
└── OneOfApi.IntegrationTests.external-oneof/ # External - OneOf 3.0.26
```

---

## 📋 Implementation Rules

### ✅ Default Projects
- **Project name**: `projectname.csproj`
- **Dependencies**: REslava.Result only
- **Usage**: Primary implementation
- **Target**: New users and production

### 🔶 External Projects
- **Project name**: `projectname.external-libraryname.csproj`
- **Dependencies**: External library + REslava.Result
- **Usage**: Specific external library integration
- **Target**: Migration and compatibility

---

## 🎯 Summary

**This naming convention provides:**
- ✅ **Clear implementation identification** from project name
- ✅ **Default behavior** uses REslava.Result only
- ✅ **External dependencies** explicitly named
- ✅ **No ambiguity** about library usage
- ✅ **Migration path** clearly defined

---

*Last Updated: February 1, 2026*
*Version: 1.0*
*Status: Implemented*
