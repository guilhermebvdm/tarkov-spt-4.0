# 001 — Perfis customizados temáticos · Review Técnica 01

**Mod:** RZCustomProfiles
**Spec técnica revisada:** [001-custom-profiles-02-spec-tech.md](001-custom-profiles-02-spec-tech.md)
**Data:** 2026-05-17

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 7 · Total: 7

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Lógica | 🔴 Bloqueador | Hideout: estações temáticas com pré-requisitos não resolvidos | ✅ Resolvido 2026-05-17 |
| PA-01-02 | A — Gap | 🟡 Importante | Skeleton JSONC tem `Items` apenas com placeholders — falta exemplo completo | ✅ Resolvido 2026-05-17 |
| PA-01-03 | B — Edge Case | 🟡 Importante | Stack limits do EFT podem quebrar consolidação via `Count` | ✅ Resolvido 2026-05-17 |
| PA-01-04 | A — Gap | 🟡 Importante | Validação prévia do `anchor-items.json` contra TPLs realmente usados no loadout | ✅ Resolvido 2026-05-17 |
| PA-01-05 | C — Lógica | 🟡 Importante | Checklist usa `file -i` (Unix) num ambiente Windows | ✅ Resolvido 2026-05-17 |
| PA-01-06 | A — Gap | 🟢 Menor | Limite numérico para `Description` não definido | ✅ Resolvido 2026-05-17 |
| PA-01-07 | B — Edge Case | 🟢 Menor | Comportamento se BaseProfile ≠ 0 não documentado | ✅ Resolvido 2026-05-17 |

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

### PA-01-01 · C — Lógica · ✅ Resolvido em 2026-05-17

**Hideout: 4 das 10 classes têm estação temática com pré-requisitos não resolvidos**

**Resolução:** opção (B) aceita — adotada restrição de design "apenas estações sem pré-requisitos". As 4 classes afetadas foram remapeadas: Caçador → `Heating`, Batedor → `Security`, Saqueador → `Security`, Gerente → `Generator` + `Heating`. Lista de estações elegíveis e racional registrados no [planejamento §Hideout inicial](./001-custom-profiles-00-planejamento.md). Tabelas da spec técnica (§4 Arquivos e §Composições por classe) e §7 Riscos atualizados.

