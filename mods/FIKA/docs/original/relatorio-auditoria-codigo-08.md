---
title: "Relatório de Auditoria Técnica de Código — FIKA (Review 08: Headless Dedicated Client & Asset Nuker)"
date: 2026-09-02
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — FIKA (Review 08: Headless Dedicated Client & Asset Nuker)

## 1. Resumo Executivo da Auditoria

Este relatório consolida o diagnóstico estático aprofundado e minucioso da **Partição 8 (Cliente Dedicado Headless, Asset Nuker, Otimizações de CPU/VRAM, Culling de Renderers/Áudio e Patches de Automação de Servidor)** do código original do mod **FIKA**, inspecionando ~5.700 linhas de código C# distribuídas nos módulos `Fika.Headless/`, `Fika.Headless/Patches/` e `Fika.Headless.AssetNuker/`.

| Severidade | Quantidade | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 1 | Uso de `async void RetryConnect` com `Task.Run` e falta de descarte de delegates de eventos no `HeadlessWebSocket`. |
| 🟠 **Alto** | 1 | Chamada síncrona bloqueante `.Await()` em `Resources.UnloadUnusedAssets()` no `Update()` de `FikaHeadlessPlugin`, travando a Main Thread. |
| 🟡 **Médio** | 2 | Ausência de descarte (`Dispose`) de buffer nativo `Image<Bgra32>` no `AssetNuker` e patches legados de DLSS/Reflex ativos no modo headless. |
| 💡 **Otimização** | 1 | Trava estrita de `Application.targetFrameRate` acoplada ao tickrate de simulação física para economia de CPU. |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|:---:|---|---|---|
| `AUD-08-01` | 🔴 Crítico | [`HeadlessWebSocket.cs:L122, L126`](../../original/Fika-Headless/Fika.Headless/Classes/HeadlessWebSocket.cs#L126) | Concorrência / Crash | `RetryConnect` implementado como `async void` chamado via `Task.Run`, com risco de exceção não tratada na reconexão. |
| `AUD-08-02` | 🟠 Alto | [`FikaHeadlessPlugin.cs:L159`](../../original/Fika-Headless/Fika.Headless/FikaHeadlessPlugin.cs#L159) | Thread Blocking | `Resources.UnloadUnusedAssets().Await()` bloqueia a Main Thread durante a coleta periódica de memória. |
| `AUD-08-03` | 🟡 Médio | [`Program.cs:L24, L94`](../../original/Fika-Headless/Fika.Headless.AssetNuker/Program.cs#L94) | Resource Leak | `_replacementImage` (`Image<Bgra32>`) não é descartada (`.Dispose()`), retendo memória não gerenciada. |
| `AUD-08-04` | 🟡 Médio | [`Fika.Headless/Patches/DLSS/`](../../original/Fika-Headless/Fika.Headless/Patches/DLSS/) | Código Morto | Patches de DLSS e Reflex registrados mesmo sem GPU/renderizador no modo headless. |
| `AUD-08-05` | 💡 Otimização | [`FikaHeadlessPlugin.cs:L105`](../../original/Fika-Headless/Fika.Headless/FikaHeadlessPlugin.cs#L105) | Desempenho | Configuração estrita de `Application.targetFrameRate = 60` para estabilidade térmica de CPU do servidor dedicado. |

---

## 3. Detalhamento dos Achados

### AUD-08-01 · `async void` e Falta de Descarte no `HeadlessWebSocket`
- **Severidade:** 🔴 Crítico
- **Localização:** [`HeadlessWebSocket.cs:L122, L126-138`](../../original/Fika-Headless/Fika.Headless/Classes/HeadlessWebSocket.cs#L126)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md:AP-01`](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O método `RetryConnect` possui retorno `async void` e é disparado com `Task.Run(RetryConnect)`. Qualquer exceção disparada durante a tentativa de reconexão com o servidor de matchmaking escapa sem ser observada pela ThreadPool do .NET, com risco de encerramento do processo. Além disso, a classe não implementa `IDisposable` e mantém delegates vinculados ao `_webSocket`.
- **Impacto Técnico Real:** Instabilidade em servidores dedicados autônomos caso o servidor backend sofra reinicialização rápida.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Converter `RetryConnect` para retornar `async Task`, aguardar adequadamente com captura de exceções `try-catch`, e implementar `IDisposable` no `HeadlessWebSocket`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-08-02 · Bloqueio Síncrono da Main Thread em `Resources.UnloadUnusedAssets().Await()`
- **Severidade:** 🟠 Alto
- **Localização:** [`FikaHeadlessPlugin.cs:L159`](../../original/Fika-Headless/Fika.Headless/FikaHeadlessPlugin.cs#L159)
- **Causa Raiz:** No loop de `Update()`, a rotina periódica de GC executa `Resources.UnloadUnusedAssets().Await()`, forçando uma espera síncrona bloqueante sobre uma operação assíncrona nativa do Unity.
- **Impacto Técnico Real:** Queda abrupta de responsividade e congelamento de frame no servidor dedicado durante a limpeza de memória fora de raid.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Executar o descarregamento de assets de forma assíncrona sem bloquear a Main Thread ou disparar via coroutine `yield return Resources.UnloadUnusedAssets();`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-08-03 · Retenção de Memória Nativa em `AssetNuker` (`Image<Bgra32>`)
- **Severidade:** 🟡 Médio
- **Localização:** [`Program.cs:L24, L94`](../../original/Fika-Headless/Fika.Headless.AssetNuker/Program.cs#L94)
- **Causa Raiz:** A imagem substituta de textura `_replacementImage` é instanciada via `Image.Load<Bgra32>(...)` mas nunca descartada no encerramento da aplicação CLI.
- **Impacto Técnico Real:** Vazamento de buffers nativos de imagem não gerenciados da biblioteca ImageSharp.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Chamar `_replacementImage?.Dispose();` no encerramento de `Program.Main`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-08-04 · Patches Desnecessários de DLSS e Reflex em Modo Headless
- **Severidade:** 🟡 Médio
- **Localização:** [`Fika.Headless/Patches/DLSS/`](../../original/Fika-Headless/Fika.Headless/Patches/DLSS/)
- **Causa Raiz:** O plugin aplica patches para interceptar DLSS e Reflex que não têm utilidade prática quando a renderização gráfica está completamente eliminada.
- **Impacto Técnico Real:** Overhead inútil de patching no bootstrap do cliente dedicado.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Desativar ou remover esses patches do carregamento headless.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-08-05 · Otimização de TargetFrameRate no Servidor Dedicado
- **Severidade:** 💡 Otimização
- **Localização:** [`FikaHeadlessPlugin.cs:L105`](../../original/Fika-Headless/Fika.Headless/FikaHeadlessPlugin.cs#L105)
- **Causa Raiz:** Sem display, o loop da Unity pode consumir ciclos de CPU ociosos se a taxa de frames não estiver limitada.
- **Impacto Técnico Real:** Aquecimento e consumo desnecessário de CPU em servidores dedicados.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Definir explicitamente `Application.targetFrameRate = 60;` e `QualitySettings.vSyncCount = 0;` na inicialização do Headless.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Salvaguarda de Contratos Públicos e Compatibilidade com Mods Terceiros

Para assegurar 100% de compatibilidade com orquestradores e lançadores externos de servidores dedicados:

| Símbolo Público | Consumidores Externos | Diretriz Estrita |
|---|---|---|
| `FikaHeadlessPlugin.HeadlessVersion` | *Orchestrator APIs*, *FikaServer* | Preservar constante de versão e identificadores BepInEx. |
| `HeadlessWebSocket` | *Server Matchmaking* | Preservar protocolo de mensagens WebSocket `/fika/headless/client`. |
| `FikaHeadlessTransitController` | *Dedicated GameMode* | Preservar assinaturas de controle de trânsito. |

---

## 5. Validação Automática

```bash
bash .agents/hooks/validate-doc-header.sh mods/FIKA/docs/original/relatorio-auditoria-codigo-08.md
```
