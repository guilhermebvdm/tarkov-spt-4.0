# 057 — Identidade de classe per-player em coop (Fika) · Code Review 01

**Mod:** CustomClasses
**Spec funcional:** [057-class-identity-coop-01-spec.md](057-class-identity-coop-01-spec.md)
**Spec técnica:** [057-class-identity-coop-02-spec-tech.md](057-class-identity-coop-02-spec-tech.md)
**Asbuild:** [057-class-identity-coop-05-asbuild.md](057-class-identity-coop-05-asbuild.md)
**Data:** 2026-07-03

> Análise crítica do código implementado por `/code-mod` (commit `9c912a9`). Review executada por agente
> adversarial de contexto limpo (verificou specs, diff, callers não-tocados e fontes FIKA/spt-source);
> decisões tomadas em modo autônomo (`/g-autodev`) e registradas em cada ponto.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 3 · 🟢 Menores: 6 · ✅ Resolvidos: 8 (aplicados nesta rodada) · Total: 9

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | C — Gap vs. spec | 🟡 | Aviso de degradação repete a cada raid (critério "1 aviso") | ✅ Aplicado |
| CR-01-02 | B — Bug latente | 🟡 | `SeenUnknownEditions`: HashSet estático × requests concorrentes | ✅ Aplicado (remoção) |
| CR-01-03 | C — Gap vs. spec | 🟢 | Set write-only + "log órfão 1×" da 01-spec não existe | ✅ Aplicado (remoção + emenda) |
| CR-01-04 | C — Gap vs. spec | 🟡 | `nameColor` null → tint branco sobrescreve estilo FIKA | ✅ Aplicado |
| CR-01-05 | B — Bug latente | 🟢 | Postfix não espelha early-return do AddPlayer (netId duplicado) | ✅ Aplicado |
| CR-01-06 | E — Legibilidade | 🟢 | `Local()!` seguido de `if (id == null)` | ✅ Aplicado |
| CR-01-07 | B — Bug latente | 🟢 | Fallback do `Show()` cairia pro LOCAL em linha remota | ✅ Aplicado |
| CR-01-08 | F — Melhoria | 🟢 | `FieldInfo` do Nickname resolvido por linha | ✅ Aplicado |
| CR-01-09 | D — Arquitetura | 🟢 | `_lastLoadingScreen` retém referência à tela destruída | ⏭️ Rejeitado (aceito como está) |

## Pontos

### CR-01-01 · C — Gap vs. spec · 🟡 ✅ Aplicado

**Aviso de degradação repete a cada raid — critério "no máximo 1 aviso" violado**

**Local:** `modded/Client/ClassIdentities.cs` (Reset/EnsureLoaded)

**Problema:** o `Reset()` por tela de loading (PA-01-04) re-arma `_loaded = false` e o `LogWarning` da rota
ausente está no `EnsureLoaded` → com mod server desatualizado, 1 warning **por raid** (5 raids = 5 warnings).
O critério da 01-spec exige "no máximo 1 aviso". Efeito colateral do PA-01-04 não registrado.

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** flag `_warnedUnavailable` que NÃO é limpa pelo `Reset()`:
primeira falha → `LogWarning`; subsequentes → `LogDebug`. O re-fetch por raid permanece (é desejado).

### CR-01-02 · B — Bug latente · 🟡 ✅ Aplicado (remoção)

**`SeenUnknownEditions`: HashSet estático mutado por requests concorrentes do Kestrel**

**Local:** `modded/Server/ClassIdentitiesRouter.cs:20,51`

**Problema:** `HashSet<T>.Add` não é thread-safe e o caso central do 057 é todos os clients do coop fazendo GET
simultâneo no início do loading; dois `Add` paralelos no primeiro request pós-boot podem corromper o set — e um
handler travado bloqueia o `GetJson` síncrono do client.

**Decisão:** `[x]` Aceitar sugestão (variante remoção) — **Resolução:** set removido (era write-only, ver
CR-01-03). `visual == null → continue` permanece com comentário da decisão.

### CR-01-03 · C — Gap vs. spec · 🟢 ✅ Aplicado (remoção + emenda)

**Set write-only e o "log informativo 1×" do corner órfão não existe em lugar nenhum**

**Local:** `modded/Server/ClassIdentitiesRouter.cs:18-20,51` · 01-spec (corner "perfil órfão")

**Problema:** a resolução PA-01-09 prometia log Debug 1×; a spec técnica autorizava degradar para "acumular sem
log" **com a condição** de emendar o corner da 01-spec — a emenda não aconteceu e o código ficou no meio-termo
(sem log, sem emenda, set morto com risco de concorrência).

