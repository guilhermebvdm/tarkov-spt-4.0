# PROPRIEDADES.md — CustomClasses (client F12 / BepInEx)

Plugin: `customclasses.mdj.client` ("CustomClasses") — ver [modded/Client/Plugin.cs](modded/Client/Plugin.cs).

Propriedades expostas no menu de configuração (F12 / ConfigurationManager). Nenhuma é **(Avançado)**.

## Seção `General`

| Nome (EN) | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `EnableSkillMultipliers` | Ativar multiplicadores de skill | bool | `true` | true/false | Liga/desliga a escala de ganho de XP de skill por classe (CustomClasses). |
| `ShowMultiplierOnSkills` | Mostrar multiplicador nas skills | bool | `true` | true/false | Mostra o destaque do multiplicador nas skills (borda colorida no ícone + seta ±X% ao lado do nome + tooltip da classe). |
| `ShowClassOnPlayerName` | Classe no nome do jogador | bool | `true` | true/false | Aplica o **gradiente** da cor da classe no nome + ícone da classe (tingido) + **tooltip** "This player is \<classe\>" (menu, character, deploy, confirmation). No **menu** mostra o nome da classe numa 2ª linha e usa a cor da classe no EXP/blur/detalhes (via AccentColor do Menu-Overhaul). (item 015) |
| `ShowClassIdentity` | Selo separado (Skills) | bool | `false` | true/false | Selo separado da classe (ícone+nome) no topo da **tela de Skills**. **Off por padrão**. (item 012) |
| `ShowSkillsButton` | Botão SKILLS no menu | bool | `true` | true/false | Adiciona um botão SKILLS no menu (abaixo de CHARACTER) que abre direto a aba Skills. (item 013) |
| `ShowLevelUpFlavor` | Mensagem de level-up | bool | `true` | true/false | Customiza a notificação "skill leveled up" com `EASILY` (buff, verde) / `FINALLY` (debuff, vermelho) nas skills com multiplicador da classe. (item 014) |

## Seção `Class identity position`

> Offsets (em px) do "selo" da classe na **tela de Skills** (só aparece com `ShowClassIdentity` ligado). **Sliders** (barra de arrastar, faixa −1000..1000) que aplicam **em tempo real** (com a tela aberta, arrastar move o selo na hora). *(O menu não usa selo separado — a identidade vai no próprio nome do jogador via `ShowClassOnPlayerName`.)*

| Nome (EN) | Tradução pt-BR | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| `SkillsClassPosX` | Skills — X | float | `0` | Selo na tela de Skills — offset horizontal a partir do centro (px). 0 = centralizado. |
| `SkillsClassPosY` | Skills — Y | float | `-20` | Selo na tela de Skills — offset vertical a partir do topo (px). Negativo = baixo. |
| `ClassIconRatio` | Proporção do ícone | float | `1.35` | Tamanho do ícone da classe como **múltiplo da fonte do nome de cada tela** (ícone = fonte × ratio). Mantém a proporção ícone:fonte **idêntica em todas as telas** (menu, character, deploy, confirmation), independente do tamanho da fonte. Faixa 0.8..2.5. (item 015 · 06-fix) |
| `DeployNameScale` | Escala no deploy | float | `3.0` | Tamanho do ícone+nome na tela de carregamento da raid (deploy) — **ícone e nome crescem juntos** (mesma proporção). 1.0 = original. Faixa 1.0..4.0. (item 015 · 06-fix-02) |

> Notas (i18n — item 008, revisado no 015·06-fix):
> - **Idioma automático:** os textos do mod in-game (nome da classe, tooltips, botão SKILLS) seguem o **idioma do EFT** (Settings → Language). `"po"` (Português) → pt; qualquer outro → inglês (fallback). O seletor `Language` (F12) foi **removido** — não é mais necessário.
> - **Nome da classe in-game:** vem do campo `displayName { en, pt }` no JSON da classe (menu/tooltip). O `name` (chave da edition) continua em PT.
> - **Descrição da edition no launcher:** resolvida no **servidor** (locale do servidor), não pelo EFT — é uma limitação do launcher do SPT. Para vê-la em inglês, configure o **server locale = en** (config do SPT). Os **nomes das edições** na tela de criação são a chave (PT).
> - Trocar o `.dll` do client exige **reiniciar o jogo** (plugin BepInEx).

## Histórico

| Data | Alteração |
|---|---|
| 2026-06-07 | Criado (item 008). Documenta `EnableSkillMultipliers`, `ShowMultiplierOnSkills` (itens 005/010) e `Language` (008). |
