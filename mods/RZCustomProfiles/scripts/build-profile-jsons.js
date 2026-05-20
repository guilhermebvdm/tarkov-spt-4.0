#!/usr/bin/env node
/**
 * build-profile-jsons.js
 *
 * Gera os 10 arquivos .json em modded/profiles/ a partir das recipes
 * por classe + metadata (skills, hideout, descrição).
 *
 * Fontes:
 *   - anchor-items.json (id simbólico → bsgId do EFT)
 *   - tools/tarkov-itemdb/cache/spt-raw.json (TPL → stackMaxSize)
 *   - planejamento §Inventário inicial / §Modelo de balanceamento / §Hideout inicial
 *
 * Regra de stack (PA-01-03 da review 01):
 *   - stackMax == 1  → emite N entradas { Tpl, Count: 1 }
 *   - stackMax > 1   → emite ceil(qty/stackMax) entradas com Count = stackMax
 *
 * Saída: 10 arquivos UTF-8 sem BOM em ../modded/profiles/<fileName>.json
 */

'use strict';
const fs   = require('fs');
const path = require('path');

const MOD_ROOT = path.resolve(__dirname, '..');

const ANCHOR = JSON.parse(fs.readFileSync(
  path.join(MOD_ROOT, 'backlog/anchor-items.json'), 'utf8'
));

// Versionado (extraído via extract-item-data.js a partir do tarkov-itemdb).
// Não depende de tools/tarkov-itemdb/cache/* (gitignored) em runtime.
const ITEM_DATA_PATH = path.join(__dirname, 'item-data.json');
if (!fs.existsSync(ITEM_DATA_PATH)) {
  console.error(`ERRO: ${ITEM_DATA_PATH} não encontrado. Rode 'node mods/RZCustomProfiles/scripts/extract-item-data.js' antes.`);
  process.exit(1);
}
const ITEM_DATA = JSON.parse(fs.readFileSync(ITEM_DATA_PATH, 'utf8'));

// ── Resolvers ────────────────────────────────────────────────────────────────

function bsgIdOf(anchorId) {
  const r = ANCHOR[anchorId];
  if (!r) throw new Error(`Anchor não encontrado: ${anchorId}`);
  if (!r.bsgId) throw new Error(`Anchor ${anchorId} sem bsgId`);
  return r.bsgId;
}

function itemMetaOf(tpl) {
  const m = ITEM_DATA[tpl];
  if (!m) throw new Error(`TPL ${tpl} não encontrado em item-data.json. Regenerar via extract-item-data.js?`);
  return m;
}

function stackMaxOf(tpl) {
  return itemMetaOf(tpl).stackMax;
}

// ── Baseline universal (todas as classes) ────────────────────────────────────

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

// Backup completo: weapon + munição + armor + rig + mochila + helmet + meds.
// Habilitado novamente após mudança de BaseProfile 0 (Standard) → 8 (Zero to Hero),
// que começa com stash VAZIO — então os big items 4×4 do backup têm espaço pra caber.
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

// ── Skill default set (todos os 51 nomes do exampleProfile.json) ─────────────

const SKILL_KEYS = [
  'Endurance', 'Strength', 'Vitality', 'Health', 'StressResistance', 'Metabolism', 'Immunity',
  'Perception', 'Intellect', 'Attention', 'Charisma', 'Memory',
  'Pistol', 'Revolver', 'SMG', 'Assault', 'Shotgun', 'Sniper', 'LMG', 'HMG',
  'Launcher', 'AttachedLauncher', 'Throwing', 'Melee', 'DMR', 'RecoilControl', 'AimDrills', 'TroubleShooting',
  'Surgery', 'CovertMovement', 'Search', 'MagDrills', 'Sniping', 'ProneMovement',
  'FieldMedicine', 'FirstAid', 'LightVests', 'HeavyVests', 'WeaponModding', 'AdvancedModding',
  'NightOps', 'SilentOps', 'Lockpicking', 'WeaponTreatment',
  'Freetrading', 'Auctions', 'Cleanoperations', 'Barter', 'Shadowconnections', 'Taskperformance',
  'Crafting', 'HideoutManagement',
];

function skillsFull(overrides) {
  const out = {};
  for (const k of SKILL_KEYS) out[k] = 0;
  for (const [k, v] of Object.entries(overrides)) {
    if (!SKILL_KEYS.includes(k)) throw new Error(`Skill desconhecida: ${k}`);
    out[k] = v;
  }
  return out;
}

