/**
 * Tarkov Red Line - Gerador de Torrent do Jogo Base
 * 
 * Cria o arquivo 'base-game.torrent' (~290 KB) a partir da pasta 'Launcher-Updater/base-client' (~57.4 GB)
 * com blocos de 4 MB e suporte a WebSeed HTTP.
 * 
 * Uso: node generate-base-torrent.js [caminho_base_client] [caminho_saida_torrent] [url_webseed]
 * Exemplo: node generate-base-torrent.js "D:/SPT 4.0/Launcher-Updater/base-client" "D:/SPT 4.0/Launcher-Updater/base-game.torrent"
 */

const fs = require("fs");
const path = require("path");
const crypto = require("crypto");

// Configurações Padrão
const DEFAULT_INPUT_DIR = path.resolve(process.cwd(), "Launcher-Updater", "base-client");
const DEFAULT_OUTPUT_FILE = path.resolve(process.cwd(), "Launcher-Updater", "base-game.torrent");
const DEFAULT_WEBSEED = "/redline/base-game/";
const PIECE_LENGTH = 4 * 1024 * 1024; // 4 MB (CR-01-04)

// --- Bencoder Standalone (Sem dependências externas) ---
function bencode(obj) {
    if (typeof obj === "string") {
        const buf = Buffer.from(obj, "utf8");
        return Buffer.concat([Buffer.from(`${buf.length}:`), buf]);
    } else if (Buffer.isBuffer(obj)) {
        return Buffer.concat([Buffer.from(`${obj.length}:`), obj]);
    } else if (typeof obj === "number") {
        return Buffer.from(`i${Math.floor(obj)}e`);
    } else if (Array.isArray(obj)) {
        const bufs = [Buffer.from("l")];
        for (const item of obj) {
            bufs.push(bencode(item));
        }
        bufs.push(Buffer.from("e"));
        return Buffer.concat(bufs);
    } else if (typeof obj === "object" && obj !== null) {
        const keys = Object.keys(obj).sort();
        const bufs = [Buffer.from("d")];
        for (const key of keys) {
            bufs.push(bencode(key));
            bufs.push(bencode(obj[key]));
        }
        bufs.push(Buffer.from("e"));
        return Buffer.concat(bufs);
    }
    throw new Error(`Tipo não suportado para bencode: ${typeof obj}`);
}

function getAllFiles(dirPath, arrayOfFiles = [], rootDir = dirPath) {
    const files = fs.readdirSync(dirPath);

    for (const file of files) {
        const fullPath = path.join(dirPath, file);
        if (fs.statSync(fullPath).isDirectory()) {
            getAllFiles(fullPath, arrayOfFiles, rootDir);
        } else {
            const relPath = path.relative(rootDir, fullPath).replace(/\\/g, "/");
            const size = fs.statSync(fullPath).size;
            arrayOfFiles.push({ fullPath, relPath, size });
        }
    }

    return arrayOfFiles;
}

