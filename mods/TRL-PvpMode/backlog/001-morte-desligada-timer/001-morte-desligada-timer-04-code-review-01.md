# 001 — Morte desligada com timer · Code Review (rodada 01)

**Mod:** TRL-PvpMode · **Data:** 2026-08-01
**Método:** revisão adversarial por agente com contexto limpo, conferindo o código contra o fonte do Fika
e o dump do EFT.
**Resultado:** 2 🔴 · 5 🟡 · 5 🔵 — **todos aplicados**, nenhum rejeitado. Build limpo após as correções.

---

## 🔴 C-01 — O portão da morte por desgaste era código morto

O `InstantKillPatch` lia o tipo de dano de `Player.LastDamageInfo`. Esse campo **só é escrito no caminho
de dano de combate** (`Player.ApplyDamageInfo`, `Player.ManageAggressor`). Fome, sede e overdose vão
**direto ao controlador de vida** (`ActiveHealthController.cs:1353` e `:1186`) e chegam à morte por
`DestroyBodyPart → Kill(damageType)` (`:3883`) — o tipo correto existe **só como argumento de `Kill`**.

Consequência: a condição nunca disparava, e o buraco que o review da spec (R-02) tinha identificado
**continuava aberto** — morrer de fome levaria ao estado de caído em vez de encerrar a partida. Com o
tempo configurado como `0`, o jogador ficaria caído para sempre.

Agravante descoberto na mesma análise: a guarda nativa do Fika (`ClientHealthController.cs:159`) lê a
mesma fonte podre, então **ela também falha** — não dava para confiar nela como rede de segurança.

**Causa-raiz:** eu havia removido o `KillGatePatch` da spec técnica como "simplificação", trocando-o por
uma leitura direta do campo. A simplificação estava errada; o patch voltou.

**Aplicado:** [KillGatePatch.cs](../../modded/Patches/KillGatePatch.cs) — prefixo em
`ActiveHealthController.Kill` com `Priority.First` gravando `RaidState.LastKillDamageType`, lido no mesmo
quadro pelos dois pontos de consulta.

## 🔴 C-02 — O "autodesativa" não desativava: piorava

O postfix de `CanBeRevivedByOtherPlayer` gateava só em `Settings.ENABLED.Value` e então forçava
`__result = RaidState.HasLifeAvailable`. Como `HasLifeAvailable` embute `_active`, **todo caminho em que
`Begin()` aborta** (ponte do Fika incompleta, PlayerLives instalado, esconderijo) forçava `false`.

Sem o mod, esse método devolve "há companheiro vivo?" → normalmente `true` → o jogador cai e pode ser
levantado. Com o mod "desativado" instalado → `false` → **morte instantânea e nenhum aliado conseguindo
levantar**. Ou seja, a mensagem "TRL-PvpMode DESATIVADO" aparecia na tela enquanto o mod piorava a
experiência.

**Aplicado:** `RaidState.IsActive` exposto; todos os patches saem cedo quando o modo não está governando
a raid, deixando o comportamento nativo do Fika intacto.

## 🟡 Aplicados

| ID | Achado | Correção |
|---|---|---|
| **C-03** | A opção de headshot só somava, nunca subtraía: num servidor com `headshotKills: true`, nosso F12 em `false` não desligava nada | Com o modo ativo, a decisão passa a ser **nossa por inteiro** (`__result = attrition \|\| headshot`) em vez de aditiva |
| **C-04** | `NoAllyRevivePatch` gateava só em `ENABLED`, continuando a bloquear depois do "DESATIVADO" | Gateado em `RaidState.IsActive`, igual aos demais |
| **C-05** | `Begin()` retornava antes de zerar o estado (esconderijo, `MainPlayer` nulo), contrariando o que a spec garante | Reset movido para a **primeira linha**, antes de qualquer guarda |
| **C-06** | `IsUsable` validava o membro errado: exigia o campo de dano (que sumiu com o C-01) e **não** exigia o campo que sustenta a opção de tempo — falha ali era só um aviso no log | `IsUsable` agora exige o campo de tempo; falha na aplicação do prazo virou **aviso na tela**, não só log |
| **C-07** | Corner case "cair durante transição / BTR / extração" sem tratamento nem menção | Declarado como **limitação conhecida** no `PROPRIEDADES.md` |

## 🔵 Aplicados

| ID | Achado | Correção |
|---|---|---|
| **C-08** | `IsUnlimited` validava `ENABLED != null` e desreferenciava `LIVES_PER_RAID` | Guarda no campo certo |
| **C-09** | Quatro patches num `try` só — a primeira falha derrubava os seguintes | Um `try` por patch, via `TryEnable` |
| **C-10** | `BleedoutOnPlayerDeathPatch` gateava em vidas restantes; quando o item 002 debitar a vida com o jogador ainda caído, o bloqueio cairia no meio do respawn | Gateado em "modo ativo **+ estou caído**" |
| **C-11** | Boxing de `DamageInfoStruct` por chamada | Some junto com a correção do C-01 |
| **C-12** | A spec técnica se contradizia: §2 dizia "não patchado", §4/§5/§8 exigiam o `KillGatePatch` | Spec reconciliada |

## Confirmações do revisor (checadas, sem achado)

- `ClientGameWorld.OnDestroy` **chama** `base.OnDestroy()` — o postfix de fim de raid roda, sem AP-03.
- `<BleedoutTime>k__BackingField` é o nome real do campo gerado — confirmado no binário do Fika.
- Nenhum patch roda por quadro nem por bot: os portões só são consultados no caminho da morte, e
  `GetActions` é orientado a evento de mudança de interação.
- O padrão `is ClientHealthController { ReviveEnabled: true }` é seguro para outros tipos e para nulo.

## Histórico

| Data | Evento |
|---|---|
| 2026-08-01 | Review adversarial rodada 01 — 12 achados, todos aplicados; build limpo |
