# 007 — rogues-armas-montadas · Code Review 01

**Mod:** TRL-DynamicSpawn (com réplica funcional para TRL-Fixes)
**Spec funcional:** [007-rogues-armas-montadas-01-spec.md](007-rogues-armas-montadas-01-spec.md)
**Data:** 2026-08-05T03:17:00-03:00

> Análise crítica do código implementado para a correção do acionamento de armas montadas por bots de IA (Rogues exUsec) no SPT 4.0 / FIKA.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 1 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | F — Melhoria opcional | 🟢 Menor | Migração completa dos patches de metralhadora do TRL-DynamicSpawn para o TRL-Fixes | ✅ Aplicado |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### CR-01-01 · F — Melhoria opcional · 🟢 Menor

**Organização de Arquitetura e Encapsulamento dos Patches em TRL-Fixes**

**Local:** [`mods/TRL-Fixes/modded/Patches/BotMountWeaponFixPatch.cs:1-170`](../../../../TRL-Fixes/modded/Patches/BotMountWeaponFixPatch.cs#L1)

**Problema:** Os patches de correção da arma montada viviam temporariamente dentro do `TRL-DynamicSpawn`, misturando lógica de geração dinâmica de bots com correções globais do motor do jogo e do FIKA.

**Por que importa:** Manter os patches no `TRL-Fixes` isola a funcionalidade em um mod dedicado a correções de engine (`com.trl.fixes`), permitindo reuso em qualquer raid com ou sem o mod de spawn dinâmico.

**Sugestão:** Mover os patches testados e validados (`ExUsecBrainClass`, `GClass81`, `method_4`, `FikaPlayer` e `EFT.Player`) para `mods/TRL-Fixes/modded/Patches/BotMountWeaponFixPatch.cs` e limpar o `TRL-DynamicSpawn`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Réplica e migração executadas com sucesso na versão `1.2.0` do `TRL-Fixes` e remoção completa realizada no `TRL-DynamicSpawn` `3.4.0`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-05T03:17:00-03:00 | Code review 01 criada e concluída via `/code-review` |
