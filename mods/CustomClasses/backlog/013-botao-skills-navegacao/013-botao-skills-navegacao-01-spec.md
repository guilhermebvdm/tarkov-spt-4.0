# 013 — Botão SKILLS na navegação do menu

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-08

## Visão geral

Adicionar um botão **"SKILLS"** na navegação do **menu principal**, **abaixo de "CHARACTER"**, que leve **direto à tela de Skills**. Atalho de conveniência para a tela onde a identidade da classe (012) e os multiplicadores (010) aparecem. É o item de **maior risco técnico** do conjunto (depende da navegação interna do menu do EFT) e por isso é desenvolvido por último.

## Comportamento atual

- O menu principal tem os botões CHARACTER / TRADE / HIDEOUT / etc. A tela de Skills só é acessível abrindo CHARACTER e navegando até a aba "SKILLS".
- Não existe atalho direto para Skills a partir do menu.

## Comportamento desejado

- Um botão **"SKILLS"** aparece **abaixo de "CHARACTER"** na navegação do menu principal.
- Clicar nele **abre a tela de Skills** diretamente (sem o usuário precisar abrir CHARACTER e trocar de aba manualmente).
- Funciona **com e sem** o Menu-Overhaul.
- Visualmente coerente com os demais botões da navegação (estilo herdado do botão CHARACTER).

## Critérios de aceite

- [ ] O botão **"SKILLS"** aparece **abaixo de "CHARACTER"** no menu principal.
- [ ] Clicar abre **a tela de Skills** (a mesma da aba SKILLS do personagem).
- [ ] O botão tem aparência consistente com os outros botões da navegação.
- [ ] Funciona **com e sem** o Menu-Overhaul instalado.
- [ ] Abrir/voltar ao menu várias vezes **não duplica** o botão.

## Corner cases

- [ ] **Ordem vs Menu-Overhaul:** o MO reposiciona os botões existentes; o botão SKILLS deve ficar no lugar certo (abaixo de CHARACTER) independente de quem roda primeiro.
- [ ] **Clique repetido / abrir já estando na tela:** sem erro nem telas empilhadas.
- [ ] **Reentrância:** o botão é criado uma vez e reaproveitado (não acumula clones a cada abertura do menu).
- [ ] **Sem regressão:** o botão CHARACTER (clonado como base) continua funcionando normalmente.
- [ ] **Robustez:** se a navegação interna falhar (mudança do EFT), o clique não deve travar o menu (degradar com log).

## Fora de escopo

- Identidade visual da classe (itens 011/012).
- Ícone próprio no botão SKILLS (pode reutilizar o ícone da classe do 011 como opcional, mas não é requisito).
- Reordenar/estilizar os demais botões do menu.

## Referências

- Navegação do menu / tela de Skills: investigação inicial no briefing (a tela é `SkillsAndMasteringScreen`; "SKILLS" não é item de menu nativo — abre-se via o fluxo do personagem). Detalhe técnico na spec técnica.
- Briefing macro (011–013): plano aprovado em 2026-06-08.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-08 | Item criado (briefing aprovado) |
| 2026-06-08 | Spec funcional criada via `/create-spec` |
