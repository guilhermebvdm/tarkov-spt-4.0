# Arquétipos das classes

> **Data:** 2026-06-13<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** mdj<br>
> **Referências:** [balance-model.md](./balance-model.md)<br>

---

Catálogo de arquétipos das 11 classes do CustomClasses. Para cada uma: **fantasia/conceito**, **eixos de força**, **skills-assinatura** e o **conjunto plausível** — as skills que a classe "treina de verdade" (categorias do arquétipo + skills exercitadas pelo loadout/playstyle). O conjunto plausível é o que a **regra anti-furo** do [balance-model.md](./balance-model.md) usa: um debuff só financia um buff se cair numa skill plausível; debuff fora dele é "grátis".

> **Como evolui:** este doc nasce dos arquétipos do RZCustomProfiles + estado atual das classes. Cada rodada de `/deep-research` (por grupo) **refina** o conjunto plausível e as skills-assinatura da(s) classe(s) daquele grupo, e registra a fonte. Não é congelado — é a fonte de verdade do *design de papel*, separada do *design de números* (que vive nos `.jsonc`).

## Convenção

- **Skill-assinatura:** 1–2 skills que definem a classe → fator de buff 1.5–2.0.
- **Conjunto plausível:** lista de skills (vanilla + SE) que a classe poderia treinar pelo papel. Debuff só conta para o net se estiver aqui. Categorias: **Ph** (Physical) · **M** (Mental) · **C** (Combat) · **P** (Practical).
- **Eixo de força:** o que a classe faz melhor que as outras (a fantasia traduzida em vantagem de raid).

## Grupos de rodada (afinidade de skills)

Os arquétipos se agrupam por skills compartilhadas — a pesquisa externa rende mais por grupo que por classe isolada:

| Grupo | Classes | Eixo comum |
|---|---|---|
| Recon / Stealth | Batedor · Operador Furtivo | CovertMovement, Perception, Search |
| Combate | Fuzileiro · Operador Tático | Assault, RecoilControl, AimDrills, força física |
| Suporte / Sobrevivência | Médico de Combate · Sobrevivencialista | Health, Vitality, Immunity, médicas (SE) |
| Utilitário / Hideout | Armeiro · Gerente de Operações | Intellect, WeaponTreatment, Crafting, HideoutManagement |
| Sniper (solo) | Caçador | Sniper, Perception, CovertMovement |
| Loot (solo) | Saqueador | Search, Attention, Perception, Intellect |
| Isenta | Peladão | — (classe-desafio, `noBaseline`) |

---

## Recon / Stealth

### Batedor (Scout / Recon)
- **Fantasia:** entra rápido, coleta intel e sai antes de ser detectado. Move-se em silêncio e identifica inimigos à distância.
- **Eixos de força:** mobilidade silenciosa + percepção a longa distância + loot rápido de pontos-chave.
- **Skills-assinatura:** `CovertMovement` (2.0), `Search`/`Perception` (1.5).
- **Conjunto plausível:** CovertMovement, Search, Perception, Attention, Endurance (Ph — corre muito), Throwing (C — recon usa utilitários), MagDrills. Categorias núcleo: **P + M + Ph**.
- **Debuff temático plausível:** combate sustentado — RecoilControl, Assault (não é o papel dele).
- **⚠ Peso baixo** (CovertMovement 0.94, Search 0.43, Perception 0.88): teto temático ≈ +3–4 com ×2.0. Aplica a **ressalva de viabilidade** do [balance-model.md](./balance-model.md) para mirar ~+6.

### Operador Furtivo (Stealth Operator)
- **Fantasia:** especialista em furtividade — inaudível em movimento, percepção afiada e busca eficiente.
- **Eixos de força:** stealth puro (mais que o Batedor) + eficiência de inventário sob pressão.
- **Skills-assinatura:** `CovertMovement` (2.0), `Search`/`Perception` (1.3–1.5).
- **Conjunto plausível:** CovertMovement, Search, Perception, MagDrills, Endurance, Attention. Categorias núcleo: **P + M**.
- **Debuff temático plausível:** Assault (combate aberto é o oposto do papel).
- **Diferenciação vs. Batedor:** Furtivo é stealth/eficiência (P), Batedor é stealth/recon-a-distância (P+M com Perception mais alta). Evitar que virem clones — a pesquisa do grupo deve cravar o eixo distinto de cada um.
- **⚠ Peso baixo** (mesma situação do Batedor): aplica a **ressalva de viabilidade** para mirar ~+6.

---

## Combate

