# 058 — Ativar masteries inertes · Recon técnico ("globals é consumido?")

> **Data:** 2026-07-02<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [058-ativar-masteries-inertes-00-kickoff.md](./058-ativar-masteries-inertes-00-kickoff.md)<br>

---

## 1. Veredito (topo)

**globals `SkillsSettings` é consumido para conceder XP dessas 5 skills? → NÃO.**
**Aplica efeito (recuo/ergo) dessas 5 skills? → NÃO.**

Repopular o `[]` do `globals.config.SkillsSettings` para SMG/LMG/HMG/Launcher/AttachedLauncher **NÃO PEGA**. O bloqueio é **client-side, estrutural** — não é "config vazia": o modelo C# `GlobalSkillsSettings` (`BackendConfigSettingsClass.cs:2162`) **só tem propriedades tipadas para as 6 categorias funcionais** (Pistol, Assault, Shotgun, Sniper, DMR, Revolver). As 5 inertes **não têm slot** onde bindar, e o roteador arma→skill de XP (`SkillManager.method_1`, `SkillManager.cs:2229`) **não inclui HMG/Launcher/AttachedLauncher**. Ativar de verdade exige **patch client** nas duas pernas.

> ⚠️ Nomenclatura importante — há **dois** sistemas distintos no engine, não confundir:
> - **Weapon Skills** (`WeaponSkillClass`: SMG, Assault, LMG, HMG, Launcher, AttachedLauncher, Pistol…) → aparecem na tela de Skills, config em `globals.SkillsSettings`. **É destas que o item 058 trata** (as 5 inertes).
> - **Mastering** (`MasterSkillClass`, por arma/preset) → sistema separado, config em `globals.config.Mastering` + `globals.config.Associations`, **funciona** (sobe ao acertar tiro, via `WeaponShotAction`). Não é o alvo aqui.

**Abordagem recomendada:** patch client nas duas pernas, reusando integralmente os pontos já mapeados no 050. Server-globals é descartado (não há binding).

---

## 2. Evidência — Perna 1 (ganho de XP)

### 2.1 Como uma weapon skill FUNCIONAL ganha XP

Cadeia (arma acerta alvo → XP na skill certa):

1. **Trigger do XP = acertar dano, não disparar.** `EFT/Player.cs:29992` — dentro de `ManageAggressor` (handler de dano no atirador), `player.ExecuteShotSkill(damageInfo.Weapon)`. É o **único** caller de `ExecuteShotSkill` (grep confirmou; o outro é o override em `HideoutPlayer.cs:653`). Ou seja: o XP de maestria de arma sobe ao **causar dano**, não ao atirar no ar.
2. **`ExecuteShotSkill`** — `EFT/Player.cs:29934`: `Skills.WeaponShotAction.Complete(weapon, val)`. Dispara o action global `WeaponShotAction` (um `GClass2260<Item>`, `SkillManager.cs:1467`) passando a arma como filtro.
3. **Roteamento arma→skill** — cada `WeaponSkillClass` se inscreve em `WeaponShotAction` via um `.Where(item is <Type>)` no seu construtor; `GClass2260<T>.Where` (`SkillManager.cs:544`) só repassa o XP se o predicado casar. Assim, ao acertar com uma AR, só a skill Assault ganha XP.
4. **XP creditado** — `SkillClass.OnTrigger` (`SkillClass.cs:228`) → `AbstractSkillClass.OnTrigger` (`AbstractSkillClass.cs:100`): `SetCurrent(Current + val)`.
5. **De onde vem a config** — o `FactorValue` de cada action vem de `SkillsSettings.WeaponSkillProgressRate` (`SkillManager.cs:466`, ramo `nonWeaponSkill:false`) e o array por-categoria de `Settings.<Cat>` via `GClass1796.ArrayValues` (`SkillManager.cs:2023-2028`). `Settings` = `SkillsSettings` (`SkillManager.cs:1841`).

### 2.2 Por que as 5 inertes NÃO ganham XP (evidência estrutural, 3 camadas)

