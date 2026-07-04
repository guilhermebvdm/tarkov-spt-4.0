# 058 — Rota pública de classes p/ launcher · Code review 01

**Mod:** CustomClasses
**Commit revisado:** `d001ec6` (feat: public class list route for launcher, item 058)
**Data:** 2026-07-03
**Reviewer:** adversarial, contexto limpo (não escreveu o código)
**Spec:** [058-launcher-classes-route-01-spec.md](058-launcher-classes-route-01-spec.md) · As-built: [058-launcher-classes-route-05-asbuild.md](058-launcher-classes-route-05-asbuild.md) · Contrato SP0: [004-classes-dados-reais-00-kickoff.md](../../../../launcher/Launcher4.0beta/backlog/004-classes-dados-reais/004-classes-dados-reais-00-kickoff.md)

**Arquivos revisados:** `modded/Server/ClassListRouter.cs`, `ClassListResponse.cs`, `ClassEditionKeyRegistry.cs` (novos), `ClassRegistrar.cs` (3 micro-edições). Fontes de verdade consultadas: `ClassEditorService.cs`, `CustomClassesMod.cs`, `ClassVisualRegistry.cs`, `SkillMultiplierRegistry.cs`, `SkillMultipliersRouter.cs`, `LocalizedText.cs`, `HiddenEditionsLoader.cs`, spt-source (`DI/Router.cs`, `Routers/HttpRouter.cs`, `Servers/Http/SptHttpListener.cs`, `Utils/JsonUtil.cs`), launcher (`RequestHandler.cs`, `MiniCommon/Request.cs`).

**Resultado do build (gate):** ✅ `dotnet build mods/CustomClasses/modded/Server/CustomClasses.Server.csproj -c Release` — êxito, 0 aviso(s), 0 erro(s) (1.76s), rodado neste review.

**Contagem:** 1 🔴 · 4 🟡 · 2 🟢 (notas)

---

### CR-01-01 [🔴] Sob `language=pt`, qualquer Save+hotApply no editor remapeia a editionKey servida ao launcher para a chave EN transitória — perfis registrados nessa janela ficam com edition fantasma após restart

**Onde:** interação `ClassEditorService.Save` (`def.Name` cru, sem transform de língua) × `ClassRegistrar.Commit` (`classEditionKeyRegistry.Set(SourceFileName, plan.Name)`, ClassRegistrar.cs:282-285) × `ClassListRouter` (serve a chave do último Commit).

**Status prévio:** o bug de hot-apply language-blind é PRÉ-existente e está registrado (P-058.2 — hot-apply do editor usa `def.Name` cru; spec §Edge cases). **Este review não reclassifica a existência do bug — reclassifica a CONTENÇÃO.** O as-built afirma "a rota serve a chave do último Commit (consistente com o registrado); um restart reconverge". A reconvergência vale para a ROTA, mas **não vale para os perfis criados na janela** — e o item 058 é exatamente o que passa a canalizar registros de usuários finais por essa janela.

**Cenário de falha concreto (config de produção atual: `settings.jsonc language=pt`, 7 classes com `name` EN):**
1. Launcher TRL (item 004) consome a rota; jogador vê "Caçador" (`editionKey: "Caçador"`).
2. Admin abre o editor web com o server up (fluxo rotineiro) e salva QUALQUER ajuste em `cacador.jsonc` com hot-apply. `Save` → `ValidateAndBuild(def.Name="Hunter")` → `templates.ContainsKey("Hunter")` = false → sem colisão → `Commit` registra edition NOVA "Hunter" e `keyRegistry` remapeia `cacador.jsonc → "Hunter"`.
3. A rota passa a servir `editionKey: "Hunter"` (displayName.pt continua "Caçador" — **UI idêntica, ninguém percebe**). Jogador registra → `profile.info.edition = "Hunter"`.
4. Restart do server: boot re-registra só "Caçador"; "Hunter" deixa de existir. O perfil aponta para edition fantasma → `SkillMultipliersRouter` (`visualRegistry.Contains("Hunter")` = false) devolve identidade null → o jogador **perde silenciosamente** multiplicadores/ícone/cor da classe; item 057 (identidade coop) herdaria o mesmo buraco.
5. A janela dura até o próximo restart (indefinida) e o servidor é Fika coop multi-user com registros remotos — a premissa "single-user local" que ampara as outras races NÃO ampara este fluxo.

