#!/usr/bin/env node
/**
 * build-loadouts.js (one-shot — delete after loadouts stable)
 *
 * Reads anchor-items.json and renders the "Inventário inicial" section
 * for mods/RZCustomProfiles/backlog/001-custom-profiles.md.
 *
 * Each profile defines: baseline (shared), primary (1× worn), backup
 * (3× in stash, except Armeiro = 2×), and item-tema (specialty).
 *
 * Recipe entries: { id: '<ANCHOR_ID>', qty: <n> }
 *
 * Total per profile = baseline + primary + (backup * count) + tema.
 * Target: ~2.000.000 ₽ ± 50k.
 */

'use strict';
const fs   = require('fs');
const path = require('path');

const ROOT = path.resolve(__dirname, '..');
const J = JSON.parse(fs.readFileSync(
  path.join(ROOT, 'mods/RZCustomProfiles/backlog/anchor-items.json'), 'utf8'
));

function priceOf(id) {
  const r = J[id];
  if (!r) throw new Error(`Anchor not found: ${id}`);
  // Currency: each unit is worth basePrice (Roubles=1, Euros≈140, Dollars≈140)
  if (r.tags && r.tags.includes('Currency')) return r.basePrice || 1;
  return r.avg24hPrice || 0;
}

function nameOf(id) {
  const r = J[id];
  return r ? (r.shortName || r.name) : id;
}

// ── Baseline shared by all profiles ─────────────────────────────────────────

const BASELINE = [
  { id: 'ROUBLES',         qty: 100000 },
  { id: 'SALEWA',          qty: 1 },
  { id: 'ARMY_BANDAGE',    qty: 2 },
  { id: 'ALUMINUM_SPLINT', qty: 1 },
  { id: 'ANALGIN',         qty: 1 },
  { id: 'MRE',             qty: 1 },
  { id: 'CRACKERS',        qty: 1 },
  { id: 'AQUAMARI',        qty: 1 },
  { id: 'BAYONET',         qty: 1 },
];

// ── Standard backup (used by all combat profiles) ──────────────────────────
// Tier 1-2: PACA + SSh-68 + BlackRock + ScavBP. Different weapon per profile.

function backupKit(weapon, mag, ammo, ammoCount, pistol, pistolMag) {
  return [
    { id: weapon,    qty: 1 },
    { id: mag,       qty: 4 },
    { id: ammo,      qty: ammoCount },
    { id: pistol,    qty: 1 },
    { id: pistolMag, qty: 2 },
    { id: 'PACA',          qty: 1 },
    { id: 'SSH68',         qty: 1 },
    { id: 'BLACKROCK',     qty: 1 },
    { id: 'SCAV_BACKPACK', qty: 1 },
    { id: 'IFAK',          qty: 1 },
    { id: 'ARMY_BANDAGE',  qty: 1 },
    { id: 'SQUASH',        qty: 1 },
  ];
}

// ── Profiles ────────────────────────────────────────────────────────────────

