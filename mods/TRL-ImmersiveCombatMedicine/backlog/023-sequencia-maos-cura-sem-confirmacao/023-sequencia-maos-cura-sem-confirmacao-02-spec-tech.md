# 023 — Sequência de mãos da cura sem confirmação de operação · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [023-sequencia-maos-cura-sem-confirmacao-01-spec.md](023-sequencia-maos-cura-sem-confirmacao-01-spec.md)
**Criado:** 2026-09-12

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

**Não é um novo Harmony patch.** É uma correção de sequenciamento dentro do próprio código do mod (`BandAidController.cs`), trocando espera por tempo fixo / disparo imediato por uso dos **callbacks nativos que o próprio EFT já expõe** e que o mod hoje descarta:

- `Player.SetInHands(Item item, Callback<IHandsController> callback)` — [`Player.cs:31845`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31845) — delega para `TryProceed` (linha 31856) ou, se o item tiver endereço atual, para `SetEmptyHands` primeiro (linha 31849). O `callback` recebe um `Result<IHandsController>` quando a operação de fato resolve — hoje o mod passa `(result) => { }`, um lambda vazio que descarta esse sinal (`BandAidController.cs:584`, antes da revisão da spec funcional).
- `Player.TrySetLastEquippedWeapon(bool equipFirstAvaliableOnFail = true, Callback callback = null)` — [`Player.cs:31800`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31800) — também delega para `TryProceed` (linha 31804) ou `SetFirstAvailableItem` (linha 31811), e o `callback` opcional recebe o mesmo tipo de resultado. Hoje o mod chama `doctor.TrySetLastEquippedWeapon(true)` (`BandAidController.cs:497`) sem passar `callback` — o parâmetro fica `null` e o sinal de conclusão nunca é usado.
- `Player.TryProceed(Item item, Callback<IHandsController> completeCallback, bool scheduled = true)` — [`Player.cs:32003`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32003) — é o pipeline comum por trás dos dois acima; despacha pra `Proceed(meds, ..., callback, ..., scheduled)` (linha 32085 pro caso `MedsItemClass`, que é o tipo usado por `HealRoutine`) ou equivalente por tipo de item. `completeCallback` é invocado quando essa operação específica termina (sucesso ou falha) — é o sinal de confirmação que faltava.

Ou seja: a "confirmação" que a spec funcional pede **já existe como parâmetro das próprias chamadas que o mod já faz** — não é preciso inventar um protocolo novo. O trabalho é parar de descartar esse parâmetro.

**Nota sobre `ForceFinishAnimation`:** `MedicHealPatch.ForceFinishAnimation()` (`MedicHealPatch.cs:270-287`, já existente) invoca `method_9` via reflexão **de forma síncrona** (`_method9Cached.Invoke(...)`, linha 278) — não passa por `TryProceed`/callback, então não tem o mesmo problema de assincronia que `SetInHands`/`TrySetLastEquippedWeapon` têm. `method_9` é o próprio `Player.MedsController.ObservedMedsControllerClass.method_9`, e o mod já tem um Postfix nele (`AnimCleanupPatch`, `MedicHealPatch.cs:527-571`, `TargetMethod()` resolve via `AccessTools.Inner`) usado hoje só pra resetar `IsRedirectingHeal` — não precisa de nova infraestrutura de detecção, já existe. Este item não muda `ForceFinishAnimation` nem `AnimCleanupPatch`.

**Alternativa descartada:** reescrever a sequência inteira num novo modelo de estado (máquina de estados explícita, ou um novo protocolo de handshake local) foi descartada — a spec funcional (revisão `/review-spec`) exige uma abordagem incremental e reversível dado o histórico de fragilidade dessa animação relatado pelo usuário. Usar os callbacks nativos que a API já fornece é a mudança de menor superfície possível que ainda resolve a causa (parar de assumir sucesso sem checar).

## 2. Pontos de patch

Nenhum ponto de patch novo no Assembly — a mudança é 100% em `mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs`, que já é código do mod (não decompilado). A tabela abaixo lista as chamadas ao Assembly cujo uso muda (não são patches, são call sites):

