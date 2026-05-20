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
const META_JSON      = path.resolve(__dirname, '..', 'data', 'meta.json');
const ITEMS_JSON   = path.join(ROOT, 'data', 'items.json');
const CHECKS_DAT   = path.join(SPT_DATA, 'checks.dat');
const LOG_FILE              = path.join(ROOT, 'logs', 'price-edits.jsonl');
const BAN_LOG_FILE          = path.join(ROOT, 'logs', 'ban-edits.jsonl');
const HISTORY_LOG_FILE      = path.join(ROOT, 'logs', 'price-history.jsonl');
const HANDBOOK_PRICES_LOG   = path.join(ROOT, 'data', 'handbook-prices-log.json');
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

// Recompute consolidated fields that depend on spt.fleaPrice.
// Other consolidated fields (group, conditionType, priceTraderSell, dev/market columns)
// don't depend on SPT, so leave them alone.
function recomputeConsolidated(item) {
  const c = item.consolidated;
  c.priceFleaSpt = item.spt ? (item.spt.fleaPrice ?? null) : null;

  // Canonical priority chain (must match normalize.js)
  const mkt = c.priceFleaMarketAvg24h;
  const devA = c.priceFleaDevAvg24h;
  const devL = c.priceFleaDevLastLow;
  const sptP = c.priceFleaSpt;
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

// Update handbook-prices-log.json with a new price change entry.
// Creates the entry for the tpl on first edit (originalPrice = from).
// from/to are flea prices (user intent); handbookPrice is what was written to handbook.json.
function updateHandbookPricesLog(tpl, name, shortName, from, to, handbookPrice) {
  let log = {};
  try { log = readJsonFile(HANDBOOK_PRICES_LOG); } catch (_) {}
  const ts = new Date().toISOString();
  if (!log[tpl]) {
    log[tpl] = { name, shortName, originalFleaPrice: from, currentFleaPrice: to, history: [] };
  } else {
    log[tpl].name            = name;
    log[tpl].shortName       = shortName;
    log[tpl].currentFleaPrice = to;
  }
  log[tpl].history.push({ ts, fromFlea: from, toFlea: to, handbookPrice });
  const tmp = HANDBOOK_PRICES_LOG + '.tmp';
  fs.writeFileSync(tmp, JSON.stringify(log, null, 2) + '\n', 'utf8');
  fs.renameSync(tmp, HANDBOOK_PRICES_LOG);
}

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

    try {
      // 1. Validate data/items.json has the tpl before touching SPT files.
      const items = readJsonFile(ITEMS_JSON);
      if (!items[tpl]) {
        return sendJson(res, 404, { error: 'tpl not in data/items.json — run normalize.js first' });
      }

      // 2. Update handbook.json. The user supplies the desired flea price; we
      //    back-calculate the handbook price (fleaPrice / 1.5) so that SPT's
      //    offer generator produces offers at the intended value.
      const FLEA_MULTIPLIER = 1.5;
      const handbookPrice = Math.round(price / FLEA_MULTIPLIER);
      const handbook = readJsonFile(HANDBOOK_JSON);
      const entry = (handbook.Items || []).find(i => i.Id === tpl);
      if (!entry) {
        return sendJson(res, 404, { error: 'tpl not found in handbook.json' });
      }
      const previousHandbook = entry.Price;
      const previousFlea     = Math.round(previousHandbook * FLEA_MULTIPLIER);
      entry.Price = handbookPrice;
      const handbookTmp = HANDBOOK_JSON + '.tmp';
      fs.writeFileSync(handbookTmp, JSON.stringify(handbook, null, 2) + '\n', 'utf8');
      fs.renameSync(handbookTmp, HANDBOOK_JSON);

      // 3. Update data/items.json — store intended flea price directly; basePrice
      //    is the derived handbook value written above.
      if (!items[tpl].spt) items[tpl].spt = { basePrice: null, fleaPrice: null, fleaBanned: false, fleaBanReasons: [], traders: [] };
      items[tpl].spt.basePrice = handbookPrice;
      items[tpl].spt.fleaPrice = price;
      recomputeConsolidated(items[tpl]);
      fs.writeFileSync(ITEMS_JSON, serializeItems(items), 'utf8');

      // 4. Refresh checks.dat for handbook.json.
      const checksResult = updateSptChecks({ 'database/templates/handbook.json': HANDBOOK_JSON });

      // 5. Update handbook-prices-log.json — track flea prices (user intent) +
      //    the derived handbook price actually written to disk.
      updateHandbookPricesLog(tpl, items[tpl].name || null, items[tpl].shortName || null, previousFlea, price, handbookPrice);

      // 6. Append to audit log.
      appendEditLog({
        at:           new Date().toISOString(),
        tpl,
        name:         items[tpl].name || null,
        shortName:    items[tpl].shortName || null,
        previousFlea,
        currentFlea:  price,
        handbookPrice,
        delta:        price - previousFlea,
        source:       'handbook',
        ip:           req.socket.remoteAddress || null,
      });

      return sendJson(res, 200, {
        ok: true,
        tpl,
        previousFlea, currentFlea: price, handbookPrice,
        consolidated: items[tpl].consolidated,
        checks: checksResult,
      });
    } catch (e) {
      console.error('patch-price failed:', e);
      return sendJson(res, 500, { error: e.message });
    }
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
    try {
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
    } catch (e) {
      console.error('ban toggle failed:', e);
      return sendJson(res, 500, { error: e.message });
    }
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
    try {
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
    } catch (e) {
      console.error('flea min level update failed:', e);
      return sendJson(res, 500, { error: e.message });
    }
  });
}

http.createServer((req, res) => {
  if (req.method === 'POST' && req.url === '/api/price')          return handlePatchPrice(req, res);
  if (req.method === 'POST' && req.url === '/api/ban')            return handleBanToggle(req, res);
  if (req.method === 'POST' && req.url === '/api/refresh-dev')    return handleRefreshDev(req, res);
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
  console.error(`POST /api/price → writes ${HANDBOOK_JSON}`);

  // On startup: refresh hashes for tracked SPT files that may be divergent.
  // handbook.json changes from our edits; items.json from prior mod or ban toggle.
  const sptItems = path.join(SPT_DATA, 'database', 'templates', 'items.json');
  const result = updateSptChecks({
    'database/templates/handbook.json': HANDBOOK_JSON,
    'database/templates/items.json':    sptItems,
  });
  console.error('checks.dat refresh:', JSON.stringify(result.changes));
});