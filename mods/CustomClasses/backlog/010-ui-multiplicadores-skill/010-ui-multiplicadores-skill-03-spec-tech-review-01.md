# 010 — UI dos multiplicadores de skill · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [010-ui-multiplicadores-skill-02-spec-tech.md](010-ui-multiplicadores-skill-02-spec-tech.md)
**Data:** 2026-06-07

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 5 · Total: 5
>
> Todos os pontos resolvidos no `/code-mod` (defaults aceitos sob a autorização "siga"). Ver as-built §"PA resolvidos".

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge Case | 🟡 | Borda da classe x destaque de Elite (sobrescreve/apaga laranja) | ✅ Resolvido |
| PA-01-02 | B — Edge Case | 🟡 | Glyphs ▲/▼ podem não existir na fonte TMP do EFT | ✅ Resolvido |
| PA-01-03 | A — Gap | 🟡 | Reset explícito da borda quando sem fator (stub ≠ §7) | ✅ Resolvido |
| PA-01-04 | C — Erro de Lógica | 🟢 | Marcador esticado + raycastTarget cobre o nome inteiro | ✅ Resolvido |
| PA-01-05 | A — Gap | 🟢 | `____name` pode ser nulo em skills bloqueadas/estado parcial | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 Bloqueador · 🟡 Importante · 🟢 Menor

---

## Pontos

### PA-01-01 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-06-07

**Borda da classe vs. destaque de Elite**

**Problema:** o vanilla pinta `SkillIcon._border.color` de **laranja** (`Color32(183,112,0,255)`) quando a skill é Elite e **branco** caso contrário (`SkillIcon.Class3053.method_1`, confirmado na decompilação). O `SkillIconBorderPatch` sobrescreve `_border.color` para verde/vermelho sempre que há fator ≠ 1 — **apagando o laranja de Elite** numa skill que seja Elite *e* tenha multiplicador. E o reset proposto no §7 ("voltar p/ branco quando sem fator") apagaria o laranja de Elite numa skill Elite **sem** fator.

**Por que importa:** perde-se o indicador visual de Elite (moldura laranja) — conflito direto com o corner case da spec funcional "Elite coexiste sem conflito".

**Sugestão:** no `SkillIconBorderPatch`, ler `skill.IsEliteLevel` (membro de `SkillClass`, usado pelo próprio vanilla) e:
- **com fator e não-Elite** → pinta verde/vermelho;
- **com fator e Elite** → **manter laranja** (não pintar) — Elite tem precedência visual; o buff/debuff continua sinalizado pela seta `±X%` + tooltip ao lado do nome (que não dependem da borda);
- **sem fator** → resetar para `Color.white` **apenas se não-Elite** (Elite mantém laranja).

Alternativa (se você preferir que o buff "ganhe" da moldura de Elite): pintar verde/vermelho mesmo em Elite. Decidir.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (Elite tem precedência na borda; buff/debuff fica na seta+tooltip)
- `[ ]` Caminho alternativo: _________________

**Resolução:** `SkillIconBorderPatch` só pinta verde/vermelho quando `!skill.IsEliteLevel`; Elite mantém o laranja vanilla. ([SkillIconBorderPatch.cs](../../modded/Client/Patches/SkillIconBorderPatch.cs))

### PA-01-02 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-06-07

**Glyphs ▲/▼ podem não existir na fonte TMP**

**Problema:** `MultiplierFormat.Marker()` usa `▲` (U+25B2) e `▼` (U+25BC). A fonte TMP do `_name` (Bender/Tarkov) pode **não** ter esses glyphs no atlas → renderiza `□` ou nada.

**Por que importa:** o marcador é o elemento central do pedido; um quadrado no lugar da seta quebra a UX.

**Sugestão:** não cravar ▲/▼. Implementar com fallback verificável in-game: 1ª opção setas Unicode; se aparecer `□`, trocar por alternativa garantida — (a) só texto colorido `+50%` / `-30%` (sem seta), ou (b) caracteres ASCII `^`/`v`, ou (c) reaproveitar o **sprite** da seta vanilla (`_effectivenessUp/Down` têm o sprite). Manter a escolha **isolada em `MultiplierFormat.Marker()`** para trocar num único ponto. Registrar no as-built qual ficou.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (setas Unicode com fallback p/ só-texto colorido; validar in-game)
- `[ ]` Caminho alternativo: _________________