**Camada A — o modelo C# não tem slot p/ elas.** `GlobalSkillsSettings` (`BackendConfigSettingsClass.cs:2162-2235`) declara propriedades de weapon-skill **apenas** para: `Pistol` (2196), `Assault` (2198), `Shotgun` (2200), `Sniper` (2202), `DMR` (2204), `Revolver` (2230). **Não existe** `SMG`, `LMG`, `HMG`, `Launcher` nem `AttachedLauncher`. Se o server escrever `SkillsSettings.SMG = {...}` no globals.json, o desserializador do cliente **não tem onde colocar** — o valor é ignorado. (Confirma o `[]` do globals: `globals.json` SMG:35559, LMG:35482, HMG:35376, Launcher:35483, AttachedLauncher:35261 — todos `[]`.)

**Camada B — `ArrayValues` só existe p/ as 6.** `GClass1796.cs:1-32` define o extension `ArrayValues` **só** para os 6 tipos funcionais (GClass1767 Revolver, 1768 Pistol, 1769 Assault, 1770 Shotgun, 1771 Sniper, 1772 DMR). Não há overload para as inertes. Por isso `SkillManager.cs:2032/2036/2037` reusa `array[2]` (=Assault) para SMG/LMG/HMG e `array[0]` (=Pistol) para Launcher/AttachedLauncher — **fallback hard-coded no cliente**, não leitura da config própria delas.

**Camada C — HMG/Launcher/AttachedLauncher nem entram no roteador.** `SkillManager.method_1()` (`SkillManager.cs:2229-2265`) monta o dicionário `WeaponSkills` (`Type → WeaponSkillClass`) que é a tabela consultada por `Item.GetType()`. O `switch` tem `case` só para **Pistol, Revolver, SMG, Assault, Shotgun, Sniper, LMG, DMR** — **não há case para HMG, Launcher, AttachedLauncher** (nem Melee/Misc). Logo, essas 3 **nunca** são associadas a nenhum tipo de arma → nunca casam no lookup → nunca ganham XP nem buff.

**Sinal adicional (SMG/LMG):** mesmo tendo `case`, os tipos registrados são `SmgItemClass`/`MachineGunItemClass` e o construtor original usava `typeof(int)` p/ HMG e `typeof(float)` p/ AttachedLauncher (`SkillManager.cs:2037/2039`) — tipos-sentinela que jamais casam com um `Item`. O lookup é `WeaponSkills.TryGetValue(Item.GetType(), ...)` (`Player.cs:12606/12640/10089`), **match exato de Type**. Se as armas SMG/LMG reais não forem exatamente `SmgItemClass`/`MachineGunItemClass` (e o `[]` no globals é consistente com a intenção BSG de desligá-las), o XP não credita mesmo com case presente. → **SMG/LMG são ambíguas por match-de-tipo; HMG/Launcher/AttachedLauncher são inequivocamente mortas.** A spec-tech deve confirmar in-game se SMG/LMG sobem ou não (ver §5).

**Conclusão Perna 1:** repopular globals **não concede XP** para nenhuma das 5 (nem há binding para elas). XP real exige **patch client**.

---

## 3. Evidência — Perna 2 (efeito por nível: recuo/ergo)

### 3.1 Onde o buff é aplicado e de onde lê

- O efeito é montado em `SkillManager.GetWeaponInfo(Item weapon)` (`SkillManager.cs:1869`): lê `WeaponBuffs[weapon.GetType()]` (`SkillManager.cs:1886`) → devolve um `GClass2250` com `RecoilSupression`, `DeltaErgonomics`, `ReloadSpeed`, etc.
- Consumido no `FirearmController`: `Player.cs:12403` (`controller.gclass2250_0 = player.Skills.GetWeaponInfo(controller.Item)`) e `12670`.
- **Curva é client-side.** `GetWeaponBuffs` (`SkillManager.cs:1924-2008`) constrói cada buff com `.PerLevel(...)`: recuo = `.PerLevel(Settings.WeaponSkillRecoilBonusPerLevel)` (1937), ergo = `.PerLevel(0.002f)` **constante hard-coded** (1941), reload = `.PerLevel(0.004f)` (1929). Só o coeficiente de recuo (e mounting/bipod ergo) vem de `Settings`; o resto é constante no cliente. `Settings.WeaponSkillRecoilBonusPerLevel` tem default `0.004f` no próprio C# (`BackendConfigSettingsClass.cs:2168`) — **é global, não por-categoria**.

