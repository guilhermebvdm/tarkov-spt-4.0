# 030 — Sidebar persistente de classes — Spec

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-12
**Origem:** [030-sidebar-classes-00-kickoff.md](./030-sidebar-classes-00-kickoff.md)

## Visão geral

Trocar de classe no editor hoje custa 2+ cliques e perde o contexto da vista: é preciso voltar à lista, achar a linha e clicar. Este item transforma o drawer lateral (hoje só com Home e Classes) numa **sidebar persistente de classes** — sempre visível, lista TODAS as classes do `config/classes/` com ícone tingido, nome na cor da classe, custo compacto e um indicador de saúde por classe — onde **1 clique troca de classe preservando a vista atual** (detalhe → detalhe da outra; edição → edição da outra). A sidebar vira também um painel de saúde: filtro por nome no topo e um "dot" de status por classe (inválida / desabilitada / fora do orçamento), legível com zero cliques.

A troca em 1 clique a partir de um formulário de edição com mudanças pendentes **nunca pode descartar a edição em silêncio**: um guard pergunta Salvar / Descartar / Cancelar antes de navegar.

> **Premissa (UX-W1):** os links utilitários atuais (Home, Classes) e os do épico (ex.: matriz de Skills do 032, quando existir) convivem com a lista de classes no mesmo drawer — lista no corpo, utilitários no topo/rodapé.
>
> **Premissa (dado leve, contrato 037):** a lista de classes vem da view leve da cache do item 037 (uma projeção por navegação, sem `dry-run` por render). O custo de skills exibido é o custo ponderado (cálculo barato, sem `dry-run`/sem reconstrução de loadout), computado uma vez por navegação junto com a projeção — nunca por item num loop de render. O custo de loadout (₽) fica **fora** do rótulo da sidebar (caro o suficiente para não valer por classe na lista; permanece no detalhe/edição).

## Comportamento atual

- O drawer (`NavMenu`) tem apenas dois links fixos: **Home** e **Classes**. Trocar de classe = navegar até a lista (`/customclasses/classes`), localizar a linha e clicar (2+ cliques, perda do contexto da vista corrente).
- A lista completa de classes só existe na página `/customclasses/classes` (tabela). Status, custo e ícone/cor já são exibidos **lá**, mas não no drawer.
- O drawer é um `MudDrawer` `Variant=Mini` que expande no hover (`OpenMiniOnHover`), 250px, sempre aberto.
- Sair de uma edição com alterações pendentes via **Discard** recarrega do arquivo, mas **navegar para fora** (trocar de página/classe) descarta as alterações em silêncio — o aviso de "unsaved changes" foi registrado como opcional no item 025 e não foi implementado.

## Comportamento desejado

- **Sidebar lista todas as classes:** uma entrada por arquivo de `config/classes/` (incluindo desabilitadas e inválidas), com ícone tingido + nome na `nameColor` da classe + custo de skills compacto. Ordem determinística (mesma do `ListClassFiles`/`GetCachedEntries`). A entrada da classe atualmente aberta fica destacada (strip lateral + fundo).
- **1 clique preserva a vista:** clicar numa classe navega para a **mesma vista** em que o usuário está. Se está num detalhe (`/customclasses/classes/{x}`), vai para o detalhe da outra; se está numa edição (`/customclasses/classes/{x}/edit`), vai para a edição da outra. Fora dessas vistas (Home, lista), o clique abre o **detalhe** da classe.
- **Fallback de vista para classe inválida:** trocar de `edit → edit` para uma classe que **não tem definição parseável** (parse error) cai no **detalhe** dela (que mostra os diagnostics), porque não há formulário para editar.
- **Filtro por nome:** campo de filtro no topo da sidebar filtra as entradas por nome / displayName (substring, case-insensitive). Filtro vazio mostra tudo.
- **Dot de status por classe:** um indicador colorido por entrada — **vermelho** = inválida (sem definição OU qualquer diagnostic Error), **cinza** = desabilitada, **laranja** = custo de skills fora do orçamento [28, 32] (classe válida e habilitada, mas budget estourado); classe saudável (válida, habilitada, dentro do orçamento) sem dot de alerta. Classe sem skills (custo 0) é neutra (não é "fora do orçamento").
- **Guard de unsaved changes:** ao navegar para fora de uma edição com o formulário sujo (qualquer alteração não salva), um diálogo bloqueante oferece **Salvar** (persiste e então navega), **Descartar** (navega perdendo as mudanças) e **Cancelar** (permanece na edição). A navegação só acontece após a escolha — nunca descarta em silêncio.
- **Responsividade:** a sidebar mantém o comportamento mini/expand atual (colapsa para ícones em tela estreita); no estado colapsado o ícone tingido + dot de status continuam visíveis e o tooltip traz nome + status.
- **Sem lag perceptível:** navegar entre vistas não dispara `dry-run` de validação por classe (a lista vem da view leve da cache do 037); o filtro opera sobre a projeção em memória.

## Critérios de aceite

