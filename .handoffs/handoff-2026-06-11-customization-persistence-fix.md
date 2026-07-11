# Handoff — `mods/CustomizationPersistenceFix`

> **Data:** 2026-06-11<br>
> **Origem:** sessão de polish do CustomClasses (i18n + identidade visual + bug 017)<br>
> **Para:** sessão dedicada a revisar/finalizar/publicar este mod<br>
> **Status:** ✅ implementado + compilado + instalado · ⏳ **não validado in-game** · ⛔ não commitado<br>

---

## ⚠️ ATUALIZAÇÃO 2026-06-12 (corrige este handoff)

A 1ª versão (descrita abaixo) **patcheou o método errado e era um no-op** — o teste in-game falhou. Outra sessão corrigiu:

- **Método correto = `ProfileFixerService.FixProfileBreakingInventoryItemIssues(PmcData)`** (NÃO `CheckForAndFixPmcProfileIssues`, que não toca customização). Chamado em `GameController.cs:101`, **somente quando** `core.json → fixes.fixProfileBreakingInventoryItemIssues == true`. **O install do usuário (`SPT_Data/configs/core.json:28`) tem `true`** → por isso o bug dispara.
- O patch agora alveja esse método; o **Postfix reescreve com a lógica correta** via `ResolvePiece`: peça válida → preserva; peça inválida/ausente → default da facção (cobre o **edge case** que estava pendente). Typo `DefaulUsecFeet` mantido verbatim (existe na DB do EFT).
- Arquivos `*.cs`, `README.md` e a memória `reference_spt_customization_reset_bug` já refletem isso. **Validação in-game segue pendente** (tratada em sessão dedicada).
- **Lição:** confirmar QUAL método contém o código bugado — não confiar no nome citado num handoff. As seções 2–3 abaixo descrevem a 1ª versão (método errado) e ficam só por histórico.

---

## 1. O que é e por que existe

