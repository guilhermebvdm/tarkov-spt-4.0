# SPT 4.0 — Flea price formula validation (test 6 + test 7)

Dados consolidados dos dois últimos cenários de teste para validar a fórmula de geração de preço de oferta no flea.

## Contexto

- **SPT install**: `D:/SPT/SPT/` (4.0.13)
- **FIKA**: 2.2.6
- **`useHandbookPrice`**: `true` em `configs/ragfair.json`
- **`priceMultiplier`**: `1.5` em `configs/ragfair.json`
- **DrakiaXYZ-LiveFleaPrices**: **desativado** (mod renomeado para `.disabled`)
- **Outros mods que mexem em flea**: nenhum conhecido

Todas observações foram feitas após **restart completo do server** com os valores aplicados, lendo a oferta mínima visível no flea in-game para cada item (filtros: `Exclude bartering offers` ON, `Operational only` ON).

## Identificadores dos itens

| Item | BSG Tpl | conditionType | Notes |
| --- | --- | --- | --- |
| Encrypted flash drive | `660bbc47c38b837877075e47` | none | Control item |
| IFAK individual first aid kit | `590c678286f77426c9660122` | resource (`MaxHpResource`) | Partial offers exist |
| Salewa first aid kit | `544fb45d4bdc2dee738b4568` | resource (`MaxHpResource`) | Partial offers exist |
| RB-VO marked key | `5d80c62a86f7744036212b3f` | uses (`MaximumNumberOfUsage`) | Partial offers possible |
| LEDX Skin Transilluminator | `5c0530ee86f774697952d952` | none | Clean |
| AI-2 medkit | `5755356824597772cb798962` | resource | Partial offers exist |
| Army bandage | `5751a25924597722c463c472` | resource | Partial offers exist |
| Graphics card | `57347ca924597744596b4e71` | none | Clean |
| Bolts | `57347c5b245977448d35f6e1` | none | Clean |
| Tetriz portable game console | `5c12620d86f7743f8b198b72` | none | Clean |
| Bundle of wires | `5c06779c86f77426e00dd782` | none | Clean |
| Intelligence folder | `5c12613b86f7743bbe2c3f76` | none | **NAME COLLISION**: `638e0752...` also called "Intelligence folder" — search may contaminate |

---

## TEST 6 — formula identification with resource and non-resource items

### Inputs (escritos em `D:/SPT/SPT/SPT_Data/database/templates/`)

| Item | Tpl | `prices.json[tpl]` | `handbook.Items[tpl].Price` |
| --- | --- | --- | --- |
| Encrypted flash drive | `660bbc47c38b837877075e47` | 10000000 | 100000 |
| IFAK | `590c678286f77426c9660122` | 1000 | 1000000 |
| Salewa first aid kit | `544fb45d4bdc2dee738b4568` | 1000 | 4000000 |
| RB-VO marked key | `5d80c62a86f7744036212b3f` | 1000 | 10000000 |
| LEDX | `5c0530ee86f774697952d952` | 1000 | 20000000 |
| AI-2 medkit | `5755356824597772cb798962` | 0 | 5000000 |
| Army bandage | `5751a25924597722c463c472` | **\<deleted from prices.json\>** | 3000000 |

### Observed flea offer prices (RUB unless noted)

#### Encrypted flash drive — "encrypt" search

| Trader | Total | Price |
| --- | --- | --- |
| FriedEngineer | 3 | 8,388,600 |
| TeejayMerks | 2 | 8,490,900 |
| gansik | 3 | 8,797,800 |
| Dueleus (EUR) | 10 | 66,918 € |
| Pherik | 10 | 9,104,700 |
| Ereshkigal | 7 | 9,104,700 |
| MechanicMan396 | 3 | 9,718,500 |
| OptimusChad | 7 | 9,923,100 |
| Quik | 6 | 9,923,100 |
| halik92 | 2 | 10,025,400 |
| Arys | 5 | 10,332,300 |
| skitles55 | 2 | 10,332,300 |
| (more not shown) | | |

(Test 6 had screenshot showing range 8.9M-10.6M; reset minimum closer to test 7 result of 8.39M after second restart.)

