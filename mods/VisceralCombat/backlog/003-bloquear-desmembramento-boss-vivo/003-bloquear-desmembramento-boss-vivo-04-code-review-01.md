# 003 — Bloquear Desmembramento de Perna em Boss/Escolta Vivos · Code Review 01

**Mod:** VisceralCombat
**Spec funcional:** [003-bloquear-desmembramento-boss-vivo-01-spec.md](003-bloquear-desmembramento-boss-vivo-01-spec.md)
**Spec técnica:** [003-bloquear-desmembramento-boss-vivo-02-spec-tech.md](003-bloquear-desmembramento-boss-vivo-02-spec-tech.md)
**Asbuild:** [003-bloquear-desmembramento-boss-vivo-05-asbuild.md](003-bloquear-desmembramento-boss-vivo-05-asbuild.md)
**Data:** 2026-09-10

**Memória consultada:** snapshot de 2026-09-09 (Sessão 7) — sem pendências que afetem este item. **Docs técnicos conferidos:** `spt-antipatterns.md` (sempre); `fika-packet-desync-prevention-plan.md` não se aplica (sem pacote de rede envolvido).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 0 · Total: 0

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |

## Análise

Diff revisado: [`LimbKillPatch.cs:66-79`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L66-L79).

- **Categoria A/B (crítico/bug latente):** nenhum. A guarda só executa dentro do ramo `!isDead` já existente, sem alterar o ramo pós-morte. `role.HasValue` cobre corretamente o caso de `Profile`/`Info`/`Settings` nulo (verificado contra a cadeia de tipos real do Assembly na review técnica 01) — sem risco de `NullReferenceException`.
- **Categoria C (gap vs. spec):** nenhum. Todos os critérios de aceite da spec funcional (Boss vivo não desmembra, escolta não desmembra, bot comum inalterado, pós-morte inalterado) são satisfeitos pela mesma linha de código — não há caminho alternativo não coberto.
- **Categoria D (arquitetura):** nenhum. Reusa a extension method canônica do próprio EFT (`WildSpawnType.IsBossOrFollower()`) em vez de introduzir uma lista própria de boss no mod — evita dívida de manutenção a cada boss novo do jogo. Não duplica lógica existente, não introduz estado novo, não toca sandbox fora de `modded/`.
- **Categoria E (legibilidade):** nenhum achado. Comentário inline explica o "porquê" (por que só o ramo vivo, por que sem lista própria) sem redigir o óbvio.
- **Categoria F (melhoria opcional):** nenhuma sugestão — a mudança é mínima e já está no tamanho certo pro escopo do item.

**Custo em hot path:** `IsBossOrFollower()` faz um lookup em dicionário (`Dictionary_0.TryGetValue`, já usado nativamente pelo próprio jogo para essa mesma classificação) — chamado só quando um bot vivo leva tiro de perna, não por frame; custo desprezível.

Nenhum achado a reportar nesta rodada — implementação bate com a spec técnica sem desvios.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-10 | Code review 01 criada via `/code-review` — 0 achados |