const PROFILES = [
  {
    name: 'Sanitarista',
    backupCount: 3,
    primary: [
      { id: 'AKM',             qty: 1 },
      { id: 'MAG_AKM_30',      qty: 4 },
      { id: 'AMMO_762x39_PS',  qty: 180 },
      { id: 'MAKAROV',         qty: 1 },
      { id: 'MAG_PM_8',        qty: 2 },
      { id: 'AMMO_9x18_PST',   qty: 60 },
      { id: 'LZSH',            qty: 1 },
      { id: '6B23_1',          qty: 1 },
      { id: 'BLACKROCK',       qty: 1 },
      { id: 'MBSS',            qty: 1 },
      { id: 'IFAK',            qty: 1 },
      { id: 'SALEWA',          qty: 1 },
      { id: 'ANALGIN',         qty: 1 },
      { id: 'ARMY_BANDAGE',    qty: 2 },
      { id: 'MRE',             qty: 1 },
      { id: 'AQUAMARI',        qty: 1 },
    ],
    backup: backupKit('AKM', 'MAG_AKM_30', 'AMMO_762x39_PS', 120, 'MAKAROV', 'MAG_PM_8'),
    tema: [
      { id: 'IFAK',    qty: 2 },
      { id: 'SURV12',  qty: 1 },
      { id: 'CALOK_B', qty: 1 },
    ],
  },

  {
    name: 'Franco-Atirador',
    backupCount: 3,
    primary: [
      { id: 'SV98',              qty: 1 },
      { id: 'MAG_SV98_10',       qty: 4 },
      { id: 'AMMO_762x54R_LPS',  qty: 80 },
      { id: 'PSO1',              qty: 1 },
      { id: 'BIPOD_HARRIS',      qty: 1 },
      { id: 'MAKAROV',           qty: 1 },
      { id: 'MAG_PM_8',          qty: 2 },
      { id: 'AMMO_9x18_PST',     qty: 60 },
      { id: 'LZSH',              qty: 1 },
      { id: '6B2',               qty: 1 },
      { id: 'TRIPLE_BANDOLIER',  qty: 1 },
      { id: 'PILGRIM',           qty: 1 },
      { id: 'IFAK',              qty: 1 },
      { id: 'SALEWA',            qty: 1 },
      { id: 'ANALGIN',           qty: 1 },
      { id: 'ARMY_BANDAGE',      qty: 2 },
      { id: 'MRE',               qty: 1 },
      { id: 'AQUAMARI',          qty: 1 },
    ],
    backup: [
      { id: 'MOSIN_INFANTRY',    qty: 1 },
      { id: 'AMMO_762x54R_LPS',  qty: 60 },
      { id: 'MAKAROV',           qty: 1 },
      { id: 'MAG_PM_8',          qty: 2 },
      { id: 'PACA',              qty: 1 },
      { id: 'SSH68',             qty: 1 },
      { id: 'BLACKROCK',         qty: 1 },
      { id: 'SCAV_BACKPACK',     qty: 1 },
      { id: 'IFAK',              qty: 1 },
      { id: 'ARMY_BANDAGE',      qty: 1 },
      { id: 'SQUASH',            qty: 1 },
    ],
    tema: [
      { id: 'COMPASS',         qty: 1 },
      { id: 'WOODS_MAP',       qty: 1 },
      { id: 'INTERCHANGE_MAP', qty: 1 },
      { id: 'TUSHONKA',        qty: 3 },
      { id: 'AUGMENTIN',       qty: 3 },
    ],
  },

  {
    name: 'Fuzileiro',
    backupCount: 3,
    primary: [
      { id: 'AKM',             qty: 1 },
      { id: 'MAG_AKM_30',      qty: 4 },
      { id: 'AMMO_762x39_BP',  qty: 180 },
      { id: 'OKP7',            qty: 1 },
      { id: 'MP443',           qty: 1 },
      { id: 'MAG_MP443_18',    qty: 2 },
      { id: 'AMMO_9x19_PST',   qty: 60 },
      { id: 'LZSH',            qty: 1 },
      { id: '6B23_1',          qty: 1 },
      { id: 'BLACKROCK',       qty: 1 },
      { id: 'TRIZIP',          qty: 1 },
      { id: 'IFAK',            qty: 1 },
      { id: 'SALEWA',          qty: 1 },
      { id: 'ANALGIN',         qty: 1 },
      { id: 'ARMY_BANDAGE',    qty: 2 },
      { id: 'MRE',             qty: 1 },
      { id: 'AQUAMARI',        qty: 1 },
    ],
    backup: backupKit('AKM', 'MAG_AKM_30', 'AMMO_762x39_PS', 120, 'MAKAROV', 'MAG_PM_8'),
    tema: [
      { id: 'MAG_AKM_30',      qty: 2 },
    ],
  },

  {
    name: 'Batedor',
    backupCount: 3,
    primary: [
      { id: 'AKS74U',           qty: 1 },
      { id: 'MAG_AK_30',        qty: 4 },
      { id: 'AMMO_545x39_BS',   qty: 120 },
      { id: 'MAKAROV',          qty: 1 },
      { id: 'MAG_PM_8',         qty: 2 },
      { id: 'AMMO_9x18_PST',    qty: 60 },
      { id: 'TAC_HELMET',       qty: 1 },
      { id: '6B2',              qty: 1 },
      { id: 'BLACKROCK',        qty: 1 },
      { id: 'PARATUS',          qty: 1 },
      { id: 'IFAK',             qty: 1 },
      { id: 'SALEWA',           qty: 1 },
      { id: 'ANALGIN',          qty: 1 },
      { id: 'ARMY_BANDAGE',     qty: 2 },
      { id: 'MRE',              qty: 1 },
      { id: 'AQUAMARI',         qty: 1 },
    ],
    backup: backupKit('AKS74U', 'MAG_AK_30', 'AMMO_545x39_PS', 120, 'MAKAROV', 'MAG_PM_8'),
    tema: [
      { id: 'COMPASS',          qty: 1 },
      { id: 'WOODS_MAP',        qty: 1 },
      { id: 'INTERCHANGE_MAP',  qty: 1 },
      { id: 'ETG_CHANGE',       qty: 1 },
    ],
  },

  {
    name: 'Operador Noturno',
    backupCount: 3,
    primary: [
      { id: 'AKMS',             qty: 1 },
      { id: 'MAG_AKM_30',       qty: 4 },
      { id: 'AMMO_762x39_US',   qty: 180 },
      { id: 'PBS1',             qty: 1 },
      { id: 'MAKAROV',          qty: 1 },
      { id: 'MAG_PM_8',         qty: 2 },
      { id: 'AMMO_9x18_PST',    qty: 60 },
      { id: 'LZSH',             qty: 1 },
      { id: '6B2',              qty: 1 },
      { id: 'BLACKROCK',        qty: 1 },
      { id: 'MBSS',             qty: 1 },
      { id: 'PNV10T',           qty: 1 },
      { id: 'IFAK',             qty: 1 },
      { id: 'SALEWA',           qty: 1 },
      { id: 'ANALGIN',          qty: 1 },
      { id: 'ARMY_BANDAGE',     qty: 2 },
      { id: 'MRE',              qty: 1 },
      { id: 'AQUAMARI',         qty: 1 },
    ],
    backup: backupKit('AKMS', 'MAG_AKM_30', 'AMMO_762x39_PS', 120, 'MAKAROV', 'MAG_PM_8'),
    tema: [
      { id: 'PNV10T',           qty: 1 },
      { id: 'PBS1',             qty: 1 },
      { id: 'IFAK',             qty: 1 },
      { id: 'TUSHONKA',         qty: 3 },
      { id: 'AUGMENTIN',        qty: 1 },
      { id: 'AMMO_762x39_US',   qty: 60 },
    ],
  },

  {
    name: 'Armeiro',
    backupCount: 2,
    primary: [
      { id: 'AKM',             qty: 1 },
      { id: 'MAG_AKM_30',      qty: 4 },
      { id: 'AMMO_762x39_PS',  qty: 120 },
      { id: 'MAKAROV',         qty: 1 },
      { id: 'MAG_PM_8',        qty: 2 },
      { id: 'AMMO_9x18_PST',   qty: 60 },
      { id: 'TAC_HELMET',      qty: 1 },
      { id: '6B2',             qty: 1 },
      { id: 'BLACKROCK',       qty: 1 },
      { id: 'MBSS',            qty: 1 },
      { id: 'IFAK',            qty: 1 },
      { id: 'SALEWA',          qty: 1 },
      { id: 'ANALGIN',         qty: 1 },
      { id: 'ARMY_BANDAGE',    qty: 2 },
      { id: 'MRE',             qty: 1 },
      { id: 'AQUAMARI',        qty: 1 },
    ],
    backup: backupKit('AKM', 'MAG_AKM_30', 'AMMO_762x39_PS', 90, 'MAKAROV', 'MAG_PM_8'),
    tema: [
      { id: 'WEAPON_REPAIR_KIT', qty: 1 },
      { id: 'TOOLSET',           qty: 1 },
      { id: 'WD40',              qty: 1 },
      { id: 'MULTITOOL',         qty: 1 },
      { id: 'BOLTS',             qty: 1 },
    ],
  },

  {
    name: 'Operador Tático',
    backupCount: 3,
    primary: [
      { id: 'M4A1',            qty: 1 },
      { id: 'MAG_M4_30',       qty: 4 },
      { id: 'AMMO_556x45_M855', qty: 180 },
      { id: 'OKP7',            qty: 1 },
      { id: 'MP443',           qty: 1 },
      { id: 'MAG_MP443_18',    qty: 2 },
      { id: 'AMMO_9x19_PST',   qty: 60 },
      { id: 'MICH2001',        qty: 1 },
      { id: '6B23_1',          qty: 1 },
      { id: 'BLACKROCK',       qty: 1 },
      { id: 'TRIZIP',          qty: 1 },
      { id: 'IFAK',            qty: 1 },
      { id: 'SALEWA',          qty: 1 },
      { id: 'ANALGIN',         qty: 1 },
      { id: 'ARMY_BANDAGE',    qty: 2 },
      { id: 'MRE',             qty: 1 },
      { id: 'AQUAMARI',        qty: 1 },
    ],
    backup: backupKit('AK74N', 'MAG_AK_30', 'AMMO_545x39_PS', 90, 'MAKAROV', 'MAG_PM_8'),
    tema: [
      { id: 'ETG_CHANGE',  qty: 1 },
    ],  // M4A1 caro + tier 3 armor já consomem 80% do budget; tema enxuto
  },

  {
    name: 'Sobrevivencialista',
    backupCount: 3,
    primary: [
      { id: 'SAIGA12',           qty: 1 },
      { id: 'AMMO_12_70_MAG',    qty: 30 },
      { id: 'MAKAROV',           qty: 1 },
      { id: 'MAG_PM_8',          qty: 2 },
      { id: 'AMMO_9x18_PST',     qty: 60 },
      { id: 'TAC_HELMET',        qty: 1 },
      { id: '6B2',               qty: 1 },
      { id: 'BLACKROCK',         qty: 1 },
      { id: 'PILGRIM',           qty: 1 },
      { id: 'IFAK',              qty: 1 },
      { id: 'SALEWA',            qty: 1 },
      { id: 'ANALGIN',           qty: 1 },
      { id: 'ARMY_BANDAGE',      qty: 2 },
      { id: 'MRE',               qty: 1 },
      { id: 'AQUAMARI',          qty: 1 },
    ],
    backup: [
      { id: 'TOZ106',           qty: 1 },
      { id: 'MAG_TOZ106_4',     qty: 2 },
      { id: 'AMMO_20_70_BUCK',  qty: 1 }, // pack of 25
      { id: 'MAKAROV',          qty: 1 },
      { id: 'MAG_PM_8',         qty: 2 },
      { id: 'PACA',             qty: 1 },
      { id: 'SSH68',            qty: 1 },
      { id: 'BLACKROCK',        qty: 1 },
      { id: 'SCAV_BACKPACK',    qty: 1 },
      { id: 'IFAK',             qty: 1 },
      { id: 'ARMY_BANDAGE',     qty: 1 },
      { id: 'SQUASH',           qty: 1 },
    ],
    tema: [
      { id: 'TUSHONKA',  qty: 6 },
      { id: 'AQUAMARI',  qty: 4 },
      { id: 'AUGMENTIN', qty: 5 },
      { id: 'VASELINE',  qty: 4 },
      { id: 'AI2',       qty: 7 },
      { id: 'MULTITOOL', qty: 1 },
    ],
  },

  {
    name: 'Saqueador',
    backupCount: 3,
    primary: [
      { id: 'SAIGA9',          qty: 1 },
      { id: 'MAG_SAIGA9_10',   qty: 4 },
      { id: 'AMMO_9x19_PST',   qty: 80 },
      { id: 'MAKAROV',         qty: 1 },
      { id: 'MAG_PM_8',        qty: 2 },
      { id: 'AMMO_9x18_PST',   qty: 60 },
      { id: 'TAC_HELMET',      qty: 1 },
      { id: '6B2',             qty: 1 },
      { id: 'BLACKROCK',       qty: 1 },
      { id: 'PILGRIM',         qty: 1 },
      { id: 'IFAK',            qty: 1 },
      { id: 'SALEWA',          qty: 1 },
      { id: 'ANALGIN',         qty: 1 },
      { id: 'ARMY_BANDAGE',    qty: 2 },
      { id: 'MRE',             qty: 1 },
      { id: 'AQUAMARI',        qty: 1 },
    ],
    backup: [
      { id: 'TOZ106',           qty: 1 },
      { id: 'MAG_TOZ106_4',     qty: 1 },
      { id: 'AMMO_20_70_BUCK',  qty: 1 },
      { id: 'MAKAROV',          qty: 1 },
      { id: 'MAG_PM_8',         qty: 2 },
      { id: 'PACA',             qty: 1 },
      { id: 'SSH68',            qty: 1 },
      { id: 'BLACKROCK',        qty: 1 },
      { id: 'PARATUS',          qty: 1 },
      { id: 'IFAK',             qty: 1 },
      { id: 'ARMY_BANDAGE',     qty: 1 },
      { id: 'SQUASH',           qty: 1 },
    ],
    tema: [
      { id: 'DOCUMENTS_CASE',   qty: 2 },
      { id: 'CUSTOMS_MAP',      qty: 1 },
      { id: 'INTERCHANGE_MAP',  qty: 1 },
      { id: 'WOODS_MAP',        qty: 1 },
      { id: 'ROUBLES',          qty: 200000 },
    ],
  },

  {
    name: 'Gerente de Operações',
    backupCount: 3,
    primary: [
      { id: 'SAIGA12',          qty: 1 },
      { id: 'AMMO_12_70_MAG',   qty: 20 },
      { id: 'MAKAROV',          qty: 1 },
      { id: 'MAG_PM_8',         qty: 2 },
      { id: 'AMMO_9x18_PST',    qty: 60 },
      { id: 'TAC_HELMET',       qty: 1 },
      { id: '6B2',              qty: 1 },
      { id: 'BLACKROCK',        qty: 1 },
      { id: 'MBSS',             qty: 1 },
      { id: 'IFAK',             qty: 1 },
      { id: 'SALEWA',           qty: 1 },
      { id: 'ANALGIN',          qty: 1 },
      { id: 'ARMY_BANDAGE',     qty: 2 },
      { id: 'MRE',              qty: 1 },
      { id: 'AQUAMARI',         qty: 1 },
    ],
    backup: [
      { id: 'TOZ106',           qty: 1 },
      { id: 'MAG_TOZ106_4',     qty: 1 },
      { id: 'AMMO_20_70_BUCK',  qty: 1 },
      { id: 'MAKAROV',          qty: 1 },
      { id: 'MAG_PM_8',         qty: 2 },
      { id: 'PACA',             qty: 1 },
      { id: 'SSH68',            qty: 1 },
      { id: 'BLACKROCK',        qty: 1 },
      { id: 'SCAV_BACKPACK',    qty: 1 },
      { id: 'IFAK',             qty: 1 },
      { id: 'ARMY_BANDAGE',     qty: 1 },
      { id: 'SQUASH',           qty: 1 },
    ],
    tema: [
      { id: 'TOOLSET',    qty: 2 },
      { id: 'CPU_FAN',    qty: 4 },
      { id: 'WIRES',      qty: 4 },
      { id: 'DUCT_TAPE',  qty: 3 },
      { id: 'BOLTS',      qty: 1 },
      { id: 'SCREWS',     qty: 1 },
      { id: 'ROUBLES',    qty: 300000 },
    ],
  },
];

