# Backlog — Mod Servidor TarkovRedLine (C#)

> Índice de itens de backlog do **mod servidor** TarkovRedLine (`TarkovRedLine.Server`, C# / SPT 4.0). Ciclo: mesmo workflow SDD dos mods ([WORKFLOW.md](../../../../WORKFLOW.md)) com as adaptações abaixo. Criado em 2026-08-02.

## Adaptações do workflow (servidor C# ≠ mod client)

- **Sandbox:** todo o `Server/TarkovRedLine.Server/` é editável (papel do `modded/`). A versão TS legada em `Server/TarkovRedLine-ServerMod/` é referência histórica da migração (papel do `original/` — **não editar**; ver memória `project_trl_server_mod_migration`).
- **Build:** `dotnet build Server/TarkovRedLine.Server/TarkovRedLine.Server.csproj` (o `/compile-mod` não cobre o servidor). Build de homolog: `-p:Homolog=true` (rotas/pastas/arquivos de estado prefixados via [ModRouting.cs](../TarkovRedLine.Server/ModRouting.cs), pra prod e homolog coexistirem no mesmo server).
- **Deploy:** parar `SPT.Server.exe` → copiar a DLL para `D:\SPT\SPT\user\mods\TarkovRedLine.Server\` → reiniciar. Ver [RELEASE-CHECKLIST.md](../RELEASE-CHECKLIST.md). Ambiente de trabalho = `D:\SPT`; produção = remoto `100.106.152.7`.
- **Evidência:** hierarquia normal do repo; o código-fonte do servidor SPT (`references/spt-source/`) é a fonte 🥇 para APIs de servidor; a versão TS (`TarkovRedLine-ServerMod/`) entra como 🥈 (paridade de comportamento).
- **`<ref>` nos commands:** `TarkovRedLine.Server NNN` ou o path da pasta do item.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | Cache persistente do manifesto (fim da espera "preparing the list") | O manifesto/hash é cacheado só **em memória** e gerado **sob demanda** ([ModUpdater.cs](../TarkovRedLine.Server/Controllers/ModUpdater.cs)) → some a cada restart do servidor (e o AutoSync roda em watcher com auto-restart), e o 1º player pós-boot paga o 503 "Manifesto ainda sendo gerado" → countdown de 30s no launcher. Fix: **persistir manifesto+hash em disco** com uma impressão leve do `mods_repo`; no boot, se a impressão bate, **carrega do disco** (zero espera); se mudou, regera e regrava. Elimina a espera em todo boot sem mudança de conteúdo. | [001-cache-persistente-do-manifesto/](./001-cache-persistente-do-manifesto/) | ⚪ |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado · ⚫ Adiado/descopado

## Fluxo

1. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
2. `/review-spec <ref>` → revisão crítica da spec funcional
3. `/create-technical-spec <ref>` → spec técnica (código, pontos de patch)
4. `/review-technical-spec <ref>` → revisão da spec técnica (zerar bloqueadores)
5. `/code-mod <ref>` → implementação · `/code-review <ref>` → análise crítica · `/apply-code-review <ref>`
