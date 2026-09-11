# 011 — Threading e Oclusão de Voz · Review Técnica 01

**Mod:** TRL-SpeakFromTarkov
**Spec técnica revisada:** [011-threading-oclusao-voz-02-spec-tech.md](011-threading-oclusao-voz-02-spec-tech.md)
**Data:** 2026-09-09

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🔴 Bloqueador | Stub de `Apply()` omite `ApplyAGC`/`ApplyLimiter` — regressão garantida se implementado literalmente | ✅ Resolvido |
| PA-01-02 | A — Gap | 🟢 Menor | `ApplyRNNoise`/`RNNoiseVADThreshold`/`RNNoiseGateHold` parecem já não ter efeito hoje — fora do escopo, mas vale registrar | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · ✅ Resolvido em 2026-09-09 · C — Erro de Lógica · 🔴 Bloqueador

**Stub de `Apply()` omite `ApplyAGC`/`ApplyLimiter` — regressão garantida se implementado literalmente**

**Problema:** O stub da seção 5 (`Audio/AudioFilter.cs`) reescreve `Apply(float[] buffer)` como:
```csharp
public void Apply(float[] buffer)
{
    var config = _config;
    if (_rnAvailable && config.UseRNNoise) { ApplyHPF(buffer); if (config.LpfAlpha > 0) ApplyLPF(buffer, config.LpfAlpha); ApplyRNNoise(buffer, config); }
    else { ApplyFallback(buffer, config); }
}
```
Mas o `Apply()` **real e atual** (`Audio/AudioFilter.cs:134-155`) tem mais duas etapas depois desse bloco:
```csharp
// AGC é aplicado exclusivamente em canais 2D (menu ou spectator) para não distorcer proximidade 3D
if (EnableAGC && Is2DChannel) { ApplyAGC(buffer); }
// O limiter é a última barreira de proteção de áudio
if (EnableLimiter) ApplyLimiter(buffer);
```
(`AudioFilter.cs:147-154`). O stub da spec técnica simplesmente **não tem essas 8 linhas**. Também não mostra a assinatura de `ApplyFallback`/`ApplyNoiseGate` recebendo `config` — só `ApplyNoiseGate` de fato lê `OpenThreshold`/`CloseThreshold`/`HoldTime` (`AudioFilter.cs:300` confirmado nesta review), então é ela (não `ApplyFallback` em si) que precisa do parâmetro.

**Por que importa:** Se implementado copiando o stub literalmente, o AGC (ganho automático em canais 2D) e o Limiter (proteção contra clipping — "última barreira", conforme o próprio comentário do código atual) **parariam de rodar silenciosamente**. Isso é uma regressão de áudio real e perceptível (risco de estouro/clipping sem o limiter), não um detalhe cosmético.

**Sugestão:** Corrigir o stub da seção 5 para:
```csharp
public void Apply(float[] buffer)
{
    var config = _config;

    if (_rnAvailable && config.UseRNNoise)
    {
        ApplyHPF(buffer);
        if (config.LpfAlpha > 0) ApplyLPF(buffer, config.LpfAlpha);
        ApplyRNNoise(buffer); // não lê nenhum campo de config hoje — ver PA-01-02
    }
    else
    {
        ApplyFallback(buffer, config);
    }

    if (config.EnableAGC && config.Is2DChannel)
    {
        ApplyAGC(buffer); // não lê config — sem mudança de assinatura
    }

    if (config.EnableLimiter) ApplyLimiter(buffer); // não lê config — sem mudança de assinatura
}

private void ApplyFallback(float[] buf, NoiseGateConfig config)
{
    ApplyHPF(buf);
    ApplyNoiseGate(buf, config); // ApplyNoiseGate é quem de fato lê OpenThreshold/CloseThreshold/HoldTime
}
```
E remover `ApplyRNNoise(buffer, config)`/`ApplyLPF`'s uso de `config` onde não é necessário (`ApplyLPF` já estava correto — ele SÓ precisa de `config.LpfAlpha`, não do objeto inteiro; manter como no stub original, só corrigindo `ApplyRNNoise` de volta pra assinatura sem `config`, já que ela não lê nenhum campo dele).

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Stub de `Apply()`/`ApplyFallback` corrigido na spec técnica exatamente conforme a sugestão; checklist de implementação (§8) ajustado pra listar corretamente quais métodos mudam de assinatura.

---

### PA-01-02 · ✅ Resolvido em 2026-09-09 · A — Gap · 🟢 Menor

**`RNNoiseVADThreshold`/`RNNoiseGateHold` parecem já não ter efeito hoje — fora do escopo, mas vale registrar**

**Problema:** Ao verificar `ApplyRNNoise` (`AudioFilter.cs:158-229`) pra confirmar a correção do PA-01-01, notei que esse método **não lê `RNNoiseVADThreshold` nem `RNNoiseGateHold`** — ele usa constantes hardcoded (`0.0003f`, `0.20f`, `0.01f`) pra decidir quando pular o processamento neural e quando silenciar o buffer. As duas propriedades são escritas todo frame em `MicrophoneCapturer.cs:241-242` (por isso entraram no `NoiseGateConfig` da spec técnica), mas não encontrei nenhum outro lugar em `AudioFilter.cs` que as leia.

**Por que importa:** Não é um problema causado por este item — é um achado incidental de código morto (parecido com os já catalogados no item `013-limpeza-codigo-morto-polimento/`). Não bloqueia a correção de threading (o `NoiseGateConfig` ainda resolve a race condition dos campos, mesmo que 2 deles não sejam lidos hoje), mas vale registrar pra não gerar confusão futura ("por que mudar esse limiar no F12 não faz nada?").

**Sugestão:** Não é necessário mudar nada nesta spec técnica pra resolver este ponto — só adicionar uma nota na seção 7 (Riscos): *"`RNNoiseVADThreshold`/`RNNoiseGateHold` são incluídos no `NoiseGateConfig` pela mesma razão de threading dos demais campos, mas aparentam não ter consumidor ativo em `ApplyRNNoise` hoje — possível candidato a achado de código morto numa auditoria futura, fora do escopo deste item."* Se preferir, abrir um item de backlog novo pra investigar/remover — não obrigatório agora.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Nota adicionada à seção 7 (Riscos) da spec técnica. Nenhum item de backlog novo aberto — fica como nota registrada, sem ação adicional necessária agora.

---

## Memória consultada

Snapshot de `mods/TRL-SpeakFromTarkov/memory/sessions.md` (v1.5.3, sem pendências 🔴/🟡 relacionadas a este item).
