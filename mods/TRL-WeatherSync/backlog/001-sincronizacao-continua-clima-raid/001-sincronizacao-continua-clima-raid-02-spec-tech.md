# 001 — Sincronização Contínua de Clima em Raid · Spec Técnica

**Mod:** TRL-WeatherSync
**Spec funcional:** [001-sincronizacao-continua-clima-raid-01-spec.md](001-sincronizacao-continua-clima-raid-01-spec.md)
**Criado:** 2026-09-10

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

> **Memória consultada:** snapshot de 2026-09-10 (Sessão 2) · pendências que afetam: nenhuma (P-1.2.1/2/3 já resolvidas na mesma sessão — ver [`docs/investigacao-fika-eft-2026-09-10.md`](../../docs/investigacao-fika-eft-2026-09-10.md)). Também consultado: `docs/technical/README.md` (roteamento), [`docs/technical/spt-antipatterns.md`](../../../../docs/technical/spt-antipatterns.md) (gatilho sempre — AP-01, AP-02, AP-04, AP-11 citados abaixo) e [`docs/technical/fika-packet-desync-prevention-plan.md`](../../../../docs/technical/fika-packet-desync-prevention-plan.md) (gatilho: mod declara `INetSerializable` — é a fonte de verdade canônica do repo para pacotes FIKA, usada quase literalmente na seção 5). `mods/TRL-WeatherSync/modded/` ainda não existe — este item cria a base do mod junto (ver §7, dependência de P-1.1).

## 1. Estratégia

**Nenhum Harmony patch é necessário sobre `WeatherController`/`Class444`** — ambos já expõem API pública suficiente para o mod chamar diretamente, sem tocar em internals do EFT nem do FIKA (AP-04: usar o entry point canônico, não mutação direta):

