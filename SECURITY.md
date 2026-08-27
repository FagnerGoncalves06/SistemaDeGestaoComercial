# Segurança

Não abra issue pública contendo credenciais, dados pessoais ou detalhes exploráveis de uma vulnerabilidade.

Reporte de forma privada ao mantenedor do repositório, informando versão, impacto, passos mínimos para reprodução e uma sugestão de mitigação. Secrets encontrados no histórico devem ser revogados antes de qualquer correção no código.

Versões suportadas recebem correções na branch `main`. Dependências devem ser auditadas no CI e atualizadas de forma compatível.

## Configuração segura

- Nunca versione `.env`, chave JWT, connection string com senha ou senha de seed.
- Use uma chave JWT aleatória com no mínimo 32 bytes e um secret manager em produção.
- Publique a aplicação somente por HTTPS e configure proxies confiáveis explicitamente.
- Mantenha `Seed:Enabled` desativado fora do desenvolvimento.
- Restrinja o acesso ao Swagger em ambientes públicos.
- Faça backup do banco e teste periodicamente a restauração.

## Modelo atual

Senhas usam PBKDF2-SHA512 com salt aleatório. O JWT é armazenado em cookie HttpOnly, validado por versão de sessão e revogado após alterações sensíveis. CORS, verificação de origem, rate limiting e headers básicos reduzem a superfície de ataque, mas não substituem WAF, monitoramento, MFA ou revisão periódica em uma implantação real.
