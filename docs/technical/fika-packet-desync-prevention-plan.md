---
title: Guia Canônico de Sincronização FIKA e Prevenção de Desync (NetPacketProcessor)
date: 2026-07-26
status: 🟢 Vivo
authors: Guilherme + agente
---

# 🌐 Guia Canônico de Sincronização FIKA e Prevenção de Desync (`NetPacketProcessor`)

Este documento é a **fonte de verdade técnica e arquitetural** para desenvolvimento e manutenção de mods client/server com suporte multiplayer coop no **FIKA (SPT 4.0 / Tarkov)**. Qualquer mod do projeto que transmita pacotes via rede FIKA deve aderir obrigatoriamente aos padrões descritos nesta especificação.

---

## 📑 Sumário

1. [Arquitetura de Rede do FIKA & LiteNetLib](#1-arquitetura-de-rede-do-fika--litenetlib)
2. [Causas Raiz de Desincronização & `ParseException`](#2-causas-raiz-de-desincronização--parseexception)
3. [A Dúvida da Velocidade de CPU vs Sincronia de Arquivos](#3-a-dúvida-da-velocidade-de-cpu-vs-sincronia-de-arquivos)
4. [O Padrão Canônico de Sincronização Defensiva](#4-o-padrão-canônico-de-sincronização-defensiva) · [4.1 Padrão híbrido (API de eventos)](#41-padrão-híbrido--a-api-de-eventos-do-fika--polling)
5. [Template Canônico de Código C# (Copy-Paste para Mods)](#5-template-canônico-de-código-c-copy-paste-para-mods)
6. [Inventário & Status dos Mods do Workspace](#6-inventário--status-dos-mods-do-workspace) · [6.1 Não conformes](#61-os-três-não-conformes--detalhe) · [6.2 Descartados](#62-verificado-e-descartado)
7. [Checklist de Auditoria e Validação](#7-checklist-de-auditoria-e-validação)

---

## 1. Arquitetura de Rede do FIKA & LiteNetLib

No FIKA, a transmissão de pacotes customizados de mods utiliza estruturas `INetSerializable` intermediadas pelo `NetPacketProcessor` da biblioteca nativa `LiteNetLib`, exposta através do contrato `Singleton<IFikaNetworkManager>.Instance` (`FikaClient` ou `FikaServer`).

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                             MOD CLIENT / SERVER                             │
│                                                                             │
│   EnsurePacketsRegistered()  ──►  IFikaNetworkManager  ──► SendData()       │
└─────────────────────────────────────┬───────────────────────────────────────┘
                                      │
                                      ▼
                        ┌───────────────────────────┐
                        │    NetPacketProcessor     │
                        │ (LiteNetLib Hash/Callback)│
                        └─────────────┬─────────────┘
                                      │
                                      ▼  UDP Stream
                        ┌───────────────────────────┐
                        │      Host / Peers FIKA    │
                        └───────────────────────────┘
```

- **Identificação de Pacotes**: o `NetPacketProcessor` mapeia cada `INetSerializable` por uma hash **CRC-16-CCITT de 16 bits** calculada sobre `typeof(T).ToString()` (namespace + nome do tipo) — ver `ShortHashCache<T>` e `WriteShortHash<T>` em [NetPacketProcessor.cs:27-56](../../references/fika-plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs). Três consequências práticas:
  - A hash **não depende da versão do mod**. Mudar o layout de um pacote mantendo o nome do tipo faz peers de versões diferentes aceitarem o pacote um do outro e desalinharem em silêncio. Mudança de formato exige **renomear o tipo** (sufixo `V2`), para que o peer desatualizado falhe de forma diagnosticável.
  - O registro é um `Dictionary<ushort, …>`: dois tipos que colidam fazem o segundo `RegisterPacket` **sobrescrever** o handler do primeiro, sem erro no log. Com 16 bits e dezenas de mods instalados isso é verificável — rode `node scripts/check-packet-hashes.js` (hoje: 52 tipos, 0 colisões).
  - Existe também um `HashCache<T>` FNV-1 de 64 bits no mesmo arquivo, mas ele **não é usado** no caminho `INetSerializable`.
- **Ciclo de Vida do Manager**: A instância de `IFikaNetworkManager` **não é persistente durante todo o jogo**. O FIKA destrói e recria o gerenciador de rede em transições de sessão (ex.: Menu ➔ Lobby ➔ Raid ➔ Tela de Extração/Desconexão).
- **Buffer de envio compartilhado**: `FikaClient` e `FikaServer` serializam **todos** os envios num único `NetDataWriter` de instância (`_dataWriter`, [FikaClient.cs:123](../../references/fika-plugin/Fika.Core/Networking/FikaClient.cs) e [FikaServer.cs:129](../../references/fika-plugin/Fika.Core/Networking/FikaServer.cs)), **sem lock**. Chamar `SendData` fora da main thread corrompe esse buffer — ver causa raiz 🔴 6.

---

## 2. Causas Raiz de Desincronização & `ParseException`

O estouro de exceções do tipo `ParseException: Undefined packet in NetDataReader: <HASH>` e o descongestionamento de frames (jogadores patinando/congelados) ocorrem por seis falhas estruturais. **As seis terminam no mesmo lugar** — uma exceção que sobe até o `PollEvents` e derruba a fila de eventos do frame inteiro (causa 4). Vale ler a causa 4 primeiro: ela é o mecanismo, as outras são as portas de entrada.

> ⚠️ **Nenhuma delas "descarta silenciosamente" um pacote.** Hash sem handler registrado **lança**: `GetCallbackFromData` faz `throw new ParseException(...)` em [NetPacketProcessor.cs:83-91](../../references/fika-plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs). Não existe caminho de descarte — o custo de um registro ausente é o mesmo de uma exceção em callback.

### 🔴 1. Registro Tardio (Late Registration / Timing de CPU)
Quando um jogador abre a raid, se o mod não registrar o pacote no **frame zero** da inicialização do FIKA, um pacote enviado pelo Host pode chegar antes que o Client tenha executado a chamada de registro. O LiteNetLib consulta o dicionário de handlers, não encontra a chave e **lança `ParseException`** — que sobe até o `PollEvents` e derruba **todos** os eventos daquele frame (causa 4), inclusive os `PlayerState` de movimento dos outros peers. Um mod que registra tarde não perde só o próprio pacote: congela o movimento de todo mundo naquele frame.

### 🔴 2. Perda de Registro em Trocas de Sessão (`IFikaNetworkManager` Recriado)
Ao transitar de menu/lobby para a raid, o FIKA recria a instância de `IFikaNetworkManager`. Mods que utilizam flags booleanas estáticas (`_isRegistered = true`) acreditam que o registro ainda está ativo, mas a nova instância do `NetPacketProcessor` está vazia. O efeito é idêntico à causa 1 — `ParseException` a cada pacote recebido daquele tipo, e a fila do frame cai junto.

### 🔴 3. Chamadas Nocivas a `UnregisterPacket<T>()`
Desregistrar pacotes ao sair da raid ou ao desativar funcionalidades remove o tipo do dicionário de handlers. Pacotes tardios ou retidos em buffer de rede que cheguem depois disso caem exatamente no `throw` da causa 1 — a cada datagrama, até a sessão acabar. É a causa 1 provocada de propósito.

### 🔴 4. Exceções Não-Tratadas nos Callbacks (Fila do Frame Descartada) — **o mecanismo comum**
Se um callback de mod lança uma exceção não-tratada (como `NullReferenceException`), ela **não é capturada em nenhum ponto do caminho**. Cadeia verificada, sem um único `try/catch`:

```
LiteNetManager.PollEvents           :1406-1442   ← desanexa a fila e itera sem proteção
  └─ ProcessEvent                   :443-444
       └─ FikaClient.OnNetworkReceive  :494-500
            └─ NetPacketProcessor.ReadAllPackets  :135-141
                 └─ ReadPacket → GetCallbackFromData  :88  ← throw ParseException
```

O laço final, em [`LiteNetManager.cs:1436-1441`](../../references/fika-plugin/Fika.Core/Networking/LiteNetLib/LiteNetManager.cs):

```csharp
while (pendingEvent != null)
{
    var next = pendingEvent.Next;
    ProcessEvent(pendingEvent);   // ← sem proteção
    pendingEvent = next;
}
```

O resultado é maior do que "o lote daquele datagrama": **todos os eventos pendentes do frame são descartados** — de todos os peers, de todos os outros mods, e também os `EPacketType.PlayerState`, que carregam posição e movimento. É por isso que uma falha num mod de áudio ou de pose se manifesta como jogadores patinando.

### 🔴 5. Assimetria entre `Serialize` e `Deserialize` (Desalinhamento do Reader)
`ReadAllPackets` percorre o datagrama com `while (reader.AvailableBytes > 0) ReadPacket(reader)`. Se um `Deserialize` consome um número de bytes **diferente** do que o `Serialize` escreveu, o `NetDataReader` fica desalinhado e a iteração seguinte lê 2 bytes arbitrários como hash de pacote → `ParseException: Undefined packet in NetDataReader: <lixo>` → causa 4 acima.

O sintoma é traiçoeiro: **o hash reportado no erro não corresponde a tipo nenhum**, e o mod culpado não é o que aparece no log. Para confirmar que um hash é lixo, rode `node scripts/check-packet-hashes.js --list` e procure o número.

Três fontes de assimetria, todas já observadas neste repo:
- `try/catch` dentro do `Deserialize` que engole a falha e retorna **sem** consumir o restante do payload.
- Peer com versão divergente do struct (campo novo/removido) — ver a regra de renomear o tipo em §1.
- Uso de `Get*` que lança em payload truncado. Prefira sempre as variantes **`TryGet*`**, que devolvem `false` sem lançar.

> ⚠️ **O airbag `try/catch` do callback NÃO protege o `Deserialize`.** O `Deserialize` roda dentro do lambda registrado pelo `SubscribeNetSerializable`, **antes** de `onReceive` ([NetPacketProcessor.cs:387-396](../../references/fika-plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs)). Uma exceção ali passa por fora do airbag.

### 🔴 6. `SendData` fora da Main Thread (Corrupção do Buffer Compartilhado)
`FikaClient`/`FikaServer` reusam um único `_dataWriter` para todo envio, sem lock. Enviar de uma thread de background (áudio, I/O, worker) enquanto a main thread envia `PlayerState` faz as duas escritas se intercalarem no mesmo buffer, e o que sai na rede é um datagrama malformado — que produz a causa 5 **no receptor**, sem que haja nada de errado no código dele.

**Regra:** todo `SendData` acontece na main thread. Trabalho em background deve **enfileirar** (`ConcurrentQueue`) e deixar o `Update()` drenar.

---

## 3. A Dúvida da Velocidade de CPU vs Sincronia de Arquivos

### ❓ A dúvida comum:
> *"O nosso launcher garante que todos os jogadores estão com os mesmos arquivos dos mods. Se a velocidade de processamento do computador de um jogador for diferente dos outros, a ordem de carregamento pode mudar e quebrar o pacote?"*

### 💡 Resposta Técnica:

**SIM, a diferença de velocidade de CPU/SSD pode causar falhas se o mod depender de inicialização ingênua em `Awake()`/`Start()`. O padrão deste guia reduz drasticamente a janela — mas não a fecha por completo, e vale saber por quê.**

#### Por que isso acontece?
1. O BepInEx carrega os plugins de forma paralela/indeterminística com base na descoberta de arquivos e agendamento do SO.
2. Em um PC ultra-rápido (Host), o mod `SpeakFromTarkov` pode rodar o `Awake()` 50ms antes do mod `ImmersiveCombatMedicine`. Em um PC mais lento (Guest), a ordem pode se inverter.
3. Se os pacotes fossem indexados por ordem de chamada (0, 1, 2...), a inversão quebraria a rede. **Porém, no FIKA/LiteNetLib, a hash do pacote é determinística** — CRC-16 de `typeof(T).ToString()`, ver §1. Ordem de carga dos mods é irrelevante.
4. Portanto, a única variável crítica é o **TEMPO DE REGISTRO**: se o Host enviar o pacote no frame 1 da raid e a CPU do Guest ainda estiver finalizando a carga do mod, o pacote chega sem handler — e isso **lança** (causa 1), derrubando a fila do frame.

#### O que o padrão cobre
- **Pré-envio**: antes de *qualquer* transmissão (`Broadcast`/`SendData`), o mod executa `EnsurePacketsRegistered()`.
- **Pré-recepção**: `EnsurePacketsRegistered()` é invocado no `Update()` do plugin, re-registrando assim que a instância do manager muda (§4).
- **Independência de ordem de mods**: o registro é por hash de tipo; a sequência de carga não importa.

#### ⚠️ A janela residual — por que "100%" seria mentira

`FikaClient` e `FikaServer` chamam `PollEvents()` de dentro do **próprio `Update()`** ([FikaClient.cs:312-314](../../references/fika-plugin/Fika.Core/Networking/FikaClient.cs), [FikaServer.cs:546-548](../../references/fika-plugin/Fika.Core/Networking/FikaServer.cs)). A ordem de execução de `Update()` entre `MonoBehaviour`s no Unity é **indeterminada** sem Script Execution Order configurado.

Ou seja: no primeiro frame após a criação do manager, se o `Update()` do `FikaClient` rodar **antes** do `Update()` do seu mod, o pacote é lido com o dicionário ainda vazio. A janela é de um frame e é rara — mas existe, e não se fecha com polling.

**Quem quiser fechá-la de verdade** usa a API de eventos do FIKA (§4.1): o registro acontece no momento em que o manager é criado, não no `Update()` seguinte. Mesmo assim resta uma fresta — o evento dispara **depois** de `await client.Init()`, que já abriu o socket (`FikaClient.cs:179/183`). O padrão híbrido (evento **+** polling como rede de segurança) é o mais próximo de zero que dá para chegar sem patchear o FIKA.

---

## 4. O Padrão Canônico de Sincronização Defensiva

Todo mod do projeto que transmita pacotes via FIKA deve implementar o padrão **Rastreamento por Referência de Instância**:

```
                       ┌───────────────────────────────┐
                       │       MonoBehaviour.Update()   │
                       └───────────────┬───────────────┘
                                       │
                                       ▼
                       ┌───────────────────────────────┐
                       │   EnsurePacketsRegistered()   │
                       └───────────────┬───────────────┘
                                       │
               ┌───────────────────────┴───────────────────────┐
               ▼                                               ▼
  Manager Instância Mudou?                       Manager Instância Igual?
  (instance != _lastRegistered)                  (instance == _lastRegistered)
               │                                               │
               ▼                                               ▼
  1. RegisterPacket<T>(Handler)                   Nenhuma Ação (Zero Overhead)
  2. _lastRegistered = instance
```

### Regras de Ouro de Implementação:

1. **Rastreamento por Referência de Instância (`IFikaNetworkManager`)**:
   Armazene uma referência privada `private static IFikaNetworkManager _lastRegisteredManager;`. Re-registre os pacotes **somente** quando `Singleton<IFikaNetworkManager>.Instance != _lastRegisteredManager`.
2. **Invocação Dupla (Update + SendData)**:
   Invoque `EnsurePacketsRegistered()` tanto no `Update()` principal do mod quanto imediatamente antes de qualquer chamada a `SendData()`.
3. **Proibição Absoluta de `UnregisterPacket<T>()`**:
   **NUNCA** chame `UnregisterPacket`. Desativações de lógica fora de raid devem ser tratadas com guard clauses dentro do callback (`if (!Singleton<GameWorld>.Instantiated) return;`).
4. **Airbag / Try-Catch Raiz em Callbacks**:
   Todo callback registrado no `NetPacketProcessor` deve ter o seu corpo 100% envolvido por um bloco `try { ... } catch (Exception ex) { Log.LogError(ex); }`. Em caminhos de alta frequência (áudio, tick por frame), o log precisa de **throttle** — stack completo na primeira ocorrência de cada tipo de exceção e resumo periódico depois, senão uma falha sistemática vira flood e causa hitching.
5. **Envelope de Comprimento em Todo Pacote** (fecha a causa 5):
   O corpo do pacote é gravado com prefixo de tamanho, de modo que o reader externo avance sempre exatamente o declarado — mesmo que a leitura interna falhe. É o que impede que um mod desalinhe o stream compartilhado.
6. **Renomear o Tipo ao Mudar o Formato**:
   A hash deriva do nome do tipo, não da versão. Mudou o layout → sufixo `V2`. Opcionalmente, registre um **stub do nome antigo** que apenas consome o payload: não restaura funcionalidade, mas evita que um peer desatualizado derrube a fila de eventos do frame inteiro com `Undefined packet`.
7. **Enviar Sempre da Main Thread** (fecha a causa 6):
   Produtores em background enfileiram; o `Update()` drena e transmite.

### 4.1 Padrão híbrido — a API de eventos do FIKA + polling

O FIKA expõe uma API oficial de modding em [`Fika.Core/Modding/Events/`](../../references/fika-plugin/Fika.Core/Modding/Events/), com 10 eventos. Dois interessam aqui:

| Evento | Disparado em |
|---|---|
| `FikaNetworkManagerCreatedEvent` | [`NetManagerUtils.cs:198`](../../references/fika-plugin/Fika.Core/Networking/NetManagerUtils.cs) (server) e `:207` (client) |
| `FikaNetworkManagerDestroyedEvent` | `FikaClient.cs:362` · `FikaServer.cs:603` |

Registrar **no evento** resolve a causa 2 na origem: o handler entra no `NetPacketProcessor` no instante em que o manager nasce, sem esperar o próximo `Update()`. O `Skills-Extended` deste repo já usa esse caminho ([`FikaSyncPlugin.cs:37`](../../mods/Skills-Extended/modded/FikaSync/FikaSyncPlugin.cs)).

```csharp
private void Awake()
{
    // UMA vez, no Awake. Ver ressalva 2 abaixo — não dá para desinscrever.
    FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnManagerCreated);
}

private static void OnManagerCreated(FikaNetworkManagerCreatedEvent e)
{
    try   // ver ressalva 3: sem isto, você impede os OUTROS mods de registrarem
    {
        EnsurePacketsRegistered();
    }
    catch (Exception ex) { Log.LogError($"[NET] registro no evento falhou: {ex}"); }
}
```

**Recomendação: híbrido.** Evento **e** `EnsurePacketsRegistered()` no `Update()`/send. O `EnsurePacketsRegistered` já é idempotente (compara a referência da instância), então o polling não custa nada quando o evento já registrou — e cobre o caso de o evento não ter disparado.

Três ressalvas, todas verificadas no código:

1. **A janela não é zero.** O evento dispara **depois** de `await client.Init()`, que já executou `_netClient.Start()` ([`FikaClient.cs:179/183`](../../references/fika-plugin/Fika.Core/Networking/FikaClient.cs)) — o socket já está aberto quando o handler roda.
2. **Não dá para desinscrever.** `FikaEventDispatcher.SubscribeEvent<T>` embrulha o callback num lambda novo, e `UnsubscribeEvent` faz `-=` de **outro** lambda novo — que não remove nada ([`FikaEventDispatcher.cs`](../../references/fika-plugin/Fika.Core/Modding/FikaEventDispatcher.cs)). Subscrever **uma única vez no `Awake`**; subscrever por raid vaza handlers acumulados.
3. **Airbag obrigatório no handler do evento.** `DispatchEvent` faz `OnFikaEvent?.Invoke(e)` sem `try/catch`. Como é um multicast delegate, uma exceção no seu handler **interrompe a lista de invocação** — os mods registrados depois do seu nunca recebem o evento e ficam sem registrar os pacotes deles. Um mod mal-comportado aqui causa a causa 1 em todos os outros.

> **Mods já conformes não precisam migrar.** `TRL-SpeakFromTarkov` v1.4.0, `stancesAndCameraPositionSPT4.0.11` v2.11.0 e `TRL-ImmersiveCombatMedicine` v1.11.0 foram fechados e validados in-game com polling puro, que continua correto. O híbrido é **recomendado para código novo**; a coluna "Registro" no §6 documenta qual mecanismo cada mod usa.

---

## 5. Template Canônico de Código C# (Copy-Paste para Mods)

Utilize este padrão como modelo base para qualquer manipulador de rede FIKA no repositório:

```csharp
using System;
using Comfort.Common;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;
using BepInEx.Logging;

namespace Seumod.Networking
{
    public class FikaNetworkHandler
    {
        private static IFikaNetworkManager _lastRegisteredManager;
        private static ManualLogSource Log => SeuModPlugin.Log;

        /// <summary>
        /// Garante que os pacotes estejam registrados na instância ATIVA do FIKA.
        /// Deve ser chamado no Update() do Plugin e antes de qualquer SendData.
        /// </summary>
        public static void EnsurePacketsRegistered()
        {
            if (!Singleton<IFikaNetworkManager>.Instantiated) return;

            var currentManager = Singleton<IFikaNetworkManager>.Instance;
            if (_lastRegisteredManager == currentManager) return;

            try
            {
                // Registra os pacotes do mod
                currentManager.RegisterPacket<MeuPacoteCustomizado>(OnMeuPacoteReceived);
                
                _lastRegisteredManager = currentManager;
                Log.LogInfo("[NET] Pacotes FIKA registrados com sucesso na nova instância do NetworkManager.");
            }
            catch (Exception ex)
            {
                Log.LogError($"[NET] Falha ao registrar pacotes no FIKA: {ex.Message}");
            }
        }

        /// <summary>
        /// Transmite o pacote para a rede de forma segura.
        /// </summary>
        public static void Broadcast(MeuPacoteCustomizado packet, DeliveryMethod method = DeliveryMethod.ReliableOrdered)
        {
            EnsurePacketsRegistered();
            if (!Singleton<IFikaNetworkManager>.Instantiated) return;

            try
            {
                Singleton<IFikaNetworkManager>.Instance.SendData(ref packet, method, broadcast: true);
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[NET] Erro ao transmitir pacote: {ex.Message}");
            }
        }

        /// <summary>
        /// Handler de recepção com Airbag / Try-Catch Raiz.
        /// </summary>
        private static void OnMeuPacoteReceived(MeuPacoteCustomizado packet)
        {
            try
            {
                // Guard clause: Ignora se estiver fora de raid
                if (!Singleton<EFT.GameWorld>.Instantiated) return;

                // Corpo truncado: não processar NEM retransmitir (ver Valid abaixo)
                if (!packet.Valid) return;

                // Lógica do mod...
            }
            catch (Exception ex)
            {
                // Proteção para evitar descartar a fila de eventos do frame
                Log.LogError($"[NET] Exceção capturada no handler de rede: {ex}");
            }
        }
    }
}
```

### 5.1 Envelope de Comprimento (obrigatório em todo `INetSerializable`)

> 🔁 **A instância do pacote é REUTILIZADA entre recepções.** `SubscribeNetSerializable` cria **um** `new T()` e o reusa em toda chegada — *"To reduce allocations, this method uses a single internal reference to `T`"* ([NetPacketProcessor.cs:388-397](../../references/fika-plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs)). Duas consequências:
>
> - **Vale para `struct` e `class`:** o `Deserialize` **precisa resetar todos os campos logo na entrada**. Campo não escrito nesta leitura guarda o valor da leitura **anterior** — é por isso que o template abaixo zera tudo antes de ler, e não por estilo.
> - **Só para pacote declarado como `class`:** o handler **não pode reter a referência** nem processar de forma assíncrona — o próximo pacote sobrescreve o mesmo objeto. Os pacotes deste repo são `struct` (o callback recebe cópia por valor), então o risco não se aplica aqui; aplica-se a quem copiar o padrão usando `class`.

```csharp
public struct MeuPacoteV2 : INetSerializable   // sufixo V2: a hash vem do NOME do tipo
{
    public string ProfileId;
    public float Valor;

    /// <summary>NÃO serializado. Falso quando o corpo veio truncado.</summary>
    internal bool Valid;

    [ThreadStatic] private static NetDataWriter _inner;

    public void Serialize(NetDataWriter writer)
    {
        var inner = _inner ??= new NetDataWriter(true, 256);
        inner.Reset();

        inner.Put(ProfileId ?? string.Empty);
        inner.Put(Valor);

        // `checked`: estouro falha visível em vez de truncar o comprimento em silêncio.
        // ⚠️ Usar SEMPRE o overload de 3 args. O de 1 arg — PutBytesWithLength(inner.Data) —
        // delega a PutArray(data, 1) e escreve o BUFFER INTEIRO, incluindo o padding além
        // de Length (NetDataWriter.cs:381). Transmitiria lixo e inflaria o pacote.
        writer.PutBytesWithLength(inner.Data, 0, checked((ushort)inner.Length));
    }

    public void Deserialize(NetDataReader reader)
    {
        ProfileId = string.Empty;
        Valor = 0f;
        Valid = false;

        // Consome SEMPRE o envelope inteiro; false sem lançar quando falta dado.
        if (!reader.TryGetBytesWithLength(out var payload) || payload == null) return;

        var inner = new NetDataReader(payload);

        // ATENÇÃO: TryGetString escreve null no `out` quando falha — passar o campo direto
        // destrói o default. Ler para local e atribuir só no sucesso.
        if (!inner.TryGetString(out var profileId) || profileId == null) return;
        ProfileId = profileId;

        if (!inner.TryGetFloat(out Valor)) return;

        Valid = true;
    }
}
```

### 5.2 Envio a partir de thread de background

```csharp
// Produtor (thread de background): apenas ENFILEIRA. Nada de API Unity/EFT/FIKA aqui.
public void Enqueue(byte[] dados)
{
    while (_fila.Count >= MaxItens && _fila.TryDequeue(out _)) { }   // drop-oldest
    _fila.Enqueue(dados);
}

// Consumidor (main thread): transmite.
void Update()
{
    EnsurePacketsRegistered();
    if (_fila.IsEmpty) return;

    while (_fila.TryDequeue(out var item))
    {
        var packet = new MeuPacoteV2 { /* ... */ };
        Singleton<IFikaNetworkManager>.Instance.SendData(ref packet, DeliveryMethod.Unreliable, broadcast: true);
    }
}
```

---

## 6. Inventário & Status dos Mods do Workspace

**Critério do universo auditado:** todo mod do repo que declara pelo menos um `INetSerializable` — são **6** dos 41 diretórios de `mods/`. Levantados com `grep -rl "INetSerializable" mods/ --include="*.cs"` (excluindo `original/` e `-bak/`); a contagem cruza com `node scripts/check-packet-hashes.js`. Mods sem pacote próprio não entram na tabela: mesmo referenciando `Fika.Core` por reflection (papel host/client, `FikaBackendUtils` para UI), não tocam o stream compartilhado.

A coluna **Instalado** reflete `D:/SPT/BepInEx/plugins/` **nesta máquina** — é o que separa risco ativo de dívida documental.

Auditoria de 2026-07-26:

| Mod | Pacotes | Envelope | Main thread | Airbag + guard | Registro | Instalado | Status |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| **`TRL-SpeakFromTarkov`** | 1 (`SftAudioPacketV2`) | 🟢 | 🟢 fila → `Update` | 🟢 | polling | 🟢 | 🟢 **Conforme** (v1.4.0) |
| **`stancesAndCameraPositionSPT4.0.11`** | 1 (`StanceSyncPacketV2`) | 🟢 | 🟢 já era | 🟢 | polling | 🟢 (`RealisticMobility/`) | 🟢 **Conforme** (v2.11.0) |
| **`TRL-ImmersiveCombatMedicine`** | 6 (`*V2`) | 🟢 | 🟢 já era | 🟢 | polling | 🟢 | 🟢 **Conforme** (v1.11.0) |
| `Skills-Extended` | 1 (`LockPickingSyncPacket`) | 🔴 | — | 🔴 | evento | ⚪ parcial | 🟠 **Não conforme · inativo** |
| `TrueTrauma - FINALIZADO` | 1 (`TraumaFaintPacket`) | 🔴 | — | 🔴 | evento | ⚪ não | 🟠 **Não conforme · não instalado** |
| `Band-Aid` | 4 (`BandAid*`) | 🔴 | — | — | 🔴 `bool` | ⚪ não | 🟠 **Não conforme · não instalado** |

### 6.1 Os três não conformes — detalhe

Nenhum é fork nosso; **não foram alterados**, só auditados. Nenhum representa risco ativo hoje, pelos motivos da coluna Instalado.

- **`Skills-Extended`** — [`LockPickingSyncPacket`](../../mods/Skills-Extended/modded/FikaSync/Packets/LockPickingSyncPacket.cs) serializa direto (`writer.Put`) e lê com `GetString`/`GetInt`/`GetBool` **crus**, que lançam em payload truncado (causa 5); sem envelope, sem flag `Valid`, e sem reset dos campos na entrada do `Deserialize` (§5.1). O callback em [`FikaSyncPlugin.cs:52-59`](../../mods/Skills-Extended/modded/FikaSync/FikaSyncPlugin.cs) **não tem airbag** — uma exceção ali derruba a fila do frame (causa 4). Registra por `FikaNetworkManagerCreatedEvent`, que é o padrão recomendado de §4.1.
  **Por que é inativo:** `D:/SPT/BepInEx/plugins/SkillsExtended/` contém `SkillsExtended.dll` e `SkillsExtendedCommon.dll`, mas **não** o assembly do Fika sync — o plugin que registra o pacote não está instalado. Se um update trouxer o `SkillsExtendedFika.dll`, isto vira risco ativo.
- **`TrueTrauma - FINALIZADO`** — [`TraumaFaintPacket`](<../../mods/TrueTrauma - FINALIZADO/FikaPacketManager.cs>) tem os mesmos problemas de serialização (`Put`/`Get*` crus, sem envelope, sem `Valid`, sem reset). Acerta o registro: subscreve `FikaNetworkManagerCreatedEvent` uma única vez, e o `bool _initialized` guarda a **subscrição**, não o registro do pacote — que é o uso correto do flag.
- **`Band-Aid`** — predecessor standalone do ICM. Usa `bool _initialized` para o **registro** (a antipattern da regra 1 de §4), com um paliativo que reseta o flag quando `IFikaNetworkManager` deixa de estar instanciado — frágil: se o manager for recriado sem que um frame observe `!Instantiated`, o flag continua `true` e o novo `NetPacketProcessor` fica vazio (causa 2).

> ⚠️ **FQN duplicado entre `Band-Aid` e o ICM.** `mods/Band-Aid/` declara `Band_Aid.BandAidHealPacket` e outros 3 com FQN **idêntico** aos stubs legados do ICM. Mesmo FQN → mesma hash: instalar os dois ao mesmo tempo faz um `RegisterPacket` sobrescrever o handler do outro, sem erro no log. Hoje só o ICM está instalado. `node scripts/check-packet-hashes.js` reporta os 4 casos como aviso.

### 6.2 Verificado e descartado

- **`hazelify.StanceSync.dll`** — está instalado em `D:/SPT/BepInEx/plugins/`, e o repo tem `mods/StanceSync/`. Nenhum `.cs` do mod referencia `Fika`, `INetSerializable` ou `NetDataWriter`, e o binário instalado tem **zero** ocorrências da string `Fika`. Não declara pacote — fora do escopo deste guia.

---

## 7. Checklist de Auditoria e Validação

Antes de aprovar qualquer PR ou alteração de mod que envolva sincronização FIKA, execute a seguinte lista de verificação:

**Registro e ciclo de vida**
- [ ] **Sem Flags Estáticas Booleans**: O mod utiliza rastreamento por referência (`_lastRegisteredManager == currentManager`) em vez de um simples `bool _initialized`.
- [ ] **Zero Invocação de `UnregisterPacket`**: A palavra-chave `UnregisterPacket` não existe no repositório do mod.
- [ ] **Garantia no Loop `Update()`**: A verificação `EnsurePacketsRegistered()` é chamada no `Update()` principal do plugin.
- [ ] **Segurança no Envio (`SendData`)**: `EnsurePacketsRegistered()` é chamada imediatamente antes de qualificar o envio do pacote — inclusive nos relays feitos de dentro de callbacks. Centralizar os envios num único helper é o jeito de garantir isso.
- [ ] **Callback estático**: o delegate registrado sobrevive à destruição de um `MonoBehaviour`. Se o callback for método de instância, ele pode rodar sobre um objeto Unity já destruído.

**Serialização** (causa 5)
- [ ] **Envelope de comprimento**: `Serialize` grava o corpo com `PutBytesWithLength` **no overload de 3 args** (`data, offset, length`) — o de 1 arg escreve o buffer inteiro, com padding; `Deserialize` consome com `TryGetBytesWithLength`.
- [ ] **Só `TryGet*`**: nenhum `GetString`/`GetInt`/`GetFloat` cru no `Deserialize` — eles lançam em payload truncado.
- [ ] **Campos resetados na entrada do `Deserialize`**: a instância do pacote é **reutilizada** entre recepções (§5.1); campo não escrito nesta leitura guarda o valor da anterior.
- [ ] **String lida para local**: `TryGetString` escreve `null` no `out` ao falhar; passar o campo direto destrói o default.
- [ ] **Flag `Valid`**: corpo truncado não é processado **nem retransmitido** (o host relayaria lixo re-serializado).
- [ ] **Tipo renomeado se o formato mudou**: sufixo `V2` + bump de versão + nota de release lockstep. Stub do nome antigo registrado para consumir o payload de peers desatualizados.

**Threading** (causa 6)
- [ ] **`SendData` só na main thread**: nenhum envio a partir de thread de captura/worker/`Task`. Produtores em background enfileiram.

**Runtime**
- [ ] **Callbacks com Try-Catch Total**: 100% do código dentro de `OnPacketReceived` está envelopado por `try { ... } catch (Exception ex) { Log.LogError(ex); }`, com o objeto `ex` completo (não só `ex.Message`).
- [ ] **Log com throttle** em caminhos de alta frequência.
- [ ] **Guard Clause de Instância Ativa**: Callbacks validam a existência do `GameWorld.Instantiated` antes de mutar o estado dos jogadores.

**Automatizado**
- [ ] `node scripts/check-packet-hashes.js` passa (sem colisão de CRC-16).

---

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-26 | Guilherme + agente | Criação. |
| 2026-07-26 | Guilherme + agente | Correção factual e ampliação após auditoria dos 8 mods: hash é CRC-16 de 16 bits sobre `typeof(T).ToString()` (não FNV-1/`FullName`); causas raiz 5 (assimetria `Serialize`/`Deserialize`) e 6 (`SendData` fora da main thread corrompendo o `_dataWriter` compartilhado); esclarecido que o airbag não cobre o `Deserialize` e que a exceção derruba a fila do frame no `PollEvents`, não só o lote; §5.1/5.2 com os padrões de envelope e de fila; inventário §6 refeito com os 8 mods reais; checklist reorganizado. |
| 2026-07-26 | Guilherme + agente | Segunda revisão contra `references/fika-plugin/`, com `arquivo.cs:linha` para cada afirmação. **§2:** causa 1 descrevia "descarta a leitura" — o real é `throw new ParseException` (`NetPacketProcessor.cs:88`); causas 1-3 reescritas para convergir na causa 4, com a cadeia sem `try/catch` (`PollEvents:1436-1441` → `ProcessEvent:443` → `OnNetworkReceive:494` → `ReadAllPackets:135`). **§3:** removido o "cerca 100%" — `PollEvents` roda no `Update()` do `FikaClient`/`FikaServer` e a ordem de `Update()` entre `MonoBehaviour`s é indeterminada, então resta uma janela de um frame. **§4.1 (nova):** padrão híbrido com a API oficial `Fika.Core/Modding/Events/` (`FikaNetworkManagerCreatedEvent`), com as 3 ressalvas verificadas — evento dispara após `Init()`, `UnsubscribeEvent` não remove nada, e `DispatchEvent` sem `try/catch` faz um handler que lança impedir o registro dos mods seguintes. **§5.1:** a instância do pacote é reutilizada (`SubscribeNetSerializable:388`) → reset obrigatório dos campos; aviso sobre o overload de 1 arg de `PutBytesWithLength`. **§6:** critério do universo declarado (6 mods com `INetSerializable`), coluna "Instalado" e "Registro", e §6.1/6.2 com a auditoria dos 3 que estavam fora do escopo (todos não conformes, nenhum ativo) + `StanceSync` verificado e descartado. |
| 2026-07-26 | Guilherme | chore(harness): faixa AP-NN em vez de AP-01..AP-10 e regra de rename de doc em conventions |
