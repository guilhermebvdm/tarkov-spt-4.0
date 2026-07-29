# 032 — Velocidade de download nunca funcionou (retomar o item 016) · Kickoff

**Launcher:** Launcher4.0-v2 (v2.7.3) · **Data:** 2026-07-28 · **Origem:** relato do usuário (2026-07-28) · **Deps:** 007 (motor de sync), 016 (implementação original)

> Brief de kickoff — insumo para `/create-spec`. Não é a spec.

## Sintoma relatado

"A velocidade de download, desde que a gente implementou, nunca funcionou — quando a gente está fazendo o download propriamente dito." O item [016](../016-velocidade-download-verificacao/) foi marcado 🟢 em 2026-07-04, mas a taxa **nunca apareceu de forma útil em produção**.

## Estado real do código (verificado 2026-07-28)

O motor **existe e está vivo**; a **UI foi arrancada**.

- ✅ `DownloadRateMeter` completo (média móvel de janela 5, MB/s decimal, fallback KB/s, formatação PT-BR) — [DownloadRateMeter.cs](../../project/SPT.Launcher.Base/Sync/DownloadRateMeter.cs), com 13 testes verdes ([DownloadRateMeterTests.cs](../../project/SPT.Launcher.Tests/Sync/DownloadRateMeterTests.cs)).
- ✅ Medição plugada nos dois fluxos — `WithSpeedMeter` envolve o downloader como camada mais externa em [ProfileViewModel.cs:778-798](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L778-L798) e [ModUpdateViewModel.cs:419](../../project/SPT.Launcher/ViewModels/ModUpdateViewModel.cs#L419).
- ✅ Propriedades expostas nos VMs: `DownloadSpeedText` / `HasDownloadSpeed` / `DownloadBytesPerSec`.
- 🔴 **Nenhum `.axaml` faz binding em `DownloadSpeedText`.** Os dois `TextBlock` foram removidos e substituídos por comentário:
  - [ProfileView.axaml:216](../../project/SPT.Launcher/Views/ProfileView.axaml#L216) — `<!-- velocidade de download removida a pedido -->`
  - [ModUpdateView.axaml:29](../../project/SPT.Launcher/Views/ModUpdateView.axaml#L29) — idem.
  - Removido no commit **`2f43a158`** (2026-07-05): *"feat(launcher): mensagem de sucesso ao concluir sync + remove taxa de download (016)"* — **um dia depois** de o 016 ser entregue. O motivo registrado é só "a pedido"; a hipótese é que foi pedido justamente porque nunca mostrava nada de útil.

Ou seja: hoje o launcher **mede** a taxa a cada arquivo, joga no VM, e ninguém lê. Custo zero de conserto na parte de motor; o problema real é o **modelo de medição**.

## Por que provavelmente nunca foi útil (causa candidata)

A medição é **por arquivo concluído**, não intra-arquivo:

- O downloader é `RequestHandler.DownloadModFile` ([RequestHandler.cs:207](../../project/SPT.Launcher.Base/Controllers/RequestHandler.cs#L207)), que retorna `byte[]` — bufferiza o arquivo **inteiro** antes de devolver.
- `WithSpeedMeter` só chama `AddSample(bytes, elapsed)` **depois** que o arquivo terminou ([ProfileViewModel.cs:782-786](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L782-L786)).

Consequências:
1. Num sync típico (poucos arquivos pequenos: configs, DLLs de alguns KB) cada amostra chega e some em milissegundos — a taxa nunca fica legível.
2. Amostras com `elapsed <= 0` são **descartadas** por design ([DownloadRateMeter.cs:41-44](../../project/SPT.Launcher.Base/Sync/DownloadRateMeter.cs#L41-L44)) — arquivos instantâneos não geram taxa nenhuma.
3. No cenário oposto (um bundle grande de dezenas de MB) a barra fica **parada, sem taxa, durante todo o download** e só mostra um número quando o arquivo já acabou — exatamente o inverso do que o usuário quer ver.
4. Ao fim do run, `DownloadSpeedText = ""` ([ProfileViewModel.cs:740](../../project/SPT.Launcher/ViewModels/ProfileViewModel.cs#L740)) — o último valor some.

Isto foi assumido conscientemente no as-built do 016 (**A-016.8**: *"granularidade por arquivo … intra-arquivo descartado por custo na Base compartilhada"*) e é essa decisão que precisa ser revisitada.

## Direções para a spec

- **Medir intra-arquivo (streaming).** Trocar/duplicar o caminho `DownloadModFile` por uma leitura em chunks que reporte bytes acumulados durante a transferência, alimentando o `DownloadRateMeter` a cada N ms ou N KB. Avaliar o impacto na `SPT.Launcher.Base` (é código compartilhado com o upstream).
- **Ticker de UI independente do arquivo.** A taxa deve atualizar em cadência fixa (~500 ms) mesmo quando um único arquivo grande está em voo, e cair para 0/oculto quando não há transferência.
- **Rebinding da UI** nos dois pontos removidos, com o token `.trl-mono` já previsto no 016 — e decidir se entra também bytes restantes / ETA (o usuário só pediu velocidade; ETA é escopo a confirmar).
- **Reaproveitar o que já passa nos testes**: o `DownloadRateMeter` e sua formatação PT-BR não precisam mudar — só a fonte das amostras.
- **Gate humano**: validar contra o servidor real com um arquivo grande o suficiente (bundle) — o gate P-016.1 do item 016 nunca foi fechado.

## Relação com o item 016

Este item **substitui** o 016 na prática. Ao entregar, atualizar o 016 para ⚫ (superado por 032) ou deixar 🟢 com nota de "UI revertida em `2f43a158`, retomada no 032" — decidir na spec.