#### IFAK — "ifak" search

| Trader | Quality | Price |
| --- | --- | --- |
| Therapist | LOCKED (limit 6) | 43,524 (trader offer, ignore) |
| rozema | 180/300 (partial) | 1,325,376 |
| jbs4bmx | 180/300 (partial) | 1,380,600 |
| Karthel | 300/300 (full) | **1,955,850** ← only full-charge offer |
| rayleefx | 300/300 (full) | 14,706 € |

#### Salewa first aid kit — "salewa" search

| Trader | Quality | Price |
| --- | --- | --- |
| Therapist | LOCKED | 37,061 (trader, ignore) |
| Bushtail (EUR) | 240/400 (partial) | 27,072 € |
| [666]Silent | 240/400 (partial) | 3,852,642 |
| jpdarkone | 400/400 (full) | **5,160,860** |
| KWJimWalls | 400/400 (full) | **5,640,940** |

#### RB-VO marked key — "rb-vo" search

| Trader | Total | Price |
| --- | --- | --- |
| Tyrian | 10/10 | 12,080,000 |
| konstantin90s | 10/10 | 103,183 $ |
| techy | 10/10 | 12,835,000 |
| bkreporn | 10/10 | 13,137,000 |
| MiseryMachinery | 10/10 | 13,288,000 |
| Echo55 | 10/10 | 118,283 $ |
| CWX | 10/10 | 14,345,000 |
| TR_LEOPAR | 10/10 | 14,647,000 |
| 红衣抚琴染绛天涯 | 10/10 | 14,798,000 |
| Ratthew | 10/10 | 15,100,000 |
| ThinkSlow | 10/10 | 15,855,000 |
| TROMBON | 10/10 | 16,610,000 |

#### LEDX — "ledx" search

| Trader | Total | Price |
| --- | --- | --- |
| ColonelPeePantz | 7 | 24,300,810 |
| ALameLlama | 10 | 27,000,900 |
| LightoftheWorld | 7 | 28,800,960 |
| Akiw | 10 | 29,100,970 |
| RootsNine | 8 | 29,700,990 |
| MrSarkasm | 3 | 30,001,000 |
| Floppa (EUR) | 6 | 227,827 € |
| Shibdib | 4 | 31,201,040 |
| savid_dubs | 4 | 32,401,080 |
| weardo98 | 4 | 32,701,090 |
| numberdjester (EUR) | 9 | 245,873 € |
| JaydanCan | 9 | 33,901,130 |

#### AI-2 medkit — "ai-2" search

| Trader | Quality | Price |
| --- | --- | --- |
| Therapist | LOCKED | 6,638 (trader, ignore) |
| Lavax | 60/100 (partial) | 5,865,000 |
| Navi | 60/100 (partial) | 7,176,000 |
| stckytwl | 60/100 (partial) | 8,073,000 |
| stella | 100/100 (full) | **9,200,000** |

#### Army bandage — "army bandage" search

| Trader | Quality | Price |
| --- | --- | --- |
| Therapist | LOCKED | 2,275 (trader, ignore) |
| Archy | 1/2 (partial) | 3,622,500 |
| LeftHandedCat | 2/2 (full) | **5,796,000** |
| Pherik | 2/2 (full) | **6,072,000** |
| skitles55 | 2/2 (full) | **6,417,000** |

---

## TEST 7 — `prices=0` vs `prices=missing` disambiguation (non-resource items only)

### Inputs

| Item | Tpl | `prices.json[tpl]` | `handbook.Items[tpl].Price` |
| --- | --- | --- | --- |
| Encrypted flash drive (control) | `660bbc47c38b837877075e47` | 10000000 | 100000 |
| Graphics card | `57347ca924597744596b4e71` | **\<deleted\>** | 5000000 |
| Bolts | `57347c5b245977448d35f6e1` | **0** | 5000000 |
| Tetriz | `5c12620d86f7743f8b198b72` | **\<deleted\>** | 8000000 |
| Bundle of wires | `5c06779c86f77426e00dd782` | **0** | 8000000 |
| Intelligence folder | `5c12613b86f7743bbe2c3f76` | 1000 | 6000000 |

