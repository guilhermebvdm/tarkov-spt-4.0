# 047 — Roster 11→6 (aplicar matriz) · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [047-roster-6-classes-02-spec-tech.md](047-roster-6-classes-02-spec-tech.md)
**Data:** 2026-06-21

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 3 · 🟢 Menores: 3 · ✅ Resolvidos: **6** · Total: 6
>
> **Todos os 6 pontos aceitos e aplicados na spec técnica (2026-06-21)** — ver §Resolução no fim.

Âncoras conferidas no SPT source: `ProfileHelper.GetProfileTemplateForSide`→`return null` ✅, `TraderHelper.cs:150` `matchingSide.Trader` (NRE p/ órfão) ✅, `SaveServer` itera **todos** os `SaveLoadRouter` no load ✅ (com guard `IsProfileInvalidOrUnloadable` antes — SaveServer.cs:260), `SaveLoadRouter`/`HandledRoute` shape ✅, `SkillTypes.Shadowconnections` (c minúsculo) ✅, `HiddenEditionsLoader` só mexe na blacklist de criação (não no dict de templates) ✅ → fallback "Standard" é válido. Sem bloqueador.

**Memória:** snapshot ~2026-06-12 · pendências que afetam: **P-7.3** (stash de itens compostos — vira PA-01-04).

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge Case | 🟡 | Remap amplo nuka perfis de classe mantida em troca de idioma | ✅ Resolvido |
| PA-01-02 | A — Gap | 🟡 | Config `orphanEditionFallback` não está cabeada no stub | ✅ Resolvido |
| PA-01-03 | A — Gap | 🟡 | Fallback sem defesa se "Standard" não existir nos templates | ✅ Resolvido |
| PA-01-04 | A — Gap | 🟢 | Profile-fonte do gear de fantasma/tanque não definido | ✅ Resolvido |
| PA-01-05 | C — Lógica | 🟢 | Drift de linha em ProfileHelper (809-812 → ~808-811) | ✅ Resolvido |
| PA-01-06 | A — Gap | 🟢 | Assumir que editions CustomClasses entram no GetProfileTemplates() | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

---

## Pontos

### PA-01-01 · B — Edge Case · 🟡 Importante

**Remap amplo de órfão nuka perfis de classe MANTIDA quando o idioma do launcher muda**

**Problema:** o key da edition registrada = `displayName[lang]` (`CustomClassesMod.cs:77`, `ApplyLauncherLanguage` :129-140) — ex.: "Caçador" (pt) vs "Hunter" (en). O `OrphanEditionSaveLoadRouter` proposto remapeia **qualquer** edition ausente do dict para "Standard". Se o usuário trocar `language` no `settings.jsonc` (pt→en), no próximo boot as editions são re-chaveadas; um perfil criado como "Caçador" passa a ter `Edition` órfã e o router o converte para **"Standard"** — perdendo a identidade de classe (skills/mults param de ser reconhecidos pela edition), não só as 6 aposentadas.

**Por que importa:** o router, pensado para as 6 deletadas, captura também re-chaveamento por idioma e **degrada silenciosamente** perfis válidos de classes mantidas. Pior que o crash original (que ao menos sinaliza). Cenário real: qualquer um que use pt e depois en (ou vice-versa).

**Sugestão:** restringir o remap a uma **lista conhecida de editions aposentadas** (os 6 nomes em pt **e** en — Armeiro/Field Armorer, Batedor/Scout, Gerente de Operações/Operations Manager, Operador Furtivo/Stealth Operator, Operador Tático/Special Forces, Sobrevivencialista/Survivalist), em vez de "qualquer edition ausente". Assim o router só toca as deletadas; o re-chaveamento por idioma de classes mantidas fica fora do escopo (tratá-lo é outro item — mapear edition por um ID estável, não pelo displayName). Alternativa: documentar "trocar de idioma com perfis existentes não é suportado" e manter o remap amplo (pior UX).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (lista de retired editions pt+en)
- `[ ]` Caminho alternativo: _________________

### PA-01-02 · A — Gap · 🟡 Importante

**Config `orphanEditionFallback` declarada mas não cabeada no stub**

**Problema:** a §3 promete `orphanEditionFallback` em `settings.jsonc`, mas o stub do router hardcoda `const string fallback = "Standard"` com `// TODO confirm`. Não está especificado COMO o router lê o config.

**Por que importa:** sem a cabagem, o config é fantasma — o dev ou hardcoda (config inútil) ou improvisa uma leitura divergente do padrão do mod.

**Sugestão:** reusar o padrão existente — estender o record `LauncherSettings` (`CustomClassesMod.cs:143-147`) com `[JsonPropertyName("orphanEditionFallback")] string? OrphanEditionFallback`, e no router ler `settings.jsonc` via `ModHelper`/`JsonUtil` (mesmo `LoadLauncherLanguage`, CustomClassesMod.cs:104-124), default "Standard" quando ausente/vazio. Especificar isso no stub e na tabela §4 (settings.jsonc MODIFICAR).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

### PA-01-03 · A — Gap · 🟡 Importante

