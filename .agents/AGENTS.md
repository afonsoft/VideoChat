# 🤖 GEMINI.md - Comprehensive Guide for .NET Development with GitHub Copilot

## 🎯 Overview

This document serves as the central hub for all GitHub Copilot customizations, agents, skills, and rules specifically optimized for .NET, C#, and modern development practices with Gemini API integration.

## 📁 Structure

```
agents/
├── GEMINI.md                    # This master guide
├── skills/                      # Reusable skill packages
│   ├── dotnet-best-practices/
│   ├── csharp-async-patterns/
│   ├── entity-framework-core/
│   ├── aspnet-core-api/
│   ├── testing-xunit/
│   ├── blazor-components/
│   ├── design-patterns/
│   ├── security-jwt/
│   ├── performance-optimization/
│   ├── angular-component/
│   ├── angular-di/
│   ├── angular-directives/
│   ├── angular-forms/
│   ├── angular-http/
│   ├── angular-routing/
│   ├── angular-signals/
│   ├── angular-ssr/
│   ├── angular-testing/
│   ├── angular-tooling/
│   ├── csharp-concurrency-patterns/
│   ├── csharp-developer/
│   ├── dotnet-10-csharp-14/
│   ├── dotnet-architect/
│   ├── dotnet-backend-patterns/
│   ├── dotnet-core-expert/
│   ├── dotnet-project-structure/
│   ├── efcore-patterns/
│   ├── find-skills/
│   └── modern-csharp-coding-standards/
└── rules/                       # Project-specific instructions
    ├── csharp-coding-standards.md
    ├── dotnet-project-structure.md
    ├── api-design-guidelines.md
    ├── database-conventions.md
    └── git-workflow.md
```

## 🚀 Key Components

### 1. Skills (`.github/skills/`)

Self-contained folders with `SKILL.md` files that provide reusable capabilities for GitHub Copilot.

#### Available Skills:

**🏗️ Core Development Skills**
- `dotnet-best-practices`: Ensures code follows .NET conventions and best practices
- `csharp-async-patterns`: Best practices for asynchronous programming in C#
- `design-patterns`: Implementation of SOLID principles and Gang of Four patterns
- `entity-framework-core`: EF Core best practices and optimization techniques
- `efcore-patterns`: Advanced Entity Framework Core patterns and practices
- `modern-csharp-coding-standards`: Modern C# coding standards and conventions

**🌐 Web Development Skills**
- `aspnet-core-api`: RESTful API development with ASP.NET Core
- `blazor-components`: Modern Blazor component development
- `security-jwt`: JWT authentication and authorization patterns
- `api-design-guidelines`: RESTful API design principles

**🧪 Testing & Quality Skills**
- `testing-xunit`: Unit testing with xUnit and best practices
- `integration-testing`: Integration testing strategies
- `performance-optimization`: Performance profiling and optimization
- `code-review-automation`: Automated code review patterns

**🎨 Angular Development Skills**
- `angular-component`: Modern Angular standalone components with v20+ best practices
- `angular-di`: Dependency injection in Angular v20+ using inject() and providers
- `angular-directives`: Custom directives for DOM manipulation and behavior extension
- `angular-forms`: Signal-based forms in Angular v21+ with automatic two-way binding
- `angular-http`: HTTP data fetching using resource(), httpResource(), and HttpClient
- `angular-routing`: Routing with lazy loading, functional guards, and route parameters
- `angular-signals`: Signal-based reactive state management in Angular v20+
- `angular-ssr`: Server-side rendering and hydration with @angular/ssr
- `angular-testing`: Unit and integration tests with Vitest/Jasmine and TestBed
- `angular-tooling`: Angular CLI, code generation, build, and development tools

**⚡ Advanced .NET Skills**
- `csharp-concurrency-patterns`: Choosing the right concurrency abstraction in .NET
- `csharp-developer`: Building C# applications with .NET 8+, ASP.NET Core APIs, and Blazor
- `dotnet-10-csharp-14`: .NET 10 and C# 14 development with minimal APIs and modern patterns
- `dotnet-architect`: Expert .NET backend architect specializing in C#, ASP.NET Core, and EF Core
- `dotnet-backend-patterns`: Master C#/.NET backend development patterns for robust APIs
- `dotnet-core-expert`: .NET 8 applications with minimal APIs, clean architecture, and cloud-native
- `dotnet-project-structure`: Modern .NET project structure with .slnx and central package management
- `find-skills`: Helps users discover and install agent skills when needed