### 3.2 Por que as 5 inertes não recebem buff

`WeaponBuffs` só é populado (`WeaponBuffs.Add(ofType, ...)`, `SkillManager.cs:1972`) para os tipos que passam por `GetWeaponBuffs(ofType)` — chamado dentro do construtor de cada `WeaponSkillClass` registrada. Como HMG/Launcher/AttachedLauncher não têm mapeamento válido de tipo (§2.2-C) e o lookup do efeito também é por `weapon.GetType()`, o buff dessas nunca é encontrado → cai no ramo `else` `"has no buffs"` (`SkillManager.cs:1900`). Mesmo raciocínio da Perna 1.

**Conclusão Perna 2:** globals **não aplica** o buff dessas 5. O efeito é calculado por curva no cliente e chaveado por tipo de arma que essas skills não possuem. Efeito real exige **patch client**.

---

## 4. Abordagem recomendada (por perna) — patch client, reusando o 050

Server-globals descartado nas duas pernas (sem binding). Ambas são **locais** → seguras em coop (memória `feedback_coop_multiplayer_sync`; recuo/ergo já são locais no 050).

### Perna 1 — conceder XP (client)
- **Ponto de patch:** Postfix/Prefix em `Player.ExecuteShotSkill(Item weapon)` (`EFT/Player.cs:29934`) — é o funil único do XP de weapon-skill e já roda no contexto "meu tiro acertou". Gate: `MainPlayer` local + `weapon.WeapClass` ∈ {`smg`, `machinegun`, `grenadeLauncher`, underbarrel}.
  - Alternativa se `ExecuteShotSkill` for problemático: patch em `ManageAggressor` (`Player.cs:29948`) no ponto do caller (29992), mesmo gate.
- **API para creditar XP:** a skill-alvo é uma `WeaponSkillClass`/`SkillClass` acessível via `player.Skills.<SMG|LMG|HMG|Launcher|AttachedLauncher>` (campos públicos, `SkillManager.cs:1306-1320`). Incrementar via `SetCurrent(Current + delta)` (herdado de `AbstractSkillClass`, `AbstractSkillClass.cs:115`) ou `ChangeMasteringLevel`-equivalente. **Nuance:** as 5 nascem `Locked = buffs.Length == 0` só se sem buffs (`SkillClass.cs:80`) — no vanilla elas SÃO criadas com o array de buffs de fallback, então não estão locked por buffs; o que falta é o **roteamento**, não a existência da skill. Confirmar in-game se `player.Skills.HMG` existe e aceita `SetCurrent`.
- **Mapeamento categoria→skill** (reusar `HeavyWeapon` do 050): `machinegun`→ (LMG vs HMG precisa desambiguar; o client une ambos em `weapClass="machinegun"` — ver `ClassWeaponPatches.cs:211-214`), `grenadeLauncher`→Launcher, `smg`→SMG, underbarrel→AttachedLauncher (o 050 nota que underbarrel acoplado **não expõe flag simples** no client — `ClassWeaponPatches.cs:212`; incógnita).

### Perna 2 — aplicar efeito (client) — **já existe no 050**
- **Recuo:** Prefix em `ProceduralWeaponAnimation.Shoot(ref float str)` — `ClassWeaponPatches.cs:18-68` (`ShootRecoilPatch`). Multiplicar `str` por `(1 − recoilPerLevel × skillLevel)`.
- **Ergo:** Postfix no getter `FirearmController.TotalErgonomics` — `ClassWeaponPatches.cs:167-199` (`HeavyWeaponErgoPatch`). Multiplicar `__result` por `(1 + ergoPerLevel × skillLevel)`.
- **Nível da skill:** `player.Skills.<Cat>.Level` (`AbstractSkillClass.Level`, `AbstractSkillClass.cs:58`).
- **Detecção de categoria:** reusar `HeavyWeapon.IsHeavy` / `w.WeapClass` (`ClassWeaponPatches.cs:201-221`); estender p/ `smg`.