async function main() {
    if (process.argv.includes("--help") || process.argv.includes("-h")) {
        console.log("=================================================");
        console.log("   Tarkov Red Line - Gerador de Torrent Base    ");
        console.log("=================================================");
        console.log("Uso: node generate-base-torrent.js [caminho_base] [caminho_saida] [url_webseed]");
        console.log("Exemplo:");
        console.log('  node generate-base-torrent.js "D:\\SPT 4.0\\Launcher-Updater\\base-client" "D:\\SPT 4.0\\Launcher-Updater\\base-game.torrent"');
        console.log("=================================================");
        process.exit(0);
    }

    const inputDir = process.argv[2] ? path.resolve(process.argv[2]) : DEFAULT_INPUT_DIR;
    const outputFile = process.argv[3] ? path.resolve(process.argv[3]) : DEFAULT_OUTPUT_FILE;
    const webSeedUrl = process.argv[4] || DEFAULT_WEBSEED;

    console.log("=================================================");
    console.log("   Tarkov Red Line - Gerador de Torrent Base    ");
    console.log("=================================================");
    console.log(`[+] Pasta do Jogo Base: ${inputDir}`);
    console.log(`[+] Arquivo de Saída:  ${outputFile}`);
    console.log(`[+] Tamanho de Bloco:  4 MB (${PIECE_LENGTH} bytes)`);

    if (!fs.existsSync(inputDir)) {
        console.error(`[-] ERRO: A pasta de entrada não existe: ${inputDir}`);
        process.exit(1);
    }

    console.log("[*] Escaneando arquivos do jogo base...");
    const files = getAllFiles(inputDir);
    files.sort((a, b) => a.relPath.localeCompare(b.relPath));

    let totalBytes = 0;
    const torrentFiles = [];

    for (const f of files) {
        totalBytes += f.size;
        torrentFiles.push({
            length: f.size,
            path: f.relPath.split("/")
        });
    }

    console.log(`[+] Total de Arquivos: ${files.length}`);
    console.log(`[+] Tamanho Total:     ${(totalBytes / (1024 * 1024 * 1024)).toFixed(2)} GB (${totalBytes} bytes)`);

    const totalPieces = Math.ceil(totalBytes / PIECE_LENGTH);
    console.log(`[+] Total de Peças:    ${totalPieces}`);
    console.log("[*] Calculando hashes SHA-1 dos blocos (aguarde)...");

    const pieceHashes = [];
    let currentPieceBuffer = Buffer.alloc(PIECE_LENGTH);
    let currentPieceOffset = 0;
    let processedBytes = 0;
    let lastReportPercent = -1;

    for (const file of files) {
        const fd = fs.openSync(file.fullPath, "r");
        let fileOffset = 0;
        const fileBuffer = Buffer.alloc(64 * 1024);

        while (fileOffset < file.size) {
            const bytesRead = fs.readSync(fd, fileBuffer, 0, Math.min(fileBuffer.length, file.size - fileOffset), fileOffset);
            fileOffset += bytesRead;
            processedBytes += bytesRead;

            let sourceOffset = 0;
            while (sourceOffset < bytesRead) {
                const spaceInPiece = PIECE_LENGTH - currentPieceOffset;
                const bytesToCopy = Math.min(spaceInPiece, bytesRead - sourceOffset);

                fileBuffer.copy(currentPieceBuffer, currentPieceOffset, sourceOffset, sourceOffset + bytesToCopy);
                currentPieceOffset += bytesToCopy;
                sourceOffset += bytesToCopy;

                if (currentPieceOffset === PIECE_LENGTH) {
                    const hash = crypto.createHash("sha1").update(currentPieceBuffer).digest();
                    pieceHashes.push(hash);
                    currentPieceOffset = 0;
                }
            }

            const percent = Math.floor((processedBytes * 100) / totalBytes);
            if (percent !== lastReportPercent) {
                process.stdout.write(`\r[*] Progresso do Hash: ${percent}% (${pieceHashes.length}/${totalPieces} peças)`);
                lastReportPercent = percent;
            }
        }

        fs.closeSync(fd);
    }

    // Último pedaço menor que 4 MB
    if (currentPieceOffset > 0) {
        const finalChunk = currentPieceBuffer.slice(0, currentPieceOffset);
        const hash = crypto.createHash("sha1").update(finalChunk).digest();
        pieceHashes.push(hash);
    }

    console.log("\n[+] Todos os blocos foram calculados com sucesso!");

    const piecesBuffer = Buffer.concat(pieceHashes);

    const torrentDict = {
        "announce": "http://127.0.0.1:6969/announce",
        "created by": "Tarkov Red Line Torrent Generator v2.10",
        "creation date": Math.floor(Date.now() / 1000),
        "info": {
            "files": torrentFiles,
            "name": "SPT",
            "piece length": PIECE_LENGTH,
            "pieces": piecesBuffer
        }
    };

    if (webSeedUrl) {
        torrentDict["url-list"] = [webSeedUrl];
        torrentDict["httpseeds"] = [webSeedUrl];
    }

    console.log("[*] Codificando dicionário Bencode do Torrent...");
    const torrentData = bencode(torrentDict);

    const outDir = path.dirname(outputFile);
    if (!fs.existsSync(outDir)) {
        fs.mkdirSync(outDir, { recursive: true });
    }

    fs.writeFileSync(outputFile, torrentData);
    console.log(`[✔] ARQUIVO TORRENT CRIADO: ${outputFile} (${(torrentData.length / 1024).toFixed(1)} KB)`);
    console.log("=================================================");
}

main().catch(err => {
    console.error("[-] ERRO FATAL:", err);
});
