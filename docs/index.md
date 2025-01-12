# MoodTracker

## Objetivo do projeto

O MoodTracker é uma aplicação para rastrear humor e hábitos, permitindo aos usuários monitorar seu bem-estar emocional e comportamental de forma intuitiva e visual. O sistema inclui funcionalidades como registro diário de humor, gerenciador de hábitos, blocos de notas rápidas, e painéis de visualização de dados para análise.
### Sumário

- [Configuração do projeto](./config.md)
- [Especificação do projeto](./specification.md)
- [Testes](./tests.md)

## Estrutura do Projeto

A arquitetura do MoodTracker segue o padrão de arquitetura hexagonal (Ports and Adapters), garantindo um design modular, desacoplado e fácil de manter.
```
/Application
    Casos de uso e portas que conectam o domínio às dependências externas
/adapters
    Implementações externas (ASP.NET Core, persistência de dados com Entity Framework Core)
/domain
    Lógica central do projeto e regras de negócio
/infrastructure
    Camada
/tests
    Testes automatizados (xUnit, Moq)
Program.cs
    Ponto de entrada da aplicação
```