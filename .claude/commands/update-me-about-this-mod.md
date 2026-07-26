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

### 5. Obter timestamp e versão atual

Antes de montar o relatório:

1. Rodar `date '+%Y-%m-%d %H:%M'` para o horário exato (GMT-3, fuso do usuário) — **nunca estimar**. É o mesmo
   princípio do `/update-memory` §4: o relógio é a fonte de verdade.
2. Extrair a **versão atual do mod** do snapshot "Estado atual" em `sessions.md` (procurar por "DLL atual",
   "versão atual", número de versão semver mais recente mencionado). Se não houver versão explícita na memória,
   tentar o `BepInPlugin`/`.csproj` do mod (`Version`/`AssemblyVersion`) como fallback; se nenhum dos dois existir,
   escrever "versão não identificada" em vez de adivinhar.

### 6. Montar o relatório

Formato de saída (em português, para o usuário — não para outro agente):

```markdown
# Status de <nome do mod, em linguagem natural> — para você conferir

> **Horário deste resumo:** YYYY-MM-DD HH:MM (horário de Brasília)<br>
> **Versão atual:** vX.Y.Z<br>

## 🧪 Pendente de teste (já está pronto, falta você confirmar no jogo)

### <Tema 1>
- **O que testar:** <descrição observável, critério de aceite>.
  <se houver pré-requisito: "Antes: <pré-requisito>.">
  (ref. interna: <ID>)

### <Tema 2>
- ...

(Se nada estiver pendente de teste: "Nada pendente de teste agora — tudo que foi entregue já foi validado.")

## 🔧 Pendente de desenvolvimento (ainda por fazer)

### <Tema 1>
- **O que falta:** <descrição em linguagem de produto, sem jargão>.
  (ref. interna: <ID ou item de backlog>)

### <Tema 2>
- ...

(Se nada estiver pendente: "Nada pendente de desenvolvimento agora — todo o backlog conhecido foi entregue.")

## Notas
- <observações que não cabem nos critérios acima: memória desatualizada, backlog ausente, itens cancelados
  relevantes ao contexto, etc. — opcional>
```

- Não incluir seções vazias além da mensagem de "nada pendente" — não forçar uma seção "Notas" vazia.
- **Não é uma tabela técnica.** Prosa curta com bullets, como se estivesse explicando para alguém que não vai
  abrir o código.

### 7. Não modificar nada

Este command é read-only. Se durante a leitura for notado que a memória ou o backlog parecem desatualizados
(ex.: um item 🟢 no backlog mas a memória ainda lista pendência de teste antiga, ou uma pendência sem data), citar
isso na seção "Notas" — não editar o arquivo. Se o usuário quiser corrigir, é o `/update-memory` ou edição manual
que fazem isso.

## Regras

- **Só leitura** — nunca grava em `sessions.md` ou `mod-backlog.md`.
- **Linguagem de produto sempre** — se uma frase do relatório teria que ser explicada para o usuário entender,
  reescrever antes de mostrar.
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