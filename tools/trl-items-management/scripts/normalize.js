#!/usr/bin/env node
/**
 * normalize.js — merges the three raw caches into a single normalized DB.
 *
 * Reads:
 *   cache/tarkov-dev-raw.json     (PVE+regular items + itemCategories)
 *   cache/tarkov-market-raw.json  (PVE flea data)
 *   cache/spt-raw.json            (local SPT install snapshot)
 *
 * Writes:
 *   data/items.json       (one line per Tpl, sorted, ordered keys)
 *   data/categories.json  (single annotated tree)
 *   data/meta.json        (timestamps + stats)
 */
'use strict';

const fs   = require('fs');
const path = require('path');

const CACHE_DIR = path.join(__dirname, '..', 'cache');
const DATA_DIR  = path.join(__dirname, '..', 'data');

const ITEM_KEYS = [
  'id', 'name', 'shortName', 'normalizedName', 'wikiLink',
  'image', 'imageFromClone', 'category', 'types', 'dims', 'grids',
  'modSource',
  'spt', 'tarkovDev', 'tarkovMarket', 'consolidated',
];

// ── IO helpers ───────────────────────────────────────────────────────────────

function readJson(p) { return JSON.parse(fs.readFileSync(p, 'utf8')); }

function orderedObject(obj, keys) {
  const out = {};
  for (const k of keys) out[k] = obj[k] ?? null;
  return out;
}

// ── Category tree ────────────────────────────────────────────────────────────

function buildCategoryMap(devCategories) {
  // Map id → { id, name, normalizedName, parentId }
  const map = new Map();
  for (const c of devCategories) {
    map.set(c.id, {
      id: c.id,
      name: c.name,
      normalizedName: c.normalizedName,
      parentId: c.parent ? c.parent.id : null,
    });
  }
  return map;
}

function resolvePath(categoryMap, leafId) {
  // Walk parents, returning names (root "Item" omitted as per plan).
  const names = [];
  let cur = leafId;
  const guard = new Set();
  while (cur && !guard.has(cur)) {
    guard.add(cur);
    const node = categoryMap.get(cur);
    if (!node) break;
    if (node.name === 'Item') break; // omit root
    names.unshift(node.name);
    cur = node.parentId;
  }
  return names;
}

// Per-category icon overrides — used when the SPT-shipped PNG is too low-res
// for crisp display (some are 11x11 originals). Lanczos-upscaled versions
// live under `tools/trl-items-management/viewer/icons/` and are served by serve.js
// at /viewer/icons/<filename>.
const ICON_OVERRIDES = {
  '5b5f749986f774094242f199': '/viewer/icons/icon_mod_sight_hd.png', // Special purpose sights
};

// In-game EFT/SPT 4.0 handbook display order (top → bottom). Handbook.json's
// `Order` field numbers categories 1..13 but the Unity client uses a different
// hardcoded order. This list mirrors what the in-game Handbook actually shows.
// Categories not listed fall through to JSON Order as tiebreaker.
const HANDBOOK_TOP_ORDER = [
  'Weapons',              // "Builds" in some EFT locales
  'Weapon parts & mods',  // adjacent to Weapons, per user preference
  'Barter items',
  'Gear',
  'Ammo',
  'Provisions',
  'Medication',
  'Keys',
  'Info items',
  'Special equipment',
  'Maps',
  'Money',
  'Task items',           // appended at tail (not visible in user reference)
];

function handbookOrderIndex(name) {
  const i = HANDBOOK_TOP_ORDER.indexOf(name);
  return i === -1 ? 999 : i;
}

