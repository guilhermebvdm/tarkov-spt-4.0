# stancesAndCameraPositionSPT4.0.11 — Review de Propriedades F12 · 01

**Mod:** stancesAndCameraPositionSPT4.0.11
**Fonte de verdade:** `modded/Plugin.cs` (chamadas `Config.Bind`) · **Doc espelho:** `PROPRIEDADES.md`
**Data:** 2026-07-09

> Revisão de **UX e organização** das propriedades do menu F12. IDs `MP-01-MM` permanentes. Aplicação no **`Plugin.cs`**; o `PROPRIEDADES.md` é regenerado depois.
> ⚠️ **Breaking change:** renomear seção/key descarta o valor salvo do usuário (BepInEx casa por `(seção, key)`). Marcado em cada achado. Memória consultada: snapshot Sessão 6 (2026-07-09) · pendências que afetam: nenhuma.

## Resumo

> 🔴 Bloqueadores: 1 · 🟠 Fortes: 3 · 🟡 Médios: 6 · 🟢 Menores: 2 · Total: 12
> Props no F12: **143** · das quais **23 MORTAS** (16%) · Seções: **23** · Dead-code puro (nem no F12): **9** · Breaking propostos: **3**

## Critérios avaliados

1. Ordem das seções (**ORD**) · 2. Distribuição/nome das seções (**SEC**) · 3. Alocação (**LOC**) · 4. Nome da prop (**NAM**) · 5. Tipo/edição (**TYP**) · 6. Tooltip (**TIP**) · 7. Props mortas (**DEAD**) · 8. Advanced (**ADV**)

## Impacto

- 🔴 **Bloqueador** · 🟠 **Forte** · 🟡 **Médio** · 🟢 **Menor**

## Panorama

- **Ordem real no F12 (descoberta):** 1. Manual Chambering · 2. Positions · 3. Settings · 4. General · 5. Advanced ADS Transitions · 6. ADS Default Values (Advanced) · 7. Default Hands/Arms (Advanced) · 8. Stance 0 · 9. Stance 1 · 10. Stance 2 · 11. Stance 3 · 12. Weapon Mount (Active) · 13. Weapon Mount (Passive) · 14. Stamina Management · 15. **9.** Respiração · 16. **10.** Barra de Oxigênio · 17. Animations & Transitions · 18. Movement & Inertia · 19. Action Stances · 20. **8.** Wiggle · 21. Tac Sprint · 22. Field of View · 23. Debug.
- **Seções com prefixo numérico inconsistente:** "8. Wiggle" aparece na **20ª** posição, "9. Respiração" na **15ª**, "10. Barra" na **16ª** — os números não têm relação com a ordem real.
- **Props mortas:** 23 (ver MP-01-01). A seção **"8. Wiggle" é inteiramente morta**; os 15 "ADS … Multiplier" de Stance 0/1/2 são mortos.
- **Divergência código × `PROPRIEDADES.md`:** o doc foi regenerado hoje e reflete o código — **mas** documenta as 23 mortas como se funcionassem (a doc não sabia que estavam mortas). Este review corrige isso.

## Índice

| ID | Cat | Impacto | Título | Breaking? | Status |
|---|---|---|---|---|---|
| MP-01-01 | DEAD | 🔴 | 23 propriedades no F12 não fazem nada (mortas) | não | Pendente |
| MP-01-02 | NAM | 🟠 | Eixos "Roll"/"Yaw" trocados nos keys de Stance/ADS — induz o usuário a erro | ⚠️ | Pendente |
| MP-01-03 | ORD | 🟠 | Ordem das seções não segue lógica; Manual Chambering (secundário) vem primeiro | ⚠️ | Pendente |
| MP-01-04 | SEC | 🟠 | Prefixos numéricos "8./9./10." enganosos (não batem com a posição real) | ⚠️ | Pendente |
| MP-01-05 | SEC | 🟡 | Nomes de seção genéricos ("Settings", "General", "Positions") | ⚠️ | Pendente |
| MP-01-06 | TIP | 🟡 | Idioma misto — tooltips e nomes em EN e pt-BR sem padrão | ⚠️(parcial) | Pendente |
| MP-01-07 | NAM | 🟡 | Rótulos legados: "Stance 2 - Custom" / "Stance 3 - Low Ready" contradizem a realidade | ⚠️ | Pendente |
| MP-01-08 | LOC | 🟡 | Velocidades de movimento espalhadas em 2 seções | ⚠️ | Pendente |
| MP-01-09 | ADV | 🟡 | Offsets de ADS perderam o liga/desliga (o toggle era a prop morta `Advanced ADS Transitions`) | não | Pendente |
| MP-01-10 | SEC | 🟢 | Seções Stance 0/1/2 encolhem muito após remover as mortas — reavaliar agrupamento | — | Pendente |
| MP-01-11 | DEAD | 🟢 | 9 campos dead-code puro (declarados, nunca bindados — nem aparecem no F12) | não | Pendente |
| MP-01-12 | TYP | 🟢 | `Movement Speed Multiplier` é `int` (%) enquanto os demais multiplicadores são `float` | não | Pendente |