**Fix proposto (em ordem de preferência):**
1. **Estrutural (fecha P-058.2 na raiz):** mover a resolução de língua para o pipeline compartilhado — `ClassRegistrar.ValidateAndBuild` aplica o transform (extrair `ApplyLauncherLanguage`/`LoadLauncherLanguage` de `CustomClassesMod` para um singleton, ex. `LauncherLanguageConfig`), de modo que `plan.Name` já venha re-chaveado para TODOS os callers (boot, Save, Delete). Boot deixa de ter lógica própria; `Remove(plan.Name)` e `RemoveByEdition` passam a receber a chave efetiva de graça.
2. **Tático (se o 1 não couber antes do rollout do item 004):** bloquear `hotApply` quando `language != "name"` (Save grava e avisa "aplica no restart") — fecha a janela de corrupção sem tocar na rota.

**Gate recomendado:** não liberar o fluxo de registro do launcher (item 004) em produção com `language=pt` antes de um dos dois fixes.

---

### CR-01-02 [🟡] `Delete` sob `language=pt` deixa mapeamento órfão no `ClassEditionKeyRegistry` — arquivo re-materializado sem Commit é servido como frankenclass

**Onde:** `ClassEditorService.Delete` → `classRegistrar.Remove(name cru)` → guard `!classVisualRegistry.Contains(name)` (ClassRegistrar.cs:307) retorna ANTES de `classEditionKeyRegistry.RemoveByEdition` (ClassRegistrar.cs:316).

**Cenário de falha concreto:**
1. `language=pt`. Admin deleta `cacador.jsonc` no editor (hotRemove=true). `Remove("Hunter")` → `visualRegistry.Contains("Hunter")` = false → early-return: nada é limpo. `keyRegistry` mantém `cacador.jsonc → "Caçador"` (órfão); edition "Caçador" segue registrada (parte pré-existente do P-058.2).
2. `cacador.jsonc` re-aparece no disco SEM Commit — cenários reais deste repo: `/sync-classes` re-copiando o arquivo do repo pro server, ou restore manual do `.bak1`.
3. A rota vê: entry do disco (enabled, parse OK) + mapeamento órfão "Caçador" + `visualRegistry.Contains("Caçador")` ✓ → **serve o item** com `editionKey "Caçador"` e displayName/description/skills do arquivo re-criado — que nunca foi registrado. Se o conteúdo divergir do template vivo, o launcher exibe uma classe que não corresponde ao que o registro entrega.
4. Contraste: sob `language=name` o mesmo Delete limpa o mapeamento e a rota corretamente filtra o arquivo re-materializado até um novo Commit — o comportamento conservador pretendido pelo design.

**Fix proposto:** o fix estrutural do CR-01-01 resolve (Remove passa a receber a chave efetiva). Paliativo local caso CR-01-01 demore: adicionar `ClassEditionKeyRegistry.RemoveByFile(fileName)` e chamá-lo em `ClassEditorService.Delete` (é serviço, não página Razor — fora da proibição de escopo da spec), independentemente do resultado de `classRegistrar.Remove`.

---

### CR-01-03 [🟡] Dois arquivos mapeados para a mesma edition → resposta com `editionKey` duplicada (contrato assume chave única)

**Onde:** `ClassListRouter.GetRoutes` (loop sem dedupe) × `ClassEditionKeyRegistry.Set` (N arquivos podem apontar p/ a mesma chave) × `ClassRegistrar.ValidateAndBuild` com `allowReplace=true` (aceita colisão com QUALQUER edition do próprio mod, sem distinguir o arquivo dono).

**Cenário de falha concreto (independe de `language`):**
1. Boot com `a.jsonc` e `b.jsonc` ambos com `name: "X"` — só `a.jsonc` registra (keyRegistry: `a→X`); `b.jsonc` é filtrado. Até aqui OK (a spec cobre).
2. Usuário abre `b.jsonc` no editor (que exibe o Error DuplicateClassName do CR-EP-06) e clica **Save sem mudar o nome**, hot-apply ligado. `Save` NÃO consulta a passada de colisão cross-file (ela vive só em `ListClassFiles`); `ValidateAndBuild(allowReplace=true)`: `templates.ContainsKey("X") && visualRegistry.Contains("X")` → colisão PERDOADA → Commit → `keyRegistry.Set("b.jsonc","X")`. Agora `a→X` **e** `b→X`.
3. A rota serve DOIS itens com `editionKey "X"`, cada um com os textos/skills do seu arquivo. Launcher keyando a lista por `editionKey` (o natural) → item duplicado/sobrescrito ou crash de key duplicada, dependendo da implementação do item 004. O mesmo vale pro rename cross-file (editar o `name` de um arquivo para o nome de outra classe registrada).
4. Restart reconverge (boot alfabético registra só o primeiro), mas a janela é indefinida.

