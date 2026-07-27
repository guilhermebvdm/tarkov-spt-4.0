# /update-me-about-this-mod

Gera um relatório de status **em linguagem de produto** (não técnica) sobre um mod: o que está **pendente de teste**
(coisas já prontas, esperando validação in-game do usuário) e o que está **pendente de desenvolvimento** (coisas
que ainda não foram construídas). É o command que o usuário roda para "se atualizar" sobre um mod antes de decidir
o que fazer na sessão — não assume que ele lembra o jargão técnico ou os IDs internos.

> **Só leitura.** Este command nunca edita `sessions.md`, `mod-backlog.md` nem nenhum artefato. Se algo parecer
> desatualizado, reportar como observação — não corrigir automaticamente.

## Uso

```bash
/update-me-about-this-mod [<mod-ou-caminho>]
```

- `/update-me-about-this-mod` (sem args) — usa o **mod da sessão atual**: o mod mais recentemente tocado por path
  explícito ou command direcionado nesta conversa (mesma heurística de detecção da skill `memory-curation` §3,
  pontos 1-2). Se a sessão não tocou nenhum mod ainda, perguntar qual mod o usuário quer.
- `/update-me-about-this-mod <mod-ou-caminho>` — aceita o nome da pasta (`stancesAndCameraPositionSPT4.0.11`) ou
  um caminho que contenha `mods/<mod>/...`. Extrair o slug do mod do caminho se necessário.
- Se o mod informado (ou detectado) não existir em `mods/`, listar os mods disponíveis (`ls mods/`) e perguntar.

## O que fazer

### 1. Resolver o mod-alvo

1. Se args foi passado: extrair o slug (`mods/<slug>/...` → `<slug>`; ou o próprio nome se já for o slug).
2. Se sem args: olhar a conversa atual em busca do último mod tocado (path `mods/<X>/...` em edits/reads/greps, ou
   o último `/code-mod <X>`, `/compile-mod <X>`, etc.). Se **nenhum** mod foi tocado ainda nesta sessão, perguntar
   ao usuário: "De qual mod você quer o resumo?" com a lista de `mods/*/`.
3. Confirmar que `mods/<slug>/` existe. Se não existir, tratar como erro de digitação — sugerir o mais parecido.

### 2. Coletar as fontes

Ler, nesta ordem:

1. `mods/<slug>/memory/sessions.md` — **só o topo**: blocos "Estado atual" e "Pendências / próximos passos
   conhecidos" (skill `memory-curation` §6/§7). Não ler o histórico de sessões inteiro — o snapshot já resume.
2. `mods/<slug>/backlog/mod-backlog.md` — tabela de itens e a legenda de status (⚪ Backlog · 🟡 Em progresso ·
   🟢 Entregue · 🔴 Cancelado), e a seção "Estado" no rodapé se existir.
