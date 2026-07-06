# 056 — Indicador de perk no peso · Code Review 01

**Mod:** CustomClasses
**Asbuild:** [056-perk-stat-indicators-05-asbuild.md](056-perk-stat-indicators-05-asbuild.md)
**Data:** 2026-07-03

> Análise do código do `/code-mod`. Compila 0/0. **Nenhum 🔴/🟠** — molde reusado do `SkillPanelPatch` (validado
> in-game). Achados = posição (verify-in-game) + micro-otimização.

## Resumo

> 🔴 0 · 🟠 0 · 🟡 2 · 🟢 1

| ID | Cat | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 | Posição do marcador (`preferredWidth` do peso) no 1º frame | `[ ]` Verificar in-game |
| CR-01-02 | C — Gap vs. spec | 🟡 | Marcador pode invadir o sub-painel estreito do peso | `[ ]` Verificar in-game |
| CR-01-03 | F — Melhoria | 🟢 | Marcador criado (e escondido) mesmo p/ classe sem Pack Mule | `[ ]` Aceito |

---

### CR-01-01 · B · 🟡 — verificar in-game
Posição usa `maxTmp.preferredWidth` (= PA-01-01). Se o `_maxValue` estiver vazio no 1º `method_0`, o marcador cola no
início até o próximo refresh (`Update` re-posiciona). Mesmo molde do `SkillPanelPatch`, que funciona in-game. Se
deslocar, ancorar por `rect` fixo. **Local:** [`WeightMarkerPatch.cs`](../../modded/Client/Patches/WeightMarkerPatch.cs).

### CR-01-02 · C · 🟡 — verificar in-game
O sub-painel do peso é mais estreito que a linha de skill; "▲ +30%" pode transbordar visualmente. Ajustar `MarkerGap`/
`fontSize` no gate se colidir.

### CR-01-03 · F · 🟢 — aceito
`GetOrCreateMarker` é chamado antes do gate final de classe → cria 1 GameObject (escondido) mesmo para classes sem Pack
Mule. Segue o molde do `SkillPanelPatch` (que recicla). O painel de saúde não recicla, então dá pra early-return antes
de criar; impacto = 1 objeto inerte por painel. Aceito (paridade com o molde).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-03 | Code review 01 — 0 🔴/🟠; 2 🟡 (posição, verify-in-game) + 1 🟢 |
