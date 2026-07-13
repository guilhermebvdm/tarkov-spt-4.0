# Banco de ideias — perks e drawbacks

> **Data:** 2026-07-13<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [class-design.md](./class-design.md), [balance-review-2026-07-05.md](../backlog/balance-review-2026-07-05.md)<br>

---

Ideias **não decididas** (brainstorm de 2026-07-13). Nada aqui está aprovado — é matéria-prima para escolher. Ao promover uma ideia, mover para o `mod-backlog.md` como item numerado.

## Tier A — reusam mecânica que o EFT JÁ tem

O EFT expõe uma família de bônus **elite binários** (`GClass2257` no `SkillManager`) — mecânicas que o jogo já sabe ligar e desligar. O **Quick Hands** (item 061) é exatamente isso: um Postfix no getter `IsSearchDouble` forçando `true`, e o jogo faz o resto (inclusive o cap de 2 e o no-op quando o jogador chega no elite naturalmente).

Todo perk desta lista segue o mesmo molde → **baixo risco, sem inventar mecânica**.

| Classe | EN / PT | O que faz | Mecânica (evidência) | Confirmado? |
|---|---|---|---|---|
| 🛡️ Tanque | **Deflection** / **Deflexão** | Chance de o tiro **ricochetear** no colete pesado, **sem dano ao corpo** | `HeavyVestNoBodyDamageDeflectChance` → `Player.IsShotDeflectedByHeavyArmor` (Player.cs:30056) · também tratado para peers (`BodyPartCollider.cs:196`) | ✅ **vivo no decompile** |
| 🎒 Saqueador | **Lucky Find** / **Golpe de Sorte** | Chance de a revista render um item **melhor** | `AttentionEliteLuckySearch` (SkillManager.cs:2377, `Elite(0.5f)`) | 🟡 achar consumidor |
| 🩺 Médico | **Clotting** / **Coagulação** | Sangramentos leves **param sozinhos** | `VitalityBuffBleedStop` | 🟡 achar consumidor |
| 🔫 Fuzileiro | **Ammo Sense** / **Sentido de Munição** | Vê a munição do carregador **na hora** | `MagDrillsInstantCheck` · `IntellectEliteAmmoCounter` | 🟡 provável UI |
| 👻 Furtivo | **Serpent** / **Serpente** | **Rasteja rápido** (sprint deitado) | `ProneMovementEliteSprint` | 🟡 achar consumidor |
| 🩺 / 🛡️ | **Iron Gut** / **Estômago de Ferro** | **Não desidrata** | `MetabolismEliteBuffNoDyhydration` | 🟡 achar consumidor |
| 👻 Furtivo | (elite do Covert Movement) | Furtividade elite | `CovertMovementElite` | 🟡 achar consumidor |

⚠️ **Deflection é o mais forte da lista.** Ele **casa** com a Couraça condicional (B6), que já exige colete pesado — mas os dois juntos deixam o Tanque duro demais. Provavelmente é **ou** um **ou** outro, ou o Deflection substitui parte da Couraça. Decidir no balance, não na implementação.

## Tier B — exigem patch novo (mais saborosos, mais caros)

| Classe | EN / PT | O que faz |
|---|---|---|
| 👻 Furtivo | **Silent Kill** / **Morte Silenciosa** | Abate com faca **não emite som** — nem o grito do alvo. Fecha a fantasia junto do Execution ×3.5 |
| 👻 Furtivo | **Frenzy** / **Frenesi** | Abate com melee → **+velocidade** por alguns segundos (encadear abates) |
| 🎯 Caçador | **Trophy Shot** / **Tiro de Troféu** | Headshot a longa distância **restaura o fôlego** na hora — prêmio por precisão |
| 🎯 Caçador | **Bloodhound** / **Faro** | Rastros de sangue de inimigos feridos duram mais |
| 🛡️ Tanque | **Last Stand** / **Último Suspiro** | Dano fatal → sobrevive com **1 HP**, **1× por raid** |
| 🔫 Fuzileiro | **Second Wind** / **Segundo Fôlego** | Abaixo de X% de HP → **stamina cheia** na hora, 1× por raid |
| 🩺 Médico | **Field Surgeon** / **Cirurgião de Campo** | Curar um **aliado** (coop Fika) concede buff — usa o multiplayer de verdade |

## Drawbacks

O mod é **magro em drawbacks**: hoje quase todos são multiplicador puro (recuo, ruído, velocidade, fome). Falta drawback com **mecânica**.

| Classe | EN / PT | O que faz |
|---|---|---|
| 👻 Furtivo | **Glass Cannon** / **Cristal** | Mata fácil, **morre fácil** — dano recebido +X% |
| 👻 Furtivo | **Blood Scent** / **Cheiro de Sangue** | Sangrando, os **bots te detectam melhor** |
| 🎒 Saqueador | **Butterfingers** / **Dedos de Manteiga** | Chance de **derrubar** um item ao levar tiro |
| 🛡️ Tanque | **Claustrophobia** / **Claustrofobia** | Penalidade em **espaços fechados** — o brutamontes não gosta de corredor |

## 🩲 Peladão

Ele **não deve ganhar poder** — o mérito da classe é justamente **não ter nenhum**. Mas dá para premiar o orgulho sem quebrar a identidade:

**`Naked Ambition` / `Ambição Nua`** — como começa do zero absoluto, **aprende mais rápido** (XP ×N em tudo). Não é força, é a lenda de quem sobe pelado. Combina com o texto de mérito do item 068.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-13 | Guilherme | Criação — brainstorm de perks/drawbacks; Tier A ancorado nos bônus elite binários do EFT |