3. Se algum dos dois arquivos não existir, seguir sem ele e registrar isso no relatório ("este mod ainda não tem
   backlog formal" / "este mod ainda não tem memória registrada") em vez de falhar.

### 3. Classificar cada item em UM dos dois grupos

**Pendente de teste** — o trabalho de código já foi feito; falta o usuário (ou alguém) confirmar que funciona
como esperado, dentro do jogo. Entram aqui:

- Pendências de memória marcadas como validação/gate humano (linguagem típica: "validar in-game", "testar em
  raid", "gate", "confirmar", "🔴 aberta esperando teste"), MESMO que a categoria de risco (🔴/🟡/🟢) na memória
  seja técnica — o critério aqui é "o código existe, falta confirmar o comportamento".
- Itens do backlog com status 🟢 (Entregue) cuja pendência de memória associada ainda não foi fechada.

**Pendente de desenvolvimento** — ainda falta escrever ou terminar código. Entram aqui:

- Itens do backlog com status ⚪ (Backlog) ou 🟡 (Em progresso).
- Pendências de memória que descrevem um **bug ainda não corrigido** ou uma **investigação não feita** (linguagem
  típica: "não investigado a fundo", "ainda não corrigido", "hipótese não confirmada", "falta decidir a
  abordagem").
- **Não incluir** itens 🔴 (Cancelado) em nenhum dos dois grupos — mencionar só se o usuário perguntar
  especificamente pelo histórico.

Quando um item tem elementos dos dois (parte já pronta pendente de teste, parte ainda por fazer), split-lo:
mencionar a parte pronta em "Pendente de teste" e a parte faltante em "Pendente de desenvolvimento", deixando
claro que é o mesmo item nos dois lugares.

### 4. Traduzir para linguagem de produto

Esta é a parte que mais importa — **o público-alvo é o usuário decidindo o que priorizar, não um dev lendo
código.** Para cada item:

- **Nunca** usar IDs crus sem explicação (`P-11.6`, `017-transicao-ads-cirurgica`, nomes de classe/método,
  hashes de commit). Se precisar citar o ID para rastreabilidade, colocar **entre parênteses no fim**, depois da
  descrição em português simples — nunca como a única referência.
- Descrever o **comportamento observável no jogo**, não a implementação: em vez de "`AdsWaypoint.Update` lê a
  config por stance", escrever "quando você mira vindo do Pronto Alto ou Pronto Baixo, a transição deveria seguir
  o tempo configurado para aquela postura especificamente".
- Formular como **critério de aceite** sempre que possível — uma frase que o usuário consiga testar e responder
  sim/não: "Ao checar a câmara vazia usando o ferrolho manual, o jogo deveria mostrar 'sem munição' no mesmo
  painel usado para checar o carregador." (não "implementar `OnShowAmmoDetails` no chamber-check").
- Se a pendência tiver um passo prévio necessário para testar (ex.: bindar uma tecla que não vem por padrão),
  destacar isso como **pré-requisito do teste**, separado da descrição do comportamento esperado.
- Agrupar por **tema/feature**, não por ID de sessão — o usuário pensa em "a mira", "as posturas", "a velocidade
  ao andar", não em "Sessão 11 cont. 6".
- **Cada item cabe em uma frase.** O relatório é uma tabela (§7) — uma linha por item, sem sub-bullets. Se a
  explicação não couber em ~100 caracteres, encurtar para o essencial e jogar o resto numa nota abaixo da tabela
  (§7, "Notas de linha").

### 5. Marcar o texto (`código` e **negrito**)

O relatório é lido em terminal, escaneando. A marcação existe para o olho parar no lugar certo — marcação demais
tem o mesmo efeito de marcação nenhuma. Duas regras, sem exceção:

| Marcação | Usar para | Exemplos |
|---|---|---|
| `` `código` `` | Tudo que é **literal** e copiável: teclas, nomes de config/arquivo/pasta, valores, códigos de item e referências internas | `Alt + V`, `LeanSpeed`, `config.json`, `P-11.6`, `034`, `v1.4.2` |
| **negrito** | O **conceito de domínio** ou o **verbo da decisão** — o que o usuário reconhece do jogo, e o estado do item | **Pronto Alto**, **Inclinar**, **não investigado**, **já corrigido**, **trava** |

- **Máximo 2 negritos por célula.** Se tudo está em negrito, nada está.
- **Nunca** negrito em referência interna (`P-11.6` é `código`, não negrito) nem em frase inteira.
- Nome de classe, método ou hash de commit **não entram no relatório** — nem em `código`. São jargão de dev; a §4
  já manda traduzir para comportamento observável. A única referência técnica permitida é o ID da pendência ou do
  item de backlog, na coluna `Ref`.

### 6. Obter timestamp e versão atual

Antes de montar o relatório:

1. Rodar `date '+%Y-%m-%d %H:%M'` para o horário exato (GMT-3, fuso do usuário) — **nunca estimar**. É o mesmo
   princípio do `/update-memory` §4: o relógio é a fonte de verdade.
2. Extrair a **versão atual do mod** do snapshot "Estado atual" em `sessions.md` (procurar por "DLL atual",
   "versão atual", número de versão semver mais recente mencionado). Se não houver versão explícita na memória,
   tentar o `BepInPlugin`/`.csproj` do mod (`Version`/`AssemblyVersion`) como fallback; se nenhum dos dois existir,
   escrever "versão não identificada" em vez de adivinhar.

### 7. Montar o relatório

Saída em **uma tabela única**, em português, para o usuário — não para outro agente.

#### Códigos de linha

Cada item ganha um código curto, que serve para o usuário responder ("vamos fazer o `D-2`", "o `T-1` passou"):

- `T-1`, `T-2`, … — **pendente de teste** (código pronto, falta confirmar no jogo).
- `D-1`, `D-2`, … — **pendente de desenvolvimento** (falta construir).

Numerar sequencialmente na ordem de apresentação, começando em 1 em cada prefixo. **O código é local a este
relatório** — vale para a conversa de agora, não é ID persistente e não vai para arquivo nenhum. A rastreabilidade
de verdade é a coluna `Ref`; se o usuário citar um código numa sessão futura, reconferir na tabela atual.

#### Colunas

| Coluna | Conteúdo | Alvo de tamanho |
|---|---|---|
| `#` | `🧪 T-N` ou `🔧 D-N` | fixo |
| `Tema` | Feature em linguagem de jogo: Mira, Posturas, Câmara, Velocidade | ≤ 14 caracteres |
| `O que testar / o que falta` | Uma frase, critério de aceite quando for teste | ≤ 100 caracteres |
| `Pré-req` | Passo prévio necessário (tecla a bindar, config a ligar), ou `—` | ≤ 20 caracteres |
| `Ref` | ID da pendência de memória ou do item de backlog, em `código` | ≤ 12 caracteres |

Ordem das linhas: **todos os `T-*` primeiro**, depois os `D-*`; dentro de cada bloco, agrupados por tema.

#### Formato

```markdown
# <nome do mod, em linguagem natural> — status

> **Resumo de:** YYYY-MM-DD HH:MM (Brasília) · **Versão atual:** `vX.Y.Z`<br>
> 🧪 **pronto, falta testar:** N · 🔧 **ainda por fazer:** M<br>

| # | Tema | O que testar / o que falta | Pré-req | Ref |
|---|---|---|---|---|
| 🧪 `T-1` | Mira | Mirando do **Pronto Alto**, a subida deve seguir o tempo daquela postura | `Alt + V` | `P-11.6` |
| 🧪 `T-2` | Câmara | Ferrolho manual mostra "sem munição" no mesmo painel do carregador | — | `P-12.1` |
| 🔧 `D-1` | Posturas | **Inclinar** **trava** ao subir escada — causa ainda **não investigada** | — | `034` |
| 🔧 `D-2` | Velocidade | Peso da mochila não afeta o passo lateral | — | `041` |

**Legenda:** 🧪 pronto, falta você confirmar no jogo · 🔧 ainda falta construir

### Notas de linha
- `T-1`: <detalhe que não coube na linha — passo extra de teste, ressalva, dependência>

### Notas
- <observações fora dos dois grupos: memória desatualizada, backlog ausente, item cancelado relevante>
```

- **Uma linha por item.** Item com parte pronta e parte por fazer vira **duas linhas** (um `T-*` e um `D-*`) com a
  mesma `Ref`, e o texto de cada uma deixa claro qual metade é.
- **"Notas de linha" é escape, não regra** — só quando a frase realmente não cabe. Se metade das linhas tiver nota,
  as frases estão longas demais: encurtar em vez de anotar.
- **Grupo vazio não vira seção vazia** — sai como linha de texto abaixo da tabela: "Nada pendente de teste agora —
  tudo que foi entregue já foi validado." / "Nada pendente de desenvolvimento — todo o backlog conhecido foi
  entregue." Se **os dois** estiverem vazios, não montar tabela nenhuma: só as duas frases.
- **É uma tabela de produto, não técnica.** A coluna do meio se lê como uma frase que o usuário consegue responder
  sim/não depois de entrar no jogo — não como um resumo de commit.
- Não forçar a seção "Notas" quando não há observação.

### 8. Não modificar nada

Este command é read-only. Se durante a leitura for notado que a memória ou o backlog parecem desatualizados
(ex.: um item 🟢 no backlog mas a memória ainda lista pendência de teste antiga, ou uma pendência sem data), citar
isso na seção "Notas" — não editar o arquivo. Se o usuário quiser corrigir, é o `/update-memory` ou edição manual
que fazem isso.

## Regras

- **Só leitura** — nunca grava em `sessions.md` ou `mod-backlog.md`.
- **Linguagem de produto sempre** — se uma frase do relatório teria que ser explicada para o usuário entender,
  reescrever antes de mostrar.
- **Uma tabela, uma linha por item** (§7) — nada de bullets aninhados ou parágrafo dentro de célula.
- **`código` para literal, negrito para conceito** (§5), no máximo 2 negritos por célula.
- **Códigos `T-N`/`D-N` são locais ao relatório** — servem para o usuário apontar um item na conversa, não são IDs
  persistentes e nunca são gravados.
- **Um mod por vez.** Sem flag `--all` — se o usuário quiser vários mods, rodar o command de novo para cada um.
- **Itens cancelados (🔴) ficam fora** dos dois grupos por padrão.
- **Se um dos dois arquivos-fonte não existir**, seguir com o que houver e avisar na seção "Notas" — não é erro
  bloqueante.

## Exemplos

```bash
# Mod da sessão atual (auto-detect)
/update-me-about-this-mod

# Mod específico por slug
/update-me-about-this-mod stancesAndCameraPositionSPT4.0.11

# Mod específico por caminho
/update-me-about-this-mod mods/TRL-ImmersiveCombatMedicine/
```