### Fuzileiro (Assault Rifleman)
- **Fantasia:** agressivo. Fecha distância, sustenta fogo e empurra posições com reload rápido e controle de recuo.
- **Eixos de força:** DPS sustentado de rifle + recarga/recuo + agressão a curta-média.
- **Skills-assinatura:** `Assault` (2.0), `RecoilControl`/`AimDrills`/`MagDrills` (1.5).
- **Conjunto plausível:** Assault, RecoilControl, AimDrills, MagDrills, Endurance, Strength, Attention, Throwing. Categorias núcleo: **C + Ph**.
- **Debuff temático plausível:** CovertMovement (agressão ≠ furtividade).

### Operador Tático (Special Forces)
- **Fantasia:** all-rounder de elite, sem fraqueza óbvia. Fitness superior, mira rápida, adapta-se a qualquer combate.
- **Eixos de força:** físico (Strength/Endurance) + versatilidade — bom em tudo, melhor que ninguém em uma coisa só.
- **Skills-assinatura:** `Strength`/`Endurance`/`AimDrills` (1.5) — buffs distribuídos, não um pico (coerente com "sem fraqueza").
- **Conjunto plausível:** Strength, Endurance, AimDrills, Assault, MagDrills, Attention, RecoilControl, StressResistance. Categorias núcleo: **Ph + C**.
- **Debuff temático plausível:** CovertMovement (operador pesado, não furtivo) — porém leve, para não criar fraqueza que contradiz "all-rounder".
- **Diferenciação vs. Fuzileiro:** Fuzileiro é especialista de rifle (pico em Assault), Tático é físico/versátil (espalhado, Strength 10). A pesquisa deve garantir que o Tático não seja só "Fuzileiro com Strength".

---

## Suporte / Sobrevivência

### Médico de Combate (Combat Medic)
- **Fantasia:** sobrevive a ferimentos que matariam outros. Trata dano severo rápido e segue operacional após levar dano.
- **Eixos de força:** cura/cirurgia rápida + resiliência (Health/Vitality/Immunity) + sustain em raid longa.
- **Skills-assinatura:** `Surgery` (2.0), `Vitality`/`Health` (1.5), médicas SE (`FirstAid`/`FieldMedicine`).
- **Conjunto plausível:** Surgery, Vitality, Health, Immunity, StressResistance, Metabolism, FirstAid, FieldMedicine, Assault (defende-se). Categorias núcleo: **Ph + P + SE-médica**.
- **Debuff temático plausível:** combate ofensivo — RecoilControl, Sniper (médico não é atirador de elite).
- **★ PADRÃO da meta (+6.17):** bem construído (assinatura clara, debuff temático, skills de peso alto). **Fica intacto** — é a referência de ~+6 que as outras classes miram (decisão do usuário 2026-06-13). Não cortar skills nem reduzir buffs.

### Sobrevivencialista (Survivalist)
- **Fantasia:** resiste a condições que derrubam outros — fome, dano, infecção. Aguenta o raid mais que qualquer um.
- **Eixos de força:** resiliência corporal (Immunity/Vitality/Health) + metabolismo + economia de recursos.
- **Skills-assinatura:** `Immunity`/`Vitality` (1.5), `Metabolism` (2.0).
- **Conjunto plausível:** Immunity, Vitality, Health, Metabolism, StressResistance, Endurance, Search. Categorias núcleo: **Ph + P**.
- **Debuff temático plausível:** combate de precisão — RecoilControl, AimDrills.
- **Abaixo do padrão (+3.43 vs ~+6):** subir em direção ao Médico. Immunity (peso 3.75) já no ×1.5 → levar ao ×2.0 e completar com Vitality/Health/Metabolism é a alavanca natural — **viável** chegar a ~+6 (peso alto).
- **Diferenciação vs. Médico:** Médico = cura ativa (Surgery/médicas SE) + combate; Sobrevivencialista = resiliência passiva (Immunity/Metabolism), sem foco médico ativo. A sobreposição em Ph é grande — a pesquisa do grupo deve separar "trata" (Médico) de "aguenta" (Sobreviv.).

---

## Utilitário / Hideout

### Armeiro (Field Armorer)
- **Fantasia:** mantém armas funcionando mais tempo, destrava jams e modifica equipamento no campo.
- **Eixos de força:** manutenção de arma (durabilidade/jam) + inteligência técnica + crafting de gear.
- **Skills-assinatura:** `WeaponTreatment` (2.0), `TroubleShooting`/`Intellect` (1.5).
- **Conjunto plausível:** WeaponTreatment, TroubleShooting, Intellect, Strength, Crafting, Assault. Categorias núcleo: **P + M + C**.
- **Debuff temático plausível:** Endurance (técnico de bancada, não corredor) — já presente (×0.8).