| Alvo (Assembly) | Tipo de uso | Motivo |
|---|---|---|
| [`Player.cs:31845`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31845) `SetInHands(Item, Callback<IHandsController>)` | Call site existente, callback trocado | Hoje ignora o resultado; passa a logar falha e sinalizar confirmação |
| [`Player.cs:31800`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31800) `TrySetLastEquippedWeapon(bool, Callback)` | Call site existente, callback adicionado | Hoje chama sem `callback` (fica `null`); passa a receber confirmação antes de dropar o item em `EmergencyDrop` |

## 3. Novas propriedades F12 (BepInEx)

N/A — esta correção não introduz nenhum `ConfigEntry` novo. As constantes de janela de segurança (§5) são `const`/`static readonly` no código, seguindo o mesmo padrão de `GraceWindowSeconds`/`HandsBookkeepingGraceWindowSeconds` do FIKA (não expostas no F12, marcadas como pendência de calibração se necessário).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded-V4/Patches/Medical/BandAidController.cs` | MODIFICAR | `HealRoutine`: usar o callback de `SetInHands` em vez de lambda vazio (loga se não confirmou dentro do `UseTime`). `EmergencyDrop`: passar um callback de log pra `TrySetLastEquippedWeapon` (hoje `null`), sem mudar o sequenciamento síncrono existente. |

Nenhum arquivo novo — sem novo Harmony patch, sem nova classe.

## 5. Stubs de código

> **Revisado após `/review-technical-spec` 01 (`PA-01-01`, `PA-01-02`) — aceitas as duas sugestões.** Os stubs abaixo não acessam mais nenhum membro de `result` (`Callback<T>`/`Result<T>` não são decompilados — ver §7 — então não dá pra assumir com confiança que têm `.Error`; detalhe de erro fica pra quando o compilador real confirmar os membros, no `/code-mod`). `EmergencyDrop` também deixou de virar coroutine: o drop do item continua **síncrono e imediato**, exatamente como hoje — só o callback de log foi adicionado, sem gatear nada nele. Isso elimina o risco de perder o item de cura se o `MonoBehaviour` for destruído em voo (`PA-01-02`).
>
> **Atualização pós `/code-review` 01 (`CR-01-01`) — código real difere destes stubs.** O `/code-mod` compilou os stubs abaixo exatamente como estavam, mas o `/code-review` confirmou por experimento de compilação que `result.Error` (string) existe de verdade nos dois callbacks — e restaurou essa checagem (achado `CR-01-01`, aplicado). Nesse processo também descobriu-se que **`Result<T>` é struct** (`result != null` não compila) e que um lambda implícito `(result) => {...}` sem referenciar membros de `T` deixa a inferência de tipo ambígua entre as várias sobrecargas de `SetInHands(Item, Callback<T>)` — a versão final usa delegates de **tipo explícito** (`Callback<IHandsController>` / `Callback` com `IResult`) em vez de lambda implícito. O código real e definitivo está no as-built (`023-...-05-asbuild.md`) e no arquivo `BandAidController.cs`; os stubs abaixo ficam como registro do desenho original, não como fonte de verdade atual.

> Trecho modificado de `HealRoutine` (`BandAidController.cs`, dentro do método existente — não é arquivo novo). Mantém `WaitForSeconds(totalUseTime)` como está (não regressar o tempo percebido de cura no caminho feliz) — a mudança é só logar quando a confirmação nativa não chegou a tempo, em vez de nunca saber disso.

```csharp
// BandAidController.cs — dentro de HealRoutine, substitui o trecho atual (linha ~582-589)

// Colocar o item médico nas mãos (aciona a animação visual).
// ref: Assembly-CSharp/EFT/Player.cs:31845 — SetInHands(Item, Callback<IHandsController>)
//      delega pra TryProceed (Player.cs:32003), cujo callback informa que a operação
//      resolveu — antes descartado com `(result) => { }`. Não inspeciona `result` (ver
//      nota acima) — só usa a própria invocação como sinal de "terminou".
bool setInHandsConfirmed = false;
try
{
    doctor.SetInHands(itemUsed, (result) => { setInHandsConfirmed = true; });
}
catch (Exception ex)
{
    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"SetInHands NullRef ignorado: {ex.Message}");
}

// Espera o tempo de uso do item (+2s para animação completar visualmente) — inalterado,
// não regride o tempo percebido de cura no caminho feliz (AC "sem regressão perceptível").
float totalUseTime = stats.UseTime * allyTimeMult + 2f;
yield return new WaitForSeconds(totalUseTime);

