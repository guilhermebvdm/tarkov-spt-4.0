# 006 — Compat opcional com Skills-Extended · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [006-skills-extended-compat-01-spec.md](006-skills-extended-compat-01-spec.md)
**Criado:** 2026-06-07

> Mod **server-side** (o lado client do 005/010 já é genérico por `ESkillId` e não precisa mudar). Refs do EFT não se aplicam; as refs aqui são do **servidor SPT** (`references/spt-source/`) e do mod **Skills-Extended** (`mods/Skills-Extended/`).

## 1. Estratégia

A base já funciona: as skills do SE (`FirstAid`, `FieldMedicine`, `BearRawpower`, `UsecNegotiations`) são membros de `SkillTypes`/`ESkillId` vanilla, então o loader (005) já as registra e o client (010) já as exibe/escala. Falta só:

1. **Detecção soft do SE** no server — injetar `IReadOnlyList<SptMod> loadedMods` e checar `m.ModMetadata.ModGuid == "com.cj.SkillsExtended"`. É o **mesmo padrão** que o próprio SE usa pra detectar o Fika (`ConfigController.cs:29`). Sem `BepInDependency`/referência hard.
2. **Aviso** no `RegisterClass`: quando uma classe define multiplicador para uma skill do conjunto-SE **e** o SE não foi detectado → `logger.Warning(...)` (registra mesmo assim — inócuo; a skill morta nunca dispara `OnTrigger`).
3. **Exemplo testável** — `Médico de Combate` ganha `FirstAid: 1.5` e `FieldMedicine: 1.5` no gerador.

**Client:** nenhuma mudança. `OnTriggerPatch` escala qualquer `ESkillId` que dispare ganho de XP; sem o SE, `FirstAid`/`FieldMedicine` nunca disparam (skill inativa) → no-op natural. A UI (010) só desenha quando a skill aparece na tela (o jogo só a mostra com o SE).

**Alternativa descartada:** detectar o SE no client (Chainloader) para condicionar a UI — desnecessário, pois sem o SE a skill nem aparece na tela.

## 2. Pontos de referência (server SPT + mod SE)

