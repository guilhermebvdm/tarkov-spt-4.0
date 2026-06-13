# 012 — Identidade da classe no menu + tela de Skills

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-08

## Visão geral

Exibir a **identidade da classe** (ícone + nome da classe, colorido) em dois lugares: (1) no **menu principal**, perto do personagem; e (3) no **topo da tela de Skills**. Reusa o "selo" e o pipeline de dados/assets entregues no **011**. Faz o jogador reconhecer "qual classe sou" de relance, sem abrir o launcher.

## Comportamento atual

- A classe não tem marca visual no menu principal nem na tela de Skills (só no launcher e nos tooltips de skill do item 010).
- O usuário usa o **MoxoPixel-MenuOverhaul**, que cria um painel de personagem (nome/nível/exp) na tela inicial do menu. O menu **vanilla** (sem esse mod) não tem esse painel.
- A tela de Skills (com a barra de XP no topo) já existe, sem identidade da classe.

## Comportamento desejado

- **No menu principal:** mostrar **ícone + nome da classe** (nome colorido pela cor da classe):
  - **Com Menu-Overhaul:** integrado ao painel do personagem que ele cria (ao lado de nome/nível/exp).
  - **Sem Menu-Overhaul:** num **canto fixo** da tela do menu (posição/lado configurável), já que o menu vanilla não tem painel de personagem.
- **Na tela de Skills:** mostrar **ícone + nome da classe** no **topo** da tela.
- Perfil de **edition vanilla** (não-classe) → nada é exibido.
- Reabrir o menu/tela **não duplica** nem desloca o elemento.

## Critérios de aceite

- [ ] Entrar no menu com um perfil de classe → vejo **ícone + nome da classe** colorido perto do personagem (com Menu-Overhaul) ou no canto fixo (sem ele).
- [ ] Abrir a tela de Skills com um perfil de classe → vejo **ícone + nome da classe** no topo.
- [ ] Perfil de **edition vanilla** → nada aparece em nenhum dos dois lugares.
- [ ] Sem `iconFile` configurado → aparece só o nome colorido (sem ícone).
- [ ] Abrir/fechar o menu e a tela de Skills várias vezes **não duplica** nem desloca o selo.
- [ ] Funciona **com e sem** o Menu-Overhaul instalado.

## Corner cases

- [ ] **Ordem de carregamento vs Menu-Overhaul:** ambos mexem no menu; o selo deve aparecer corretamente independente de quem roda primeiro (sem ficar fora de posição ou ausente).
- [ ] **Nome de classe longo:** não estoura/quebra o layout (do Menu-Overhaul ou do canto fixo).
- [ ] **Tela de Skills aberta no menu e em raid:** a identidade aparece nos dois contextos (a classe é conhecida em ambos).
- [ ] **Cor/ícone ausentes ou inválidos:** degrada para só nome / cor default, sem crash (herdado do 011).
- [ ] **Reentrância:** o selo é criado uma vez e reaproveitado; não acumula GameObjects nem vaza assets entre aberturas.
- [ ] **Canto fixo (sem MO) — sem label vizinho:** não há texto de referência no menu vanilla para herdar a fonte; usar uma fonte fallback (TMP default) sem quebrar.
- [ ] **Master switch desligado:** com a config de identidade off (F12), nada é desenhado em nenhum dos dois lugares.

## Fora de escopo

- Botão "SKILLS" na navegação — item 013.
- Recolorir o nickname do jogador (decisão: só o nome da classe).
- Posicionamento "pixel-perfect" definitivo — ajuste fino é iteração visual durante o playtest.

## Referências

- Base reutilizável (selo + ícone + dados): item 011.
- Menu-Overhaul: `mods/SPT-Menu-Overhaul/` (GUID `com.moxopixel.menuoverhaul`).
- Briefing macro (011–013): plano aprovado em 2026-06-08.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-08 | Item criado (briefing aprovado) |
| 2026-06-08 | Spec funcional criada via `/create-spec` |