// Guard novo: antes esse resultado era descartado sem log nenhum. Se o callback nativo
// não confirmou nem depois do UseTime inteiro, é sinal de algo fora do comum — loga e
// segue igual (ForceFinishAnimation ainda roda, devolvendo a arma de qualquer jeito).
if (!setInHandsConfirmed)
{
    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
        "HealRoutine: SetInHands não confirmou dentro do UseTime — seguindo mesmo assim (antes isso era ignorado silenciosamente).");
}
```

> Trecho modificado de `EmergencyDrop` (`BandAidController.cs`, PASSO 3 do método existente) — só adiciona o callback de log em `TrySetLastEquippedWeapon`. O sequenciamento continua **idêntico** ao de hoje: `ThrowItem` roda logo em seguida, síncrono, sem esperar o callback.

```csharp
// BandAidController.cs — dentro de EmergencyDrop, PASSO 3 (substitui a linha ~497):
// hoje: doctor.TrySetLastEquippedWeapon(true); — sem callback, sinal descartado (fica null).

// ref: Assembly-CSharp/EFT/Player.cs:31800 — TrySetLastEquippedWeapon(bool, Callback)
//      delega pra TryProceed (Player.cs:31804/32003). O callback (antes null, nunca
//      invocado) agora só loga quando a confirmação chega — não inspeciona `result`
//      (mesma razão do stub de HealRoutine acima) e NÃO atrasa o PASSO 4 (ThrowItem):
//      o callback pode disparar antes ou depois do drop, é só diagnóstico.
try
{
    doctor.TrySetLastEquippedWeapon(true, (result) =>
    {
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
            "EmergencyDrop: TrySetLastEquippedWeapon confirmou conclusão (log assíncrono, não bloqueia o drop).");
    });
    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("EmergencyDrop: Mãos limpas e arma restaurada.");
}
catch (Exception ex)
{
    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"EmergencyDrop Limpar Mãos: {ex.Message}");
}

// PASSO 4 (inalterado, síncrono e imediato — igual a hoje):
if (savedItem != null)
{
    try
    {
        doctor.InventoryController.ThrowItem(savedItem);
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"Drop emergencial: {itemName} dropado.");
    }
    catch (Exception ex)
    {
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"Erro ao dropar {itemName}: {ex.Message}");
    }
}
```

Nenhuma coroutine nova, nenhum método novo — `EmergencyDrop` continua com exatamente o mesmo formato/assinatura, só ganhou um callback de log onde antes passava `null`.

## 6. Fluxo de dados

```
[HealRoutine, caminho feliz]
[A] doctor.SetInHands(itemUsed, callback) → [B] Player.TryProceed (Player.cs:32003)
    → [C] Proceed(meds, ..., callback, scheduled) (Player.cs:32085, ramo MedsItemClass)
    → [D] callback(Result<IHandsController>) dispara — HOJE: descartado. DEPOIS: seta
      setInHandsConfirmed = true (sem inspecionar o conteúdo de result).
[E] yield WaitForSeconds(totalUseTime) — inalterado, tempo de animação percebido igual.
[F] Se !setInHandsConfirmed a essa altura: loga aviso (antes era silêncio total).
[G] MedicHealPatch.ForceFinishAnimation() → method_9 via reflexão síncrona (MedicHealPatch.cs:278)
    → [H] AnimCleanupPatch.Postfix (MedicHealPatch.cs:541) já existente, reseta IsRedirectingHeal.

[EmergencyDrop, cancelamento — SEM coroutine, sequenciamento síncrono inalterado]
[A] ForceFinishAnimation() (síncrono, como já era)
[B] doctor.TrySetLastEquippedWeapon(true, callback) → Player.TryProceed (Player.cs:31804/32003)
    → [C] callback dispara em algum momento (antes ou depois de [D]) — HOJE: `callback=null`,
      nunca invocado. DEPOIS: só loga que confirmou, não afeta [D].
[D] InventoryController.ThrowItem(savedItem) — disparado IMEDIATAMENTE após [B], exatamente
    como hoje. [C] e [D] são independentes; não há mais gate nem espera entre eles.
