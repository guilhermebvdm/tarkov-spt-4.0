import { DependencyContainer } from "tsyringe";
import {
  ISeasonalEventConfig,
  ISeasonalEvent,
} from "@spt/models/spt/config/ISeasonalEventConfig.d";

import { ConfigServer } from "@spt/servers/ConfigServer";
import { ConfigTypes } from "@spt/models/enums/ConfigTypes";
import { SeasonalEventService } from "@spt/services/SeasonalEventService";
import { zombieTypesCaps } from "../Spawning/utils";

export const baseZombieSettings = (enabled: boolean, count: number) =>
  ({
    enabled,
    name: "zombies",
    type: "Zombies",
    startDay: "1",
    startMonth: "1",
    endDay: "31",
    endMonth: "12",
    settings: {
      enableSummoning: false,
      removeEntryRequirement: [],
      replaceBotHostility: true,
      zombieSettings: {
        enabled: true,
        mapInfectionAmount: {
          Interchange: count === -1 ? randomNumber100() : count,
          Lighthouse: count === -1 ? randomNumber100() : count,
          RezervBase: count === -1 ? randomNumber100() : count,
          Sandbox: count === -1 ? randomNumber100() : count,
          Shoreline: count === -1 ? randomNumber100() : count,
          TarkovStreets: count === -1 ? randomNumber100() : count,
          Woods: count === -1 ? randomNumber100() : count,
          bigmap: count === -1 ? randomNumber100() : count,
          factory4: count === -1 ? randomNumber100() : count,
          laboratory: count === -1 ? randomNumber100() : count,
        },
        disableBosses: [],
        disableWaves: [],
      },
    },
  } as unknown as ISeasonalEvent);

const randomNumber100 = () => Math.round(Math.random() * 100);

export const resetCurrentEvents = (
  container: DependencyContainer,
  enabled: boolean,
  zombieWaveQuantity: number,
  random: boolean = false
) => {
  const configServer = container.resolve<ConfigServer>("ConfigServer");
  const eventConfig = configServer.getConfig<ISeasonalEventConfig>(
    ConfigTypes.SEASONAL_EVENT
  );

  let percentToShow = random ? -1 : Math.round(zombieWaveQuantity * 100);
  if (percentToShow > 100) percentToShow = 100;

  eventConfig.events = [baseZombieSettings(enabled, percentToShow)];

  const seasonalEventService = container.resolve<SeasonalEventService>(
    "SeasonalEventService"
  ) as any;

  // First we need to clear any existing data
  seasonalEventService.currentlyActiveEvents = [];
  seasonalEventService.christmasEventActive = false;
  seasonalEventService.halloweenEventActive = false;
  // Then re-calculate the cached data
  seasonalEventService.cacheActiveEvents();
  // seasonalEventService.addEventBossesToMaps("halloweenzombies");
};

export const setUpZombies = (container: DependencyContainer) => {
  const configServer = container.resolve<ConfigServer>("ConfigServer");
  const eventConfig = configServer.getConfig<ISeasonalEventConfig>(
    ConfigTypes.SEASONAL_EVENT
  );

  eventConfig.events = [baseZombieSettings(false, 100)];

  // eventConfig.eventBossSpawns = {
  //   zombies: eventConfig.eventBossSpawns.halloweenzombies,
  // };
  eventConfig.eventGear[eventConfig.events[0].name] = {};
  eventConfig.hostilitySettingsForEvent.zombies.default =
    eventConfig.hostilitySettingsForEvent.zombies.default
      .filter(({ BotRole }) => !["pmcBEAR", "pmcUSEC"].includes(BotRole))
      .map((host) => ({
        ...host,
        AlwaysEnemies: [
          "infectedAssault",
          "infectedPmc",
          "infectedCivil",
          "infectedLaborant",
          "infectedTagilla",
          "pmcBEAR",
          "pmcUSEC",
        ],
        AlwaysNeutral: [
          "marksman",
          "assault",
          "bossTest",
          "bossBully",
          "followerTest",
          "bossKilla",
          "bossKojaniy",
          "followerKojaniy",
          "pmcBot",
          "cursedAssault",
          "bossGluhar",
          "followerGluharAssault",
          "followerGluharSecurity",
          "followerGluharScout",
          "followerGluharSnipe",
          "followerSanitar",
          "bossSanitar",
          "test",
          "assaultGroup",
          "sectantWarrior",
          "sectantPriest",
          "bossTagilla",
          "followerTagilla",
          "exUsec",
          "gifter",
          "bossKnight",
          "followerBigPipe",
          "followerBirdEye",
          "bossZryachiy",
          "followerZryachiy",
          "bossBoar",
          "followerBoar",
          "arenaFighter",
          "arenaFighterEvent",
          "bossBoarSniper",
          "crazyAssaultEvent",
          "peacefullZryachiyEvent",
          "sectactPriestEvent",
          "ravangeZryachiyEvent",
          "followerBoarClose1",
          "followerBoarClose2",
          "bossKolontay",
          "followerKolontayAssault",
          "followerKolontaySecurity",
          "shooterBTR",
          "bossPartisan",
          "spiritWinter",
          "spiritSpring",
          "peacemaker",
          "skier",
        ],
        SavagePlayerBehaviour: "Neutral",
        BearPlayerBehaviour: "AlwaysEnemies",
        UsecPlayerBehaviour: "AlwaysEnemies",
      }));

  // console.log(eventConfig.hostilitySettingsForEvent.zombies.default);
};