### Observed flea offer prices

#### Encrypted flash drive — "encrypted" search

| Trader | Total | Price |
| --- | --- | --- |
| FriedEngineer | 3 | 8,388,600 |
| TeejayMerks | 2 | 8,490,900 |
| gansik | 3 | 8,797,800 |
| Dueleus | 10 | 66,918 € |
| Pherik | 10 | 9,104,700 |
| Ereshkigal | 7 | 9,104,700 |
| MechanicMan396 | 3 | 9,718,500 |
| OptimusChad | 7 | 9,923,100 |
| Quik | 6 | 9,923,100 |
| halik92 | 2 | 10,025,400 |
| Arys | 5 | 10,332,300 |
| skitles55 | 2 | 10,332,300 |

#### Graphics card — "graphi" search

| Trader | Total | Price |
| --- | --- | --- |
| Ref (trader) | LOCKED | 45 GP |
| WispsFlame | 2 | 6,000,000 |
| Turok | 8 | 6,150,000 |
| fryciarz7 | 2 | 6,525,000 |
| Dirtbikercj | 1 | 6,975,000 |
| Spock | 1 | 7,050,000 |
| jbs4bmx (EUR) | 10 | 53,008 € |
| CreamCheese (EUR) | 8 | 56,391 € |
| Belette | 3 | 7,575,000 |
| YankeeNoodle742 (USD) | 6 | 63,750 $ |
| ItsRenke | 9 | 8,175,000 |
| Kilgor616 | 1 | 8,175,000 |

#### Bolts — "bolts" search

| Trader | Total | Price |
| --- | --- | --- |
| jfetko | 7 | 9,430,000 |
| NastyInc (EUR) | 3 | 75,226 € |
| S3NN0M0 (EUR) | 4 | 76,090 € |
| toaddsworth (EUR) | 9 | 76,090 € |
| jordanbr | 5 | 10,465,000 |
| Tariyihika | 9 | 10,810,000 |
| Sugar | 9 | 11,270,000 |
| Super | 3 | 11,615,000 |
| VioletAmbush (USD) | 3 | 97,750 $ |

#### Tetriz portable game console — "tetriz" search

| Trader | Total | Price |
| --- | --- | --- |
| garlicbreadtcg | 1 | 10,680,000 |
| sptlaggy (USD) | 9 | 92,000 $ |
| Barlog_M | 8 | 11,520,000 |
| Pherik (EUR) | 8 | 86,617 € |
| NORVINSK_EVANGELION | 1 | 11,640,000 |
| Plaguey | 5 | 11,640,000 |
| snowythefox811 | 4 | 11,760,000 |
| btdc00 | 1 | 11,760,000 |
| 老人 | 7 | 11,760,000 |
| TheSparta | 10 | 11,880,000 |
| Golani | 1 | 11,880,000 |
| Super | 7 | 11,880,000 |

#### Bundle of wires — "wires" search

| Trader | Total | Price |
| --- | --- | --- |
| Seion | 6 | 16,744,000 |
| RagingBeardo (USD) | 5 | 142,600 $ |
| MrSarkasm | 10 | 17,296,000 |
| matt | 3 | 17,480,000 |
| blkdnm | 9 | 17,480,000 |
| BaddestDragon | 1 | 17,480,000 |
| Navi | 10 | 18,768,000 |
| _物是人非 | 10 | 18,952,000 |
| Amands2Mello (EUR) | 5 | 143,880 € |
| RakTheGoose | 7 | 19,504,000 |
| Nikita (USD) | 7 | 167,133 $ |

#### Intelligence folder — "folder" search

⚠️ **Potential contamination**: `638e0752ab150a5f56238962` also called "Intelligence folder" — not edited in this test, its original price would be ~300K. If this tpl appears in flea, observations would mix with our edited tpl.

