# 019 — Code Review · ChamberCheckAmmoPatch (v2.10.0)

> **Data:** 2026-07-19<br>
> **Status:** ✅ Aprovado (achados aplicados)<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [019-...-02-spec](./019-checar-camara-ui-02-spec.md)<br>

---

## Resumo

> 🔴 0 · 🟡 2 · 🟢 2 — review adversarial (sub-agent). Veredito: **patch correto e seguro**. Guards sólidos, Fika
> coberto (`IsYourPlayer` filtra bots e peers observados), toggle F12 live, tudo no `try/catch`.

## Achados

### CR-1 · 🟢 → ✅ Aplicado — abort silencioso se `OnShowAmmoDetails` sumir
`Traverse.Field("OnShowAmmoDetails")` retorna null se o backing field for renomeado/obfuscado num update do jogo →
`show == null` abortava sem rastro. Adicionado `LogWarning` **one-shot** (`_warnedNoEvent`) para o forense futuro.

### CR-2 · 🟢 → ✅ Aplicado — `foldingMechanimWeapon` hardcoded `false`
O vanilla calcula o 5º arg (`RevolverItemClass || Chambers.Length > 1`). Trocado por `weapon.Chambers.Length > 1`
(paridade cosmética de posicionamento do painel; revólver não chega aqui).

### CR-3 · 🟡 → ✅ Resolvido na spec — iteração de câmaras diverge do "MVP `Chambers[0]`"
O patch **itera** as câmaras e usa a 1ª bala viva (decisão de review, mais correta que `Chambers[0]` fixo — acha
bala em qualquer cano de double-barrel). Spec 02 atualizada para documentar a iteração + a limitação (o painel
arredonda `count >= max-1` para "Full", então 1-de-2 e 2-de-2 exibem "Full"). Código mantido; **não** é regressão.

### CR-4 · 🟡 → teste in-game prioritário — "Empty" depende de `CheckChamber()==true` com câmara vazia
Único pressuposto load-bearing do headline feature. Pelos fatos do Assembly, câmara vazia retorna `true` (não está
entre os casos de `false`). Registrado como **primeiro teste in-game** na spec 02 (§ Teste in-game prioritário).

## Verificado sem problema

- **Guards:** `__result` + `HasChambers` + `IsYourPlayer` + `FirstPersonPointOfView` sólidos; `MalfState` guard é
  redundante (malf já dá `__result==false`) mas defensivo e inofensivo — mantido.
- **Câmara esvaziada pelo item 010:** lê `Chambers[].ContainedItem` real (não o animator), então o truque visual
  `SetAmmoInChamber(1)` com slot vazio reporta "Empty" corretamente.
- **Estojo deflagrado** (`IsUsed==true`): filtrado → "Empty".
- **Fika:** bot e peer observado (`IsYourPlayer==false`) não disparam a UI no cliente local; sem pacote de rede.
- **Perf/reentrância:** roda por input (não por frame); invocar o delegate só atualiza texto do painel; nada
  escapa do `try`.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Code review adversarial (sub-agent) da v2.10.0. 0 🔴. 2 🟢 aplicados no código (log one-shot, folding param); 1 🟡 resolvido na spec (iteração de câmaras); 1 🟡 vira teste in-game prioritário ("Empty" load-bearing). Build 0/0. |
