#!/usr/bin/env node
/**
 * load-spt.js — extracts item, price, trader and handbook data from a local
 * SPT install into cache/spt-raw.json, keyed by BSG Tpl.
 *
 * Reads SPT_PATH env (default D:/SPT/SPT). Detects whether path points at the
 * SPT root or the SPT_Data subfolder.
 *
 * Output schema (per Tpl):
 *   { id, name, shortName, basePrice, fleaPrice, fleaFloor, fleaMultiplier,
 *     isHideoutCraftItem, fleaOverride, effectiveFleaPrice,
 *     weight, width, height, stackMaxSize, conditionType,
 *     handbookCategoryId, parentClassId,
 *     traders: [{ name, priceRUB, currency, loyaltyLevel, unlimited, stock, questLocked }] }
 *
 * Flea price math (SPT 4.0, validated vs source + 7 in-game scenarios; see
 * docs/flea-formula-validation.md + docs/flea-override-plan.md):
 *   fleaMultiplier = M(tpl) = itemTplMultiplierOverride | itemTypeMultiplierOverride(baseclass)
 *                    | priceMultiplier(1.5)   + (hideoutCraftMultiplier 0.8 if craft)
 *   fleaPrice      = round(basePrice × M)                      ← the ADDITIVE bonus term
 *   fleaFloor      = round(basePrice × K_trader)               ← offer base can't drop below this
 *   fleaCeiling    = round(basePrice × unreasonableMult)       ← cap for Weapon Mods (×6) /
 *                    Electronics (×11); null otherwise (unreasonableModPrices)
 *   fleaOverride   = ragfair.json:dynamic.itemPriceOverrideRouble[tpl]  (or null)
 *   effectiveFleaPrice = clamp( (fleaOverride ?? prices.json[tpl] ?? 0) + fleaPrice , fleaFloor , fleaCeiling )
 * KEY: the override does NOT overwrite — ApplyFleaPriceOverrides assigns the override
 * into the prices dict, then ReplaceFleaBasePrices ADDS the bonus (AddOrUpdate +=),
 * then the offer generator floors to handbook×K_trader. So the viewer sets a desired
 * price X by writing override = X − fleaPrice (valid for X ≥ fleaFloor). See docs/spt-internals.md.
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
const CURRENCY_GP  = '5d235b4d86f7742e017bc88a'; // GP coin — used by some traders (handbook price ≈ 7500 RUB)
const FALLBACK_USD_RATE = 120;  // RUB per USD; verified from handbook
const FALLBACK_EUR_RATE = 133;  // RUB per EUR; verified from handbook
const FALLBACK_GP_RATE  = 7500; // RUB per GP coin; verified from handbook

// Fence never sells money-priced editable assort (its stock is recycled player
// flea/insurance items). Offers from Fence are flagged non-editable regardless.
const FENCE_ID = '579dc571d53a0658a154fbec';

// ── Helpers ──────────────────────────────────────────────────────────────────

function readJson(p) {
  const txt = fs.readFileSync(p, 'utf8').replace(/^﻿/, '');
  return JSON.parse(txt);
}

function readJsonc(p) {
  const txt = fs.readFileSync(p, 'utf8').replace(/^﻿/, '');
  // Strip block comments then line comments
  const stripped = txt.replace(/\/\*[\s\S]*?\*\//g, '').replace(/\/\/[^\n]*/g, '');
  return JSON.parse(stripped);
}

