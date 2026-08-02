# 003 + 004 · Code Review (rodada 01)

**Mod:** TRL-PvpMode · **Data:** 2026-08-02
**Método:** revisão adversarial por agente com contexto limpo, com auditoria completa do checklist de
rede do repo (AP-11).
**Resultado:** 1 🔴 · 6 🟡 · 5 🔵 — **todos aplicados**, exceto um critério de aceite que passou a
limitação declarada (ver fim). Build limpo.

---

## 🔴 E-01 — O conserto reintroduzia o defeito que ele existe para corrigir

Limpar o histórico de posições para impedir o deslize **também apaga a única informação que o Fika usa
para rejeitar estado atrasado**: ele só descarta um pacote fora de ordem se já houver algo no buffer.
Depois da limpeza, o primeiro estado a chegar é aceito sem questionamento — mesmo tendo sido produzido
**antes** do teleporte — e forma par com o seguinte. O corpo volta a percorrer o trajeto inteiro.

E não havia nada segurando isso: o estado de posição trafega em canal **não-confiável**, o aviso em
canal **confiável** — não existe ordem garantida entre canais diferentes. Pior, o código avisava no
**quinto** passo, depois de já ter teleportado no primeiro, então o remetente emitia estados com a
posição nova antes do aviso sair.

Falha silenciosa, com sintoma idêntico ao de não ter o mod.

**Aplicado, em duas frentes:**
1. O aviso passou a ser o **primeiro** passo do renascimento, antes do teleporte.
2. O receptor não confia só na limpeza: ele **defende a posição cravada por 1,5s**, reaplicando-a se
   o corpo aparecer a mais de 20 m dela. Um estado atrasado que escape agora é desfeito no quadro
   seguinte.

## 🟡 Aplicados

| ID | Achado | Correção |
|---|---|---|
| **E-02** | Nenhuma validação de plausibilidade: o envelope garante contagem de bytes, não valor. Um payload do tamanho certo com `NaN` dentro deixaria o corpo daquele par **permanentemente inválido** — colisor e renderização quebrados pelo resto da partida | Rejeita `NaN`, infinito e posição a mais de 2 km da origem |
| **E-03** | O Fika já limpa **duas** coisas no cenário equivalente (reconexão); o mod limpava só uma. Ícones de sangramento e fratura ficavam pendurados na plaquinha do par mesmo depois de a cura tê-los removido | Limpa também os efeitos da plaquinha |
| **E-04** | Falha de registro sem limite: `LogError` a 60–144 Hz pela sessão inteira, e o próprio volume de log causa engasgo | Desiste após 5 tentativas na mesma instância |
| **E-05** | `OnGUI` roda uma vez **por evento** (mínimo dois por quadro); o corpo inteiro, incluindo duas alocações de texto, rodava em todos, e só um pinta | Sai fora do evento de pintura; erro desarma o indicador em vez de repetir |
| **E-06** | Corner case explícito da spec 004 não cumprido: o contador ficava por cima da tela de fim de raid, porque o desligamento só acontecia no último evento da saída | Guarda de jogador vivo |
| **E-07** | O campo de rotação era **carga morta**: a escrita é revertida no quadro seguinte pelo valor interpolado da rede, e o teleporte nem muda a rotação do remetente | Campo removido do pacote |

## 🔵 Aplicados

| ID | Achado | Correção |
|---|---|---|
| **E-08** | `Teleport()` e a escrita direta miravam **o mesmo transform**; e `Teleport` num par observado ainda traz reclassificação de ambiente e reset de queda, que não fazem sentido para um boneco que não simula localmente | Só a escrita direta |
| **E-09** | O comentário prometia "não retransmitir", garantia que a marca de validade não tem — o relay do anfitrião acontece em bytes crus **antes** da decodificação | Comentário corrigido |
| **E-10** | O símbolo de infinito compila certo, mas é o único lugar do mod com símbolo (o resto escreve "ilimitadas") | Mantido; validar em partida |
| **E-11** | O aviso levava a posição *pretendida*, não a *resultante* — o controlador resolve assentamento no chão depois | Aceito: divergência de ordem de metros, e o fluxo normal reconverge |
| **E-12** | Nits: `using` desnecessário, e `broadcast: true` não é o que faz o pacote chegar a todos quando quem envia é o anfitrião | Corrigidos/comentados |

## Auditoria de rede: passou inteira

O revisor rodou o checklist do guia item a item contra o pacote. **Todos os itens de serialização
passaram**: envelope com o overload de 3 argumentos, só `TryGet*`, reset de todos os campos na
entrada, marca de validade, `struct` (não `class`), rastreio por instância, zero `UnregisterPacket`,
airbag no callback, envio na linha principal, e `check-packet-hashes.js` limpo (56 tipos, 0 colisão).

Também confirmou o que eu não tinha verificado: **o roteamento entre clientes funciona nos dois
sentidos** — o anfitrião retransmite pacotes de cliente para os demais antes de processá-los
localmente, então o buraco clássico "cliente→cliente não chega" não existe aqui. E o **servidor sem
tela processa o aviso** normalmente.

---

## ❌ Critério de aceite NÃO cumprido — vira limitação declarada

**"Bots que perseguiam o jogador antes da morte não continuam atacando o lugar antigo."**

Cravar a posição move o jogador de fato, então o bot **vê** a posição nova. Mas a memória do bot
guarda a última posição conhecida num cache próprio, que teleporte nenhum invalida — ele continua
vasculhando o ponto da morte pelo tempo de memória dele.

Resolver exige mexer na memória de inimigos dos bots, que é outro sistema e outro escopo. Registrado
como limitação conhecida no `PROPRIEDADES.md`; se incomodar em partida, vira item próprio.

## Histórico

| Data | Evento |
|---|---|
| 2026-08-02 | Review adversarial rodada 01 — 12 achados aplicados; 1 critério de aceite virou limitação declarada |
