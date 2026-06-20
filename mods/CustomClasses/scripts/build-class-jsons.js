#!/usr/bin/env node
/**
 * build-class-jsons.js — Fatia 5 do item 003.
 *
 * Gera as 10 classes reais (portadas do RZCustomProfiles) no formato do CustomClasses
 * (modded/Server/config/classes/<fileName>.jsonc), MAS com itens equipados/compostos
 * (o RZ só fazia "tudo no stash").
 *
 * Mapeamento de placement (auto via categoria do trl-items-management):
 *   - 1ª arma não-pistola  → FirstPrimaryWeapon  (preset + loadedMag + chambered + ammo)
 *   - 1ª pistola           → Holster             (preset + loadedMag + ammo)
 *   - 1ª Body armor        → ArmorVest
 *   - 1º Headgear          → Headwear
 *   - 1º Tactical/Chest rig→ TacticalVest
 *   - 1ª Backpack          → Backpack
 *   - todo o resto         → stash (o GridPacker do mod posiciona + faz stack-split)
 *
 * Ammo: dentro de `primary`, ammo[0] = arma primária, ammo[1] = pistola (ordem das recipes do RZ).
 *
 * Fontes: scripts/anchor-items.json (id→bsgId, cópia própria) e tools/trl-items-management/data/items.json (categoria).
 */
'use strict';
const fs = require('fs');
const path = require('path');

// item 019 (config guard rails): this generator is frozen as BOOTSTRAP-only.
// Existing .jsonc files that diverge from what would be generated are NOT
// overwritten (they may carry editor/manual edits); pass --force to overwrite.
const FORCE = process.argv.includes('--force');

const REPO = path.resolve(__dirname, '../../..');
// item 007: anchor próprio (cópia local) — sem dependência de mods/RZCustomProfiles (aposentado).
const ANCHOR = JSON.parse(fs.readFileSync(path.join(__dirname, 'anchor-items.json'), 'utf8'));
const DB = JSON.parse(fs.readFileSync(path.join(REPO, 'tools/trl-items-management/data/items.json'), 'utf8'));
const OUT_DIR = path.join(__dirname, '../modded/Server/config/classes');

const bsg = id => { const a = ANCHOR[id]; if (!a || !a.bsgId) throw new Error(`anchor sem bsgId: ${id}`); return a.bsgId; };
const catPath = id => { const d = DB[bsg(id)]; return d && d.category && d.category.path ? d.category.path : []; };

// categoria → "slot" lógico
function slotOf(id) {
  const p = catPath(id).map(s => s.toLowerCase());
  if (p.includes('weapons') || p.includes('weapon')) return p.includes('pistols') ? 'pistol' : 'primary';
  if (p.includes('body armor') || (p.includes('armor') && !p.includes('rigs'))) return 'armor';
  if (p.includes('headgear') || p.includes('headwear')) return 'helmet';
  if (p.includes('tactical rigs') || p.includes('chest rigs') || p.includes('rigs')) return 'rig';
  if (p.includes('backpacks')) return 'backpack';
  if (p.includes('ammo')) return 'ammo';
  if (p.includes('magazines')) return 'mag';
  return 'stash';
}

// ── dados do RZ (skills/hideout/recipes) ─────────────────────────────────────
const BASELINE = [
  { id: 'ROUBLES', qty: 100000 }, { id: 'SALEWA', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 2 },
  { id: 'ALUMINUM_SPLINT', qty: 1 }, { id: 'ANALGIN', qty: 1 }, { id: 'MRE', qty: 1 },
  { id: 'CRACKERS', qty: 1 }, { id: 'AQUAMARI', qty: 1 }, { id: 'BAYONET', qty: 1 },
];
const backupKit = (w, mag, ammo, ac, pistol, pmag) => [
  { id: w, qty: 1 }, { id: mag, qty: 4 }, { id: ammo, qty: ac }, { id: pistol, qty: 1 }, { id: pmag, qty: 2 },
  // munição da pistola backup (sempre MAKAROV 9x18 nas recipes) — antes faltava (pente vazio).
  { id: 'AMMO_9x18_PST', qty: 60 },
  { id: 'PACA', qty: 1 }, { id: 'SSH68', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'SCAV_BACKPACK', qty: 1 },
  { id: 'IFAK', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 1 }, { id: 'SQUASH', qty: 1 },
];

