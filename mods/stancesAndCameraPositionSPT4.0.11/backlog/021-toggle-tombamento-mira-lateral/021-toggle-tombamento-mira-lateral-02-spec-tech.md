# 021 — Toggle para tombamento de mira lateral · Spec Técnica

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [021-toggle-tombamento-mira-lateral-01-spec.md](021-toggle-tombamento-mira-lateral-01-spec.md)
**Criado:** 2026-09-08

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

**Memória consultada:** snapshot de 2026-08-30 (Sessão 16) · pendências que afetam: nenhuma diretamente ([P-13.1]/[P-13.3] abertas mas não relacionadas a este item).
**Docs técnicos lidos (gatilho disparado):** `spt-antipatterns.md` (obrigatório sempre) — AP-01 a AP-09 avaliados na §9. Nenhum outro doc de `docs/technical/` disparou (não é item/inventário, não é `INetSerializable`, não é mod novo, não é migração 3.x→4.0).
**Grafo:** `graphify-eft` (MCP) indisponível nesta sessão (`CONNECTION_CLOSED`) — fallback aplicado: Grep manual com a mesma disciplina do AP-03 (ver §9 check 3) para confirmar que `ApplyComplexRotation`/`ApplySimpleRotation` não têm overrides.

## 1. Estratégia

**Tipo:** Postfix (já existente — este item **modifica** o corpo de dois Postfix já aplicados pelo mod, não cria patch novo).

**Alvo:** `EFT.Animations.ProceduralWeaponAnimation.ApplyComplexRotation(float)` e `.ApplySimpleRotation(float)` — ambos métodos públicos **não-virtuais** ([ProceduralWeaponAnimation.cs:1749](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L1749) e [:1790](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L1790)). Confirmado via Grep em todo `references/eft-decompiled/Assembly-CSharp/` que não existe nenhuma outra declaração ou `override` desses dois nomes — AP-03 não se aplica (não há virtual dispatch a auditar).

**O que muda:** hoje, os dois Postfix (`modded/Patches/ApplyComplexRotationPatch.cs:192-397` e `modded/Patches/ApplySimpleRotationPatch.cs:126-217`) leem o campo `_targetScopeRotation` do nativo só para um guard de NaN, e nunca o usam no cálculo — a rotação final aplicada em `WeaponRootAnim.SetPositionAndRotation` é sempre `weapRotation * CurrentRotation` (`CurrentRotation` vem só do `StanceManager`, sistema de postura do mod). Isso descarta o tombamento nativo de miras em trilho lateral (canted/off-axis), que o próprio jogo calcula via `_scopeRotation` (campo já suavizado por `Quaternion.Lerp`, [ProceduralWeaponAnimation.cs:1774](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L1774) / [:1799](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L1799)).

A mudança: ler `_scopeRotation` (não `_targetScopeRotation` — ver §"Por que `_scopeRotation` e não `_targetScopeRotation`" abaixo) e, quando a nova opção do F12 estiver **desativada** (padrão), compor `weapRotation * scopeRotation * CurrentRotation` em vez de `weapRotation * CurrentRotation`. Quando a opção estiver **ativada**, usar `Quaternion.identity` no lugar de `scopeRotation` — resultado idêntico ao comportamento atual (nenhuma regressão para quem preferir a arma sempre reta).

**Por que `_scopeRotation` e não `_targetScopeRotation`:** o campo hoje lido pelo patch (`_targetScopeRotation`, [:278](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L278)) é o alvo **instantâneo** (sem transição) calculado por `method_24()` a partir de `SightNBone.Rotation` ([:2322-2342](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L2322-L2342)) sempre que a arma/mira muda — usá-lo diretamente causaria um "pop" ao entrar em ADS (sem a suavização nativa). O campo `_scopeRotation` ([:276](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L276)) é o valor **já interpolado por `Quaternion.Lerp`** a cada frame pelo próprio método nativo que acabamos de patchear (`_scopeRotation = Quaternion.Lerp(_scopeRotation, IsAiming ? _targetScopeRotation : Quaternion.identity, CameraSmoothTime * _aimingSpeed * dt)`, linha 1774/1799) — quando o Postfix roda, esse valor já reflete a transição suave do frame atual, incluindo o caso "não mirando" (`Quaternion.identity`), satisfazendo de graça o corner case "transição suave de ADS" (critério de aceite correspondente) sem nenhuma lógica adicional no mod.

