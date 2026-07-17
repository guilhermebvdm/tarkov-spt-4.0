# Handoff — TRL-ItemsManagement: fix de cache stale + redesign do audit log com undo

> **Data:** 2026-07-15<br>
> **De:** sessão longa (bug de produção → cache-patch → redesign do audit log → code-review final)<br>
> **Para:** próxima sessão do mod `TRL-ItemsManagement`<br>

## ⚡ O estado em uma frase

Tudo **implementado e validado localmente** (`D:\SPT`, via Chrome DevTools MCP) — mas **nada disto está
commitado**. `git status` mostra 19 arquivos modificados/novos, +1028/-78 linhas, 100% uncommitted.

## ⚠️ A única decisão que importa antes de qualquer coisa nova

**Este trabalho precisa ser commitado antes de continuar.** A árvore de trabalho está sentando em cima de:
cache-patch fix (flea-price + ban), `ItemCatalogPatcher.cs`/`AuditLogService.cs`/`AuditLogController.cs`
(novos), redesign completo do audit log (feed + undo) em `wwwroot/index.html`/`components.css`, e o fix do
`package-release.sh` (exclusão de `logs/` do bundle). **Nenhum commit existe** desde
`ebd5c9cb chore: bump to v1.0.1` — inclusive a versão 1.0.2 (já rodando em produção, 100.106.152.7) nunca
foi commitada. Antes de somar mais mudanças em cima disso, considerar:

1. Decidir o chunking dos commits (provavelmente: 1️⃣ cache-patch fix + `ItemCatalogPatcher`, 2️⃣ audit log
   redesign + undo + `AuditLogController`, 3️⃣ fix do `package-release.sh`/`NodeScriptRunner` — são
   logicamente separáveis).
2. **Bump de versão** antes do próximo deploy — o código novo (redesign + undo) ainda está em `1.0.2`, a
   mesma versão já em produção sem esse redesign.
3. Só depois: commitar, e então decidir deploy em produção (não pedido ainda pelo usuário).

## Fonte de verdade para contexto (ler nesta ordem)

1. [`mods/TRL-ItemsManagement/memory/sessions.md`](../mods/TRL-ItemsManagement/memory/sessions.md) —
   **primeira entrada do mod** (Sessão 1, criada nesta sessão): snapshot no topo + decisões-chave, lições,
   cronologia completa e as 2 pendências (P-1.1 deploy pendente, P-1.2 flea-cap cache gap).
2. `C:\Users\guime\.claude\plans\e-como-podemos-juntar-lively-rocket.md` — plano original que motivou o
   audit log (fora do repo, pasta pessoal de plans do usuário). Cobre o desenho completo:
   `AuditLogService`, hooks nos 7 controllers, script de backfill, rollout local→produção.
3. [`mods/TRL-ItemsManagement/docs/validacao-endpoints-api.md`](../mods/TRL-ItemsManagement/docs/validacao-endpoints-api.md)
   — validação dos endpoints (de uma rodada anterior, antes do audit log).

## Estado do repositório

- **Branch: `main`**, 35 commits à frente do `origin/main` (nada pushado ainda nesta sessão).
- **Nada commitado desta feature.** `git status -sb` mostra os 19 arquivos do diff (ver lista abaixo).
  Arquivos novos: `ItemCatalogPatcher.cs`, `AuditLogService.cs`, `Api/AuditLogController.cs`,
  `mods/TRL-ItemsManagement/memory/` (criada nesta sessão), `references/graphs/mods/TRL-ItemsManagement/`
  (grafo gerado pela 1ª vez nesta sessão — 269 nós / 335 arestas / 32 comunidades).
- **Build local ok** — último `compile-mod.sh TRL-ItemsManagement` compilou sem warnings/erros e foi
  deployado em `D:/SPT/SPT/user/mods/TRL-ItemsManagement` (ambiente de teste local). Produção
  (100.106.152.7) está em v1.0.2 **sem** o redesign do audit log/undo (deploy anterior a esta sessão).

## O que foi feito nesta sessão

**Resumo — ver a Sessão 1 da memória (link acima) pra decisões/lições/cronologia completas:**

1. Root-caused e corrigido o bug de produção: `data/items.json` (cache do mod, lido pelo browser) nunca
   resincronizava após uma escrita — só um "rescan" manual atualizava. Fix: `ItemCatalogPatcher.cs` (write-
   back compartilhado) + mutação direta do bloco `spt` em `FleaPriceController.cs`/`BanController.cs`.
   `FleaCapController` tem o mesmo sintoma mas é bulk/categórico — deixado documentado, não corrigido
   (P-1.2).
2. Redesenhado o audit log inteiro: de tabela técnica pra feed de atividade (ícones, deltas coloridos,
   nomes de trader, tempo relativo PT-BR, resumo colapsável de baseline, busca por nome) + botão de
   "Desfazer" que reusa os mesmos endpoints de escrita (undo = nova entrada no log, nunca apaga histórico).
3. Code-review final achou e corrigiu 4 bugs (todos testados ao vivo): guard errado no undo de
   `flea-price/set`, `renderBaselineSummary()` ficando expandida-mas-vazia após refresh, busca sem
   correspondência mostrando o feed inteiro sem filtro em vez de "nenhum resultado", toggle do teto de
   flea dessincronizando do topbar após undo.

## Regras deste mod que economizam tempo

- **`data/items.json` é o cache do MOD, não o `ragfair.json`/`items.json` que o jogo lê** — são dois
  arquivos diferentes com propósitos diferentes. Qualquer escrita nova precisa considerar as DUAS
  camadas (ver `ItemCatalogPatcher`'s class doc).
- **`wwwroot/index.html` é um único arquivo inline** — o `ModValidator` do SPT rejeita o mod inteiro se
  achar qualquer `.js`/`.ts` separado na pasta instalada. Todo JS novo vai dentro da mesma tag `<script>`.
- **"Só toca disco em mutação real"** — ações no-op (delete de override inexistente, toggle pro valor já
  atual) nunca escrevem cache nem geram entrada de audit log. Regra já quebrada 1x nesta sessão (`DeletePrice`)
  e corrigida — vale conferir em qualquer endpoint novo.
- **Undo sempre reusa o endpoint de escrita existente**, nunca um endpoint de "revert" dedicado — undo
  cria uma NOVA entrada de log, não apaga a antiga.
- **Build:** `bash .agents/scripts/compile-mod.sh TRL-ItemsManagement` (compila client+server, copia
  `wwwroot/` e a DLL pro `D:/SPT` local). Servidor local (`SPT.Server.exe`) **não precisa restart** pra
  mudanças em `wwwroot/` (arquivos estáticos servidos direto do disco a cada request).

## Pendências vivas (IDs da memória, Sessão 1)

| ID | O quê | Tipo |
|---|---|---|
| **P-1.1** | Redesign do audit log + undo implementado e validado só localmente — falta commit, bump de versão e deploy em produção | 🟡 débito |
| **P-1.2** | `FleaCapController` não resincroniza `data/items.json` após toggle (mesmo gap que existia pro flea-price/ban) — decisão consciente de não corrigir (fix bulk/categórico, não per-tpl); revisitar se virar reclamação recorrente | 🟢 ideia |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-15 | Guilherme | Criação — handoff pós fix de cache stale + redesign do audit log com undo, tudo pendente de commit |