### Gerente de Operações (Operations Manager)
- **Fantasia:** maximiza output do hideout e sobe skills mais rápido. Vantagem cumulativa, não de raid.
- **Eixos de força:** economia/hideout (Crafting/HideoutManagement) + skills mentais (aprende rápido).
- **Skills-assinatura:** `Crafting`/`HideoutManagement` (2.0), `Intellect`/`Memory`/`Charisma` (1.5).
- **Conjunto plausível:** Crafting, HideoutManagement, Intellect, Memory, Charisma, Attention, Shotgun (autodefesa leve). Categorias núcleo: **M + P**.
- **Debuff temático plausível:** físico/combate — Strength (já ×0.7), Endurance, qualquer arma de precisão.
- **⚠ Peso baixo — não chega a +6 com teto ×2.0:** buffs em Charisma 0.40, Memory 0.50, Crafting 0.33, HideoutManagement 0.39, Intellect 0.68 — teto temático ≈ **+2.9** mesmo tudo no ×2.0. Aplica a **ressalva de viabilidade** do [balance-model.md](./balance-model.md): a rodada decide com o usuário entre (a) teto de buff maior na assinatura (Crafting/Hideout ×2.5–3.0), (b) piso documentado < +6 (coerente — é a classe "fora de raid"), ou (c) skill temática de peso maior. Não transformar em classe de combate.

---

## Sniper (solo)

### Caçador (Hunter / Sniper)
- **Fantasia:** paciente e preciso. Domina posições elevadas, minimiza movimento e elimina antes de ser detectado.
- **Eixos de força:** tiro de longa distância (Sniper) + percepção + posicionamento furtivo.
- **Skills-assinatura:** `Sniper` (2.0), `Perception`/`CovertMovement` (1.5).
- **Conjunto plausível:** Sniper, DMR (subutilizada — temática!), Perception, CovertMovement, Attention, Endurance, AimDrills. Categorias núcleo: **C + M + P**.
- **Debuff temático plausível:** Assault (sniper não faz close-quarters) — já presente (×0.7).
- **Oportunidade:** `DMR` está zerada no jogo todo — o Caçador é o lar natural dela (marksman rifle). Ativar na rodada.

---

## Loot (solo)

### Saqueador (Scavenger)
- **Fantasia:** esvazia containers em segundos, detecta loot à distância e identifica itens valiosos na hora.
- **Eixos de força:** velocidade/eficiência de loot (Search) + percepção de valor (Attention/Intellect) + memória de spawns.
- **Skills-assinatura:** `Search` (2.0), `Attention`/`Perception`/`Intellect` (1.5).
- **Conjunto plausível:** Search, Attention, Perception, Intellect, Memory, Strength (carrega muito), Endurance. Categorias núcleo: **P + M + Ph**.
- **Debuff temático plausível:** combate — RecoilControl (já ×0.7), Assault.
- **⚠ Peso baixo — não chega a +6 com teto ×2.0:** buffs em Search 0.43, Attention 0.60, Memory 0.50, Intellect 0.68, Perception 0.88 — teto temático ≈ **+3.6** mesmo tudo no ×2.0. Aplica a **ressalva de viabilidade**: rodada decide (a) teto maior na assinatura Search, (b) piso < +6, ou (c) incluir Strength/Endurance temático (carrega muito loot). Levar ao usuário.

---

## Isenta

### Peladão (The Streaker)
- **Fantasia:** quem precisa de armadura quando tem confiança? Chegou ao raid como veio ao mundo.
- **Status de balance:** **`noBaseline`** — sem `skills` nem `skillMultipliers` por design (classe-desafio). O snapshot a marca como **isenta**, não como erro. **Fora de todas as rodadas de balance.**

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-13 | mdj | Criação. 11 arquétipos (fantasia, eixos, skills-assinatura, conjunto plausível, debuff temático) + grupos de rodada. Base: estado atual dos `.jsonc` + arquétipos RZ. A refinar por `/deep-research` em cada rodada. |
| 2026-06-13 | mdj | Meta revista (Médico = padrão ~+6 intacto): notas por classe reenquadradas de "acima/abaixo de +2.0" para "gap até +6"; ressalva de viabilidade (peso baixo) marcada em Gerente/Saqueador/Batedor/Furtivo. |
