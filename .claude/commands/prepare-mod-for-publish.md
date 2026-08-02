# /prepare-mod-for-publish

Auditoria completa de prontidão para **publicar um mod a público** (SPT Forge). Roda em 5 fases, sendo a
primeira um **portão bloqueante**: elegibilidade → código → identidade → pacote/página → interface web (quando
houver). Cria `mods/<mod>/publish/PUBLISH-AUDIT-NN.md` a cada execução (NN incremental) com achados acionáveis.

> **Skills obrigatórias:** carregar `trl-mod-publishing` (regras do Forge + padrão de identidade TRL) antes de
> qualquer coisa. Nas fases 2–3, carregar também `csharp-mod-best-practices`, `spt-mod-best-practices` e
> `repo-workflow-best-practices`. Fase 5 só quando o mod tem interface web: `trl-ds-validation`.
> Consultar `memory-curation` §14 para o passo de contexto de memória.

> **Só audita — não corrige.** O command produz achados; nada no mod é editado por ele.

### Como um achado vira correção

O `PUBLISH-AUDIT-NN.md` **não é consumível pelo `/apply-code-review`** — esse command resolve `<ref>` para uma
pasta de item de backlog e exige `<NNN>-<slug>-04-code-review-NN.md` + `05-asbuild.md` ao lado. A auditoria vive
em `mods/<mod>/publish/` e não tem nenhum dos dois. Caminhos válidos, por tipo de achado:

| Tipo | Como corrigir |
|---|---|
| Código (fase 2) | Agrupar os achados aceitos num **item de backlog** (`/add-backlog-item`) e seguir o ciclo normal. Um item por frente, não um por achado |
| Opções do F12 (fase 3) | Pelo `/review-mod-properties`, que é o dono desse escopo — aplicação no `Plugin.cs` |
| Identidade, pacote, política | Trabalho manual, marcado direto na auditoria |

O formato de achado abaixo espelha o do `/code-review` de propósito — é vocabulário comum entre os commands,
**não** promessa de aplicação automática.

## Uso

```bash
/prepare-mod-for-publish <mod> [--fase N] [--reauditar]
```

- `<mod>` — slug da pasta em `mods/`, ou caminho que contenha `mods/<mod>/...`. Sem argumento, usa o mod da
  sessão atual (heurística da skill `memory-curation` §3); se a sessão não tocou nenhum mod, perguntar.
- `--fase N` — roda **só** a fase N (1–5), para reauditar uma frente já corrigida. **N > 1 exige que a fase 1 já
  tenha rodado neste mod e não tenha terminado em REPROVADO.** Decisão humana pendente **não** impede: repetir
  as pendências no cabeçalho do relatório e seguir. Nunca rodou a fase 1, ou reprovou → recusar e mandar rodar
  a fase 1 primeiro. O portão não se pula por flag.
- `--reauditar` — reavalia **tudo do zero**, inclusive o que auditorias anteriores marcaram `✅ Resolvido`
  (sem a flag, esses achados não voltam a ser levantados). Use quando o mod mudou muito desde a última.

## Pré-condição

`mods/<mod>/` existe e tem `modded/` (ou o diretório de código do mod). Falhou, parar com mensagem clara.

> **Não é pré-condição, é contexto:** defeito conhecido em aberto — bloqueador 🔴 num `04-code-review-NN.md` ou
> pendência 🔴/🟡 na memória — **não impede** a auditoria. A auditoria de publicação não substitui o ciclo nem é
> refém dele. Todo defeito conhecido tem um destino único: **fase 4**, porque a decisão é de produto (corrigir
> antes, ou publicar declarando a limitação na página do mod). Ver Fase 4, item 5.

## O que fazer

### Contexto (antes das fases)

