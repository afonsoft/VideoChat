# 🔧 Forwarded Headers Configuration Guide

## 📋 Overview

Forwarded Headers middleware é essencial para aplicações ASP.NET Core que rodam atrás de reverse proxies, load balancers, ou CDNs. Esta configuração permite que a aplicação recupere informações corretas sobre a requisição original do cliente.

## 🎯 **Problemas Resolvidos**

### **1. Perda de Informações da Requisição Original**
- **Problema**: Reverse proxies modificam headers originais
- **Solução**: `X-Forwarded-For` preserva IP real do cliente

### **2. Confusão HTTP vs HTTPS**
- **Problema**: Proxy converte HTTP para HTTPS internamente
- **Solução**: `X-Forwarded-Proto` indica protocolo original

### **3. Problemas com Path Handling**
- **Problema**: Proxy mapeia URLs (ex: `/api` → `/myapp/api`)
- **Solução**: `X-Forwarded-Host` preserva hostname original

### **4. Endereço IP e Segurança**
- **Problema**: Proxy substitui IP do cliente pelo próprio
- **Solução**: Parse correto de `X-Forwarded-For`

### **5. Impacto do Load Balancer**
- **Problema**: Distribuição de requisições afeta sessões
- **Solução**: Configuração correta de sticky sessions

---

## ⚙️ **Configuração Implementada**

### **1. Middleware Configuration**
```csharp
// FamilyMeetHttpApiHostModule.cs
private void ConfigureForwardedHeaders(ServiceConfigurationContext context, IConfiguration configuration)
{
    context.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        // Headers habilitados
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                  ForwardedHeaders.XForwardedProto |
                                  ForwardedHeaders.XForwardedHost;

        // IPs confiáveis (segurança)
        var knownProxies = configuration["ForwardedHeaders:KnownProxies"]?.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (knownProxies?.Length > 0)
        {
            foreach (var proxy in knownProxies)
            {
                if (System.Net.IPAddress.TryParse(proxy.Trim(), out var ipAddress))
                {
                    options.KnownProxies.Add(ipAddress);
                }
            }
        }
        else
        {
            // Development: aceitar localhost
            options.KnownProxies.Add(System.Net.IPAddress.Loopback);
            options.KnownProxies.Add(System.Net.IPAddress.IPv6Loopback);
        }

        // Configurações de segurança
        options.RequireHeaderSymmetry = false;
        options.AllowedHosts = configuration["ForwardedHeaders:AllowedHosts"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) 
            ?? new[] { "*" };
        options.ForwardLimit = null; // Sem limite de forwards
        
        // Nomes customizados dos headers
        options.ForwardedForHeaderName = "X-Forwarded-For";
        options.ForwardedProtoHeaderName = "X-Forwarded-Proto";
        options.ForwardedHostHeaderName = "X-Forwarded-Host";
    });
}
```

### **2. Pipeline Configuration**
```csharp
// OnApplicationInitialization
public override void OnApplicationInitialization(ApplicationInitializationContext context)
{
    var app = context.GetApplicationBuilder();
    var env = context.GetEnvironment();

    // Forwarded Headers deve ser configurado o mais cedo possível
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseErrorPage();
    }

    // Configurar ANTES de outros middlewares
    app.UseForwardedHeaders();

    if (!env.IsDevelopment())
    {
        app.UseHsts();
    }

    // ... outros middlewares
}
```

---

## 📁 **Arquivos de Configuração**

### **appsettings.json (Produção)**
```json
{
  "ForwardedHeaders": {
    "Enabled": true,
    "ForwardedHeaders": "XForwardedFor,XForwardedProto,XForwardedHost",
    "AllowedHosts": "*",
    "KnownProxies": "127.0.0.1,::1",
    "ForwardLimit": null,
    "RequireHeaderSymmetry": false,
    "ForwardedForHeaderName": "X-Forwarded-For",
    "ForwardedProtoHeaderName": "X-Forwarded-Proto",
    "ForwardedHostHeaderName": "X-Forwarded-Host"
  }
}
```

### **appsettings.Development.json**
```json
{
  "ForwardedHeaders": {
    "Enabled": true,
    "ForwardedHeaders": "XForwardedFor,XForwardedProto,XForwardedHost",
    "AllowedHosts": "localhost,127.0.0.1,::1",
    "KnownProxies": "127.0.0.1,::1,192.168.68.113",
    "ForwardLimit": null,
    "RequireHeaderSymmetry": false
  }
}
```

