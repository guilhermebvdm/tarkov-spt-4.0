# 014 — Mensagem de level-up de skill · As-Built

**Mod:** CustomClasses
**Spec funcional:** [014-mensagem-levelup-skill-01-spec.md](014-mensagem-levelup-skill-01-spec.md)
**Spec técnica:** [014-mensagem-levelup-skill-02-spec-tech.md](014-mensagem-levelup-skill-02-spec-tech.md)
**Review técnica:** [014-mensagem-levelup-skill-03-spec-tech-review-01.md](014-mensagem-levelup-skill-03-spec-tech-review-01.md)
**Build:** 2026-06-09

> Reescreve a notificação "X skill leveled up to Y" com `EASILY` (buff, verde) / `FINALLY` (debuff, vermelho) para skills com multiplicador da classe. Prefix em `NotificationManagerClass.DisplayMessageNotification` (o ponto de montagem do EFT é inacessível — locale por enum). Compilado **0 warn/err** (client 37.4 KB). **A validar in-game.**

## Arquivos alterados

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `modded/Client/Patches/SkillLevelUpNotificationPatch.cs` | prefix `DisplayMessageNotification(ref message)`; detecta `" skill leveled up to "`, mapeia `skillName→ESkillId`, e reescreve com `EASILY`/`FINALLY` colorido se houver multiplicador. |
| MODIFICADO | `modded/Client/Plugin.cs` | config `ShowLevelUpFlavor` (default true) + registra o patch. |
| MODIFICADO | `mods/CustomClasses/PROPRIEDADES.md` | `ShowLevelUpFlavor`. |

## Decisões implementadas

- **Ponto:** interceptar `DisplayMessageNotification(string)` (a string da notificação não existe em nenhum DLL → ponto de montagem inacessível). Reescrita in-place via `ref message`.
- **Mapeamento:** `Enum.TryParse<ESkillId>(skillName.Replace(" ",""), ignoreCase)` — display inglês ≈ enum. Sem match/sem multiplicador → vanilla intacta.
- **Texto:** buff → `… skill <verde>EASILY</verde> leveled up to N ;)`; debuff → `… skill <vermelho>FINALLY</vermelho> leveled up to N`. Cores = `MultiplierFormat.GreenHex/RedHex` (consistente com 005/010).
- **Diagnóstico (PA-01-01):** `LogDebug` do `message` quando detecta level-up sem flavor — para achar o caminho real se a notificação não passar por `DisplayMessageNotification`.

## A validar (playtest)

- Subir skill com **buff** → `EASILY` verde + ` ;)`; com **debuff** → `FINALLY` vermelho; sem multiplicador → vanilla.
- ⚠️ Confirmar que a notificação **passa por** `DisplayMessageNotification` (se não reescrever, habilitar LogDebug e ver o caminho real).
- ⚠️ Confirmar que a notificação renderiza **rich-text** (`<color>` não aparece cru).

## Mudanças posteriores

**2026-06-09 — ponto REAL encontrado (varredura exaustiva a pedido do usuário):** a 1ª versão interceptava `DisplayMessageNotification(string)` — **errado**: a notificação de skill usa `DisplayNotification(new GClass2549(skill))` (objeto), não a versão string. Varredura (2 agentes EFT/SPT + ilspycmd) confirmou o caminho: `LocalPlayer.OnSkillLevelChanged(skill)` → `DisplayNotification(new GClass2549(skill))`; o ctor do `GClass2549` monta `String_0 = Format(Localized("SkillLevelUpMessage"), Localized(skill.Id), skill.Level)`.
- **Correção:** o patch agora alveja o **construtor** do `GClass2549`, **resolvido por assinatura** (`NotificationAbstractClass` concreto + `ctor(AbstractSkillClass)`) — não pelo nome `GClassNNNN`. Postfix modifica o campo `String_0` (achado por tipo `string`) inserindo `EASILY`/`FINALLY` colorido. Mantém ícone/duração/estilo vanilla; usa `skill.Id` (ESkillId) direto (sem parsear nome).
- **Gym:** mesmo caminho confirmado (`HideoutPlayer : LocalPlayer`). Não é SPT (EFT vanilla client-side).
- **Robustez:** registrado só se `CanEnable` (tipo+campo resolvidos), senão `LogWarning` e desativa (sem crash).
- Recompilado **0 warn/err** (client 37.9 KB).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-09 | Build via fluxo SSD completo (spec → tech-spec → review → code-mod → compile). 0 warn/err. |
