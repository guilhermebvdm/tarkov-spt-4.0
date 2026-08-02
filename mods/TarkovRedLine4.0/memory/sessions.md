# Memória de Sessões — TarkovRedLine4.0

> Memória cronológica do mod TarkovRedLine4.0 (server mod C#, tooling de servidor e artefatos de distribuição). Criada em 2026-08-02 — sem backfill de sessões anteriores; histórico anterior vive no `git log` e nos docs do mod.

## Estado atual (snapshot ao fim da última sessão)

- **AutoSync-Cache.ps1 v2** no repo (commits `b72d25c5` + `eedac894`): o gatilho de abrir o jogo passou de "hash agregado de `user\mods` mudou" para **cobertura direta do cache 3D** — só abre o headless se existir `*.bundle` de mod sem cópia válida (existência+tamanho) em `SPT\user\cache\bundles`. Mods só de lógica/JSON (31 dos 44) nunca abrem o jogo.
- Estado do script em `autosync-state.json` (raiz do servidor): inventário `path → size|mtimeUtcTicks` dos `bundles.json` + `*.bundle` por mod, mais lista `knownMissing` (bundles que um warmup não conseguiu gerar — não reabrem o jogo a cada execução). `ultimo_mod_hash.txt` extinto (auto-removido na primeira gravação de estado).
- Deploy preparado: `D:\SPT_Files\AutoSync-Cache.ps1` contém a v2 validada; o `AutoSync-Cache.bat` do servidor **não muda** (conteúdo idêntico ao do repo; diferença era só CRLF vs LF, e CRLF é o correto para `.bat`).
- **Produção (100.106.152.7) ainda roda a versão antiga** até o usuário substituir o `.ps1` — ver [P-1.1].
- `mods/TarkovRedLine3.11/AutoSync-Cache.ps1` é a variante do servidor legado 3.11 — intocada de propósito.
- Doc de produto dos fluxos (AutoSync + "Verificar arquivos"): [launcher/Launcher4.0-v2/docs/01-fluxo-autosync-e-verificar-arquivos.md](../../launcher/Launcher4.0-v2/docs/01-fluxo-autosync-e-verificar-arquivos.md) (commit `b73faa33`), com critérios de aceite CA-A1..A7 do AutoSync.

## Pendências / próximos passos conhecidos

- **[P-1.1]** (aberta 2026-08-02) Validar a **primeira execução real da v2 no servidor de produção**: esperado NÃO abrir o jogo (cache completo), criar `autosync-state.json`, apagar `ultimo_mod_hash.txt` e remover eventuais órfãos do cache/`mods_repo`. Sugerido rodar antes com `-CheckOnly`. Categoria: 🟡 débito (código pronto, validação pendente — cf. memória global `feedback_spt_validation`).

---

## 2026-08-02 02:43 (GMT-3) — Sessão 1: rework do AutoSync-Cache (gatilho inteligente de warmup do cache 3D)

**Tema central:** reescrever o `AutoSync-Cache.ps1` do servidor de produção para abrir o jogo (headless) só quando necessário, e responder se o hash que ele gerava ainda tinha uso após o launcher 2.0.0.

**Decisões-chave:**
- **Gatilho por cobertura, não por mudança:** o jogo abre apenas se algum bundle de mod não tem cópia válida no cache do cliente — porque `user\cache\bundles` espelha 1:1 os relpaths de `user\mods\<mod>\bundles\` e só 13/44 mods têm bundles. Ref: [AutoSync-Cache.ps1](../AutoSync-Cache.ps1) (funções `Get-SourceBundleState`/`Get-CacheGaps`).
- **Sem hash no script:** detecção por `size|mtime` + verificação de existência/tamanho; hashear 6,7 GB a cada boot era desnecessário. O `ultimo_mod_hash.txt` nunca foi lido pelo launcher (100% interno ao script antigo) — extinto, auto-migração na primeira execução.
- **Manutenção sem abrir o jogo:** bundle alterado → cópia do cache invalidada (cobre mudança de conteúdo com mesmo tamanho); mod removido → órfãos deletados do cache diretamente (o warmup antigo nem limpava — lixo era distribuído em GB via `mods_repo`).
- **`knownMissing` anti-loop:** bundle que o warmup não popular vira pendência registrada e não reabre o jogo a cada execução (jogadores baixam in-game — fallback normal do SPT); nova tentativa só se a origem mudar.
- **Robustez:** timeouts no warmup (420s servidor / 900s cliente), falha → estado NÃO gravado → retry na próxima execução; robocopy com `/R:2 /W:5` (default de 1M retries × 30s congelava o script em arquivo travado); novos params `-CheckOnly` (relatório sem mutação) e `-Root` (teste em outra instalação).
- **`bundleHashCache.json` continua no espelho** para o `mods_repo` (55 KB inofensivos) — decisão consciente de não mudar o manifesto de produção sem necessidade.

**Lições / hipóteses descartadas:**
- **Premissa do script antigo refutada:** "abrir o jogo gera o cache do servidor". Na verdade `bundleHashCache.json` é escrito pelo **servidor no boot** (`BundleHashCacheService`/`BundleLoader` em `references/spt-source/Libraries/SPTarkov.Server.Core/`); só `user\cache\bundles` é cache do **cliente** (o servidor serve bundles direto de `user/mods/<mod>/bundles/` via `/files/bundle` e nunca escreve ali). Detalhe completo na memória global `reference_spt_bundle_cache_pipeline`.
- **PS 5.1: `return , $array` + `@(...)` no caller = wrap duplo** — a lista imprime N itens mas `Count`=1 e `Sort-Object` não ordena (ordenou 1 elemento). Sintoma visto no primeiro dry-run ("gaps: 1" com 25 itens fora de ordem). Fix: `return $array` puro quando todo caller envolve com `@()` (vazio vira 0 elementos corretamente).
- **O hash antigo se sabotava:** `-Exclude "*.log","*.txt","*.json"` excluía justamente o `bundles.json` (manifesto de bundles do mod), além de usar `FullName` absoluto e concat O(n²).
- **`.bat` repo vs servidor:** diferença de 18 bytes era só line ending (repo LF, servidor CRLF, via `fc.exe /B`); CRLF é o formato correto para `.bat` (labels de `goto`) — servidor mantido intocado.

**Atividade cronológica:**
1. 3 agentes Explore em paralelo — localização do script (só existe no repo + produção), motor de sync do launcher 2.0.0, mapeamento de `user\cache`/bundles em `D:\SPT` (809 arquivos, 6,67 GB, 13 mods com bundles).
2. Reescrita completa do script (374 linhas) — commit `b72d25c5`.
3. Dry-run `-CheckOnly -Root D:\SPT` → bug de contagem de arrays → diagnóstico empírico do wrap duplo → fix → re-run OK (25 gaps, 82 órfãos detectados na instalação local).
4. Testes round-trip do estado (funções reais extraídas via AST do parser): 9/9 OK, incluindo array de 1 elemento no JSON.
5. Revisão final → poda de `knownMissing` resolvidos no branch sem warmup + comentários 100% ASCII — commit `eedac894`.
6. Validação dos 2 arquivos copiados do servidor (`D:\SPT_Files`): só o `.ps1` precisa ser substituído; v2 copiada para lá.
7. Release note de produto + esclarecimento dos hashes remanescentes do ecossistema (MD5 do manifesto = `ModUpdater.cs:347`; CRC32 = servidor no boot).

**Pendências abertas nesta sessão:**
- [P-1.1] (aberta 2026-08-02) Validar primeira execução real no servidor de produção. Categoria: 🟡 débito.

**Cross-refs:**
- Trabalho paralelo nesta sessão no launcher (doc de fluxo + descobertas do sync engine): ver `launcher/Launcher4.0-v2/memory/sessions.md` 2026-08-02 (Sessão 2).