// Item 005/010: multiplicadores de XP de skill por classe (buffs + debuffs temáticos; ajustáveis aqui ou no JSON).
// fator 1 = vanilla, 2.0 = +100% (buff, seta/borda verde), 0.5 = −50% (debuff, seta/borda vermelha).
// Cada classe tem trade-off: forte na especialidade (buff) e fraca onde não é o foco (debuff < 1).
const SKILL_MULTIPLIERS = {
  // FirstAid/FieldMedicine = skills do Skills-Extended (item 006); só ganham efeito com o SE instalado.
  medicoDeCombate:    { Surgery: 2.0, Vitality: 1.5, Health: 1.5, Immunity: 1.5, StressResistance: 1.3, RecoilControl: 0.7, FirstAid: 1.5, FieldMedicine: 1.5 },
  cacador:            { Sniper: 2.0, Perception: 1.5, CovertMovement: 1.5, Attention: 1.3, Assault: 0.7 },
  fuzileiro:          { Assault: 2.0, RecoilControl: 1.5, AimDrills: 1.5, MagDrills: 1.5, CovertMovement: 0.7 },
  batedor:            { CovertMovement: 2.0, Search: 1.5, Perception: 1.5, Endurance: 1.3, RecoilControl: 0.8 },
  operadorFurtivo:    { CovertMovement: 2.0, Search: 1.5, Perception: 1.3, MagDrills: 1.3, Assault: 0.7 },
  armeiro:            { WeaponTreatment: 2.0, TroubleShooting: 1.5, Intellect: 1.5, Strength: 1.3, Endurance: 0.8 },
  operadorTatico:     { Strength: 1.5, Endurance: 1.5, AimDrills: 1.5, Assault: 1.3, MagDrills: 1.3, CovertMovement: 0.7 },
  sobrevivencialista: { Metabolism: 2.0, Immunity: 1.5, Vitality: 1.5, Health: 1.3, Search: 1.3, RecoilControl: 0.8 },
  saqueador:          { Search: 2.0, Attention: 1.5, Perception: 1.5, Intellect: 1.5, Memory: 1.3, RecoilControl: 0.7 },
  gerenteDeOperacoes: { Crafting: 2.0, HideoutManagement: 2.0, Intellect: 1.5, Memory: 1.5, Charisma: 1.5, Strength: 0.7 },
};

// Item 011: identidade visual por classe (ícone PNG + cor do nome em hex). iconFile = arquivo em
// BepInEx/plugins/CustomClasses/icons/ (placeholders agora; trocar a arte depois sem recompilar).
const CLASS_VISUAL = {
  // Paleta Tarkov-dark: tons dessaturados/militares (bronze, ferrugem, oliva, cáqui, âmbar) +
  // frios apagados (azul-aço, ardósia, gunmetal) p/ recon/furtivo. Inspirada nos selos de edição
  // do EFT (Unheard/EOD, âmbar-dourado). O ícone (silhueta branca) é tingido com esta cor em runtime.
  medicoDeCombate:    { iconFile: 'medicoDeCombate.png',    nameColor: '#6f9455' },
  cacador:            { iconFile: 'cacador.png',            nameColor: '#c2973f' },
  fuzileiro:          { iconFile: 'fuzileiro.png',          nameColor: '#b0573a' },
  batedor:            { iconFile: 'batedor.png',            nameColor: '#5f7f93' },
  operadorFurtivo:    { iconFile: 'operadorFurtivo.png',    nameColor: '#7d7392' },
  armeiro:            { iconFile: 'armeiro.png',            nameColor: '#a8824e' },
  operadorTatico:     { iconFile: 'operadorTatico.png',     nameColor: '#7a818c' },
  sobrevivencialista: { iconFile: 'sobrevivencialista.png', nameColor: '#97934e' },
  saqueador:          { iconFile: 'saqueador.png',          nameColor: '#c4ad45' },
  gerenteDeOperacoes: { iconFile: 'gerenteDeOperacoes.png', nameColor: '#4f8a80' },
  // item 016 (Peladão): identidade visual adiantada — só vira JSON quando o profile existir em class-recipes.js.
  peladao:            { iconFile: 'peladao.png',            nameColor: '#c28a60' },
};

// Item 008 (i18n): nome da classe em INGLÊS. O `name` (PT) continua sendo a CHAVE da edition no launcher;
// este nome aparece in-game (menu/tooltip) quando o EFT está em inglês. Em "po" (português) mostra o `name`.
const DISPLAY_NAME_EN = {
  medicoDeCombate:    'Combat Medic',
  cacador:            'Hunter',
  fuzileiro:          'Rifleman',
  batedor:            'Scout',
  operadorFurtivo:    'Stealth Operator',
  armeiro:            'Armorer',
  operadorTatico:     'Tactical Operator',
  sobrevivencialista: 'Survivalist',
  saqueador:          'Scavenger',
  gerenteDeOperacoes: 'Operations Manager',
  peladao:            'NAKED',
};

const PROFILES = require('./class-recipes.js')(BASELINE, backupKit);

// ── helpers ──────────────────────────────────────────────────────────────────
function aggregate(p) {
  const t = new Map();
  const add = (arr, m = 1) => { for (const it of arr) t.set(it.id, (t.get(it.id) || 0) + it.qty * m); };
  if (!p.noBaseline) { add(BASELINE); }   // item 016: Peladão nasce 100% sem itens (sem o kit comum)
  add(p.tema); add(p.primary); add(p.backup, p.backupCount);
  return t;
}

