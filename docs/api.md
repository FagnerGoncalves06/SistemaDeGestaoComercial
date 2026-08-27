# API HTTP

Base local: `http://localhost:5291/api`. As mesmas rotas também estão disponíveis sob `/api/v1`.

## Autenticação

- `POST /auth/login` — autentica e grava o cookie `gestao_access_token`.
- `POST /auth/logout` — encerra a sessão no navegador.
- `GET /auth/session` — restaura os dados da sessão atual.

O navegador deve enviar credenciais (`withCredentials: true`). O token não é retornado no corpo do login. No Swagger, um token Bearer pode ser informado para testes manuais.

## Recursos

- `/clientes` — cadastro, consulta, alteração e exclusão lógica.
- `/clientes/{id}/compras` — histórico paginado de compras.
- `/produtos` — catálogo e manutenção de produtos.
- `/estoque/movimentacoes` — histórico e ajustes de estoque.
- `/vendas` — listagem e criação de vendas.
- `/vendas/{id}/cancelar` — cancelamento transacional.
- `/vendas/{id}/recibo` — dados do recibo.
- `/financeiro` e `/financeiro/despesas` — movimentações e despesas.
- `/dashboard` — indicadores consolidados.
- `/usuarios` — administração de usuários; exige perfil Administrador.
- `/cep/{cep}` — consulta de endereço via ViaCEP.

## Convenções

- Listagens recebem `pagina` e `tamanhoPagina`; busca de clientes e produtos recebe `busca`.
- Criação de venda exige o header `Idempotency-Key`, com valor único por intenção de venda.
- Erros usam `application/problem+json` conforme RFC 7807.
- `401` indica ausência, expiração ou revogação da sessão; `403`, falta de permissão; `409`, conflito de persistência ou idempotência.
- `GET /health` informa atividade do processo e `GET /health/ready` verifica também o SQL Server.

A especificação completa e interativa é gerada pelo Swagger em `http://localhost:5291/swagger` no ambiente Development.