// ── Hideout default set ──────────────────────────────────────────────────────

const HIDEOUT_KEYS = [
  'Stash', 'Generator', 'Vents', 'Security', 'WaterCloset', 'Heating', 'WaterCollector',
  'MedStation', 'Kitchen', 'RestSpace', 'Workbench', 'IntelligenceCenter', 'ShootingRange',
  'Library', 'ScavCase', 'Illumination', 'PlaceOfFame', 'AirFilteringUnit', 'SolarPower',
  'BoozeGenerator', 'BitcoinFarm', 'ChristmasIllumination', 'EmergencyWall', 'Gym',
  'WeaponStand', 'WeaponStandSecondary', 'EquipmentPresetsStand', 'CircleOfCultists',
];

function hideoutFull(overrides) {
  const out = {};
  for (const k of HIDEOUT_KEYS) out[k] = 0;
  out.Stash = 1; // padrão Standard
  for (const [k, v] of Object.entries(overrides)) {
    if (!HIDEOUT_KEYS.includes(k)) throw new Error(`Hideout station desconhecida: ${k}`);
    out[k] = v;
  }
  return out;
}

// ── Trader loyalty default (todos em 0, conforme planejamento atual) ─────────

const TRADER_IDS = [
  '54cb50c76803fa8b248b4571', // Prapor
  '54cb57776803fa99248b456e', // Therapist
  '579dc571d53a0658a154fbec', // Fence
  '58330581ace78e27b8b10cee', // Skier
  '5935c25fb3acc3127c3d8cd9', // Peacekeeper
  '5a7c2eca46aef81a7ca2145d', // Mechanic
  '5ac3b934156ae10c4430e83c', // Ragman
  '5c0647fdd443bc2504c2d371', // Jaeger
  '6617beeaa9cfa777ca915b7c', // Ref
  '656f0f98d80a697f855d34b1', // BTR
  '638f541a29ffd1183d187f57', // Lighthousekeeper
];

function tradersFull() {
  const out = {};
  for (const id of TRADER_IDS) out[id] = { Standing: 0.0, SalesSum: 0 };
  return out;
}

// ── Profiles (recipes + metadata) ────────────────────────────────────────────