Mod **server-side** (SPT 4.0, C#/.NET 9) que corrige um **bug do SPT core**: a **customização do PMC (roupas/skin — `Body`/`Hands`/`Feet`) não persiste entre sessões**. Ao recarregar o jogo, o personagem volta sempre ao **uniforme default da facção**, descartando qualquer skin equipada.

Nasceu durante o **item 017 do CustomClasses** (a skin da classe Peladão não aparecia/persistia). A investigação mostrou que **não é bug do CustomClasses** — é do SPT, e afeta **qualquer perfil/skin** (o usuário confirmou: já acontecia antes do mod). Por isso foi extraído para um **mod dedicado** (decisão do usuário), em vez de embutir no CustomClasses.

## 2. Causa-raiz (o bug do SPT)

`references/spt-source/Libraries/SPTarkov.Server.Core/Services/ProfileFixerService.cs` → `CheckForAndFixPmcProfileIssues(PmcData)` (chamado em `Controllers/GameController.cs:116`, no `/client/game/start`) tem a checagem de customização com a **lógica invertida**:

```csharp
// Head — CORRETO: reseta SÓ se a peça for inválida
if (!customizationDb.ContainsKey(Head)) { Head = DefaultUsecHead; }

// Body / Hands / Feet — BUG: falta o "!"  → reseta toda peça VÁLIDA p/ default
if (customizationDb.ContainsKey(Body))  { Body  = DefaultUsecBody; }
if (customizationDb.ContainsKey(Hands)) { Hands = DefaultUsecHands; }
if (customizationDb.ContainsKey(Feet))  { Feet  = DefaultUsecFeet; }   // + typo no SPT: busca "DefaulUsecFeet"
```

Resultado: a cada `game/start`, qualquer roupa **válida** vira default. (Há ainda um typo secundário no SPT: o nome do feet default USEC é procurado como `"DefaulUsecFeet"` — não afeta o nosso fix.)

**Evidência coletada (sessão 017):** o template em memória tinha a skin correta (havaiana, `Body=6847e338`, válida), mas o perfil salvo voltava a `5cde95d9` (DefaultUsecBody) após reabrir. `AddSuitsToProfile` só **desbloqueia** o suit (`CustomisationUnlocks`), não muda a aparência. Versão: **SPT 4.0.13** (`compatibleTarkovVersion 0.16.9`).

## 3. A solução (este mod)

Patch **Harmony Prefix/Postfix** em `CheckForAndFixPmcProfileIssues`:
- **Prefix** captura `Body`/`Hands`/`Feet` originais (antes do método bugado rodar).
- **Postfix** restaura cada peça **se ela ainda for válida** na DB de customização (desfaz só o reset indevido). Peças realmente inválidas seguem o que o método decidiu — **não pioramos** esse caso.

Harmony **2.15.0** já vem com o servidor (`D:/SPT/SPT/0Harmony.dll`); referenciado com `Private=false` (compila contra ele, usa o do servidor em runtime, não redistribui).

## 4. Arquivos

```
mods/CustomizationPersistenceFix/
├── .gitignore                         # References/ obj/ bin/
├── README.md                          # explicação do bug + fix (user-facing)
└── modded/Server/
    ├── CustomizationPersistenceFix.csproj      # net9.0; SPTarkov.* 4.0.0 + Reference 0Harmony (Private=false)
    ├── CustomizationPersistenceFixMetadata.cs  # AbstractModMetadata; GUID customizationpersistencefix.mdj; SPT ~4.0.0; MIT
    ├── CustomizationPersistenceFixMod.cs       # [Injectable(PostDBModLoader)] IOnLoad: guarda DatabaseService estático + Harmony.PatchAll
    ├── ProfileFixerCustomizationPatch.cs       # [HarmonyPatch(ProfileFixerService, CheckForAndFixPmcProfileIssues)] Prefix/Postfix
    └── References/0Harmony.dll                  # gitignored — copiado de D:/SPT/SPT/0Harmony.dll (2.15.0)
```

Pontos de design no código:
- `CustomizationPersistenceFixMod.Db` (static) é setado no `OnLoad` e usado pelo patch estático para validar peças (`Db.GetTemplates().Customization`).
- `_patched` guard evita re-patch.
- O `OnLoad` (PostDBModLoader) roda muito antes do primeiro `game/start`, então `Db` está pronto quando o Postfix executa.
- Snapshot via `struct` no `__state` do Harmony (Prefix→Postfix).

## 5. Build & install

```bash
bash .agents/scripts/compile-mod.sh CustomizationPersistenceFix
```
- Detectado como **server-csharp** (grep `SPTarkov.` no csproj).
- Instala **só** `CustomizationPersistenceFix.dll` (+ `.pdb`) em `D:/SPT/SPT/user/mods/CustomizationPersistenceFix/` — o `0Harmony` (Private=false) **não** é redistribuído (usa o do servidor).
- Último build: **0 warn / 0 err**, DLL ~11.8 KB. **Requer reiniciar o servidor.**

> ⚠️ Se clonar/mover o repo: `References/0Harmony.dll` é gitignored — recopiar de `D:/SPT/SPT/0Harmony.dll` antes de buildar, senão o csproj falha a referência.

## 6. Como validar in-game (PENDENTE)

1. Reiniciar **servidor + jogo** (com o mod instalado).
2. Equipar **qualquer** skin no jogo (ou usar um perfil de classe do CustomClasses com `outfit`).
3. **Fechar e reabrir** → a skin deve **persistir** (antes voltava ao default).
4. Contraprova: sem o mod (mover a pasta p/ fora de `user/mods`), a skin volta ao default.

Contexto do teste atual: o perfil **Peladon** (`6a28a1e3…`) teve a skin havaiana **gravada manualmente** (`Body=6847e338`, `Hands=6847e7ec`, `Feet=642d4d8e`; backup `.bak-skin-test`) — com este mod ativo, deve persistir.

## 7. Pendências / pontos para a próxima sessão

- 🔴 **Validar in-game** (seção 6) — nunca testado.
- 🟡 **Compatibilidade:** o usuário roda **FIKA + ~40 mods**, vários de customização (**AllTheClothes, WTT-HeadVoiceSelector, WTT-Artem, WTT-PackNStrap**). Confirmar que nenhum outro patcha `ProfileFixerService` (ordem de patch) e que o fix não conflita. Testar com AllTheClothes (skins "aparência direta") e WTT.
- 🟡 **Edge case — peça inválida:** hoje, se a peça original for inválida, deixamos como está (o método bugado também não a reseta). O comportamento "correto" seria resetar para o default da facção. Decidir se vale tratar (raro: só se a DB mudar / mod removido). Exigiria resolver os defaults por nome (e contornar o typo `DefaulUsecFeet`).
- 🟢 **Head:** intencionalmente **não** tocado (a lógica do Head no SPT já está correta). Confirmar que não há caso em que o Head também precise.
- 🟢 **Report upstream ao SPT:** é bug do core — abrir issue/PR (1-char fix: adicionar `!` em Body/Hands/Feet + corrigir o typo `DefaulUsecFeet`). Se o SPT corrigir numa versão futura, **este mod pode ser aposentado**.
- 🟢 **Metadados finais:** nome/GUID/versão/autor/licença/URL definitivos antes de publicar (hoje: GUID `customizationpersistencefix.mdj`, v1.0.0, autor `mdj`, MIT).
- ⛔ **Commit:** nada commitado (decisão de agrupamento do working tree é do usuário).

## 8. Refs

- **Bug:** `references/spt-source/.../Services/ProfileFixerService.cs` (~linhas 165–205, blocos Head/Body/Hands/Feet) · `Controllers/GameController.cs:116` (chamada no `game/start`).
- **Investigação (017):** [mods/CustomClasses/backlog/017-customizacao-nao-persiste/017-customizacao-nao-persiste-00-bug.md](../mods/CustomClasses/backlog/017-customizacao-nao-persiste/017-customizacao-nao-persiste-00-bug.md).
- **Memória global:** `reference_spt_customization_reset_bug` (o bug) · `reference_spt_customization_model` (como a aparência é setada por template) · `project_customclasses_session_split` (coordenação multi-sessão).
- **Modelo de customização:** `Customization.Body/Hands/Feet` = `MongoId?`; aplicação por template em `CreateProfileService.cs:44-61/134` (só Head/Voice vêm do request; Body/Feet/Hands do template; suits via `AddSuitsToProfile`).

## 9. Resumo de uma linha

> Mod Harmony server que **impede o `ProfileFixerService` do SPT 4.0.13 de resetar roupas válidas (Body/Hands/Feet) para o default a cada login** — corrige a não-persistência de skins; candidato a report upstream e a aposentadoria quando o SPT corrigir.