```

A diferença central em ambos os fluxos: o passo do callback nativo já existia e já disparava — só não era **usado** nem logado. A versão revisada (pós `PA-01-01`/`PA-01-02`) usa esse sinal só pra observabilidade (logar quando não confirma), sem introduzir nenhuma espera nova no `EmergencyDrop` — o `HealRoutine` já tinha um `yield` no meio (o `WaitForSeconds` existente), então checar a flag ali não adiciona atraso nenhum. Nenhum novo mecanismo de rede, patch ou coroutine é criado; a mudança é estritamente de observabilidade sobre um sequenciamento que já existia.

## 7. Riscos e dependências

- **`AnimCleanupPatch` (`MedicHealPatch.cs:527-571`)** — Postfix já existente em `method_9`. Este item não o modifica, mas qualquer trabalho futuro que mexer em `ForceFinishAnimation`/`method_9` precisa considerar essa dependência (`IsCurrentInstance`/`_currentObservedMedsControllerClass`).
- **`HandsStateGuard.cs`** — guarda de entrada que impede iniciar uma NOVA cura enquanto as mãos já estão ocupadas com item de cura/comida. Não muda com este item, mas a spec funcional deixou como "a definir" se ele deveria também guardar contra reentrância da própria sequência (nova cura antes da anterior confirmar) — este item **não** resolve isso; se `HealRoutine` for chamado de novo antes de uma coroutine anterior confirmar, o comportamento é o mesmo de hoje (não pior, não corrigido).
- **FIKA (itens `003`/`004`/`006`)** — a tolerância de colisão de mãos do lado FIKA continua sendo a rede de segurança pra qualquer desalinhamento que sobrar entre client e host em coop; este item reduz a chance de o próprio ICM abrir uma janela de colisão desnecessária, não substitui aquela proteção.
- **`Callback<T>`/`Result<T>`/`Callback`** — tipos usados nas assinaturas de `SetInHands`/`TrySetLastEquippedWeapon`/`TryProceed`, mas **não fazem parte de `Assembly-CSharp.dll`** (não aparecem no `types-index.json` deste decompile) — são de uma assembly de suporte referenciada (`Comfort.dll`, já listada em `References/` do `.csproj` do mod). Risco reduzido após `PA-01-01`: os stubs de §5 não acessam mais nenhum membro de `result` (só usam a invocação do callback como sinal), então compilam independente da forma exata desses tipos. Detalhe de sucesso/erro (`.Error` ou equivalente) fica como melhoria futura, adicionada só depois que o compilador do `/code-mod` confirmar os membros reais.
- **Compatibilidade binária:** nenhuma mudança de assinatura pública do mod — `HealRoutine`/`EmergencyDrop` continuam com a mesma assinatura e o mesmo modelo de execução (síncrono/coroutine) que já tinham hoje. Após `PA-01-02`, `EmergencyDrop` não ganhou nenhuma coroutine nova — o único código novo é o callback de log passado pra `TrySetLastEquippedWeapon`.

## 8. Checklist de implementação

- [x] Em `HealRoutine`, trocar o lambda vazio de `SetInHands` pelo callback que seta `setInHandsConfirmed = true` (sem inspecionar `result`), e logar aviso se ainda não confirmou depois do `WaitForSeconds(totalUseTime)` existente.
- [x] Em `EmergencyDrop` (PASSO 3), passar um callback de log pra `TrySetLastEquippedWeapon` (hoje `null`) — sem extrair coroutine, sem mudar a ordem síncrona com o PASSO 4 (`ThrowItem`).
- [x] Confirmar por compilação real (`/code-mod`) que os stubs simplificados (sem acessar membros de `result`) compilam de primeira, dado que `Callback<T>`/`Result<T>` não são decompilados (ver §7). **Confirmado:** `dotnet build -c Release` em `modded-V4` — 0 erros, 0 avisos.
- [ ] Validar manualmente (sem latência) que o tempo de cura percebido não mudou — comparar antes/depois no caminho feliz. (pendente — requer teste in-game do usuário)
- [ ] Validar em raid Headless/coop com rajada de spawn de bot (cenário do item `007` do FIKA) que a cura completa sem travar mãos nem gerar log de colisão do lado FIKA. (pendente — requer teste in-game do usuário)
- [ ] Validar `EmergencyDrop` (cancelamento no meio da cura) sob a mesma condição de latência — arma volta, item dropa, sem travar. (pendente — requer teste in-game do usuário)

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | N/A | Este item não introduz nenhuma coroutine, patch ou hook novo — só variáveis locais dentro de métodos já existentes (`HealRoutine`, já uma coroutine gerenciada por `_activeHealCoroutine`; `EmergencyDrop`, síncrono, sem mudança de modelo de execução após `PA-01-02`). Nada de lifecycle novo pra cobrir. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Não é um patch novo; a sequência tocada só roda quando o médico é o `MainPlayer` local, per o Ownership Guard já existente e citado na spec funcional §Comportamento atual (`revisao-item-04-animacao-e-redirecionamento.md` §2). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | Nenhum novo alvo de patch — nenhum override a auditar. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | O próprio ponto do item: já usa a API canônica (`SetInHands`/`TrySetLastEquippedWeapon`) — a correção é passar a **consumir** o `callback` que essa API canônica já oferece, em vez de contornar com timer fixo. Side-effects mapeados em §6. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Ver spec funcional, critério "Estado entre raids: N/A" — sequenciamento local por cura, sem estado persistente. Após `PA-01-02`, `EmergencyDrop` continua 100% síncrono (sem coroutine nova) — o item de cura é sempre dropado no mesmo frame, igual a hoje, eliminando o risco de perda de item numa destruição de `MonoBehaviour` em voo que a coroutine original introduziria. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | N/A | Nenhum `ConfigEntry` novo (§3). |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | Não é um patch (Harmony) — não há recursão de patch envolvida. Reentrância de `HealRoutine`/`EmergencyDrop` (duas curas ao mesmo tempo) é tratada como corner case aberto na spec funcional (`HandsStateGuard`), não resolvida por este item — ver §7 Riscos. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ | `setInHandsConfirmed` é variável **local** de `HealRoutine`, capturada por closure de cada execução — cada cura tem seu próprio estado isolado, sem risco de "vazar" de uma cura pra outra. `EmergencyDrop` não tem mais flag nenhuma após `PA-01-02` (o callback só loga, não seta estado consultado depois). |
| 9 | Todo patch-point reconfirmado no `.cs` do dump (não só no recon); "não existe" conferido no `types-index.json`, nunca num grep vazio — AP-09 | ✅ | `SetInHands`/`TrySetLastEquippedWeapon`/`TryProceed` lidos diretamente em `Player.cs:31845/31800/32003` nesta sessão (decompile gerado 2026-09-12). `Callback<T>`/`Result<T>` conferidos como **ausentes** de `types-index.json` (não um grep vazio por acidente — são de assembly externa, `Comfort.dll`, já referenciada pelo `.csproj` do mod) — documentado em §7; após `PA-01-01`, os stubs não dependem mais de nenhum membro desses tipos. |
| 10 | Skill EFT usada como lever confirmada não-inerte (`SkillsSettings` ≠ `[]` no `globals.json`); se inerte, efeito entregue por patch direto — AP-10 | N/A | Não envolve skills de personagem. |
| 11 | Pacote FIKA próprio: envelope de comprimento + só `TryGet*` + flag `Valid`, campos resetados no `Deserialize`, envio só na main thread, registro por instância/evento (nunca `bool`), zero `UnregisterPacket`, airbag com throttle em todo callback — AP-11 | N/A | Este item não cria nem modifica nenhum `INetSerializable` — é puramente sequenciamento de chamadas locais de hands controller, sem tráfego de rede novo. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Spec técnica criada via `/create-technical-spec`. Decompile do Assembly-CSharp gerado nesta sessão (não existia antes) para confirmar `Player.SetInHands`/`TrySetLastEquippedWeapon`/`TryProceed` (`Player.cs:31845/31800/32003`) — achado central: os callbacks de confirmação já existem na API nativa e são descartados pelo mod hoje; a correção é usá-los, não inventar mecanismo novo. |
| 2026-09-12 | `/review-technical-spec` 01 encontrou 2 achados (`PA-01-01` 🔴, `PA-01-02` 🟡), ambos aceitos e aplicados nesta spec: stubs simplificados pra não acessar membros não confirmados de `result` (`Callback<T>`/`Result<T>` fora do decompile); `EmergencyDrop` deixou de virar coroutine — o drop do item de cura continua síncrono e imediato, eliminando o risco de perda de item numa destruição de `MonoBehaviour` em voo. §5/§6/§7/§8/§9 atualizados de acordo. |
