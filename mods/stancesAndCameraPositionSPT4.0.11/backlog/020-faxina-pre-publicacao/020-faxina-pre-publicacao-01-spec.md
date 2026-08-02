# 020 — Faxina pré-publicação · Spec funcional

**Mod:** stancesAndCameraPositionSPT4.0.11
**Data:** 2026-08-02
**Origem:** onda 2 do plano de publicação no SPT Forge (ver `memory/sessions.md`, Sessão 12 e [P-12.2])

> Item de **higiene de código**, não de gameplay. Nada aqui muda o que o jogador vê ou sente — o critério de
> aceite geral é **"o jogo se comporta exatamente como antes"**. O que muda é o que um revisor de terceiro
> encontra ao abrir o repositório, e o comportamento do mod quando algo dá errado em raid.

## Por que existe

O mod vai a público no SPT Forge. Código morto, laço principal sem proteção contra falha e mecanismo inerte
são o tipo de coisa que vira issue de terceiro e desgasta a percepção do mod — além de, no caso do laço
principal, produzir um modo de falha real em raid.

## Escopo

Cinco frentes, todas confirmadas com evidência em 2026-08-01/02. **Três alvos que a memória listava foram
verificados e caíram** (ver §Fora de escopo) — o levantamento vale mais que o registro antigo.

### F1 — Remover a classe de balanço de câmera nunca usada

`modded/CameraBobbingScript.cs` (20 linhas) define um `MonoBehaviour` que **nunca é instanciado**: não há
`new CameraBobbingScript` nem `AddComponent<CameraBobbingScript>` em lugar algum do mod. Não existe no
upstream (`original/`) — é código do fork que nunca foi ligado.

**Critério de aceite:** o arquivo deixa de existir; o mod compila; nenhuma referência órfã sobra.

### F2 — Remover o campo de reflexão resolvido e nunca lido

`Patches/PlayerSpringPatch.cs:13-18` resolve `_cameraOffsetField` via `AccessTools.Field` e **nunca o usa** —
o Postfix escreve `__instance.CameraOffset` diretamente.

**Critério de aceite:** o campo e sua resolução somem; o offset de câmera continua sendo aplicado igual
(conferir em raid que a posição da câmera responde às opções da seção `Camera Position`).

### F3 — Remover o `FixedUpdate` vazio

`Plugin.cs:1503-1506` declara `FixedUpdate` sem corpo. O Unity chama esse método a cada passo de física
(~50×/s) para executar nada — o custo é pequeno, mas é gratuito de eliminar e confunde quem lê.

**Critério de aceite:** o método some; nada mais muda.

### F4 — Proteger o laço principal contra exceção

`Plugin.Update` chama sete subsistemas em sequência sem `try/catch` (achado **CR-07**, nunca aplicado). Uma
exceção no primeiro cancela os seis seguintes — **a cada quadro**, indefinidamente, porque nada interrompe o
laço. É o formato de falha que produz "o mod parou de funcionar no meio da raid".

**Decisão de escopo (usuário, 2026-08-02): proteger o laço principal e os patches que escrevem estado
compartilhado. Os patches de leitura pura e os de câmera já validados ficam como estão** — `try/catch` em
caminho de animação tende a mascarar erro em vez de expor, e esse código está validado há meses.

**Critérios de aceite:**
- Uma exceção em qualquer subsistema **não** impede os demais de rodarem naquele quadro.
- O log **não** floda: primeira ocorrência de cada tipo de erro sai completa (com rastreamento de pilha),
  repetições são limitadas por tempo — o padrão que a v2.11.0 já usa nos erros de rede.
- Nenhum `catch` silencioso: erro engolido sem registro é proibido.

### F5 — Remover o aparato inerte de visibilidade do F12

Achado **CR-05**, nunca aplicado. O mecanismo que mostrava/escondia opções conforme o modo de scroll virou
inofensivo (`Browsable` é fixado em `true` para todas), mas ainda **força a reconstrução do menu** a cada
mudança do modo de scroll.

**Critério de aceite:** todas as opções continuam visíveis no F12, na mesma ordem e nas mesmas seções; o menu
deixa de ser reconstruído sem motivo. ⚠️ **Não pode alterar a ordem de descoberta dos `Config.Bind`** — é ela
que define a ordem das seções no F12.

## Fora de escopo (verificado e descartado)

| Alvo registrado | Por que saiu |
|---|---|
| `ApplySimpleRotationPatch` com amortecimento fixo `12f` | **Já corrigido** (CR-08): hoje lê `Plugin._StanceOvershootDamping?.Value ?? 12f` |
| Comentário "Stance 0: irrelevante" seria falso | **Obsoleto** — o texto atual é qualificado e correto (fala do waypoint de ADS, cujo bind é nulo na Stance 0) |
| "Reflexão rodando a cada quadro" | **Impreciso** — as resoluções acontecem uma vez, em `GetTargetMethod`. Por quadro só há leitura/escrita em campo já resolvido, que é o padrão aceitável |
| Separar o `Plugin.cs` (1720 linhas) | **Adiado por decisão do usuário** (2026-08-02) — vira item próprio, com o mod já publicável. Não misturar refatoração grande com correções na mesma validação in-game |

## Critério de aceite geral

1. **Nada muda para o jogador.** Uma raid completa com posturas, mira, mount, recarga e checagem de câmara se
   comporta exatamente como na v2.14.0.
2. **O F12 fica idêntico** — mesmas seções, mesma ordem, mesmas opções, mesmos valores salvos preservados.
3. A build sai sem avisos.

## Riscos

- **F5 é o de maior risco:** mexer nos atributos do ConfigurationManager pode alterar a ordem das seções, que
  depende da ordem de descoberta dos binds. Conferir o F12 lado a lado antes/depois.
- **F4 pode mascarar um erro existente:** se algum subsistema já lança exceção hoje e ninguém notou, o
  `try/catch` vai revelá-la no log — o que é bom, mas pode parecer "regressão nova". Erro que aparecer no log
  depois desta faxina provavelmente já existia.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-02 | Guilherme | Criação — levantamento da onda 2 do plano de publicação, com 3 alvos descartados por verificação e 4 decisões de escopo tomadas por múltipla escolha |
