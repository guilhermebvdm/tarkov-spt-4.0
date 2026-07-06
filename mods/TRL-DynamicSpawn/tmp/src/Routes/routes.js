"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.setupRoutes = void 0;
const Spawning_1 = require("../Spawning/Spawning");
const GlobalValues_1 = require("../GlobalValues");
const utils_1 = require("../utils");
const updateUtils_1 = require("../SpawnZoneChanges/updateUtils");
const setupRoutes = (container) => {
    const staticRouterModService = container.resolve("StaticRouterModService");
    staticRouterModService.registerStaticRouter(`moarAddBotSpawn`, [
        {
            url: "/moar/addBotSpawn",
            action: async (url, req, sessionID, output) => {
                (0, updateUtils_1.updateBotSpawn)(req.map, req.position, req.type);
                return "success";
            },
        },
    ], "moarAddBotSpawn");
    staticRouterModService.registerStaticRouter(`moarDeleteBotSpawn`, [
        {
            url: "/moar/deleteBotSpawn",
            action: async (url, req, sessionID, output) => {
                // console.log(req);
                (0, updateUtils_1.deleteBotSpawn)(req.map, req.position, req.type);
                return "success";
            },
        },
    ], "moarDeleteBotSpawn");
    // Make buildwaves run on game end
    staticRouterModService.registerStaticRouter(`moarUpdater`, [
        {
            url: "/client/match/local/end",
            action: async (_url, info, sessionId, output) => {
                (0, Spawning_1.buildWaves)(container);
                return output;
            },
        },
    ], "moarUpdater");
    staticRouterModService.registerStaticRouter(`moarGetCurrentPreset`, [
        {
            url: "/moar/currentPreset",
            action: async () => {
                return GlobalValues_1.globalValues.forcedPreset || "random";
            },
        },
    ], "moarGetCurrentPreset");
    staticRouterModService.registerStaticRouter(`moarGetAnnouncePreset`, [
        {
            url: "/moar/announcePreset",
            action: async () => {
                if (GlobalValues_1.globalValues.forcedPreset?.toLowerCase() === "random") {
                    return GlobalValues_1.globalValues.currentPreset;
                }
                return GlobalValues_1.globalValues.forcedPreset || GlobalValues_1.globalValues.currentPreset;
            },
        },
    ], "moarGetAnnouncePreset");
    staticRouterModService.registerStaticRouter(`getDefaultConfig`, [
        {
            url: "/moar/getDefaultConfig",
            action: async () => {
                return JSON.stringify(GlobalValues_1.globalValues.baseConfig);
            },
        },
    ], "getDefaultConfig");
    staticRouterModService.registerStaticRouter(`getServerConfigWithOverrides`, [
        {
            url: "/moar/getServerConfigWithOverrides",
            action: async () => {
                return JSON.stringify({
                    ...(GlobalValues_1.globalValues.baseConfig || {}),
                    ...(GlobalValues_1.globalValues.overrideConfig || {}),
                });
            },
        },
    ], "getServerConfigWithOverrides");
    staticRouterModService.registerStaticRouter(`getServerConfigWithOverrides`, [
        {
            url: "/moar/getServerConfigWithOverrides",
            action: async () => {
                return JSON.stringify({
                    ...GlobalValues_1.globalValues.baseConfig,
                    ...GlobalValues_1.globalValues.overrideConfig,
                });
            },
        },
    ], "getServerConfigWithOverrides");
    staticRouterModService.registerStaticRouter(`moarGetPresetsList`, [
        {
            url: "/moar/getPresets",
            action: async () => {
                let result = [
                    ...Object.keys(PresetWeightingsConfig).map((preset) => ({
                        Name: (0, utils_1.kebabToTitle)(preset),
                        Label: preset,
                    })),
                    { Name: "Random", Label: "random" },
                    { Name: "Custom", Label: "custom" },
                ];
                return JSON.stringify({ data: result });
            },
        },
    ], "moarGetPresetsList");
    staticRouterModService.registerStaticRouter("setOverrideConfig", [
        {
            url: "/moar/setOverrideConfig",
            action: async (url, overrideConfig = {}, sessionID, output) => {
                GlobalValues_1.globalValues.overrideConfig = overrideConfig;
                (0, Spawning_1.buildWaves)(container);
                return "Success";
            },
        },
    ], "setOverrideConfig");
    staticRouterModService.registerStaticRouter("moarSetPreset", [
        {
            url: "/moar/setPreset",
            action: async (url, { Preset }, sessionID, output) => {
                GlobalValues_1.globalValues.forcedPreset = Preset;
                (0, Spawning_1.buildWaves)(container);
                return `Current Preset: ${(0, utils_1.kebabToTitle)(GlobalValues_1.globalValues.forcedPreset || "Random")}`;
            },
        },
    ], "moarSetPreset");
};
exports.setupRoutes = setupRoutes;