// ─────────────────────────────────────────────────────────────────────────────
// DEFERRED (pré-pronto): live-server item source for code-injecting mods.
//
// Mod items are discovered below (§4c) by scanning user/mods/<mod>/db/CustomItems/
// *.json(c). That covers EVERY mod in this install — WTT-Artem, WTT-PackNStrap,
// HackerServer, LoadAmmoAnimServer, TEP300Backport — because they all DECLARE
// items as JSON. A mod that instead builds items purely in code (pushing into
// tables.templates.items at postDBLoad with no JSON file) would NOT be seen by the
// file scan. None installed today → not wired in. Enable this when one appears.
//
// The running SPT server holds the MERGED item DB (every item, however injected):
//   POST http://<http.ip>:<http.port>/client/items        (default 127.0.0.1:6969)
//   headers { requestcompressed:'0', responsecompressed:'0' }   // plain in, plain out
//   body    '{}'
//   → { err, errmsg, data:{ <tpl>: <ItemTemplate>, … } }         // GetUnclearedBody wrapper
// Transport derived from references SptHttpListener.cs (req/resp compression flags)
// + DataCallbacks.cs:46 (GetTemplateItems → databaseService.GetItems()).
// NOTE: not yet exercised against a live server (was down at authoring time) —
// validate the wrapper shape + that no session cookie is required before relying on it.
//
// Wiring plan when enabling (set env SPT_LIVE_ITEMS=1; the §4c block must become async):
//   1. const live = await fetchLiveServerItems();   // null if server down → fall back to file scan
//   2. const liveModTpls = Object.keys(live.data).filter(t => !pricesDisk[t] && !items[t] && !handbookSet.has(t));
//      // better vanilla test: tpl absent from the on-disk vanilla templates/items.json
//   3. ATTRIBUTION — the live DB is FLAT (no "which mod added this"). Match each
//      liveModTpl against files under each user/mods/<mod>/ (search the 24-hex id) →
//      modSource = that folder. Tpls in no folder → modSource = '(unknown mod)'.
//   4. Build the same record shape as §4c and run the same flea math (mhb/mflea/mBonus/mEff).
async function fetchLiveServerItems(sptDataDir) {
  let ip = '127.0.0.1', port = 6969;
  try {
    const http = readJson(path.join(sptDataDir, 'configs', 'http.json'));
    ip = http.ip || ip; port = http.port || port;
  } catch (_) { /* use defaults */ }
  return new Promise((resolve) => {
    const httpMod = require('http');
    const req = httpMod.request({
      host: ip, port, path: '/client/items', method: 'POST',
      headers: { requestcompressed: '0', responsecompressed: '0', 'Content-Type': 'application/json', 'Content-Length': 2 },
    }, (res) => {
      const chunks = [];
      res.on('data', c => chunks.push(c));
      res.on('end', () => {
        try {
          const j = JSON.parse(Buffer.concat(chunks).toString('utf8'));
          resolve(j && j.data ? j.data : j);   // unwrap { err, errmsg, data }
        } catch (e) { console.error(`  live /client/items: bad response (${e.message})`); resolve(null); }
      });
    });
    req.on('error', (e) => { console.error(`  live /client/items unreachable (${e.code || e.message}) — falling back to file scan`); resolve(null); });
    req.setTimeout(15000, () => req.destroy(new Error('timeout')));
    req.end('{}');
  });
}
void fetchLiveServerItems;  // referenced to keep linters quiet until §4c wires it in

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
    // Storage grids — containers, rigs, backpacks have internal cells.
    // Shape: [{ name, cellsH, cellsV }] (only items that declare grids).
    let grids = null;
    if (Array.isArray(p.Grids) && p.Grids.length > 0) {
      grids = p.Grids.map(g => ({
        name: g._name || null,
        cellsH: g._props && g._props.cellsH || 0,
        cellsV: g._props && g._props.cellsV || 0,
      })).filter(g => g.cellsH > 0 && g.cellsV > 0);
      if (!grids.length) grids = null;
    }

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
      grids,
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

  // 3a. Hideout crafts — set of tpls that appear as ingredients in any recipe.
  // These items get an extra +0.8 to fleaMultiplier (SPT default config:
  // dynamic.generateBaseFleaPrices.hideoutCraftMultiplier=0.8, useHideoutCraftMultiplier=true).
  // Vanilla fleaMultiplier becomes 1.5 + 0.8 = 2.3 for craft items. See
  // RagfairPriceService.cs:73-103 in references/spt-source.
  console.error('Reading database/hideout/production.json (craft items)...');
  const productionPath = path.join(dataDir, 'database', 'hideout', 'production.json');
  const production = readJson(productionPath);
  const hideoutCraftItems = new Set();
  // SPT vanilla uses camelCase; tolerate PascalCase from forks/mods just in case.
  const recipes = production.recipes || production.Recipes || [];
  for (const recipe of recipes) {
    const reqs = recipe.requirements || recipe.Requirements || [];
    for (const req of reqs) {
      const type = req.type || req.Type;
      const tpl  = req.templateId || req.TemplateId;
      if (type === 'Item' && tpl) hideoutCraftItems.add(tpl);
    }
  }
  console.error(`  ${hideoutCraftItems.size} unique tpls are hideout craft ingredients (from ${recipes.length} recipes)`);

  // Audit trail: dump the craft set so we can diff between SPT versions.
  const hideoutCraftsOut = path.join(__dirname, '..', 'data', 'hideout-crafts.json');
  fs.mkdirSync(path.dirname(hideoutCraftsOut), { recursive: true });
  fs.writeFileSync(hideoutCraftsOut, JSON.stringify({
    generatedAt: new Date().toISOString(),
    sptDataDir: dataDir,
    recipeCount: recipes.length,
    craftItemCount: hideoutCraftItems.size,
    tpls: Array.from(hideoutCraftItems).sort(),
  }, null, 2) + '\n', 'utf8');

  // 3b. Ragfair config — overrides, blacklist, AND the flea base-price multiplier
  // config. Read defensively from the file (installs may tune these).
  console.error('Reading configs/ragfair.json (overrides + multiplier config + blacklist)...');
  const ragfair = readJson(path.join(dataDir, 'configs', 'ragfair.json'));
  const dyn = ragfair.dynamic || {};
  const fleaOverridesMap = dyn.itemPriceOverrideRouble || {};
  console.error(`  ${Object.keys(fleaOverridesMap).length} flea overrides active in ragfair.json`);
  // itemPriceMultiplier: an OFFER-TIME factor applied in RagfairPriceService
  // .GetDynamicOfferPriceForOffer (price *= multiplier) AFTER GetFleaPriceForItem
  // and AFTER any mod's CustomItemService re-write. It is therefore the only lever
  // that works for MOD items (overrides are wiped for those). Also affects vanilla
  // items (SPT ships 2 defaults). Applied to effectiveFleaPrice below.
  const fleaMultiplierMap = dyn.itemPriceMultiplier || {};
  console.error(`  ${Object.keys(fleaMultiplierMap).length} flea offer-multipliers active in ragfair.json`);

  const gbfp = dyn.generateBaseFleaPrices || {};
  const FLEA_BASE_MULTIPLIER     = gbfp.priceMultiplier ?? 1.5;
  const HIDEOUT_CRAFT_MULTIPLIER = (gbfp.useHideoutCraftMultiplier ?? true) ? (gbfp.hideoutCraftMultiplier ?? 0.8) : 0;
  const TPL_MULT_OVERRIDE        = gbfp.itemTplMultiplierOverride || {};                  // { tpl: multiplier }
  const TYPE_MULT_OVERRIDE       = Object.entries(gbfp.itemTypeMultiplierOverride || {}); // [[baseclassTpl, multiplier]]
  const USE_TRADER_FLOOR         = dyn.useTraderPriceForOffersIfHigher === true;

  // M(tpl): per-tpl override wins, then per-baseclass override, then the base
  // multiplier. Mirrors RagfairPriceService.GetFleaBasePriceMultiplier (source).
  // Baseclass test walks the _parent chain in the raw template DB.
  const parentById = new Map();
  for (const id of Object.keys(itemsRaw)) parentById.set(id, itemsRaw[id]._parent || null);
  function isOfBaseclass(tpl, base) {
    let cur = tpl, guard = 0;
    while (cur && guard++ < 64) { if (cur === base) return true; cur = parentById.get(cur); }
    return false;
  }
  function fleaMultiplierFor(tpl) {
    if (TPL_MULT_OVERRIDE[tpl] != null) return TPL_MULT_OVERRIDE[tpl];
    for (const [base, mult] of TYPE_MULT_OVERRIDE) if (isOfBaseclass(tpl, base)) return mult;
    return FLEA_BASE_MULTIPLIER;
  }

  // Flea CEILING — RagfairPriceService.AdjustUnreasonablePrice caps "unreasonably
  // priced" baseclasses: if the offer price exceeds handbook × overMult it is set
  // to handbook × newMult. Default config caps Weapon Mods (×6) and Electronics
  // (×11). Discovered in-game: a GPU (Electronics) override targeting 3.0M was
  // capped to handbook×11 = 2.178M. fleaCeiling = round(handbook × newMult), or
  // null when the item is in no capped baseclass.
  const UNREASONABLE = Object.entries(dyn.unreasonableModPrices || {})
    .filter(([, c]) => c && c.enabled)
    .map(([base, c]) => [base, c.handbookPriceOverMultiplier, c.newPriceHandbookMultiplier]);
  function fleaCeilingFor(tpl, hb) {
    if (hb == null) return null;
    for (const [base, overMult, newMult] of UNREASONABLE) {
      if (isOfBaseclass(tpl, base)) return Math.round(hb * (newMult ?? overMult));
    }
    return null;
  }

  // Trader buyback FLOOR. SPT's GetHighestSellToTraderPrice has no category
  // filter, so traderSell = handbook × K_trader, where K_trader = max over
  // traders of (100 - loyaltyLevels[0].buy_price_coef)/100. Special traders
  // (caretaker/БТР/Storyteller) ship coef 0 → K_trader = 1.0 → floor = handbook.
  // The offer generator applies this via useTraderPriceForOffersIfHigher
  // (RagfairPriceService.GetDynamicItemPrice). Validated in-game: an override
  // pushing LEDX base to 400k still floored to handbook (~970k).
  let K_trader = 0;
  if (USE_TRADER_FLOOR) {
    const tdir = path.join(dataDir, 'database', 'traders');
    for (const tid of fs.readdirSync(tdir)) {
      const bp = path.join(tdir, tid, 'base.json');
      if (!fs.existsSync(bp)) continue;
      let b; try { b = readJson(bp); } catch (_) { continue; }
      const ll = b.loyaltyLevels || b.LoyaltyLevels;
      if (!ll || !ll.length) continue;
      const coef = ll[0].buy_price_coef ?? ll[0].BuyPriceCoefficient;
      if (coef == null) continue;
      const buyback = (100 - coef) / 100;
      if (buyback > K_trader) K_trader = buyback;
    }
  }
  console.error(`  K_trader (flea floor multiplier) = ${K_trader}${USE_TRADER_FLOOR ? '' : ' (trader floor disabled)'}`);

  // prices.json — on-disk dynamic prices. Contributes to the VANILLA flea base
  // (RagfairPriceService.ReplaceFleaBasePrices does pricePool.AddOrUpdate += bonus
  // on top of any existing value). An override REPLACES the prices.json value
  // (ApplyFleaPriceOverrides assigns into the same dict BEFORE the +=).
  // Caveat: if LiveFleaPrices is re-enabled it mutates this dict in memory at
  // boot, making the on-disk value stale for the vanilla-base display.
  const pricesPath = path.join(dataDir, 'database', 'templates', 'prices.json');
  const pricesDisk = fs.existsSync(pricesPath) ? readJson(pricesPath) : {};

  // 3c. Handbook → base price + flea formula (validated vs source + 7 in-game scenarios).
  //   bonus  = round(handbook × M(tpl))                         additive term, M incl. overrides + craft
  //   floor  = round(handbook × K_trader)                       offer base can't drop below this
  //   dynBase= (override ?? pricesDisk[tpl] ?? 0) + bonus       override replaces pricesDisk
  //   effectiveFleaPrice = max(dynBase, floor)                  what the flea actually uses
  // Viewer sets a desired price X by writing override = X − bonus (= fleaPrice).
  // See docs/flea-override-plan.md + docs/flea-formula-validation.md.
  const handbookPath = path.join(dataDir, 'database', 'templates', 'handbook.json');
  console.error('Reading handbook.json...');
  const handbook = readJson(handbookPath);
  let baseCount = 0, priceCount = 0, craftItemsWithPrice = 0, overridesApplied = 0, flooredCount = 0, cappedCount = 0;
  for (const e of handbook.Items) {
    if (!items[e.Id]) continue;
    items[e.Id].basePrice = e.Price;
    items[e.Id].handbookCategoryId = e.ParentId;
    const isCraft = hideoutCraftItems.has(e.Id);
    const multiplier = fleaMultiplierFor(e.Id) + (isCraft ? HIDEOUT_CRAFT_MULTIPLIER : 0);
    items[e.Id].isHideoutCraftItem = isCraft;
    items[e.Id].fleaMultiplier = multiplier;
    const hb = e.Price;
    const bonus = hb != null ? Math.round(hb * multiplier) : null;
    const floor = (hb != null && USE_TRADER_FLOOR) ? Math.round(hb * K_trader) : 0;
    const ceiling = fleaCeilingFor(e.Id, hb);   // null unless capped baseclass (mods ×6 / electronics ×11)
    items[e.Id].fleaPrice = bonus;     // additive term = handbook × M (what the viewer subtracts)
    items[e.Id].fleaFloor = floor;     // offer base can't drop below this (handbook × K_trader)
    items[e.Id].fleaCeiling = ceiling; // offer base can't rise above this (handbook × unreasonableMult), or null
    const ov = fleaOverridesMap[e.Id];
    items[e.Id].fleaOverride = ov != null ? ov : null;
    if (ov != null) overridesApplied++;
    const offerMult = fleaMultiplierMap[e.Id];
    items[e.Id].fleaOfferMultiplier = offerMult != null ? offerMult : null;
    if (bonus != null) {
      const dynBase = (ov != null ? ov : (pricesDisk[e.Id] ?? 0)) + bonus;
      items[e.Id].fleaBaseRaw = dynBase;   // = GetFleaPriceForItem (what itemPriceMultiplier scales)
      let eff = Math.max(dynBase, floor);
      if (offerMult != null) eff = Math.round(eff * offerMult);  // itemPriceMultiplier (offer-time)
      if (ceiling != null && eff > ceiling) { eff = ceiling; cappedCount++; }
      else if (eff > dynBase) flooredCount++;
      items[e.Id].effectiveFleaPrice = eff;
      priceCount++;
      if (isCraft) craftItemsWithPrice++;
    } else {
      items[e.Id].fleaBaseRaw = null;
      items[e.Id].effectiveFleaPrice = null;
    }
    baseCount++;
  }
  for (const id of Object.keys(items)) {
    if (items[id].basePrice          === undefined) items[id].basePrice          = null;
    if (items[id].handbookCategoryId === undefined) items[id].handbookCategoryId = null;
    if (items[id].fleaPrice          === undefined) items[id].fleaPrice          = null;
    if (items[id].fleaFloor          === undefined) items[id].fleaFloor          = 0;
    if (items[id].fleaCeiling        === undefined) items[id].fleaCeiling        = null;
    if (items[id].fleaMultiplier     === undefined) items[id].fleaMultiplier     = null;
    if (items[id].isHideoutCraftItem === undefined) items[id].isHideoutCraftItem = false;
    if (items[id].fleaOverride       === undefined) items[id].fleaOverride       = null;
    if (items[id].fleaOfferMultiplier === undefined) items[id].fleaOfferMultiplier = null;
    if (items[id].fleaBaseRaw        === undefined) items[id].fleaBaseRaw        = null;
    if (items[id].effectiveFleaPrice === undefined) items[id].effectiveFleaPrice = items[id].fleaPrice;
  }
  console.error(`  basePrice on ${baseCount}, fleaPrice on ${priceCount} (${craftItemsWithPrice} craft), overrides: ${overridesApplied}, trader-floored: ${flooredCount}, ceiling-capped: ${cappedCount}, K_trader=${K_trader}`);

  // Currency rates from handbook
  const usdEntry = handbook.Items.find(x => x.Id === CURRENCY_USD);
  const eurEntry = handbook.Items.find(x => x.Id === CURRENCY_EUR);
  const gpEntry  = handbook.Items.find(x => x.Id === CURRENCY_GP);
  const usdRate = usdEntry ? usdEntry.Price : FALLBACK_USD_RATE;
  const eurRate = eurEntry ? eurEntry.Price : FALLBACK_EUR_RATE;
  const gpRate  = gpEntry  ? gpEntry.Price  : FALLBACK_GP_RATE;
  const rateSource = (usdEntry && eurEntry && gpEntry) ? 'handbook' : 'fallback';
  console.error(`  currency rates (${rateSource}): USD=${usdRate}, EUR=${eurRate}, GP=${gpRate}`);

  // Resolve a single barter_scheme requirement to { currency, priceRUB }.
  // currency: 'RUB'|'USD'|'EUR'|'GP' for money tpls, 'BARTER' otherwise.
  // priceRUB is the requirement converted to roubles via handbook rate (null for barter).
  function resolveCurrency(reqTpl, count) {
    if (reqTpl === CURRENCY_RUB) return { currency: 'RUB', priceRUB: count };
    if (reqTpl === CURRENCY_USD) return { currency: 'USD', priceRUB: Math.round(count * usdRate) };
    if (reqTpl === CURRENCY_EUR) return { currency: 'EUR', priceRUB: Math.round(count * eurRate) };
    if (reqTpl === CURRENCY_GP)  return { currency: 'GP',  priceRUB: Math.round(count * gpRate) };
    return { currency: 'BARTER', priceRUB: null };
  }
  const MONEY_CURRENCIES = new Set(['RUB', 'USD', 'EUR', 'GP']);

  // 4a. Global flea config (player level gate)
  let fleaMinUserLevel = null;
  try {
    const globals = readJson(path.join(dataDir, 'database', 'globals.json'));
    fleaMinUserLevel = globals && globals.config && globals.config.RagFair && globals.config.RagFair.minUserLevel || null;
    console.error(`  flea unlock: minUserLevel = ${fleaMinUserLevel}`);
  } catch (e) {
    console.error('  WARNING: failed to read globals.json for minUserLevel:', e.message);
  }

  // 4b. Flea blacklist (per-item CanSellOnRagfair + ragfair.json:dynamic.blacklist.custom)
  // ragfair object already loaded above in step 3b.
  const blacklist = (ragfair.dynamic && ragfair.dynamic.blacklist) || {};
  const bsgListEnabled = blacklist.enableBsgList !== false;
  // blacklist.custom is a Set/array of tpls; in SPT 4.0 it both bans the tpl
  // from offer generation AND acts as an exception list in
  // PostDbLoadService.SetAllDbItemsAsSellableOnFlea (items not in custom get
  // their CanSellOnRagfair forced to true at boot).
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

  // 4c. Mod-added items from user/mods/*/db/CustomItems/*.json(c)
  // Mod items define name, price and traders inline — no assort.json involvement.
  // Loaded after handbook (need currency rates) and ragfair blacklist (need customBanned).
  // For mods that inject items purely in code (no CustomItems JSON), see the DEFERRED
  // fetchLiveServerItems() helper above — wire it in here when such a mod is installed.
  const modsDir = path.join(SPT_PATH, 'user', 'mods');
  const MOD_CURRENCY_TPL = {
    MONEY_ROUBLES: CURRENCY_RUB,
    MONEY_DOLLARS: CURRENCY_USD,
    MONEY_EUROS:   CURRENCY_EUR,
  };
  // Normalize mod trader name: RAGMAN → Ragman (single-word trader names)
  function normTraderName(s) {
    return s.charAt(0).toUpperCase() + s.slice(1).toLowerCase();
  }

  // Mod-enabled test. SPT C# server mods (DLL) and JS mods can be disabled
  // without removing the folder via a `.disabled` marker or a package.json with
  // "isEnabled": false. Skip those — their data must not leak into the cache.
  function isModEnabled(modDir) {
    if (fs.existsSync(path.join(modDir, '.disabled'))) return false;
    const pkgP = path.join(modDir, 'package.json');
    if (fs.existsSync(pkgP)) {
      try { const pkg = readJson(pkgP); if (pkg.isEnabled === false) return false; } catch (_) { /* ignore */ }
    }
    return true;
  }

  // ── Trader registry (vanilla + mod), built BEFORE the mod-item loop ─────────
  // Maps trader nickname → { id, avatar }. Built here (ahead of §4c and §5) so
  // mod-item offers (§4c) and assort offers (§5) can resolve traderId. Also
  // returns nameToId for name→id lookups and modTraderSources: the list of
  // mod-shipped { id, nickname, base, assort } to parse in §5 like vanilla.
  const traderAvatarsDir = path.join(dataDir, 'images', 'trader', 'avatar');
  const avatarFiles = fs.existsSync(traderAvatarsDir) ? new Set(fs.readdirSync(traderAvatarsDir)) : new Set();
  function resolveAvatar(avatarField) {
    if (!avatarField) return null;
    const filename = path.basename(avatarField);
    const stem = filename.replace(/\.[^.]+$/, '');
    const match = avatarFiles.has(filename) ? filename
                : [...avatarFiles].find(f => f.startsWith(stem + '.'));
    return match ? '/spt-images/trader/avatar/' + match : null;
  }

  const traders = {};            // nickname -> { id, avatar }  (→ data/traders.json)
  const nameToId = {};           // nickname -> traderId  (for §4c mod-offer resolution)
  const idToNick = {};           // traderId -> nickname
  function registerTrader(nickname, id, avatar) {
    if (!nickname || !id) return;
    if (!traders[nickname]) traders[nickname] = { id, avatar: avatar ?? null };
    nameToId[nickname] = id;
    idToNick[id] = nickname;
  }

  // Vanilla traders from <SPT_Data>/database/traders/<id>/base.json
  const vanillaTradersDir = path.join(dataDir, 'database', 'traders');
  const vanillaTraderIds = fs.existsSync(vanillaTradersDir)
    ? fs.readdirSync(vanillaTradersDir).filter(d => fs.statSync(path.join(vanillaTradersDir, d)).isDirectory())
    : [];
  for (const traderId of vanillaTraderIds) {
    const baseP = path.join(vanillaTradersDir, traderId, 'base.json');
    if (!fs.existsSync(baseP)) continue;
    let base; try { base = readJson(baseP); } catch (_) { continue; }
    registerTrader(base.nickname || traderId, traderId, resolveAvatar(base.avatar));
  }

  // Mod traders. Two layouts:
  //   (a) WTT-Artem pattern: <mod>/db/base.json + <mod>/db/assort.json
  //   (b) alt pattern:       <mod>/db/traders/<id>/base.json + assort.json
  // A db/base.json is only treated as a trader if it has nickname + _id +
  // sell_category (the field BSG uses to mark a sellable trader). Disabled mods
  // are skipped. Each accepted trader contributes its assort to §5.
  const modTraderSources = []; // { modName, id, nickname, baseDir }
  function tryRegisterModTrader(modName, base, baseDir) {
    if (!base || !base.nickname || !base._id || !base.sell_category) return false;
    registerTrader(base.nickname, base._id, resolveAvatar(base.avatar));
    modTraderSources.push({ modName, id: base._id, nickname: base.nickname, baseDir });
    return true;
  }
  if (fs.existsSync(modsDir)) {
    for (const modName of fs.readdirSync(modsDir)) {
      const modDir = path.join(modsDir, modName);
      let st; try { st = fs.statSync(modDir); } catch (_) { continue; }
      if (!st.isDirectory() || !isModEnabled(modDir)) continue;

      // (a) <mod>/db/base.json
      const baseP = path.join(modDir, 'db', 'base.json');
      if (fs.existsSync(baseP)) {
        let base; try { base = readJson(baseP); } catch (_) { base = null; }
        if (base && tryRegisterModTrader(modName, base, path.join(modDir, 'db'))) {
          console.error(`  mod trader: ${base.nickname} (${modName})`);
        }
      }
      // (b) <mod>/db/traders/<id>/base.json
      const modTradersDir = path.join(modDir, 'db', 'traders');
      if (fs.existsSync(modTradersDir)) {
        for (const tid of fs.readdirSync(modTradersDir)) {
          const tDir = path.join(modTradersDir, tid);
          let tst; try { tst = fs.statSync(tDir); } catch (_) { continue; }
          if (!tst.isDirectory()) continue;
          const bP = path.join(tDir, 'base.json');
          if (!fs.existsSync(bP)) continue;
          let base; try { base = readJson(bP); } catch (_) { base = null; }
          if (base && tryRegisterModTrader(modName, base, tDir)) {
            console.error(`  mod trader: ${base.nickname} (${modName}/db/traders/${tid})`);
          }
        }
      }
    }
  }
  console.error(`Trader registry: ${Object.keys(traders).length} traders (${vanillaTraderIds.length} vanilla, ${modTraderSources.length} mod)`);

  let modItemCount = 0;
  if (fs.existsSync(modsDir)) {
    for (const modName of fs.readdirSync(modsDir)) {
      const ciDir = path.join(modsDir, modName, 'db', 'CustomItems');
      if (!fs.existsSync(ciDir)) continue;

      const files = fs.readdirSync(ciDir).filter(f => /\.jsonc?$/i.test(f));
      let modFilesCount = 0;
      for (const file of files) {
        const fp = path.join(ciDir, file);
        let raw;
        try {
          raw = file.toLowerCase().endsWith('.jsonc') ? readJsonc(fp) : readJson(fp);
        } catch (e) {
          console.error(`  WARNING: ${modName}/${file}: parse error: ${e.message}`);
          continue;
        }

        for (const [tpl, def] of Object.entries(raw)) {
          if (items[tpl]) {
            console.error(`  WARNING: ${modName}/${file}: tpl ${tpl} already exists, skipping`);
            continue;
          }
          const op  = def.overrideProperties || {};
          const loc = (def.locales && def.locales.en) || {};

          let grids = null;
          if (Array.isArray(op.Grids) && op.Grids.length > 0) {
            grids = op.Grids
              .map(g => ({ name: g._name || null, cellsH: (g._props && g._props.cellsH) || 0, cellsV: (g._props && g._props.cellsV) || 0 }))
              .filter(g => g.cellsH > 0 && g.cellsV > 0);
            if (!grids.length) grids = null;
          }

          // Trader offers from mod JSON: { TRADERNAME: { assortId: { barterSettings, barters } } }
          // traderId resolved from the registry built above (vanilla + mod).
          const modTraders = [];
          if (def.addtoTraders && def.traders) {
            for (const [nameUpper, offers] of Object.entries(def.traders)) {
              const nick     = normTraderName(nameUpper);
              const traderId = nameToId[nick] || nameToId[nameUpper] || null;
              for (const offer of Object.values(offers)) {
                const bs      = offer.barterSettings || {};
                const barters = offer.barters || [];
                if (barters.length !== 1) continue; // skip multi-req barters
                const req = barters[0];
                const currTpl = MOD_CURRENCY_TPL[req._tpl] || req._tpl;
                const { currency, priceRUB } = resolveCurrency(currTpl, req.count);
                const loyaltyLevel = bs.loyalLevel ?? 1;
                modTraders.push({
                  name: nick,
                  traderId,
                  priceRUB,
                  currency,
                  loyaltyLevel,
                  unlimited: !!bs.unlimitedCount,
                  stock: bs.unlimitedCount ? null : (bs.stackObjectsCount ?? 0),
                  questLocked: false,
                  editable: MONEY_CURRENCIES.has(currency) && traderId !== FENCE_ID,
                });
              }
            }
          }

          const canSellOnRagfair = op.CanSellOnRagfair !== false && !op.QuestItem;
          const fleaBanReasons   = [];
          if (!canSellOnRagfair)    fleaBanReasons.push('bsg');
          if (customBanned.has(tpl)) fleaBanReasons.push('custom');

          // Flea math for mod items (validated in-game 2026-06-07): SPT's
          // CustomItemService adds the item to the handbook (handbookPriceRoubles)
          // AND sets Prices[tpl] = fleaPriceRoubles. That mod write runs AFTER
          // ApplyFleaPriceOverrides, so ragfair overrides are WIPED for mod items
          // (fleaOverride stays null — the viewer can't override these). Then
          // ReplaceFleaBasePrices adds handbook×M. Net:
          //   effectiveFleaPrice = clamp( fleaPriceRoubles + basePrice×M , floor , ceiling )
          // Confirmed: Thermaster 125921+75945×1.5=239839; Citadel 15850+12000×1.5=33850;
          // Fanny 10900+7250×1.5=21775.
          const mhb   = def.handbookPriceRoubles ?? null;   // basePrice
          const mflea = def.fleaPriceRoubles     ?? null;   // mod's prices.json contribution
          if (mhb != null) parentById.set(tpl, def.parentId || null);  // let isOfBaseclass walk mod items
          const mIsCraft = hideoutCraftItems.has(tpl);
          const mMult    = mhb != null ? fleaMultiplierFor(tpl) + (mIsCraft ? HIDEOUT_CRAFT_MULTIPLIER : 0) : null;
          const mBonus   = (mhb != null && mMult != null) ? Math.round(mhb * mMult) : null;
          const mFloor   = (mhb != null && USE_TRADER_FLOOR) ? Math.round(mhb * K_trader) : 0;
          const mCeiling = fleaCeilingFor(tpl, mhb);
          // Offer-time multiplier (itemPriceMultiplier) — the lever the viewer uses
          // to edit MOD-item prices (overrides don't work here, the mod re-sets the
          // price). Applied to the raw base, before the ceiling, matching SPT.
          const mOfferMult = fleaMultiplierMap[tpl];
          const mBaseRaw   = (mBonus != null) ? ((mflea ?? 0) + mBonus) : null;  // = GetFleaPriceForItem
          let mEff = null;
          if (mBaseRaw != null) {
            mEff = Math.max(mBaseRaw, mFloor);
            if (mOfferMult != null) mEff = Math.round(mEff * mOfferMult);
            if (mCeiling != null && mEff > mCeiling) mEff = mCeiling;
          }

          items[tpl] = {
            id: tpl,
            internalName: tpl,
            parentClassId: def.parentId || null,
            name:      loc.name      || tpl,
            shortName: loc.shortName || loc.name || tpl,
            weight:       op.Weight       ?? null,
            width:        op.Width        ?? null,
            height:       op.Height       ?? null,
            stackMaxSize: op.StackMaxSize ?? null,
            grids,
            conditionType: deriveConditionType(op),
            canSellOnRagfair,
            basePrice:          mhb,
            handbookCategoryId: def.handbookParentId    || null,
            fleaPrice:          mBonus,        // additive bonus = basePrice × M (consistent w/ base items)
            fleaFloor:          mFloor,
            fleaCeiling:        mCeiling,
            fleaMultiplier:     mMult,
            isHideoutCraftItem: mIsCraft,
            fleaOverride:       null,          // override has no effect on mod items (mod re-sets the price)
            fleaOfferMultiplier: mOfferMult != null ? mOfferMult : null,  // itemPriceMultiplier[tpl] — the working lever for mod items
            fleaBaseRaw:        mBaseRaw,      // = fleaPriceRoubles + basePrice×M (what the multiplier scales)
            effectiveFleaPrice: mEff,          // = clamp(fleaBaseRaw × offerMult, floor, ceiling)
            fleaBanned:        fleaBanReasons.length > 0,
            fleaBanReasons,
            traders: modTraders,
            modSource: modName,
            cloneTpl: def.itemTplToClone || null,  // mod items clone a vanilla base; used as image proxy in normalize
          };
          modFilesCount++;
          modItemCount++;
        }
      }
      if (modFilesCount > 0) console.error(`  ${modName}: +${modFilesCount} items`);
    }
  } else {
    console.error(`  WARNING: user/mods dir not found at ${modsDir}`);
  }
  console.error(`Mod items total: ${modItemCount}`);

  // 5. Trader assorts (vanilla + mod). The registry (nickname/id/avatar) was
  // already built above (§4c preamble); here we parse each trader's assort —
  // top offers (parentId==='hideout'), barter_scheme, loyal_level_items — the
  // same shape for vanilla and mod traders (WTT-Artem pattern + db/traders/<id>).
  // NOTE: do NOT zero items[].traders — mod items carry inline offers from §4c.
  for (const id of Object.keys(items)) { if (!Array.isArray(items[id].traders)) items[id].traders = []; }

  let totalOffers = 0;
  let barterOffers = 0;
  let questLockedOffers = 0;

  // Unified source list: { nickname, id, baseDir } for vanilla and mod traders.
  const assortSources = [];
  for (const traderId of vanillaTraderIds) {
    const nick = idToNick[traderId] || traderId;
    assortSources.push({ nickname: nick, id: traderId, baseDir: path.join(vanillaTradersDir, traderId) });
  }
  for (const src of modTraderSources) {
    assortSources.push({ nickname: src.nickname, id: src.id, baseDir: src.baseDir });
  }

  for (const { nickname, id: traderId, baseDir } of assortSources) {
    const assortP = path.join(baseDir, 'assort.json');
    const questAP = path.join(baseDir, 'questassort.json');
    if (!fs.existsSync(assortP)) {
      console.error(`  ${nickname}: missing assort.json, skipping offers`);
      continue;
    }
    let assort; try { assort = readJson(assortP); } catch (e) {
      console.error(`  ${nickname}: assort.json parse failed (${e.message})`); continue;
    }
    if (!Array.isArray(assort.items)) continue;

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

    const barterScheme = assort.barter_scheme || {};
    const loyalItems   = assort.loyal_level_items || {};

    // Iterate top-level offers
    const topItems = assort.items.filter(x => x.parentId === 'hideout');
    let traderOffers = 0;
    for (const offer of topItems) {
      const tpl = offer._tpl;
      if (!items[tpl]) continue; // not in base items or known mods

      // Resolve price/currency (RUB/USD/EUR/GP money, else BARTER)
      const scheme = barterScheme[offer._id];
      let priceRUB = null;
      let currency = 'BARTER';
      if (Array.isArray(scheme) && Array.isArray(scheme[0]) && scheme[0].length === 1) {
        const req = scheme[0][0];
        ({ currency, priceRUB } = resolveCurrency(req._tpl, req.count));
      }
      if (currency === 'BARTER') barterOffers++;

      const upd = offer.upd || {};
      const questLocked = questLockedIds.has(offer._id);
      if (questLocked) questLockedOffers++;
      const loyaltyLevel = loyalItems[offer._id] ?? 1;

      items[tpl].traders.push({
        name: nickname,
        traderId,
        priceRUB,
        currency,
        loyaltyLevel,
        unlimited: !!upd.UnlimitedCount,
        stock: upd.UnlimitedCount ? null : (upd.StackObjectsCount ?? 0),
        questLocked,
        // Editable only for money offers from a non-Fence trader.
        editable: MONEY_CURRENCIES.has(currency) && traderId !== FENCE_ID,
      });
      traderOffers++;
    }
    totalOffers += traderOffers;
    console.error(`  ${nickname.padEnd(15)} ${traderOffers} offers`);
  }

  // Dedup intra-trader: keep ONE offer per (trader, tpl). Preference order:
  //   1. an editable (money) offer beats a barter offer (so the editor has a price)
  //   2. then lowest loyalty level
  //   3. then lowest priceRUB
  // Retained offer always carries traderId + currency.
  let dedupRemoved = 0;
  for (const tpl of Object.keys(items)) {
    const offers = items[tpl].traders;
    if (offers.length < 2) continue;
    const byTrader = {};
    for (const o of offers) {
      const key = o.traderId || o.name; // group by trader id (fallback name)
      const prev = byTrader[key];
      if (!prev) { byTrader[key] = o; continue; }
      const oMoney    = o.editable === true;
      const prevMoney = prev.editable === true;
      let replace;
      if (oMoney !== prevMoney) {
        replace = oMoney; // prefer the money/editable offer
      } else {
        replace =
          (o.loyaltyLevel < prev.loyaltyLevel) ||
          (o.loyaltyLevel === prev.loyaltyLevel && (o.priceRUB ?? Infinity) < (prev.priceRUB ?? Infinity));
      }
      if (replace) byTrader[key] = o;
      dedupRemoved++;
    }
    items[tpl].traders = Object.values(byTrader);
  }

  console.error(`Trader offers: ${totalOffers} total, ${barterOffers} barter, ${questLockedOffers} quest-locked, ${dedupRemoved} dedup-removed`);

  // 5.qr Quest item rewards — build map { tpl: [{ questId, name, trader, count }] }
  console.error('Reading quests.json (item rewards)...');
  try {
    const quests = readJson(path.join(dataDir, 'database', 'templates', 'quests.json'));
    const questRewardsByTpl = {};
    let questsWithRewards = 0, rewardEntries = 0;
    const traderNameById = {};
    for (const [nick, t] of Object.entries(traders)) traderNameById[t.id] = nick;
    for (const [qid, q] of Object.entries(quests)) {
      const succ = q.rewards && q.rewards.Success;
      if (!Array.isArray(succ)) continue;
      let hadAny = false;
      for (const r of succ) {
        if (r.type !== 'Item' || !Array.isArray(r.items) || !r.items[0]) continue;
        const it0 = r.items[0];
        const tpl = it0._tpl;
        if (!tpl || !items[tpl]) continue;
        const count = (it0.upd && it0.upd.StackObjectsCount) || 1;
        const qname = locale[qid + ' name'] || q.QuestName || qid;
        const tname = traderNameById[q.traderId] || null;
        if (!questRewardsByTpl[tpl]) questRewardsByTpl[tpl] = [];
        questRewardsByTpl[tpl].push({ questId: qid, name: qname, trader: tname, count });
        hadAny = true; rewardEntries++;
      }
      if (hadAny) questsWithRewards++;
    }
    console.error(`  quests: ${Object.keys(quests).length}, with item rewards: ${questsWithRewards}, total entries: ${rewardEntries}, distinct tpls: ${Object.keys(questRewardsByTpl).length}`);
    for (const tpl of Object.keys(questRewardsByTpl)) {
      if (items[tpl]) items[tpl].questRewards = questRewardsByTpl[tpl];
    }
  } catch (e) {
    console.error('  WARNING: failed to read quests.json:', e.message);
  }

  // 6. Emit cache
  const out = {
    loadedAt: new Date().toISOString(),
    sptDataPath: dataDir,
    currencyRates: { source: rateSource, USD: usdRate, EUR: eurRate },
    fleaPricesSource: 'handbook',
    // mtime of handbook.json — answers "when were base prices last edited?"
    fleaPricesMtime: fs.statSync(handbookPath).mtime.toISOString(),
    fleaMinUserLevel,
    fleaBlacklist: {
      bsgListEnabled,
      customCount: customBanned.size,
      questListEnabled: blacklist.enableQuestList === true,    // not resolved (limitation)
      traderItemsBanned: blacklist.traderItems === true,        // not resolved (limitation)
      damagedAmmoPacksBanned: blacklist.damagedAmmoPacks === true,
    },
    counts: {
      items: Object.keys(items).length,
      modItems: modItemCount,
      withFleaPrice: priceCount,
      withBasePrice: baseCount,
      traders: Object.keys(traders).length,
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
