# PROPRIEDADES.md — CustomClasses (client F12 / BepInEx)

Plugin: `customclasses.mdj.client` ("CustomClasses") — ver [modded/Client/Plugin.cs](modded/Client/Plugin.cs).

Propriedades expostas no menu de configuração (F12 / ConfigurationManager). Nenhuma é **(Avançado)**.

## Seção `General`

| Nome (EN) | Tradução pt-BR | Tipo | Padrão | Faixa | Tooltip (pt-BR) |
|---|---|---|---|---|---|
| `EnableSkillMultipliers` | Ativar multiplicadores de skill | bool | `true` | true/false | Liga/desliga a escala de ganho de XP de skill por classe (CustomClasses). |
| `ShowMultiplierOnSkills` | Mostrar multiplicador nas skills | bool | `true` | true/false | Mostra o destaque do multiplicador nas skills (borda colorida no ícone + seta ±X% ao lado do nome + tooltip da classe). |
| `Language` | Idioma | enum (`English` / `Portugues`) | `English` | English / Portugues | Idioma dos textos do mod na tela (tooltip dos multiplicadores). / Language of the mod's in-game texts. |

> Notas:
> - `Language` (item 008) afeta **só os textos renderizados pelo mod in-game** (o tooltip do multiplicador). A **descrição da edition no launcher** segue a língua do **servidor** (não este seletor). Os nomes das edições são a chave (sem tradução).
> - Trocar `Language` aplica na próxima vez que a tela de Skills/o tooltip é montado (não precisa reiniciar o jogo).
> - Trocar o `.dll` do client exige **reiniciar o jogo** (plugin BepInEx).

## Histórico

| Data | Alteração |
|---|---|
| 2026-06-07 | Criado (item 008). Documenta `EnableSkillMultipliers`, `ShowMultiplierOnSkills` (itens 005/010) e `Language` (008). |