**🔧 Utility Skills**
- `find-skills`: Discover and install available agent skills for enhanced capabilities

### 2. Rules (`.github/instructions/`)

Context-specific instructions that Copilot reads when working on matching files.

#### Available Rules:

**📝 Coding Standards**
- `csharp-coding-standards.md`: C# naming conventions and formatting
- `dotnet-project-structure.md`: Solution and project organization
- `api-design-guidelines.md`: RESTful API design principles

**🗄️ Data & Architecture**
- `database-conventions.md`: Database naming and design patterns
- `microservices-patterns.md`: Microservices architecture guidelines
- `domain-driven-design.md`: DDD implementation patterns

**🔄 Workflow & Process**
- `git-workflow.md`: Git branching and commit conventions
- `ci-cd-pipelines.md`: Azure DevOps/GitHub Actions patterns
- `documentation-standards.md`: XML documentation and README standards

## 📊 Skill Statistics

**Total Skills Available**: 33 specialized skills

**By Category:**
- **Core Development**: 6 skills
- **Web Development**: 4 skills  
- **Testing & Quality**: 4 skills
- **Angular Development**: 10 skills
- **Advanced .NET**: 8 skills
- **Utility**: 1 skill

**By Technology:**
- **.NET/C#**: 18 skills
- **Angular**: 10 skills
- **Testing**: 4 skills
- **Architecture**: 6 skills
- **Performance**: 2 skills

## 🎯 Usage Instructions

### Activating Skills

Skills can be activated in several ways:

1. **Command Palette**: `Ctrl+Shift+P` → "Copilot: Use Skill"
2. **Chat Command**: `/skill <skill-name>` in Copilot Chat
3. **Auto-discovery**: Agents automatically discover relevant skills

### Applying Rules

Rules are automatically applied when working on matching files:

```bash
# Files matching these patterns will trigger rules:
**/*.cs          # C# coding standards
**/*.csproj      # Project structure rules
**/*.csx         # Script file conventions
**/Controllers/**.cs  # API design guidelines
**/Models/**.cs      # Entity conventions
```

## 🛠️ Configuration

### VS Code Settings

Ensure these settings are in your `settings.json`:

```json
{
  "github.copilot.chat.codeGeneration.useInstructionFiles": true,
  "github.copilot.chat.useProjectTemplates": true,
  "chat.promptFiles": true,
  "chat.promptFilesLocations": {
    ".github/prompts": true
  },
  "chat.modeFilesLocations": {
    ".github/agents": true
  },
  "chat.useAgentsMdFile": true,
  "chat.useNestedAgentsMdFiles": true
}
```

### Environment Setup

1. **Gemini API Configuration**: Ensure API key is configured in settings
2. **Project Structure**: Follow the documented folder structure
3. **Git Integration**: Enable GitLens and Copilot Git integration

## 📋 Best Practices

### For Skills Development

1. **Kebab-case naming**: Use hyphens in folder and skill names
2. **Clear descriptions**: Provide detailed when-to-use guidance
3. **Bundled assets**: Include templates and reference materials
4. **Step-by-step workflows**: Clear, actionable instructions
5. **Troubleshooting**: Common issues and solutions

### For Rules Creation

1. **Specific patterns**: Use precise glob patterns for `applyTo`
2. **Context-aware**: Rules should match file types and scenarios
3. **Concise instructions**: Clear, actionable guidance
4. **Examples**: Provide good and bad examples
5. **References**: Link to documentation and standards

## 🎨 Agent Integration

### Available Agents

1. **C# Expert**: General C# development and best practices
2. **.NET Upgrade**: Framework migration and modernization
3. **API Architect**: RESTful API design and implementation
4. **Database Expert**: EF Core and database design
5. **Testing Specialist**: Unit and integration testing strategies
6. **Angular Expert**: Modern Angular development with standalone components
7. **Frontend Architect**: Web frontend architecture and optimization
8. **DevOps Engineer**: CI/CD pipelines and deployment automation
9. **Security Specialist**: Application security and authentication patterns

