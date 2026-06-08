/**
 * modUpdater.ts — Manifesto de mods e download para o launcher
 */

import * as fs from "fs";
import * as path from "path";
import * as crypto from "crypto";
import { ServerResponse } from "http";
import { ILogger } from "@spt/models/spt/utils/ILogger";

interface ManifestFile {
    path: string;
    hash: string;
    size: number;
    optional?: boolean;
    optionalGroup?: string;
}

interface OptionalGroupMeta {
    id: string;
    name: string;
    description: string;
    hasOffMode: boolean;
    targetSubDir: string;
    offFolders: string[];
}

interface Manifest {
    serverVersion: string;
    generatedAt: string;
    totalFiles: number;
    managedPaths: string[];
    deleteFiles: string[];
    ignoredFiles: string[];
    optionalGroups: OptionalGroupMeta[];
    files: ManifestFile[];
}

let manifestCache: Manifest | null = null;
let manifestGenerating: boolean = false;
let manifestHash: string = "";
let fileMapCache: Record<string, string> = {}; // Mapeia: virtualPath -> physicalPath

function getFileHashAsync(filePath: string): Promise<string> {
    return new Promise((resolve, reject) => {
        const hash = crypto.createHash("md5");
        const stream = fs.createReadStream(filePath);
        stream.on("data", (chunk: any) => hash.update(chunk));
        stream.on("end", () => resolve(hash.digest("hex")));
        stream.on("error", reject);
    });
}

function getFileHash(filePath: string): string {
    return crypto.createHash("md5").update(fs.readFileSync(filePath)).digest("hex");
}

function yieldEventLoop(): Promise<void> {
    return new Promise(resolve => setTimeout(resolve, 0));
}

