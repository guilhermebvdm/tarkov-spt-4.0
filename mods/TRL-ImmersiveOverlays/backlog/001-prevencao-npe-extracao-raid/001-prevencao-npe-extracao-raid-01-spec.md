# 001 — prevencao-npe-extracao-raid

**Mod:** TRL-ImmersiveOverlays
**Status:** Backlog
**Criado:** 2026-09-19

## Visão geral

Ao extrair ou finalizar uma raid (seja por sobrevivência, morte ou desconexão), o controlador de sobreposição visual tenta verificar o ponto de vista da câmera em primeira pessoa durante a desmontagem dos componentes do jogador. Como os subsistemas visuais e de câmera do personagem são descarregados pelo jogo antes da destruição total da cena, o controlador entra em falha nula contínua a cada quadro, gerando centenas de erros no console da partida até a transição para os menus. Esta tarefa visa garantir a desmontagem segura e o desligamento imediato das verificações de inventário e câmera no momento exato em que a partida for finalizada.

## Comportamento atual

- O controlador de interface visual permanece ativo de forma persistente através de todas as telas e transições.
- A cada quadro, a rotina de atualização contínua verifica se o jogador local está equipado com o item configurado e se a visualização está em primeira pessoa.
- Durante a extração ou fim de raid, os módulos de visão do jogador são destruídos pelo motor de jogo antes que a cena seja totalmente descarregada.
- A rotina de atualização continua executando no intervalo de transição de telas e tenta consultar o ponto de vista de um personagem já parcialmente desmontado, disparando exceções de referência nula ininterruptas no console de log.

## Comportamento desejado

- A rotina de verificação visual deve validar a integridade completa dos subsistemas de câmera e do estado de partida antes de consultar o ponto de vista ou inventário do jogador.
- Ao detectar que a raid foi concluída ou que a extração foi iniciada, o controlador deve interromper imediatamente todas as verificações contínuas e ocultar qualquer sobreposição visual remanescente na tela.
- Nenhuma exceção de referência nula deve ser emitida no console durante o encerramento da partida ou na transição para as telas de pós-raid.

## Critérios de aceite

- [ ] Cessar imediatamente a execução de consultas de câmera e inventário quando o jogador ou os controladores de visão entrarem em estado de desmontagem ou descarregamento.
- [ ] Ocultar e resetar o estado da sobreposição visual na tela assim que o gatilho de fim de partida ou extração for disparado, impedindo artefatos visuais no menu de debriefing.
- [ ] Concluir o ciclo de extração sem nenhum registro de exceção de referência nula proveniente do mod no console do jogo.
- [ ] **Fika/multiplayer:** Funcionar de forma transparente em sessões cooperativas hospedadas ou clientes, reagindo corretamente tanto a extrações individuais quanto a encerramentos forçados ou reinícios de servidor.
- [ ] **Estado entre raids:** O controlador deve permanecer dormente nas telas de menu pós-raid e se reativar de maneira limpa e confiável ao iniciar uma nova partida (Raid 1 → Menu/Debrief → Raid 2).

## Corner cases

- [ ] **Morte súbita do jogador:** O personagem pode morrer e ter sua câmera transferida para terceira pessoa ou visão livre de morte enquanto a tela escurece; as checagens devem suspender sem falha.
- [ ] **Desconexão abrupta ou Alt+F4:** A partida pode ser interrompida bruscamente durante o salvamento ou carregamento; o controlador deve tolerar a ausência imediata do ambiente de jogo.
- [ ] **Extração forçada via atalho (ex: F5 no Fika):** O encerramento pode ocorrer fora do fluxo nativo tradicional de zonas de extração; o mod deve reconhecer a parada de jogo sem lançar exceções.

## Fora de escopo

- [ ] Modificações no funcionamento estético, posição ou arte dos elementos visuais de sobreposição (máscaras, texturas ou transparência).
- [ ] Resolução de dependências externas entre outros mods de terceiros presentes no cliente (ex: dependências de scripts entre modificações de arrombamento de portas e scripts de rede).

## Referências

- Relatório de log de console com exceções na saída de raid: `OverlayController.cs:110` e `OverlayController.cs:82`.
- [mod-backlog.md](../mod-backlog.md)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-19 | Item criado via `/add-backlog-item` |
