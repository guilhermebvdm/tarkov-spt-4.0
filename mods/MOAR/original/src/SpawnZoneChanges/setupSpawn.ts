import { DatabaseServer } from "@spt/servers/DatabaseServer";
import { configLocations, originalMapList } from "../Spawning/constants";
import { DependencyContainer } from "tsyringe";
import mapConfig from "../../config/mapConfig.json";
import advancedConfig from "../../config/advancedConfig.json";
import { ISpawnPointParam } from "@spt/models/eft/common/ILocationBase";
import { globalValues } from "../GlobalValues";
import {
  AddCustomBotSpawnPoints,
  BuildCustomPlayerSpawnPoints,
  AddCustomPmcSpawnPoints,
  AddCustomSniperSpawnPoints,
  cleanClosest,
  getClosestZone,
  removeClosestSpawnsFromCustomBots,
} from "../Spawning/spawnZoneUtils";
import { shuffle } from "../Spawning/utils";
import { PlayerSpawns, PmcSpawns, ScavSpawns, SniperSpawns } from ".";
import { updateAllBotSpawns } from "./updateUtils";
import { showMapCullingDebug } from "../../config/advancedConfig.json"

export const setupSpawns = (container: DependencyContainer) => {
  const databaseServer = container.resolve<DatabaseServer>("DatabaseServer");
  const { locations } = databaseServer.getTables();

  const indexedMapSpawns: Record<number, ISpawnPointParam[]> = {};

  const mapsToExcludeFromPlayerCulling = new Set([
    "factory4_day",
    "factory4_night",
    "laboratory",
  ]);

  originalMapList.forEach((map, mapIndex) => {
    const configMap = configLocations[mapIndex];
    const allZones = [
      ...new Set(
        locations[map].base.SpawnPointParams.filter(
          ({ BotZoneName }: ISpawnPointParam) => !!BotZoneName
        ).map(({ BotZoneName }: ISpawnPointParam) => BotZoneName)
      ),
    ];

    locations[map].base.OpenZones = allZones.join(",");

    let bossSpawns: ISpawnPointParam[] = [];
    let scavSpawns: ISpawnPointParam[] = [];
    let sniperSpawns: ISpawnPointParam[] = [];

    let pmcSpawns: ISpawnPointParam[] = [];

    const bossZoneList = new Set([
      "Zone_Blockpost",
      "Zone_RoofRocks",
      "Zone_RoofContainers",
      "Zone_RoofBeach",
      "Zone_TreatmentRocks",
      "Zone_TreatmentBeach",
      "Zone_Hellicopter",
      "Zone_Island",
      "BotZoneGate1",
      "BotZoneGate2",
      "BotZoneBasement",
    ]);

    const isGZ = map.includes("sandbox");

    shuffle<ISpawnPointParam[]>(locations[map].base.SpawnPointParams).forEach(
      (point) => {
        switch (true) {
          case point.Categories.includes("Boss") ||
            bossZoneList.has(point.BotZoneName):
            bossSpawns.push(point);
            break;

          case point.BotZoneName?.toLowerCase().includes("snipe") ||
            (map !== "lighthouse" && point.DelayToCanSpawnSec > 40):
            sniperSpawns.push(point);
            break;

          case !!point.Infiltration || point.Categories.includes("Coop"):
            pmcSpawns.push(point);
            break;
          default:
            scavSpawns.push(point);
            break;
        }
      }
    );

    // fix GZ
    if (isGZ) {
      sniperSpawns.map((point, index) => {
        if (index < 2) {
          point.BotZoneName = Math.random()
            ? "ZoneSandSnipeCenter"
            : "ZoneSandSnipeCenter2";
        } else {
          point.BotZoneName = ["ZoneSandSnipeCenter", "ZoneSandSnipeCenter2"][
            index
          ];
        }
        return point;
      });
    }

    if (advancedConfig.ActivateSpawnCullingOnServerStart) {
      ScavSpawns[map] =
        removeClosestSpawnsFromCustomBots(
          ScavSpawns,
          scavSpawns,
          map,
          configLocations[mapIndex]
        ) || [];
      PmcSpawns[map] =
        removeClosestSpawnsFromCustomBots(
          PmcSpawns,
          pmcSpawns,
          map,
          configLocations[mapIndex]
        ) || [];
      PlayerSpawns[map] =
        removeClosestSpawnsFromCustomBots(
          PlayerSpawns,
          pmcSpawns,
          map,
          configLocations[mapIndex]
        ) || [];
      SniperSpawns[map] =
        removeClosestSpawnsFromCustomBots(
          SniperSpawns,
          sniperSpawns,
          map,
          configLocations[mapIndex]
        ) || [];
    }

    const { spawnMinDistance: limit } = mapConfig[configLocations[mapIndex]];

    let playerSpawns = BuildCustomPlayerSpawnPoints(
      map,
      locations[map].base.SpawnPointParams
    );

    playerSpawns = cleanClosest(
      playerSpawns,
      mapIndex,
      mapConfig[configMap].mapCullingNearPointValuePlayer
    );

    scavSpawns = cleanClosest(
      AddCustomBotSpawnPoints(scavSpawns, map),
      mapIndex,
      mapConfig[configMap].mapCullingNearPointValueScav
    ).map((point, botIndex) => {
      if (point.ColliderParams?._props?.Radius < limit) {
        point.ColliderParams._props.Radius = limit;
      }

      return !!point.Categories.length
        ? {
          ...point,
          BotZoneName: isGZ ? "ZoneSandbox" : point?.BotZoneName,
          Categories: ["Bot"],
          Sides: ["Savage"],
          CorePointId: 1,
        }
        : point;
    });

    pmcSpawns = cleanClosest(
      AddCustomPmcSpawnPoints(pmcSpawns, map),
      mapIndex,
      mapConfig[configMap].mapCullingNearPointValuePmc
    ).map((point, pmcIndex) => {
      if (point.ColliderParams?._props?.Radius < limit) {
        point.ColliderParams._props.Radius = limit;
      }

      return !!point.Categories.length
        ? {
          ...point,
          BotZoneName: isGZ
            ? "ZoneSandbox"
            : getClosestZone(
              scavSpawns,
              point.Position.x,
              point.Position.y,
              point.Position.z
            ),
          Categories: ["Coop", Math.random() ? "Group" : "Opposite"],
          Sides: ["Pmc"],
          CorePointId: 0,
        }
        : point;
    });

    sniperSpawns = AddCustomSniperSpawnPoints(sniperSpawns, map);

    indexedMapSpawns[mapIndex] = [
      ...sniperSpawns.map((point) => ({ ...point, type: "sniper" })),
      ...bossSpawns.map((point) => ({ ...point, type: "boss" })),
      ...scavSpawns.map((point) => ({ ...point, type: "scav" })),
      ...pmcSpawns.map((point) => ({ ...point, type: "pmc" })),
      ...playerSpawns.map((point) => ({ ...point, type: "player" })),
    ];
    showMapCullingDebug &&
      console.log(
        "sniperSpawns",
        sniperSpawns.length,
        "bossSpawns",
        bossSpawns.length,
        "scavSpawns",
        scavSpawns.length,
        "pmcSpawns",
        pmcSpawns.length,
        "playerSpawns",
        playerSpawns.length,
        map
      );

    locations[map].base.SpawnPointParams = indexedMapSpawns[mapIndex];

    const listToAddToOpenZones = [
      ...new Set(
        locations[map].base.SpawnPointParams.map(
          ({ BotZoneName }) => BotZoneName
        ).filter((zone) => !!zone)
      ),
    ];

    locations[map].base.OpenZones = listToAddToOpenZones.join(",");
  });

  //  PlayerSpawns, PmcSpawns, ScavSpawns, SniperSpawns
  if (advancedConfig.ActivateSpawnCullingOnServerStart) {
    updateAllBotSpawns(PlayerSpawns, "playerSpawns");
    updateAllBotSpawns(PmcSpawns, "pmcSpawns");
    updateAllBotSpawns(ScavSpawns, "scavSpawns");
    updateAllBotSpawns(SniperSpawns, "sniperSpawns");
  }
  globalValues.indexedMapSpawns = indexedMapSpawns;
};