| Trader | Total | Price |
| --- | --- | --- |
| NugentGL (USD) | 3 | 97,757 $ |
| EpicRangeTime | 6 | 12,696,920 |
| btdc00 | 5 | 13,248,960 |
| OXIdiezd | 4 | 13,386,970 |
| Hobbes | 10 | 13,801,000 |
| FiveF (USD) | 5 | 128,809 $ |
| aburiu | 9 | 15,595,130 |

---

## Resumo do ratio `observed_min / handbook` (full-charge offers, RUB only)

| Item | prices state | handbook | observed_min (full) | k_min | k_mean |
| --- | --- | --- | --- | --- | --- |
| **Encrypted (test 6 + 7)** | prices=10M | 100K | 8,388,600 | n/a (prices dominates) | mean 9.36M |
| **GPU (test 7)** | DELETED | 5M | 6,000,000 | 1.20 | ~1.42 |
| **Bolts (test 7)** | 0 | 5M | 9,430,000 | 1.89 | ~2.10 |
| **Tetriz (test 7)** | DELETED | 8M | 10,680,000 | 1.335 | ~1.41 |
| **Wires (test 7)** | 0 | 8M | 16,744,000 | 2.09 | ~2.27 |
| **Intelligence (test 7)** | 1000 | 6M | 12,696,920 | 2.12 | ~2.36 ⚠️ contam |
| **IFAK (test 6)** | 1000 | 1M | 1,955,850 (only full) | 1.96 | 1.96 |
| **Salewa (test 6, full)** | 1000 | 4M | 5,160,860 | 1.29 | ~1.35 |
| **RB-VO (test 6)** | 1000 | 10M | 12,750,850 | 1.275 | ~1.36 |
| **LEDX (test 6)** | 1000 | 20M | 24,300,810 | 1.215 | ~1.455 |
| **AI-2 (test 6, full)** | 0 | 5M | 9,200,000 (only full) | 1.84 | 1.84 |
| **Army bandage (test 6, full)** | DELETED | 3M | 5,796,000 | 1.93 | ~2.03 |

## Patterns observed (empirical)

| `prices.json[tpl]` state | Effective multiplier k | Data points |
| --- | --- | --- |
| **`> 0` (any positive value)** | **k ≈ 1.5** with normal variance | RB-VO, LEDX, Salewa (full), Encrypted |
| **`= 0` (explicit zero)** | **k ≈ 2.0** | Bolts, Wires, AI-2 (full) |
| **Key MISSING from JSON** | **k ≈ 1.5** | GPU, Tetriz, Army bandage (full) — but Army bandage observed mean closer to 2.0 |

**Anomaly**: Intelligence folder (prices=1000) shows k≈2.12 — inconsistent with other prices>0 items. Likely contamination from `638e0752...` (another tpl with same name "Intelligence folder").

