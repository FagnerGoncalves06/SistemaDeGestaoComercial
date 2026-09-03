# Implementação de Mensageria com RabbitMQ no Sistema de Gestão Comercial

## Contexto

Você está trabalhando no projeto:

`SistemaDeGestaoComercial`

Repositório:

`FagnerGoncalves06/SistemaDeGestaoComercial`

O sistema é uma aplicação full stack de gestão comercial construída com:

- .NET 10;
- C#;
- ASP.NET Core;
- Entity Framework Core;
- SQL Server;
- React;
- TypeScript;
- Docker;
- Clean Architecture pragmática;
- SOLID;
- JWT;
- testes automatizados.

A arquitetura atual segue aproximadamente:

```text
Dominio
   ↑
Aplicacao
   ↑
Infraestrutura
   ↑
Api
```

O sistema possui módulos de:

- clientes;
- produtos;
- estoque;
- vendas;
- financeiro;
- usuários;
- dashboard.

A criação de uma venda atualmente é uma operação crítica e transacional, envolvendo:

1. criação da venda;
2. baixa do estoque;
3. movimentação financeira;
4. registro de idempotência da requisição;
5. commit da transação.

Essa consistência deve ser preservada.

---

# Objetivo

Implementar mensageria utilizando RabbitMQ para introduzir uma arquitetura orientada a eventos no sistema.

O evento inicial será:

```text
VendaRealizada
```

Após a confirmação de uma venda, deverá ser produzido um evento assíncrono que poderá ser consumido por diferentes funcionalidades.

A primeira funcionalidade concreta será:

> detectar produtos que ficaram com estoque igual ou inferior ao estoque mínimo após uma venda e gerar um alerta de estoque baixo.

A implementação deve demonstrar domínio técnico de:

- RabbitMQ;
- Producer;
- Consumer;
- Exchanges;
- Queues;
- Routing Keys;
- eventos de integração;
- processamento assíncrono;
- Transactional Outbox Pattern;
- retry;
- Dead Letter Queue;
- idempotência de consumidores;
- Inbox Pattern;
- BackgroundService;
- observabilidade;
- Docker;
- testes automatizados.

Não implemente RabbitMQ apenas como chamada direta dentro do `VendaService`.

A solução deve utilizar **Transactional Outbox**.

---

# Regra arquitetural principal

NÃO substituir a transação atual de:

```text
Venda
+
Estoque
+
Financeiro
```

por mensageria.

Essas operações pertencem à mesma transação de negócio e devem continuar fortemente consistentes.

A mensageria será utilizada apenas para consequências assíncronas posteriores à conclusão da venda.

Fluxo desejado:

```text
POST /api/vendas
       ↓
VendaService
       ↓
┌───────────────────────────────┐
│ TRANSAÇÃO SQL SERVER          │
│                               │
│ Venda                         │
│ Estoque                       │
│ Financeiro                    │
│ Idempotência HTTP             │
│ OutboxMessage                 │
│                               │
└───────────────────────────────┘
       ↓
COMMIT
       ↓
OutboxProcessor
       ↓
RabbitMQ
       ↓
VendaRealizadaConsumer
       ↓
Verificação de estoque
       ↓
Alerta de estoque baixo
```

Não publicar diretamente no RabbitMQ antes ou depois do commit da venda.

---

# Regras antes de começar

Antes de alterar qualquer arquivo:

1. analise toda a solution;
2. identifique todos os projetos `.csproj`;
3. analise as referências entre projetos;
4. analise `VendaService`;
5. analise `Produto`;
6. analise `AppDbContext`;
7. analise `DependencyInjection`;
8. analise `docker-compose`;
9. analise as migrations existentes;
10. analise a suíte de testes;
11. analise os padrões de nomenclatura e organização já existentes.

Não introduza padrões incompatíveis com o projeto.

Preserve:

- Clean Architecture;
- SOLID;
- CancellationToken;
- async/await;
- injeção de dependência;
- configuração por variáveis de ambiente;
- tratamento de erros existente;
- convenções atuais de entidades;
- datas em UTC;
- testes existentes.

Antes de implementar, apresente resumidamente quais arquivos serão criados e quais serão alterados.

Depois disso, execute a implementação.

---

# ETAPA 1 — Preparar RabbitMQ com Docker

