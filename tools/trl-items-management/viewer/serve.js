#!/usr/bin/env node
/**
 * Tiny static server for the viewer. Serves files from tools/trl-items-management/
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
const zlib   = require('zlib');
require('../scripts/load-env.js');  // populate process.env from tool-root/.env (TARKOV_MARKET_API_KEY)

const PORT     = parseInt(process.argv[2] || '8080', 10);
// Bind localhost by default — the viewer mutates live SPT config and has no auth,
// so it must not be reachable off-box. Set ITEMDB_HOST=0.0.0.0 to expose on the
// LAN (only behind your own auth/network controls — there is none built in).
const HOST     = process.env.ITEMDB_HOST || '127.0.0.1';
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
// Trader-price overrides live in a companion server mod (TRLTraderPrices), not in
// SPT_Data — the mod applies overrides.json at boot. Resolve both the mod dir
// (presence check) and the config file (read/written at serve time).
const TRL_MOD_DIR        = path.join(SPT_PATH, 'user', 'mods', 'TRLTraderPrices');
const TRADER_OVERRIDES   = path.join(TRL_MOD_DIR, 'config', 'overrides.json');
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

// Text types worth gzipping. Binary (.png/.webp) is already compressed — skip.
const GZIP_TYPES = new Set(['.json', '.js', '.css', '.html', '.svg']);
// Cache of gzipped buffers, keyed by absolute path, invalidated by mtime+size.
// items.json (~17MB) is rewritten on every edit/refresh/rescan, so re-gzip only
// when it actually changes — never per request.
const _gzipCache = new Map();  // path -> { mtimeMs, size, buf }
function getGzipped(filePath, mtimeMs, size, cb) {
  const hit = _gzipCache.get(filePath);
  if (hit && hit.mtimeMs === mtimeMs && hit.size === size) return cb(null, hit.buf);
  fs.readFile(filePath, (err, raw) => {
    if (err) return cb(err);
    zlib.gzip(raw, (gerr, buf) => {
      if (gerr) return cb(gerr);
      _gzipCache.set(filePath, { mtimeMs, size, buf });
      cb(null, buf);
    });
  });
}

function readJsonFile(p) {
  return JSON.parse(fs.readFileSync(p, 'utf8').replace(/^﻿/, ''));
}

// SPT 4.0 validates each templates/*.json against an MD5 manifest stored in
// checks.dat (base64-encoded JSON array of {Path, Hash}). When we modify a
// tracked file we must update its hash here, otherwise SPT logs a file-validation
// failure on boot. Paths use forward slashes relative to SPT_Data.
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
          // Require an EXACT bsgId match — never accept a sole fuzzy result, which
          // for an item tarkov-market doesn't list would store the wrong item's
          // price (the q= name search is fuzzy). Better no data than wrong data.
          const match = arr.find(x => x && x.bsgId === tpl);
          if (!match) return reject(new Error(`item not on tarkov-market (q="${name}", ${arr.length} results, none with bsgId ${tpl})`));
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

// POST /api/rescan — re-read the LOCAL SPT database (incl. mod items added under
// user/mods/*/db/CustomItems since the last run) and re-merge, WITHOUT hitting
// tarkov.dev/market. Fast (load-spt reads from disk — the SPT server need not be
// running). Use after installing/removing a mod to pick up its items + prices.
// Body: none.
function handleRescan(req, res) {
  let body = '';
  req.on('data', c => { body += c; if (body.length > 1e6) req.destroy(); });
  req.on('end', () => {
    const t0 = Date.now();
    withWriteLock(async () => {
      await runScript('load-spt.js', []);   // re-read SPT DB + user/mods + ragfair (overrides + multipliers)
      await runScript('normalize.js', []);  // re-merge caches → items.json
      let itemCount = null, modCount = null;
      try {
        const items = readJsonFile(ITEMS_JSON);
        itemCount = Object.keys(items).length;
        modCount = Object.values(items).filter(i => i && i.modSource).length;
      } catch (_) {}
      return sendJson(res, 200, { ok: true, itemCount, modCount, durationMs: Date.now() - t0 });
    }).catch(e => {
      console.error('rescan failed:', e);
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
        // Mod items: the additive override is WIPED by the mod's CustomItemService
        // (it re-sets Prices[tpl] = fleaPriceRoubles AFTER ApplyFleaPriceOverrides).
        // The working lever is ragfair.dynamic.itemPriceMultiplier[tpl] — an
        // offer-time factor (RagfairPriceService.cs:341) applied AFTER the mod's
        // write. To land on X we write multiplier = X / base, base = fleaBaseRaw
        // (= fleaPriceRoubles + handbook×M = GetFleaPriceForItem). The multiplier
        // is applied BEFORE the ceiling, so X is still capped by fleaCeiling.
        const sptB    = item.spt || {};
        const base    = sptB.fleaBaseRaw ?? sptB.effectiveFleaPrice ?? null;  // fallback if items.json predates fleaBaseRaw
        const floor   = sptB.fleaFloor || 0;
        const ceiling = sptB.fleaCeiling ?? null;
        if (!base || base <= 0) {
          return sendJson(res, 422, { error: `no flea base for "${item.shortName || tpl}" — multiplier undefined (run rescan/normalize)` });
        }
        if (ceiling != null && price > ceiling) {
          return sendJson(res, 422, {
            error: `price ${price} is above the ceiling ${ceiling} (unreasonableModPrices: Weapon Mod ×6 / Electronics ×11). The multiplier is applied before the ceiling, so the flea cannot exceed this.`,
            ceiling,
          });
        }
        const multiplier = Math.round((price / base) * 1e6) / 1e6;  // 6-decimal precision
        const ragfair = readJsonFile(RAGFAIR_JSON);
        if (!ragfair.dynamic) return sendJson(res, 500, { error: 'ragfair.json missing .dynamic' });
        ragfair.dynamic.itemPriceMultiplier = ragfair.dynamic.itemPriceMultiplier || {};
        const previousMultiplier = ragfair.dynamic.itemPriceMultiplier[tpl] ?? null;
        ragfair.dynamic.itemPriceMultiplier[tpl] = multiplier;
        writeJsonPreservingStyle(RAGFAIR_JSON, ragfair);
        const checksResult = updateSptChecks({ 'configs/ragfair.json': RAGFAIR_JSON });

        // Sync items.json: effective = clamp(base × multiplier, floor, ceiling) ≈ X.
        let eff = Math.round(base * multiplier);
        if (eff < floor) eff = floor;
        if (ceiling != null && eff > ceiling) eff = ceiling;
        item.spt.fleaBaseRaw = base;
        item.spt.fleaOfferMultiplier = multiplier;
        item.spt.effectiveFleaPrice = eff;
        recomputeConsolidated(item);
        fs.writeFileSync(ITEMS_JSON, serializeItems(items), 'utf8');

        appendEditLog({
          at: new Date().toISOString(), action: 'set-multiplier', tpl,
          name: item.name || null, shortName: item.shortName || null, modSource: item.modSource,
          desiredFlea: price, base, multiplier, previousMultiplier,
          ip: req.socket.remoteAddress || null,
        });

        return sendJson(res, 200, {
          ok: true, tpl, mode: 'multiplier', multiplier, previousMultiplier,
          base, effectiveFleaPrice: eff, ceiling,
          consolidated: item.consolidated, checks: checksResult,
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

      // Mod items are edited via itemPriceMultiplier — restoring default means
      // removing THAT entry, not the (always-empty) override entry.
      if (item.modSource) {
        const mmap = (ragfair.dynamic && ragfair.dynamic.itemPriceMultiplier) || {};
        const previousMultiplier = mmap[tpl] ?? null;
        if (previousMultiplier == null) {
          return sendJson(res, 200, { ok: true, tpl, noop: true, message: 'no multiplier to remove' });
        }
        delete mmap[tpl];
        writeJsonPreservingStyle(RAGFAIR_JSON, ragfair);
        const checksResult = updateSptChecks({ 'configs/ragfair.json': RAGFAIR_JSON });

        // Restore default mod-item effective price = clamp(base, floor, ceiling).
        const sptB    = item.spt || {};
        const base    = sptB.fleaBaseRaw ?? null;
        const floor   = sptB.fleaFloor || 0;
        const ceiling = sptB.fleaCeiling ?? null;
        let eff = base != null ? base : (sptB.effectiveFleaPrice ?? null);
        if (eff != null) {
          if (eff < floor) eff = floor;
          if (ceiling != null && eff > ceiling) eff = ceiling;
        }
        if (item.spt) {
          item.spt.fleaOfferMultiplier = null;
          item.spt.effectiveFleaPrice = eff;
          recomputeConsolidated(item);
        }
        fs.writeFileSync(ITEMS_JSON, serializeItems(items), 'utf8');

        appendEditLog({
          at: new Date().toISOString(), action: 'delete-multiplier', tpl,
          name: item.name || null, shortName: item.shortName || null, modSource: item.modSource,
          previousMultiplier, effectiveFleaPrice: eff, ip: req.socket.remoteAddress || null,
        });

        return sendJson(res, 200, {
          ok: true, tpl, removed: true, mode: 'multiplier', previousMultiplier,
          effectiveFleaPrice: eff, consolidated: item.consolidated, checks: checksResult,
        });
      }

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

// ---------------------------------------------------------------------------
// Trader-price overrides (companion mod TRLTraderPrices)
//
// Unlike flea prices (written into SPT_Data/configs/ragfair.json), trader prices
// are overridden by a separate server mod that reads its own config/overrides.json
// at boot. The viewer just maintains that file. Shape:
//   { "<traderId>": { "<tpl>": <priceRUB> } }
// All writes are atomic (tmp + fs.renameSync) and serialized through the same
// in-process write lock as the flea APIs. NO checks.dat refresh — the file lives
// outside SPT_Data and is not part of SPT's MD5 manifest.
// ---------------------------------------------------------------------------

// Read overrides.json at serve time, tolerating a missing file/dir.
function readTraderOverrides() {
  if (!fs.existsSync(TRADER_OVERRIDES)) return {};
  const obj = readJsonFile(TRADER_OVERRIDES);
  return obj && typeof obj === 'object' ? obj : {};
}

// Atomically write the overrides map. Creates config/ on demand. Uses 2-space
// indent + trailing newline (the mod owns this file; vanilla-style preservation
// via writeJsonPreservingStyle is not applicable when the file may not exist).
function writeTraderOverrides(obj) {
  fs.mkdirSync(path.dirname(TRADER_OVERRIDES), { recursive: true });
  const serialized = JSON.stringify(obj, null, 2) + '\n';
  const tmp = TRADER_OVERRIDES + '.tmp';
  fs.writeFileSync(tmp, serialized, 'utf8');
  fs.renameSync(tmp, TRADER_OVERRIDES);
}

// GET /api/trader-overrides — current overrides.json map (read at serve time).
// Returns {} when the mod / file is not present yet.
function handleGetTraderOverrides(req, res) {
  try {
    return sendJson(res, 200, { ok: true, overrides: readTraderOverrides() });
  } catch (e) {
    return sendJson(res, 500, { error: e.message });
  }
}

// PATCH /api/trader-price — set a trader-specific price for a tpl.
// Body: { tpl, traderId, priceRUB }. Writes overrides[traderId][tpl] = priceRUB.
function handlePatchTraderPrice(req, res) {
  let body = '';
  req.on('data', c => { body += c; if (body.length > 1e6) { req.destroy(); }});
  req.on('end', () => {
    let payload;
    try { payload = JSON.parse(body); }
    catch (e) { return sendJson(res, 400, { error: 'invalid JSON' }); }

    const tpl      = payload.tpl;
    const traderId = payload.traderId;
    const priceRUB = payload.priceRUB;
    if (typeof tpl !== 'string' || !/^[a-f0-9]{24}$/i.test(tpl)) {
      return sendJson(res, 400, { error: 'invalid tpl (expected 24-char hex BSG id)' });
    }
    if (typeof traderId !== 'string' || !/^[a-f0-9]{24}$/i.test(traderId)) {
      return sendJson(res, 400, { error: 'invalid traderId (expected 24-char hex MongoId)' });
    }
    if (!Number.isFinite(priceRUB) || priceRUB <= 0) {
      return sendJson(res, 400, { error: 'invalid priceRUB (expected finite number > 0)' });
    }

    withWriteLock(() => {
      const overrides = readTraderOverrides();
      if (!overrides[traderId] || typeof overrides[traderId] !== 'object') overrides[traderId] = {};
      const previousPrice = overrides[traderId][tpl] ?? null;
      overrides[traderId][tpl] = priceRUB;
      writeTraderOverrides(overrides);

      const modInstalled = fs.existsSync(TRL_MOD_DIR);
      const resp = {
        ok: true, tpl, traderId, priceRUB, previousPrice,
      };
      if (!modInstalled) {
        resp.warning = 'mod not installed — saved but will not apply until TRLTraderPrices is installed';
      }
      return sendJson(res, 200, resp);
    }).catch(e => {
      console.error('set-trader-price failed:', e);
      if (!res.headersSent) return sendJson(res, 500, { error: e.message });
    });
  });
}

// DELETE /api/trader-price — remove a single trader override for a tpl.
// Body: { tpl, traderId }. Prunes the trader object when it becomes empty.
function handleDeleteTraderPrice(req, res) {
  let body = '';
  req.on('data', c => { body += c; if (body.length > 1e6) { req.destroy(); }});
  req.on('end', () => {
    let payload;
    try { payload = JSON.parse(body || '{}'); }
    catch (e) { return sendJson(res, 400, { error: 'invalid JSON' }); }

    const tpl      = payload.tpl;
    const traderId = payload.traderId;
    if (typeof tpl !== 'string' || !/^[a-f0-9]{24}$/i.test(tpl)) {
      return sendJson(res, 400, { error: 'invalid tpl (expected 24-char hex BSG id)' });
    }
    if (typeof traderId !== 'string' || !/^[a-f0-9]{24}$/i.test(traderId)) {
      return sendJson(res, 400, { error: 'invalid traderId (expected 24-char hex MongoId)' });
    }

    withWriteLock(() => {
      const overrides = readTraderOverrides();
      const traderMap = overrides[traderId];
      const previousPrice = (traderMap && traderMap[tpl] != null) ? traderMap[tpl] : null;
      if (previousPrice == null) {
        return sendJson(res, 200, { ok: true, tpl, traderId, noop: true, message: 'no override to remove' });
      }
      delete traderMap[tpl];
      if (Object.keys(traderMap).length === 0) delete overrides[traderId];  // prune empty trader
      writeTraderOverrides(overrides);
      return sendJson(res, 200, { ok: true, tpl, traderId, removed: true, previousPrice });
    }).catch(e => {
      console.error('delete-trader-price failed:', e);
      if (!res.headersSent) return sendJson(res, 500, { error: e.message });
    });
  });
}

// DELETE /api/trader-price/all — reset all trader overrides to {} (atomic).
function handleDeleteAllTraderPrices(req, res) {
  withWriteLock(() => {
    const overrides = readTraderOverrides();
    const count = Object.values(overrides).reduce((n, m) => n + (m && typeof m === 'object' ? Object.keys(m).length : 0), 0);
    writeTraderOverrides({});
    return sendJson(res, 200, { ok: true, reset: true, removedCount: count });
  }).catch(e => {
    console.error('reset-trader-prices failed:', e);
    if (!res.headersSent) return sendJson(res, 500, { error: e.message });
  });
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
  if (req.method === 'GET'    && req.url === '/api/trader-overrides') return handleGetTraderOverrides(req, res);
  if (req.method === 'PATCH'  && req.url === '/api/trader-price') return handlePatchTraderPrice(req, res);
  if (req.method === 'DELETE' && req.url === '/api/trader-price/all') return handleDeleteAllTraderPrices(req, res);
  if (req.method === 'DELETE' && req.url === '/api/trader-price') return handleDeleteTraderPrice(req, res);
  if (req.method === 'POST' && req.url === '/api/ban')            return handleBanToggle(req, res);
  if (req.method === 'POST' && req.url === '/api/refresh-dev')    return handleRefreshDev(req, res);
  if (req.method === 'POST' && req.url === '/api/refresh-market') return handleRefreshMarket(req, res);
  if (req.method === 'POST' && req.url === '/api/refresh-all')    return handleRefreshAll(req, res);
  if (req.method === 'POST' && req.url === '/api/rescan')         return handleRescan(req, res);
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

  // Brand URL: /TRLItemsManagement[/...] serves the viewer/ folder. (Old /viewer/
  // paths still work.) The UI uses relative asset URLs, so an asset requested as
  // /TRLItemsManagement/components.css is rewritten to /viewer/components.css.
  if (urlPath === '/TRLItemsManagement' || urlPath === '/TRLItemsManagement/') urlPath = '/viewer/index.html';
  else if (urlPath.startsWith('/TRLItemsManagement/')) urlPath = '/viewer/' + urlPath.slice('/TRLItemsManagement/'.length);
  if (urlPath === '/' || urlPath === '/viewer' || urlPath === '/viewer/') urlPath = '/viewer/index.html';
  const filePath = path.join(ROOT, urlPath);
  if (!filePath.startsWith(ROOT)) { res.writeHead(403); res.end('forbidden'); return; }
  fs.stat(filePath, (err, st) => {
    if (err || !st.isFile()) { res.writeHead(404); res.end('not found: ' + urlPath); return; }
    const ext  = path.extname(filePath);
    const type = MIME[ext] || 'application/octet-stream';
    // Data files mutate (items.json rewritten on every edit/refresh/rescan): no-cache
    // + ETag revalidation (304 when unchanged). Static assets cache 1h.
    const isData = urlPath.startsWith('/data/');
    const cacheControl = isData ? 'no-cache' : 'public, max-age=3600';
    // gzip text types when the client accepts it (items.json ~17MB → ~2-3MB on the wire).
    const useGzip = GZIP_TYPES.has(ext) && /\bgzip\b/.test(req.headers['accept-encoding'] || '');
    const etag = `"${st.size.toString(16)}-${Math.round(st.mtimeMs).toString(16)}${useGzip ? '-gz' : ''}"`;

    if (req.headers['if-none-match'] === etag) {
      res.writeHead(304, { 'ETag': etag, 'Cache-Control': cacheControl, 'Vary': 'Accept-Encoding' });
      return res.end();
    }

    if (useGzip) {
      // Serve a cached gzip buffer keyed by mtime+size (re-gzips only when the
      // file changes) — never gzip 17MB on every request.
      getGzipped(filePath, st.mtimeMs, st.size, (gerr, buf) => {
        if (gerr) { res.writeHead(500); res.end('gzip error: ' + gerr.message); return; }
        res.writeHead(200, {
          'Content-Type': type, 'Content-Encoding': 'gzip', 'Content-Length': buf.length,
          'ETag': etag, 'Cache-Control': cacheControl, 'Vary': 'Accept-Encoding',
        });
        res.end(buf);
      });
      return;
    }

    res.writeHead(200, {
      'Content-Type': type, 'Content-Length': st.size,
      'ETag': etag, 'Cache-Control': cacheControl, 'Vary': 'Accept-Encoding',
    });
    fs.createReadStream(filePath).pipe(res);
  });
}).listen(PORT, HOST, () => {
  console.error(`Serving ${ROOT} at http://${HOST}:${PORT}/TRLItemsManagement/`);
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