export async function generateManifest(
    modsRepoPath: string,
    managedPaths: string[],
    deleteFiles: string[],
    ignoredFiles: string[],
    serverVersion: string,
    logger: ILogger,
    optionalGroups: any[],
    opcionaisPath: string
): Promise<Manifest | null> {
    if (manifestGenerating) return manifestCache;
    manifestGenerating = true;
    try {
        const files: ManifestFile[] = [];

        // === 1. Escanear mods obrigatórios (modsRepoPath) ===
        const dirsToScan: { dir: string; rel: string }[] = [{ dir: modsRepoPath, rel: "" }];
        while (dirsToScan.length > 0) {
            const { dir, rel } = dirsToScan.shift()!;
            if (!fs.existsSync(dir)) continue;
            let entries: fs.Dirent[];
            try { entries = fs.readdirSync(dir, { withFileTypes: true }); } catch (e: any) { continue; }
            for (const entry of entries) {
                const fullPath = path.join(dir, entry.name);
                const relPath = rel ? `${rel}/${entry.name}` : entry.name;

                const relPathLower = relPath.toLowerCase().replace(/\\/g, "/");
                if (ignoredFiles && ignoredFiles.some(ignore => relPathLower.includes(ignore.toLowerCase().replace(/\\/g, "/")))) {
                    continue; // Pula este arquivo/pasta (lista de exceção)
                }

                if (entry.isDirectory()) { dirsToScan.push({ dir: fullPath, rel: relPath }); }
                else if (entry.isFile()) {
                    try {
                        const stats = fs.statSync(fullPath);
                        const hash = await getFileHashAsync(fullPath);
                        files.push({ path: relPath, hash, size: stats.size });
                        fileMapCache[relPath.replace(/\\/g, "/")] = fullPath;
                    } catch (e: any) { logger.warning(`[ModUpdater] Erro ao ler ${relPath}: ${e.message}`); }
                }
            }
            await yieldEventLoop();
        }

        logger.info(`[ModUpdater] Obrigatórios: ${files.length} arquivos escaneados`);

        // === 2. Escanear mods opcionais (opcionaisPath/optionalGroups) ===
        const manifestOptionalGroups: OptionalGroupMeta[] = [];
        if (Array.isArray(optionalGroups) && opcionaisPath) {
            for (const group of optionalGroups) {
                const groupMeta: OptionalGroupMeta = {
                    id: group.id,
                    name: group.name,
                    description: group.description || "",
                    hasOffMode: !!(group.offFolders && group.offFolders.length > 0),
                    targetSubDir: group.targetSubDir || "",
                    offFolders: group.offFolders || []
                };
                manifestOptionalGroups.push(groupMeta);

                if (!Array.isArray(group.folders)) continue;

                let groupFileCount = 0;
                for (const folder of group.folders) {
                    const folderPath = path.join(opcionaisPath, folder);
                    if (!fs.existsSync(folderPath)) {
                        logger.warning(`[ModUpdater] Pasta opcional não encontrada: ${folderPath}`);
                        continue;
                    }

                    // Escanear recursivamente
                    const optDirs: { dir: string; rel: string }[] = [{ dir: folderPath, rel: "" }];
                    while (optDirs.length > 0) {
                        const { dir: od, rel: or } = optDirs.shift()!;
                        let optEntries: fs.Dirent[];
                        try { optEntries = fs.readdirSync(od, { withFileTypes: true }); } catch (e: any) { continue; }
                        for (const entry of optEntries) {
                            const fp = path.join(od, entry.name);
                            const rp = or ? `${or}/${entry.name}` : entry.name;
                            if (entry.isDirectory()) { optDirs.push({ dir: fp, rel: rp }); }
                            else if (entry.isFile()) {
                                try {
                                    const stats = fs.statSync(fp);
                                    const hash = await getFileHashAsync(fp);
                                    // Calcular path final: targetSubDir + relativePath
                                    const finalPath = group.targetSubDir
                                        ? `${group.targetSubDir}/${rp}`
                                        : rp;

                                    const finalPathNormalized = finalPath.replace(/\\/g, "/");
                                    files.push({
                                        path: finalPathNormalized,
                                        hash,
                                        size: stats.size,
                                        optional: true,
                                        optionalGroup: group.id
                                    });
                                    fileMapCache[finalPathNormalized] = fp;
                                    groupFileCount++;
                                } catch (e: any) { /* skip */ }
                            }
                        }
                        await yieldEventLoop();
                    }
                }
                logger.info(`[ModUpdater] Opcional '${group.id}': ${groupFileCount} arquivos`);
            }
        }

        manifestCache = {
            serverVersion,
            generatedAt: new Date().toISOString(),
            totalFiles: files.length,
            managedPaths,
            deleteFiles,
            ignoredFiles,
            optionalGroups: manifestOptionalGroups,
            files
        };
        const manifestJson = JSON.stringify(manifestCache);
        manifestHash = crypto.createHash("md5").update(manifestJson).digest("hex");
        logger.info(`[ModUpdater] Manifesto gerado: ${files.length} arquivos (${manifestOptionalGroups.length} grupos opcionais) | hash: ${manifestHash}`);
        return manifestCache;
    } catch (e: any) {
        logger.error(`[ModUpdater] Erro: ${e.message}`);
        return manifestCache;
    } finally {
        manifestGenerating = false;
    }
}

export function getManifest(): Manifest | null { return manifestCache; }
export function getManifestHash(): string { return manifestHash; }
export function invalidateCache(): void { manifestCache = null; manifestHash = ""; }

// Handlers HTTP
export function handleManifestHash(res: ServerResponse): void {
    const hash = getManifestHash();
    if (!hash) { res.writeHead(503); res.end(JSON.stringify({ error: "Manifesto ainda sendo gerado" })); return; }
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify({ hash }));
}

export function handleManifest(res: ServerResponse): void {
    const manifest = getManifest();
    if (!manifest) { res.writeHead(503); res.end(JSON.stringify({ error: "Manifesto ainda sendo gerado" })); return; }
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify(manifest));
}

