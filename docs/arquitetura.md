# Arquitetura e decisões

O sistema é um monólito modular baseado em Clean Architecture.

```text
Frontend React -> API ASP.NET Core -> Aplicação -> Domínio
                              |          |
                              v          v
                       Infraestrutura -> SQL Server / ViaCEP
```

- **Domínio:** entidades, invariantes e tipos do negócio; não depende das demais camadas.
- **Aplicação:** casos de uso, DTOs e portas de persistência/serviços.
- **Infraestrutura:** EF Core, SQL Server, repositórios, segurança, cache de sessão e ViaCEP.
- **API:** autenticação, autorização, composição de dependências, HTTP e observabilidade básica.
- **Frontend:** módulos de interface que consomem somente contratos HTTP.

Vendas, itens, estoque, financeiro e idempotência são gravados na mesma transação. O número da venda vem de uma sequence do SQL Server. Produtos usam `rowversion` para concorrência otimista. Datas são persistidas em UTC e os limites de relatórios são calculados no fuso configurado para o negócio.

## Regra de dependências

```text
Dominio <- Aplicacao <- Infraestrutura <- API
                ^                       /
                +----------------------+
```

O Domínio não possui dependências de projeto. A Aplicação depende apenas do Domínio. A Infraestrutura implementa as portas da Aplicação. A API é a raiz de composição e referencia Aplicação e Infraestrutura. O frontend se comunica exclusivamente por HTTP.

## Módulos funcionais

- Clientes e histórico de compras.
- Produtos e movimentações de estoque.
- PDV, venda, recibo, cancelamento e estorno.
- Movimentações financeiras e dashboard.
- Usuários, perfis e sessão autenticada.
- Consulta externa de CEP.

## Consistência e concorrência

O fluxo de venda abre uma transação com isolamento `Serializable`. A persistência da venda, itens, estoque, movimento financeiro e chave idempotente é atômica. A sequence garante unicidade do número mesmo sob concorrência; não é exigido que seus valores sejam consecutivos, pois rollbacks podem deixar lacunas.

Produtos usam concorrência otimista por `rowversion`. Violações únicas conhecidas são convertidas em conflito, enquanto erros inesperados de banco não são ocultados como regras de negócio.

## Datas e finanças

Datas persistidas são UTC. Um relógio de negócio converte os limites do calendário conforme `Negocio:FusoHorario`, cujo padrão é `America/Sao_Paulo`. Receitas, despesas e estornos têm tipos distintos; indicadores usam valores líquidos e preservam o histórico original.

## Autenticação

O backend emite JWT e o transporta em cookie HttpOnly. A cada requisição autenticada, emissor, audiência, assinatura, expiração, usuário, perfil e versão do token são validados. A consulta de sessão possui cache local curto e invalidação explícita.

## Evolução planejada

Para múltiplas instâncias, substituir o cache em memória por cache distribuído. Integrações assíncronas futuras devem usar outbox transacional. A separação em microsserviços deve ocorrer apenas quando houver justificativa de escala, domínio ou autonomia de equipes.