**Por que isso não reintroduz o bug que motivou o abandono do `scopeRotation` (a marcação `<!-- review -->` da spec funcional):** o comentário existente no código (`ApplyComplexRotationPatch.cs:345`, idêntico em `ApplySimpleRotationPatch.cs:176`) alerta para nunca usar `scopeRotation.eulerAngles` — extrair Euler de um Quaternion perto de certos ângulos (gimbal) produz uma tripla que, alimentada na mola de interpolação por Euler do mod (`SpringLerpAngle`), diverge e vira a câmera (é exatamente o mecanismo documentado no comentário de `ClampAngles`, linha ~93-98 do mesmo arquivo, sobre o gimbal-flip da PRÓPRIA mola do mod). Este item **nunca chama `.eulerAngles`** em `scopeRotation` — ele só multiplica o `Quaternion` bruto (`weapRotation * scopeRotation * CurrentRotation`), operação que não tem singularidade de gimbal (multiplicação de quaternions é sempre bem-definida). O único risco residual é um `_targetScopeRotation` corrompido (NaN) na fonte, contaminando `_scopeRotation` via `Lerp` indefinidamente — e esse caso **já é coberto** pelo guard de NaN existente (`ApplyComplexRotationPatch.cs:231-235` / `ApplySimpleRotationPatch.cs:167-171`), que hoje testa exatamente esse campo (só precisa continuar apontando para `_scopeRotation` em vez de `_targetScopeRotation`, ver §5). **Resolução da marcação `<!-- review -->`:** nenhuma proteção adicional além do guard de NaN já existente é necessária — a composição por multiplicação de quaternion (nunca por Euler) é o que evita a classe de bug documentada.

**Plano B para a ordem de composição (ref: PA-01-01):** a ordem escolhida (`scopeRotation` antes de `CurrentRotation`, ver §5) segue a cadeia nativa por analogia, mas os dois fatores giram em eixos locais diferentes (`scopeRotation` = roll em Y; `CurrentRotation` = pitch em X / yaw em Z nas Stances 1-3 — ver memória do mod, "eixos são locais, não os do Unity") e multiplicação de quaternions em eixos diferentes não comuta. Se a validação in-game do item 8(b) do checklist (Stance customizada + mira lateral) mostrar uma pose incorreta, a alternativa a testar é inverter para `weapRotation * CurrentRotation * scopeRotation`.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT.Animations/ProceduralWeaponAnimation.cs:1749`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L1749) (`ApplyComplexRotation`) | Postfix (já existente — só o corpo muda) | Ponto onde o nativo aplicaria `_temporaryRotation * _scopeRotation`; o mod intercepta e substitui pela composição própria. |
| [`EFT.Animations/ProceduralWeaponAnimation.cs:1790`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L1790) (`ApplySimpleRotation`) | Postfix (já existente — só o corpo muda) | Caminho alternativo (qualidade gráfica reduzida) do mesmo mecanismo — CR-08 já registrado no arquivo exige que ele espelhe o Complex. |

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Stance Transition & Kick` | `Straighten Weapon On Canted Sights` | bool | `false` | — | — | Quando ativado, mantém a arma sempre reta mesmo ao mirar com uma mira montada em trilho lateral (mira inclinada/canted), ignorando o tombamento nativo do jogo para essa mira. Quando desativado (padrão), a arma tomba normalmente como no vanilla. |