---

## Achados

### MP-01-01 · DEAD — Propriedade morta · 🔴 Bloqueador

**23 propriedades aparecem no F12 e não fazem absolutamente nada**

**Local:** várias seções · [`Plugin.cs`](../stancesAndCameraPositionSPT4.0.11/modded/Plugin.cs) (linhas abaixo). Confirmado por varredura: o `.Value` de cada uma **nunca é lido** em `modded/`.

**Problema:** 23 `ConfigEntry` bindadas cujo valor nunca é consumido — restos de sistemas removidos (curva cinematográfica de wiggle, multiplicadores de ADS por stance, shoulder-throw):
- **Seção "8. Wiggle (Q/E) Dynamics" — INTEIRA (5):** `Animation Curve Duration` (L1100), `Stance Pitch/Yaw/Roll/Position Multiplier` (L1108/1116/1124/1132).
- **"ADS … Multiplier" das Stances 0/1/2 (15):** cada uma tem `ADS Pitch/Yaw/Roll Multiplier` + `ADS Pos Y/Z Multiplier` (Stance 0: L1141-1169; Stance 1: L1177-1205; Stance 2: L1213-1241).
- **`Advanced ADS Transitions`** (L663, seção homônima) — toggle do "shoulder throw" removido.
- **`ADS Transition Speed`** (L560, Settings) — duplicata morta de `Stance Transition Speed` (este cobre stance + ADS).
- **`Stance Change Sound Volume`** (L568, Settings).

**Por que importa (UX):** o usuário mexe nessas opções esperando efeito e **nada acontece** — a pior experiência possível de config (parece bug do mod). Poluem 16% do menu. A seção "8. Wiggle" é 100% fantasma.

**Sugestão:** **remover** as 23 `Config.Bind` + os campos `_Xxx`. Não é breaking no sentido funcional (removem-se opções que já não faziam nada; nenhum comportamento muda). Efeitos colaterais a tratar juntos: a seção "8. Wiggle" desaparece (MP-01-04 fica mais fácil); as seções Stance 0/1/2 encolhem (MP-01-10); e o toggle `Advanced ADS Transitions` sumir expõe MP-01-09 (os offsets ADS ficam sem gate). Aplicar este achado **antes** dos de organização.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### MP-01-02 · NAM — Nome enganoso · 🟠 Forte · ⚠️ BREAKING

**Os keys "Roll (Tombar Arma)" e "Yaw (Apontar Esq/Dir)" estão trocados em relação ao eixo real**

