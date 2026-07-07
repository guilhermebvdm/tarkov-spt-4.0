# 018 — Segurança do auto-update (cert pinning + verificação de assinatura) · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Origem:** [AUDIT-2026-07-04](../../AUDIT-2026-07-04-code-product-ds.md) §B1 · **Severidade:** 🔴 Blocker (segurança) · **Deps:** —

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Objetivo
Fechar o vetor de **RCE** do auto-update. Hoje `LauncherUpdateHelper` (`:22`, `:76`) desliga a validação de TLS (`ServerCertificateCustomValidationCallback = (...) => true`) e baixa/executa `SPT.Launcher_Update.exe` via `.bat` **sem verificar assinatura/hash**. A `serverUrl` vem de um Gist público (`ConnectServerViewModel.cs:54`).

## Cenário de ameaça
MITM na rede (ou Gist/DNS comprometido) responde `/redline/launcher/version` com versão maior e serve um exe arbitrário em `/redline/launcher/download` → o launcher baixa e roda → **execução remota de código** na máquina do jogador. Sem validação de cert, o atacante nem precisa de certificado válido.

## Critérios de aceite (seed)
- Validação de TLS **ligada** (remover o callback que aceita tudo) ou cert/public-key **pinning** explícito do servidor.
- Exe baixado **verificado** por hash assinado (ou assinatura Authenticode) antes de o `.bat` executar; falha → aborta e avisa, sem deixar o launcher quebrado.
- Fonte de versão/URL confiável (não um Gist mutável por terceiros) ou assinada.

## Notas
Coordenar com o server (`TarkovRedLine.Server` → `LauncherUpdaterController`) para publicar o hash/assinatura. **Bloqueia distribuição em produção.**
