# 019 — Investigação técnica + gate de pré-implementação

> **Data:** 2026-07-19<br>
> **Status:** ✅ Aprovado (fatos confirmados via `ilspycmd` no Assembly real) — **veredito GO**<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [019-...-00-ideia](./019-checar-camara-ui-00-ideia.md), [010-manual-chambering](../010-manual-chambering/)<br>

---

Fundamenta a spec do item 019. Fonte de verdade: Assembly real do EFT
(`D:/SPT/EscapeFromTarkov_Data/Managed/Assembly-CSharp.dll`) via `ilspycmd 10.1.0.8386` — **não** o
`references/eft-decompiled/` (102 namespaces vazios, ver `reference_eft_decompile_incomplete`).

## Gate 1 — já existe pré-implementação? **NÃO** (nem vanilla, nem mods instalados)

**Mods instalados (varredura em `D:/SPT/BepInEx/plugins` + configs):** nenhum cobre "mostrar a bala da câmara ao
checar in-raid".
- **MoreCheckmarks** — checkmarks de quest/hideout/barter no inventário. ❌
- **MunitionsExpert** — colore ícones de munição por classe de armadura. ❌
- **UIFixes** (Tyfon) — a keybind "check a magazine" é do **menu de item no inventário** (examine/fold/turn), não o
  chamber check in-raid. ❌
- **MagCheckInterrupt** (ozen) — interrompe/desacelera a **animação** do check de carregador para recarregar antes;
  não mexe em UI de câmara. ❌
- BetterAmmoLoadingList / ContinuousLoadAmmo / LoadAmmoAnim — carregar munição no carregador. ❌

**Vanilla in-raid:** o `CheckChamber()` **NÃO** exibe a munição no HUD. Ele toca a animação de inspeção e marca a
câmara como "conhecida" — isso só revela a bala **depois, na tela de inspeção do inventário**, não durante a raid.
Ou seja, a feature pedida (ver o tipo da bala no HUD ao checar a câmara) **não existe no jogo**.

## Gate 2 — a UI do check-magazine é reutilizável? **SIM**

O painel on-screen do check-carregador é **totalmente reutilizável**. Cadeia (tudo em Assembly-CSharp):

- Evento **`Player.OnShowAmmoDetails`** — `event Action<int,int,int,string,bool>` = `(ammoCount, maxAmmoCount,
  mastering, details, foldingMechanimWeapon)` (`Player.cs:25504`).
- → `GamePlayerOwner.method_8` → `GInterface472.ShowAmmoDetails(...)` (campo `BattleUIScreenController` em
  `GamePlayerOwner`) → `EFT.UI.BattleUIScreen<,>.ShowAmmoDetails` → `AmmoCountPanel.GetAmmoCountByLevel(count,max,
  mastering)` monta a linha superior → **`EFT.UI.AmmoCountPanel.Show(string message, string details)`**
  (`AmmoCountPanel.cs:24`): `_ammoCount.text = message` (linha 1) e `_ammoDetails.text = details` (linha 2, o nome
  da munição; escondida se `details == null`).
- Precisão por skill (`GetAmmoCountByLevel`): level 0 = difuso, 1 = "Approx. N", ≥2 = exato. **`details` (nome da
  munição) é sempre exibido quando != null.** Para a câmara (1 bala) isso é irrelevante — sempre exibimos o tipo.

## Como o vanilla dispara (referência — é o que copiamos)

`FirearmController.GClass2037.CheckAmmo()` (`Player.cs:5754`) calcula a última cartucho do carregador e invoca:
```csharp
Player_0.OnShowAmmoDetails?.Invoke(count, maxCount, Mathf.Max(Profile.MagDrillsMastering, mag.CheckOverride),
    ammoItemClass != null ? GClass2348.Localized(ammoItemClass.Name) : null, Weapon_0 is RevolverItemClass || ...);
```

## Ler a munição da câmara (já fazemos no 010)

- `FirearmController.Weapon` (= `.Item`) → `EFT.InventoryLogic.Weapon`; `Weapon.HasChambers` (bool),
  `Weapon.ChamberAmmoCount` (int), `Weapon.Chambers` = `Slot[]` (`Weapon.cs:670`).
- **Bala viva:** `weapon.Chambers[0].ContainedItem is AmmoItemClass round && !round.IsUsed` (`IsUsed==false` =
  round vivo; `true` = estojo deflagrado — confirmado em `Weapon.cs:876`).
- **Nome:** `GClass2348.Localized(round.Name)` (mesmo helper do `CheckAmmo`).
- `Patches/ManualChamberingPatches.cs` já usa `Weapon.Chambers[].ContainedItem`, `ChamberAmmoCount`, `HasChambers`
  e obtém o Player via `Traverse.Create(fc).Field<Player>("_player").Value`.

## Ponto de plugue (veredito GO)

Harmony **`[PatchPostfix]`** em **`EFT.Player.FirearmController.CheckChamber()`** (`Player.cs:13902` — nível de
controller, cobre todas as operações). Guardas: `__result == true`, `weapon.HasChambers`, sem malfunction,
`player.IsYourPlayer && player.FirstPersonPointOfView`. Disparo da UI por reflexão do delegate (o evento é privado
para invocação externa — exatamente como o `CheckAmmo` faz internamente):
```csharp
Traverse.Create(player).Field("OnShowAmmoDetails")
        .GetValue<Action<int,int,int,string,bool>>()
        ?.Invoke(1, 1, 2, GClass2348.Localized(round.Name), false);  // "Full" + nome da bala
```
- **Caveat de texto:** `Invoke(1,1,2,name,false)` produz a linha superior "**Full**" (porque `1 >= maxCount-1=0`) +
  o nome da bala embaixo. Para um texto próprio ("Câmara: <bala>"), chamar **`AmmoCountPanel.Show(message, details)`**
  direto na instância do painel — pula o `GetAmmoCountByLevel`. **Decisão de spec.**
- **Revólver:** `CheckChamber()` já retorna `false` para `RevolverItemClass` — a feature vale para armas de câmara
  não-revólver (aceitável; revólver mostra as balas no tambor de outra forma).

## Riscos / notas para a spec

- **Fika:** UI **local** (só quem checa vê) → **sem pacote de rede** (ver `reference_fika_peer_effects_client_side`).
- **Interação com o 010 (Manual Chambering):** câmara vazia é o caso mais útil — a UI deve mostrar "vazia"
  corretamente. Confirmar que `ChamberAmmoCount==0` / `round.IsUsed` são lidos certo nesse cenário.
- **Ponto de decisão de produto (spec):** (1) texto nativo "Full"+nome vs custom "Câmara: <bala>"; (2) mostrar ou
  não quando vazia; (3) idioma do label; (4) toggle F12; (5) gate de skill (recomendação: **não** gatear — 1 bala,
  o jogador está olhando).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Investigação via `ilspycmd` no Assembly real (sub-agent). Gate 1 (pré-implementação): NÃO existe (vanilla nem mods). Gate 2 (UI reutilizável): SIM (`OnShowAmmoDetails`/`AmmoCountPanel`). Veredito **GO**; ponto de plugue = postfix em `FirearmController.CheckChamber()`. |
