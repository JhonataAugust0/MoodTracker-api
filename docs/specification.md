# Especificação do Projeto

O MoodTracker é uma aplicação projetada para ajudar os usuários a rastrear e analisar seu humor e hábitos ao longo do tempo. Ele permite o registro diário de humor, o gerenciamento de hábitos, a criação de notas rápidas e a visualização de dados para melhorar o bem-estar e a produtividade.


## Personas

1. Usuário Principal:
   - Quer registrar seu humor, gerenciar hábitos e acompanhar sua evolução.
   - Deseja uma interface fácil de usar, com gráficos interativos e feedback visual.
2. Administrador da Aplicação:
   - Precisa monitorar o sistema, garantindo que as funcionalidades estejam funcionando corretamente e que os dados dos usuários estejam seguros.

## Histórias de Usuário

| EU COMO... `PERSONA`                                                                                         | QUERO/PRECISO ... `FUNCIONALIDADE`                                                             |PARA ... `MOTIVO/VALOR`               |
|--------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------|--------------------------------------|
|Usuário Principal|	Registrar meu humor diário	Acompanhar minhas variações emocionais |Acompanhar minhas variações emocionais|
|Usuário Principal|	Criar e gerenciar hábitos	Monitorar minha produtividade | Monitorar minha produtividade|
|Usuário Principal|	Consultar gráficos e relatórios	Identificar padrões de comportamento | Identificar padrões de comportamento|
|Administrador|	Garantir a segurança dos dados dos usuários	Manter a privacidade e integridade das informações |Manter a privacidade e integridade das informações|


## Requisitos Funcionais

| ID     | Descrição do Requisito                                                                                                                        | Prioridade | 
|--------|-----------------------------------------------------------------------------------------------------------------------------------------------|------------| 
| RF-001 | A API deve permitir o registro do usuário com email e senha.	                                                                                 | Alta       |
| RF-002 | A API deve gerar um SALT aleatório para armazenar o hash da senha do usuário	                                                                 | Alta       |
| RF-003 | A API deve exigir autenticação JWT usando o email e senha do usuário com criptografia SHA-256 para acessar a área do dashboard                | Alta       |
| RF-004 | A API deve implementar refresh token para manter a sessão do usuário.                                                                         | Alta       |
| RF-005 | A API deve fornecer ao usuário a possibilidade de alterar sua senha em caso de esquecimento enviando um link de recuperação ao email do mesmo | Alta       | 
| RF-006 | A API deve permitir o encerramento da sessão do usuário.	                                                                                     | Alta       |
| RF-007 | A API deve permitir o registro de humor diário.	                                                                                             | Alta       |
| RF-008 | A API deve permitir múltiplos registros de humor por dia.	                                                                                 | Alta       |
| RF-009 | A API deve permitir adição de notas ao registro de humor	                                                                                     | Alta       |
| RF-010 | A API deve permitir a visualização do histórico do registro de humor	                                                                         | Alta       |
| RF-011 | A API deve permitir o gerenciamento de hábitos (criar, editar, excluir).	                                                                     | Alta       |
| RF-012 | A API deve permitir o registro da realização de hábitos.		                                                                                 | Alta       |
| RF-013 | A API deve permitir a visualização do histórico da realização de hábitos	                                                                     | Alta       |
| RF-014 | A API deve permitir criar, editar, excluir tags	                                                                                             | Alta       |
| RF-015 | A API deve permitir a associação de hábitos com tags                                                                                          | Alta       |
| RF-016 | A API deve oferecer a visualização de calendário interativo para visualização de dados (humor, hábitos).	                                     | Alta       |
| RF-017 | A API deve permitir a criação, edição e exclusão de notas rápidas.	                                                                         | Alta       |                                                                     | Alta       |

## Requisitos não Funcionais

|ID     | Descrição do Requisito  |Prioridade |
|-------|-------------------------|----|
|RNF-001|	A aplicação deve garantir alta disponibilidade (99,9% de uptime).	|Alta|
|RNF-002|	A API deve ser responsiva com tempos de resposta inferiores a 1 segundo.	|Alta|
|RNF-003|	A aplicação deve ser documentada com Swagger para fácil integração.|	Alta|
|RNF-004|	O sistema deve suportar crescimento escalável.	|Média|

## Restrições

|ID| Restrição                                             |
|--|-------------------------------------------------------|
|R-001|	O projeto deve ser desenvolvido utilizando C# e ASP.NET Core.
|R-002|O banco de dados deve ser implementado com PostgreSQL.
|R-003|O sistema deve seguir o padrão de arquitetura hexagonal.
|R-004|	A aplicação deve ser compatível com dispositivos móveis e desktops.