**Router não se defende se o próprio fallback não existir nos templates**

**Problema:** o router faz `profile.Edition = fallback` sem checar se `fallback` está em `GetProfileTemplates()`. Se "Standard" (ou o valor configurado) não existir, o perfil continua órfão — só troca de nome — e o NRE de `TraderHelper.cs:150` volta.

**Por que importa:** confiar cegamente em "Standard" reabre o crash que o router existe para evitar (config errado, build estranho, edition base diferente).

**Sugestão:** após resolver o fallback, validar: `if (!templates.ContainsKey(fallback)) fallback = templates.Keys.FirstOrDefault() ?? edition;` e logar `Error` se não houver nenhum template. Assim o remap sempre aterna numa key real. (Como `GetProfileTemplates()` sempre tem vanilla, é cinto-e-suspensório barato.)

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

### PA-01-04 · A — Gap · 🟢 Menor

**Profile-fonte do gear de fantasma/tanque não definido (P-7.3)**

**Problema:** a §7 diz que o gear das 2 novas classes sai do `extract-from-profile.mjs` "a partir do profile-fonte escolhido", mas não nomeia o profile nem decide o placeholder. A spec funcional pôs "gear definitivo" fora de escopo, então gear inicial/placeholder basta — mas a decisão segue aberta.

**Por que importa:** sem um source, o passo de gear do `/code-mod` trava; e a pendência **P-7.3** (itens compostos do stash precisam nascer montados) exige validação in-game do que for autorado.

**Sugestão:** para 047, decidir entre (a) clonar o loadout de uma classe existente próxima como placeholder (Fantasma ← base furtiva; Tanque ← base pesada) ou (b) nomear 1 profile-fonte por classe para o `extract-from-profile.mjs`. Registrar a escolha na §4/§8 da spec técnica; deixar o curado para depois.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (placeholder via clone)
- `[ ]` Caminho alternativo: _________________

### PA-01-05 · C — Lógica · 🟢 Menor

**Drift de linha em ProfileHelper**

**Problema:** a spec cita `ProfileHelper.cs:809-812` para o `TryGetValue`/`return null`; no source o método começa em 804 e o `TryGetValue`/`return null` ficam em ~808-811.

**Por que importa:** precisão de âncora (regra do fluxo: linha citada deve bater). Não afeta a lógica — o código existe e confere.

**Sugestão:** ajustar a referência para `ProfileHelper.cs:808-811` (ou "GetProfileTemplateForSide, ~L804-811") na §2 e no stub.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

### PA-01-06 · A — Gap · 🟢 Menor

**Assumir que as editions CustomClasses entram no dict que o router lê**

**Problema:** o router decide "não-órfão" por `GetProfileTemplates().ContainsKey(edition)`. Isso pressupõe que as editions registradas por `ClassRegistrar.Commit` estão **no mesmo dict** retornado por `databaseService.GetProfileTemplates()` no momento do load. A spec não confirma explicitamente.

**Por que importa:** se `Commit` escrevesse num dict diferente (ou `GetProfileTemplates()` retornasse cópia), um perfil de classe **válida** seria visto como órfão e remapeado — regressão.

**Sugestão:** confirmar na spec (1 linha + ref) que `ClassRegistrar.Commit` injeta no `DatabaseService` profile-templates (a mesma origem do `GetProfileTemplates()` usado por `CreateProfileService`/`GetProfileTemplateForSide`). Como as classes já aparecem na criação de perfil, a evidência existe — só citar.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

---

## Resolução (2026-06-21)

Todos os 6 pontos **aceitos** e aplicados na [spec técnica](047-roster-6-classes-02-spec-tech.md):

- **PA-01-01** ✅ — router escopado a uma `HashSet` das 6 aposentadas (name/en + pt: Armorer/Armeiro, Scout/Batedor, Operations Manager/Gerente de Operações, Stealth Operator/Operador Furtivo, Tactical Operator/Operador Tático, Survivalist/Sobrevivencialista); só remapeia essas → re-chave por idioma de classe mantida não é tocado.
- **PA-01-02** ✅ — `LoadFallbackEdition()` lê `orphanEditionFallback` de `settings.jsonc` (padrão do `LoadLauncherLanguage`), default `"Standard"`; nota p/ unificar com `LauncherSettings`.
- **PA-01-03** ✅ — guarda `if (!templates.ContainsKey(fallback)) fallback = templates.Keys.FirstOrDefault()`; `Error` se não houver template.
- **PA-01-04** ✅ — gear placeholder por clone (fantasma ← operadorFurtivo; tanque ← operadorTatico/sobrevivencialista) **antes** de deletar; curado depois. §4/§8 atualizados.
- **PA-01-05** ✅ — âncora corrigida p/ `ProfileHelper.cs:808-811`.
- **PA-01-06** ✅ — confirmado: `Commit` escreve `GetProfileTemplates()[plan.Name]` (ClassRegistrar.cs:282) = mesmo dict que o router lê; linha citada na §2.

**Status:** 0 pendentes. Spec técnica pronta para `/code-mod`.
