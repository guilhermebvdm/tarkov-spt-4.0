# 014 — Invasor de raid não vira inimigo dos bots (corrida com registro de jogador vivo) · Review Técnica 01

**Mod:** FIKA
**Spec técnica revisada:** [014-invasor-nao-vira-inimigo-bots-02-spec-tech.md](014-invasor-nao-vira-inimigo-bots-02-spec-tech.md)
**Data:** 2026-09-24

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.
>
> **Memória consultada:** `mods/FIKA/memory/sessions.md` — desatualizada desde Sessão 10 (2026-09-14), não cobre este item. Nenhuma pendência registrada que afete esta tarefa.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| [PA-01-01](#pa-01-01--a--gap--🟡-importante) | A | 🟡 | Teto de tentativas em contagem de frames é impreciso num host headless | ✅ Resolvido 2026-09-24 |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · A — Gap · ✅ Resolvido em 2026-09-24

**Teto de tentativas em contagem de frames é impreciso num host headless**

**Problema:** O stub (§5) usa `const int MaxAliveCheckAttempts = 300; // ~5s a 60fps` como teto pra nova espera, incrementado uma vez por `yield return null` (uma vez por frame renderizado). Essa suposição de "60fps" não tem nenhuma evidência de que se aplica ao cenário **principal** que este item existe pra corrigir: um servidor **headless**. Um headless não renderiza nada — na prática costuma rodar sem cap de frame (podendo terminar 300 frames em bem menos de 1 segundo de tempo real) ou, sob carga pesada (muitos bots/jogadores, cenário típico de invasão de raid em andamento), pode rodar bem abaixo de 60fps (fazendo o teto real de tempo passar de 5s pra um valor bem maior ou menor, dependendo da carga do momento). A spec não justifica por que 300 frames é um teto adequado nesse contexto especificamente, nem cita evidência de qual é o framerate típico de um `HeadlessGameController`.

**Por que importa:** Contagem de frames é uma unidade que varia com a performance do host — não é um teto confiável de "tempo real de espera". Isso não muda a CORREÇÃO da lógica (o `while` ainda funciona, ainda sai assim que `GetAlivePlayerByProfileID` resolver), mas a spec funcional (`01-spec.md`, critério de aceite) pede explicitamente um "número/tempo limitado e **definido**" — um teto em frames não dá essa previsibilidade num host cuja taxa de frame não é controlada por nós nem documentada aqui.

**Sugestão:** Trocar a condição de teto de contagem de frames pra tempo real decorrido, usando `Time.time`/`Time.realtimeSinceStartup` (mesmo padrão que já apareceria numa medida de tempo de jogo) em vez de um contador incrementado por `yield return null`. Concretamente, no stub §5:

```csharp
var aliveCheckDeadline = Time.time + AliveCheckTimeoutSeconds; // ex.: 5f — constante nomeada, documentando a intenção em segundos reais, não em frames
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
```

Isso preserva a mesma lógica/estrutura do stub atual (ainda é um polling por `yield return null`, mesmo estilo dos dois `while` já existentes acima na mesma coroutine — `CoopHandler.cs:538-546`), só troca a unidade do teto de "frames" pra "segundos reais", que é o que a spec funcional já pede.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Aplicada em `014-invasor-nao-vira-inimigo-bots-02-spec-tech.md` (§5, §8) — o teto trocou de contagem de frames pra `Time.time`/segundos reais (`AliveCheckTimeoutSeconds`).