const PROFILES = [
  {
    fileName: 'medicoDeCombate',
    name: 'Médico de Combate',
    description: 'Combat Medic. Sobrevive a ferimentos que matariam outros. Trata dano severo rápido e continua operacional após levar dano.',
    skillOverrides: { Assault: 5, Vitality: 5, Health: 3, StressResistance: 4, Attention: 2, Surgery: 7 },
    hideout: { MedStation: 1 },
    backupCount: 2,
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
    fileName: 'cacador',
    name: 'Caçador',
    description: 'Sniper. Paciente e preciso. Domina posições elevadas, minimiza movimento e elimina antes de ser detectado.',
    skillOverrides: { Sniper: 8, Endurance: 5, Perception: 6, Attention: 4, CovertMovement: 5 },
    hideout: { Heating: 1 },
    backupCount: 2,
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
      { id: 'COMPASS',   qty: 1 },
      { id: 'VASELINE',  qty: 1 },
      { id: 'TUSHONKA',  qty: 3 },
      { id: 'AUGMENTIN', qty: 3 },
    ],
  },

  {
    fileName: 'fuzileiro',
    name: 'Fuzileiro',
    description: 'Assault Rifleman. Agressivo. Entra em contato, sustenta fogo e empurra posições com recargas rápidas e controle de recuo.',
    skillOverrides: { Assault: 10, RecoilControl: 4, AimDrills: 4, Endurance: 3, Attention: 3, MagDrills: 6 },
    hideout: { Workbench: 1 },
    backupCount: 2,
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
      { id: 'MAG_AKM_30', qty: 2 },
    ],
  },

  {
    fileName: 'batedor',
    name: 'Batedor',
    description: 'Scout / Recon. Entra rápido, coleta informação, sai antes de ser detectado. Move-se em silêncio e identifica inimigos à distância.',
    skillOverrides: { Assault: 4, Endurance: 5, Perception: 8, Attention: 5, CovertMovement: 8, Search: 8 },
    hideout: { Security: 1 },
    backupCount: 2,
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
      { id: 'COMPASS',     qty: 1 },
      { id: 'AQUAMARI',    qty: 1 },
      { id: 'ETG_CHANGE',  qty: 1 },
    ],
  },

  {
    fileName: 'operadorFurtivo',
    name: 'Operador Furtivo',
    description: 'Stealth Operator. Especialista em furtividade — silêncio em movimento, percepção apurada e busca eficiente. Move-se sem ser ouvido.',
    skillOverrides: { Assault: 5, Endurance: 5, Perception: 6, CovertMovement: 8, Search: 5, MagDrills: 4 },
    hideout: { Generator: 1 },
    backupCount: 2,
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
    fileName: 'armeiro',
    name: 'Armeiro',
    description: 'Field Armorer. Mantém armas funcionando por mais tempo, corrige encravamentos e modifica equipamento em campo.',
    skillOverrides: { Assault: 4, Strength: 3, Intellect: 6, WeaponTreatment: 8, TroubleShooting: 4 },
    hideout: { Workbench: 1 },
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
    fileName: 'operadorTatico',
    name: 'Operador Tático',
    description: 'Special Forces. Generalista de elite. Sem fraquezas evidentes. Físico superior, mira rápida e adaptação a qualquer combate.',
    skillOverrides: { Assault: 5, AimDrills: 5, Strength: 10, Endurance: 7, Attention: 4, MagDrills: 4 },
    hideout: { RestSpace: 1 },
    backupCount: 2,
    primary: [
      { id: 'M4A1',             qty: 1 },
      { id: 'MAG_M4_30',        qty: 4 },
      { id: 'AMMO_556x45_M855', qty: 180 },
      { id: 'OKP7',             qty: 1 },
      { id: 'MP443',            qty: 1 },
      { id: 'MAG_MP443_18',     qty: 2 },
      { id: 'AMMO_9x19_PST',    qty: 60 },
      { id: 'MICH2001',         qty: 1 },
      { id: '6B23_1',           qty: 1 },
      { id: 'BLACKROCK',        qty: 1 },
      { id: 'TRIZIP',           qty: 1 },
      { id: 'IFAK',             qty: 1 },
      { id: 'SALEWA',           qty: 1 },
      { id: 'ANALGIN',          qty: 1 },
      { id: 'ARMY_BANDAGE',     qty: 2 },
      { id: 'MRE',              qty: 1 },
      { id: 'AQUAMARI',         qty: 1 },
    ],
    backup: backupKit('AK74N', 'MAG_AK_30', 'AMMO_545x39_PS', 90, 'MAKAROV', 'MAG_PM_8'),
    tema: [
      { id: 'ETG_CHANGE', qty: 1 },
    ],
  },

  {
    fileName: 'sobrevivencialista',
    name: 'Sobrevivencialista',
    description: 'Survivalist. Fica em raid por mais tempo que qualquer outro. Drena recursos devagar e resiste a efeitos negativos.',
    skillOverrides: { Shotgun: 3, Metabolism: 10, Vitality: 4, Immunity: 2, Health: 1, Perception: 3, Search: 4 },
    hideout: { WaterCollector: 1 },
    backupCount: 2,
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
      { id: 'TUSHONKA',  qty: 6 },
      { id: 'AQUAMARI',  qty: 4 },
      { id: 'AUGMENTIN', qty: 5 },
      { id: 'VASELINE',  qty: 4 },
      { id: 'AI2',       qty: 7 },
      { id: 'MULTITOOL', qty: 1 },
    ],
  },

  {
    fileName: 'saqueador',
    name: 'Saqueador',
    description: 'Scavenger. Esvazia containers em segundos, detecta loot à distância e identifica itens valiosos instantaneamente.',
    skillOverrides: { Assault: 2, Strength: 2, Attention: 10, Perception: 10, Intellect: 8, Memory: 5, Search: 10 },
    hideout: { Security: 1 },
    backupCount: 2,
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
      { id: 'DOCUMENTS_CASE', qty: 2 },
      { id: 'MULTITOOL',      qty: 1 },
      { id: 'SCREWS',         qty: 1 },
      { id: 'WIRES',          qty: 1 },
      { id: 'DUCT_TAPE',      qty: 1 },
      { id: 'ROUBLES',        qty: 200000 },
    ],
  },

  {
    fileName: 'gerenteDeOperacoes',
    name: 'Gerente de Operações',
    description: 'Operations Manager. Maximiza rendimento do hideout e progride skills mais rápido. Vantagem cumulativa, não imediata em raid.',
    skillOverrides: { Shotgun: 2, Strength: 4, Memory: 10, Intellect: 10, Charisma: 10, Crafting: 10, HideoutManagement: 10 },
    hideout: { Generator: 1, Heating: 1 },
    backupCount: 2,
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
      { id: 'TOOLSET',   qty: 2 },
      { id: 'CPU_FAN',   qty: 4 },
      { id: 'WIRES',     qty: 4 },
      { id: 'DUCT_TAPE', qty: 3 },
      { id: 'BOLTS',     qty: 1 },
      { id: 'SCREWS',    qty: 1 },
      { id: 'ROUBLES',   qty: 300000 },
    ],
  },
];