function buildHandbookTree(handbookCategories) {
  // Handbook is the UI-friendly taxonomy that mirrors the in-game Handbook
  // (Builds → Assault rifles, Gear → Backpacks, etc). Each category has an
  // Icon path referring to a PNG in <SPT_DATA>/images/handbook/ — served by
  // serve.js as /spt-images/handbook/<file>.png.
  const nodes = new Map();
  for (const c of handbookCategories) {
    const name = (c.name && c.name.trim()) || null;
    if (!name || /^[a-f0-9]{24}$/i.test(name)) continue; // skip unresolved / wishlist-like
    nodes.set(c.id, {
      id: c.id,
      name,
      icon: (c.icon && /\.\w+$/.test(c.icon)) ? c.icon.replace(/^\/files\//, '/spt-images/') : null,
      order: c.order != null ? c.order : 9999,
      children: [],
    });
  }
  // Build parentId map for icon-inheritance walks (some sub-cats in handbook
  // have empty Icon fields; we fall back to the nearest ancestor with one).
  const parentIdById = new Map(handbookCategories.map(c => [c.id, c.ParentId || c.parentId || null]));
  function inheritIcon(id) {
    let cur = id;
    const seen = new Set();
    while (cur && !seen.has(cur)) {
      seen.add(cur);
      const node = nodes.get(cur);
      if (node && node.icon) return node.icon;
      cur = parentIdById.get(cur);
    }
    return null;
  }
  // Second pass: fill empty icons from ancestors, then apply per-id overrides.
  for (const node of nodes.values()) {
    if (!node.icon) node.icon = inheritIcon(node.id);
    if (ICON_OVERRIDES[node.id]) node.icon = ICON_OVERRIDES[node.id];
  }

  const roots = [];
  for (const c of handbookCategories) {
    const node = nodes.get(c.id);
    if (!node) continue;
    const parent = nodes.get(c.ParentId || c.parentId);
    if (parent) parent.children.push(node);
    else roots.push(node);
  }
  // Top-level uses the hardcoded EFT display order; sub-children fall back to
  // JSON Order field (which seems correct for sub-cats based on icons/grouping).
  function sortSubChildren(n) {
    if (!n.children || !n.children.length) return;
    n.children.sort((a,b) => (a.order - b.order) || (a.name||'').localeCompare(b.name||''));
    n.children.forEach(sortSubChildren);
  }
  roots.sort((a,b) => (handbookOrderIndex(a.name) - handbookOrderIndex(b.name))
                       || (a.order - b.order)
                       || (a.name||'').localeCompare(b.name||''));
  roots.forEach(sortSubChildren);
  return roots.length === 1 ? roots[0] : { id: null, name: 'Item', children: roots };
}

// ── Consolidated derivation ──────────────────────────────────────────────────

function deriveConsolidated(item, conditionType) {
  const dev    = item.tarkovDev && item.tarkovDev.pve;
  const market = item.tarkovMarket && item.tarkovMarket.pve;
  const spt    = item.spt;

  // priceTraderSell: max sellFor across non-flea vendors
  let priceTraderSell = null;
  if (dev && Array.isArray(dev.sellFor)) {
    let bestPrice = -1;
    let bestVendor = null;
    for (const s of dev.sellFor) {
      if (!s.vendor || s.vendor.normalizedName === 'flea-market') continue;
      const p = s.priceRUB ?? 0;
      if (p > bestPrice) { bestPrice = p; bestVendor = s.vendor.name; }
    }
    if (bestVendor) priceTraderSell = { value: bestPrice, vendor: bestVendor };
  }

  // Effective flea price = what the flea actually uses (override+bonus or vanilla,
  // floored). Falls back to fleaPrice for items.json built before these fields.
  const priceFleaSpt          = spt    ? (spt.effectiveFleaPrice ?? spt.fleaPrice ?? null) : null;
  const priceFleaDevLastLow   = dev    ? (dev.lastLow    ?? null) : null;
  const priceFleaDevAvg24h    = dev    ? (dev.avg24h     ?? null) : null;
  const priceFleaMarketAvg24h = market ? (market.avg24h  ?? null) : null;

  // Canonical priority chain
  let priceFleaCanonical = null;
  let priceFleaSource    = null;
  if (priceFleaMarketAvg24h != null) {
    priceFleaCanonical = priceFleaMarketAvg24h;
    priceFleaSource    = 'tarkov-market-avg24h';
  } else if (priceFleaDevAvg24h != null) {
    priceFleaCanonical = priceFleaDevAvg24h;
    priceFleaSource    = 'tarkov.dev-avg24h';
  } else if (priceFleaDevLastLow != null) {
    priceFleaCanonical = priceFleaDevLastLow;
    priceFleaSource    = 'tarkov.dev-lastLow';
  } else if (priceFleaSpt != null) {
    priceFleaCanonical = priceFleaSpt;
    priceFleaSource    = 'spt';
  }

  return {
    group: item.category ? item.category.name : null,
    conditionType,
    priceTraderSell,
    priceFleaSpt,
    priceFleaDevLastLow,
    priceFleaDevAvg24h,
    priceFleaMarketAvg24h,
    priceFleaCanonical,
    priceFleaSource,
  };
}

// ── Main ─────────────────────────────────────────────────────────────────────

function main() {
  const t0 = Date.now();

  // Load caches
  const dev     = readJson(path.join(CACHE_DIR, 'tarkov-dev-raw.json'));
  const market  = readJson(path.join(CACHE_DIR, 'tarkov-market-raw.json'));
  const sptRaw  = readJson(path.join(CACHE_DIR, 'spt-raw.json'));
  console.error(`Loaded: dev.pve=${dev.pve.length}, dev.regular=${dev.regular.length}, market=${market.length}, spt=${Object.keys(sptRaw.items).length}`);

  // Build indexes
  const devPveById = new Map(dev.pve.map(x => [x.id, x]));
  const devRegById = new Map(dev.regular.map(x => [x.id, x]));
  // tarkov-market uses bsgId as canonical Tpl
  const marketById = new Map();
  for (const m of market) {
    if (m.bsgId) marketById.set(m.bsgId, m);
  }
  const sptItems = sptRaw.items;
  const categoryMap = buildCategoryMap(dev.categories || []);
  // Handbook lookup for per-item category resolution (preferred over tarkov.dev)
  const handbookById = new Map((sptRaw.handbookCategories || []).map(c => [c.id, c]));

  // Union of Tpls
  const allTpls = new Set();
  for (const id of devPveById.keys()) allTpls.add(id);
  for (const id of devRegById.keys()) allTpls.add(id);
  for (const id of marketById.keys()) allTpls.add(id);
  for (const id of Object.keys(sptItems)) allTpls.add(id);
  console.error(`Union of Tpls: ${allTpls.size}`);

  // Stats
  let withAllThree = 0, withSptOnly = 0, withDevOnly = 0, withMarketOnly = 0;
  let onlySptNoImage = 0;

  // Build normalized items
  const items = {};
  for (const tpl of allTpls) {
    const d = devPveById.get(tpl);
    const r = devRegById.get(tpl);
    const m = marketById.get(tpl);
    const s = sptItems[tpl];

    const hasDev = !!d;
    const hasMkt = !!m;
    const hasSpt = !!s;
    if (hasDev && hasMkt && hasSpt) withAllThree++;
    else if (hasSpt && !hasDev && !hasMkt) withSptOnly++;
    else if (hasDev && !hasSpt && !hasMkt) withDevOnly++;
    else if (hasMkt && !hasDev && !hasSpt) withMarketOnly++;

    // Names / dims / image / types: tarkov.dev preferred, SPT fallback
    const name           = (d && d.name)            || (s && s.name) || null;
    const shortName      = (d && d.shortName)       || (s && s.shortName) || null;
    const normalizedName = (d && d.normalizedName)  || null;
    const wikiLink       = (d && d.wikiLink)        || null;
    let image = d ? {
      icon:  d.iconLink       || null,
      grid:  d.gridImageLink  || null,
      large: d.image512pxLink || null,
    } : null;
    let imageFromClone = false;
    // Mod items aren't on tarkov.dev (no image). They clone a vanilla base
    // (itemTplToClone) whose real custom icon only exists as an in-game-rendered
    // bundle prefab — no static PNG ships. Use the cloned vanilla item's tarkov.dev
    // icon as a representative proxy (100% coverage for the installed WTT mods).
    if (!image && s && s.cloneTpl) {
      const cd = devPveById.get(s.cloneTpl);
      if (cd && (cd.iconLink || cd.gridImageLink || cd.image512pxLink)) {
        image = { icon: cd.iconLink || null, grid: cd.gridImageLink || null, large: cd.image512pxLink || null };
        imageFromClone = true;
      }
    }
    if (!image && s) onlySptNoImage++;

    const dims = d ? {
      weight: d.weight ?? null,
      width:  d.width  ?? null,
      height: d.height ?? null,
    } : (s ? {
      weight: s.weight ?? null,
      width:  s.width  ?? null,
      height: s.height ?? null,
    } : null);

    // Storage grids (containers, rigs, backpacks). Only present on items with grids.
    const grids = (s && s.grids) || null;

    const types = d ? (d.types || []) : [];

    // Category — prefer handbook (UI taxonomy matching in-game Handbook).
    // Falls back to tarkov.dev category only when item lacks SPT presence.
    let category = null;
    if (s && s.handbookCategoryId && handbookById.has(s.handbookCategoryId)) {
      const hb = handbookById.get(s.handbookCategoryId);
      // Resolve path by walking up handbook parents
      const pathNames = [];
      let cur = hb.id;
      const seen = new Set();
      while (cur && !seen.has(cur)) {
        seen.add(cur);
        const node = handbookById.get(cur);
        if (!node) break;
        const nm = (node.name && node.name.trim()) || null;
        if (nm && !/^[a-f0-9]{24}$/i.test(nm)) pathNames.unshift(nm);
        cur = node.parentId || node.ParentId;
      }
      // Inherit icon from nearest ancestor when this category's icon is empty
      let iconCur = hb.id;
      let iconUrl = null;
      const iconSeen = new Set();
      while (iconCur && !iconSeen.has(iconCur)) {
        iconSeen.add(iconCur);
        const n = handbookById.get(iconCur);
        if (!n) break;
        if (n.icon && /\.\w+$/.test(n.icon)) { iconUrl = n.icon.replace(/^\/files\//, '/spt-images/'); break; }
        iconCur = n.parentId || n.ParentId;
      }
      category = {
        id: hb.id,
        name: (hb.name && hb.name.trim()) || hb.id,
        icon: ICON_OVERRIDES[hb.id] || iconUrl,
        path: pathNames,
        source: 'handbook',
      };
    } else if (d && d.category) {
      category = {
        id: d.category.id,
        name: d.category.name,
        normalizedName: d.category.normalizedName || null,
        path: resolvePath(categoryMap, d.category.id),
        source: 'tarkov.dev',
      };
    }

    // SPT block. Flea fields validated vs source + 7 in-game scenarios — see
    // docs/flea-override-plan.md. fleaPrice = additive bonus (handbook × M);
    // effectiveFleaPrice = max((override ?? pricesDisk ?? 0) + bonus, fleaFloor).
    const spt = s ? {
      basePrice:          s.basePrice ?? null,
      fleaPrice:          s.fleaPrice ?? null,            // additive bonus = basePrice × M (what the viewer subtracts)
      fleaFloor:          s.fleaFloor ?? 0,               // offer base can't drop below this (basePrice × K_trader)
      fleaCeiling:        s.fleaCeiling ?? null,          // cap for mods ×6 / electronics ×11 (unreasonableModPrices), or null
      fleaMultiplier:     s.fleaMultiplier ?? null,       // M: 1.5/2.3 std/craft, or 1.8/2.5 tpl/type overrides
      isHideoutCraftItem: s.isHideoutCraftItem === true,
      fleaOverride:       s.fleaOverride ?? null,         // ragfair.json:itemPriceOverrideRouble[tpl], or null
      fleaOfferMultiplier: s.fleaOfferMultiplier ?? null, // ragfair.json:itemPriceMultiplier[tpl] — offer-time factor (lever for mod items), or null
      fleaBaseRaw:        s.fleaBaseRaw ?? null,          // = GetFleaPriceForItem (raw base the offer-multiplier scales)
      effectiveFleaPrice: s.effectiveFleaPrice ?? null,   // clamp((override ?? pricesDisk ?? 0 + bonus) × offerMult, floor, ceiling)
      fleaBanned:         s.fleaBanned === true,
      fleaBanReasons:     s.fleaBanReasons || [],
      traders:            s.traders || [],
      questRewards:       s.questRewards || [],
    } : null;

    // tarkov.dev block
    const tarkovDev = (d || r) ? {
      pve:     d ? {
        lastLow:        d.lastLowPrice         ?? null,
        avg24h:         d.avg24hPrice          ?? null,
        low24h:         d.low24hPrice          ?? null,
        high24h:        d.high24hPrice         ?? null,
        change48h:      d.changeLast48h        ?? null,
        change48hPct:   d.changeLast48hPercent ?? null,
        updated:        d.updated              ?? null,
        sellFor:        d.sellFor              || [],
        buyFor:         d.buyFor               || [],
      } : null,
      regular: r ? {
        lastLow:        r.lastLowPrice         ?? null,
        avg24h:         r.avg24hPrice          ?? null,
        low24h:         r.low24hPrice          ?? null,
        high24h:        r.high24hPrice         ?? null,
        change48h:      r.changeLast48h        ?? null,
        change48hPct:   r.changeLast48hPercent ?? null,
        updated:        r.updated              ?? null,
        sellFor:        r.sellFor              || [],
        buyFor:         r.buyFor               || [],
      } : null,
    } : null;

    // tarkov-market block
    const tarkovMarket = m ? {
      pve: {
        avg24h:      m.avg24hPrice  ?? null,
        avg7days:    m.avg7daysPrice ?? null,
        price:       m.price        ?? null,
        traderName:  m.traderName   ?? null,
        traderPrice: m.traderPrice  ?? null,
        link:        m.link         ?? null,
        updated:     m.updated      ?? null,
      },
    } : null;

    const conditionType = s ? s.conditionType : 'unknown';

    const item = {
      id: tpl,
      name, shortName, normalizedName, wikiLink,
      image, imageFromClone,
      category,
      types,
      dims,
      grids,
      modSource: (s && s.modSource) || null,
      spt,
      tarkovDev,
      tarkovMarket,
    };
    item.consolidated = deriveConsolidated(item, conditionType);
    items[tpl] = orderedObject(item, ITEM_KEYS);
  }

  // Stats
  let tradeable = 0;
  for (const it of Object.values(items)) {
    if (it.consolidated.priceFleaCanonical != null || (it.spt && it.spt.traders.length > 0)) tradeable++;
  }

  // Serialize items.json with one Tpl per line
  console.error('Serializing items.json...');
  const sortedTpls = Array.from(allTpls).sort();
  const lines = sortedTpls.map(t => JSON.stringify(t) + ':' + JSON.stringify(items[t]));
  const out = '{\n' + lines.join(',\n') + '\n}';
  fs.mkdirSync(DATA_DIR, { recursive: true });
  const itemsPath = path.join(DATA_DIR, 'items.json');
  fs.writeFileSync(itemsPath, out);

  // Read-back validation
  console.error('Validating items.json...');
  const reparsed = JSON.parse(fs.readFileSync(itemsPath, 'utf8'));
  const reparsedKeys = Object.keys(reparsed).length;
  if (reparsedKeys !== sortedTpls.length) {
    console.error(`FATAL: read-back count mismatch (wrote ${sortedTpls.length}, read ${reparsedKeys})`);
    process.exit(1);
  }

  // categories.json
  console.error('Building categories.json...');
  const categoryTree = buildHandbookTree(sptRaw.handbookCategories || []);
  fs.writeFileSync(path.join(DATA_DIR, 'categories.json'), JSON.stringify(categoryTree, null, 2));

  // traders.json (nickname → metadata for viewer to render avatars)
  fs.writeFileSync(path.join(DATA_DIR, 'traders.json'), JSON.stringify(sptRaw.traders || {}, null, 2));

  // meta.json
  const devCacheStat = fs.statSync(path.join(CACHE_DIR, 'tarkov-dev-raw.json'));
  const mktCacheStat = fs.statSync(path.join(CACHE_DIR, 'tarkov-market-raw.json'));
  const meta = {
    generatedAt: new Date().toISOString(),
    sources: {
      tarkovDev:    { fetchedAt: devCacheStat.mtime.toISOString(), pveCount: dev.pve.length, regularCount: dev.regular.length, categoryCount: (dev.categories || []).length },
      tarkovMarket: { fetchedAt: mktCacheStat.mtime.toISOString(), count: market.length },
      spt:          { loadedAt: sptRaw.loadedAt, fleaPricesMtime: sptRaw.fleaPricesMtime || null, fleaPricesSource: sptRaw.fleaPricesSource || null, fleaMinUserLevel: sptRaw.fleaMinUserLevel || null, path: sptRaw.sptDataPath, itemCount: sptRaw.counts.items, priceCount: sptRaw.counts.withFleaPrice, traderCount: sptRaw.counts.traders, offerCount: sptRaw.counts.offers, currencyRates: sptRaw.currencyRates },
    },
    stats: {
      totalTpls: allTpls.size,
      withAllThree, withSptOnly, withTarkovDevOnly: withDevOnly, withTarkovMarketOnly: withMarketOnly,
      tradeable,
      onlySptNoImage,
    },
  };
  fs.writeFileSync(path.join(DATA_DIR, 'meta.json'), JSON.stringify(meta, null, 2));

  // Divergence logs
  console.error('\nStats:');
  console.error(JSON.stringify(meta.stats, null, 2));

  console.error('\nTop 10 dev × market avg24h divergences:');
  const devMktDiffs = [];
  for (const it of Object.values(items)) {
    const a = it.consolidated.priceFleaDevAvg24h;
    const b = it.consolidated.priceFleaMarketAvg24h;
    if (a != null && b != null && a > 0 && b > 0) {
      const diff = Math.max(a,b) / Math.min(a,b) - 1;
      devMktDiffs.push({ id: it.id, name: it.shortName, dev: a, market: b, pct: (diff*100).toFixed(1) });
    }
  }
  devMktDiffs.sort((x,y) => parseFloat(y.pct) - parseFloat(x.pct));
  for (const d of devMktDiffs.slice(0, 10)) {
    console.error(`  ${d.pct.padStart(6)}%  ${(d.name||'').padEnd(25)}  dev=${d.dev}  market=${d.market}`);
  }

  console.error('\nTop 5 spt × dev-lastLow divergences (LiveFleaPrices staleness):');
  const sptDevDiffs = [];
  for (const it of Object.values(items)) {
    const a = it.consolidated.priceFleaSpt;
    const b = it.consolidated.priceFleaDevLastLow;
    if (a != null && b != null && a > 0 && b > 0) {
      const diff = Math.max(a,b) / Math.min(a,b) - 1;
      sptDevDiffs.push({ id: it.id, name: it.shortName, spt: a, dev: b, pct: (diff*100).toFixed(1) });
    }
  }
  sptDevDiffs.sort((x,y) => parseFloat(y.pct) - parseFloat(x.pct));
  for (const d of sptDevDiffs.slice(0, 5)) {
    console.error(`  ${d.pct.padStart(6)}%  ${(d.name||'').padEnd(25)}  spt=${d.spt}  dev-lastLow=${d.dev}`);
  }

  const sizeMB = (fs.statSync(itemsPath).size / 1048576).toFixed(2);
  console.error(`\nWrote data/items.json (${sizeMB} MB, ${sortedTpls.length} Tpls) in ${((Date.now()-t0)/1000).toFixed(1)}s`);
}

main();
