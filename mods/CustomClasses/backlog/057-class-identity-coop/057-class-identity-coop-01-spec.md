# 057 — Identidade de classe per-player em coop (Fika)

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-07-03

## Visão geral

Hoje toda a identidade visual de classe (ícone, cor, popover de perks/drawbacks) aparece **apenas para o player
local** — os demais membros do grupo coop aparecem como nomes puros. Este item faz a **tela de carregamento da raid
(FIKA)** mostrar a identidade da classe de **cada** player: hover na linha de qualquer membro abre o popover do 055
com os perks/drawbacks **daquela** classe. **Escopo (decisão do usuário 2026-07-03): só a tela de loading do FIKA** —
demais superfícies coop (painel de ready do lobby, nametag in-raid, chat) ficam para item futuro.

## Comportamento atual

- O cliente conhece **só a própria classe**: a rota `/customclasses/skill-multipliers` responde apenas com a
  identidade do perfil da sessão (nome de exibição en/pt, ícone, cor, nickname) e o cache do cliente
  (`SkillMultipliers`) guarda esse único registro.
- O popover do loading (055, `ClassDetailLoadingPatch`) só é anexado à linha cujo nickname coincide com o nickname
  local — as linhas dos outros players não reagem ao hover.
- O cache de ícones do cliente (`ClassIconCache`) carrega apenas o ícone da própria classe.
- O servidor conhece a classe de **todos** os perfis (registry de classes completo, com nome en+pt, cor e ícone de
  cada uma), mas não expõe esse catálogo aos clientes.
- **Hipótese do recon do backlog (a confirmar na spec técnica):** a informação que permite descobrir a classe de um
  player remoto já existiria sincronizada pelo coop — a "edição" do perfil, onde o mod grava o nome de exibição da
  classe na criação do personagem, **no idioma do jogo de quem criou** (en OU pt). Caminho alternativo caso não se
  confirme: o servidor (que conhece todos os perfis) expõe o mapeamento player→classe diretamente.

## Comportamento desejado

- O cliente busca **uma vez** no servidor o **catálogo completo de classes** (por classe: nome de exibição en E pt,
  cor, arquivo de ícone) e o mantém em cache.
- Na tela de carregamento da raid (FIKA), **cada linha de player** cuja classe for resolvida ganha:
  - **Identidade visível sem hover:** indicação visual da classe na própria linha (cor da classe no nome e/ou
    brasão junto ao nome — mesmo vocabulário visual da identidade local existente).
  - **Popover no hover:** o mesmo popover do 055 (hover-only + zoom) com header, brasão, cor e cards de
    perks/drawbacks **da classe daquele player**.
- A resolução da classe de um player remoto deve funcionar **independente do idioma do jogo de quem criou o
  perfil** (perfis gravam o nome de exibição no idioma do criador — en OU pt). O mecanismo concreto (informação
  sincronizada pelo coop vs. mapeamento player→classe exposto pelo servidor) é decisão da spec técnica.
- O player local continua funcionando exatamente como hoje (mesmo caminho ou equivalente — sem regressão do 055).
- Player com classe vanilla/desconhecida: linha sem identidade e sem popover, como hoje — sem erro e sem log de spam.

## Critérios de aceite

- [ ] Em coop com 2+ players de **classes diferentes** do mod, o hover na linha de cada player na tela de
      carregamento abre o popover com a classe **correta daquele player** (nome, cor, brasão e cards — não os do
      player local).
- [ ] Sem hover, a linha de cada player com classe do mod já mostra **indicação visual da classe** (cor do nome
      e/ou brasão) — inclusive players criados com o jogo em idioma diferente do observador.
- [ ] Player com perfil vanilla (sem classe do mod) não ganha popover nem identidade na própria linha; nenhum erro
      no console do cliente.
- [ ] Com o mod server **desatualizado ou sem a rota do catálogo**, o cliente degrada para o comportamento atual
      (popover só no player local), com no máximo **1 aviso** no log — sem exceção e sem re-tentativas em loop.
