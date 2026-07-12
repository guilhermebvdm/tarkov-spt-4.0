# Changelog

Changelog do fork (Tarkov Red Line). O histórico do mod original vai até a v1.1.4 e está em
[CHANGELOG_SIMPLIFIED.md](./CHANGELOG_SIMPLIFIED.md).

Versões mais recentes primeiro.

---

## v2.0.0 (2026-07-11)

> ### ⚠️ Leia antes de atualizar: suas configurações serão perdidas
>
> Esta versão renomeia as seções e as chaves do menu F12. O BepInEx casa cada opção salva pelo par
> (seção, chave) **literal**, então as opções antigas não são reconhecidas e **todas as configurações
> voltam ao padrão**. Não há migração automática do `.cfg`.
>
> **O que fazer:** nada, se você nunca mexeu no F12 — os valores padrão reproduzem exatamente o
> comportamento testado. Se você tinha uma calibração própria, anote-a antes de atualizar (ou guarde
> uma cópia do `BepInEx/config/com.shwng.fpscamerastances.cfg`) e refaça no F12 depois. Vale a pena
> reconfigurar do zero: **8 opções tinham os eixos Roll e Yaw trocados no rótulo**, ou seja, quem
> calibrou pelo nome estava mexendo no eixo errado.

### Novidades

- **Bloqueio do apoio de arma nas posturas** — apoiar a arma em superfícies (mount) agora só é
  possível na Stance 0 (vanilla), com a mira em ADS ou deitado. Nas Stances 1/2/3 o apoio é
  recusado, em vez de deixar a arma numa pose inconsistente. O bipé **não** é afetado.

### Correções

- **Sync das posturas no Fika: o braço agora acompanha a arma.** Para os outros jogadores, a postura
  era aplicada tarde demais no pipeline de animação (depois do IK), então só a arma se movia e o
  braço ficava parado. O offset passou a ser aplicado na janela pré-IK, e braço e arma se movem
  juntos.
  *Em partidas coop, recomenda-se que todos atualizem: o pacote de rede não mudou (jogadores em
  versões diferentes continuam se conectando normalmente), mas quem estiver na versão antiga vai
  continuar vendo os companheiros com a arma solta do braço.*
- **Eixos Roll e Yaw destrocados** em 8 opções de rotação (posturas e ADS): o rótulo dizia um eixo e
  o código aplicava o outro.
- **Rótulos legados das Stances 2 e 3** corrigidos no ciclo de posturas — os nomes ainda refletiam a
  ordem antiga, anterior à troca entre "Low Ready" e "Custom".

### Menu F12 reorganizado

- **23 propriedades mortas removidas** — não faziam nada: apareciam no menu, mas nenhuma delas era
  lida pelo código. O menu foi de 143 para **120 opções**, e de 23 para **21 seções**.
- **Seções renomeadas** para nomes descritivos em inglês (sem os prefixos numéricos antigos).
- **Todas as descrições agora são bilíngues** — inglês na primeira linha, português abaixo.

### Interno

- Os patches do Manual Chambering ganharam proteção contra exceções (uma falha ali não derruba mais o
  resto do mod).
- Logs de diagnóstico temporários removidos.
- Versão do assembly (`.csproj`) passa a acompanhar a versão do plugin — antes a DLL era compilada
  como `1.0.0.0` independentemente da versão anunciada no BepInEx.

---

## v1.3.1 e anteriores

Versões de desenvolvimento do fork, não distribuídas com changelog próprio. Acumulam, sobre o mod
original: stamina e velocidade por postura, ciclo linear de posturas e teclas dedicadas, snap para a
Stance 0 ao atirar, velocidade de agachar/inclinar, inércia e velocidade máxima, troca automática de
postura ao recarregar/checar arma, animação orgânica de transição (Wiggle), Manual Chambering, apoio
passivo de arma sobre o mount nativo, controlador central de stamina de braço e o sync visual das
posturas no Fika.
