# 002 — Drop de Arma na Morte + Capacete/Óculos no Desmembramento de Cabeça · Code Review 04

**Mod:** VisceralCombat
**Spec funcional:** [002-drop-arma-capacete-oculos-cabeca-01-spec.md](002-drop-arma-capacete-oculos-cabeca-01-spec.md)
**Spec técnica:** [002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md](002-drop-arma-capacete-oculos-cabeca-02-spec-tech.md)
**Asbuild:** [002-drop-arma-capacete-oculos-cabeca-05-asbuild.md](002-drop-arma-capacete-oculos-cabeca-05-asbuild.md)
**Data:** 2026-09-20

> Quarta rodada — validação das correções `CR-03-01`/`CR-03-02` (aplicadas nesta mesma sessão) e nova auditoria do estado atual do código, sem re-levantar nenhum ponto já `✅ Resolvido`/`✅ Aplicado` nas rodadas 01-03.

**Memória consultada:** snapshot de 2026-09-20 (Sessão 13), `mods/VisceralCombat/memory/sessions.md`. **Pendências que afetam esta revisão:** `[P-10.3]` (🟡 — aberta pela própria rodada 03, sobre o mecanismo desativado; não é código a revisar, é uma investigação futura já registrada) / nenhuma outra. **Docs técnicos conferidos:** `spt-antipatterns.md` (AP-02, AP-04, AP-09), nenhum pacote FIKA novo introduzido.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-04-01 | C — Gap vs. spec | 🟢 Menor | `ShootOffHelmetPatch` (pós `CR-02-02`) não dispara mais em nenhum tiro fatal, mudando o comportamento que a spec técnica original descrevia como inalterado | ✅ Aplicado em 2026-09-20 |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Validação das correções da rodada 03

Reconferido diretamente no código atual (não presumido a partir do documento anterior):

- **`CR-03-01`:** confirmado em [`LimbKillPatch.cs:264-274, 280-281`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L264-L281) — as duas chamadas `DropCorpseHeadEquipment(player);` estão comentadas, com o efeito visual (`DismemberLimb`) intacto. `DropCorpseHeadEquipment` ([:298-319](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs#L298-L319)) mantida implementada com o gate `FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer` corretamente adicionado. `using Fika.Core.Main.Utils;` presente. **Correto, sem regressão.**
- **`CR-03-02`:** confirmado em [`DeathInventoryDropPatch.cs:117-132`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs#L117-L132) — checagem de `LastBodyPart != Head` agora roda antes da checagem de tipo de dano. **Correto, sem regressão.**
- Build confirmado limpo (`dotnet build`, Release — Build succeeded) antes desta rodada começar.
- Registro de patches em `VisceralEntry.cs:293-295` conferido — `ShootOffHelmetPatch`, `DeathInventoryDropPatch`, `WeaponDropOnDeathSkipVanillaFlingPatch` cada um registrado exatamente uma vez, sem entradas órfãs de nomes de classe antigos (`WeaponDropOnDeathPatch`).
- Gate de autoridade expandido por `CR-02-01` (`... || player.IsYourPlayer`, [`DeathInventoryDropPatch.cs:75`](../../modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs#L75)) reconferido quanto a risco de dupla execução: `IsYourPlayer` só é verdadeiro pro personagem local de quem está rodando aquela instância do jogo — nunca verdadeiro para um `ObservedPlayer` representando outro humano — então a expansão não introduz nenhum caminho de processamento duplicado entre peers.

## Pontos

### CR-04-01 · C — Gap vs. spec · 🟢 Menor · ✅ Aplicado em 2026-09-20

**`ShootOffHelmetPatch` (pós `CR-02-02`) não dispara mais em nenhum tiro fatal, mudando o comportamento que a spec técnica original descrevia como inalterado**

**Local:** [`mods/VisceralCombat/modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs:22-27`](../../modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/ShootOffHelmetPatch.cs#L22-L27)

**Problema:**
```csharp
if (__instance.HealthController == null || !__instance.HealthController.IsAlive) return;
if (!(FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer)) return;
```
`ActiveHealthController.Kill()` seta `IsAlive = false` de forma síncrona, antes mesmo de `Player.ApplyDamageInfo` retornar (ver `DeathInventoryDropPatch.cs:17-30`, mesma cadeia já documentada nesta sessão). Isso significa que, pro Postfix de `ApplyDamageInfo` que `ShootOffHelmetPatch` usa, `IsAlive` **já está `false`** pra qualquer tiro que tenha matado o bot — não só os que desmembram a cabeça. Ou seja: o guard de `CR-02-02` (correto pra evitar a duplicação que motivou o fix) tem o efeito colateral de desligar a chance configurável de "Helmet Knock Off" (`HelmetShootOffChance`) pra **qualquer** tiro de cabeça fatal, mesmo quando a cabeça não chega a ser desmembrada. A spec técnica original (`002-...-02-spec-tech.md`, tooltip de `Drop Headwear/Eyewear On Head Dismemberment`) descreve esse mecanismo de chance como "gatilho distinto... que continua funcionando como antes" — o que não é mais literalmente verdade pro caso de tiro fatal sem desmembramento (antes: tinha chance de cair; agora: nunca cai, já que nem `ShootOffHelmetPatch` nem o drop garantido por desmembramento disparam nesse caso específico).

**Por que importa:** Efeito puramente de gameplay (capacete de bot morto por tiro de cabeça fatal sem desmembramento agora sempre permanece no cadáver, quando antes tinha uma chance configurável de cair) — não é um bug de corrupção de dado nem risco de duplicação (a correção de `CR-02-02` continua sendo a escolha certa pra evitar isso). É só uma mudança de comportamento não documentada como tal, potencialmente surpreendente pra quem lembra do texto original da spec.

**Sugestão:** Nenhuma ação de código necessária — a troca de comportamento é uma consequência aceitável e mais segura da correção de `CR-02-02` (a alternativa, manter o `ThrowItem` rodando em bot já morto, é exatamente o bug de duplicação que motivou o fix). Recomendo só atualizar o tooltip/documentação de `Helmet Knock Off Chance` em `PROPRIEDADES.md` pra deixar explícito que ele só se aplica a tiros de cabeça **não-fatais** — evita confusão futura sobre por que o capacete "nunca mais cai por chance" num tiro que matou o bot.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto.
**Aplicação:** `mods/VisceralCombat/PROPRIEDADES.md` — tooltip de "Helmet Knock Off Chance" atualizado explicitando que só dispara em tiros de cabeça não-fatais; referência cruzada adicionada no tooltip de "Drop Headwear/Eyewear On Head Dismemberment". Nenhuma mudança de código (achado puramente documental).

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-20 | Code review 04 criada via `/code-review` — valida as correções `CR-03-01`/`CR-03-02` (confirmadas corretas, sem regressão) e levanta 1 achado novo, 🟢 menor, puramente documental. |
| 2026-09-20 | Aplicação de 1 achado via `/apply-code-review` — ID: CR-04-01 (mudança só em `PROPRIEDADES.md`). |
