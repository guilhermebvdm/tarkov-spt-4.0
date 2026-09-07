# Roadmap — TRL-WeatherSync

> **Ecossistema de Clima Dinâmico, Sincronização Contínua Multiplayer, Ciclo Natural de Estações e Otimização Extrema para Tarkov SPT 4.0 / FIKA.**

---

## 1. Visão Geral & Diagnóstico da Necessidade

No SPT Tarkov e especialmente em partidas cooperativas (FIKA), o ecossistema climático atual sofre de limitações críticas:
1. **Desync em Partidas Multiplayer:** O FIKA realiza a troca das curvas climáticas apenas uma única vez na tela de carregamento (`GenerateWeathers`). Como o Tarkov calcula variações dinâmicas de chuva (`RainRandomness`), vento e nuvens localmente via geradores de ruído da Unity, após alguns minutos de raid o clima diverge completamente (ex.: sol para o Host e tempestade torrencial para o cliente).
2. **Estações Estáticas ou Fragmentadas:** A alternância de estações do ano é frequentemente manual, incompleta ou desincronizada entre os pares.
3. **Quedas Bruscas de FPS na Neve e Chuva:** Em computadores de entrada ou intermediários, o inverno e chuvas fortes causam perda severa de quadros por segundo devido ao excesso de partículas (até 65.532 flocos no `SnowFlakes.cs`), passes extras de CommandBuffer HDR em 10 bits e dupla renderização em miras óticas (*scopes*).

O **TRL-WeatherSync** assume a autoridade climática total da partida, estabelecendo sincronização contínua de alta performance, gerindo o ciclo sazonal com suas sub-fases nativas e introduzindo otimizações profundas de renderização.

---

## 2. Ciclo Natural e Mapeamento de Fases das Estações no EFT

O motor do Escape From Tarkov possui suporte nativo a **sub-fases específicas** em cada estação (`ESeason`, `ESeasonStatus`, `ERainControllerStatus` e `Class444` / `SeasonsController`). 

A ordem cronológica astronômica implementada pelo mod conecta todas as fases e transições naturais:

```
┌─────────────────────────────────────────────────────────────────────────────────────────────┐
│                                                                                             │
▼                                                                                             │
🌱 Primavera Inicial ──▶ 🌱 Primavera Plena ──▶ ☀️ Verão ──▶ 🍂 Outono Dourado ──▶ 🍂 Outono Tardio ──▶ ❄️ Inverno ──┘
   (SpringEarly)            (Spring)             (Summer)       (Autumn)             (AutumnLate)        (Winter)
```

### 2.1. Detalhamento de Cada Fase e Sub-Tipo

#### A. 🌱 Primavera (Spring) — 2 Fases Nativas:
1. **🌱 Primavera Inicial / Degelo (`ESeason.SpringEarly` / `Class448`):**
   * *Mecânica exclusiva:* O EFT ativa o vetor de degelo **`SpringSnowFactor`** ([`Class448: public Class448(Class444 controller, Vector3 springSnowFactor)`]).
   * *Visual:* Restos de neve derretendo em encostas e montanhas, muita lama e solo encharcado, acúmulo de poças de água (`LyingWater`), árvores secas começando a brotar e névoa densa de evaporação.
2. **🌱 Primavera Plena (`ESeason.Spring` / `Class447`):**
   * *Visual:* Desaparecimento total da neve residual, grama verde jovem brotando, flores silvestres, árvores florescendo, clima ameno e chuvas limpas de primavera.

#### B. ☀️ Verão (Summer) — 2 Modos:
1. **☀️ Verão Pleno (`ESeason.Summer` / `Class446`):**
   * *Visual:* Folhagem densa, árvores carregadas de folhas verdes, iluminação solar brilhante (`TOD_Sky`), noites curtas e alta visibilidade geral.
2. **⛈️ Tempestade de Verão (`ESeasonStatus.Storm` / `Class451`):**
   * *Visual:* Pancadas de chuva torrencial repentinas com trovoadas intensas, ventos fortes e retorno rápido ao sol limpo.

#### C. 🍂 Outono (Autumn) — 2 Fases Nativas:
1. **🍂 Outono Dourado (`ESeason.Autumn` / `Class449`):**
   * *Visual:* Folhagem avermelhada e amarelada, folhas caídas cobrindo as trilhas e estradas, iluminação dourada e céu encoberto por nuvens cinzentas (`Overcast`).
   * *Clima:* Chuvas contínuas e vento fresco constante.
2. **🍂 Outono Tardio / Pré-Gelo (`ESeason.AutumnLate` / `Class450`):**
   * *Visual:* Árvores completamente despidas de folhas (galhos secos e retorcidos), vegetação rasteira morta em tons marrons/cinzentos, poças de lama endurecendo com o frio, prenúncio de geada e ar gélido.

#### D. ❄️ Inverno (Winter) — 2 Fases / Modos:
1. **❄️ Inverno Nevado (`ESeason.Winter` / `Class453`):**
   * *Visual:* Cobertura espessa de neve branca em todo o terreno e vegetação, lagos/poças com superfícies congeladas, iluminação fria e vapor saindo da respiração do personagem.
