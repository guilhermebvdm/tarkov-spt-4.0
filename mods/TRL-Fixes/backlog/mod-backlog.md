# Backlog — TRL-Fixes

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões.
>
> Escopo deste mod: **correções de bugs de plataforma** (EFT vanilla, SPT, Fika) que afetam qualquer mod ou nenhum. Regra de admissão: se o bug existe com este mod como único mod instalado, é candidato daqui — bugs de gameplay de um mod específico ficam no backlog daquele mod.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | Fika PoolManager NullReferenceException | Recarga cartucho-a-cartucho de jogador remoto chamava `PoolManagerClass.CreateItem` de 4 parâmetros, que depende de referências de câmera nulas no cliente observador — NRE derrubava a conexão do jogador local. Redireciona para a sobrecarga de 2 parâmetros quando a origem é remota. | [001-fika-poolmanager-nre/](./001-fika-poolmanager-nre/) | 🟢 |
| 002 | Fika: hitbox perdida após revive | `ReviveInteractable.RemoveRagdoll` devolve a hierarquia inteira para a layer `Player` e nunca re-promove os `BodyPartCollider` para `HitCollider` — que é a layer que a máscara balística enxerga. O jogador revivido fica visível mas atravessável por bala/faca. As placas de armadura, desativadas pelo ragdoll, também não voltam. | [002-fika-revive-hitbox/](./002-fika-revive-hitbox/) | ⚪ |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
