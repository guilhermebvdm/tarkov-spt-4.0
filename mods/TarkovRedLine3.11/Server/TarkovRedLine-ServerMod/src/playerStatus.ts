/**
 * playerStatus.ts — Rastreamento de Status In-Game dos jogadores
 */

import { DependencyContainer } from "tsyringe";
import { ILogger } from "@spt/models/spt/utils/ILogger";
import { supabaseRequest } from "./targramSync";

const IN_MENU  = "No Menu / Stash";
const IN_HIDEOUT = "No Hideout";
const OFFLINE  = "Offline";

const MAP_NAMES: Record<string, string> = {
    "bigmap":          "Customs",
    "factory4_day":    "Fábrica",
    "factory4_night":  "Fábrica (Noite)",
    "woods":           "Woods",
    "shoreline":       "Shoreline",
    "interchange":     "Interchange",
    "rezervbase":      "Reserva Militar",
    "laboratory":      "Labs",
    "lighthouse":      "Lighthouse",
    "tarkovstreets":   "Streets of Tarkov",
    "sandbox":         "Ground Zero",
    "sandbox_high":    "Ground Zero"
};

class PlayerStatusTracker {
    private statusMap: Record<string, string> = {};
    private lastSeen: Record<string, number> = {};
    private lastActionTime: Record<string, number> = {};
    private logger: ILogger | null = null;

    public init(container: DependencyContainer, logger: ILogger): void {
        this.logger = logger;

        try {
            // Limpa fantasmas da última sessão ao reiniciar o servidor
            this.clearGhostsOnStartup();

            const httpListener = container.resolve<any>("HttpListener");
            if (!httpListener) throw new Error("HttpListener não encontrado.");

            const originalGetResponse = httpListener.getResponse.bind(httpListener);

            // Substituímos o HttpListener principal para capturar todas as requisições
            httpListener.getResponse = (sessionId: string, req: any, body: string) => {
                if (sessionId && req && req.url) {
                    this.lastSeen[sessionId] = Date.now();
                    const url: string = req.url;

                    if (url.includes("/client/game/profile/items/moving")) {
                        this.updateStatus(sessionId, "No inventário");
                    } else if (url.includes("/client/hideout/")) {
                        this.updateStatus(sessionId, "No esconderijo");
                    } else if (url.includes("/client/ragfair/")) {
                        this.updateStatus(sessionId, "Mercado das Pulgas");
                    } else if (url.includes("/client/trading/")) {
                        this.updateStatus(sessionId, "Comerciantes");
                    } else if (url.includes("/client/match/local/start")) {
                        let mapId = "Desconhecido";
                        try {
                            const data = JSON.parse(body);
                            mapId = data.location || mapId;
                        } catch (e) { }

                        const displayMap = MAP_NAMES[mapId?.toLowerCase()] || mapId;
                        this.updateStatus(sessionId, `Em Raid (${displayMap})`);
                    } else if (url.includes("/client/match/local/end") || url.includes("/client/game/version/validate") || url.includes("/client/game/start")) {
                        this.updateStatus(sessionId, "No menu");
                        this.lastActionTime[sessionId] = Date.now();
                    } else if (url.includes("/client/game/logout")) {
                        this.updateStatus(sessionId, OFFLINE);
                    } else if (url.includes("/client/game/keepalive")) {
                        const current = this.statusMap[sessionId] || "";
                        const timeSinceAction = Date.now() - (this.lastActionTime?.[sessionId] || 0);

                        // Volta para o menu do limbo "Offline" ou se já deu um tempo grande (25s)
                        // desde a última ação (esconderijo, mercado) e não está numa Raid.
                        if (current === OFFLINE || (!current.includes("Em Raid") && timeSinceAction > 25000)) {
                            this.updateStatus(sessionId, "No menu");
                        }
                    } else {
                        // Outras requisições arbitrárias de interface não catalogadas também atualizam o timestamp de "Ação"
                        if (!url.includes("keepalive")) {
                            this.lastActionTime[sessionId] = Date.now();
                        }
                    }
                }

                // Repassa para o SPT/Fika normal
                return originalGetResponse(sessionId, req, body);
            };

            this.logger.success("[RedLine] Tracker de Status In-Game injetado com sucesso no HttpListener (Universal).");

            // Loop de faxina de fantasmas a cada 30 segundos
            setInterval(() => {
                const now = Date.now();
                for (const sessId in this.lastSeen) {
                    if (now - this.lastSeen[sessId] > 180000) { // 3 minutos sem ping
                        if (this.statusMap[sessId] && this.statusMap[sessId] !== OFFLINE) {
                            this.updateStatus(sessId, OFFLINE);
                            this.logger.info(`[Targram] Jogador sofreu timeout ou fechou o jogo bruscamente (ID: ${sessId}). Removido para Offline.`);
                        }
                        delete this.lastSeen[sessId];
                    }
                }
            }, 30000);

        } catch (e) {
            this.logger.error(`[RedLine] Erro ao injetar tracker de status: ${e.message}`);
        }
    }

    private async clearGhostsOnStartup(): Promise<void> {
        try {
            await supabaseRequest("PATCH", `users?ingame_status=neq.Offline`, { ingame_status: OFFLINE });
            if (this.logger) this.logger.success("[Targram] Limpeza de fantasmas (Wipe Status) concluída na inicialização.");
        } catch (e) { /* ignore */ }
    }

    private async updateStatus(sessionId: string, rawStatus: string): Promise<void> {
        if (!sessionId || sessionId.length < 5) return; // ignora sessionId vazios ou internos

        // Evita flood se o status for igual
        if (this.statusMap[sessionId] === rawStatus) return;
        this.statusMap[sessionId] = rawStatus;

        try {
            await supabaseRequest("PATCH", `users?spt_account_id=eq.${sessionId}`, {
                ingame_status: rawStatus
            });
        } catch (e) { /* silent */ }
    }
}

export default new PlayerStatusTracker();
