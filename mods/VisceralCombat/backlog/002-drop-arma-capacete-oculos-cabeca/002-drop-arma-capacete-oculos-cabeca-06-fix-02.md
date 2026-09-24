# 002 — Fix 02 · Fone (Earpiece) e Máscara (FaceCover) Adicionados ao Drop de Equipamento de Cabeça

**Mod:** VisceralCombat
**Item raiz:** [002-drop-arma-capacete-oculos-cabeca-01-spec.md](002-drop-arma-capacete-oculos-cabeca-01-spec.md)
**Asbuild:** [002-drop-arma-capacete-oculos-cabeca-05-asbuild.md](002-drop-arma-capacete-oculos-cabeca-05-asbuild.md)
**Criado:** 2026-09-20
**Disparado por:** Pedido direto do usuário, logo após validar o `06-fix-01.md` (mecanismo seguro de drop post-mortem).

## Contexto

A feature de drop de equipamento de cabeça (item 002) cobria só `EquipmentSlot.Headwear` (capacete) e `EquipmentSlot.Eyewear` (óculos). O usuário pediu pra estender a mesma lógica pra `EquipmentSlot.FaceCover` (máscara/balaclava) e `EquipmentSlot.Earpiece` (fone/headset) — fisicamente os 4 itens ficam presos na cabeça, então desmembrar/estourar a cabeça deveria derrubar os 4, não só 2.

## Causa raiz

N/A — não é correção de bug, é extensão de escopo de uma feature existente.

## Mudanças aplicadas

Reutiliza 100% o mecanismo já existente e validado (`06-fix-01.md`) — nenhum código de rede/posicionamento novo, só os 2 slots adicionais nos 2 pontos que já derrubavam capacete/óculos:

| Arquivo | Mudança |
|---|---|
| `modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs` | `ResolveAndDropHeadEquipment`: lê `EquipmentSlot.FaceCover`/`EquipmentSlot.Earpiece` além de `Headwear`/`Eyewear`; joga os 4 via `controller.ThrowItem(...)` (mesmo caminho já usado, sem mudança de mecanismo). |
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs` | `DropCorpseHeadEquipment`: mesma extensão, usando `ThrowFromCorpsePosition(...)` (o helper do fix-01) pros 4 itens. |
| `modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | Tooltip de `DropHeadEquipmentOnDismemberment` atualizado pra mencionar os 4 itens. Nome da chave (`"Drop Headwear/Eyewear On Head Dismemberment"`) **mantido de propósito** — trocar quebraria o valor já salvo pelos usuários (`Config.Bind` casa por `(section, key)` literal, ver `repo-workflow-best-practices` §7). Versão `3.10.2` → `3.11.0`. |
| `modded/VisceralCombat/VisceralCombat.csproj` | `<Version>` `3.10.2` → `3.11.0`. |
| `mods/VisceralCombat/PROPRIEDADES.md` | Tooltip de "Drop Headwear/Eyewear On Head Dismemberment" e nota de sincronização da seção "General" atualizados pra mencionar os 4 itens. |

**Decisão de escopo (confirmada com o usuário via pergunta direta):** fone e máscara reaproveitam o MESMO toggle já existente (`DropHeadEquipmentOnDismemberment`), não um `ConfigEntry` novo — conceitualmente é a mesma coisa (equipamento preso na cabeça caindo junto).

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `compile-mod.sh` sem erros (0 erros, mesmos warnings pré-existentes — build 3.11.0)
- [x] **In-raid:** fone e máscara caem corretamente junto com capacete/óculos — confirmado pelo usuário em 2026-09-20 ("Fone/máscara confirmados")
- [x] **Fika/multiplayer:** herda o comportamento já validado em `06-fix-01.md` (mesmo mecanismo de rede) — sem relato de duplicata
- [x] **raid1 → exit → raid2:** N/A — nenhum estado novo introduzido
- [x] **alt-F4 / morte / MIA:** N/A
- [x] Memória do mod atualizada (`/update-memory`) com a lição do fix

## Histórico

| Data | Evento |
|---|---|
| 2026-09-20 | Fix criado e aplicado — extensão de escopo a pedido do usuário, reaproveitando 100% o mecanismo de `06-fix-01.md`. Build 3.11.0 compilada e instalada em `E:\Tarkov Red Line` e `E:\Tarkov Red Line - SERVER TEST`. |
| 2026-09-20 | **Validado em raid pelo usuário** ("Fone/máscara confirmados"). Fix fechado. |