// ── Stack-aware Items[] builder ──────────────────────────────────────────────

function aggregateRecipes(p) {
  // Soma todas as recipes (baseline + tema + primary + backup × N) por anchorId
  const totals = new Map();
  function add(items, mult = 1) {
    for (const it of items) {
      totals.set(it.id, (totals.get(it.id) || 0) + it.qty * mult);
    }
  }
  add(BASELINE);
  add(p.tema);
  add(p.primary);
  add(p.backup, p.backupCount);
  return totals;
}

function buildItemsArray(p) {
  const totals = aggregateRecipes(p);
  const items = [];
  for (const [anchorId, totalQty] of totals.entries()) {
    const tpl = bsgIdOf(anchorId);
    const stackMax = stackMaxOf(tpl);

    if (stackMax >= totalQty) {
      // Cabe em 1 stack
      items.push({ Tpl: tpl, Count: totalQty });
    } else {
      // Divide em múltiplas entradas
      let remaining = totalQty;
      while (remaining > 0) {
        const chunk = Math.min(stackMax, remaining);
        items.push({ Tpl: tpl, Count: chunk });
        remaining -= chunk;
      }
    }
  }
  return items;
}

// ── Profile JSON builder ────────────────────────────────────────────────────

function buildProfileJson(p) {
  return {
    Enabled: true,
    // BaseProfile 8 = SPT Zero to Hero. Escolhido por começar com stash VAZIO,
    // dando espaço para o nosso loadout (primary + backup × 2 + tema + baseline).
    // Standard (0) traz itens iniciais que somavam com o nosso e causavam overflow.
    BaseProfile: 8,
    Name: p.name,
    Description: p.description,
    AllItemsExamined: false,
    MaxLevel: false,
    StartingLevel: null,
    StartingPrestigeLevel: null,
    MaxSkills: false,
    SkillOverrides: skillsFull(p.skillOverrides),
    ClearEquipment: false,
    ClearStash: false,
    SecureContainer: 0,
    AdditionalStartingItems: {
      Enabled: true,
      Items: buildItemsArray(p),
    },
    TradersLoyalty: tradersFull(),
    HideoutStartingLevels: hideoutFull(p.hideout),
  };
}

// ── Validation helpers ──────────────────────────────────────────────────────

// Multiplicadores: ver mods/RZCustomProfiles/backlog/002-custom-profiles/002-custom-profiles-00-multiplicadores.md
// 20 skills mortas em SPT 4.0.13 (globals.json com []) foram REMOVIDAS — não usar:
//   Combat:    SMG, LMG, HMG, Launcher, AttachedLauncher
//   Practical: Sniping, ProneMovement, FieldMedicine, FirstAid, WeaponModding,
//              AdvancedModding, NightOps, SilentOps, Lockpicking
//   Trading:   Freetrading, Auctions, Cleanoperations, Barter, Shadowconnections, Taskperformance
// Clamp ampliado de [0.25, 3.00] para [0.25, 5.00]: Immunity/LightVests/HeavyVests/DMR
// passaram de 3.00 → 3.75 (15/4 = 3.75 real, antes truncado).
const SKILL_MULTS = {
  // Ph — Physical
  Endurance: 1.00, Strength: 0.47, Vitality: 1.67, Health: 1.67,
  StressResistance: 0.88, Metabolism: 0.29, Immunity: 3.75,
  // M — Mental
  Perception: 0.88, Intellect: 0.68, Attention: 0.60, Charisma: 0.40, Memory: 0.50,
  // C — Combat
  Pistol: 3.00, Revolver: 0.71, Assault: 1.00, Shotgun: 2.50, Sniper: 1.50,
  DMR: 3.75, Throwing: 0.83, Melee: 1.88,
  RecoilControl: 1.00, AimDrills: 1.15, TroubleShooting: 2.50,
  // P — Practical
  Surgery: 1.25, CovertMovement: 0.94, Search: 0.43, MagDrills: 0.94,
  LightVests: 3.75, HeavyVests: 3.75, WeaponTreatment: 1.25,
  Crafting: 0.33, HideoutManagement: 0.39,
};

