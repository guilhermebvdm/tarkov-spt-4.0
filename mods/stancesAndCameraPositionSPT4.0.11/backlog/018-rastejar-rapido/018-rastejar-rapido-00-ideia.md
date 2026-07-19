# 018 — Rastejar rápido (crawl + run / high-crawl)

> **Data:** 2026-07-19<br>
> **Status:** ⚪ Backlog (ideia bruta — não investigado)<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [005-velocidade-agachar-inclinar](../005-velocidade-agachar-inclinar/), [012-controlador-central-stamina](../012-controlador-central-stamina/)<br>

---

## Ideia (do usuário)

Enquanto **rastejando** (prone), permitir um **rastejar acelerado** — um "crawl + run" (análogo ao *high-crawl*
tático). **Gatilho:** apertar **andar para frente + agachado + correr** (sprint) estando deitado. Hoje o movimento
prone do EFT é muito lento; a ideia dá mobilidade tática sem precisar levantar.

## Perguntas em aberto (fechar na spec)

- **Gatilho exato:** qual combinação de inputs dispara? "Forward + prone + sprint key" — como detectar sem colidir
  com o sprint em pé nem com o agachar? Segurar ou toggle? (ver o modelo de input das hotkeys do item 002).
- **O EFT tem estado/animação de high-crawl nativo**, ou só o low-crawl lento? Investigar `MovementContext` /
  estados de prone no Assembly real (`ilspycmd` — o decompilado tem namespaces vazios, ver
  `reference_eft_decompile_incomplete`). Se **não houver** animação rápida, é só multiplicador de velocidade sobre
  o crawl lento (sem animação nova) ou precisa de mais?
- **Só velocidade** (multiplicador na velocidade de prone, como o item 005 faz para agachar/inclinar) **ou**
  também postura/câmera?
- **Custo de stamina:** integra com o controlador central (item 012)? Novo cenário de stamina (`ProneSprint`)?
- **Sync Fika:** peers veem o crawl acelerado? Se for movimento **nativo** do EFT, provavelmente sim sem pacote —
  confirmar (ver `reference_fika_peer_effects_client_side`).
- **Interações:** com as stances (o prone já força Stance 0), com mount, com ADS deitado, com o snap-on-fire.

## Escopo (a definir)

Provável ataque inicial: um multiplicador de velocidade de prone gateado por (forward + sprint) enquanto deitado,
espelhando o item 005 (agachar/inclinar) e drenando stamina pelo item 012. Só sobe para "animação nova" se a
investigação mostrar que o multiplicador sozinho fica ruim.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Criação do item (ideia bruta capturada; pendente investigação técnica e spec). |
