# 051 — Levers da zona stances (Steady Arms + Tireless Arms)

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-07-03

## Visão geral

Dois efeitos de classe anunciados nos cards ficaram "em breve" porque caem na zona onde o **stances mod**
(`mods/stancesAndCameraPositionSPT4.0.11`) é autoridade única da stamina de braço: **Steady Arms** (Caçador —
fadiga de braço ao mirar ×0.65) e **Tireless Arms** (Tanque — braço não cansa segurando arma pesada). **Decisão
do usuário 2026-07-03: opção (a)** — coordenar via **hook de composição** no `StaminaController` do stances
(análise no [kickoff](051-stances-zone-levers-00-kickoff.md)): o stances expõe um multiplicador externo de dreno
e o CustomClasses o preenche por soft-detect, sem dependência hard entre os mods.

## Comportamento atual

- O stances neutraliza o Process/Consume vanilla do braço do player local e escreve `hands.Current` por frame
  segundo o multiplicador do cenário ativo (StandAds, ProneAds, HoldBreath…) — qualquer multiplicador aplicado
  por fora é sobrescrito no frame seguinte (evidência no kickoff).
- Os cards "Steady Arms" (Caçador) e "Tireless Arms" (Tanque) aparecem na aba CLASS como **"· em breve"**
  (desde `852a51c`); nenhum efeito de classe toca a stamina de braço hoje.

## Comportamento desejado

- **Com os dois mods ativos:**
  - Caçador mirando (cenários de ADS do stances): o dreno de braço fica **35% mais lento** (fator ×0.65).
  - Tanque com **arma pesada em mãos** (mesmo gate de LMG/HMG/GL do Bunker): o braço **não drena** (fator ×0).
  - Demais classes e situações: fator neutro (×1) — o comportamento do stances fica **idêntico** ao atual.
- **Composição, não substituição:** o fator de classe multiplica o dreno do cenário do stances (só o DRENO —
  nunca a recuperação); os multiplicadores próprios do stances continuam mandando.
- **Sem o stances instalado (ou versão sem o hook):** efeito inativo, sem crash, 1 log informativo — os cards
  permanecem "em breve" nesse cenário? Não: os cards saem do "em breve" quando o 051 entregar; a degradação sem
  stances é documentada como limitação (o efeito é uma colaboração entre os dois mods).
- Cards "Steady Arms"/"Tireless Arms" deixam de ser `pending` na entrega.

## Critérios de aceite

- [ ] Caçador em ADS com stances ativo: a barra de braço esvazia visivelmente mais devagar que numa classe sem o
      perk, no MESMO cenário/arma (comparável via overlay de debug do stances ou cronômetro).
- [ ] Tanque segurando LMG/HMG/GL com stances ativo: a barra de braço **não cai** (fator ×0) enquanto o gate de
      arma pesada estiver ativo; trocar pra arma leve restaura o dreno normal no mesmo frame/cenário seguinte.
- [ ] Classe sem os perks (ex. Furtivo): comportamento do stances **byte-idêntico** ao atual (fator neutro) —
      regressão zero nos 16 cenários.
- [ ] Stances ausente/desatualizado: CustomClasses não crasha, loga 1× "hook indisponível" e os efeitos ficam
      inativos.
- [ ] **Fika/multiplayer:** efeito 100% local (o `StaminaController` já é MainPlayer-only); host e cliente se
      comportam igual — validar como cliente.
- [ ] **Estado entre raids:** fator re-avaliado por frame a partir do estado atual (classe/arma/cenário) — raid1
      → exit → raid2 e morte/alt-F4 não vazam fator (sem cache raid-scoped).

## Corner cases

- [ ] **Troca rápida de arma** (pesada↔leve) durante o dreno: fator atualiza no próximo tick, sem "grudar" o ×0.
- [ ] **Hold breath** (cenários `*HoldBreath` do stances): o fator compõe também ali (revisão: SIM por
      construção — o fator entra no `delta` de DRENO, agnóstico ao cenário; recuperação nunca é tocada).
- [ ] **Arma estacionária montada** (NSV/AGS — cenário ActiveStance0 do stances): Tireless Arms NÃO aplica (o
      gate é "arma pesada EM MÃOS"; stationary não é FirearmController) — fora de escopo, comportamento do
      stances puro (revisão: corner adicionado).
- [ ] **Stances desligado mid-session** (config/contexto inativo → `ControllingHands=false`): vanilla volta a
      mandar; o CustomClasses NÃO aplica nada no caminho vanilla (fora de escopo) — sem efeito e sem crash.
- [ ] **Os dois perks na mesma situação impossível** (Caçador ≠ Tanque) — fator é por classe local; nunca compõe
      dois perks.
- [ ] **Ordem de carga dos plugins:** o hook é resolvido lazy (primeiro uso), não no Awake — independe da ordem
      BepInEx.

## Fora de escopo

- Mexer nos multiplicadores/cenários próprios do stances (F12 dele continua soberano).
- Aplicar os efeitos no caminho vanilla quando o stances não está instalado (documentado como limitação).
- Outras classes/efeitos.

## Referências

- [051-stances-zone-levers-00-kickoff.md](051-stances-zone-levers-00-kickoff.md) — análise de decisão (evidência
  do mecanismo do stances) e a escolha (a).
- `mods/stancesAndCameraPositionSPT4.0.11/modded/StaminaController.cs` · `Patches/StanceStaminaRecoveryPatch.cs`.
- Memória: `project_stances_mod` (estado do mod vizinho) · constraint de coordenação entre sessões.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-20 | Kickoff criado (redesign 11→6, ponta solta #6) |
| 2026-07-03 | Análise de decisão no kickoff; decisão do usuário: **(a) coordenar** |
| 2026-07-03 | Spec funcional criada via `/create-spec` com a decisão fixada |
| 2026-07-04 | Revisão `/review-spec` (inline) — semântica do fator fixada no DRENO (recuperação intocada, hold-breath incluso por construção) + corner de arma estacionária adicionado |