// Categoria por skill (alinhada com as abas do UI do EFT/SPT).
// Usada na validação de cobertura mínima (cada classe deve ter ≥ 1 ponto em cada categoria).
const SKILL_CATEGORIES = {
  Endurance: 'Ph', Strength: 'Ph', Vitality: 'Ph', Health: 'Ph',
  StressResistance: 'Ph', Metabolism: 'Ph', Immunity: 'Ph',
  Perception: 'M', Intellect: 'M', Attention: 'M', Charisma: 'M', Memory: 'M',
  Pistol: 'C', Revolver: 'C', Assault: 'C', Shotgun: 'C', Sniper: 'C',
  DMR: 'C', Throwing: 'C', Melee: 'C',
  RecoilControl: 'C', AimDrills: 'C', TroubleShooting: 'C',
  Surgery: 'P', CovertMovement: 'P', Search: 'P', MagDrills: 'P',
  LightVests: 'P', HeavyVests: 'P', WeaponTreatment: 'P',
  Crafting: 'P', HideoutManagement: 'P',
};

function weightedCost(overrides) {
  let cost = 0;
  for (const [k, v] of Object.entries(overrides)) {
    const m = SKILL_MULTS[k];
    if (m === undefined) throw new Error(`Multiplicador não encontrado: ${k}`);
    cost += v * m;
  }
  return cost;
}

function loadoutTotalRub(p) {
  const totals = aggregateRecipes(p);
  let sum = 0;
  for (const [anchorId, qty] of totals.entries()) {
    const r = ANCHOR[anchorId];
    if (!r) continue;
    const unit = (r.tags && r.tags.includes('Currency'))
      ? (r.basePrice || 1)
      : (r.avg24hPrice || 0);
    sum += unit * qty;
  }
  return Math.round(sum);
}

// Stash L1 do Standard = 10 colunas × 28 fileiras = 280 slots.
// Restrição rígida de design: loadout precisa caber em Stash:1 (sem auto-bump).
// Quando passa, a recipe da classe precisa ser cortada (ex: backupCount menor).
const STASH_SLOTS_L1 = 280;

function stashSlotsRequired(items) {
  // Cada entrada em Items[] já é UM stack (a regra de stack-aware do build
  // dividiu em N entries quando necessário). Logo, ocupa width × height células
  // independente do Count interno do stack.
  let total = 0;
  for (const it of items) {
    const m = itemMetaOf(it.Tpl);
    total += m.width * m.height;
  }
  return total;
}

