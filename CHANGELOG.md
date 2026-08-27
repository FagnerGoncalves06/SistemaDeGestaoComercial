# Changelog

Todas as mudanças relevantes deste projeto são documentadas aqui.

## [1.0.0] - 2026-08-27

### Adicionado

- Módulos de clientes, produtos, estoque, PDV, vendas, financeiro, usuários e dashboard.
- Clean Architecture pragmática com domínio, aplicação, infraestrutura, API e frontend.
- Autenticação JWT em cookie HttpOnly, autorização por perfil e revogação de sessão.
- Paginação ponta a ponta e idempotência na criação de vendas.
- Sequence para número da venda e `rowversion` para concorrência de produto.
- Migrations, script SQL idempotente, health checks e Swagger/OpenAPI.
- Testes unitários, HTTP, SQL Server, componentes React e E2E.
- CI com GitHub Actions, SQL Server e validações backend/frontend.
- Documentação de arquitetura, API, segurança e contribuição.
