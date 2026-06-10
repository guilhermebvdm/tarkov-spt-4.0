# 001 — skin-tagilla-academia

**Mod:** AutoGym
**Status:** Backlog
**Criado:** 2026-06-09

## Visão geral

Ao iniciar um treino na academia do hideout, o visual do torso do personagem (Upper Body) deve trocar temporariamente para a skin "Tagilla's Chest" (fornecida pelo mod AllTheClothes — torso nu tatuado do boss Tagilla). Ao encerrar o treino, o visual anterior do torso deve ser restaurado exatamente como estava. A troca é puramente cosmética e local à sessão: o perfil do jogador nunca é alterado.

## Comportamento atual

O AutoGym hoje faz duas coisas durante o treino na academia: completa o QTE automaticamente (seção `General` do F12) e oculta temporariamente o equipamento vestível — mochila, coletes, capacete, fone, máscara e óculos — restaurando tudo ao sair (seção `Visuals`, propriedade `Hide Workout Gear`). A roupa do personagem (skin de torso/pernas) não é tocada: o personagem treina com a Upper Body que estiver equipada no perfil.

O mod AllTheClothes (instalado em `D:\SPT\SPT\user\mods\AllTheClothes`) disponibiliza skins de bosses como roupas compráveis no Fence, entre elas a "Tagilla's Chest" (Upper Body, id `66a258e3c6b9ee37e81abcd2`, bundle `top_boss_tagilla.skin.bundle`).

## Comportamento desejado

- Ao iniciar o treino na academia, a Upper Body visível do personagem passa a ser "Tagilla's Chest".
- A Upper Body em uso no momento do início do treino é guardada em memória.
- Ao encerrar o treino (conclusão, cancelamento ou saída da estação), a Upper Body guardada é restaurada.
- A troca é apenas visual/runtime: o perfil persistido do jogador nunca registra a skin Tagilla por causa desta feature (não é necessário possuí-la/comprá-la no Fence).
- A feature pode ser ligada/desligada por propriedade própria no F12 (seção `Visuals`), independente de `Hide Workout Gear`.
- O estado da propriedade F12 é lido no **início** de cada treino; mudar a propriedade no meio de um treino em andamento não afeta o ciclo corrente (a restauração do ciclo já iniciado sempre acontece).

<!-- review: valor padrão da nova propriedade F12 — ligada (true) ou desligada (false)? Assumido true por consistência com Hide Workout Gear; confirmar. -->

## Critérios de aceite

- [ ] Iniciar treino na academia com a feature ligada troca o torso visível do personagem para "Tagilla's Chest" (verificável visualmente in-game).
- [ ] Encerrar o treino restaura o torso visível para a skin que estava equipada antes do treino, sem resíduo visual.
- [ ] Após treinar com a skin trocada, fechar e reabrir o jogo mostra o personagem com a skin original do perfil (nenhuma persistência da Tagilla's Chest).
- [ ] Com a feature desligada no F12, o treino ocorre sem nenhuma troca de skin.
- [ ] Com AllTheClothes ausente (ou a skin indisponível), o treino ocorre normalmente sem troca, sem erro visível e com aviso no log do cliente.
- [ ] A feature funciona de forma idêntica com `Hide Workout Gear` ligado ou desligado (as duas restaurações não interferem entre si).

## Corner cases

- [ ] Jogador já está usando "Tagilla's Chest" como skin do perfil → trocar/restaurar deve ser idempotente (nenhuma mudança visível, nenhum erro).
- [ ] Treinos consecutivos sem sair do hideout → cada ciclo troca/restaura corretamente, sem vazar estado do ciclo anterior (a "skin anterior" guardada é sempre a do perfil, nunca a Tagilla).
- [ ] Encerramento abrupto do treino (sair da estação no meio do QTE, erro durante o treino) → restauração deve ocorrer mesmo no caminho de exceção.
- [ ] AllTheClothes instalado mas bundle ausente/corrompido → falha de carregamento não pode travar o treino; comportamento é o mesmo de "skin indisponível".
- [ ] Personagem com corpo feminino (skins de Upper Body são específicas por corpo) → se a skin não for aplicável, tratar como indisponível (sem troca, sem erro).
- [ ] Início de treino disparado duas vezes sem encerramento entre eles (re-preparo da estação) → a "skin anterior" guardada não pode ser sobrescrita pela Tagilla (guardar apenas no primeiro início; segunda troca é no-op).
- [ ] Desligar a propriedade F12 durante um treino em andamento → a restauração do ciclo corrente ainda ocorre ao encerrar (nunca deixar o personagem preso na skin Tagilla).
- [ ] Sessão coop (FIKA presente) → a troca é estritamente visual e local; não pode enviar atualização de customização para o servidor nem para outros clientes.

## Fora de escopo

- [ ] A definir

## Referências

- [PROPRIEDADES.md](../../PROPRIEDADES.md) — propriedades F12 atuais do AutoGym
- Mod AllTheClothes — `D:\SPT\SPT\user\mods\AllTheClothes` (fonte da skin, id `66a258e3c6b9ee37e81abcd2`)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-09 | Item criado via `/add-backlog-item` |
| 2026-06-09 | Spec funcional criada via `/create-spec` |
| 2026-06-09 | Revisão `/review-spec` — 1 gap (config lida no início do ciclo) + 3 corner cases adicionados; 1 decisão pendente marcada |
