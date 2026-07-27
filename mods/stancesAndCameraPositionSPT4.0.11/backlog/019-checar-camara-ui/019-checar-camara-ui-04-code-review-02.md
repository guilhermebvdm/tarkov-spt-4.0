# 019 — Chamber Check Ammo UI · Code Review 02

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec funcional/técnica:** [019-checar-camara-ui-02-spec.md](./019-checar-camara-ui-02-spec.md)
**Review anterior:** [019-checar-camara-ui-04-code-review.md](./019-checar-camara-ui-04-code-review.md) (rodada 01, sem sufixo `-NN` — tratada aqui como round 01)
**Data:** 2026-07-26

> Análise crítica do código atual de `ChamberCheckAmmoPatch.cs`, motivada pelo teste in-game ao vivo do gate humano
> **[P-11.6]** (memória do mod, Sessão 11 cont. 6): o usuário testou câmara cheia e vazia em v2.11.1 e **o painel não
> apareceu em nenhum dos dois casos**. Este review cobre (a) o código de produção existente desde a v2.10.0 — não
> re-analisado a fundo na rodada 01 à luz do bug real agora observado — e (b) a instrumentação de diagnóstico
> `[DEBUG-cc01]` adicionada nesta sessão (v2.11.1/2.11.2) para achar a causa.

**Memória consultada:** snapshot de 2026-07-25 (Sessão 11 cont. 6) · pendências que afetam: **[P-11.6] 🔴 validar
item 019 in-game — ATIVA, é exatamente o bug sendo investigado neste review** · [P-11.7] 🟡 subir versão ao
servidor (não afetada por este review).

## Resumo

> 🔴 Bloqueadores: 1 · 🟠 Fortes: 1 · 🟡 Médios: 1 · 🟢 Menores: 3 · Total: 6

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | C — Gap vs. spec | 🔴 | Painel não aparece: `show()` roda DEPOIS que a operação de câmara já começou | Sob investigação (hipótese líder, teste ao vivo em andamento) |
| CR-02-02 | E — Legibilidade | 🟠 | Comentário da review 01 sobre `MalfState` está factualmente errado — guard não é redundante | Pendente |
| CR-02-03 | D — Arquitetura/convenção | 🟡 | `ConfigEntry` "Show Chamber Ammo On Check" nunca foi documentada em `PROPRIEDADES.md` | Pendente |
| CR-02-04 | E — Legibilidade | 🟢 | Instrumentação `[DEBUG-cc01]` é código temporário e precisa de remoção explícita marcada | Pendente |
| CR-02-05 | B — Bug latente | 🟢 | `DelayedShowProbe` não guarda contra fim de raid nem GameWorld inválido durante os 20 frames de espera | Pendente |
| CR-02-06 | F — Melhoria opcional | 🟢 | Log `[DEBUG-cc01]` do valor bruto de `round.Name` (não localizado) pode confundir leitura do log | Pendente |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade/manutenção** · **F — Melhoria opcional**

## Impacto

- 🔴 Bloqueador · 🟠 Forte · 🟡 Médio · 🟢 Menor

---

## Pontos

### CR-02-01 · C — Gap vs. spec · 🔴