Analise o `docker-compose` atual.

Adicione RabbitMQ utilizando imagem oficial com Management Plugin.

Utilizar conceitualmente:

```text
rabbitmq:management
```

Expor:

```text
5672
```

para comunicação AMQP.

E:

```text
15672
```

para o RabbitMQ Management.

Não deixar usuário e senha reais hardcoded no repositório.

Adicionar variáveis correspondentes ao `.env.example`.

Exemplo conceitual:

```text
RABBITMQ_USER
RABBITMQ_PASSWORD
```

Configurar volume persistente para RabbitMQ.

Depois da alteração, deve ser possível iniciar:

```bash
docker compose up -d
```

e acessar o Management Plugin localmente.

---

# ETAPA 2 — Configuração do RabbitMQ

Criar configuração tipada.

Exemplo de estrutura:

```text
SistemaDeGestaoComercial.Infraestrutura
└── Mensageria
    └── RabbitMqOptions.cs
```

A configuração deve conter, quando aplicável:

```text
HostName
Port
UserName
Password
VirtualHost
Exchange
```

Utilizar `IOptions`.

Não espalhar strings mágicas pelo projeto.

Centralizar nomes de:

- exchange;
- routing keys;
- filas;
- dead letter exchange;
- filas de retry;
- filas de dead letter.

Criar constantes ou configuração apropriada.

---

# ETAPA 3 — Contrato do evento VendaRealizada

Criar um contrato explícito para o evento.

Não serializar e publicar diretamente a entidade de domínio `Venda`.

Criar algo semelhante a:

```csharp
public sealed record VendaRealizadaEvent(
    Guid EventoId,
    Guid VendaId,
    string NumeroVenda,
    Guid? ClienteId,
    decimal Total,
    DateTime DataVenda,
    IReadOnlyCollection<ItemVendaRealizadaEvent> Itens
);
```

E:

```csharp
public sealed record ItemVendaRealizadaEvent(
    Guid ProdutoId,
    int Quantidade
);
```

Ajustar nomes ou namespaces conforme padrões existentes.

O evento deve ter um identificador único:

```text
EventoId
```

Esse identificador será posteriormente utilizado na idempotência dos consumidores.

Não colocar objetos EF Core ou propriedades desnecessárias dentro da mensagem.

---

# ETAPA 4 — Implementar Transactional Outbox

Criar uma entidade/tabela de Outbox.

Nome sugerido:

```text
OutboxMessage
```

Ela deverá conter pelo menos:

```text
Id
Tipo
Conteudo
CreatedAt
ProcessedAt
Tentativas
Erro
```

Considere também, se útil:

```text
CorrelationId
```

Requisitos:

- `Id` deve identificar unicamente a mensagem;
- `Conteudo` deve armazenar JSON;
- `Tipo` deve identificar o contrato/evento;
- `CreatedAt` deve ser UTC;
- `ProcessedAt` deve permanecer nulo enquanto a mensagem não tiver sido publicada com sucesso;
- `Tentativas` deve registrar falhas;
- `Erro` deve armazenar informação resumida da última falha.

Não misturar a entidade Outbox com regras de negócio da entidade Venda.

Criar configuração EF Core adequada.

Criar migration.

---

# ETAPA 5 — Abstração da Outbox

Criar uma abstração adequada na camada Aplicação.

Exemplo conceitual:

```text
IOutboxRepositorio
```

Ela deve permitir operações como:

```text
Adicionar(...)
ObterPendentesAsync(...)
MarcarComoProcessada(...)
RegistrarFalha(...)
```

A implementação deve ficar na Infraestrutura.

Evite fazer a Aplicação depender de EF Core.

---

# ETAPA 6 — Registrar VendaRealizada na mesma transação da venda

Alterar cuidadosamente o fluxo de `VendaService.CriarVendaAsync`.

Hoje a operação deve continuar realizando:

```text
Venda
Estoque
Financeiro
Idempotência
```

Adicionar também:

```text
OutboxMessage
```

dentro da MESMA transação SQL Server.

Fluxo esperado:

```text
BEGIN TRANSACTION

INSERT Venda
UPDATE Estoque
INSERT MovimentoEstoque
INSERT MovimentoFinanceiro
INSERT Idempotencia
INSERT OutboxMessage

COMMIT
```

Se qualquer parte falhar:

```text
ROLLBACK
```

O evento da Outbox só deve existir se a venda também existir.

Não chamar RabbitMQ dentro de `VendaService`.

Após persistir a venda e a Outbox, confirmar a transação normalmente.

Preservar toda a idempotência já existente no endpoint de vendas.

---

# ETAPA 7 — Criar abstração para publicação de eventos

Criar interface adequada, por exemplo:

```csharp
public interface IEventPublisher
{
    Task PublicarAsync(
        OutboxMessage mensagem,
        CancellationToken cancellationToken);
}
```

Ou uma abstração equivalente mais coerente com a arquitetura existente.

A Aplicação não deve depender diretamente de RabbitMQ.

A implementação concreta ficará na Infraestrutura.

---

# ETAPA 8 — Implementar RabbitMqPublisher

Criar:

```text
RabbitMqPublisher
```

Responsabilidades:

1. estabelecer conexão com RabbitMQ;
2. declarar exchange;
3. publicar mensagens;
4. configurar mensagens como persistentes;
5. adicionar `MessageId`;
6. adicionar `CorrelationId`, quando disponível;
7. utilizar routing key adequada;
8. utilizar confirmação de publicação quando apropriado;
9. registrar logs estruturados.

Exchange principal sugerida:

```text
gestao-comercial.events
```

Routing key inicial:

```text
venda.realizada
```

Utilizar exchange apropriada para permitir futuros consumidores.

Preferência:

```text
topic
```

ou outra opção tecnicamente justificável.

Não criar uma nova conexão RabbitMQ para cada mensagem se isso puder ser evitado.

Implementar gerenciamento de conexão seguindo boas práticas da biblioteca utilizada.

---

# ETAPA 9 — Implementar OutboxProcessor

Criar um `BackgroundService`.

Nome sugerido:

```text
OutboxProcessor
```

Responsabilidade:

```text
SQL Server Outbox
        ↓
RabbitMQ
```

Fluxo:

1. consultar mensagens com `ProcessedAt == null`;
2. limitar a quantidade por lote;
3. publicar individualmente;
4. após confirmação do RabbitMQ, marcar como processada;
5. registrar falhas;
6. incrementar tentativas;
7. continuar processando outras mensagens quando apropriado.

Utilizar scopes corretamente porque o `DbContext` é scoped.

Não injetar diretamente um `DbContext` scoped em singleton `BackgroundService`.

Utilizar:

```text
IServiceScopeFactory
```

ou abordagem equivalente correta.

Processar pequenos lotes.

Exemplo:

```text
50 mensagens
```

O intervalo de polling deve ser configurável.

Evitar loop agressivo consumindo CPU.

Utilizar `CancellationToken`.

Adicionar logs como:

```text
Mensagem Outbox publicada
Falha ao publicar mensagem Outbox
Quantidade de mensagens processadas
```

---

# ETAPA 10 — Configurar filas e routing

Criar arquitetura inicial:

```text
Exchange:
gestao-comercial.events
```

Routing key:

```text
venda.realizada
```

Fila:

```text
gestao-comercial.estoque
```

Binding:

```text
gestao-comercial.events
        │
        │ venda.realizada
        ↓
gestao-comercial.estoque
```

A fila deve ser durable.

As mensagens relevantes devem sobreviver a reinicializações quando possível.

---

# ETAPA 11 — Criar consumidor VendaRealizada

Criar consumidor para:

```text
VendaRealizadaEvent
```

Nome sugerido:

```text
VendaRealizadaConsumer
```

Implementar inicialmente dentro da solução atual.

Não transformar o sistema em microserviços neste momento.

O consumidor deve funcionar de forma assíncrona utilizando `BackgroundService`, serviço hospedado ou abordagem equivalente adequada.

Fluxo:

```text
RabbitMQ
   ↓
VendaRealizadaConsumer
   ↓
carrega produtos envolvidos
   ↓
verifica estoque atual
```

Para cada produto:

```csharp
QuantidadeEstoque <= EstoqueMinimo
```

deve ser considerado estoque baixo.

---

# ETAPA 12 — Criar funcionalidade de Alerta de Estoque

Criar uma entidade adequada para representar um alerta.

Nome sugerido:

```text
AlertaEstoque
```

Possíveis propriedades:

```text
Id
ProdutoId
VendaId
QuantidadeAtual
EstoqueMinimo
CreatedAt
Visualizado
```

Ajustar de acordo com o domínio existente.

Quando uma venda deixar determinado produto com estoque igual ou inferior ao mínimo:

```text
QuantidadeEstoque <= EstoqueMinimo
```

o consumidor deverá criar um alerta.

Exemplo:

```text
Produto: Teclado Logitech
Estoque atual: 3
Estoque mínimo: 5
Venda: V000000000152
```

Evitar criar alertas duplicados indevidamente para o mesmo evento.

Criar migration.

---

# ETAPA 13 — Inbox Pattern e idempotência do consumidor

RabbitMQ normalmente trabalha com entrega `at least once`.

Portanto uma mensagem poderá ser entregue mais de uma vez.

O consumidor deve ser idempotente.

Criar tabela:

```text
InboxMessages
```

Campos mínimos:

```text
MessageId
Consumer
ProcessedAt
```

Adicionar informações extras somente se justificadas.

Antes de processar:

```text
VendaRealizadaEvent
```

verificar se:

```text
EventoId + Consumer
```

já foi processado.

Se já tiver sido:

```text
ACK
```

sem executar novamente as regras.

Caso contrário:

1. processar;
2. criar alertas;
3. registrar Inbox;
4. salvar tudo na mesma transação local;
5. somente depois confirmar a mensagem no RabbitMQ.

O objetivo é impedir criação duplicada de alertas.

---

# ETAPA 14 — ACK e NACK

Configurar o consumidor para trabalhar com confirmação explícita.

Não utilizar confirmação automática sem avaliar a consequência.

Fluxo de sucesso:

```text
Mensagem recebida
↓
Processamento
↓
Banco salvo
↓
ACK
```

Fluxo de erro:

```text
Mensagem recebida
↓
Erro
↓
NACK / estratégia de retry
```

Nunca confirmar uma mensagem antes de concluir com sucesso o processamento local.

---

# ETAPA 15 — Retry

Implementar estratégia de retry.

Evitar retry infinito imediato.

Utilizar uma fila de retry ou estratégia RabbitMQ adequada.

Exemplo de arquitetura:

```text
gestao-comercial.estoque
        ↓
       erro
        ↓
gestao-comercial.estoque.retry
        ↓
      TTL
        ↓
gestao-comercial.estoque
```

Definir número máximo de tentativas.

Exemplo:

```text
3 tentativas
```

O número exato pode ser ajustado tecnicamente.

Não criar loop infinito de redelivery.

Registrar logs de retry.

---

# ETAPA 16 — Dead Letter Queue

Após exceder o limite de tentativas, encaminhar para:

```text
gestao-comercial.estoque.dlq
```

Criar também Dead Letter Exchange quando necessário.

Arquitetura:

```text
gestao-comercial.estoque
       ↓ erro
retry
       ↓ erro
retry
       ↓ erro
retry
       ↓
DLQ
```

A mensagem na DLQ deve preservar informações que permitam diagnóstico.

Adicionar logs claros quando mensagem for enviada à DLQ.

---

# ETAPA 17 — Endpoint de alertas

Criar endpoints apropriados.

Exemplo:

```text
GET /api/estoque/alertas
```

Permitir consultar alertas de estoque.

Quando fizer sentido:

```text
PUT /api/estoque/alertas/{id}/visualizar
```

ou operação equivalente.

Não misturar esse endpoint com movimentações de estoque se a arquitetura indicar separação melhor.

Manter autorização consistente com o restante da API.

Utilizar DTOs.

Não retornar entidade EF Core diretamente.

---

# ETAPA 18 — Frontend

Adicionar uma forma simples de visualizar alertas.

Pode ser:

```text
Dashboard
```

ou:

```text
Tela de Estoque
```

Exibir informações como:

```text
Produto
Quantidade atual
Estoque mínimo
Data
Status
```

Opcionalmente apresentar contador:

```text
3 produtos com estoque baixo
```

Não fazer grande redesign.

Manter o padrão visual existente.

---

# ETAPA 19 — Health Check do RabbitMQ

Adicionar verificação de saúde para RabbitMQ.

O projeto já possui conceitos de:

```text
/health
/health/ready
```

Integrar RabbitMQ ao readiness quando tecnicamente apropriado.

