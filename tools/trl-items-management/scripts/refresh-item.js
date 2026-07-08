#!/usr/bin/env node
/**
 * Refreshes ONE item's tarkov.dev or tarkov-market data and re-derives its consolidated view.
 * Invoked via Process.Start from the TRL-ItemsManagement mod's ItemRefreshController (the C# port
 * deliberately keeps this in Node rather than duplicating the canonical price-priority logic — see
 * normalize.js:deriveConsolidated, which this mirrors exactly, same as serve.js's own comment).
 *
 * Usage: node refresh-item.js --source dev|market --tpl <24-char-hex-tpl>
 *
 * fetchTarkovDevItem/fetchTarkovMarketItem/recomputeConsolidated/serializeItems below are copied
 * verbatim from viewer/serve.js (not reimplemented from scratch) to inherit its already-validated
 * behavior instead of risking a fresh reinterpretation drifting from it.
 */
'use strict';

const fs = require('fs');
const path = require('path');
const https = require('https');
require('./load-env.js');   // populates process.env from tool-root/.env (TARKOV_MARKET_API_KEY)

const ROOT = path.resolve(__dirname, '..');
const ITEMS_JSON = path.join(ROOT, 'data', 'items.json');
const TARKOV_DEV_URL = 'https://api.tarkov.dev/graphql';

function readJson(p) { return JSON.parse(fs.readFileSync(p, 'utf8').replace(/^﻿/, '')); }

// Verbatim from viewer/serve.js (lines ~184-223).
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

// Verbatim from viewer/serve.js (lines ~228-261).
function fetchTarkovMarketItem(name, tpl) {
  return new Promise((resolve, reject) => {
    const apiKey = process.env.TARKOV_MARKET_API_KEY;
    if (!apiKey) return reject(new Error('TARKOV_MARKET_API_KEY not set — set it in tool-root/.env to use tarkov-market refresh'));
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

// Verbatim from viewer/serve.js's recomputeConsolidated (lines ~484-522).
function recomputeConsolidated(item) {
  const c = item.consolidated;
  const dev    = item.tarkovDev && item.tarkovDev.pve;
  const market = item.tarkovMarket && item.tarkovMarket.pve;
  const spt    = item.spt;

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

  const mkt = c.priceFleaMarketAvg24h, devA = c.priceFleaDevAvg24h, devL = c.priceFleaDevLastLow, sptP = c.priceFleaSpt;
  if (mkt != null)        { c.priceFleaCanonical = mkt;  c.priceFleaSource = 'tarkov-market-avg24h'; }
  else if (devA != null)  { c.priceFleaCanonical = devA; c.priceFleaSource = 'tarkov.dev-avg24h'; }
  else if (devL != null)  { c.priceFleaCanonical = devL; c.priceFleaSource = 'tarkov.dev-lastLow'; }
  else if (sptP != null)  { c.priceFleaCanonical = sptP; c.priceFleaSource = 'spt'; }
  else                    { c.priceFleaCanonical = null; c.priceFleaSource = null; }
}

// Verbatim from viewer/serve.js's serializeItems (lines ~525-528).
function serializeItems(items) {
  const tpls = Object.keys(items).sort();
  return '{\n' + tpls.map(t => JSON.stringify(t) + ':' + JSON.stringify(items[t])).join(',\n') + '\n}\n';
}

async function main() {
  const args = process.argv.slice(2);
  const source = args[args.indexOf('--source') + 1];
  const tpl = args[args.indexOf('--tpl') + 1];
  if ((source !== 'dev' && source !== 'market') || !tpl || !/^[a-f0-9]{24}$/i.test(tpl)) {
    console.error('Usage: node refresh-item.js --source dev|market --tpl <24-char-hex-tpl>');
    process.exit(1);
  }

  const items = readJson(ITEMS_JSON);
  const item = items[tpl];
  if (!item) {
    console.error(`tpl ${tpl} not in data/items.json`);
    process.exit(1);
  }

  if (source === 'dev') {
    const fresh = await fetchTarkovDevItem(tpl);
    item.tarkovDev = { pve: mapMode(fresh.pve), regular: mapMode(fresh.regular) };
  } else {
    const m = await fetchTarkovMarketItem(item.name, tpl);
    item.tarkovMarket = {
      pve: {
        avg24h:      m.avg24hPrice   ?? null,
        avg7days:    m.avg7daysPrice ?? null,
        price:       m.price         ?? null,
        traderName:  m.traderName    ?? null,
        traderPrice: m.traderPrice   ?? null,
        link:        m.link          ?? null,
        updated:     m.updated       ?? null,
      },
    };
  }

  recomputeConsolidated(item);
  fs.writeFileSync(ITEMS_JSON, serializeItems(items), 'utf8');
  console.log(`refreshed ${tpl} (${item.shortName || item.name || ''}) from ${source}`);
}

main().catch(e => {
  console.error(e.message);
  process.exit(1);
});
