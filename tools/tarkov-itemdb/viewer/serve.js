#!/usr/bin/env node
/**
 * Tiny static server for the viewer. Serves files from tools/tarkov-itemdb/
 * so the viewer can fetch ../data/items.json without CORS / file:// issues.
 *
 * Usage:  node viewer/serve.js [port]
 * Open:   http://localhost:8080/viewer/
 */
'use strict';

const http   = require('http');
const https  = require('https');
const fs     = require('fs');
const path   = require('path');
const crypto = require('crypto');

const PORT     = parseInt(process.argv[2] || '8080', 10);
const ROOT     = path.resolve(__dirname, '..');
const SPT_PATH = process.env.SPT_PATH || 'D:/SPT/SPT';
const SPT_DATA = path.join(SPT_PATH, 'SPT_Data');
const SPT_ITEMS_JSON = path.join(SPT_DATA, 'database', 'templates', 'items.json');
const HANDBOOK_JSON  = path.join(SPT_DATA, 'database', 'templates', 'handbook.json');
const GLOBALS_JSON   = path.join(SPT_DATA, 'database', 'globals.json');
const RAGFAIR_JSON   = path.join(SPT_DATA, 'configs', 'ragfair.json');
const PRICES_JSON    = path.join(SPT_DATA, 'database', 'templates', 'prices.json');
const META_JSON      = path.resolve(__dirname, '..', 'data', 'meta.json');
const ITEMS_JSON   = path.join(ROOT, 'data', 'items.json');
const CHECKS_DAT   = path.join(SPT_DATA, 'checks.dat');
const LOG_FILE              = path.join(ROOT, 'logs', 'price-edits.jsonl');
const BAN_LOG_FILE          = path.join(ROOT, 'logs', 'ban-edits.jsonl');
const HISTORY_LOG_FILE      = path.join(ROOT, 'logs', 'price-history.jsonl');
const TARKOV_DEV_URL   = 'https://api.tarkov.dev/graphql';

const MIME = {
  '.html': 'text/html; charset=utf-8',
  '.js':   'application/javascript; charset=utf-8',
  '.css':  'text/css; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.svg':  'image/svg+xml',
  '.png':  'image/png',
  '.webp': 'image/webp',
};

function readJsonFile(p) {
  return JSON.parse(fs.readFileSync(p, 'utf8').replace(/^﻿/, ''));
}

// SPT 4.0 validates each templates/*.json against an MD5 manifest stored in
// checks.dat (base64-encoded JSON array of {Path, Hash}). When we modify a
// tracked file we must update its hash here, otherwise SPT logs "validação de
// arquivo falhou" on boot. Paths use forward slashes relative to SPT_Data.
function updateSptChecks(updates /* { 'database/templates/prices.json': '<absPath>' } */) {
  if (!fs.existsSync(CHECKS_DAT)) return { ok: false, error: 'checks.dat not found' };
  const raw = fs.readFileSync(CHECKS_DAT, 'utf8');
  const manifest = JSON.parse(Buffer.from(raw, 'base64').toString('utf8'));
  const changes = [];
  for (const [relPath, absPath] of Object.entries(updates)) {
    const buf = fs.readFileSync(absPath);
    const md5 = crypto.createHash('md5').update(buf).digest('hex').toUpperCase();
    const entry = manifest.find(x => x.Path === relPath);
    if (!entry) { changes.push({ relPath, status: 'not-in-manifest' }); continue; }
    if (entry.Hash === md5) { changes.push({ relPath, status: 'already-current' }); continue; }
    const prev = entry.Hash;
    entry.Hash = md5;
    changes.push({ relPath, status: 'updated', from: prev, to: md5 });
  }
  // Re-encode and write back, preserving original 2-space JSON style + base64
  // + trailing newline (the original .dat ends with a \n after the base64 blob).
  const reencoded = Buffer.from(JSON.stringify(manifest, null, 2), 'utf8').toString('base64') + '\n';
  fs.writeFileSync(CHECKS_DAT, reencoded, 'utf8');
  return { ok: true, changes };
}

function sendJson(res, status, body) {
  const data = JSON.stringify(body);
  res.writeHead(status, { 'Content-Type': 'application/json; charset=utf-8', 'Content-Length': Buffer.byteLength(data) });
  res.end(data);
}

// Append-only audit log of every successful price edit. JSONL = one object
// per line, easy to grep / parse / tail.
function appendEditLog(entry) {
  try {
    fs.mkdirSync(path.dirname(LOG_FILE), { recursive: true });
    fs.appendFileSync(LOG_FILE, JSON.stringify(entry) + '\n', 'utf8');
  } catch (e) {
    console.error('audit log write failed:', e.message);
  }
}

