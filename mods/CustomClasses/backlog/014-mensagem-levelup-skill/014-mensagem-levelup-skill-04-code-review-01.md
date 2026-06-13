# 014 — Mensagem de level-up · Code Review 01

**Mod:** CustomClasses · **Asbuild:** [014-mensagem-levelup-skill-05-asbuild.md](014-mensagem-levelup-skill-05-asbuild.md) · **Data:** 2026-06-09

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 1 · 🟡 Médios: 1 · 🟢 Menores: 2

## CR-01-01 · B — Bug latente · 🟠 Forte · ✅ Resolvido em 2026-06-09

> **Resolução:** varredura exaustiva confirmou que a notificação **NÃO** passa por `DisplayMessageNotification(string)` — usa `DisplayNotification(new GClass2549(skill))`. O patch foi **refeito** para alvejar o **construtor do `GClass2549`** (resolvido por assinatura) e modificar o texto `String_0`. Caminho 100% confirmado (`LocalPlayer.OnSkillLevelChanged`). CR-01-02 (rich-text) segue a validar.

**A notificação pode não passar por `DisplayMessageNotification(string)`**

**Local:** [`SkillLevelUpNotificationPatch.cs`](../../modded/Client/Patches/SkillLevelUpNotificationPatch.cs)

**Problema:** herdado da PA-01-01 — a string da notificação não está em DLL nenhum, então não dá pra garantir o caminho. Se a notificação usar `DisplayNotification(objeto)` direto, o prefix não a alcança.

**Por que importa:** o flavor pode simplesmente não aparecer (sem crash).

**Mitigação no código:** degradação graciosa (vanilla intacta) + `LogDebug` do `message`. **Decisão:** validar in-game; se não reescrever, o log aponta o caminho e ajusto o alvo do patch.

## CR-01-02 · B — Bug latente · 🟡 Médio

**Rich-text `<color>` na notificação pode renderizar cru**

Se o TMP da notificação tiver `richText=false`, a tag `<color=…>` apareceria como texto. A maioria das notificações do EFT é TMP com rich-text ligado, mas **validar**. Se ocorrer, alternativa: usar `textColor` do `DisplayMessageNotification` (cor do texto inteiro) em vez de tag por palavra — mas perde o "só a palavra colorida".

## CR-01-03 · D — Escopo · 🟢 Menor

**Prefix roda para toda notificação de mensagem**

Aceitável: sai cedo se não casa `" skill leveled up to "`. Não é hot path (notificações não são por-frame).

## CR-01-04 · C — i18n · 🟢 Menor

**Marcador fixo em inglês**

Só funciona com o jogo em inglês (escopo do item). Documentado; i18n futuro centralizaria o marcador/keywords.

## Pontos sólidos
- Degradação graciosa + try/catch + log; idempotente (reescrita determinística do mesmo texto).
- Reusa `MultiplierFormat` (cores consistentes com 005/010) e `SkillMultipliers`.
- Mastering não afetado (marcador exige "skill").

## Histórico

| Data | Evento |
|---|---|
| 2026-06-09 | Code review 01 — 0 🔴; 1 🟠 (caminho da notificação, mitigado) + 1 🟡 (rich-text) + 2 🟢. |
