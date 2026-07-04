# 027 — Hardening de auth/transporte (plaintext + password/delete + TLS global) · Kickoff

**Launcher:** Launcher4.0beta · **Data:** 2026-07-04 · **Origem:** deferido dos itens 020 (DP-020.B) e 018 (review) · **Severidade:** 🟡 (segurança) · **Deps:** 020, 018

> Brief de kickoff — insumo para `/create-spec`. Não é a spec. **Tratar plaintext + password/delete JUNTOS** (o gate do delete depende de remover o plaintext).

## Achados
- **Plaintext (020 DP-020.B):** `/redline/profile/get` ecoa a senha em texto puro; a auth do launcher é client-side comparando com esse eco (`LoginViewModel:58-66`). Substituir por endpoint `/redline/password/verify` server-side + repovoar `SelectedAccount.password` da senha digitada. Blast radius: login, reset-HWID, create-password, change, remove, wipe (~6 fluxos de auth).
- **`password/delete` não-autenticado (020 review):** o endpoint novo do 020 zera a chave de qualquer conta viva **sem gate**. Hoje sem exposição nova (o eco plaintext já concede takeover maior), mas ao remover o plaintext acima é **obrigatório** gatear o `password/delete` (sessão/HWID válido, ou só do host).
- **TLS global bypass (018 review):** `MiniCommon/Request.cs:33` seta `ServicePointManager.ServerCertificateValidationCallback => true` — afeta as chamadas `WebRequest` da API do server (login/profile) → MITM nessas rotas. Pinar/validar (o path de auto-update já foi fechado no 018; o `WireGuardHelper` com outro `=> true` já foi deletado no 025).
- **Gist como fonte de `Server.Url` (018 D-018.3):** Gist público mutável por terceiros afeta login/profile. Assinar/pinar a fonte.

## Critérios de aceite (seed)
- Senha nunca ecoada em plaintext por endpoint algum; verificação server-side.
- `password/delete` recusa requisição não-autenticada.
- Sem `=> true` de validação de cert em nenhum caminho de rede vivo.

## Gate humano
Regressão dos ~6 fluxos de auth in-game (login certo/errado, reset senha, create, change, remove, wipe) após remover o eco plaintext.
