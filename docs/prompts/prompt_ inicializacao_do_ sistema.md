Você é um **Engenheiro de Software Sênior** responsável por projetar e implementar uma aplicação de gestão comercial completa, escalável, segura, testável e de fácil manutenção.

Antes de iniciar qualquer implementação, analise cuidadosamente os requisitos, identifique possíveis inconsistências e organize a solução de forma clara.

A aplicação deve seguir os princípios de:

* Clean Code
* SOLID
* Clean Architecture
* Separação de responsabilidades
* Baixo acoplamento
* Alta coesão
* Código legível e testável

Evite overengineering e não introduza tecnologias que não sejam necessárias para o problema.

## Objetivo

Criar uma aplicação de **Gestão Comercial** contendo:

* Gestão de clientes
* Gestão de produtos
* Controle de estoque
* Frente de caixa / PDV
* Gestão de vendas
* Histórico de compras
* Controle financeiro básico
* Dashboard financeiro
* Consulta de endereço por CEP
* Autenticação e controle de acesso

## Stack obrigatória

## Backend

Utilize:

* .NET 10 LTS
* C# 14
* ASP.NET Core 10 Web API
* Entity Framework Core 10
* SQL Server
* Swagger / OpenAPI
* JWT para autenticação
* xUnit para testes

Todas as operações de entrada e saída devem utilizar programação assíncrona quando aplicável.

Utilize `CancellationToken` em operações assíncronas que permitam cancelamento.

## Arquitetura

Utilize **Clean Architecture**, aplicando os princípios SOLID.

Organize a solução nas seguintes camadas:

* Domínio
* Aplicação
* Infraestrutura
* API
* Testes Unitários
* Testes de Integração

## Domínio

Responsável por:

* Entidades
* Regras de negócio
* Objetos de valor
* Enumerações
* Exceções de domínio

Não deve depender de:

* Entity Framework Core
* ASP.NET Core
* SQL Server
* Bibliotecas de infraestrutura

## Aplicação

Responsável por:

* Casos de uso
* DTOs
* Interfaces de serviços
* Validações
* Serviços de aplicação
* Mapeamentos

## Infraestrutura

Responsável por:

* Entity Framework Core
* DbContext
* Repositórios
* SQL Server
* Persistência
* Integrações externas
* Integração com ViaCEP

## API

Responsável por:

* Controllers
* Autenticação
* Autorização
* Middlewares
* Dependency Injection
* Swagger
* Configuração da aplicação

## Regras arquiteturais

Controllers devem ser enxutos.

Controllers devem apenas:

* Receber requisições
* Validar parâmetros básicos
* Delegar a execução para a camada de aplicação
* Retornar respostas HTTP

Não colocar regras de negócio nos Controllers.

Não acessar o DbContext diretamente nos Controllers.

Não retornar entidades do Entity Framework diretamente pela API.

Utilizar DTOs para entrada e saída de dados.

Implementar tratamento global de exceções.

Padronizar respostas de erro utilizando ProblemDetails.

## Banco de dados

Utilize SQL Server.

Utilize Entity Framework Core 10.

Utilize migrations para criação e evolução da estrutura do banco.

Valores monetários devem utilizar `decimal`.

Não utilizar `float` ou `double` para valores financeiros.

Datas devem ser armazenadas em UTC.

O frontend será responsável pela conversão das datas para o horário local.

## Convenção de nomenclatura das entidades

As entidades e propriedades relacionadas ao domínio devem ser escritas em **português**.

Exemplos:

* Cliente
* Produto
* Venda
* ItemVenda
* MovimentacaoEstoque
* MovimentacaoFinanceira
* Usuario
* Nome
* Descricao
* Quantidade
* PrecoVenda
* Situacao
* FormaPagamento

Como exceção, campos técnicos de auditoria relacionados às datas de criação e atualização devem utilizar a convenção em inglês:

* `CreatedAt` para data de criação
* `UpdatedAt` para data da última atualização

Datas pertencentes diretamente ao domínio continuam em português.

Exemplos:

* `DataNascimento`
* `DataVenda`
* `DataMovimentacao`

Campos de usuário responsável pela auditoria permanecem em português:

* `CriadoPor`
* `AtualizadoPor`

## Entidade Cliente

Criar a entidade `Cliente` contendo:

* Id
* Nome
* CPF
* Email
* Telefone
* DataNascimento
* CEP
* Logradouro
* Numero
* Complemento
* Bairro
* Cidade
* UF
* Ativo
* CreatedAt
* UpdatedAt
* CriadoPor
* AtualizadoPor

## Regras de Cliente

