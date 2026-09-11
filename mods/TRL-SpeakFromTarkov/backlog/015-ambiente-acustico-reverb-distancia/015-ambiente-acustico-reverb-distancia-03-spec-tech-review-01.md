# 015 — Ambiente Acústico: Reverb e Curva de Distância · Review Técnica 01

**Mod:** TRL-SpeakFromTarkov
**Spec técnica revisada:** [015-ambiente-acustico-reverb-distancia-02-spec-tech.md](015-ambiente-acustico-reverb-distancia-02-spec-tech.md)
**Data:** 2026-09-09

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟡 Importante | `bypassReverbZones = true` já existente pode ser mal interpretado — spec não esclarece que é um sistema diferente | ✅ Resolvido |
| PA-01-02 | A — Gap | 🟢 Menor | `Singleton<BetterAudio>` funciona, mas `BetterAudio.Instance` direto é mais simples — confirmado via `MonoBehaviourSingleton<T>` | ✅ Resolvido |

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

### PA-01-01 · ✅ Resolvido em 2026-09-09 · A — Gap · 🟡 Importante

**`bypassReverbZones = true` já existente pode ser mal interpretado — spec não esclarece que é um sistema diferente**

**Problema:** Confirmei em `Audio/RemoteSpeaker.cs:115-117` que o `AudioSource` do `RemoteSpeaker` já seta explicitamente `bypassEffects = true`, `bypassListenerEffects = true` **e `bypassReverbZones = true`**. A spec técnica não menciona essas 3 linhas em nenhum momento. Isso é relevante porque `bypassReverbZones` bypassa especificamente o sistema clássico do Unity (`AudioReverbZone`/`AudioReverbFilter`) — o mesmo que a seção 1 da spec já descarta como "alternativa" em favor do `AudioMixerGroup`/`AudioMixerSnapshot` nativo do jogo. `outputAudioMixerGroup` é um caminho de sinal **independente** desses 3 flags (eles não afetam roteamento por mixer group), então a correção proposta continua válida — mas a spec não deixa isso explícito.

**Por que importa:** Sem essa nota, quem for implementar (ou revisar de novo depois) pode se confundir de duas formas opostas: (a) achar que `bypassReverbZones = true` já "desliga reverb de propósito" e que a spec contradiz uma decisão anterior do mod, ou (b) remover essas linhas por engano achando que elas bloqueiam a correção — o que reintroduziria a interferência do sistema de `AudioReverbZone` clássico que o próprio comentário da linha 117 sugere ter sido deliberadamente evitado.

**Sugestão:** Adicionar à seção 0 (ou seção 1) da spec: *"Nota: `RemoteSpeaker.cs:115-117` já seta `bypassEffects`/`bypassListenerEffects`/`bypassReverbZones = true` — esses 3 flags bypassam sistemas de áudio diferentes (efeitos por componente na GameObject, efeitos do AudioListener, e `AudioReverbZone` clássico do Unity, respectivamente) e **não afetam** o roteamento por `outputAudioMixerGroup`, que é um caminho de sinal independente. Essas 3 linhas permanecem inalteradas — a correção deste item não as remove nem depende de removê-las."*

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Nota adicionada à seção 0 da spec técnica, logo após a conclusão sobre a causa raiz da ausência de reverb.

---

### PA-01-02 · ✅ Resolvido em 2026-09-09 · A — Gap · 🟢 Menor

**`Singleton<BetterAudio>` funciona, mas `BetterAudio.Instance` direto é mais simples — confirmado via `MonoBehaviourSingleton<T>`**

**Problema:** O stub usa `Comfort.Common.Singleton<BetterAudio>.Instantiated`/`.Instance`. Confirmei em `references/eft-decompiled/Assembly-CSharp/MonoBehaviourSingleton-1.cs:6-12` que `BetterAudio : MonoBehaviourSingleton<BetterAudio>` já expõe `public static T Instance => Singleton<T>.Instance;` e `public static bool Instantiated => Singleton<T>.Instantiated;` diretamente — são idênticos, só que `BetterAudio.Instance`/`BetterAudio.Instantiated` é mais direto (sem precisar do `Comfort.Common.Singleton<>` explícito).

**Por que importa:** Puramente estilístico — as duas formas funcionam de forma idêntica (confirmado no Assembly), não é um bug. Só um ajuste de clareza.

**Sugestão:** Trocar `Comfort.Common.Singleton<BetterAudio>.Instantiated`/`.Instance` por `BetterAudio.Instantiated`/`BetterAudio.Instance` no stub da seção 5, opcionalmente.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Todas as ocorrências na spec técnica trocadas para `BetterAudio.Instance`/`BetterAudio.Instantiated` direto.

---

## Memória consultada

Snapshot de `mods/TRL-SpeakFromTarkov/memory/sessions.md` (v1.5.3, sem pendências 🔴/🟡 relacionadas a este item).
