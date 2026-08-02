# Renomeação para `TRL-StancesAndMobility`

> **Data:** 2026-08-02<br>
> **Status:** 🔵 Em andamento — identidade do plugin trocada; estrutura pendente<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [PERMISSION.md](./PERMISSION.md) · [`trl-mod-publishing`](../../../.claude/skills/trl-mod-publishing/SKILL.md)<br>

---

## Feito na v2.16.0

| Onde | Antes | Agora |
|---|---|---|
| GUID do plugin | `com.shwng.fpscamerastances` | `com.trl.stancesandmobility` |
| Nome do plugin (o que o F12 mostra) | `shwngFpsCameraStances4` | `TRL-StancesAndMobility` |
| Nome do arquivo compilado | `shwngFpsCameraStances4.dll` | `TRL-StancesAndMobility.dll` |
| Arquivo de configuração | `com.shwng.fpscamerastances.cfg` | `com.trl.stancesandmobility.cfg` |
| Prefixo das mensagens de log | `[CameraRotationMod]` | `[TRL-StancesAndMobility]` |
| Linha de boot | `Plugin shwngFpsCameraStances4 is loaded!` | `Plugin TRL-StancesAndMobility is loaded!` |

Build limpa (0 erros, 0 avisos) com o nome novo.

## ⚠️ O que isso quebra para quem atualiza

**Todos os ajustes do F12 voltam ao padrão.** O BepInEx guarda a configuração num arquivo derivado do GUID;
com GUID novo, ele cria um arquivo em branco e o antigo fica órfão. Não há migração automática — é o mesmo
efeito da v2.0.0, e desta vez é deliberado.

**A pasta de instalação muda.** Hoje o mod vive em `BepInEx/plugins/RealisticMobility/` com uma DLL de nome
antigo. Depois desta versão, a pasta correta é `BepInEx/plugins/TRL-StancesAndMobility/`, com os seis assets
(3 `.ogg` + 3 `.png`) ao lado da DLL. **Quem só copiar a DLL nova sem remover a antiga terá as duas
carregadas** — dois plugins, dois conjuntos de patches, comportamento imprevisível.

### Plano de migração (obrigatório antes de distribuir)

1. Distribuir o `.cfg` novo **já preenchido** com os valores calibrados, pelo canal `config-server` do
   launcher (que sobrescreve). O canal `config` não serve: ele só cria quando o arquivo falta.
2. Garantir que a instalação **remova a pasta antiga** (`RealisticMobility/`) em vez de só adicionar a nova.
3. Anunciar a quebra no changelog e na página do mod, não deixar o jogador descobrir sozinho.

## Pendente — as duas mudanças estruturais

Ficaram de fora por serem invasivas e merecerem verificação própria:

| # | O quê | Por que não foi feito junto |
|---|---|---|
| 1 | **Namespace C#** `CameraRotationMod` → `TarkovRedLine.StancesAndMobility` | Toca **39 arquivos**. É busca e substituição mecânica, mas com 39 arquivos alterados de uma vez, qualquer erro se esconde no volume do diff |
| 2 | **Pasta do mod no repositório** `mods/stancesAndCameraPositionSPT4.0.11/` → `mods/TRL-StancesAndMobility/` | Quebra caminhos em memória, backlog, grafos de código e nas ferramentas do harness — e há sessão paralela com arquivos abertos nesta pasta. Precisa de checkout tranquilo e de uma varredura de referências |

Nenhuma das duas é visível para o jogador: são organização interna. A identidade que aparece no jogo, no F12
e no arquivo de configuração **já está trocada**.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-08-02 | Guilherme | Criação — identidade do plugin renomeada na v2.16.0; namespace e pasta do repo registrados como pendentes |
