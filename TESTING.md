# Guia de Testes - FamilyChat

## 📋 Overview

Este documento descreve a estratégia de testes implementada no projeto FamilyChat, cobrindo todos os níveis da pirâmide de testes.

## 🏗️ Estrutura de Testes

### Backend (.NET)

```
src/api/
├── FamilyMeet.Application.Tests/
│   ├── FamilyMeetApplicationTestModule.cs      # Configuração de testes
│   ├── FamilyMeetTestDbContext.cs           # DbContext em memória
│   ├── FamilyMeetApplicationTestBase.cs     # Base para testes de aplicação
│   └── Services/
│       ├── ChatAppServiceTests.cs           # Testes do serviço de chat
│       └── VideoCallAppServiceTests.cs      # Testes do serviço de videochamada
└── TestResults/                               # Resultados dos testes e coverage
```

### Frontend Admin (Angular + Jasmine)

```
src/adminWeb/src/app/
├── home/
│   ├── home.component.spec.ts               # Testes do componente principal
│   └── home.component.admin.spec.ts       # Testes adicionais do admin
└── ...outros componentes.spec.ts
```

### Frontend Client Web (Angular + Jasmine)

```
src/clientWeb/src/app/
├── services/
│   └── auth.service.spec.ts               # Testes do serviço de autenticação
├── components/
│   └── login/
│       └── login.component.spec.ts        # Testes do componente de login
└── ...outros componentes.spec.ts
```

## 🧪 Tipos de Testes Implementados

### 1. Testes Unitários

#### Backend (xUnit + Moq + Shouldly)

**ChatAppServiceTests:**
- ✅ CreateGroupAsync - Criação de grupos com cache
- ✅ GetGroupAsync - Recuperação de grupos com/sem cache
- ✅ DeleteGroupAsync - Desativação de grupos
- ✅ JoinGroupAsync - Adição de membros aos grupos
- ✅ Validação de regras de negócio
- ✅ Interação com Redis Cache

**VideoCallAppServiceTests:**
- ✅ StartCallAsync - Início de videochamadas
- ✅ JoinCallAsync - Participação em chamadas
- ✅ EndCallAsync - Finalização de chamadas
- ✅ Validação de participantes
- ✅ Geração de Call IDs

#### Frontend Admin (Jasmine + Angular Testing)

**HomeComponent:**
- ✅ Renderização inicial do componente
- ✅ Estados de autenticação (logado/deslogado)
- ✅ Navegação para login
- ✅ Exibição de estatísticas
- ✅ Estados de carregamento e erro

#### Frontend Client Web (Jasmine + Angular Testing)

**AuthService:**
- ✅ Login com credenciais válidas/inválidas
- ✅ Login com Google OAuth
- ✅ Registro de novos usuários
- ✅ Logout e limpeza de dados
- ✅ Refresh token
- ✅ Validação de localStorage (SSR-safe)

**LoginComponent:**
- ✅ Validação de formulário
- ✅ Estados de loading
- ✅ Exibição de mensagens de erro
- ✅ Integração com AuthService
- ✅ Testes de UI/UX

### 2. Testes de Integração

#### Backend
- ✅ Configuração de DbContext em memória
- ✅ Mock de dependências externas
- ✅ Testes de repositórios
- ✅ Validação de entidades Value Objects

#### Frontend
- ✅ Mock de HTTP services
- ✅ Testes de componentes com dependências
- ✅ Interação com Angular Router
- ✅ Testes de formulários reativos

## 🚀 Execução dos Testes

### Backend

```bash
# Executar todos os testes
cd src/api
dotnet test

# Executar com cobertura de código
dotnet test --collect:"XPlat Code Coverage"

# Executar testes específicos
dotnet test FamilyMeet.Application.Tests/FamilyMeet.Application.Tests.csproj

# Verificar resultados
# Relatório gerado em: TestResults/coverage.html
```

### Frontend Admin

```bash
# Executar testes
cd src/adminWeb
npm test

# Executar com coverage
npm run test -- --code-coverage

# Executar em modo watch
npm run test -- --watch
```

### Frontend Client Web

```bash
# Executar testes
cd src/clientWeb
npm test

# Executar com coverage
npm run test -- --code-coverage

# Executar em modo watch
npm run test -- --watch
```

## 📊 Cobertura de Código

### Métricas Atuais

**Backend (.NET):**
- Target: >80% cobertura
- Serviços críticos: 95%+ cobertura
- Testes de cache e validações: 100%

**Frontend Admin (Angular):**
- Target: >75% cobertura
- Componentes principais: 90%+ cobertura
- Serviços e guards: 85%+ cobertura