**Problema:** A spec técnica define `HideoutStartingLevels` por classe (§5 Skeleton, §Composições por classe) com base no [planejamento §Hideout inicial](./001-custom-profiles-00-planejamento.md). Quatro classes definem estações que **exigem outras estações já construídas em vanilla EFT 0.16.x** [fonte externa: [playerassist.com — Hideout Guide](https://playerassist.com/escape-from-tarkov-hideout-guide/)]:

| Classe | Estação proposta | Pré-requisito para L1 (não setado) |
|--------|------------------|-----------------------------------|
| Caçador | `ShootingRange: 1` | `Illumination: 2` |
| Batedor | `IntelligenceCenter: 1` | `Security: 2` + `Vents: 2` |
| Saqueador | `ScavCase: 1` | `IntelligenceCenter: 2` (→ que exige Security 2 + Vents 2) |
| Gerente de Operações | `IntelligenceCenter: 1` (+ `Generator: 1`) | `Security: 2` + `Vents: 2` |

As demais estações temáticas (`MedStation`, `Workbench`, `RestSpace`, `WaterCollector`, `Generator`) **não têm pré-requisitos** e estão OK.

**Por que importa:** O comportamento ao setar uma estação L1 sem seus pré-requisitos é indefinido — três cenários possíveis, todos ruins:
1. A UI do Hideout quebra (linhas de dependência inconsistentes, botões de upgrade não aparecem).
2. O `RZCustomProfiles.dll` silenciosamente descarta o valor (estação fica em 0 — bug silencioso, critério de aceite falha).
3. A estação aparece em L1 "orfã" no jogo, mas o jogador nunca consegue evoluir porque a árvore de dependências está inconsistente.

Para o `Gerente de Operações` (que já recebe 2 estações), a situação é especialmente ruim — o conceito da classe é "hideout-focused", e a estação core (IntelligenceCenter) é exatamente a que mais quebra.

**Sugestão:** Três caminhos possíveis (escolher um):

- **(A) Pré-setar toda a cadeia de dependências.** Ex: Batedor recebe `IntelligenceCenter: 1` + `Security: 2` + `Vents: 2` + `Generator: 1`. Mantém o conceito original mas o JSON cresce, e classes ficam com 4-5 estações destravadas em vez de 1 — desbalanceia vs classes simples (MedStation, Workbench).
- **(B) Trocar as 4 estações problemáticas por alternativas sem pré-requisitos.** Sugestões: Caçador → `RestSpace: 1` (recuperação pós-tiro longo) ou `Workbench: 1`; Batedor → `WaterCollector: 1` (sustento de recon longo) ou `RestSpace: 1`; Saqueador → `MedStation: 1` ou `Workbench: 1`; Gerente → `Generator: 1` + `Workbench: 1` (mantém 2 estações sem cadeia).
- **(C) Manter o planejamento e validar empiricamente.** Setar como está, testar em ambiente SPT, e ajustar caso falhe. Risco de retrabalho mas confirma comportamento real do mod.

Recomendação: **opção (B)** — atualizar o [planejamento §Hideout inicial](./001-custom-profiles-00-planejamento.md) com as estações revistas, propagar para a spec funcional, técnica e tabelas.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (B — trocar 4 estações)
- `[ ]` Caminho alternativo: _________________

---

### PA-01-02 · A — Gap · ✅ Resolvido em 2026-05-17

**Skeleton JSONC tem `Items` apenas com placeholders — implementador precisa inferir estrutura completa**

**Resolução:** caminho alternativo — em vez de inflar o skeleton com 90+ linhas manuais (sujeitas a erro), adotada **geração mecânica via script**. Spec técnica §5.1 nova ("Geração mecânica via script") aponta para o existente [scripts/build-loadouts.js](../../scripts/build-loadouts.js), que já contém as recipes por classe. Implementação estende esse script para emitir `.json` por classe aplicando a regra de stack (PA-01-03). Checklist atualizado.

**Problema:** O skeleton em [§5](001-custom-profiles-02-spec-tech.md) mostra `AdditionalStartingItems.Items` com a baseline universal preenchida (9 itens), mas os blocos de tema, primary e backup×3 estão apenas comentados `// ...`. O implementador precisa abrir o planejamento, ler 4 tabelas (tema + primary + 3 backups idênticos), resolver TPLs no anchor JSON e consolidar — sem garantia de que a saída casa com a intenção.

**Por que importa:** Sem um exemplo concreto completo de pelo menos UMA classe, há ambiguidade sobre:
- Ordem dos itens (importa para review? para parsing?)
- Como consolidar `IFAK` que aparece em tema (2x) + primary (1x) + backup×3 (1x cada = 3x) = 6 IFAKs totais → vira `Count: 6`, mas precisa pegar de 4 lugares.
- Comentários inline são desejáveis ou poluem?
- Itens monetários (`ROUBLES`) com `Count: 100000` são números (`100000`) ou strings (`"100000"`)?

**Sugestão:** Expandir o skeleton para mostrar o **Médico de Combate completo** (uma classe inteira como exemplo canônico), com:
- Cabeçalho de comentário separando cada bloco (`// === Baseline ===`, `// === Tema ===`, `// === Primary ===`, `// === Backup ×3 (consolidado) ===`)
- Todos os ~30-40 itens com `Tpl` resolvido e `Count` consolidado
- Linha de soma comentada ao fim: `// Total: ~1.977.163 ₽`

Os outros 9 arquivos derivam da mesma estrutura, então 1 exemplo completo é suficiente.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (expandir Médico de Combate completo)
- `[ ]` Caminho alternativo: _________________

---

### PA-01-03 · B — Edge Case · ✅ Resolvido em 2026-05-17

**Stack limits do EFT podem quebrar consolidação via `Count`**

**Resolução:** confirmado empiricamente via [tools/tarkov-itemdb/cache/spt-raw.json](../../../../tools/tarkov-itemdb/cache/spt-raw.json) que itens críticos têm `stackMaxSize: 1` (IFAK, Salewa, MRE, Bandage, Aquamari, Crackers, Aluminum splint, Analgin, Bayonet, todas as magazines, weapons, armor) — consolidação via `Count > 1` para eles é **inviável**. Ammo tem stack 60, Roubles 1.000.000. Regra de stack stack-aware adicionada em §5 do skeleton:
- `stackMax == 1` → emitir N linhas com `Count: 1`
- `stackMax > 1` → emitir `ceil(qty/stackMax)` linhas com `Count` = stackMax
Smoke test do comportamento do mod ao receber `Count > stackMax` adicionado ao checklist como confirmação de proteção.

**Problema:** A spec técnica (§5, nota final do skeleton) recomenda consolidar itens duplicados via `Count` somado: "5 IFAKs no total → uma única entrada com `Count: 5`". Em EFT 0.16.x, **cada tipo de item tem um stack limit interno**:
- IFAK, Salewa, MRE, Aquamari, Bandages → stack 1 (não empilha)
- Munição → stack varia (geralmente 60-120 por chambra)
- Ammo box / Pack of screws / Roubles → stack alto

Se `AdditionalStartingItems.Items[].Count` for `> stack_limit` para o item, três comportamentos possíveis (não documentados):
1. Mod cria N entradas separadas (cada uma 1 unit) — funciona, ocupa N slots no stash.
2. Mod cria 1 entrada com `Count` = stack_limit + perde o restante (silently).
3. Mod aborta com erro.

**Por que importa:** Se for (2), o jogador recebe menos itens do que o planejamento prevê (ex: 5 IFAKs viram 1 IFAK). Critério de aceite de loadout falha. Isso afeta praticamente todas as classes (Médico tem 6+ IFAKs consolidados, Sanitarista do planejamento).

**Sugestão:**
- Validar empiricamente o comportamento do `AdditionalStartingItems` ao passar `Count > stack_limit` para um item não-stackável (smoke test: criar um perfil dummy com `IFAK Count: 5`, abrir stash, contar IFAKs).
- Documentar o resultado na spec técnica.
- Se for (1): manter consolidação via `Count` (mais conciso).
- Se for (2) ou (3): **NÃO consolidar** — emitir N entradas separadas no JSON. Skeleton e §5 precisam atualizar a recomendação.
- Adicionar tarefa de smoke test no checklist como precondição de geração dos 10 JSONs.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (smoke test + atualizar §5)
- `[ ]` Caminho alternativo: _________________

---

### PA-01-04 · A — Gap · ✅ Resolvido em 2026-05-17

**Validação prévia do `anchor-items.json` contra TPLs realmente usados no loadout**

**Resolução:** elevado o nível de validação. Em vez de validar só contra `anchor-items.json` (100 itens), validar contra [tools/tarkov-itemdb](../../../../tools/tarkov-itemdb/) (5630 TPLs com metadados completos). §2 Fontes da spec técnica atualizada para listar o itemdb como fonte autoritativa. Checklist atualizado com cross-check: cada ID simbólico do `build-loadouts.js` → bsgId no anchor → TPL existe no itemdb com `stackMaxSize` definido.

**Problema:** A spec aponta [anchor-items.json](../anchor-items.json) como fonte autoritativa de TPLs (§2 e §5). Confirmei via grep que mags (`MAG_AKM_30`, `MAG_PM_8`) e ammo (`AMMO_762x39_PS`, `AMMO_545x39_BS`) **estão** no anchor JSON. Mas a spec não exige validar **antes da implementação** que **todos** os IDs simbólicos referenciados nos loadouts de todas as 10 classes existem no anchor.

**Por que importa:** Se um único ID estiver faltando ou typado (ex: `MAG_AKMS_30` em vez de `MAG_AKM_30` para o AKMS do Operador Noturno), a geração do JSON quebra silenciosamente — o item simplesmente fica fora. Detectado só em playtest, depois de já ter gasto tempo gerando todos os arquivos.

**Sugestão:** Adicionar tarefa **no início** do checklist:
> Cross-check: extrair todos os IDs simbólicos das tabelas de loadout do planejamento (baseline + 10× tema + 10× primary + 10× backup), gerar lista deduplicada, e validar que cada ID existe como chave em [anchor-items.json](../anchor-items.json). Pode ser um one-liner em Node ou um script `scripts/validate-loadout-ids.js`.

Custo da tarefa: 10-15 min. Evita retrabalho de horas em playtest.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (cross-check ID + adicionar ao checklist)
- `[ ]` Caminho alternativo: _________________

---

### PA-01-05 · C — Lógica · ✅ Resolvido em 2026-05-17

**Checklist usa `file -i` que é Unix-only — falha no ambiente nativo Windows**

**Resolução:** checklist atualizado com snippet PowerShell nativo que lê os 3 primeiros bytes e detecta BOM. Comando `file -i` mantido como alternativa entre parênteses para usuários com Git Bash/WSL.

**Problema:** Em [§8 Checklist](001-custom-profiles-02-spec-tech.md), o item de validação de encoding usa:
> `file -i mods/RZCustomProfiles/modded/profiles/*.json` deve retornar `charset=utf-8`

O comando `file` não existe nativamente no Windows PowerShell. O ambiente deste repo é `win32` com PowerShell (per CLAUDE.md). O `file` só está disponível via Git Bash, MSYS ou WSL.

**Por que importa:** Implementador no Windows nativo executa o checklist e o item falha — pode ignorar e assumir UTF-8 sem validar, ou perde tempo configurando ambiente. Validação de BOM/encoding é crítica (corner case real — caracteres acentuados em `Name`/`Description`).

**Sugestão:** Substituir a linha por uma alternativa portátil. Opções:

- **PowerShell nativo (Windows-first):**
  ```powershell
  Get-ChildItem mods/RZCustomProfiles/modded/profiles/*.json | ForEach-Object {
    $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
    if ($bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
      Write-Host "BOM detectado: $($_.Name)" -ForegroundColor Red
    }
  }
  ```
- **Node.js (cross-platform):**
  ```bash
  node -e "['file1.json','...'].forEach(f => { const b = require('fs').readFileSync(f); console.log(f, b[0]===0xEF?'BOM':'OK'); })"
  ```
- **Bash equivalente preservado entre parênteses** caso o dev tenha Git Bash.

Recomendação: PowerShell como primário (alinhado ao ambiente nativo) + nota "se tiver Git Bash: `file -i ...`".

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (PowerShell primário)
- `[ ]` Caminho alternativo: _________________

---

### PA-01-06 · A — Gap · ✅ Resolvido em 2026-05-17

**Limite numérico para `Description` não definido**

**Resolução:** nota "Limite de design: Description ≤ 200 caracteres" adicionada no bloco final do skeleton em §5 da spec técnica.

**Problema:** A spec funcional (corner case) menciona "≤ 200 chars" para Description longa. O skeleton da spec técnica mostra a Description do Médico de Combate com 117 chars — OK, mas não há critério numérico explícito reaplicável às outras 9.

**Por que importa:** Implementador pode escrever descriptions de tamanhos muito variados; alguns truncam no launcher, outros não. Inconsistência visual entre classes.

**Sugestão:** Adicionar nota explícita em §5 do skeleton:
> Limite de design: `Description` ≤ 200 caracteres. Truncamento no launcher ainda não confirmado empiricamente — 200 é margem segura para a maioria dos widgets de seleção de perfil.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (adicionar limite 200 chars no §5)
- `[ ]` Caminho alternativo: _________________

---

### PA-01-07 · B — Edge Case · ✅ Resolvido em 2026-05-17

**Comportamento se BaseProfile ≠ 0 não documentado**

**Resolução:** bloco "Premissa fixa do schema" adicionado no topo do §7 Riscos da spec técnica explicitando que todos os 10 perfis usam `BaseProfile: 0` e que mudar para Unheard/EOD requer re-auditoria dos zeros explícitos.

**Problema:** A spec funcional ([§Comportamento desejado](001-custom-profiles-01-spec.md)) reza que zeros explícitos em `TradersLoyalty`/`HideoutStartingLevels`/`SkillOverrides` são "identidade do Standard". Isso é verdade **apenas** se `BaseProfile: 0` (Standard). Se algum dia algum dos 10 perfis usar `BaseProfile: 4` (Unheard — começa com Stash 3, várias estações em L1+, traders elevados), os zeros do JSON podem fazer **downgrade silencioso**.

**Por que importa:** Hoje todos os 10 usam BaseProfile 0, sem risco. Mas a spec técnica não documenta o gatilho — uma mudança futura para Unheard ou EOD passa despercebida.

**Sugestão:** Adicionar nota destacada em §1 Estratégia ou §7 Riscos:
> **Premissa do schema:** todos os 10 perfis usam `BaseProfile: 0` (Standard). Mudar para Unheard/EOD requer auditoria dos zeros explícitos em `TradersLoyalty`/`HideoutStartingLevels`/`SkillOverrides`, que podem downgradar silenciosamente o base profile.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (nota em §7)
- `[ ]` Caminho alternativo: _________________
