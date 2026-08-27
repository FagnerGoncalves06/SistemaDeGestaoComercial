# Contribuindo

1. Crie uma branch a partir de `main`.
2. Mantenha as dependências entre as camadas descritas em `docs/arquitetura.md`.
3. Inclua testes para regras ou fluxos alterados.
4. Execute `dotnet test SistemaDeGestaoComercial.sln` e, em `frontend`, `npm run lint`, `npm run test` e `npm run build`.
5. Não versione connection strings, senhas, tokens ou arquivos `.env`.
6. Abra um pull request descrevendo motivação, impacto, evidências de teste e eventual migration.

## Padrões de código

- Use nomes descritivos em português e não reduza parâmetros a letras isoladas.
- Preserve `Nullable` e TypeScript estrito; não suprima alertas sem justificativa.
- Mantenha regras de negócio no Domínio/Aplicação e detalhes externos na Infraestrutura.
- Para alterações no modelo, gere uma nova migration; não reescreva migrations já publicadas.
- Não altere contratos HTTP sem atualizar frontend, testes e `docs/api.md`.

## Formatação

```powershell
dotnet csharpier check .
cd frontend
npm.cmd run format:check
```

Os hooks locais são opcionais; o CI é a fonte final de validação.