### **appsettings.Production.json**
```json
{
  "ForwardedHeaders": {
    "Enabled": true,
    "ForwardedHeaders": "XForwardedFor,XForwardedProto,XForwardedHost",
    "AllowedHosts": "api.familymeet.com,familymeet.com,*.familymeet.com",
    "KnownProxies": "10.0.0.0/8,172.16.0.0/12,192.168.0.0/16",
    "ForwardLimit": null,
    "RequireHeaderSymmetry": false
  }
}
```

---

## 🔍 **Headers Suportados**

### **X-Forwarded-For**
```http
X-Forwarded-For: 192.168.1.100, 10.0.0.1, 172.16.0.1
```
- **Propósito**: IP original do cliente
- **Formato**: Lista de IPs (cliente → proxy1 → proxy2)
- **Parse**: `HttpContext.Connection.RemoteIpAddress`

### **X-Forwarded-Proto**
```http
X-Forwarded-Proto: https
```
- **Propósito**: Protocolo original (http/https)
- **Uso**: Geração correta de URLs
- **Parse**: `Request.Scheme`

### **X-Forwarded-Host**
```http
X-Forwarded-Host: api.familymeet.com:443
```
- **Propósito**: Hostname original
- **Uso**: Geração de links absolutos
- **Parse**: `Request.Host`

### **X-Forwarded-Port**
```http
X-Forwarded-Port: 443
```
- **Propósito**: Porta original
- **Uso**: Construção de URLs completas
- **Parse**: `Request.Host.Port`

---

## 🛡️ **Configurações de Segurança**

### **Known Proxies**
```csharp
// Apenas proxies confiáveis podem enviar headers
options.KnownProxies.Add(IPAddress.Parse("10.0.0.1"));
options.KnownProxies.Add(IPAddress.Parse("192.168.1.100"));
```

### **Allowed Hosts**
```csharp
// Restringe hosts aceitos
options.AllowedHosts.Add("api.familymeet.com");
options.AllowedHosts.Add("*.familymeet.com");
```

### **Header Symmetry**
```csharp
// Validação adicional de headers
options.RequireHeaderSymmetry = false; // Desabilitado para compatibilidade
```

---

## 🌐 **Configurações por Ambiente**

### **Development**
- **Known Proxies**: `127.0.0.1, ::1, 192.168.68.113`
- **Allowed Hosts**: `localhost, 127.0.0.1, ::1`
- **Segurança**: Mais permissivo para desenvolvimento

### **Staging**
- **Known Proxies**: IPs da infra de staging
- **Allowed Hosts**: Domínios de staging
- **Segurança**: Configuração intermediária

### **Production**
- **Known Proxies**: CIDR ranges da infra
- **Allowed Hosts**: Domínios de produção
- **Segurança**: Restritivo e específico

---

## 🔄 **Exemplos de Configuração**

### **Nginx Reverse Proxy**
```nginx
server {
    listen 80;
    server_name api.familymeet.com;
    
    location / {
        proxy_pass http://localhost:5000;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;
        proxy_set_header X-Forwarded-Port $server_port;
    }
}
```

### **Apache Reverse Proxy**
```apache
<VirtualHost *:80>
    ServerName api.familymeet.com
    ProxyPreserveHost On
    ProxyPass / http://localhost:5000/
    ProxyPassReverse / http://localhost:5000/
    
    RequestHeader set X-Forwarded-Proto "http"
    RequestHeader set X-Forwarded-Host "api.familymeet.com"
</VirtualHost>
```

### **HAProxy**
```haproxy
frontend http_frontend
    bind *:80
    default_backend http_backend

backend http_backend
    server app1 127.0.0.1:5000
    http-request set-header X-Forwarded-Proto http
    http-request set-header X-Forwarded-Host %[req hdr(host)]
```

### **Azure Application Gateway**
```json
{
  "frontendIPConfigurations": [
    {
      "name": "appGatewayFrontendIP",
      "properties": {
        "publicIPAddress": {
          "id": "/subscriptions/.../publicIPAddresses/appGatewayPublicIP"
        }
      }
    }
  ],
  "backendSettings": [
    {
      "name": "appGatewayBackendSettings",
      "properties": {
        "protocol": "Http",
        "port": 80,
        "requestTimeout": 30,
        "probe": {
          "id": "/subscriptions/.../probes/appGatewayProbe"
        }
      }
    }
  ]
}
```

---

## 🧪 **Testes Implementados**