* CPF obrigatório
* CPF válido
* CPF único
* Email deve possuir formato válido
* Email deve ser único quando informado
* CEP deve possuir 8 dígitos
* Não permitir clientes duplicados pelo CPF
* Cliente que possuir vendas não deve ser excluído fisicamente
* Utilizar exclusão lógica quando houver histórico

Permitir busca por:

* Nome
* CPF
* Telefone

Todas as listagens devem possuir paginação no backend.

## Consulta de CEP

Utilizar a API pública ViaCEP.

Endpoint:

`GET https://viacep.com.br/ws/{cepLimpo}/json/`

O CEP deve ser enviado somente com os 8 números.

A integração deve ficar na camada de Infraestrutura.

Utilizar `IHttpClientFactory`.

Não criar instâncias de HttpClient diretamente nos serviços.

Caso o ViaCEP esteja indisponível:

* Tratar corretamente a falha
* Não interromper o funcionamento da aplicação
* Permitir preenchimento manual do endereço

Caso o CEP não exista, retornar mensagem adequada ao frontend.

## Histórico de compras do cliente

Na tela de detalhes do cliente, exibir o histórico de compras contendo:

* Número da venda
* Data
* Produtos
* Quantidades
* Valor total
* Forma de pagamento
* Situação da venda

## Entidade Produto

Criar a entidade `Produto` contendo:

* Id
* Codigo
* Nome
* Descricao
* PrecoCusto
* PrecoVenda
* QuantidadeEstoque
* EstoqueMinimo
* Ativo
* CreatedAt
* UpdatedAt
* CriadoPor
* AtualizadoPor

## Regras de Produto

* Código obrigatório
* Código único
* Nome obrigatório
* Preço de venda não pode ser negativo
* Preço de custo não pode ser negativo
* Estoque não pode ficar negativo
* Produto utilizado em vendas não deve ser excluído fisicamente
* Utilizar exclusão lógica quando necessário

Permitir busca por:

* Código
* Nome

Todas as listagens devem possuir paginação no backend.

## Controle de Estoque

Toda alteração de estoque deve gerar uma movimentação.

Não alterar a quantidade disponível sem registrar sua origem.

## Entidade MovimentacaoEstoque

Criar a entidade `MovimentacaoEstoque` contendo:

* Id
* ProdutoId
* TipoMovimentacao
* Quantidade
* QuantidadeAnterior
* QuantidadePosterior
* ReferenciaId
* Observacao
* CreatedAt
* CriadoPor

Tipos de movimentação:

* Entrada
* Venda
* Ajuste
* Devolucao

Toda movimentação deve registrar:

* Estoque anterior
* Quantidade movimentada
* Estoque posterior
* Data
* Usuário responsável
* Origem da movimentação quando aplicável

Nunca permitir estoque negativo.

## Entrada de Estoque

Permitir registro manual de entrada de produtos.

Informar:

* Produto
* Quantidade
* Observação

Ao registrar uma entrada:

* Validar produto
* Validar quantidade maior que zero
* Atualizar estoque
* Registrar movimentação do tipo Entrada

A atualização do estoque e a criação da movimentação devem ocorrer na mesma transação de banco de dados.

## Estoque mínimo

Cada produto deve possuir uma quantidade mínima de estoque.

Considerar estoque baixo quando:

`QuantidadeEstoque <= EstoqueMinimo`

Criar funcionalidade para listar produtos com estoque baixo.

Produtos com estoque baixo também devem aparecer no dashboard.

## Frente de Caixa / PDV

Criar uma tela específica para realização de vendas.

O operador deve conseguir:

* Pesquisar produtos
* Adicionar produtos ao carrinho
* Alterar quantidade
* Remover itens
* Selecionar cliente opcionalmente
* Aplicar desconto quando permitido
* Selecionar forma de pagamento
* Visualizar subtotal
* Visualizar desconto
* Visualizar total
* Finalizar venda

## Entidade Venda

Criar a entidade `Venda` contendo:

* Id
* Numero
* ClienteId opcional
* DataVenda
* Subtotal
* Desconto
* Total
* FormaPagamento
* Situacao
* CreatedAt
* CriadoPor

## Entidade ItemVenda

Criar a entidade `ItemVenda` contendo:

* Id
* VendaId
* ProdutoId
* Quantidade
* PrecoUnitario
* Desconto
* Total

O preço utilizado no momento da venda deve ser armazenado no ItemVenda.

Nunca recalcular vendas antigas utilizando o preço atual do produto.

## Formas de pagamento

Suportar inicialmente:

* Dinheiro
* Pix
* CartaoDebito
* CartaoCredito

