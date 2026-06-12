# 036 — Modo comparação A×B no dashboard — Spec

**Mod:** CustomClasses
**Status:** Especificado
**Criado:** 2026-06-12
**Origem:** [036-comparacao-classes-00-kickoff.md](./036-comparacao-classes-00-kickoff.md)
**Refs:** [031-01-spec](../031-skills-ordem-canonica/031-skills-ordem-canonica-01-spec.md) · [033-02-spec-tech](../033-detalhe-single-screen/033-detalhe-single-screen-02-spec-tech.md)

## Visão geral

A tela de detalhe de uma classe (**classe A**) ganha um modo **comparação A×B**: o usuário escolhe uma segunda classe (**B**) num picker "Compare with…" no header e a tela passa a mostrar, lado a lado, as diferenças entre A e B — skill a skill (delta colorido), custo ponderado de skills, valor ₽ do loadout, nº de skills, e os multiplicadores de XP das duas. A comparação é **read-only e efêmera**: não altera nenhum arquivo de classe e some ao limpar o picker.

A motivação é de balanceamento: a matriz (032) dá a visão panorâmica de todas as classes; falta a comparação **profunda de duas**. Sem este modo, comparar "o Caçador está mais forte que o Batedor?" exige abrir duas abas do browser lado a lado. Com a ordem canônica fixa do `SkillCanonicalList` (031), o delta por linha torna a leitura instantânea.

## Comportamento atual

- O detalhe (`ClassDetail.razor`, dashboard 2 colunas do 033 + coluna direita visual do 034) mostra **uma** classe: badges de custo no header (skill cost ponderado, loadout ₽, base edition), `SkillCanonicalList` na coluna esquerda (skills em ordem canônica, barras, custo inline, chip ±% de multiplicador), hideout, outfit, e a coluna direita com gear/stash visuais.
- O componente `SkillCanonicalList` (031) **já nasceu** com o parâmetro `Compare` (classe B) e a coluna de delta ▲/▼ implementados — porém **nenhum caller passa** `Compare` hoje, então o modo está inerte.
- Não há picker de segunda classe nem deep-link de comparação.

## Comportamento desejado

- **Picker "Compare with…" no header:** ao lado das ações existentes (Edit/Duplicate/Delete), um seletor lista as **demais** classes válidas (exclui a própria A). Escolhida a classe B, a tela entra em modo comparação. Cada item do picker mostra o nome da classe na sua cor (`NameColor`) e, quando houver, o ícone — coerente com a identidade visual já usada na matriz (032) e no sidebar (030).
- **Coluna de delta por skill (ativar 031):** o `SkillCanonicalList` da coluna esquerda passa a receber `Compare="@_compareDef"`. Cada linha ganha a coluna de delta = `B.level − A.level`: **▲ verde** quando A > B (A leva vantagem), **▼ vermelho** quando A < B, **=** neutro quando iguais (inclusive ambas 0). Skills que só B tem aparecem na seção de transbordo (já tratado pelo componente).
- **Deltas de resumo no header:** quando há B selecionada, os badges de custo passam a mostrar A **e** B com o delta entre eles:
  - **Skill cost (ponderado):** `A.Total` vs `B.Total` + delta numérico (sinal/cor).
  - **Loadout ₽:** `A.TotalRub` vs `B.TotalRub` + delta em ₽ (sinal/cor).
  - **Nº de skills:** contagem de skills definidas (nível > 0) de A vs B + delta.
- **Multiplicadores XP lado a lado:** a visão de multiplicadores ±% das duas classes na mesma linha — a forma exata (chips A | B por skill, dentro do próprio `SkillCanonicalList` ou num bloco compacto) fica para a spec técnica decidir reusando o que já existe; sem reescrever o componente.
- **Hideout / outfit:** comparação simples em duas colunas compactas (sem o visual rico do loadout). Fora de escopo aprofundar.
- **Limpar comparação:** uma ação "×"/"Clear" no picker volta ao dashboard de classe única (estado de antes).
- **Deep-link `?compare=<classe>`:** a URL do detail aceita `?compare=<fileName-sem-extensão>`; abrir essa URL já entra em modo comparação com B resolvida. Selecionar/limpar B no picker atualiza a query (compartilhável). `<classe>` inválida/inexistente/igual a A → ignora silenciosamente (volta a single).

## Premissas registradas (decisões autônomas)

