# Investigação — Handshake de Clima do FIKA & API Real de Clima/Estações do EFT

**Data:** 2026-09-10 · **Sessão:** 2 · **Tipo:** Pesquisa de leitura (sem alteração de código)

> Objetivo: substituir as suposições do `ROADMAP.md` por citações reais de código (arquivo:linha) para as seções que ainda não tinham nenhuma — a arquitetura de rede do FIKA (§5/§6 do Roadmap) — e validar/corrigir as seções que já citavam código do EFT decompilado (§2 e §4).

---

## A. Handshake de clima no FIKA (client-side plugin)

### A.1. Onde o clima é gerado e aplicado hoje

Toda a lógica vive em `mods\FIKA\modded\Fika-Plugin\Fika.Core\Main\GameMode\`:

- **`BaseGameController.cs:85`** — `public WeatherClass[] WeatherClasses { get; set; }` (propriedade compartilhada entre Host/Client).
- **`BaseGameController.cs:350`** — `public abstract Task GenerateWeathers();` (contrato abstrato, cada lado implementa o seu).
- **`HostGameController.cs:530-545`** — `GenerateWeathers()` do Host: chama `_backendSession.WeatherRequest()` (pede o clima ao backend SPT local do Host), guarda `Season`/`SeasonsSettings`, e se `!UseCustomWeather`, aplica via `WeatherController.Instance.method_0(WeatherClasses)` (linha 542).
- **`HostGameController.cs:364-385`** — `SetupCustomWeather(TimeAndWeatherSettings timeAndWeather)`: monta 2 `WeatherClass` (início e +1 dia) a partir de `CloudinessType`/`RainType`/`WindType`/`FogType.ToValue()`, e aplica com o mesmo `WeatherController.Instance.method_0(weatherClasses)` (linha 384).
- **`HostGameController.cs:387-400`** — `StartBotSystemsAndCountdown`: no mapa `laboratory` pula geração de clima (`Season = ESeason.Summer` fixo); nos demais, chama `await GenerateWeathers()` **uma única vez**, antes do spawn dos bots.
- **`ClientGameController.cs:132-141`** — `GenerateWeathers()` do Client: chama `GetWeather()` e então `WeatherController.Instance.method_0(WeatherClasses)` (linha 139).
- **`ClientGameController.cs:147-165`** — `GetWeather()`: monta um `RequestPacket { Type = ERequestSubPacketType.Weather }`, envia ao Host (`client.SendData`), e faz **polling a cada 1000ms** (`while (!WeatherReady)`) reenviando o pedido até a resposta chegar.

**Conclusão A.1:** confirma exatamente o diagnóstico do Roadmap §1 — o clima é resolvido **uma única vez**, num handshake requisição/resposta (`RequestPacket` tipo `Weather`) disparado durante o loading screen, antes do raid começar. Não existe nenhum reenvio periódico depois disso — o gargalo é real e está localizado.

### A.2. Método `method_0` do `WeatherController` (citado no Roadmap sem contexto)

`WeatherController.method_0(WeatherClass[] nodes)` (ver seção C) é o ponto de entrada único, tanto do Host quanto do Client, para aplicar uma nova curva de clima — é o alvo natural para o TRL-WeatherSync reaplicar clima a qualquer momento da raid, não só no handshake inicial.

---

## B. Arquitetura de pacotes de rede do FIKA

### B.1. Padrão "Request/Response" (usado hoje para clima — 1x por raid)

`mods\FIKA\modded\Fika-Plugin\Fika.Core\Networking\Packets\World\RequestPacket.cs` — envelope genérico: campo `Type` (`ERequestSubPacketType`), payload polimórfico `IRequestPacket RequestSubPacket`. Serializa o tipo + delega ao subpacote (linhas 15-61).

`ERequestSubPacketType.cs` — enum simples: `SpawnPoint, Weather, Exfiltration, TraderServices, CharacterSync`.

`RequestSubPackets.cs:90-164` — classe `WeatherRequest : IRequestPacket`:
- Campos: `ESeason Season`, `Vector3 SpringSnowFactor`, `WeatherClass[] WeatherClasses`.
- `HandleRequest(NetPeer peer, FikaServer server)` (linha 113) roda **no Host**: responde com o `WeatherClasses` atual do `GameController` + `Season`/`SpringSnowFactor` via `server.SendDataToPeer(..., ReliableOrdered, peer)`.
- `HandleResponse()` (linha 133) roda **no Client**: aplica `Season`, `SeasonsSettings.SpringSnowFactor` e `WeatherClasses` recebidos.

Esse é o padrão pull (cliente pede, host responde) — adequado para handshake, mas **não é o modelo certo para sync contínuo**, que precisa ser push periódico do Host.

### B.2. Padrão "broadcast periódico" — o modelo certo para o TrlWeatherSyncPacket

Achado central: o próprio FIKA já implementa exatamente o padrão que o TRL-WeatherSync precisa, só que para estatísticas de FPS do servidor:

- **`FikaServer.cs:138`** — `private float _sendThreshold;` e **`FikaServer.cs:170`** — `_sendThreshold = 2f;` (2 segundos).
- **`FikaServer.cs:546-570`** — `Update()` (MonoBehaviour, roda todo frame): acumula `_statisticsCounter += Time.unscaledDeltaTime`; quando ultrapassa `_sendThreshold`, zera o contador e chama `SendStatisticsPacket()`.
- **`FikaServer.cs:572-583`** — `SendStatisticsPacket()`: monta `new StatisticsPacket(fps)` e envia com **`SendData(ref packet, DeliveryMethod.Unreliable)`** — sem parâmetro de peer, ou seja, broadcast para todos os clients conectados.
- **`Packets\Backend\StatisticsPacket.cs`** (arquivo inteiro, 16 linhas) — `struct StatisticsPacket(int serverFps) : INetSerializable`, com `Serialize`/`Deserialize` de um único `int`. Exemplo mínimo de payload compacto.
- **`FikaClient.Callbacks.cs:749-752`** — `OnStatisticsPacketReceived(StatisticsPacket packet)`: só atribui `ServerFPS = packet.ServerFPS`. Nenhuma interpolação — o `TrlWeatherSyncPacket` precisará da sua própria lógica de `Lerp` no handler (Roadmap §5.2 já prevê isso, mas hoje não existe nenhum handler do FIKA fazendo interpolação suave — teremos que implementar do zero).

**Modelo recomendado para `TrlWeatherSyncPacket`:** replicar literalmente o par `FikaServer.Update()` (accumulator) + `struct : INetSerializable` + `SendData(..., DeliveryMethod.Unreliable)` broadcast, trocando `_sendThreshold = 2f` por um valor configurável (10-15s conforme Roadmap §5.1).

### B.3. Como registrar um pacote novo (ponto de extensão real)

- **`FikaClient.cs:239-264`** — método privado `RegisterPacketsAndTypes()`: lista **hardcoded** de `RegisterPacket<T>(handler)` para cada tipo conhecido, incluindo `RegisterPacket<StatisticsPacket>(OnStatisticsPacketReceived)` (linha 256).
- **`FikaClient.cs:970-1000`** / **`FikaServer.cs:582-607`** — `_packetProcessor.SubscribeNetSerializable(handle)` é o mecanismo por trás de `RegisterPacket` (via `NetPacketProcessor` do LiteNetLib, `LiteNetLib\Utils\NetPacketProcessor.cs`).

**Implicação prática:** como `RegisterPacketsAndTypes()` é privado e a lista é fixa em tempo de compilação do FIKA, o TRL-WeatherSync (mod externo, BepInEx separado) **não pode simplesmente adicionar uma linha** nesse método. Duas rotas viáveis:
1. **Harmony postfix** em `FikaClient.RegisterPacketsAndTypes` / equivalente do `FikaServer` para chamar `RegisterPacket<TrlWeatherSyncPacket>(...)` via reflection (o método genérico `RegisterPacket<T>` provavelmente é `private`/`protected` — precisa checar modificador de acesso antes de commitar a essa rota).
2. **Processor próprio**: o TRL-WeatherSync mantém seu próprio `NetPacketProcessor`/canal LiteNetLib, escutando na mesma conexão via um listener adicional, sem depender do registro interno do FIKA.

Isso ainda não está resolvido — é a pendência técnica mais crítica antes de codar (ver seção "Pendências" abaixo). Precisa de mais uma rodada de leitura em `FikaClient.cs`/`FikaServer.cs` focada especificamente no modificador de acesso de `RegisterPacket<T>` e em como `_netServer`/`_netClient` (LiteNetLib `NetManager`) são expostos (public? singleton?).

### B.4. Base de transporte

Tudo roda sobre uma implementação vendorizada do **LiteNetLib** (`Networking\LiteNetLib\*`, ~30 arquivos, é o LiteNetLib real copiado para dentro do projeto, não um pacote externo) com uma camada de compressão **LZ4** própria (`Networking\LZ4\*`). `DeliveryMethod.ReliableOrdered` é usado para handshake/estado importante; `DeliveryMethod.Unreliable` para dados tolerantes a perda e de alta frequência (estatísticas) — o `TrlWeatherSyncPacket`, por ser reenviado a cada poucos segundos e tolerante a perda pontual (o próximo pacote corrige), deve usar `Unreliable` também.

---

## C. API real de clima e estações do EFT (decompilado)

### C.1. `EFT.Weather.WeatherController` (`references\eft-decompiled\Assembly-CSharp\EFT.Weather\WeatherController.cs`)

- Singleton clássico: `public static WeatherController Instance;`, setado em `Awake()` (linha 129), limpo em `OnDestroy()` (linha 158).
- **`method_0(WeatherClass[] nodes)`** (linha 112) — **é o método real que aplica uma curva de clima**: `iweatherCurve_0 = new WeatherCurve(nodes); WeatherDebug.CopyParams(...)`. Confirmado como o ponto de entrada usado pelo FIKA em `HostGameController.cs:384/542` e `ClientGameController.cs:139` (ver seção A).
- **`SetWeatherForce(WeatherClass end)`** (linha 120) — alternativa que interpola da curva atual (`iweatherCurve_0`) até `end`, via `new WeatherCurve(iweatherCurve_0, end)`. **Candidato mais interessante que `method_0` para o TRL-WeatherSync**: `method_0` troca a curva inteira (pode gerar "salto" visual), enquanto `SetWeatherForce` já é uma transição host-controlada nativa do próprio EFT — vale investigar `WeatherCurve` (`new WeatherCurve(iweatherCurve_0, end)`) na próxima sessão para saber se ele já faz o Lerp que o Roadmap §5.2 planeja reimplementar manualmente.
- `WeatherCurve` (propriedade, linha 86) — retorna `WeatherDebug` se `WeatherDebug.Enabled`, senão `iweatherCurve_0`. Ou seja, existe um modo debug (`Debug_SetTestWeather`, `Debug_LoadDebugWeatherPreset`) que sobrepõe qualquer clima sincronizado — relevante para o painel BepInEx (Roadmap §8): se o usuário tiver o weather debug do EFT ativo, ele vai brigar com o TRL-WeatherSync.
- `LateUpdate()` (linha 173) chama `TimeOfDayController.Update()` e `method_4()` — `method_4()` é o loop que recalcula fog/nuvens/vento a partir de `WeatherCurve` a cada frame (linha 217-247). Isso confirma que **aplicar `method_0`/`SetWeatherForce` já é suficiente** — não é preciso mexer em `TimeOfDayController` ou nos `method_5..14` internos, eles só consomem `WeatherCurve`.
- Não encontrado: nenhum evento público de "clima mudou" (ex.: `WeatherChangedEvent`) na classe — só o `SnowLevelOnTerrainChangedEvent` (delega para `RainController.WettingChangedEvent`) e `StatusChangedEvent` (ver `Class444` abaixo). Se o SAIN (Roadmap §7) precisar reagir a clima, provavelmente vai ler `WeatherCurve.Rain`/`.Fog` diretamente, não por evento.

### C.2. `Class444` — controlador de estações (rótulo comunitário "SeasonsController", **não confirmado oficialmente** — já assim marcado no cabeçalho do arquivo)

**Correção importante ao Roadmap §2 e §8:** os enums reais são mais simples do que o documentado.

`references\eft-decompiled\Assembly-CSharp\ESeason.cs`:
```csharp
public enum ESeason : byte { Summer, Autumn, Winter, Spring, AutumnLate, SpringEarly }
```
`references\eft-decompiled\Assembly-CSharp\ESeasonStatus.cs`:
```csharp
public enum ESeasonStatus : byte { Summer, Autumn, Winter, Spring, AutumnLate, SpringEarly, Storm }
```

- **`ERainControllerStatus` NÃO EXISTE** no assembly decompilado (busca por arquivo e por texto não encontrou nada). O Roadmap §2.D.2 cita `ERainControllerStatus.WinterStorm` — isso não tem base no código atual. **Remover essa citação do Roadmap.**
- **`ESeason` não tem valor `Storm`** — logo "Tempestade de Verão" não é uma macro-estação selecionável junto com `SpringEarly/Spring/Summer/Autumn/AutumnLate/Winter` no `cycleOrder` do `server/config.json` (Roadmap §3.2). `Storm` só existe em `ESeasonStatus`, e é alcançado por **transição de runtime**, não por seleção direta.
- **Confirmado em `Class444.cs:691-756`**: o fluxo normal (`method_0(ESeason season, ...)`) só aceita os 6 valores de `ESeason` e instancia a classe de estado correspondente:
  - `Summer → Class446` ✓ (bate com o Roadmap)
  - `Autumn → Class449` ✓
  - `Winter → Class453` ✓
  - `Spring → Class447` ✓
  - `AutumnLate → Class450` ✓
  - `SpringEarly → Class448(controller, seasonsSettings.SpringSnowFactor)` ✓ (construtor bate exatamente com a citação do Roadmap §2.A.1)
- **`Storm` (`Class451`/`Class452`) não é alcançado por `method_0`.** É disparado por um **evento global vindo do servidor**: `Class444.cs:764-768` — `method_1(StormStartedEvent)` é inscrito em `Class444.cs:699` via `GlobalEventHandlerClass.Instance.SubscribeOnEvent<StormStartedEvent>(method_1)`, e delega para `Interface3_0.StormStarted(visual)` do estado atual (ex.: `Class446.StormStarted`, linha 151-159), que troca para `Class451` (Storm). `Class452` é uma variante usada apenas em `HandleReconnect` (reconexão em meio a uma tempestade já em curso, linha 218-222).
- **`StormStartedEvent`** (`references\eft-decompiled\Assembly-CSharp\EFT.GlobalEvents\StormStartedEvent.cs`) herda de **`SyncEventFromServer`**, payload vazio (`Serialize`/`Deserialize` sem corpo). Isso é uma peça-chave para o motivo da sincronização contínua ser necessária: em singleplayer, esse evento vem do backend SPT local de cada instalação. **Em FIKA, cada cliente roda sua própria sessão de backend SPT** — se cada cliente recebe/gera seu próprio `StormStartedEvent` de forma independente, é exatamente aí que a tempestade em um lado e sol no outro (citado no Roadmap §1.1) acontece. Ainda não localizei o ponto exato que dispara `StormStartedEvent` no lado do servidor SPT — fica como pendência para a próxima sessão (provavelmente em `references` do lado server, não do client decompilado).
- **`Class456` ("StateFactoryWinter") NÃO é uma "Nevasca/Tempestade de Neve" genérica** como o Roadmap §2.D.2 descreve. Evidência (`Class444.cs:595-618` e `:715-724`): `Class456` só é instanciado quando `WeatherController.Instance == null` **e** `season == ESeason.Winter` **e** `FactoryWinterController.Instance != null` — ou seja, é um estado de fallback específico do mapa **Factory** (chama `FactoryWinterController.Instance.method_0()`), usado antes do `WeatherController` estar pronto, não uma sub-fase de nevasca aplicável a todos os mapas. `Class454` ("StateUnknown") e `Class455` ("StateFactoryDefault") são fallbacks semelhantes para quando não há `FactoryWinterController`. **Não existe, no código lido até agora, nenhum estado equivalente a "nevasca/whiteout" universal** — o Roadmap precisa remover essa alegação ou marcá-la como não verificada.
- API pública útil de `Class444` confirmada: `Season` (linha 632), `Status` (linha 634), `SnowLevelOnTerrain` (linha 636, delega a `RainController.Wetting`), evento `StatusChangedEvent` (linha 638-666, dispara em toda troca de estado — **esse sim é um bom gancho de evento** para o TRL-WeatherSync notificar mudança de estação), `Run(ESeason, SeasonsSettingsClass)` (ponto de entrada assíncrono, linha 691) e `HandleReconnect(ESeasonStatus, SeasonsSettingsClass)` (linha 758, usado quando um jogador reconecta em meio a uma raid — relevante para o FIKA lidar com reconexão de cliente).

### C.3. `TimeAndWeatherSettings` (`references\eft-decompiled\Assembly-CSharp\EFT\TimeAndWeatherSettings.cs`)

Confirmado struct real, exatamente como o Roadmap §6 assume:
```csharp
public struct TimeAndWeatherSettings(bool randomTime, bool randomWeather, int cloudinessType, int rainType, int windSpeed, int fogType, int timeFlowType, int hourOfDay = -1)
{
    public bool IsRandomTime; public bool IsRandomWeather;
    public ECloudinessType CloudinessType; public ERainType RainType;
    public EWindSpeed WindType; public EFogType FogType;
    public ETimeFlowType TimeFlowType; public int HourOfDay;
}
```
Campos extras não citados no Roadmap: `IsRandomTime`, `IsRandomWeather`, `TimeFlowType`, `HourOfDay` — relevantes se o TRL-WeatherSync quiser assumir controle total (§1, "autoridade climática total"), já que hoje o FIKA já usa esse struct como ponto de entrada em `CoopGame.cs:86` (assinatura do método de criação da raid recebe `TimeAndWeatherSettings timeAndWeather` como parâmetro).

---

## D. Sanity check das citações já existentes no Roadmap (§4)

Todas conferem, com uma citação ainda mais precisa encontrada:

- **`RainFollow.cs:21`** — confere exatamente: `transform_0.position = _target.position + new Vector3(0f, _offset, 0f);`, com `_offset = 10f` (linha 10, serializado no Inspector).
- **`SnowFlakes.cs:190`** — confere: `_lightsPropertiesComputeBuffer.SetData(list_0);`.
- **Bônus não citado pelo Roadmap:** `SnowFlakes.cs:24` — `private const int int_1 = 16383;` — 4 malhas (`material_0..3`, ver `method_2()` linhas 192-195) × 16.383 ≈ **65.532**, batendo exatamente com o número "65.532 partículas" do Roadmap §4.2.1. Citação mais forte para usar no lugar da aproximação atual.
- **`SnowWetRenderer.cs:322`** — confere: `if (currentCamera == instance.Camera || currentCamera == instance.OpticCameraManager.Camera)` dentro de `method_3(Camera)`, chamado 2x por frame (uma vez por câmera) — é literalmente a duplicação de renderização citada.
- **`SnowWetRenderer.cs:350`** — confere: `buffer.GetTemporaryRT(int_16, -1, -1, 0, FilterMode.Point, RenderTextureFormat.ARGB2101010);`.

---

## E. Resolução das pendências P-1.2.1 a P-1.2.3 (mesma sessão, continuação)

### E.1. [P-1.2.1] Como registrar o `TrlWeatherSyncPacket` sem alterar o FIKA — RESOLVIDO

**`RegisterPacket<T>` é público**, tanto em `FikaClient.cs:580` quanto em `FikaServer.cs:968`:
```csharp
public void RegisterPacket<T>(Action<T> handle) where T : INetSerializable, new()
```
E mais importante: ele faz parte do **contrato público `IFikaNetworkManager`** (`Networking\IFikaNetworkManager.cs:129`), junto com `SendData<T>`, `SendDataToPeer<T>`, `SendGenericPacket`, etc. — ou seja, é uma **API de extensão oficialmente exposta pelo FIKA**, não um detalhe interno. `NetClient`/`NetServer` (o `NetManager` do LiteNetLib por trás) também são propriedades públicas (`FikaClient.cs:57`).

**Decisão de arquitetura recomendada: nenhum Harmony patch nos internals do FIKA, nenhuma reflection, nenhum fork.** O TRL-WeatherSync só precisa:
1. Detectar quando `Singleton<FikaClient>.Instantiated` (no client) ou `Singleton<FikaServer>.Instantiated` (no host) fica verdadeiro — um simples polling/coroutine no `Awake()` do próprio plugin BepInEx do TRL-WeatherSync, sem tocar em nada do FIKA.
2. Chamar `Singleton<FikaClient>.Instance.RegisterPacket<TrlWeatherSyncPacket>(OnPacketReceived)` (client) e o equivalente em `FikaServer` (host), usando a API pública normalmente, do jeito que qualquer mod de terceiro consome uma API de outro mod.
3. Para o host emitir o broadcast periódico, replicar o padrão de `FikaServer.cs` (`_sendThreshold`/`Update()`/`SendData(..., DeliveryMethod.Unreliable)`, ver B.2) **dentro do código do próprio TRL-WeatherSync** (um `MonoBehaviour` novo do mod, não uma edição do `FikaServer.cs`).

Isso satisfaz o requisito de publicação: o mod funciona sobre um FIKA stock, sem exigir uma build customizada do FIKA nem patches invasivos nos métodos internos dele — só consome a interface pública que o próprio FIKA disponibiliza para mods de terceiros.

### E.2. [P-1.2.2] "Tempestade" é clima, Halloween e "congelamento" são sistemas separados — RESOLVIDO

Investigação por três frentes, todas confirmadas como **sistemas independentes entre si**:

- **`ESeasonStatus.Storm` (`Class451`/`Class452`) é clima de verdade** — chuva/vento/trovão intensificados, dentro do próprio `WeatherController`/`Class444` (ver C.2). Nada a ver com evento sazonal de calendário (Halloween/Natal).
- **Halloween é um sistema totalmente à parte**, com sua própria família de classes: `HalloweenEventControllerClass.cs`, `BotHalloweenEvent.cs`, `BotHalloweenWithZombies.cs`, `HalloweenEffect.cs`, e eventos de rede dedicados em `EFT.GlobalEvents`: `HalloweenSummonStartedEvent.cs`, `HalloweenSyncStateEvent.cs`, `HalloweenSyncExitsEvent.cs`. **O FIKA já sincroniza Halloween por conta própria** — achamos `RegisterPacket<HalloweenEventPacket>(OnHalloweenEventPacketReceived)` já registrado em `FikaClient.cs:254`, então não é algo que o TRL-WeatherSync precise (nem deva) mexer.
- **O "congelamento" que você mencionou não é clima, é um efeito de saúde do personagem**: `references\eft-decompiled\Assembly-CSharp\EFT.HealthSystem\ActiveHealthController.cs:1458` define `protected class Frostbite : GClass3008, GInterface372, IEffect, GInterface329` — é um **efeito aplicado numa parte do corpo** (`EBodyPart.Head`, linha 2994) via `EStimulatorBuffType.FrostbiteBuff` (linha 2989), ou seja, é acionado por um **buff de estimulante/injetor** (ex.: um remédio com efeito colateral), não pela estação do ano nem pelo clima. O visual (`FrostbiteEffect.cs`, um post-processing de "gelo na tela") é só a representação gráfica desse debuff de saúde. `ChristmasTreePoI.cs`/`ChristmasIlluminationBehaviour.cs` também existem, mas são decoração de mapa/hideout (árvore de natal, iluminação), não mecânica de clima.

**Conclusão prática:** não existe, em lugar nenhum encontrado até agora, uma "nevasca jogável" (queda de FPS por vento horizontal, tela branca, etc.) ligada ao ciclo de estações. O Roadmap deve tratar §2.D como tendo só 1 sub-fase real de inverno (`Winter`/`Class453`) — o que já foi corrigido na Sessão 2 original — e não tentar simular uma nevasca a partir do sistema de Frostbite (que é saúde, não visual de mundo) nem do Halloween (que é outro evento, já sincronizado pelo próprio FIKA).

### E.3. [P-1.2.3] `SetWeatherForce` já faz a interpolação suave nativa — RESOLVIDO, muda o plano do §5.2

Lido `references\eft-decompiled\Assembly-CSharp\EFT.Weather\WeatherCurve.cs` por completo. Achado central:

- `WeatherCurve` é baseada em `UnityEngine.AnimationCurve` (curvas de keyframe nativas da Unity) para Wind/Rain/Cloudiness/Fog/Temperature, avaliadas por um tempo normalizado entre `StartTime` e `FinishTime` (propriedade `Single_0`, linha 60).
- O construtor usado por `WeatherController.method_0()` (`WeatherCurve(WeatherClass[] nodes)`, linha 70) monta uma curva com **todos os nós** do array — é o que já é usado hoje pelo FIKA no handshake único.
- O construtor usado por `WeatherController.SetWeatherForce(WeatherClass end)` (`WeatherCurve(IWeatherCurve start, WeatherClass end)`, linhas 92-110) é diferente: ele lê os **valores atuais e instantâneos** da curva em execução (`start.Rain`, `start.Wind`, `start.Fog`, etc. — tudo avaliado "agora") como ponto de partida, e cria uma curva nova de 2 pontos indo até `end` (`end.Time` sendo o momento alvo). Ou seja: **`SetWeatherForce` já É a transição suave "de onde eu estou agora até o novo clima"** — exatamente o que o Roadmap §5.2 planejava reimplementar manualmente com `Mathf.Lerp`.

**Mudança de plano recomendada para §5.1/§5.2:** ao receber um `TrlWeatherSyncPacket`, o TRL-WeatherSync **não deveria chamar `method_0()`** (troca a curva inteira, pode saltar visualmente) — deveria montar um `WeatherClass` alvo com os valores recebidos e chamar `WeatherController.Instance.SetWeatherForce(weatherClassAlvo)`, com `weatherClassAlvo.Time` = agora + 2-3s (a mesma janela de anti-snap que o Roadmap já queria). O motor da Unity cuida da interpolação via `AnimationCurve.Evaluate()` nativamente — **não é necessário nenhum loop de `Mathf.Lerp` por frame no código do mod**. Isso simplifica bastante a implementação de P-1.2.

**Ressalva encontrada:** o construtor de `SetWeatherForce` chama incondicionalmente `SetHalloweenWind(45, end.WindDirection)` (`WeatherCurve.cs:106`), que força uma rajada de vento num padrão fixo de 45 segundos — nome sugere que foi escrito originalmente só para o evento de Halloween, mas roda sempre que `SetWeatherForce` é chamado, Halloween ativo ou não. Precisa ser validado em teste (raid real) se isso causa algum comportamento de vento estranho quando o TRL-WeatherSync usar esse método fora de outubro — marcar como item de teste antes de fechar a spec técnica.

---

## F. Onde o servidor decide uma tempestade — RESOLVIDO (P-1.2.2), com uma descoberta importante

Fonte nova consultada: `references\spt-source\` (código-fonte real do servidor SPT 4.0, C#, pinado no manifest — não é mais só o assembly do cliente).

### F.1. O servidor SPT **não tem o conceito de "Storm"**

`references\spt-source\Libraries\SPTarkov.Server.Core\Models\Spt\Config\WeatherConfig.cs:72-77`:
```csharp
public enum WeatherPreset
{
    SUNNY = 1,
    RAINY = 2,
    CLOUDY = 3,
}
```
**Só existem 3 presets no servidor.** Não existe `STORM` em lugar nenhum do `spt-source` (busquei por "StormStarted" e "STORM" no C# do servidor inteiro — zero ocorrências fora de arquivos não relacionados). O servidor gera, via `WeatherGenerator.cs` (`GenerateWeatherByPreset`), uma previsão de 24h (`RaidWeatherService.GenerateFutureWeatherAndCache`) só com valores contínuos: `Rain`/`RainIntensity`, `Cloud`, `WindSpeed`/`WindGustiness`, `Fog`, `Temperature`, `Pressure` (`Models\Eft\Weather\WeatherData.cs:25-74`) — **nenhum campo booleano ou enum de "tempestade"**.

### F.2. Cada FIKA player roda seu próprio gerador de clima, isolado

`RaidWeatherService.cs:13-14` — `[Injectable(InjectionType.Singleton)]`: é um singleton **por processo de servidor**. Em SPT normal (singleplayer) isso é óbvio e correto. Mas em **FIKA, cada jogador roda sua própria instância local do servidor SPT** por trás dos panos — ou seja, cada um tem seu próprio `RaidWeatherService`, com seu próprio cache de previsão de 24h gerado por RNG local (`weightedRandomHelper`/`randomUtil`, sem nenhuma seed compartilhada entre máquinas). O handshake do FIKA (`WeatherRequest`, ver seção A/B) existe **exatamente pra tapar esse buraco** — o Cliente descarta sua própria previsão local e usa a do Host. Isso já era sabido, mas agora está confirmado com o código do servidor: sem esse handshake, cada FIKA player literalmente jogaria uma raid com clima gerado do zero, de forma totalmente independente.

### F.3. Então de onde vem a "tempestade" do cliente? — Provavelmente uma decisão local, não do servidor

Como o servidor nunca manda um "Storm" explícito, `ESeasonStatus.Storm` só pode vir de uma decisão tomada **dentro do próprio motor do cliente EFT**, a partir dos valores contínuos que ele já recebeu (chuva/nuvens). Evidência:
- `IWeatherCurve.LightningThunderProbability` (`IWeatherCurve.cs:19`) é calculada como `Mathf.InverseLerp(0.5f, 1f, Cloudiness)` — uma **probabilidade**, não um booleano. Isso é uma pista forte de que o motor "sorteia" contra essa probabilidade em algum lugar para decidir se entra em tempestade.
- `StormStartedEvent` (`EFT.GlobalEvents\StormStartedEvent.cs`) é disparado através do `GlobalEventHandlerClass` (`GlobalEventHandlerClass.cs`) — que, lendo o código completo, é um **barramento de eventos local do próprio cliente** (`Dictionary<Type, List<Action<...>>>`, `SubscribeOnEvent<T>`, `ApplyEvents()`), não uma camada de rede em si. Ele serve tanto para eventos genuinamente empurrados pelo servidor quanto para eventos criados localmente pelo próprio jogo — o nome da classe-base `SyncEventFromServer` não é prova de que toda instância vem de fato de uma chamada de rede ativa durante a raid.
- Não encontrei, em nenhum arquivo do assembly do cliente, o ponto exato (`new StormStartedEvent()` ou `CreateEvent<StormStartedEvent>()`) que efetivamente cria esse evento — provavelmente está dentro de um método com nome ofuscado (`method_N`) que não contém o nome literal da classe como string, o que escapa de busca por texto. Achar isso exigiria decompilação orientada a IL/bytecode, fora do escopo de uma busca textual.

### F.4. Conclusão prática para o TRL-WeatherSync

Isso muda a recomendação para o `ThunderEventTrigger` do Roadmap §5.1: **não vale a pena tentar fazer cada cliente "chegar sozinho" na mesma conclusão sobre quando começar uma tempestade** — mesmo que todos recebam exatamente os mesmos valores de chuva/nuvens, se a decisão de entrar em `Storm` envolve qualquer sorteio de probabilidade local (`LightningThunderProbability`), cada máquina pode sortear resultados diferentes nos mesmos dados, reproduzindo o desync original ("tempestade pra um, sol pro outro") mesmo com o clima "sincronizado".

**A solução correta é o Host decidir e mandar a decisão pronta, não a probabilidade:** o `TrlWeatherSyncPacket` deve carregar `ThunderEventTrigger` como um **evento explícito e autoritativo** ("agora É tempestade, ponto final") que força `Class444.Interface3_0.StormStarted(visual)` (ou o equivalente acessível) em todos os clientes ao mesmo tempo — nunca deixar cada cliente decidir sozinho a partir da curva. Isso é consistente com a arquitetura host-authoritative que o Roadmap já propunha em §5.1, só que agora com uma razão concreta (não só teórica) de por que é necessário.

---

## G. E a chuva normal/fraca — tem o mesmo problema da tempestade? NÃO, e isso corrige uma premissa do Roadmap original

Pergunta natural depois de F: se `Storm` diverge por ser decisão local, o mesmo vale pra intensidade normal de chuva? **Não.** Investigação:

### G.1. Chuva normal é dado contínuo de curva, não uma decisão local

`Rain` (o campo que controla a intensidade de chuva "normal") é um dos valores que `WeatherCurve.method_4()` usa pra montar `RainCurve` (`WeatherCurve.cs:262`, `RainCurve.AddKey(time2, num4)`) — a mesmíssima máquina de interpolação por `AnimationCurve` usada por `Cloudiness`/`Wind`/`Fog`/`Temperature` (ver C.1/E.3). Ou seja: **chuva normal não tem "sorteio" nenhum** — é só um número que sobe e desce suavemente ao longo do tempo, igual, determinístico, em qualquer máquina que receba a mesma curva. Diferente de `Storm`, que é um **estado discreto** (`ESeasonStatus`) decidido por fora da curva.

### G.2. O campo `RainRandomness` existe, mas o nome engana — e hoje ele nem é usado pela curva

`WeatherClass.RainRandomness` (`WeatherClass.cs:34`) parecia, pelo nome, ser candidato a "gerador de ruído local" citado na intuição original do Roadmap (§1.1: "`RainRandomness` [...] localmente via geradores de ruído da Unity"). Investigado a fundo:

- **O FIKA já serializa esse campo hoje, por completo**, na sincronização de clima: `FikaSerializationExtensions.cs:837` (`writer.Put(weatherClass.RainRandomness)`) e `:869` (`reader.GetFloat()` na volta) — **não há perda de dado no pacote atual do FIKA**, contrariando a hipótese de que esse era o vetor de divergência.
- **`RainRandomness` não é lido em nenhum lugar de `IWeatherCurve`/`WeatherCurve`** — a interface (`IWeatherCurve.cs`) simplesmente não expõe essa propriedade, e `WeatherCurve.method_4()` (que monta a curva a partir dos nós) nunca acessa `weatherClass.RainRandomness`. Ou seja, **o pipeline que realmente controla a chuva na tela (`WeatherController.method_10/11` → `RainController`) não consome esse valor via curva**.
- **O nome "RainRandomness" é um artefato de tradução/legado, não uma descrição funcional.** Achado em `GClass2606.cs` (rótulo comunitário `EFT.Weather.WeatherSerializer`, não confirmado oficialmente) — é o serializer JSON usado na comunicação HTTP cliente↔servidor, e mapeia `RainRandomness` (cliente) ↔ campo `rain_intensity` (linhas 57/77). Isso bate exatamente com o campo `RainIntensity` do lado do **servidor SPT** (`references\spt-source\...\Models\Eft\Weather\WeatherData.cs:36-37`, JSON `"rain_intensity"`, com o comentário do lado de `Rain`: *"1-3 light rain, 3+ 'rain'"*). **Conclusão: `RainRandomness` é só o nome (mal escolhido) que o cliente EFT usa pra "intensidade de chuva" vinda do servidor — não é ruído/aleatoriedade gerada localmente.** A hipótese original do Roadmap (§1.1) de um "gerador de ruído da Unity" causando divergência de chuva **não se sustentou** nesta investigação; foi encontrado só um único uso real de `UnityEngine.Random` ligado a clima (`WeatherClass.cs:208`, `TopWindDirection = UnityEngine.Random.insideUnitCircle * ...`), e é dentro de um método de **clima de teste/debug** (usado por `WeatherController.Debug_SetTestWeather`), não no caminho normal de jogo.

### G.3. Conclusão prática

Chuva normal/fraca **já fica coberta pelo mecanismo padrão do `TrlWeatherSyncPacket`** (campo `RainIntensity`, ver §5.1) — não precisa de nenhum tratamento especial como o `ThunderEventTrigger` da tempestade. O único jeito de ela divergir ao longo de uma raid longa seria por **drift de relógio** entre o `GameDateTime` do Host e do Cliente (a curva é avaliada por tempo decorrido — se os relógios desalinharem, os dois avaliam pontos ligeiramente diferentes da mesma curva) — e é exatamente esse tipo de drift que o broadcast periódico (a cada 10-15s, reaplicando `SetWeatherForce`) já corrige por reforço contínuo, então não é um problema novo, é só mais uma razão pra manter o pacote periódico em vez de confiar só no handshake único.

---

## H. `ERainControllerStatus` existe de verdade — autocorreção, e o elo Class444↔RainController (2026-09-10, sessão de continuação)

Pergunta do usuário que motivou esta seção: *"E se nós não estamos encontrando a tempestade porque ela é meio que a chuva no nível máximo (1 a 5)?"*. Resposta curta: não é bem isso (não achei uma checagem direta tipo "se Rain==5, dispara Storm"), mas investigar essa hipótese levou a uma correção importante de um erro cometido na sessão inicial deste mesmo dia.

### H.1. Autocorreção: `ERainControllerStatus` existe — está aninhado, não é tipo de topo de arquivo

A seção C.2 desta investigação (mais acima) e o `ROADMAP.md` §2.D afirmavam que `ERainControllerStatus` **não existe** no assembly, baseado numa busca por arquivo/nome de tipo de topo que não achou nada. **Isso estava errado.** `ERainControllerStatus` é um `enum` **aninhado dentro da classe `RainController`** ([`RainController.cs:19-29`](../../../../references/eft-decompiled/Assembly-CSharp/RainController.cs#L19)) — não tem arquivo próprio, por isso a busca por nome de arquivo/tipo de topo não achou. Valores reais:

```csharp
public enum ERainControllerStatus : byte
{
    Summer, WinterStorm, WinterStormReconnect, Winter, Spring, SpringEarly, Autumn, AutumnLate
}
```

Lição prática (já documentada em `graph-code-navigation`, reforçada aqui com caso real): "sem entrada numa busca por tipo de topo ≠ não existe" — vale também pra tipos aninhados, que podem escapar de buscas centradas em arquivo.

### H.2. `RainController` é um segundo state machine, paralelo ao de `Class444`, e os dois se conectam no evento de tempestade

`RainController.cs` tem sua própria hierarquia de estados (`Class670`..`Class678`, análoga à de `Class444.Class446`..`Class456`), só que cuidando do **visual** de chuva/neve, não da lógica de estação. `Class677` (o estado `WinterStorm`) faz exatamente o que o `ROADMAP.md` original (Sessão 1) descrevia como "nevasca":

```csharp
public class Class677 : Class675 {
    public Class677(RainController controller) : base(controller, ERainControllerStatus.WinterStorm) {
        base.RainController_0._snowWetRenderer.StormEnabled = true;
        base.RainController_0._snowFlakes.StormFactor = 1f;   // ativa StormSideSpeed (SnowFlakes.cs:49)
    }
}
```

**O elo entre os dois sistemas** (não documentado antes desta sessão): quando `Class444` transiciona pra `ESeasonStatus.Storm`, os construtores de `Class451`/`Class452` chamam diretamente o `RainController`:

```csharp
// Class444.cs:435-441
public class Class451 : Class445, Interface3, IDisposable {
    public Class451(Class444 controller) : base(controller, ESeasonStatus.Storm, ESeason.Summer) {
        Class445.RainController_0.method_8();   // → RainController entra em WinterStorm
    }
}
// Class444.cs:459-465 — Class452 (variante de reconexão) chama method_9() → WinterStormReconnect
```

`RainController.method_8()`/`method_9()` ([`RainController.cs:690-698`](../../../../references/eft-decompiled/Assembly-CSharp/RainController.cs#L690)) delegam pro state machine interno (`Class668_0.vmethod_6()`/`vmethod_7()`), que no estado `Summer` (`Class670`, o estado base) instancia `Class677`/`Class678`. **Conclusão: `ESeasonStatus.Storm` (lógica/gameplay, em `Class444`) e `ERainControllerStatus.WinterStorm` (visual de chuva/neve, em `RainController`) são duas metades do mesmo evento de tempestade, disparadas juntas.** O nome "WinterStorm" é usado mesmo quando a tempestade acontece em contexto de Verão (`Class451` usa `ESeason.Summer`) — o motor de neve parece ser reaproveitado como efeito visual de tempestade em geral, não exclusivo de inverno (ainda não confirmado se isso é intencional ou um artefato de como o jogo foi desenvolvido).

### H.3. A hipótese "chuva nível 5 = tempestade" — não confirmada, mas plausível como correlação

Não encontrado nenhum código que compare `Rain`/`Cloudiness` contra um limiar pra disparar `StormStartedEvent` diretamente — o disparo continua sendo via evento discreto (`SyncEventFromServer`), não uma fórmula contínua. Porém, dados novos desta sessão (ver I abaixo) tornam a correlação plausível mesmo sem prova direta: o servidor já pesa a escolha de `Rain` (1 a 5) e `Cloudiness` por estação, e `IWeatherCurve.LightningThunderProbability` é literalmente derivado de `Cloudiness` — então é razoável que o disparo de tempestade dependa (por sorteio, contra essa probabilidade) de estar num momento de chuva/nuvem alta, mesmo sem ser um "if rain==5" direto.

## I. Dados reais do `weather.json` do servidor — pesos por estação e datas de calendário reais (2026-09-10)

Levantado a pedido do usuário, que quer comparar com dados reais de chuva por estação. Arquivo: [`references/spt-source/Libraries/SPTarkov.Server.Assets/SPT_Data/configs/weather.json`](../../../../references/spt-source/Libraries/SPTarkov.Server.Assets/SPT_Data/configs/weather.json).

### I.1. Estações seguem datas reais de calendário, não um ciclo abstrato

`seasonDates` mapeia dia/mês reais pra `Season` (enum do servidor, [`Season.cs`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Enums/Season.cs): `SUMMER=0, AUTUMN=1, WINTER=2, SPRING=3, AUTUMN_LATE=4, SPRING_EARLY=5, STORM=6`):

| Estação | Período (dia/mês) |
|---|---|
| Verão | 23/06 → 15/10 |
| Outono | 15/10 → 01/11 |
| Outono Tardio | 01/11 → 13/12 |
| Inverno (início) | 13/12 → 31/12 |
| Inverno (fim) | 01/01 → 09/01 |
| Primavera Inicial | 09/01 → 20/03 |
| Primavera | 20/03 → 23/06 |

Achado extra: uma janela nomeada `"STORM"` (`seasonType: 4`, mesmo valor de `AUTUMN_LATE`) de **24/10 a 04/11** — bem em cima do período real de Halloween. Não confirmado se isso influencia `ESeasonStatus.Storm` de alguma forma ou é só um rótulo não usado — `Season.STORM = 6` existe como valor de enum mas essa entrada de `seasonDates` usa `seasonType: 4`, então na prática essa janela é tratada como Outono Tardio, não como "estação Storm". Pendência de investigação se for relevante depois.

`GetActiveWeatherSeason()` ([`SeasonalEventService.cs:307-333`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/SeasonalEventService.cs#L307)) usa a data real do servidor (`timeUtil.GetDateTimeNow()`, não o relógio acelerado da raid) contra essa tabela pra decidir a estação ativa.

### I.2. Pesos de clima por estação — inverno é configurado pra chover/nevar bem mais

`weatherPresetWeight` no JSON:

```json
"WINTER_START": { "SUNNY": 3, "RAINY": 8, "CLOUDY": 3 },
"WINTER_END":   { "SUNNY": 3, "RAINY": 8, "CLOUDY": 3 },
"default":      { "SUNNY": 13, "RAINY": 7, "CLOUDY": 7 }
```

Inverno: 57% chance de `RAINY` (que em contexto de inverno o motor renderiza como neve, via `RainController` no estado `Winter`/`Class676`). Resto do ano ("default"): 48% sol, 26% chuva, 26% nublado. **Bate com a intuição de vida real do usuário** — inverno é a estação mais chuvosa/nevada por design.

**Suspeita de bug no próprio SPT, não confirmada:** `WeatherGenerator.GetWeatherPresetWeightsBySeason()` ([`WeatherGenerator.cs:77-82`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Generators/WeatherGenerator.cs#L77)) busca a chave por `currentSeason.ToString()`. Como `Season.WINTER = 2` é o único membro do enum com esse valor, `.ToString()` retorna `"WINTER"` — que **não bate** com nenhuma das chaves do JSON (`"WINTER_START"`, `"WINTER_END"`, `"default"`). Se essa leitura estiver certa, a tabela especial de inverno nunca é encontrada na prática, e o inverno usa os pesos de `"default"` por engano (48% sol em vez de 57% chuva/neve). **Não testado em jogo/log — é uma leitura de código, não uma confirmação empírica.** Relevante pra quando formos desenhar a política de clima por estação do TRL-WeatherSync: não copiar esse detalhe sem verificar primeiro se é intencional ou bug.

### I.3. Confirmação adicional pro fix do CR-01-01 (code review)

`"rain": {"2":5,"3":5,"4":5,"5":5}` pro preset `RAINY` — o servidor de fato sorteia `Rain` num valor discreto entre 1 e 5, reforçando que a correção aplicada em `WeatherSyncSession.cs` (`Mathf.Lerp(1f, 5f, curve.Rain)` antes de transmitir) usa a escala certa.

### I.4. Nota de design (pedido do usuário, registrar pra quando formos desenhar a feature de estações)

O usuário quer adaptar essas taxas/datas reais pra proposta do TRL-WeatherSync de **estação semanal + configurável** (não usar as datas reais de calendário/EFT diretamente, mas se inspirar nos pesos relativos — ex.: inverno ~2x mais chance de chuva/neve que as outras estações — pra dar realismo dentro do ciclo comprimido de 1 semana por estação do Roadmap §3). Fica registrado aqui como insumo de design pra quando o item de backlog do Gerenciador de Estações for criado — não é código ainda, é contexto.

## Pendências abertas para a próxima sessão de pesquisa

1. ~~Localizar o disparo server-side de `StormStartedEvent`~~ — **Resolvido parcialmente** (ver F): confirmado que o servidor SPT não tem conceito de "Storm" (só `SUNNY/RAINY/CLOUDY`), então a decisão é local ao cliente. O call site exato dentro do assembly do cliente não foi localizado (provavelmente em método ofuscado) — só vale a pena retomar isso se a estratégia de "Host decide e força" (ver F.4) não for suficiente na prática.
2. **Validar em raid real** se `SetHalloweenWind` (chamado por dentro de `SetWeatherForce`, ver E.3) produz algum artefato visual de vento quando usado fora do contexto de Halloween — teste rápido antes de weather técnica.
3. Mapear o método exato em `Class444`/`Interface3_0` acessível externamente para forçar `StormStarted(visual)` a partir de um patch do TRL-WeatherSync — `Interface3_0` é público (`Class444.cs:621`) mas `StormStarted` é membro da interface `Interface3`, então dá pra chamar `Class444.Instance.Interface3_0.StormStarted(visual)` diretamente; falta confirmar de onde pegar a instância de `GInterface49 visual` esperada pelo método.
4. **Confirmar (ou descartar) a suspeita de bug do `WINTER`/`WINTER_START`/`WINTER_END`** (ver I.2) — testar em jogo/log se o inverno realmente usa os pesos especiais ou cai em "default" por engano. Relevante só como curiosidade pro upstream SPT; não bloqueia o TRL-WeatherSync (que vai definir sua própria política, não herdar a do servidor vanilla).
5. Investigar se a janela `"STORM"` de 24/10-04/11 em `seasonDates` (ver I.1) tem alguma relação real com `ESeasonStatus.Storm` ou é vestigial/não usada.