1. Resolver `<mod>`. Criar `mods/<mod>/publish/` se não existir.
2. Calcular `NN` (maior `PUBLISH-AUDIT-*.md` + 1; primeira = `01`).
3. Ler o topo de `mods/<mod>/memory/sessions.md` (snapshot + pendências) e `backlog/mod-backlog.md`. Todo
   defeito conhecido que o jogador vai encontrar — pendência 🔴/🟡 da memória, bloqueador de code-review, item
   🟡 do backlog com comportamento parcial — vai para a **fase 4** (item 5): **não se publica com defeito
   conhecido que não esteja corrigido ou declarado na página do mod**.
4. Ler auditorias anteriores. Achado `✅ Resolvido` não volta (salvo `--reauditar`).
5. **Recuperar as decisões humanas já registradas** (bloco "Decisões humanas" das auditorias anteriores):
   política de IA, autorização do autor original, licença de cada asset. **Não perguntar de novo o que já foi
   respondido** — só o que está vazio ou mudou desde então. Toda resposta nova volta para esse bloco.

### Fase 1 — Elegibilidade (PORTÃO: reprovou, para tudo)

Aplicar `trl-mod-publishing` §1. **Antes de julgar, reconferir a redação vigente** em
<https://forge.sp-tarkov.com/content-guidelines> — a skill cita seções (`§4.2`, `§6.1`, `§6.2`) de uma fonte
externa que muda sem aviso. Divergência entre a skill e o site vale como achado de manutenção do harness.

Para cada portão, emitir veredito **APROVADO / REPROVADO / DECISÃO HUMANA**:

1. **Licença.** Ler `LICENSE` do mod. OSI-approved → APROVADO. Creative Commons cobrindo o **código** →
   DECISÃO HUMANA (a diretriz aponta CC para conteúdo não-código, não proíbe literalmente): registrar a
   pergunta "relicenciar como MIT/GPL?" ao autor original quando o mod é fork.
2. **Origem.** Ler `mod.json` → `upstream_url`. Se houver upstream, o mod é derivado: exigir registro da
   autorização do autor original. Sem registro → **DECISÃO HUMANA** (perguntar; não assumir que existe).
3. **Política de IA.** Perguntar e registrar a declaração do usuário. Nunca inferir do histórico de commits.
4. **Assets.** Listar todo binário não-código (`assets/`, `*.png`, `*.ogg`, `*.bundle`, fontes) e cobrar a
   origem/licença de cada um. Sem resposta → achado aberto.
5. **Conteúdo proibido.** Varrer por ofuscação, anti-debug, escrita fora da pasta SPT
   (`Environment.SpecialFolder`, `HKEY_`, caminhos absolutos fora do jogo), rede não declarada
   (`HttpClient`, `WebRequest`, `Socket` fora do Fika), telemetria.

**Se qualquer portão REPROVAR:** escrever o relatório só com a fase 1, reportar o bloqueio e **parar**. Não
gastar contexto auditando código de um mod que não pode ser publicado.

**DECISÃO HUMANA não é reprovação:** registrar a pergunta no bloco "Decisões humanas", seguir para as fases
seguintes e refletir no veredito final como `BLOQUEADO — aguardando decisão: [quais]`. O que trava é REPROVADO.

### Fase 2 — Código

Cada frente vira uma seção de achados, no vocabulário do `/code-review` (categorias A–F × impactos 🔴🟠🟡🟢) —
ver "Como um achado vira correção" acima para o que fazer com eles depois.

> **Mod grande (acima de ~2.000 linhas em `modded/`, ou mais de 3 frentes com achado esperado):** delegar cada
> frente a um sub-agent **read-only** em paralelo e consolidar os achados aqui, como faz o
> `/review-mod-properties` na extração de propriedades. Mod pequeno: sequencial, sem cerimônia.

1. **Código morto** — classe nunca instanciada, método vazio chamado pelo Unity, campo resolvido por reflection
   e nunca lido, `ConfigEntry` bindada e nunca consultada, valor fixo no código que ignora a opção do F12.
   Ferramenta: grafo do mod (skill `graph-code-navigation`) — nó sem caller de entrada é candidato.