- **PA-036-01 — "B fixa enquanto A navega" via query param, NÃO via NavMenu.** O kickoff (DoD) pede que trocar a classe A pelo sidebar mantenha B ativa. Preservar a seleção de B **ao navegar pelo sidebar** exigiria o NavMenu (030) propagar `?compare=` em cada link de classe — território do 030. **Decisão:** este item implementa B **só via query param `?compare=` no `ClassDetail`**. Enquanto a navegação interna não propagar a query, trocar A pelo sidebar **perde** a comparação (o usuário reescolhe B, ou edita a URL). A propagação automática da query na navegação fica para o 035 (densidade/cliques/preferências de navegação) ou um follow-up do 030. Registrado como limitação conhecida, não bloqueia o DoD essencial ("comparar 2 classes em 2 cliques a partir do detail").
- **PA-036-02 — Persistência em localStorage fora de escopo.** O kickoff já joga a memória da última escolha de B para o 035. Aqui a comparação é efêmera (só URL).
- **PA-036-03 — `SkillCanonicalList` NÃO é reescrito.** O parâmetro `Compare` e a coluna de delta já existem (031). Este item apenas **passa** `Compare` e adiciona CSS de delta se necessário. Qualquer ajuste no componente é aditivo e mínimo (ex.: chip de multiplicador de B), nunca uma reescrita.
- **PA-036-04 — Coluna direita (gear/stash visual do 034) intacta.** O picker e os deltas vivem no **header** e na **coluna esquerda**. A coluna direita (034) não é tocada; nenhuma interseção de território.
- **PA-036-05 — Picker exclui classes inválidas.** Só classes com `Definition` parseável e diferentes de A entram no picker (comparar contra um arquivo que não parseia não tem dados). Classes desabilitadas: incluídas (a comparação é de design, não de runtime), mas sinalizadas se a UI permitir sem custo.

## Critérios de aceite

- [ ] No detalhe de uma classe A, abrir o picker "Compare with…" e escolher B entra em modo comparação em **2 cliques** (abrir + escolher); o picker lista todas as outras classes válidas, cada uma com sua cor/ícone, e nunca a própria A.
- [ ] Em modo comparação, cada linha do `SkillCanonicalList` mostra o delta `B−A`: ▲ verde quando A>B, ▼ vermelho quando A<B, = quando iguais; skill que só B possui aparece na seção de transbordo.
- [ ] Os badges do header passam a mostrar A vs B com delta para: skill cost ponderado, loadout ₽ e nº de skills (sinal e cor coerentes — vantagem de A em verde).
- [ ] Os multiplicadores de XP de A e B aparecem lado a lado (mesma skill, dois valores), sem reescrever o `SkillCanonicalList`.
- [ ] Abrir a URL `/customclasses/classes/<A>?compare=<B>` entra direto em modo comparação; selecionar/limpar B atualiza a query; `<B>` inválida ou igual a A é ignorada (volta a single).
- [ ] Limpar a comparação ("×"/Clear) volta exatamente ao dashboard de classe única, sem recarregar a página.
- [ ] Nenhum arquivo de classe é escrito em todo o fluxo (comparação read-only/efêmera).

## Corner cases

1. **B = A (deep-link aponta para a própria classe):** ignorado — entra em single (não faz sentido comparar consigo).
2. **`?compare=` aponta para arquivo inexistente ou que não parseia:** ignorado silenciosamente; UI fica em single (sem erro vermelho — é só um link velho/quebrado).
3. **A navega pelo sidebar com B ativa:** perde a comparação (PA-036-01) — limitação registrada; B é reescolhida pelo picker.
4. **A ou B sem nenhuma skill (classe "pelado"):** todas as linhas com delta válido (`0 − 0 = =`, ou `▲/▼` quando só uma tem nível); contagem de skills 0 de um lado, delta = -count do outro.
5. **Skill que só B define:** aparece na seção de transbordo do componente (já tratado em `BuildOverflowEntries` do 031, que inclui chaves de `Compare`).
6. **B desabilitada/inválida selecionada via picker:** picker só oferece classes parseáveis (PA-036-05); desabilitada é comparável (design-time).
7. **Multiplicador de XP definido por A mas não por B (ou vice-versa):** lado sem multiplicador mostra vazio/—; o outro mostra o chip ±%.
8. **Loadout ₽ com preço faltante em A ou B:** o total já incorpora 0 para itens sem preço (`CostService`); o delta usa os totais como vêm — sem tratamento especial (o aviso de preço faltante continua no fluxo single da própria classe).

## Fora de escopo

- Propagação automática de `?compare=` na navegação do sidebar (NavMenu, 030) — ver PA-036-01.
- Persistência da última escolha de B em `localStorage` (035) — ver PA-036-02.
- Comparação de **mais de duas** classes (a matriz 032 cobre a visão N-classes).
- Comparação visual rica de gear/stash entre A e B (a coluna direita do 034 fica single); só hideout/outfit ganham 2 colunas textuais compactas.
- Qualquer mudança no modelo de custo, pesos, schema da classe ou no algoritmo do `CostService`/`SkillCanonicalList`.