| Símbolo | Arquivo | Uso |
|---|---|---|
| `IReadOnlyList<SptMod> loadedMods` (injeção) | [LauncherController.cs:19](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/LauncherController.cs#L19) | lista de mods carregados (injetável) |
| `AbstractModMetadata.ModGuid` / `.Name` | [AbstractModMetadata.cs:21](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Spt/Mod/AbstractModMetadata.cs#L21) | identificar mod por GUID |
| `loadedMods.Any(m => m.ModMetadata.ModGuid == "Fika")` (padrão) | `mods/Skills-Extended/modded/Server/Core/ConfigController.cs:29` | padrão de soft-detect copiado |
| `SeModMetadata.ModGuid = "com.cj.SkillsExtended"` | `mods/Skills-Extended/modded/Server/Metadata.cs:16` | GUID exato do SE a procurar |
| `SkillTypes.FirstAid/FieldMedicine/BearRawpower/UsecNegotiations` | [SkillTypes.cs:42](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Enums/SkillTypes.cs#L42) | conjunto de skills "do SE" |
| `CustomClassesMod.RegisterClass` loop de `SkillMultipliers` | [CustomClassesMod.cs:167](../../modded/Server/CustomClassesMod.cs#L167) | onde entra o aviso |

## 3. Novas propriedades F12

Nenhuma (server-side).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/SkillsExtendedCompat.cs` | CRIAR | helper estático: GUID, conjunto de skills do SE, `IsPresent(loadedMods)`. |
| `modded/Server/CustomClassesMod.cs` | MODIFICAR | injeta `loadedMods`; computa `_seInstalled` (log 1x); aviso no loop de `skillMultipliers`. |
| `scripts/build-class-jsons.js` | MODIFICAR | `medicoDeCombate` += `FirstAid: 1.5, FieldMedicine: 1.5`. |
| `modded/Server/config/classes/medicoDeCombate.jsonc` | REGENERAR | via gerador. |
| `modded/Server/config/classes/_docs/` (ou README) | DOC | listar as 4 skills do SE suportadas. |

## 5. Stubs de código

### SkillsExtendedCompat.cs

```csharp
using SPTarkov.Server.Core.Models.Spt.Mod;   // SptMod

namespace CustomClasses;

/// <summary>
///     Item 006: soft-detect do Skills-Extended (sem dependência hard) + conjunto de skills que ele "revive".
///     Essas skills são membros de SkillTypes/ESkillId vanilla, mas só ganham XP com o SE instalado.
/// </summary>
public static class SkillsExtendedCompat
{
    public const string ModGuid = "com.cj.SkillsExtended";   // ref: SE Metadata.cs:16

    /// <summary>Skills revividas pelo SE (nomes de SkillTypes — ref: SkillTypes.cs:42,43,66,70).</summary>
    public static readonly HashSet<string> Skills =
        new(StringComparer.OrdinalIgnoreCase) { "FirstAid", "FieldMedicine", "BearRawpower", "UsecNegotiations" };

    /// <summary>True se o SE está nos mods carregados. Padrão idêntico ao SE p/ Fika (ConfigController.cs:29).</summary>
    public static bool IsPresent(IReadOnlyList<SptMod> loadedMods) =>
        loadedMods.Any(m => string.Equals(m.ModMetadata?.ModGuid, ModGuid, StringComparison.Ordinal));
}
```

### CustomClassesMod.cs (trechos)

```csharp
// ctor: + IReadOnlyList<SptMod> loadedMods   (using SPTarkov.Server.Core.Models.Spt.Mod;)

private bool _seInstalled;

// no início de OnLoad():
_seInstalled = SkillsExtendedCompat.IsPresent(loadedMods);
logger.Info($"[CustomClasses] Skills-Extended detectado: {(_seInstalled ? "sim" : "não")}.");

// dentro do loop de def.SkillMultipliers, após validar a skill (st):
if (!_seInstalled && SkillsExtendedCompat.Skills.Contains(st.ToString()))
{
    logger.Warning(
        $"[CustomClasses] '{name}': skill '{st}' depende do Skills-Extended (não detectado) — " +
        $"multiplicador registrado, mas sem efeito até instalar o SE.");
}
clean[st.ToString()] = factor < 0 ? 0 : factor;   // registra mesmo assim (inócuo sem o SE)
```

### build-class-jsons.js (exemplo)

```js
medicoDeCombate: { Surgery: 2.0, Vitality: 1.5, Health: 1.5, Immunity: 1.5, StressResistance: 1.3,
                   RecoilControl: 0.7, FirstAid: 1.5, FieldMedicine: 1.5 },   // FirstAid/FieldMedicine = Skills-Extended
```

## 6. Fluxo de dados

```
[load] CustomClassesMod.OnLoad
   ├─ _seInstalled = SkillsExtendedCompat.IsPresent(loadedMods)   (ModGuid "com.cj.SkillsExtended")
   └─ RegisterClass(def) → loop skillMultipliers
        ├─ skill ∈ {FirstAid,FieldMedicine,BearRawpower,UsecNegotiations} && !_seInstalled → logger.Warning
        └─ registry.Set(name, clean)   (registra sempre; inócuo sem o SE)
[runtime] rota /customclasses/skill-multipliers → client (005/010) escala/exibe se a skill estiver ativa (= SE presente)
```

## 7. Riscos e dependências

- **Detecção assimétrica:** o aviso é server-side e checa o **mod server** do SE (que existe — tem `Metadata.cs`). Se alguém instalar só o client do SE, o server avisaria falso-positivo — improvável (SE é bundle client+server) e é só um aviso, não bloqueia.
- **Hard-code do conjunto de skills:** as 4 skills do SE são fixas no código. Se o SE adicionar skills novas (fora de `ESkillId`), ficam fora do alcance — limite declarado na spec funcional.
- **Não bloquear o carregamento:** o aviso **nunca** impede registrar a classe (a skill morta é inofensiva). Mantém o comportamento do 005.
- **`loadedMods` injeção:** confirmado injetável (LauncherController/ConfigController do SE usam). Adiciona 1 dependência ao ctor do `CustomClassesMod`.
- **Exemplo no Médico:** sem o SE, `FirstAid`/`FieldMedicine` não aparecem na tela de Skills (skill inativa) → o usuário só vê o aviso no log; com o SE, aparecem com o buff (UI do 010).

## 8. Checklist de implementação

- [ ] `SkillsExtendedCompat.cs` (GUID + conjunto + `IsPresent`).
- [ ] `CustomClassesMod`: injeta `loadedMods`; `_seInstalled` + log 1x; aviso no loop de `skillMultipliers`.
- [ ] Gerador: `medicoDeCombate` += `FirstAid`/`FieldMedicine`; regenerar `.jsonc`.
- [ ] Doc das 4 skills do SE suportadas (`_docs`/README).
- [ ] `/compile-mod` server 0 warn/err; config copiada.
- [ ] Playtest: (a) sem SE → server loga aviso p/ FirstAid/FieldMedicine, sem crash; (b) com SE → Médico mostra FirstAid/FieldMedicine com buff e ganha XP escalado.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Spec técnica criada via `/create-technical-spec` (soft-detect via loadedMods/ModGuid; client sem mudanças; refs SE+spt-source verificadas) |
