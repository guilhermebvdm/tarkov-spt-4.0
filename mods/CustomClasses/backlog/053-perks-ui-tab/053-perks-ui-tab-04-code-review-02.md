# 053 — Perks/Drawbacks UI · Code Review 02 (requisitos de negócio)

**Mod:** CustomClasses
**Foco:** validação de **requisitos de negócio** — a UI de perks entrega o que o design (`docs/class-design.md`) promete, e o que promete de fato **existe** (vs. deferido)?
**Fontes:** `docs/class-design.md` (verdade de negócio) · `backlog/050-signature-patches-05-asbuild.md` (status implementado/deferido) · `PerksCatalog.cs` (o que a UI anuncia)
**Data:** 2026-07-01

> Complementa a Code Review 01 (qualidade de código). Aqui o eixo é **Categoria C — Gap vs. negócio**: a UI é uma **promessa ao jogador** (e, no servidor Fika Coop PVE, parte da identidade vista por outros). Promessa não cumprida = defeito de negócio.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 2 · 🟡 Médios: 3 · 🟢 Menores: 1 · ✅ Resolvidos: 4 · ⏸️ Deferidos: 2 · Total: 6
>
> **Aplicado (2026-07-01):** CR-02-01/02 → UI honesta (selo **"em breve"** âmbar em Combat Medic e Quick Hands); CR-02-03 → cópia do Iron Lungs corrigida; CR-02-04 → Sharpshooter mantido "sempre mais rápido" (decisão do usuário) + cópia e `class-design.md` alinhados à impl real. **Deferidos:** CR-02-05 (coop-sync → item 057), CR-02-06 (perfis legados, edge). Detalhe na seção **Resolução**.

**Completude ✅:** todos os perks/drawbacks das 6 classes do `class-design.md` estão no catálogo (nenhum perk faltando). **Chave de gating ✅:** o rename Ghost→Stealth **foi propagado** (`furtivo.jsonc` no install: `"name":"Stealth"`), então a chave EN do catálogo bate com o runtime.

**O problema é honestidade:** a UI anuncia como **ativos** efeitos que estão **deferidos** (não implementados) — Combat Medic (inteiro), Quick Hands (inteiro), Iron Lungs "less sway" (parcial). Um deles (Combat Medic) deixa a classe **só com o drawback funcionando** — pior que neutro.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | C — Gap vs. negócio | 🟠 Forte | Combat Medic anunciado mas **totalmente deferido** → Médico só com drawback ativo | ✅ Aplicado (A — selo "em breve") |
| CR-02-02 | C — Gap vs. negócio | 🟠 Forte | Quick Hands anunciado mas **deferido** (server-side, não implementado) | ✅ Aplicado (A — selo "em breve") |
| CR-02-03 | C — Gap vs. negócio | 🟡 Médio | Iron Lungs anuncia "less sway" mas o **sway está deferido** (fôlego/braço ok) | ✅ Aplicado (cópia corrigida) |
| CR-02-04 | C — Gap vs. design | 🟡 Médio | Sharpshooter sem a **penalidade de AR (×1.15)** do design — impl aplica ×0.85 em tudo | ✅ Aceito (manter simples + design alinhado) |
| CR-02-05 | D — Arquitetura/coop | 🟡 Médio | Perks de som (Ghost Step/Silent Looter/Loud Operator) são client-only → não sincronizam pra outros players (item 057) | ⏸️ Deferido → item 057 |
| CR-02-06 | C — Gap vs. negócio | 🟢 Menor | Perfis legados "Ghost/Fantasma" (pré-rename) resolvem pra nenhuma classe → UI mostra "vanilla" | ⏸️ Aceito (edge) |

## Categorias / Impacto

_(idênticas à Code Review 01)_

---

## Pontos

### CR-02-01 · C — Gap vs. negócio · 🟠 Forte

**Combat Medic é anunciado como perk ativo, mas está totalmente deferido — o Médico fica só com o drawback funcionando**