- [ ] **Fika/multiplayer:** validado rodando como **cliente** (não-host) — host mascara bugs de cliente (memória
      `feedback_coop_multiplayer_sync`). Host e cliente veem, cada um, a classe correta do outro.
- [ ] **Estado entre raids:** em raid1 → exit → raid2 na mesma sessão, o popover per-player funciona de novo na
      segunda tela de carregamento (cache não corrompe; ícones não vêm vazios); alt-F4/morte/MIA não quebram o
      próximo carregamento.

## Corner cases

- [ ] **Idiomas mistos no grupo:** perfil criado com o jogo em pt × en grava nomes diferentes — o matching aceita
      os dois; classes em que o nome en == pt não podem gerar resolução duplicada/ambígua.
- [ ] **Perfil "órfão"** (emendado no code-review 01, CR-01-03): classe renomeada ou apagada no editor web
      depois da criação do perfil → player sem identidade, sem crash, **sem log** (o server não distingue edition
      órfã de edition vanilla legítima; comportamento seguro — diagnóstico via editor web quando necessário).
- [ ] **Dois players com a mesma classe:** ambos resolvem e mostram o popover correto (cache por classe, não por
      player).
- [ ] **Late join/reconnect:** linha de player adicionada tardiamente à tela de carregamento também ganha o hover.
- [ ] **Ícone ausente no cliente** (arquivo da classe remota não existe): popover degrada para versão sem brasão
      (comportamento já existente do painel), sem quad branco.
- [ ] **Catálogo ainda não respondeu** quando a tela de loading abre (latência/servidor lento): a UI não trava nem
      bloqueia o carregamento; identidade aparece se/quando resolvida (mínimo aceitável: comportamento atual).
- [ ] **Mapa com trânsito** (ex. Streets): na segunda passagem pela tela de carregamento, os hovers são
      re-adicionados (mesma família do CR-02-03 do 055).
- [ ] **Player entrando como SCAV** (emendado na review técnica 01, PA-01-02): raid scav **local** → nenhuma
      linha ganha identidade (patch no-op). Player **remoto** em raid scav pode exibir a classe do PMC do dono —
      **limitação conhecida** do mecanismo por nickname (o FIKA usa o nickname do PMC no loading e o side da raid
      remota não trafega); cosmético e aceito.
- [ ] **Nicknames duplicados no servidor** (se o SPT permitir): resolução ambígua não pode crashar nem travar a
      tela — aceitar a primeira correspondência ou nenhuma, de forma determinística.

## Fora de escopo

- Outras superfícies coop — painel de ready do lobby, nametag in-raid, ícone no chat (decisão do usuário
  2026-07-03; candidatas a item futuro).
- Efeitos de perks/drawbacks per-player (por design os efeitos são locais de cada cliente).
- Mudanças no editor web e no schema de classes.

## Referências

- Entrada 057 em [mod-backlog.md](../mod-backlog.md) (linha "Identidade de classe per-player em coop").
- [055-class-detail-lobby/](../055-class-detail-lobby/) — popover reusado (hover-only + zoom).
- [HANDOFF.md](../../HANDOFF.md) — pendência #6 (057) e constraint de coordenação do `modded/Server`.
- Memória: `feedback_coop_multiplayer_sync` (validar como cliente, não só host).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-23 | Item registrado no `mod-backlog.md` (sessão 10 do redesign 11→6) |
| 2026-07-03 | Spec funcional criada via `/create-spec`; decisões do usuário: escopo restrito ao **loading FIKA** e `modded/Server` **liberado** para esta sessão |
| 2026-07-03 | Revisão `/review-spec` — 2 gaps (identidade na linha sem hover; mecanismo de resolução rebaixado a hipótese/decisão da spec técnica) + 2 corner cases (SCAV, nickname duplicado) corrigidos |
| 2026-07-03 | Corner SCAV emendado pela review técnica 01 (PA-01-02): no-op em raid scav local; scav remoto pode exibir classe do PMC do dono (limitação do mecanismo por nickname, documentada) |
