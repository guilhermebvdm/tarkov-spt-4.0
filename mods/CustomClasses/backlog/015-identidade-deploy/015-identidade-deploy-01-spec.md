# 015 — Identidade da classe na tela de deploy

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-08

## Visão geral

Aplicar a identidade da classe ao **nome do jogador local** em **todos os lugares** que usam o widget comum **`EFT.UI.ChatSpecialIcon`** (ícone "person" + nome) — confirmado como a "mesma variável" em: tela de **deploy** ("DEPLOYING TO LOCATION"), tela de **character** (`OverallScreen`/`PlayerProfilePreview`), lista **online players**. Para o jogador local: trocar o ícone "person" pelo **ícone da classe**, **colorir o nome** (gradiente, igual itens 011/012) e exibir o **título da classe** (nome da classe). Inspirado no selo de tipo de conta (Unheard), porém por classe.

**Por que um patch só:** `ChatSpecialIcon.Show(EMemberCategory, playerName, ...)` (campos `_icon` Image + `_specialLabel` TMP) é reutilizado nesses lugares — um postfix cobre todos. **Cuidado:** só customizar quando for o **jogador local** (comparar `playerName` com o nickname do perfil), para não recolorir outros jogadores.

## Comportamento atual

Na tela de deploy, o painel do jogador mostra um **ícone genérico de "person"** (silhueta) + o **nome do jogador em branco**. Edições especiais (Unheard) usam ícone + cor de nome próprios (via `EMemberCategory`/`ChatSpecialIconSettings`) — mas isso é fixo e não cobre classes do mod.

## Comportamento desejado

No painel do jogador da tela de deploy, para um perfil de **classe do mod**:
- **Ícone:** substituir o "person" pelo **ícone da classe** (PNG do item 011), com **tamanho controlado** (não maior que o ícone original).
- **Cor do nome:** o **nome do jogador** passa a usar a **cor da classe** (`nameColor`).
- **Nome da classe:** exibir também o **nome da classe** (texto) — ver opções de layout abaixo.
- Perfil de **edition vanilla** → comportamento original (sem mudança).

## Layout do nome da classe (decidido)

- **(A) 2ª linha — DESCARTADA:** em coop/Fika o painel lista vários jogadores; se o item tem **altura fixa**, a linha extra empurra/sobrepõe o jogador de baixo. Risco confirmado pelo usuário.
- **(B) Mesma linha (ESCOLHIDA):** `[ícone] <nome do jogador> [<nome da classe>]` — não altera a altura do item → sem conflito vertical com outros jogadores. Único cuidado: largura (há espaço à direita no painel).
- (C) Só ícone + tooltip — descartada (usuário quer o nome visível).

**Cor do nome (decidido):** **gradiente TMP** (cor da classe → tom mais claro) via `TextMeshProUGUI.colorGradient` / `VertexGradient` — efeito mais "premium" estilo selo especial. Fallback para cor sólida se o gradiente falhar.

## Critérios de aceite

- [ ] Na tela de deploy, perfil de classe → ícone da classe (no lugar do "person") + nome do jogador na cor da classe + nome da classe visível.
- [ ] O ícone respeita o **tamanho** do slot original (não estoura o layout).
- [ ] Perfil **vanilla** → painel original inalterado.
- [ ] Sem ícone configurado → mantém o ícone original (ou nenhum) + nome colorido; sem crash.
- [ ] Cor/estilo consistentes com o selo do menu/Skills (mesmo `nameColor`).

## Corner cases

- [ ] **Identificar o jogador local:** o `ChatSpecialIcon` é usado para QUALQUER jogador (online list, chat). Só aplicar a identidade quando `playerName` == nickname do perfil local (senão recolore outros jogadores com a classe errada).
- [ ] **Título da classe (2ª linha) em listas:** na tela de **character** (1 jogador) cabe abaixo do nome; em **listas** (online players, deploy com grupo) a 2ª linha pode conflitar com o item de baixo → nesses casos usar mesma-linha/sufixo ou omitir o título (só ícone+nome colorido).
- [ ] **Coop/Fika (vários jogadores no painel):** só o **jogador local** (dono do perfil) recebe a identidade da classe; outros jogadores ficam como estão. (A tela mostra também membros do grupo.)
- [ ] **Reuso/atualização do painel** (a lista de jogadores recicla itens): idempotência — não duplicar/contaminar entre jogadores.
- [ ] **Cor/ícone ausentes:** degrada limpo (nome colorido default / ícone original).
- [ ] **Efeito visual do Unheard:** a cor do nome em edições especiais pode ter gradiente/efeito — avaliar se aplicamos algo parecido (gradiente TMP) ou cor sólida.

## Decisões travadas (2026-06-08)

- **Só ChatSpecialIcon:** este item passa a ser o padrão **único** de identidade no nome do jogador. O **selo separado do 012** (menu-MO + tela de Skills) fica **desligado por padrão** (`ShowClassIdentity` default `false`), mas **reversível** no F12 (não removido do código) — pois o ChatSpecialIcon **não** cobre o painel grande do menu (recriado pelo MO) nem o topo da tela de Skills.
- **Título da classe:** exibido em **todos**; na tela de character (espaço) idealmente 2ª linha; em listas, **sufixo na mesma linha** `[Classe]` (sem risco de conflito). 1ª versão: **sufixo mesma-linha em todos** (seguro); 2ª-linha-no-character como refinamento (precisa detectar contexto).
- **Cor:** gradiente (reusa `ClassIdentityView.ApplyGradient`).
- **Jogador local:** identificado comparando `playerName` (passado ao `ChatSpecialIcon`) com o **nickname do perfil**, exposto pela rota (novo campo `nickname`).

## Fora de escopo

- Mudar o nome do jogador real (só a cor/estilo de exibição).
- Aplicar a outras telas além do deploy (menu/Skills são os itens 012).

## Referências

- Base (ícone/cor por classe): item 011 (`SkillMultipliers.IconFile/NameColor`, `ClassIconCache`).
- Selo do menu/Skills: item 012.
- EFT: painel de jogador do matchmaker/deploy (`EFT.UI.Matchmaker.*PlayerPanel` / `GroupMemberView` — a confirmar na tech-spec); cor de membership: `ChatSpecialIcon`/`ChatSpecialIconSettings` (`IconColor`).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-08 | Item criado (pedido do usuário, com print da tela de deploy) |
