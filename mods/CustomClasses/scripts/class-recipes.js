'use strict';
/**
 * class-recipes.js — dados das 10 classes (portados de mods/RZCustomProfiles/scripts/build-profile-jsons.js).
 * Exporta uma função (BASELINE, backupKit) => PROFILES[] para o build-class-jsons.js consumir.
 * skillOverrides e hideout usam os mesmos nomes (devem casar com SkillTypes / HideoutAreas do SPT).
 */
module.exports = (BASELINE, backupKit) => [
  {
    fileName: 'medicoDeCombate', name: 'Médico de Combate',
    description: {
      en: 'Combat Medic. Survives wounds that would kill others. Treats severe damage fast and stays operational after taking hits.',
      pt: 'Médico de combate. Sobrevive a ferimentos que matariam outros. Trata dano severo rápido e continua operacional após levar dano.',
    },
    skillOverrides: { Assault: 5, Vitality: 5, Health: 3, StressResistance: 4, Attention: 2, Surgery: 7 },
    hideout: { MedStation: 1 }, backupCount: 2,
    primary: [
      { id: 'AKM', qty: 1 }, { id: 'MAG_AKM_30', qty: 4 }, { id: 'AMMO_762x39_PS', qty: 180 },
      { id: 'MAKAROV', qty: 1 }, { id: 'MAG_PM_8', qty: 2 }, { id: 'AMMO_9x18_PST', qty: 60 },
      { id: 'LZSH', qty: 1 }, { id: '6B23_1', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'MBSS', qty: 1 },
      { id: 'IFAK', qty: 1 }, { id: 'SALEWA', qty: 1 }, { id: 'ANALGIN', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 2 }, { id: 'MRE', qty: 1 }, { id: 'AQUAMARI', qty: 1 },
    ],
    backup: backupKit('AKM', 'MAG_AKM_30', 'AMMO_762x39_PS', 120, 'MAKAROV', 'MAG_PM_8'),
    tema: [{ id: 'IFAK', qty: 2 }, { id: 'SURV12', qty: 1 }, { id: 'CALOK_B', qty: 1 }],
  },
  {
    fileName: 'cacador', name: 'Caçador',
    description: {
      en: 'Sniper. Patient and precise. Owns elevated positions, minimises movement and eliminates before being spotted.',
      pt: 'Sniper. Paciente e preciso. Domina posições elevadas, minimiza movimento e elimina antes de ser detectado.',
    },
    skillOverrides: { Sniper: 8, Endurance: 5, Perception: 6, Attention: 4, CovertMovement: 5 },
    hideout: { Heating: 1 }, backupCount: 2,
    // exemplo p/ teste: militar/camo (upper compartilhado USEC Predator; lower por facção)
    outfit: {
      usec: { upper: '64ef3fa81a5f313cb144bf89', lower: '5e9dccd686f774343b592592' }, // USEC Predator / USEC Deep Recon
      bear: { upper: '64ef3fa81a5f313cb144bf89', lower: '5d1f647186f7744bce0ef70c' }, // USEC Predator / BEAR Gorka SSO
    },
    primary: [
      { id: 'SV98', qty: 1 }, { id: 'MAG_SV98_10', qty: 4 }, { id: 'AMMO_762x54R_LPS', qty: 80 },
      { id: 'PSO1', qty: 1 }, { id: 'BIPOD_HARRIS', qty: 1 },
      { id: 'MAKAROV', qty: 1 }, { id: 'MAG_PM_8', qty: 2 }, { id: 'AMMO_9x18_PST', qty: 60 },
      { id: 'LZSH', qty: 1 }, { id: '6B2', qty: 1 }, { id: 'TRIPLE_BANDOLIER', qty: 1 }, { id: 'PILGRIM', qty: 1 },
      { id: 'IFAK', qty: 1 }, { id: 'SALEWA', qty: 1 }, { id: 'ANALGIN', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 2 }, { id: 'MRE', qty: 1 }, { id: 'AQUAMARI', qty: 1 },
    ],
    backup: [
      { id: 'MOSIN_SNIPER', qty: 1 }, { id: 'AMMO_762x54R_LPS', qty: 60 }, { id: 'MAKAROV', qty: 1 }, { id: 'MAG_PM_8', qty: 2 },
      { id: 'PACA', qty: 1 }, { id: 'SSH68', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'SCAV_BACKPACK', qty: 1 }, { id: 'IFAK', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 1 }, { id: 'SQUASH', qty: 1 },
    ],
    tema: [{ id: 'COMPASS', qty: 1 }, { id: 'VASELINE', qty: 1 }, { id: 'TUSHONKA', qty: 3 }, { id: 'AUGMENTIN', qty: 3 }],
  },
  {
    fileName: 'fuzileiro', name: 'Fuzileiro',
    description: {
      en: 'Assault Rifleman. Aggressive. Closes the distance, sustains fire and pushes positions with fast reloads and recoil control.',
      pt: 'Fuzileiro de assalto. Agressivo. Entra em contato, sustenta fogo e empurra posições com recargas rápidas e controle de recuo.',
    },
    skillOverrides: { Assault: 10, RecoilControl: 4, AimDrills: 4, Endurance: 3, Attention: 3, MagDrills: 6 },
    hideout: { Workbench: 1 }, backupCount: 2,
    primary: [
      { id: 'AKM', qty: 1 }, { id: 'MAG_AKM_30', qty: 4 }, { id: 'AMMO_762x39_BP', qty: 180 }, { id: 'OKP7', qty: 1 },
      { id: 'MP443', qty: 1 }, { id: 'MAG_MP443_18', qty: 2 }, { id: 'AMMO_9x19_PST', qty: 60 },
      { id: 'LZSH', qty: 1 }, { id: '6B23_1', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'TRIZIP', qty: 1 },
      { id: 'IFAK', qty: 1 }, { id: 'SALEWA', qty: 1 }, { id: 'ANALGIN', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 2 }, { id: 'MRE', qty: 1 }, { id: 'AQUAMARI', qty: 1 },
    ],
    backup: backupKit('AKM', 'MAG_AKM_30', 'AMMO_762x39_PS', 120, 'MAKAROV', 'MAG_PM_8'),
    tema: [{ id: 'MAG_AKM_30', qty: 2 }],
  },
  {
    fileName: 'batedor', name: 'Batedor',
    description: {
      en: 'Scout / Recon. Moves in fast, gathers intel and leaves before being detected. Moves silently and spots enemies at range.',
      pt: 'Batedor / Recon. Entra rápido, coleta informação e sai antes de ser detectado. Move-se em silêncio e identifica inimigos à distância.',
    },
    skillOverrides: { Assault: 4, Endurance: 5, Perception: 8, Attention: 5, CovertMovement: 8, Search: 8 },
    hideout: { Security: 1 }, backupCount: 2,
    primary: [
      { id: 'AKS74U', qty: 1 }, { id: 'MAG_AK_30', qty: 4 }, { id: 'AMMO_545x39_BS', qty: 120 },
      { id: 'MAKAROV', qty: 1 }, { id: 'MAG_PM_8', qty: 2 }, { id: 'AMMO_9x18_PST', qty: 60 },
      { id: 'TAC_HELMET', qty: 1 }, { id: '6B2', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'PARATUS', qty: 1 },
      { id: 'IFAK', qty: 1 }, { id: 'SALEWA', qty: 1 }, { id: 'ANALGIN', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 2 }, { id: 'MRE', qty: 1 }, { id: 'AQUAMARI', qty: 1 },
    ],
    backup: backupKit('AKS74U', 'MAG_AK_30', 'AMMO_545x39_PS', 120, 'MAKAROV', 'MAG_PM_8'),
    tema: [{ id: 'COMPASS', qty: 1 }, { id: 'AQUAMARI', qty: 1 }, { id: 'ETG_CHANGE', qty: 1 }],
  },
  {
    fileName: 'operadorFurtivo', name: 'Operador Furtivo',
    description: {
      en: 'Stealth Operator. Stealth specialist — silent on the move, sharp perception and efficient searching. Moves unheard.',
      pt: 'Operador furtivo. Especialista em furtividade — silêncio em movimento, percepção apurada e busca eficiente. Move-se sem ser ouvido.',
    },
    skillOverrides: { Assault: 5, Endurance: 5, Perception: 6, CovertMovement: 8, Search: 5, MagDrills: 4 },
    hideout: { Generator: 1 }, backupCount: 2,
    // TESTE skin de MOD "aparência direta" (AllTheClothes): upper = top_boss_tagilla_nohead (BodyPart=Body, Body=null)
    // valida o caminho endurecido do OutfitBuilder. lower vanilla por facção. (soft-dep: requer AllTheClothes)
    outfit: {
      usec: { upper: '66a25a3af12f29d8a2599527', lower: '66589cceb00aec5c0278573c' }, // [MOD] Tagilla top / Outdoor Tactical
      bear: { upper: '66a25a3af12f29d8a2599527', lower: '619b99ad604fcc392676806c' }, // [MOD] Tagilla top / BEAR Recon
    },
    primary: [
      { id: 'AKMS', qty: 1 }, { id: 'MAG_AKM_30', qty: 4 }, { id: 'AMMO_762x39_US', qty: 180 }, { id: 'PBS1', qty: 1 },
      { id: 'MAKAROV', qty: 1 }, { id: 'MAG_PM_8', qty: 2 }, { id: 'AMMO_9x18_PST', qty: 60 },
      { id: 'LZSH', qty: 1 }, { id: '6B2', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'MBSS', qty: 1 }, { id: 'PNV10T', qty: 1 },
      { id: 'IFAK', qty: 1 }, { id: 'SALEWA', qty: 1 }, { id: 'ANALGIN', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 2 }, { id: 'MRE', qty: 1 }, { id: 'AQUAMARI', qty: 1 },
    ],
    backup: backupKit('AKMS', 'MAG_AKM_30', 'AMMO_762x39_PS', 120, 'MAKAROV', 'MAG_PM_8'),
    tema: [{ id: 'PNV10T', qty: 1 }, { id: 'PBS1', qty: 1 }, { id: 'IFAK', qty: 1 }, { id: 'TUSHONKA', qty: 3 }, { id: 'AUGMENTIN', qty: 1 }, { id: 'AMMO_762x39_US', qty: 60 }],
  },
  {
    fileName: 'armeiro', name: 'Armeiro',
    description: {
      en: 'Field Armorer. Keeps weapons running longer, clears jams and modifies gear in the field.',
      pt: 'Armeiro de campo. Mantém armas funcionando por mais tempo, corrige encravamentos e modifica equipamento em campo.',
    },
    skillOverrides: { Assault: 4, Strength: 3, Intellect: 6, WeaponTreatment: 8, TroubleShooting: 4 },
    hideout: { Workbench: 1 }, backupCount: 2,
    primary: [
      { id: 'AKM', qty: 1 }, { id: 'MAG_AKM_30', qty: 4 }, { id: 'AMMO_762x39_PS', qty: 120 },
      { id: 'MAKAROV', qty: 1 }, { id: 'MAG_PM_8', qty: 2 }, { id: 'AMMO_9x18_PST', qty: 60 },
      { id: 'TAC_HELMET', qty: 1 }, { id: '6B2', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'MBSS', qty: 1 },
      { id: 'IFAK', qty: 1 }, { id: 'SALEWA', qty: 1 }, { id: 'ANALGIN', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 2 }, { id: 'MRE', qty: 1 }, { id: 'AQUAMARI', qty: 1 },
    ],
    backup: backupKit('AKM', 'MAG_AKM_30', 'AMMO_762x39_PS', 90, 'MAKAROV', 'MAG_PM_8'),
    tema: [{ id: 'WEAPON_REPAIR_KIT', qty: 1 }, { id: 'TOOLSET', qty: 1 }, { id: 'WD40', qty: 1 }, { id: 'MULTITOOL', qty: 1 }, { id: 'BOLTS', qty: 1 }],
  },
  {
    fileName: 'operadorTatico', name: 'Operador Tático',
    description: {
      en: 'Special Forces. Elite all-rounder with no obvious weaknesses. Superior fitness, fast aim and adapts to any fight.',
      pt: 'Forças especiais. Generalista de elite, sem fraquezas evidentes. Físico superior, mira rápida e adaptação a qualquer combate.',
    },
    skillOverrides: { Assault: 5, AimDrills: 5, Strength: 10, Endurance: 7, Attention: 4, MagDrills: 4 },
    hideout: { RestSpace: 1 }, backupCount: 2,
    primary: [
      { id: 'M4A1', qty: 1 }, { id: 'MAG_M4_30', qty: 4 }, { id: 'AMMO_556x45_M855', qty: 180 }, { id: 'OKP7', qty: 1 },
      { id: 'MP443', qty: 1 }, { id: 'MAG_MP443_18', qty: 2 }, { id: 'AMMO_9x19_PST', qty: 60 },
      { id: 'MICH2001', qty: 1 }, { id: '6B23_1', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'TRIZIP', qty: 1 },
      { id: 'IFAK', qty: 1 }, { id: 'SALEWA', qty: 1 }, { id: 'ANALGIN', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 2 }, { id: 'MRE', qty: 1 }, { id: 'AQUAMARI', qty: 1 },
    ],
    backup: backupKit('AK74N', 'MAG_AK_30', 'AMMO_545x39_PS', 90, 'MAKAROV', 'MAG_PM_8'),
    tema: [{ id: 'ETG_CHANGE', qty: 1 }],
  },
  {
    fileName: 'sobrevivencialista', name: 'Sobrevivencialista',
    description: {
      en: 'Survivalist. Stays in raid longer than anyone else. Drains resources slowly and resists negative effects.',
      pt: 'Sobrevivencialista. Fica em raid por mais tempo que qualquer outro. Drena recursos devagar e resiste a efeitos negativos.',
    },
    skillOverrides: { Shotgun: 3, Metabolism: 10, Vitality: 4, Immunity: 2, Health: 1, Perception: 3, Search: 4 },
    hideout: { WaterCollector: 1 }, backupCount: 2,
    primary: [
      { id: 'SAIGA12', qty: 1 }, { id: 'AMMO_12_70_MAG', qty: 30 },
      { id: 'MAKAROV', qty: 1 }, { id: 'MAG_PM_8', qty: 2 }, { id: 'AMMO_9x18_PST', qty: 60 },
      { id: 'TAC_HELMET', qty: 1 }, { id: '6B2', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'PILGRIM', qty: 1 },
      { id: 'IFAK', qty: 1 }, { id: 'SALEWA', qty: 1 }, { id: 'ANALGIN', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 2 }, { id: 'MRE', qty: 1 }, { id: 'AQUAMARI', qty: 1 },
    ],
    backup: [
      { id: 'TOZ106', qty: 1 }, { id: 'MAG_TOZ106_4', qty: 2 }, { id: 'AMMO_20_70_BUCK', qty: 1 }, { id: 'MAKAROV', qty: 1 }, { id: 'MAG_PM_8', qty: 2 },
      { id: 'PACA', qty: 1 }, { id: 'SSH68', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'SCAV_BACKPACK', qty: 1 }, { id: 'IFAK', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 1 }, { id: 'SQUASH', qty: 1 },
    ],
    tema: [{ id: 'TUSHONKA', qty: 6 }, { id: 'AQUAMARI', qty: 4 }, { id: 'AUGMENTIN', qty: 5 }, { id: 'VASELINE', qty: 4 }, { id: 'AI2', qty: 7 }, { id: 'MULTITOOL', qty: 1 }],
  },
  {
    fileName: 'saqueador', name: 'Saqueador',
    description: {
      en: 'Scavenger. Empties containers in seconds, detects loot at range and instantly identifies valuable items.',
      pt: 'Saqueador. Esvazia containers em segundos, detecta loot à distância e identifica itens valiosos instantaneamente.',
    },
    skillOverrides: { Assault: 2, Strength: 2, Attention: 10, Perception: 10, Intellect: 8, Memory: 5, Search: 10 },
    hideout: { Security: 1 }, backupCount: 2,
    // exemplo p/ teste: casual/scav (Adik tracksuit; lower por facção)
    outfit: {
      usec: { upper: '5cdea42e7d6c8b0474535dad', lower: '685d092aed4e253164064e05' }, // Adik tracksuit / USEC Day Off
      bear: { upper: '5cdea42e7d6c8b0474535dad', lower: '5df8e72186f7741263108806' }, // Adik tracksuit / BEAR Oldschool
    },
    primary: [
      { id: 'SAIGA9', qty: 1 }, { id: 'MAG_SAIGA9_10', qty: 4 }, { id: 'AMMO_9x19_PST', qty: 80 },
      { id: 'MAKAROV', qty: 1 }, { id: 'MAG_PM_8', qty: 2 }, { id: 'AMMO_9x18_PST', qty: 60 },
      { id: 'TAC_HELMET', qty: 1 }, { id: '6B2', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'PILGRIM', qty: 1 },
      { id: 'IFAK', qty: 1 }, { id: 'SALEWA', qty: 1 }, { id: 'ANALGIN', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 2 }, { id: 'MRE', qty: 1 }, { id: 'AQUAMARI', qty: 1 },
    ],
    backup: [
      { id: 'TOZ106', qty: 1 }, { id: 'MAG_TOZ106_4', qty: 1 }, { id: 'AMMO_20_70_BUCK', qty: 1 }, { id: 'MAKAROV', qty: 1 }, { id: 'MAG_PM_8', qty: 2 },
      { id: 'PACA', qty: 1 }, { id: 'SSH68', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'PARATUS', qty: 1 }, { id: 'IFAK', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 1 }, { id: 'SQUASH', qty: 1 },
    ],
    tema: [{ id: 'DOCUMENTS_CASE', qty: 2 }, { id: 'MULTITOOL', qty: 1 }, { id: 'SCREWS', qty: 1 }, { id: 'WIRES', qty: 1 }, { id: 'DUCT_TAPE', qty: 1 }, { id: 'ROUBLES', qty: 200000 }],
  },
  {
    fileName: 'gerenteDeOperacoes', name: 'Gerente de Operações',
    description: {
      en: 'Operations Manager. Maximises hideout output and levels skills faster. A cumulative edge, not an in-raid one.',
      pt: 'Gerente de operações. Maximiza o rendimento do hideout e progride skills mais rápido. Vantagem cumulativa, não imediata em raid.',
    },
    skillOverrides: { Shotgun: 2, Strength: 4, Memory: 10, Intellect: 10, Charisma: 10, Crafting: 10, HideoutManagement: 10 },
    hideout: { Generator: 1, Heating: 1 }, backupCount: 2,
    primary: [
      { id: 'SAIGA12', qty: 1 }, { id: 'AMMO_12_70_MAG', qty: 20 },
      { id: 'MAKAROV', qty: 1 }, { id: 'MAG_PM_8', qty: 2 }, { id: 'AMMO_9x18_PST', qty: 60 },
      { id: 'TAC_HELMET', qty: 1 }, { id: '6B2', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'MBSS', qty: 1 },
      { id: 'IFAK', qty: 1 }, { id: 'SALEWA', qty: 1 }, { id: 'ANALGIN', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 2 }, { id: 'MRE', qty: 1 }, { id: 'AQUAMARI', qty: 1 },
    ],
    backup: [
      { id: 'TOZ106', qty: 1 }, { id: 'MAG_TOZ106_4', qty: 1 }, { id: 'AMMO_20_70_BUCK', qty: 1 }, { id: 'MAKAROV', qty: 1 }, { id: 'MAG_PM_8', qty: 2 },
      { id: 'PACA', qty: 1 }, { id: 'SSH68', qty: 1 }, { id: 'BLACKROCK', qty: 1 }, { id: 'SCAV_BACKPACK', qty: 1 }, { id: 'IFAK', qty: 1 }, { id: 'ARMY_BANDAGE', qty: 1 }, { id: 'SQUASH', qty: 1 },
    ],
    tema: [{ id: 'TOOLSET', qty: 2 }, { id: 'CPU_FAN', qty: 4 }, { id: 'WIRES', qty: 4 }, { id: 'DUCT_TAPE', qty: 3 }, { id: 'BOLTS', qty: 1 }, { id: 'SCREWS', qty: 1 }, { id: 'ROUBLES', qty: 300000 }],
  },
  // item 016 — Peladão (PLACEHOLDER: skin/itens/descrição a revisar). Base SPT Zero to Hero; sem skills nem multiplicadores.
  {
    fileName: 'peladao', name: 'Peladão',
    description: {
      en: 'The Streaker. Who needs armor when you have confidence? Showed up to the raid the way he came into the world. (placeholder — review skin/items)',
      pt: 'O Peladão. Quem precisa de armadura quando se tem confiança? Apareceu na raid do jeitinho que veio ao mundo. (placeholder — revisar skin/itens)',
    },
    skillOverrides: {}, hideout: {}, backupCount: 0, noBaseline: true,   // 100% pelado: sem skills e sem itens
    // skin "leve" placeholder (camisa havaiana) — revisar depois.
    outfit: {
      usec: { upper: '6847e663f43abfdda205835a', lower: '66589cceb00aec5c0278573c' }, // Blue Hawaii shirt / Outdoor Tactical
      bear: { upper: '6847e76f3f4cd20a97097a93', lower: '6658a1d54de4820934746dd4' }, // Green Hawaii shirt / BEAR Centurion
    },
    primary: [], backup: [], tema: [],
  },
];