// ── Compute & render ────────────────────────────────────────────────────────

function sum(items) {
  return items.reduce((s, x) => s + priceOf(x.id) * x.qty, 0);
}

function fmt(n) {
  return Math.round(n).toLocaleString('pt-BR');
}

function renderItems(items, indent = '') {
  const lines = [];
  for (const it of items) {
    const subtotal = priceOf(it.id) * it.qty;
    lines.push(`${indent}| ${nameOf(it.id)} | \`${it.id}\` | ${it.qty.toLocaleString('pt-BR')} | ${fmt(priceOf(it.id))} | ${fmt(subtotal)} |`);
  }
  return lines.join('\n');
}

function renderProfile(p) {
  const baselineTotal = sum(BASELINE);
  const primaryTotal  = sum(p.primary);
  const backupUnit    = sum(p.backup);
  const backupTotal   = backupUnit * p.backupCount;
  const temaTotal     = sum(p.tema);
  const grand         = baselineTotal + primaryTotal + backupTotal + temaTotal;

  const lines = [];
  lines.push(`### ${p.name}`);
  lines.push('');
  lines.push(`**Item-tema (${p.name.toLowerCase()}):**`);
  lines.push('');
  lines.push('| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |');
  lines.push('|------|----|----:|-------:|-----------:|');
  lines.push(renderItems(p.tema));
  lines.push(`| **Subtotal item-tema** | | | | **${fmt(temaTotal)}** |`);
  lines.push('');
  lines.push(`**Primary loadout** (vestido):`);
  lines.push('');
  lines.push('| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |');
  lines.push('|------|----|----:|-------:|-----------:|');
  lines.push(renderItems(p.primary));
  lines.push(`| **Subtotal primary** | | | | **${fmt(primaryTotal)}** |`);
  lines.push('');
  lines.push(`**Backup loadout** (×${p.backupCount} no stash):`);
  lines.push('');
  lines.push('| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |');
  lines.push('|------|----|----:|-------:|-----------:|');
  lines.push(renderItems(p.backup));
  lines.push(`| **Subtotal backup unit** | | | | **${fmt(backupUnit)}** |`);
  lines.push(`| **Backup × ${p.backupCount}** | | | | **${fmt(backupTotal)}** |`);
  lines.push('');
  lines.push('**Resumo:**');
  lines.push('');
  lines.push('| Bloco | Subtotal ₽ |');
  lines.push('|-------|-----------:|');
  lines.push(`| Baseline universal | ${fmt(baselineTotal)} |`);
  lines.push(`| Item-tema | ${fmt(temaTotal)} |`);
  lines.push(`| Primary loadout | ${fmt(primaryTotal)} |`);
  lines.push(`| Backup × ${p.backupCount} | ${fmt(backupTotal)} |`);
  lines.push(`| **Total perfil** | **${fmt(grand)}** |`);
  lines.push(`| Distância de 2.000.000 ₽ | ${fmt(grand - 2000000)} |`);
  lines.push('');

  return { md: lines.join('\n'), grand };
}