Assim, o 058 é essencialmente **Perna 1 nova (XP) + Perna 2 = generalizar os patches do 050** de "flat por classe Tank" para "escalar por nível de skill".

---

## 5. Riscos / incógnitas (só validação in-game confirma)

1. **SMG/LMG — sobem no vanilla ou não?** Têm `case` no `method_1()` (por `SmgItemClass`/`MachineGunItemClass`), mas o `[]` no globals e o match-exato-de-tipo deixam ambíguo se instâncias reais casam. **Ambíguo — a spec-tech precisa confirmar in-game** (equipar SMG, atirar/acertar, ver se a barra sobe). Se subirem sozinhas, o 058 só precisa tratar HMG/Launcher/AttachedLauncher na Perna 1.
2. **Underbarrel (AttachedLauncher):** o client não expõe flag simples de "underbarrel acoplado disparando" (`ClassWeaponPatches.cs:212`). Detectar o modo de disparo do GP-25/M203 é a maior incógnita de gating — pode exigir inspecionar o fire mode/estado do launcher acoplado. Provável ponto mais difícil do item.
3. **A skill existe no `SkillManager` mas está na UI como "cinza"?** Confirmar que `SetCurrent` numa dessas propaga para a UI e **persiste no perfil** (server salva o progresso da skill no profile). Se o server não persistir skills que ele considera inertes, o XP pode zerar entre raids — validar.
4. **HMG vs LMG:** ambas são `weapClass="machinegun"` no client (050) — para skills separadas, desambiguar HMG de LMG exige outro discriminante (peso? templateId? categoria do handbook). Pode forçar unificar num só buff ou usar dado extra.
5. **Match de tipo p/ efeito:** como o buff é chaveado por `weapon.GetType()` e não por `weapClass`, o patch de efeito (Perna 2) deve gatear por `WeapClass` (como o 050 já faz), **não** tentar reusar `WeaponBuffs` do engine (que estará vazio p/ essas armas).

### Relação com o perk Bunker (050) — opções (não decidir aqui)
O Bunker (classe **Tank**) já dá flat recuo ×0.85 / ergo ×1.15 com arma pesada (`ClassWeaponPatches.cs:51-56, 189-192`), e o próprio perk usa `ESkillId.LMG` como ícone (`PerksCatalog.cs:149`). Opções a levantar para a spec:
- **(a) Coexistir:** skill (escala por nível, todas as classes) **+** Bunker flat extra só p/ Tank → multiplicadores compostos (risco de empilhar forte).
- **(b) Substituir:** skill assume o papel; Bunker é aposentado/re-escopado.
- **(c) Elite bonus:** Bunker vira o bônus de nível 51 (elite) da skill p/ a Tank.
Decisão fica para a spec funcional.

---

## Síntese (resumo)

O engine EFT 0.16.x **NÃO consome** `globals.SkillsSettings` para XP nem para efeito das 5 skills inertes (SMG/LMG/HMG/Launcher/AttachedLauncher) — e não é "config vazia" reversível: o modelo C# `GlobalSkillsSettings` (`BackendConfigSettingsClass.cs:2162`) só tem slots para as 6 categorias funcionais, e o roteador `SkillManager.method_1()` (`SkillManager.cs:2229`) nem registra HMG/Launcher/AttachedLauncher. Repopular o globals **não pega**. Ambas as pernas exigem **patch client**: (1) XP via Postfix em `Player.ExecuteShotSkill` (`Player.cs:29934`), gateando por `weapClass` e creditando `player.Skills.<Cat>.SetCurrent`; (2) efeito **reusando os patches do 050** — recuo em `ProceduralWeaponAnimation.Shoot` e ergo em `FirearmController.TotalErgonomics` — trocando o flat da Tank por escala ×nível-de-skill. Incógnitas para validação in-game: se SMG/LMG já sobem sozinhas (têm `case`, mas `[]`+match-de-tipo deixam ambíguo), como detectar underbarrel acoplado, se o server persiste o progresso dessas skills, e como separar HMG de LMG (ambas `weapClass=machinegun`).
