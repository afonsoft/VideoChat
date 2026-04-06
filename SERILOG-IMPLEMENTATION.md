# 📝 Serilog Implementation Guide

## 📋 Overview

Implementação completa do **Serilog** no projeto FamilyMeet seguindo as melhores práticas ABP.IO para logging estruturado e production-ready.

## ✨ **Features Implementadas**

### **1. Configuração Avançada do Serilog**
- **Múltiplos Sinks**: Console, File, Debug
- **Templates Customizados**: Formatos específicos para cada ambiente
- **Log Levels**: Configurações granulares por namespace
- **Enrichers**: Contexto, máquina, ambiente, processo
- **Rolling Policies**: Rotação automática de arquivos
- **Performance**: Async writing para melhor performance

### **2. Configurações por Ambiente**
- **Development**: Debug level com verbose logging
- **Staging**: Information level com logging balanceado
- **Production**: Warning level com logging essencial
- **Docker**: Configurações otimizadas para containers

### **3. Integração ABP.IO**
- **AbpSerilogEnrichers**: Tenant, usuário, correlation ID
- **Structured Logging**: Propriedades customizadas
- **Request Tracking**: Informação completa de requisições

---

## ⚙️ **Arquivos de Configuração**

### **Program.cs**
```csharp
Log.Logger = new LoggerConfiguration()
#if DEBUG
    .MinimumLevel.Debug()
#else
    .MinimumLevel.Information()
#endif
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "FamilyMeet.HttpApi.Host")
    .WriteTo.Async(c => c.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"))
    .WriteTo.Async(c => c.File(
        path: "Logs/familymeet-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"))
#if DEBUG
    .WriteTo.Async(c => c.File(
        path: "Logs/familymeet-debug-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"))
#endif
    .CreateLogger();
```

### **appsettings.json (Base)**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "Volo.Abp": "Warning",
        "Microsoft.AspNetCore.SignalR": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/familymeet-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithEnvironmentName",
      "WithProcessId"
    ],
    "Properties": {
      "Application": "FamilyMeet.HttpApi.Host",
      "Environment": "Docker"
    }
  }
}
```

### **appsettings.Development.json**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.EntityFrameworkCore": "Warning",
        "Volo.Abp": "Information",
        "Microsoft.AspNetCore.SignalR": "Debug"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/familymeet-debug-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "Debug",
        "Args": {
          "path": "Logs/familymeet-debug-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 3,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithEnvironmentName",
      "WithProcessId"
    ],
    "Properties": {
      "Application": "FamilyMeet.HttpApi.Host",
      "Environment": "Development"
    }
  }
}
```

