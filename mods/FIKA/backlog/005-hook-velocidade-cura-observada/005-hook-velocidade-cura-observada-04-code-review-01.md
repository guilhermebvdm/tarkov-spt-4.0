# 005 — Hook genérico de velocidade da animação de cura observada · Code Review 01

**Mod:** FIKA
**Spec funcional:** [005-hook-velocidade-cura-observada-01-spec.md](005-hook-velocidade-cura-observada-01-spec.md)
**Spec técnica:** [005-hook-velocidade-cura-observada-02-spec-tech.md](005-hook-velocidade-cura-observada-02-spec-tech.md)
**Asbuild:** [005-hook-velocidade-cura-observada-05-asbuild.md](005-hook-velocidade-cura-observada-05-asbuild.md)
**Data:** 2026-09-09

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

Memória consultada: snapshot 2026-09-08 (Sessão 4, `mods/FIKA/memory/sessions.md`) · pendências que afetam este item: nenhuma diretamente — mas o achado CR-01-01 abaixo toca exatamente o padrão de versionamento que as Sessões 2-4 já seguiram em todo bump anterior. Doc técnico lido (gatilho sempre): `spt-antipatterns.md` — nenhuma violação de AP-NN encontrada no código implementado.

Todos os 4 pontos da [spec-tech-review-01](005-hook-velocidade-cura-observada-03-spec-tech-review-01.md) (PA-01-01 a PA-01-04) foram confirmados implementados 1:1 no código: doc XML de ciclo de vida presente, null-guard em `ResolveExtra`, comentário de log correto (sem `TODO`), e a edição em `ObservedMedsController.cs` preserva o formato de chaves original — nenhum precisa ser reaberto.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | D | 🟠 Forte | `mods/FIKA/mod.json` não foi atualizado junto com `FikaVersion` | ✅ Aplicado 2026-09-09 |
| CR-01-02 | E | 🟢 Menor | Doc XML do hook não avisa que o delegate roda síncrono na main thread | ✅ Aplicado 2026-09-09 |

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

### CR-01-01 · D — Arquitetura · 🟠 Forte · ✅ Aplicado em 2026-09-09

**`mods/FIKA/mod.json` não foi atualizado junto com `FikaVersion`**

**Local:** [`mods/FIKA/mod.json:11`](../../../mod.json#L11)

**Problema:** `FikaPlugin.cs:49` foi bumpado de `2.3.15` para `2.3.16` (build deste item), mas `mod.json` (`components.plugin.version`) continua em `"2.3.15"`. Conferido no histórico do repo: os itens anteriores mantiveram os dois em lockstep — o commit `ea0b0e77` (itens 001-003) já trazia `mod.json` bumpado junto com o `FikaVersion` daquela leva, e o diff não commitado do item 004 (verificado nesta review) também já tinha `mod.json` em `2.3.15` alinhado ao `FikaVersion` da mesma versão. Este é o primeiro bump que quebra essa consistência.

```json
// mods/FIKA/mod.json:8-15 (estado atual)
"plugin": {
  "name": "Fika-Plugin",
  "upstream_url": "https://github.com/project-fika/Fika-Plugin.git",
  "version": "2.3.15",   // ← deveria ser "2.3.16"
  ...
```

**Por que importa:** `mod.json` é o manifesto canônico de versão do componente (usado por quem consulta o repo pra saber "que versão do fork está rodando" sem abrir o `.cs`). Ficar dessincronizado do `FikaVersion` real semeia confusão em auditorias futuras e diverge do padrão que os 4 itens anteriores estabeleceram sem exceção.

**Sugestão:** Atualizar `mods/FIKA/mod.json:11` de `"2.3.15"` para `"2.3.16"`.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `mods/FIKA/mod.json:11` — `"version": "2.3.15"` → `"2.3.16"`.

### CR-01-02 · E — Legibilidade/manutenção · 🟢 Menor · ✅ Aplicado em 2026-09-09

**Doc XML do hook não avisa que o delegate roda síncrono na main thread**

**Local:** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsSpeedHook.cs:35-40`](../../../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsSpeedHook.cs#L35-L40)

**Problema:** `ExtraSpeedMultiplier` é invocado de dentro de `HealthController_EffectRemovedEvent`/`ObservedStart` — callbacks de evento do Unity, na main thread, num caminho de gameplay (cada troca de parte do corpo curada). A doc XML documenta o contrato de retorno e o ciclo de vida, mas não diz explicitamente que o delegate roda **síncrono, na main thread, dentro de um evento de gameplay** — um consumidor que colocar algo custoso ali (I/O, alocação pesada, reflection não cacheada) causaria um hitch perceptível a cada parte do corpo curada, de QUALQUER peer observado.

**Por que importa:** Puramente preventivo — não há bug hoje (o único consumidor conhecido, `CustomClasses/090`, faz um lookup de dicionário + leitura de config, barato). Mas como este é um ponto de extensão PÚBLICO pensado para outros mods reaproveitarem (a própria spec funcional diz "qualquer mod pode assinar"), documentar a expectativa de performance evita que um futuro consumidor descuidado introduza uma reprodução do próprio problema que a skill `spt-mod-best-practices` §3 adverte (alocação/custo em hot path de gameplay).

**Sugestão:** Adicionar uma frase à doc XML de `ExtraSpeedMultiplier` (ou a `ResolveExtra`): *"Chamado de forma SÍNCRONA, na main thread, de dentro de um callback de gameplay (por parte do corpo curada) — mantenha a implementação rápida e sem I/O/alocação pesada."*

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `ObservedMedsSpeedHook.cs` — parágrafo `ref: CR-01-02` adicionado à doc XML de `ExtraSpeedMultiplier`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-09 | Code review 01 criada via `/code-review` |
| 2026-09-09 | Aplicação automática de 2 achados via `/apply-code-review` — IDs aplicados: CR-01-01, CR-01-02; rejeitados: nenhum |
