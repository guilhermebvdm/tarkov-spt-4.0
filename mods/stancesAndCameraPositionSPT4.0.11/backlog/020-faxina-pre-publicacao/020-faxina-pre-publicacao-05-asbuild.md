# 020 — Faxina pré-publicação · As-build

**Mod:** stancesAndCameraPositionSPT4.0.11 · **Versão:** v2.15.0 · **Data:** 2026-08-02
**Spec:** [01-spec.md](020-faxina-pre-publicacao-01-spec.md)

## Arquivos tocados

| Arquivo | O que mudou |
|---|---|
| `modded/CameraBobbingScript.cs` | **Removido** (F1) — `MonoBehaviour` nunca instanciado |
| `modded/Patches/PlayerSpringPatch.cs` | F2 — `_cameraOffsetField` (resolvido, nunca lido) removido |
| `modded/Plugin.cs` | F3 (`FixedUpdate` vazio) · F4 (laço protegido) · F5 (aparato do F12) |
| `modded/ThrottledLog.cs` | **Novo** (F4) — log com limite de repetição, extraído do módulo de rede |
| `modded/Networking/FikaSyncManager.cs` | F4 — `LogErrorThrottled` passa a delegar para `ThrottledLog` |
| `modded/StanceManager.cs` | F4 — `ThrottledLog.Reset()` no início de raid |

## Decisões tomadas durante a execução

1. **Sem helper `Tick(Action, string)` no laço principal.** A primeira versão usava um helper com delegate por
   subsistema; isso alocaria **7 delegates por quadro** (um deles capturando `this`) no caminho mais quente do
   mod — trocaria um bug por lixo de coletor. Escrito na mão, um `try/catch` por linha.
2. **Um `try` por subsistema, não um envolvendo os sete.** Um bloco único reproduziria o mesmo efeito dominó
   dentro do `try` — o objetivo é justamente que a falha de um não cancele os outros.
3. **O mecanismo de log foi extraído, não duplicado.** Já existia em `FikaSyncManager` (v2.11.0) para erros de
   rede. Virou `ThrottledLog`, e o módulo de rede passou a delegar — mesma mecânica, um dono só. Os dois
   compartilham o balde de supressão de propósito: o que importa é o console não afogar.
4. **`ThrottledLog` sem `Initialize`/campo de logger próprio.** Usa `Plugin.Logger` direto. A primeira versão
   tinha um `Initialize` que ninguém chamava — código morto criado dentro da faxina de código morto.
5. **`ThrottledLog.Reset()` no início de raid**, senão um tipo de erro visto na raid anterior nunca mais
   renderia rastreamento completo.
6. **`using` órfãos não removidos.** Não têm custo em runtime e mexer neles amplia a superfície de mudança sem
   ganho verificável in-game.
7. **Sem branch dedicada.** Há sessão paralela ativa no mesmo checkout; criar branch agora atrapalharia mais
   que ajudaria. Commits cirúrgicos, separando os hunks alheios.

## Verificação

- **Build:** `dotnet build -c Release` → **0 erros, 0 avisos**.
- **`Browsable` é `bool?`** (`References/ConfigurationManagerAttributes.cs:90`) — ao remover o aparato, os
  atributos passaram a ser criados sem esse campo, o que significa `null` = **visível por padrão**. Se fosse
  `bool` não-nulável, as 4 opções de ciclo teriam sumido do F12 sem erro de compilação. **Este era o risco
  principal do F5 e está descartado.**
- **`Order` preservado** nos quatro binds (65, 64, 63, 62) — a ordem dentro da seção não muda.
- **Ordem de descoberta das seções intacta** — nenhum `Config.Bind` mudou de posição no `Awake`, então a ordem
  das seções no F12 é a mesma.

## Pendente de validação in-game

1. **F12 idêntico** — 18 seções, mesma ordem; os 4 interruptores de ciclo (`Include Stance 0…`, `Enable Stance
   1/2/3…`) visíveis na seção `Stance Cycle & Hotkeys`, e os valores salvos preservados.
2. **Raid completa** — posturas, mira, mount, recarga, checagem de câmara: tudo como na v2.14.0.
3. **Posição de câmera** — a seção `Camera Position` continua respondendo (F2 mexeu nesse patch).
4. ⚠️ **Erro novo no log provavelmente é antigo.** Com o laço protegido, uma exceção que antes passava
   despercebida (cancelando silenciosamente os subsistemas seguintes) agora aparece registrada. Se surgir algo
   no console depois desta versão, a hipótese primeira é que já existia.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-02 | Guilherme | Criação — execução das 5 frentes via `/g-autodev`, build limpa, pendente validação in-game |