**Painel não aparece: `show()` roda DEPOIS que a operação de câmara já começou, ao contrário do nativo `CheckAmmo()`**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/ChamberCheckAmmoPatch.cs:29-95`](../../modded/Patches/ChamberCheckAmmoPatch.cs#L29)

**Problema:** o patch é um `[PatchPostfix]` no método **roteador** `Player.FirearmController.CheckChamber()`
(`Player.cs:13902`). O roteador só faz:

```csharp
public virtual bool CheckChamber()
{
    if (Blindfire) return false;
    if (_player._leftHandController.InAction) return false;
    if (Item is RocketLauncherItemClass) return false;
    _player.RemoveLeftHandItem(3f);
    if (_player.MovementContext.IsInMountedState) _player.MovementContext.StartExitingMountedState();
    return CurrentOperation.CheckChamber();   // Player.cs:13921
}
```

Ou seja, quando nosso Postfix roda, `CurrentOperation.CheckChamber()` **já terminou por completo** — incluindo, na
implementação padrão (`Player.cs:5795-5834`):

```csharp
public override bool CheckChamber()
{
    ...
    if (Weapon_0.MalfState.State == Weapon.EMalfunctionState.None)
    {
        ...
        SetAiming(isAiming: false);              // Player.cs:5815
        FirearmsAnimator_0.CheckChamber();
        Player_0.InventoryController.CheckChamber(Weapon_0, status: true);
        RunUtilityOperation(GClass2038.EUtilityType.CheckChamber);   // Player.cs:5818 — inicia a "operação utilitária"
    }
    ...
    return true;
}
```

Compare com o nativo `FirearmController.GClass20xx.CheckAmmo()` (check de **carregador**, `Player.cs:5754-5793`),
que É o modelo que copiamos:

```csharp
if (Player_0.FirstPersonPointOfView)
{
    ...
    Player_0.OnShowAmmoDetails?.Invoke(...);     // Player.cs:5770 — MOSTRA O PAINEL AQUI
    Player_0.InventoryController.StrictCheckMagazine(currentMagazine, status: true);
}
SetAiming(isAiming: false);                      // Player.cs:5773 — só DEPOIS
FirearmsAnimator_0.CheckAmmo();
...
RunUtilityOperation(GClass2038.EUtilityType.CheckMagazine);   // Player.cs:5781 — e a operação começa por último
```

O nativo mostra o painel **antes** de `SetAiming(false)` e antes de `RunUtilityOperation`. Nosso patch, por ser
Postfix no roteador externo, invoca `show()` **depois** que ambos já rodaram dentro do `CheckChamber()` da
operação. `SetAiming(false)`/`RunUtilityOperation` disparam transições de estado de UI (ex.:
`BattleUIScreen.UpdatePanelsVisibility(false)` em `EFT.UI/BattleUIScreen-2.cs:516-523`, que chama
`_ammoCountPanel.Hide()` explicitamente) que plausivelmente rodam num frame seguinte e escondem qualquer coisa que
tenha acabado de aparecer — exatamente o sintoma reportado (o teste ao vivo confirmou, via `[DEBUG-cc01]`, que
`show()` é chamado com sucesso, sem exceção, mas nada aparece na tela).

**Por que importa:** é o **critério de aceite central** do item ("Checar a câmara com bala → painel mostra Full +
nome") não sendo cumprido em NENHUM cenário (nem cheia, nem vazia) — confirmado pelo teste do usuário nesta sessão.
O item não pode ser considerado entregue enquanto isso não for corrigido.

**Sugestão:** inverter a ordem para espelhar o nativo — mostrar o painel **antes** de a operação começar, não
depois. Duas rotas possíveis:
1. Converter para `[PatchPrefix]` no mesmo roteador (`Player.FirearmController.CheckChamber()`), replicando as
   3 guardas do próprio roteador (`Blindfire`, `_leftHandController.InAction`, `Item is RocketLauncherItemClass`)
   antes de chamar `show()` — assim a exibição acontece antes de `CurrentOperation.CheckChamber()` rodar
   `SetAiming`/`RunUtilityOperation`, igual ao `CheckAmmo`.
2. Alternativa mais simples de validar primeiro: mover só a chamada de `show()` para dentro do `DelayedShowProbe`
   (hoje instrumentação de diagnóstico) com um atraso pequeno, **depois** que a transição de UI se estabilizar —
   mas isso é gambiarra; a rota 1 é a correta a médio prazo caso o teste ao vivo confirme a hipótese.

**Status desta sessão:** a build v2.11.2 (com a sonda `[DEBUG-cc01]`/`DelayedShowProbe`) está sendo testada ao vivo
pelo usuário no momento deste review. Se o painel aparecer no log "ATRASADO (+20 frames)" e não no "imediato",
confirma a hipótese acima e a Sugestão 1 deve ser implementada como fix real (`06-fix-NN.md`). Se NENHUM dos dois
aparecer, esta hipótese cai e o próximo suspeito é o `BattleUIScreen`/`AmmoCountPanel` específico do contexto do
Estande de Tiro (`GClass3867` em `EFT.UI/EftBattleUIScreen.cs:220-318`, variante de `HideoutPlayerOwner`) não estar
com os panels vinculados da mesma forma que numa raid normal — precisaria reproduzir numa raid real (não no
estande) para isolar a variável.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-02-02 · E — Legibilidade/manutenção · 🟠

**Comentário herdado da review 01 está factualmente errado — o guard de `MalfState` NÃO é redundante**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/ChamberCheckAmmoPatch.cs:42`](../../modded/Patches/ChamberCheckAmmoPatch.cs#L42), contradizendo [`019-checar-camara-ui-04-code-review.md:36-37`](./019-checar-camara-ui-04-code-review.md)

**Problema:** a review 01 registrou: *"`MalfState` guard é redundante (malf já dá `__result==false`) mas
defensivo e inofensivo — mantido."* Isso é **incorreto**. Lendo `Player.cs:5809-5834` (`FirearmController`
padrão), o método `CheckChamber()` da operação tem dois branches (`MalfState.State == None` e o `else` de
malfunction) e **ambos terminam em `return true;`** na linha 5834 — malfunction **não** faz `__result` virar
`false`. O guard em `ChamberCheckAmmoPatch.cs:42` (`if (weapon.MalfState.State != None) return;`) é, portanto, o
**único** motivo de o painel não aparecer incorretamente durante uma malfunção — ele é **necessário**, não
redundante.

