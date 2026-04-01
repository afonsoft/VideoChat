# FamilyMeet - Estrutura Completa do Projeto

## 🏗️ Arquitetura Geral

```
FamilyMeet/
├── src/
│   ├── api/                    # API Backend (.NET 8)
│   │   ├── FamilyMeet.HttpApi/
│   │   ├── FamilyMeet.Application/
│   │   ├── FamilyMeet.Domain/
│   │   ├── FamilyMeet.Domain.Shared/
│   │   └── FamilyMeet.EntityFrameworkCore/
│   ├── clientWeb/               # Frontend Chat Simples (Angular)
│   └── adminWeb/                # Frontend Admin (ABP Angular)
│       ├── angular/               # Frontend admin
│       └── aspnet-core/         # Backend admin
├── .github/workflows/           # GitHub Actions
├── docker-compose.yml           # Containers
└── README.md
```

## 📱 clientWeb - Chat Simples

### Tecnologias
- **Angular 18+** com standalone components
- **TypeScript** para tipagem forte
- **SCSS** para estilização moderna
- **RxJS** para programação reativa
- **WebSocket** para comunicação em tempo real

### Estrutura de Arquivos
```
clientWeb/
├── src/app/
│   ├── components/
│   │   ├── login/              # Tela de login
│   │   │   ├── login.component.ts
│   │   │   ├── login.component.html
│   │   │   └── login.component.scss
│   │   └── chat/               # Tela principal de chat
│   │       ├── chat.component.ts
│   │       ├── chat.component.html
│   │       └── chat.component.scss
│   ├── services/
│   │   ├── auth.service.ts      # Serviço de autenticação
│   │   └── chat.service.ts     # Serviço de chat/WebSocket
│   ├── guards/
│   │   └── auth.guard.ts      # Guard de rota
│   ├── app.routes.ts            # Rotas da aplicação
│   └── app.ts                 # Componente raiz
├── package.json
└── angular.json
```

### Funcionalidades
- ✅ Login com email e senha
- ✅ Login com Google OAuth
- ✅ Lista de grupos de chat
- ✅ Mensagens em tempo real
- ✅ Criação de novos grupos
- ✅ Interface responsiva (WhatsApp-like)
- ✅ Indicadores de status online
- 🚧 Videochamada (em desenvolvimento)
- 🚧 Compartilhamento de arquivos
- 🚧 Notificações push

## 🛠️ adminWeb - Administração ABP

### Tecnologias
- **ABP Framework 10.2.0** com LeptonX Theme
- **Angular 18+** com componentes ABP
- **Entity Framework Core** para dados
- **PostgreSQL** como banco principal
- **SignalR** para tempo real
- **Swagger/OpenAPI** para documentação

### Estrutura de Arquivos
```
adminWeb/
├── angular/                     # Frontend Angular
│   ├── src/
│   │   ├── app/
│   │   │   ├── modules/
│   │   │   │   ├── identity/        # Gestão de usuários
│   │   │   │   ├── tenant-management/ # Multi-tenant
│   │   │   │   ├── setting-management/ # Configurações
│   │   │   │   └── account/          # Conta do usuário
│   │   │   └── shared/          # Componentes compartilhados
│   │   └── assets/
│   └── package.json
└── aspnet-core/               # Backend API
    ├── src/
    │   ├── FamilyMeet.Admin.HttpApi/
    │   ├── FamilyMeet.Admin.Application/
    │   ├── FamilyMeet.Admin.Domain/
    │   └── FamilyMeet.Admin.EntityFrameworkCore/
    └── FamilyMeet.Admin.sln
```

### Funcionalidades ABP
- ✅ **Identity Management**: Usuários, papéis, permissões
- ✅ **Tenant Management**: Multi-tenancy
- ✅ **Setting Management**: Configurações do sistema
- ✅ **Account Management**: Perfil, senha, 2FA
- ✅ **Audit Logging**: Logs de auditoria
- ✅ **Permission System**: RBAC granular
- ✅ **Feature Management**: Features flags
- ✅ **Localization**: Multi-idiomas
- ✅ **Background Jobs**: Tarefas agendadas
- ✅ **Swagger UI**: Documentação interativa

## 🚀 API Backend Compartilhada

