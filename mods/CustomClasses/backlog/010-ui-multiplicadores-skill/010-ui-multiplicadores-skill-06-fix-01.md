# 010 — UI dos multiplicadores de skill · Fix 01

**Mod:** CustomClasses
**Data:** 2026-06-08
**Origem:** feedback de playtest do usuário (2 pontos).

> Correção pontual pós-fechamento do 010, a partir do teste in-game.

## Pontos do feedback

1. **Skills do Skills-Extended "beta":** `Field Medicine`/`First Aid` apareciam acinzentadas com "Not available in the current Beta version", mas com nosso marcador `+50%` em cima. **Causa raiz:** o **Skills-Extended não estava instalado** — essas skills são "beta" do EFT e só o SE as habilita. (Usuário instalou o SE depois.) Independente disso, o marcador não deveria aparecer em skill **bloqueada**.
2. **Marcador `±X%` sobre o nome:** em nomes longos (ex.: "Stress Resistance"), o `+30%` ficava **sobre** o fim do nome. Pedido: deslocar p/ a direita (~50-60px) para não cobrir o nome.

## Correções

| Arquivo | Mudança |
| --- | --- |
| `modded/Client/Patches/SkillPanelPatch.cs` | (1) Pula skills `Locked` (`SkillClass.Locked = buffs.Length == 0` → skill "beta"/não usável): `has = got && !Locked && IsActive`. (2) Marcador agora ancorado à esquerda do `_name` (pivot 0) e posicionado em `_name.preferredWidth + 20px` no refresh → fica **logo após o fim do texto do nome**, sem sobrepor (robusto a nomes curtos/longos). |
| `modded/Client/Patches/SkillIconBorderPatch.cs` | Pula skills `Locked` (não colore borda de skill beta/não usável): `has = got && !skill.Locked && IsActive`. |

## Verificação

- Recompilado **0 warn/err** (client 19.0 KB). Build em `mods/CustomClasses/builds/` + instalado no SPT.
- **Reiniciar o jogo** (plugin client). Esperado: o `±X%` aparece **após** o nome (sem cobrir); skills beta/locked não recebem marcador/borda; com o **Skills-Extended instalado**, `Field Medicine`/`First Aid` deixam de ser beta e recebem o `+50%` normalmente.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-08 | Fix 01 — guard `Locked` (ponto 1) + reposicionamento do marcador via `preferredWidth` (ponto 2). |
