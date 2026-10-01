# 023 — Atraso na checagem de carregador (Magazine Check Delay) · As-Built

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional:** [023-atraso-checagem-carregador-01-spec.md](023-atraso-checagem-carregador-01-spec.md)
**Spec técnica:** [023-atraso-checagem-carregador-02-spec-tech.md](023-atraso-checagem-carregador-02-spec-tech.md)
**Última review técnica:** [023-atraso-checagem-carregador-03-spec-tech-review-01.md](023-atraso-checagem-carregador-03-spec-tech-review-01.md)
**Build inicial:** desconhecida — implementado ad-hoc fora do fluxo formal, entre **2026-09-10** (build 2.24.0, último estado com documentação formal completa neste mod) e **2026-09-22** (quando o usuário perguntou "o que mudou de 2.24 pra 2.25" e a feature foi descoberta já em produção, build 2.25.1).

> Documentação **pós-implementação**, gerada RETROATIVAMENTE via `/apply-code-review` (não `/code-mod` —
> não existia até este momento). Reflete o estado real do código já em produção antes desta auditoria
> formal (spec + spec técnica + reviews, todas de 2026-09-22).

## Arquivos alterados (build inicial, retroativo)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/MagCheckDelayPatch.cs` | Prefix em `EftBattleUIScreen.ShowAmmoDetails` — suprime exibição imediata, agenda Coroutine de atraso configurável. |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Plugin.cs` | 2 `ConfigEntry` novas (`_EnableMagCheckDelay`, `_MagCheckDelaySeconds`) + `SafeEnable("MagCheckDelayPatch", ...)`. |
| MODIFICADO | `mods/stancesAndCameraPositionSPT4.0.11/modded-testchannel/Patches/RaidLifecyclePatches.cs` | `GameWorldOnDestroyPatch.Postfix` passou a chamar `MagCheckDelayPatch.OnRaidEnd()`. |

## PA-NN-MM resolvidos durante o build

> Não aplicável — a implementação original não passou por spec técnica/review (é o motivo desta
> auditoria retroativa). Os 2 pontos da review técnica 01 (PA-01-01, PA-01-02) foram resolvidos
> **na própria spec técnica** (documentação), não no código — ver seção "Mudanças posteriores" abaixo
> pros achados de code-review que SIM tocam código.

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada.

### Rodada 1 (code-review 01, 2026-09-22)

3 achados aplicados (nenhum rejeitado) — ver [023-atraso-checagem-carregador-04-code-review-01.md](023-atraso-checagem-carregador-04-code-review-01.md):

| ID | Resumo | Arquivo(s) |
|---|---|---|
| CR-01-01 | `{ex.Message}` → `{ex}` (preserva stack trace) | `Patches/MagCheckDelayPatch.cs:88,130` |
| CR-01-02 | Tooltips corrigidos (mencionam carregador E câmara) | `Plugin.cs:357-374` |
| CR-01-03 | Nova subseção `Weapon Inspection` criada em `PROPRIEDADES.md` (gap pré-existente, não só deste item) | `PROPRIEDADES.md` |

Versão `2.25.1 → 2.25.2` (patch — fixes de documentação/logging, sem mudança de comportamento).

## Validação in-game (progresso)

- [x] **Duração do atraso testada e calibrada pelo usuário (2026-09-22):** `2.0s` (original, nunca
      calibrado) → **`1.0s`** ("acabei de testar e ficou bom"). Novo default aplicado em `Plugin.cs` e
      no fallback de `MagCheckDelayPatch.cs`. **1 dos 5 testes da seção "Validação pendente" (spec
      técnica §8) considerado coberto** por este teste (item 1: "checar carregador, confirmar atraso").
- [ ] Checar câmara, confirmar que TAMBÉM atrasa (valida o achado principal desta auditoria).
- [ ] Checar duas vezes rápido, confirmar substituição limpa.
- [ ] Trocar de arma durante o atraso, confirmar que nenhum painel aparece depois.
- [ ] Sair de raid com atraso pendente, confirmar que não vaza pra próxima raid.

> ⚠️ **Nota pro usuário:** mudar o DEFAULT no código só afeta instalações novas ou uma key ausente do
> `.cfg` — quem já tem o mod instalado com um `.cfg` salvo (valor `2.0` gravado) **não** recebe o novo
> default automaticamente (BepInEx casa por `(seção, chave)` literal). Se quiser `1.0` refletido na sua
> instalação atual, ajuste manualmente no F12 (ou apague a key `Magazine Check Delay Seconds` do
> `.cfg` pra ela recriar com o novo default).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-22 | `05-asbuild.md` criado retroativamente via `/apply-code-review`, documentando a implementação ad-hoc pré-existente (build 2.24.0 → 2.25.1) antes de aplicar os achados do code-review 01. |
| 2026-09-22 | Aplicação de 3 achados do code-review 01 (CR-01-01/02/03). Compilado — 0 erros/0 warnings. Versão `2.25.1 → 2.25.2`. |
| 2026-09-22 | Usuário testou in-game e pediu calibração do default: `2.0s → 1.0s`. Aplicado em `Plugin.cs` (ConfigEntry) e `MagCheckDelayPatch.cs` (fallback), mais `PROPRIEDADES.md`. Compilado — 0 erros/0 warnings. Versão `2.25.2 → 2.25.3`. |
