# 019 — Spec (funcional + técnica) · Chamber Check Ammo UI

> **Data:** 2026-07-19<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [019-...-01-investigacao](./019-checar-camara-ui-01-investigacao.md), [010-manual-chambering](../010-manual-chambering/)<br>

---

## Objetivo

Ao **checar a câmara** in-raid (tecla de check chamber), exibir no HUD **o mesmo painel** do check-carregador
(`AmmoCountPanel`) mostrando **se há bala e qual é**. Hoje o vanilla não mostra nada no HUD ao checar a câmara
(só marca o estado pra tela de inventário) — ver investigação 01.

## Decisões travadas (com o usuário)

1. **Linha de cima = formato nativo.** Reutiliza o evento `Player.OnShowAmmoDetails` (sem tocar no
   `AmmoCountPanel`). Com bala → `"Full"` + nome da munição. Vazia → `"Empty"` (sem 2ª linha).
2. **Câmara vazia mostra "Empty"** (não fica em silêncio) — útil junto do Manual Chambering (010).
3. **Toggle F12** (bool, default **true**), seção nova **"Weapon Inspection"**.
4. **Sem gate de skill** — a câmara é 1 bala e o jogador está olhando; sempre exibe o tipo.
5. **Sem sync Fika** — UI local (só quem checa vê).

## Comportamento

`PatchPostfix` em `EFT.Player.FirearmController.CheckChamber()`. Após o check nativo rodar:

- **Guardas (não faz nada se qualquer falhar):** toggle F12 on; `__result == true`; `player != null`,
  `player.IsYourPlayer`, `player.FirstPersonPointOfView`; `weapon.HasChambers`; `weapon.MalfState.State == None`.
- **Com bala** (`Chambers[0].ContainedItem is AmmoItemClass round && !round.IsUsed`):
  `OnShowAmmoDetails.Invoke(1, 1, 2, GClass2348.Localized(round.Name), false)` → painel "Full" + nome.
- **Vazia:** `OnShowAmmoDetails.Invoke(0, 1, 2, null, false)` → painel "Empty".
- Invocação por reflexão (`Traverse.Create(player).Field("OnShowAmmoDetails")`), pois o evento é privado para
  invocação externa — mesmo padrão interno do `CheckAmmo`. Player obtido via
  `Traverse.Create(fc).Field<Player>("_player").Value` (padrão do `ManualChamberingPatches.cs`).

## Critérios de aceite

- [ ] Checar a câmara com bala → painel on-screen mostra "Full" + **nome da munição** (mesma UI do check-carregador).
- [ ] Checar a câmara vazia → painel mostra "Empty".
- [ ] Com o toggle F12 **off** → comportamento vanilla (nenhum painel extra).
- [ ] Só o **jogador local** vê (bot/peer Fika não dispara; sem pacote de rede).
- [ ] Munição **localizada** no idioma do jogo (usa `GClass2348.Localized`, como o check-carregador).
- [ ] Não quebra o check nativo (postfix puro, não altera `__result` nem o fluxo).

## Corner cases

- [ ] **Revólver:** `CheckChamber()` já retorna `false` para `RevolverItemClass` → não dispara (aceitável; o tambor
      mostra as balas de outra forma).
- [ ] **Malfunction:** `MalfState.State != None` → não dispara (a câmara não é "lida" limpa; evita texto enganoso).
- [ ] **Arma sem câmara** (`HasChambers == false`): não dispara.
- [ ] **Estojo deflagrado na câmara** (`round.IsUsed == true`): trata como vazia → "Empty".
- [ ] **Múltiplas câmaras** (double-barrel, ex.: MP-43): o patch **itera** as câmaras e usa a **primeira bala
      viva** encontrada (não `Chambers[0]` fixo) → detecta bala em qualquer cano; mostra "Full" + o tipo dessa
      bala. Limitação conhecida: o `AmmoCountPanel` arredonda `count >= max-1` para "Full", então 1-de-2 e 2-de-2
      canos exibem ambos "Full" (o painel nativo não expõe contagem fina de câmara). Aceitável — o valor é o
      **tipo** da bala; a distinção 1/2 vs 2/2 fica fora do MVP.
- [ ] **Trocar de arma / soltar no meio:** postfix é stateless (dispara e esquece) — sem estado preso.
- [ ] **Câmara vazia via item 010 (Manual Chambering):** o "Empty" deve aparecer corretamente (é o caso de uso
      mais valioso — saber se precisa dar rack no ferrolho).

## Teste in-game prioritário (gate humano)

> **Suposição load-bearing:** o branch "Empty" só dispara se `CheckChamber()` retornar `true` com câmara vazia.
> Pelos fatos do Assembly, câmara vazia **não** está entre os casos de retorno-`false` (revólver / gatilho / stationary
> / malfunction), então deve retornar `true`. **Validar no jogo primeiro:** arma de câmara vazia (via Manual
> Chambering do 010) → checar câmara → o painel "Empty" aparece? Se não aparecer, o `CheckChamber` está devolvendo
> `false` com câmara vazia e o guard `__result` precisa afrouxar para esse caso.

## Implementação

- Novo `Patches/ChamberCheckAmmoPatch.cs` (`ModulePatch`).
- Toggle F12 em `Plugin.cs`: seção "Weapon Inspection", `Show Chamber Ammo On Check` (bool, default true).
- `SafeEnable` do patch no bloco de enable do `Awake` (padrão dos outros patches).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Spec criada a partir da investigação 01 (GO). Decisões: UI nativa "Full"/"Empty" + nome, toggle F12, sem skill/Fika. |