export function handleDownload(url: string, modsRepoPath: string, res: ServerResponse, logger: ILogger): void {
    try {
        const urlObj = new URL(url, "http://localhost");
        const filePath = urlObj.searchParams.get("file");
        if (!filePath) { res.writeHead(400); res.end(JSON.stringify({ error: "Missing 'file'" })); return; }

        let fullPath = "";
        const normalizedFile = path.normalize(filePath).replace(/\\/g, "/").replace(/^\/+/, "");

        if (fileMapCache[normalizedFile]) {
            // Usa o mapeamento gerado no manifesto (suporta mods obrigatórios e opcionais)
            fullPath = fileMapCache[normalizedFile];
        } else {
            // Fallback: tenta buscar dentro do modsRepoPath
            fullPath = path.join(modsRepoPath, path.normalize(filePath).replace(/\.\./g, ""));
        }

        if (!fs.existsSync(fullPath) || !fs.statSync(fullPath).isFile()) {
            res.writeHead(404); res.end(JSON.stringify({ error: "File not found: " + normalizedFile })); return;
        }

        const stats = fs.statSync(fullPath);
        res.writeHead(200, { "Content-Type": "application/octet-stream", "Content-Length": stats.size, "Content-Disposition": `attachment; filename="${path.basename(fullPath)}"` });
        res.end(fs.readFileSync(fullPath));
    } catch (e: any) {
        logger.error(`[ModUpdater] Erro download: ${e.message}`);
        res.writeHead(500); res.end(JSON.stringify({ error: e.message }));
    }
}

export function handleOpcionaisList(opcionaisPath: string, res: ServerResponse): void {
    try {
        if (!fs.existsSync(opcionaisPath)) { res.writeHead(200); res.end(JSON.stringify({ folders: [] })); return; }
        const folders = fs.readdirSync(opcionaisPath, { withFileTypes: true }).filter((e: any) => e.isDirectory()).map((e: any) => e.name);
        res.writeHead(200, { "Content-Type": "application/json" });
        res.end(JSON.stringify({ folders }));
    } catch (e: any) { res.writeHead(500); res.end(JSON.stringify({ error: e.message })); }
}

export function handleOpcionaisManifest(url: string, opcionaisPath: string, res: ServerResponse, logger: ILogger): void {
    try {
        const urlObj = new URL(url, "http://localhost");
        const folder = urlObj.searchParams.get("folder");
        if (!folder) { res.writeHead(400); res.end(JSON.stringify({ error: "Missing 'folder'" })); return; }
        const folderPath = path.join(opcionaisPath, path.normalize(folder).replace(/\.\./g, ""));
        if (!folderPath.startsWith(opcionaisPath) || !fs.existsSync(folderPath)) {
            res.writeHead(404); res.end(JSON.stringify({ error: "Folder not found" })); return;
        }
        const files: ManifestFile[] = [];
        function scan(dir: string, rel: string): void {
            for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
                const fp = path.join(dir, entry.name);
                const rp = rel ? `${rel}/${entry.name}` : entry.name;
                if (entry.isDirectory()) scan(fp, rp);
                else { try { files.push({ path: rp, hash: getFileHash(fp), size: fs.statSync(fp).size }); } catch (e: any) { /* skip */ } }
            }
        }
        scan(folderPath, "");
        res.writeHead(200, { "Content-Type": "application/json" });
        res.end(JSON.stringify({ folder, totalFiles: files.length, files }));
    } catch (e: any) {
        logger.error(`[Opcionais] Erro manifesto: ${e.message}`);
        res.writeHead(500); res.end(JSON.stringify({ error: e.message }));
    }
}

export function handleOpcionaisDownload(url: string, opcionaisPath: string, res: ServerResponse, logger: ILogger): void {
    try {
        const urlObj = new URL(url, "http://localhost");
        const folder = urlObj.searchParams.get("folder");
        const filePath = urlObj.searchParams.get("file");
        if (!folder || !filePath) { res.writeHead(400); res.end(JSON.stringify({ error: "Missing params" })); return; }
        const full = path.join(opcionaisPath, path.normalize(folder).replace(/\.\./g, ""), path.normalize(filePath).replace(/\.\./g, ""));
        if (!full.startsWith(opcionaisPath) || !fs.existsSync(full) || !fs.statSync(full).isFile()) {
            res.writeHead(404); res.end(JSON.stringify({ error: "File not found" })); return;
        }
        const stats = fs.statSync(full);
        res.writeHead(200, { "Content-Type": "application/octet-stream", "Content-Length": stats.size, "Content-Disposition": `attachment; filename="${path.basename(full)}"` });
        res.end(fs.readFileSync(full));
    } catch (e: any) {
        logger.error(`[Opcionais] Erro download: ${e.message}`);
        res.writeHead(500); res.end(JSON.stringify({ error: e.message }));
    }
}