function validateProfile(p, jsonItems) {
  const issues = [];

  // Skills — total ∈ [28, 32], skill ≤ 10, cobertura mínima Ph/M/C/P (002).
  const cost = weightedCost(p.skillOverrides);
  if (cost < 28 || cost > 32) {
    issues.push(`Custo ponderado fora de [28, 32]: ${cost.toFixed(2)}`);
  }
  for (const [k, v] of Object.entries(p.skillOverrides)) {
    if (v > 10) issues.push(`Skill ${k} = ${v} > 10`);
  }
  // Cobertura de categorias (regra 002)
  const categoriesCovered = new Set();
  for (const [k, v] of Object.entries(p.skillOverrides)) {
    if (v > 0) {
      const cat = SKILL_CATEGORIES[k];
      if (!cat) issues.push(`Skill ${k} sem categoria definida (provavelmente morta)`);
      else categoriesCovered.add(cat);
    }
  }
  const missing = ['Ph', 'M', 'C', 'P'].filter(c => !categoriesCovered.has(c));
  if (missing.length > 0) issues.push(`Categorias sem cobertura: ${missing.join(', ')}`);
  const skillCount = Object.values(p.skillOverrides).filter(v => v > 0).length;

  // Loadout — total ₽. Faixa relaxada após backupCount=2 (reduzido de 3 originalmente).
  // Após mudança BaseProfile 8 (Zero to Hero), stash inicial vazio comporta backup × 2.
  const total = loadoutTotalRub(p);
  if (total < 1500000 || total > 2050000) {
    issues.push(`Total ₽ fora de [1.5M, 2.05M]: ${total.toLocaleString('pt-BR')}`);
  }

  // Loadout — slots do stash. Restrição rígida: Stash:1 = 280 slots.
  // ATENÇÃO: BaseProfile 8 (Zero to Hero) começa com stash VAZIO, então toda
  // a capacidade está disponível para nosso loadout. Margem para packing real
  // estimada em ~85% (overflow esperado a partir de ~238 cells).
  const slots = stashSlotsRequired(jsonItems);
  if (slots > STASH_SLOTS_L1) {
    issues.push(`Slots ${slots} > ${STASH_SLOTS_L1} (Stash L1) — reduzir loadout`);
  }

  // Warning: >85% (238 cells) sinaliza margem apertada para packing real.
  const slotWarning = slots > Math.round(STASH_SLOTS_L1 * 0.85) && slots <= STASH_SLOTS_L1;

  return { cost: cost.toFixed(2), skillCount, total, slots, slotWarning, issues };
}

// ── Main ─────────────────────────────────────────────────────────────────────

const OUTPUT_DIR = path.join(MOD_ROOT, 'modded/profiles');
if (!fs.existsSync(OUTPUT_DIR)) fs.mkdirSync(OUTPUT_DIR, { recursive: true });

// 1ª passada: buildar tudo em memória + validar. NADA escrito ainda.
// Regra rígida: loadout precisa caber em Stash:1 (280 slots).
// Auto-bump removido por decisão de design.
const builds = PROFILES.map(p => {
  const json = buildProfileJson(p);
  const v = validateProfile(p, json.AdditionalStartingItems.Items);
  return { profile: p, json, validation: v };
});

// Se qualquer perfil falhar, aborta sem escrever nada.
const allIssues = builds.flatMap(b =>
  b.validation.issues.map(i => `[${b.profile.fileName}] ${i}`)
);

if (allIssues.length > 0) {
  console.log('\n⚠️  Issues encontradas (NADA escrito em disco):');
  for (const i of allIssues) console.log('  -', i);
  process.exit(1);
}

// 2ª passada: validação OK em todos — pode escrever.
const summary = [];
for (const b of builds) {
  const out = JSON.stringify(b.json, null, 2) + '\n';
  const filePath = path.join(OUTPUT_DIR, `${b.profile.fileName}.json`);
  fs.writeFileSync(filePath, out, { encoding: 'utf8' });

  // Confirmar UTF-8 sem BOM (paranoia — fs com 'utf8' não emite BOM, mas confirma)
  const buf = fs.readFileSync(filePath);
  if (buf[0] === 0xEF && buf[1] === 0xBB && buf[2] === 0xBF) {
    throw new Error(`BOM detectado em ${b.profile.fileName}.json — serializer falhou`);
  }

  summary.push({
    fileName: b.profile.fileName,
    name: b.profile.name,
    skillCost: b.validation.cost,
    skillCount: b.validation.skillCount,
    loadoutTotal: b.validation.total.toLocaleString('pt-BR'),
    itemEntries: b.json.AdditionalStartingItems.Items.length,
    slots: b.validation.slots,
    slotWarning: b.validation.slotWarning,
    stashLevel: b.json.HideoutStartingLevels.Stash,
    stashBumped: b.validation.stashBumped,
  });
}

// ── Report ──────────────────────────────────────────────────────────────────

console.log('\n=== 10 perfis gerados em modded/profiles/ ===\n');
console.log('Arquivo'.padEnd(22), 'Nome'.padEnd(24), 'Custo'.padEnd(8), 'Skills', 'Total ₽'.padEnd(14), 'Items[]'.padEnd(8), 'Slots', 'Stash');
console.log('-'.repeat(102));
for (const s of summary) {
  const slotsStr = s.slotWarning ? `${s.slots} ⚠️` : String(s.slots);
  console.log(
    `${s.fileName}.json`.padEnd(22),
    s.name.padEnd(24),
    s.skillCost.padEnd(8),
    String(s.skillCount).padEnd(6),
    s.loadoutTotal.padEnd(14),
    String(s.itemEntries).padEnd(8),
    String(slotsStr).padEnd(7),
    s.stashLevel
  );
}

