# Suíte de Compatibilidade — Trauma 2.0

> **Data:** 2026-07-20<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [trauma-matrix.md](./trauma-matrix.md)<br>

---

Consolida as 8 auditorias de compatibilidade da decisão D20 ([trauma-matrix.md](./trauma-matrix.md)), hoje espalhadas em 5+ specs técnicas e reviews dos itens 003-008. Nenhuma prova é refeita aqui — este documento só aponta para o artefato original onde a evidência foi produzida. Item de backlog 009 (hardening coop).

Todos os 8 mods estão instalados na máquina de desenvolvimento (`launcher/Launcher4.0-v2/.../BepInEx/plugins/`) — nenhuma lacuna de "mod ausente, não auditável".

## Tabela de veredito

| Mod | Veredito | Mecanismo de convivência | Auditado em |
|---|---|---|---|
| **SAIN** | ✅ Sem conflito de escrita conhecido | Sem patch nos getters de velocidade/pose que o SAIN também usa; camada BigBrain do mod (prioridade 90) sempre acima das camadas SAIN/ORBIT (4-19); interop por reflection (`BotComponent.ActiveLayer`, padrão `TrySainSetTargetPose`) | `004-pernas-cair-ciclo-02-spec-tech.md:766,50`; `003-pernas-mancar` (dip cosmético SAIN-aware); `007-desmaio-percentual-02-spec-tech.md:264` (0 hits em `ApplyDamageInfo`/`ApplyDamageEvent`, grep no repo + no DLL instalado) |
| **ORBIT** | ✅ Sem conflito | Prioridade BigBrain do mod (90) sempre acima da camada ORBIT; pausar/retomar sempre deixa a camada RE-DECIDIR (nunca mata a camada) | `004-pernas-cair-ciclo-02-spec-tech.md` (mismatch de versão repo 1.2.1 vs. instalado 1.1.0 já reconciliado contra o DLL real) |
| **UNTAR** | ✅ Sem ação necessária | Não é um mod — é um nome de facção de bot (`WildSpawnType`) que ORBIT/outros tratam como substring excludable; D15 ("UNTAR segue as mesmas regras — são bots") satisfeito por construção: o motor Trauma 2.0 nunca discrimina por tipo de bot | Prioridade da camada UNTAR (4/5) mapeada no spike 001 (P6), sempre abaixo da camada do mod (90) |
| **CustomClasses (Tank)** | ✅ Sem conflito ao nível de mecanismo | Baseline de velocidade lê o getter `MaxSpeed` **vivo** (já composto pelo multiplicador do CustomClasses); "sem patch nos getters = sem colisão" — confirmado o multiplicador Tank real (`HeavyFrame` −10%) em `ClassMovementPatches.cs:52-55` | `003-pernas-mancar-02-spec-tech.md:138,329` |
| **SPTRecoilRework** | ✅ Fechado | Postfix-only no mesmo alvo do nosso patch de cancela-ADS; idempotente por construção Harmony — evidência de IL real | `005-bracos-tremor-ads-03-spec-tech-review-01.md:56` (`rr.il:1719`) |
| **Fontaine-FOVFix** | ✅ Fechado | Mesmo alvo, mesmo padrão postfix-only; evidência de IL real | `005-bracos-tremor-ads-03-spec-tech-review-01.md:56` (`ff.il:4283-4316`); reforçado em `docs/trauma-primitives.md:457` |
| **BringBackConcussion** | ✅ Fechado | Prefix `void` sem `[HarmonyPriority]` (prioridade padrão `Normal`=400, menor que nosso `High`=600 — nosso Prefix sempre roda primeiro); corpo só chama `DoContusion`/`DoStun`, nunca toca `GetBodyPartHealth`/HP de parte | Decompile via `ilspycmd` do DLL instalado: `007-desmaio-percentual-04-code-review-01.md:75-100`, `007-desmaio-percentual-03-spec-tech-review-02.md:46-49` (nota: a 1ª prova da spec comparou a garantia errada do Harmony; conclusão final estava certa, corrigida na review 02) |
| **Visceral Combat** | ✅ Fechado | 2 Postfixes no mesmo alvo, 0 Prefixes — não pode interferir na captura de `__state` do nosso Prefix; DLL de 21.880 linhas com 0 hits em `SetPoseLevel`/`IsInPronePose`/`ToggleProne`/`ChangePose` (é só ragdoll, não mexe em pose) | Mesmo decompile do item acima; `docs/trauma-primitives.md:198` |
| **tarkin-ladders** | ✅ Fechado (item 009) | Escadas interativas reais (distinto do vanilla, que não tem esse tipo) — o guard D7 (adiar agachar/cair em escada/corda/BTR/vault) já cobre esse contexto desde a entrega do item 004; formalizado na lista D20 pelo item 009 (antes só existia como nota do spike 001, nunca no texto canônico) | `docs/trauma-primitives.md:197`; guard D7 implementado em `004-pernas-cair-ciclo-02-spec-tech.md` |

## O que NÃO está coberto por esta suíte (validação manual pendente)

Auditoria estática de compatibilidade ≠ validação em jogo real. Nenhum dos vereditos acima foi confirmado rodando o jogo com o mod de terceiros ativo simultaneamente ao Trauma 2.0. Em particular:

- **Smoke SAIN/ORBIT do re-derrubar de bot** (bot cai → SAIN/ORBIT tentam levantar → condição persiste → bot é re-derrubado, sem travar num estado inconsistente) — executável em raid solo (host com bots), não exige 2º PC. Roteiro no item 009 do backlog.
- **CustomClasses-Tank jogado de fato** (não só a prova de mecanismo do getter composto).
- Os demais mods (RecoilRework, FOVFix, BringBackConcussion, VisceralCombat) não têm cenário de smoke específico pendente além do já coberto pelo plano de teste geral de `docs/trauma-behavior-matrix.md §5`.

## Auditoria transversal já fechada (não é deste documento, mas relevante)

A garantia "espelho Fika nunca aplica efeito duplicado" (checklist AP-02 de cada item) está **100% auditada e aprovada** para todos os itens 002-008 — cada spec técnica tem essa evidência na própria seção "§9 Conformidade com skills". Não é repetida aqui por não ser uma compatibilidade de mod de terceiro, e sim uma garantia interna do próprio Trauma 2.0.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-20 | Guilherme (com Claude) | Criação — consolida as 8 auditorias de compat (D20) já feitas em 003-008, sem refazer nenhuma prova. Item 009 do backlog. |