**Anomaly**: Army bandage (DELETED) shows k≈1.93-2.03 — inconsistent with GPU/Tetriz (also DELETED) which show k≈1.4. Possibly resource-item-specific behavior (Army bandage has charges, GPU/Tetriz don't).

## Hypotheses for the actual SPT formula

Para validar contra o código-fonte SPT:

```pseudo
# Hypothesis A: clean max with k=1.5
if (prices.json[tpl] exists AND > 0):
    base = max(prices.json[tpl], handbook.Items[tpl].Price * 1.5)
else if (prices.json[tpl] === 0):
    base = handbook.Items[tpl].Price * 2.0
else:  # missing key
    base = handbook.Items[tpl].Price * 1.5

# Hypothesis B: only prices.json drives, with handbook fallback
if (prices.json[tpl] exists AND > 0):
    base = prices.json[tpl]  # NOT max'd
else:
    base = handbook.Items[tpl].Price * 1.5  # fallback when missing or 0

# Then apply per-offer variance
offerPrice = base * random(~0.85, ~1.15)
```

Hipótese A explica os dados mas é estranha (por que k=2.0 quando prices=0?).
Hipótese B simpler mas falha em RB-VO test 4 (prices=100K, hb=10M, observed ~14M which suggests fallback to handbook×1.5=15M, not prices=100K).

A fórmula real do código-fonte SPT deve esclarecer.

---

## Conclusão (2026-05-19) — fórmula real do código fonte

Fonte: `references/spt-source/Libraries/SPTarkov.Server.Core/` (SPT 4.0.13, SHA `c87cc3c6...`).

### Fórmula validada

```text
Passo 1: P_mem = prices.json[tpl] (se key existe; senão indefinido)
Passo 2: bonus = handbook × (1.5 + 0.8 se tpl é craft do hideout, senão 0)
         se P_mem indefinido → P_mem = bonus
         senão              → P_mem = P_mem + bonus       ← SOMA (AddOrUpdate +=)
Passo 3: se ragfair.json:itemPriceOverrideRouble[tpl] existe → P_mem = override (atribui)
         ⚠️ CORRIGIDO 2026-06-07: ApplyFleaPriceOverrides roda ANTES de ReplaceFleaBasePrices,
         então o override entra como base e o bonus é SOMADO por cima. Ver §"Override" no fim.
Oferta:  P_mem × qualityModifier × random(0.8..1.2, bias 2,2)
```

### Refutação das hipóteses A e B

- **Hipótese A** estava parcialmente certa (k=2.0 em alguns casos, k=1.5 em outros) mas pelo motivo errado: atribuía a "prices=0 vs missing", quando a variável real era "é craft do hideout?".
- **Hipótese B** descartada — a fórmula real **soma** (não substitui) o bônus do handbook ao valor existente em `prices.json`.

### Insight central: o gotcha do `AddOrUpdate +=`

`DictionaryExtensions.AddOrUpdate` em `Libraries/SPTarkov.Server.Core/Extensions/DictionaryExtensions.cs:12-19` tem nome enganoso. Se a key existe faz `dict[key] += value`, não `dict[key] = value`. Por isso editar `prices.json[tpl]` diretamente NÃO substitui — soma em cima do bônus calculado a partir do handbook.

### Tabela: predição × observado (todos os 12 cenários batem)

`k_predição = (1.5 + 0.8 if craft) × 0.8..1.2`. Para items com `prices.json[tpl] != 0`, somar `prices/handbook` ao bonus.

| Item | prices | handbook | É craft? | Predição (RUB) | Observed min (RUB) | Resultado |
|---|---|---|---|---|---|---|
| GPU | ausente | 5M | não | 6M – 9M | 6.00M | ✓ |
| Tetriz | ausente | 8M | não | 9.6M – 14.4M | 10.68M | ✓ |
| Salewa (full) | 1000 | 4M | não | 4.8M – 7.2M | 5.16M | ✓ |
| RB-VO key | 1000 | 10M | não | 12M – 18M | 12.08M | ✓ |
| LEDX | 1000 | 20M | não | 24M – 36M | 24.30M | ✓ |
| Bolts | 0 | 5M | sim | 9.2M – 13.8M | 9.43M | ✓ |
| Wires | 0 | 8M | sim | 14.7M – 22M | 16.74M | ✓ |
| AI-2 (full) | 0 | 5M | sim | 9.2M – 13.8M | 9.20M | ✓ |
| Army bandage (full) | ausente | 3M | sim | 5.5M – 8.3M | 5.79M | ✓ |
| IFAK (full) | 1000 | 1M | sim | 1.84M – 2.76M | 1.95M | ✓ |
| Encrypted | 10M | 100K | sim | 8.18M – 12.28M | 8.39M | ✓ |
| Intel folder | 1000 | 6M | sim | 11M – 16.6M | 12.70M | ✓ |

12/12 dentro da faixa predita. Sem contaminação no Intel folder (a "anomalia" da análise inicial era o multiplier de craft, não tpl alternativo).

### Reinterpretação dos "padrões" observados

| Padrão antigo (hipótese A) | Causa real |
|---|---|
| "k≈2.0 quando prices=0" | Acaso: todos os items com `prices=0` testados (Bolts, Wires, AI-2) são craft. Multiplier 2.3, não 2.0. |
| "k≈1.5 quando key ausente" | Acaso: GPU/Tetriz (ausentes) são não-craft. Multiplier real é 1.5. |
| "Army bandage (ausente) com k≈2.0" | Army bandage é craft. Multiplier 2.3, igual aos outros craft items. |
| "Intel folder anomalia k≈2.12" | Intel folder é craft. Multiplier 2.3. Não havia contaminação. |

A semântica de `prices.json[tpl] = 0` vs ausente, na prática, é **irrelevante** para o flea: ambos os casos dão `0 + handbook × multiplier = handbook × multiplier` no passo 2.

### Caminho recomendado para editar preço de flea programaticamente

Escrever **`ragfair.json:dynamic.itemPriceOverrideRouble[tpl] = X − bonus`** (override **compensado**). Ver §"Override" abaixo para o porquê do `− bonus`. Reversível removendo a key. Não muda handbook in-game.

---

## Override — validação (2026-06-07, 7 cenários in-game)

A premissa "override sobrescreve" foi **falsificada**. A ordem real de boot é `ApplyFleaPriceOverrides` (assignment) **antes** de `ReplaceFleaBasePrices` (`AddOrUpdate +=`), então:

```text
base = (override ?? prices.json ?? 0) + bonus,   depois clamp(base, floor, ceiling)
  bonus   = handbook × M       (M incl. overrides tpl 1.8 / tipo 2.5, + craft 0.8)
  floor   = handbook × K_trader (≈ handbook; useTraderPriceForOffersIfHigher)
  ceiling = handbook × mult     (Weapon Mod ×6, Electronics ×11; senão ∞ — unreasonableModPrices)
```

| Cenário | Item | Evidência |
|---|---|---|
| **Aditivo** | Bolts ov=123456 | oferta exata 148.756 = 123456 + (11000×2.3) → bonus somado APÓS override |
| **Substitui prices.json** | Money case | ov + bonus = alvo; não somou o `prices.json`=1.4M (senão daria 3.4M) |
| **M de tipo** | Keycard Blue | M=2.5 confirmado (oferta mín > alvo×1.5×1.2 → impossível com 1.5) |
| **Piso** | LEDX (alvo < handbook) | pousou em ~handbook (trader buyback), não no alvo |
| **Teto** | GPU → 3.0M | capado em handbook 198000 × 11 = 2.178.000 (Electronics) |

**Compensação do viewer:** `override = X − bonus` → `base = X`, válido para `floor ≤ X ≤ ceiling`. Harness: [`scripts/smoke-matrix.js`](../scripts/smoke-matrix.js).

## Estado de revert

Após validar com o código-fonte, restaurar todos os items para os valores originais. Originais em [`.test-validation-state.json`](../.test-validation-state.json) e [`.revert-state.json`](../.revert-state.json):

| Item | Tpl | `prices.json` original | `handbook` original |
| --- | --- | --- | --- |
| Encrypted flash drive | `660bbc47c38b837877075e47` | 3999999 | 10000 |
| RB-VO marked key | `5d80c62a86f7744036212b3f` | 10141854 | 250000 |
| Salewa first aid kit | `544fb45d4bdc2dee738b4568` | 33019 | 18863 |
| IFAK | `590c678286f77426c9660122` | 17622 | 23764 |
| LEDX | `5c0530ee86f774697952d952` | 1224953 | 970000 |
| AI-2 medkit | `5755356824597772cb798962` | 5744 | 5200 |
| Army bandage | `5751a25924597722c463c472` | 3753 | 2049 |
| Graphics card | `57347ca924597744596b4e71` | 640964 | 198000 |
| Bolts | `57347c5b245977448d35f6e1` | 49090 | 11000 |
| Tetriz | `5c12620d86f7743f8b198b72` | 137604 | 120000 |
| Bundle of wires | `5c06779c86f77426e00dd782` | 32545 | 7400 |
| Intelligence folder | `5c12613b86f7743bbe2c3f76` | 299806 | 69000 |

Backups dos arquivos inteiros (anteriores ao test 1):

- `D:/SPT/SPT/SPT_Data/database/templates/prices.json.test-backup` (126.154 bytes)
- `D:/SPT/SPT/SPT_Data/database/templates/handbook.json.test-backup` (518.762 bytes)
- `D:/SPT/SPT/SPT_Data/checks.dat.test-backup` (46.553 bytes)
