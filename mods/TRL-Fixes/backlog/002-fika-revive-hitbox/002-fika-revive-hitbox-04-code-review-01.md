---
title: Fika Revive Hitbox Loss Fix — Code Review 01
date: 2026-07-26
status: 🟢 Vivo
authors: [Claude, Guilherme]
---

# 002 — Fika: hitbox perdida após revive · Code Review 01

**Mod:** TRL-Fixes
**Spec funcional:** [002-fika-revive-hitbox-01-spec.md](./002-fika-revive-hitbox-01-spec.md)
**Spec técnica:** [002-fika-revive-hitbox-02-spec-tech.md](./002-fika-revive-hitbox-02-spec-tech.md)

**Desvio de gate declarado:** sem `/review-technical-spec` e sem `05-asbuild.md`, igual ao item 013 do ICM e pelo mesmo motivo — fix pontual com causa-raiz provada no Assembly antes do código. Arquivo tocado: `modded/Patches/Patch_FikaReviveHitbox.cs` (novo) + registro no `modded/Plugin.cs`.

**Memória consultada:** este mod não tem `memory/sessions.md` — sem memória prévia. O achado vem da memória do ICM (1º teste in-game, 2026-07-26).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Aplicados: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | D — Arquitetura | 🔴 Bloqueador | Patch resolve tipo do Fika por nome sem declarar SoftDependency — falha silenciosa disfarçada de "Fika ausente" | ✅ Aplicado |
| CR-01-02 | E — Legibilidade/manutenção | 🟡 Médio | `UnityEngine.Debug` em vez do `ManualLogSource` do BepInEx, em todo o mod | ✅ Aplicado |

---

### CR-01-01 · D — Arquitetura · 🔴 Bloqueador

**Patch resolve tipo do Fika por nome sem declarar SoftDependency — falha silenciosa disfarçada de "Fika ausente"**

**Local:** [`modded/Plugin.cs`](../../modded/Plugin.cs) · [`modded/Patches/Patch_FikaReviveHitbox.cs`](../../modded/Patches/Patch_FikaReviveHitbox.cs) — `Enable()`

**Problema:** o `Enable()` resolve `Fika.Core.Main.Components.ReviveInteractable` por `AccessTools.TypeByName` no `Awake()` do plugin, e trata `type == null` como "Fika não instalado". Mas o plugin não declarava nenhuma dependência do Fika, então a ordem de carga do BepInEx é indeterminada.

**Por que importa:** este é o pior modo de falha possível para este item específico. Se o BepInEx carregar o `TRL-Fixes` antes do `Fika.Core`, o patch é dispensado e o log diz **"Fika nao encontrado — patch de hitbox pos-revive dispensado"**. Alguém lendo esse log no próximo teste concluiria que o Fika não está instalado — quando na verdade está, e o bug de hitbox continuaria acontecendo com o fix "aplicado". O item existe justamente para investigar um sintoma confuso; um segundo sintoma confuso em cima dele custaria uma sessão de teste inteira.

Classificado 🔴 e não 🟠 (diferente do achado equivalente no ICM, onde o mesmo problema é pré-existente e comprovadamente funciona hoje) porque aqui é código novo, nunca executado, e o modo de falha engana o diagnóstico.

**Sugestão:** `[BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]` no plugin. `SoftDependency` ordena a carga quando o Fika está presente, sem exigir que esteja.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** aplicado, com o comentário explicando por que a dependência é obrigatória apesar de o patch ser opcional. GUID `com.fika.core` confirmado em `references/fika-plugin/Fika.Core/FikaPlugin.cs:40`; padrão idêntico ao de DiscordRaidMap e MOAR-Client no repo.

---

### CR-01-02 · E — Legibilidade/manutenção · 🟡 Médio

**`UnityEngine.Debug` em vez do `ManualLogSource` do BepInEx, em todo o mod**

**Local:** os 4 patches em [`modded/Patches/`](../../modded/Patches/)

**Problema:** `csharp-mod-best-practices` §8 é explícito: *"One `ManualLogSource` per plugin. Never `Console.WriteLine` or `Debug.Log` — those bypass the SPT log infrastructure."* Os três patches pré-existentes usam `UnityEngine.Debug`, e o patch novo seguiu a convenção local.

**Por que importa:** `Debug.Log` sai no log sem o prefixo do plugin e sem nível BepInEx, o que atrapalha exatamente o que este item precisa: encontrar, no `LogOutput.log` de duas máquinas, se o hook subiu. A linha de confirmação do patch é o pré-requisito declarado na §4 da spec técnica — ela precisa ser localizável.

Aplicar só no patch novo criaria inconsistência com os três vizinhos, então a correção certa é o mod inteiro. São 15 chamadas em 4 arquivos, mecânico.

**Sugestão:** expor `internal static ManualLogSource Log` no `Plugin`, atribuir no `Awake()`, e trocar as 15 chamadas por `Plugin.Log?.LogInfo/LogWarning/LogError`. O `?.` cobre o caso de um patch ser exercitado antes do `Awake()` concluir.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** aplicado nos 4 patches (15 chamadas). Escopo estendido além do item de propósito, para não deixar o mod com dois padrões de log. `using UnityEngine` removido do arquivo novo, que não usa mais nada do namespace; mantido nos outros três, que usam.

---

## Não-achados (verificados e descartados)

- **Guard de headless ausente** — deliberado e documentado na §2 da spec técnica: o Fika já faz early-return em headless no `Init` (`:56-61`) e no `RemoveRagdoll` (`:116-119`), então o Postfix roda sobre um corpo cuja layer nunca foi mexida. `SetupHitColliders` é idempotente e não toca câmera, render nem animator (corpo em `Player.cs:29832-29846`). Um guard por reflection em `FikaBackendUtils.IsHeadless` adicionaria superfície de quebra sem resolver risco.
- **Reflection não cacheada** — `_observedPlayerField` é resolvido uma vez no `Enable()` e validado ali, com log claro se o shape do Fika mudar. `TypeByName`/`Method` também rodam uma vez só.
- **Postfix sem try/catch** — tem.
- **AP-03 (dispatch virtual)** — `SetupHitColliders` é `public virtual`, mas nenhum tipo na cadeia `ObservedPlayer → FikaPlayer → LocalPlayer → Player` o sobrescreve; a chamada resolve para a implementação de `Player`. `RemoveRagdoll` não é virtual.
- **`public class` em vez de `internal`** — o checklist §5 prefere `internal`, mas os três patches vizinhos são `public` e o `Plugin` os instancia por `new Patches.X()`. Divergir num arquivo só não vale; se o mod for arrumado, é de uma vez.

## Verificação pendente

Nenhum achado bloqueia. Duas confirmações em jogo, ambas no cenário **C2** do [roteiro happy-flow do ICM](../../../TRL-ImmersiveCombatMedicine/docs/happy-flow-test-plan.md):

1. **O hook subiu** — `TRL-Fixes: Hook no ReviveInteractable.RemoveRagdoll aplicado com sucesso!` no `LogOutput.log`. Sem essa linha, qualquer conclusão do teste é inválida.
2. **A divergência bot × jogador** — a spec técnica §3 registra que a hipótese de auto-recuperação por troca de equipamento foi **refutada** na leitura do código, e que portanto o relato de "bots acertavam, jogador não" continua sem explicação. Medir as duas fontes de dano separadamente. O fix é correto de qualquer forma (restaura a invariante de `Player.Init`), mas o item só fecha quando essa divergência for explicada ou deixar de se reproduzir.
