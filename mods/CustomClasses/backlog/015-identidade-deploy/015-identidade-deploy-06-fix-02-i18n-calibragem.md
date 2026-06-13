# 015 · 06-fix-02 — i18n do nome da classe + calibragem proporcional dos ícones

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Tipo:** fix pontual do item 015 (toca também 008/010/011/013)

> Pedido do usuário (playtest): (1) **"nome e descrição da classe em inglês não está aplicado — revise toda a implementação"**; (2) **"calibre a proporção dos ícones em todas as telas — cada tela tem um tamanho de ícone diferente em relação à fonte; seja rigoroso, proporcional à fonte de cada tela"**. Decisões travadas: i18n **segue o idioma do jogo (EFT)**; **nome EN por classe**.

## 1. i18n — nome da classe segue o idioma do EFT

### Diagnóstico (varredura no Assembly + spt-source)
- **Nome da edition no launcher** = a **chave** do `profileTemplates` (`LauncherController.cs:48` `Editions = …Select(x => x.Key)`) → sempre PT, é o identificador (não localizável).
- **Descrição da edition** = resolvida **no servidor** (`LauncherController.cs:63` `serverLocalisationService.GetText(DescriptionLocaleKey)`) pelo **locale do servidor** — **não** pelo EFT. Limitação do launcher: não há como o mod fazê-la seguir o cliente.
- **Idioma do EFT (cliente)** = `LocaleManagerClass.LocaleManagerClass.String_0` (singleton, ex.: `"en"`, `"po"`, `"ru"`). **`"po"` = Português** (existe `locales/global/po.json`).
- **Nome da classe in-game** (menu 2ª linha + tooltip "This player is …") = `SkillMultipliers.ClassName`, que vinha da rota como a **edition (PT)** → nunca em inglês. **Esta é a parte corrigível e a que o usuário vê.**

### Solução
| Camada | Mudança |
|---|---|
| JSON | novo `displayName: { en, pt }` por classe (o `name`/chave continua PT) |
| `ClassDefinition` | campo `DisplayName` (LocalizedText) |
| `ClassVisualRegistry.Visual` | + `DisplayNameEn`, `DisplayNamePt`; `Set(...)` recebe ambos |
| `CustomClassesMod` | passa `def.DisplayName?.En/.Pt` ao registry |
| `SkillMultipliersResponse` / Router | expõe `classNameEn` + `classNamePt` (fallback ao edition/PT) |
| `SkillMultipliers` (client) | guarda en/pt; getter `ClassName` resolve pelo **idioma do EFT** |
| `UI/GameLocale.cs` (novo) | `IsPortuguese` ← `LocaleManagerClass.String_0` (`po`/`pt*` → pt, senão en) |

**Migração do seletor `Language` (F12, item 008):** removido. Todos os textos do mod (nome da classe, tooltip do multiplicador, botão SKILLS) passam a seguir o **idioma do EFT** via `GameLocale` (antes `Plugin.Lang`). O `enum Language` e o `Config.Bind("General","Language",…)` foram retirados do `Plugin.cs` — **breaking** (a entrada some do F12), aceitável (era um seletor manual agora redundante).

**Descrição (launcher):** mantida server-side (limitação). Para vê-la em inglês: configurar o **server locale = en** no SPT. Documentado em `PROPRIEDADES.md`.

### Nomes EN (mapa em `scripts/build-class-jsons.js` → `DISPLAY_NAME_EN`)
Combat Medic · Hunter · Rifleman · Scout · Stealth Operator · Armorer · Tactical Operator · Survivalist · Scavenger · Operations Manager · Streaker (Peladão).

## 2. Calibragem — ícone proporcional à fonte de CADA tela

**Causa:** `ApplyClassIcon` usava **px absoluto** (`ClassIconSize=40`) igual em todas as telas → como cada tela tem fonte de tamanho diferente, a proporção ícone:fonte ficava inconsistente (menu × OVERALL × deploy × confirmation).

**Fix:** o ícone passa a ser **proporcional ao `fontSize` do nome de cada tela**:
`iconPx = nameTMP.fontSize × ClassIconRatio` (com clamp 14..110 px). Como cada patch usa o `fontSize` do **seu** nome, a proporção ícone:fonte fica **idêntica** em todas as telas.

