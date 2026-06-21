# Resumo das classes — visão de uma página

> **Data:** 2026-06-20<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [class-levers.md](./class-levers.md)<br>

---

Visão **rápida e completa** das 6 classes: o que cada uma é, sua habilidade-assinatura e tudo que será implementado por camada. Para os números e a engenharia, ver [class-levers.md](./class-levers.md) (matriz, decisões, patches) e [`class-matrix.mjs`](../scripts/class-matrix.mjs) (reproduzível).

**Como ler os cards:** `×` = multiplicador de XP da skill (`>1` sobe mais rápido 🟢, `<1` atrofia 🔴) · `Lv` = nível inicial · ⚠️ = depende de coordenar com o stances mod.
**Camadas:** 🎯 skill · 🧪 skill custom · 🔧 patch · 🎒 loadout (gear) · 🏠 hideout (inicial + −50%) · 🌐 global.
**Configurável:** todo lever 🔧/🧪 será exposto no **F12** (ajuste em runtime/restart); a matriz 🎯 (skills/mults) no editor web (perfil novo/restart). Tabela de parâmetros em [class-levers.md §6.4](./class-levers.md).

---

## Comparação rápida

| Classe | Pilar | ⭐ Signature | Faz melhor | Balance |
|---|---|---|---|---|
| 🩺 **Médico** | Suporte | 🔧 cura quase instantânea, sobrevive a tudo | cura/cirurgia · vitalidade | **topo** · net +6.12 |
| 🔫 **Fuzileiro** | Combate | 🧪 Adrenalina pós-abate | rifle · recarga · recuo | **topo** · net +6.27 |
| 🎯 **Caçador** | Precisão | 🧪 Fôlego de Aço (respiração ×3) | sniper/DMR · furtividade | **topo** · net +6.21 |
| 👻 **Fantasma** | Furtividade | 🔧 Execução (melee ×20) | silêncio · pistola · melee | **topo** · net +6.12 |
| 🎒 **Saqueador** | Pilhagem | 🧪 Mãos Rápidas + Pack Mule | lockpick · loot · carga | **base** · net +4.09 |
| 🛡️ **Tanque** | Resistência | 🔧 Couraça + Pack Mule | armadura · HP · lança-granadas | **base** · net +4.28 |

> **Topo ~+6 · base ~+4** — Saqueador e Tanque têm netMult menor de propósito; são compensados pelas signatures 🔧/🧪 (que ficam fora do netMult).

---

## 🩺 Médico — *Medic* · Suporte

> ⭐ **Médico de Combate** 🔧 — cura quase instantânea (tempo **×0.3**), **+50% HP**, cura **andando e atirando** (sem lock de movimento/arma).

- **🟢 Sobe rápido** — **FirstAid** ×2.5 `Lv5` · **FieldMedicine** ×2 `Lv5` · **Surgery** ×2 `Lv4` · **Vitality** ×2 `Lv4` · HideoutManagement ×1.5 `Lv6` · Crafting ×1.5 · Immunity ×1.2 `Lv1`
- **🔴 Atrofia** — Assault ×0.6 · AimDrills ×0.7 · CovertMovement ×0.7 · Perception ×0.8
- **🔧 Extra** — **cirurgia/restauração de membro destruído** (CMS/Surv12) em **×0.5** do tempo *(a costura lenta de membro blackado — distinta da cura de HP da signature)*
- **🏠 Hideout** MedStation (inicial + −50%)

## 🔫 Fuzileiro — *Rifleman* · Combate

> ⭐ **Adrenalina** 🧪 — após um abate: **−recuo / −recarga / −ADS** por `3s + 0.5s/nível`.

- **🟢 Sobe rápido** — **Assault** ×2.5 `Lv7` · **UsecArsystems** (NATO) ×2 `Lv3` · **BearAksystems** (Leste) ×2 `Lv3` · AimDrills ×1.5 `Lv5` · MagDrills ×1.5 `Lv4` · Endurance ×1.5 `Lv5` · StressResistance ×1.3 · Pistol ×1.2
- **🔴 Atrofia** — CovertMovement ×0.6 · Attention ×0.7 · Search ×0.8
- **🔧 Extra** — resistência a supressão (aim-punch **×0.5**) · antitravamento (malfunction **×0.5**, conserto **×2**)
- **🏠 Hideout** Workbench (inicial + −50%)

## 🎯 Caçador — *Hunter* · Precisão

