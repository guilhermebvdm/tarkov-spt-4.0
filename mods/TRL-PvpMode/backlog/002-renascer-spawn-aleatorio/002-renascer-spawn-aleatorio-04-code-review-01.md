# 002 — Renascer em spawn aleatório · Code Review (rodada 01)

**Mod:** TRL-PvpMode · **Data:** 2026-08-02
**Método:** revisão adversarial por agente com contexto limpo.
**Resultado:** 2 🔴 · 5 🟡 · 6 🔵 — **todos aplicados**. Build limpo.

---

## 🔴 D-01 — O ponto não era sorteado: era sempre o mesmo

A implementação chamava `ISpawnSystem.SelectSpawnPoint` cinco vezes passando um identificador
aleatório, acreditando que isso produzia variação e afastamento de inimigos. **As duas premissas eram
falsas.**

Com os argumentos disponíveis (sem grupo, sem time), a busca cai num ramo que devolve **o ponto mais
distante de todos os jogadores** — puro argmax, sem sorteio. Chamá-la cinco vezes no mesmo quadro
devolve cinco vezes o mesmo ponto. O identificador aleatório só influencia o desvio de transição,
nada de distância.

Duas consequências: o laço de cinco tentativas era **código morto** que pagava cinco vezes o custo de
uma varredura sobre todos os pontos × todos os jogadores e bots; e como o próprio jogador entrava na
conta de distância, ele renascia **sempre no mesmo canto extremo do mapa**, morte após morte.

O título do item — "spawn **aleatório**" — não estava sendo entregue.

**Aplicado:** sorteio de verdade. Montamos a lista de candidatos (categoria de jogador + lado
compatível), filtramos por distância mínima de quem está vivo, e escolhemos ao acaso. Se nenhum ponto
atender à distância, o filtro é relaxado em vez de impedir o renascimento. Nova opção no F12:
`Min Spawn Distance (m)`, padrão 80.

## 🔴 D-02 — O teto de revives do Fika anulava o contador de vidas em silêncio

`CanBeDowned` tem três termos; patchávamos só um. O segundo — `(_maxRevives == 0 || _revives < _maxRevives)`
— é alimentado pelo `maxRevives` do `fika.jsonc`, e **cada renascimento nosso incrementa esse contador**.

Com `maxRevives: 2` no servidor e 5 vidas no F12, o jogador renasceria duas vezes e na terceira morte
morreria de vez **com 3 vidas ainda no contador e no indicador de tela**. Sem log, sem aviso.

Hoje o servidor está com `maxRevives: 0` (ilimitado), então não morde — mas "hoje" não é garantia.

**Aplicado:** o teto é zerado no início da raid, passando a contagem inteiramente para o nosso
contador. É a mesma postura já adotada no resto do mod: quando dividimos uma decisão com o Fika,
assumimos ela por inteiro. Se a escrita falhar, avisa na tela.

## 🟡 Aplicados

| ID | Achado | Correção |
|---|---|---|
| **D-03** | O Fika **nunca limpa** `Downed` quando o prazo acaba. Por 1–2 quadros o jogador segue na lista de vivos com `Downed == true`, já com cadáver criado e morte anunciada — completar a tecla nessa janela renasceria em cima do próprio cadáver | Teste de `IsAlive` antes do teste de `Downed` |
| **D-04** | O jogador renascia **deitado**: `ToggleDowned(true)` aplica a pose prone e o ramo de saída não a desfaz | `IsInPronePose = false` + pose de pé após religar |
| **D-05** | `RestoreFullHealth` remove só sangramento — **fratura, dor e intoxicação sobreviviam** ao renascimento. Perna quebrada mancando com 100% de vida | `RemoveNegativeEffects` no respawn |
| **D-06** | A vida era debitada **antes** de cinco passos que podem falhar; o `catch` engolia o meio do caminho, deixando "vida gasta, teleportado e ainda caído" | Débito movido para depois do ponto de não-retorno |
| **D-07** | Leitura de teclado crua: digitar no chat com a tecla rebindada gastaria uma vida. O próprio Fika guarda três estados antes de ler teclado | Guardas de inventário, console e chat |

## 🔵 Aplicados

| ID | Achado | Correção |
|---|---|---|
| **D-08** | O detector de falha do sorteio era acidental — a busca nunca devolve nulo, devolve um objeto sentinela com posição na origem | Teste explícito de posição na origem, comentado |
| **D-09** | `Reset()` largava a proteção sem devolver o coeficiente de dano | Restaura antes de largar |
| **D-10** | Aviso de "sem vidas" inalcançável (o chamador já barrava antes) | Removido |
| **D-11** | Falha de sorteio com a tecla segurada re-notificava em laço | Antirrepique de 3s |
| **D-12** | Comentário e spec descreviam ordem diferente da implementada | Reconciliados |
| **D-13** | Proteção de spawn é de mão única (não sofre dano, mas atira) — explorável | Registrado como decisão consciente na spec |

## Correção de entendimento (não era defeito de código)

A spec técnica justificava a ordem "teleportar antes de religar" dizendo que ela evitaria o deslize.
**A justificativa estava errada:** o aviso de "levantei" sai *dentro* de `ToggleDowned`, enquanto a
posição só sai no tique seguinte — então o par recebe a mudança de estado antes da posição de
qualquer jeito. A ordem é inócua para esse fim (e continua correta por outros motivos). Quem resolve
o deslize de fato é o pacote de sincronia do item 003.

## Histórico

| Data | Evento |
|---|---|
| 2026-08-02 | Review adversarial rodada 01 — 13 achados, todos aplicados |