**Resolução:** ▲/▼ implementados e isolados em `MultiplierFormat.Marker()` (ponto único de troca). **Validar in-game:** se aparecer `□`, trocar lá por só-texto. ([MultiplierFormat.cs](../../modded/Client/MultiplierFormat.cs))

### PA-01-03 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-06-07

**Reset da borda quando sem fator — stub não implementa o que o §7 exige**

**Problema:** o stub do `SkillIconBorderPatch` (§5) só pinta quando há fator e comenta "sem multiplicador: não mexe". Mas o §7 ("Riscos") afirma que é preciso **resetar `_border.color` p/ branco** quando sem fator, senão a cor vaza entre células recicladas (scroll reusa o `SkillIcon`, e `Show` é chamado por célula). Os dois trechos se contradizem.

**Por que importa:** sem o reset, ao rolar a lista uma skill **sem** fator pode herdar a borda verde/vermelha de uma skill **com** fator que ocupou a mesma célula antes — bug visual silencioso.

**Sugestão:** consolidar: no `SkillIconBorderPatch`, ramo `else` (sem fator) → `____border.color = skill.IsEliteLevel ? <laranja vanilla> : Color.white;` (casa com PA-01-01). Atualizar o stub do §5 no code-mod para refletir isso. Reproduzir a cor laranja vanilla via constante (`new Color32(183,112,0,255)`).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** ramo `else` (sem fator) reseta `____border.color = skill.IsEliteLevel ? EliteBorder : Color.white` — casa com PA-01-01 e evita vazar cor entre células recicladas. ([SkillIconBorderPatch.cs](../../modded/Client/Patches/SkillIconBorderPatch.cs))

### PA-01-04 · C — Erro de Lógica · 🟢 Menor · ✅ Resolvido em 2026-06-07

**Marcador esticado (0,0)-(1,1) + `raycastTarget=true` captura hover do nome inteiro**

**Problema:** o marcador é filho do `_name` com anchors esticados cobrindo toda a área do nome e `raycastTarget=true`. O `HoverTooltipArea` então dispara ao passar o mouse em **qualquer ponto do nome**, não só sobre o `±X%`. Além disso, pode interceptar o raycast destinado ao próprio `_name` (se houver algum).

**Por que importa:** desvia levemente do pedido ("tooltip na setinha/percentual"); o tooltip da classe abre no nome inteiro. Geralmente é tolerável (superset), mas pode confundir.

**Sugestão:** aceitável como está (hover no nome+marcador). Se quiser precisão, ancorar o marcador à **direita** com largura fixa (`anchorMin=(1,0) anchorMax=(1,1)`, `pivot=(1,0.5)`, `sizeDelta.x≈90`) para o hover cobrir só a faixa do `±X%`. Decidir na implementação/validação visual; isolado no `GetOrCreateMarker`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (deixar esticado agora; ajustar anchor se incomodar no playtest)
- `[ ]` Caminho alternativo: _________________

**Resolução:** marcador esticado mantido (hover cobre nome+marcador). Anchor à direita fica como ajuste opcional pós-playtest.

### PA-01-05 · A — Gap · 🟢 Menor · ✅ Resolvido em 2026-06-07

**`____name`/`____border` nulos em estados parciais**

**Problema:** os stubs já fazem guard `____name is null` / `____border is null` (bom). Vale confirmar que `method_1`/`Show` nunca rodam com esses campos nulos no fluxo real (skills bloqueadas usam `_alphaBlockedSkill`, mas `_name`/`_border` são `[SerializeField]` sempre presentes no prefab).

**Por que importa:** se sempre presentes, o guard é só defensivo (ok). Se houver um caminho com prefab incompleto, o early-return evita NRE — comportamento correto (não desenha nada).

**Sugestão:** manter os guards `is null` (já presentes) + o try/catch com log. Nenhuma mudança necessária; ponto registrado para fechar a dúvida. Fechar como "defensivo, sem ação".

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão (manter guards; sem ação adicional)
- `[ ]` Caminho alternativo: _________________

**Resolução:** guards `is null` + try/catch mantidos nos dois patches (defensivo, sem ação adicional).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Review 01 criada via `/review-technical-spec` — 0 🔴 · 3 🟡 · 2 🟢 |
| 2026-06-07 | PA-01-01..05 resolvidos no `/code-mod` (defaults aceitos sob "siga") — ver as-built §"PA resolvidos" |
