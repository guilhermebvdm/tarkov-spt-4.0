# `docs/technical/` — documentação técnica canônica

Camada do harness lida **durante o ciclo de desenvolvimento**, não só quando alguém lembra. Cada doc tem um **gatilho**: uma condição da tarefa que torna a leitura obrigatória.

**Como usar:** ao iniciar spec técnica, review ou código, olhe a coluna *Gatilho* e leia **só os docs que a sua tarefa dispara**. Não é para ler a pasta inteira.

## Roteamento

| Doc | Gatilho — o item toca… | Fases | Enforcement |
|---|---|---|---|
| [`spt-antipatterns.md`](spt-antipatterns.md) | **sempre** | 02·03·04·05 | §9 da spec técnica (checks `AP-NN`) |
| [`fika-packet-desync-prevention-plan.md`](fika-packet-desync-prevention-plan.md) | mod declara `INetSerializable` / envia pela rede FIKA | 02·03·04·05 | §9 check 11 + checklist 11 da skill `spt-mod-best-practices` |
| [`spt4-items-inventory-hideout.md`](spt4-items-inventory-hideout.md) | itens, inventário, contêiner, grade, preset, munição, hideout | 02·03·05 | skills `spt-mod-best-practices` e `csharp-mod-best-practices` |
| [`spt4-vs-spt41-gclass-deobfuscation.md`](spt4-vs-spt41-gclass-deobfuscation.md) | spec ou código cita `GClassNNNN` / `GStructNNNN` / `GInterfaceNNNN` | 02·03·04 | regra de conceito do `/code-review` |
| [`spt4-mod-creation.md`](spt4-mod-creation.md) | mod novo, ou primeira feature server-side de um mod | 02 | — |
| [`spt3-to-spt4-mod-migration.md`](spt3-to-spt4-mod-migration.md) | port de mod 3.x | 02 | — |
| [`spt4-csharp-build.md`](spt4-csharp-build.md) | launcher ou `TarkovRedLine.Server` | build (fora do ciclo de artefatos) | — |

> **Legenda da coluna Fases** — os números são os do artefato no ciclo: `01` spec funcional · `02` spec técnica · `03` review da spec técnica · `04` code review · `05` as-build. Ver [WORKFLOW.md](../../WORKFLOW.md).

## Fronteiras

- **Escopo.** `technical/` é a **única** camada canônica de `docs/`. As outras pastas — `ideas/`, `migration/`, `discord-mods-topics/`, `files-from-4.1/` — não participam do roteamento. (`files-from-4.1/` é dado bruto, consultado por `grep` a partir das regras em [.agents/resources.md](../../.agents/resources.md).)
- **Manutenção.** Doc novo nesta pasta exige uma linha nesta tabela — sem ela, o roteamento não o alcança e ele nasce órfão.
- **Renomear ou remover** um doc daqui quebra referências de fora (commands, skills, memórias, artefatos de backlog). Procedimento em [.agents/conventions.md](../../.agents/conventions.md) § "Renomear ou remover um doc de `docs/technical/`".
- **`.archived/`** guarda docs de arquitetura descontinuada. Não é fonte — está fora do roteamento.

## Relação com as outras camadas

| Camada | Papel | Onde |
|---|---|---|
| **Docs técnicos** (esta pasta) | o **porquê** e a evidência: casos reais, mecanismos, histórico | `docs/technical/` |
| **Skills** | o **o quê** prescritivo e curto, carregado pelos commands | `.claude/skills/` |
| **Hierarquia de evidência** | **onde** provar uma afirmação (Assembly 🥇 → web 🪛) | [.agents/resources.md](../../.agents/resources.md) |
| **Memória** | o que **esta linha de trabalho** já aprendeu, por mod | `mods/<mod>/memory/sessions.md` |
| **Grafos** | **navegação** — achar callers, overrides, cadeias | [references/graphs/](../../references/graphs/) |

Skills são curtas de propósito e apontam para cá quando o caso precisa de evidência ou histórico. Um doc desta pasta nunca substitui a leitura do `arquivo.cs:linha` — ele diz onde olhar e o que já deu errado.
