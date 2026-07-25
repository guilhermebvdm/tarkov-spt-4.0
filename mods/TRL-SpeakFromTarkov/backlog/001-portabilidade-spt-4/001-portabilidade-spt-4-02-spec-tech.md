# 001 — portabilidade-spt-4 · Spec Técnica

**Mod:** TRL-ImmersiveVoip
**Spec funcional:** [001-portabilidade-spt-4-01-spec.md](001-portabilidade-spt-4-01-spec.md)
**Criado:** 2026-07-16

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

## 1. Estratégia

O mod atual envia dados codificados de voz via `UdpClient` para `127.0.0.1` de forma fixa. Para o Fika 4.0.x (que roda a lógica de multiplayer do SPT), nós precisamos rotear os arrays codificados em Opus pelo sistema de rede do Fika, permitindo que outros jogadores cooperativos recebam e toquem os sons (distribuindo o array de bytes pela partida).
Ao invés de reinventar a roda com o `UdpClient`, aproveitaremos o `Singleton<IFikaNetworkManager>.Instance` e a API do Fika (baseada no LiteNetLib) para envio de dados e broadcast pelo servidor, seja através da criação de um pacote genérico (`SendGenericPacket` / `RegisterPacket`) ou de um pacote de payload `INetSerializable`.
O Fika tem sua própria implementação de voz embutida (baseada na engine de VOIP Dissonance). Reutilizar o pipeline do Dissonance (`SendVOIPData`) com nossos pacotes Opus corromperia o sistema. Assim, usaremos uma rota separada para transmissão dos nossos bytes, e aplicaremos um patch no Fika para mutar a inicialização do Dissonance caso o jogador prefira usar apenas a engine do ImmersiveVoip.

## 2. Pontos de patch e Interações

| Alvo (Assembly / Fika) | Tipo | Motivo |
|---|---|---|
| `IFikaNetworkManager.RegisterPacket<T>` | API | Registrar um tipo de pacote estruturado (ex: `SftAudioPacket`) que os clientes recebam sem interferir no Dissonance nativo do jogo. |
| `IFikaNetworkManager.SendData` ou similar | API | Envio do Array de bytes (Opus) ao servidor, com flag de broadcast configurada pelo servidor ao repassar. |
| `Fika.Core.Networking.VOIP.FikaCommsNetwork` (ou assemelhados) | Patch/Disable | Desativar (via patch ou reflection) a inicialização da rede do Dissonance se a config `DisableFikaVOIP` estiver `true`. |

## 3. Novas propriedades F12 (BepInEx)

(Mesmas de `PROPRIEDADES.md`, serão mantidas sem alterações ou adições).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/NetworkManager.cs` | MODIFICAR | Substituir o envio UdpClient hardcoded pela nova lógica baseada no `FikaNetworkManager`. |
| `modded/Packets/SftAudioPacket.cs` | CRIAR | Definição do pacote customizado para tráfego via LiteNetLib do Fika (`INetSerializable`). |
| `modded/VOIPPlugin.cs` | MODIFICAR | Ajustar os atributos `[BepInDependency]` para as guids corretas do novo ecossistema Fika 4.0. |
| `modded/GameSessionPatcher.cs` | MODIFICAR | Refatorar referências antigas (ex: `ActiveHealthController.Kill`) que possam ter mudado de namespace no Assembly-CSharp. |

## 5. Stubs de código

```csharp
// modded/Packets/SftAudioPacket.cs
using Fika.Core.Networking;
using LiteNetLib.Utils;
using System;

namespace SpeakFromTarkov.Packets
{
    // Pacote que encapsula o frame Opus. Deve implementar INetSerializable.
    public struct SftAudioPacket : INetSerializable
    {
        public byte Channel;
        public byte[] AudioData;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Channel);
            writer.PutBytesWithLength(AudioData);
        }

        public void Deserialize(NetDataReader reader)
        {
            Channel = reader.GetByte();
            AudioData = reader.GetBytesWithLength();
        }
    }
}
```

```csharp
// modded/NetworkManager.cs (Trecho)
using Fika.Core.Networking;
using Comfort.Common;
using SpeakFromTarkov.Packets;

namespace SpeakFromTarkov
{
    internal class NetworkManager : MonoBehaviour
    {
        // ...
        public void BroadcastVoipData(byte[] data, byte channel)
        {
            if (!IsSessionActive) return;

            // Criar o pacote INetSerializable e usar a API do Fika
            SftAudioPacket packet = new SftAudioPacket { Channel = channel, AudioData = data };
            
            // Broadcast garante que o servidor envia pra todo mundo. 
            // ref: references/fika-plugin/Fika.Core/Networking/IFikaNetworkManager.cs:72
            Singleton<IFikaNetworkManager>.Instance.SendData(ref packet, LiteNetLib.DeliveryMethod.Sequenced, broadcast: true);
        }
    }
}
```

## 6. Fluxo de dados

```
[A] SpeakFromTarkov(Microfone + Concentus Opus) 
  → [B] NetworkManager.BroadcastVoipData(SftAudioPacket) 
  → [C] FikaNetworkManager.SendData 
  → [D] Fika Server (Host/Headless) repassa (broadcast=true) 
  → [E] Clientes (FikaNetworkManager) invocam Action registrada em RegisterPacket
  → [F] Decodificação e Áudio 3D (AudioSource)
```

O cliente envia o `SftAudioPacket` empacotando o frame Opus usando a camada do Fika. O servidor simplesmente repassa. No destino final, o pacote aciona nosso delegate registrado, efetuando o Decode Opus de volta a PCM float e chamando a engine de som 3D da Unity para emitir pelo local correspondente do jogador emissor.

## 7. Riscos e dependências

- **Compatibilidade com pacotes Fika:** A API do LiteNetLib usada pelo Fika restringe o `SendData` a tipos `INetSerializable`. É necessário entender como registrar tipos customizados no Fika, o que é feito por `Singleton<IFikaNetworkManager>.Instance.RegisterPacket<SftAudioPacket>(OnReceiveVoip)`.
- **Roteamento Servidor:** Como não temos acesso para editar o FikaServer base/Headless diretamente, garantir que `broadcast: true` repasse adequadamente os bytes do pacote de um plugin de terceiro.

## 8. Checklist de implementação

- [ ] Mapear as assinaturas corretas para os pacotes no `IFikaNetworkManager`.
- [ ] Atualizar atributos e guid de dependência do Fika plugin.
- [ ] Implementar a struct `SftAudioPacket`.
- [ ] Refatorar os Patches de Evento do jogador (PlayerInit, OnDead).
- [ ] Testar se pacotes `INetSerializable` enviados pelo cliente Fika sofrem relay autônomo pelo Host ou Headless server se `broadcast=true`.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | ✅ | Já limpa sessão via hook em `GameWorld.Dispose`. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Verifica `IsYourPlayer` em `Player.Init`. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | Avaliação dos métodos descompilados será validada em fase de build. |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | N/A | |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | N/A | |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-16 | Spec técnica inicial baseada na análise da API de rede do FIKA plugin. |