- Aplicar clima recebido: `WeatherController.Instance.SetWeatherForce(WeatherClass end)` ([`WeatherController.cs:120`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Weather/WeatherController.cs#L120)) — já interpola nativamente a partir dos valores atuais até `end` via `AnimationCurve` ([`WeatherCurve.cs:92-110`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Weather/WeatherCurve.cs#L92)), cobrindo o critério de aceite "sem salto perceptível" sem nenhum loop de `Lerp` no mod.
- Forçar/encerrar tempestade: `Class443.Controller` (`public static GInterface29 Controller`, [`Class443.cs:28-38`](../../../../references/eft-decompiled/Assembly-CSharp/Class443.cs#L28), `[CanBeNull]`, lê `Singleton<GameWorld>.Instance.GInterface29_0` — retorna `null` fora de raid, guard já embutido) → `.HandleReconnect(ESeasonStatus seasonStatus, SeasonsSettingsClass seasonsSettings)` ([`GInterface29.cs:17`](../../../../references/eft-decompiled/Assembly-CSharp/GInterface29.cs#L17)). Confirmado em [`Class444.cs:181-224`](../../../../references/eft-decompiled/Assembly-CSharp/Class444.cs#L181) que `Class446.HandleReconnect` (estado Verão) trata `case ESeasonStatus.Storm` criando `Class452` e transicionando — ou seja, **entrar em tempestade a partir de um estado normal via `HandleReconnect` é confirmado e seguro**. **TODO confirmar (raid real):** os estados de tempestade (`Class451`/`Class452`) não sobrescrevem `HandleReconnect` no trecho lido — herdariam o no-op de `Class445.HandleReconnect` ([`Class444.cs:94-98`](../../../../references/eft-decompiled/Assembly-CSharp/Class444.cs#L94), só loga e retorna `Task.CompletedTask`), o que sugeriria que **encerrar** uma tempestade chamando `HandleReconnect` de novo a partir de dentro dela pode não funcionar. Plano B se confirmado: deixar a tempestade seguir seu curso nativo (o próprio EFT decide quando termina) em vez de forçar o fim — ver §7.

Um único conjunto de patches é necessário, e só para **lifecycle de sessão** (AP-01) — saber quando iniciar/parar o loop de sincronização, não para interceptar lógica de clima:

- `GameWorld.OnGameStarted()` ([`GameWorld.cs:2584`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2584)) — Postfix — inicia a sessão de sync (resolve papel Host/Client via `FikaBackendUtils.IsServer`, confirmado em uso real em [`RequestSubPackets.cs:45`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/Packets/World/RequestSubPackets.cs#L45)).
- `GameWorld.OnDestroy()` ([`GameWorld.cs:2111`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2111)) — Postfix — encerra a sessão (idempotente). Confirmado que `ClientGameWorld.OnDestroy()` ([`ClientGameWorld.cs:219-223`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/ClientGameWorld.cs#L219)) chama `base.OnDestroy()` — o alvo dispara normalmente em raid real (PA-01-01, review 01).
- `CoopGame.Stop(string, ExitStatus, string, float)` ([`CoopGame.cs:718`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs#L718)) — Postfix — encerra a sessão também (cobre `Left`/`Killed`/`MIA`). **Correção pós-review (PA-01-01):** `BaseLocalGame<T>.Stop` ([`BaseLocalGame-1.cs:1018`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/BaseLocalGame-1.cs#L1018)) é `virtual`, e a classe de jogo real usada em toda raid FIKA — `CoopGame : BaseLocalGame<EftGamePlayerOwner>` — sobrescreve `Stop` e **nunca chama `base.Stop(...)`** (confirmado por grep no arquivo inteiro). Patchear a base genérica nunca dispararia em raid real (AP-03). Como este mod é FIKA-only por natureza, mirar `CoopGame.Stop` diretamente é o alvo correto — não há caminho não-FIKA a cobrir.

O pacote de rede (`TrlWeatherSyncPacket`) segue o **Padrão Canônico de Sincronização Defensiva** do repo ([`fika-packet-desync-prevention-plan.md` §4](../../../../docs/technical/fika-packet-desync-prevention-plan.md#4-o-padrão-canônico-de-sincronização-defensiva)) — registro por referência de instância + padrão híbrido (evento `FikaNetworkManagerCreatedEvent` + polling no `Update()`), nunca por `bool` estático (AP-11).

**Pré-requisito de instalação (decisão pós-review, PA-01-02):** TRL-WeatherSync exige instalação em **todos** os participantes da raid (Host e Clientes) — não há handshake de capacidade que verifique isso antes do primeiro broadcast. Um peer sem o mod recebendo `TrlWeatherSyncPacket` gera `ParseException` no `NetPacketProcessor` dele ([`NetPacketProcessor.cs:88`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs#L88)), que derruba a fila de eventos de rede do frame inteiro para todos os peers ([`LiteNetManager.cs:1439`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/LiteNetManager.cs#L1439), [`fika-packet-desync-prevention-plan.md` §2](../../../../docs/technical/fika-packet-desync-prevention-plan.md#2-causas-raiz-de-desincronização--parseexception) causa 4). Essa restrição deve ficar documentada com destaque em `README.md` e no tooltip da `ConfigEntry` `Enable Weather Sync` (§3) — ver §7 para a mitigação de timing complementar.

### 1.1. Papel de autoridade de clima — não é sempre o Host de rede (resolve P-2.2 da memória do mod)

**Problema de origem:** em raid hospedada por um servidor `Fika-Headless`, `HeadlessGameController : HostGameController` ([`HeadlessGameController.cs:18-20`](../../../../mods/FIKA/modded/Fika-Headless/Fika.Headless/Classes/GameMode/HeadlessGameController.cs#L18)) — o processo Headless **é sempre** o Host de rede (`FikaBackendUtils.IsServer == true`), mesmo quando um jogador humano é quem "inicia" a partida. E o Headless desativa deliberadamente `RainController`/`CloudController` (`Fika.Headless.Patches.DestroyGraphics.RainController_Awake_Patch.cs`/`CloudController_Patches.cs`, `Object.Destroy` no `Awake`/`OnEnable`) e nunca deixa `Class444.Run` rodar de verdade (`Fika.Headless.Patches.Class444_Run_Patch.cs`) — logo, ler `WeatherController.Instance.WeatherCurve` no processo Headless para gerar o pacote de sync é arriscado (risco não totalmente caracterizado, ver §7).

**Solução — desacoplar "autoridade de clima" de "Host de rede":** o FIKA já expõe as flags públicas necessárias para essa decisão, sem precisar inventar handshake ou eleição:

- `FikaBackendUtils.IsHeadless` — `true` **só** no processo Headless em si ([`FikaBackendUtils.cs:59`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Utils/FikaBackendUtils.cs#L59), doc XML: *"Headless clients are always raid hosts (IsServer is always true)"*).
- `FikaBackendUtils.IsHeadlessGame` — `true` em **todo cliente** conectado a uma raid hospedada por Headless ([`FikaBackendUtils.cs:67`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Utils/FikaBackendUtils.cs#L67)).
- `FikaBackendUtils.IsHeadlessRequester` — `true` no cliente que **pediu** a sessão Headless ([`FikaBackendUtils.cs:71`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Utils/FikaBackendUtils.cs#L71)) — é literalmente "quem iniciou a raid" (pergunta do usuário). Já usado pelo próprio FIKA para decidir quem pode iniciar a contagem regressiva ([`ClientGameController.cs:57`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/ClientGameController.cs#L57), `if (FikaBackendUtils.IsHeadlessRequester || ...AnyoneCanStartRaid)`).

**Regra de resolução de papel** (calculada uma vez em `WeatherSyncSession.Begin()`, mesmo ponto onde `_isHost` era resolvido antes):

```
IsHeadless          → papel = Relay    (nunca lê o próprio WeatherController; só retransmite)
IsHeadlessGame
  && IsHeadlessRequester → papel = Source   (autoridade de clima — lê a própria curva e transmite)
IsHeadlessGame
  && !IsHeadlessRequester → papel = Receiver (aplica o que chega, nunca transmite)
!IsHeadlessGame && IsServer → papel = Source   (raid normal — Host de sempre, comportamento inalterado)
!IsHeadlessGame && !IsServer → papel = Receiver
```

**Fluxo de rede em raid Headless:** um Cliente só enxerga um peer (o servidor) — não existe conexão direta Cliente↔Cliente ([`FikaClient.cs:388`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaClient.cs#L388), `_netClient.SendToAll` de um Cliente alcança só o peer dele, o servidor). Por isso o `Source` (o jogador que iniciou a raid) manda o pacote pro Headless, e o Headless — no papel `Relay` — **retransmite** pra todos os outros peers, sem nunca chamar `SetWeatherForce`/tocar no próprio `WeatherController`. Isso elimina o risco de P-2.2 por completo para o processo Headless: ele passa a ser só um "cabo" pro pacote, nunca uma fonte de dados de clima.

**Limitação conhecida, não resolvida nesta spec:** se o `IsHeadlessRequester` desconectar no meio da raid, ninguém reassume o papel `Source` automaticamente — a sincronização de clima simplesmente para (cada peer fica com o último clima aplicado; não trava nada, só deixa de evoluir em conjunto). Reeleição de autoridade fica fora de escopo deste item.

## 2. Pontos de patch

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`GameWorld.cs:2584`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2584) `OnGameStarted()` | Postfix | Início da sessão de sync — resolve o papel (`Source`/`Relay`/`Receiver`, §1.1), cria/reseta o `WeatherSyncSession`. |
| [`GameWorld.cs:2111`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GameWorld.cs#L2111) `OnDestroy()` | Postfix | Fim da sessão de sync (idempotente). |
| [`CoopGame.cs:718`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs#L718) `Stop(string, ExitStatus, string, float)` | Postfix | Fim da sessão de sync — cobre saída por extração/morte/MIA (idempotente com o acima). Alvo corrigido de `BaseLocalGame<T>.Stop` na review 01 (PA-01-01) — `CoopGame` sobrescreve sem chamar base. |

Nenhum patch em `WeatherController`/`Class444`/FIKA — só chamadas à API pública deles (§1).

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Networking` | `Enable Weather Sync` | bool | `true` | — | — | Ativa a sincronização contínua de clima entre Host e Clientes em raids FIKA. Sem efeito fora de raids coop. |
| `Networking` | `Sync Interval Seconds` | float | `10.0` | 5 a 30 | Sim | Intervalo, em segundos, entre cada pacote de sincronização de clima enviado pelo Host. Valores menores deixam o clima mais preciso, mas aumentam o tráfego de rede. |

## 4. Arquivos do mod

> `mods/TRL-WeatherSync/modded/` ainda não existe — todos os arquivos abaixo são criação nova. O `.csproj`/estrutura de solução (P-1.1) é pré-requisito e é criado junto neste item, já que nenhum código pode compilar sem ele.

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/TRL-WeatherSync.csproj` | CRIAR | Projeto BepInEx 5 (.NET Standard 2.1), referenciando `Fika.Core` do `mods/FIKA/modded/Fika-Plugin/` e as DLLs do jogo resolvidas por `/compile-mod`. |
| `modded/Plugin.cs` | CRIAR | `BaseUnityPlugin`, registra `ConfigEntry`s de §3, registra os patches de lifecycle do §2. |
| `modded/Patches/RaidLifecyclePatches.cs` | CRIAR | 3 patches do §2 (start/stop da sessão). |
| `modded/Networking/TrlWeatherSyncPacket.cs` | CRIAR | `struct : INetSerializable` com envelope de comprimento (AP-11 §5.1). |
| `modded/Networking/WeatherSyncNetworkHandler.cs` | CRIAR | Registro híbrido (evento + polling), envio, callback de recepção com airbag. |
| `modded/WeatherSyncSession.cs` | CRIAR | `MonoBehaviour` raid-scoped: no Host, acumula tempo e envia o pacote (padrão `FikaServer._sendThreshold`); no Client, aplica o pacote recebido via `SetWeatherForce`. |

## 5. Stubs de código

### 5.1. `TrlWeatherSyncPacket.cs`

Campos escolhidos como o subconjunto de `WeatherClass` ([`WeatherClass.cs:22-56`](../../../../references/eft-decompiled/Assembly-CSharp/WeatherClass.cs#L22)) necessário para reconstruir um `WeatherClass` alvo passável a `SetWeatherForce`, mais o campo próprio `ThunderEventTrigger` (não existe em `WeatherClass` — é metadado do mod, decisão explícita do Host sobre `ESeasonStatus.Storm`, ver §1). **`AtmospherePressure` removido na review 01 (PA-01-04)** — `IWeatherCurve` ([`IWeatherCurve.cs`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.Weather/IWeatherCurve.cs)) não expõe pressão, e `WeatherCurve.method_4()` (o método que constrói as curvas interpoladas a partir de `WeatherClass[]`) não lê `AtmospherePressure` — mesmo padrão já confirmado para `RainRandomness` na investigação (`docs/investigacao-fika-eft-2026-09-10.md` seção G): campo existe na struct de transporte mas não afeta o clima visível via `SetWeatherForce`. Segue o template canônico de envelope ([`fika-packet-desync-prevention-plan.md` §5.1](../../../../docs/technical/fika-packet-desync-prevention-plan.md#51-envelope-de-comprimento-obrigatório-em-todo-inetserializable)) — obrigatório porque a instância do pacote é **reutilizada** entre recepções (`NetPacketProcessor.cs:387-397`, confirmado em [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/LiteNetLib/Utils/NetPacketProcessor.cs)).

```csharp
// modded/Networking/TrlWeatherSyncPacket.cs
using System;
using Fika.Core.Networking.LiteNetLib.Utils;

namespace TRLWeatherSync.Networking;

/// <summary>
/// Pacote broadcast periódico do Host com o alvo de clima corrente da raid.
/// Campos espelham o subconjunto necessário de WeatherClass (Assembly-CSharp/WeatherClass.cs:22-56)
/// para reconstruir um WeatherClass e chamar WeatherController.Instance.SetWeatherForce(...).
/// </summary>
public struct TrlWeatherSyncPacket : INetSerializable
{
    /// <summary>Ticks (long) do momento-alvo da transição — vira WeatherClass.Time.</summary>
    public long TargetTime;

    public float Cloudness;
    public float Wind;
    public int WindDirection;
    public float Rain;
    public float ScaterringFogDensity;
    public float Temperature;

    /// <summary>Decisão explícita e autoritativa do Host: true = forçar ESeasonStatus.Storm agora.</summary>
    public bool ThunderEventTrigger;

    /// <summary>NÃO serializado. Falso quando o corpo veio truncado — não processar nem retransmitir.</summary>
    internal bool Valid;

    [ThreadStatic] private static NetDataWriter _inner;

    public void Serialize(NetDataWriter writer)
    {
        var inner = _inner ??= new NetDataWriter(true, 64);
        inner.Reset();

        inner.Put(TargetTime);
        inner.Put(Cloudness);
        inner.Put(Wind);
        inner.Put(WindDirection);
        inner.Put(Rain);
        inner.Put(ScaterringFogDensity);
        inner.Put(Temperature);
        inner.Put(ThunderEventTrigger);

        // overload de 3 args — o de 1 arg escreve o buffer inteiro com padding (ver AP-11 §5.1).
        writer.PutBytesWithLength(inner.Data, 0, checked((ushort)inner.Length));
    }

    public void Deserialize(NetDataReader reader)
    {
        // reset total: a instância é reutilizada entre recepções (NetPacketProcessor.cs:387-397).
        TargetTime = 0;
        Cloudness = 0f;
        Wind = 0f;
        WindDirection = 0;
        Rain = 0f;
        ScaterringFogDensity = 0f;
        Temperature = 0f;
        ThunderEventTrigger = false;
        Valid = false;

        if (!reader.TryGetBytesWithLength(out var payload) || payload == null) return;

        var inner = new NetDataReader(payload);

        if (!inner.TryGetLong(out TargetTime)) return;
        if (!inner.TryGetFloat(out Cloudness)) return;
        if (!inner.TryGetFloat(out Wind)) return;
        if (!inner.TryGetInt(out WindDirection)) return;
        if (!inner.TryGetFloat(out Rain)) return;
        if (!inner.TryGetFloat(out ScaterringFogDensity)) return;
        if (!inner.TryGetFloat(out Temperature)) return;
        if (!inner.TryGetBool(out ThunderEventTrigger)) return;

        Valid = true;
    }
}
```

### 5.2. `WeatherSyncNetworkHandler.cs`

Adaptado quase literalmente do template canônico ([`fika-packet-desync-prevention-plan.md` §5](../../../../docs/technical/fika-packet-desync-prevention-plan.md#5-template-canônico-de-código-c-copy-paste-para-mods)) — registro por referência de instância (nunca `bool`), padrão híbrido evento+polling, `SendData` só na main thread, airbag em todo callback. **Atualizado para o modelo de 3 papéis (§1.1):** o registro agora depende do `WeatherRole` resolvido pela sessão — `Source` não registra nada (só envia), `Receiver` registra o handler normal (aplica via `SetWeatherForce`), `Relay` (só o processo Headless) registra a variante `RegisterPacket<T, TUserData>` com `TUserData = NetPeer` para saber de quem retransmitir, e nunca toca em `WeatherController`.

```csharp
// modded/Networking/WeatherSyncNetworkHandler.cs
using System;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Networking.LiteNetLib;

namespace TRLWeatherSync.Networking;

public static class WeatherSyncNetworkHandler
{
    private static IFikaNetworkManager _lastRegisteredManager;
    private static WeatherRole _lastRegisteredRole = WeatherRole.Unset;
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;

    /// <summary>Assinar UMA vez no Awake do plugin — FikaEventDispatcher não suporta unsubscribe real
    /// (ver ressalva 2, fika-packet-desync-prevention-plan.md §4.1).</summary>
    public static void SubscribeManagerCreatedEvent()
    {
        FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnManagerCreated);
    }

    private static void OnManagerCreated(FikaNetworkManagerCreatedEvent e)
    {
        try
        {
            // Este evento pode disparar antes de GameWorld.OnGameStarted (que cria WeatherSyncSession.Instance
            // via Begin()) — nesse caso Role resolve pra Unset e EnsurePacketsRegistered não faz nada aqui de
            // propósito (PA-02-02, review 02). O polling em WeatherSyncSession.Update() cobre essa janela assim
            // que Instance/Role existir — é o padrão híbrido evento+polling do doc canônico (§4.1).
            EnsurePacketsRegistered(WeatherSyncSession.Instance?.Role ?? WeatherRole.Unset);
        }
        catch (Exception ex)
        {
            // airbag: um handler que lança aqui impede os handlers de OUTROS mods de rodar (DispatchEvent sem try/catch).
            Log.LogError($"[TRL-WeatherSync] Falha no registro via evento: {ex}");
        }
    }

    /// <summary>Chamar no Update() do plugin/sessão E imediatamente antes de qualquer SendData.
    /// `role` vem de WeatherSyncSession — Source E Receiver registram o mesmo handler de recepção
    /// (PA-02-01, review 02: em raid Headless o Relay ecoa o pacote de volta pro Source original,
    /// que precisa de handler registrado ou sofre ParseException na própria máquina); Relay (só
    /// Headless) registra a variante com NetPeer, pra saber de quem veio e reencaminhar.</summary>
    public static void EnsurePacketsRegistered(WeatherRole role)
    {
        if (role == WeatherRole.Unset) return; // ainda não estamos em raid / sessão não resolvida
        if (!Singleton<IFikaNetworkManager>.Instantiated) return;

        var currentManager = Singleton<IFikaNetworkManager>.Instance;
        if (_lastRegisteredManager == currentManager && _lastRegisteredRole == role) return;

        try
        {
            switch (role)
            {
                case WeatherRole.Source:
                case WeatherRole.Receiver:
                    currentManager.RegisterPacket<TrlWeatherSyncPacket>(OnWeatherSyncPacketReceived);
                    break;
                case WeatherRole.Relay:
                    currentManager.RegisterPacket<TrlWeatherSyncPacket, NetPeer>(OnWeatherSyncPacketReceivedForRelay);
                    break;
            }

            _lastRegisteredManager = currentManager;
            _lastRegisteredRole = role;
            Log.LogInfo($"[TRL-WeatherSync] Pacote registrado (papel: {role}) na instância ativa do NetworkManager.");
        }
        catch (Exception ex)
        {
            Log.LogError($"[TRL-WeatherSync] Falha ao registrar pacote: {ex.Message}");
        }
    }

    /// <summary>Chamado só pelo Source (autoridade de clima) — envia pro(s) peer(s) visível(is).
    /// Num Cliente isso alcança só o servidor (FikaClient.cs:388); num Host normal, todos os peers.
    /// Sempre a partir da main thread.</summary>
    public static void Broadcast(TrlWeatherSyncPacket packet)
    {
        if (!Singleton<IFikaNetworkManager>.Instantiated) return;

        try
        {
            Singleton<IFikaNetworkManager>.Instance.SendData(ref packet, DeliveryMethod.Unreliable, broadcast: true);
        }
        catch (Exception ex)
        {
            Log.LogWarning($"[TRL-WeatherSync] Erro ao transmitir pacote: {ex.Message}");
        }
    }

    private static void OnWeatherSyncPacketReceived(TrlWeatherSyncPacket packet)
    {
        try
        {
            if (!Singleton<GameWorld>.Instantiated) return;   // fora de raid, ignora
            if (!packet.Valid) return;                        // corpo truncado — não processa nem retransmite

            WeatherSyncSession.Instance?.ApplyReceivedWeather(packet);
        }
        catch (Exception ex)
        {
            // protege a fila de eventos do frame inteiro (AP-11, causa 4) — nunca deixar escapar.
            Log.LogError($"[TRL-WeatherSync] Exceção no handler de rede: {ex}");
        }
    }

    /// <summary>Só roda no processo Headless (papel Relay). NUNCA toca em WeatherController —
    /// só reencaminha o pacote recebido do Source pra todos os peers (o eco de volta pro próprio
    /// Source é inofensivo: ele só reaplicaria a si mesmo um clima muito próximo do que já tem).</summary>
    private static void OnWeatherSyncPacketReceivedForRelay(TrlWeatherSyncPacket packet, NetPeer sender)
    {
        try
        {
            if (!packet.Valid) return;

            Broadcast(packet);
        }
        catch (Exception ex)
        {
            Log.LogError($"[TRL-WeatherSync] Exceção ao retransmitir pacote (papel Relay): {ex}");
        }
    }
}
```

### 5.3. `RaidLifecyclePatches.cs`

```csharp
// modded/Patches/RaidLifecyclePatches.cs
using System;
using System.Reflection;
using BepInEx.Logging;
using EFT;
using Fika.Core.Main.GameMode;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLWeatherSync.Networking;

namespace TRLWeatherSync.Patches;

public class GameWorldOnGameStartedPatch : ModulePatch
{
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;

    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/EFT/GameWorld.cs:2584
        return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
    }

    [PatchPostfix]
    private static void Postfix()
    {
        try
        {
            WeatherSyncSession.Begin();
        }
        catch (Exception ex)
        {
            Log.LogError($"[TRL-WeatherSync] Falha ao iniciar sessão de sync: {ex}");
        }
    }
}

public class GameWorldOnDestroyPatch : ModulePatch
{
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;

    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/EFT/GameWorld.cs:2111
        return typeof(GameWorld).GetMethod(nameof(GameWorld.OnDestroy));
    }

    [PatchPostfix]
    private static void Postfix()
    {
        try
        {
            WeatherSyncSession.End();
        }
        catch (Exception ex)
        {
            Log.LogError($"[TRL-WeatherSync] Falha ao encerrar sessão de sync (GameWorld.OnDestroy): {ex}");
        }
    }
}

public class CoopGameStopPatch : ModulePatch
{
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;

    protected override MethodBase GetTargetMethod()
    {
        // ref: mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs:718
        // CoopGame : BaseLocalGame<EftGamePlayerOwner> sobrescreve Stop sem chamar base (PA-01-01,
        // review 01) — mirar a classe real usada em toda raid FIKA, não a base genérica.
        return AccessTools.Method(typeof(CoopGame), nameof(CoopGame.Stop));
    }

    [PatchPostfix]
    private static void Postfix()
    {
        try
        {
            WeatherSyncSession.End();
        }
        catch (Exception ex)
        {
            Log.LogError($"[TRL-WeatherSync] Falha ao encerrar sessão de sync (CoopGame.Stop): {ex}");
        }
    }
}
```

### 5.4. `WeatherSyncSession.cs` (esqueleto — papel resolvido por sessão, ver §1.1)

```csharp
// modded/WeatherSyncSession.cs
using System;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.Weather;
using Fika.Core.Main.Utils; // FikaBackendUtils
using UnityEngine;
using TRLWeatherSync.Networking;

namespace TRLWeatherSync;

/// <summary>Papel de autoridade de clima desta sessão — ver §1.1 da spec técnica.
/// Independente de FikaBackendUtils.IsServer quando a raid é hospedada por Fika-Headless.</summary>
public enum WeatherRole
{
    Unset,
    /// <summary>Lê a própria WeatherCurve e transmite periodicamente.</summary>
    Source,
    /// <summary>Só o processo Headless — retransmite o pacote do Source, nunca toca em WeatherController.</summary>
    Relay,
    /// <summary>Aplica o que chega via SetWeatherForce/HandleReconnect. Nunca transmite.</summary>
    Receiver,
}

public class WeatherSyncSession : MonoBehaviour
{
    public static WeatherSyncSession Instance { get; private set; }
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;

    public WeatherRole Role { get; private set; } = WeatherRole.Unset;
    private float _accumulator;
    private bool _ended;

    public static void Begin()
    {
        if (Instance != null) return; // idempotente — Begin pode disparar mais de uma vez por engano
        var gameWorld = Singleton<GameWorld>.Instance;
        if (gameWorld == null || gameWorld.MainPlayer == null) return;
        if (gameWorld.MainPlayer is HideoutPlayer) return; // AP guard: nunca em hideout

        var go = new GameObject(nameof(WeatherSyncSession));
        var session = go.AddComponent<WeatherSyncSession>();
        session.Role = ResolveRole();
        Instance = session;
    }

    /// <summary>Regra de resolução de papel — ver §1.1. Calculada uma vez por raid.</summary>
    private static WeatherRole ResolveRole()
    {
        // ref: FikaBackendUtils.cs:59 — true só no processo Headless em si.
        if (FikaBackendUtils.IsHeadless) return WeatherRole.Relay;

        // ref: FikaBackendUtils.cs:67 — true em todo cliente conectado a uma raid hospedada por Headless.
        if (FikaBackendUtils.IsHeadlessGame)
        {
            // ref: FikaBackendUtils.cs:71 — true no cliente que pediu a sessão Headless.
            return FikaBackendUtils.IsHeadlessRequester ? WeatherRole.Source : WeatherRole.Receiver;
        }

        // Raid normal (Host humano) — comportamento inalterado da v1 desta spec.
        // ref: mods/FIKA/modded/.../RequestSubPackets.cs:45
        return FikaBackendUtils.IsServer ? WeatherRole.Source : WeatherRole.Receiver;
    }

    public static void End()
    {
        if (Instance == null || Instance._ended) return; // idempotente (AP-01)
        Instance._ended = true;
        Destroy(Instance.gameObject);
        Instance = null;
    }

    private void Update()
    {
        WeatherSyncNetworkHandler.EnsurePacketsRegistered(Role);
        if (Role != WeatherRole.Source) return; // Relay e Receiver não enviam nada — só reagem a pacotes.
        if (!TRLWeatherSyncPlugin.EnableWeatherSync.Value) return;

        _accumulator += Time.unscaledDeltaTime;
        if (_accumulator < TRLWeatherSyncPlugin.SyncIntervalSeconds.Value) return;
        _accumulator = 0f;

        try
        {
            // try/catch defensivo: mesmo o Source podendo ser um Cliente humano em raid Headless
            // (nunca o próprio processo Headless, que agora é sempre Relay — ver §1.1), mantido por
            // robustez geral do broadcast.
            BroadcastCurrentWeather();
        }
        catch (Exception ex)
        {
            Log.LogError($"[TRL-WeatherSync] Falha ao montar/enviar broadcast de clima: {ex}");
        }
    }

    private void BroadcastCurrentWeather()
    {
        if (WeatherController.Instance == null) return;

        var curve = WeatherController.Instance.WeatherCurve; // ref: WeatherController.cs:86-96
        // TODO confirmar: política de decisão de tempestade fica fora do escopo deste item —
        // ponto de extensão único (ex: item de backlog futuro pode plugar a lógica aqui).
        var packet = new TrlWeatherSyncPacket
        {
            TargetTime = DateTime.UtcNow.AddSeconds(TRLWeatherSyncPlugin.SyncIntervalSeconds.Value).Ticks,
            Cloudness = curve.Cloudiness,
            Wind = curve.Wind.magnitude,
            WindDirection = NearestWindDirectionIndex(curve.Wind),
            Rain = curve.Rain,
            ScaterringFogDensity = curve.Fog,
            Temperature = curve.Temperature,
            ThunderEventTrigger = false, // TODO confirmar: fonte da decisão de tempestade do Host.
        };

        WeatherSyncNetworkHandler.Broadcast(packet);
    }

    /// <summary>
    /// Converte a direção contínua da curva (Vector2) para o índice discreto que WeatherClass
    /// espera (PA-01-05, review 01) — WeatherClass.WindDirections[] tem 9 vetores fixos
    /// (WeatherClass.cs:9-20); escolhe o de maior produto escalar com a direção normalizada.
    /// </summary>
    private static int NearestWindDirectionIndex(Vector2 wind)
    {
        if (wind.sqrMagnitude < 0.0001f) return 0;

        var normalized = wind.normalized;
        var bestIndex = 0;
        var bestDot = float.NegativeInfinity;

        for (var i = 0; i < WeatherClass.WindDirections.Length; i++)
        {
            var dot = Vector2.Dot(normalized, WeatherClass.WindDirections[i].normalized);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public void ApplyReceivedWeather(TrlWeatherSyncPacket packet)
    {
        if (WeatherController.Instance == null) return;

        var target = new WeatherClass
        {
            Time = packet.TargetTime,
            Cloudness = packet.Cloudness,
            Wind = packet.Wind,
            WindDirection = packet.WindDirection,
            Rain = packet.Rain,
            ScaterringFogDensity = packet.ScaterringFogDensity,
            Temperature = packet.Temperature,
        };

        // ref: WeatherController.cs:120 — interpola nativamente via AnimationCurve (WeatherCurve.cs:92-110).
        WeatherController.Instance.SetWeatherForce(target);

        if (packet.ThunderEventTrigger)
        {
            // ref: Class443.cs:28 (Controller) + GInterface29.cs:17 (HandleReconnect)
            // Confirmado que entrar em Storm a partir de estado normal funciona (Class444.cs:181-224).
            // TODO confirmar em raid: encerrar uma tempestade chamando isso de novo pode ser no-op
            // (Class451/Class452 não sobrescrevem HandleReconnect — herdam Class445's no-op, Class444.cs:94-98).
            Class443.Controller?.HandleReconnect(ESeasonStatus.Storm, SeasonsSettingsClass.Default);
        }
    }
}
```

## 6. Fluxo de dados

**Caso A — raid normal, Host humano (`Role = Source` no Host, `Receiver` em todo Cliente):**

```
[Host, Role = Source]
WeatherController.Instance.WeatherCurve (valores ao vivo, WeatherController.cs:86)
        │  a cada N segundos (accumulator, WeatherSyncSession.Update, espelha FikaServer.cs:546-570/_sendThreshold)
        ▼
TrlWeatherSyncPacket montado (WeatherSyncSession.BroadcastCurrentWeather)
        │  WeatherSyncNetworkHandler.Broadcast → IFikaNetworkManager.SendData(Unreliable, broadcast: true)
        ▼
LiteNetLib / UDP  (mods/FIKA/modded/.../LiteNetLib/LiteNetManager.cs)
        ▼
[Cliente, Role = Receiver]
NetPacketProcessor callback (registrado via WeatherSyncNetworkHandler.EnsurePacketsRegistered)
        │  guard Singleton<GameWorld>.Instantiated + packet.Valid
        ▼
WeatherSyncSession.ApplyReceivedWeather
        │  monta WeatherClass alvo
        ▼
WeatherController.Instance.SetWeatherForce(target)     — WeatherController.cs:120
        │  interpolação nativa via AnimationCurve       — WeatherCurve.cs:92-110
        ▼
Visual de clima do jogador (chuva/vento/neblina/nuvem/temperatura)
```

**Caso B — raid hospedada por Fika-Headless (§1.1):**

```
[Cliente que iniciou a raid, Role = Source — IsHeadlessRequester == true]
WeatherController.Instance.WeatherCurve (própria curva, nunca a do Headless)
        ▼
TrlWeatherSyncPacket montado → WeatherSyncNetworkHandler.Broadcast
        │  SendData(broadcast: true) de um Cliente só alcança o único peer dele: o servidor (FikaClient.cs:388)
        ▼
[Processo Headless, Role = Relay — FikaBackendUtils.IsHeadless == true]
RegisterPacket<TrlWeatherSyncPacket, NetPeer> → OnWeatherSyncPacketReceivedForRelay
        │  NUNCA chama WeatherController/SetWeatherForce — só reencaminha
        ▼
WeatherSyncNetworkHandler.Broadcast (de novo, agora do lado do servidor — alcança TODOS os peers,
        incluindo o Cliente Source original, que reaplica em si mesmo um valor ~idêntico, inofensivo)
        ▼
[Demais Clientes, Role = Receiver]
(mesmo caminho do Caso A a partir daqui — ApplyReceivedWeather → SetWeatherForce)
```

**Ramo tempestade (`ThunderEventTrigger`, comum aos dois casos), dentro de `ApplyReceivedWeather` em todo `Role = Receiver`:**

```
Class443.Controller.HandleReconnect(ESeasonStatus.Storm, ...)  — Class443.cs:28, GInterface29.cs:17
        ▼
Class444 transiciona o estado atual para Class451/Class452     — Class444.cs:181-224 (confirmado só para ENTRAR)
```

`CoopGame.Stop` ([`CoopGame.cs:718`](../../../../mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/GameMode/CoopGame.cs#L718)) e `GameWorld.OnDestroy` alimentam `WeatherSyncSession.End()` (fim de sessão), fora do diagrama acima por serem hooks de lifecycle, não parte do fluxo de dados de clima em si.

## 7. Riscos e dependências

- **Depende de P-1.1 (stack de compilação), ainda não entregue.** `mods/TRL-WeatherSync/modded/` não existe — este item cria o `.csproj` e a estrutura base junto (§4), então P-1.1 deixa de ser um item separado na prática; ajustar `mod-backlog.md` quando `/code-mod` rodar.
- **Dependência hard de FIKA instalado.** Toda a sessão é guardada por `Singleton<IFikaNetworkManager>.Instantiated`/`Singleton<GameWorld>.MainPlayer` — sem FIKA, a feature inteira fica inerte (consistente com o critério "Fika/multiplayer" da spec funcional).
- **Pré-requisito de instalação em todos os peers (decisão do usuário, PA-01-02, review 01).** TRL-WeatherSync deve ser tratado como **exigido em toda a raid** (Host e Clientes), não opcional por peer — ver §1. Documentar com destaque em `README.md`/tooltip F12; sem isso, um peer sem o mod sofre `ParseException` a cada broadcast (§1, causa 4 do doc canônico), derrubando a fila de eventos de rede de todos.
- **Risco não resolvido: encerrar uma tempestade forçada (pendência P-2.1 na memória do mod).** `HandleReconnect` confirmado para ENTRAR em `Storm` a partir de um estado normal; não há evidência de que funcione para SAIR de `Storm` (ver §1 e §5.4 `TODO confirmar`). Se confirmado que é no-op, plano B: não forçar o fim — deixar a tempestade nativa terminar sozinha, e o mod só garante que todos entraram juntos (ainda cobre a maior parte do critério de aceite "início e fim sincronizados", com o fim potencialmente natural em vez de forçado).
- **Risco de Host Headless (P-2.2 da memória do mod) — mitigado pelo modelo de 3 papéis (§1.1), não eliminado por completo.** O processo Headless (`mods/FIKA/modded/Fika-Headless`) desativa deliberadamente `RainController`/`CloudController` (`Object.Destroy` no `Awake`/`OnEnable`, `DestroyGraphics/RainController_Awake_Patch.cs`/`CloudController_Patches.cs`) e nunca deixa o controlador de estações real rodar (`Class444_Run_Patch.cs`, comentário no código: *"This prevents the season controller from running due to no graphics being used"*). Com o papel `Relay` (§1.1), o processo Headless **nunca mais lê `WeatherController.WeatherCurve` nem chama `SetWeatherForce`** — só reencaminha bytes, então o risco central (ler/aplicar clima num `WeatherController` capenga) deixa de existir para ele. **O que NÃO foi resolvido:** (a) se `IsHeadlessRequester` desconectar, ninguém reassume o papel `Source` — ver limitação documentada em §1.1; (b) o próprio `WeatherController.LateUpdate()` do processo Headless já roda nativamente hoje (mesmo sem este mod) e chama `RainController.method_14(...)` sem checar nulo — se isso já lança `MissingReferenceException` por conta própria (pré-existente ao mod, fora do nosso controle), continua acontecendo; não é mais um risco introduzido pelo TRL-WeatherSync, mas vale confirmar em teste real que não interfere na aplicação de `SetWeatherForce` do próprio mod caso o Headless algum dia precise aplicar localmente. Item de checklist §8 mantido para validação em servidor headless real.
- **Política de decisão de tempestade fora de escopo.** Esta spec entrega o transporte e a aplicação (pacote + `SetWeatherForce`/`HandleReconnect`); QUANDO o Host decide iniciar uma tempestade (probabilidade, timer, replicar o `LightningThunderProbability` nativo) não está definido — ponto de extensão único em `BroadcastCurrentWeather`, deliberadamente deixado simples (`ThunderEventTrigger = false` fixo no stub).
- **`SetHalloweenWind` embutido em `SetWeatherForce`.** Achado da investigação (`WeatherCurve.cs:106`) — toda chamada a `SetWeatherForce` dispara uma rajada de vento de 45s com nome ligado ao evento de Halloween, incondicionalmente. Validar em raid real fora de outubro (corner case já registrado na spec funcional).
- **Drift de relógio Host/Cliente (corner case da spec funcional) já coberto por design.** O broadcast periódico (§5.1/§5.4) é o próprio mecanismo de correção: cada ciclo reaplica os valores atuais do Host via `SetWeatherForce`, limitando qualquer divergência acumulada à janela de `SyncIntervalSeconds` (PA-01-06, review 01).
- **Compatibilidade com outros mods que declaram `INetSerializable`.** `fika-packet-desync-prevention-plan.md` §6 mapeia 6 mods no repo com pacote próprio — nome `TrlWeatherSyncPacket` não colide com nenhum FQN existente (grep feito nesta sessão). Rodar `node scripts/check-packet-hashes.js` depois do `/code-mod` (checklist §8).
- **Sem patches em `modded/` pré-existentes** — mod novo, sem conflito interno a auditar.

## 8. Checklist de implementação

- [x] Criar `modded/TRL-WeatherSync.csproj` (BepInEx 5, .NET Standard 2.1) referenciando `Fika.Core` de `mods/FIKA/modded/Fika-Plugin/` — rodar `/compile-mod` para resolver DLLs do jogo.
- [x] Criar `TrlWeatherSyncPacket.cs` (§5.1) e validar compilação isolada. *(compilação efetiva pendente de `/compile-mod`)*
- [x] Criar `WeatherSyncNetworkHandler.cs` (§5.2).
- [x] Criar `RaidLifecyclePatches.cs` (§5.3) — alvo `CoopGame.Stop` já resolvido (PA-01-01, review 01), sem `TODO` pendente.
- [x] Criar `WeatherSyncSession.cs` (§5.4) — conversão de `WindDirection` resolvida (PA-01-05) e `AtmospherePressure` removido (PA-01-04). Política de `ThunderEventTrigger` deixada como ponto de extensão (sempre `false` no envio) — fora do escopo deste item, conforme §7.
- [x] `Plugin.cs`: registrar `ConfigEntry`s (§3), assinar `WeatherSyncNetworkHandler.SubscribeManagerCreatedEvent()` uma vez no `Awake`, registrar os 3 patches de lifecycle.
- [x] Atualizar `mods/TRL-WeatherSync/PROPRIEDADES.md` com as 2 `ConfigEntry`s novas.
- [x] Documentar em `README.md` (e no tooltip da `ConfigEntry` `Enable Weather Sync`) que o mod é **exigido em todos os peers da raid** (PA-01-02, review 01) — não é opcional por jogador.
- [x] Implementar `WeatherRole`/`ResolveRole()` (§1.1/§5.4) e a variante `RegisterPacket<TrlWeatherSyncPacket, NetPeer>` no `WeatherSyncNetworkHandler` (§5.2) para o papel `Relay`.
- [ ] Testar em raid hospedada por `Fika-Headless` real: confirmar que o processo Headless resolve `Role = Relay`, nunca chama `SetWeatherForce`, e que quem iniciou a raid (`IsHeadlessRequester`) resolve `Role = Source` e consegue transmitir; confirmar que os demais Clientes recebem via retransmissão do Headless. **(requer teste in-game, não feito neste `/code-mod`)**
- [ ] Testar o caso limite do requester desconectar em raid Headless — confirmar degradação graciosa (clima para de sincronizar, sem travar nada) conforme documentado em §1.1. **(requer teste in-game)**
- [ ] Testar em raid solo (sem FIKA) — confirmar que a sessão nunca inicia (`Singleton<IFikaNetworkManager>.Instantiated == false`) e não há overhead. **(requer teste in-game)**
- [ ] Testar em raid FIKA 2+ jogadores (Host humano, sem Headless) — confirmar clima convergente ao longo da raid, transição suave, e entrada em tempestade sincronizada. **(requer teste in-game)**
- [ ] Testar reconexão em meio a tempestade — cliente reconectando recebe o próximo broadcast e aplica `ThunderEventTrigger` corretamente. **(requer teste in-game)**
- [ ] Testar raid1 → exit → raid2 — sessão nova, sem estado vazado (guard `_ended`/`Instance == null`), papel recalculado do zero a cada raid. **(requer teste in-game)**
- [ ] Rodar `node scripts/check-packet-hashes.js` — confirmar zero colisão. **(requer `/compile-mod` antes)**
- [x] Rodar `/review-technical-spec` antes de `/code-mod`. *(reviews 01 e 02, 0 bloqueadores)*

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | ✅ | §2 (3 patches, incl. `CoopGame.Stop` corrigido na review 01) + §5.3/5.4 (`_ended`, `Instance == null` guards, try/catch em todo Postfix) |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Feature não reage a ação de jogador (tiro/movimento/postura) — reage a tempo decorrido (`Source`) e a pacote de rede (`Relay`/`Receiver`). Guard equivalente aplicado: `Singleton<GameWorld>.Instantiated` (§5.2) e `WeatherRole.ResolveRole()` (§1.1/§5.4, via `FikaBackendUtils.IsHeadless`/`IsHeadlessGame`/`IsHeadlessRequester`/`IsServer`) definem o papel corretamente, inclusive no caso Headless. N/A reconfirmado na review 01 e mantido após o modelo de 3 papéis. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | **Corrigido na review 01 (PA-01-01):** `CoopGame.Stop` (`CoopGame.cs:718`) é o único alvo virtual patcheado — `BaseLocalGame<T>.Stop` era virtual e `CoopGame` (a classe real de toda raid FIKA) o sobrescreve sem chamar base; alvo corrigido para a classe real, resolvido por nome direto (não ofuscado). `GameWorld.OnGameStarted`/`OnDestroy` também virtuais, mas confirmados sem override problemático na cadeia FIKA (`ClientGameWorld.OnDestroy` chama base, `OnGameStarted` não é sobrescrito). `WeatherController`/`Class444` não são patcheados, só chamados (§1). |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | §1 — `SetWeatherForce` e `HandleReconnect` são os métodos públicos que o próprio FIKA/EFT já usam para essa classe de transição (`method_0`/`HandleReconnect` chamados nativamente em `HostGameController.cs:384`/`Class444.cs:758`). Nenhuma mutação direta de campo interno. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | §5.4 `Begin()`/`End()` idempotentes, guardados por `Instance`/`_ended`; 3 patches de lifecycle (§2) cobrem os caminhos de saída. Checklist §8 inclui teste explícito raid1→raid2. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | §3 — `EnableWeatherSync` default `true`, `SyncIntervalSeconds` default `10.0`, faixa `5-30` explícita. |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | Nenhum patch chama de volta o próprio método patcheado nem invoca a si mesmo via reflection. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | N/A | Não há cache de arma/operação/tela — o único estado persistente é `WeatherSyncSession.Instance`, revalidado a cada `Begin()`/`End()` por raid. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump (não só no recon); "não existe" conferido no `types-index.json`, nunca num grep vazio — AP-09 | ✅ | Todos os `arquivo:linha` desta spec foram lidos diretamente nesta sessão (§1, §2, §5) — não só citados de memória/pesquisa anterior. |
| 10 | Skill EFT usada como lever confirmada não-inerte (`SkillsSettings` ≠ `[]` no `globals.json`); se inerte, efeito entregue por patch direto — AP-10 | N/A | Feature não usa skill do EFT como alavanca de efeito. |
| 11 | Pacote FIKA próprio: envelope de comprimento + só `TryGet*` + flag `Valid`, campos resetados no `Deserialize`, envio só na main thread, registro por instância/evento (nunca `bool`), zero `UnregisterPacket`, airbag com throttle em todo callback — AP-11 | ✅ | §5.1 (envelope + `TryGet*` + `Valid` + reset total) e §5.2 (registro por `_lastRegisteredManager`+`_lastRegisteredRole` referência + híbrido evento/polling, `SendData` só chamado de `Update()`/broadcast na main thread, zero `UnregisterPacket`, airbag em `OnWeatherSyncPacketReceived` e `OnWeatherSyncPacketReceivedForRelay`). O papel `Relay` usa a variante `RegisterPacket<T, TUserData>` com `TUserData = NetPeer` — mesmo mecanismo do `NetPacketProcessor`, só com o peer de origem disponível ao callback; segue o mesmo contrato de envelope/`Valid` do pacote em si (§5.1), sem necessidade de um segundo tipo. Throttle de log **não** aplicado — volume esperado é baixo (1 pacote/10s), registrado como aceitável; revisitar em `/review-technical-spec` se o volume mudar. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-10 | Spec técnica criada via `/create-technical-spec` |
| 2026-09-10 | Resolvidos os 6 pontos da review 01 (PA-01-01 a PA-01-06): alvo do 3º patch corrigido para `CoopGame.Stop`; decisão do usuário — mod exigido em todos os peers da raid; try/catch adicionado a todos os Postfixes e ao broadcast; `AtmospherePressure` removido do pacote; algoritmo de conversão `WindDirection` adicionado; nota de cobertura do drift de relógio adicionada. Registrado risco novo não-bloqueante (Host em servidor Headless desativa `RainController`/`CloudController`/`Class444.Run`) — ver §7 e pendência na memória do mod. |
| 2026-09-10 | **Modelo de 3 papéis adicionado (§1.1), a pedido do usuário, resolvendo o risco central da pendência P-2.2.** Autoridade de clima desacoplada de `FikaBackendUtils.IsServer` — em raid Headless, o papel `Source` (quem transmite, lendo a própria `WeatherCurve`) vai para o cliente com `FikaBackendUtils.IsHeadlessRequester == true` (quem iniciou a raid), e o processo Headless vira `Relay` (só retransmite, nunca lê o próprio `WeatherController`/`Class444`, evitando os componentes destruídos por `Fika.Headless.Patches.DestroyGraphics`). §2/§5.2/§5.4/§6/§7/§9 atualizados. Corrigido também um bug de namespace pré-existente nos stubs (`using LiteNetLib;`/`using LiteNetLib.Utils;` → `using Fika.Core.Networking.LiteNetLib;`/`.Utils` — a cópia vendorizada do FIKA usa esses namespaces, não o pacote NuGet original), não pego na review 01. |
| 2026-09-10 | Review 02 (autoiniciada, 2 pontos): PA-02-01 🔴 — `Role = Source` não registrava handler de recepção, e o eco do `Relay` de volta pro `Source` causaria `ParseException` na máquina de quem iniciou a raid; corrigido fazendo `Source` registrar o mesmo handler que `Receiver` (§5.2). PA-02-02 🟢 — comentário adicionado esclarecendo a janela evento-antes-da-sessão-existir (coberta pelo polling). 0 bloqueadores restantes. |
