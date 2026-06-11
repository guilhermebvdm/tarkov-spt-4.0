/**
 * hwid.ts — Gerenciamento de HWID (Hardware ID)
 * Responsável por: registro de HWID por usuário e validação para reset de senha
 */

import * as fs from "fs";
import * as path from "path";
import { ServerResponse } from "http";
import { ILogger } from "@spt/models/spt/utils/ILogger";

const hwidRegistryPath: string = path.join(process.cwd(), "user", "hwid_registry.json");
let hwidRegistry: Record<string, string> = {};

export function loadHwidRegistry(profilesPath: string, logger: ILogger): void {
    try {
        if (fs.existsSync(hwidRegistryPath)) {
            hwidRegistry = JSON.parse(fs.readFileSync(hwidRegistryPath, "utf8"));
            logger.info(`[HWID] Registry carregado: ${Object.keys(hwidRegistry).length} entradas`);
        } else {
            logger.info("[HWID] Registry não encontrado. Migrando HWIDs dos perfis...");
            if (fs.existsSync(profilesPath)) {
                const profileFiles = fs.readdirSync(profilesPath).filter(f => f.endsWith(".json"));
                for (const file of profileFiles) {
                    try {
                        const data = JSON.parse(fs.readFileSync(path.join(profilesPath, file), "utf8"));
                        if (data.info?.username && data.info?.hwid && data.info.hwid !== "") {
                            hwidRegistry[data.info.username.toLowerCase()] = data.info.hwid;
                        }
                    } catch (e) { /* skip */ }
                }
                logger.info(`[HWID] Migrados ${Object.keys(hwidRegistry).length} HWIDs.`);
            }
            _save(logger);
        }
    } catch (e) {
        logger.error(`[HWID] Erro ao carregar: ${e.message}`);
    }
}

function _save(logger?: ILogger): void {
    try {
        const dir = path.dirname(hwidRegistryPath);
        if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
        fs.writeFile(hwidRegistryPath, JSON.stringify(hwidRegistry, null, "\t"), "utf8", err => {
            if (err && logger) logger.error(`[HWID] Erro ao salvar: ${err.message}`);
        });
    } catch (e) {
        if (logger) logger.error(`[HWID] Erro ao salvar: ${e.message}`);
    }
}

export function getHwid(username: string): string {
    return hwidRegistry[username.toLowerCase()] || "";
}

export function setHwid(username: string, hwid: string): void {
    hwidRegistry[username.toLowerCase()] = hwid;
    _save();
}

// Handlers HTTP
export function handleRegister(body: string, res: ServerResponse, logger: ILogger): void {
    const { username, hwid } = JSON.parse(body);
    if (!username || !hwid) { res.writeHead(400); res.end(JSON.stringify({ status: "INVALID_REQUEST" })); return; }
    if (getHwid(username) !== "") {
        logger.info(`[HWID] ${username} já registrado.`);
        res.writeHead(200); res.end(JSON.stringify({ status: "ALREADY_REGISTERED" })); return;
    }
    setHwid(username, hwid);
    logger.success(`[HWID] Registrado para ${username}: ${hwid.substring(0, 8)}...`);
    res.writeHead(200); res.end(JSON.stringify({ status: "OK" }));
}

export function handleResetPassword(body: string, res: ServerResponse, logger: ILogger): void {
    const { username, hwid } = JSON.parse(body);
    if (!username || !hwid) { res.writeHead(400); res.end(JSON.stringify({ status: "INVALID_REQUEST" })); return; }
    const stored = getHwid(username);
    if (stored === "") {
        logger.warning(`[HWID] ${username} não tem HWID registrado.`);
        res.writeHead(403); res.end(JSON.stringify({ status: "NO_HWID_REGISTERED" })); return;
    }
    if (stored !== hwid) {
        logger.warning(`[HWID] ${username} HWID incorreto.`);
        res.writeHead(403); res.end(JSON.stringify({ status: "HWID_MISMATCH" })); return;
    }
    logger.success(`[HWID] Reset autorizado para ${username}.`);
    res.writeHead(200); res.end(JSON.stringify({ status: "OK" }));
}