Tooltip EN (par bilíngue, formato `"<EN>\n\n<pt>"` já usado no mod): "When enabled, keeps the weapon perfectly straight even when aiming with a sight mounted on a side rail (canted/off-axis sight), ignoring the game's native tilt for that sight. When disabled (default), the weapon tilts naturally like in vanilla."

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Plugin.cs` | MODIFICAR | Novo campo `ConfigEntry<bool> _FlattenCantedSightTilt` (linha ~99, junto às demais de "Stance Transition & Kick") + `Config.Bind` correspondente (após a bind de `_StanceOvershootDamping`, linha ~465), `Order = 94`. |
| `modded/Patches/ApplyComplexRotationPatch.cs` | MODIFICAR | `GetTargetMethod` (linha 181): repointar `_scopeRotationField` de `"_targetScopeRotation"` para `"_scopeRotation"`. `Postfix` (linha 386): compor `weapRotation * effectiveScopeRotation * CurrentRotation` em vez de `weapRotation * CurrentRotation`. |
| `modded/Patches/ApplySimpleRotationPatch.cs` | MODIFICAR | Mesma mudança (linha 116 e linha 205). |
| `mods/stancesAndCameraPositionSPT4.0.11/PROPRIEDADES.md` | MODIFICAR | Nova linha na tabela "Stance Transition & Kick" (ver §3). |

## 5. Stubs de código

> Os dois arquivos já existem por inteiro (ver §4) — os trechos abaixo mostram só os pontos que mudam, com contexto suficiente para localizar e compilar. `ApplySimpleRotationPatch.cs` recebe o mesmo par de mudanças (mesmos nomes de variável, mesma estrutura).

```csharp
// modded/Patches/ApplyComplexRotationPatch.cs — GetTargetMethod (era linha 181)
protected override MethodBase GetTargetMethod()
{
    // ref: Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs:276 — campo já suavizado por
    // Quaternion.Lerp dentro do próprio ApplyComplexRotation/ApplySimpleRotation (linha 1774/1799).
    // Antes apontava para "_targetScopeRotation" (linha 278 — alvo instantâneo, sem suavização),
    // usado só no guard de NaN e nunca na composição final.
    _scopeRotationField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_scopeRotation");
    _weapTempPositionField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_temporaryPosition");
    _weapTempRotationField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_temporaryRotation");
    _isAimingField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_isAiming");
    _currentRotationField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_cameraIdenity");
    _firearmControllerField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_firearmController");
    _aimingSpeedField = AccessTools.Field(typeof(EFT.Animations.ProceduralWeaponAnimation), "_aimingSpeed");

    return typeof(EFT.Animations.ProceduralWeaponAnimation).GetMethod("ApplyComplexRotation", BindingFlags.Instance | BindingFlags.Public);
}
```

```csharp
// modded/Patches/ApplyComplexRotationPatch.cs — trecho final do Postfix (era linha 384-386)
// scopeRotation aqui já é o campo "_scopeRotation" (ver GetTargetMethod acima) — suavizado pelo
// próprio nativo, nunca convertido para Euler (ver Estratégia §1 sobre por que isso evita o
// gimbal-flip que o comentário abaixo documenta).
// Item 021 — toggle F12 "Straighten Weapon On Canted Sights" (default false = preserva o tombamento nativo).
bool flattenCantedSights = Plugin._FlattenCantedSightTilt?.Value ?? false;
Quaternion effectiveScopeRotation = flattenCantedSights ? Quaternion.identity : scopeRotation;

Vector3 orientedPositionOffset = weapRotation * CurrentPosition;
__instance.HandsContainer.WeaponRootAnim.SetPositionAndRotation(
    weaponPosition + orientedPositionOffset,
    weapRotation * effectiveScopeRotation * CurrentRotation);
```

```csharp
// modded/Plugin.cs — novo campo (junto aos demais de "Stance Transition & Kick", ~linha 99)
public static ConfigEntry<bool> _FlattenCantedSightTilt;
```

```csharp
// modded/Plugin.cs — novo Config.Bind (logo após o bind de _StanceOvershootDamping, ~linha 465)
_FlattenCantedSightTilt = Config.Bind(
    GeneralSection,
    "Straighten Weapon On Canted Sights",
    false,
    new ConfigDescription(
        "When enabled, keeps the weapon perfectly straight even when aiming with a sight mounted on a side rail (canted/off-axis sight), ignoring the game's native tilt for that sight. When disabled (default), the weapon tilts naturally like in vanilla.\n\nQuando ativado, mantém a arma sempre reta mesmo ao mirar com uma mira montada em trilho lateral (mira inclinada/canted), ignorando o tombamento nativo do jogo para essa mira. Quando desativado (padrão), a arma tomba normalmente como no vanilla.",
        null,
        new ConfigurationManagerAttributes { Order = 94 }));
