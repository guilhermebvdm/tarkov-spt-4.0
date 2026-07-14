# Config de distribuição — `com.shwng.fpscamerastances.cfg`

Este é o `.cfg` **calibrado do servidor Tarkov Red Line**, que acompanha a DLL do mod. Ele existe aqui para
rastreabilidade: qual configuração corresponde a qual versão do mod.

> **Versão do mod:** v2.5.0 · **Gerado em:** 2026-07-14 (pelo próprio jogo, não à mão)
> **Conteúdo:** 19 seções · 111 opções · sem chaves órfãs

## Por que ele precisa ser distribuído junto com a DLL

A partir da **v2.0.0** as chaves do F12 foram **renomeadas** (seções para inglês, sufixos traduzidos, eixos
Yaw/Roll corrigidos). O BepInEx casa cada opção salva pelo par `(seção, chave)` **literal** — então uma DLL nova
com um `.cfg` antigo **não reconhece nada** e recria tudo com os valores padrão: o jogador perde a calibração e
as posturas ficam diferentes das do servidor.

**Os dois têm que subir juntos.** DLL nova + config velha (ou o contrário) = calibração perdida para todo mundo.

## Como distribuir

O launcher tem duas pastas com semânticas diferentes:

| Pasta | Comportamento | Serve aqui? |
|---|---|---|
| `config-server` | **Espelho** — sobrescreve o arquivo do jogador | ✅ **é esta** |
| `config` | *Seed-if-missing* — só copia se o jogador não tiver o arquivo | ❌ não adianta: quem já tem o `.cfg` antigo não receberia nada e cairia nos defaults |

## Como este arquivo foi gerado (não editar à mão)

1. Instalar a DLL da versão alvo e **abrir o jogo uma vez** — o BepInEx binda todas as opções e escreve o arquivo.
2. Remover o entulho: seções e chaves que o BepInEx **preserva** de versões antigas mesmo sem existirem mais no
   mod. A marca de uma opção real é o bloco de comentário `# Setting type:` — órfãs não têm.
3. Conferir: nº de `# Setting type:` deve bater com o nº de linhas `chave = valor` (aqui: **114 = 114**).

⚠️ **Nunca inventar uma chave à mão.** Um `=` dentro do nome de uma chave é proibido pelo BepInEx e **aborta a
inicialização do mod** — foi o que derrubou a v2.2.0 (ver `CHANGELOG.md`).