Avaliar cuidadosamente:

- API pode continuar respondendo se RabbitMQ estiver temporariamente fora?
- readiness deve considerar RabbitMQ?
- Outbox permite tolerar indisponibilidade temporária?

Documentar a decisão.

A arquitetura Outbox deve permitir:

```text
RabbitMQ indisponível
       ↓
Venda continua sendo salva
       ↓
Outbox permanece pendente
       ↓
RabbitMQ volta
       ↓
OutboxProcessor publica
```

Esse comportamento deve ser testado.

---

# ETAPA 20 — Observabilidade

Adicionar logs estruturados com dados relevantes.

Exemplos:

```text
EventoId
VendaId
NumeroVenda
MessageId
CorrelationId
Queue
RoutingKey
Tentativa
```

Não registrar dados sensíveis.

Logs desejados:

```text
Outbox criada
Evento publicado
Evento recebido
Evento ignorado por idempotência
Retry realizado
Mensagem enviada para DLQ
Alerta de estoque criado
```

---

# ETAPA 21 — Testes unitários

Criar testes para regras independentes quando apropriado.

Cobrir ao menos:

### Outbox

Verificar que uma venda concluída cria uma mensagem Outbox.

### Estoque

Quando:

```text
QuantidadeEstoque > EstoqueMinimo
```

não criar alerta.

Quando:

```text
QuantidadeEstoque <= EstoqueMinimo
```

criar alerta.

### Inbox

Uma mensagem já processada não deve criar outro alerta.

---

# ETAPA 22 — Testes de integração com SQL Server

Utilizar a infraestrutura de testes já existente.

Criar testes para verificar:

### Cenário 1

```text
Venda criada
+
estoque alterado
+
financeiro criado
+
Outbox criada
```

na mesma transação.

### Cenário 2

Simular falha durante a venda.

Resultado esperado:

```text
Venda não salva
Estoque não alterado
Financeiro não salvo
Outbox não criada
```

### Cenário 3

Reutilizar `Idempotency-Key`.

Resultado:

```text
não criar segunda venda
não criar segunda Outbox
```

---

# ETAPA 23 — Testes RabbitMQ

Criar testes de integração quando tecnicamente viável utilizando RabbitMQ em Docker.

Verificar:

```text
Outbox
↓
Publisher
↓
RabbitMQ
```

e:

```text
RabbitMQ
↓
Consumer
↓
Inbox
↓
AlertaEstoque
```

Não transformar os testes em dependência obrigatória para desenvolvedores sem Docker caso a suíte atual tenha estratégia de testes opcionais externos.

Seguir o padrão já usado pelo projeto para SQL Server externo.

---

# ETAPA 24 — Testar indisponibilidade do RabbitMQ

Criar ou documentar teste manual/integrado:

1. desligar RabbitMQ;
2. realizar uma venda;
3. confirmar que a venda foi salva normalmente;
4. verificar que a Outbox permanece pendente;
5. iniciar RabbitMQ;
6. confirmar que o `OutboxProcessor` publica posteriormente;
7. confirmar processamento;
8. confirmar `ProcessedAt`;
9. confirmar criação de alerta quando aplicável.

Esse cenário é um dos principais motivos da utilização da Outbox.

---

# ETAPA 25 — Testar mensagem duplicada

Simular publicação duplicada de:

```text
VendaRealizadaEvent
```

Resultado esperado:

```text
primeira mensagem
→ processada

segunda mensagem com mesmo EventoId
→ identificada pela Inbox
→ nenhuma duplicação
→ ACK
```

Garantir que não seja criado segundo `AlertaEstoque`.

---

# ETAPA 26 — Testar DLQ

Forçar erro controlado no consumidor.

Executar até atingir o limite de retry.

Confirmar:

```text
mensagem original
↓
retry
↓
retry
↓
retry
↓
DLQ
```

Validar através do RabbitMQ Management.

---

# ETAPA 27 — Segurança

Não colocar no Git:

```text
RabbitMQ password
SQL password
JWT secret
```

Atualizar:

```text
.env.example
```

com apenas exemplos seguros.

Validar configurações obrigatórias na inicialização quando apropriado.

Não expor credenciais nos logs.

---

# ETAPA 28 — Dependency Injection

Registrar corretamente:

```text
RabbitMqOptions
RabbitMqConnection
IEventPublisher
RabbitMqPublisher
IOutboxRepositorio
IInboxRepositorio
OutboxProcessor
VendaRealizadaConsumer
```

Respeitar lifetimes.

Avaliar cuidadosamente:

```text
Singleton
Scoped
Transient
```

Não colocar dependências scoped diretamente em hosted services singleton.

---

# ETAPA 29 — Migration

Criar migrations necessárias para:

```text
OutboxMessages
InboxMessages
AlertasEstoque
```

Garantir índices adequados.

Avaliar pelo menos índices para:

```text
OutboxMessages.ProcessedAt
InboxMessages.MessageId + Consumer
AlertasEstoque.ProdutoId
```

Criar constraint única para impedir Inbox duplicada.

---

# ETAPA 30 — README

Atualizar o README.

Adicionar RabbitMQ na lista de tecnologias.

Adicionar seção:

```text
Mensageria e arquitetura orientada a eventos
```

Explicar:

- RabbitMQ;
- Transactional Outbox;
- Inbox;
- retry;
- DLQ;
- idempotência;
- consumidores.

Adicionar diagrama semelhante:

```text
                    ┌───────────────┐
                    │      PDV      │
                    └───────┬───────┘
                            │
                            ▼
                    ┌───────────────┐
                    │ VendaService  │
                    └───────┬───────┘
                            │
                 SQL Transaction
                            │
       ┌────────────────────┼─────────────────────┐
       ▼                    ▼                     ▼
     Venda                Estoque              Financeiro
       │
       ▼
     Outbox
       │
       │ COMMIT
       ▼
 OutboxProcessor
       │
       ▼
    RabbitMQ
       │
       │ venda.realizada
       ▼
VendaRealizadaConsumer
       │
       ▼
     Inbox
       │
       ▼
Alerta de estoque
```

Explicar por que RabbitMQ não é chamado diretamente durante a transação.

---

# ETAPA 31 — Documentação arquitetural

Atualizar:

```text
docs/arquitetura.md
```

ou documento equivalente.

Registrar uma Architecture Decision Record, se o projeto utilizar ou puder utilizar ADR.

Sugestão:

```text
ADR - Uso de RabbitMQ com Transactional Outbox
```

Explicar:

## Problema

Existe necessidade de executar efeitos assíncronos após eventos de negócio sem acoplar o processamento à requisição HTTP.

## Decisão

Utilizar:

```text
RabbitMQ
+
Transactional Outbox
+
Inbox
```

## Motivos

- desacoplamento;
- resiliência;
- processamento assíncrono;
- consistência;
- extensibilidade;
- retry;
- tolerância a falhas.

## Alternativas consideradas

- chamada síncrona;
- publicação RabbitMQ direta no serviço;
- Kafka;
- microserviços.

Explique por que não foram escolhidas neste momento.

---

# ETAPA 32 — Não implementar microserviços agora

Não separar cada módulo em microserviço.

O projeto deve continuar como monólito modular.

RabbitMQ será utilizado internamente para demonstrar arquitetura orientada a eventos e preparar o sistema para futuras integrações.

Apenas documentar que futuramente consumidores poderiam ser extraídos para:

```text
Worker Services
```

ou microserviços independentes.

---

# ETAPA 33 — Possível evolução futura

Documentar sem necessariamente implementar agora:

```text
VendaRealizada
       │
       ├── EstoqueConsumer
       ├── EmailConsumer
       ├── AnalyticsConsumer
       ├── AuditoriaConsumer
       └── IntegracaoFiscalConsumer
```

A implementação atual deverá permitir essa expansão sem alterar o `VendaService`.

Esse desacoplamento é um dos critérios arquiteturais mais importantes.

---

# Critérios obrigatórios de aceite

A implementação somente será considerada concluída se todos estes pontos forem verdadeiros:

### Venda

- venda continua funcionando;
- estoque continua sendo atualizado;
- financeiro continua sendo registrado;
- idempotência HTTP continua funcionando.

### Outbox

- toda nova venda válida gera exatamente uma mensagem Outbox;
- Outbox participa da transação da venda;
- rollback da venda também remove/impede a Outbox.

### RabbitMQ

- evento é publicado;
- exchange existe;
- queue existe;
- routing key funciona;
- mensagens são persistentes quando apropriado.

