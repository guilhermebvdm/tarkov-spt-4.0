# Perfis customizados — planejamento

> **Premissa:** skills começam no nível 0. Cada perfil distribui até **6 skills** com teto de **10 por skill**, calibrado por um **budget de 28 a 32 pontos ponderados** (ver "Modelo de balanceamento" abaixo). Além disso, cada classe pode receber:
> - **1 estação inicial de hideout em nível 1** (ver "Hideout inicial")
> - **Loadout inicial** (~2M ₽) com itens no stash (ver "Inventário inicial")
>
> Nenhum perfil altera traders, nível inicial do personagem ou quests.

---

## Classes em resumo

| # | Classe | Conceito | Estilo de jogo |
|---|--------|----------|----------------|
| 1 | **Médico de Combate** | Combat Medic — medicina tática de campo | Sobrevive a ferimentos que matariam outros. Trata dano severo rápido e continua operacional. |
| 2 | **Caçador** | Sniper — engajamentos de longa distância | Paciente e preciso. Domina posições elevadas, minimiza movimento e elimina antes de ser detectado. |
| 3 | **Fuzileiro** | Assault Rifleman — combate direto | Agressivo. Entra em contato, sustenta fogo e empurra posições com recargas rápidas e controle de recuo. |
| 4 | **Batedor** | Scout / Recon — informação e mobilidade | Entra rápido, coleta informação, sai antes de ser detectado. Move-se em silêncio e identifica inimigos à distância. |
| 5 | **Operador Noturno** | Night Ops — operações sob escuridão | Domina o ambiente noturno. Usa NVG e supressores enquanto inimigos lutam contra a visibilidade. |
| 6 | **Armeiro** | Field Armorer — manutenção de armamento | Mantém armas funcionando por mais tempo, corrige encravamentos e modifica equipamento em campo. |
| 7 | **Operador Tático** | Special Forces — generalista de elite | Sem fraquezas evidentes. Físico superior, mira rápida e adaptação a qualquer tipo de combate. |
| 8 | **Sobrevivencialista** | Survivalist — autossuficiência no longo prazo | Fica em raid por mais tempo que qualquer outro. Drena recursos devagar e resiste a efeitos negativos. |
| 9 | **Saqueador** | Scavenger — extração de valor | Esvazia containers em segundos, detecta loot à distância e identifica itens valiosos instantaneamente. |
| 10 | **Gerente de Operações** *(bônus)* | Operations Manager — logística e produção | Maximiza rendimento do hideout e progride skills mais rápido. Vantagem cumulativa, não imediata em raid. |

---

## Modelo de balanceamento

Tratar 1 ponto = 1 nível para todas as skills é injusto: subir Metabolism para 10 é trivial (sobe comendo/bebendo), enquanto subir Endurance ou FirstAid para 10 representa dezenas de horas de raid focada. Para equilibrar isso, cada skill recebe um **multiplicador de custo** derivado empiricamente de um personagem de referência lvl 43 (screenshots em [../assets/](../assets/)).

### Fórmula

```text
multiplicador_skill = BASELINE / nivel_observado_no_lvl_43
custo_skill          = nivel_atribuído × multiplicador_skill
custo_classe         = Σ custo_skill
```

- `BASELINE = 15` (mediana das skills neutras do personagem de referência — Assault e Endurance ficaram exatamente nesse valor).
- Resultado em `[0.25, 3.00]` por clamp.
- **Budget alvo por classe: 28 a 32 pontos ponderados.**

### Multiplicadores — skills observadas no personagem de referência

| Skill | Nível @ lvl 43 | Multiplicador |
|-------|---------------:|--------------:|
| `Metabolism` | 51 (ELITE) | 0.29 |
| `Crafting` | 45 | 0.33 |
| `HideoutManagement` | 38 | 0.39 |
| `Search` | 35 | 0.43 |
| `Strength` | 32 | 0.47 |
| `Attention` | 25 | 0.60 |
| `Intellect` | 22 | 0.68 |
| `Revolver` | 21 | 0.71 |
| `Throwing` | 18 | 0.83 |
| `StressResistance` | 17 | 0.88 |
| `Perception` | 17 | 0.88 |
| `CovertMovement` | 16 | 0.94 |
| `MagDrills` | 16 | 0.94 |
| `Assault` | 15 | 1.00 |
| `Endurance` | 15 | 1.00 |
| `AimDrills` | 13 | 1.15 |
| `Surgery` | 12 | 1.25 |
| `WeaponTreatment` | 12 | 1.25 |
| `Vitality` | 9 | 1.67 |
| `Health` | 9 | 1.67 |
| `Melee` | 8 | 1.88 |
| `Shotgun` | 6 | 2.50 |
| `TroubleShooting` | 6 | 2.50 |
| `Pistol`, `SMG` | 5 | 3.00 |
| `DMR`, `Bolt-action` | 4 | 3.00 |
| `Immunity` | 4 | 3.00 |
| `LightVests` | 4 | 3.00 |
| `HeavyVests` | 4 | 3.00 |
| `LMG` | 2 | 3.00 |
| `HMG`, `Launcher`, `AttachedLauncher` | 0 | 3.00 |

### Multiplicadores — skills não observadas (premissas)

| Skill | Multiplicador | Premissa |
|-------|--------------:|----------|
| `FirstAid` | 1.25 | Análoga a Surgery (uso ativo de meds) |
| `FieldMedicine` | 1.50 | Mais rara — meds caros em movimento |
| `Sniper` | 1.50 | Mastering raro de bolt-action longo |
| `Sniping` | 1.50 | Exige paciência e scope |
| `ProneMovement` | 1.50 | Análoga a CovertMovement mas usada menos |
| `RecoilControl` | 1.00 | Sobe naturalmente atirando — baseline |
| `WeaponModding` | 1.00 | Pode ser farmado no workbench |
| `AdvancedModding` | 2.00 | Gated por WeaponModding alto |
| `NightOps` | 2.50 | Gear-gated (precisa NVG) |
| `SilentOps` | 2.50 | Gear-gated (precisa supressor) |
| `Lockpicking` | 2.50 | Ação rara |
| `Memory` | 0.50 | Sobe junto com todas as outras skills |
| `Charisma` | 0.40 | Passiva em trades/quests |
| `Barter` | 1.50 | Depende de trades de barter ativas |
| `Freetrading`, `Auctions` | 2.00 | Depende de uso da flea |
| `Cleanoperations`, `Shadowconnections` | 2.50 | Rotas específicas de quest |
| `Taskperformance` | 2.00 | Sobe com quests completadas |

> Recalibração futura: rodar a fórmula com um personagem de referência mais recente atualiza toda a tabela.

---

## Perfis propostos

### 1. Médico de Combate
*Combat Medic — especialista em medicina tática de campo*

**Estilo de jogo:** Sobrevive a situações que matariam outros jogadores. Trata ferimentos com mais velocidade e eficiência, permitindo continuar operacional mesmo após tomar dano severo.

| Skill | Nível | Mult. | Custo | Justificativa |
|-------|------:|------:|------:|---------------|
| `FirstAid` | 7 | 1.25 | 8.75 | Uso rápido de bandagens, splints e hemostáticos — base de qualquer médico |
| `FieldMedicine` | 5 | 1.50 | 7.50 | Aplicar curativos em movimento, sem parar em cobertura |
| `Surgery` | 5 | 1.25 | 6.25 | Reparar membros com kit cirúrgico mais rápido |
| `Vitality` | 3 | 1.67 | 5.01 | HP máximo maior e menor chance de sangramento |
| `Health` | 2 | 1.67 | 3.34 | Regeneração passiva: sai de raids em melhor condição |
| **Custo ponderado** | | | **30.85 / 32** | |

---

### 2. Caçador
*Sniper — especialista em engajamentos de longa distância*

**Estilo de jogo:** Paciente e preciso. Domina posições elevadas, minimiza movimento após se posicionar e entrega tiros certeiros antes de ser detectado.

