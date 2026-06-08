# 004 — Outfits por classe · As-Built

**Mod:** CustomClasses
**Spec funcional:** [004-outfits-01-spec.md](004-outfits-01-spec.md)
**Spec técnica:** [004-outfits-02-spec-tech.md](004-outfits-02-spec-tech.md)
**Review técnica:** [review-01](004-outfits-03-spec-tech-review-01.md)
**Build:** 2026-06-07

> **Capacidade entregue** (DTO + builder + wiring), compila 0 warn/err. **Pendente:** popular as 10 classes com outfits (D1 — depende das escolhas de skin do amigo) + teste in-game. 🟡.

## Arquivos alterados

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `modded/Server/ClassDefinition.cs` | + `Outfit` (Usec/Bear) + `OutfitSide` (upper/lower). |
| CRIADO | `modded/Server/OutfitBuilder.cs` | Por lado: resolve a peça do customization DB, valida **slot** (upper=Body, lower=Feet — PA-01-01) + **facção** (`_props.Side`), seta `Customization.Body/Feet/Hands` e adiciona aos `Suits` (→ OBTAINED no criar perfil). Head/Voice não controláveis. |
| MODIFICADO | `modded/Server/CustomClassesMod.cs` | Injeta `OutfitBuilder` + chama no `RegisterClass` (Usec/Bear) + log com contagem de outfit. |
| CRIADO | `scripts/suits-catalog.json` | Catálogo de 147 peças (nome↔ID↔aparência, upper/lower, USEC/BEAR) gerado do customization DB — referência p/ popular outfits (D2). |

## PA resolvidos (review 01)

| ID | Resolução |
| --- | --- |
| PA-01-01 | `ApplyPiece` valida o slot (upper exige `Body`, lower exige `Feet`); senão pula com aviso e não adiciona aos Suits. |
| PA-01-02 | Campos do `CustomizationItem` confirmados ao codar (`Properties.Body/Feet/Hands` MongoId?, `Side` List<string>). |
| PA-01-03 | Comentado o comportamento lenient (Side nulo/vazio = sem restrição de facção). |

## Achados-chave (API de customization)

- **Head/Voice = escolha do jogador** na criação (`CreateProfileService.cs:58/61` sobrescrevem o template) → não controláveis por classe.
- **Body/Feet/Hands** vêm do template → controláveis.
- **Roupas viram OBTAINED** via `TemplateSide.Suits` → `AddSuitsToProfile` (`CreateProfileService.cs:134`). Só setar aparência sem isso = UNAVAILABLE.
- Peça de roupa = `_type:"Item"` no customization DB; upper tem `Body`+`Hands`, lower tem `Feet`; `_props.Side` = facção. (Salvo na memória global `reference_spt_customization_model`.)

## Pendências

1. ✅ Compila (0 warn/err, DLL 49.7 KB) + instalado.
2. **Popular as 10 classes** (D1) — aguardando o amigo escolher skin↔classe in-game (mapear nomes → IDs via `suits-catalog.json`, depois preencher `outfit` em `class-recipes.js`/JSONs).
3. **Teste in-game** — criar classe com `outfit` → nasce vestido + dono das peças (OBTAINED), USEC e BEAR.
4. Após (2)+(3) OK → 🟢.

## Mudanças posteriores

**2026-06-07 — code review 01 aplicada:** CR-01-01 (try/catch no `new MongoId` — id de roupa malformado pula só a peça, não a classe) + CR-01-03 (comentário defensivo). CR-01-02 (ownership de aparência-direta) aceito p/ reavaliar no playtest. Recompilado 0 warn/err (50.2 KB).

**2026-06-07 — OutfitBuilder endurecido p/ "aparência direta" (skins de mod):** `ApplyPiece` agora resolve a aparência cobrindo 2 padrões — vanilla (`_props.Body/Feet` referenciam a malha) E direta (ex.: AllTheClothes: `_props.Body/Feet` nulos + `_props.BodyPart=="Body"/"Feet"` → usa o próprio id). Validação de slot mantida (pula se não houver Body/Feet nem BodyPart correspondente). Compilado 0 warn/err (50.2 KB). **Wire de teste:** upper do **Operador Furtivo** trocado p/ `66a25a3af12f29d8a2599527` (AllTheClothes `top_boss_tagilla_nohead`, aparência direta) + lower vanilla — pra validar o caminho endurecido no próximo playtest. Soft-dep AllTheClothes (se faltar, builder pula o upper → base).

**2026-06-07 — outfit validado in-game:** perfil novo Caçador (TestClass6, USEC) nasceu com USEC Predator+Deep Recon EQUIPPED (CA atendido — skin aplicada/vestida, não só liberada). Causa do 1º teste falho: perfil era antigo (outfit só aplica na criação). Bug de thumbnail branco na tela de roupas = client-side (afeta vanilla também; modelo 3D ok), fora do escopo.

**2026-06-07 — 3 outfits de exemplo (teste antes da definição do amigo):** gerador (`build-class-jsons.js`) passa a emitir `outfit` quando a recipe define; adicionados outfits a 3 classes em `class-recipes.js` — Caçador (militar: USEC Predator + Deep Recon/Gorka SSO), Operador Furtivo (escuro: Cultist jacket + Outdoor Tactical/BEAR Recon), Saqueador (casual: Adik tracksuit + USEC Day Off/BEAR Oldschool). As outras 7 ficam sem outfit (controle). Regenerado + recompilado + instalado. **Provisório** — serão substituídos pelas escolhas finais do amigo (D1).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Capacidade implementada via `/code-mod` (DTO + OutfitBuilder + wiring). Compilado 0 warn/err. Review 01 (PA-01-01..03) dobrada. |
