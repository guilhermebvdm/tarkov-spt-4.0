# 057 — Fix 01 · Popover do loading nunca abria (guard de nickname falso-positivo)

**Mod:** CustomClasses
**Item raiz:** [057-class-identity-coop-01-spec.md](057-class-identity-coop-01-spec.md)
**Asbuild:** [057-class-identity-coop-05-asbuild.md](057-class-identity-coop-05-asbuild.md)
**Criado:** 2026-07-03
**Disparado por:** feedback in-game 2026-07-03 (gate humano) — "o painel não está aparecendo, mesmo com o mouse
em cima do boneco, ou clicando" (tela Deploying to Location, print do usuário; linha `mdj_tank2` sem tint e sem hover).

## Contexto

Na tela de loading do FIKA, nenhuma linha ganhou tint nem popover — nem a do player local. O tooltip nativo do
015 aparecia (EventSystem/raycast funcionando), mas o `LoadingClassHover` nunca era anexado.

## Causa raiz

O guard do **CR-01-05** (aplicado no code review 01) comparava `nickTmp.text != nickname` para espelhar o
early-return do `AddPlayer` do FIKA. Mas o FIKA seta o nome via **`TMP_Text.SetText()`**
(ref: fika-plugin/LoadingScreenPlayer.cs:24-27), e `SetText` **não atualiza a property `.text`** — ela devolve o
placeholder do prefab. Resultado: o guard disparava para TODAS as linhas → `return` antes do tint e do
`AddComponent<LoadingClassHover>`. (Refuta a resolução do CR-01-05 no
[04-code-review-01](057-class-identity-coop-04-code-review-01.md) — artefato citado, não editado.)

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/Client/Patches/ClassDetailLoadingPatch.cs` | Guard por texto REMOVIDO; espelho fiel do early-return do FIKA via mapa estático `SeenNetIds` (netId→1º nickname visto; divergência → no-op), limpo junto com o `Reset()` por instância da tela. |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `/compile-mod` sem erros (2026-07-03, client 0 erros)
- [ ] **In-raid:** hover na linha do loading abre o popover (local e remoto); tint da cor da classe na linha
- [ ] **Fika/multiplayer:** validado como CLIENTE com 2+ players de classes diferentes
- [ ] **raid1 → exit → raid2:** popover funciona na segunda tela (SeenNetIds/mapa resetados por instância)
- [ ] **alt-F4 / morte / MIA:** sem exceção no LogOutput.log
- [ ] Memória do mod atualizada (`/update-memory`) com a lição do fix (TMP `SetText` × `.text`)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-03 | Fix criado e aplicado (compilado; aguardando re-teste in-game) |