| Skill | Nível | Mult. | Custo | Justificativa |
|-------|------:|------:|------:|---------------|
| `Sniper` | 5 | 1.50 | 7.50 | Maestria com rifles de precisão: menos recuo, melhor ergonomia |
| `Sniping` | 5 | 1.50 | 7.50 | Redução de oscilação de scope — essencial para acertar a distâncias extremas |
| `ProneMovement` | 5 | 1.50 | 7.50 | Se reposicionar de bruços sem sair da posição fetal |
| `CovertMovement` | 4 | 0.94 | 3.76 | Aproximar-se da posição de sniping sem ser ouvido |
| `Perception` | 4 | 0.88 | 3.52 | Detectar inimigos pelo som antes de revelar a posição |
| **Custo ponderado** | | | **29.78 / 32** | |

---

### 3. Fuzileiro
*Assault Rifleman — infantaria de linha, combate direto*

**Estilo de jogo:** Agressivo e direto. Entra em contato com o inimigo, sustenta o fogo e empurra posições. Depende de recargas rápidas e controle de recuo para ganhar trocas de tiro.

| Skill | Nível | Mult. | Custo | Justificativa |
|-------|------:|------:|------:|---------------|
| `Assault` | 10 | 1.00 | 10.00 | Maestria com rifles de assalto — a arma principal da classe |
| `MagDrills` | 8 | 0.94 | 7.52 | Troca de carregador rápida é diferencial decisivo em firefights |
| `RecoilControl` | 5 | 1.00 | 5.00 | Controle de recuo em rajadas, especialmente com ARs baratas |
| `AimDrills` | 4 | 1.15 | 4.60 | Mira rápida em movimento — diferencial em close quarters |
| `Endurance` | 3 | 1.00 | 3.00 | Stamina para flanquear, empurrar e sair de situações adversas |
| **Custo ponderado** | | | **30.12 / 32** | |

---

### 4. Batedor
*Scout / Reconhecimento — informação e mobilidade*

**Estilo de jogo:** Entra rápido, coleta informações e sai antes de ser detectado. Identifica inimigos e loot à distância, e se move em silêncio absoluto pelo mapa.

| Skill | Nível | Mult. | Custo | Justificativa |
|-------|------:|------:|------:|---------------|
| `CovertMovement` | 8 | 0.94 | 7.52 | Silêncio total em movimento — a base do reconhecimento |
| `Perception` | 10 | 0.88 | 8.80 | Maior raio de escuta e detecção de sons de inimigos e loot |
| `Endurance` | 5 | 1.00 | 5.00 | Sprint longo para cobrir distâncias e escapar rapidamente |
| `Search` | 10 | 0.43 | 4.30 | Saque rápido durante janelas de oportunidade curtas |
| `Attention` | 7 | 0.60 | 4.20 | Detecta itens escondidos durante o reconhecimento |
| **Custo ponderado** | | | **29.82 / 32** | |

---

### 5. Operador Noturno
*Night Ops — especialista em operações sob escuridão*

**Estilo de jogo:** Domina o ambiente noturno e usa silenciadores com maestria. Transforma a escuridão em vantagem tática enquanto inimigos lutam contra a visibilidade.

| Skill | Nível | Mult. | Custo | Justificativa |
|-------|------:|------:|------:|---------------|
| `NightOps` | 4 | 2.50 | 10.00 | Desempenho máximo com visão noturna e em ambientes escuros |
| `SilentOps` | 4 | 2.50 | 10.00 | Bônus com silenciadores — a principal ferramenta da classe |
| `CovertMovement` | 4 | 0.94 | 3.76 | Silêncio em movimento complementa a invisibilidade noturna |
| `ProneMovement` | 3 | 1.50 | 4.50 | Movimentação de bruços para aproximações táticas no escuro |
| `Perception` | 2 | 0.88 | 1.76 | Detecta sons na escuridão antes do inimigo reagir |
| **Custo ponderado** | | | **30.02 / 32** | |

---

### 6. Armeiro
*Field Armorer — técnico de armamento e manutenção*

**Estilo de jogo:** Mantém as armas funcionando por mais tempo, corrige encravamentos sob pressão e modifica equipamento em campo. Tira mais valor de cada arma encontrada.

| Skill | Nível | Mult. | Custo | Justificativa |
|-------|------:|------:|------:|---------------|
| `WeaponTreatment` | 8 | 1.25 | 10.00 | Armas degradam mais lentamente — crucial com armas de baixa durabilidade |
| `TroubleShooting` | 4 | 2.50 | 10.00 | Limpar encravamentos em segundos em vez de morrer com a arma travada |
| `WeaponModding` | 6 | 1.00 | 6.00 | Modificar armas no campo mais rapidamente |
| `Intellect` | 6 | 0.68 | 4.08 | Melhor qualidade de reparo e exame de itens mais rápido |
| **Custo ponderado** | | | **30.08 / 32** | |

---

### 7. Operador Tático
*Special Forces — soldado de elite, habilidades equilibradas*

**Estilo de jogo:** Generalista de alto nível. Sem fraquezas evidentes. Físico superior, mira rápida e adaptação a qualquer tipo de combate ou equipamento.

| Skill | Nível | Mult. | Custo | Justificativa |
|-------|------:|------:|------:|---------------|
| `Strength` | 10 | 0.47 | 4.70 | Carrega mais equipamento sem penalidade de stamina |
| `Endurance` | 7 | 1.00 | 7.00 | Stamina para manter o ritmo em operações longas |
| `AimDrills` | 6 | 1.15 | 6.90 | Mira extremamente rápida — diferencial no primeiro tiro |
| `MagDrills` | 6 | 0.94 | 5.64 | Recargas rápidas para manter pressão de fogo |
| `LightVests` | 2 | 3.00 | 6.00 | Bônus com armadura leve, o padrão de operações especiais |
| **Custo ponderado** | | | **30.24 / 32** | |

---

### 8. Sobrevivencialista
*Survivalist — resistência e autossuficiência no longo prazo*

**Estilo de jogo:** Fica em raid por mais tempo do que qualquer outro. Drena recursos mais lentamente, resiste a efeitos negativos e se recupera de situações que eliminariam outros jogadores.

| Skill | Nível | Mult. | Custo | Justificativa |
|-------|------:|------:|------:|---------------|
| `Metabolism` | 10 | 0.29 | 2.90 | Energia e hidratação drenam muito mais devagar — mais tempo em raid |
| `Vitality` | 5 | 1.67 | 8.35 | HP máximo maior e menor chance de sangramento grave |
| `Immunity` | 3 | 3.00 | 9.00 | Resistência a venenos, toxinas e efeitos negativos |
| `StressResistance` | 5 | 0.88 | 4.40 | Reduz tremores e efeitos de dor — mantém a mira estável |
| `Health` | 3 | 1.67 | 5.01 | Regeneração passiva de HP entre confrontos |
| **Custo ponderado** | | | **29.66 / 32** | |

---

### 9. Saqueador
*Scavenger — especialista em extração de valor*

**Estilo de jogo:** Esvazia containers em segundos, detecta loot à distância e identifica itens valiosos instantaneamente. Maximiza o retorno por raid.

| Skill | Nível | Mult. | Custo | Justificativa |
|-------|------:|------:|------:|---------------|
| `Attention` | 10 | 0.60 | 6.00 | Velocidade de saque e chance de encontrar itens extras |
| `Search` | 10 | 0.43 | 4.30 | Busca em containers em fração do tempo normal |
| `Perception` | 10 | 0.88 | 8.80 | Detecta loot e sons de containers a maior distância |
| `Intellect` | 10 | 0.68 | 6.80 | Examina itens desconhecidos mais rápido |
| `Memory` | 8 | 0.50 | 4.00 | Skills sobem mais rápido — benefício composto durante runs de loot |
| **Custo ponderado** | | | **29.90 / 32** | |

---

### 10. Gerente de Operações *(Bônus — Hideout focus)*
*Operations Manager — especialista em logística e produção*

**Estilo de jogo:** Maximiza o rendimento do hideout e sobe skills com mais eficiência. Menos impacto imediato em raid, mas vantagem cumulativa significativa na progressão.