Não implementar pagamento dividido nesta primeira versão.

## Registro de Venda

Ao finalizar uma venda:

* Validar os produtos
* Validar as quantidades
* Validar estoque disponível
* Calcular subtotal
* Calcular descontos
* Calcular total
* Criar a venda
* Criar os itens
* Baixar o estoque
* Registrar movimentações de estoque
* Registrar a entrada financeira

Todas essas operações devem ocorrer dentro de uma única transação de banco.

Se qualquer etapa falhar, toda a operação deve ser revertida.

Nenhum registro parcial deve permanecer no banco.

## Concorrência de Estoque

Considere que mais de um operador pode tentar vender o mesmo produto simultaneamente.

A aplicação deve impedir que vendas concorrentes façam o estoque ficar negativo.

A validação de estoque deve ocorrer obrigatoriamente no backend.

Utilizar estratégia adequada de concorrência utilizando Entity Framework Core e SQL Server.

## Venda sem estoque

Não permitir finalizar uma venda quando a quantidade solicitada for maior que o estoque disponível.

Retornar erro de negócio claro para o frontend.

## Cancelamento de Venda

Venda nunca deve ser excluída fisicamente.

Situações possíveis:

* Concluida
* Cancelada

Ao cancelar uma venda:

* Alterar a situação para Cancelada
* Devolver os produtos ao estoque
* Registrar movimentações do tipo Devolucao
* Registrar o estorno financeiro
* Preservar todo o histórico da venda

O cancelamento deve ocorrer dentro de uma única transação.

Não permitir cancelar novamente uma venda já cancelada.

## Recibo

Após finalizar uma venda, permitir emissão de recibo simples contendo:

* Número da venda
* Data
* Cliente, quando informado
* Produtos
* Quantidades
* Preço unitário
* Subtotal
* Desconto
* Total
* Forma de pagamento

Não é necessário implementar NFC-e ou integração fiscal nesta versão.

## Financeiro

Criar controle financeiro básico.

## Entidade MovimentacaoFinanceira

Criar a entidade `MovimentacaoFinanceira` contendo:

* Id
* TipoMovimentacao
* Descricao
* Valor
* DataMovimentacao
* VendaId opcional
* CreatedAt
* CriadoPor

Tipos:

* Entrada
* Saida
* Estorno

Uma venda concluída deve gerar automaticamente uma entrada financeira.

Uma venda cancelada deve gerar um estorno.

Permitir também registro manual de despesas.

Exemplos:

* Energia
* Aluguel
* Fornecedor
* Manutenção
* Material de escritório
* Outros

## Dashboard

Criar dashboard financeiro contendo:

* Faturamento bruto do dia
* Faturamento bruto do mês
* Despesas do dia
* Despesas do mês
* Saldo do dia
* Saldo do mês
* Quantidade de vendas do dia
* Quantidade de vendas do mês
* Ticket médio do dia
* Ticket médio do mês
* Produtos com estoque baixo

Criar endpoints específicos para esses indicadores.

Os cálculos devem ocorrer preferencialmente no backend e no banco de dados.

Não enviar grandes quantidades de registros ao frontend apenas para realizar cálculos de dashboard.

## Autenticação e autorização

Implementar autenticação JWT.

Criar os perfis:

* Administrador
* Operador

## Administrador

Pode acessar:

* Dashboard
* Clientes
* Produtos
* Estoque
* Vendas
* Financeiro
* Usuários

## Operador

Pode acessar:

* Clientes
* Consulta de produtos
* PDV
* Histórico de vendas autorizado

Todas as permissões devem ser validadas também no backend.

Não confiar apenas na ocultação de funcionalidades no frontend.

Senhas devem ser armazenadas utilizando hash seguro.

Nunca armazenar senhas em texto puro.

## Entidade Usuario

Criar a entidade `Usuario` contendo:

* Id
* Nome
* Email
* SenhaHash
* Perfil
* Ativo
* CreatedAt
* UpdatedAt

## API

Criar APIs RESTful.

Utilizar corretamente:

* GET
* POST
* PUT
* PATCH quando apropriado
* DELETE quando apropriado

Utilizar status HTTP adequados.

Configurar Swagger/OpenAPI para documentação e testes dos endpoints.

## Frontend

Utilizar:

* React
* TypeScript
* Vite
* Tailwind CSS
* shadcn/ui
* React Hook Form
* Zod
* Axios

Configurar TypeScript em modo estrito.

## Regra de TypeScript

É proibido utilizar:

* `any`
* `@ts-ignore`
* `@ts-nocheck`

