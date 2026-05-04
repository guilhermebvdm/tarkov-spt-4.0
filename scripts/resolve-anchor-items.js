#!/usr/bin/env node
/**
 * resolve-anchor-items.js (one-shot — delete after loadouts are stable)
 *
 * Resolves each anchor query against the local PVE cache, picks the
 * best match, and writes full records (incl. image URLs) to
 *   mods/RZCustomProfiles/backlog/anchor-items.json
 *
 * Match priority:
 *   1. shortName === q (case-insensitive exact)
 *   2. name === q (exact)
 *   3. shortName starts-with q
 *   4. name contains q
 *   5. shortName contains q
 *
 * Ambiguous matches (multiple top-tier hits) are reported on stderr.
 * Missing items are reported and skipped.
 */

'use strict';

const fs   = require('fs');
const path = require('path');

const ROOT  = path.resolve(__dirname, '..');
const CACHE = path.join(__dirname, 'cache', 'tarkov-pve-items.json');
const OUT   = path.join(ROOT, 'mods/RZCustomProfiles/backlog/anchor-items.json');

// ── Anchor definitions ──────────────────────────────────────────────────────
// id → internal name we'll use in loadout tables and code
// q  → search query (matched against shortName / name)

const ANCHORS = [
  // Meds
  { id: 'WEAPON_REPAIR_KIT', q: 'Weapon repair kit' },
  { id: 'IFAK',              q: 'IFAK' },
  { id: 'SALEWA',            q: 'Salewa' },
  { id: 'AI2',               q: 'AI-2' },
  { id: 'CAR_FIRST_AID',     q: 'Car first aid' },
  { id: 'SURV12',            q: 'Surv12' },
  { id: 'GRIZZLY',           q: 'Grizzly' },
  { id: 'CMS',               q: 'CMS surgical' },
  { id: 'CALOK_B',           q: 'CALOK-B' },
  { id: 'ESMARCH',           q: 'Esmarch' },
  { id: 'CAT_TOURNIQUET',    q: 'CAT hemostatic' },
  { id: 'HEMOSTOP',          q: 'Hemostatic drug' },
  { id: 'ARMY_BANDAGE',      q: 'Army bandage' },
  { id: 'ALUMINUM_SPLINT',   q: 'Aluminum splint' },
  { id: 'ANALGIN',           q: 'Analgin' },
  { id: 'PROPITAL',          q: 'Propital' },
  { id: 'ETG_CHANGE',        q: 'eTG-change' },
  { id: 'MORPHINE',          q: 'Morphine' },
  { id: 'VASELINE',          q: 'Vaseline' },
  { id: 'AUGMENTIN',         q: 'Augmentin' },

  // Weapons (use full canonical names to disambiguate from parts)
  { id: 'AKM',               q: 'Kalashnikov AKM 7.62x39 assault rifle' },
  { id: 'AK74N',             q: 'Kalashnikov AK-74N 5.45x39 assault rifle' },
  { id: 'AKMS',              q: 'Kalashnikov AKMS 7.62x39 assault rifle' },
  { id: 'AKS74U',            q: 'Kalashnikov AKS-74U 5.45x39 assault rifle' },
  { id: 'M4A1',              q: 'Colt M4A1 5.56x45 assault rifle' },
  { id: 'MOSIN_INFANTRY',    q: 'Mosin 7.62x54R bolt-action rifle (Infantry)' },
  { id: 'SV98',              q: 'SV-98 7.62x54R bolt-action sniper rifle' },
  { id: 'TOZ106',            q: 'TOZ-106 20ga bolt-action shotgun' },
  { id: 'SAIGA12',           q: 'Saiga-12K ver.10 12ga semi-automatic shotgun' },
  { id: 'SAIGA9',            q: 'Saiga-9 9x19 carbine' },
  { id: 'MAKAROV',           q: 'Makarov PM 9x18PM pistol' },
  { id: 'MP443',             q: 'Yarygin MP-443 Grach 9x19 pistol' },
  { id: 'MP153',             q: 'MP-153 12ga semi-automatic shotgun' },

  // Armor
  { id: 'PACA',              q: 'PACA Soft Armor' },
  { id: '6B2',               q: '6B2' },
  { id: 'KIRASA',            q: 'Kirasa-N' },
  { id: '6B23_1',            q: '6B23-1' },

  // Helmets
  { id: 'SSH68',             q: 'SSh-68 steel helmet' },
  { id: 'TAC_HELMET',        q: 'Tac-Kek FAST MT' },
  { id: 'LZSH',              q: 'LShZ' },
  { id: 'MICH2001',          q: 'TC-2001' },

  // Rigs
  { id: 'BLACKROCK',         q: 'BlackRock chest rig' },
  { id: 'TRIPLE_BANDOLIER',  q: 'Wartech TV-110' },

  // Backpacks
  { id: 'TRIZIP',            q: 'Camelbak Tri-Zip assault backpack (Foliage)' },
  { id: 'PILGRIM',           q: 'Pilgrim tourist backpack' },
  { id: 'MBSS',              q: 'Flyye MBSS backpack' },
  { id: 'PARATUS',           q: 'Paratus' },
  { id: 'SCAV_BACKPACK',     q: 'Scav Backpack' },
  { id: '6SH118',            q: '6Sh118 raid backpack' },
  { id: 'LBT_2670',          q: 'LBT-2670' },

  // Optics
  { id: 'PSO1',              q: 'BelOMO PSO-1 4x24 scope' },
  { id: 'EOTECH553',         q: 'EOTech 553' },
  { id: 'OKP7',              q: 'OKP-7' },
  { id: 'PK06',              q: 'PK-06' },

  // Suppressors / NVG
  { id: 'PBS1',              q: 'PBS-1' },
  { id: 'PNV10T',            q: 'PNV-10T night vision' },
  { id: 'BIPOD_HARRIS',      q: 'Harris bipod' },

  // Specialty / tools
  { id: 'TOOLSET',           q: 'Toolset' },
  { id: 'DOCUMENTS_CASE',    q: 'Documents case' },
  { id: 'PISTOL_CASE',       q: 'Pistol case' },
  { id: 'WD40',              q: 'WD-40 (100ml)' },
  { id: 'MULTITOOL',         q: 'Leatherman' },
  { id: 'COMPASS',           q: 'EYE MK.2 professional hand-held compass' },
  { id: 'CUSTOMS_MAP',       q: 'Customs plan map' },
  { id: 'WOODS_MAP',         q: 'Woods plan map' },
  { id: 'INTERCHANGE_MAP',   q: 'Interchange plan map' },

  // Food/drink
  { id: 'MRE',               q: 'MRE' },
  { id: 'TUSHONKA',          q: 'Can of beef stew (Large)' },
  { id: 'CRACKERS',          q: 'Crackers' },
  { id: 'AQUAMARI',          q: 'Aquamari' },
  { id: 'VITA_JUICE',        q: 'Vita juice' },
  { id: 'SQUASH',            q: 'Pack of apple juice' },

  // Materials
  { id: 'BAYONET',           q: '6Kh5 Bayonet' },
  { id: 'DUCT_TAPE',         q: 'Duct tape' },
  { id: 'WIRES',             q: 'Wires' },
  { id: 'CPU_FAN',           q: 'CPU fan' },
  { id: 'BOLTS',             q: 'Bolts' },
  { id: 'SCREWS',            q: 'Screws' },
  { id: 'ROUBLES',           q: 'Roubles' },

  // Magazines
  { id: 'MAG_AK_30',         q: 'AK-12 5.45x39 30-round magazine' },
  { id: 'MAG_AKM_30',        q: "AK 7.62x39 30-round magazine (issued '55 or later)" },
  { id: 'MAG_M4_30',         q: 'AR-15 5.56x45 Magpul PMAG 30 GEN M3' },
  { id: 'MAG_SV98_10',       q: 'SV-98 7.62x54R 10-round magazine' },
  { id: 'MAG_SAIGA9_10',     q: 'Saiga-9 9x19 sb.7 10-round magazine' },
  { id: 'MAG_TOZ106_4',      q: 'TOZ-106 20ga MTs 20-01 Sb.3 4-shot magazine' },
  { id: 'MAG_PM_8',          q: 'PM 9x18PM 90-93 8-round magazine' },
  { id: 'MAG_MP443_18',      q: 'MP-443 Grach 9x19 18-round magazine' },

  // Ammo
  { id: 'AMMO_762x39_PS',    q: '7.62x39mm PS gzh' },
  { id: 'AMMO_762x39_BP',    q: '7.62x39mm BP gzh' },
  { id: 'AMMO_762x39_US',    q: '7.62x39mm US gzh' },
  { id: 'AMMO_545x39_PS',    q: '5.45x39mm PS gs' },
  { id: 'AMMO_545x39_BS',    q: '5.45x39mm BS gs' },
  { id: 'AMMO_556x45_M855',  q: '5.56x45mm M855' },
  { id: 'AMMO_762x54R_LPS',  q: '7.62x54mm R LPS gzh' },
  { id: 'AMMO_762x54R_SNB',  q: '7.62x54mm R SNB gzh' },
  { id: 'AMMO_12_70_MAG',    q: '12/70 7mm buckshot' },
  { id: 'AMMO_20_70_BUCK',   q: '20/70 7.5mm buckshot ammo pack (25 pcs)' },
  { id: 'AMMO_9x18_PST',     q: '9x18mm PM PSt gzh' },
  { id: 'AMMO_9x19_PST',     q: '9x19mm Pst gzh' },
  { id: 'AMMO_9x19_PSO',     q: '9x19mm PSO gzh' },
];

