# /review-mod-properties

Revisão crítica de **UX e organização das propriedades F12** (BepInEx `ConfigEntry`) de um mod. Avalia ordem/nomes de seções, alocação, nomes/tipos/tooltips das opções, propriedades mortas e uso de "Advanced". Cria um relatório novo `PROPRIEDADES-review-NN.md` a cada execução (NN incremental) com achados priorizados e sugestões acionáveis. As correções aceitas são aplicadas no **`Plugin.cs`** (as `Config.Bind`); o `PROPRIEDADES.md` é regenerado depois.

> **Skills obrigatórias:** carregar `csharp-mod-best-practices` (tipos de `ConfigEntry`, `AcceptableValueRange`, `ConfigurationManagerAttributes`), `repo-workflow-best-practices` (§7 — `PROPRIEDADES.md` como single source, section rename = breaking change) e `spt-mod-best-practices`. Consultar `memory-curation` §14 (passo de contexto de memória).

## Uso

```
/review-mod-properties <mod>
```

- `<mod>` — nome da pasta em `mods/` (ex.: `stancesAndCameraPositionSPT4.0.11`). Validar que existe.

## Pré-condições

1. `mods/<mod>/modded/` existe e contém as chamadas `Config.Bind(...)` (normalmente em `Plugin.cs`; pode haver binds em métodos auxiliares/loops — ex.: `BindStance`, `BindStaminaManagement`).
2. `mods/<mod>/PROPRIEDADES.md` existe (opcional). Se não existir, seguir mesmo assim usando só o código, e recomendar gerá-lo depois.

Se `mods/<mod>/` não existir, listar os mods disponíveis e parar.

## O que fazer

1. **Resolver `<mod>`.**

2. **Calcular `NN` da review.** Listar `mods/<mod>/PROPRIEDADES-review-*.md`. Próximo NN = maior + 1, padded a 2 dígitos. Primeira = `01`.

3. **Ler contexto (memória — `memory-curation` §14):** topo de `mods/<mod>/memory/sessions.md` (snapshot + pendências) + entradas que citem "propriedade", "config", "F12", "PROPRIEDADES". Reportar pendências que afetam (ex.: rename de seção já planejado, breaking change conhecido). Emitir a linha `Memória consultada: ...`. Se não existir, registrar "sem memória prévia".

4. **Extrair TODAS as `Config.Bind`** (a **fonte de verdade** é o código, não o `PROPRIEDADES.md`). Para cada uma capturar: **seção** (string literal, incluindo sufixos como `(Advanced)`), **key**, **tipo** (`bool`/`float`/`int`/`KeyCode`/enum), **default**, **faixa** (`AcceptableValueRange<T>` ou —), **tooltip** (texto do `ConfigDescription`), e **Advanced?** (`ConfigurationManagerAttributes { IsAdvanced = true }` e/ou `Order`). Resolver binds gerados em loop/helper para as instâncias concretas. Anotar `Plugin.cs:linha` de cada bind. Para mods grandes (50+ props), delegar a extração a um sub-agent (read-only) e consolidar.

5. **Detectar propriedades mortas (critério 7 — DEAD).** Para cada `ConfigEntry` (campo `_Xxx`), Grep o uso de `_Xxx.Value` (ou o campo) em `modded/`. **Bindada mas nunca lida = morta** (🔴). Distinguir de props lidas só condicionalmente (não é morta). Uma prop cujo `.Value` só alimenta outra prop também morta é morta transitiva.

6. **Reconciliar código × `PROPRIEDADES.md`.** Divergências (key no doc que não existe no código, ou vice-versa; default/faixa/seção diferentes) são achados (o doc está defasado) e entram no Panorama.

7. **Analisar por 8 categorias × 4 impactos** (critérios do template `.agents/templates/mod-properties-review.md.tmpl`):
   - **ORD** (ordem de seções): a ordem no F12 é por **descoberta** (primeira `Config.Bind` de cada seção) — documentar a ordem real e apontar quando seções relacionadas ficam espalhadas (sugerir reordenar os binds). Ref: `repo-workflow-best-practices §7`.
   - **SEC** (distribuição/nome de seção): seções gigantes vs minúsculas; nomes pouco intuitivos; prefixos numéricos inconsistentes (ex.: só algumas seções numeradas); sufixos `(Advanced)` inconsistentes.
   - **LOC** (alocação): prop na seção temática errada (ex.: opção de ADS numa seção de movimento).
   - **NAM** (nome da prop): key que não reflete o efeito; **nome enganoso** (o rótulo diz um eixo mas o código aplica outro); rótulo **legado** desatualizado; idioma inconsistente.
   - **TYP** (tipo/edição): `float` com range → slider (bom) vs sem range → campo aberto (ruim quando o valor é limitado); `int` vs `float`; enum modelado como número/string; range incoerente (min>max, ou que não cobre casos úteis); keybind como texto.
   - **TIP** (tooltip): ausente; não diz o efeito prático/unidade; idioma inconsistente com o resto (o repo usa **pt-BR** — `create-technical-spec`); jargão sem explicação.
   - **DEAD** (morta): resultado do passo 5.
   - **ADV** (Advanced): prop comum escondida em Advanced (usuário não acha) → 🟠; prop técnica/perigosa (afeta perf, debug, quebra balance) **sem** Advanced → 🟡.

