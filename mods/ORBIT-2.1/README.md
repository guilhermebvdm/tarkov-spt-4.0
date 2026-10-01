# ORBIT-2.1

**Versão base:** 2.1.0 · **Licença:** MIT
**Upstream:** https://github.com/Chazut/ORBIT @ `8fd7e661073aa8b46ec8dc00a2b1d62ea7875ea7` (branch `2.1`)
**Forge:** https://sp-mod.com/mod/2706/orbit
**Alvo deste fork:** SPT 4.0.13 / EFT 0.16.9.40087 (o upstream 2.1 é para SPT 4.1)

---

## O que é

ORBIT (Objective-driven Raid Bot Intelligence Tactics) dá objetivos reais aos esquadrões de bots: ir a um ponto de quest, limpar uma área de loot, caçar em pontos de PvP, saquear como jogador, trocar de equipamento e extrair. A versão 2.x acrescenta o **Ghost Mode** (bots distantes dormem para liberar CPU enquanto o mod segue simulando o que eles fazem), um **painel web** no servidor (`/orbit`), o **editor de zonas** e **presets**.

Esta pasta é o **port de versão para baixo**: o upstream 2.1 só existe para SPT 4.1, e aqui ele foi adaptado para o SPT 4.0.13. São três partes:

| Parte | Projeto | Saída | Onde instala |
|---|---|---|---|
| Plugin de cliente | `modded/Orbit/` | `ORBIT.dll` 2.1.0 | `BepInEx/plugins/ORBIT-2.1/` |
| Addon Fika (áudio de combate fantasma e estado de portas) | `modded/Orbit.Fika/` | `Orbit.Fika.dll` 1.1.0 | `BepInEx/plugins/ORBIT-2.1/` |
| Mod de servidor (painel, config, zonas, presets) | `modded/Orbit.Server/` | `Orbit.Server.dll` 2.1.0 | `SPT/user/mods/ORBIT-2.1/` |

Dependências do cliente: BigBrain, Waypoints e SAIN. O ORBIT 1.2.1 ([mods/ORBIT/](../ORBIT/)) tem o mesmo identificador de plugin: só um dos dois pode ficar instalado.

**Estado em 2026-09-30:** compila e passa as verificações fora do jogo; **ainda não foi validado em jogo** e não está instalado. Ver o [as-built](backlog/001-downgrade-spt41-spt40/001-downgrade-spt41-spt40-05-asbuild.md).

## Como o port foi feito

O fonte continua usando os nomes do SPT 4.1. A diferença para o 4.0 fica concentrada em quatro arquivos de compatibilidade, mais edições pontuais:

| Arquivo | O que resolve |
|---|---|
| [modded/Orbit/Compat/Spt40TypeAliases.cs](modded/Orbit/Compat/Spt40TypeAliases.cs) | Nome de tipo 4.1 → tipo 4.0 (`Ammo` → `AmmoItemClass`, `ExfiltrationLayer` → `GClass75`) |
| [modded/Orbit/Compat/Spt40TypeNames.cs](modded/Orbit/Compat/Spt40TypeNames.cs) | Nome de classe comparado como texto em tempo de execução |
| [modded/Orbit/Compat/Spt40Members.cs](modded/Orbit/Compat/Spt40Members.cs) | Campos e métodos do jogo buscados por texto (reflexão, Harmony) |
| [modded/Orbit.Server/Compat/Spt40ServerCompat.cs](modded/Orbit.Server/Compat/Spt40ServerCompat.cs) | API do servidor 4.0 e MudBlazor 8.13 |

De-para completo, evidências e riscos: [spec técnica](backlog/001-downgrade-spt41-spt40/001-downgrade-spt41-spt40-02-spec-tech.md). Procedimento reutilizável para outros mods: [docs/technical/spt41-to-spt4-mod-downgrade.md](../../docs/technical/spt41-to-spt4-mod-downgrade.md).

## Estrutura desta pasta

| Pasta | Conteúdo |
|---|---|
| `original/` | Clone do repositório oficial, sem `.git`. **Não modificar.** Referência intocada usada para diff e atualizações. |
| `modded/` | Cópia de trabalho. Modificações vão aqui. |
| `backlog/` | Itens de backlog com spec, reviews e as-built. 001 = o port; 002 = pacotes Fika do addon. |
| `builds/` | Builds geradas (não versionadas). |
| `scripts/` | Verificação fora do jogo e instalação (abaixo). |
| `memory/` | Memória de sessões deste mod. |
| `mod.json` | Metadados machine-readable (alimenta o inventário de mods). |
| `PROPRIEDADES.md` | Opções do F12. |

## Build

```bash
/compile-mod ORBIT-2.1 --no-install     # compila os três projetos em builds/, sem tocar no SPT
```

Os projetos resolvem as DLLs do jogo e do servidor a partir de `SPT_PATH` ou do `.spt-path` da raiz do repositório ([modded/Directory.Build.props](modded/Directory.Build.props)). Sem `--no-install`, o `/compile-mod` copia as DLLs para o SPT, mas **não** remove o ORBIT 1.2.1; para instalar use o script abaixo.

## Verificação fora do jogo

| Comando | O que confere |
|---|---|
| `bash mods/ORBIT-2.1/scripts/verify-port.sh` | Cada patch Harmony resolve o alvo no jogo instalado, os parâmetros ligam, os transpilers rodam sobre o IL real; as ligações com SAIN, MoreBotsAPI e UNTAR resolvem; os nomes de tipo comparados como texto estão cobertos; nenhuma linha monta caminho do SPT 4.1 |
| `powershell -File mods\ORBIT-2.1\scripts\server-smoke-test.ps1` | Uma cópia descartável do servidor 4.0 carrega o mod, as 13 páginas e as 3 rotas respondem |

Nenhum dos dois escreve na instalação do SPT. Nenhum dos dois substitui a validação em jogo.

## Instalar e desfazer

```bash
bash mods/ORBIT-2.1/scripts/install-to-spt.sh              # jogo e servidor fechados
bash mods/ORBIT-2.1/scripts/install-to-spt.sh --rollback   # volta ao ORBIT 1.2.1
```

O script move a pasta do ORBIT 1.2.1 para `BepInEx/plugins-disabled/` (não apaga) e copia as três DLLs. Em coop, host, headless e todos os clientes precisam das mesmas `ORBIT.dll` e `Orbit.Fika.dll`.

## Workflow de desenvolvimento

Ciclo completo de backlog/specs/reviews/código/memória/grafos: ver [WORKFLOW.md](../../WORKFLOW.md).

## Mapa de código

Grafo do código deste mod (graphify): [`GRAPH_REPORT.md`](../../references/graphs/mods/ORBIT-2.1/GRAPH_REPORT.md). Regenerar após mudanças: `/update-mod-graph ORBIT-2.1` (ou `bash scripts/update-graphs.sh ORBIT-2.1`).

## Comparar modificações com o original

```bash
diff -r -x obj -x bin -x References -x graphify-out mods/ORBIT-2.1/original/ mods/ORBIT-2.1/modded/
```

## Atualizar do upstream

Para portar uma versão nova do ORBIT 2.x: substituir `original/` pelo clone novo, reaplicar em `modded/` o diff atual contra o `original/` antigo, compilar, e rodar `scripts/verify-port.sh`. Nomes novos que o upstream passar a usar aparecem como erro de compilação (tipos e membros), como `FAIL` no `verify-port.sh` (patches e reflexão) ou como `MISSING` (nomes de tipo em texto).

---

_Adicionado em 2026-10-01T01:12:35Z_