### Agent Configuration

Agents are defined in `.github/agents/*.agent.md` files:

```yaml
---
name: C#/.NET Expert
description: Expert in modern C#/.NET development with Gemini API integration
model: gemini-1.5-flash
tools: ["editor", "terminal", "git"]
mcp-servers:
  - name: filesystem
    command: npx
    args: ["-y", "@modelcontextprotocol/server-filesystem"]
---
```

## 🔧 Advanced Features

### MCP Server Integration

Leverage Model Context Protocol servers for enhanced capabilities:

- **Filesystem**: File operations and project structure
- **Git**: Version control and repository management
- **Database**: Database schema and migrations
- **Web**: HTTP requests and API testing

### Custom Prompts

Create reusable prompts in `.github/prompts/*.prompt.md`:

```markdown
---
name: generate-repository-pattern
description: Generate Repository pattern implementation for Entity Framework Core
---

Generate a complete Repository pattern implementation with:
- Generic repository interface
- Specific repository implementations
- Unit of Work pattern
- Async methods
- Error handling
- Unit tests
```

## 📊 Performance Optimization

### Gemini API Optimization

1. **Context management**: Use efficient context windows
2. **Token optimization**: Minimize token usage in prompts
3. **Caching strategies**: Cache common responses
4. **Batch operations**: Group similar requests

### Development Workflow

1. **Skill chaining**: Combine multiple skills for complex tasks
2. **Agent collaboration**: Use multiple agents for different aspects
3. **Progressive enhancement**: Start with basic, add complexity gradually
4. **Feedback loops**: Continuously refine based on results

## 🚨 Troubleshooting

### Common Issues

1. **Skills not discovered**: Check folder structure and naming
2. **Rules not applying**: Verify glob patterns and file matching
3. **Agent not responding**: Check MCP server configuration
4. **Performance issues**: Optimize prompts and context usage
5. **Angular skills not working**: Ensure Angular CLI is installed and project is Angular v20+
6. **.NET 10 skills not available**: Verify .NET 10 SDK is installed
7. **Gemini API rate limits**: Check API key quota and usage

### Debug Tools

1. **Copilot Chat Debug**: Enable developer mode in VS Code settings
2. **Skill Discovery**: Use `Ctrl+Shift+P` → "Copilot: List Available Skills"
3. **Rule Application**: Check `.github/instructions/` folder for active rules
4. **Agent Logs**: Review agent execution logs in VS Code output panel

## 📚 Additional Resources

### Documentation
- [GitHub Copilot Documentation](https://docs.github.com/en/copilot)
- [Gemini API Documentation](https://ai.google.dev/docs)
- [Angular v20+ Documentation](https://angular.dev/)
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)

### Community & Support
- [GitHub Copilot Community](httpsgithub.com/github/copilot/discussions)
- [Stack Overflow .NET Tag](https://stackoverflow.com/questions/tagged/.net)
- [Angular Community](https://discord.gg/angular)
- [Microsoft Q&A](https://learn.microsoft.com/en-us/answers/)

### Best Practices
- Follow semantic versioning for skill updates
- Use descriptive commit messages for skill changes
- Test skills in isolated environments before deployment
- Document skill dependencies and prerequisites
- Provide migration guides for breaking changes

## 🔄 Updates & Maintenance

### Regular Tasks
- **Monthly**: Review and update skills based on new framework releases
- **Quarterly**: Audit skill usage and remove deprecated skills
- **Bi-annually**: Major refactoring and architecture updates
- **As needed**: Bug fixes and security updates

### Version Control
- Use semantic versioning for skill releases
- Tag releases with descriptive names
- Maintain CHANGELOG.md for each skill
- Use feature branches for major changes

---

**Last Updated**: 2026-03-18  
**Version**: 2.1.0  
**Total Skills**: 33 specialized skills  
**Supported Technologies**: .NET 8+, .NET 10, Angular v20+, C# 12+, Entity Framework Core, Gemini API
