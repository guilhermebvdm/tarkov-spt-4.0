# 019 — UI ao checar a câmara (mostrar bala + tipo, como o check do carregador)

> **Data:** 2026-07-19<br>
> **Status:** ⚪ Backlog (ideia bruta — não investigado)<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [010-manual-chambering](../010-manual-chambering/)<br>

---

## Ideia (do usuário)

Ao **checar a câmara** (chamber check), mostrar **se existe bala e qual é** (tipo de munição) — usando a **mesma
UI** que aparece ao **verificar o carregador** (magazine check). Hoje o check da câmara não dá esse feedback
textual/visual como o do carregador dá.

## ⚠️ Validação obrigatória antes de qualquer código: já existe algo pré-implementado?

**Primeiro passo do item** (gate): confirmar o que já existe, antes de assumir que precisa ser construído.

- O **EFT vanilla** já mostra alguma UI/feedback ao checar a câmara? (só animação + som, ou texto?) Investigar no
  Assembly real (`ilspycmd` — o decompilado tem namespaces vazios, ver `reference_eft_decompile_incomplete`).
- Existe **algum mod já instalado** no servidor que faça isso? (varrer `BepInEx/plugins` — ex.: mods de QoL de
  munição, `MunitionsExpert`, `MoreCheckmarks`, UIFixes). Se já cobre, o item **fecha sem código**.
- A **UI do check-magazine** (quantidade + tipo de munição) é **reutilizável/acessível** para o caso da câmara, ou
  é acoplada ao fluxo de carregador?

## Perguntas em aberto (fechar na spec, se sobreviver ao gate acima)

- Como ler o estado da câmara (munição no chamber slot)? Via `FirearmController` / `Weapon.Chambers[]`.
- **Interação com o item 010 (Manual Chambering):** quando a câmara está vazia (cenário criado pelo 010), a UI
  deve mostrar "vazia" corretamente — é justamente o caso em que o feedback é mais útil.
- **Sync Fika:** é UI **local** (só quem checa vê) → provavelmente sem pacote de rede (ver
  `reference_fika_peer_effects_client_side`).
- Gatilho: engancha no evento nativo de check-chamber, ou é preciso interceptar o input?

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Criação do item (ideia bruta). Gate inicial: validar pré-implementação (vanilla + mods instalados) antes de codar. |
