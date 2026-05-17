#!/usr/bin/env node
/**
 * load-spt.js — extracts item, price, trader and handbook data from a local
 * SPT install into cache/spt-raw.json, keyed by BSG Tpl.
 *
 * Reads SPT_PATH env (default D:/SPT/SPT). Detects whether path points at the
 * SPT root or the SPT_Data subfolder.
 *
 * Output schema (per Tpl):
 *   { id, name, shortName, basePrice, fleaPrice, weight, width, height,
 *     stackMaxSize, conditionType, handbookCategoryId, parentClassId,
 *     traders: [{ name, priceRUB, currency, loyaltyLevel, unlimited, stock, questLocked }] }
 *
 * Usage:  node scripts/load-spt.js
 * Output: cache/spt-raw.json
 */
'use strict';

const fs   = require('fs');
const path = require('path');

const SPT_PATH = process.env.SPT_PATH || 'D:/SPT/SPT';
const CACHE    = path.join(__dirname, '..', 'cache', 'spt-raw.json');

const CURRENCY_RUB = '5449016a4bdc2d6f028b456f';
const CURRENCY_USD = '5696686a4bdc2da3298b456a';
const CURRENCY_EUR = '569668774bdc2da2298b4568';
const FALLBACK_USD_RATE = 120; // RUB per USD; verified from handbook
const FALLBACK_EUR_RATE = 133; // RUB per EUR; verified from handbook

// ── Helpers ──────────────────────────────────────────────────────────────────

function readJson(p) {
  const txt = fs.readFileSync(p, 'utf8').replace(/^﻿/, '');
  return JSON.parse(txt);
}

function resolveSptDataDir(sptPath) {
  const candidates = [
    path.join(sptPath, 'SPT_Data'),
    sptPath, // user already pointed at the SPT_Data folder
  ];
  for (const c of candidates) {
    if (fs.existsSync(path.join(c, 'database', 'templates', 'items.json'))) {
      return c;
    }
  }
  throw new Error(`SPT_Data not found under ${sptPath}. Set SPT_PATH env to your SPT install root.`);
}

function deriveConditionType(props) {
  if (!props) return 'unknown';
  if ((props.MaximumNumberOfUsage || 0) > 0) return 'uses';
  if ((props.MaxDurability       || 0) > 0) return 'durability';
  // Meds use MaxHpResource; food/bag-of-bolts/etc use MaxResource
  if ((props.MaxHpResource       || 0) > 0) return 'resource';
  if ((props.MaxResource         || 0) > 0) return 'resource';
  return 'none';
}

// ── Main ─────────────────────────────────────────────────────────────────────