| Skill | Nível | Mult. | Custo | Justificativa |
|-------|------:|------:|------:|---------------|
| `Crafting` | 10 | 0.33 | 3.30 | Crafting de itens mais rápido desde o início |
| `HideoutManagement` | 10 | 0.39 | 3.90 | Produção do hideout mais eficiente (menos combustível, mais output) |
| `Memory` | 10 | 0.50 | 5.00 | Todas as outras skills sobem mais rápido — benefício composto |
| `Intellect` | 10 | 0.68 | 6.80 | Examina e repara itens com melhor qualidade |
| `Charisma` | 10 | 0.40 | 4.00 | Desconto em traders e melhores condições de quest |
| `WeaponModding` | 7 | 1.00 | 7.00 | Workbench/mod stand mais produtivo desde o início |
| **Custo ponderado** | | | **30.00 / 32** | |

---

## Referência rápida

| Perfil | Skills principais | Custo ponderado |
|--------|------------------|----------------:|
| Médico de Combate | FirstAid 7, FieldMedicine 5, Surgery 5, Vitality 3, Health 2 | 30.85 |
| Caçador | Sniper 5, Sniping 5, ProneMovement 5, CovertMovement 4, Perception 4 | 29.78 |
| Fuzileiro | Assault 10, MagDrills 8, RecoilControl 5, AimDrills 4, Endurance 3 | 30.12 |
| Batedor | CovertMovement 8, Perception 10, Endurance 5, Search 10, Attention 7 | 29.82 |
| Operador Noturno | NightOps 4, SilentOps 4, CovertMovement 4, ProneMovement 3, Perception 2 | 30.02 |
| Armeiro | WeaponTreatment 8, TroubleShooting 4, WeaponModding 6, Intellect 6 | 30.08 |
| Operador Tático | Strength 10, Endurance 7, AimDrills 6, MagDrills 6, LightVests 2 | 30.24 |
| Sobrevivencialista | Metabolism 10, Vitality 5, Immunity 3, StressResistance 5, Health 3 | 29.66 |
| Saqueador | Attention 10, Search 10, Perception 10, Intellect 10, Memory 8 | 29.90 |
| Gerente de Operações | Crafting 10, HideoutManagement 10, Memory 10, Intellect 10, Charisma 10, WeaponModding 7 | 30.00 |

---

## Hideout inicial (estação temática)

Cada classe começa com **1 estação extra do hideout em nível 1** (além de `Stash: 1` que é padrão). A escolha reflete a identidade da classe — head-start prático mas modesto.