| Item | Mudança |
|---|---|
| `Plugin.cs` | `ClassIconSize` (px) → **`ClassIconRatio`** (float, default `1.35`, faixa `0.8..2.5`) — **breaking** (rename de key) |
| `ClassIdentityView` | novo `IconSizeFor(TextMeshProUGUI nameTmp)` → `clamp(fontSize × ratio)` |
| 4 patches | passam `IconSizeFor(<nameTMP da tela>)`: menu `nick`, OVERALL `_nicknameLabel`, deploy/chat `_specialLabel`, confirmation `_name` |

`DeployNameScale` (default **2.2**, faixa 1.0..3.5): escala o conjunto ícone+nome no deploy **preservando** a proporção (ambos escalam juntos via `localScale` do `ChatSpecialIcon`). Subido de 1.2→2.2 no playtest (estava pequeno). **Nota:** o BepInEx persiste o valor no `.cfg` — mudar só o default no código não atualiza instalações existentes; o `.cfg` foi editado junto.

## 3. Arquivos

| Ação | Path |
|---|---|
| CRIAR | `Client/UI/GameLocale.cs` (idioma do EFT) |
| MODIFICAR | `Server/{ClassDefinition,ClassVisualRegistry,CustomClassesMod,SkillMultipliersResponse,SkillMultipliersRouter}.cs` |
| MODIFICAR | `Client/SkillMultipliers.cs` (en/pt + getter por idioma), `Client/UI/ClassIdentityView.cs` (`IconSizeFor` + tooltip via GameLocale) |
| MODIFICAR | `Client/MultiplierFormat.cs`, `Client/Patches/SkillsNavButtonPatch.cs` (GameLocale) |
| MODIFICAR | `Client/Patches/{ChatSpecialIcon,PlayerModelWithStats,PlayerNamePanel,MenuClass}IdentityPatch.cs` (IconSizeFor) |
| MODIFICAR | `Client/Plugin.cs` (remove `Language`; `ClassIconSize`→`ClassIconRatio`) |
| MODIFICAR | `scripts/build-class-jsons.js` (`DISPLAY_NAME_EN` + emite `displayName`) + regenerar `config/classes/*.jsonc` |
| MODIFICAR | `PROPRIEDADES.md` |

## 4. Verificação (in-game)

Reiniciar **server + jogo**, perfil de classe:
- **EFT em inglês:** nome da classe (menu 2ª linha + tooltip) em **inglês** (ex.: "Streaker", "Scavenger"). EFT em **Português ("po")**: em PT.
- **Ícones:** mesma proporção ícone:fonte em **menu, OVERALL, deploy, confirmation** (ajuste fino via slider `ClassIconRatio`).
- **Sem** o seletor `Language` no F12 (removido). Tooltip do multiplicador + botão SKILLS seguem o idioma do EFT.
- Descrição no launcher: segue o **locale do servidor** (esperado).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-09 | i18n (nome via `displayName` + idioma do EFT; seletor `Language` removido) + calibragem proporcional do ícone (`ClassIconRatio`). Compilado 0 warn/err (client 39.9 KB, server 65.5 KB). A validar in-game. |
| 2026-06-09 | Playtest: ícones das telas OK; **deploy ainda pequeno** → `DeployNameScale` 1.2→**2.2** (faixa até 3.5), `.cfg` instalado editado junto. Recompilado 0 warn/err. |
| 2026-06-10 | Deploy 2.2→**3.0** (faixa até 4.0). **Gradiente do nome destoava do glow/EXP do menu** (glow/EXP = cor sólida da classe; nome = gradiente clareado 40%). Causa: `LayoutHelpers.UpdateTopGlowColor` usa AccentColor.rgb sólida; MO pinta nickname/EXP sólidos (`PlayerProfileFeaturesPatch:617/690`); nosso `ApplyGradient` clareava o topo 40%. Fix (escolha do usuário): **clareamento 0.4→0.15** em `ApplyGradient` (todas as telas) — nome quase na cor base, bate com o glow. Recompilado 0 warn/err. |
| 2026-06-10 | **Gradiente também nos ÍCONES** (pedido: aspecto premium tipo Unheard/EOD). Novo `UI/ClassIconGradient.cs` (`BaseMeshEffect` — Unity UI Image não tem gradiente nativo): silhueta branca × degradê vertical (topo clareado 0.15, base = cor da classe), espelhando os nomes. Aplicado via `ClassIdentityView.ApplyIconGradient` no `ApplyClassIcon` (menu/OVERALL/deploy/confirmation) + selo de Skills. `RevertIconGradient` desliga o efeito p/ células recicladas (outros jogadores). Recompilado 0 warn/err (client 42.5 KB). |
