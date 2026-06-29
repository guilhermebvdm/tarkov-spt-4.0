import { DependencyContainer } from "tsyringe";
import { buildWaves } from "../Spawning/Spawning";
import { StaticRouterModService } from "@spt/services/mod/staticRouter/StaticRouterModService";
import { globalValues } from "../GlobalValues";
import { kebabToTitle } from "../utils";
import PresetWeightingsConfig from "../../config/PresetWeightings.json";
import { Ixyz } from "@spt/models/eft/common/Ixyz";
import {
  deleteBotSpawn,
  updateBotSpawn,
} from "../SpawnZoneChanges/updateUtils";

export const setupRoutes = (container: DependencyContainer) => {
  const staticRouterModService = container.resolve<StaticRouterModService>(
    "StaticRouterModService"
  );

  interface AddSpawnRequest {
    map: string;
    position: Ixyz;
    type: "player" | "pmc" | "scav" | "sniper";
  }

  staticRouterModService.registerStaticRouter(
    `moarAddBotSpawn`,
    [
      {
        url: "/moar/addBotSpawn",
        action: async (
          url: string,
          req: AddSpawnRequest,
          sessionID,
          output
        ) => {
          updateBotSpawn(req.map, req.position, req.type);
          return "success";
        },
      },
    ],
    "moarAddBotSpawn"
  );

  staticRouterModService.registerStaticRouter(
    `moarDeleteBotSpawn`,
    [
      {
        url: "/moar/deleteBotSpawn",
        action: async (
          url: string,
          req: AddSpawnRequest,
          sessionID,
          output
        ) => {
          // console.log(req);
          deleteBotSpawn(req.map, req.position, req.type);
          return "success";
        },
      },
    ],
    "moarDeleteBotSpawn"
  );

  // Make buildwaves run on game end
  staticRouterModService.registerStaticRouter(
    `moarUpdater`,
    [
      {
        url: "/client/match/local/end",
        action: async (_url, info, sessionId, output) => {
          buildWaves(container);
          return output;
        },
      },
    ],
    "moarUpdater"
  );

  staticRouterModService.registerStaticRouter(
    `moarGetCurrentPreset`,
    [
      {
        url: "/moar/currentPreset",
        action: async () => {
          return globalValues.forcedPreset || "random";
        },
      },
    ],
    "moarGetCurrentPreset"
  );

  staticRouterModService.registerStaticRouter(
    `moarGetAnnouncePreset`,
    [
      {
        url: "/moar/announcePreset",
        action: async () => {
          if (globalValues.forcedPreset?.toLowerCase() === "random") {
            return globalValues.currentPreset;
          }
          return globalValues.forcedPreset || globalValues.currentPreset;
        },
      },
    ],
    "moarGetAnnouncePreset"
  );

  staticRouterModService.registerStaticRouter(
    `getDefaultConfig`,
    [
      {
        url: "/moar/getDefaultConfig",
        action: async () => {
          return JSON.stringify(globalValues.baseConfig);
        },
      },
    ],
    "getDefaultConfig"
  );

  staticRouterModService.registerStaticRouter(
    `getServerConfigWithOverrides`,
    [
      {
        url: "/moar/getServerConfigWithOverrides",
        action: async () => {
          return JSON.stringify({
            ...(globalValues.baseConfig || {}),
            ...(globalValues.overrideConfig || {}),
          });
        },
      },
    ],
    "getServerConfigWithOverrides"
  );

  staticRouterModService.registerStaticRouter(
    `getServerConfigWithOverrides`,
    [
      {
        url: "/moar/getServerConfigWithOverrides",
        action: async () => {
          return JSON.stringify({
            ...globalValues.baseConfig,
            ...globalValues.overrideConfig,
          });
        },
      },
    ],
    "getServerConfigWithOverrides"
  );

  staticRouterModService.registerStaticRouter(
    `moarGetPresetsList`,
    [
      {
        url: "/moar/getPresets",
        action: async () => {
          let result = [
            ...Object.keys(PresetWeightingsConfig).map((preset) => ({
              Name: kebabToTitle(preset),
              Label: preset,
            })),
            { Name: "Random", Label: "random" },
            { Name: "Custom", Label: "custom" },
          ];

          return JSON.stringify({ data: result });
        },
      },
    ],
    "moarGetPresetsList"
  );

  staticRouterModService.registerStaticRouter(
    "setOverrideConfig",
    [
      {
        url: "/moar/setOverrideConfig",
        action: async (
          url: string,
          overrideConfig: typeof globalValues.overrideConfig = {},
          sessionID,
          output
        ) => {
          globalValues.overrideConfig = overrideConfig;

          buildWaves(container);

          return "Success";
        },
      },
    ],
    "setOverrideConfig"
  );

  staticRouterModService.registerStaticRouter(
    "moarSetPreset",
    [
      {
        url: "/moar/setPreset",
        action: async (url: string, { Preset }, sessionID, output) => {
          globalValues.forcedPreset = Preset;
          buildWaves(container);

          return `Current Preset: ${kebabToTitle(
            globalValues.forcedPreset || "Random"
          )}`;
        },
      },
    ],
    "moarSetPreset"
  );
};