// ── Match logic ─────────────────────────────────────────────────────────────

function score(item, q) {
  const Q = q.toLowerCase();
  const n = (item.name      || '').toLowerCase();
  const s = (item.shortName || '').toLowerCase();
  if (s === Q) return 100;
  if (n === Q) return 90;
  if (s.startsWith(Q)) return 80;
  if (n.startsWith(Q)) return 70;
  if (n.includes(Q))   return 60;
  if (s.includes(Q))   return 50;
  return 0;
}

function resolve(items, anchor) {
  const scored = items
    .map((it) => ({ it, sc: score(it, anchor.q) }))
    .filter((x) => x.sc > 0)
    .sort((a, b) => b.sc - a.sc);

  if (scored.length === 0) {
    return { ok: false, reason: 'no match' };
  }

  // Top tier = same score as the best match
  const topScore = scored[0].sc;
  const top = scored.filter((x) => x.sc === topScore);

  if (top.length > 1) {
    // Ambiguous — return first but flag
    return {
      ok: true,
      ambiguous: true,
      pick: top[0].it,
      candidates: top.map((x) => ({
        name: x.it.name,
        shortName: x.it.shortName,
        bsgId: x.it.bsgId,
      })),
    };
  }

  return { ok: true, ambiguous: false, pick: top[0].it };
}

