# 🌈 MoodTracker API - Seu Bem-Estar em Dados Seguros

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-24.0-2496ED?logo=docker)](https://www.docker.com/)
[![License: CC BY-NC-SA 4.0](https://img.shields.io/badge/License-CC%20BY--NC--SA%204.0-lightgrey.svg)](https://creativecommons.org/licenses/by-nc-sa/4.0/)
[![Swagger](https://img.shields.io/badge/Swagger-3.0-85EA2D?logo=swagger)](https://swagger.io/)
[![Redis](https://img.shields.io/badge/redis-%23DD0031.svg?style=for-the-badge&logo=redis&logoColor=white)](https://redis.io)

**API robusta e escalável para gerenciamento de bem-estar emocional e hábitos.**  
*Desenvolvida com arquitetura limpa e segurança de nível enterprise.*

<p align="center">
  <img src="https://raw.githubusercontent.com/JhonataAugust0/MoodTracker-api/refs/heads/master/public/image.png" width="800" alt="Dashboard Preview">
</p>

## 🌟 Objetivo do Projeto

O **MoodTracker** é uma aplicação inovadora projetada para ajudar os usuários a monitorar e melhorar seu bem-estar emocional e comportamental. Com uma interface intuitiva e visual, o MoodTracker permite o registro diário de humor, gerenciamento de hábitos, criação de notas rápidas e visualização de dados para análise.


## 🚀 Funcionalidades Principais

### 🔐 Autenticação e Segurança
- **JWT com Refresh Tokens**: Autenticação stateless e renovação de sessão segura.
- **Criptografia SHA-256**: Proteção de senhas com salt aleatório.

### 📊 Domínio de Bem-Estar
- **Registro de Humor**: Múltiplas entradas diárias com notas e tags.
- **Gestão de Hábitos**: CRUD completo com tracking de streaks e métricas.
- **Análise de Dados**: Agregação de dados para insights semanais/mensais.

### 🔔 Notificações Inteligentes com Redis

- **Lembretes de Atividade**: Receba e-mails amigáveis após 3 dias de inatividade, incentivando você a continuar sua jornada.
- **Notificações Offline**: Se você estiver desconectado, as notificações são armazenadas no Redis e entregues assim que retornar ao app.
- **Escalabilidade**: O Redis permite milhares de notificações simultâneas sem impactar a performance.

### ⚙️ Infraestrutura
- **Migrations Automatizadas**: EF Core com histórico de alterções versionado.
- **Health Checks**: Monitoramento de dependências (DB).

## 📦 Stack Tecnológica

| Camada              | Tecnologias                                           |
|---------------------|-------------------------------------------------------|
| **Core**            | .NET 8.0, C# 12, Entity Framework Core 8              |
| **Banco de Dados**  | PostgreSQL 15                                         |
| **Infra**           | Docker Compose, GitHub Actions CI/CD                  |
| **Segurança**       | JWT, ASP.NET Identity, Rate Limiting, CORS policies   |
| **Notificações**    | SignalR, Redis, System.Net.Mail                       |


## 📚 Documentação

- [Configuração do Projeto](./docs/config.md)
- [Especificação do Projeto](./docs/specification.md)
- [Testes](./docs/tests.md)


## 🛠️ Primeiros Passos

### Pré-requisitos
- .NET 8 SDK
- Docker Engine
- PostgreSQL 15 ou Docker

### Configuração Local
```bash
# Clone o repositório
git clone https://github.com/JhonataAugust0/moodtracker-backend.git

# Configure as variáveis de ambiente (renomeie .env.example para .env)
cp env.example .env

# Suba os containers (DB + API)
docker-compose -f docker-compose.dev.yml up --build

# Acesse a API em:
http://localhost:5000/swagger
```

# 🧪 Testes
Garanta a qualidade com nossa suite de testes:
```
bash
Copy
dotnet test
```

| Tipo de Teste|	Cobertura| 	Frameworks |
|-------------|--------------| ---------------|
| Unitários|	85%| 	xUnit, Moq, FluentAssertions |
| Integração|	70%	| Testcontainers para PostgreSQL |
| Stress Test	|- |	Locust (1000+ reqs/sec) |
Obs.: Testes em desenvolvimento. 

# 🔄 CI/CD Pipeline
- GitHub Actions: Build, test e deploy automático para o Docker Registry.
- SonarQube: Análise estática de código e cobertura (via docker).


# 🤝 Contribuição
Fork o projeto (git clone https://github.com/JhonataAugust0/moodtracker-backend.git)

Crie sua branch (git checkout -b feature/incrivel)

Commit suas mudanças (git commit -m 'feat: Adiciona endpoint X')

Push (git push origin feature/incrivel)

Abra um Pull Request!

# 📄 Licença
Distribuído sob licença CC BY-NC-SA 4.0. Veja LICENSE para detalhes. 
<br>
[![CC BY-NC-SA 4.0](https://licensebuttons.net/l/by-nc-sa/4.0/88x31.png)](https://creativecommons.org/licenses/by-nc-sa/4.0/)


   
# Notas de atualização v1.1.0
Foi implementado um novo serviço de criptografia intermediário na camada de aplicação do sistema. Agora, todos os dados sensíveis (como humor, notas de humor, anotações rápidas e hábitos) são criptografados em repouso. Dessa forma, mesmo que terceiros obtenham acesso ao banco de dados, as informações dos usuários permanecerão protegidas. As adições incluem:

- Uso de um serviço de criptografia (AES) para criptografar dados antes de salvá-los no banco.
- Descriptografia automática no momento da leitura, garantindo que apenas o usuário autenticado visualize seus dados em texto legível.
- Configuração de chaves de criptografia por meio de variáveis de ambiente, maximizando a segurança sem expor segredos no código.