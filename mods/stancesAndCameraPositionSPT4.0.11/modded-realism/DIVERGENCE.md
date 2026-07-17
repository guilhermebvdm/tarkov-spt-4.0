# DIVERGENCE — modded-realism × modded (canônico)

> **Data:** 2026-07-17<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>

---

O fork `modded-realism/` é uma **pasta**, não uma branch git — sincronização com o canônico é manual e este
arquivo é o registro. Regra do experimento (item 016): o canônico `modded/` fica em regime de **só hotfix**;
todo hotfix aplicado lá entra na tabela abaixo e é replicado aqui **no fechamento da fase corrente**.

**Ponto de partida do fork:** cópia limpa do `modded/` @ v2.5.0 (commit `9da4dc8`, 2026-07-17), sem
`bin/ obj/ graphify-out/` e sem artefatos (`.zip`/`.dll`).

## Commits do canônico aplicados/pendentes no fork

| Commit (modded/) | O quê | Status no fork |
|---|---|---|
| — | (nenhum hotfix no canônico desde o fork) | — |

## Divergências próprias do fork (resumo por fase)

| Fase | O quê |
|---|---|
| F0 | Versão 3.0.0 + banner REALISM FORK no Awake + este arquivo |

## Como comparar

```bash
diff -r mods/stancesAndCameraPositionSPT4.0.11/modded/ mods/stancesAndCameraPositionSPT4.0.11/modded-realism/
```

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-17 | Guilherme | Criação junto com o fork (item 016, fase F0). |