const warnings = summary.filter(s => s.slotWarning);
if (warnings.length > 0) {
  console.log(`\n⚠️  ${warnings.length} classe(s) próximas do limite (>${Math.round(STASH_SLOTS_L1*0.85)}/${STASH_SLOTS_L1}). Margem para packing pode ficar apertada — validar em playtest.`);
}

console.log('\n✓ Validação OK — todas as 10 classes dentro dos limites de design.');

// ── Emit profiles-meta.json (viewer HTML) ────────────────────────────────────

function emitProfilesMeta() {
  const ITEMS_DB_PATH = path.join(MOD_ROOT, '../../tools/tarkov-itemdb/data/items.json');
  if (!fs.existsSync(ITEMS_DB_PATH)) {
    console.error(`\nAVISO: ${ITEMS_DB_PATH} não encontrado — profiles-meta.json não emitido.`);
    return;
  }
  const itemsDb = JSON.parse(fs.readFileSync(ITEMS_DB_PATH, 'utf8'));

  function resolveItem(anchorId, qty) {
    const bsgId = bsgIdOf(anchorId);
    const dbItem = itemsDb[bsgId];
    if (!dbItem) throw new Error(`bsgId ${bsgId} (anchor: ${anchorId}) não encontrado em items.json — atualize o DB`);
    // dims de ITEM_DATA (slot grid real do EFT); items.json.dims são físicos (incorretos pra slots)
    const meta = ITEM_DATA[bsgId] || {};
    return {
      id: anchorId,
      bsgId,
      qty,
      name: dbItem.name || null,
      shortName: dbItem.shortName || null,
      iconUrl: (dbItem.image && dbItem.image.icon) || null,
      dims: { width: meta.width || 1, height: meta.height || 1 },
      priceRub: (dbItem.consolidated && dbItem.consolidated.priceFleaCanonical != null)
        ? dbItem.consolidated.priceFleaCanonical
        : null,
      categoryName: (dbItem.category && dbItem.category.name) || null,
      types: Array.isArray(dbItem.types) ? dbItem.types : [],
      weight: (dbItem.dims && dbItem.dims.weight != null) ? dbItem.dims.weight : null,
    };
  }

  const out = {
    _generatedBy: 'mods/RZCustomProfiles/scripts/build-profile-jsons.js',
    _doNotEdit: true,
    generatedAt: new Date().toISOString(),
    profiles: {},
  };

  for (const p of PROFILES) {
    const skillsByCategory = { Ph: [], M: [], C: [], P: [] };
    for (const [name, level] of Object.entries(p.skillOverrides)) {
      if (level === 0) continue;
      const cat = SKILL_CATEGORIES[name];
      const cost = parseFloat((level * SKILL_MULTS[name]).toFixed(2));
      skillsByCategory[cat].push({ name, level, cost });
    }

    const loadout = {
      baseline:    BASELINE.map(it => resolveItem(it.id, it.qty)),
      tema:        p.tema.map(it => resolveItem(it.id, it.qty)),
      primary:     p.primary.map(it => resolveItem(it.id, it.qty)),
      backup:      p.backup.map(it => resolveItem(it.id, it.qty)),
      backupCount: p.backupCount,
    };

    out.profiles[p.fileName] = {
      fileName:    p.fileName,
      name:        p.name,
      description: p.description,
      baseProfile: 8,
      totalCost:   parseFloat(weightedCost(p.skillOverrides).toFixed(2)),
      totalRub:    loadoutTotalRub(p),
      skillsByCategory,
      hideout: {
        thematic:   p.hideout,
        stashLevel: 1,
      },
      loadout,
    };
  }

  const outPath = path.join(MOD_ROOT, '../../tools/tarkov-itemdb/data/profiles-meta.json');
  fs.writeFileSync(outPath, JSON.stringify(out, null, 2) + '\n', { encoding: 'utf8' });
  console.log(`\n✓ profiles-meta.json emitido: ${outPath}`);
}

emitProfilesMeta();