**Decisão:** `[x]` Aceitar sugestão (variante remoção) — **Resolução:** set removido; corner da 01-spec emendado
("sem crash, **sem log** — decisão CR-01-03: server não distingue órfã de edition vanilla; comportamento seguro,
diagnóstico via editor web"). Fecha CR-01-02 junto.

### CR-01-04 · C — Gap vs. spec · 🟡 ✅ Aplicado

**Classe com `nameColor` null: tint pinta BRANCO por cima do estilo FIKA e não indica nada**

**Local:** `modded/Client/Patches/ClassDetailLoadingPatch.cs` (tint) + `UI/ClassIdentityView.cs:30-45`

**Problema:** `ApplyGradient(tmp, null, Color.white)` sobrescreve incondicionalmente a cor do TMP do FIKA com
branco (sem caminho de revert). `nameColor` é Optional no schema → classe sem cor é legítima; o critério
"indicação visual sem hover" vira alteração visual sem significado.

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** tint pulado quando `id.NameColor` é null/whitespace
(preserva o estilo FIKA; a identidade dessas classes fica no popover). Alternativa "cor default do mod"
rejeitada: inventar cor pra classe configurada sem cor contraria o schema.

### CR-01-05 · B — Bug latente · 🟢 ✅ Aplicado

**Postfix não espelha o early-return do `AddPlayer` (netId duplicado com nickname diferente)**

**Local:** `modded/Client/Patches/ClassDetailLoadingPatch.cs` vs. FIKA `LoadingScreenUI.cs:99-101`

**Problema:** `AddPlayer` do FIKA é no-op se o netId já existe (não atualiza o texto), mas o Postfix roda e
aplicaria tint/popover do nickname do PARÂMETRO na row VELHA → identidade do player B na linha do player A.

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** no-op quando `nickTmp.text != nickname` (proxy do
early-return; `SetNickname` grava o texto cru — LoadingScreenPlayer.cs:24-27).

### CR-01-06 · E — Legibilidade · 🟢 ✅ Aplicado

**`id = ClassIdentities.Local()!;` seguido de `if (id == null)`**

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** variável local anulável + early-return; sem null-forgiving.

### CR-01-07 · B — Bug latente · 🟢 ✅ Aplicado

**Fallback `Identity ?? ClassIdentities.Local()` no `Show()` escolheria o dado errado**

**Local:** `modded/Client/Patches/ClassDetailLoadingPatch.cs` (`LoadingClassHover.Show`)

**Problema:** se `Identity` fosse null numa linha remota, o fallback mostraria a classe LOCAL — falha silenciosa
com dado plausível.

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** `Identity == null → return` (não mostrar nada).

### CR-01-08 · F — Melhoria · 🟢 ✅ Aplicado

**`FieldInfo` do `Nickname` resolvido por reflection a cada `AddPlayer`**

**Decisão:** `[x]` Aceitar sugestão — **Resolução:** cacheado em static na primeira resolução (o tipo da row é
sempre `LoadingScreenPlayer`), consistente com `PlayersField`/`IsScavGetter`.

### CR-01-09 · D — Arquitetura · 🟢 ⏭️ Rejeitado (aceito como está)

**`_lastLoadingScreen` retém referência forte à tela destruída entre raids**

**Decisão:** `[x]` Rejeitar (aceitar como dívida consciente) — retenção é o shell gerenciado de um MonoBehaviour
destruído (objeto nativo já liberado), intencional pro `ReferenceEquals` e substituída na próxima tela.
`WeakReference` seria purismo sem ganho mensurável.

## Verificações que passaram limpas (evidência no relatório do revisor)

Reciclagem de row (FIKA instancia/destrói, sem pooling) · thread-safety client (tudo main thread) · caminho
`CharacterData.PmcData.Info.Nickname` (precedente compilando) · gate scav (getter público) · delegação
`GroupsFor` (callers `PerkDiagnostics`/`BuildNotificationText` intactos) · aba CLASS 053/059 sem regressão
(wrapper preserva ordem de efeitos) · corner ícone ausente (null-safe, sem quad branco) · degradação solo ·
trânsito (mesmo Postfix) · dedup determinístico · tint pós-toggle F12 (tela efêmera) · fetch síncrono (PA-01-05).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-03 | Code review 01 criada (agente adversarial) — 0 🔴 · 3 🟡 · 6 🟢; 8 aplicados via `/apply-code-review`, 1 rejeitado com registro |
