# 014 — Mensagem de level-up de skill · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [014-mensagem-levelup-skill-01-spec.md](014-mensagem-levelup-skill-01-spec.md)
**Criado:** 2026-06-09

> Reescreve a notificação "X skill leveled up to Y" inserindo `EASILY`/`FINALLY` colorido quando a skill tem multiplicador da classe. Investigação via ilspycmd/strings.

## 1. Investigação (onde a notificação é montada)

- A chave de locale `SkillLevelUpMessage` = "{0} skill leveled up to {1}" existe **só nos assets de locale** do SPT (`references/spt-source/.../locales/global/*.json`).
- A string **não aparece em nenhum DLL** (Assembly-CSharp nem plugins SPT — busca ASCII e UTF-16). O EFT resolve o locale por enum/hash em runtime → **não há ponto de montagem patchável** de forma estável.
- **Saída:** interceptar a saída comum de notificações de texto — `NotificationManagerClass.DisplayMessageNotification(string message, …)` ([Assembly:523]) — e reescrever o `message` pelo padrão do texto. É onde toda notificação de mensagem passa (`DisplayMessageNotification` → `new GClass2551(message,…)` → `DisplayNotification`).

## 2. Estratégia

Prefix em `NotificationManagerClass.DisplayMessageNotification` com `ref string message`:
1. Procurar o marcador `" skill leveled up to "` no `message` (inglês — a notificação do jogo é em inglês).
2. Extrair `skillName` (antes) e `rest`/nível (depois).
3. Mapear `skillName` → `ESkillId` via `Enum.TryParse<ESkillId>(skillName.Replace(" ",""), ignoreCase:true)` (o display em inglês ≈ nome do enum; normaliza espaço/caixa).
4. `SkillMultipliers.TryGet(id, out factor)` + `MultiplierFormat.IsActive(factor)`.
5. Reescrever: `"{skillName} skill <color=HEX>KEYWORD</color> leveled up to {nível}{sufixo}"` — buff: `EASILY` verde + ` ;)`; debuff: `FINALLY` vermelho.
6. Qualquer falha/sem-match → não toca (notificação vanilla intacta).

## 2. Refs (Assembly-CSharp / mod)

| Símbolo | Origem |
|---|---|
| `NotificationManagerClass.DisplayMessageNotification(string,ENotificationDurationType,ENotificationIconType,Color?)` | Assembly:523 (static) |
| `EFT.ESkillId` (enum) | nome do enum ≈ display em inglês |
| `SkillMultipliers.TryGet(ESkillId,out float)` / `MultiplierFormat.{GreenHex,RedHex,IsActive}` | mod (itens 005/010) |

## 3. Nova config F12

| Seção | Nome | Tipo | Padrão | Tooltip (pt-BR) |
|---|---|---|---|---|
| `General` | `ShowLevelUpFlavor` | bool | `true` | Customiza a notificação de level-up (EASILY/FINALLY) das skills com multiplicador da classe. |

## 4. Arquivos

| Ação | Path | Resumo |
|---|---|---|
| CRIAR | `modded/Client/Patches/SkillLevelUpNotificationPatch.cs` | prefix `DisplayMessageNotification(ref message)`; detecta padrão + reescreve. |
| MODIFICAR | `modded/Client/Plugin.cs` | `ShowLevelUpFlavor` + registra o patch. |
| MODIFICAR | `mods/CustomClasses/PROPRIEDADES.md` | `ShowLevelUpFlavor`. |

## 5. Riscos

- **🟠 A notificação passa por `DisplayMessageNotification`?** Não confirmado 100% (a string não está nos DLLs). Se passar por `DisplayNotification(objeto)` direto, o prefix não a alcança → degrada gracioso (vanilla intacta, sem crash). **Validar in-game**; se não reescrever, adicionar `LogDebug` do `message` p/ achar o caminho real.
- **Nome localizado vs `ESkillId`:** em inglês ≈ enum (normalizando espaço/caixa). Skills com display muito diferente do enum não casam → não reescreve (aceitável).
- **i18n:** marcador em inglês; só funciona com o jogo em inglês (escopo do item). Centralizar se for i18n no futuro.
- **Hot path:** `DisplayMessageNotification` não é por-frame; o prefix sai cedo se não casar o marcador.
- **Lifecycle:** funciona em raid e hideout (mesma notificação); try/catch + log.

## 6. Checklist

- [ ] `SkillLevelUpNotificationPatch` (prefix + detecção + reescrita).
- [ ] `Plugin`: `ShowLevelUpFlavor` + registro.
- [ ] `PROPRIEDADES.md`.
- [ ] `/compile-mod` 0 warn/err.
- [ ] Playtest: subir skill com buff → `EASILY` verde + `;)`; com debuff → `FINALLY` vermelho; sem mult → vanilla.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-09 | Spec técnica. Ponto de montagem inacessível (locale por enum) → intercepta `DisplayMessageNotification`. |
