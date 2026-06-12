// build-icons.mjs — authoring-time asset pipeline (NOT part of the mod runtime).
//
// Rasterizes the vendored game-icons.net SVGs in `icon-sources/` into white PNG
// class icons (256x256, white silhouette + transparent alpha) in TWO destinations:
// `../modded/Client/icons/` (tinted at runtime by the client — ChatSpecialIconPatch /
// ClassIdentityView) and `../modded/Server/wwwroot/icons/` (served by the SPT web
// host at /CustomClasses-Server/icons/ for the class editor preview — item 020).
// Same file name as in CLASS_VISUAL (build-class-jsons.js).
//
// game-icons.net is CC BY 3.0 — see ../modded/Client/icons/ATTRIBUTION.md.
// Usage:  npm install && npm run build:icons   (run from this scripts/ folder)
// Swap art = replace the .svg here and re-run; no DLL recompile needed.

import { readdir, readFile, writeFile, mkdir } from "node:fs/promises";
import { fileURLToPath } from "node:url";
import { dirname, join, basename } from "node:path";
import sharp from "sharp";

const here = dirname(fileURLToPath(import.meta.url));
const SRC = join(here, "icon-sources");
const OUTS = [
  join(here, "..", "modded", "Client", "icons"),
  join(here, "..", "modded", "Server", "wwwroot", "icons"),
];

const SIZE = 256; // final square canvas
const PAD = 28; // transparent padding per side (~11% breathing room)
const INNER = SIZE - PAD * 2; // 200

// A game-icons.net SVG is: <path d="M0 0h512v512H0z"/> (black background square)
// followed by <path fill="#fff" .../> (the icon). We strip the background rect so
// only the white icon remains on transparent — ready to be tinted at runtime.
const BG_RECT = /<path\s+d="M0 0h512v512H0z"\s*\/>/;
const TRANSPARENT = { r: 0, g: 0, b: 0, alpha: 0 };

const run = async () => {
  for (const out of OUTS) {
    await mkdir(out, { recursive: true });
  }
  const files = (await readdir(SRC)).filter((f) => f.endsWith(".svg")).sort();
  if (files.length === 0) {
    throw new Error(`no .svg files found in ${SRC}`);
  }

  for (const f of files) {
    const name = basename(f, ".svg");
    let svg = await readFile(join(SRC, f), "utf8");
    if (!BG_RECT.test(svg)) {
      console.warn(`! ${f}: expected game-icons background rect not found — emitting as-is`);
    }
    svg = svg.replace(BG_RECT, "");

    const png = await sharp(Buffer.from(svg), { density: 512 })
      .resize(INNER, INNER, { fit: "contain", background: TRANSPARENT })
      .extend({ top: PAD, bottom: PAD, left: PAD, right: PAD, background: TRANSPARENT })
      .png({ compressionLevel: 9 })
      .toBuffer();

    for (const out of OUTS) {
      await writeFile(join(out, `${name}.png`), png);
    }

    console.log(`✓ ${name}.png (${SIZE}x${SIZE})`);
  }

  console.log(`\n${files.length} icons → ${OUTS.join(" , ")}`);
};

run().catch((e) => {
  console.error(e);
  process.exit(1);
});