### Consumer

- recebe `VendaRealizadaEvent`;
- processa estoque;
- cria alerta quando necessário.

### Idempotência

- mesma mensagem entregue duas vezes não gera efeitos duplicados.

### Retry

- erros transitórios geram retry.

### DLQ

- mensagens que excedem retries vão para DLQ.

### Resiliência

RabbitMQ indisponível não pode provocar perda da venda.

Fluxo obrigatório:

```text
RabbitMQ OFF

Venda
  ↓
SQL Server OK
  ↓
Outbox pendente

RabbitMQ ON

OutboxProcessor
  ↓
publicação
  ↓
Consumer
```

### Segurança

- nenhuma senha real versionada;
- nenhum secret hardcoded.

### Testes

- testes existentes continuam passando;
- novos testes devem ser adicionados;
- build deve passar.

---

# Validações finais

Ao final execute:

```bash
dotnet restore
```

```bash
dotnet build
```

```bash
dotnet test
```

Executar também os testes do frontend existentes.

Se Docker estiver disponível, testar:

```bash
docker compose up -d
```

Validar SQL Server e RabbitMQ.

Executar uma venda real pelo sistema ou API.

Confirmar no RabbitMQ Management:

```text
Exchange
Queue
Consumer
Message rates
```

Confirmar no banco:

```text
OutboxMessages
InboxMessages
AlertasEstoque
```

---

# Importante durante a implementação

Não faça mudanças massivas de uma só vez.

Execute em pequenas etapas.

Depois de cada etapa:

1. verificar compilação;
2. verificar testes existentes;
3. corrigir erros antes de avançar.

Não remover funcionalidades existentes para facilitar a implementação.

Não alterar contratos HTTP existentes sem necessidade.

Não alterar regras de negócio atuais sem justificativa.

Não substituir arquitetura existente por uma nova arquitetura.

Não introduzir bibliotecas pesadas sem necessidade.

Para RabbitMQ, prefira implementação que demonstre claramente conhecimento dos conceitos de mensageria.

Se optar por alguma biblioteca de abstração adicional, explique antes por que ela é necessária.

---

# Resultado esperado

Ao final, o projeto deverá demonstrar profissionalmente conhecimentos de:

```text
C#
.NET
ASP.NET Core
EF Core
SQL Server
RabbitMQ
Docker
Clean Architecture
SOLID
REST
Transactional Outbox
Inbox Pattern
Producer / Consumer
Event Driven Architecture
Idempotência
Retry
Dead Letter Queue
BackgroundService
Concorrência
Transações
Testes automatizados
CI/CD
```

A mensageria deve resolver um problema real do sistema e não existir apenas como demonstração artificial de tecnologia.

---

# Entrega final

Depois de concluir toda a implementação, apresente um relatório contendo:

## 1. Arquivos criados

Liste cada arquivo criado e sua responsabilidade.

## 2. Arquivos alterados

Liste cada arquivo alterado e explique brevemente a mudança.

## 3. Fluxo completo

Explique:

```text
Venda
→ Outbox
→ RabbitMQ
→ Consumer
→ Inbox
→ AlertaEstoque
```

## 4. Decisões arquiteturais

Explique principalmente:

- por que RabbitMQ;
- por que Outbox;
- por que Inbox;
- por que não publicar diretamente no `VendaService`;
- por que não utilizar Kafka;
- por que não transformar o sistema em microserviços agora.

## 5. Resiliência

Explique o comportamento quando RabbitMQ estiver indisponível.

## 6. Idempotência

Explique como duplicação de mensagens é tratada.

## 7. Retry e DLQ

Explique o fluxo implementado.

## 8. Testes

Informe quais testes foram criados e os resultados obtidos.

## 9. Como executar

Forneça comandos completos para:

```text
Docker
SQL Server
RabbitMQ
migrations
backend
frontend
testes
```

## 10. Como demonstrar em entrevista

Descreva um pequeno roteiro prático mostrando:

1. RabbitMQ Management;
2. realização de uma venda;
3. criação da Outbox;
4. publicação do evento;
5. consumo;
6. criação do alerta;
7. Inbox;
8. exemplo de retry;
9. exemplo de DLQ.

Não considere a tarefa encerrada apenas porque o código compila. Valide o fluxo ponta a ponta.
