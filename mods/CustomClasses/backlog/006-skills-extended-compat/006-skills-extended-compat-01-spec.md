# 006 — Compat opcional com Skills-Extended

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-06-07

## Visão geral

Permitir que uma classe defina multiplicadores de XP para as skills **habilitadas pelo Skills-Extended (SE)**, aplicando-os só quando o SE está presente e sendo **no-op** quando ausente — sem dependência hard. O mecanismo de multiplicadores (item 005) é string-keyed sobre `SkillTypes` (server) / `ESkillId` (client).

## Comportamento atual

- O SE (`com.cj.SkillsExtended`) **não cria skills novas**: ele **revive** membros de `ESkillId`/`SkillTypes` que já existem no enum vanilla mas estavam inativos — confirmados: **`FirstAid`, `FieldMedicine`, `BearRawpower`, `UsecNegotiations`** (`SkillTypes.cs:42,43,66,70`; `SkillClassConstructorPatch.cs` do SE).
- Como essas skills **já estão no enum**, o loader do mod (005) **já as aceita** (`Enum.TryParse` + `Enum.IsDefined` passam) e o registry/rota já as serve; o client (010) já desenha borda/seta/tooltip pra qualquer `ESkillId`.
- **Sem o SE instalado:** essas skills existem no enum mas ficam "mortas" (não ganham XP / o jogo não as exibe ativas). Um multiplicador pra elas hoje é **registrado silenciosamente e nunca tem efeito** — sem aviso de que dependem do SE.
- Não há **detecção** do SE nem aviso ao usuário; não há documentação de quais skills são "do SE".

## Comportamento desejado

- **Com o SE instalado:** um multiplicador para uma skill do SE (ex.: `FirstAid: 1.5`) escala o ganho de XP dessa skill normalmente (igual a qualquer skill vanilla).
- **Sem o SE:** o mod carrega sem erro; um multiplicador para uma skill "do SE" é **inócuo** (não quebra nada) e gera um **aviso de log** claro ("skill X depende do Skills-Extended, que não está instalado — multiplicador ignorado/sem efeito").
- **Detecção soft** do SE (sem `BepInDependency`/referência hard): server via lista de mods carregados; client via plugins carregados. Ausência = degradação limpa.
- Documentar (README/_docs do mod) quais skills do SE são suportadas.

## Critérios de aceite

- [ ] Com SE instalado, `FirstAid: 1.5` numa classe faz a skill FirstAid ganhar XP ~50% mais rápido in-game.
- [ ] Sem SE, o servidor **inicia sem erro** com a mesma config de classe.
- [ ] Sem SE, há **um aviso de log** identificando a(s) skill(s) que dependem do SE e que ficaram sem efeito.
- [ ] O mod **não** declara dependência hard do SE (carrega e funciona com ou sem ele).
- [ ] A detecção do SE não usa nome de tipo/símbolo do SE em tempo de compilação (só string do GUID/nome do mod).
- [ ] **Exemplo testável:** o **Médico de Combate** ganha um buff de exemplo em `FirstAid` e `FieldMedicine` (skills do SE), visível na UI e funcional com o SE instalado.

## Corner cases

- [ ] **SE só no client ou só no server** (instalação assimétrica): a escala de XP é client-side (patch `OnTrigger`), o aviso é server-side; cada lado detecta seu próprio SE e degrada sozinho, sem assumir o outro.
- [ ] **Multiplicador de skill do SE com SE ausente:** inócuo + aviso, nunca crash nem bloqueio do carregamento das classes.
- [ ] **Skill inexistente em ambos** (nem vanilla nem SE): continua rejeitada com aviso (comportamento do 005 mantido).
- [ ] **Versão futura do SE** que adicione skills realmente novas (fora de `ESkillId`): fora do alcance do enum atual — declarar como limite conhecido (suportamos as 4 skills revividas que estão no enum vanilla).
- [ ] **SE detectado mas skill não habilitada** por config do próprio SE (o SE permite ligar/desligar skills): o multiplicador ainda registra; efeito depende do SE — aceitável (não temos visibilidade da config do SE).

## Fora de escopo

- Criar skills próprias ou modificar o Skills-Extended.
- UI específica para skills do SE — a UI do item 010 já cobre qualquer `ESkillId`.
- Ler/interpretar a configuração interna do SE (quais skills ele ligou).

## Referências

- Item 005 (mecanismo de multiplicadores): [005-skill-multipliers-01-spec.md](../005-skill-multipliers/005-skill-multipliers-01-spec.md)
- Item 010 (UI dos multiplicadores): [010-ui-multiplicadores-skill-01-spec.md](../010-ui-multiplicadores-skill/010-ui-multiplicadores-skill-01-spec.md)
- Skills-Extended: `mods/Skills-Extended/original/Plugin/` (GUID `com.cj.SkillsExtended`)

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Item criado via `/add-backlog-item` |
| 2026-06-07 | Spec funcional criada via `/create-spec` (achado: SE reusa ESkillId vanilla → mecanismo já suporta; escopo = detecção + aviso) |
| 2026-06-07 | `/review-spec` — escopo travado pelo usuário: "aviso + exemplo testável". +1 critério (exemplo no Médico de Combate: FirstAid/FieldMedicine). |