```

## 6. Fluxo de dados

```
[A] Jogador equipa arma com mira em trilho lateral e entra em ADS
        │
[B] Nativo: ProceduralWeaponAnimation.method_24() detecta SightNBone.Rotation acima do
    threshold e seta _targetScopeRotation = Quaternion.Euler(0, rotation, 0)
    (ProceduralWeaponAnimation.cs:2322-2342, threshold em EFTHardSettings.SCOPE_ROTATION_THRESHOLD)
        │
[C] Nativo: ApplyComplexRotation/ApplySimpleRotation suavizam a cada frame
    _scopeRotation = Quaternion.Lerp(_scopeRotation, IsAiming ? _targetScopeRotation : identity, ...)
    (ProceduralWeaponAnimation.cs:1774 / :1799) — roda ANTES do Postfix do mod (mesmo frame)
        │
[D] Postfix do mod (ApplyComplexRotationPatch.cs:192 / ApplySimpleRotationPatch.cs:126) lê
    _scopeRotation fresco via reflection (__instance), lê o toggle F12
    (Plugin._FlattenCantedSightTilt.Value) e decide effectiveScopeRotation
    (scopeRotation ou Quaternion.identity)
        │
[E] Postfix compõe weapRotation * effectiveScopeRotation * CurrentRotation
    (CurrentRotation = pose de postura do StanceManager, já existente) e aplica via
    WeaponRootAnim.SetPositionAndRotation (ApplyComplexRotationPatch.cs:386 / ApplySimpleRotationPatch.cs:205)