2. **Correção** — releitura crítica do que foi implementado, com foco no que nunca rodou fora da máquina do
   autor. Estado estático não resetado entre raids; patch sem `try/catch` em caminho de frame; guard que
   diverge do guard nativo do EFT que ele espelha.
3. **Desempenho** — reflection resolvida por frame (deve ser cacheada no `GetTargetMethod`/estático),
   alocação em `Update`/`OnGUI`, `GetComponent` em laço quente, `GameObject`/listener órfão entre raids
   (skill `spt-memory-leak-analysis` quando o mod aloca estado por raid).
4. **Manutenibilidade** — estrutura de pastas, nomes de arquivo/classe/variável que não dizem o que fazem,
   arquivo-monólito (> ~1000 linhas) que concentra binds + lógica, comentário fóssil (descreve comportamento
   que não existe mais) e comentário excessivo (narra o óbvio linha a linha).

### Fase 3 — Identidade e configuração

Aplicar `trl-mod-publishing` §4. Emitir uma **tabela de conformidade** com o valor atual × esperado para **cada
linha da tabela §4.1 da skill** (ela é a fonte — não fixar a contagem aqui, para a skill poder crescer), mais:

1. **Versão nos 3 lugares** (`BepInPlugin`, `.csproj`, `CHANGELOG.md`) — divergência é 🔴.
2. **Plano de migração de config** quando a auditoria propõe renomear GUID/seção/chave: qual `.cfg` será
   distribuído, por qual canal, e a linha de aviso no changelog. Sem plano → 🔴 (o usuário perde os ajustes).
3. **F12 saneado — via [`/review-mod-properties`](review-mod-properties.md), não à mão.** Esse command já é o
   dono da UX das opções (ordem e nomes de seções, alocação, tipos, tooltips, props mortas, `Advanced`) e
   produz `PROPRIEDADES-review-NN.md`. Aqui:
   - **Invocar** `/review-mod-properties <mod>` — salvo se já existir uma review posterior à última mudança em
     `Plugin.cs`, caso em que reusar a existente e dizer qual.
   - **Não duplicar** os achados dele nesta auditoria: referenciar por ID (`MP-NN-MM`) e trazer para cá **só o
     que é bloqueante para publicar** — opção que não faz nada, unidade errada (todo `float` de faixa
     exatamente 0–1 vira **porcentagem** no ConfigurationManager), chave com caractere proibido
     (`= [ ] " ' \ tab`), tooltip que descreve comportamento inexistente.
   - **Achado do `/review-mod-properties` que implica renomear seção/chave alimenta o plano de migração** do
     item 2 acima — os dois não podem sair com respostas diferentes.
   - Se o mod não tem `Config.Bind`, registrar "não se aplica".

### Fase 4 — Pacote e página

Aplicar `trl-mod-publishing` §2. Verificar o que dá para verificar em disco e **listar como pendência humana** o
que depende de ação externa (subir repositório, gerar VirusTotal):

1. **Estrutura do pacote** — montar a árvore do `.zip` proposto e conferir que extrair na raiz do SPT basta.
2. **Repositório público** — o mod precisa sair do monorepo para um repo próprio com o código exato da build e
   instruções de build reproduzíveis.
3. **README de publicação** — instalação passo a passo, dependências por versão, uso/configuração, créditos ao
   autor original, compatibilidade SPT declarada.
4. **Rede** — se o mod fala com o Fika ou qualquer outra coisa, produzir a descrição de cada pacote/rota para a
   página. Impacto conforme a força da regra confirmada na fase 1 (ver `trl-mod-publishing` §1, nota sobre
   citação verificada × leitura resumida).
5. **Defeitos conhecidos** — consolidar o que veio do passo 0.3 (memória, code-review, backlog) numa lista, e
   para cada um registrar a decisão de produto: **corrigir antes de publicar** (vira item de backlog) ou
   **publicar declarando** (vira linha de "limitações conhecidas" na página do mod). Defeito sem decisão é 🔴 —
   o que não pode acontecer é o jogador descobrir sozinho.
