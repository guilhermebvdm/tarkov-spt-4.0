// One-off verification: query the live SPT server to confirm a trader override is
// served. Proves the assort (direct buy) AND, since RagfairOfferGenerator clones the
// live assort at generation time (after our mod ran), the flea reflects the same price.
// Usage: node verify-trader-route.js <PHPSESSID> [port]
const https = require('https'), zlib = require('zlib');
const SID = process.argv[2];
const PORT = parseInt(process.argv[3] || '6969', 10);
const RAGMAN = '5ac3b934156ae10c4430e83c';
const BELT = '6761b213607f9a6f79017af3';

function req(method, path, bodyObj) {
  return new Promise((res, rej) => {
    const body = bodyObj ? Buffer.from(JSON.stringify(bodyObj)) : null;
    const r = https.request({
      host: '127.0.0.1', port: PORT, path, method, rejectUnauthorized: false,
      // requestcompressed:0 tells SPT the POST body is NOT zlib-compressed (else it tries to inflate plain JSON → empty).
      headers: Object.assign({ 'Cookie': 'PHPSESSID=' + SID }, body ? { 'Content-Type': 'application/json', 'Content-Length': body.length, 'requestcompressed': '0' } : {}),
    }, resp => { const c = []; resp.on('data', x => c.push(x)); resp.on('end', () => res({ status: resp.statusCode, buf: Buffer.concat(c) })); });
    r.on('error', rej); if (body) r.write(body); r.end();
  });
}
function decomp(buf) {
  for (const fn of [zlib.inflateSync, zlib.gunzipSync, zlib.inflateRawSync, b => b]) {
    try { const s = fn(buf).toString('utf8'); if (s[0] === '{' || s[0] === '[') return s; } catch (e) {}
  }
  return buf.toString('utf8');
}
const unwrap = d => (d && typeof d === 'object' && 'data' in d && 'err' in d) ? d.data : d;

(async () => {
  // 1) Direct trader assort (what you pay buying from Ragman)
  const a = await req('GET', '/client/trading/api/getTraderAssort/' + RAGMAN);
  const at = decomp(a.buf);
  let assort; try { assort = unwrap(JSON.parse(at)); } catch (e) { console.log('ASSORT status', a.status, 'parse-fail, first 200:', at.slice(0, 200)); return; }
  const items = assort.items || [], bs = assort.barter_scheme || {};
  const matches = items.filter(it => it._tpl === BELT && (it.parentId === 'hideout' || !it.parentId));
  console.log('=== getTraderAssort(Ragman) status', a.status, '— Polytech belt entries:', matches.length, '===');
  for (const it of matches) {
    const s = bs[it._id]; const p = s && s[0] && s[0][0];
    console.log('  assort', it._id, '->', p ? (p.count + ' (' + p._tpl + ')') : '(no scheme)');
  }
  // 2) Flea: search ragfair for the belt, list Ragman's offer price
  const search = {
    page: 0, limit: 30, sortType: 5, sortDirection: 0, currency: 0, priceFrom: 0, priceTo: 0,
    quantityFrom: 0, quantityTo: 0, conditionFrom: 0, conditionTo: 100, oneHourExpiration: false,
    removeBartering: true, offerOwnerType: 0, onlyFunctional: false, updateOfferCount: true,
    handbookId: BELT, linkedSearchId: '', neededSearchId: '', buildItems: {}, buildCount: 0, tm: 1, reload: 1,
  };
  const f = await req('POST', '/client/ragfair/find', search);
  const ft = decomp(f.buf);
  let off; try { off = unwrap(JSON.parse(ft)); } catch (e) { console.log('\n=== ragfair/find status', f.status, 'parse-fail, first 300:', ft.slice(0, 300), '==='); return; }
  const offers = (off && off.offers) || [];
  const traderOffers = offers.filter(o => o.user && (o.user.memberType === 4 || o.user.nickname === 'Ragman' || o.user.id === RAGMAN));
  console.log('\n=== ragfair/find(Polytech) status', f.status, '— total offers:', offers.length, '| TRADER offers:', traderOffers.length, '===');
  for (const o of traderOffers) {
    const r = o.requirements && o.requirements[0];
    const who = o.user ? (o.user.nickname || o.user.id) : '?';
    console.log('   [TRADER]', who, 'memberType', o.user && o.user.memberType, '->', r ? r.count + ' (' + r._tpl + ')' : '(no req)');
  }
  const pl = offers.filter(o => !traderOffers.includes(o)).slice(0, 3);
  console.log('   (cheapest player offers for context:)');
  for (const o of pl) { const r = o.requirements && o.requirements[0]; console.log('     ', o.user && o.user.nickname, '->', r && r.count); }
})().catch(e => console.log('ERR', e.message));
