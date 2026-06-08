# 002 — Schema de classe + loader multi-classe

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-07

## Visão geral

Transformar a classe única hardcoded do item 001 num **sistema dinâmico baseado em arquivos**: **um arquivo JSON por classe**, numa pasta de config do mod, carregado na inicialização. Cada arquivo válido vira uma edition selecionável no launcher, com sua identificação, perfil base, descrição e skills iniciais. **Adicionar uma classe nova = soltar um `.json` na pasta** (sem recompilar). Este item externaliza o que o 001 fazia em código (id, base, descrição, skills); o **schema é projetado para crescer** nos itens seguintes (itens/equip no 003, outfits no 004, multiplicadores no 005…), mas o 002 entrega apenas id/base/descrição/skills.

## Comportamento atual

O mod (001) registra **uma** classe ("Test Class") **hardcoded** no código. Não há como adicionar, remover ou ajustar classes sem editar o código-fonte e recompilar. O `BaseEdition`, a descrição e as skills são constantes embutidas.

## Comportamento desejado

Na inicialização, o mod lê **todos** os arquivos `.json` de uma pasta de config dedicada (uma por classe) e registra cada classe válida como uma edition no launcher — honrando, por arquivo: identificação (nome exibido), **perfil base** (default "SPT Zero to hero" = stash vazio, se omitido), descrição e skills iniciais. Editar/adicionar/remover uma classe passa a ser editar a pasta de JSONs e reiniciar o servidor — **sem recompilar**. Arquivos inválidos são **ignorados com log claro**, sem derrubar o servidor nem impedir o carregamento das demais classes. Os JSONs de config acompanham a instalação do mod (ficam na pasta do mod no servidor).

**Validade dos campos:** a **identificação/nome é obrigatória**; **base, descrição e skills são opcionais** (base ausente → default "SPT Zero to hero"; skills ausentes/vazias → classe válida sem alteração de skill; descrição ausente → cai no nome). O mod acompanha um **arquivo de exemplo** documentando o formato.

<!-- review: decisões de UX/formato a confirmar antes do /create-technical-spec:
(a) caminho da pasta de config no install que o usuário vai editar (proposta: user/mods/CustomClasses/config/classes/*.json);
(b) a chave/identidade da edition vem de um CAMPO no JSON ou do NOME do arquivo? (afeta colisão e "soltar arquivo = nova classe");
(c) suportar um campo "enabled" (bool) para desligar uma classe sem apagar o arquivo, como o RZCustomProfiles tinha? -->


## Critérios de aceite

- [ ] Com N arquivos de classe válidos na pasta de config, o launcher lista **N editions** (uma por arquivo), cada uma com seu nome e descrição.
- [ ] Criar um perfil com qualquer classe aplica **as skills iniciais configuradas naquele arquivo** (verificável in-game).
- [ ] O **perfil base** de cada classe é lido do arquivo (ex.: classe com base "SPT Zero to hero" nasce com stash vazio); se o campo for omitido, usa o default "SPT Zero to hero".
- [ ] **Adicionar um arquivo JSON novo** e reiniciar o servidor faz uma edition nova aparecer — **sem recompilar** o mod.
- [ ] Um arquivo malformado/inválido é **ignorado com log claro** e as demais classes válidas continuam carregando (servidor não quebra).
- [ ] Após build/deploy, os arquivos de classe estão presentes na pasta do mod instalada no servidor e são efetivamente lidos.
- [ ] No boot, o servidor loga um **resumo**: quantas classes foram carregadas e quantos arquivos foram pulados (com o motivo de cada skip).
- [ ] O mod inclui um arquivo de classe de **exemplo** (documentando o formato) que carrega como uma classe válida.

## Corner cases

- [ ] **Pasta vazia / nenhum `.json`:** o mod registra zero classes, loga que nenhuma foi encontrada, e o servidor roda normalmente (edições nativas intactas).
- [ ] **JSON com erro de sintaxe:** o arquivo é pulado com log de erro identificando o arquivo; os outros carregam.
- [ ] **Campo obrigatório ausente** (sem identificação/nome): arquivo pulado com log. Campos opcionais ausentes (base/descrição/skills) usam default, sem pular.
- [ ] **Classe desabilitada** (se o campo `enabled` for adotado — ver review): `enabled:false` faz a classe não registrar, sem erro (apenas info no log).
- [ ] **Chave de edition duplicada** (entre dois arquivos, ou vs. edição nativa/de outro mod): não sobrescrever silenciosamente — pular a duplicata com aviso.
- [ ] **Skill inválida ou nível fora do intervalo** num arquivo: pular aquela skill / limitar o nível, com log — sem invalidar a classe inteira.
- [ ] **Perfil base inválido** num arquivo (chave que não existe): abortar **apenas aquela classe** com log claro (mesmo padrão do 001), sem afetar as demais.
- [ ] **Acentos/encoding** (nome/descrição em pt-BR): tratado como UTF-8 corretamente (sem mojibake).

## Fora de escopo

- Itens iniciais (stash/equipado/composto) — item 003.
- Outfits/skins — item 004.
- Multiplicadores de skill — item 005.
- Compatibilidade com Skills-Extended — item 006.
- Migração das 10 classes do RZCustomProfiles — item 007.
- Locale keys reais por idioma / seletor F12 — item 008 (aqui a descrição segue como no 001).
- Coexistência com o RZCustomProfiles (clobber) — item 007.
- **Hot-reload** (reler classes sem reiniciar o servidor) — fora de escopo; reinício é necessário.
- Tratamento fino de `LastAccess`/`Max`/`Min` para skills não presentes no perfil base (CR-01-01, herdado do 001) — entra aqui pois as skills passam a ser arbitrárias por JSON.

## Referências

- Item 001 (base do mecanismo): [001-walking-skeleton-01-spec.md](../001-walking-skeleton/001-walking-skeleton-01-spec.md) · [as-built](../001-walking-skeleton/001-walking-skeleton-05-asbuild.md)
- Achados herdados: CR-01-01 (skills arbitrárias) e CR-01-04 (magic strings → JSON) do [code review 01](../001-walking-skeleton/001-walking-skeleton-04-code-review-01.md).
- Scripts/anchor/balance reaproveitáveis do RZCustomProfiles ([mods/RZCustomProfiles/scripts/](../../../RZCustomProfiles/scripts/)).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Item criado via `/add-backlog-item` |
| 2026-06-07 | Spec funcional criada via `/create-spec` |
| 2026-06-07 | Revisão `/review-spec` — firmado validade de campos (obrigatório vs opcional) + exemplo; +2 critérios (resumo no log, arquivo de exemplo); +2 corner cases (campo obrigatório, classe desabilitada); 3 decisões de UX/formato marcadas |
