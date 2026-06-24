# 014 — Sync de stances Fika · Code Review 02

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [014-sync-stances-fika-01-spec.md](014-sync-stances-fika-01-spec.md)
**Spec técnica:** [014-sync-stances-fika-02-spec-tech.md](014-sync-stances-fika-02-spec-tech.md)
**Asbuild:** [014-sync-stances-fika-05-asbuild.md](014-sync-stances-fika-05-asbuild.md)
**Fixes:** [06-fix-01](014-sync-stances-fika-06-fix-01.md) (ProcessEffectors — não funcionou) · [06-fix-02](014-sync-stances-fika-06-fix-02.md) (ObservedVisualPass Postfix — alvo desta review)
**Data:** 2026-06-23

> Review **de validação por referências** (pedido do usuário: confirmar, sem teste in-game, se o código atual deve funcionar para os requisitos). Análise por **2 validadores independentes** (sub-agents de contexto limpo) sobre os 2 elos decisivos (hook/acúmulo e transform/render), + auditoria manual da cadeia de rede. Memória consultada: snapshot Sessão 5 (desatualizada — não cobre 011-014) · pendências que afetam: nenhuma registrada para o item 014.

## Veredito

**O mecanismo central DEVE funcionar para o requisito principal** — a arma do jogador observado acompanhar a stance, coexistindo com lean/troca de ombro, sem quebrar o vanilla. Cada elo foi confirmado contra o Assembly/Fika:

| # | Elo validado | Evidência (arquivo:linha) | Status |
|---|---|---|---|
| 1 | Hook roda **todo frame** por observado | `ObservedVisualPass` chamado em `ObservedPlayer.LateUpdate` ([ObservedPlayer.cs:1529](../../../../references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1529)) | ✅ |
| 2 | **Não acumula** no caminho normal | `ShiftWeaponRoot` ([PlayerBones.cs:408-418](../../../../references/eft-decompiled/Assembly-CSharp/PlayerBones.cs#L408)) e `Kinematics` ([PlayerBones.cs:317-326](../../../../references/eft-decompiled/Assembly-CSharp/PlayerBones.cs#L317)) re-setam `Weapon_Root_Anim` por `SetPositionAndRotation` (valor **absoluto**) a cada frame, **antes** do Postfix | ✅ |
| 3 | `Weapon_Root_Anim` é **pai da malha** da arma 3ª pessoa | `Weapon_root_anim` (índice 4) é pai de `Weapon_root` (índice 5 → mesh); ref hierárquica em [Player.cs:26799](../../../../references/eft-decompiled/Assembly-CSharp/Player.cs#L26799) e `GetChild(0)` em PlayerBones | ✅ |
| 4 | Modificar `.local*` após `SetPositionAndRotation` (world) é **coerente** | padrão usado pelo próprio EFT: `SetPositionAndRotation` seguido de `.localPosition`/`.localRotation` em [PlayerBones.cs:350,385,393,401](../../../../references/eft-decompiled/Assembly-CSharp/PlayerBones.cs#L350) | ✅ |
| 5 | Offset **aditivo coexiste** com lean/ombro | lean/shoulder já estão no valor base re-setado; o Postfix soma o stance por cima ([ObservedStanceAnimator.cs:50-51](../../modded/Networking/ObservedStanceAnimator.cs#L50)) | ✅ |
| 6 | **Vanilla intacto** | sem componente → `?.` no-op; MainPlayer local não é `ObservedPlayer`, logo o patch não o toca (AP-02) | ✅ |
| 7 | **Cadeia de rede completa** | registro ([FikaSyncManager.cs:44](../../modded/Networking/FikaSyncManager.cs#L44)) → send ([:65](../../modded/Networking/FikaSyncManager.cs#L65)) → receive cria/`Init`/`SetStance` ([:68-87](../../modded/Networking/FikaSyncManager.cs#L68)) | ✅ |
| 8 | **Estado entre raids** OK | `ObservedStanceAnimator` é MonoBehaviour no gameObject do observado — destruído com ele ao fim da raid; sem estática persistente além de flags de debug | ✅ |

**Ressalvas:** há **1 achado 🟠** (acúmulo num caso de borda visível — vale corrigir *antes* do teste para não mascarar o resultado) e **1 gap 🟡** de fidelidade durante ADS. Nenhum bloqueia o caminho golden, mas o 🟠 pode produzir um sintoma assustador (arma girando) num cenário plausível.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 3 · 🟢 Menores: 2 · Total: 6

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| CR-02-01 | B | 🟠 | Postfix ignora o early-return de `ObservedVisualPass` → acúmulo quando o transform não é re-setado | ✅ Aplicado |
| CR-02-02 | C | 🟡 | `SendStance` só dispara na troca de stance, não em ADS → `_isAiming` remoto desatualizado | ✅ Aplicado |
| CR-02-03 | D | 🟡 | Eixo/escala do `GetTargetRotation` (1ª pessoa) pode divergir no `Weapon_Root_Anim` 3ª pessoa | Deferido (calibração in-game) |
| CR-02-04 | E | 🟡 | `FikaNetworkSync.cs` é código morto (paralelo ao `FikaSyncManager` atual) | ✅ Aplicado |
| CR-02-05 | F | 🟢 | `GetComponent<ObservedStanceAnimator>()` por frame (migrou de CR-01-02) | Deferido (cache → leak) |
| CR-02-06 | E | 🟢 | `_loggedHook`/`_loggedApply` estáticos não confirmam por-peer | Deferido (logs úteis no teste) |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade/manutenção** · **F — Melhoria opcional**

## Impacto

- 🔴 **Bloqueador** · 🟠 **Forte** · 🟡 **Médio** · 🟢 **Menor**

---

## Pontos

### CR-02-01 · B — Bug latente · 🟠 Forte · ✅ Aplicado em 2026-06-23

**O Postfix não respeita o early-return de `ObservedVisualPass`; quando o método retorna cedo mas o player é renderizado, o offset aditivo acumula sem o transform ser re-setado**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/ObservedStanceVisualPatch.cs:26-39`](../../modded/Patches/ObservedStanceVisualPatch.cs#L26)

**Problema:** `ObservedVisualPass` tem um early-return logo no início ([ObservedPlayer.cs:1841-1843](../../../../references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1841)):

```csharp
if (CustomAnimationsAreProcessing || !_cullingHandler.IsVisible || !HealthController.IsAlive)
    return;   // pula ShiftWeaponRoot (1876) E Kinematics (1889)
```

Quando isso ocorre, **nada re-seta** `Weapon_Root_Anim` naquele frame — mas o Postfix do Harmony **roda mesmo assim** e executa `wra.localRotation = wra.localRotation * Quaternion.Euler(_euler)` ([ObservedStanceAnimator.cs:50](../../modded/Networking/ObservedStanceAnimator.cs#L50)). Como `_euler` converge para um alvo ~constante (stance fixa), cada frame multiplica de novo → a rotação **acumula linearmente** (ex.: 10°/frame ≈ 600°/s). Para `!IsVisible` (culling) o efeito é invisível e se autocorrige ao voltar a renderizar; mas `CustomAnimationsAreProcessing` pode ser `true` com o player **visível** (animação scriptada) → arma girando descontroladamente na tela.

**Por que importa:** é o único modo de falha visível plausível no código atual. Se ocorrer durante o teste in-game, mascara o resultado ("a arma roda louca") e leva a diagnóstico errado. A confirmação #2 do veredito (não-acúmulo) só vale **enquanto o método não retorna cedo**.

**Sugestão:** replicar o gate no Postfix — só aplicar quando o vanilla também processou o frame. Usar acessor público quando existir; senão, refletir as 3 condições:

```csharp
[PatchPostfix]
private static void Postfix(ObservedPlayer __instance)
{
    try
    {
        // Respeita o early-return de ObservedVisualPass (ObservedPlayer.cs:1841):
        // se o vanilla não re-setou o Weapon_Root_Anim neste frame, somar offset acumularia.
        if (__instance.HealthController == null || !__instance.HealthController.IsAlive) return;
        if (!__instance.IsVisible) return;   // confirmar nome do acessor; senão refletir _cullingHandler.IsVisible
        // (CustomAnimationsAreProcessing: confirmar acessor; se não houver público, refletir o campo)
        ...
        __instance.gameObject.GetComponent<Networking.ObservedStanceAnimator>()?.ApplyToWeaponRoot(__instance.PlayerBones);
    }
    catch (Exception ex) { Plugin.Logger.LogError($"[StanceSync-014] VisualPatch {ex.Message}"); }
}
```

Alternativa robusta (independe de acessores): no `ApplyToWeaponRoot`, capturar `Weapon_Root_Anim.localRotation/localPosition` do frame **antes** de somar e, se forem idênticos ao último frame (transform não mudou ⇒ não re-setado), **pular** a soma. Mais código, porém imune a renomeação de membros do Fika.

**Decisão:**
- `[x]` Aceitar com modificação: **abordagem robusta** (independe de acessores privados do Fika) — preferida à reflexão de `_cullingHandler`/`CustomAnimationsAreProcessing`, cujos nomes não pude confirmar no binário.

**Resolução:** aplicada a alternativa robusta no `ObservedStanceAnimator`. Novos campos `_lastWrittenRot`/`_lastWrittenPos`/`_hasWritten`; no topo de `ApplyToWeaponRoot`, se o `Weapon_Root_Anim` atual é idêntico ao último valor que **nós** escrevemos, o vanilla não o re-setou neste frame → `return` sem somar (não acumula). Como `offset != 0` garante `base != base+offset`, a guarda só dispara em frame de fato congelado (early-return / culling), e é inócua quando o offset já convergiu a ~0.

**Aplicação:** [`ObservedStanceAnimator.cs:21-27, 38-46, 58-61`](../../modded/Networking/ObservedStanceAnimator.cs#L38) (`// ref: CR-02-01`).

---

### CR-02-02 · C — Gap vs. spec · 🟡 Médio · ✅ Aplicado em 2026-06-23

**`SendStance` só é disparado em `OnStanceChanged`; mirar/desmirar sem trocar de stance não reenvia o packet, e o `_isAiming` do observado fica desatualizado**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/StanceManager.cs:1267-1280`](../../modded/StanceManager.cs#L1267)

**Problema:** o único call-site de `SendStance` é dentro de `OnStanceChanged` ([:1279](../../modded/StanceManager.cs#L1279)) — só roda quando a **stance** muda. O `IsAiming` é amostrado nesse instante e enviado, mas se o jogador entra/sai de ADS **mantendo a mesma stance**, nenhum packet novo é enviado. No observado, `ApplyToWeaponRoot` segue usando o `_isAiming` antigo em `StanceManager.GetTargetRotation((Stance)_stance, _isAiming)` ([ObservedStanceAnimator.cs:39-40](../../modded/Networking/ObservedStanceAnimator.cs#L39)) → a pose remota usa o offset de ADS errado.

**Por que importa:** a spec quer que a pose remota reflita o estado real (incl. ADS). O sintoma é sutil (pose levemente off durante mira), não um crash — por isso 🟡, não bloqueador. Pode ser confundido com "desalinhamento" no teste.

**Sugestão:** disparar `SendStance` também na transição de ADS. Opções: (a) um Postfix leve em `FirearmController.set_IsAiming`/equivalente que, se a stance ≠ 0, reenvia `SendStance(CurrentStance, isAiming)`; (b) no `Tick`/`Update` do mod, detectar mudança de `IsAiming` (comparar com o último enviado) e reenviar. Throttle a 1 envio por transição (não por frame). Se o offset de ADS for desprezível na prática, registrar como limitação consciente no asbuild em vez de implementar.

**Decisão:**
- `[x]` Aceitar sugestão — opção (b): polling leve no `Update`, com throttle por borda.

**Resolução:** novo `StanceManager.TickAdsNetworkSync()` chamado 1×/frame no `Plugin.Update`. Compara `IsAiming` atual com `_lastSentAiming`; só reenvia `SendStance((int)CurrentStance, isAiming)` na **borda** da mudança, e apenas com stance ativa (`CurrentStance != Stance.Default` — em Default o observado ignora o offset). `OnStanceChanged` passou a atualizar `_lastSentAiming` após enviar, evitando reenvio redundante logo após a troca de stance.

**Aplicação:** [`StanceManager.cs:1267-1311`](../../modded/StanceManager.cs#L1267) (campo `_lastSentAiming` + `TickAdsNetworkSync`, `// ref: CR-02-02`), [`Plugin.cs:1622`](../../modded/Plugin.cs#L1622) (chamada no `Update`).

---

### CR-02-03 · D — Arquitetura · 🟡 Médio

**`GetTargetRotation`/`GetTargetPosition` foram calibrados para o WeaponRootAnim de 1ª pessoa (player local); o `PlayerBones.Weapon_Root_Anim` 3ª pessoa pode ter convenção de eixo/escala diferente**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Networking/ObservedStanceAnimator.cs:39-51`](../../modded/Networking/ObservedStanceAnimator.cs#L39)

**Problema:** o jogador local aplica o stance via `ApplyComplexRotation` no `HandsContainer.WeaponRootAnim` (transform de 1ª pessoa). O observado reusa os **mesmos** `GetTargetRotation/GetTargetPosition` no `PlayerBones.Weapon_Root_Anim` (transform de 3ª pessoa) — pai diferente, possivelmente eixos/escala diferentes. O movimento **vai** ocorrer (validado, confirmação #3/#4), mas a direção e a magnitude podem não bater exatamente com a 1ª pessoa.

**Por que importa:** não impede o requisito "a arma acompanha a stance", mas a fidelidade (pose remota ≈ pose local) pode exigir calibração. É exatamente o "risco residual" já previsto no [06-fix-02 §Risco residual](014-sync-stances-fika-06-fix-02.md) (descolamento mão↔arma / RotateAround no pivô da mão).

**Sugestão:** tratar como passo de calibração in-game — se a arma mover na direção/magnitude errada, introduzir um fator de conversão por eixo (ou aplicar a rotação em torno do pivô da mão via `RotateAround`, conforme já planejado). Não alterar nada antes do teste; este ponto existe para orientar a interpretação do resultado.

**Decisão:**
- `[x]` Aceitar com modificação: **deferido para calibração in-game** — não há fator a aplicar às cegas; o valor (ou a decisão por `RotateAround` no pivô da mão) sai da observação do teste. Sem mudança de código nesta rodada.

---

### CR-02-04 · E — Legibilidade/manutenção · 🟡 Médio · ✅ Aplicado em 2026-06-23

**`FikaNetworkSync.cs` é código morto, paralelo ao `Networking/FikaSyncManager.cs` em uso**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/FikaNetworkSync.cs:33,60`](../../modded/FikaNetworkSync.cs#L33)

**Problema:** a classe `FikaNetworkSync` e seu método `SendStanceUpdate(string, Stance, bool isMounting)` não têm **nenhum** call-site (grep em todo `modded` retornou só as 2 declarações). É um sistema antigo de packet `StanceSyncPacket` que foi substituído pelo `FikaSyncManager` (`SendStance`/`OnStanceSyncPacketReceived`). Os dois manipulam o **mesmo** tipo de packet.

**Por que importa:** confunde quem lê (dois "sync managers" para a mesma feature) e é um risco latente — se algum dia `FikaNetworkSync.Initialize` for chamado por engano, poderia registrar o packet em dobro. Não afeta o funcionamento atual (está inerte).

**Sugestão:** após confirmar (grep) que `Plugin.cs` não invoca `FikaNetworkSync`, **deletar** `FikaNetworkSync.cs`. Se houver algo reutilizável (ex.: o campo `isMounting`, ainda não modelado no fluxo novo), portar conscientemente para `FikaSyncManager` antes de remover.

**Decisão:**
- `[x]` Aceitar com modificação: removido **também** o `PlayerStanceController.cs` — grep confirmou que ele só era referenciado pelo `FikaNetworkSync` morto (springs nunca inicializados, offsets calculados e nunca consumidos); era o mesmo sistema legado.

**Resolução:** `git rm` de `FikaNetworkSync.cs` (classe `FikaNetworkSync` + o 2º `StanceSyncPacket` conflitante no namespace `CameraRotationMod.FikaSync`) e `PlayerStanceController.cs`. Build limpo de duplicidade de packet e de um experimento abandonado. `Init()` nunca era chamado, logo nada de runtime muda.

**Aplicação:** `git rm mods/.../modded/FikaNetworkSync.cs mods/.../modded/PlayerStanceController.cs`.

---

### CR-02-05 · F — Melhoria opcional · 🟢 Menor

**`GetComponent<ObservedStanceAnimator>()` por frame no Postfix (migração de CR-01-02, antes deferido)**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/ObservedStanceVisualPatch.cs:36`](../../modded/Patches/ObservedStanceVisualPatch.cs#L36)

**Problema:** o mesmo achado **CR-01-02** (deferido na review 01, então em `ApplyComplexRotationPatch.cs:162`) migrou para o novo Postfix — `GetComponent` é chamado por observado por frame (LateUpdate). Custo O(1) com cache interno do Unity, null-safe via `?.`; trivial.

**Por que importa:** micro-otimização; só relevante com muitos observados. Se o CR-02-01 for aplicado com a referência do animator obtida no receive, esse `GetComponent` some de graça.

**Sugestão:** cachear o `ObservedStanceAnimator` (ex.: dict `ProfileId→animator` no `FikaSyncManager`, populado em `OnStanceSyncPacketReceived`, lido pelo Postfix). Opcional — só faz sentido se o CR-02-01 já reescrever o Postfix.

**Decisão:**
- `[x]` Rejeitar (deferir): um dict estático `ProfileId→animator` viola `csharp-mod-best-practices §1` (static collection precisa de ponto de limpeza por raid) e introduziria leak/stale entre raids para poupar um `GetComponent` O(1) com cache interno do Unity. A troca não compensa — mesma conclusão do CR-01-02. A abordagem do CR-02-01 não tocou esse caminho, então a premissa da sugestão ("só se CR-02-01 reescrever o Postfix") não se concretizou.

---

### CR-02-06 · E — Legibilidade/manutenção · 🟢 Menor

**`_loggedHook`/`_loggedApply` são `static bool` → o log de confirmação dispara uma vez global, não por peer**

**Local:** [`ObservedStanceVisualPatch.cs:21`](../../modded/Patches/ObservedStanceVisualPatch.cs#L21) e [`ObservedStanceAnimator.cs:22,53`](../../modded/Networking/ObservedStanceAnimator.cs#L22)

**Problema:** com múltiplos observados, `_loggedHook`/`_loggedApply` (estáticos) emitem o log de diagnóstico **uma única vez** no processo. Para o teste decisivo de 2 clientes é suficiente (basta saber se *roda*), mas não confirma que cada peer está sendo processado.

**Por que importa:** só diagnóstico. Pode dar falsa sensação de "só 1 peer sincroniza" quando na verdade o log é global.

**Sugestão:** para o teste de validação, manter como está (evita spam). Depois de validado, ou trocar para log gated por config (debug toggle) por-peer, ou remover. Não-bloqueante.

**Decisão:**
- `[x]` Rejeitar (deferir): a própria sugestão recomenda **manter no teste** — esses logs `[StanceSync-014]` são o instrumento de diagnóstico do próximo teste de 2 clientes. Revisitar (gate por toggle de debug ou remoção) **após** a validação in-game.

---

## Recomendação de sequência

1. **Antes do teste in-game:** aplicar **CR-02-01** (barato, remove o único modo de falha visível que mascararia o resultado) e, se quiser fidelidade de ADS, **CR-02-02**.
2. **Durante o teste:** usar o resultado para decidir **CR-02-03** (calibração de eixo / RotateAround) — só faz sentido com a arma se movendo na tela.
3. **Pós-validação (limpeza):** **CR-02-04** (remover código morto), **CR-02-05**/**CR-02-06** (perf/log) — não-bloqueantes.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-23 | Code review 02 (validação por referências, 2 validadores independentes + auditoria da rede) — 0 🔴, 1 🟠, 3 🟡, 2 🟢. Veredito: mecanismo central deve funcionar; CR-02-01 recomendado antes do teste. |
| 2026-06-23 | Aplicados CR-02-01 (guarda anti-acúmulo no `ObservedStanceAnimator`), CR-02-02 (`TickAdsNetworkSync` reenvia stance ao mirar), CR-02-04 (removidos `FikaNetworkSync.cs` + `PlayerStanceController.cs` mortos). CR-02-03 deferido p/ calibração in-game; CR-02-05/06 deferidos (cache→leak / logs úteis no teste). |
