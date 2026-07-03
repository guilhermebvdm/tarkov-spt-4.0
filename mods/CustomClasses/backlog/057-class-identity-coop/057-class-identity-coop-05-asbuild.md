# 057 — Identidade de classe per-player em coop (Fika) · As-Built

**Mod:** CustomClasses
**Spec funcional:** [057-class-identity-coop-01-spec.md](057-class-identity-coop-01-spec.md)
**Spec técnica:** [057-class-identity-coop-02-spec-tech.md](057-class-identity-coop-02-spec-tech.md)
**Última review técnica:** [057-class-identity-coop-03-spec-tech-review-01.md](057-class-identity-coop-03-spec-tech-review-01.md)
**Build inicial:** 2026-07-03

> Documentação **pós-implementação**. Reflete o estado real do código entregue pelo `/code-mod` e atualizado por `/apply-code-review`. Quando o conteúdo aqui diverge da spec técnica, este documento ganha — a spec é planejamento, o asbuild é o que foi feito.
>
> **Nota de sessão:** implementado em git worktree dedicado (`tarkov-spt-4.0-wt-057`, branch
> `feat/053-perks-property-model`) porque o working tree principal foi trocado de branch pela sessão paralela
> do editor durante o desenvolvimento.

## Arquivos alterados (build inicial)

| Ação | Path | Resumo |
| --- | --- | --- |
| CRIADO | `mods/CustomClasses/modded/Server/ClassIdentitiesResponse.cs` | DTOs (`ClassIdentitiesResponse`/`PlayerClassIdentity`) do payload da rota, molde da `SkillMultipliersResponse`. |
| CRIADO | `mods/CustomClasses/modded/Server/ClassIdentitiesRouter.cs` | Rota estática `/customclasses/class-identities`: enumera perfis ordenados (PA-01-10), resolve Edition→`ClassVisualRegistry`, dedup de nickname, null-safety, órfãs acumuladas (PA-01-09). |
| CRIADO | `mods/CustomClasses/modded/Client/ClassIdentities.cs` | Cache client nickname→`Identity` (fetch lazy + `Reset()` por tela de loading — PA-01-04), `Local()` (fallback via SkillMultipliers — PA-01-07/08), aviso 1× na degradação. |
| MODIFICADO | `mods/CustomClasses/modded/Client/SkillMultipliers.cs` | Accessor `ClassNamePt` (PA-01-08). |
| MODIFICADO | `mods/CustomClasses/modded/Client/PerksCatalog.cs` | `GroupsFor(classNameEn)` extraído; `LocalGroups()` delega. |
| MODIFICADO | `mods/CustomClasses/modded/Client/PerksPanelView.cs` | `Refresh(panel, Identity?)` parametrizado (header/brasão/marca d'água/cards da identidade recebida); wrapper `Refresh(panel)` → `ClassIdentities.Local()`; idempotência per-panel via `PanelState` (substitui `_lastPanelClass` estático — PA-01-07). |
| MODIFICADO | `mods/CustomClasses/modded/Client/Patches/ClassDetailLoadingPatch.cs` | Postfix per-player: gate scav local (PA-01-02, `FikaBackendUtils.IsScav` via reflection), refetch por instância da tela (PA-01-04), resolução por nickname c/ fallback local, tint do `Nickname` TMP (PA-01-03), `LoadingClassHover.Identity` + `DisableRaycast()` pós-Refresh (PA-01-11). |

## PA-NN-MM resolvidos durante o build

> Pontos da última review técnica que foram **aplicados como parte da implementação** (não como /apply-code-review posterior).

| ID | Categoria · Impacto | Resumo da resolução |
| --- | --- | --- |
| PA-01-01 | C — Lógica · 🟡 | Usings do router copiados 1:1 da `SkillMultipliersRouter.cs:1-5` (`Models.Eft.Common` p/ `EmptyRequestData`); `namespace CustomClasses`. |
| PA-01-02 | B — Edge · 🔴 | Gate `FikaBackendUtils.IsScav` (reflection, `FikaBackendUtils.cs:47`) → raid scav local é no-op; limitação do scav remoto documentada (01-spec emendada). |
| PA-01-03 | A — Gap · 🟡 | Tint-only na linha (`ClassIdentityView.ApplyGradient` no TMP `Nickname`); brasão só no popover. |
| PA-01-04 | B — Edge · 🟡 | `_lastLoadingScreen` (ReferenceEquals) → `ClassIdentities.Reset()` a cada nova tela de loading (1 fetch/raid; perfis novos sem restart). |
| PA-01-05 | B — Edge · 🟡 | Fetch síncrono aceito e documentado (precedente 055 no mesmo Postfix, LAN, 1×/raid); promotable a 06-fix se o gate mostrar hitch. |
| PA-01-06 | C — Lógica · 🟢 | Refs de linha corrigidas na spec (OnDestroy 197, Headless :25, `_lastPanelClass` :23, `LocalGroups` :178, `Edition` :100). |
| PA-01-07 | A — Gap · 🟢 | Contrato único `Refresh(panel, Identity?)`; `ClassIdentities.Local()`; `PanelState` com `GetComponent ?? AddComponent` (sem NRE). |
| PA-01-08 | A — Gap · 🟢 | `SkillMultipliers.ClassNamePt` exposto (1 linha). |
| PA-01-09 | B — Edge · 🟢 | `SeenUnknownEditions` (HashSet estático) acumula editions fora do registry 1×; sem logger no molde do router (degradação prevista na review). |
| PA-01-10 | B — Edge · 🟢 | `OrderBy(kv => kv.Key.ToString(), Ordinal)` → dedup determinístico entre restarts. |
| PA-01-11 | B — Edge · 🟢 | `DisableRaycast()` chamado APÓS cada `Refresh` (rebuild de cards cria Graphics novos) — popover só-exibição no loading. |

## Mudanças posteriores

> Atualizado por `/apply-code-review` a cada rodada. Cada entrada lista os achados aplicados/rejeitados/pulados naquela rodada e os arquivos tocados.

(vazio inicialmente — preenchido por `/apply-code-review`)

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-03 | Build concluído via `/code-mod` (compile client+server 0 erros; instalado em D:/SPT — rota nova exige restart do SPT.Server) |