**Por que importa:** um mantenedor futuro lendo só a review 01 (achado "redundante, inofensivo") pode remover essa
guarda achando-a supérflua, reintroduzindo o bug de "mostrar Full/Empty com a câmara emperrada/malfuncionando"
que o guard existe justamente para prevenir. Reviews são artefatos imutáveis (não se edita a 01 retroativamente,
`repo-workflow-best-practices` §5) — a correção precisa viver aqui.

**Sugestão:** nenhuma mudança de código necessária (o guard já está certo). Apenas registrar a correção factual
nesta review (feito) e, opcionalmente, trocar o comentário inline em `ChamberCheckAmmoPatch.cs:42` de "câmara não
é 'lida' limpa em malf" para algo que deixe explícito que **sem esta guarda o método ainda retornaria `true` e
mostraria o painel** — ex.: `// OBRIGATÓRIO: CheckChamber() retorna true mesmo em malfunction (Player.cs:5834) — sem este guard o painel apareceria com a câmara emperrada.`

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-02-03 · D — Arquitetura/convenção · 🟡

**`ConfigEntry` "Show Chamber Ammo On Check" nunca foi documentada em `PROPRIEDADES.md`**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Plugin.cs:308-315`](../../modded/Plugin.cs#L308) · ausente em [`mods/stancesAndCameraPositionSPT4.0.11/PROPRIEDADES.md`](../../PROPRIEDADES.md)

**Problema:** a seção nova "Weapon Inspection" (criada para este item, `Plugin.cs:305-315`) não tem entrada
correspondente em `PROPRIEDADES.md` — o grep pela chave `Show Chamber Ammo On Check` e pelo título da seção
`Weapon Inspection` não retorna nada no arquivo. As seções existentes (A–F) cobrem todos os outros grupos do F12
(`Índice por tema`, linha 12), mas "Weapon Inspection"/item 019 não aparece em nenhuma delas.

**Por que importa:** viola a convenção documentada em `repo-workflow-best-practices` §7 ("Toda nova `ConfigEntry`
exposta no F12 exige update em `PROPRIEDADES.md`"). `PROPRIEDADES.md` é a fonte única de verdade sobre o que existe
no F12 — um usuário ou sessão futura consultando esse arquivo pra saber "o que essa opção faz" não vai encontrar a
nova config, apesar dela já estar ativa desde a v2.10.0.

**Sugestão:** adicionar uma nova seção em `PROPRIEDADES.md` (ex.: sob "F — Respiração, UI e debug" ou uma nova "G —
Inspeção de arma") com: Nome EN `Show Chamber Ammo On Check`, tradução pt-BR, Tipo `bool`, Padrão `true`, Tooltip
pt-BR (copiar a `ConfigDescription` de `Plugin.cs:313`). Atualizar também o "Índice por tema" (linha 12) e "Ordem
no menu F12" (linha 272) se aplicável.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-02-04 · E — Legibilidade/manutenção · 🟢

**Instrumentação `[DEBUG-cc01]` é código de diagnóstico temporário e precisa de remoção rastreável**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/ChamberCheckAmmoPatch.cs:34,35,36,39,40,41,42,45,46,49,50,74,80,85-92,97-106`](../../modded/Patches/ChamberCheckAmmoPatch.cs#L34)

**Problema:** ~11 chamadas `Plugin.Logger.LogInfo("[DEBUG-cc01] ...")` e o método inteiro `DelayedShowProbe` (mais
o `using System.Collections;` que ele exige) foram adicionados nesta sessão só para diagnosticar CR-02-01. Nenhum
deles é código de produção — todos violam `csharp-mod-best-practices` §8 ("Never log per-frame at Info or higher")
na prática (cada input de check-chamber agora gera ~8 linhas de log) e não têm gate de config nem nível `Debug`.

**Por que importa:** se este arquivo for commitado/lançado assim, todo usuário passa a ter o BepInEx console
inundado de `[DEBUG-cc01]` a cada checagem de câmara — ruído puro em produção, e overhead de log desnecessário.

**Sugestão:** remover **todas** as linhas `[DEBUG-cc01]` e o método `DelayedShowProbe` (com o `using
System.Collections`) assim que o CR-02-01 for resolvido e a hipótese confirmada/refutada — não misturar com o fix
real num mesmo commit de release. `grep -rn "DEBUG-cc01" mods/stancesAndCameraPositionSPT4.0.11/modded/` deve
retornar vazio antes de fechar o item 019 e antes de qualquer bump de versão que não seja explicitamente marcado
como build de diagnóstico (como já foi feito para 2.11.1/2.11.2).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-02-05 · B — Bug latente · 🟢

**`DelayedShowProbe` não guarda contra fim de raid/GameWorld inválido durante a espera de 20 frames**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/ChamberCheckAmmoPatch.cs:97-106`](../../modded/Patches/ChamberCheckAmmoPatch.cs#L97)

**Problema:** a coroutine é iniciada em `Plugin.Instance` (correto — sobrevive entre raids, não é destruído,
`csharp-mod-best-practices` §2), mas não verifica `Singleton<GameWorld>.Instantiated` nem se o `player`/`show`
ainda são válidos antes de invocar `show(...)` ~0.33s depois. Se o jogador extrair ou morrer exatamente nessa
janela, o delegate capturado pode apontar para um `GamePlayerOwner`/`BattleUIScreenController` já descartado.

**Por que importa:** o `try/catch` interno (`ChamberCheckAmmoPatch.cs:100-105`) já absorve qualquer exceção daí, e
a janela é curtíssima (~0,33s) — risco realista é baixíssimo. Como é código de diagnóstico temporário (ver
CR-02-04) que será removido, não vale um fix dedicado; registrado só para constar caso a rota 2 da Sugestão do
CR-02-01 (usar delay na produção) seja adotada — nesse caso este guard passaria a ser obrigatório.

**Sugestão:** se `DelayedShowProbe` (ou algo com o mesmo padrão) sobreviver como código de produção, adicionar
`if (!Comfort.Common.Singleton<GameWorld>.Instantiated) yield break;` antes de invocar `show(...)`. Como
instrumentação temporária, nenhuma ação é necessária além da remoção via CR-02-04.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-02-06 · F — Melhoria opcional · 🟢

**Log de diagnóstico imprime `round.Name` bruto (não localizado), diferente do texto realmente mostrado ao jogador**

**Local:** [`mods/stancesAndCameraPositionSPT4.0.11/modded/Patches/ChamberCheckAmmoPatch.cs:74`](../../modded/Patches/ChamberCheckAmmoPatch.cs#L74)

**Problema:** `Plugin.Logger.LogInfo($"... round={round.Name}")` loga o valor cru de `round.Name` (ex.:
`56d59d3ad2720bdb418b4577 Name`, uma chave de localização, não texto legível), enquanto a chamada real usa
`GClass2348.Localized(round.Name)` (linha 76). No log capturado durante o teste, isso já causou confusão leve na
leitura ("round=56d59d3ad2720bdb418b4577 Name" parece um erro de concatenação à primeira vista, mas é só a chave
de locale não resolvida).

**Por que importa:** puramente cosmético — não afeta o comportamento do patch, só a legibilidade do log de
diagnóstico durante esta investigação pontual.

**Sugestão:** se mais alguma rodada de diagnóstico for necessária, trocar para
`GClass2348.Localized(round.Name)` no log também, por consistência. Não vale a pena corrigir isoladamente já que
todo o bloco `[DEBUG-cc01]` será removido (CR-02-04).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Verificado sem problema (reconfirmado nesta rodada)

- **Alvo do patch não é obfuscado nem virtual-dispatch-ambíguo:** `Player.FirearmController.CheckChamber()` é um
  método público, nomeado, no roteador externo (não uma classe `GClassNNNN` aninhada) — o mesmo nível que resolveu
  o bug histórico do F4/`SetTriggerPressed` (AP-03). Não há overload ambíguo (confirmado por leitura direta do
  Assembly: só existe uma assinatura `CheckChamber()` na classe externa).
- **Filtro Fika (`IsYourPlayer`/`FirstPersonPointOfView`)** continua correto — bots e peers observados não
  disparam a UI local.
- **`try/catch` cobre todo o corpo do Postfix** — nenhuma exceção do patch pode derrubar o `CheckChamber()` nativo.
- **Nenhuma mutação de estado do jogo** — o patch é estritamente de leitura + UI; `__result`/fluxo nativo
  intocados.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-26 | Code review 02 criada via `/code-review` — motivada pelo bug ao vivo do gate [P-11.6]. 1 🔴 (achado central, sob investigação simultânea), 1 🟠, 1 🟡, 3 🟢. |
