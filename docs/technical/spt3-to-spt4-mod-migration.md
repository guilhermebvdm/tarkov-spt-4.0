---
title: Atualizar um Mod do SPT 3.x para o SPT 4.0
date: 2026-07-26
status: 🟢 Vivo
authors: Guilherme + agente
---

# Como Atualizar um Mod do SPT 3.x para o SPT 4.0

A transição 3.x → 4.0 foi a maior mudança arquitetural da história do projeto: o que era "SPT-AKI" virou "SPT", e o servidor foi **reescrito do zero**. Este guia cobre o que muda e como fazer o port **dentro do fluxo deste repositório**.

> **Pré-requisito:** [spt4-mod-creation.md](spt4-mod-creation.md) descreve o setup e o build do repo. Este doc assume aquilo e foca no que é específico de migração.

---

## 1. A mudança de fundo: o fim do Node.js

Até a 3.x o servidor rodava em **Node.js** e todo server mod era **TypeScript**. No 4.0 o servidor é **C# / .NET 9**.

Consequências práticas:

- **Server mods 3.x estão mortos.** Não existe caminho de compatibilidade — mudar a versão no `package.json` não faz nada. Todo o TypeScript precisa ser reescrito em C#.
- **Client mods sobrevivem em linguagem, não em código.** Continuam C#/BepInEx, mas a base mudou o suficiente para exigir reconferência de cada ponto de patch.
- **Uma linguagem só.** Antes: TypeScript no servidor + C# no cliente. Agora: C# nos dois.

> A wiki oficial é direta: **nenhum mod 3.11 é compatível com 4.0**. Perfis sem mod migram; mods, não. Ver [wiki/spt/FAQs_40.md](../../wiki/spt/FAQs_40.md) e [wiki/spt/Updating_SPT.md](../../wiki/spt/Updating_SPT.md) (major/minor quebram todos os mods; só patch preserva compat).

---

## 2. Server mod: TypeScript → C#

### O que muda no modelo mental

