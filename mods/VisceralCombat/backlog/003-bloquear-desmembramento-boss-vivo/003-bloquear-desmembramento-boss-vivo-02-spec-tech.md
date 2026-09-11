# 003 — Bloquear Desmembramento de Perna em Boss/Escolta Vivos · Spec Técnica

**Mod:** VisceralCombat
**Spec funcional:** [003-bloquear-desmembramento-boss-vivo-01-spec.md](003-bloquear-desmembramento-boss-vivo-01-spec.md)
**Criado:** 2026-09-10

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT cita `arquivo.cs:linha`.

**Memória consultada:** snapshot de 2026-09-09 (Sessão 7) — pendências [P-7.1]🔴/[P-7.2]🟡/[P-7.3]🟡 são todas do item 002 (validação in-raid coop e dívida de pacotes Fika); nenhuma afeta este item. **Docs técnicos lidos (gatilho disparado):** `spt-antipatterns.md` (sempre). `fika-packet-desync-prevention-plan.md` não se aplica — este item não cria nem toca pacote de rede.

## 1. Estratégia

Adicionar uma guarda de papel de bot (`WildSpawnType.IsBossOrFollower()`) dentro do gate "vivo" já existente em `LimbKillPatch.ProcessLimbKill`. Não é um novo patch Harmony — é uma edição de uma condicional dentro de um método já patcheado pelo mod. A checagem usa a extension method canônica do próprio EFT (data-driven, sem lista hardcoded de nomes de boss no mod), evitando que o mod precise manter uma lista própria de bosses que quebraria a cada atualização do jogo (wipe/temporada nova, boss novo).

**Alternativa descartada:** manter uma lista própria de `WildSpawnType` de boss no mod (ex.: `HashSet<WildSpawnType>` com `bossKilla`, `bossTagilla`, etc.) — descartada porque duplica uma classificação que o próprio jogo já mantém (`BotSettingsRepoClass`, populada a partir dos dados de bot do servidor) e ficaria desatualizada a cada boss novo adicionado em updates futuros do EFT/SPT.

## 2. Pontos de patch

| Alvo (mod) | Tipo | Motivo |
|---|---|---|
| [`LimbKillPatch.cs:68-71`](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L68-L71) (código do próprio mod, dentro do método já patcheado `ProcessLimbKill`) | Edição inline | Adicionar guarda `WildSpawnType.IsBossOrFollower()` ao gate "vivo" existente |

Nenhum novo alvo Harmony é criado. Evidência de suporte no Assembly (não são pontos de patch, são a API consumida):

