# 019 — Chamber Check Ammo UI · Code Review 03

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional/técnica:** [019-checar-camara-ui-02-spec.md](./019-checar-camara-ui-02-spec.md)
**Reviews anteriores:** [04-code-review.md](./019-checar-camara-ui-04-code-review.md) (rodada 01) · [04-code-review-02.md](./019-checar-camara-ui-04-code-review-02.md) (rodada 02)
**Data:** 2026-07-26

> Cobre a reescrita completa do patch (v2.10.0/2.11.x → **v2.12.0**): trocou a abordagem de disparar
> `Player.OnShowAmmoDetails` por reflexão (achado CR-02-01 na rodada 02 — subscriber confirmado, sem exceção, mas
> o painel nunca renderizava no estande de tiro) por uma chamada **direta** a
> `Singleton<CommonUI>.Instance.EftBattleUIScreen.ShowAmmoDetails(...)`, replicando a implementação real e validada
> do `RealismMod` 0.14.8/SPT 3.11 (`ChamberCheckUIPatch.cs`, decompilado em `mods/RealismMod/Client/DLL
> descompilada/RealismMod/RealismMod/ChamberCheckUIPatch.cs:33-44`).

**Memória consultada:** snapshot de 2026-07-25 (Sessão 11 cont. 6) · pendência ativa: [P-11.6] 🔴 validar item 019
in-game — a v2.12.0 desta rodada ainda **não foi confirmada visualmente pelo usuário** no momento deste review
(troca de abordagem feita, teste ao vivo pendente de resultado).

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 1 · 🟢 Menores: 1 · Herdados resolvidos nesta reescrita: 4 · Total novo: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-03-01 | B — Bug latente | 🟡 | Arma de câmara múltipla (`folding=true`) com 1 bala mostra "1" em vez de "Full" | Pendente |
| CR-03-02 | F — Melhoria opcional | 🟢 | Comentário de classe ficou longo/narrativo — considerar mover histórico pra memória do mod | Pendente |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade/manutenção** · **F — Melhoria opcional**

## Impacto

- 🔴 Bloqueador · 🟠 Forte · 🟡 Médio · 🟢 Menor

---

## Pontos herdados da rodada 02 — status

- ✅ **CR-02-01** (🔴 painel não aparecia) — **causa raiz não confirmada com 100% de certeza**, mas a hipótese de
  ordering (`SetAiming`/`UpdatePanelsVisibility`) foi **refutada** pelo teste com sonda atrasada (+20 frames,
  mesmo resultado). O teste seguinte (dump da invocation list do evento) confirmou que `GamePlayerOwner.method_8`
  **estava** corretamente inscrito (`target=HideoutPlayerOwner`) e a chamada não lançava exceção — ou seja, a
  cadeia `OnShowAmmoDetails → method_8 → BattleUIScreenController.ShowAmmoDetails` executava sem erro, mas o
  `AmmoCountPanel.Show` real nunca era alcançável de fato pela instância visível (a mesma investigação com o
  RealismMod mostrou que a implementação de referência **evita esse caminho inteiro**, indo direto ao
  `Singleton<CommonUI>`). Como a reescrita **substitui completamente** o mecanismo suspeito, o achado fica
  **superado pela mudança de arquitetura** — não há mais um "ordering" a corrigir porque o caminho antigo não
  existe mais no código. Fechado por reescrita, não por fix pontual.