// ── Main ────────────────────────────────────────────────────────────────────

if (!fs.existsSync(CACHE)) {
  console.error(`Cache not found: ${CACHE}\nRun fetch-tarkov-prices.js first.`);
  process.exit(1);
}

const items = JSON.parse(fs.readFileSync(CACHE, 'utf8'));
console.error(`Loaded ${items.length} items from cache.`);

const out  = {};
const miss = [];
const amb  = [];

for (const anchor of ANCHORS) {
  const r = resolve(items, anchor);
  if (!r.ok) {
    miss.push(anchor);
    continue;
  }
  if (r.ambiguous) amb.push({ id: anchor.id, q: anchor.q, candidates: r.candidates });

  const it = r.pick;
  out[anchor.id] = {
    id:         anchor.id,
    query:      anchor.q,
    name:       it.name,
    shortName:  it.shortName,
    bsgId:      it.bsgId,
    uid:        it.uid,
    avg24hPrice:  it.avg24hPrice,
    avg7daysPrice: it.avg7daysPrice,
    price:        it.price,
    basePrice:    it.basePrice,
    traderName:   it.traderName,
    traderPrice:  it.traderPrice,
    diff24h:      it.diff24h,
    diff7days:    it.diff7days,
    slots:        it.slots,
    tags:         it.tags,
    icon:         it.icon,
    img:          it.img,
    imgBig:       it.imgBig,
    link:         it.link,
    wikiLink:     it.wikiLink,
    updated:      it.updated,
  };
}

// Write JSON
fs.mkdirSync(path.dirname(OUT), { recursive: true });
fs.writeFileSync(OUT, JSON.stringify(out, null, 2));
console.error(`\n✓ Wrote ${Object.keys(out).length} records to ${OUT}`);

// Report ambiguities & misses
if (amb.length) {
  console.error(`\n⚠️  ${amb.length} ambiguous match(es) — picked first; review:`);
  for (const a of amb) {
    console.error(`  [${a.id}] q="${a.q}"`);
    for (const c of a.candidates) console.error(`     · ${c.shortName.padEnd(20)} ${c.name.padEnd(45)} ${c.bsgId}`);
  }
}

if (miss.length) {
  console.error(`\n✗ ${miss.length} missing — refine query:`);
  for (const m of miss) console.error(`  [${m.id}] q="${m.q}"`);
}

// Markdown summary
console.log('\n## Resolved anchor items\n');
console.log('| ID | shortName | Tpl | avg24h ₽ | trader ₽ |');
console.log('|----|-----------|-----|---------:|---------:|');
for (const id of Object.keys(out).sort()) {
  const r = out[id];
  const avg = (r.avg24hPrice ?? 0).toLocaleString('pt-BR');
  const tr  = (r.traderPrice ?? 0).toLocaleString('pt-BR');
  console.log(`| ${id} | ${r.shortName} | \`${r.bsgId}\` | ${avg} | ${tr} |`);
}
console.log('\n> Preços via [tarkov-market.com](https://tarkov-market.com) (PVE, avg 24h).');
