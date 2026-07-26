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
4. [O Padrão Canônico de Sincronização Defensiva](#4-o-padrão-canônico-de-sincronização-defensiva)
5. [Template Canônico de Código C# (Copy-Paste para Mods)](#5-template-canônico-de-código-c-copy-paste-para-mods)
6. [Inventário & Status dos Mods do Workspace](#6-inventário--status-dos-mods-do-workspace)
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

O estouro de exceções do tipo `ParseException: Undefined packet in NetDataReader: <HASH>` e o descongestionamento de frames (jogadores patinando/congelados) ocorrem por quatro falhas estruturais:

### 🔴 1. Registro Tardio (Late Registration / Timing de CPU)
Quando um jogador abre a raid, se o mod não registrar o pacote no **frame zero** da inicialização do FIKA, um pacote enviado pelo Host pode chegar na placa de rede do Client antes que o Client tenha executado a chamada de registro. O LiteNetLib consulta o dicionário de handlers, não encontra a chave e descarta a leitura.

### 🔴 2. Perda de Registro em Trocas de Sessão (`IFikaNetworkManager` Recriado)
Ao transitar de menu/lobby para a raid, o FIKA recria a instância de `IFikaNetworkManager`. Mods que utilizam flags booleanas estáticas (`_isRegistered = true`) acreditam que o registro ainda está ativo, mas a nova instância do `NetPacketProcessor` está vazia.

### 🔴 3. Chamadas Nocivas a `UnregisterPacket<T>()`
Desregistrar pacotes ao sair da raid ou ao desativar funcionalidades remove o tipo da tabela de hashes do LiteNetLib. Se pacotes tardios ou retidos em buffer de rede chegarem após a desativação, a camada de transporte é corrompida.

### 🔴 4. Exceções Não-Tratadas nos Callbacks (Fila do Frame Descartada)
Se um callback de mod lança uma exceção não-tratada (como `NullReferenceException`), ela **não é capturada em nenhum ponto do caminho**: sobe por `ReadAllPackets` → `OnNetworkReceive` → `ProcessEvent` até [`LiteNetManager.PollEvents`](../../references/fika-plugin/Fika.Core/Networking/LiteNetLib/LiteNetManager.cs), que desanexa a fila de eventos pendentes e a percorre **sem `try/catch`**:

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

### 💡 Resposta Técnica Definitiva:

**SIM, a diferença de velocidade de processamento da CPU/SSD pode causar falhas SE o mod depender de inicialização ingênua em `Awake()` ou `Start()`. Mas a solução proposta CERCA 100% esse problema.**

#### Por que isso acontece?
1. O BepInEx carrega os plugins de forma paralela/indeterminística com base na descoberta de arquivos e agendamento do SO.
2. Em um PC ultra-rápido (Host), o mod `SpeakFromTarkov` pode rodar o `Awake()` 50ms antes do mod `ImmersiveCombatMedicine`. Em um PC mais lento (Guest), a ordem pode se inverter.
3. Se os pacotes fossem indexados por ordem de chamada (0, 1, 2...), a inversão quebraria a rede. **Porém, no FIKA/LiteNetLib, a hash do pacote é determinística (deriva do nome da classe `typeof(T).FullName`).**
4. Portanto, a única variável crítica é o **TEMPO DE REGISTRO**: se o Host enviar o pacote no frame 1 da raid e a CPU do Guest ainda estiver finalizando a carga do mod no frame 2, o pacote chega sem handler cadastrado no Guest.

#### Como o nosso padrão cerca 100% este problema:
- **Garantia Pré-Envio**: Antes de *qualquer* transmissão (`Broadcast` / `SendData`), o mod executa `EnsurePacketsRegistered()`.
- **Garantia Pré-Recepção**: O método `EnsurePacketsRegistered()` é invocado no topo do loop `Update()` da Unity no frame zero de carregamento do FIKA, registrando os handlers no `NetPacketProcessor` **antes que a fila de rede comece a ler os pacotes da raid**.
- **Independência de Ordem de Mods**: Como o registro é feito por hash de tipo (`typeof(T)`), a ordem em que o Mod A ou o Mod B são carregados pela CPU é irrelevante; o LiteNetLib associa a hash exata da classe independentemente da sequência.

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

Auditoria de 2026-07-26 (8 mods do workspace verificados um a um):

| Mod | Pacotes FIKA | Envelope | Main thread | Airbag + guard | Status |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **`TRL-SpeakFromTarkov`** | 1 (`SftAudioPacketV2`) | 🟢 | 🟢 fila → `Update` | 🟢 | 🟢 **Conforme** (v1.4.0) |
| **`stancesAndCameraPositionSPT4.0.11`** | 1 (`StanceSyncPacketV2`) | 🟢 | 🟢 já era | 🟢 | 🟢 **Conforme** (v2.11.0) |
| **`TRL-ImmersiveCombatMedicine`** | 6 (`*V2`) | 🟢 | 🟢 já era | 🟢 | 🟢 **Conforme** (v1.11.0) |
| `CustomClasses` | — | — | — | — | ⚪ N/A (só reflection de `FikaBackendUtils` p/ UI) |
| `TarkovIRL-SPT4.0-beta` | — | — | — | — | ⚪ N/A (zero símbolos de rede) |
| `TRL-DynamicSpawn` | — | — | — | — | ⚪ N/A (reflection de papel host/client) |
| `TRL-Fixes` | — | — | — | — | ⚪ N/A (Harmony local) |
| `TRL-ItemsManagement` | — | — | — | — | ⚪ N/A (nem referencia `Fika.Core`) |

**Fora do escopo da auditoria, mas presentes no repo:** `Skills-Extended` (`LockPickingSyncPacket`), `TrueTrauma` (`TraumaFaintPacket`) e `mods/Band-Aid/` (predecessor standalone do ICM) — nenhum revisado contra este guia.

> ⚠️ `mods/Band-Aid/` declara `Band_Aid.BandAidHealPacket` e outros 3 com FQN **idêntico** aos stubs legados do ICM. Instalar os dois ao mesmo tempo faz um registro sobrescrever o outro. Hoje só o ICM está instalado; `node scripts/check-packet-hashes.js` avisa se isso mudar.

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
- [ ] **Envelope de comprimento**: `Serialize` grava o corpo com `PutBytesWithLength`; `Deserialize` consome com `TryGetBytesWithLength`.
- [ ] **Só `TryGet*`**: nenhum `GetString`/`GetInt`/`GetFloat` cru no `Deserialize` — eles lançam em payload truncado.
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