### Camadas DDD
```
api/
├── FamilyMeet.HttpApi/          # Controllers e endpoints
├── FamilyMeet.Application/        # Services e lógica de negócio
├── FamilyMeet.Domain/            # Entidades e regras de domínio
├── FamilyMeet.Domain.Shared/     # Enums, constantes, Value Objects
└── FamilyMeet.EntityFrameworkCore/ # Mapeamento ORM e migrations
```

### Funcionalidades da API
- ✅ **Autenticação JWT**: Login, refresh, Google OAuth
- ✅ **Videochamada WebRTC**: Sinais, salas, gravação
- ✅ **SignalR Hub**: Comunicação em tempo real
- ✅ **Gestão de Grupos**: CRUD de grupos de chat
- ✅ **Sistema de Mensagens**: Envio, edição, exclusão
- ✅ **Cache Redis**: Performance e escalabilidade
- ✅ **Logging Estruturado**: Diferentes níveis de log
- ✅ **Validação de Dados**: Input validation e sanitização
- ✅ **Tratamento de Erros**: Global exception handling
- ✅ **Documentação Swagger**: Auto-documentação da API

## 🔧 GitHub Workflows com Gemini AI

### Workflows Disponíveis
1. **pr-review-by-openhands.yml**: Review automático de PRs
2. **pr-review-evaluation.yml**: Avaliação da qualidade do review
3. **vulnerability-remediation-gemini.yml**: Scan e correção de vulnerabilidades
4. **assign-reviews-gemini.yml**: Atribuição inteligente de revisores
5. **ci-build-test.yml**: Build e testes automatizados
6. **auto-pr-from-main.yml**: Criação automática de PRs

### Configuração dos Workflows
- **Modelo AI**: Gemini 2.0 Flash (rápido e eficiente)
- **Secrets Necessários**: 
  - `GEMINI_API_KEY`: Chave da API Google Gemini
  - `ALLHANDS_BOT_GITHUB_PAT`: Token do bot GitHub
- **Recursos**: Análise de código, segurança, performance
- **Automação**: Reviews, atribuições, correções

## 🗄️ Banco de Dados

### PostgreSQL (Principal)
- **Container**: PostgreSQL 15
- **Connection String**: Configurada em docker-compose
- **Migrations**: Entity Framework Core automáticas
- **Seed Data**: Dados iniciais para desenvolvimento

### Redis (Cache)
- **Propósito**: Cache de sessões, mensagens, dados frequentes
- **TTL**: Configurações diferentes por tipo de dado
- **Cluster**: Suporte para Redis Cluster em produção

## 🐳 Docker e Deploy

### Docker Compose
```yaml
services:
  api:
    build: ./src/api
    ports: ["5000:5000"]
    environment:
      - ConnectionStrings__DefaultConnection=...
      - Redis__Configuration=...
  
  client-web:
    build: ./src/clientWeb
    ports: ["4200:4200"]
    depends_on: [api]
  
  admin-web:
    build: ./src/adminWeb/angular
    ports: ["4201:4200"]
    depends_on: [api]
  
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: FamilyChat_db
  
  redis:
    image: redis:7-alpine
```

### Estratégias de Deploy
- **Desenvolvimento**: Docker Compose local
- **Homologação**: Docker em staging environment
- **Produção**: 
  - Frontend: Vercel/Netlify (static)
  - API: Azure App Service/Heroku
  - Banco: Azure PostgreSQL/Heroku Postgres
  - Cache: Redis Cloud

## 📱 Versão Mobile (Planejada)

### Tecnologias Planejadas
- **Ionic Framework** ou **React Native**
- **Mesmas APIs** do backend existente
- **Autenticação Compartilhada**: JWT tokens
- **Sincronização**: Tempo real com SignalR
- **Notificações**: Firebase Cloud Messaging

### Funcionalidades Mobile
- 🚧 Chat em tempo real
- 🚧 Videochamada
- 🚧 Notificações push
- 🚧 Modo offline
- 🚧 Sincronização de contatos
- 🚧 Compartilhamento de mídia

## 🔐 Segurança

### Autenticação
- **JWT Tokens**: Com refresh automático
- **Google OAuth 2.0**: Integração completa
- **Password Hashing**: BCrypt com salt
- **2FA Support**: TOTP e SMS (planejado)
- **Session Management**: Redis store com TTL

