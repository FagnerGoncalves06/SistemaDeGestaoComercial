# Análise técnica

## Parecer executivo

O projeto está adequado para publicação como portfólio e demonstra competências de desenvolvimento full stack, modelagem de domínio, persistência transacional, segurança e testes. A solução adota um monólito modular com separação inspirada em Clean Architecture. Essa escolha é proporcional ao porte atual: mantém implantação simples e permite crescimento por módulos sem introduzir a complexidade operacional de microsserviços.

O sistema não deve ser apresentado como produto pronto para operação crítica em larga escala. Para esse cenário ainda seriam necessários observabilidade centralizada, gestão externa de segredos, cache distribuído, estratégia de backup e recuperação, testes de carga, mais cenários E2E e controles como MFA. Essas limitações não invalidam o projeto de portfólio; documentá-las demonstra maturidade técnica.

## Avaliação por camada

### Domínio

- Entidades concentram invariantes e usam `decimal` para valores monetários.
- Vendas preservam o preço praticado e não são removidas fisicamente.
- Cancelamento gera estorno separado de receita e despesa, mantendo rastreabilidade financeira.
- Produto possui token `rowversion`, mais explícito que usar quantidade para detectar concorrência.
- O domínio não referencia frameworks nem outras camadas.

Ponto de evolução: dividir o arquivo agregado de entidades por módulo quando o domínio crescer e considerar tipos-valor para dinheiro, documento, e-mail e endereço.

### Aplicação

- Casos de uso dependem de portas, não de EF Core.
- DTOs delimitam os contratos expostos pela API.
- Venda, estoque, financeiro e idempotência participam da mesma unidade de trabalho.
- A sessão validada por JWT usa cache curto e invalidação explícita, reduzindo consultas repetidas.
- Relógio de negócio abstraído permite testar limites de data e fuso horário.

Ponto de evolução: separar classes hoje agrupadas em arquivos grandes por caso de uso e adicionar validação declarativa por contrato caso a quantidade de comandos aumente.

### Infraestrutura

- EF Core e SQL Server implementam repositórios e unidade de trabalho.
- Sequence do SQL Server evita colisão na numeração de vendas.
- Índices atendem consultas por datas e filtros operacionais.
- Restrições de tamanho evitam o uso indiscriminado de `nvarchar(max)`.
- `rowversion`, transação e isolamento serializável protegem fluxos concorrentes críticos.
- O seed exige ativação e senhas por configuração; não existe hash administrativo fixo versionado.

Ponto de evolução: usar cache distribuído em múltiplas instâncias e introduzir outbox se integrações assíncronas passarem a fazer parte do produto.

### API

- Autenticação JWT, autorização por perfil, cookie `HttpOnly`, CORS com credenciais e proteção de origem.
- Rate limiting global e específico para login.
- Erros seguem RFC 7807 (`ProblemDetails`).
- Health checks distinguem processo ativo e prontidão do banco.
- Correlation ID facilita rastreamento básico.
- Rotas `/api` são mantidas e há aliases versionados em `/api/v1`.
- Swagger/OpenAPI está disponível em desenvolvimento.

Ponto de evolução: logs estruturados em coletor externo, métricas, tracing distribuído, rotação/refresh token conforme o modelo de implantação e política formal de proteção de dados.

### Frontend

- React e TypeScript estrito consomem contratos tipados.
- Formulários usam React Hook Form e Zod.
- O token não fica em `localStorage` ou `sessionStorage`; a sessão é restaurada por cookie.
- Listagens compartilham paginação compatível com `pagina` e `tamanhoPagina` do backend.
- Busca possui debounce e o carrinho do PDV não perde itens ao trocar página.
- Venda envia uma chave idempotente estável durante tentativas repetidas.

Ponto de evolução: ampliar testes de componentes e E2E, aplicar code splitting por rota e adotar uma biblioteca de cache de servidor se o volume de telas crescer.

## Banco de dados

O banco é SQL Server, acessado por EF Core com migrations versionadas. O modelo utiliza chaves `uniqueidentifier`, valores financeiros com precisão explícita, textos com limites, índices operacionais, chave estrangeira, sequence para número da venda e `rowversion` para concorrência otimista. O arquivo `migrations.sql` permite revisar ou aplicar o esquema por script.

A aplicação mantém venda, itens, baixa/reposição de estoque, movimento financeiro e registro de idempotência na mesma transação. Repetir uma criação de venda com a mesma chave e o mesmo conteúdo retorna a venda existente; reutilizar a chave com conteúdo diferente gera conflito.

## Autenticação e segurança

- Hash de senha PBKDF2-SHA512, salt aleatório e comparação em tempo constante.
- Chave JWT obrigatória com pelo menos 32 bytes e fornecida fora do código.
- JWT com emissor, audiência, validade e assinatura verificados.
- Cookie de acesso `HttpOnly`, `SameSite=Strict` e `Secure` sob HTTPS.
- Sessões revogadas quando usuário, perfil ou senha mudam.
- Respostas não devolvem o token ao JavaScript.
- Login limitado por IP; requisições mutáveis com origem divergente são rejeitadas.
- Headers básicos de segurança e HTTPS/HSTS em produção.

Em produção, a chave deve residir em um secret manager, a aplicação deve estar atrás de proxy confiável com TLS e a lista de proxies conhecidos deve ser configurada. MFA e uma estratégia de renovação de sessão são evoluções recomendadas para dados sensíveis.

## Testes automatizados

Na validação local de 27 de agosto de 2026:

- 13 testes unitários de domínio e aplicação passaram.
- 7 testes de integração HTTP passaram.
- 4 testes de integração com SQL Server estão implementados e são habilitados por `GESTAO_TEST_SQLSERVER`; sem essa variável são ignorados de forma intencional.
- 2 testes de componente frontend passaram com Vitest e Testing Library.
- 1 teste E2E passou no Chromium com Playwright.
- Build backend em Release passou sem avisos ou erros.
- Prettier, ESLint, TypeScript estrito e build frontend passaram.

O CI sobe um SQL Server isolado e executa também os quatro testes reais, cobrindo migrations, rollback, idempotência, cancelamento/estorno e concorrência.

## Escalabilidade

O desenho suporta evolução para um sistema maior desde que os módulos continuem separados e as operações permaneçam stateless fora do cache local. Escala vertical e replicação da API são viáveis; para múltiplas instâncias, o cache de sessão em memória deve migrar para Redis ou equivalente. A separação futura em serviços só se justifica quando limites de domínio, volume ou equipes independentes exigirem isso.

## Conclusão

Para portfólio, o projeto está tecnicamente consistente, executável, testado e documentado. Os principais riscos que existiam — paginação divergente, hash fixo, colisão no número da venda, concorrência, validação, estorno ambíguo e armazenamento inseguro do token — possuem tratamento explícito. As evoluções restantes são de maturidade operacional e expansão de cobertura, não bloqueadores para a publicação.
