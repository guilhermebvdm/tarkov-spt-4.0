/**
 * playerIps.ts — Rastreamento de IPs dos jogadores
 */

import * as fs from "fs";
import * as path from "path";
import { ServerResponse } from "http";
import { ILogger } from "@spt/models/spt/utils/ILogger";

interface PlayerIpEntry {
    ip: string;
    lastSeen: string;
}

const playerIpsPath: string = path.join(process.cwd(), "user", "player_ips.json");
let playerIps: Record<string, PlayerIpEntry> = {};

export function load(logger: ILogger): void {
    try {
        if (fs.existsSync(playerIpsPath)) {
            playerIps = JSON.parse(fs.readFileSync(playerIpsPath, "utf8"));
            logger.info(`[PlayerIPs] Carregado: ${Object.keys(playerIps).length} jogadores`);
        }
    } catch (e) {
        logger.error(`[PlayerIPs] Erro ao carregar: ${e instanceof Error ? e.message : String(e)}`);
    }
}

export function save(logger?: ILogger): void {
    try {
        const dir = path.dirname(playerIpsPath);
        if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
        fs.writeFile(playerIpsPath, JSON.stringify(playerIps, null, "\t"), "utf8", err => {
            if (err && logger) logger.error(`[PlayerIPs] Erro ao salvar: ${err.message}`);
        });
    } catch (e) {
        if (logger) logger.error(`[PlayerIPs] Erro ao salvar: ${e instanceof Error ? e.message : String(e)}`);
    }
}

export function handleRegisterIp(body: string, res: ServerResponse, logger: ILogger): void {
    const { username, ip } = JSON.parse(body);
    if (!username || !ip) { res.writeHead(400); res.end(JSON.stringify({ status: "INVALID_REQUEST" })); return; }
    const lastSeen = new Date().toLocaleString("pt-BR", {
        timeZone: "America/Sao_Paulo", day: "2-digit", month: "2-digit",
        year: "numeric", hour: "2-digit", minute: "2-digit", second: "2-digit"
    });
    playerIps[username] = { ip, lastSeen };
    save(logger);
    logger.info(`[PlayerIPs] ${username} => ${ip}`);
    res.writeHead(200); res.end(JSON.stringify({ status: "OK" }));
}

export function handleList(res: ServerResponse): void {
    res.writeHead(200, { "Content-Type": "application/json" });
    res.end(JSON.stringify(playerIps));
}