**Restrição de design:** apenas estações **sem pré-requisitos** são elegíveis (podem ser construídas direto, sem cadeia de dependências). Estações como `ShootingRange` (← Illumination L2), `IntelligenceCenter` (← Security L2 + Vents L2) e `ScavCase` (← IntelligenceCenter L2) foram **descartadas** porque pré-setá-las em L1 sem os requisitos resultaria em UI quebrada ou valor silenciosamente ignorado [fonte externa: [playerassist.com — Hideout Guide](https://playerassist.com/escape-from-tarkov-hideout-guide/)].

**Estações elegíveis (sem pré-requisitos):** `MedStation`, `Workbench`, `RestSpace`, `WaterCollector`, `Generator`, `Heating`, `Vents`, `Security`.

| Classe | Estação extra | Racional |
|--------|--------------|----------|
| Médico de Combate | `MedStation: 1` | Posto médico básico — coerente com a função |
| Caçador | `Heating: 1` | Caça em ambientes hostis/frios — controle térmico do esconderijo |
| Fuzileiro | `Workbench: 1` | Manutenção e mods básicos do AR |
| Batedor | `Security: 1` | Vigilância e perímetro — alinhado ao tema de recon e detecção |
| Operador Noturno | `Generator: 1` | NVG/equipamento elétrico demanda energia |
| Armeiro | `Workbench: 1` | Workbench é a estação-mãe do armeiro |
| Operador Tático | `RestSpace: 1` | Recuperação entre operações longas |
| Sobrevivencialista | `WaterCollector: 1` | Auto-suficiência hídrica |
| Saqueador | `Security: 1` | Proteger o loot acumulado no esconderijo |
| Gerente de Operações | `Generator: 1` + `Heating: 1` | Infraestrutura elétrica + térmica — fundação do hideout (recebe 2 estações como bônus de identidade) |

> Todas as outras estações permanecem em nível 0. `Stash: 1` é mantido em todas as classes (idêntico ao Standard base). Repetições intencionais: `Workbench` (Fuzileiro + Armeiro), `Security` (Batedor + Saqueador) — classes diferentes podem compartilhar afinidade.

---

## Tabela-âncora de preços (PVE flea)

Snapshot de preços validados via [tarkov-market.com](https://tarkov-market.com) (PVE, avg 24h).
Registros completos com URLs de imagem, wiki, slot, tags em [`anchor-items.json`](anchor-items.json) — para uso futuro em renderização visual do stash.

> ⚠️ Alguns preços de listings raros podem aparecer distorcidos (ex: 6Sh118 raid backpack a ~1.47M reflete listings PVE com pouca liquidez — não usar em loadouts iniciais).

| ID | Nome | Short | Tpl | avg24h ₽ | trader ₽ |
|----|------|-------|-----|---------:|---------:|
| `6SH118` | 6Sh118 raid backpack (EMR) | 6Sh118 | `5df8a4d786f77412672a1e3b` | 1.469.992 | 89.280 |
| `WEAPON_REPAIR_KIT` | Weapon repair kit | Weapon repair kit | `5910968f86f77425cf569c32` | 577.654 | 39.200 |
| `PISTOL_CASE` | Pistol case | Pistols | `567143bf4bdc2d1a0f8b4567` | 321.481 | 6.600 |
| `ETG_CHANGE` | eTG-change regenerative stimulant injector | eTG-c | `5c0e534186f7747fa1419867` | 304.556 | 23.832 |
| `CUSTOMS_MAP` | Customs plan map | Customs | `5798a2832459774b53341029` | 118.512 | 8.459 |
| `DOCUMENTS_CASE` | Documents case | Docs | `590c60fc86f77412b13fddcf` | 106.323 | 105.505 |
| `TRIZIP` | Camelbak Tri-Zip assault backpack (Foliage) | Tri-Zip | `545cdae64bdc2d39198b4568` | 100.920 | 38.021 |
| `COMPASS` | EYE MK.2 professional hand-held compass | Compass | `5f4f9eb969cdc30ff33f09db` | 91.212 | 36.792 |
| `SURV12` | Surv12 field surgical kit | Surv12 | `5d02797c86f774203f38e30a` | 89.675 | 36.288 |
| `PILGRIM` | Pilgrim tourist backpack | Pilgrim | `59e763f286f7742ee57895da` | 78.999 | 48.979 |
| `TRIPLE_BANDOLIER` | WARTECH TV-110 plate carrier (Coyote) | TV-110 | `5c0e746986f7741453628fe5` | 74.041 | 16.146 |
| `MP153` | MP-153 12ga semi-automatic shotgun | MP-153 | `56dee2bdd2720bc8328b4567` | 70.980 | 8.988 |
| `CALOK_B` | CALOK-B hemostatic applicator | CALOK-B | `5e8488fa988a8701445df1e4` | 70.300 | 2.879 |
| `PROPITAL` | Propital regenerative stimulant injector | Propital | `5c0e530286f7747fa1419862` | 69.550 | 13.267 |
| `M4A1` | Colt M4A1 5.56x45 assault rifle | M4A1 | `5447a9cd4bdc2dbd208b4567` | 65.000 | 10.302 |
| `GRIZZLY` | Grizzly medical kit | Grizzly | `590c657e86f77412b013051d` | 60.988 | 17.661 |
| `PARATUS` | 3V Gear Paratus 3-Day Operator's Tactical backpack (Foliage Grey) | Paratus | `5c0e805e86f774683f3dd637` | 58.900 | 36.518 |
| `HEMOSTOP` | Zagustin hemostatic drug injector | Zagustin | `5c0e533786f7747fa23f4d47` | 57.503 | 21.130 |
| `SV98` | SV-98 7.62x54R bolt-action sniper rifle | SV-98 | `55801eed4bdc2d89578b4588` | 55.975 | 14.993 |
| `CMS` | CMS surgical kit | CMS | `5d02778e86f774203e7dedbe` | 49.783 | 18.144 |
| `KIRASA` | BNTI Kirasa-N body armor | Kirasa-N | `5b44d22286f774172b0c9de8` | 46.706 | 12.858 |
| `BLACKROCK` | BlackRock chest rig (Gray) | BlackRock | `5648a69d4bdc2ded0b8b457b` | 46.634 | 25.234 |
| `EOTECH553` | EOTech 553 holographic sight | 553 | `570fd6c2d2720bc6458b457f` | 45.394 | 12.980 |
| `MRE` | MRE ration pack | MRE | `590c5f0d86f77413997acfab` | 44.986 | 10.748 |
| `6B23_1` | 6B23-1 body armor (EMR) | 6B23-1 EMR | `5c0e5bab86f77461f55ed1f3` | 43.849 | 15.547 |
| `LBT_2670` | LBT-2670 Slim Field Med Pack (Black) | SFMP | `5e4abc6786f77406812bd572` | 43.590 | 12.400 |
| `AKM` | Kalashnikov AKM 7.62x39 assault rifle | AKM | `59d6088586f774275f37482f` | 43.342 | 16.527 |
| `MOSIN_INFANTRY` | Mosin 7.62x54R bolt-action rifle (Infantry) | Mosin Infantry | `5bfd297f0db834001a669119` | 41.577 | 6.900 |
| `TUSHONKA` | Can of beef stew (Large) | Tushonka | `57347da92459774491567cf5` | 41.447 | 10.327 |
| `SAIGA12` | Saiga-12K ver.10 12ga semi-automatic shotgun | Saiga-12K | `576165642459773c7a400233` | 40.050 | 9.910 |
| `PBS1` | AKM PBS-1 7.62x39 sound suppressor | PBS-1 | `5a0d63621526d8dba31fe3bf` | 38.927 | 26.978 |
| `DUCT_TAPE` | Duct tape | Duct tape | `57347c1124597737fb1379e3` | 38.690 | 1.574 |
| `MICH2001` | MSA ACH TC-2001 MICH Series helmet (Olive Drab) | TC-2001 | `5d5e7d28a4b936645d161203` | 37.458 | 32.326 |
| `AUGMENTIN` | Augmentin antibiotic pills | Augmentin | `590c695186f7741e566b64a2` | 37.189 | 6.936 |
| `TAC_HELMET` | Tac-Kek FAST MT helmet (Replica) | TK FAST MT | `5ea05cf85ad9772e6624305d` | 36.276 | 6.820 |
| `MBSS` | Flyye MBSS backpack (UCP) | MBSS | `544a5cde4bdc2d39388b456b` | 35.930 | 11.358 |
| `AKMS` | Kalashnikov AKMS 7.62x39 assault rifle | AKMS | `59ff346386f77477562ff5e2` | 35.342 | 15.607 |
| `BOLTS` | Bolts | Bolts | `57347c5b245977448d35f6e1` | 35.215 | 6.930 |
| `MP443` | Yarygin MP-443 Grach 9x19 pistol | MP-443 Grach | `576a581d2459771e7b1bc4f1` | 34.910 | 5.844 |
| `SCREWS` | Pack of screws | Screws | `59e35ef086f7741777737012` | 33.050 | 1.978 |
| `LZSH` | LShZ lightweight helmet (Olive Drab) | LShZ | `5b432d215acfc4771e1c6624` | 31.883 | 19.929 |
| `AK74N` | Kalashnikov AK-74N 5.45x39 assault rifle | AK-74N | `5644bd2b4bdc2d3b4c8b4572` | 30.927 | 12.962 |
| `MULTITOOL` | Leatherman Multitool | MultiTool | `544fb5454bdc2df8738b456a` | 30.567 | 8.602 |
| `PACA` | PACA Soft Armor | PACA | `5648a7494bdc2d9d488b4583` | 29.583 | 16.283 |
| `WIRES` | Bundle of wires | Wires | `5c06779c86f77426e00dd782` | 29.459 | 4.662 |
| `SCAV_BACKPACK` | Scav backpack | ScavBP | `56e335e4d2720b6c058b456d` | 28.697 | 12.462 |
| `AQUAMARI` | Aquamari water bottle with filter | Aquamari | `5c0fa877d174af02a012e1cf` | 27.100 | 19.915 |
| `PNV10T` | PNV-10T night vision goggles | PNV-10T | `5c0696830db834001d23f5da` | 25.253 | 16.800 |
| `AKS74U` | Kalashnikov AKS-74U 5.45x39 assault rifle | AKS-74U | `57dc2fa62459775949412633` | 25.120 | 7.640 |
| `SALEWA` | Salewa first aid kit | Salewa | `544fb45d4bdc2dee738b4568` | 25.104 | 9.506 |
| `ALUMINUM_SPLINT` | Aluminum splint | Alu splint | `5af0454c86f7746bf20992e8` | 24.178 | 4.193 |
| `VASELINE` | Vaseline balm | Vaseline | `5755383e24597772cb798966` | 23.942 | 7.481 |
| `CPU_FAN` | CPU fan | CPU fan | `5734779624597737e04bf329` | 23.017 | 2.942 |
| `SAIGA9` | Saiga-9 9x19 carbine | Saiga-9 | `59f9cabd86f7743a10721f46` | 21.836 | 5.569 |
| `TOOLSET` | Toolset | Toolset | `590c2e1186f77425357b6124` | 21.317 | 19.530 |
| `IFAK` | IFAK individual first aid kit | IFAK | `590c678286f77426c9660122` | 21.141 | 11.976 |
| `6B2` | 6B2 body armor (Flora) | 6B2 | `5df8a2ca86f7740bfe6df777` | 20.882 | 11.124 |
| `CRACKERS` | Army crackers | Crackers | `5448ff904bdc2d6f028b456e` | 20.400 | 1.513 |
| `SSH68` | SSh-68 steel helmet (Olive Drab) | SSh-68 | `5c06c6a80db834001b735491` | 19.852 | 16.244 |
| `WD40` | WD-40 (100ml) | WD-40 | `590c5bbd86f774785762df04` | 19.532 | 4.383 |
| `SQUASH` | Pack of apple juice | Apple | `57513f07245977207e26a311` | 18.444 | 2.155 |
| `VITA_JUICE` | Pack of Vita juice | Vita | `57513fcc24597720a31c09a6` | 18.010 | 2.177 |
| `PSO1` | BelOMO PSO-1 4x24 scope | PSO-1 | `5c82342f2e221644f31c060e` | 15.944 | 10.670 |
| `BAYONET` | 6Kh5 Bayonet | 6Kh5 | `5bffdc370db834001d23eca8` | 15.433 | 16.824 |
| `TOZ106` | TOZ-106 20ga bolt-action shotgun | TOZ-106 | `5a38e6bac4a2826c6e06d79b` | 13.710 | 2.115 |
| `CAT_TOURNIQUET` | CAT hemostatic tourniquet | CAT | `60098af40accd37ef2175f27` | 12.322 | 1.612 |
| `MORPHINE` | Morphine injector | Morphine | `544fb3f34bdc2d03748b456a` | 11.745 | 12.753 |
| `MAKAROV` | Makarov PM 9x18PM pistol | PM | `5448bd6b4bdc2dfc2f8b4569` | 11.230 | 2.294 |
| `OKP7` | OKP-7 reflex sight | OKP-7 | `570fd79bd2720bc7458b4583` | 11.044 | 5.034 |
| `CAR_FIRST_AID` | Car first aid kit | Car | `590c661e86f7741e566b646a` | 9.447 | 4.656 |
| `PK06` | BelOMO PK-06 reflex sight | PK-06 | `57ae0171245977343c27bfcf` | 9.192 | 4.023 |
| `BIPOD_HARRIS` | AI AXMC KeySlot Harris bipod mount | AI Harris | `671126a210d67adb5b08e925` | 8.138 | 943 |
| `INTERCHANGE_MAP` | Interchange plan map | Interchange | `5be4038986f774527d3fae60` | 6.728 | 5.764 |
| `WOODS_MAP` | Woods plan map | Woods | `5900b89686f7744e704a8747` | 6.638 | 8.012 |
| `AI2` | AI-2 medkit | AI-2 | `5755356824597772cb798962` | 5.322 | 2.620 |
| `ANALGIN` | Analgin painkillers | Analgin | `544fb37f4bdc2dee738b4567` | 4.848 | 2.872 |
| `ESMARCH` | Esmarch tourniquet | Esmarch | `5e831507ea0a7c419c2f9bd9` | 3.405 | 967 |
| `ARMY_BANDAGE` | Army bandage | Bandage | `5751a25924597722c463c472` | 2.087 | 1.032 |
| `ROUBLES` | Roubles | RUB | `5449016a4bdc2d6f028b456f` | — | — |

---

## Inventário inicial

> **Atualização 2026-05-17:** após a entrega do item 001 com `AdditionalStartingItems` plano (Opção 1 simplificada — todos os itens no stash sem equipped/nested/slot), foi detectado **overflow do stash inicial L1** (280 slots) em playtest real, mesmo com a soma teórica de slots cabendo nominalmente. Causa raiz: o `BaseProfile: 0` (Standard) já traz itens iniciais que ocupam slots do stash, **somando** com o nosso loadout adicional.
>
> **Decisão de design final:** mudar para `BaseProfile: 8` (**SPT Zero to Hero**), que começa com **stash VAZIO** — toda a capacidade de 280 slots fica disponível para nosso loadout. Como mitigação adicional do tamanho do loadout, mantemos `backup × 2` (reduzido de × 3 originalmente). Stash:1 preservado em todas as 10 classes. Total ₽ por classe: 1.63M–2.02M (faixa: 1.5M–2.05M).
>
> Armeiro permanece com `backup × 2` original (já tinha esse valor por causa do tema caríssimo). As tabelas abaixo ainda mostram a estrutura original (backup × 3) como referência histórica — o gerador [build-profile-jsons.js](../scripts/build-profile-jsons.js) aplica `backupCount: 2` + `BaseProfile: 8` em runtime para todas as classes.

Cada perfil recebe **3 loadouts** (1 vestido + 2 no stash) — calibrados originalmente para ~2.000.000 ₽ totais (preços avg 24h PVE flea via tarkov-market.com); após a redução de backups o intervalo real ficou em 1.63M–2.02M ₽.
Tier-cap: armor classe 1-2 dominante, classe 3 raro (apenas no primary de alguns perfis); helmets até MICH 2001; sem plate hard, sem GPNVG-18.

### Baseline universal (todos os perfis recebem)

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| RUB | `ROUBLES` | 100.000 | 1 | 100.000 |
| Salewa | `SALEWA` | 1 | 25.104 | 25.104 |
| Bandage | `ARMY_BANDAGE` | 2 | 2.087 | 4.174 |
| Alu splint | `ALUMINUM_SPLINT` | 1 | 24.178 | 24.178 |
| Analgin | `ANALGIN` | 1 | 4.848 | 4.848 |
| MRE | `MRE` | 1 | 44.986 | 44.986 |
| Crackers | `CRACKERS` | 1 | 20.400 | 20.400 |
| Aquamari | `AQUAMARI` | 1 | 27.100 | 27.100 |
| 6Kh5 | `BAYONET` | 1 | 15.433 | 15.433 |
| **Total baseline** | | | | **266.223** |

---

### Médico de Combate

**Item-tema (sanitarista):**

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| IFAK | `IFAK` | 2 | 21.141 | 42.282 |
| Surv12 | `SURV12` | 1 | 89.675 | 89.675 |
| CALOK-B | `CALOK_B` | 1 | 70.300 | 70.300 |
| **Subtotal item-tema** | | | | **202.257** |

**Primary loadout** (vestido):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| AKM | `AKM` | 1 | 43.342 | 43.342 |
| AK55 | `MAG_AKM_30` | 4 | 6.467 | 25.868 |
| PS | `AMMO_762x39_PS` | 180 | 700 | 126.000 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| Pst | `AMMO_9x18_PST` | 60 | 50 | 3.000 |
| LShZ | `LZSH` | 1 | 31.883 | 31.883 |
| 6B23-1 EMR | `6B23_1` | 1 | 43.849 | 43.849 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| MBSS | `MBSS` | 1 | 35.930 | 35.930 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Salewa | `SALEWA` | 1 | 25.104 | 25.104 |
| Analgin | `ANALGIN` | 1 | 4.848 | 4.848 |
| Bandage | `ARMY_BANDAGE` | 2 | 2.087 | 4.174 |
| MRE | `MRE` | 1 | 44.986 | 44.986 |
| Aquamari | `AQUAMARI` | 1 | 27.100 | 27.100 |
| **Subtotal primary** | | | | **500.329** |

**Backup loadout** (×3 no stash):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| AKM | `AKM` | 1 | 43.342 | 43.342 |
| AK55 | `MAG_AKM_30` | 4 | 6.467 | 25.868 |
| PS | `AMMO_762x39_PS` | 120 | 700 | 84.000 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| PACA | `PACA` | 1 | 29.583 | 29.583 |
| SSh-68 | `SSH68` | 1 | 19.852 | 19.852 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| ScavBP | `SCAV_BACKPACK` | 1 | 28.697 | 28.697 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Bandage | `ARMY_BANDAGE` | 1 | 2.087 | 2.087 |
| Apple | `SQUASH` | 1 | 18.444 | 18.444 |
| **Subtotal backup unit** | | | | **336.118** |
| **Backup × 3** | | | | **1.008.354** |

**Resumo:**

| Bloco | Subtotal ₽ |
|-------|-----------:|
| Baseline universal | 266.223 |
| Item-tema | 202.257 |
| Primary loadout | 500.329 |
| Backup × 3 | 1.008.354 |
| **Total perfil** | **1.977.163** |
| Distância de 2.000.000 ₽ | -22.837 |

---

### Caçador

**Item-tema (caçador):**

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| Compass | `COMPASS` | 1 | 91.212 | 91.212 |
| Vaseline | `VASELINE` | 1 | 23.942 | 23.942 |
| Tushonka | `TUSHONKA` | 3 | 41.447 | 124.341 |
| Augmentin | `AUGMENTIN` | 3 | 37.189 | 111.567 |
| **Subtotal item-tema** | | | | **351.062** |

**Primary loadout** (vestido):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| SV-98 | `SV98` | 1 | 55.975 | 55.975 |
| SV-98 | `MAG_SV98_10` | 4 | 27.367 | 109.468 |
| LPS | `AMMO_762x54R_LPS` | 80 | 705 | 56.400 |
| PSO-1 | `PSO1` | 1 | 15.944 | 15.944 |
| AI Harris | `BIPOD_HARRIS` | 1 | 8.138 | 8.138 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| Pst | `AMMO_9x18_PST` | 60 | 50 | 3.000 |
| LShZ | `LZSH` | 1 | 31.883 | 31.883 |
| 6B2 | `6B2` | 1 | 20.882 | 20.882 |
| TV-110 | `TRIPLE_BANDOLIER` | 1 | 74.041 | 74.041 |
| Pilgrim | `PILGRIM` | 1 | 78.999 | 78.999 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Salewa | `SALEWA` | 1 | 25.104 | 25.104 |
| Analgin | `ANALGIN` | 1 | 4.848 | 4.848 |
| Bandage | `ARMY_BANDAGE` | 2 | 2.087 | 4.174 |
| MRE | `MRE` | 1 | 44.986 | 44.986 |
| Aquamari | `AQUAMARI` | 1 | 27.100 | 27.100 |
| **Subtotal primary** | | | | **598.553** |

**Backup loadout** (×3 no stash):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| Mosin Infantry | `MOSIN_INFANTRY` | 1 | 41.577 | 41.577 |
| LPS | `AMMO_762x54R_LPS` | 60 | 705 | 42.300 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| PACA | `PACA` | 1 | 29.583 | 29.583 |
| SSh-68 | `SSH68` | 1 | 19.852 | 19.852 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| ScavBP | `SCAV_BACKPACK` | 1 | 28.697 | 28.697 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Bandage | `ARMY_BANDAGE` | 1 | 2.087 | 2.087 |
| Apple | `SQUASH` | 1 | 18.444 | 18.444 |
| **Subtotal backup unit** | | | | **266.785** |
| **Backup × 3** | | | | **800.355** |

**Resumo:**

| Bloco | Subtotal ₽ |
|-------|-----------:|
| Baseline universal | 266.223 |
| Item-tema | 351.062 |
| Primary loadout | 598.553 |
| Backup × 3 | 800.355 |
| **Total perfil** | **2.016.193** |
| Distância de 2.000.000 ₽ | 16.193 |

---

### Fuzileiro

**Item-tema (fuzileiro):**

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| AK55 | `MAG_AKM_30` | 2 | 6.467 | 12.934 |
| **Subtotal item-tema** | | | | **12.934** |

**Primary loadout** (vestido):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| AKM | `AKM` | 1 | 43.342 | 43.342 |
| AK55 | `MAG_AKM_30` | 4 | 6.467 | 25.868 |
| BP | `AMMO_762x39_BP` | 180 | 1.111 | 199.980 |
| OKP-7 | `OKP7` | 1 | 11.044 | 11.044 |
| MP-443 Grach | `MP443` | 1 | 34.910 | 34.910 |
| MP-443 | `MAG_MP443_18` | 2 | 4.633 | 9.266 |
| Pst | `AMMO_9x19_PST` | 60 | 100 | 6.000 |
| LShZ | `LZSH` | 1 | 31.883 | 31.883 |
| 6B23-1 EMR | `6B23_1` | 1 | 43.849 | 43.849 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| Tri-Zip | `TRIZIP` | 1 | 100.920 | 100.920 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Salewa | `SALEWA` | 1 | 25.104 | 25.104 |
| Analgin | `ANALGIN` | 1 | 4.848 | 4.848 |
| Bandage | `ARMY_BANDAGE` | 2 | 2.087 | 4.174 |
| MRE | `MRE` | 1 | 44.986 | 44.986 |
| Aquamari | `AQUAMARI` | 1 | 27.100 | 27.100 |
| **Subtotal primary** | | | | **681.049** |

**Backup loadout** (×3 no stash):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| AKM | `AKM` | 1 | 43.342 | 43.342 |
| AK55 | `MAG_AKM_30` | 4 | 6.467 | 25.868 |
| PS | `AMMO_762x39_PS` | 120 | 700 | 84.000 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| PACA | `PACA` | 1 | 29.583 | 29.583 |
| SSh-68 | `SSH68` | 1 | 19.852 | 19.852 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| ScavBP | `SCAV_BACKPACK` | 1 | 28.697 | 28.697 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Bandage | `ARMY_BANDAGE` | 1 | 2.087 | 2.087 |
| Apple | `SQUASH` | 1 | 18.444 | 18.444 |
| **Subtotal backup unit** | | | | **336.118** |
| **Backup × 3** | | | | **1.008.354** |

**Resumo:**

| Bloco | Subtotal ₽ |
|-------|-----------:|
| Baseline universal | 266.223 |
| Item-tema | 12.934 |
| Primary loadout | 681.049 |
| Backup × 3 | 1.008.354 |
| **Total perfil** | **1.968.560** |
| Distância de 2.000.000 ₽ | -31.440 |

---

### Batedor

**Item-tema (batedor):**

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| Compass | `COMPASS` | 1 | 91.212 | 91.212 |
| Aquamari | `AQUAMARI` | 1 | 27.100 | 27.100 |
| eTG-c | `ETG_CHANGE` | 1 | 304.556 | 304.556 |
| **Subtotal item-tema** | | | | **422.868** |

**Primary loadout** (vestido):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| AKS-74U | `AKS74U` | 1 | 25.120 | 25.120 |
| AK-12 | `MAG_AK_30` | 4 | 8.033 | 32.132 |
| BS | `AMMO_545x39_BS` | 120 | 780 | 93.600 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| Pst | `AMMO_9x18_PST` | 60 | 50 | 3.000 |
| TK FAST MT | `TAC_HELMET` | 1 | 36.276 | 36.276 |
| 6B2 | `6B2` | 1 | 20.882 | 20.882 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| Paratus | `PARATUS` | 1 | 58.900 | 58.900 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Salewa | `SALEWA` | 1 | 25.104 | 25.104 |
| Analgin | `ANALGIN` | 1 | 4.848 | 4.848 |
| Bandage | `ARMY_BANDAGE` | 2 | 2.087 | 4.174 |
| MRE | `MRE` | 1 | 44.986 | 44.986 |
| Aquamari | `AQUAMARI` | 1 | 27.100 | 27.100 |
| **Subtotal primary** | | | | **460.367** |

**Backup loadout** (×3 no stash):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| AKS-74U | `AKS74U` | 1 | 25.120 | 25.120 |
| AK-12 | `MAG_AK_30` | 4 | 8.033 | 32.132 |
| PS | `AMMO_545x39_PS` | 120 | 281 | 33.720 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| PACA | `PACA` | 1 | 29.583 | 29.583 |
| SSh-68 | `SSH68` | 1 | 19.852 | 19.852 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| ScavBP | `SCAV_BACKPACK` | 1 | 28.697 | 28.697 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Bandage | `ARMY_BANDAGE` | 1 | 2.087 | 2.087 |
| Apple | `SQUASH` | 1 | 18.444 | 18.444 |
| **Subtotal backup unit** | | | | **273.880** |
| **Backup × 3** | | | | **821.640** |

**Resumo:**

| Bloco | Subtotal ₽ |
|-------|-----------:|
| Baseline universal | 266.223 |
| Item-tema | 422.868 |
| Primary loadout | 460.367 |
| Backup × 3 | 821.640 |
| **Total perfil** | **1.971.098** |
| Distância de 2.000.000 ₽ | -28.902 |

---

### Operador Noturno

**Item-tema (operador noturno):**

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| PNV-10T | `PNV10T` | 1 | 25.253 | 25.253 |
| PBS-1 | `PBS1` | 1 | 38.927 | 38.927 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Tushonka | `TUSHONKA` | 3 | 41.447 | 124.341 |
| Augmentin | `AUGMENTIN` | 1 | 37.189 | 37.189 |
| US | `AMMO_762x39_US` | 60 | 455 | 27.300 |
| **Subtotal item-tema** | | | | **274.151** |

**Primary loadout** (vestido):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| AKMS | `AKMS` | 1 | 35.342 | 35.342 |
| AK55 | `MAG_AKM_30` | 4 | 6.467 | 25.868 |
| US | `AMMO_762x39_US` | 180 | 455 | 81.900 |
| PBS-1 | `PBS1` | 1 | 38.927 | 38.927 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| Pst | `AMMO_9x18_PST` | 60 | 50 | 3.000 |
| LShZ | `LZSH` | 1 | 31.883 | 31.883 |
| 6B2 | `6B2` | 1 | 20.882 | 20.882 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| MBSS | `MBSS` | 1 | 35.930 | 35.930 |
| PNV-10T | `PNV10T` | 1 | 25.253 | 25.253 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Salewa | `SALEWA` | 1 | 25.104 | 25.104 |
| Analgin | `ANALGIN` | 1 | 4.848 | 4.848 |
| Bandage | `ARMY_BANDAGE` | 2 | 2.087 | 4.174 |
| MRE | `MRE` | 1 | 44.986 | 44.986 |
| Aquamari | `AQUAMARI` | 1 | 27.100 | 27.100 |
| **Subtotal primary** | | | | **489.442** |

**Backup loadout** (×3 no stash):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| AKMS | `AKMS` | 1 | 35.342 | 35.342 |
| AK55 | `MAG_AKM_30` | 4 | 6.467 | 25.868 |
| PS | `AMMO_762x39_PS` | 120 | 700 | 84.000 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| PACA | `PACA` | 1 | 29.583 | 29.583 |
| SSh-68 | `SSH68` | 1 | 19.852 | 19.852 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| ScavBP | `SCAV_BACKPACK` | 1 | 28.697 | 28.697 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Bandage | `ARMY_BANDAGE` | 1 | 2.087 | 2.087 |
| Apple | `SQUASH` | 1 | 18.444 | 18.444 |
| **Subtotal backup unit** | | | | **328.118** |
| **Backup × 3** | | | | **984.354** |

**Resumo:**

| Bloco | Subtotal ₽ |
|-------|-----------:|
| Baseline universal | 266.223 |
| Item-tema | 274.151 |
| Primary loadout | 489.442 |
| Backup × 3 | 984.354 |
| **Total perfil** | **2.014.170** |
| Distância de 2.000.000 ₽ | 14.170 |

---

### Armeiro

**Item-tema (armeiro):**

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| Weapon repair kit | `WEAPON_REPAIR_KIT` | 1 | 577.654 | 577.654 |
| Toolset | `TOOLSET` | 1 | 21.317 | 21.317 |
| WD-40 | `WD40` | 1 | 19.532 | 19.532 |
| MultiTool | `MULTITOOL` | 1 | 30.567 | 30.567 |
| Bolts | `BOLTS` | 1 | 35.215 | 35.215 |
| **Subtotal item-tema** | | | | **684.285** |

**Primary loadout** (vestido):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| AKM | `AKM` | 1 | 43.342 | 43.342 |
| AK55 | `MAG_AKM_30` | 4 | 6.467 | 25.868 |
| PS | `AMMO_762x39_PS` | 120 | 700 | 84.000 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| Pst | `AMMO_9x18_PST` | 60 | 50 | 3.000 |
| TK FAST MT | `TAC_HELMET` | 1 | 36.276 | 36.276 |
| 6B2 | `6B2` | 1 | 20.882 | 20.882 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| MBSS | `MBSS` | 1 | 35.930 | 35.930 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Salewa | `SALEWA` | 1 | 25.104 | 25.104 |
| Analgin | `ANALGIN` | 1 | 4.848 | 4.848 |
| Bandage | `ARMY_BANDAGE` | 2 | 2.087 | 4.174 |
| MRE | `MRE` | 1 | 44.986 | 44.986 |
| Aquamari | `AQUAMARI` | 1 | 27.100 | 27.100 |
| **Subtotal primary** | | | | **439.755** |

**Backup loadout** (×2 no stash):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| AKM | `AKM` | 1 | 43.342 | 43.342 |
| AK55 | `MAG_AKM_30` | 4 | 6.467 | 25.868 |
| PS | `AMMO_762x39_PS` | 90 | 700 | 63.000 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| PACA | `PACA` | 1 | 29.583 | 29.583 |
| SSh-68 | `SSH68` | 1 | 19.852 | 19.852 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| ScavBP | `SCAV_BACKPACK` | 1 | 28.697 | 28.697 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Bandage | `ARMY_BANDAGE` | 1 | 2.087 | 2.087 |
| Apple | `SQUASH` | 1 | 18.444 | 18.444 |
| **Subtotal backup unit** | | | | **315.118** |
| **Backup × 2** | | | | **630.236** |

**Resumo:**

| Bloco | Subtotal ₽ |
|-------|-----------:|
| Baseline universal | 266.223 |
| Item-tema | 684.285 |
| Primary loadout | 439.755 |
| Backup × 2 | 630.236 |
| **Total perfil** | **2.020.499** |
| Distância de 2.000.000 ₽ | 20.499 |

---

### Operador Tático

**Item-tema (operador tático):**

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| eTG-c | `ETG_CHANGE` | 1 | 304.556 | 304.556 |
| **Subtotal item-tema** | | | | **304.556** |

**Primary loadout** (vestido):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| M4A1 | `M4A1` | 1 | 65.000 | 65.000 |
| GEN M3 | `MAG_M4_30` | 4 | 17.335 | 69.340 |
| M855 | `AMMO_556x45_M855` | 180 | 272 | 48.960 |
| OKP-7 | `OKP7` | 1 | 11.044 | 11.044 |
| MP-443 Grach | `MP443` | 1 | 34.910 | 34.910 |
| MP-443 | `MAG_MP443_18` | 2 | 4.633 | 9.266 |
| Pst | `AMMO_9x19_PST` | 60 | 100 | 6.000 |
| TC-2001 | `MICH2001` | 1 | 37.458 | 37.458 |
| 6B23-1 EMR | `6B23_1` | 1 | 43.849 | 43.849 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| Tri-Zip | `TRIZIP` | 1 | 100.920 | 100.920 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Salewa | `SALEWA` | 1 | 25.104 | 25.104 |
| Analgin | `ANALGIN` | 1 | 4.848 | 4.848 |
| Bandage | `ARMY_BANDAGE` | 2 | 2.087 | 4.174 |
| MRE | `MRE` | 1 | 44.986 | 44.986 |
| Aquamari | `AQUAMARI` | 1 | 27.100 | 27.100 |
| **Subtotal primary** | | | | **600.734** |

**Backup loadout** (×3 no stash):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| AK-74N | `AK74N` | 1 | 30.927 | 30.927 |
| AK-12 | `MAG_AK_30` | 4 | 8.033 | 32.132 |
| PS | `AMMO_545x39_PS` | 90 | 281 | 25.290 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| PACA | `PACA` | 1 | 29.583 | 29.583 |
| SSh-68 | `SSH68` | 1 | 19.852 | 19.852 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| ScavBP | `SCAV_BACKPACK` | 1 | 28.697 | 28.697 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Bandage | `ARMY_BANDAGE` | 1 | 2.087 | 2.087 |
| Apple | `SQUASH` | 1 | 18.444 | 18.444 |
| **Subtotal backup unit** | | | | **271.257** |
| **Backup × 3** | | | | **813.771** |

**Resumo:**

| Bloco | Subtotal ₽ |
|-------|-----------:|
| Baseline universal | 266.223 |
| Item-tema | 304.556 |
| Primary loadout | 600.734 |
| Backup × 3 | 813.771 |
| **Total perfil** | **1.985.284** |
| Distância de 2.000.000 ₽ | -14.716 |

---

### Sobrevivencialista

**Item-tema (sobrevivencialista):**

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| Tushonka | `TUSHONKA` | 6 | 41.447 | 248.682 |
| Aquamari | `AQUAMARI` | 4 | 27.100 | 108.400 |
| Augmentin | `AUGMENTIN` | 5 | 37.189 | 185.945 |
| Vaseline | `VASELINE` | 4 | 23.942 | 95.768 |
| AI-2 | `AI2` | 7 | 5.322 | 37.254 |
| MultiTool | `MULTITOOL` | 1 | 30.567 | 30.567 |
| **Subtotal item-tema** | | | | **706.616** |

**Primary loadout** (vestido):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| Saiga-12K | `SAIGA12` | 1 | 40.050 | 40.050 |
| 7mm | `AMMO_12_70_MAG` | 30 | 77 | 2.310 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| Pst | `AMMO_9x18_PST` | 60 | 50 | 3.000 |
| TK FAST MT | `TAC_HELMET` | 1 | 36.276 | 36.276 |
| 6B2 | `6B2` | 1 | 20.882 | 20.882 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| Pilgrim | `PILGRIM` | 1 | 78.999 | 78.999 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Salewa | `SALEWA` | 1 | 25.104 | 25.104 |
| Analgin | `ANALGIN` | 1 | 4.848 | 4.848 |
| Bandage | `ARMY_BANDAGE` | 2 | 2.087 | 4.174 |
| MRE | `MRE` | 1 | 44.986 | 44.986 |
| Aquamari | `AQUAMARI` | 1 | 27.100 | 27.100 |
| **Subtotal primary** | | | | **371.974** |

**Backup loadout** (×3 no stash):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| TOZ-106 | `TOZ106` | 1 | 13.710 | 13.710 |
| Sb.3x4 | `MAG_TOZ106_4` | 2 | 1.489 | 2.978 |
| 7.5mm | `AMMO_20_70_BUCK` | 1 | 22.122 | 22.122 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| PACA | `PACA` | 1 | 29.583 | 29.583 |
| SSh-68 | `SSH68` | 1 | 19.852 | 19.852 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| ScavBP | `SCAV_BACKPACK` | 1 | 28.697 | 28.697 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Bandage | `ARMY_BANDAGE` | 1 | 2.087 | 2.087 |
| Apple | `SQUASH` | 1 | 18.444 | 18.444 |
| **Subtotal backup unit** | | | | **221.718** |
| **Backup × 3** | | | | **665.154** |

**Resumo:**

| Bloco | Subtotal ₽ |
|-------|-----------:|
| Baseline universal | 266.223 |
| Item-tema | 706.616 |
| Primary loadout | 371.974 |
| Backup × 3 | 665.154 |
| **Total perfil** | **2.009.967** |
| Distância de 2.000.000 ₽ | 9.967 |

---

### Saqueador

**Item-tema (saqueador):**

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| Docs | `DOCUMENTS_CASE` | 2 | 106.323 | 212.646 |
| MultiTool | `MULTITOOL` | 1 | 30.567 | 30.567 |
| Screws | `SCREWS` | 1 | 33.050 | 33.050 |
| Wires | `WIRES` | 1 | 29.459 | 29.459 |
| Duct tape | `DUCT_TAPE` | 1 | 38.690 | 38.690 |
| RUB | `ROUBLES` | 200.000 | 1 | 200.000 |
| **Subtotal item-tema** | | | | **544.412** |

**Primary loadout** (vestido):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| Saiga-9 | `SAIGA9` | 1 | 21.836 | 21.836 |
| Sb.7 | `MAG_SAIGA9_10` | 4 | 13.981 | 55.924 |
| Pst | `AMMO_9x19_PST` | 80 | 100 | 8.000 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| Pst | `AMMO_9x18_PST` | 60 | 50 | 3.000 |
| TK FAST MT | `TAC_HELMET` | 1 | 36.276 | 36.276 |
| 6B2 | `6B2` | 1 | 20.882 | 20.882 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| Pilgrim | `PILGRIM` | 1 | 78.999 | 78.999 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Salewa | `SALEWA` | 1 | 25.104 | 25.104 |
| Analgin | `ANALGIN` | 1 | 4.848 | 4.848 |
| Bandage | `ARMY_BANDAGE` | 2 | 2.087 | 4.174 |
| MRE | `MRE` | 1 | 44.986 | 44.986 |
| Aquamari | `AQUAMARI` | 1 | 27.100 | 27.100 |
| **Subtotal primary** | | | | **415.374** |

**Backup loadout** (×3 no stash):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| TOZ-106 | `TOZ106` | 1 | 13.710 | 13.710 |
| Sb.3x4 | `MAG_TOZ106_4` | 1 | 1.489 | 1.489 |
| 7.5mm | `AMMO_20_70_BUCK` | 1 | 22.122 | 22.122 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| PACA | `PACA` | 1 | 29.583 | 29.583 |
| SSh-68 | `SSH68` | 1 | 19.852 | 19.852 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| Paratus | `PARATUS` | 1 | 58.900 | 58.900 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Bandage | `ARMY_BANDAGE` | 1 | 2.087 | 2.087 |
| Apple | `SQUASH` | 1 | 18.444 | 18.444 |
| **Subtotal backup unit** | | | | **250.432** |
| **Backup × 3** | | | | **751.296** |

**Resumo:**

| Bloco | Subtotal ₽ |
|-------|-----------:|
| Baseline universal | 266.223 |
| Item-tema | 544.412 |
| Primary loadout | 415.374 |
| Backup × 3 | 751.296 |
| **Total perfil** | **1.977.305** |
| Distância de 2.000.000 ₽ | -22.695 |

---

### Gerente de Operações

**Item-tema (gerente de operações):**

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| Toolset | `TOOLSET` | 2 | 21.317 | 42.634 |
| CPU fan | `CPU_FAN` | 4 | 23.017 | 92.068 |
| Wires | `WIRES` | 4 | 29.459 | 117.836 |
| Duct tape | `DUCT_TAPE` | 3 | 38.690 | 116.070 |
| Bolts | `BOLTS` | 1 | 35.215 | 35.215 |
| Screws | `SCREWS` | 1 | 33.050 | 33.050 |
| RUB | `ROUBLES` | 300.000 | 1 | 300.000 |
| **Subtotal item-tema** | | | | **736.873** |

**Primary loadout** (vestido):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| Saiga-12K | `SAIGA12` | 1 | 40.050 | 40.050 |
| 7mm | `AMMO_12_70_MAG` | 20 | 77 | 1.540 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| Pst | `AMMO_9x18_PST` | 60 | 50 | 3.000 |
| TK FAST MT | `TAC_HELMET` | 1 | 36.276 | 36.276 |
| 6B2 | `6B2` | 1 | 20.882 | 20.882 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| MBSS | `MBSS` | 1 | 35.930 | 35.930 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Salewa | `SALEWA` | 1 | 25.104 | 25.104 |
| Analgin | `ANALGIN` | 1 | 4.848 | 4.848 |
| Bandage | `ARMY_BANDAGE` | 2 | 2.087 | 4.174 |
| MRE | `MRE` | 1 | 44.986 | 44.986 |
| Aquamari | `AQUAMARI` | 1 | 27.100 | 27.100 |
| **Subtotal primary** | | | | **328.135** |

**Backup loadout** (×3 no stash):

| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |
|------|----|----:|-------:|-----------:|
| TOZ-106 | `TOZ106` | 1 | 13.710 | 13.710 |
| Sb.3x4 | `MAG_TOZ106_4` | 1 | 1.489 | 1.489 |
| 7.5mm | `AMMO_20_70_BUCK` | 1 | 22.122 | 22.122 |
| PM | `MAKAROV` | 1 | 11.230 | 11.230 |
| PM | `MAG_PM_8` | 2 | 2.620 | 5.240 |
| PACA | `PACA` | 1 | 29.583 | 29.583 |
| SSh-68 | `SSH68` | 1 | 19.852 | 19.852 |
| BlackRock | `BLACKROCK` | 1 | 46.634 | 46.634 |
| ScavBP | `SCAV_BACKPACK` | 1 | 28.697 | 28.697 |
| IFAK | `IFAK` | 1 | 21.141 | 21.141 |
| Bandage | `ARMY_BANDAGE` | 1 | 2.087 | 2.087 |
| Apple | `SQUASH` | 1 | 18.444 | 18.444 |
| **Subtotal backup unit** | | | | **220.229** |
| **Backup × 3** | | | | **660.687** |

**Resumo:**

| Bloco | Subtotal ₽ |
|-------|-----------:|
| Baseline universal | 266.223 |
| Item-tema | 736.873 |
| Primary loadout | 328.135 |
| Backup × 3 | 660.687 |
| **Total perfil** | **1.991.918** |
| Distância de 2.000.000 ₽ | -8.082 |

---

### Resumo de calibração

| Perfil | Total ₽ | Δ 2M |
|--------|--------:|-----:|
| Médico de Combate | 1.977.163 | -22.837 ✓ |
| Caçador | 2.016.193 | 16.193 ✓ |
| Fuzileiro | 1.968.560 | -31.440 ✓ |
| Batedor | 1.971.098 | -28.902 ✓ |
| Operador Noturno | 2.014.170 | 14.170 ✓ |
| Armeiro | 2.020.499 | 20.499 ✓ |
| Operador Tático | 1.985.284 | -14.716 ✓ |
| Sobrevivencialista | 2.009.967 | 9.967 ✓ |
| Saqueador | 1.977.305 | -22.695 ✓ |
| Gerente de Operações | 1.991.918 | -8.082 ✓ |

