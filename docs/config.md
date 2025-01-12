# Configuração do Projeto

Este repositório utiliza o .NET 8.0 e Entity Framework Core para gerenciar a aplicação e suas dependências.

## Estrutura do Projeto

1. Para criar a estrutura inicial do projeto:
```
bash
dotnet new webapi -n MoodTracker
```

2. Para instalar pacotes necessários:
```
bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Swashbuckle.AspNetCore
```

3. Para rodar a aplicação:
```
bash
dotnet run
```

## Como Executar

1. Clone o repositório:
```
bash
git clone https://github.com/JhonataAugust0/moodtracker.git
```
2. Configure as credenciais do banco de dados no arquivo appsettings.json.
          
3. Aplique as migrações:
```
bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

4. Inicie o servidor:
```
bash
dotnet run
```

5. Alternativamente, use Docker:
```
bash
docker-compose up -d --build
```