6. **Prontidão de terceiro** (`trl-mod-publishing` §3) — plano de teste em instalação limpa, sem Fika, e ao lado
   dos mods populares. Sai como **roteiro de teste**, não como achado.

### Fase 5 — Interface web

**Aplica quando** o mod tem `wwwroot/`, ou qualquer `.html`/`.razor`/`.cshtml` fora de `original/` e de pastas
de documentação. Nenhum dos dois: registrar "não se aplica" e seguir.

Carregar `trl-ds-validation` e rodar as 4 lentes (leiturabilidade, acessibilidade, i18n, dataviz) contra o TRL
Design System.

### Encerramento — relatório

Renderizar `.agents/templates/publish-audit.md.tmpl` preenchendo `{{MOD}}`, `{{CANONICAL_NAME}}` (o nome TRL
proposto pela fase 3), `{{VERSION}}` (a do `BepInPlugin`), `{{CREATED_AT}}` e `{{AUDIT_NN}}`. Cada achado no
formato:

```markdown
### PUB-NN-MM · Fase N — Frente · Impacto

**Título resumido**

**Local:** [`caminho:linha`](link) — ou `—` quando é ação externa

**Problema:** [o que está errado, com evidência]

**Por que importa:** [consequência concreta para quem baixar o mod, ou qual regra do Forge é violada]

**Sugestão:** [ação específica]

**Decisão:**
- `[ ]` Pendente · `[ ]` Aceitar · `[ ]` Aceitar com modificação: ___ · `[ ]` Rejeitar (dívida): ___
```

`NN` = número desta auditoria; `MM` = ordem do achado **na auditoria inteira** (não por fase), começando em
`01`. IDs permanentes, nunca reutilizados entre auditorias. Achado que só referencia outro artefato (ex.: um
`MP-NN-MM` do `/review-mod-properties`) entra pelo ID de origem, sem `PUB-` próprio.

### Encerramento — saída no terminal

```text
✓ Auditoria de publicação NN: <path>
  Fase 1 (elegibilidade): APROVADO / REPROVADO em [portão] / [N] decisões humanas pendentes
  Fase 2 (código): 🔴 N · 🟠 N · 🟡 N · 🟢 N
  Fase 3 (identidade): [N] divergências · migração de config: OK / ausente
                       F12: PROPRIEDADES-review-NN [criada agora / reusada / não se aplica]
  Fase 4 (pacote): [N] pendências humanas
  Fase 5 (web): [N achados] / não se aplica
Veredito: PUBLICÁVEL / BLOQUEADO por [motivo] / BLOQUEADO — aguardando decisão: [quais]
Próximo passo: [ação concreta]
```

### Depois de publicar (fora da auditoria, mas parte do processo)

Quando o mod for de fato ao ar, fechar o ciclo — senão o repo fica sem registro de que a publicação aconteceu:

1. **`mod.json`** — preencher `forge_url` (o endereço da página), `license` (a licença final) e `spt_version`.
2. **`CHANGELOG.md`** — marcar qual versão foi a publicada.
3. **`/update-memory <mod>`** — registrar a publicação, a licença acordada e o que ficou como limitação
   declarada na página. É o que impede a próxima sessão de reabrir decisões já tomadas.

## Regras

- **Fase 1 é portão real.** Reprovou, para — não auditar código de mod impublicável.
- **Sempre criar arquivo novo.** Auditorias são imutáveis; ganham só anotações de resolução.
- **Decisão humana nunca é inferida.** Licença de asset, autorização de autor e política de IA se perguntam —
  e ficam registradas para as próximas auditorias não perguntarem de novo.
- **Todo achado com evidência** — `arquivo:linha` ou a regra do Forge citada. Sem evidência não entra.
- **Regra externa não verificada não gera 🔴 sozinha.** Se a redação vigente não foi confirmada na fase 1, o
  achado sai como "confirmar antes de publicar", não como bloqueio.
- Um mod por execução.