| Referência (Assembly) | O que fornece |
|---|---|
| [`BotSettingsRepoClass.cs:555-559`](../../../../references/eft-decompiled/Assembly-CSharp/BotSettingsRepoClass.cs#L555-L559) | `public static bool IsBossOrFollower(this WildSpawnType role)` — extension method canônica, classe no namespace global (sem `using` extra necessário, já que `EFT` — namespace de `WildSpawnType` — já está importado em `LimbKillPatch.cs:4`) |
| [`BotSettingsRepoClass.cs:492-499`](../../../../references/eft-decompiled/Assembly-CSharp/BotSettingsRepoClass.cs#L492-L499) | `IsBoss(this WildSpawnType role)`, usada internamente por `IsBossOrFollower` — confirma que a classificação é data-driven (`Dictionary_0`), não uma lista fixa |
| [`ProfileInfoSettingsClass.cs:7`](../../../../references/eft-decompiled/Assembly-CSharp/ProfileInfoSettingsClass.cs#L7) | `public WildSpawnType Role = WildSpawnType.assault;` — campo consumido via `player.Profile.Info.Settings.Role`, mesmo padrão de acesso já usado pelo próprio EFT em `Player.cs:30532` (`Profile.Info.Settings.Role == WildSpawnType.exUsec`) |

## 3. Novas propriedades F12 (BepInEx)

Nenhuma. O comportamento é uma correção de escopo da feature já existente (item 001), não uma opção configurável nova — segue a decisão implícita do próprio pedido ("Boss vivo nunca desmembra", sem menção a toggle). Se o usuário quiser reverter, o toggle já existente `EnableDismemberment` (ou desabilitar a feature via `/review-mod-properties` futuramente) continua servindo como corte geral.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs` | MODIFICAR | Gate "vivo" em `ProcessLimbKill` passa a excluir Boss/escolta (`IsBossOrFollower()`), com defesa contra `Profile`/`Info`/`Settings` nulo |

## 5. Stubs de código

Edição em [`LimbKillPatch.cs:66-71`](../../../../mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L66-L71):

```csharp
// Only process dead players OR living AI bots when VisceralCombat is present for all players
bool isDead = (player.HealthController == null || !player.HealthController.IsAlive) && !RagdollHelperClass.IsPlayerDowned(player);
if (!isDead)
{
    if (!player.IsAI || !VisceralEntry.AllPlayersHaveVisceralCombat) return;

    // Boss e escolta nunca entram no desmembramento de perna em vivos (rastejo/agonia) —
    // só bots comuns. Pós-morte (isDead == true, mais abaixo neste mesmo método) continua
    // liberado geral, sem essa restrição — ver spec funcional 003, item "Comportamento desejado".
    // ref: Assembly-CSharp/BotSettingsRepoClass.cs:555-559 — WildSpawnType.IsBossOrFollower()
    // (data-driven no próprio jogo; nenhuma lista de boss mantida pelo mod).
    WildSpawnType? role = player.Profile?.Info?.Settings?.Role;
    if (role.HasValue && role.Value.IsBossOrFollower()) return;
}
```

`role.HasValue` cobre o corner case de `Profile`/`Info`/`Settings` nulo (spec funcional, corner case): se qualquer nível da cadeia for nulo, `role` fica `null`, `HasValue` é `false`, e o `if` não bloqueia nada — comportamento cai para o que já existia antes desta mudança (bot comum, desmembramento em vivos segue normal), sem `NullReferenceException`.

## 6. Fluxo de dados

```
[A] Tiro na perna de um bot vivo → LimbKillPatch.ProcessLimbKill (LimbKillPatch.cs:36)
  → [B] isDead == false (bot vivo) → checagem IsAI + AllPlayersHaveVisceralCombat (já existente)
    → [C] player.Profile.Info.Settings.Role (ProfileInfoSettingsClass.cs:7) → .IsBossOrFollower()
      (BotSettingsRepoClass.cs:555-559, consulta a tabela data-driven do próprio jogo)
      → é Boss/escolta? → [D] return — sem desmembramento, dano normal segue seu curso vanilla
      → não é? → [D'] segue pro fluxo normal de desmembramento de perna em vivos (30% chance,
        já existente, inalterado)
```

## 7. Riscos e dependências

- **Nenhum pacote de rede envolvido.** A checagem é puramente local, decidida a partir de um dado já sincronizado pelo profile do bot (`WildSpawnType`, definido na criação do bot e idêntico em todos os peers) — não há round-trip nem risco de dessincronia entre host/clientes (todos os peers avaliam `IsBossOrFollower()` sobre o mesmo `Role`, chegando ao mesmo resultado independentemente de quem processa o tiro).
- **Sem interação com outros patches do mod.** `ProcessLimbKill` já é o único ponto que decide desmembramento em vivos; esta mudança só adiciona uma condição de saída antecipada dentro dele. Não toca no ramo pós-morte (linhas seguintes do mesmo método), no `KillPatch.cs`, nem em `ShootOffHelmetPatch`/`WeaponDropOnDeathPatch`/`DropHeadEquipment` (itens 002).
- **Dependência da classificação nativa do jogo.** Se uma atualização futura do EFT mudar como bosses são classificados (`Dictionary_0` interno de `BotSettingsRepoClass`), o mod acompanha automaticamente — não há lista própria a manter, mas também não há como o mod "corrigir" se o jogo classificar algo de forma inesperada (aceito como trade-off, documentado na spec funcional como corner case).

## 8. Checklist de implementação

- [x] Editar `LimbKillPatch.cs:68-71` conforme §5.
- [x] Compilar (`dotnet build ... -c Release -o mods/VisceralCombat/builds/...` — sem instalar automaticamente no jogo). Build succeeded em 2026-09-10 (versão 3.9.11 → 3.9.12).
- [ ] Validar solo: atirar na perna de um Boss vivo (Killa/Reshala/etc., via debug de spawn ou raid com boss) → sem rastejo/agonia forçada. Atirar na perna de um Scav comum → comportamento inalterado (30% chance normal). Matar o Boss e atirar na perna do cadáver → desmembramento pós-morte funciona normalmente.
- [ ] Validar escolta: atirar na perna de um seguidor de boss vivo (ex.: `followerBully`) → mesmo bloqueio do boss.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Nenhum estado novo alocado (estático ou por raid); a checagem é avaliada a cada tiro, sem cache entre raids |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | O gate já existente (`!player.IsAI`) exclui humanos antes desta checagem rodar; a nova condição só refina o universo de bots, sem introduzir reação a ação de outro player |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Não há novo alvo Harmony; `IsBossOrFollower`/`IsBoss` são extension methods concretas (não virtuais) em `BotSettingsRepoClass.cs:492-559` |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | Usa a extension method canônica do próprio jogo (`WildSpawnType.IsBossOrFollower()`) em vez de reimplementar a classificação; nenhum side-effect além do `return` antecipado (sem mutar nenhum estado) |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | N/A | Nenhum estado persiste — checagem stateless a cada tiro |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhuma `ConfigEntry` nova (§3) |
| 7 | Reentry-guard em re-invocação de método patcheado — AP-07 | N/A | Não há re-invocação do método patcheado |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | N/A | Nenhum cache de contexto; `role` é resolvido fresco a cada chamada, direto do `Profile` atual do `player` recebido por parâmetro |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | `IsBossOrFollower`/`IsBoss` (BotSettingsRepoClass.cs:492-559), `ProfileInfoSettingsClass.Role` (linha 7), e o padrão de acesso `Profile.Info.Settings.Role` confirmado em uso real por `Player.cs:30532` — todos lidos diretamente no dump local |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não usa skill do EFT |
| 11 | Pacote FIKA próprio: envelope, `TryGet*`, `Valid`, registro por instância, zero `UnregisterPacket` — AP-11 | N/A | Nenhum pacote de rede criado ou tocado por este item |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-10 | Spec técnica criada via `/create-technical-spec` |