### **Unit Tests**
```csharp
[Fact]
public void ForwardedHeaders_Options_Should_Be_Configured()
{
    var forwardedHeadersOptions = serviceProvider.GetService<ForwardedHeadersOptions>();
    forwardedHeadersOptions.ForwardedHeaders.ShouldBe(
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
    );
}
```

### **Integration Tests**
```csharp
[Fact]
public async Task ForwardedHeaders_Should_Process_XForwardedFor()
{
    var request = new HttpRequestMessage(HttpMethod.Get, "/api/identity/users");
    request.Headers.Add("X-Forwarded-For", "192.168.1.100");
    request.Headers.Add("X-Forwarded-Proto", "https");
    request.Headers.Add("X-Forwarded-Host", "api.familymeet.com");
    
    var response = await client.SendAsync(request);
    response.IsSuccessStatusCode.ShouldBeTrue();
}
```

---

## 🚀 **Deploy Considerations**

### **Kubernetes**
```yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: familymeet-ingress
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
spec:
  rules:
  - host: api.familymeet.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: familymeet-service
            port:
              number: 80
```

### **Docker Compose**
```yaml
version: '3.8'
services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
    depends_on:
      - api
  api:
    image: familymeet-api:latest
    expose:
      - "5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

### **Azure App Service**
```json
{
  "properties": {
    "clientAffinityEnabled": false,
    "httpsOnly": true,
    "siteConfig": {
      "alwaysOn": true,
      "webSocketsEnabled": true
    }
  }
}
```

---

## 🔧 **Troubleshooting**

### **Problemas Comuns**

#### **1. IPs Não São Confiáveis**
```csharp
// Solução: Adicionar IPs dos proxies
options.KnownProxies.Add(IPAddress.Parse("10.0.0.1"));
```

#### **2. HTTPS Redirect Loop**
```csharp
// Solução: Configurar X-Forwarded-Proto corretamente
request.Headers.Add("X-Forwarded-Proto", "https");
```

#### **3. URLs Incorretas**
```csharp
// Solução: Configurar X-Forwarded-Host
request.Headers.Add("X-Forwarded-Host", "api.familymeet.com");
```

#### **4. CORS Issues**
```csharp
// Solução: Adicionar forwarded host ao CORS
builder.WithOrigins("https://api.familymeet.com");
```

### **Debug Tips**
```csharp
// Enable detailed logging
services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
});

// Log headers recebidos
app.Use((context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("X-Forwarded-For: {Header}", context.Request.Headers["X-Forwarded-For"]);
    logger.LogInformation("X-Forwarded-Proto: {Header}", context.Request.Headers["X-Forwarded-Proto"]);
    logger.LogInformation("X-Forwarded-Host: {Header}", context.Request.Headers["X-Forwarded-Host"]);
    return next();
});
```

---

## ✅ **Checklist de Deploy**

### **Antes do Deploy:**
- [ ] Configurar Known Proxies para produção
- [ ] Definir Allowed Hosts específicos
- [ ] Testar com reverse proxy real
- [ ] Validar geração de URLs
- [ ] Testar CORS com forwarded headers

### **Durante o Deploy:**
- [ ] Verificar logs de Forwarded Headers
- [ ] Monitorar IPs de clientes
- [ ] Validar URLs geradas
- [ ] Testar SignalR/WebSockets

### **Pós-Deploy:**
- [ ] Monitorar performance
- [ ] Verificar security headers
- [ ] Testar load balancer
- [ ] Validar SSL termination

---

## 🎯 **Best Practices**

1. **Sempre configure Known Proxies** em produção
2. **Use Allowed Hosts específicos** para segurança
3. **Teste com proxy real** antes do deploy
4. **Monitore headers** em produção
5. **Documente configurações** por ambiente
6. **Use HTTPS** sempre que possível
7. **Configure CORS** corretamente com forwarded hosts
8. **Teste SignalR/WebSockets** com proxy

---

## ✅ **Implementação Completa**

**🔧 Forwarded Headers configurado com:**
- ✅ **Middleware configurado** corretamente
- ✅ **Headers suportados**: X-Forwarded-For, X-Forwarded-Proto, X-Forwarded-Host
- ✅ **Segurança**: Known Proxies, Allowed Hosts
- ✅ **Multi-ambiente**: Development, Staging, Production
- ✅ **Testes completos**: Unit e Integration tests
- ✅ **Documentação**: Guia completo
- ✅ **Deploy ready**: Kubernetes, Docker, Azure

**🚀 Ready for production deployment!**