function pickEquipped(p) {
  // varre `primary` na ordem: arma primária, pistola, armor, helmet, rig, backpack + ammos
  const eq = {};
  const ammos = [];
  for (const it of p.primary) {
    const s = slotOf(it.id);
    if (s === 'ammo') { ammos.push(it.id); continue; }
    if (s === 'primary' && !eq.primary) eq.primary = it.id;
    else if (s === 'pistol' && !eq.pistol) eq.pistol = it.id;
    else if (s === 'armor' && !eq.armor) eq.armor = it.id;
    else if (s === 'helmet' && !eq.helmet) eq.helmet = it.id;
    else if (s === 'rig' && !eq.rig) eq.rig = it.id;
    else if (s === 'backpack' && !eq.backpack) eq.backpack = it.id;
  }
  eq.primaryAmmo = ammos[0] || null;
  eq.pistolAmmo = ammos[1] || null;
  return eq;
}

function buildLoadout(p) {
  const eq = pickEquipped(p);
  const equipped = {};
  if (eq.primary) {
    // arma principal = premium (preset mais kitado: mira/foregrip/tac). Etapa 1.
    equipped.FirstPrimaryWeapon = { preset: bsg(eq.primary), premium: true, loadedMag: true, chambered: true };
    if (eq.primaryAmmo) equipped.FirstPrimaryWeapon.ammo = bsg(eq.primaryAmmo);
  }
  if (eq.pistol) {
    equipped.Holster = { preset: bsg(eq.pistol), loadedMag: true };
    if (eq.pistolAmmo) equipped.Holster.ammo = bsg(eq.pistolAmmo);
  }
  if (eq.armor) equipped.ArmorVest = { tpl: bsg(eq.armor) };
  if (eq.helmet) equipped.Headwear = { tpl: bsg(eq.helmet) };
  if (eq.rig) equipped.TacticalVest = { tpl: bsg(eq.rig) };
  if (eq.backpack) equipped.Backpack = { tpl: bsg(eq.backpack) };

  // stash = agregado - 1 de cada item equipado
  const totals = aggregate(p);
  for (const k of ['primary', 'pistol', 'armor', 'helmet', 'rig', 'backpack']) {
    if (eq[k] && totals.has(eq[k])) {
      const n = totals.get(eq[k]) - 1;
      if (n > 0) totals.set(eq[k], n); else totals.delete(eq[k]);
    }
  }
  const stash = [];
  for (const [id, qty] of totals.entries()) stash.push({ tpl: bsg(id), count: qty });
  return { equipped, stash };
}

// ── emit ─────────────────────────────────────────────────────────────────────
// item 019: normalize EOL + trailing whitespace before comparing, so EOL-only
// differences (CRLF checkout, editor rewrite) never count as a real divergence.
const normalizeText = s => s.replace(/\r\n/g, '\n').replace(/[ \t]+$/gm, '');

if (!fs.existsSync(OUT_DIR)) fs.mkdirSync(OUT_DIR, { recursive: true });
const report = [];
let written = 0, frozenCount = 0, unchanged = 0;
for (const p of PROFILES) {
  const loadout = buildLoadout(p);
  const json = {
    name: p.name,
    displayName: { en: DISPLAY_NAME_EN[p.fileName] || p.name, pt: p.name },   // item 008 (i18n)
    baseEdition: 'SPT Zero to hero',
    description: p.description,
    ...(CLASS_VISUAL[p.fileName] ? CLASS_VISUAL[p.fileName] : {}),   // item 011: iconFile + nameColor
    skills: p.skillOverrides,
    ...(SKILL_MULTIPLIERS[p.fileName] ? { skillMultipliers: SKILL_MULTIPLIERS[p.fileName] } : {}),
    hideout: p.hideout,
    ...(p.outfit ? { outfit: p.outfit } : {}),
    loadout,
  };
  const file = path.join(OUT_DIR, `${p.fileName}.jsonc`);
  const next = JSON.stringify(json, null, 2) + '\n';

  // item 019: freeze guard — never overwrite a diverging existing file without --force.
  let status = 'written';
  if (fs.existsSync(file)) {
    const current = fs.readFileSync(file, 'utf8');
    if (normalizeText(current) === normalizeText(next)) {
      status = 'unchanged';
      unchanged++;
    } else if (!FORCE) {
      status = 'frozen';
      frozenCount++;
      console.warn(`  ! ${p.fileName}.jsonc frozen — editor/manual edits detected; use --force to overwrite`);
    }
  }
  if (status === 'written') {
    fs.writeFileSync(file, next, { encoding: 'utf8' });
    written++;
  }
  report.push({ name: p.name, status, equipped: Object.keys(loadout.equipped).length, stash: loadout.stash.length });
}

console.log(`\n=== ${PROFILES.length} classes processadas em modded/Server/config/classes/ ===`);
for (const r of report) console.log(`  ${r.name.padEnd(26)} [${r.status}] equipado=${r.equipped}  stash=${r.stash}`);
console.log(`\nSummary: ${written} written, ${frozenCount} skipped (frozen), ${unchanged} unchanged.`);
if (frozenCount > 0) console.log('Frozen files diverge from the generator output (bootstrap-only). Re-run with --force to overwrite.');
if (written > 0) console.log('✓ Rode /compile-mod CustomClasses para shippar os JSONs ao servidor.');