function main() {
  const t0 = Date.now();
  const dataDir = resolveSptDataDir(SPT_PATH);
  console.error(`SPT data dir: ${dataDir}`);

  // 1. Items template
  console.error('Reading items.json...');
  const itemsRaw = readJson(path.join(dataDir, 'database', 'templates', 'items.json'));
  const items = {};
  let nodeCount = 0;
  for (const id of Object.keys(itemsRaw)) {
    const it = itemsRaw[id];
    if (it._type !== 'Item') { nodeCount++; continue; }
    const p = it._props || {};
    items[id] = {
      id,
      internalName: it._name,
      parentClassId: it._parent,
      // Locale-resolved name added below; _props.Name is i18n key
      _nameKey: p.Name,
      _shortNameKey: p.ShortName,
      weight: p.Weight ?? null,
      width: p.Width ?? null,
      height: p.Height ?? null,
      stackMaxSize: p.StackMaxSize ?? null,
      conditionType: deriveConditionType(p),
      canSellOnRagfair: p.CanSellOnRagfair !== false, // per-item BSG flag
    };
  }
  console.error(`  items: ${Object.keys(items).length} (skipped ${nodeCount} nodes)`);

  // 2. Locale (en) for name resolution
  console.error('Reading locales/global/en.json...');
  const localePath = path.join(dataDir, 'database', 'locales', 'global', 'en.json');
  let locale = {};
  let nameResolved = 0;
  if (fs.existsSync(localePath)) {
    locale = readJson(localePath);
    for (const id of Object.keys(items)) {
      const name      = locale[id + ' Name'];
      const shortName = locale[id + ' ShortName'];
      if (name)      { items[id].name = name; nameResolved++; }
      if (shortName) { items[id].shortName = shortName; }
    }
    console.error(`  names resolved: ${nameResolved} / ${Object.keys(items).length}`);
  } else {
    console.error('  WARNING: en.json not found, names will use i18n keys');
  }
  // Fallback for items without locale entry
  for (const id of Object.keys(items)) {
    if (!items[id].name)      items[id].name = items[id]._nameKey || items[id].internalName;
    if (!items[id].shortName) items[id].shortName = items[id]._shortNameKey || items[id].name;
    delete items[id]._nameKey;
    delete items[id]._shortNameKey;
  }

  // 3. Flea prices — source of truth is SPT's prices.json on disk. Edits via
  // the viewer (future) will write back to this file.
  const pricesPath = path.join(dataDir, 'database', 'templates', 'prices.json');
  const pricesSource = 'spt-prices.json';
  console.error(`Reading prices from ${pricesPath}...`);
  const prices = readJson(pricesPath);
  let priceCount = 0;
  for (const id of Object.keys(prices)) {
    if (items[id]) { items[id].fleaPrice = prices[id]; priceCount++; }
  }
  for (const id of Object.keys(items)) {
    if (items[id].fleaPrice === undefined) items[id].fleaPrice = null;
  }
  console.error(`  fleaPrice set on ${priceCount} items`);

  // 4. Handbook (base prices + categories)
  console.error('Reading handbook.json...');
  const handbook = readJson(path.join(dataDir, 'database', 'templates', 'handbook.json'));
  let baseCount = 0;
  for (const e of handbook.Items) {
    if (items[e.Id]) {
      items[e.Id].basePrice = e.Price;
      items[e.Id].handbookCategoryId = e.ParentId;
      baseCount++;
    }
  }
  for (const id of Object.keys(items)) {
    if (items[id].basePrice === undefined)         items[id].basePrice = null;
    if (items[id].handbookCategoryId === undefined) items[id].handbookCategoryId = null;
  }
  console.error(`  basePrice set on ${baseCount} items`);

  // Currency rates from handbook
  const usdEntry = handbook.Items.find(x => x.Id === CURRENCY_USD);
  const eurEntry = handbook.Items.find(x => x.Id === CURRENCY_EUR);
  const usdRate = usdEntry ? usdEntry.Price : FALLBACK_USD_RATE;
  const eurRate = eurEntry ? eurEntry.Price : FALLBACK_EUR_RATE;
  const rateSource = (usdEntry && eurEntry) ? 'handbook' : 'fallback';
  console.error(`  currency rates (${rateSource}): USD=${usdRate}, EUR=${eurRate}`);

  // 4b. Flea blacklist (ragfair.json + per-item CanSellOnRagfair)
  console.error('Reading configs/ragfair.json (flea blacklist)...');
  const ragfair = readJson(path.join(dataDir, 'configs', 'ragfair.json'));
  const blacklist = (ragfair.dynamic && ragfair.dynamic.blacklist) || {};
  const bsgListEnabled = blacklist.enableBsgList !== false;
  const customBanned = new Set(Array.isArray(blacklist.custom) ? blacklist.custom : []);
  let bannedBsg = 0, bannedCustom = 0;
  for (const id of Object.keys(items)) {
    const reasons = [];
    if (bsgListEnabled && items[id].canSellOnRagfair === false) { reasons.push('bsg'); bannedBsg++; }
    if (customBanned.has(id)) { reasons.push('custom'); bannedCustom++; }
    items[id].fleaBanned = reasons.length > 0;
    items[id].fleaBanReasons = reasons; // [] when not banned
  }
  console.error(`  flea blacklist: bsgList=${bsgListEnabled?'ON':'OFF'}, bsg-banned=${bannedBsg}, custom-banned=${bannedCustom} (${customBanned.size} in config)`);
  // Note: questList and traderItems flags in the blacklist are not yet resolved
  // (would require quest-active-state + trader exclusivity logic). Documented as a limitation.

  // 5. Traders
  const tradersDir = path.join(dataDir, 'database', 'traders');
  const traderIds = fs.readdirSync(tradersDir).filter(d =>
    fs.statSync(path.join(tradersDir, d)).isDirectory()
  );
  console.error(`Reading ${traderIds.length} traders...`);
  for (const id of Object.keys(items)) items[id].traders = [];

  let totalOffers = 0;
  let barterOffers = 0;
  let questLockedOffers = 0;

  // Trader metadata: nickname → { id, avatar URL served by serve.js /spt-images/ }.
  // Files live in SPT_Data/images/trader/avatar/. base.json `avatar` field uses
  // /files/ prefix (legacy) and may say .jpg even though the file is .png — we
  // resolve by reading the actual directory.
  const traderAvatarsDir = path.join(dataDir, 'images', 'trader', 'avatar');
  const avatarFiles = fs.existsSync(traderAvatarsDir) ? new Set(fs.readdirSync(traderAvatarsDir)) : new Set();
  const traders = {};

  for (const traderId of traderIds) {
    const baseP   = path.join(tradersDir, traderId, 'base.json');
    const assortP = path.join(tradersDir, traderId, 'assort.json');
    const questAP = path.join(tradersDir, traderId, 'questassort.json');
    if (!fs.existsSync(baseP) || !fs.existsSync(assortP)) {
      console.error(`  ${traderId}: missing base or assort, skipping`);
      continue;
    }
    const base   = readJson(baseP);
    const assort = readJson(assortP);
    const nickname = base.nickname || traderId;

    // Resolve avatar: base.json says /files/trader/avatar/<id>.jpg but real file may be .png
    let avatarUrl = null;
    if (base.avatar) {
      const filename = path.basename(base.avatar);
      const stem = filename.replace(/\.[^.]+$/, '');
      // Try exact match first, then any extension
      const match = avatarFiles.has(filename) ? filename
                  : [...avatarFiles].find(f => f.startsWith(stem + '.'));
      if (match) avatarUrl = '/spt-images/trader/avatar/' + match;
    }
    traders[nickname] = { id: traderId, avatar: avatarUrl };

    // assortId -> tpl map
    const idToTpl = {};
    for (const it of assort.items) idToTpl[it._id] = it._tpl;

    // Quest-locked assortIds
    const questLockedIds = new Set();
    if (fs.existsSync(questAP)) {
      try {
        const qa = readJson(questAP);
        for (const bucket of ['success', 'started', 'fail']) {
          if (qa[bucket] && typeof qa[bucket] === 'object') {
            for (const aid of Object.keys(qa[bucket])) questLockedIds.add(aid);
          }
        }
      } catch (e) {
        console.error(`  ${nickname}: questassort.json parse failed (${e.message})`);
      }
    }

    // Iterate top-level offers
    const topItems = assort.items.filter(x => x.parentId === 'hideout');
    let traderOffers = 0;
    for (const offer of topItems) {
      const tpl = offer._tpl;
      if (!items[tpl]) continue; // unknown to items.json (mods may add these)

      // Resolve price
      const scheme = assort.barter_scheme[offer._id];
      let priceRUB = null;
      let currency = 'BARTER';
      if (Array.isArray(scheme) && Array.isArray(scheme[0]) && scheme[0].length === 1) {
        const req = scheme[0][0];
        if (req._tpl === CURRENCY_RUB) {
          priceRUB = req.count;
          currency = 'RUB';
        } else if (req._tpl === CURRENCY_USD) {
          priceRUB = Math.round(req.count * usdRate);
          currency = 'USD';
        } else if (req._tpl === CURRENCY_EUR) {
          priceRUB = Math.round(req.count * eurRate);
          currency = 'EUR';
        }
      }
      if (currency === 'BARTER') barterOffers++;

      const upd = offer.upd || {};
      const questLocked = questLockedIds.has(offer._id);
      if (questLocked) questLockedOffers++;

      items[tpl].traders.push({
        name: nickname,
        priceRUB,
        currency,
        loyaltyLevel: assort.loyal_level_items[offer._id] ?? 1,
        unlimited: !!upd.UnlimitedCount,
        stock: upd.UnlimitedCount ? null : (upd.StackObjectsCount ?? 0),
        questLocked,
      });
      traderOffers++;
    }
    totalOffers += traderOffers;
    console.error(`  ${nickname.padEnd(15)} ${traderOffers} offers`);
  }

  // Dedup intra-trader: same tpl appearing multiple times → keep lowest loyalty, then lowest price
  let dedupRemoved = 0;
  for (const tpl of Object.keys(items)) {
    const offers = items[tpl].traders;
    if (offers.length < 2) continue;
    const byTrader = {};
    for (const o of offers) {
      const key = o.name;
      const prev = byTrader[key];
      if (!prev) { byTrader[key] = o; continue; }
      const prevWorse =
        (o.loyaltyLevel < prev.loyaltyLevel) ||
        (o.loyaltyLevel === prev.loyaltyLevel && (o.priceRUB ?? Infinity) < (prev.priceRUB ?? Infinity));
      if (prevWorse) byTrader[key] = o;
      dedupRemoved++;
    }
    items[tpl].traders = Object.values(byTrader);
  }

  console.error(`Trader offers: ${totalOffers} total, ${barterOffers} barter, ${questLockedOffers} quest-locked, ${dedupRemoved} dedup-removed`);

  // 6. Emit cache
  const out = {
    loadedAt: new Date().toISOString(),
    sptDataPath: dataDir,
    currencyRates: { source: rateSource, USD: usdRate, EUR: eurRate },
    fleaPricesSource: pricesSource,
    fleaBlacklist: {
      bsgListEnabled,
      customCount: customBanned.size,
      questListEnabled: blacklist.enableQuestList === true,    // not resolved (limitation)
      traderItemsBanned: blacklist.traderItems === true,        // not resolved (limitation)
      damagedAmmoPacksBanned: blacklist.damagedAmmoPacks === true,
    },
    counts: {
      items: Object.keys(items).length,
      withFleaPrice: priceCount,
      withBasePrice: baseCount,
      traders: traderIds.length,
      offers: totalOffers,
      barterOffers,
      questLockedOffers,
      bannedBsg,
      bannedCustom,
    },
    traders,
    handbookCategories: handbook.Categories.map(c => ({
      id: c.Id, parentId: c.ParentId, icon: c.Icon || null,
      order: c.Order != null ? Number(c.Order) : 9999, // Order is the in-game handbook sort key (string in JSON)
      name: (locale[c.Id] && locale[c.Id].trim()) || c.Id,
    })),
    items,
  };

  fs.mkdirSync(path.dirname(CACHE), { recursive: true });
  fs.writeFileSync(CACHE, JSON.stringify(out, null, 2));
  const sizeMB = (fs.statSync(CACHE).size / 1048576).toFixed(2);
  console.error(`Wrote ${CACHE} (${sizeMB} MB) in ${((Date.now()-t0)/1000).toFixed(1)}s`);
}

main();