### Autorização
- **RBAC**: Role-Based Access Control
- **Resource-based**: Permissões granulares
- **API Rate Limiting**: Prevenção de abuso
- **CORS Configurado**: Domínios específicos
- **Input Validation**: Sanitização e validação

### Segurança de Dados
- **PII Protection**: Dados pessoais criptografados
- **Audit Trail**: Logs completos de ações
- **Data Encryption**: TLS 1.3 em transmissão
- **Backup Strategy**: Backups automáticos diários

## 📊 Monitoramento e Logging

### Estrutura de Logs
```
Logs/
├── Application Logs      # Lógica de negócio
├── Security Logs        # Autenticação, autorização
├── Performance Logs     # Tempo de resposta, erros
├── Audit Logs          # Ações dos usuários
└── System Logs         # Infraestrutura
```

### Métricas
- **Application Performance**: Tempo de resposta, throughput
- **User Analytics**: Usuários ativos, mensagens enviadas
- **System Health**: CPU, memória, disco
- **Business Metrics**: Grupos criados, chamadas realizadas
- **Error Rates**: Taxa de erros por endpoint

## 🔄 CI/CD Pipeline

### Build Pipeline
1. **Code Quality**: ESLint, SonarQube
2. **Unit Tests**: Jest/xUnit com cobertura >80%
3. **Integration Tests**: Testes de API endpoints
4. **E2E Tests**: Playwright/Cypress
5. **Security Scan**: Verificação de vulnerabilidades
6. **Performance Test**: Load testing com k6

### Deploy Pipeline
1. **Staging**: Deploy automático para cada PR
2. **Production**: Deploy manual/aprovado
3. **Rollback**: Automático se falhar testes
4. **Health Check**: Verificação pós-deploy

## 📚 Documentação

### Documentação Técnica
- **API Docs**: Swagger/OpenAPI interativo
- **Component Docs**: Storybook para Angular
- **Database Schema**: Diagramas e migrations
- **Architecture Docs**: Decisões e padrões

### Documentação de Usuário
- **Getting Started**: Guia de instalação
- **User Manual**: Como usar cada funcionalidade
- **Admin Guide**: Como gerenciar o sistema
- **API Reference**: Exemplos de uso da API
- **Troubleshooting**: Problemas comuns e soluções

## 🚀 Roadmap Futuro

### Short Term (1-3 meses)
- [ ] Finalizar implementação do clientWeb
- [ ] Corrigir erros TypeScript
- [ ] Implementar videochamada no client
- [ ] Configurar módulos ABP no admin
- [ ] Implementar testes E2E
- [ ] Otimizar performance da API

### Medium Term (3-6 meses)
- [ ] Desenvolver versão mobile
- [ ] Implementar notificações push
- [ ] Sistema de arquivos e mídia
- [ ] Advanced moderation tools
- [ ] Analytics e reporting avançado
- [ ] Multi-language support

### Long Term (6-12 meses)
- [ ] AI-powered features
- [ ] Advanced video call features
- [ ] Integration com plataformas externas
- [ ] Enterprise features (SSO, LDAP)
- [ ] Global deployment strategy
- [ ] Advanced security features

## 📋 Próximos Passos Imediatos

1. **Finalizar clientWeb**:
   - Corrigir erros de injeção de dependência
   - Implementar videochamada WebRTC
   - Adicionar testes unitários
   - Melhorar responsividade

2. **Configurar adminWeb**:
   - Customizar módulos ABP para negócio
   - Implementar dashboards específicos
   - Configurar permissões customizadas
   - Integrar com clientWeb

3. **Testes e QA**:
   - Implementar suíte de testes completa
   - Configurar CI/CD pipeline
   - Testes de carga e performance
   - Testes de segurança automatizados

4. **Documentação**:
   - Completar documentação da API
   - Criar guias de usuário
   - Documentar arquitetura
   - Gravar vídeos tutoriais

---

## 📞 Contato e Suporte

Para dúvidas, sugestões ou problemas:
- **Issues**: GitHub repository issues
- **Discussions**: GitHub discussions para dúvidas
- **Email**: support@familymeet.dev
- **Documentation**: `/docs` no repositório

---

*Última atualização: 31/03/2026*