> ⭐ **Fôlego de Aço** 🧪 — prende a respiração `×(1+0.1·nível) ≤ ×3`, com **−sway**.

- **🟢 Sobe rápido** — **Sniper** ×2.5 `Lv7` · **DMR** ×1.5 `Lv2` · AimDrills ×1.5 · ProneMovement ×1.5 `Lv3` · Pistol ×1.3 `Lv2` · Perception ×1.3 `Lv2` · Metabolism ×1.3 · CovertMovement ×1.2 `Lv3`
- **🔴 Atrofia** — Assault ×0.6
- **🔧 Extra** — saque de pistola **×0.5** · ADS por arma (sniper/DMR ×0.85, AR ×1.15) · ⚠️ resistência de braço em ADS
- **🏠 Hideout** Shooting Range (inicial) + Intelligence Center (−50%)

## 👻 Fantasma — *Ghost* · Furtividade

> ⭐ **Execução** 🔧 — dano de **melee ×20**.

- **🟢 Sobe rápido** — **SilentOps** ×2.5 `Lv6` · **Pistol** ×1.8 `Lv2` *(silenciada)* · CovertMovement ×1.5 `Lv6` · Perception ×1.5 `Lv5` · Melee ×1.5 `Lv3` · LightVests ×1.3 · ProneMovement ×1.5 · Lockpicking ×1.3 `Lv3`
- **🔴 Atrofia** — Assault ×0.6 · StressResistance ×0.7 · Shotgun ×0.7
- **🔧 Extra** — **Passo Fantasma** (ruído de todas as ações até **−50%**, não silêncio total). *(Execução agora dá +velocidade c/ a melee na mão; o MaxSpeed ×1.1 fixo foi removido.)*
- **🏠 Hideout** Lavatory (inicial + −50%)

## 🎒 Saqueador — *Looter* · Pilhagem

> ⭐ **Mãos Rápidas** 🧪 (busca/loot mais rápido) + **Pack Mule** 🧪 (peso `×(1−[0.10→0.50])`).

- **🟢 Sobe rápido** — **Lockpicking** ×3 `Lv8` · **Strength** ×3 `Lv7` · **ShadowConnections** ×2.5 `Lv6` · Attention ×1.3 `Lv8` · Perception ×1.3 `Lv5` · Search ×1.3 `Lv6` · HideoutManagement ×1.2 · Intellect ×1.2 · Charisma ×1.2
- **🔴 Atrofia** — Assault ×0.6 · AimDrills ×0.7 · StressResistance ×0.7
- **🔧 Extra** — loot silencioso · **🌐 revelar valor ₽** (global — todos veem)
- **🎒 Loadout** contêiner seguro de 6 slots  ·  **🏠 Hideout** Scav Case (inicial + −50%)

## 🛡️ Tanque — *Tank* · Resistência

> ⭐ **Couraça** 🔧 (dano recebido `×(1−[0.05→0.25])`) + **Pack Mule** 🧪 (compartilhada com o Saqueador).

- **🟢 Sobe rápido** — **StressResistance** ×2 · **HeavyVests** ×1.5 `Lv3` · Health ×1.5 `Lv4` · Vitality ×1.5 `Lv4` · Strength ×1.5 `Lv5` · Shotgun ×1.5 `Lv1` · Throwing ×1.5 `Lv1` · Melee ×1.5
- **🔴 Atrofia** — Pistol ×0.7 · DMR ×0.7 · AimDrills ×0.7 · CovertMovement ×0.5 · Metabolism ×0.5
- **🔧 Extra** — maestria de lança-granadas (sem penalidade de ergo) · ⚠️ stamina segurando arma pesada **×0** · velocidade **×0.9** · −fome/sede **×0.7**
- **🎒 Loadout** placas laterais  ·  **🏠 Hideout** Rest Station (inicial) + Kitchen (−50%)

---

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-20 | Guilherme | Criação. One-pager de resumo das 6 classes (tabela de comparação + card por classe com todas as camadas), derivado de [class-levers.md](./class-levers.md). |
| 2026-06-20 | Guilherme | Sincronizado com a recalibração da Fase 4 (gems por categoria, SMG/AttachedLauncher inertes removidos, custos aparados). Nets: topo +6.12–6.27 · base +4.09–4.28. |
| 2026-06-20 | Guilherme | Revisão pós-Fase 4: glossário 🎒/🏠 (gear vs hideout), nota de configurabilidade (F12 + editor web), Médico esclarecido (cirurgia/restauração de membro destruído ×0.5). |