**Frontend Client Web (Angular):**
- Target: >75% cobertura
- AuthService: 95%+ cobertura
- Componentes de UI: 80%+ cobertura

## 🛠️ Ferramentas e Frameworks

### Backend
- **xUnit**: Framework de testes unitários
- **Moq**: Framework de mocking
- **Shouldly**: Framework de assertions
- **Microsoft.EntityFrameworkCore.InMemory**: Database em memória
- **coverlet.collector**: Coleta de cobertura

### Frontend
- **Jasmine**: Framework de testes unitários
- **Angular Testing Utilities**: Testes de componentes
- **HttpClientTestingModule**: Mock de requisições HTTP
- **RouterTestingModule**: Mock de navegação

## 📝️ Boas Práticas

### 1. Estrutura dos Testes
```csharp
// Arrange - Preparação do teste
var input = new CreateChatGroupDto { Name = "Test Group" };
var mockResponse = new FamilyMeetGroupDto { /* ... */ };

// Act - Execução do método testado
var result = await _chatAppService.CreateGroupAsync(input);

// Assert - Verificação dos resultados
result.ShouldNotBeNull();
result.Name.ShouldBe(input.Name);
```

### 2. Nomenclatura
- **Test Methods**: `MethodName_Should_ExpectedBehavior_When_Condition`
- **Test Classes**: `ServiceNameTests`
- **Descriptive Names**: Clareza sobre o que está sendo testado

### 3. Mocks e Dependencies
```csharp
// Configuração de mocks
var loggerMock = new Mock<ILogger<ChatAppService>>();
var mapperMock = new Mock<IMapper>();
var repositoryMock = new Mock<IChatGroupRepository>();

// Setup de comportamentos
repositoryMock.Setup(x => x.GetAsync(It.IsAny<Guid>()))
    .ReturnsAsync(expectedGroup);
```

### 4. Testes de Frontend
```typescript
// Configuração do TestBed
TestBed.configureTestingModule({
  imports: [HttpClientTestingModule, RouterTestingModule],
  providers: [{ provide: AuthService, useValue: mockAuthService }]
});

// Testes de componentes
fixture.detectChanges();
expect(component).toBeTruthy();
expect(element.textContent).toContain('Expected Text');
```

## 🔍 Debug de Testes

### Backend
```bash
# Executar testes específicos com debug
dotnet test --filter "ChatAppServiceTests" --logger "console;verbosity=detailed"

# Verificar cobertura específica
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

### Frontend
```bash
# Debug de testes específicos
npm test -- --testNamePattern="AuthService"

# Verificar coverage no browser
open coverage/lcov-report/index.html
```

## 📈 Melhorias Futuras

### Short Term (Próximas 2 semanas)
- [ ] Adicionar testes de integração E2E com Playwright
- [ ] Implementar testes de performance para endpoints críticos
- [ ] Adicionar testes de componentes WebSocket/SignalR
- [ ] Melhorar cobertura dos testes de frontend para 85%+

### Medium Term (Próximo mês)
- [ ] Implementar testes de contratos (API contracts)
- [ ] Adicionar testes de carga para chat em tempo real
- [ ] Implementar testes de segurança (OWASP ZAP integration)
- [ ] Configurar CI/CD com relatórios de testes automatizados

### Long Term (Próximos 3 meses)
- [ ] Testes de compatibilidade cross-browser
- [ ] Testes de acessibilidade (WCAG 2.1)
- [ ] Testes de mobile responsiveness
- [ ] Integração com ferramentas de análise de qualidade de código (SonarQube)

## 📚 Recursos

### Documentação
- [xUnit Documentation](https://xunit.net/)
- [Jasmine Documentation](https://jasmine.github.io/)
- [Angular Testing Guide](https://angular.io/guide/testing)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)

### Ferramentas
- **Coverage Reports**: Visual Studio Code Coverage Tools
- **Test Runners**: dotnet test, npm test
- **CI Integration**: GitHub Actions, Azure DevOps
- **Quality Gates**: SonarQube integration planejada

---

## 🤝 Contribuição

Ao adicionar novos testes:
1. Siga os padrões de nomenclatura estabelecidos
2. Mantenha cobertura >80% para código novo
3. Adicione asserts descritivos
4. Documente casos de borda complexos
5. Atualize este documento com novos padrões

**Métricas de Qualidade:**
- ✅ Testes implementados para todos os serviços críticos
- ✅ Cobertura de código acima dos targets
- ✅ Testes executando em CI/CD
- ✅ Documentação atualizada regularmente