**Fix proposto:** dedupe barato no router — agrupar por `editionKey` após o loop (primeiro vence, ordem já determinística por fileName) + `logger.Warning` listando os arquivos em conflito. ~5 linhas, só no arquivo novo. Adicionalmente, o contrato SP0 no kickoff merece a linha "editionKey é única no array".

---

### CR-01-04 [🟡] Instrução de validação do as-built (P-058.1) falha: resposta GET sai zlib-comprimida — `curl` puro mostra binário

**Onde:** as-built §"Como testar manualmente" passo 2 (`curl http://127.0.0.1:6969/customclasses/classes`) × `SptHttpListener.SendResponse` → `SendZlibJson` (spt-source SptHttpListener.cs:135/201 — TODO response sem serializer dedicado é zlib deflate, `Content-Type: application/json` enganoso).

**Cenário de falha concreto:** executar o passo 2 do as-built como escrito → bytes zlib no terminal → o gate humano P-058.1 (validar a rota com server rodando) pode ser mal-diagnosticado como "rota quebrada" e disparar debugging desnecessário — ou pior, o item 004 do launcher ser implementado com `HttpClient` cru + `JsonSerializer` e falhar em produção.

**Não é defeito da rota:** o launcher existente descomprime (`Request.GetJson` → `SimpleZlib.Decompress`, SPT.Launcher.Base/MiniCommon/Request.cs:97) — desde que o item 004 use essa infra, o consumo funciona.

**Fix proposto (doc, 2 linhas no as-built):** `curl -H "responsecompressed: 0" http://127.0.0.1:6969/customclasses/classes` (o header ativa `IsDebugRequest` → `SendJson` plano, SptHttpListener.cs:146-149). E registrar no kickoff do 004: consumir via `Request`/`RequestHandler` existentes (zlib), não `HttpClient` cru.

---

### CR-01-05 [🟡] Proveniência mista dentro de um mesmo item: `displayName/description/skills` = arquivo ATUAL; `editionKey/skillMultipliers` = último Commit — edit externo sem hot-apply serve dados que o registro não entrega

**Onde:** `ClassListRouter` monta o item com `entry.Definition` (cache por mtime — reflete o DISCO agora) + `keyRegistry`/`multiplierRegistry` (refletem o último Commit).

**Cenário de falha concreto (workflow real deste repo):**
1. `/sync-classes` copia `cacador.jsonc` atualizado (Sniper 7→10) para o server vivo, sem restart e sem Save no editor (memória do repo: pushes externos de arquivo são rotina).
2. A rota passa a servir `skills: { "Sniper": 10 }` (arquivo atual), mas o template registrado — o que o jogador RECEBE ao registrar com `editionKey "Caçador"` — continua o build antigo com Sniper 7. Launcher mostra 10, perfil nasce com 7. Nenhum erro em nenhum lugar.
3. Variante de normalização (mesma raiz): `skills` sai cru do arquivo — `{ "sniper": 99 }` é servido verbatim, enquanto o pipeline aplica `Sniper` nível 51 (`Enum.TryParse ignoreCase` + `Math.Clamp 0..51`, ClassRegistrar.ApplySkills). Skill miscased/desconhecida vai ao launcher com um nome que não casa com o enum (ícone/label quebram) e nível que não existe. A spec declarou "cru do arquivo" como decisão — registrado; a consequência acima fica explícita aqui.

**Fix proposto:** mínimo e local — normalizar `skills` na rota (TryParse ignoreCase + IsDefined + clamp 0..51, skip desconhecidas), espelhando `ApplySkills`; isso elimina a variante 3 sem mudar a fonte. Para a variante 1-2 (mais rara), ou snapshot de `skills` no Commit (novo campo no keyRegistry/registry próprio — consistência 100% commit-time), ou aceitar e documentar no contrato SP0 que `skills` reflete o ARQUIVO e pode divergir do template até restart/hot-apply.

