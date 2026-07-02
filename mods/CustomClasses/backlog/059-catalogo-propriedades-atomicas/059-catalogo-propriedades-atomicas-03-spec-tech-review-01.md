# 059 — Catálogo de propriedades atômicas + fix da aba CLASS · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [059-catalogo-propriedades-atomicas-02-spec-tech.md](059-catalogo-propriedades-atomicas-02-spec-tech.md)
**Data:** 2026-07-02

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 1 · 🟢 Menores: 3 · ✅ Resolvidos: 4 · Total: 4

**Refs conferidas:** `Tab.cs` 17/20/26/37/61/147 e `MultiplierFormat.cs` 21/11-12 batem com o Assembly/mod. Conformidade §9: 5 ✅ + 3 N/A justificados (sem ❌). **Nenhum bloqueador** → pode seguir pro `/code-mod`. Os 4 pontos (1🟡 + 3🟢) foram **resolvidos na spec** nesta rodada (autonomia).

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟡 Importante | Falta tabela de polaridade por propriedade | ✅ Resolvido |
| PA-01-02 | C — Lógica (clareza) | 🟢 Menor | Stub do `ValueToken` usa `2f − m` obscuro | ✅ Resolvido |
| PA-01-03 | B — Edge Case | 🟢 Menor | Perks condicionais (Adrenaline — janela de combate) | ✅ Resolvido |
| PA-01-04 | A — Gap | 🟢 Menor | Notificação compacta: grupo `AllPending` (deferido) | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

---

## Pontos

### PA-01-01 · A — Gap · 🟡 Importante — ✅ Resolvido em 2026-07-02

**Falta tabela de polaridade por propriedade**

**Problema:** a derivação perk/drawback depende da `Polarity` (HigherBetter vs LowerBetter) de **cada**
propriedade, mas a spec só mostra alguns exemplos no stub. Sem uma lista canônica, o `/code-mod` pode atribuir
polaridade errada (ex.: tratar "recuo" como HigherBetter) → efeito classificado invertido (perk↔drawback).

**Por que importa:** classificação invertida quebra o critério de aceite central ("−15% damage taken" = perk).

**Sugestão:** adicionar uma **tabela de polaridade** na §2b/§6 da spec técnica mapeando cada propriedade →
HigherBetter/LowerBetter (speed/carry/ergo/melee/breath/draw = Higher; damage-taken/recoil/hunger/aim-punch/
noise/ADS-time/inertia/move-while-ADS = Lower).

**Decisão:** `[x]` Aceitar sugestão
**Resolução:** tabela de polaridade adicionada à spec técnica (§6, "Polaridade por propriedade").

---

### PA-01-02 · C — Erro de Lógica (clareza) · 🟢 Menor — ✅ Resolvido em 2026-07-02

**Stub do `ValueToken` usa `2f − m` (obscuro)**

**Problema:** no stub, a magnitude percentual é `Percent(m > 1 ? m : 2f − m)` — correto mas críptico.

**Por que importa:** legibilidade/manutenção; risco de o dev copiar a "gambiarra".

**Sugestão:** trocar por `Mathf.RoundToInt(Mathf.Abs(m − 1f) · 100f)` + o sinal por `m > 1 ? "+" : "−"`.

**Decisão:** `[x]` Aceitar sugestão
**Resolução:** stub do `ValueToken` reescrito com `Mathf.Abs` na spec técnica.

---

### PA-01-03 · B — Edge Case · 🟢 Menor — ✅ Resolvido em 2026-07-02

**Perks condicionais (Adrenaline — janela de combate)**

**Problema:** a Adrenalina só vale numa janela de combate (25s). O modelo `PerkLine` não tem campo de condição;
a spec não diz como exibir isso.

**Por que importa:** exibir "−30% recoil" sem o "(na janela de combate)" engana o jogador.

**Sugestão:** embutir o qualificador no `LabelEn/Pt` da linha (ex.: `recoil (combat window)` / `recuo (janela de
combate)`) — sem novo campo. Registrar como convenção na §2b.

**Decisão:** `[x]` Aceitar sugestão
**Resolução:** convenção "qualificador no Label" documentada na §2b da spec técnica.

---

### PA-01-04 · A — Gap · 🟢 Menor — ✅ Resolvido em 2026-07-02

**Notificação compacta: grupo `AllPending` (deferido)**

**Problema:** na notificação compacta (1 linha/grupo), não estava definido se um grupo todo-deferido (ex.: Combat
Medic) aparece.

**Por que importa:** consistência com o painel (que mostra "em breve").

**Sugestão:** a notificação **lista o grupo normalmente** (nome colorido por `IsPerk`); o marcador "em breve" fica
**só no painel** (a notificação é resumo). Documentar na §6.

**Decisão:** `[x]` Aceitar sugestão
**Resolução:** comportamento documentado na §6 da spec técnica (notificação lista todos os grupos; "em breve" só no painel).

---

## Histórico

| Data | Evento |
|---|---|
| 2026-07-02 | Review técnica 01 criada via `/review-technical-spec` — 0 🔴, 1 🟡, 3 🟢; todos resolvidos na spec |
