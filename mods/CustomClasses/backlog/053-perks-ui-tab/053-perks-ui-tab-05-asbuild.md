# 053 — Painel Perks/Drawbacks na tela de Skills · As-built

> **Data:** 2026-06-24<br>
> **Status:** 🔵 Compila + instalado; pendente validação visual in-game<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [class-design.md](../../docs/class-design.md)<br>

---

## O que foi feito

| Arquivo (modded/Client) | Mudança |
|---|---|
| `PerksCatalog.cs` | + `BuildPanelText()` — título (classe + "Perks & Drawbacks") + lista com marcador `+`/`−`, perks verde / drawbacks vermelho (`MultiplierFormat`), bilíngue (`GameLocale`). Reusa `LocalEntries()`. |
| `Patches/SkillsPerksPanelPatch.cs` *(novo)* | Postfix em `SkillsAndMasteringScreen.Show` (3 params — mesmo hook do selo 012). Cria/atualiza (idempotente) um painel `CC_PerksPanel`: caixa escura translúcida (`Image`) + `VerticalLayoutGroup` + `ContentSizeFitter` + 1 `TextMeshProUGUI` (rich-text, word-wrap, largura 440). Fonte herdada da tela. Ancorado no **canto superior-direito** (evita o selo, que fica no topo-centro). |
| `PerksConfig.cs` | + `PerksPanelEnabled` (F12 "Perks — UI", default on). |
| `Plugin.cs` | `.Enable()` do patch. |

- Build **0 erros**, instalado. Client DLL 86528 bytes.
- **Decisão de escopo:** entregue como **PAINEL** (não uma aba de tab-control nativa do EFT — integrar no controle de abas do `SkillsAndMasteringScreen` é bem mais complexo). O painel entrega o valor (ver perks/drawbacks da classe na tela de Skills) reusando 100% a infra pronta. Uma aba real fica como refinamento.

## Pendente (gate visual in-game)
- [ ] Abrir a tela de Skills com uma classe do mod → painel aparece no canto sup.-direito com os perks (verde) e drawbacks (vermelho) da classe, bilíngue.
- [ ] Não sobrepõe o selo de identidade (topo-centro) nem o conteúdo essencial das skills. Se sobrepuser, ajustar `anchoredPosition` (hoje `-24, -90`) ou expor no F12.
- [ ] Classe vanilla/desconhecida → painel não aparece (BuildPanelText null).
- [ ] Toggle F12 `Skills-screen perks panel` liga/desliga.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-24 | Guilherme | Painel implementado (MVP) — postfix em SkillsAndMasteringScreen.Show, reusa PerksCatalog. Compila. |
