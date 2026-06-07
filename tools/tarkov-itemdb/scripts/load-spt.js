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
const FALLBACK_USD_RATE = 120; // RUB per USD; verified from handbook
const FALLBACK_EUR_RATE = 133; // RUB per EUR; verified from handbook

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
    if (bonus != null) {
      const dynBase = (ov != null ? ov : (pricesDisk[e.Id] ?? 0)) + bonus;
      let eff = Math.max(dynBase, floor);
      if (ceiling != null && eff > ceiling) { eff = ceiling; cappedCount++; }
      else if (eff > dynBase) flooredCount++;
      items[e.Id].effectiveFleaPrice = eff;
      priceCount++;
      if (isCraft) craftItemsWithPrice++;
    } else {
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
    if (items[id].effectiveFleaPrice === undefined) items[id].effectiveFleaPrice = items[id].fleaPrice;
  }
  console.error(`  basePrice on ${baseCount}, fleaPrice on ${priceCount} (${craftItemsWithPrice} craft), overrides: ${overridesApplied}, trader-floored: ${flooredCount}, ceiling-capped: ${cappedCount}, K_trader=${K_trader}`);

  // Currency rates from handbook
  const usdEntry = handbook.Items.find(x => x.Id === CURRENCY_USD);
  const eurEntry = handbook.Items.find(x => x.Id === CURRENCY_EUR);
  const usdRate = usdEntry ? usdEntry.Price : FALLBACK_USD_RATE;
  const eurRate = eurEntry ? eurEntry.Price : FALLBACK_EUR_RATE;
  const rateSource = (usdEntry && eurEntry) ? 'handbook' : 'fallback';
  console.error(`  currency rates (${rateSource}): USD=${usdRate}, EUR=${eurRate}`);

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
          const modTraders = [];
          if (def.addtoTraders && def.traders) {
            for (const [nameUpper, offers] of Object.entries(def.traders)) {
              for (const offer of Object.values(offers)) {
                const bs      = offer.barterSettings || {};
                const barters = offer.barters || [];
                if (barters.length !== 1) continue; // skip multi-req barters
                const req = barters[0];
                const currTpl = MOD_CURRENCY_TPL[req._tpl] || req._tpl;
                let priceRUB = null;
                let currency = 'BARTER';
                if      (currTpl === CURRENCY_RUB) { priceRUB = req.count;                          currency = 'RUB'; }
                else if (currTpl === CURRENCY_USD) { priceRUB = Math.round(req.count * usdRate);   currency = 'USD'; }
                else if (currTpl === CURRENCY_EUR) { priceRUB = Math.round(req.count * eurRate);   currency = 'EUR'; }
                modTraders.push({
                  name: normTraderName(nameUpper),
                  priceRUB,
                  currency,
                  loyaltyLevel: bs.loyalLevel ?? 1,
                  unlimited: !!bs.unlimitedCount,
                  stock: bs.unlimitedCount ? null : (bs.stackObjectsCount ?? 0),
                  questLocked: false,
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
          let mEff = null;
          if (mBonus != null) {
            mEff = Math.max((mflea ?? 0) + mBonus, mFloor);
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
            effectiveFleaPrice: mEff,          // = fleaPriceRoubles + basePrice×M (clamped)
            fleaBanned:        fleaBanReasons.length > 0,
            fleaBanReasons,
            traders: modTraders,
            modSource: modName,
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
      if (!items[tpl]) continue; // not in base items or known mods

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