2. **🌨️ Nevasca / Tempestade de Neve (`ERainControllerStatus.WinterStorm` / `Class456`):**
   * *Visual:* Ventania uivante com neve horizontal em alta velocidade (`StormSideSpeed`), visibilidade drasticamente reduzida (efeito *Whiteout*), forçando combate a curta distância e uso de equipamentos térmicos.

---

## 3. Controle de Período das Estações via Servidor (`server/config.json`)

Para atender ao objetivo de **1 semana real por estação** (ou por sub-fase), a lógica do servidor não se apoia no relógio in-game (`GameDateTime.TimeFactor`, que corre ~7x mais rápido). Em vez disso, utiliza **timestamps reais (UTC)** imunes a distorções de tempo de jogo.

### 3.1. Padrão Oficial Adotado: Opção A (1 Semana Real por Estação Macro)
* **Duração Total do Ciclo Completo:** 4 semanas reais (28 dias corridos / 1 mês real para o ano inteiro do Tarkov).
* **Distribuição Semanal e Sub-Fases:**
  * **Semana 1 (Dias 1 a 7) — 🌱 Primavera:**
    * *Dias 1 a 3,5:* **Primavera Inicial (`SpringEarly`)** com degelo progressivo (`SpringSnowFactor`), muita lama e poças d'água.
    * *Dias 3,5 a 7:* **Primavera Plena (`Spring`)** com brotos verdes, flores silvestres e ar fresco.
  * **Semana 2 (Dias 8 a 14) — ☀️ Verão:**
    * *Dias 8 a 14:* **Verão Pleno (`Summer`)** com folhagem densa, sol brilhante e tempestades de verão repentinas (`Storm`).
  * **Semana 3 (Dias 15 a 21) — 🍂 Outono:**
    * *Dias 15 a 18,5:* **Outono Dourado (`Autumn`)** com folhas alaranjadas no chão, ventania e céu cinzento.
    * *Dias 18,5 a 21:* **Outono Tardio (`AutumnLate`)** com árvores despidas (galhos secos), solo gélido e prenúncio de congelamento.
  * **Semana 4 (Dias 22 a 28) — ❄️ Inverno:**
    * *Dias 22 a 28:* **Inverno Nevado (`Winter`)** com manto branco completo e nevascas periódicas intensas (`WinterStorm`).

### 3.2. Estrutura do `server/config.json`:
```json
{
  "seasonalCycle": {
    "enabled": true,
    "durationMode": "RealTimeDays",
    "daysPerMacroSeason": 7,
    "splitSubPhases": true,
    "subPhaseIntervalDays": 3.5,
    "cycleOrder": [
      "SpringEarly",
      "Spring",
      "Summer",
      "Autumn",
      "AutumnLate",
      "Winter"
    ],
    "referenceEpochUtc": 1756771200,
    "allowManualOverride": false,
    "manualSeason": "Winter"
  }
}
```

### 3.3. Mecânica de Cálculo Cirúrgica:
$$\text{tempoDecorridoSegundos} = \text{DataAtualUtc} - \text{DataReferenciaUtc}$$
$$\text{segundosPorEstaçãoMacro} = \text{daysPerMacroSeason} \times 86.400 = 604.800 \text{ s}$$
$$\text{semanaAtual} = \left\lfloor \frac{\text{tempoDecorridoSegundos}}{\text{segundosPorEstaçãoMacro}} \right\rfloor \pmod 4$$

* **Exatidão Absoluta:** O ciclo dura rigorosamente 7 dias reais (168 horas corridas) por estação macro, totalizando 28 dias para o ciclo anual completo.
* **Sincronização 100% Determinística:** Qualquer jogador ou servidor que consultar o endpoint `/weather/season` receberá exatamente o mesmo estado sazonal e a mesma sub-fase.

---

## 4. Otimização Extrema de Desempenho (Chuva e Neve)

