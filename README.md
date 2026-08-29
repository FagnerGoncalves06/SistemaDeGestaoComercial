# Sistema de Gestão Comercial

Aplicação full stack de gestão comercial para clientes, produtos, estoque, PDV, vendas, financeiro, usuários e dashboard. O backend usa Clean Architecture pragmática e o frontend consome contratos HTTP tipados.

## Tecnologias e arquitetura

- .NET 10/C# 14, ASP.NET Core, EF Core 10, SQL Server, JWT, Swagger/OpenAPI e xUnit.
- React 19, TypeScript estrito, Vite, Tailwind CSS, componentes locais no padrão shadcn/ui, React Hook Form, Zod e Axios.
- Dependências: `Dominio <- Aplicacao <- Infraestrutura <- API`. A Aplicação contém casos de uso separados por módulo e portas de persistência; a Infraestrutura implementa EF Core, repositórios e unidade de trabalho.
- Estoque, venda, movimentos e financeiro são persistidos na mesma transação. Vendas usam isolamento `Serializable` e `rowversion` para concorrência otimista.
- Números de venda são obtidos de uma sequence do SQL Server, evitando colisões entre requisições concorrentes.

## Desenvolvimento assistido por Inteligência Artificial

Este projeto também foi utilizado como estudo prático de desenvolvimento de software assistido por Inteligência Artificial, incorporando ferramentas de IA generativa e AI Coding ao fluxo de engenharia sem substituir a análise e a responsabilidade técnica do desenvolvedor.

A IA foi utilizada como apoio em diferentes etapas do ciclo de desenvolvimento, incluindo:

- análise e refinamento de requisitos;
- planejamento de funcionalidades e divisão de tarefas;
- apoio à implementação de código backend e frontend;
- análise de código e identificação de possíveis problemas;
- investigação e correção de bugs;
- refatoração e melhoria de código existente;
- criação e evolução de testes unitários, de integração e end-to-end;
- documentação técnica e definição de instruções reutilizáveis para o desenvolvimento.

As sugestões produzidas por IA são tratadas como insumos para análise, e não como decisões automáticas. Código, arquitetura, regras de negócio, segurança, persistência, concorrência e demais decisões técnicas são revisados e validados antes de serem incorporados ao projeto.

O repositório também mantém instruções e prompts utilizados durante o processo em docs/prompts, permitindo documentar parte da interação entre engenharia de software e ferramentas de IA.

O objetivo dessa abordagem é explorar como a IA pode aumentar a produtividade, acelerar ciclos de análise e implementação e apoiar a qualidade do software, mantendo princípios como SOLID, Clean Architecture, testes automatizados, revisão técnica e responsabilidade sobre o código produzido.

## Pré-requisitos

- SDK .NET 10.
- Node.js 22+ e npm.
- SQL Server 2022, local ou via Docker.

## SQL Server e configurações

Copie `.env.example` somente como referência e defina os valores no ambiente. Não versione o arquivo `.env`.

```powershell
$env:SQLSERVER_SA_PASSWORD = '<senha-forte>'
docker compose up -d
$env:ConnectionStrings__SqlServer = "Server=localhost,1433;Database=GestaoComercial;User Id=sa;Password=$env:SQLSERVER_SA_PASSWORD;TrustServerCertificate=True"
$env:GESTAO_JWT_KEY = '<chave-aleatoria-com-32-ou-mais-bytes>'
```

Para dados fictícios de desenvolvimento:

```powershell
$env:Seed__Enabled = 'true'
$env:Seed__AdminPassword = '<senha-admin>'
$env:Seed__OperadorPassword = '<senha-operador>'
```

Os logins criados são `admin@gestao.test` e `operador@gestao.test`; as senhas são exclusivamente as variáveis acima. Clientes, produtos, estoque, venda e despesa são fictícios.

## Migrations e execução

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project .\SistemaDeGestaoComercial.Infraestrutura\SistemaDeGestaoComercial.Infraestrutura.csproj --startup-project .\SistemaDeGestaoComercial.Api\SistemaDeGestaoComercial.Api.csproj --context AppDbContext
$env:Frontend__Url = 'http://localhost:5173'
$env:Negocio__FusoHorario = 'America/Sao_Paulo'
dotnet run --project .\SistemaDeGestaoComercial.Api\SistemaDeGestaoComercial.Api.csproj --urls 'http://localhost:5291'
```

Swagger fica em `http://localhost:5291/swagger`. A migration inicial está em `SistemaDeGestaoComercial.Infraestrutura/Persistencia/Migrations`.

