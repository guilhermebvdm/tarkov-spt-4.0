# Code-review adversarial — lote não-revisado de 2026-07-04 (051 · 057 fix-03 · 060 · UI r5 · cor v2 · router)

**Mod:** CustomClasses (+ 1 finding no stances)
**Criado:** 2026-07-04
**Método:** 4 agentes de review adversarial em contexto limpo (1 por área), findings verificados no código
(incl. decompile da Assembly-CSharp instalada) antes de aplicar.
**Escopo (commits sem code-review desde `cdbaa15`):** `18c1721` (051) · `12459ab` (060) · `74a9f29` (guard
sessão) · `db85943` (aba r5) · `5727a3e` (cor v2) · `994f0c1` (057 06-fix-03).

**Resultado: 19 findings (1 alta, 10 médias, 8 baixas) — 18 aplicados, 1 aceito com tradeoff documentado.**

## Findings e disposição

| ID | Sev | Defeito (resumo) | Disposição |
|---|---|---|---|
| CR-057F3-01 | **alta** | Host do 06-fix-03 (`RaidReadyPlayerPanel.Show`) é código morto no SPT (branch Local fecha o painel) — feature inteira (e a escala do 015) nunca disparou | ✅ Re-host em `PartyPlayerItem` — ver [06-fix-04](057-class-identity-coop/057-class-identity-coop-06-fix-04.md) |
| CR-057F3-02 | média | `ChatSpecialIconPatch` REVERTIA identidade de qualquer nickname ≠ local na superfície real da linha | ✅ Resolve remotos via `ClassIdentities.TryResolve` |
| CR-057F3-03 | média | Mapa nickname→classe stale entre visitas ao deploy (Reset só no raid-start, que roda DEPOIS) | ✅ `PartyInfoPanelPrefetchPatch` (Prefix na tela) |
| CR-057F3-04 | média | Fetch síncrono na thread de UI na abertura do deploy | ⚠️ ACEITO — 1 GET pequeno por tela (LAN); async se incomodar |
| CR-057F3-05 | média | `PositionAtPointer` usava canvas aninhado mais próximo + `worldCamera` do canvas | ✅ Rect do parent real (root canvas) + `enterEventCamera` |
| CR-057F3-06 | baixa | Painel FollowCursor mantinha âncoras "fill" até o 1º posicionamento; early-return ativava painel gigante | ✅ Âncoras no `Ensure`; exibe só com posição válida |
| CR-057F3-07 | baixa | Tooltip 015 + popover abrem juntos no mesmo hover | ✅ `ClassTooltip.Clear` na linha com popover |
| CR-051-01 | média | `EnsureLoaded` (HTTP síncrono) podia disparar DENTRO do Tick de stamina no 1º ADS (UIs off no F12) | ✅ Warm no raid-start + prefetch no deploy |
| CR-051-02 | média | `Mathf.Clamp` não segura NaN de hook externo → `hands.Current` envenenado (stances, contrato público) | ✅ `float.IsNaN` guard no stances (DLL deployada) |
| CR-051-03 | baixa | `SkillMultipliers.Reset()` sem caller — troca de classe no editor web exigia restart | ✅ Reset+refetch por tela de deploy |
| CR-051-04 | baixa | Warn-once do boot disparava mensagem enganosa TODO boot (ordem alfabética do chainloader) e silenciava o retry | ✅ `TryAttach(finalAttempt)` — Info no boot, Warning só no raid-start |
| CR-060-01 | média | MasteryFooter renderiza config do CLIENT LOCAL no popover de OUTRO player | ✅ Footer só no host da aba CLASS (`showMasteryFooter`) |
| CR-060-02 | média | Catch sem filtro/log no router → falha REAL de perfil virava vanilla silencioso (cacheado a sessão inteira) | ✅ sessionId vazio explícito sem log; resto loga Warning c/ contexto |
| CR-060-03 | baixa | Footer apertava a altura fixa do popover | ✅ Convergiu com CR-060-01 (suprimido no popover) |
| CR-UI5-01 | média | 3 call sites passavam `tmp.color` VIVO como fallback → clareamento composto (drift geométrico rumo ao branco) | ✅ Sem nameColor → label nativo (guard nos 3) |
| CR-UI5-02 | média | Revert de célula reciclada não restaurava `.color` (na v2 ele é o portador da cor) → nome de OUTRO player na minha cor | ✅ `RestoreNativeLabel` + captura 1× (`ConditionalWeakTable`) |
| CR-UI5-03 | baixa | `tmp.color = light` forçava alpha ≈1 (matava fade/translucidez nativa) | ✅ `light.a = tmp.color.a` |
| CR-UI5-04 | baixa | Comentário prometia invariância TOTAL a `.color` externo (é parcial em canais escuros) | ✅ Comentário corrigido (aceito e documentado) |
| CR-UI5-05 | baixa | Reabertura da tela Skills não re-estilizava a aba → brasão/tint da classe ANTIGA após troca in-session | ✅ `StyleClassTab` também no early-return |

## Verificado SEM defeito (destaques dos agentes)

- 051: exceção no hook nunca escapa (try/catch dos 2 lados); zero reflection no hot path; FIKA — observed
  players não passam pelo caminho; `delta×0` → `-0f` não dispara efeitos colaterais; idempotência do attach.
- 057: **sem pooling** nas rows (`AddViewList` instancia por player) → sem identidade stale por early-return;
  clamp×escala do popover correto (ApplyScale compensa); scav degrada.
- Cor v2: matemática do ratio segura (preto puro ok, sem NaN, canal base > light impossível); rodada 5 sem
  label duplicado (`Tab.method_0` alterna os pais); tint escuro persiste (nativo não reescreve `Image.color`).
- 060: `{}` deserializa sem NRE; footer sem duplicação/leak; i18n ok.

## Pós-aplicação

- Compile CustomClasses client+server **0 erros** (19:13), instalados em `D:/SPT`.
- Stances recompilado (0 erros) e deployado em `BepInEx/plugins/RealisticMobility/` (19:14).
- `DeployNameScale`: default 3.0 → 1.2 + cfg instalado ajustado (calibragem cega contra host morto).
- ⚠️ Server: DLL nova em `user/mods/CustomClasses` — **restart do SPT.Server** pra carregar o log novo do router.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-04 | Review executado (4 agentes) + 18/19 findings aplicados + builds deployados |