| SPT 3.x (TypeScript) | SPT 4.0 (C#) |
|---|---|
| Injeção via TSyringe (`container.resolve("DatabaseServer")`) | DI nativa do .NET, serviços tipados |
| `postDBLoad(container)` | interfaces de ciclo de vida do servidor C# |
| Objetos/arrays JS, acesso solto ao JSON | classes fortemente tipadas geradas pelo time do SPT |
| Métodos de array (`map`/`filter`/`reduce`) | LINQ |
| `package.json` com `"version"` | `AbstractModMetadata` + `SemanticVersioning` |
| Porta secundária para rota própria | `[ApiController]` hospedado na porta raiz |

### Roteiro

1. **Criar o projeto** a partir do template oficial de server mod C# ([dev.sp-tarkov.com](https://dev.sp-tarkov.com/)), em `mods/<mod>/modded/Server/`.
2. **Mapear a API antiga para a nova.** A fonte de verdade é [`references/spt-source/`](../../references/spt-source/) — código-fonte vendorizado do servidor, read-only. Procure ali o serviço/helper equivalente **antes** de reimplementar: `ItemHelper`, `PresetHelper`, `InventoryHelper`, `ICloner` e companhia já existem.
3. **Reescrever a lógica.** LINQ no lugar dos métodos de array; tipos concretos no lugar de acesso solto ao JSON.
4. **Preencher a metadata.** Sem `AbstractModMetadata` corretamente preenchido (`Version`, `SptVersion`, contribuidores), o servidor recusa o mod.
5. **Validar in-game.** Compilar não é entregar ([AP-06](spt-antipatterns.md)).

> 📦 Se o mod toca **itens, inventário, equipamento, contêineres, presets, munição ou hideout**, a doc canônica é [spt4-items-inventory-hideout.md](spt4-items-inventory-hideout.md) — estrutura `_id`/`_tpl`/`parentId`/`slotId`, `location {x,y,r}`, re-id ao clonar árvores. Não montar árvore de item de cabeça.

> ⚠️ **Padrão que não migra:** mod 3.x que editava arquivos de `SPT_Data/database/` em disco. No 4.0 (como já no 3.x, aliás) o caminho correto é patch **em memória** no `postDBLoad`. Ver `AGENTS.md`.

---

## 3. Client mod: mesma linguagem, alvos diferentes

A linguagem é a mesma, mas o `Assembly-CSharp.dll` do EFT mudou — e a BSG renomeia classes ofuscadas (`GClass1234`, `Class567`) a cada patch. **Todo `[HarmonyPatch]` que apontava para um tipo ofuscado precisa ser reconferido.**

### Roteiro

1. **Limpar as referências antigas.** Remova as DLLs da 3.x do `.csproj`. **Não** copie as novas à mão: `/compile-mod` popula `modded/References/` a partir do path em `.spt-path`. Um gate de pre-commit (`check-csproj-references.sh`) avisa se sobrou `<HintPath>` absoluto.

2. **Reconferir cada ponto de patch — no repo, não no dnSpy.** O reflexo antigo era abrir o `Assembly-CSharp.dll` numa ferramenta externa. Neste repo o assembly já está descompilado (8.683 tipos), indexado e grafado. Ordem:

   | Situação | Ferramenta |
   |---|---|
   | Sei o nome do tipo | grafo (`graphify-eft`, skill `graph-code-navigation`) → depois abrir o `.cs` para provar a assinatura |
   | Sei só o conceito | alias 4.1 no topo de cada `.cs` e no `types-index.json`; ou [`consolidated-mappings.txt`](../files-from-4.1/consolidated-mappings.txt) |
   | Preciso saber se existe | **`references/eft-decompiled/types-index.json`** — nunca um `grep` vazio (o dump é gitignored; ausência em disco ≠ ausência no assembly) |

   `ilspycmd -t <FQN>` continua legítimo em três casos: tipo `// DECOMPILE-ERROR`, tipo fora do índice, ou dump não gerado nesta máquina.

3. **Não pinar nome ofuscado.** Resolva o alvo por **assinatura/predicado estável** (tipo de retorno + parâmetros + fragmento de nome), nunca por `GClassNNNN` literal — a numeração não é estável nem dentro da mesma major version. Ver [AP-03](spt-antipatterns.md) e o padrão `ResolveBackingFieldByCandidates` já usado no mod de stances.

4. **Auditar overrides antes de patchear virtual.** Harmony intercepta o IL do método patcheado; se o override não chama `base.X()`, o patch na base virtual **nunca dispara** naquele caminho. Caso real neste repo: dos 14 overrides de `SetTriggerPressed`, só 1 chamava a base — o patch parecia funcionar e não funcionava. Ver [AP-03](spt-antipatterns.md).

5. **Revisitar as premissas de multiplayer.** Um mod 3.x provavelmente assumia jogador único. Aqui o cenário default é **Fika coop** — todo patch que reage a ação de player precisa distinguir o jogador local de bots e de outros peers ([AP-02](spt-antipatterns.md)). Se o mod sincroniza estado próprio pela rede, [fika-packet-desync-prevention-plan.md](fika-packet-desync-prevention-plan.md) é leitura obrigatória.

6. **Recompilar e validar.** `/compile-mod <mod>` (com bump de versão obrigatório) e então raid real.

---

## 4. O que o 4.1 muda nisso

A próxima versão desofusca os identificadores — `GClass774` vira `Stamina`, `GInterface34` vira `ISourceGroup`. As tabelas de mapeamento já estão no repo ([docs/files-from-4.1/](../files-from-4.1/)) e são úteis **hoje** para entender o que um tipo ofuscado significa.

Duas ressalvas, detalhadas em [spt4-vs-spt41-gclass-deobfuscation.md](spt4-vs-spt41-gclass-deobfuscation.md):

- O nome à direita é **rótulo de fonte comunitária** — aponta o conceito, não prova assinatura. Não pinar.
- A cobertura não é total: `GClass898` e `GClass3008`, usados no próprio repo, não têm entrada. **Sem entrada ≠ não existe.**

A desofuscação **não elimina** o AP-03: assinatura de método pode mudar mesmo com nome estável. Mantenha os fallbacks defensivos.

---

## 5. Onde buscar ajuda

**No repo:** [.agents/resources.md](../../.agents/resources.md) mapeia cada tipo de dúvida à fonte certa; [WORKFLOW.md](../../WORKFLOW.md) descreve o ciclo; [spt-antipatterns.md](spt-antipatterns.md) lista os erros já cometidos aqui.

**Fora:** changelog oficial do SPT 4.0 (seção "Developer Notes" lista classes renomeadas) · [deepwiki.com/sp-tarkov/server-csharp](https://deepwiki.com/sp-tarkov/server-csharp/1-overview) para arquitetura do servidor · SPT Discord `#mods-development`.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-06 | Guilherme | chore(launcher): remove empty placeholder diff.txt |
| 2026-07-26 | Guilherme + agente | Reescrito e promovido a 🟢 Vivo. Substituída a instrução de abrir o `Assembly-CSharp.dll` no dnSpy pela ordem do harness (grafo → `.cs` → `types-index.json`; `ilspycmd` só nos 3 casos legítimos — AP-09) e a troca manual de referências por `/compile-mod` + `.spt-path`. Adicionados: tabela de mapeamento mental 3.x→4.0, `references/spt-source/` como fonte da API nova, auditoria de overrides (AP-03), premissas de Fika coop (AP-02), seção sobre o 4.1 e validação in-game como critério de entrega. |
| 2026-07-26 | Guilherme | docs(technical): arquiva legado TS 3.x e redige credenciais Supabase expostas |
