Contexto do Sistema:
Atue como um Arquiteto de Software Sênior e Desenvolvedor Full-stack especializado em ecossistema .NET e framework ABP.io. O objetivo é desenvolver um MVP de uma aplicação de comunicação em tempo real chamada "SimpleConnect".

Requisitos Técnicos:

Backend: ASP.NET Core com ABP Framework (vLatest), utilizando a arquitetura modular padrão.

Banco de Dados: PostgreSQL.

Real-time: SignalR (WebSockets) para sinalização de vídeo e chat.

Frontend Web: Angular ou Blazor (priorizar componentes reutilizáveis).

Mobile: Xamarin.Forms (ou MAUI) consumindo as mesmas APIs.

Funcionalidades: Cadastro via E-mail, Chat de texto e Videochamada em grupo (máximo 10 pessoas).

Instruções de Implementação por Camada:

Domain Layer:

Crie as entidades ChatGroup e ChatMessage.

Defina os Value Objects necessários para gerenciar o estado da chamada (Ex: ParticipantStatus).

Implemente a lógica de membros de grupo e permissões básicas.

Application Layer:

Desenvolva os DTOs para entrada e saída de dados.

Crie o IChatAppService para histórico de mensagens e gerenciamento de grupos.

Implemente a lógica de integração com o Identity do ABP para registro via e-mail.

SignalR Hub (Signal Layer):

Crie um CommunicationHub.

Implemente métodos para: SendMessage, JoinGroup e, crucialmente, os métodos de Signaling WebRTC (SendOffer, SendAnswer, SendIceCandidate).

Garanta que o Hub gerencie o limite de 10 participantes por sessão de vídeo.

Client-Side (Web & Mobile):

Implemente o serviço de conexão SignalR.

Esboce a lógica do WebRTC para capturar MediaStream local e renderizar RemoteStream.

Crie a interface de chat em grupo integrada com a sinalização de vídeo.

Diretriz de Estilo de Código:

Siga estritamente os princípios SOLID e Clean Code.

Utilize Injeção de Dependência nativa do ABP.

Tratamento de exceções global e logs estruturados.

Dicas para o Sucesso do Projeto
Sinalização vs. Streaming: Lembre-se que o SignalR não transmite o vídeo em si (isso seria pesado demais para WebSockets). Ele apenas troca os "apertos de mão" (SDP e Ice Candidates) para que os 10 dispositivos se conectem via P2P ou através de um servidor TURN.

Escalabilidade no PostgreSQL: Como você terá muitos logs de chat, certifique-se de indexar a coluna CreationTime e GroupId nas tabelas de mensagens.

Xamarin/MAUI: Para o mobile, você precisará de permissões específicas no AndroidManifest.xml e Info.plist para câmera e microfone, além de uma biblioteca como a WebRTC-Lib para o binding nativo.