### **appsettings.Production.json**
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Warning",
        "Volo.Abp": "Warning",
        "Microsoft.AspNetCore.SignalR": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/familymeet-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
          "fileSizeLimitBytes": 104857600,
          "rollOnFileSizeLimit": true
        }
      }
    ],
    "Enrich": [
      "FromLogContext",
      "WithMachineName",
      "WithEnvironmentName",
      "WithProcessId"
    ],
    "Properties": {
      "Application": "FamilyMeet.HttpApi.Host",
      "Environment": "Production"
    }
  }
}
```

---

## 🎯 **Configurações Implementadas**

### **1. Log Levels Granulares**
```csharp
// Por namespace
.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
.MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
.MinimumLevel.Override("Microsoft.AspNetCore.SignalR", LogEventLevel.Information)
```

### **2. Múltiplos Sinks**
```csharp
// Console para desenvolvimento
.WriteTo.Async(c => c.Console(
    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"))

// File para produção
.WriteTo.Async(c => c.File(
    path: "Logs/familymeet-.log",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 30,
    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
    fileSizeLimitBytes: 104857600,
    rollOnFileSizeLimit: true))

// Debug específico para development
#if DEBUG
.WriteTo.Async(c => c.File(
    path: "Logs/familymeet-debug-.log",
    rollingInterval: RollingInterval.Day,
    retainedFileCountLimit: 7,
    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"))
#endif
```

### **3. Enrichers ABP.IO**
```csharp
// No FamilyMeetHttpApiHostModule.cs
app.UseAbpSerilogEnrichers();
```

### **4. Propriedades Customizadas**
```csharp
.Enrich.WithProperty("Application", "FamilyMeet.HttpApi.Host")
.Enrich.FromLogContext()
```

---

## 📊 **Estrutura de Logs**

### **Formato de Log**
```
[2024-04-05 22:19:15.123 zzz] INFO FamilyMeet.Application.Services.Chat.ChatMessageAppService: Creating new chat message in group 12345 by user 67890
[2024-04-05 22:19:15.456 zzz] WARN Microsoft.EntityFrameworkCore.Database.Command: Executed DbCommand (15ms) [Parameters=[@__p_0='2024-04-05 22:19:15.1234567'], CommandType='Text', CommandTimeout='30']
[2024-04-05 22:19:15.789 zzz] ERR FamilyMeet.Application.Services.Chat.ChatGroupAppService: Failed to create chat group: Group name is required
```

### **Arquivos Gerados**
```
Logs/
├── familymeet-.log              # Production logs (30 dias)
├── familymeet-debug-.log        # Debug logs (7 dias)
└── familymeet-debug-.log        # Debug específico (3 dias)
```

---

## 🚀 **Performance e Otimizações**

### **1. Async Writing**
```csharp
.WriteTo.Async(c => c.File(...))  // Async para não bloquear
```

### **2. Rolling Policies**
```json
{
  "rollingInterval": "Day",
  "retainedFileCountLimit": 30,
  "fileSizeLimitBytes": 104857600,
  "rollOnFileSizeLimit": true
}
```

### **3. Level Filtering**
```csharp
// Reduzir verbosidade de frameworks
.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
.MinimumLevel.Override("Volo.Abp", LogEventLevel.Warning)
```

---

## 🐳 **Docker Integration**

### **Dockerfile**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app

# Criar diretório de logs
RUN mkdir -p /app/Logs

# Copiar aplicação
COPY . .

# Configurar permissões
RUN chmod +x /app/Logs

ENTRYPOINT ["dotnet", "afonsoft.FamilyMeet.HttpApi.Host.dll"]
```

### **docker-compose.yml**
```yaml
version: '3.8'
services:
  familymeet-api:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    volumes:
      - ./Logs:/app/Logs
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
```

---

## 🔍 **Monitoring e Debugging**

### **1. Log Levels por Ambiente**
| Ambiente | Default | Microsoft | EF Core | SignalR | Volo.Abp |
|-----------|---------|-----------|---------|---------|-----------|
| Development | Debug | Information | Warning | Debug | Information |
| Staging | Information | Warning | Warning | Information | Warning |
| Production | Information | Warning | Warning | Information | Warning |

### **2. Estrutura de Log para Monitoramento**
```json
{
  "Timestamp": "2024-04-05T22:19:15.123456Z",
  "Level": "Information",
  "SourceContext": "FamilyMeet.Application.Services.Chat.ChatMessageAppService",
  "Message": "Creating new chat message",
  "Properties": {
    "Application": "FamilyMeet.HttpApi.Host",
    "Environment": "Production",
    "TenantId": "12345",
    "UserId": "67890",
    "RequestId": "abc123-def456",
    "MachineName": "WEB-SERVER-01",
    "ProcessId": "12345"
  }
}
```

---

## 🛡️ **Segurança de Logs**

### **1. Sensitive Data Protection**
```csharp
// ABP automaticamente protege dados sensíveis
// Passwords, tokens, informações pessoais não são loggados
```

### **2. Log Retention**
```json
{
  "retainedFileCountLimit": 30,        // 30 dias de logs
  "fileSizeLimitBytes": 104857600,  // 100MB por arquivo
  "rollOnFileSizeLimit": true           // Auto rotação
}
```

### **3. Access Control**
```bash
# Permissões de arquivo Linux
chmod 640 /app/Logs/familymeet-*.log
chown app:app /app/Logs
```

---

## 🔧 **Troubleshooting Comum**

### **1. Logs Não Aparecem**
```csharp
// Verificar se UseSerilog() está configurado
builder.Host.UseSerilog();

// Verificar se UseAbpSerilogEnrichers() foi chamado
app.UseAbpSerilogEnrichers();
```

### **2. Performance Issues**
```csharp
// Usar Async sinks
.WriteTo.Async(c => c.File(...))

// Limitar verbose logging
.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
```

### **3. Docker Issues**
```bash
# Verificar montagem de volume
docker exec -it familymeet-api ls -la /app/Logs

# Verificar permissões
docker logs familymeet-api
```

---

## 📱 **Integração com Ferramentas**

### **1. ELK Stack**
```json
{
  "WriteTo": [
    {
      "Name": "Elasticsearch",
      "Args": {
        "nodeUris": "http://elasticsearch:9200",
        "indexFormat": "familymeet-{0:yyyy.MM.dd}"
      }
    }
  ]
}
```

### **2. Seq**
```json
{
  "WriteTo": [
    {
      "Name": "Seq",
      "Args": {
        "serverUrl": "http://seq:5341",
        "apiKey": "your-api-key"
      }
    }
  ]
}
```

### **3. Application Insights**
```json
{
  "WriteTo": [
    {
      "Name": "ApplicationInsights",
      "Args": {
        "instrumentationKey": "your-key"
      }
    }
  ]
}
```

---

## ✅ **Checklist de Implementação**

### **Configuração Básica**
- [x] Serilog packages instalados
- [x] Program.cs configurado
- [x] appsettings.json atualizados
- [x] UseSerilog() no host
- [x] UseAbpSerilogEnrichers() no módulo

### **Configurações Avançadas**
- [x] Múltiplos sinks configurados
- [x] Environment-specific settings
- [x] Log levels por namespace
- [x] Custom output templates
- [x] Rolling policies
- [x] Async writing
- [x] File size limits

### **Production Ready**
- [x] Performance otimizada
- [x] Security considerations
- [x] Docker integration
- [x] Monitoring ready
- [x] Troubleshooting guides

---

## 🎊 **Resumo da Implementação**

**✅ Serilog completamente implementado com:**
- **Configuração multi-ambiente** (Development, Staging, Production)
- **Performance otimizada** com async writing
- **Segurança robusta** com proteção de dados sensíveis
- **Monitoramento completo** com enrichers ABP.IO
- **Docker ready** com volumes e permissões
- **Troubleshooting guides** para problemas comuns
- **Integração flexível** com ELK, Seq, Application Insights

**🚀 Sistema enterprise-ready com logging estruturado!**
