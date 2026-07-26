---
title: Códigos Legados (TypeScript — SPT 3.x)
date: 2026-07-26
status: ⚫ Arquivado
authors: Guilherme + agente
---

# Códigos Legados (TypeScript - SPT 3.x)

> ⚫ **Arquivado em 2026-07-26.** Código TypeScript da era SPT 3.x (servidor Node.js), arquitetura descontinuada no SPT 4.0 — ver [spt3-to-spt4-mod-migration.md](../spt3-to-spt4-mod-migration.md). Mantido apenas como referência histórica das lógicas do `TarkovRedLine-ServerMod`. **Não seguir como modelo.**
>
> 🔐 **Nota de segurança.** Este arquivo continha, em claro, a `SUPABASE_URL` e uma chave **`service_role`** do Supabase (que ignora Row Level Security). Os valores foram substituídos por `<REDACTED>` nesta data. O repositório é **público** e a chave esteve exposta em `origin/main` desde 2026-06-07 (`cbf3fc87`) — **a chave foi/deve ser rotacionada no painel do Supabase**; a redação aqui não desfaz a exposição no histórico do git.

Este arquivo foi gerado para salvar as lógicas extraídas do mod `TarkovRedLine-ServerMod` que não farão parte da migração imediata para o SPT 4.0, para que possam ser reaproveitadas em mods futuros.

## 1. DatabaseModifier (Fix Flash-Reload da IA)

Essa lógica era aplicada no método `postDBLoad` para zerar o tempo de recarga dos bots (fazendo eles recarregarem instantaneamente ou algo similar). 

```typescript
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
```

## 2. Targram Sync (`targramSync.ts`)

Essa lógica era responsável por sincronizar em background o progresso (Hideout, XP, Conquistas, etc) dos jogadores do SPT para um banco de dados no Supabase. O arquivo completo está abaixo:

```typescript
"use strict";
/**
 * targramSync.js — Sincronização Desacoplada com o Supabase (Targram)
 * Não usa NENHUM hook ou classe interna do SPT. Lida apenas com arquivos físicos.
 */

import * as fs from "fs";
import * as path from "path";
import * as https from "https";
import CUSTOM_POSTS from "./targramPostStrings";

const SUPABASE_URL = "<REDACTED>";
const SUPABASE_KEY = "<REDACTED>"; // era uma chave de service_role — ver nota de segurança no topo

const HIDEOUT_AREA_NAMES = {
    0: "Ventilação", 1: "Segurança", 2: "Banheiro", 3: "Armazém (estoque)",
    4: "Gerador", 5: "Aquecedor", 6: "Coletor de água", 7: "Enfermaria",
    8: "Cozinha", 9: "Dormitório", 10: "Bancada de trabalho",
    11: "Centro de Inteligência", 12: "Campo de tiro", 13: "Biblioteca", 14: "Caixa Scav",
    15: "Iluminação", 16: "Hall da fama", 17: "Filtro de ar",
    18: "Energia solar", 19: "Alambique", 20: "Mineração de Bitcoin",
    21: "Árvore de natal", 22: "Parede danificada", 23: "Academia",
    24: "Estante de arma", 25: "Estante de Armas Secundária", 
    26: "Manequim", 27: "Círculo cultista"
};

// Cache em memória para saber quando os arquivos foram enviados pela última vez
const lastModifiedMap = {};

let poDict = {};
try {
    const poPath = path.join(process.cwd(), "SPT_Data", "Server", "database", "locales", "global", "po.json");
    if (fs.existsSync(poPath)) {
        poDict = JSON.parse(fs.readFileSync(poPath, "utf8"));
    }
} catch (err) {
    console.error("[Targram] Aviso: Não foi possível carregar po.json para nomes de conquistas automáticos.");
}

function supabaseRequest(method, endpoint, payload) {
    return new Promise((resolve, reject) => {
        const body = payload ? JSON.stringify(payload) : null;
        const url  = new URL(`${SUPABASE_URL}/rest/v1/${endpoint}`);
        const options = {
            hostname: url.hostname,
            path: url.pathname + url.search,
            method,
            headers: {
                "apikey":         SUPABASE_KEY,
                "Authorization":  `Bearer ${SUPABASE_KEY}`,
                "Content-Type":   "application/json",
                "Prefer":         "resolution=merge-duplicates,return=representation",
            }
        };
        if (body) options.headers["Content-Length"] = Buffer.byteLength(body);
        const req = https.request(options, res => {
            let data = "";
            res.on("data", chunk => data += chunk);
            res.on("end", () => resolve({ status: res.statusCode, body: data }));
        });
        req.on("error", reject);
        if (body) req.write(body);
        req.end();
    });
}

async function syncProfile(sessionID, profileData, logger) {
    try {
        if (!profileData.characters?.pmc?.Info?.Nickname) return false;

        const pmc          = profileData.characters.pmc;
        const sptAccountId = profileData.info.id;
        const displayName  = pmc.Info.Nickname;
        const faction      = pmc.Info.Side;
        const level        = pmc.Info.Level;
        const experience   = pmc.Info.Experience;

        // --- SISTEMA DE MIGRAÇÃO AUTOMÁTICA DE CONTAS (WIPE / NICK RECICLADO) ---
        // O servidor SPT é a fonte de verdade — se um perfil existe aqui, ele é o ativo.
        // Quando encontramos outro spt_account_id com o mesmo nick, migramos automaticamente
        // e limpamos os dados órfãos da conta antiga.
        try {
            const existingRes = await supabaseRequest("GET", `users?display_name=eq.${encodeURIComponent(displayName)}&select=id,spt_account_id,experience,display_name`);
            const parsed = JSON.parse(existingRes.body);
            if (parsed && parsed.length > 0) {
                for (const existingAccount of parsed) {
                   if (existingAccount.spt_account_id !== sptAccountId) {
                       const oldExp = existingAccount.experience || 0;
                       const oldSptId = existingAccount.spt_account_id;
                       const oldDbId = existingAccount.id;

                       // Limpa dados vinculados à conta antiga antes de migrar
                       try {
                           await supabaseRequest("DELETE", `user_stats?user_id=eq.${oldDbId}`);
                           await supabaseRequest("DELETE", `user_achievements?user_id=eq.${oldDbId}`);
                           await supabaseRequest("DELETE", `skills_cache?user_id=eq.${oldDbId}`);
                           await supabaseRequest("DELETE", `hideout_cache?user_id=eq.${oldDbId}`);
                       } catch (cleanErr) { /* silent — tabelas podem não existir ou estar vazias */ }

                       // Migra o registro do usuário para o novo spt_account_id
                       await supabaseRequest("PATCH", `users?id=eq.${oldDbId}`, { spt_account_id: sptAccountId, experience });

                       if (logger && typeof logger.info === 'function') {
                           if (experience >= oldExp) {
                               logger.info(`[Targram] Nick reciclado detectado para '${displayName}'. Migrando (${oldSptId} -> ${sptAccountId}).`);
                           } else {
                               logger.info(`[Targram] Wipe detectado para '${displayName}'. Migrando e resetando dados (${oldSptId} -> ${sptAccountId}, XP: ${oldExp} -> ${experience}).`);
                           }
                       }
                   }
                }
            }
        } catch (err) {
            if (logger && typeof logger.error === 'function') logger.error(`[Targram] Erro na migração de conta: ${err.message}`);
        }
        // --------------------------------------------------

        const payload = {
            spt_account_id: sptAccountId,
            display_name:   displayName,
            password:       profileData.info.password || "",
            faction,
            level,
            experience,
        };
        const upsertRes = await supabaseRequest("POST", "users?on_conflict=spt_account_id", payload);

        let dbUserId = null;
        try {
            const parsed = JSON.parse(upsertRes.body);
            dbUserId = Array.isArray(parsed) ? parsed[0]?.id : parsed?.id;
        } catch (_) {}

        if (!dbUserId) {
            const r = await supabaseRequest("GET", `users?spt_account_id=eq.${sptAccountId}&select=id`);
            dbUserId = JSON.parse(r.body)?.[0]?.id;
        }

        if (!dbUserId) return false;

        const skillMap = {};
        for (const s of (pmc.Skills?.Common || [])) skillMap[s.Id] = s.Progress;

        await supabaseRequest("POST", "skills_cache?on_conflict=user_id", {
            user_id:              dbUserId,
            endurance_progress:   skillMap["Endurance"]  || 0,
            strength_progress:    skillMap["Strength"]   || 0,
            metabolism_progress:  skillMap["Metabolism"] || 0,
            vitality_progress:    skillMap["Vitality"]   || 0,
            health_progress:      skillMap["Health"]     || 0,
            updated_at:           new Date().toISOString(),
        });

        const achievementsObj = pmc.Achievements;
        if (achievementsObj && typeof achievementsObj === "object") {
            const currentAchReq = await supabaseRequest("GET", `user_achievements?user_id=eq.${dbUserId}&select=achievement_id`);
            let existingAchIds = [];
            try {
                const parsed = JSON.parse(currentAchReq.body);
                if (Array.isArray(parsed)) existingAchIds = parsed.map(r => r.achievement_id);
            } catch(e) {}

            await supabaseRequest("DELETE", `user_achievements?user_id=eq.${dbUserId}`);
            const rows = Object.keys(achievementsObj).map(achId => ({
                user_id:        dbUserId,
                achievement_id: achId,
                unlocked_at:    new Date(achievementsObj[achId] * 1000).toISOString(),
            }));
            if (rows.length > 0) await supabaseRequest("POST", "user_achievements", rows);

            // Gerar Posts Automáticos para Novas Conquistas
            const newAchIds = Object.keys(achievementsObj).filter(id => !existingAchIds.includes(id));
            if ((existingAchIds.length > 0 || newAchIds.length < 5) && newAchIds.length > 0) {
                const posts = newAchIds.map(achId => {
                    const dictName = poDict[`${achId} name`];
                    const achName = dictName || "Nova Conquista Oculta";
                    let contentBody = `🏆 Desbloqueei a conquista: **${achName}**!\n\n[ACHIEVEMENT:${achId}]`;
                    if (CUSTOM_POSTS.ACHIEVEMENTS && CUSTOM_POSTS.ACHIEVEMENTS[achId]) {
                        contentBody = CUSTOM_POSTS.ACHIEVEMENTS[achId];
                    }
                    return {
                        spt_account_id: sptAccountId,
                        content: contentBody,
                    };
                });
            }
        }

        const eft = pmc.Stats?.Eft || {};
        await supabaseRequest("POST", "user_stats?on_conflict=user_id", {
            user_id:        dbUserId,
            raids_total:    eft.Sessions               || 0,
            raids_survived: eft.ExitStatus?.Survived   || 0,
            kills_total:    eft.Kills                  || 0,
            headshots:      eft.Headshots              || 0,
            deaths:         eft.Deaths                 || 0,
            damage_dealt:   eft.TotalDamageDealt       || 0,
            damage_taken:   eft.TotalDamageTaken       || 0,
            time_in_game_s: eft.TotalInGameTime        || 0,
            items_found_ir: eft.FoundInRaidItems       || 0,
            updated_at:     new Date().toISOString(),
        });

        const hideoutAreas = pmc.Hideout?.Areas || [];
        if (hideoutAreas.length > 0) {
            const currentAreaReq = await supabaseRequest("GET", `hideout_cache?user_id=eq.${dbUserId}&select=area_id,level`);
            let existingAreas = {};
            try {
                const parsed = JSON.parse(currentAreaReq.body);
                if (Array.isArray(parsed)) {
                    parsed.forEach(r => existingAreas[r.area_id] = r.level);
                }
            } catch(e) {}

            await supabaseRequest("DELETE", `hideout_cache?user_id=eq.${dbUserId}`);
            const areaRows = hideoutAreas.map(area => ({
                user_id:   dbUserId,
                area_id:   area.type,
                area_name: HIDEOUT_AREA_NAMES[area.type] || `Area ${area.type}`,
                level:     area.level || 0,
                updated_at: new Date().toISOString(),
            }));
            if (areaRows.length > 0) await supabaseRequest("POST", "hideout_cache", areaRows);

            // Gerar Posts Automáticos para Hideout
            const upgradedAreas = [];
            hideoutAreas.forEach(area => {
                const oldLevel = existingAreas[area.type] !== undefined ? existingAreas[area.type] : -1;
                // Ignora upgrades no primeiro sync (oldLevel === -1)
                if (oldLevel !== -1 && area.level > oldLevel) {
                     upgradedAreas.push(area);
                }
            });

            if (upgradedAreas.length > 0) {
                const posts = upgradedAreas.map(area => {
                    const areaName = HIDEOUT_AREA_NAMES[area.type] || `Area ${area.type}`;
                    const customKey = `${area.type}_${area.level}`;
                    let contentBody = `🏗️ Evolui meu(minha) **${areaName}** para o Nível **${area.level}** no Hideout!\n\n[HIDEOUT:${area.type}:${area.level}]`;
                    if (CUSTOM_POSTS.HIDEOUT && CUSTOM_POSTS.HIDEOUT[customKey]) {
                        contentBody = CUSTOM_POSTS.HIDEOUT[customKey];
                    }
                    return {
                        spt_account_id: sptAccountId,
                        content: contentBody,
                    };
                });
            }
        }

        return true;
    } catch (err) {
        return false;
    }
}

async function checkProfilesFolder(logger) {
    try {
        const profilesPath = path.join(process.cwd(), "user", "profiles");
        if (!fs.existsSync(profilesPath)) return;

        const files = fs.readdirSync(profilesPath).filter(f => f.endsWith('.json'));
        let syncedThisRound = 0;

        for (const file of files) {
            const sessionID = file.replace('.json', '');
            const filePath = path.join(profilesPath, file);
            
            try {
                const stats = fs.statSync(filePath);
                
                // Se o arquivo é novo, ou foi modificado (mtime) recentemente (e não sincronizamos ainda essa versão)
                if (!lastModifiedMap[sessionID] || stats.mtimeMs > lastModifiedMap[sessionID]) {
                    const raw = fs.readFileSync(filePath, "utf8");
                    const profileData = JSON.parse(raw);
                    
                    // Só tenta sincronizar se a conta for válida e tiver Nickname da PMC
                    if (profileData && profileData.characters?.pmc?.Info?.Nickname) {
                        const success = await syncProfile(sessionID, profileData, logger);
                        if (success) {
                            lastModifiedMap[sessionID] = stats.mtimeMs;
                            syncedThisRound++;
                            logger.info(`[Targram] Perfil silenciosamente atualizado no Supabase: ${profileData.characters.pmc.Info.Nickname}`);
                        }
                    } else {
                        // Perfil em processo inicial de criação
                        lastModifiedMap[sessionID] = stats.mtimeMs;
                    }
                }
            } catch (e) {}
        }
        
        if (syncedThisRound > 0) {
            logger.success(`[Targram] Batch Sync concluído: ${syncedThisRound} perfil(is) enviado(s) ao Supabase.`);
        }
    } catch (e) {}
}

function startSyncLoop(logger) {
    logger.success("[Targram] Inicializando Sincronizador Fantasma (File System Watcher isolado)");
    
    setTimeout(() => {
        logger.info("[Targram] Executando varredura inicial de todos os perfis...");
        checkProfilesFolder(logger);
    }, 5000);

    setInterval(() => {
        supabaseRequest("PATCH", "users?spt_account_id=eq.admin_tarkov_red_line", {
            last_ping: new Date().toISOString(),
            ingame_status: 'Rodando',
            targram_online: true
        }).catch(() => {});
    }, 20000); // 20s Ping

    setInterval(() => {
        checkProfilesFolder(logger);
    }, 300000); // 300.000 ms = 5 minutos
}

export { startSyncLoop, supabaseRequest };
```

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-06 | Guilherme | chore(launcher): remove empty placeholder diff.txt |
| 2026-07-26 | Guilherme + agente | Arquivado. Código TS da era SPT 3.x, arquitetura descontinuada no 4.0 e sem inbound link no repo. Credenciais Supabase (`SUPABASE_URL` + chave `service_role`) redigidas para `<REDACTED>`; frontmatter adicionado (o arquivo falhava o `validate-doc-header.sh` e bloqueava qualquer commit que o tocasse). Substituído, como referência de migração, por [spt3-to-spt4-mod-migration.md](../spt3-to-spt4-mod-migration.md). |
