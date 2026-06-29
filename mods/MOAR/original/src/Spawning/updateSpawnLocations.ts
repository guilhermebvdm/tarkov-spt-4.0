import { ILocation } from "@spt/models/eft/common/ILocation";
import { configLocations } from "./constants";
import _config from "../../config/config.json";
import { getRandomInArray, shuffle } from "./utils";
import advancedConfig from "../../config/advancedConfig.json";
import { ISpawnPointParam } from "@spt/models/eft/common/ILocationBase";
import { globalValues } from "../GlobalValues";
import getSortedSpawnPointList, {
  getClosestZone,
  getDistance,
  uuidv4,
} from "./spawnZoneUtils";

export default function updateSpawnLocations(
  locationList: ILocation[],
  config: typeof _config
) {
  for (let index = 0; index < locationList.length; index++) {
    const map = configLocations[index];
    const mapSpawns = [...globalValues.indexedMapSpawns[index]];

    const playerSpawns = mapSpawns.filter(
      (point) => point?.["type"] === "player"
    );

    const playerSpawn: ISpawnPointParam = getRandomInArray(playerSpawns);
    globalValues.playerSpawn = playerSpawn;

    const { x, y, z } = playerSpawn.Position;

    const sortedSpawnPointList = getSortedSpawnPointList(mapSpawns, x, y, z);

    const possibleSpawnList: ISpawnPointParam[] = [];

    sortedSpawnPointList.forEach((point) => {
      if (
        possibleSpawnList.length < advancedConfig.SpawnpointAreaTarget &&
        point?.["type"] === "player"
      ) {
        point.ColliderParams._props.Radius = 1
        possibleSpawnList.push(point);
      }
    });

    // const possibleSpawnListSet = new Set(possibleSpawnList.map(({ Id }) => Id));

    locationList[index].base.SpawnPointParams = [
      ...possibleSpawnList,
      ...sortedSpawnPointList.filter((point) => point["type"] !== "player"),
    ];

    //  {
    // if (point["type"] === "player" && !possibleSpawnListSet.has(point.Id)) {
    //   point.Categories = [];
    //   point.Sides = [];
    // }

    // return point;
    // }

    // console.log(
    //   map,
    //   locationList[index].base.SpawnPointParams.filter(
    //     (point) => point?.["type"] === "player"
    //   ).length,
    //   locationList[index].base.SpawnPointParams.filter(
    //     (point) => point?.Categories[0] === "Player"
    //   ).length
    // );
  }
}
