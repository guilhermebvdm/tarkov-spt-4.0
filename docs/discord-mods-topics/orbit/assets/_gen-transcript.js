// Gera 01-transcricao.md a partir de _capture.json + _manifest.json
const fs = require('fs');
const path = require('path');
const dir = __dirname;
const cap = JSON.parse(fs.readFileSync(path.join(dir, '_capture.json'), 'utf8'));
const man = JSON.parse(fs.readFileSync(path.join(dir, '_manifest.json'), 'utf8'));

// snowflake -> [{file, kind, origName}]
const attByMsg = {};
for (const it of man.list) {
  (attByMsg[it.msgSnow] = attByMsg[it.msgSnow] || []).push(it);
}

// Deriva o horário do próprio snowflake do Discord (epoch 2015-01-01), 100% confiável
function snowflakeToMs(snow) {
  return Number((BigInt(snow) >> 22n) + 1420070400000n);
}
function toGMT3(snow) {
  const ms = snowflakeToMs(snow);
  const t = new Date(ms - 3 * 3600 * 1000); // UTC-3
  const dd = String(t.getUTCDate()).padStart(2, '0');
  const mm = String(t.getUTCMonth() + 1).padStart(2, '0');
  const yyyy = t.getUTCFullYear();
  const hh = String(t.getUTCHours()).padStart(2, '0');
  const mi = String(t.getUTCMinutes()).padStart(2, '0');
  return { dateKey: `${dd}/${mm}/${yyyy}`, time: `${hh}:${mi}` };
}

function cleanText(s) {
  if (!s) return '';
  let t = s;
  // remove edit-tooltip date strings (ex.: "quinta-feira, 4 de junho de 2026 às 18:58")
  t = t.replace(/(segunda|ter[çc]a|quarta|quinta|sexta|s[áa]bado|domingo)-feira,[^\n]*às\s*\d{1,2}:\d{2}/gi, '');
  t = t.replace(/(s[áa]bado|domingo),[^\n]*às\s*\d{1,2}:\d{2}/gi, '');
  // collapse the orphan "(editado)" spacing
  t = t.replace(/[ \t]*\(editado\)[ \t]*/g, ' _(editado)_ ');
  // collapse 3+ newlines
  t = t.replace(/\n{3,}/g, '\n\n');
  // trim trailing spaces per line
  t = t.split('\n').map(l => l.replace(/[ \t]+$/,'')).join('\n').trim();
  return t;
}

// markdown hard-break for internal newlines
function mdMultiline(s) {
  return s.split('\n').map(l => l.length ? l : '').join('  \n');
}

const out = [];
out.push('---');
out.push('title: "ORBIT — Transcrição do thread no Discord (SPT, #mods-development)"');
out.push('date: 2026-06-04');
out.push('status: 🔵 Em andamento');
out.push('authors: Guilherme');
out.push('---');
out.push('');
out.push('# ORBIT — Transcrição do thread no Discord');
out.push('');
out.push('> Captura **fiel** do thread `"ORBIT"` no canal **#mods-development** do servidor Discord da comunidade **SPT** (SPT Pub).');
out.push('>');
out.push('> - **Link:** <https://discord.com/channels/875684761291599922/1509314495019745451>');
out.push('> - **Iniciado por:** Chazut — 27/05/2026 18:57 (GMT-3)');
out.push(`> - **Última mensagem capturada:** 04/06/2026 19:15 (GMT-3)`);
out.push(`> - **Total de mensagens:** ${cap.meta.total}`);
out.push('> - **Anexos:** 30 imagens + 2 logs, salvos em [`assets/`](./assets/)');
out.push('> - **Método:** navegação autenticada no Discord (extração do DOM via Chrome DevTools), capturado em 2026-06-04.');
out.push('> - **Notas:** horários convertidos para **GMT-3** (como exibidos no Discord do autor da captura). Texto **preservado no idioma original** (majoritariamente inglês). `↳@X` indica resposta direcionada a X. Linhas `📎` são anexos.');
out.push('');
out.push('---');
out.push('');

let curDay = null;
let n = 0;
for (const m of cap.messages) {
  n++;
  const { dateKey, time } = toGMT3(m.snowflake);
  if (dateKey !== curDay) {
    curDay = dateKey;
    out.push('');
    out.push(`## ${dateKey}`);
    out.push('');
  }
  const reply = (m.replyUser && m.replyUser.trim()) ? ` ↳${m.replyUser.replace(/^@?/, '@')}` : '';
  const author = (m.author || '???').trim();
  const text = cleanText(m.text);
  let line = `**${author}** · \`${time}\`${reply}`;
  if (text) {
    if (text.includes('\n')) {
      out.push(line);
      out.push(mdMultiline(text));
    } else {
      out.push(`${line} — ${text}`);
    }
  } else {
    out.push(line);
  }
  // attachments
  const atts = attByMsg[m.snowflake];
  if (atts) {
    for (const a of atts) {
      const icon = a.kind === 'log' ? '🧾' : '🖼️';
      out.push(`  ${icon} 📎 [\`${a.file}\`](./assets/${a.file})`);
    }
  }
  out.push('');
}

out.push('---');
out.push('');
out.push(`_Fim da transcrição — ${cap.meta.total} mensagens, de 27/05/2026 a 04/06/2026 (GMT-3)._`);
out.push('');

fs.writeFileSync(path.join(dir, '..', '01-transcricao.md'), out.join('\n'), 'utf8');
console.log('OK: 01-transcricao.md gerado com', n, 'mensagens.');
