// Gera icons-data.js com os badges das classes embutidos como data URI (base64),
// para o template usar como mask-image sem esbarrar no bloqueio de file:// do Chrome.
const fs = require("fs");
const path = require("path");

const dir = path.join(__dirname, "icons");
const out = {};
for (const f of fs.readdirSync(dir)) {
  if (!f.toLowerCase().endsWith(".png")) continue;
  const key = path.basename(f, path.extname(f));
  const b64 = fs.readFileSync(path.join(dir, f)).toString("base64");
  out[key] = "data:image/png;base64," + b64;
}
fs.writeFileSync(path.join(__dirname, "icons-data.js"), "window.ICON_DATA = " + JSON.stringify(out) + ";\n");
console.log("wrote icons-data.js with", Object.keys(out).length, "icons:", Object.keys(out).join(", "));
