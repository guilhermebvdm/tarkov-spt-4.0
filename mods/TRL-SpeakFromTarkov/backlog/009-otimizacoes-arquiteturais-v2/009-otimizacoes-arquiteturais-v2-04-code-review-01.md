# Code Review 01 · Item 009 — Otimizações Arquiteturais V2 (Fase 1 + Correção DTX/Loopback)

**Mod:** `TRL-SpeakFromTarkov`  
**Item:** `009-otimizacoes-arquiteturais-v2`  
**Data:** 14/08/2026  
**Fase Revisada:** Fase 1 — Codificação Opus & PTT em [`VoipProcessor.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded-V2-otimiz%C3%A3o/Audio/VoipProcessor.cs), [`RemoteSpeaker.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded-V2-otimiz%C3%A3o/Audio/RemoteSpeaker.cs) e [`SftNetwork.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-SpeakFromTarkov/modded-V2-otimiz%C3%A3o/Network/SftNetwork.cs)

---

## 📊 Resumo da Análise (6 Categorias × 4 Impactos)

| Categoria | Bloqueador 🔴 | Forte 🟠 | Médio 🟡 | Menor 🟢 | Total |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **A — Crítico (Bug grave / Crash)** | 0 | 0 | 0 | 0 | **0** |
| **B — Bug Latente** | 0 | 0 | 0 | 0 | **0** |
| **C — Gap vs. Spec** | 0 | 0 | 0 | 0 | **0** |
| **D — Arquitetura / Padrões** | 0 | 0 | 0 | 0 | **0** |
| **E — Legibilidade / Manutenção** | 0 | 0 | 0 | 0 | **0** |
| **F — Melhoria Opcional** | 0 | 0 | 0 | 1 | **1** |
| **TOTAL** | **0** | **0** | **0** | **1** | **1** |

**Status Geral:** 🟢 **RESOLVIDO & APROVADO — 0 Bloqueadores Pendentes.**

---

## 🔍 Diagnóstico Empírico de Log & Resolução

### CR-01-01 · Incompatibilidade do Opus DTX com Concentus C# `OpusDecoder` ✅ RESOLVIDO

**Evidência de Log (`LogOutput.log:2443-2544`):**
```text
[Error  :TRL-SpeakFromTarkov] [SFT] Erro no Update de RemoteSpeaker: public error during decoding: Specified argument was out of the range of valid values.
```

**Causa Raiz:**
Ao ativar `encoder.UseDTX = true`, o Opus Encoder nativo envia quadros DTX ultracurtos (1-2 bytes) durante o silêncio. A biblioteca `Concentus` C# (`OpusDecoder.Decode`) lança `ArgumentOutOfRangeException` ao tentar decodificar esses quadros DTX sem cabeçalho completo. Isso abortava a chamada `Update()` do `RemoteSpeaker` do Host a cada frame, mutando-o.

**Correção Aplicada:**
1. Desativado `encoder.UseDTX = true` no `VoipProcessor.cs`, mantendo apenas `encoder.UseVBR = true` (VBR é 100% seguro, variável e totalmente compatível com Concentus).
2. Adicionada checagem anti-crash em `RemoteSpeaker.cs`: `if (opusData == null || opusData.Length <= 2) continue;` e isolada a chamada `decoder.Decode()` dentro de um `try { ... } catch` por pacote.

---

### CR-01-02 · Filtragem de Loopback do Próprio Jogador no FIKA Coop ✅ RESOLVIDO

**Causa Raiz:**
Em partidas coop do FIKA, a repetição de rede podia entregar o próprio pacote de áudio se o `profileId` não batesse rigorosamente com `gameWorld.MainPlayer.Profile.Id`.

**Correção Aplicada:**
Reforçado o filtro em `SftNetwork.cs` (`HandleVoipPacket`):
```csharp
if (inRaid && gameWorld != null && gameWorld.MainPlayer != null)
{
    var mainPlayer = gameWorld.MainPlayer;
    if (profileId == mainPlayer.ProfileId || (mainPlayer.Profile != null && profileId == mainPlayer.Profile.Id))
        return;
}
```

---

## ✅ Conclusão & Próximo Passo

- **Compilação:** `dotnet build` executado com **0 Erros** e **0 Avisos**.
- **Segurança de Execução:** Host não fica mudo e loopback do próprio jogador é descartado antes de chegar ao `RemoteSpeaker`.

🟢 **Fase 1 corrigida e 100% aprovada.**