**Local:** todas as seções Stance (1/2/3) e "ADS Default Values" · ex.: [`Plugin.cs:799`](../stancesAndCameraPositionSPT4.0.11/modded/Plugin.cs#L799) (`Stance 1 Roll (Tombar Arma)` grava o campo `_Stance1HandsYawRotation`) e [`Plugin.cs:807`](../stancesAndCameraPositionSPT4.0.11/modded/Plugin.cs#L807) (`Stance 1 Yaw (Apontar Esq/Dir)` grava `_Stance1HandsRollRotation`). Mesmo cruzamento nas Stances 2/3 e no ADS.

**Problema:** o key diz "Roll" mas o valor alimenta a rotação de **Yaw**, e vice-versa. Quem quer inclinar a arma (roll) mexe no campo errado.

**Por que importa (UX):** o usuário configura o eixo errado e conclui que o mod está bugado. Nome de opção que mente é pior que tooltip ruim.

**Sugestão:** duas opções —
- **(a) Não-breaking:** corrigir só o **tooltip** para descrever o eixo real que aquele key controla (o valor salvo persiste). Menos ideal (o nome continua "errado"), mas zero perda de config.
- **(b) ⚠️ Breaking:** trocar os keys para bater com o eixo (`Roll` ↔ `Yaw`), corrigindo de vez. Descarta o valor salvo dessas entradas (voltam ao default). Recomendo (b) num release com nota de migração, já que os defaults dessas rotações são majoritariamente 0.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (b — breaking)
- `[ ]` Aceitar com modificação (ex.: a — só tooltip): _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-03 · ORD — Ordem de seções · 🟠 Forte · ⚠️ BREAKING (reordena binds)

**A ordem no F12 não segue lógica — a primeira seção é "Manual Chambering" (mecânica secundária)**

**Local:** ordem de `Config.Bind` no `Awake`/binds (`Plugin.cs`). Ordem atual no Panorama acima.

**Problema:** o ConfigurationManager ordena por descoberta. Hoje a 1ª seção é **Manual Chambering** (item 010, nicho), seguida de **Positions** (câmera) antes de **Settings** (o núcleo: ciclo de stances). Seções afins ficam distantes (Movement & Inertia na 18, Tac Sprint na 21; as speeds de Animations na 17).

**Por que importa (UX):** o usuário abre o F12 e a primeira coisa é uma mecânica de nicho; as opções principais (trocar de stance) estão no meio. Difícil formar um modelo mental.

**Sugestão:** reordenar os blocos de `Config.Bind` para um fluxo temático, ex.:
1. Stances (ciclo/hotkeys) → 2. Transição & câmera → 3. Poses (Stance 0/1/2/3) → 4. ADS → 5. Mount → 6. Stamina → 7. Movimento → 8. Mecânicas (Chambering, Action Stances) → 9. Respiração/UI → 10. Avançado/Debug.
Reordenar os binds muda a ordem no F12 (não descarta valores — só a posição), mas mover um bind **junto com** um rename conta como breaking. Fazer em conjunto com MP-01-04/05.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-04 · SEC — Nome de seção · 🟠 Forte · ⚠️ BREAKING

**Prefixos numéricos "8./9./10." enganam — não têm relação com a posição real**

**Local:** `8. Wiggle (Q/E) Dynamics (Stance Based)` (L1100), `9. Respiração (Hold Breath)` (L969), `10. Barra de Oxigênio (UI)` (L1014).

**Problema:** só 3 seções são numeradas, e os números (8/9/10) não batem com a ordem real (20ª/15ª/16ª). Resíduo de uma numeração antiga. Além disso a "8. Wiggle" é toda morta (MP-01-01) e vai sumir.

**Por que importa (UX):** numeração parcial e furada sugere seções faltando/desordenadas.

**Sugestão:** remover os prefixos `8.`/`9.`/`10.` → `Respiração (Hold Breath)` e `Barra de Oxigênio (UI)` (a "8. Wiggle" some com MP-01-01). ⚠️ Breaking (renomeia seção). Migração: nota no changelog; os valores de Respiração/Oxigênio voltam ao default (poucos e sensatos).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-05 · SEC — Nome de seção · 🟡 Médio · ⚠️ BREAKING

**"Settings", "General" e "Positions" são genéricos demais**

**Local:** `Settings` (L449+), `General` (L528+), `Positions` (L410+).

**Problema:** "Settings" e "General" não dizem o que contêm (ambos têm coisas de stance/transição); "Positions" (offset de câmera) é ambíguo.

**Por que importa (UX):** o usuário não sabe em qual das duas seções genéricas procurar.

**Sugestão (⚠️ breaking):** `Settings` → `Stance Cycle & Hotkeys`; `General` → `Transitions & Kick`; `Positions` → `Camera Position`. Ou, se preferir pt-BR (ver MP-01-06): `Ciclo & Teclas`, `Transições & Kick`, `Posição da Câmera`. Fazer junto com MP-01-06.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-06 · TIP/SEC — Idioma · 🟡 Médio · ⚠️ BREAKING (só se renomear seções)

**Idioma misto: metade das seções/tooltips em inglês, metade em pt-BR**

**Local:** nomes/tooltips EN (Settings, General, Positions, Stances, ADS, Movement, FOV, Tac Sprint, Wiggle, Hold Breath, Oxygen UI) vs pt-BR (Manual Chambering, Mount, Stamina Management, Action Stances).

**Problema:** inconsistência de idioma dentro do mesmo menu.

**Por que importa (UX):** parece remendado; dificulta leitura para o público-alvo.

**Sugestão:** escolher **um idioma** e padronizar. O repo prefere pt-BR para o usuário (convenção de tooltip em `create-technical-spec`), mas o mod é distribuído publicamente (talvez EN seja melhor alcance) — **decisão sua**. Tooltips: não-breaking (edita `ConfigDescription`). Renomear seções para pt-BR: breaking (junta com MP-01-04/05). Recomendo: **tooltips pt-BR** (não-breaking) primeiro; nomes de seção conforme a decisão de idioma.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (padronizar pt-BR)
- `[ ]` Aceitar com modificação (padronizar EN / outra): _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-07 · NAM — Rótulo legado · 🟡 Médio · ⚠️ BREAKING

**"Enable Stance 2 - Custom in Cycle" e "Enable Stance 3 - Low Ready in Cycle" contradizem a realidade**

**Local:** [`Plugin.cs:471`](../stancesAndCameraPositionSPT4.0.11/modded/Plugin.cs#L471) e [`Plugin.cs:480`](../stancesAndCameraPositionSPT4.0.11/modded/Plugin.cs#L480). Hoje Stance 2 = **Low Ready** e Stance 3 = **Custom** (troca do 06-fix-01), mas esses dois keys mantêm os nomes antigos.

**Problema:** o key diz "Stance 2 - Custom" mas a Stance 2 é Low Ready. Contradição direta com as próprias seções "Stance 2 - Low Ready"/"Stance 3 - Custom".

**Por que importa (UX):** confunde qual stance a opção controla.

**Sugestão (⚠️ breaking):** `Enable Stance 2 - Low Ready in Cycle` e `Enable Stance 3 - Custom in Cycle`. Migração: reset ao default (`true`), inócuo.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-08 · LOC — Alocação · 🟡 Médio · ⚠️ BREAKING (troca de seção)

**Velocidades de movimento espalhadas em duas seções**

**Local:** `Crouch Speed Multiplier` (L1057) e `Lean Speed Multiplier` (L1248) em **"Animations & Transitions"**; `Walk/Sprint Speed Multiplier` (L1074/1082) e `Inertia` em **"Movement & Inertia"**.

**Problema:** conceitos irmãos (velocidade de movimento/animação) em seções diferentes.

**Por que importa (UX):** para ajustar "quão rápido eu me movo", o usuário precisa caçar em duas seções.

**Sugestão:** consolidar as velocidades numa seção só (ex.: mover Crouch/Lean para "Movement & Inertia", ou criar "Movement & Animation Speed"). ⚠️ Breaking (troca a seção da key). Fazer junto com a reorganização (MP-01-03).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-09 · ADV — Comportamento após remover a morta · 🟡 Médio

**Os offsets de ADS perdem o liga/desliga ao remover a prop morta `Advanced ADS Transitions`**

**Local:** `_ADSHands*` lidos em [`StanceManager.cs:722-730`](../stancesAndCameraPositionSPT4.0.11/modded/StanceManager.cs#L722); o gate era o `_EnableAdvancedADSTransitions` (morto, MP-01-01).

**Problema:** hoje os offsets de ADS (seção "ADS Default Values (Advanced)") são aplicados **sempre** — o toggle que deveria ligá-los já está morto. Ao remover o toggle (MP-01-01), o comportamento não muda (continua sempre-aplica), mas some qualquer possibilidade de desligar.

**Por que importa:** os defaults desses offsets são 0 (sem efeito visível), então na prática é inócuo hoje; mas se o usuário setar um offset, não terá como desligar em bloco.

**Sugestão:** decidir — (a) **manter sem gate** (os offsets valem sempre; default 0 = sem efeito; mais simples), ou (b) **reconectar** um toggle vivo "Enable ADS Position Offsets" que realmente gateie `_ADSHands*`. Recomendo (a) pela simplicidade, já que "Reset Positions When Aiming" (vivo, L676) já é o controle principal de ADS.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar (a — sem gate)
- `[ ]` Aceitar com modificação (b — toggle vivo): _________________
- `[ ]` Rejeitar: _________________

---

### MP-01-10 · SEC — Distribuição após limpeza · 🟢 Menor

**As seções Stance 0/1/2 ficam pequenas/vazias após remover as mortas**

**Local:** Stance 0 (L1141-1169 morrem → sobra só velocidade/stamina), Stance 1/2 (perdem os 5 ADS multipliers cada).

**Problema:** removidos os ADS multipliers mortos, a "Stance 0 - Vanilla" fica só com 3 opções de velocidade; as demais stances ficam mais enxutas.

**Sugestão:** após MP-01-01, reavaliar se "Stance 0 - Vanilla" (que não tem pose própria) merece seção — talvez mover suas 3 opções de velocidade/stamina para a seção de movimento/stamina. Reavaliar na aplicação; não é urgente.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

### MP-01-11 · DEAD — Dead code puro · 🟢 Menor

**9 campos declarados mas nunca bindados — dead code (nem aparecem no F12)**

**Local:** [`Plugin.cs:149,244,252,260,262,263,264,265,267`](../stancesAndCameraPositionSPT4.0.11/modded/Plugin.cs#L149): `_AffectStanceTransitionToo`, `_Stance0/1/2ADSOvershootDamping`, `_OvershootAmplitude`, `_OvershootFrequency`, `_CameraBobbingMultiplier`, `_StanceTransitionDamping`, `_MaxLeanLimit`.

**Problema:** campos `ConfigEntry` declarados sem `Config.Bind` e sem leitura — lixo de código (não afetam o F12, só sujam o `Plugin.cs`).

**Por que importa:** manutenção/legibilidade (não é UX — não aparecem no menu).

**Sugestão:** remover as 9 declarações. Trivial e seguro.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

### MP-01-12 · TYP — Tipo · 🟢 Menor

**`Movement Speed Multiplier` é `int` (50-100 %) enquanto os outros multiplicadores são `float`**

**Local:** `Stance N Movement Speed Multiplier` (via `BindStance`, ex.: L1665), `int` 50-100.

**Problema:** inconsistência de tipo — os demais multiplicadores do mod são `float` (ex.: `Inertia Multiplier` 0.1-3.0). Aqui é `int` em %.

**Por que importa (UX):** menor — na verdade um `int` em % (50-100) é uma UX razoável (slider inteiro, sem casas decimais). Só destoa do padrão float do resto.

**Sugestão:** **manter** (o `int %` é intuitivo para velocidade). Registrado só para consistência; não recomendo mudar (mudar o tipo é breaking e piora a UX). Provável "Rejeitar".

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir — manter int %): _________________

---

## Recomendação de sequência

1. **MP-01-01 + MP-01-11** (remover as 23 mortas + 9 dead-code) — **primeiro**, não-breaking, limpa 16% do menu e simplifica o resto.
2. **MP-01-09** (decidir o gate dos ADS offsets) — consequência direta do passo 1.
3. **MP-01-02** (eixos trocados) — corrigir (tooltip ou rename).
4. **Reorganização** (MP-01-03/04/05/07/08) — todas breaking; fazer **num único release** com nota de migração no changelog (o usuário reconfigura uma vez).
5. **MP-01-06** (idioma) — decisão sua; tooltips pt-BR são não-breaking.
6. **MP-01-10 / MP-01-12** — polimento/decisão, opcionais.

Após aplicar: **regenerar** o `PROPRIEDADES.md` e incrementar a versão do mod (release com breaking changes de config).

## Histórico

| Data | Evento |
|---|---|
| 2026-07-09 | Review de propriedades 01 via `/review-mod-properties` — 1 🔴 (23 props mortas), 3 🟠, 6 🟡, 2 🟢. Detecção de mortas por varredura de `.Value` (sub-agent). |