- ✅ **CR-02-02** (🟠 comentário sobre `MalfState` factualmente errado) — **resolvido fora do fluxo automatizado**:
  o comentário em [`ChamberCheckAmmoPatch.cs:40`](../../modded/Patches/ChamberCheckAmmoPatch.cs#L40) agora diz
  explicitamente *"OBRIGATÓRIO: CheckChamber() retorna true mesmo em malfunction (Player.cs:5834) — sem este guard
  o painel apareceria com a câmara emperrada"*, corrigindo a afirmação incorreta da rodada 01.
- ⬜ **CR-02-03** (🟡 `PROPRIEDADES.md` sem a seção "Weapon Inspection") — **ainda não resolvido**. Não fazia parte
  do escopo desta reescrita (troca de mecanismo de UI, não de config). Continua pendente — ver
  `mods/stancesAndCameraPositionSPT4.0.11/PROPRIEDADES.md`.
- ✅ **CR-02-04, CR-02-05, CR-02-06** (instrumentação `[DEBUG-cc01]`/`[DEBUG-cc02]` temporária) — **resolvidos fora
  do fluxo automatizado**: o arquivo foi inteiramente reescrito (`Write`, não `Edit` incremental) e não contém mais
  nenhuma linha `[DEBUG-cc01]`/`[DEBUG-cc02]`, a classe `AmmoCountPanelShowProbe`, nem o `DelayedShowProbe`/
  `StartCoroutine`. Confirmado: `grep -rn "DEBUG-cc0" mods/stancesAndCameraPositionSPT4.0.11/modded/` retorna vazio.

---

## Pontos novos

### CR-03-01 · B — Bug latente · 🟡

**Arma de câmara múltipla (`folding=true`) com 1 bala viva mostra "1" em vez de "Full"**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/ChamberCheckAmmoPatch.cs:59-72`](../../modded/Patches/ChamberCheckAmmoPatch.cs#L59)

**Problema:** o fix do `maxAmmoCount` (1→2, para corrigir "Empty" caindo sempre em "Full" — ver comentário em
`ChamberCheckAmmoPatch.cs:61-64`) resolve corretamente o caminho **não-folding**
(`AmmoCountPanel.GetAmmoCountByLevel`, `EFT.UI/AmmoCountPanel.cs:37-64`), mas introduz uma regressão no caminho
**folding** (`weapon.Chambers.Length > 1`, ex.: espingardas double-barrel como MTs-255/TOZ-106), que usa uma
fórmula de threshold diferente:

```csharp
// EFT.UI/AmmoCountPanel.cs:66-77
public static string GetAmmoCountByLevelForFoldingMechanismWeapon(int ammoCount, int maxAmmoCount)
{
    if (ammoCount >= maxAmmoCount) return GClass2348.Localized("Full");   // <- sem o "-1" da versão normal
    if (ammoCount == 0) return GClass2348.Localized("Empty");
    return ammoCount.ToString();
}
```

Com `ammoCount=1, maxAmmoCount=2` (nossos valores fixos): `1 >= 2` é falso, `1 == 0` é falso → cai no `return
ammoCount.ToString()` → **exibe literalmente `"1"`**, não `"Full"`. Antes do fix (com `maxAmmoCount=1`), o mesmo
cálculo daria `1 >= 1` → **"Full"** corretamente — ou seja, o fix da rodada 02 (que era necessário pro caminho
principal) quebrou especificamente o texto do caminho folding.

**Por que importa:** é um cenário de baixa frequência (só armas com múltiplas câmaras — double-barrel), mas viola
o critério de aceite original ("Com bala → painel mostra Full + nome da munição") especificamente nesse
sub-caso. O comportamento de "Empty" no folding continua correto (`0 >= 2`? não; `0==0`? sim → "Empty").

**Sugestão:** usar valores de `maxAmmoCount` diferentes por caminho — manter `2` para o caso não-folding (onde a
fórmula usa `-1`), mas passar `1` só quando `folding == true` (onde a fórmula não subtrai 1):

```csharp
int maxAmmoCount = folding ? 1 : 2;
if (round != null)
    screen.ShowAmmoDetails(1, maxAmmoCount, 2, GClass2348.Localized(round.Name), folding);
else
    screen.ShowAmmoDetails(0, maxAmmoCount, 2, null, folding);
```
Confirmar que `GetAmmoCountByLevelForFoldingMechanismWeapon(1, 1)` → `1>=1` → "Full" e `(0, 1)` → `0==0` →
"Empty" — ambos corretos com `maxAmmoCount=1` no caminho folding.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-03-02 · F — Melhoria opcional · 🟢

**Comentário de classe (`<summary>`) narra o histórico v1→v2 inteiro — considerar mover pra memória do mod**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/ChamberCheckAmmoPatch.cs:12-24`](../../modded/Patches/ChamberCheckAmmoPatch.cs#L12)

**Problema:** o XMLDoc da classe inclui a narrativa completa de por que a v1 (evento `OnShowAmmoDetails`) falhou e
por que a v2 usa `Singleton<CommonUI>` — informação valiosa, mas mistura "o quê o código faz" com "a história de
como chegamos aqui", que normalmente vive na memória do mod (`memory/sessions.md`) ou na própria review, não no
comentário inline.

**Por que importa:** puramente estilístico — não afeta o comportamento. O comentário atual não atrapalha a
leitura (é curto o suficiente), mas se crescer mais em futuras iterações vira ruído no arquivo de produção.

**Sugestão:** opcional — encurtar o `<summary>` pra descrever só o comportamento atual (chama
`EftBattleUIScreen.ShowAmmoDetails` direto, sem sync Fika) e mover o "porquê da v1 ter sido abandonada" pra
`memory/sessions.md` (que já vai registrar esta sessão via `/update-memory`) ou para esta própria review. Não
bloqueia o fechamento do item.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Verificado sem problema

- **`Singleton<CommonUI>.Instantiated` antes de `.Instance`** — padrão já estabelecido no resto do mod
  (`Networking/FikaSyncManager.cs:105,136,182`, `UI/OxygenUI.cs:32`) — consistente, sem risco de
  `NullReferenceException` se `CommonUI` ainda não existir (ex.: telas de menu antes de qualquer raid/hideout).
- **Guard `player.IsYourPlayer` continua obrigatório e agora é AINDA MAIS crítico** que na v1: como a chamada vai
  direto pro `Singleton<CommonUI>` (a tela do **cliente local**, não mais um evento por-instância do `Player`
  específico), sem esse guard um bot ou peer Fika checando a própria câmara forçaria o painel a aparecer na tela
  do jogador local. O guard está presente e na ordem certa (antes de resolver `screen`).
- **Reflexão residual (`Traverse.Create(__instance).Field<Player>("_player")`)** — não cacheada, mas é o mesmo
  padrão não-cacheado já usado em `ActionStancePatches.cs` (várias classes) neste mod; aceitável por não ser hot
  path (disparado por input, não por frame).
- **`try/catch` cobre o corpo inteiro** — nenhuma exceção pode escapar pro `CheckChamber()` nativo.
- **Nenhum resíduo de instrumentação** — confirmado por grep, sem `[DEBUG-cc0` no arquivo.
- **`AccessTools.Method` no roteador externo `Player.FirearmController.CheckChamber()`** — mesmo alvo já validado
  nas rodadas anteriores (não-obfuscado, sem overload ambíguo, cobre o AP-03 de virtual dispatch).

## Status

**Bloqueia fechamento do item?** Não — 0 🔴. CR-03-01 é um edge case raro (double-barrel) que pode ir pra um
`06-fix-NN` depois da validação principal, ou ser corrigido junto se o usuário preferir agora.

**Gate real para fechar o item 019 continua sendo o teste in-game** ([P-11.6]): confirmar que a v2.12.0
efetivamente mostra o painel "Full"/"Empty" ao checar a câmara — ainda não confirmado no momento deste review.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-26 | Code review 03 criada via `/code-review` — reescrita completa do patch (evento → chamada direta ao `EftBattleUIScreen`, inspirada no RealismMod). 4 achados da rodada 02 resolvidos (1 por reescrita, 3 pelo cleanup), 1 ainda pendente (`PROPRIEDADES.md`). 2 achados novos: regressão no texto "Full" pra armas folding (🟡), sugestão de mover histórico do comentário pra memória (🟢). |
