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
const fs     = require('fs');
const path   = require('path');
const crypto = require('crypto');

const PORT     = parseInt(process.argv[2] || '8080', 10);
const ROOT     = path.resolve(__dirname, '..');
const SPT_PATH = process.env.SPT_PATH || 'D:/SPT/SPT';
const SPT_DATA = path.join(SPT_PATH, 'SPT_Data');
const PRICES_JSON = path.join(SPT_DATA, 'database', 'templates', 'prices.json');
const ITEMS_JSON  = path.join(ROOT, 'data', 'items.json');
const CHECKS_DAT  = path.join(SPT_DATA, 'checks.dat');
const LOG_FILE    = path.join(ROOT, 'logs', 'price-edits.jsonl');

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
    if (!Number.isFinite(price) || price < 0 || !Number.isInteger(price)) {
      return sendJson(res, 400, { error: 'invalid price (expected non-negative integer)' });
    }

    try {
      // 1. Update SPT prices.json (source of truth)
      const prices = readJsonFile(PRICES_JSON);
      const previous = prices[tpl] ?? null;
      prices[tpl] = price;
      fs.writeFileSync(PRICES_JSON, JSON.stringify(prices, null, 4) + '\n', 'utf8');

      // 2. Update data/items.json in sync (so viewer reloads stay consistent)
      const items = readJsonFile(ITEMS_JSON);
      if (!items[tpl]) {
        return sendJson(res, 404, { error: 'tpl not in data/items.json — run normalize.js first' });
      }
      if (!items[tpl].spt) items[tpl].spt = { basePrice: null, fleaPrice: null, fleaBanned: false, fleaBanReasons: [], traders: [] };
      items[tpl].spt.fleaPrice = price;
      recomputeConsolidated(items[tpl]);
      fs.writeFileSync(ITEMS_JSON, serializeItems(items), 'utf8');

      // 3. Refresh SPT's checks.dat so the boot validation passes
      const checksResult = updateSptChecks({ 'database/templates/prices.json': PRICES_JSON });

      // 4. Append to audit log
      appendEditLog({
        at:        new Date().toISOString(),
        tpl,
        name:      items[tpl].name || null,
        shortName: items[tpl].shortName || null,
        previous,
        current:   price,
        delta:     previous != null ? price - previous : null,
        ip:        req.socket.remoteAddress || null,
      });

      return sendJson(res, 200, {
        ok: true,
        tpl, previous, current: price,
        consolidated: items[tpl].consolidated,
        checks: checksResult,
      });
    } catch (e) {
      console.error('patch-price failed:', e);
      return sendJson(res, 500, { error: e.message });
    }
  });
}

http.createServer((req, res) => {
  if (req.method === 'POST' && req.url === '/api/price') return handlePatchPrice(req, res);

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
  console.error(`POST /api/price → writes ${PRICES_JSON}`);

  // On startup: refresh hashes for tracked SPT files that may be divergent
  // (prices.json from our edits; items.json from some prior mod). Idempotent.
  const sptItems = path.join(SPT_DATA, 'database', 'templates', 'items.json');
  const result = updateSptChecks({
    'database/templates/prices.json': PRICES_JSON,
    'database/templates/items.json':  sptItems,
  });
  console.error('checks.dat refresh:', JSON.stringify(result.changes));
});