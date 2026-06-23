# 014 — Sync de stances Fika · Spec Tech Review 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica:** [014-sync-stances-fika-02-spec-tech.md](014-sync-stances-fika-02-spec-tech.md)
**Data:** 2026-06-22

> Análise crítica adversarial. Refs do stub verificadas no Assembly/mod. IDs `PA-01-MM` permanentes.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 1 · Total: 3

Memória: snapshot Sessão 5 (desatualizada — não cobre 011-014) · sem pendência registrada sobre o sync 006. **Fatos dos stubs confirmados:** `SpringLerpAngle`/`SpringLerp` têm a assinatura exata (ApplyComplexRotationPatch.cs:70,101 — `private`, virar `public`); `GetTargetPosition` usa `_cachedStanceXPosition` (StanceManager.cs:813+). Sem 🔴 — pode iniciar `/code-mod`; os 🟡 são validações in-game inerentes (não craváveis no papel).

## Índice

| ID | Cat | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B | 🟡 | Timing: Postfix de `ApplyComplexRotation` × cópia para `PlayerBones.Offset` | ✅ Aceito (validar in-game; plano B na §7) |
| PA-01-02 | B | 🟡 | Coexistência lean/ombro depende da pose em `weapRotation` no Postfix | ✅ Aceito (validar in-game) |
| PA-01-03 | A | 🟢 | Edição de `RaidLifecyclePatches` é desnecessária (sem dict estático) | ✅ Resolvido (removido do escopo §4/§8) |

---

### PA-01-01 · B — Edge Case · 🟡 Importante

**O fix depende de o Postfix de `ApplyComplexRotation` rodar ANTES de o Fika copiar `WeaponRootAnim` → `PlayerBones.Offset/DeltaRotation`**

**Problema:** a renderização da arma do observed usa `PlayerBones.Offset/DeltaRotation`, copiados de `WeaponRootAnim` em [ObservedPlayer.cs:1852](../../../../references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs#L1852). Se a cópia ocorrer **antes** do nosso Postfix, o offset escrito no `WeaponRootAnim` não chega à renderização. O diagnóstico (sub-agent 3) indica que `ApplyComplexRotation` roda **dentro** de `ProcessEffectors` (chamado em :1851, antes da cópia :1852), o que valida a ordem — mas é um fato de runtime.

**Por que importa:** se a ordem for inversa, o sync continua sem efeito visível (mesmo sintoma).

**Sugestão:** manter a abordagem (alta probabilidade de funcionar) e **validar com 2 clientes**. Plano B já documentado (§7): aplicar direto em `PlayerBones.Offset/DeltaRotation`, ou mover o Postfix para `ProcessEffectors`. Marcar como assunção a confirmar in-game.

**Decisão:** `[x]` Aceitar (validar in-game; plano B pronto) · `[ ]` Caminho alternativo

---

### PA-01-02 · B — Edge Case · 🟡 Importante

**A coexistência stance + lean + troca de ombro pressupõe que `weaponPosition`/`weapRotation` já contenham a pose nativa no momento do Postfix**

**Problema:** o offset de stance é aplicado por cima de `weapRotation` (mesma fórmula do local, ApplyComplexRotationPatch.cs:280). A coexistência com lean/ombro vem de o lean ser uma camada de **corpo/câmera** (não da rotação local da arma) e a stance ser um offset **da arma** — camadas independentes que se somam, como no jogador local. Mas isso precisa ser confirmado visualmente nas combinações da spec funcional.

**Por que importa:** é o critério central de aceite (stance + lean + ombro sem conflito).

**Sugestão:** validar in-game as combinações (Low Ready + lean esq/dir; High Ready + troca de ombro; sequências). Se houver conflito, ajustar a ordem de composição (ex.: aplicar o offset relativo ao bone de braço em vez do root). Nenhuma mudança de spec necessária agora.

**Decisão:** `[x]` Aceitar (validar in-game) · `[ ]` Caminho alternativo

---

### PA-01-03 · A — Gap · 🟢 Menor

**A edição de `RaidLifecyclePatches` (limpar dict) não se aplica — o estado vive nos components**

**Problema:** o §4 lista uma edição defensiva em `RaidLifecyclePatches` para "limpar dict estático", mas a arquitetura **não tem** dict estático: o estado (stance + spring) vive no `ObservedStanceAnimator`, que é component do GameObject do observed player e é destruído no despawn/fim de raid. Não há órfão a limpar.

**Por que importa:** edição desnecessária = ruído.

**Sugestão:** remover `RaidLifecyclePatches` da tabela §4 e do checklist §8. Confirmar apenas que o `FikaSyncManager` (estáticos `_initialized`/`_fikaNetworkManager`) lida com re-raid — o `RegisterPacket` é idempotente por sessão de rede; se necessário, anotar, mas sem nova edição.

**Decisão:** `[x]` Aceitar sugestão (remover do escopo) · `[ ]` Caminho alternativo

---

## Histórico

| Data | Evento |
|---|---|
| 2026-06-22 | Review 01 via `/review-technical-spec` — 0 🔴, 2 🟡, 1 🟢. Fatos dos stubs confirmados no Assembly/mod. |