```

## 7. Riscos e dependências

- **Patches existentes que tocam `WeaponRootAnim` do jogador local:** só os dois alvos deste item (`ApplyComplexRotationPatch.cs`, `ApplySimpleRotationPatch.cs`). `PassiveMountDetectPatch.cs` também referencia `WeaponRootAnim`, mas só **lê** posição/direção para detecção de encosto — nunca chama `SetPositionAndRotation` (confirmado via Grep). Sem conflito.
- **Dependência do item 017** (waypoint/atenuação de ADS): quando o waypoint está ativo, `targetEuler`/`targetPosition` são forçados a zero (`ApplyComplexRotationPatch.cs:350-355`) — isso afeta só `CurrentRotation` (postura), não `scopeRotation`, que continua vindo direto do nativo já suavizado. Ou seja, o tombamento nativo continua aparecendo normalmente mesmo durante o waypoint — comportamento esperado pelo corner case correspondente da spec funcional.
- **Compatibilidade com outros mods:** nenhuma dependência nova. A leitura de `_scopeRotation` é a mesma técnica (reflection sobre `ProceduralWeaponAnimation`) já usada para todos os outros campos deste mesmo Postfix.
- **Ordem de inicialização:** nenhuma mudança — o novo `Config.Bind` entra na mesma seção/`Awake` já existente, sem reordenar seções (evita o breaking-change de renome documentado na memória do mod).
- **Troca de arma/mira (ref: PA-01-02):** `method_11()` chama `method_24()` ([ProceduralWeaponAnimation.cs:1551](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Animations/ProceduralWeaponAnimation.cs#L1551)) toda vez que `AvailableScopesChanged` dispara, recalculando `_targetScopeRotation` para a mira nova; `_scopeRotation` então faz `Lerp` até o novo alvo nos frames seguintes — o mesmo mecanismo de suavização documentado na §1, sem necessidade de lógica adicional no mod.
- **Hideout / estande de tiro (ref: PA-01-03):** sem guard de raid nestes dois Postfix hoje (nenhum `if (!Singleton<GameWorld>.Instantiated) return`); o `ProceduralWeaponAnimation` do jogador roda igual em hideout/estande de tiro, então o corner case "hideout" da spec funcional é herdado do comportamento pré-existente do mod, não precisa de tratamento novo.

## 8. Checklist de implementação

- [x] Adicionar `public static ConfigEntry<bool> _FlattenCantedSightTilt;` em `Plugin.cs` (~linha 99).
- [x] Adicionar o `Config.Bind` correspondente em `GeneralSection`, default `false`, `Order = 94`, tooltip bilíngue (§3/§5).
- [x] Em `ApplyComplexRotationPatch.GetTargetMethod()`: repointar `_scopeRotationField` para `"_scopeRotation"`.
- [x] Em `ApplyComplexRotationPatch.Postfix`: calcular `effectiveScopeRotation` e compor no `SetPositionAndRotation` final.
- [x] Repetir os 2 passos acima em `ApplySimpleRotationPatch.cs`.
- [x] Atualizar `PROPRIEDADES.md` (seção "Stance Transition & Kick") com a nova prop.
- [x] Compilar (`/compile-mod` ou `dotnet build`) — 0 erros/warnings novos.
- [ ] Validar in-game: (a) opção desativada + mira lateral + Stance 0 → arma tomba; (b) mesma combinação + Stance 1/2/3 → tomba E posiciona a postura; (c) opção ativada → idêntico ao comportamento anterior à mudança; (d) troca rápida de arma no meio do ADS; (e) hideout/estande de tiro.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Feature não introduz estado raid-scoped novo. `_scopeRotation` é lido fresco do `__instance` a cada frame (sem cache); o único estado novo é o `ConfigEntry` do BepInEx, que é intencionalmente persistente entre raids (é a natureza de uma opção do F12), não algo que precisa de teardown. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Guard já existente e inalterado: `ApplyComplexRotationPatch.cs:209` / `ApplySimpleRotationPatch.cs:142` (`if (player == null \|\| !player.IsYourPlayer) return;`), executado ANTES de qualquer código deste item. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | `ApplyComplexRotation`/`ApplySimpleRotation` são métodos públicos não-virtuais (`ProceduralWeaponAnimation.cs:1749`/`:1790`); Grep em todo `references/eft-decompiled/Assembly-CSharp/` confirma declaração única de cada nome — sem override a auditar. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Não é uma mutação nova: o mod já sobrescrevia `WeaponRootAnim.SetPositionAndRotation` antes deste item (padrão pré-existente, fora de escopo mudar aqui). Este item apenas passa a **ler** (nunca escrever) um campo (`_scopeRotation`) que o próprio nativo já calcula e usa no mesmo método, via seu próprio `Quaternion.Lerp` canônico (`:1774`/`:1799`) — nenhum side-effect novo é pulado além dos que o mod já pulava antes deste item. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Sem estado raid-scoped novo (ver check 1). O `ConfigEntry` persiste do jeito que toda opção do F12 já persiste — comportamento herdado, não introduzido aqui. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | Default explícito `false` = preserva tombamento nativo (pedido do usuário). `true` = comportamento idêntico ao pré-existente (arma sempre reta). Estado neutro (mira sem tombamento nativo, a maioria): `scopeRotation` já vem como `Quaternion.identity` do próprio nativo — nenhuma das duas posições do toggle muda nada nesse caso (corner case da spec funcional). |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` — AP-07 | N/A | Nenhuma chamada de volta ao método patcheado. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ | `scopeRotation` é lido do `__instance` (a PWA corrente) a cada frame via reflection — não há flag/cache que sobreviva a uma troca de arma para ficar stale (mesmo padrão já usado por `weapRotation`, `isAiming` etc. no mesmo Postfix). |
| 9 | Todo patch-point reconfirmado no `.cs` do dump; "não existe" nunca de grep vazio — AP-09 | ✅ | `_scopeRotation` (`:276`), `_targetScopeRotation` (`:278`), `ApplyComplexRotation`/`ApplySimpleRotation` (`:1749`/`:1790`), `method_24` + `SCOPE_ROTATION_THRESHOLD` (`:2322-2342`) — todos lidos diretamente no dump local (não vieram de recon/mapping). |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Feature não usa skill do EFT. |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Mod não declara `INetSerializable`; esta feature é puramente client-side/local (ver critério "Fika/multiplayer" da spec funcional). |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-08 | Spec técnica criada via `/create-technical-spec` |