Frontend:

```powershell
cd frontend
cmd /c npm install
$env:VITE_API_URL = 'http://localhost:5291/api'
npm.cmd run dev -- --host localhost --port 5173 --strictPort
```

## Endpoints principais

- `POST /api/auth/login`
- `POST /api/auth/logout` e `GET /api/auth/session`
- `/api/clientes` e `/api/clientes/{id}/compras`
- `/api/produtos` e `/api/estoque/movimentacoes`
- `/api/vendas`, `/api/vendas/{id}/cancelar` e `/api/vendas/{id}/recibo`
- `/api/financeiro`, `/api/financeiro/despesas` e `/api/dashboard`
- `/api/usuarios` (Administrador) e `/api/cep/{cep}`

Listagens aceitam `pagina` e `tamanhoPagina`; clientes e produtos aceitam `busca`. Erros usam RFC 7807 `ProblemDetails`.

`GET /health` verifica a disponibilidade do processo e `GET /health/ready` verifica também a conexão com o SQL Server.

## Builds e testes

```powershell
$env:GESTAO_JWT_KEY = 'chave-exclusiva-de-teste-com-32-bytes'
.\.dotnet\dotnet.exe restore SistemaDeGestaoComercial.sln
.\.dotnet\dotnet.exe build SistemaDeGestaoComercial.sln --no-restore
.\.dotnet\dotnet.exe test SistemaDeGestaoComercial.sln --no-build --no-restore
cd frontend
cmd /c npm run lint
cmd /c npm run build
```

## Decisões principais

- Valores financeiros são `decimal`; datas persistidas são UTC.
- Exclusão de cliente/produto com histórico é lógica; vendas nunca são apagadas.
- Itens guardam o preço do momento da venda.
- Senhas usam PBKDF2-SHA512 com salt aleatório e comparação em tempo constante.
- Tokens JWT expiram em uma hora e são invalidados quando o usuário é desativado, troca de perfil ou senha.
- O JWT é entregue em cookie `HttpOnly`, `SameSite=Strict`; não fica acessível ao JavaScript. A validação da sessão usa cache curto com invalidação explícita para reduzir consultas ao banco.
- O login possui rate limiting e a sessão é encerrada automaticamente após uma resposta `401`.
- Criações de venda exigem `Idempotency-Key`; chave e hash do payload impedem duplicação por retry.
- Datas são persistidas em UTC e os relatórios usam o fuso configurado em `Negocio__FusoHorario`.
- Estornos são apresentados separadamente de despesas; faturamento e saldo usam valores líquidos.
- ViaCEP usa cliente HTTP gerenciado e permite preenchimento manual quando indisponível.
- Secrets e senhas entram apenas por configuração/variáveis de ambiente.

## Testes reais com SQL Server

A suíte SQL Server cria um banco temporário exclusivo, aplica todas as migrations e o remove ao final. Ela cobre rollback da venda, idempotência, cancelamento/estorno e concorrência por `rowversion`.

```powershell
$env:GESTAO_TEST_SQLSERVER = 'Server=localhost,1433;Database=master;User Id=sa;Password=<senha>;Encrypt=False;TrustServerCertificate=True'
dotnet test SistemaDeGestaoComercial.Testes.Integracao
```

Sem essa variável, somente esses testes externos são ignorados. O GitHub Actions executa a suíte completa com SQL Server isolado.

Testes do frontend:

```powershell
cd frontend
npm run test
npm run test:e2e
```

## Documentação

- [Análise técnica e parecer para portfólio](docs/analise-tecnica.md)
- [Arquitetura e decisões](docs/arquitetura.md)
- [Catálogo da API](docs/api.md)
- [Como contribuir](CONTRIBUTING.md)
- [Política e configuração de segurança](SECURITY.md)
- [Histórico de mudanças](CHANGELOG.md)