---

### CR-01-06 [🟢] Thread-safety do `ClassEditionKeyRegistry` — consistente com a premissa aceita do repo; nota

`Dictionary` sem lock, escrita no boot/hot-apply (circuito Blazor) e leitura em thread HTTP (rota nova). Tecnicamente, `TryGetValue` concorrente com `Set` (resize) ou com a enumeração LINQ de `RemoveByEdition` pode lançar/ler estado torto. É o MESMO padrão documentado dos registries existentes (premissa item 021, declarada no header do arquivo) e a janela é minúscula (7 entradas). Aceito como está; se a premissa cair um dia (editor multi-admin), trocar por `ConcurrentDictionary` é drop-in aqui (a enumeração de `RemoveByEdition` já materializa com `ToList`).

---

### CR-01-07 [🟢] Rota não consulta `CreateNewProfileTypesBlacklist` (hidden-editions) — ok hoje, vetor de divergência futuro; nota

`HiddenEditionsLoader` alimenta a blacklist do launcher v1 para ocultar editions VANILLA — se um dia alguém ocultar uma CLASSE por `hidden-editions.jsonc`, ela some do `editions[]` vanilla mas continua na rota nova (gate = keyRegistry + visualRegistry, não olha a blacklist). Sem cenário de falha hoje (config atual só lista vanilla). Registrar a intenção: se ocultação de classe virar requisito, o filtro entra na rota, não na blacklist.

---

## Áreas verificadas e declaradas limpas

- **DI / callers do `ClassRegistrar` (pergunta 2):** construção EXCLUSIVA via DI (`[Injectable(Singleton)]`; zero `new ClassRegistrar(` em `modded/`); `ClassEditionKeyRegistry` é `[Injectable(Singleton)]` no mesmo assembly → resolvido em todos os pontos. Não existem projetos de teste no mod (nada a quebrar). Build verde confirma o nível compile.
- **Pipeline StaticRouter (pergunta 3):** GET suportado (`SptHttpListener.SupportedMethods` inclui GET; body null → `HandleStatic` materializa `EmptyRequestData`, spt-source Router.cs:73-78); matching exato por path (`Router.CanHandle` / `HttpRouter`, query string fora do `Path`); shape idêntico ao `SkillMultipliersRouter` comprovado em produção. Rota atingida antes do boot completo devolve `[]` gracioso (registry vazio) — casa com o fallback previsto no item 004.
- **Serialização (pergunta 3):** `JsonUtil` confirmado no source: `WhenWritingNull` (JsonUtil.cs:22) e `UnsafeRelaxedJsonEscaping` (JsonUtil.cs:26 — "Caçador" sai UTF-8 cru, sem `\uXXXX`). `JsonUtil` NÃO define `PropertyNamingPolicy` — o camelCase depende 100% dos `[JsonPropertyName]`, e todos os campos dos DTOs os têm. `LocalizedPair` como record separado NÃO herda o `LocalizedTextConverter` (que é `[JsonConverter]` na classe `LocalizedText`) — a decisão anti-colapso do shape está correta.
- **Gate `keyRegistry + visualRegistry` (pergunta 4):** classe sem `iconFile`/`nameColor` ENTRA no `ClassVisualRegistry` (`Set` incondicional no Commit, visual com nulls) — sem gap nesse eixo. Classe desabilitada pós-boot some da rota pelo filtro `entry.Enabled`, independente do resultado do hot-remove (inclusive sob `language=pt`, onde o Remove é no-op — mas a edition segue registrável pelo `editions[]` vanilla até restart; consequência anexada ao CR-01-01/P-058.2). Ordem do Commit (visual → key → templates) é fail-safe para leitor concorrente: key ausente → filtrado.
- **Performance por request (pergunta 3):** hot path = varredura de diretório + `FileInfo` por arquivo + reads de dicionário, zero dry-run (verificado em `ListClassFiles`/cache 037); cold = exatamente 1 dry-run por arquivo alterado (design). Nenhum IO pesado novo na rota.
- **iconUrl:** montagem idêntica às 5 ocorrências existentes nas páginas web (`/CustomClasses-Server/icons/{icon}`); mount `wwwroot` do `IModWebMetadata` confirmado (`CustomClassesMetadata`).
- **Colisão cross-file no BOOT:** segundo arquivo sem mapeamento → filtrado, sem classe fantasma (o buraco é só pós-boot via Save — CR-01-03).