### 4.1. Como o EFT Renderiza Chuva e Neve:
* **Fato Comprovado no Código:** Em [`RainFollow.cs:L21`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/RainFollow.cs#L21):
  `transform_0.position = _target.position + new Vector3(0f, _offset, 0f);`
  A chuva e a neve **NÃO caem no mapa inteiro**. As partículas são geradas em uma pequena caixa presa à cabeça/câmera do jogador a apenas 10m de altura.

### 4.2. Os 4 Vilões do FPS Identificados no EFT:
1. **Excesso de Partículas de Neve (`SnowFlakes.cs`):** O EFT instancia 4 malhas simultâneas totalizando **até 65.532 partículas de neve**, além de despachar buffers para a GPU via `ComputeBuffer.SetData()` todo frame ([`SnowFlakes.cs:L190`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/SnowFlakes.cs#L190)).
2. **Passes Extras em HDR 10-bit (`SnowWetRenderer.cs`):** CommandBuffers de tela cheia em formato `ARGB2101010` dedicados ao brilho de cristais de neve (`SNOW_GLITTERS`) ([`SnowWetRenderer.cs:L350`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/SnowWetRenderer.cs#L350)).
3. **Renderização Dupla com Miras Óticas (`SnowWetRenderer.cs:L322`):** Mirar com luneta ativa a `OpticCameraManager.Camera`, desenhando o pipeline pesado de neve **duas vezes no mesmo frame** e derrubando o FPS pela metade.
4. **Impactos de Chuva no Solo (`RainSplashController`):** Criação massiva de quads e partículas de splash no chão molhado.

### 4.3. Soluções e Modos de Performance do TRL-WeatherSync:

| Otimização | Mecânica Técnica | Ganho de Performance |
|---|---|:---:|
| **Partículas de Neve Otimizadas** | Reduzir a malha do `SnowFlakes` de 65.532 para **4.096 ou 8.192 partículas**. Quase imperceptível aos olhos, mas reduz a carga de desenho em até 87%. | 🟢 **+15 a 25 FPS** na neve |
| **Culling de Neve na Mira Ótica** | Desativar o passe de neve na `OpticCameraManager.Camera` quando mirando com lunetas (opção *Optic Snow Culling*). | 🟢 **Elimina a queda de FPS ao mirar** |
| **Desativação de Snow Glitters** | Omitir o CommandBuffer `SNOW_GLITTERS` em PCs de entrada, liberando largura de banda de VRAM. | 🟢 Redução de uso de memória de vídeo |
| **Throttling de Splashes de Chuva** | Limitar o teto máximo de emissão de partículas de impacto no chão pelo `RainSplashController`. | 🟢 Estabilidade de quadros na chuva forte |

---

## 5. Motor de Sincronização Contínua em Raid (Real-Time Weather Sync)

### 5.1. Arquitetura Host-Authoritative
* O Host ou Servidor Headless emite a cada **10 a 15 segundos** um pacote compacto (`TrlWeatherSyncPacket` < 32 bytes):
  * `RainIntensity` (0.0 a 1.0)
  * `Cloudness` (0.0 a 1.0)
  * `WindSpeed` & `WindDirection` (Vector2 compacto)
  * `FogDensity` & `FogHeight`
  * `Temperature` & `AtmosphericPressure`
  * `ThunderEventTrigger` (disparo síncrono de trovão/relâmpago)

### 5.2. Interpolação Suave nos Clientes (Anti-Snap)
* O cliente aplica amortecimento linear (`Mathf.Lerp` em 2-3s) nos parâmetros recebidos, garantindo que o início ou término de chuvas e variações de neblina ocorram de maneira imperceptível e natural.

---

## 6. Correção do Handshake Pré-Raid (Loading Screen)

* **Saneamento de `TimeAndWeatherSettings`:** Repasse integral de `CloudinessType`, `RainType`, `WindType` e `FogType` do Host para o `CoopGame.Create` do cliente antes da inicialização do `LocalGame`.
* **Proteção contra Condição de Corrida de `WeatherReady`:** O cliente só destrava o carregamento quando o array completo de `WeatherClasses` estiver confirmado e aplicado em `WeatherController.Instance.method_0()`.

---

## 7. Integração com IA (SAIN & Bots Nacionais)

* **Visibilidade Dinâmica de Bots:** Redução de alcance e acurácia de visão no SAIN (`EnemyGainSightClass` e `EnemyVisionDistanceClass`) proporcional à densidade da chuva e da neblina.
* **Máscara Sonora de Tempestades:** O som da chuva torrencial abafa passos de jogadores nos ouvidos dos bots (`HearingInputClass`).

---

## 8. Configuração & Painel BepInEx (F12)

| Seção | Opção | Valores / Padrão | Descrição |
|---|---|:---:|---|
| **1. Seasons** | `CycleMode` | RealTime / InGame / Fixed | Modo de controle temporal das estações. |
| **1. Seasons** | `CurrentSeason` | Enum de 6 fases | Seleção da estação atual. |
| **1. Seasons** | `DaysPerSeason` | 7 (1 a 30) | Duração de cada fase em dias reais. |
| **2. Performance** | `SnowParticleQuality` | Ultra (65k) / Med (8k) / Low (4k) | Quantidade de flocos de neve gerados. |
| **2. Performance** | `DisableOpticSnow` | True / False (Padrão: True) | Não desenha neve dentro de miras óticas. |
| **2. Performance** | `DisableSnowGlitters` | True / False | Desativa passe de cristais HDR 10-bit. |
| **2. Performance** | `RainSplashLimit` | 25% / 50% / 100% | Limite de partículas de impacto de chuva. |
| **3. Networking** | `SyncIntervalSeconds` | 10s (5s a 30s) | Intervalo do pacote de sincronismo de clima. |
| **3. Networking** | `EnableThunderSync` | True / False | Trovões disparados simultaneamente na raid. |
| **4. AI Integration** | `WeatherAffectsAI` | True / False | Chuva e neblina afetam visão e audição da IA. |
