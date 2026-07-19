# 003 — Pernas (mancar N1/N2 + agachar involuntário) · Code Review 01

> **Data:** 2026-07-19<br>
> **Status:** ✅ Aprovado (todos os achados resolvidos em v1.3.1, commit `04855ff3`)<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [003-pernas-mancar-02-spec-tech.md](./003-pernas-mancar-02-spec-tech.md)<br>

---

Revisão da implementação v1.3.0 (commit `b061b606`). Registro retroativo: a rodada correu in-conversation (revisor de contexto limpo); este arquivo consolida os achados para o histórico SDD do item.

**Limpo em:** conformidade com a spec técnica (mapa linha→efeito, Remove+Add em `(ESpeedLimit)1000`, gate CanSprint tier-N2, espelhos Fika imunes) · aposentadoria do legado de pernas (D10 — decisão 10 da matriz: seed de `ImpactTimers`/`LegPenaltyTimers` removido) · coop (efeitos só em `IsOwnedHere`).

## Achados

### CR-01-01 · D — Arquitetura · 🟠 Forte
**Toggle placeholder carrega `false` gravado que não foi escolha do usuário.** A key `Legs Effects (item 003)` nasceu OFF como placeholder nas v1.2.x; o .cfg persistiu esse `false`, e na entrega o default ON não valeria para ninguém que já rodou o mod. **Fix (padrão rename-at-delivery):** key renomeada para `Legs Effects` (nasce ON para todos); `MigrateOrphanedConfigKeys()` DELETA a órfã sem copiar o valor + `Config.Save` (lição CR-03-01 da rodada 03 do heal: sem delete o BepInEx re-persiste a key morta). Padrão registrado no PROPRIEDADES para os placeholders dos itens 004/005/006/007. ✅ Resolvido.

### CR-01-02 · B — Bug latente · 🟡 Médio
**`_applied` sem higiene para player morto/destruído e troca de mundo.** Sweep podia logar "cap OFF" para morto e o bookkeeping vazava entre raids. **Fix:** `RemoveCapGuarded` pula morto/destruído (só bookkeeping); poda oportunista de entradas `GetLine == None` no Update ativo; `_trackedWorld` + `ReferenceEquals` espelhando o world-swap do motor. ✅ Resolvido.

### CR-01-03 · B — Bug latente · 🟡 Médio
**`BotCrouchDip` com pose já baixa consumia cooldown e agendava restore inútil.** **Fix:** NOOP com refund do cooldown via `TryGetOneShotDeadline` + `ReportOneShotCanceled`, sem restore agendado, log `bot dip NOOP (pose already low)`. ✅ Resolvido.

### CR-01-04 · B — Bug latente · 🟢 Menor
**`TryInvoluntaryCrouch` com `MovementContext` null retornava sem refund do cooldown.** **Fix:** refund antes do return (mesmo padrão do NOOP). ✅ Resolvido.

### CR-01-05 · B — Bug latente · 🟢 Menor
**`ResolveLadderType` casava GUID por `"ladders"` OU `"tarkin"`** — falso-positivo com outros mods do mesmo autor. **Fix:** match restrito a `"ladders"`. ✅ Resolvido.

## Resolução

Todos aplicados pelo implementador em 2026-07-19 · build 0 erros · v1.3.0 → **v1.3.1** · DLL implantada em `D:/SPT` · commit `04855ff3`.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Registro retroativo da rodada 1 (5 achados, todos ✅ resolvidos em v1.3.1). |