**Local:** [`mods/CustomClasses/modded/Client/PerksCatalog.cs:38-41`](../../modded/Client/PerksCatalog.cs#L38) · evidência: [`050-signature-patches-05-asbuild.md:105`](../050-signature-patches/050-signature-patches-05-asbuild.md#L105)

**Problema:** o catálogo mostra `Combat Medic — faster meds & surgery, surgery on the move` como perk (🟢 verde, "ativo"). Mas o asbuild 050 registra: *"**Combat Medic (Médico) DEFERIDO** — … precisa transpiler … cirurgia sem lock não localizável no estático … é a única feature do 050.3 que sobra."* Ou seja, **nada** do Combat Medic funciona in-game. O único efeito ativo do Médico é o **drawback** Shaky Hands (recuo ×1.25, implementado). Resultado: quem escolhe Médico vê uma promessa 100% não cumprida **e** joga com uma classe que, na prática, **só tem desvantagem**.

**Por que importa:** num servidor Coop PVE abrindo pra vários players, anunciar um perk que não faz nada — e ainda deixar a classe net-negativa — quebra a confiança e a proposta da classe. É o gap de negócio mais grave.

**Sugestão:** resolução é **decisão de produto** (3 caminhos):
- **(A) UI honesta (recomendado, barato):** marcar o Combat Medic como **"em breve/soon"** (acento âmbar + tag, efeito mostrado como planejado). Preserva o design à vista sem mentir. Reversível.
- **(B) Ocultar** o perk até implementar.
- **(C) Implementar** (grande: transpiler em `DoMedEffect` + investigação de runtime do lock de cirurgia) — fora do escopo de UI.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (A)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

### CR-02-02 · C — Gap vs. negócio · 🟠 Forte

**Quick Hands é anunciado como ativo, mas está deferido (server-side, não implementado)**

**Local:** [`mods/CustomClasses/modded/Client/PerksCatalog.cs:50-56`](../../modded/Client/PerksCatalog.cs#L50) · evidência: [`050-signature-patches-05-asbuild.md:121`](../050-signature-patches/050-signature-patches-05-asbuild.md#L121)

**Problema:** o catálogo mostra `Quick Hands — search 2 items at once`. O asbuild: *"**Quick Hands (Saqueador) DEFERIDO** → 'Search Double' é server-side … melhor ativar via server mod."* Não há efeito client-side ativo. O Saqueador ainda tem 2 perks reais (Silent Looter, Pack Mule), então não é net-negativo — mas o card mente.

**Por que importa:** mesmo caso do CR-02-01, impacto menor (a classe tem outros perks funcionando).

**Sugestão:** mesma resolução (A/B/C). Recomendo **(A) marcar "em breve"** junto com o CR-02-01. (C) aqui é um **server mod** (coordenar com a sessão do editor — toca `modded/Server`).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (A)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

### CR-02-03 · C — Gap vs. negócio · 🟡 Médio

**Iron Lungs anuncia "less sway", mas o sway está deferido (só fôlego + fadiga de braço funcionam)**

**Local:** [`mods/CustomClasses/modded/Client/PerksCatalog.cs:44-48`](../../modded/Client/PerksCatalog.cs#L44) · evidência: [`050-signature-patches-05-asbuild.md:122`](../050-signature-patches/050-signature-patches-05-asbuild.md#L122)

**Problema:** copy = `Iron Lungs — longer breath hold, less sway & arm fatigue`. O asbuild: *"**Iron Lungs sway DEFERIDO** (a duração foi feita; o sway é `BreathEffector.Process` — frágil)."* Fôlego (duração) e fadiga de braço estão ok; **"less sway" não**. Over-claim parcial.

**Por que importa:** o jogador espera menos oscilação de mira e não recebe. Menor porque 2/3 do perk funcionam.

**Sugestão:** **corrigir a cópia** removendo "less sway" (fix não-ambíguo): EN `longer breath hold & less arm fatigue` · PT `respiração longa e menos fadiga de braço`. Quando o sway for implementado (item 051 zona stances), re-adicionar.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

### CR-02-04 · C — Gap vs. design · 🟡 Médio

**Sharpshooter não tem a penalidade de AR (×1.15) prevista no design — a implementação aplica ×0.85 em TODAS as armas**

**Local:** [`mods/CustomClasses/modded/Client/Patches/ClassWeaponPatches.cs:145-149`](../../modded/Client/Patches/ClassWeaponPatches.cs#L145) · design: [`docs/class-design.md:52`](../../docs/class-design.md#L52)

**Problema:** o design do Sharpshooter (linha 52) é: pistola ×0.5 · **sniper/DMR ×0.85 (mais rápido)** · **fuzil de assalto (AR) ×1.15 (15% mais lento — penalidade fora da especialidade)**. A implementação:
```csharp
// 🔧 Sharpshooter (Caçador): ADS mais rápido (sempre).
if (... IsLocalClass("Hunter")) { ... _aimingSpeed *= SharpshooterAdsTime (0.85) ... }
```
aplica o ×0.85 **incondicionalmente em qualquer arma** — sem o ramo por `weapClass` (sniper/DMR vs AR). Logo o perk virou **buff puro** (ADS mais rápido até com AR), **sem o drawback de balance** que o design previa. A UI está correta em relação ao *código* (não menciona penalidade), mas **código e UI divergem do design**.

**Por que importa:** o Caçador fica mais forte que o desenhado (ADS rápido em tudo, zero downside). Afeta o balance validado no `class-matrix`.

**Sugestão:** **decisão de balance** — (A) implementar o ramo por `weapClass` (sniper/DMR → 0.85; AR → 1.15) pra casar o design *(code-mod em `ClassWeaponPatches`)*; ou (B) aceitar a simplificação "sempre mais rápido" e **atualizar o `class-design.md`** pra refletir. Não auto-corrijo (muda gameplay).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (A — implementar penalidade AR)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (aceitar simplificação + atualizar design): _________________

---

### CR-02-05 · D — Arquitetura/coop · 🟡 Médio

**Perks de som (Ghost Step / Silent Looter / Loud Operator) são client-only — não sincronizam pra outros players no Coop**

**Local:** perks de som no catálogo · gap conhecido: item 057 (identidade/sync coop) · memória global [[feedback_coop_multiplayer_sync]]

**Problema:** a UI anuncia "−30% de todo o ruído" (Ghost Step), "saque silencioso" etc. Esses efeitos são aplicados **client-side no player local**; a percepção de som/IA por **outros clientes** (Fika Coop PVE) depende de sync que ainda **não existe** (item 057). Então um Furtivo se acha silencioso pra si e pros bots do host, mas **outros players humanos** podem ouvi-lo normalmente.

**Por que importa:** o servidor vai abrir pra vários players — a promessa "silencioso" é parcialmente falsa no contexto multiplayer, que é o contexto real de uso.

**Sugestão:** deferir pro **item 057** (coop-sync já mapeado). Opcional: na UI, uma nota "efeito local" nos perks de som até o sync existir. Não bloqueia esta UI.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (deferir p/ 057)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### CR-02-06 · C — Gap vs. negócio · 🟢 Menor

**Perfis legados "Ghost/Fantasma" (criados antes do rename) resolvem pra nenhuma classe → UI mostra "vanilla"**

**Local:** gating via `SkillMultipliers.ClassNameEn` · evidência: `.bak1` órfão com `"name":"Ghost"` no install

**Problema:** o rename Ghost→Stealth foi propagado (`furtivo.jsonc` novo), mas um perfil criado sob a edition antiga ("Fantasma"/"Ghost") teria `GameVersion` que o registro atual não mapeia → `ClassNameEn` null → `LocalEntries` null → card "Classe vanilla — sem perks". Idem os efeitos 050.

**Por que importa:** só afeta perfis legados pré-rename (edge). O usuário testa com perfil "Stealth" novo → não reproduz. Mas outro player com perfil antigo pode cair nisso.

**Sugestão:** aceitar como edge conhecido (perfis novos ok). Se aparecer, recriar o perfil ou mapear "Ghost"→"Stealth" no registro do server (item 054/057).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

## Resolução (2026-07-01)

DLL `CustomClasses-Client.dll` **100864 bytes** · compile 0/0.

| ID | Decisão | O que mudou |
| --- | --- | --- |
| CR-02-01 | ✅ Aplicado (A) | `Entry.Pending` + Combat Medic marcado; card renderiza acento **âmbar** + selo `· em breve/soon` (nem verde-ativo). [`PerksCatalog.cs`](../../modded/Client/PerksCatalog.cs) + [`SkillsClassTabPatch.cs` BuildCard](../../modded/Client/Patches/SkillsClassTabPatch.cs) |
| CR-02-02 | ✅ Aplicado (A) | Quick Hands marcado `Pending` (mesmo tratamento). |
| CR-02-03 | ✅ Aplicado | Iron Lungs: EN `longer breath hold & less arm fatigue` · PT `respiração longa e menos fadiga de braço` (removido "less sway"). |
| CR-02-04 | ✅ Aceito (manter simples) | Decisão do usuário: manter ADS ×0.85 flat. **Descoberta extra:** o "saque de pistola ×0.5" também **nunca foi implementado**. Cópia do catálogo corrigida p/ `faster aim (ADS) on all weapons` e `class-design.md:52` alinhado à impl (+ linha no Histórico). [`PerksCatalog.cs`](../../modded/Client/PerksCatalog.cs), [`class-design.md:52`](../../docs/class-design.md#L52) |
| CR-02-05 | ⏸️ Deferido | Coop-sync dos perks de som → **item 057** (já mapeado; memória `feedback_coop_multiplayer_sync`). |
| CR-02-06 | ⏸️ Aceito | Edge de perfil legado pré-rename; perfis novos ("Stealth") ok. |

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-01 | Code review 02 (requisitos de negócio) criada — cross-check UI × `class-design.md` × status implementado |
| 2026-07-01 | CR-02-01/02/03 aplicados (UI honesta "em breve" + cópia); 04/05/06 deferidos p/ decisão; DLL 100864 bytes, 0/0 |
