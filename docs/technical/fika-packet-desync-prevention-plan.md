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

- **Identificação de Pacotes**: O `NetPacketProcessor` do LiteNetLib mapeia cada estrutura `INetSerializable` usando a hash determinística do nome do tipo (`typeof(T).FullName`).
- **Ciclo de Vida do Manager**: A instância de `IFikaNetworkManager` **não é persistente durante todo o jogo**. O FIKA destrói e recria o gerenciador de rede em transições de sessão (ex.: Menu ➔ Lobby ➔ Raid ➔ Tela de Extração/Desconexão).

---

## 2. Causas Raiz de Desincronização & `ParseException`

O estouro de exceções do tipo `ParseException: Undefined packet in NetDataReader: <HASH>` e o descongestionamento de frames (jogadores patinando/congelados) ocorrem por quatro falhas estruturais:

### 🔴 1. Registro Tardio (Late Registration / Timing de CPU)
Quando um jogador abre a raid, se o mod não registrar o pacote no **frame zero** da inicialização do FIKA, um pacote enviado pelo Host pode chegar na placa de rede do Client antes que o Client tenha executado a chamada de registro. O LiteNetLib consulta o dicionário de handlers, não encontra a chave e descarta a leitura.

### 🔴 2. Perda de Registro em Trocas de Sessão (`IFikaNetworkManager` Recriado)
Ao transitar de menu/lobby para a raid, o FIKA recria a instância de `IFikaNetworkManager`. Mods que utilizam flags booleanas estáticas (`_isRegistered = true`) acreditam que o registro ainda está ativo, mas a nova instância do `NetPacketProcessor` está vazia.

### 🔴 3. Chamadas Nocivas a `UnregisterPacket<T>()`
Desregistrar pacotes ao sair da raid ou ao desativar funcionalidades remove o tipo da tabela de hashes do LiteNetLib. Se pacotes tardios ou retidos em buffer de rede chegarem após a desativação, a camada de transporte é corrompida.

### 🔴 4. Exceções Não-Tratadas nos Callbacks (Lote Descartado)
No LiteNetLib, múltiplos pacotes de rede são processados em lote (`ReadAllPackets`). Se um callback de mod lança uma exceção não-tratada (como `NullReferenceException`), a execução do lote é interrompida abruptamente, descartando todos os pacotes restantes daquele frame para os demais mods.

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
   Todo callback registrado no `NetPacketProcessor` deve ter o seu corpo 100% envolvido por um bloco `try { ... } catch (Exception ex) { Log.LogError(ex); }`.

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

                // Lógica do mod...
            }
            catch (Exception ex)
            {
                // Proteção para evitar descartar o lote de rede dos outros mods
                Log.LogError($"[NET] Exceção capturada no handler de rede: {ex}");
            }
        }
    }
}
```

---

## 6. Inventário & Status dos Mods do Workspace

| Mod | Possui Pacotes FIKA? | Padrão Aplicado (`EnsurePacketsRegistered`) | Callbacks Protegidos com Airbag? | Status |
| :--- | :---: | :---: | :---: | :---: |
| **`TRL-SpeakFromTarkov`** | Sim (`SftAudioPacket`) | 🟢 Sim (`SftNetwork.cs`) | 🟢 Sim | 🟢 **Conforme** |
| **`TRL-ImmersiveCombatMedicine`** | Sim (6 pacotes) | 🟢 Sim (`BandAidNetworkHandler.cs`) | 🟢 Sim | 🟢 **Conforme** |
| **`stancesAndCameraPositionSPT4.0.11`** | Sim (`StanceSyncPacket`) | 🟢 Sim (`FikaSyncManager.cs`) | 🟢 Sim | 🟢 **Conforme** |
| **`TrueTrauma`** | Sim (`TraumaFaintPacket`) | 🟢 Sim (`FikaPacketManager.cs`) | 🟢 Sim | 🟢 **Conforme** |
| **`Skills-Extended`** | Sim (`LockPickingSyncPacket`) | 🟢 Sim (`FikaSyncPlugin.cs`) | 🟢 Sim | 🟢 **Conforme** |
| **`TRL-DynamicSpawn`** | Não | N/A (Usa reflexão de estado) | N/A | 🟢 **Conforme** |

---

## 7. Checklist de Auditoria e Validação

Antes de aprovar qualquer PR ou alteração de mod que envolva sincronização FIKA, execute a seguinte lista de verificação:

- [ ] **Sem Flags Estáticas Booleans**: O mod utiliza rastreamento por referência (`_lastRegisteredManager == currentManager`) em vez de um simples `bool _initialized`.
- [ ] **Zero Invocação de `UnregisterPacket`**: A palavra-chave `UnregisterPacket` não existe no repositório do mod.
- [ ] **Garantia no Loop `Update()`**: A verificação `EnsurePacketsRegistered()` é chamada no `Update()` principal do plugin.
- [ ] **Segurança no Envio (`SendData`)**: `EnsurePacketsRegistered()` é chamada imediatamente antes de qualificar o envio do pacote.
- [ ] **Callbacks com Try-Catch Total**: 100% do código dentro de `OnPacketReceived` está envelopado por `try { ... } catch (Exception ex) { Log.LogError(ex); }`.
- [ ] **Guard Clause de Instância Ativa**: Callbacks validam a existência do `GameWorld.Instantiated` antes de mutar o estado dos jogadores.

---

*Documento revisado e validado de acordo com as diretrizes arquiteturais do projeto tarkov-spt-4.0.*