## Resoluções

> Aplicadas em 2026-07-03 (/apply-code-review, execução autônoma). Review acima imutável; código anotado com `// ref: CR-01-NN`. Build gate re-rodado: ✅ Release, 0 aviso(s), 0 erro(s).

- **CR-01-01 ✅ aplicado — fix ESTRUTURAL (opção 1).** Resolução de língua extraída de `CustomClassesMod` para o singleton novo **`LauncherLanguageConfig`** (`Language` lazy do settings.jsonc + `ResolveEditionKey(def)`); `ClassRegistrar.ValidateAndBuild` agora resolve a chave via config (substitui `def.Name.Trim()`) → `plan.Name` vem re-chaveado para TODOS os callers (boot, Save/hotApply, Delete). Boot perdeu `LoadLauncherLanguage`/`ApplyLauncherLanguage`/`LauncherSettings` (removidos). `ClassRegistrar.ResolveEditionKey(def)` público novo para callers sem plan. O `name` no ARQUIVO nunca é reescrito (re-chaveamento é preocupação de registro). Assinaturas públicas `ValidateAndBuild`/`Commit`/`Remove` intactas (sessão paralela do editor segura). Bônus na mesma raiz: `ClassEditorService.BuildEntry` passa a computar `Registered` pela chave efetiva — fecha o falso-negativo dos chips do editor (parte 1 do P-058.2).
- **CR-01-02 ✅ aplicado — via estrutural + Delete.** `ClassEditorService.Delete` resolve a chave efetiva do def parseado (`classRegistrar.ResolveEditionKey`) antes do hot-remove → `Remove("Caçador")` acha a edition, limpa templates/registries/keyRegistry. Paliativo `RemoveByFile` desnecessário (não aplicado).
- **CR-01-03 ✅ aplicado (dedupe no router).** `ClassListRouter`: `HashSet` por editionKey, primeiro vence (ordem determinística por fileName), `logger.Warning` nomeando o arquivo pulado. Nota: com o fix estrutural o Save de arquivo colidido AINDA comete sobre a edition do outro arquivo (allowReplace perdoa colisão com edition do próprio mod sem distinguir o arquivo dono) — o dedupe contém o dano na rota; guard de dono no Save fica como pendência de editor (P-058.3). A linha "editionKey é única no array" no contrato SP0 do kickoff 004 **não** foi editada aqui (doc do launcher, sessão paralela é dona — registrado no as-built p/ o item 004 incorporar).
- **CR-01-04 ✅ aplicado (doc).** As-built corrigido: teste manual com `curl -H "responsecompressed: 0" ...` (resposta default é zlib deflate — `IsDebugRequest` → `SendJson` plano) + aviso explícito de que o item 004 deve consumir via `Request`/`RequestHandler` existentes do launcher (que já descomprimem), não `HttpClient` cru.
- **CR-01-05 ✅ parcial (variante 3) / ⏭️ variantes 1-2 aceitas.** Normalização de `skills` na rota (`NormalizeSkills`: TryParse ignoreCase + IsDefined + clamp 0..51, desconhecidas fora — espelha `ApplySkills`). Proveniência mista (arquivo atual vs último Commit em edit externo sem hot-apply): **aceita e documentada** (doc do router + as-built §Decisões) — snapshot commit-time não compensa (campo/registry extra p/ janela rara que restart/hot-apply reconverge; a rota é lista de display, o template é a verdade no registro).
- **CR-01-06 ⏭️ nota aceita.** Sem mudança — padrão de concorrência consistente com a premissa do item 021; `RemoveByEdition` já materializa com `ToList` (troca por `ConcurrentDictionary` é drop-in se a premissa cair).
- **CR-01-07 ⏭️ nota registrada.** Sem mudança — intenção documentada: se ocultação de CLASSE virar requisito, o filtro entra na rota (não na blacklist vanilla).

## Histórico

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-03 | Claude (adversarial review) | Criação — review do commit d001ec6: 1 🔴, 4 🟡, 2 🟢; build verde verificado. |
| 2026-07-03 | Claude (apply-code-review) | Seção Resoluções: CR-01-01/02 fix estrutural (LauncherLanguageConfig), CR-01-03 dedupe, CR-01-04 doc curl, CR-01-05 normalização skills; 06/07 notas aceitas. Build verde. |
