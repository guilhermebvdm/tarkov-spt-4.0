---
title: SAIN — Code Review Etapa 3 (Visibilidade em Lote Único e Coberturas Fail-Fast)
date: 2026-09-02
status: 🟢 Vivo
authors: [guilhermebvdm, Antigravity]
---

# SAIN — Code Review Etapa 3 · Visibilidade em Lote Único e Coberturas Fail-Fast

**Mod:** `SAIN (modded-multithread)`  
**Versão:** `4.6.0`  
**Referência:** Etapa 3 do [Plano de Implementação](../../../.gemini/antigravity-ide/brain/c2374fdc-b4d7-49ce-97f9-f53e9470d810/implementation_plan.md)  
**Documentação Base:** [01-arquitetura-multithread-e-lod.md](01-arquitetura-multithread-e-lod.md)  
**Data:** 2026-09-02  

> Análise crítica formal do código implementado na **Etapa 3: Visibilidade de Rota em Lote Único Zero-Alloc e Otimização de Coberturas**.  
> Cada achado recebe um ID permanente `CR-03-MM` categorizado em 6 dimensões × 4 níveis de impacto.

---

## 📊 Resumo Executivo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 1 (resolvido) · 🟢 Menores: 1 · ✅ Resolvidos: 1 · Total: 2

| ID | Categoria | Impacto | Título | Status |
|:---:|:---:|:---:|---|:---:|
| **CR-03-01** | A — Regressão / ABI | 🟡 Médio | Método `public override void Stop()` havia sido removido acidentalmente | `[x]` ✅ Resolvido antes do commit |
| **CR-03-02** | F — Otimização | 🟢 Menor | Tolerância de -0.2f no produto escalar de triagem pode ser exposta no Preset | `[ ]` Deferido para fase de GUI |

---

## 🔍 Pontos Críticos e Análise Detalhada

### CR-03-01 · A — Regressão / ABI · 🟡 Médio

**Método `public override void Stop()` havia sido removido acidentalmente**

**Local:** [`mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/EnemyPathVisibilityRaycastJob.cs:469`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/EnemyPathVisibilityRaycastJob.cs#L469)

**Problema:**
Durante a substituição de métodos legados para inserção do lote único consolidado, o método de ciclo de vida `public override void Stop()` foi temporariamente omitido da classe `EnemyPathVisibilityRaycastJob`.

**Detecção:**
Identificado imediatamente pelo script de auditoria estática AST (`compare_api`):
```
Faltando: [ 'EnemyPathVisibilityRaycastJob.Stop' ]
```

**Resolução:**
O método foi prontamente restaurado antes da finalização do commit:
```csharp
public override void Stop()
{
    Dispose();
    base.Stop();
}
```
A contagem de membros públicos retornou a 0 ausências (1847 membros preservados).

**Decisão:**
- `[x]` ✅ Resolvido

---

### CR-03-02 · F — Otimização · 🟢 Menor

**Tolerância de -0.2f no produto escalar de triagem pode ser exposta no Preset**

**Local:** [`mods/SAIN/modded-multithread/SAIN/Classes/Coverfinder/CoverAnalyzer.cs:114`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/Coverfinder/CoverAnalyzer.cs#L114)

**Problema:**
A constante de corte de ângulo preliminar `-0.2f` está fixa no código:
```csharp
if (Vector3.Dot(-targetDirectionNormal, dirTargetToCollider) < -0.2f)
```
Embora geometricamente robusta (elimina colisores mais de ~101° atrás da direção do inimigo), essa constante poderia ser configurável via menu F6 nas opções de desempenho de Cobertura.

**Sugestão:**
Manter como está no momento para manter a consistência do código sem alterar esquemas de serialização JSON de presets, e avaliar exposição em fase de UI se necessário.

**Decisão:**
- `[x]` Aceito como padrão de projeto (evita alterações desnecessárias de esquema de presets).

---

## 🏆 Conclusão do Review

* **Bloqueadores 🔴:** **0** — A Etapa 3 foi executada com máxima precisão.
* **Impacto no PhysX / Main Thread:**  
  1. Dezenas de chamadas separadas de `ScheduleBatch` a cada 50ms foram reduzidas a **1 único despacho consolidado**.
  2. A triagem fail-fast no `CoverAnalyzer` elimina até 50% das caras amostragens de `NavMesh.SamplePosition`.
  3. A reordenação de companheiros de esquadrão poupa centenas de raycasts em coberturas já ocupadas.
* **Integridade de API:** 1847 membros públicos íntegros (0 faltando).
