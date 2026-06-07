import { DependencyContainer } from "tsyringe";
import { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";
import { StaticRouterModService } from "@spt/services/mod/staticRouter/StaticRouterModService";
import { ILogger } from "@spt/models/spt/utils/ILogger";
import * as path from "path";
import * as fs from "fs";
import * as http from "http";
import * as child_process from "child_process";

// Módulos TypeScript
import * as hwid       from "./hwid";
import * as playerIps  from "./playerIps";
import * as updater    from "./modUpdater";
// import * as targram    from "./targramSync";
import playerStatus    from "./playerStatus";

class TarkovRedLineMod implements IPreSptLoadMod, IPostDBLoadMod {

    // === Configurações de votação (RedLineRestart 2.7 - Veto Reversível) ===
    private VOTE_TIME     = 180000;    // 3 minutos
    private VETO_TIME     = 30000;     // 30 segundos (tempo encurtado no veto)
    private COOLDOWN_TIME = 600000;    // 10 minutos

    private state = {
        inProgress: false,
        isVetoed: false,
        yesVotes: 0,
        noVotes: 0,
        endTime: 0,
        originalEndTime: 0,
        cooldownUntil: 0,
        triggerRestart: false,
        message: "Pronto para votação",
        votedIPs: {} as Record<string, boolean> 
    };

    // === Configurações de VPN ===
    private vpnConfig = {
        serverPublicKey: "mm0k+XBK4tu/JARXszXZN19Tkh2fIDgGb8MY/AKlmWA=",
        wireGuardInterfaceName: "Tarkov_Red_Line",
        wireGuardConfPath: "C:\\Escape.From.Tarkov.v0.16.1.3.35392-P2P\\VPN\\Tarkov_Red_Line.conf",
        networkPrefix: "10.0.0."
    };
    private peersDbPath!: string;
    private logger: ILogger;

    public preSptLoad(container: DependencyContainer): void {
        this.logger = container.resolve<ILogger>("WinstonLogger");
        this.peersDbPath = path.join(__dirname, "..", "db", "peers.json");

        if (!fs.existsSync(path.dirname(this.peersDbPath))) {
            fs.mkdirSync(path.dirname(this.peersDbPath), { recursive: true });
        }

        const staticRouterModService = container.resolve<StaticRouterModService>("StaticRouterModService");
    }

    public postDBLoad(container: DependencyContainer): void {
        const logger      = this.logger || container.resolve<ILogger>("WinstonLogger");
        const profilesPath = path.join(process.cwd(), "user", "profiles");
        const configPath  = path.join(__dirname, "..", "config.json");

        // Configuração HWID/Launcher
        let modsRepoPath   = path.join(__dirname, "..", "mods_repo");
        let opcionaisPath  = path.join(__dirname, "..", "Opcionais");
        let managedPaths   = [];
        let deleteFiles    = [];
        let ignoredFiles   = [];
        let serverVersion  = "0.0.0";
        let launcherVersion = "1.0.0";
        let optionalGroups = [];

        try {
            if (fs.existsSync(configPath)) {
                const config = JSON.parse(fs.readFileSync(configPath, "utf8"));
                if (config.modsRepoPath)                modsRepoPath    = path.resolve(process.cwd(), config.modsRepoPath);
                if (config.opcionaisPath)               opcionaisPath   = path.resolve(process.cwd(), config.opcionaisPath);
                if (Array.isArray(config.managedPaths)) managedPaths    = config.managedPaths;
                if (Array.isArray(config.deleteFiles))  deleteFiles     = config.deleteFiles;
                if (Array.isArray(config.ignoredFiles)) ignoredFiles    = config.ignoredFiles;
                if (Array.isArray(config.optionalGroups)) optionalGroups = config.optionalGroups;
                if (config.serverVersion)               serverVersion   = config.serverVersion;
                if (config.launcherVersion)             launcherVersion = config.launcherVersion;
            }
        } catch (e: any) {
            logger.error(`[RedLine] Erro ao ler config.json: ${e.message}`);
        }

        try {
            const staticRouterModService = container.resolve<StaticRouterModService>("StaticRouterModService");
            staticRouterModService.registerStaticRouter(
                "TarkovRedLineVersionRouter",
                [
                    {
                        url: "/launcher/mods/version",
                        action: async (url: string, info: any, sessionId: string, output: string) => {
                            return JSON.stringify({ version: serverVersion });
                        }
                    },
                    {
                        url: "/redline/vote/status",
                        action: async (url: string, info: any, sessionId: string, output: string) => {
                            const now = Date.now();
                            const timeLeft = this.state.inProgress ? Math.max(0, Math.floor((this.state.endTime - now) / 1000)) : 0;
                            const cooldownLeft = Math.max(0, Math.floor((this.state.cooldownUntil - now) / 1000));
                            return JSON.stringify({ 
                                inProgress: this.state.inProgress, 
                                isVetoed: this.state.isVetoed, 
                                yesVotes: this.state.yesVotes, 
                                timeLeft, 
                                cooldownLeft,
                                triggerRestart: this.state.triggerRestart
                            });
                        }
                    },
                    {
                        url: "/redline/headless/ack-restart",
                        action: async (url: string, info: any, sessionId: string, output: string) => {
                            this.state.triggerRestart = false;
                            logger.success("[REDLINE] ACK de restart recebido do Headless na porta 7073! Flag triggerRestart zerada com precisão.");
                            return JSON.stringify({ status: "ok" });
                        }
                    }
                ],
                "redline-version"
            );
        } catch (e: any) {}

        // ─── Fix Crucial Fika: ProfileHelper.sanitizeProfileForClient null-check ──
        try {
            const profileHelper = container.resolve<any>("ProfileHelper");
            const originalGetCompleteProfile = profileHelper.getCompleteProfile.bind(profileHelper);
            
            profileHelper.getCompleteProfile = function(sessionId: string) {
                try {
                    const saveServer = container.resolve<any>("SaveServer");
                    const profile = saveServer.getProfile(sessionId);
                    if (profile) {
                        if (profile.characters?.pmc && !profile.characters.pmc.TradersInfo) {
                            profile.characters.pmc.TradersInfo = {};
                        }
                        if (!profile.dialogues) {
                            profile.dialogues = {};
                        }
                    }
                } catch (fixError) {}
                return originalGetCompleteProfile(sessionId);
            };
            logger.info("[RedLine] Fix ProfileHelper (Fika) aplicado.");
        } catch (err: any) {}

        // ─── Fix Flash-Reload da IA ──────────────────────────────────────────
        try {
            const databaseServer = container.resolve<any>("DatabaseServer");
            const tables = databaseServer.getTables();
            if (tables?.bots?.types) {
                for (const botType in tables.bots.types) {
                    const skills = tables.bots.types[botType]?.skills?.Common;
                    if (skills?.BotReload) {
                        skills.BotReload.min = 0;
                        skills.BotReload.max = 0;
                    }
                }
            }
        } catch (err: any) {}

        // Inicializar módulos JS Legacy
        hwid.loadHwidRegistry(profilesPath, logger);
        playerIps.load(logger);
        playerStatus.init(container, logger);

        logger.info("[ModUpdater] Pré-gerando manifesto em background...");
        updater.generateManifest(modsRepoPath, managedPaths, deleteFiles, ignoredFiles, serverVersion, logger, optionalGroups, opcionaisPath);

        const self = this;

        // === Servidor HTTP Unificado (Porta 7075) ===
        // Atende HWID, Launcher, ModUpdater, e Vote Restart // Targram desativado
        const server = http.createServer((req: any, res: any) => {
            res.setHeader("Access-Control-Allow-Origin", "*");
            res.setHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            res.setHeader("Access-Control-Allow-Headers", "Content-Type");

            if (req.method === "OPTIONS") { res.writeHead(200); res.end(); return; }

            const url      = req.url || "";
            const clientIP = req.socket.remoteAddress || "unknown";

            // GET Routes
            if (req.method === "GET") {
                if (url.includes("/redline/launcher/version")) { res.writeHead(200, { "Content-Type": "application/json" }); res.end(JSON.stringify({ version: launcherVersion })); return; }
                if (url.includes("/redline/launcher/download")) {
                    const launcherFile = path.join(__dirname, "..", "launcher", "Tarkov Red Line.exe");
                    if (!fs.existsSync(launcherFile)) { res.writeHead(404); res.end(JSON.stringify({ error: "Launcher não encontrado" })); return; }
                    const stats = fs.statSync(launcherFile);
                    res.writeHead(200, { "Content-Type": "application/octet-stream", "Content-Length": stats.size.toString(), "Content-Disposition": `attachment; filename="Tarkov Red Line.exe"` });
                    fs.createReadStream(launcherFile).pipe(res);
                    return;
                }
                if (url.includes("/launcher/hwid/version")) { res.writeHead(200, { "Content-Type": "application/json" }); res.end(JSON.stringify({ version: serverVersion })); return; }
                if (url.includes("/launcher/mods/optionals-list")) { updater.handleOpcionaisList(opcionaisPath, res); return; }
                if (url.includes("/launcher/mods/optionals-manifest")) { updater.handleOpcionaisManifest(url, opcionaisPath, res, logger); return; }
                if (url.includes("/launcher/mods/optional-download")) { updater.handleOpcionaisDownload(url, opcionaisPath, res, logger); return; }
                if (url === "/launcher/mods/version") { res.writeHead(200, { "Content-Type": "application/json" }); res.end(JSON.stringify({ version: serverVersion })); return; }
                if (url.includes("/launcher/mods/manifest-hash")) { updater.handleManifestHash(res); return; }
                if (url.includes("/launcher/mods/manifest")) { updater.handleManifest(res); return; }
                if (url.includes("/launcher/mods/download")) { updater.handleDownload(url, modsRepoPath, res, logger); return; }
                if (url.includes("/launcher/mods/refresh")) {
                    updater.invalidateCache();
                    updater.generateManifest(modsRepoPath, managedPaths, deleteFiles, ignoredFiles, serverVersion, logger, optionalGroups, opcionaisPath);
                    res.writeHead(200, { "Content-Type": "application/json" });
                    res.end(JSON.stringify({ status: "OK", message: "Regenerando manifesto em background" }));
                    return;
                }
                if (url.includes("/redline/player-ips")) { playerIps.handleList(res); return; }

                // --- VOTE GET STATUS ---
                if (url.includes("/redline/vote/status")) {
                    const now = Date.now();
                    const timeLeft = self.state.inProgress ? Math.max(0, Math.floor((self.state.endTime - now) / 1000)) : 0;
                    const cooldownLeft = Math.max(0, Math.floor((self.state.cooldownUntil - now) / 1000));
                    res.writeHead(200, { "Content-Type": "application/json" });
                    res.end(JSON.stringify({ 
                        inProgress: self.state.inProgress, 
                        isVetoed: self.state.isVetoed, 
                        yesVotes: self.state.yesVotes, 
                        timeLeft, 
                        cooldownLeft,
                        triggerRestart: self.state.triggerRestart
                    }));
                    return;
                }

                // --- DEBUG: Estado completo do servidor (para diagnóstico) ---
                if (url.includes("/redline/debug/state")) {
                    const now = Date.now();
                    res.writeHead(200, { "Content-Type": "application/json" });
                    res.end(JSON.stringify({
                        ...self.state,
                        votedIPsCount: Object.keys(self.state.votedIPs).length,
                        serverTime: new Date().toISOString(),
                        triggerRestartExpiresInMs: self.state.triggerRestart ? Math.max(0, (self.state.endTime + 60000) - now) : 0
                    }, null, 2));
                    return;
                }

            }

            // POST Routes
            let body = "";
            req.on("data", (chunk: any) => { body += chunk.toString(); });
            req.on("end", () => {
                try {
                    const now = Date.now();
                    if (req.method === "POST") {
                        if (url.includes("/api/vpn/register")) {
                            try {
                                const data = JSON.parse(body);
                                const result = self.handleVpnRegisterRequest(data);
                                res.writeHead(200, { "Content-Type": "application/json" });
                                res.end(JSON.stringify(result));
                            } catch (err: any) {
                                self.logger.error(`[RedLineVPN] Error: ${err.message}`);
                                res.writeHead(400, { "Content-Type": "application/json" });
                                res.end(JSON.stringify({ error: err.message }));
                            }
                            return;
                        }

                        if (url.includes("/launcher/hwid/register")) { hwid.handleRegister(body, res, logger); return; }
                        if (url.includes("/launcher/hwid/reset-password")) { hwid.handleResetPassword(body, res, logger); return; }
                        if (url.includes("/redline/register-player-ip")) { playerIps.handleRegisterIp(body, res, logger); return; }

                        // --- HEADLESS ACK RESTART ---
                        // O Headless envia este POST confirmando que recebeu o triggerRestart e vai encerrar.
                        if (url.includes("/redline/headless/ack-restart")) {
                            self.state.triggerRestart = false;
                            logger.success("[REDLINE] ACK de restart recebido do Headless! Flag triggerRestart zerada com precisão.");
                            res.writeHead(200, { "Content-Type": "application/json" });
                            res.end(JSON.stringify({ status: "ok" }));
                            return;
                        }

                        // --- VOTE POST CAST ---
                        if (url.includes("/redline/vote/cast")) {
                            const data = JSON.parse(body);
                            const voteValue = data.vote === true;
                            const voterId = data.clientId || clientIP;

                            if (!self.state.inProgress) {
                                if (now < self.state.cooldownUntil) { res.writeHead(403); res.end(); return; }
                                self.state.inProgress = true;
                                self.state.isVetoed = false;
                                self.state.yesVotes = 0;
                                self.state.noVotes = 0;
                                self.state.endTime = now + self.VOTE_TIME;
                                self.state.originalEndTime = self.state.endTime;
                                self.state.votedIPs = {};
                                logger.warning("[REDLINE] Nova votação iniciada.");
                            }

                            if (self.state.votedIPs[voterId] !== undefined) {
                                res.writeHead(200); res.end(JSON.stringify({ status: "already_voted" })); return;
                            }

                            self.state.votedIPs[voterId] = voteValue;
                            if (voteValue === false) {
                                self.state.noVotes++;
                                if (!self.state.isVetoed) {
                                    self.state.isVetoed = true;
                                    self.state.endTime = Date.now() + self.VETO_TIME;
                                    logger.error("[REDLINE] VETO ACIONADO! Tempo reduzido.");
                                }
                            } else {
                                self.state.yesVotes++;
                            }

                            res.writeHead(200); res.end(JSON.stringify({ status: "ok" }));
                            return;
                        }

                        // --- VOTE CANCEL (Veto Reversível) ---
                        if (url.includes("/redline/vote/cancel")) {
                            const data = JSON.parse(body || "{}");
                            const voterId = data.clientId || clientIP;

                            if (self.state.inProgress && self.state.votedIPs[voterId] !== undefined) {
                                const previousVote = self.state.votedIPs[voterId];
                                if (previousVote === true) {
                                    if (self.state.yesVotes > 0) self.state.yesVotes--;
                                } else {
                                    if (self.state.noVotes > 0) self.state.noVotes--;
                                    if (self.state.noVotes === 0 && self.state.isVetoed) {
                                        self.state.isVetoed = false;
                                        if (self.state.originalEndTime > Date.now()) {
                                            self.state.endTime = self.state.originalEndTime;
                                        }
                                        logger.success("[REDLINE] Veto removido! Tempo restaurado.");
                                    }
                                }
                                delete self.state.votedIPs[voterId];
                                logger.info(`[REDLINE] Voto removido por ${voterId}.`);
                            }
                            res.writeHead(200); res.end(JSON.stringify({ status: "cancelled", currentVotes: self.state.yesVotes }));
                            return;
                        }

                        // --- VOTE TERMINATE ---
                        if (url.includes("/redline/vote/terminate")) {
                            if (self.state.inProgress) {
                                self.state.inProgress = false;
                                self.state.endTime = 0;
                                self.state.yesVotes = 0;
                                self.state.noVotes = 0;
                                self.state.votedIPs = {};
                                logger.warning(`[REDLINE] Encerrado manualmente pelo Host.`);
                            }
                            res.writeHead(200); res.end(JSON.stringify({ status: "terminated" }));
                            return;
                        }
                    }
                    res.writeHead(404); res.end(JSON.stringify({ error: "Not found" }));
                } catch (e: any) {
                    logger.error(`[RedLine] Erro: ${e.message}`);
                    res.writeHead(500); res.end(JSON.stringify({ error: e.message }));
                }
            });
        });

        server.listen(7075, "0.0.0.0", () => {
            logger.success(`[RedLine] HTTP Server Unificado escutando na porta 7075 (Vote, HWID, Update)`);
        });

        // targram.startSyncLoop(logger);
        setInterval(() => this.checkVoteLogic(logger), 1000);

        logger.success(`[RedLine] TarkovRedLine Unificado v${serverVersion} TS iniciado`);
        logger.success(`[RedLine] Sistemas: HWID | PlayerIPs | ModUpdater | Vote Restart | VPN WG`);
    }

    private checkVoteLogic(logger: ILogger) {
        const now = Date.now();

        // Fallback de segurança: limpa a flag 60s após ser ativada caso o ACK do Headless nunca chegue
        if (this.state.triggerRestart && now > this.state.endTime + 60000) {
            this.state.triggerRestart = false;
            logger.warning("[REDLINE] triggerRestart zerado por fallback (60s sem ACK do Headless).");
        }

        if (!this.state.inProgress) return;
        if (now >= this.state.endTime) {
            this.state.inProgress = false;
            this.state.cooldownUntil = now + this.COOLDOWN_TIME;
            this.state.votedIPs = {};
            if (this.state.isVetoed) {
                logger.info("[REDLINE] Encerrado por VETO.");
            } else if (this.state.yesVotes === 0) {
                logger.info("[REDLINE] Encerrado: 0 votos.");
            } else {
                logger.error("[REDLINE] APROVADO! Enviando comando de SHUTDOWN para o plugin Headless...");
                this.state.triggerRestart = true;
            }
        }
    }

    // === LÓGICA VPN ===
    private handleVpnRegisterRequest(info: any): any {
        const reqPubkey = info.publicKey;
        const reqUsername = info.username || "UnknownPlayer";

        if (!reqPubkey) { throw new Error("PublicKey is required"); }

        let peersDb: Record<string, any> = {};
        if (fs.existsSync(this.peersDbPath)) {
            peersDb = JSON.parse(fs.readFileSync(this.peersDbPath, "utf8"));
        }

        let assignedIp = "";
        // Busca peer existente pela chave pública
        let existingByKey = Object.keys(peersDb).find(k => peersDb[k].publicKey === reqPubkey);
        // Busca peer existente pelo username (chave pode ter mudado)
        let existingByUsername = Object.keys(peersDb).find(k => peersDb[k].username === reqUsername);

        if (existingByKey) {
            // Chave já conhecida, mesmo IP
            assignedIp = existingByKey;
            this.logger.info(`[RedLineVPN] Peer re-registrado (chave conhecida): ${reqUsername} -> ${assignedIp}`);
        } else if (existingByUsername) {
            // Mesmo jogador, chave nova (gerou novas chaves) — atualiza sem mudar IP
            assignedIp = existingByUsername;
            peersDb[assignedIp].publicKey = reqPubkey;
            peersDb[assignedIp].updatedAt = new Date().toISOString();
            fs.writeFileSync(this.peersDbPath, JSON.stringify(peersDb, null, 4));
            this.logger.info(`[RedLineVPN] Chave atualizada para ${reqUsername} (${assignedIp})`);
        } else {
            // Novo jogador — atribui próximo IP disponível
            const usedIps = Object.keys(peersDb);
            for (let i = 2; i <= 254; i++) {
                let candidate = `${this.vpnConfig.networkPrefix}${i}`;
                if (!usedIps.includes(candidate)) {
                    assignedIp = candidate;
                    break;
                }
            }
            if (!assignedIp) { throw new Error("No available IPs in the VPN pool."); }
            peersDb[assignedIp] = { publicKey: reqPubkey, username: reqUsername, registeredAt: new Date().toISOString() };
            fs.writeFileSync(this.peersDbPath, JSON.stringify(peersDb, null, 4));
            this.logger.info(`[RedLineVPN] Novo peer registrado: ${reqUsername} -> ${assignedIp}`);
        }

        // Sempre sincronizar o arquivo .conf com o banco de peers
        this.updateWireGuardConfig(peersDb);

        return {
            assignedIp: assignedIp,
            serverPublicKey: this.vpnConfig.serverPublicKey,
            serverEndpoint: "" // O client já deduz o Endpoint da URL do Pastebin!
        };
    }

    private updateWireGuardConfig(peersDb: Record<string, any>) {
        try {
            if (!fs.existsSync(this.vpnConfig.wireGuardConfPath)) {
                this.logger.error(`[RedLineVPN] WG Config file not found at: ${this.vpnConfig.wireGuardConfPath}`);
                return;
            }

            let confContent = fs.readFileSync(this.vpnConfig.wireGuardConfPath, "utf8");
            let lines = confContent.split("\n");
            let newLines = [];
            let interfaceSectionEnded = false;

            for (let line of lines) {
                if (line.trim().startsWith("[Peer]")) { interfaceSectionEnded = true; break; }
                newLines.push(line);
            }

            for (const [ip, data] of Object.entries(peersDb)) {
                newLines.push(`\n[Peer] # ${data.username}`);
                newLines.push(`PublicKey = ${data.publicKey}`);
                newLines.push(`AllowedIPs = ${ip}/32`);
            }

            const finalContent = newLines.join("\n");
            fs.writeFileSync(this.vpnConfig.wireGuardConfPath, finalContent);
            this.logger.info(`[RedLineVPN] WireGuard config updated. Restarting tunnel...`);
            
            // Windows: usar wg.exe para aplicar config em tempo real (sem derrubar o túnel)
            const wgExe = `"C:\\Program Files\\WireGuard\\wg.exe"`;
            const confPath = this.vpnConfig.wireGuardConfPath;
            const ifaceName = this.vpnConfig.wireGuardInterfaceName;

            // Gera um arquivo temporário "stripped" (sem Address/DNS/etc) para o wg syncconf
            const strippedLines: string[] = [];
            const confRaw = fs.readFileSync(confPath, "utf8").split("\n");
            for (const line of confRaw) {
                const trimmed = line.trim().toLowerCase();
                if (trimmed.startsWith("address") || trimmed.startsWith("dns") || trimmed.startsWith("mtu") || trimmed.startsWith("table") || trimmed.startsWith("preup") || trimmed.startsWith("postup") || trimmed.startsWith("predown") || trimmed.startsWith("postdown") || trimmed.startsWith("saveconfig")) {
                    continue;
                }
                strippedLines.push(line);
            }
            const strippedPath = confPath.replace(".conf", "_stripped.conf");
            fs.writeFileSync(strippedPath, strippedLines.join("\n"));

            child_process.exec(`${wgExe} syncconf ${ifaceName} "${strippedPath}"`, (error: any, stdout: any, stderr: any) => {
                if (error) {
                    this.logger.warning(`[RedLineVPN] wg syncconf failed (${stderr}). Restarting tunnel service...`);
                    child_process.exec(`net stop "WireGuardTunnel$${ifaceName}" & net start "WireGuardTunnel$${ifaceName}"`, { shell: "cmd.exe" });
                } else {
                    this.logger.info(`[RedLineVPN] wg syncconf applied successfully.`);
                }
                // Limpa arquivo temporário
                try { fs.unlinkSync(strippedPath); } catch(_) {}
            });

        } catch (e: any) {
            this.logger.error(`[RedLineVPN] Failed to update WireGuard config: ${e.message}`);
        }
    }
}

module.exports = { mod: new TarkovRedLineMod() };
