# 014 — Invasor de raid não vira inimigo dos bots (corrida com registro de jogador vivo) · Code Review 01

**Mod:** FIKA
**Spec funcional:** [014-invasor-nao-vira-inimigo-bots-01-spec.md](014-invasor-nao-vira-inimigo-bots-01-spec.md)
**Spec técnica:** [014-invasor-nao-vira-inimigo-bots-02-spec-tech.md](014-invasor-nao-vira-inimigo-bots-02-spec-tech.md)
**Asbuild:** [014-invasor-nao-vira-inimigo-bots-05-asbuild.md](014-invasor-nao-vira-inimigo-bots-05-asbuild.md)
**Data:** 2026-09-24

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.
>
> **Memória consultada:** `mods/FIKA/memory/sessions.md` — desatualizada desde Sessão 10 (2026-09-14), não cobre este item. Nenhuma pendência que afete esta tarefa.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| [CR-01-01](#cr-01-01--c--gap-vs-spec--🟡-médio) | C | 🟡 | Sem rastro de diagnóstico quando a corrida acontece mas é resolvida pelo retry | ✅ Aplicado 2026-09-24 |

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

### CR-01-01 · C — Gap vs. spec · 🟡 Médio · ✅ Aplicado em 2026-09-24

**Sem rastro de diagnóstico quando a corrida acontece mas é resolvida pelo retry**

**Local:** [`mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Main/Components/CoopHandler.cs:559-572`](../../modded-V2/Fika-Plugin/Fika.Core/Main/Components/CoopHandler.cs#L559-L572)

**Problema:** O critério de aceite da spec funcional (`01-spec.md`) exige: "Nenhuma falha de registro fica completamente sem rastro — pelo menos em modo de diagnóstico, é possível confirmar que uma tentativa falhou e que uma nova tentativa aconteceu depois." O código implementado só loga em dois casos: o teto de 5s esgotado (`LogError` incondicional, linha 568) ou a falha da verificação final pós-`AddActivePLayer` (linha 598, pré-existente). Não existe nenhum log — nem em build `DEBUG` — para o caso em que a corrida **acontece e é corrigida pelo retry** (o laço das linhas 562-572 itera 1+ vezes e depois sai com sucesso). O log de sucesso já existente (`"Adding Client..."`, linha 575, `#if DEBUG`) dispara igual tanto se a corrida nunca ocorreu (0 iterações) quanto se ocorreu e foi resolvida — não dá pra diferenciar os dois casos pelos logs, nem em build de diagnóstico.

**Por que importa:** Validar em raid real *se a corrida realmente acontece e é corrigida* (não só "o jogador virou inimigo no fim", que já funcionaria mesmo sem essa correção em boa parte dos casos por sorte de timing) fica impossível de confirmar pelos logs — só dá pra inferir pelo resultado final, nunca provar que o mecanismo novo foi o que fez a diferença. O próprio checklist da spec técnica (`02-spec-tech.md`, §8) pede validação explícita desse cenário.

**Sugestão:** Adicionar um log `#if DEBUG` (mesmo padrão dos outros logs desta coroutine) uma única vez, logo após o laço das linhas 562-572 sair com sucesso, só quando ele de fato precisou esperar pelo menos um frame:

```csharp
var aliveCheckStartTime = Time.time;
var aliveCheckDeadline = aliveCheckStartTime + AliveCheckTimeoutSeconds;
var gameWorld = Singleton<GameWorld>.Instance;
while (gameWorld.GetAlivePlayerByProfileID(playerToAdd.ProfileId) == null)
{
    if (Time.time >= aliveCheckDeadline)
    {
        _logger.LogError($"AddClientToBotEnemies: {playerToAdd.Profile.GetCorrectedNickname()} nunca foi reconhecido como vivo no GameWorld após {AliveCheckTimeoutSeconds}s -- abortando registro nos bots.");
        yield break;
    }
    yield return null;
}
#if DEBUG
if (Time.time > aliveCheckStartTime)
{
    _logger.LogInfo($"AddClientToBotEnemies: {playerToAdd.Profile.GetCorrectedNickname()} precisou esperar {Time.time - aliveCheckStartTime:F2}s até ser reconhecido como vivo no GameWorld (corrida do backlog 014 detectada e resolvida pelo retry).");
}
#endif
```

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `mods/FIKA/modded-V2/Fika-Plugin/Fika.Core/Main/Components/CoopHandler.cs` — adicionado log `#if DEBUG` logo após o laço de espera, disparando só quando `Time.time > aliveCheckStartTime` (ou seja, quando a espera de fato iterou pelo menos um frame), mesclado no bloco `#if DEBUG` de sucesso já existente logo abaixo.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-24 | Code review 01 criada via `/code-review` |
| 2026-09-24 | Aplicação automática de 1 achado via `/apply-code-review` — IDs aplicados: CR-01-01 |