// Write a JSON file preserving the original indent style (SPT vanilla configs
// use TAB) and trailing-newline state, atomically (tmp + rename). Keeps diffs
// against vanilla minimal and avoids partial writes.
function writeJsonPreservingStyle(p, obj) {
  const original = fs.readFileSync(p, 'utf8').replace(/^﻿/, '');
  const m = original.match(/^([\t ]+)"/m);
  const indent = !m ? '\t' : (m[1][0] === '\t' ? '\t' : m[1].length);
  let serialized = JSON.stringify(obj, null, indent);
  if (original.endsWith('\n')) serialized += '\n';
  const tmp = p + '.tmp';
  fs.writeFileSync(tmp, serialized, 'utf8');
  fs.renameSync(tmp, p);
}

// In-process mutex so concurrent override writes don't read-modify-write the
// same ragfair.json / items.json and lose updates. Serialises the price APIs.
let _writeChain = Promise.resolve();
function withWriteLock(fn) {
  const run = _writeChain.then(fn, fn);
  // Keep the chain alive regardless of success/failure of fn.
  _writeChain = run.then(() => {}, () => {});
  return run;
}

// The flea offer base for a tpl = clamp((override ?? prices.json ?? 0) + bonus,
// floor, ceiling). bonus = spt.fleaPrice (handbook × M), floor = spt.fleaFloor
// (handbook × K_trader), ceiling = spt.fleaCeiling (handbook × unreasonableMult
// for Weapon Mods/Electronics, else null). All from data/items.json (load-spt.js,
// validated vs source + in-game). The viewer sets a desired price X by writing
// override = X − bonus into ragfair.json:dynamic.itemPriceOverrideRouble[tpl]
// (valid for floor ≤ X ≤ ceiling).
function effectivePrice(overrideOrNull, bonus, floor, ceiling, pricesDiskVal) {
  const base = (overrideOrNull != null ? overrideOrNull : (pricesDiskVal ?? 0)) + bonus;
  let eff = Math.max(base, floor || 0);
  if (ceiling != null && eff > ceiling) eff = ceiling;
  return eff;
}

function appendBanLog(entry) {
  try {
    fs.mkdirSync(path.dirname(BAN_LOG_FILE), { recursive: true });
    fs.appendFileSync(BAN_LOG_FILE, JSON.stringify(entry) + '\n', 'utf8');
  } catch (e) {
    console.error('ban audit log write failed:', e.message);
  }
}

// Append a snapshot of price metrics for a given Tpl. Used to build a
// time-series chart later. Each line is a self-contained event.
function appendHistory(entry) {
  try {
    fs.mkdirSync(path.dirname(HISTORY_LOG_FILE), { recursive: true });
    fs.appendFileSync(HISTORY_LOG_FILE, JSON.stringify(entry) + '\n', 'utf8');
  } catch (e) {
    console.error('history log write failed:', e.message);
  }
}

// Fetch a single item from tarkov.dev GraphQL (PVE + regular flea metrics
// + vendor offers). Returns { pve, regular } or throws.
function fetchTarkovDevItem(tpl) {
  return new Promise((resolve, reject) => {
    const query = `query Single($id: ID!) {
      pve: item(id: $id, gameMode: pve) {
        id name shortName
        avg24hPrice lastLowPrice low24hPrice high24hPrice changeLast48h changeLast48hPercent updated
        sellFor { vendor { name normalizedName } price priceRUB currency }
        buyFor  { vendor { name normalizedName } price priceRUB currency }
      }
      regular: item(id: $id, gameMode: regular) {
        avg24hPrice lastLowPrice low24hPrice high24hPrice changeLast48h changeLast48hPercent updated
        sellFor { vendor { name normalizedName } price priceRUB currency }
        buyFor  { vendor { name normalizedName } price priceRUB currency }
      }
    }`;
    const body = Buffer.from(JSON.stringify({ query, variables: { id: tpl } }));
    const u = new URL(TARKOV_DEV_URL);
    const req = https.request({
      method: 'POST', hostname: u.hostname, path: u.pathname,
      headers: { 'Content-Type': 'application/json', 'Content-Length': body.length },
    }, (res) => {
      const chunks = [];
      res.on('data', c => chunks.push(c));
      res.on('end', () => {
        try {
          const data = JSON.parse(Buffer.concat(chunks).toString('utf8'));
          if (data.errors) return reject(new Error('GraphQL: ' + JSON.stringify(data.errors)));
          if (!data.data || !data.data.pve) return reject(new Error('item not found on tarkov.dev'));
          resolve(data.data);
        } catch (e) { reject(e); }
      });
    });
    req.on('error', reject);
    req.setTimeout(15000, () => { req.destroy(new Error('tarkov.dev timeout')); });
    req.write(body);
    req.end();
  });
}

// Fetch a single item from tarkov-market.com (PVE prices). The API has no
// bsgId lookup — only by name (q) or uid — so we query by name and filter the
// results by bsgId to pin the exact tpl. Requires TARKOV_MARKET_API_KEY.
function fetchTarkovMarketItem(name, tpl) {
  return new Promise((resolve, reject) => {
    const apiKey = process.env.TARKOV_MARKET_API_KEY;
    if (!apiKey) return reject(new Error('TARKOV_MARKET_API_KEY not set — restart serve.js with the env var to use tarkov-market refresh'));
    if (!name)   return reject(new Error('item has no name to query tarkov-market'));
    const u = new URL('https://api.tarkov-market.app/api/v1/pve/item');
    u.searchParams.set('q', name);
    const req = https.request({
      method: 'GET', hostname: u.hostname, path: u.pathname + u.search,
      headers: { 'x-api-key': apiKey },
    }, (res) => {
      const chunks = [];
      res.on('data', c => chunks.push(c));
      res.on('end', () => {
        if (res.statusCode !== 200) {
          return reject(new Error(`tarkov-market HTTP ${res.statusCode}` + (res.statusCode === 401 ? ' (bad/missing API key)' : '')));
        }
        try {
          const data = JSON.parse(Buffer.concat(chunks).toString('utf8'));
          const arr = Array.isArray(data) ? data : (data ? [data] : []);
          // Prefer the exact bsgId match; fall back to the sole result if the
          // name query returned exactly one item.
          const match = arr.find(x => x && x.bsgId === tpl) || (arr.length === 1 ? arr[0] : null);
          if (!match) return reject(new Error(`item not found on tarkov-market (q="${name}", ${arr.length} results)`));
          resolve(match);
        } catch (e) { reject(e); }
      });
    });
    req.on('error', reject);
    req.setTimeout(15000, () => { req.destroy(new Error('tarkov-market timeout')); });
    req.end();
  });
}

function handleRefreshDev(req, res) {
  let body = '';
  req.on('data', c => { body += c; if (body.length > 1e6) req.destroy(); });
  req.on('end', async () => {
    let payload;
    try { payload = JSON.parse(body); }
    catch (e) { return sendJson(res, 400, { error: 'invalid JSON' }); }
    const tpl = payload.tpl;
    if (typeof tpl !== 'string' || !/^[a-f0-9]{24}$/i.test(tpl)) {
      return sendJson(res, 400, { error: 'invalid tpl' });
    }
    try {
      const fresh = await fetchTarkovDevItem(tpl);
      const items = readJsonFile(ITEMS_JSON);
      if (!items[tpl]) return sendJson(res, 404, { error: 'tpl not in data/items.json' });

      const item = items[tpl];
      const prevPve = item.tarkovDev && item.tarkovDev.pve ? {
        lastLow: item.tarkovDev.pve.lastLow,
        avg24h:  item.tarkovDev.pve.avg24h,
        low24h:  item.tarkovDev.pve.low24h,
        high24h: item.tarkovDev.pve.high24h,
        updated: item.tarkovDev.pve.updated,
      } : null;

      function mapMode(m) {
        if (!m) return null;
        return {
          lastLow:      m.lastLowPrice         ?? null,
          avg24h:       m.avg24hPrice          ?? null,
          low24h:       m.low24hPrice          ?? null,
          high24h:      m.high24hPrice         ?? null,
          change48h:    m.changeLast48h        ?? null,
          change48hPct: m.changeLast48hPercent ?? null,
          updated:      m.updated              ?? null,
          sellFor:      m.sellFor              || [],
          buyFor:       m.buyFor               || [],
        };
      }
      item.tarkovDev = {
        pve:     mapMode(fresh.pve),
        regular: mapMode(fresh.regular),
      };

      // Recompute consolidated (mirrors normalize.js logic)
      recomputeConsolidated(item);

      // Persist + sync checks.dat (items.json hash changed)
      fs.writeFileSync(ITEMS_JSON, serializeItems(items), 'utf8');

      // History snapshot — current + previous, easy to chart later
      appendHistory({
        at: new Date().toISOString(),
        tpl,
        name: item.name || null,
        shortName: item.shortName || null,
        source: 'tarkov.dev',
        previous: prevPve,
        current: item.tarkovDev.pve ? {
          lastLow: item.tarkovDev.pve.lastLow,
          avg24h:  item.tarkovDev.pve.avg24h,
          low24h:  item.tarkovDev.pve.low24h,
          high24h: item.tarkovDev.pve.high24h,
          change48h: item.tarkovDev.pve.change48h,
          change48hPct: item.tarkovDev.pve.change48hPct,
          updated: item.tarkovDev.pve.updated,
        } : null,
        ip: req.socket.remoteAddress || null,
      });

      return sendJson(res, 200, {
        ok: true, tpl,
        previous: prevPve,
        tarkovDev: item.tarkovDev,
        consolidated: item.consolidated,
      });
    } catch (e) {
      console.error('refresh-dev failed:', e);
      return sendJson(res, 502, { error: e.message });
    }
  });
}

// POST /api/refresh-market — re-fetch one item's PVE prices from tarkov-market
// and update item.tarkovMarket.pve + consolidated. Mirrors /api/refresh-dev.
function handleRefreshMarket(req, res) {
  let body = '';
  req.on('data', c => { body += c; if (body.length > 1e6) req.destroy(); });
  req.on('end', async () => {
    let payload;
    try { payload = JSON.parse(body); }
    catch (e) { return sendJson(res, 400, { error: 'invalid JSON' }); }
    const tpl = payload.tpl;
    if (typeof tpl !== 'string' || !/^[a-f0-9]{24}$/i.test(tpl)) {
      return sendJson(res, 400, { error: 'invalid tpl' });
    }
    try {
      // Read the name to query tarkov-market (network happens OUTSIDE the write lock).
      const pre = readJsonFile(ITEMS_JSON)[tpl];
      if (!pre) return sendJson(res, 404, { error: 'tpl not in data/items.json' });
      const m = await fetchTarkovMarketItem(pre.name, tpl);
      const mapped = {
        avg24h:      m.avg24hPrice   ?? null,
        avg7days:    m.avg7daysPrice ?? null,
        price:       m.price         ?? null,
        traderName:  m.traderName    ?? null,
        traderPrice: m.traderPrice   ?? null,
        link:        m.link          ?? null,
        updated:     m.updated       ?? null,
      };
      return withWriteLock(() => {
        const items = readJsonFile(ITEMS_JSON);
        const item = items[tpl];
        if (!item) return sendJson(res, 404, { error: 'tpl not in data/items.json' });
        const prev = item.tarkovMarket && item.tarkovMarket.pve
          ? { avg24h: item.tarkovMarket.pve.avg24h, price: item.tarkovMarket.pve.price, updated: item.tarkovMarket.pve.updated }
          : null;
        item.tarkovMarket = { pve: mapped };
        recomputeConsolidated(item);
        fs.writeFileSync(ITEMS_JSON, serializeItems(items), 'utf8');
        appendHistory({
          at: new Date().toISOString(), tpl,
          name: item.name || null, shortName: item.shortName || null,
          source: 'tarkov-market', previous: prev, current: mapped,
          ip: req.socket.remoteAddress || null,
        });
        return sendJson(res, 200, {
          ok: true, tpl, previous: prev,
          tarkovMarket: item.tarkovMarket,
          consolidated: item.consolidated,
        });
      }).catch(e => {
        console.error('refresh-market write failed:', e);
        if (!res.headersSent) return sendJson(res, 500, { error: e.message });
      });
    } catch (e) {
      console.error('refresh-market failed:', e);
      return sendJson(res, 502, { error: e.message });
    }
  });
}

// Run a pipeline script as a child process, inheriting env (so the API key
// reaches fetch-tarkov-market). Resolves on exit 0, rejects with tail of stderr.
function runScript(scriptName, args) {
  return new Promise((resolve, reject) => {
    const { spawn } = require('child_process');
    const child = spawn(process.execPath, [path.join(ROOT, 'scripts', scriptName), ...args], {
      cwd: ROOT, env: process.env,
    });
    let stderr = '';
    child.stderr.on('data', d => { stderr += d.toString(); });
    child.on('error', reject);
    child.on('close', code => {
      if (code === 0) return resolve();
      const tail = stderr.trim().split('\n').slice(-3).join(' | ').slice(0, 400);
      reject(new Error(`${scriptName} exited ${code}: ${tail}`));
    });
  });
}

// POST /api/refresh-all — bulk update ALL items from one source by re-fetching
// the full dump and re-merging. NOT per-item (tarkov-market is 5 req/min → per
// item would take hours); the bulk endpoints pull everything in one shot.
// Runs fetch(--force) → load-spt → normalize, rebuilding data/items.json. The
// client reloads afterwards. Held under withWriteLock for the whole run.
// Body: { source: 'dev' | 'market' }.
function handleRefreshAll(req, res) {
  let body = '';
  req.on('data', c => { body += c; if (body.length > 1e6) req.destroy(); });
  req.on('end', () => {
    let payload;
    try { payload = JSON.parse(body || '{}'); }
    catch (e) { return sendJson(res, 400, { error: 'invalid JSON' }); }
    const source = payload.source;
    if (source !== 'dev' && source !== 'market') {
      return sendJson(res, 400, { error: "source must be 'dev' or 'market'" });
    }
    const fetchScript = source === 'market' ? 'fetch-tarkov-market.js' : 'fetch-tarkov-dev.js';
    const t0 = Date.now();
    withWriteLock(async () => {
      await runScript(fetchScript, ['--force']);   // refresh that source's cache
      await runScript('load-spt.js', []);          // pick up current SPT state + overrides
      await runScript('normalize.js', []);         // re-merge all caches → items.json
      let itemCount = null;
      try { itemCount = Object.keys(readJsonFile(ITEMS_JSON)).length; } catch (_) {}
      return sendJson(res, 200, { ok: true, source, itemCount, durationMs: Date.now() - t0 });
    }).catch(e => {
      console.error('refresh-all failed:', e);
      if (!res.headersSent) return sendJson(res, 502, { error: e.message });
    });
  });
}

// Recompute the `consolidated` view from the raw source blocks (spt / tarkovDev /
// tarkovMarket). Mirrors normalize.js:deriveConsolidated exactly so a live edit
// or a per-item refresh produces the same result as a full pipeline run. Must
// re-derive ALL price columns — a previous version only touched priceFleaSpt,
// so /api/refresh-dev (and /api/refresh-market) left the dev/market columns +
// canonical stale (the visible column reads consolidated, not the raw block).
// `group` and `conditionType` don't change at runtime, so they're left as-is.
function recomputeConsolidated(item) {
  const c = item.consolidated;
  const dev    = item.tarkovDev && item.tarkovDev.pve;
  const market = item.tarkovMarket && item.tarkovMarket.pve;
  const spt    = item.spt;

  // priceTraderSell: highest sell-to-trader price (from tarkov.dev pve sellFor).
  let priceTraderSell = null;
  if (dev && Array.isArray(dev.sellFor)) {
    let best = -1, vendor = null;
    for (const s of dev.sellFor) {
      if (!s.vendor || s.vendor.normalizedName === 'flea-market') continue;
      const p = s.priceRUB ?? 0;
      if (p > best) { best = p; vendor = s.vendor.name; }
    }
    if (vendor) priceTraderSell = { value: best, vendor };
  }
  c.priceTraderSell = priceTraderSell;

  c.priceFleaSpt          = spt    ? (spt.effectiveFleaPrice ?? spt.fleaPrice ?? null) : null;
  c.priceFleaDevLastLow   = dev    ? (dev.lastLow   ?? null) : null;
  c.priceFleaDevAvg24h    = dev    ? (dev.avg24h    ?? null) : null;
  c.priceFleaMarketAvg24h = market ? (market.avg24h ?? null) : null;

  // Canonical priority chain (must match normalize.js).
  const mkt = c.priceFleaMarketAvg24h, devA = c.priceFleaDevAvg24h, devL = c.priceFleaDevLastLow, sptP = c.priceFleaSpt;
  if (mkt != null)        { c.priceFleaCanonical = mkt;  c.priceFleaSource = 'tarkov-market-avg24h'; }
  else if (devA != null)  { c.priceFleaCanonical = devA; c.priceFleaSource = 'tarkov.dev-avg24h'; }
  else if (devL != null)  { c.priceFleaCanonical = devL; c.priceFleaSource = 'tarkov.dev-lastLow'; }
  else if (sptP != null)  { c.priceFleaCanonical = sptP; c.priceFleaSource = 'spt'; }
  else                    { c.priceFleaCanonical = null; c.priceFleaSource = null; }
}

// Serialize items.json with one Tpl per line (matches normalize.js format).
function serializeItems(items) {
  const tpls = Object.keys(items).sort();
  return '{\n' + tpls.map(t => JSON.stringify(t) + ':' + JSON.stringify(items[t])).join(',\n') + '\n}\n';
}

// POST /api/price — set a desired flea price X for a tpl by writing a COMPENSATED
// override into ragfair.json:dynamic.itemPriceOverrideRouble[tpl].
//
// Formula (validated vs source + 7 in-game scenarios, see docs/flea-override-plan.md):
//   the boot does Templates.Prices[tpl] = override (assign), THEN += handbook×M
//   (AddOrUpdate), THEN the offer generator floors to handbook×K_trader. So:
//       offerBase = max(override + bonus, floor)
//   To land on X we write  override = X − bonus  (bonus = spt.fleaPrice).
//   Valid only for X ≥ floor (spt.fleaFloor); below that the flea floors to `floor`.
// Body: { tpl, price }  (price = desired flea price X, positive integer).
function handlePatchPrice(req, res) {
  let body = '';
  req.on('data', c => { body += c; if (body.length > 1e6) { req.destroy(); }});
  req.on('end', () => {
    let payload;
    try { payload = JSON.parse(body); }
    catch (e) { return sendJson(res, 400, { error: 'invalid JSON' }); }

    const tpl = payload.tpl;
    const price = payload.price;
    if (typeof tpl !== 'string' || !/^[a-f0-9]{24}$/i.test(tpl)) {
      return sendJson(res, 400, { error: 'invalid tpl (expected 24-char hex BSG id)' });
    }
    if (!Number.isFinite(price) || price <= 0 || !Number.isInteger(price)) {
      return sendJson(res, 400, { error: 'invalid price (expected positive integer)' });
    }

    withWriteLock(() => {
      // 1. Validate data/items.json has the tpl + a handbook-derived bonus.
      const items = readJsonFile(ITEMS_JSON);
      const item = items[tpl];
      if (!item) {
        return sendJson(res, 404, { error: 'tpl not in data/items.json — run normalize.js first' });
      }
      if (item.modSource) {
        return sendJson(res, 422, {
          error: `"${item.shortName || tpl}" é item do mod ${item.modSource} — não dá pra editar via override. O CustomItemService do mod re-define o preço (Prices[tpl] = fleaPriceRoubles) DEPOIS do override, apagando-o (validado in-game). Pra mudar, edite o fleaPriceRoubles no db/CustomItems do mod.`,
        });
      }
      const bonus = item.spt ? item.spt.fleaPrice : null;          // handbook × M
      const floor = (item.spt && item.spt.fleaFloor) || 0;         // handbook × K_trader
      const ceiling = item.spt ? (item.spt.fleaCeiling ?? null) : null;  // handbook × unreasonableMult, or null
      if (bonus == null) {
        return sendJson(res, 422, { error: 'no handbook-derived bonus for this tpl (mod item / not in handbook) — override unsupported' });
      }
      if (price < floor) {
        return sendJson(res, 422, {
          error: `price ${price} is below the flea floor ${floor} (= handbook × trader buyback). The flea cannot go below this for this item.`,
          floor,
        });
      }
      if (ceiling != null && price > ceiling) {
        return sendJson(res, 422, {
          error: `price ${price} is above the flea ceiling ${ceiling} (Weapon Mod / Electronics are capped at handbook × multiplier by SPT's unreasonableModPrices). The flea cannot exceed this for this item.`,
          ceiling,
        });
      }

      // 2. Compute the compensated override and write it to ragfair.json.
      const override = price - bonus;  // integer; may be negative when floor ≤ price < bonus
      const ragfair = readJsonFile(RAGFAIR_JSON);
      if (!ragfair.dynamic) return sendJson(res, 500, { error: 'ragfair.json missing .dynamic' });
      ragfair.dynamic.itemPriceOverrideRouble = ragfair.dynamic.itemPriceOverrideRouble || {};
      const previousOverride = ragfair.dynamic.itemPriceOverrideRouble[tpl] ?? null;
      ragfair.dynamic.itemPriceOverrideRouble[tpl] = override;
      writeJsonPreservingStyle(RAGFAIR_JSON, ragfair);

      // 3. Refresh checks.dat for ragfair.json.
      const checksResult = updateSptChecks({ 'configs/ragfair.json': RAGFAIR_JSON });

      // 4. Sync data/items.json — override replaces prices.json, so effective = X.
      item.spt.fleaOverride = override;
      item.spt.effectiveFleaPrice = effectivePrice(override, bonus, floor, ceiling);  // = price (floor ≤ price ≤ ceiling)
      recomputeConsolidated(item);
      fs.writeFileSync(ITEMS_JSON, serializeItems(items), 'utf8');

      // 5. Audit log.
      appendEditLog({
        at:        new Date().toISOString(),
        action:    'set-override',
        tpl,
        name:      item.name || null,
        shortName: item.shortName || null,
        desiredFlea: price,
        bonus, floor, override, previousOverride,
        ip:        req.socket.remoteAddress || null,
      });

      return sendJson(res, 200, {
        ok: true, tpl,
        override, previousOverride,
        effectiveFleaPrice: item.spt.effectiveFleaPrice,
        bonus, floor, ceiling,
        consolidated: item.consolidated,
        checks: checksResult,
      });
    }).catch(e => {
      console.error('set-override failed:', e);
      if (!res.headersSent) return sendJson(res, 500, { error: e.message });
    });
  });
}

// DELETE /api/price — remove the override for a tpl, restoring the vanilla flea
// price (max((prices.json[tpl] ?? 0) + bonus, floor)). Body: { tpl }.
function handleDeletePrice(req, res) {
  let body = '';
  req.on('data', c => { body += c; if (body.length > 1e6) { req.destroy(); }});
  req.on('end', () => {
    let payload;
    try { payload = JSON.parse(body || '{}'); }
    catch (e) { return sendJson(res, 400, { error: 'invalid JSON' }); }
    const tpl = payload.tpl;
    if (typeof tpl !== 'string' || !/^[a-f0-9]{24}$/i.test(tpl)) {
      return sendJson(res, 400, { error: 'invalid tpl (expected 24-char hex BSG id)' });
    }

    withWriteLock(() => {
      const items = readJsonFile(ITEMS_JSON);
      const item = items[tpl];
      if (!item) return sendJson(res, 404, { error: 'tpl not in data/items.json' });

      const ragfair = readJsonFile(RAGFAIR_JSON);
      const map = (ragfair.dynamic && ragfair.dynamic.itemPriceOverrideRouble) || {};
      const previousOverride = map[tpl] ?? null;
      if (previousOverride == null) {
        return sendJson(res, 200, { ok: true, tpl, noop: true, message: 'no override to remove' });
      }
      delete map[tpl];
      writeJsonPreservingStyle(RAGFAIR_JSON, ragfair);
      const checksResult = updateSptChecks({ 'configs/ragfair.json': RAGFAIR_JSON });

      // Restore vanilla effective price (prices.json + bonus, clamped floor/ceiling).
      const bonus = item.spt ? item.spt.fleaPrice : null;
      const floor = (item.spt && item.spt.fleaFloor) || 0;
      const ceiling = item.spt ? (item.spt.fleaCeiling ?? null) : null;
      let pricesDiskVal = null;
      try { pricesDiskVal = readJsonFile(PRICES_JSON)[tpl] ?? null; } catch (_) {}
      if (item.spt) {
        item.spt.fleaOverride = null;
        item.spt.effectiveFleaPrice = bonus != null ? effectivePrice(null, bonus, floor, ceiling, pricesDiskVal) : null;
        recomputeConsolidated(item);
      }
      fs.writeFileSync(ITEMS_JSON, serializeItems(items), 'utf8');

      appendEditLog({
        at: new Date().toISOString(), action: 'delete-override', tpl,
        name: item.name || null, shortName: item.shortName || null,
        previousOverride, effectiveFleaPrice: item.spt ? item.spt.effectiveFleaPrice : null,
        ip: req.socket.remoteAddress || null,
      });

      return sendJson(res, 200, {
        ok: true, tpl, removed: true, previousOverride,
        effectiveFleaPrice: item.spt ? item.spt.effectiveFleaPrice : null,
        consolidated: item.consolidated, checks: checksResult,
      });
    }).catch(e => {
      console.error('delete-override failed:', e);
      if (!res.headersSent) return sendJson(res, 500, { error: e.message });
    });
  });
}

// GET /api/overrides — current ragfair.json:dynamic.itemPriceOverrideRouble map.
// Includes SPT's vanilla defaults (a handful of gift items) alongside user edits.
function handleGetOverrides(req, res) {
  try {
    const ragfair = readJsonFile(RAGFAIR_JSON);
    const overrides = (ragfair.dynamic && ragfair.dynamic.itemPriceOverrideRouble) || {};
    return sendJson(res, 200, { ok: true, overrides });
  } catch (e) {
    return sendJson(res, 500, { error: e.message });
  }
}

// POST /api/ban — toggles _props.CanSellOnRagfair in SPT's items.json.
// Body: { tpl: "<bsgTpl>", banned: true|false }.
//
// Why CanSellOnRagfair and not ragfair.json:dynamic.blacklist.custom: SPT 4.0
// dropped the per-tpl custom list — its blacklist config class only
// deserializes EnableBsgList, EnableQuestList, CustomItemCategoryList, etc.
// (verified by inspecting backing fields in SPTarkov.Server.Core.dll).
// CanSellOnRagfair on each item template is the only mechanism the server
// actually honors for per-tpl bans (gated by enableBsgList: true, which is on
// by default).
function handleBanToggle(req, res) {
  let body = '';
  req.on('data', c => { body += c; if (body.length > 1e6) req.destroy(); });
  req.on('end', () => {
    let payload;
    try { payload = JSON.parse(body); }
    catch (e) { return sendJson(res, 400, { error: 'invalid JSON' }); }
    const tpl = payload.tpl;
    const banned = !!payload.banned;
    if (typeof tpl !== 'string' || !/^[a-f0-9]{24}$/i.test(tpl)) {
      return sendJson(res, 400, { error: 'invalid tpl' });
    }
    withWriteLock(() => {
      // 1. Precondition: enableBsgList must be true, else CanSellOnRagfair is
      //    ignored by SPT and the toggle would silently no-op in-game.
      const ragfair = readJsonFile(RAGFAIR_JSON);
      const bsgListEnabled = !(ragfair.dynamic && ragfair.dynamic.blacklist &&
                                ragfair.dynamic.blacklist.enableBsgList === false);
      if (!bsgListEnabled) {
        return sendJson(res, 409, {
          error: 'enableBsgList is false in ragfair.json — CanSellOnRagfair toggles would be ignored by SPT',
        });
      }

      // 2. Validate viewer DB has the tpl before touching the SPT file — keeps
      //    the two stores in lockstep on the failure path.
      const items = readJsonFile(ITEMS_JSON);
      if (!items[tpl]) return sendJson(res, 404, { error: 'tpl not in data/items.json' });

      // 3. Toggle CanSellOnRagfair in SPT items.json (18 MB — full parse + write).
      //    Write to tmp + rename for atomicity: items.json is load-bearing for
      //    the SPT server; a truncated/partial write would brick the boot.
      const sptItems = readJsonFile(SPT_ITEMS_JSON);
      const sptEntry = sptItems[tpl];
      if (!sptEntry || !sptEntry._props) {
        return sendJson(res, 404, { error: 'tpl not in SPT items.json' });
      }
      const wasBanned = sptEntry._props.CanSellOnRagfair === false;
      sptEntry._props.CanSellOnRagfair = !banned;
      // SPT items.json uses 2-space indent + CRLF; preserve to minimize diff.
      const serialized = JSON.stringify(sptItems, null, 2).replace(/\n/g, '\r\n');
      const tmpPath = SPT_ITEMS_JSON + '.tmp';
      fs.writeFileSync(tmpPath, serialized, 'utf8');
      fs.renameSync(tmpPath, SPT_ITEMS_JSON);

      // 4. Update data/items.json so the viewer reflects the new state immediately.
      if (!items[tpl].spt) items[tpl].spt = { basePrice: null, fleaPrice: null, fleaBanned: false, fleaBanReasons: [], traders: [] };
      const reasons = new Set(items[tpl].spt.fleaBanReasons || []);
      reasons.delete('custom');               // legacy reason — clean up if present
      if (banned) reasons.add('bsg');
      else        reasons.delete('bsg');
      items[tpl].spt.fleaBanReasons = [...reasons];
      items[tpl].spt.fleaBanned = reasons.size > 0;
      fs.writeFileSync(ITEMS_JSON, serializeItems(items), 'utf8');

      // 5. Refresh checks.dat (items.json hash changed).
      const checksResult = updateSptChecks({ 'database/templates/items.json': SPT_ITEMS_JSON });

      // 6. Audit log.
      appendBanLog({
        at: new Date().toISOString(),
        tpl,
        name: items[tpl].name || null,
        shortName: items[tpl].shortName || null,
        action: banned ? 'ban' : 'unban',  // audit log: 'ban' | 'unban'
        method: 'CanSellOnRagfair',
        wasBanned,
        ip: req.socket.remoteAddress || null,
      });

      return sendJson(res, 200, {
        ok: true, tpl, banned,
        fleaBanned: items[tpl].spt.fleaBanned,
        fleaBanReasons: items[tpl].spt.fleaBanReasons,
        checks: checksResult,
      });
    }).catch(e => {
      console.error('ban toggle failed:', e);
      if (!res.headersSent) return sendJson(res, 500, { error: e.message });
    });
  });
}

// POST /api/flea-min-level — updates the global flea unlock level in globals.json.
// Body: { minUserLevel: <int 1..99> }. The value lives at
// config.RagFair.minUserLevel and gates who can access the flea at all.
// Per-item flea level doesn't exist in BSG data — this is the only knob.
function handleFleaMinLevel(req, res) {
  let body = '';
  req.on('data', c => { body += c; if (body.length > 1e6) req.destroy(); });
  req.on('end', () => {
    let payload;
    try { payload = JSON.parse(body); }
    catch (e) { return sendJson(res, 400, { error: 'invalid JSON' }); }
    const lvl = payload.minUserLevel;
    if (!Number.isInteger(lvl) || lvl < 1 || lvl > 99) {
      return sendJson(res, 400, { error: 'minUserLevel must be integer 1..99' });
    }
    withWriteLock(() => {
      const globals = readJsonFile(GLOBALS_JSON);
      if (!globals.config || !globals.config.RagFair) {
        return sendJson(res, 500, { error: 'globals.json missing config.RagFair' });
      }
      const previous = globals.config.RagFair.minUserLevel;
      globals.config.RagFair.minUserLevel = lvl;
      // globals.json is 1 MB, 2-space indent, LF — preserve format.
      const tmp = GLOBALS_JSON + '.tmp';
      fs.writeFileSync(tmp, JSON.stringify(globals, null, 2), 'utf8');
      fs.renameSync(tmp, GLOBALS_JSON);

      // Mirror into data/meta.json so the viewer's META reflects the new value
      // on next page load (in-memory update is the caller's responsibility).
      try {
        const meta = readJsonFile(META_JSON);
        if (meta.sources && meta.sources.spt) {
          meta.sources.spt.fleaMinUserLevel = lvl;
          fs.writeFileSync(META_JSON, JSON.stringify(meta, null, 2) + '\n', 'utf8');
        }
      } catch (e) {
        console.error('meta.json mirror failed (non-fatal):', e.message);
      }

      const checksResult = updateSptChecks({ 'database/globals.json': GLOBALS_JSON });
      return sendJson(res, 200, { ok: true, minUserLevel: lvl, previous, checks: checksResult });
    }).catch(e => {
      console.error('flea min level update failed:', e);
      if (!res.headersSent) return sendJson(res, 500, { error: e.message });
    });
  });
}

http.createServer((req, res) => {
  if (req.method === 'POST'   && req.url === '/api/price')        return handlePatchPrice(req, res);
  if (req.method === 'DELETE' && req.url === '/api/price')        return handleDeletePrice(req, res);
  if (req.method === 'GET'    && req.url === '/api/overrides')    return handleGetOverrides(req, res);
  if (req.method === 'POST' && req.url === '/api/ban')            return handleBanToggle(req, res);
  if (req.method === 'POST' && req.url === '/api/refresh-dev')    return handleRefreshDev(req, res);
  if (req.method === 'POST' && req.url === '/api/refresh-market') return handleRefreshMarket(req, res);
  if (req.method === 'POST' && req.url === '/api/refresh-all')    return handleRefreshAll(req, res);
  if (req.method === 'POST' && req.url === '/api/flea-min-level') return handleFleaMinLevel(req, res);

  let urlPath = decodeURIComponent(req.url.split('?')[0]);

  // Proxy SPT image assets (handbook icons etc) — /spt-images/handbook/<file>.png
  // → <SPT_DATA>/images/handbook/<file>.png
  if (urlPath.startsWith('/spt-images/')) {
    const relPath = urlPath.slice('/spt-images/'.length);
    const imagePath = path.join(SPT_DATA, 'images', relPath);
    if (!imagePath.startsWith(path.join(SPT_DATA, 'images'))) { res.writeHead(403); res.end('forbidden'); return; }
    fs.stat(imagePath, (err, st) => {
      if (err || !st.isFile()) { res.writeHead(404); res.end('image not found: ' + relPath); return; }
      res.writeHead(200, {
        'Content-Type': MIME[path.extname(imagePath)] || 'image/png',
        'Cache-Control': 'public, max-age=3600',
      });
      fs.createReadStream(imagePath).pipe(res);
    });
    return;
  }

  if (urlPath === '/' || urlPath === '/viewer' || urlPath === '/viewer/') urlPath = '/viewer/index.html';
  const filePath = path.join(ROOT, urlPath);
  if (!filePath.startsWith(ROOT)) { res.writeHead(403); res.end('forbidden'); return; }
  fs.stat(filePath, (err, st) => {
    if (err || !st.isFile()) { res.writeHead(404); res.end('not found: ' + urlPath); return; }
    res.writeHead(200, { 'Content-Type': MIME[path.extname(filePath)] || 'application/octet-stream' });
    fs.createReadStream(filePath).pipe(res);
  });
}).listen(PORT, () => {
  console.error(`Serving ${ROOT} at http://localhost:${PORT}/viewer/`);
  console.error(`POST/DELETE /api/price → writes override in ${RAGFAIR_JSON}`);

  // On startup: refresh hashes for tracked SPT files that may be divergent.
  // ragfair.json changes from override edits; items.json from ban toggles;
  // handbook.json kept for legacy/manual edits.
  const sptItems = path.join(SPT_DATA, 'database', 'templates', 'items.json');
  const result = updateSptChecks({
    'configs/ragfair.json':             RAGFAIR_JSON,
    'database/templates/items.json':    sptItems,
    'database/templates/handbook.json': HANDBOOK_JSON,
  });
  console.error('checks.dat refresh:', JSON.stringify(result.changes));
});