Não utilizar casts inseguros apenas para esconder problemas de tipagem.

Todos os contratos da API devem possuir tipos explícitos.

## Organização do Frontend

Organizar o frontend por responsabilidade e funcionalidade.

Separar módulos para:

* Autenticação
* Clientes
* Produtos
* Estoque
* Vendas
* Financeiro
* Dashboard

Também separar:

* Componentes compartilhados
* Serviços
* Hooks
* Schemas
* Tipos
* Utilitários
* Rotas

Evitar componentes excessivamente grandes.

## Axios

Criar uma instância centralizada do Axios.

Centralizar:

* URL base
* Headers
* Token de autenticação
* Interceptadores quando necessários
* Tratamento padronizado de erros

Todas as chamadas devem possuir contratos fortemente tipados.

## Formulários e validação

Utilizar React Hook Form com Zod.

Utilizar Zod para validação dos formulários.

Quando apropriado, validar também dados críticos retornados pela API.

Validação no frontend serve para experiência do usuário.

Todas as regras de segurança e negócio devem ser novamente validadas no backend.

## Gerenciamento de estado

Não utilizar Redux sem necessidade real.

Priorizar:

* Estado local
* Hooks
* Context quando necessário

Evitar adicionar complexidade desnecessária.

## Telas obrigatórias

Criar:

* Login
* Dashboard
* Lista de clientes
* Cadastro de cliente
* Detalhes de cliente
* Edição de cliente
* Lista de produtos
* Cadastro de produto
* Edição de produto
* Controle de estoque
* Histórico de movimentações de estoque
* Frente de caixa / PDV
* Lista de vendas
* Detalhes da venda
* Financeiro
* Gestão de usuários

A gestão de usuários deve ser acessível somente pelo Administrador.

## Experiência do usuário

Implementar:

* Estados de carregamento
* Mensagens de erro
* Mensagens de sucesso
* Confirmação de operações críticas
* Estados sem dados
* Tratamento adequado de erros da API
* Responsividade básica

Utilizar componentes do shadcn/ui sempre que apropriado.

## Integração Frontend e Backend

Antes de integrar uma tela React com um endpoint:

* Implementar o endpoint
* Executar o backend
* Fazer uma requisição real utilizando CURL
* Validar o status HTTP
* Analisar o JSON retornado
* Confirmar o contrato da resposta
* Somente depois implementar a chamada Axios

Não presumir o formato retornado pela API.

## Testes Backend

Utilizar xUnit.

Criar testes unitários para regras de negócio.

Criar testes de integração para endpoints críticos.

Cobrir obrigatoriamente:

## Clientes

* CPF inválido
* CPF duplicado
* Criação válida
* Cliente inexistente

## Produtos

* Código duplicado
* Preço negativo
* Estoque inválido

## Estoque

* Entrada
* Ajuste
* Tentativa de estoque negativo

## Vendas

* Venda válida
* Venda sem estoque
* Cálculo de total
* Persistência do preço histórico
* Baixa de estoque
* Movimentação de estoque
* Entrada financeira
* Rollback em caso de falha
* Cancelamento
* Devolução ao estoque
* Estorno financeiro
* Cancelamento duplicado

## Financeiro

* Receita
* Despesa
* Saldo
* Faturamento diário
* Faturamento mensal

## Dados iniciais de desenvolvimento

Criar dados fictícios para ambiente de desenvolvimento contendo:

* Usuário Administrador
* Usuário Operador
* Clientes
* Produtos
* Estoque
* Vendas
* Despesas

Não utilizar dados pessoais reais.

## Segurança

Nunca armazenar diretamente no código:

* Senhas
* Tokens
* Secrets
* Credenciais
* Connection strings sensíveis

Utilizar configurações apropriadas para cada ambiente.

No frontend, nunca incluir secrets privados no bundle da aplicação.

## Dependências

Se uma dependência necessária não estiver instalada:

* Instalar
* Configurar
* Registrar corretamente
* Validar o funcionamento
* Executar novamente o build

Não adicionar bibliotecas desnecessárias.

Verificar primeiro se o próprio framework já oferece uma solução adequada.

## Restrições

É proibido:

* Utilizar `any`
* Ignorar erros de TypeScript
* Colocar regras de negócio em Controllers
* Acessar DbContext diretamente em Controllers
* Retornar entidades EF diretamente pela API
* Utilizar float ou double para valores monetários
* Instanciar HttpClient diretamente
* Alterar estoque sem gerar movimentação
* Permitir estoque negativo
* Excluir vendas fisicamente
* Armazenar senhas em texto puro
* Utilizar secrets hardcoded
* Deixar código morto
* Manter TODOs relacionados a requisitos obrigatórios
* Criar funcionalidades falsas apenas para fazer o projeto compilar
* Ignorar testes quebrados
* Utilizar bibliotecas desnecessárias
* Introduzir complexidade arquitetural sem necessidade