- [ ] A sidebar lista **todas** as classes de `config/classes/` (mesma contagem que a página `/customclasses/classes`), incluindo desabilitadas e inválidas, cada uma com ícone (quando há `iconFile`), nome na `nameColor` e custo de skills compacto.
- [ ] A partir do **detalhe** de uma classe, 1 clique noutra classe abre o **detalhe** dela; a partir da **edição** de uma classe, 1 clique noutra classe (válida) abre a **edição** dela — verificável pela URL resultante (`/{x}` vs `/{x}/edit`).
- [ ] 1 clique numa classe **inválida** a partir de uma edição abre o **detalhe** dessa classe (não a edição), e o detalhe exibe os diagnostics de erro.
- [ ] O dot de status reflete corretamente os quatro estados: vermelho (inválida), cinza (desabilitada), laranja (válida+habilitada mas custo de skills fora de [28,32]), nenhum/saudável — comprovável comparando com o `StatusChip` da lista/detalhe para o mesmo conjunto de classes.
- [ ] Digitar no filtro reduz a lista às classes cujo nome ou displayName contém o texto (case-insensitive); limpar o filtro restaura a lista completa — sem recarregar a página nem disparar revalidação.
- [ ] Navegar para fora de uma edição **com alterações pendentes** sempre abre o diálogo Salvar/Descartar/Cancelar: Cancelar mantém o usuário na edição com as mudanças intactas; Descartar navega perdendo-as; Salvar persiste (pipeline do `ClassEditorService.Save`) e então navega. Navegar a partir de uma edição **sem** mudanças não mostra diálogo.
- [ ] Trocar de classe pela sidebar **não** emite linhas de log de `dry-run` de validação de classe (a lista usa a view leve da cache do 037; a navegação reaproveita o cache quente).

## Corner cases

- [ ] **Edição com classe inválida aberta + clique noutra classe inválida:** ambas caem no detalhe; o guard de unsaved-changes não dispara porque uma classe sem definição parseável não tem formulário editável (não há estado sujo).
- [ ] **Classe ativa removida/renomeada por fora** (`/sync-classes` ou edição manual) enquanto a sidebar está aberta: a próxima projeção (via cache do 037, invalidada por mtime/length) não lista mais o arquivo antigo; o destaque de "ativa" simplesmente não casa com nenhuma entrada (sem erro). O arquivo novo aparece na próxima navegação.
- [ ] **Filtro que não casa com nenhuma classe:** a lista fica vazia com uma mensagem neutra ("no classes match"); os utilitários do topo/rodapé permanecem.
- [ ] **Clique na própria classe ativa** (mesma vista): navegação para a mesma URL — no-op visual, sem reabrir guard nem recarregar desnecessariamente.
- [ ] **Guard: Salvar falha por validação** (Error no dry-run do Save): a navegação é **abortada**, o usuário permanece na edição e os diagnostics do Save aparecem (nada é descartado nem navegado) — espelha o contrato do Save do 025 ("Error bloqueia o write").
- [ ] **Navegação iniciada por fora da sidebar** com edição suja (ex.: botão "voltar" do detalhe, link Home, refresh do browser): o guard de unsaved-changes cobre toda saída do circuito de edição que o framework permite interceptar; a saída via refresh/fechamento de aba do browser não é interceptável e fica **fora de escopo** (documentado).
- [ ] **`nameColor` ausente ou inválida:** a entrada renderiza com a cor padrão do tema (sem quebrar), espelhando o fallback já usado na lista/detalhe.
- [ ] **Sidebar colapsada (mini):** com só o ícone visível, o dot de status continua perceptível e o tooltip do item traz nome + status; classe sem `iconFile` mostra um glifo/inicial de fallback (degradação para texto, como na lista).

## Fora de escopo

- [ ] Recalcular custo de **loadout** (₽) por classe na sidebar — permanece no detalhe/edição (caro demais para a lista; o rótulo da sidebar usa só o custo de skills).
- [ ] Qualquer mudança no `ClassEditorService` além de expor a projeção leve `ListClassSummaries()` sobre a cache do 037 (leitura da cache; sem novo `dry-run`).
- [ ] Persistir o estado do filtro/scroll da sidebar entre navegações (a sidebar é re-renderizada por navegação; persistência é melhoria futura).
- [ ] Interceptar saída via refresh/fechamento de aba do browser (não interceptável de forma confiável no Blazor Server sem prompt nativo — não fará parte do guard).
- [ ] Reordenar/agrupar classes na sidebar (ordem é a determinística do `GetCachedEntries`).
- [ ] Otimização de performance da listagem/validação — é o território do item **037** (este item apenas consome a view leve da cache).

## Referências

- Kickoff: [030-sidebar-classes-00-kickoff.md](./030-sidebar-classes-00-kickoff.md)
- Performance / contrato da cache: [037-performance-cache-01-spec.md](../037-performance-cache/037-performance-cache-01-spec.md)
- Status/custo/ícone já implementados na lista: [024-class-viewer-01-spec.md](../024-class-viewer/024-class-viewer-01-spec.md)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-12 | Spec funcional criada via `/create-spec` (a partir do kickoff de 2026-06-10) |
