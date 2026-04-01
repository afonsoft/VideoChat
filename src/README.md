# FamilyMeet - Plataforma de Comunicação Familiar

## Estrutura do Projeto

Este projeto foi dividido em duas aplicações frontend distintas:

### 📱 clientWeb
Aplicação simples de chat estilo WhatsApp para comunicação entre usuários.

**Características:**
- Login com email e Google OAuth
- Lista de grupos de chat
- Mensagens em tempo real com WebSocket
- Interface responsiva e moderna
- Criação de novos grupos
- Indicador de usuários online

**Tecnologias:**
- Angular 18+ com standalone components
- TypeScript
- SCSS
- RxJS para gerenciamento de estado
- WebSocket para comunicação em tempo real

### 🛠️ adminWeb
Aplicação administrativa completa com todos os módulos ABP Framework.

**Características:**
- Dashboard administrativo
- Gestão de usuários e permissões
- Configuração de sistema
- Logs e monitoramento
- Gestão de grupos de chat
- Configuração de providers
- Interface baseada em ABP Framework

**Tecnologias:**
- ABP Framework 10.2.0
- Angular com LeptonX Theme
- Entity Framework Core
- Módulos de Identity, Tenant Management, Settings
- Swagger/OpenAPI

### 🚀 Como Executar

#### clientWeb (Chat)
```bash
cd src/clientWeb
npm install
npm start
```
Acessar: http://localhost:4200

#### adminWeb (Administração)
```bash
cd src/adminWeb/angular
yarn install
yarn start
```
Acessar: http://localhost:4200

#### API Backend
```bash
cd src/api
dotnet run --project FamilyMeet.HttpApi.Host
```
API: http://localhost:5000
Swagger: http://localhost:5000/swagger

### 🔧 Configuração

#### Variáveis de Ambiente
- **API_URL**: URL da API backend
- **WS_URL**: URL do WebSocket para comunicação em tempo real
- **GOOGLE_CLIENT_ID**: ID do cliente Google OAuth
- **JWT_SECRET**: Chave secreta para tokens JWT

#### Banco de Dados
- PostgreSQL configurado no docker-compose.yml
- Connection string em appsettings.json
- Migrations automáticas via Entity Framework

### 📋 Estrutura de Pastas

```
src/
├── clientWeb/          # Aplicação de chat simples
│   ├── src/app/
│   │   ├── components/
│   │   │   ├── login/
│   │   │   └── chat/
│   │   ├── services/
│   │   │   ├── auth.service.ts
│   │   │   └── chat.service.ts
│   │   └── guards/
│   │       └── auth.guard.ts
│   └── package.json
├── adminWeb/           # Frontend Angular Aplicação administrativa ABP
└── api/               # API principal compartilhada
    ├── HttpApi/
    ├── Application/
    ├── Domain/
    └── EntityFrameworkCore/
```

### 🎥 Funcionalidades de Videochamada

Implementadas na API backend:
- WebRTC para comunicação P2P
- SignalR Hub para gerenciamento de salas
- Controle de áudio/vídeo
- Compartilhamento de tela
- Gravação de chamadas
- Múltiplos participantes

### 🔐 Autenticação

- JWT tokens com refresh automático
- Google OAuth 2.0 integration
- Gerenciamento de sessão
- Guards de rota no Angular

### 🚀 Deploy

#### Docker
```bash
docker-compose up -d
```

#### Produção
- Frontend: Vercel/Netlify
- API: Azure App Service/Heroku
- Banco: Azure PostgreSQL/Heroku Postgres

### 📱 Versão Mobile

A versão mobile será desenvolvida com base no clientWeb, utilizando:
- Ionic Framework ou React Native
- Mesmas APIs do backend
- Interface adaptada para dispositivos móveis
- Notificações push

### 🔄 Próximos Passos

1. **Finalizar clientWeb**: Corrigir erros de TypeScript e testar
2. **Configurar adminWeb**: Ajustar módulos ABP para negócio
3. **Integração Mobile**: Iniciar desenvolvimento da versão mobile
4. **Testes E2E**: Implementar testes automatizados
5. **Deploy**: Configurar ambientes de staging/produção

### 📞 Suporte

Para dúvidas e suporte:
- Documentação da API: `/swagger`
- Logs da aplicação: Verificar console do navegador
- Issues: Criar ticket no repositório GitHub