8. **Renderizar `.agents/templates/mod-properties-review.md.tmpl`** preenchendo `{{MOD}}`, `{{CREATED_AT}}`, `{{REVIEW_NN}}`, o **Panorama** (ordem das seções, contagem por seção, mortas, divergências com o doc) e cada achado no formato `MP-NN-MM`.

9. **Adicionar achados** no formato do template. Cada um cita `seção · key · Plugin.cs:linha`. **Toda sugestão é acionável** (novo nome/seção/tipo/range/tooltip com o valor exato). Sugestão que renomeia **seção ou key** recebe `⚠️ BREAKING` no título + coluna "Breaking?" = ⚠️ no índice, e descreve a migração (aceitar reset ao default, ou nota de changelog; nunca renomear silenciosamente).

10. **Atualizar índice e contadores** no topo.

11. **Reportar:**
    ```text
    ✓ Review de propriedades NN criada: <path>
      Memória consultada: snapshot de YYYY-MM-DD (Sessão N) · pendências que afetam: [...] / nenhuma
      Props analisadas: N · Seções: N · Mortas: N · Breaking propostos: N
      🔴 N · 🟠 N · 🟡 N · 🟢 N
    Próximo passo:
      Marque "Aceitar sugestão" nos achados a corrigir.
      Aplicar = editar as Config.Bind no Plugin.cs; renomes de seção/key são BREAKING (comunicar no changelog).
      Depois, regenerar PROPRIEDADES.md do código.
    ```

## Categorias × impactos

Ver a tabela de critérios e a escala de impacto no template. Códigos: **ORD · SEC · LOC · NAM · TYP · TIP · DEAD · ADV**.

## Regras

- **Fonte de verdade = código** (`Config.Bind` no `Plugin.cs`), nunca o `PROPRIEDADES.md` (que pode estar defasado — reconciliar é parte do review).
- **Não editar o `PROPRIEDADES.md` neste command.** Ele documenta; as mudanças reais são no `Plugin.cs`. O doc é **regenerado** após aplicar os achados aceitos.
- **Breaking change (obrigatório sinalizar):** renomear `(section, key)` recria a entrada e descarta o valor salvo do usuário (`repo-workflow-best-practices §7`). Só propor rename quando o ganho de UX compensa; sempre com estratégia de migração.
- **Reviews são artefatos imutáveis** — cada execução cria um arquivo novo; achados ganham só anotações de resolução depois. Pontos já `✅ Aplicado` não voltam.
- **Cada achado cita evidência** (`Plugin.cs:linha` do bind, ou o grep que prova a morte). Análise sem evidência não vai.
- Não confundir "Advanced" (esconder no F12) com "avançado tecnicamente" — o critério é **exposição ao usuário comum**.
- Versão alvo: SPT 4.0+ / EFT 0.16.x.

## Aplicação (fora deste command)

As correções aceitas são aplicadas manualmente (ou por edição dirigida) no `Plugin.cs`:
- **NAM/TIP/TYP** (mesma `(section,key)`): editar o `ConfigDescription`/tipo/`AcceptableValueRange` — **não** é breaking (o valor salvo persiste se a key não muda).
- **SEC/NAM que renomeiam** + **ORD** (mover bind) + **LOC** (trocar de seção): **breaking** — comunicar no changelog do mod.
- **DEAD:** remover a `Config.Bind` + o campo `_Xxx`.
- **ADV:** ajustar `ConfigurationManagerAttributes { IsAdvanced = ... }` (ou mover para/de uma seção `(Advanced)`).

Depois de aplicar, **regenerar** o `PROPRIEDADES.md` a partir do código e incrementar a versão do mod se for release (`feedback_version_increment_on_release`).
