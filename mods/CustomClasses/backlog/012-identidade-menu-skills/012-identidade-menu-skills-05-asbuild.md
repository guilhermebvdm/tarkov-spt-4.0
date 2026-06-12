# 012 — Identidade da classe no menu + tela de Skills · As-Built

**Mod:** CustomClasses
**Spec funcional:** [012-identidade-menu-skills-01-spec.md](012-identidade-menu-skills-01-spec.md)
**Spec técnica:** [012-identidade-menu-skills-02-spec-tech.md](012-identidade-menu-skills-02-spec-tech.md)
**Última review técnica:** [012-identidade-menu-skills-03-spec-tech-review-01.md](012-identidade-menu-skills-03-spec-tech-review-01.md)
**Build inicial:** 2026-06-08

> Client-only, reusa a base do 011 (`ClassIdentityView`/`ClassIconCache`/`SkillMultipliers`). Selo (ícone+nome colorido) no menu (integra Menu-Overhaul ou canto fixo) e no topo da tela de Skills. Compilado **0 warn/err** (client 23.5 KB). **Posições finas a ajustar no playtest.**

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `modded/Client/Patches/MenuClassIdentityPatch.cs` | postfix `MenuScreen.Show` (3 params); coroutine espera o painel do MO ou usa canto fixo. |
| CRIADO | `modded/Client/Patches/SkillsScreenIdentityPatch.cs` | postfix `SkillsAndMasteringScreen.Show` (3 params); selo no topo. |
| MODIFICADO | `modded/Client/UI/ClassIdentityView.cs` | + `ResolveColor(hex, fallback)`. |
| MODIFICADO | `modded/Client/Plugin.cs` | + `Instance` (coroutine host) + `ShowClassIdentity` + registra os 2 patches. |
| MODIFICADO | `mods/CustomClasses/PROPRIEDADES.md` | + `ShowClassIdentity`. |

## PA-NN-MM resolvidos durante o build

| ID | Categoria · Impacto | Resolução |
| --- | --- | --- |
| PA-01-01 | C · 🟡 | `GetTargetMethod` resolve o `Show` por **contagem de params (3)** via `AccessTools.GetDeclaredMethods` — evita ambiguidade do overload (e o tipo ofuscado `MatchmakerPlayerControllerClass`). Idem `SkillsAndMasteringScreen`. |
| PA-01-02 | D · 🟡 | Identidade gateada **só** por `Plugin.ShowClassIdentity` (independente do `ShowOnUi` do multiplicador). |
| PA-01-03 | B · 🟡 | Coroutine aborta se `menu == null` após a espera (menu fechado) antes de usar `menu.transform`. |
| PA-01-04 | B · 🟢 | `GameObject.Find("MainMenuPlayerModelView")` — sem MO/nome ausente → degrada p/ canto fixo (aceito). |

## Mudanças posteriores

**2026-06-08 — calibração pós-playtest (feedback do usuário):** o selo foi validado in-game (menu: "Médico de Combate" verde abaixo do EXP; Skills: topo). Ajustes:
- **F12 (seção "Class identity position"):** 4 floats `MenuClassPosX/Y` e `SkillsClassPosX/Y` para posicionar o selo sem recompilar (`Plugin.cs` + `PROPRIEDADES.md`).
- O selo recebe `LayoutElement.ignoreLayout = true` (sai do `VerticalLayoutGroup` do `BottomField` do MO) para o offset X/Y valer; ancoragem: menu com MO = centro-base do painel, sem MO = canto inferior-direito; **tela de Skills = topo-centro (centralizado horizontalmente)**.
- `modded/Client/icons/README.md` documentando o formato/edição dos PNGs (EN).
- Recompilado 0 warn/err (client 25.1 KB).

**2026-06-08 — gradiente no nome da classe (pedido do usuário):** `ClassIdentityView.ApplyGradient` aplica um **gradiente vertical TMP** (topo mais claro → base = cor da classe) no nome — usado no **menu e na tela de Skills**, e reutilizável pelo item 015 (deploy) para consistência. Recompilado 0 warn/err.

**2026-06-08 — posição X/Y em tempo real + slider (feedback: não atualizava):** as 4 posições viraram `ConfigEntry<float>` (lidas em tempo real, não capturadas no `Awake`); `AcceptableValueRange<float>(-1000,1000)` → o F12 renderiza como **slider** (barra de arrastar). `Plugin.RepositionSeals()` é chamado via `SettingChanged` de cada slider (padrão do Menu-Overhaul) e reaplica a `anchoredPosition` dos selos (`CC_ClassSeal_Menu`/`CC_ClassSeal_Skills`) achados por nome → move com o menu/tela aberto. Recompilado 0 warn/err (client 26.1 KB).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-08 | Build concluído via `/code-mod`. 0 warn/err. Selo no menu (MO/canto) + topo da tela de Skills. Posições a calibrar no playtest. |
