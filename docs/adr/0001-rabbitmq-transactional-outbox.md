# ADR 0001 — RabbitMQ com Transactional Outbox

## Status

Aceita.

## Decisão

O monólito modular usa RabbitMQ, Transactional Outbox e Inbox. A Outbox é gravada na transação SQL
da venda; um serviço hospedado publica após o commit. O consumidor grava Inbox e alertas numa
transação local e só então envia ACK. Retry usa fila com TTL; após três falhas a mensagem vai à DLQ.

RabbitMQ indisponível não impede vendas e entregas repetidas não repetem efeitos graças à chave
única `(MessageId, Consumer)`. O exchange tópico permite adicionar email, analytics, auditoria e
integração fiscal sem alterar `VendaService`.

## Alternativas

- Chamada síncrona acoplaria a disponibilidade do efeito à venda.
- Publicação direta criaria uma janela de inconsistência entre SQL e broker.
- Kafka adicionaria custo sem necessidade atual de retenção/replay em grande escala.
- Microserviços aumentariam a complexidade operacional prematuramente.

Consumidores podem futuramente ser extraídos para Worker Services ou microserviços independentes.
