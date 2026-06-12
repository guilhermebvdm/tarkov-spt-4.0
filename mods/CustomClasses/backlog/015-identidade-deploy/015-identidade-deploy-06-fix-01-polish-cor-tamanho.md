# 015 · 06-fix-01 — Polish visual (cor/efeito + tamanho + AccentColor do MO)

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Tipo:** fix pontual do item 015 (tech-spec + review + as-built)

> Pedido do usuário (playtest do Peladão): tamanhos de ícone inconsistentes; EXP no menu não seguia a cor da classe; deploy pequeno; **cores/efeitos divergentes entre telas**. Plano aprovado em `~/.claude/plans/`. OVERALL = padrão.

## 1. Detalhamento CANÔNICO da cor/efeito (o que o usuário pediu para conferir)

| Elemento | Cor | Efeito |
|---|---|---|
| Nome do jogador (menu, OVERALL, deploy, confirmation) | `NameColor` da classe | **GRADIENTE** vertex (topo `Lerp(cor,branco,0.4)` → base `cor`) |
| Nome da classe (CLASSE menu + selo Skills) | `NameColor` | **GRADIENTE** vertex |
| Ícone (todas) | `NameColor` | **TINT** `icon.color` (silhueta branca → cor exata) + **tamanho absoluto** |
| EXP / top glow / botões / luzes (menu) | `NameColor` | **SÓLIDO** (via `AccentColor` do Menu-Overhaul) |
| Tooltip | `NameColor` | sólido |

**Causas das divergências (corrigidas):** (1) selo de Skills estava em gradiente e o resto sólido → **unificado em gradiente**; (2) menu mostrava o vermelho do MO no EXP/detalhes → **AccentColor = cor da classe**; (3) tint do ícone fiel (PNGs são silhuetas brancas) + tamanho agora **absoluto** (era escala relativa → tamanhos diferentes).

## 2. Pontos de patch / refs

| Símbolo | Origem | Uso |
|---|---|---|
| `ClassIdentityView.ApplyGradient(TMP, baseColor)` | mod | efeito canônico (Lerp 0.4) — todos os nomes |
| `ClassIdentityView.ApplyClassIcon(Image/ChatSpecialIcon, …, size)` | mod | sprite + tint + `sizeDelta`/`LayoutElement` absoluto |
| `MoxoPixel.MenuOverhaul.Utils.Settings.AccentColor` (`public static ConfigEntry<Color>`) | MO (`com.moxopixel.menuoverhaul`) | EXP `:690`, top glow `:217`, botões `:55` → `SettingChanged` recolore |
| `Chainloader.PluginInfos[GUID].Instance.GetType().Assembly` | BepInEx | resolve o assembly do MO p/ reflection |

## 3. Config F12 (alterada)

| Nome | Antes | Agora |
|---|---|---|
| `ClassIconScale` (escala 1.0–1.5) | removido | → **`ClassIconSize`** (px, default 40, 24–80) |
| `DeployNameScale` | default 1.2 (1.0–2.0) | default **1.5** (1.0–2.5) |

## 4. Arquivos

| Ação | Path |
|---|---|
| CRIAR | `UI/MenuOverhaulBridge.cs` (reflection no AccentColor do MO; guarda/restaura original) |
| MODIFICAR | `UI/ClassIdentityView.cs` (ApplyGradient Lerp 0.4 + overload; ApplyClassIcon tamanho absoluto; remove `BuildColoredName` morto) |
| MODIFICAR | `Patches/{ChatSpecialIcon,PlayerModelWithStats,PlayerNamePanel,MenuClass}IdentityPatch.cs` (gradiente unificado; menu = AccentColor + gradiente; reativar limpeza não-local no Chat) |
| MODIFICAR | `Plugin.cs` (`ClassIconSize`, `DeployNameScale` 1.5, restore no OnDestroy), `PROPRIEDADES.md` |

## 5. Review (riscos) — `/review-technical-spec`

- **🟡 Gradiente vertex no menu vs MO:** o MO seta `.color` (AccentColor); o vertex gradient vence o `.color` e é reaplicado a cada `Show`. Se o MO recriar o TMP, cai para sólido na cor da classe (AccentColor) — ainda correto. **Validar.**
- **🟠 Vazamento do gradiente em listas (chat/grupo):** reativada a limpeza `enableVertexGradient=false` + tint neutro no ramo **não-local** do `ChatSpecialIconPatch`. (Sprite em Default ainda pode vazar — deferido, jogo solo.)
- **🟡 Tamanho absoluto vs LayoutGroup:** setamos `sizeDelta` **e** `LayoutElement.preferred` → cobre os dois casos. Default 40px calibrado pela OVERALL; ajuste fino via slider.
- **🟡 AccentColor grava no `.cfg` do MO:** guardamos/restauramos o original (perfil vanilla / `ShowClassOnPlayerName` off / OnDestroy).
- **🟢 MO ausente:** `MenuOverhaulBridge` é no-op (sem crash).

## 6. Verificação

Reiniciar server+jogo, perfil de classe:
- **Menu:** nome + CLASSE com **gradiente** da cor da classe; **EXP/blur/detalhes** na cor da classe (AccentColor); ícone do tamanho consistente. Trocar p/ perfil vanilla → menu volta ao vermelho do MO.
- **OVERALL/deploy/confirmation:** nome com gradiente + ícone tingido **do mesmo tamanho** (slider `ClassIconSize`); deploy maior (`DeployNameScale` 1.5).
- **Selo de Skills:** gradiente (consistente com o resto).
- Outro jogador (coop) intacto; sem NRE.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-09 | Polish (06-fix): gradiente unificado + tamanho absoluto + AccentColor do MO. Compilado 0 warn/err (client 38.9 KB). A validar in-game. |
