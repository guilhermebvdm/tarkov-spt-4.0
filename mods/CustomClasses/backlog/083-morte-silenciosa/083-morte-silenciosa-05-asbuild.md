# 083 — Morte Silenciosa (faca sem som) · As-Built

**Mod:** CustomClasses · **Épico:** [rebalance-v2-2026-07-25.md](../rebalance-v2-2026-07-25.md) · **Build:** 2026-07-26 · **Versão:** 0.11.1

Perk NOVO (Furtivo): a faca não faz SOM — sacar, golpear e acertar são silenciosos.

## Achado do spike (mudou a premissa, para MENOS)
O board marcava o 083 como "o mais caro do épico — COOP + AI CRÍTICO". O spike no decompile mostrou que é **muito mais simples**:
- **Um único choke de áudio:** `BaseSoundPlayer.PlayClip` (BaseSoundPlayer.cs:395) — cobre deploy + swing + hit, local E peer. Prefix → `return false`.
- **NÃO há canal de IA para melee.** O `AISoundType` só tem step/silencedGun/gun; nenhum call-site de `BotEventHandler.PlaySound` para faca. Bots só reagem ao **dano**, nunca ao som — o "AI crítico" já é verdade no vanilla. **Nada a patchar do lado da IA.**
- **Coop sem protocolo novo:** o peer Fika usa `ObservedKnifeController : Player.KnifeController` sobre `ObservedPlayer : … : Player`, com **PlayerBridge** cujo `iPlayer` é um `EFT.Player` real — um patch client-side gateado pela classe do EMISSOR (rota 057) cobre você-não-ouve-sua-faca e você-não-ouve-a-do-peer.

## Implementação
- `SilentKnifePatch` — Prefix em `BaseSoundPlayer.PlayClip`. Ordem barata→cara: perk-off → `is WeaponSoundPlayer` (descarta arma de fogo) → resolve emissor via `playersBridge.iPlayer` (reflection crua: o tipo `IObserverToPlayerBridge` está em assembly não referenciável → `FieldRef` tipado impossível; resolvido num static ctor guardado) → `HandsController.Item is KnifeItemClass` → `ClassNameEnOf(emitter) == "Stealth"` → `return false`.
- Config `SilentKnifeEnabled` (default true, seção Stealth) · catálogo grupo `silent_knife` (perk qualitativo) · ByClass Stealth +silent_knife · Plugin `Enable` em try/catch.

## Arquivos
| Ação | Path |
|---|---|
| CRIA | `Patches/SilentKnifePatch.cs` |
| MOD | `PerksConfig.cs` (+SilentKnifeEnabled) · `PerksCatalog.cs` (+silent_knife, ByClass Stealth) · `Plugin.cs` (+Enable) |

## Code-review (sub-agent adversarial) — 0 bloqueadores
| Sev | Achado | Resolução |
|---|---|---|
| 🟡 | Comentário XML descrevia o coop INVERTIDO (dizia ObserverBridge; o Fika usa PlayerBridge) — risco de um mantenedor relaxar o guard `is Player` | **Corrigido** — comentário reescrito + documentado o gap seguro sem Fika |
| 🟢 | Init de `IPlayerProp` fora do try/catch (risco de TypeInitializationException) | **Corrigido** — static ctor único guardado |
| 🟢 | Reflection crua vs FieldRef do projeto | Mantido — FieldRef tipado é impossível (tipo não referenciável); cache + firearms filtrados antes tornam o custo irrelevante |

**Verificado limpo:** sem vazamento 075 (bot→ClassNameEnOf null; não-Furtivo→false; granada/meds tocam normal); tiro não passa por PlayClip; peer coberto via PlayerBridge; `return false` não deixa source órfão; fail-open no catch.

## Pendências de validação in-game (feedback_spt_validation)
- Confirmar que o som de **ACERTO** (impacto em carne/superfície) roteia por `PlayClip` (swing/deploy confirmados; o impacto pode ter caminho de material próprio).
- Confirmar que nenhuma faca traz `WeaponSoundPlayer` (senão o descarte da etapa 1 zeraria o perk).
- **SAIN:** confirmar que o SAIN não sintetiza percepção de melee própria (não verificável no decompile do EFT).

## Histórico
| Data | Evento |
|---|---|
| 2026-07-26 | Build via g-autodev; spike (choke único, sem canal de IA, coop via PlayerBridge); code-review 🟡+🟢 aplicados; 0.11.1 |
