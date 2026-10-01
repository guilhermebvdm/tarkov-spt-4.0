/**
 * Tarkov Red Line - Gerador de Manifesto do Jogo Base
 * 
 * Cria o arquivo 'base-manifest.json' a partir da pasta local do jogo base.
 * Esse manifesto é lido pelo Launcher para realizar o download HTTP direto multi-thread da Cloudflare R2.
 * 
 * Uso: node generate-base-manifest.js [caminho_pasta_base] [caminho_saida_manifest]
 * Exemplo: node generate-base-manifest.js "E:/Tarkov Red Line" "base-manifest.json"
 */

const fs = require("fs");
const path = require("path");
const crypto = require("crypto");

// Lista de arquivos a ignorar (temporários ou logs)
const IGNORE_PATTERNS = [
    /\.log$/i,
    /\.tmp$/i,
    /base-manifest\.json$/i,
    /base-game\.torrent$/i,
    /base-game-state\.json$/i,
    /desktop\.ini$/i,
    /thumbs\.db$/i,
    /\.git/i
];

function shouldIgnore(relPath) {
    return IGNORE_PATTERNS.some(pattern => pattern.test(relPath));
}

function getAllFiles(dirPath, arrayOfFiles = [], rootDir = dirPath) {
    const files = fs.readdirSync(dirPath);

    for (const file of files) {
        const fullPath = path.join(dirPath, file);
        const stat = fs.statSync(fullPath);

        if (stat.isDirectory()) {
            getAllFiles(fullPath, arrayOfFiles, rootDir);
        } else {
            const relPath = path.relative(rootDir, fullPath).replace(/\\/g, "/");
            if (!shouldIgnore(relPath)) {
                arrayOfFiles.push({ fullPath, relPath, size: stat.size });
            }
        }
    }

    return arrayOfFiles;
}

function computeSha256(filePath) {
    return new Promise((resolve, reject) => {
        const hash = crypto.createHash("sha256");
        const stream = fs.createReadStream(filePath);
        stream.on("data", chunk => hash.update(chunk));
        stream.on("end", () => resolve(hash.digest("hex")));
        stream.on("error", reject);
    });
}

async function main() {
    if (process.argv.includes("--help") || process.argv.includes("-h")) {
        console.log("=================================================");
        console.log("   Tarkov Red Line - Gerador de Manifesto Base   ");
        console.log("=================================================");
        console.log("Uso: node generate-base-manifest.js [caminho_base] [caminho_saida]");
        console.log("Exemplo:");
        console.log('  node generate-base-manifest.js "E:\\Tarkov Red Line" "base-manifest.json"');
        console.log("=================================================");
        process.exit(0);
    }

    const inputDir = process.argv[2] 
        ? path.resolve(process.argv[2]) 
        : path.resolve(process.cwd(), "Launcher-Updater", "base-client");

    const outputFile = process.argv[3] 
        ? path.resolve(process.argv[3]) 
        : path.resolve(process.cwd(), "base-manifest.json");

    console.log("=================================================");
    console.log("   Tarkov Red Line - Gerador de Manifesto Base   ");
    console.log("=================================================");
    console.log(`[+] Pasta do Jogo Base: ${inputDir}`);
    console.log(`[+] Arquivo de Saída:  ${outputFile}`);

    if (!fs.existsSync(inputDir)) {
        console.error(`\n[-] ERRO: A pasta de entrada não existe: ${inputDir}`);
        console.log(`\n💡 Dica: Especifique o caminho da sua pasta base:`);
        console.log(`  node mods/TarkovRedLine4.0/Server/scripts/generate-base-manifest.js "C:\\caminho\\da\\pasta" "base-manifest.json"\n`);
        process.exit(1);
    }

    console.log("\n[*] Escaneando arquivos...");
    const files = getAllFiles(inputDir);
    files.sort((a, b) => a.relPath.localeCompare(b.relPath));

    console.log(`[+] Total de arquivos encontrados: ${files.length}`);

    let totalBytes = 0;
    const manifestFiles = [];

    console.log("\n[*] Calculando hashes SHA-256 (aguarde alguns segundos)...");
    const startTime = Date.now();

    for (let i = 0; i < files.length; i++) {
        const item = files[i];
        totalBytes += item.size;

        if ((i + 1) % 100 === 0 || i === files.length - 1) {
            const percent = (((i + 1) / files.length) * 100).toFixed(1);
            process.stdout.write(`\r[*] Progresso: ${percent}% (${i + 1}/${files.length} arquivos)`);
        }

        const sha256 = await computeSha256(item.fullPath);
        manifestFiles.push({
            path: item.relPath,
            size: item.size,
            sha256: sha256
        });
    }

    const elapsedSec = ((Date.now() - startTime) / 1000).toFixed(1);
    console.log(`\n[+] Hashes calculados em ${elapsedSec}s!`);

    const manifest = {
        version: "1.0.0",
        generatedAt: new Date().toISOString(),
        totalFiles: manifestFiles.length,
        totalBytes: totalBytes,
        totalGigabytes: (totalBytes / (1024 * 1024 * 1024)).toFixed(2),
        files: manifestFiles
    };

    const outputDir = path.dirname(outputFile);
    if (!fs.existsSync(outputDir)) {
        fs.mkdirSync(outputDir, { recursive: true });
    }

    fs.writeFileSync(outputFile, JSON.stringify(manifest, null, 2), "utf8");

    console.log("\n=================================================");
    console.log("   MANIFESTO GERADO COM SUCESSO! 🎉");
    console.log("=================================================");
    console.log(`[+] Arquivo:      ${outputFile}`);
    console.log(`[+] Total Arquivos: ${manifest.totalFiles}`);
    console.log(`[+] Tamanho Total:  ${manifest.totalGigabytes} GB (${totalBytes} bytes)`);
    console.log("=================================================\n");
    console.log("👉 Próximo passo: Arraste esse arquivo 'base-manifest.json' para a raiz do seu bucket na Cloudflare (via Cyberduck)!\n");
}

main().catch(err => {
    console.error("[-] Falha crítica:", err);
    process.exit(1);
});
