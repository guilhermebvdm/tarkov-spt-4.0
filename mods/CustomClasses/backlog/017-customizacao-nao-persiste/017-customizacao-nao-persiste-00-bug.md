# 017 — BUG: customização (skin) da classe não persiste no perfil

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Tipo:** bug report / investigação (pré-spec) · **Prioridade:** 🔴 alta

> Reportado pelo usuário: "quando defino a skin/aparência da classe e saio do jogo, na próxima vez que abre ela volta para a default — nunca salva o preset no profile". Analisado o perfil **Peladon** (edition Peladão), criado hoje.

## Sintoma

Personagem de uma classe com `outfit` (skin) nasce/aparece com a **aparência default da facção**, não a skin definida na classe.

## Evidência coletada

| Camada | Estado | Esperado |
|---|---|---|
| **Recipe** (`class-recipes.js` peladao) | `outfit.usec.upper = 6847e663…` (Blue Hawaii shirt) | — |
| **DB de customização** (`templates/customization.json`) | `6847e663…` = `usec_kit_upper_hawaii_01`, `_props.Body = 6847e338…` | — |
| **Server log** (boot) | `Registered 'Peladão' (… outfit usec=2/bear=2 …)` → OutfitBuilder aplicou 2 peças/lado | aplicado ✅ |
| **Template em memória** (`GetProfileTemplates()[Peladão].usec.Character.Customization.Body`) | `6847e338…` (havaiana) — setado por `OutfitBuilder.cs:112` | havaiana ✅ |
| **Perfil salvo** (`user/profiles/<id>.json` → `characters.pmc.Customization.Body`) | **`5cde95d9…` = `DefaultUsecBody`** ❌ | `6847e338…` |

## Análise da causa-raiz (parcial)

O caminho server está **correto**:
- `OutfitBuilder.Apply` seta `customization.Body/Feet/Hands` no template + adiciona o suit a `side.Suits` ([OutfitBuilder.cs:112-125](../../modded/Server/OutfitBuilder.cs#L112)).
- `ProfileHelper.GetProfileTemplateForSide` lê de `databaseService.GetProfileTemplates()` (`ProfileHelper.cs:806`) — o mesmo dicionário que o mod modifica.
- `CreateProfileService` clona o template, faz `pmcData = template.Character` e **só sobrescreve `Customization.Head` e `Customization.Voice`** (`CreateProfileService.cs:58/61`) — **não toca em `Body/Feet/Hands`**.

→ Logo, o perfil **deveria** nascer com `Body = 6847e338…`. Como nasce com o **default**, o reset ocorre **depois** do `CreateProfileService`. Hipóteses a confirmar:

1. **EFT client / FIKA sincroniza a aparência com o SUIT equipado** (não com `Customization.Body` cru): `AddSuitsToProfile` marca o suit como **obtido**, mas não como **equipado** — então o cliente renderiza/salva o suit default. ⭐ hipótese principal (ambiente é **FIKA 2.3.0**).
2. O cliente envia um update de customização (default da facção) logo após criar o perfil, sobrescrevendo o `Body` no save.
3. `cloner.Clone` do template perde o campo `Customization.Body` (improvável — Feet/Hands idem).

## Próximos passos (investigação)

- [ ] Confirmar onde o `Body` é resetado: comparar o `Body` **logo após criar** (antes de entrar no jogo) vs **após 1ª sessão**. Se já nasce default → causa no client/criação; se vira default só depois → reset no load/save.
- [ ] Verificar como o suit "equipado" é representado no profile (campo `Inventory`/`Customization`/storage `suites`) e se é preciso **equipar** o suit (não só adicioná-lo a `Suits`).
- [ ] Checar fluxo de customização do **FIKA** (pode diferir do SPT puro).
- [ ] **Skills:** verificar separadamente se as skills iniciais da classe (ex.: `medicoDeCombate`) persistem — o usuário citou "skills" junto, mas o exemplo (Peladão) é `noBaseline` (sem skills). O `CreateProfileService` copia `Skills.Common` do template e não as reseta → provável que persistam; confirmar com um perfil de classe com skills.

## Refs

- [OutfitBuilder.cs](../../modded/Server/OutfitBuilder.cs) · [CustomClassesMod.cs:169-175](../../modded/Server/CustomClassesMod.cs#L169) (chama o OutfitBuilder)
- spt-source: `CreateProfileService.cs:44-61/134` · `ProfileHelper.cs:804-816`
- Perfil analisado: `user/profiles/6a28a1e32a52bab18860f7a5.json` (Peladon, Usec, edition Peladão)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-09 | Bug reportado + investigação inicial. Confirmado: template tem a skin, perfil salvo tem default. Reset ocorre após o `CreateProfileService` (provável client/FIKA via suit não-equipado). |
| 2026-06-09 | Confirmado que o `Body` havaiano (`6847e338` = `usec_upper_hawaii_01`, BodyPart=Body) é **válido** (idêntico em estrutura ao DefaultUsecBody) — não é valor inválido. `AddSuitsToProfile` só **desbloqueia** (`CustomisationUnlocks`), não muda aparência. **Teste decisivo aplicado:** perfil Peladon editado com `Body/Feet/Hands` havaianos (backup `.bak-skin-test`). |
| 2026-06-10 | **CAUSA-RAIZ CONFIRMADA — bug do SPT core (não do CustomClasses).** Após gravar a havaiana com tudo fechado e reabrir, o perfil voltou para `DEFAULT-USEC` em Body/Hands/Feet. O usuário confirmou que afeta **qualquer** skin (já ocorria antes do mod). Causa: `ProfileFixerService.CheckForAndFixPmcProfileIssues` (roda em `/client/game/start`) tem a checagem de **Body/Hands/Feet com a lógica invertida** — `if (customizationDb.ContainsKey(x))` (sem `!`) → reseta toda peça **válida** para o default. Só o Head está correto (`!ContainsKey`). SPT 4.0.13 / `compatibleTarkovVersion 0.16.9`. |
| 2026-06-10 | **FIX entregue em mod server dedicado** [`mods/CustomizationPersistenceFix`](../../../CustomizationPersistenceFix/) (decisão do usuário: mod separado, pois é bug geral do SPT). Patch Harmony Prefix/Postfix preserva Body/Hands/Feet válidos no load. Compilado 0 warn/err. A validar in-game. **Este item (017) está resolvido pelo mod externo** — a skin do Peladão (e qualquer skin) deve passar a persistir. |