// ── Main ────────────────────────────────────────────────────────────────────

const lines = [];
lines.push('## Inventário inicial');
lines.push('');
lines.push('Cada perfil recebe 4 loadouts (Armeiro 3) — 1 vestido + N no stash, calibrados para ~2.000.000 ₽ totais (preços avg 24h PVE flea via tarkov-market.com).');
lines.push('Tier-cap: armor classe 1-2 dominante, classe 3 raro (apenas no primary de alguns perfis); helmets até MICH 2001; sem plate hard, sem GPNVG-18.');
lines.push('');
lines.push('### Baseline universal (todos os perfis recebem)');
lines.push('');
lines.push('| Item | ID | Qtd | Unit ₽ | Subtotal ₽ |');
lines.push('|------|----|----:|-------:|-----------:|');
lines.push(renderItems(BASELINE));
const baselineTotal = sum(BASELINE);
lines.push(`| **Total baseline** | | | | **${fmt(baselineTotal)}** |`);
lines.push('');

const summaries = [];

for (const p of PROFILES) {
  const out = renderProfile(p);
  lines.push('---');
  lines.push('');
  lines.push(out.md);
  summaries.push({ name: p.name, total: out.grand, delta: out.grand - 2000000 });
}

lines.push('---');
lines.push('');
lines.push('### Resumo de calibração');
lines.push('');
lines.push('| Perfil | Total ₽ | Δ 2M |');
lines.push('|--------|--------:|-----:|');
for (const s of summaries) {
  const flag = Math.abs(s.delta) <= 50000 ? '✓' : '⚠️';
  lines.push(`| ${s.name} | ${fmt(s.total)} | ${fmt(s.delta)} ${flag} |`);
}

const out = lines.join('\n') + '\n';
console.log(out);
