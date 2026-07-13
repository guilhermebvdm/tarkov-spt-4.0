# Plano de teste — perks de som entre players (coop Fika)

> **Data:** 2026-07-12<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [class-design.md](./class-design.md), [balance-review-2026-07-05.md](../backlog/balance-review-2026-07-05.md)<br>

---

Valida **B14** (o que os bots ouvem de um peer) e **B20** (o que um humano ouve de um peer). Antes deles, todo perk de som de quem jogava como **cliente** era placebo — e o bug era invisível em solo, porque solo = você é o host.

## Por que não dá para testar "de ouvido"

O canal da IA **não é audível** (o que muda é a percepção de um bot), e "achei que soou mais baixo" não é evidência. Por isso o toggle **`0 · General → Perk Diagnostics overlay`** (F12) agora também **loga os perks de som aplicados a peers**, com a classe resolvida e o antes→depois.

**Ligar o toggle em TODAS as máquinas.** Cada uma escreve no seu próprio `D:\SPT\BepInEx\LogOutput.log`:

```
[CustomClasses][diag/peer] rolloff (you hear): 'NickFurtivo' [Stealth] 45.2 → 31.6 (×0.70)
[CustomClasses][diag/peer] AI hear power:      'NickFurtivo' [Stealth] 12.0 → 8.4  (×0.70)
```

### Como ler os logs (regra de ouro)

| Linha | Aparece no log de quem | Porque |
|---|---|---|
| `AI hear power` | **só do HOST** | os bots vivem no processo dele |
| `rolloff (you hear)` | **de quem está OUVINDO** (host ou cliente) | o áudio é calculado no cliente de quem ouve |

O **seu próprio** som nunca sai no log de peer — ele vai para o overlay (`Audio radius — you hear`).

### Multiplicadores esperados

| Classe | Perk | Esperado |
|---|---|---|
| Furtivo | Ghost Step | **×0.70** |
| Caçador | Stalker | **×0.80** |
| Fuzileiro / Tanque | Loud Operator | **×1.30** |
| Médico / Saqueador | — | **nenhuma linha** |

---

## Cenários

### C1 · Você é HOST, o Furtivo é CLIENTE — *(2 pessoas)*
O cenário que era **100% placebo** antes do B14.

- **Log do host:** as duas linhas, `[Stealth] ×0.70`.
- **Log do Furtivo:** nenhuma linha de peer (ele é o emissor).
- **Em jogo:** os bots devem reagir menos ao passo dele.

### C2 · Cliente ouvindo cliente — *(3 pessoas: host + Furtivo + você)* ⭐
O caso que o board dizia ser **impossível sem protocolo**. É o teste mais importante.

- **Seu log (cliente observador):** `rolloff (you hear)` do Furtivo, `×0.70`.
- **Log do host:** `AI hear power` do Furtivo (a IA é dele).
- **Bônus:** se o `×0.70` for **igual** no seu log e no do host, isso **prova que a config está idêntica** entre as máquinas — que é a premissa que substitui o sync.

### C3 · Controle inverso — peer **Tanque** ou **Fuzileiro**
Mesma montagem do C2, mas o peer é Loud Operator: os números têm que **subir** (`×1.30`). Se o Furtivo baixa e o Tanque sobe, não é viés nem placebo.

### C4 · Peer entrando de **SCAV**
Era um furo real (o mapa só tinha o nickname do PMC). As linhas devem continuar aparecendo — com o **nickname do scav** (cirílico, ex.: `'Платон Пупкин' [Stealth]`).

### C5 · Controle neutro — peer **Médico** (ou perfil vanilla)
**Nenhuma** linha para ele. Se aparecer, o gate de classe está errado.

### C6 · Bots não podem herdar classe
Nenhuma linha com um nome que **não** seja de um jogador real. O EFT gera nome de bot a partir de um pool de **nicknames reais**, então uma colisão daria a classe de um jogador a um bot — o gate `IsAI` existe para isso.

### C7 · Seu próprio passo (mudou também para você)
O B20 corrigiu um efeito **pela metade**: o volume do passo vinha de um cache que o patch nunca alcançava. Agora seu passo é mais **baixo** *e* mais **curto** (antes só mais curto).

### C8 · Silent Looter (Saqueador) — ⚠️ **suspeita de placebo**
Abrir container / porta / zíper e ver se o som muda. No decompile, o único call-site de `PlayInteractionSound` é interação de "Generator/Repair" — se a suspeita se confirmar, o perk só vale contra o SAIN e o **texto do card precisa mudar**.

---

## Sinais de bug (o que reportar)

- Linha de peer com **classe errada** → colisão de nickname no mapa (server).
- Linha com nome de **bot** → gate `IsAI` furado.
- **Nenhuma** linha para um peer que tem perk → mapa vazio (procurar `class-identities indisponível` no log) ou o peer entrou de scav e o fix não pegou.
- `×N` **diferente** entre as máquinas → a config **não** está idêntica (quebra a premissa do projeto).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-12 | Guilherme | Criação — plano de validação do B14/B20 (perks de som entre players) |