## Escopo arquitetural

Implementar inicialmente como um **monólito modular**.

Não utilizar sem necessidade concreta:

* Microservices
* Kafka
* RabbitMQ
* Redis
* Kubernetes
* Event Sourcing
* CQRS complexo

## Ordem de desenvolvimento

Seguir obrigatoriamente esta sequência:

## Fase 1 — Arquitetura

Criar:

* Solução
* Projetos
* Dependências
* Estrutura de diretórios
* Dependency Injection
* Configurações básicas

Validar o build antes de continuar.

## Fase 2 — Domínio e Banco de Dados

Criar:

* Entidades
* Relacionamentos
* DbContext
* Configurações do Entity Framework
* Migrations
* Banco SQL Server

Validar antes de continuar.

## Fase 3 — Autenticação

Implementar:

* Usuários
* Perfis
* Login
* JWT
* Autorização

Testar endpoints com CURL.

## Fase 4 — Clientes

Implementar:

* CRUD
* Paginação
* Busca
* Validações
* ViaCEP
* Histórico de compras
* Testes

Testar endpoints com CURL.

## Fase 5 — Produtos e Estoque

Implementar:

* CRUD
* Entrada de estoque
* Ajuste
* Movimentações
* Estoque mínimo
* Testes

Testar endpoints com CURL.

## Fase 6 — Vendas e PDV

Implementar:

* Vendas
* Itens
* Controle de estoque
* Concorrência
* Transações
* Financeiro
* Cancelamento
* Recibo
* Testes

Testar endpoints com CURL.

## Fase 7 — Dashboard

Implementar os endpoints necessários para:

* Faturamento
* Despesas
* Saldo
* Ticket médio
* Quantidade de vendas
* Estoque baixo

Testar endpoints com CURL.

## Fase 8 — Frontend

Implementar:

* Autenticação
* Layout
* Rotas
* Clientes
* Produtos
* Estoque
* PDV
* Vendas
* Financeiro
* Dashboard

Integrar somente endpoints previamente testados.

## Fase 9 — Validação Final

Executar todos os testes, builds e validações.

Corrigir qualquer falha encontrada antes de considerar a tarefa concluída.

## Critérios de conclusão

A tarefa somente poderá ser considerada concluída quando:

## Backend

* Dependências restauradas com sucesso
* Build concluído sem erros
* Testes executados com sucesso
* Migrations válidas
* Endpoints críticos testados

## Frontend

* Dependências instaladas
* TypeScript sem erros
* Lint sem erros relevantes
* Build concluído com sucesso
* Se você precisar usar uma dependência/biblioteca não instaladao, **SEMPRE** a instale e a configure.

## Regra absoluta

Não declarar a aplicação concluída caso exista:

* Erro de build
* Erro de compilação
* Erro de TypeScript
* Teste falhando
* Migration inválida
* Dependência ausente
* Import quebrado
* Uso de `any`
* Funcionalidade obrigatória não implementada
* TODO relacionado a requisito obrigatório
* Implementação simulada apresentada como funcional

Se um problema for encontrado:

* Identificar
* Corrigir
* Executar novamente os testes
* Executar novamente o build
* Continuar apenas após a validação

## README

Criar README contendo:

* Objetivo da aplicação
* Arquitetura utilizada
* Tecnologias
* Pré-requisitos
* Configuração do SQL Server
* Configuração da aplicação
* Migrations
* Como executar o backend
* Como executar o frontend
* Usuários de desenvolvimento
* Endpoints principais
* Como executar os testes
* Como executar os builds
* Principais decisões arquiteturais

## Resultado esperado

Ao final deverá existir uma aplicação funcional contendo:

* Autenticação
* Controle de acesso
* Gestão de clientes
* Consulta de CEP
* Histórico de compras
* Gestão de produtos
* Controle de estoque
* Movimentações de estoque
* Controle de estoque mínimo
* PDV
* Gestão de vendas
* Cancelamento de vendas
* Recibo
* Controle financeiro
* Dashboard
* Frontend React funcional
* Backend ASP.NET Core funcional
* SQL Server
* Testes automatizados
* Swagger
* README

Priorize sempre:

* Correção
* Consistência dos dados
* Segurança
* Simplicidade
* Legibilidade
* Manutenibilidade
* Testabilidade

Não introduza complexidade que não resolva um problema real.
