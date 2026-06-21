# 011 — Mount passivo sobre o vanilla · Review Técnica 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Spec técnica:** [011-mount-passivo-vanilla-02-spec-tech.md](011-mount-passivo-vanilla-02-spec-tech.md)
**Spec funcional:** [011-mount-passivo-vanilla-01-spec.md](011-mount-passivo-vanilla-01-spec.md)
**Data:** 2026-06-21

> Análise crítica da spec técnica. IDs `PA-01-MM` permanentes. Resolver 🔴 antes do `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 5 · 🟢 Menores: 1 · Total: 6

Sem bloqueadores — a spec é implementável. Os pontos são de **corretude/validação** (a maioria só confirmável in-game) e devem ser tratados durante o `/code-mod`.

## Categorias: A=Gap · B=Edge case · C=Erro de lógica · Impacto: 🔴/🟡/🟢

---

### PA-01-01 · C — Erro de lógica · 🟡

**Frequência de chamada do `method_11` não confirmada**

**Problema:** a detecção depende do EFT chamar `Player.FirearmController.method_11` continuamente. O único caller localizado é [Player.cs:12966](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L12966) (cálculo do `TurnAway`). Se esse caminho só roda sob certas condições (ex.: já perto de obstáculo), o passivo pode não "ligar" ao se aproximar, ou atualizar com baixa frequência.

**Por que importa:** se `method_11` não roda quando a arma está livre, a transição livre→encostado pode falhar/atrasar.

**Sugestão:** no `/code-mod`, logar (debug temporário) a taxa de chamada do Postfix em raid; se for esparsa/condicional, complementar a detecção com um tick leve (no `PassiveMountUI.Update`, que já roda por frame) que recomputa a partir do `WeaponRootAnim` quando `method_11` não rodou no último intervalo. O RealismMod usou o mesmo hook com sucesso, então é provável que baste — confirmar.

**Decisão:** `[x]` Aceitar sugestão (validar taxa no code-mod; ter fallback no tick da UI)

---

### PA-01-02 · A — Gap · 🟡

**"Passivo < ativo" não é garantido pelos valores fixos**

**Problema:** a spec fixa `Passive Recoil Multiplier = 0.7`. Mas a redução do **mount ativo (vanilla)** é interna (`GClass2667`/bônus nativos) e desconhecida — não há como provar, só pelos números, que o passivo (0.7) é mais fraco que o ativo. O AC3 ("passivo < ativo") pode falhar se o vanilla reduzir menos.

**Por que importa:** AC3 é critério de aceite; valores mal calibrados invertem a relação desejada.

**Sugestão:** tratar `0.7 / 0.65` como **pontos de partida** e calibrar in-game medindo recoil/sway montado (vanilla) vs. encostado (passivo), ajustando os multiplicadores até o passivo ficar perceptivelmente acima (mais fraco) do ativo. Registrar os valores finais na spec/`PROPRIEDADES.md`.

**Decisão:** `[x]` Aceitar sugestão (calibração in-game obrigatória antes de fechar)

---

### PA-01-03 · B — Edge case · 🟡

**`PassiveMountState.IsBracing` pode ficar preso quando `method_11` para de rodar**

**Problema:** o estado só é atualizado dentro do Postfix do `method_11`. Se o jogador **guarda a arma**, abre inventário, ou troca para item não-arma, `method_11` deixa de ser chamado e `IsBracing` fica no último valor (`true`) — ícone órfão e buffs potencialmente ativos sem contexto. Mesma classe do "heartbeat órfão" do áudio (Sessão 5).

**Por que importa:** ícone preso no HUD + buff fantasma fora de contexto.

**Sugestão:** no `PassiveMountUI.Update` (roda por frame) revalidar a cada frame: se `MainPlayer` sem `FirearmController` em mãos, ou `IsMountedState`/prone/sprint, ou se o último `method_11` foi há > ~0.3s, chamar `PassiveMountState.Reset()`. Também resetar no raid end (já previsto).

**Decisão:** `[x]` Aceitar sugestão (reset por revalidação no tick da UI + timeout)

---

### PA-01-04 · A — Gap · 🟡

**Integração com `StanceStaminaRecoveryPatch` está vaga**

**Problema:** a spec diz "estender o guard para poupar stamina quando `IsBracing`", mas não define o efeito concreto. O patch hoje retorna `__result = 5f` (regen) quando `IsMountedState`. Falta dizer o que fazer no passivo (regen menor? pausa do drain?).

**Por que importa:** sem definição, o implementador chuta; o passivo deve ser mais fraco que o ativo (AC3) também na stamina.

**Sugestão:** no passivo (`IsBracing && !IsMountedState && Passive Stamina Save`), aplicar um regen/economia **menor** que o do montado (ex.: `__result = 2.5f` vs. `5f` do ativo), atrás do toggle `Passive Stamina Save`. Detalhar isso no stub do `/code-mod`.

**Decisão:** `[x]` Aceitar sugestão (regen passivo ≈ metade do ativo, via toggle)

---

### PA-01-05 · C — Erro de lógica · 🟡

**Efeito real de recoil/sway não confirmado (só compile/enable [validado-004])**

**Problema:** `[validado-004]` garante que `AddRecoilForce(ref incomingForce)` e `ProceduralWeaponAnimation.ProcessEffectors` → `Breath.Intensity` **compilam e habilitam**, mas **não** que o multiplicador surte efeito: (a) o nome do parâmetro `incomingForce` precisa casar para o Harmony injetar; (b) `ProcessEffectors` também existe nas estratégias `GClass909-912` (que recebem a PWA) — patchar na PWA pode ter timing em que o sway já foi aplicado.

**Por que importa:** o passivo pode "ligar" (ícone) sem reduzir recoil/sway de fato.

**Sugestão:** no `/code-mod`, validar in-game o efeito (disparar/mirar encostado vs. livre). Se o sway não reduzir, avaliar patchar o ponto correto (a estratégia que chama `ApplyComplex/SimpleRotation`, ou o `Breath` antes do consumo). Confirmar o nome do parâmetro de `AddRecoilForce` no Assembly real.

**Decisão:** `[x]` Aceitar sugestão (validar efeito; ajustar ponto se necessário)

---

### PA-01-06 · A — Gap · 🟢

**Stub do `PassiveMountUI` é resumo, não completo**

**Problema:** §5 traz só um esqueleto do `PassiveMountUI` (sem `CreateGameObject`/`Update` completos).

**Por que importa:** o `/code-mod` precisa do detalhe para o ícone direcional + pulsar de alpha.

**Sugestão:** recuperar a implementação do `MountingUI`/`BattleUIScreenPatch` removidos (em `git show ebc2312^:mods/.../modded-beta/MountingUI.cs`) como base, adaptando: anexar ao gameObject do plugin, ler `PassiveMountState.Direction`, e remover qualquer resíduo de mount ativo/`MountState`.

**Decisão:** `[x]` Aceitar sugestão (reusar MountingUI antigo do git como base)

---

## Histórico

| Data | Evento |
|---|---|
| 2026-06-21 | Review técnica 01 criada via `/review-technical-spec` — 0 🔴, 5 🟡, 1 🟢 (todos aceitos para tratar no `/code-mod`) |
