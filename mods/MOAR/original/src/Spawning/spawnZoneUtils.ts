import _config from "../../config/config.json";
import { ISpawnPointParam } from "@spt/models/eft/common/ILocationBase";
import mapConfig from "../../config/mapConfig.json";
import { Ixyz } from "@spt/models/eft/common/Ixyz";
import { configLocations } from "./constants";
import {
  ScavSpawns,
  PlayerSpawns,
  SniperSpawns,
  PmcSpawns,
} from "../SpawnZoneChanges";
import { MapConfigType } from "./utils";

function sq(n: number) {
  return n * n;
}

function pt(a: number, b: number) {
  return Math.sqrt(sq(a) + sq(b));
}

export const getDistance = (
  x: number,
  y: number,
  z: number,
  mX: number,
  mY: number,
  mZ: number
) => {
  (x = Math.abs(x - mX)), (y = Math.abs(y - mY)), (z = Math.abs(z - mZ));

  return pt(pt(x, z), y);
};

export default function getSortedSpawnPointList(
  SpawnPointParams: ISpawnPointParam[],
  mX: number,
  my: number,
  mZ: number,
  cull?: number
): ISpawnPointParam[] {
  let culledAmount = 0;

  const sorted = SpawnPointParams.sort((a, b) => {
    const a1 = getDistance(
      a.Position.x,
      a.Position.y,
      a.Position.z,
      mX,
      my,
      mZ
    );
    const b1 = getDistance(
      b.Position.x,
      b.Position.y,
      b.Position.z,
      mX,
      my,
      mZ
    );
    return a1 - b1;
  }).filter((_, index) => {
    if (!cull) return true;
    const result = index > SpawnPointParams.length * cull;
    if (!result) culledAmount++;

    return result;
  });

  if (_config.debug && culledAmount > 0) {
    console.log(
      "Reduced to " +
      Math.round((sorted.length / SpawnPointParams.length) * 100) +
      "% of original spawns",
      SpawnPointParams.length,
      ">",
      sorted.length,
      "\n"
    );
  }
  return sorted;
}

export function cleanClosest(
  SpawnPointParams: ISpawnPointParam[],
  mapIndex: number,
  mapCullingNearPointValue: number
): ISpawnPointParam[] {
  const map = configLocations[mapIndex];

  const okayList = new Set();
  const filteredParams = SpawnPointParams.map((point) => {
    const {
      Position: { x: X, y: Y, z: Z },
    } = point;
    const result = !SpawnPointParams.some(({ Position: { z, x, y }, Id }) => {
      const dist = getDistance(X, Y, Z, x, y, z);
      return mapCullingNearPointValue > dist && dist !== 0 && !okayList.has(Id);
    });

    if (!result) {
      okayList.add(point.Id);
    }

    return result
      ? point
      : {
        ...point,
        DelayToCanSpawnSec: 9999999,
        CorePointId: 99999,
        Categories: [],
        Sides: [],
      };
  });

  if (_config.debug) {
    const actualCulled = filteredParams.filter(
      ({ Categories }) => !!Categories.length
    );
    console.log(
      map,
      filteredParams.length,
      ">",
      actualCulled.length,
      "Reduced to " +
      Math.round((actualCulled.length / filteredParams.length) * 100) +
      "% of original spawns",
      // player ? "player" : "bot"
    ); // high, low}
  }

  return filteredParams.filter((point) => !!point.Categories.length);

  // if (!_config.debug) {
  //   const actualCulled = culled.filter(({ Categories }) => !!Categories.length);
  //   console.log(
  //     map,
  //     "Reduced to " +
  //     Math.round((actualCulled.length / culled.length) * 100) +
  //     "% of original spawns",
  //     culled.length,
  //     ">",
  //     actualCulled.length
  //     // "\n"
  //   ); // high, low}
  // }
}

export function uuidv4() {
  return "10000000-1000-4000-8000-100000000000".replace(/[018]/g, (c) =>
    (
      +c ^
      (crypto.getRandomValues(new Uint8Array(1))[0] & (15 >> (+c / 4)))
    ).toString(16)
  );
}

export const AddCustomPmcSpawnPoints = (
  SpawnPointParams: ISpawnPointParam[],
  map: string
) => {
  if (!PmcSpawns[map] || !PmcSpawns[map].length) {
    _config.debug && console.log("no custom Bot spawns for " + map);
    return SpawnPointParams;
  }

  const playerSpawns = PmcSpawns[map].map((coords: Ixyz, index: number) => ({
    BotZoneName: getClosestZone(SpawnPointParams, coords.x, coords.y, coords.z),
    Categories: ["Coop", Math.random() ? "Group" : "Opposite"],
    Sides: ["Pmc"],
    CorePointId: 0,
    ColliderParams: {
      _parent: "SpawnSphereParams",
      _props: {
        Center: {
          x: 0,
          y: 0,
          z: 0,
        },
        Radius: 20,
      },
    },
    DelayToCanSpawnSec: 4,
    Id: uuidv4(),
    Infiltration: "",
    Position: coords,
    Rotation: random360(),
  }));

  return [...SpawnPointParams, ...playerSpawns];
};

export const AddCustomBotSpawnPoints = (
  SpawnPointParams: ISpawnPointParam[],
  map: string
) => {
  if (!ScavSpawns[map] || !ScavSpawns[map].length) {
    _config.debug && console.log("no custom Bot spawns for " + map);
    return SpawnPointParams;
  }

  const scavSpawns = ScavSpawns[map].map((coords: Ixyz) => ({
    BotZoneName: getClosestZone(SpawnPointParams, coords.x, coords.y, coords.z),
    Categories: ["Bot"],
    ColliderParams: {
      _parent: "SpawnSphereParams",
      _props: {
        Center: {
          x: 0,
          y: 0,
          z: 0,
        },
        Radius: 20,
      },
    },
    CorePointId: 1,
    DelayToCanSpawnSec: 4,
    Id: uuidv4(),
    Infiltration: "",
    Position: coords,
    Rotation: random360(),
    Sides: ["Savage"],
  }));

  return [...SpawnPointParams, ...scavSpawns];
};

export const AddCustomSniperSpawnPoints = (
  SpawnPointParams: ISpawnPointParam[],
  map: string
) => {
  if (!SniperSpawns[map] || !SniperSpawns[map].length) {
    _config.debug && console.log("no custom Player spawns for " + map);
    return SpawnPointParams;
  }

  const sniperSpawns = SniperSpawns[map].map((coords: Ixyz, index: number) => ({
    BotZoneName:
      getClosestZone(SpawnPointParams, coords.x, coords.y, coords.z) ||
      "custom_snipe_" + index,
    Categories: ["Bot"],
    ColliderParams: {
      _parent: "SpawnSphereParams",
      _props: {
        Center: {
          x: 0,
          y: 0,
          z: 0,
        },
        Radius: 20,
      },
    },
    CorePointId: 1,
    DelayToCanSpawnSec: 4,
    Id: uuidv4(),
    Infiltration: "",
    Position: coords,
    Rotation: random360(),
    Sides: ["Savage"],
  }));

  return [...SpawnPointParams, ...sniperSpawns];
};

export const random360 = () => Math.random() * 360;

export const BuildCustomPlayerSpawnPoints = (
  map: string,
  refSpawns: ISpawnPointParam[]
) => {
  const playerOnlySpawns = refSpawns
    .filter((item) => !!item.Infiltration && item.Categories[0] === "Player")
    .map((point) => {
      point.ColliderParams._props.Radius = 1;
      point.Position.y = point.Position.y + 0.5;
      return {
        ...point,
        BotZoneName: "",
        isCustom: true,
        Id: uuidv4(),
        Sides: ["Pmc"],
      };
    });

  // console.log(map, playerOnlySpawns.length);
  if (!PlayerSpawns[map] || !PlayerSpawns[map].length) {
    _config.debug && console.log("no custom Player spawns for " + map);
    return playerOnlySpawns;
  }

  const getClosestInfil = (X: number, Y: number, Z: number) => {
    let closest = Infinity;
    let selectedInfil = "";

    playerOnlySpawns.forEach(({ Infiltration, Position: { x, y, z } }) => {
      const dist = getDistance(X, Y, Z, x, y, z);
      if (!!Infiltration && dist < closest) {
        closest = dist;
        selectedInfil = Infiltration;
      }
    });

    return selectedInfil;
  };

  const playerSpawns = PlayerSpawns[map].map((coords: Ixyz, index) => ({
    BotZoneName: "",
    Categories: ["Player"],
    ColliderParams: {
      _parent: "SpawnSphereParams",
      _props: {
        Center: {
          x: 0,
          y: 0,
          z: 0,
        },
        Radius: 1,
      },
    },
    isCustom: true,
    CorePointId: 0,
    DelayToCanSpawnSec: 4,
    Id: uuidv4(),
    Infiltration: getClosestInfil(coords.x, coords.y, coords.z),
    Position: coords,
    Rotation: random360(),
    Sides: ["Pmc"],
  }));

  // TODO: Check infils
  // console.log(map);
  // console.log(playerOnlySpawns[0], playerSpawns[0]);

  return [...playerOnlySpawns, ...playerSpawns];
};

export const getClosestZone = (
  params: ISpawnPointParam[],
  x: number,
  y: number,
  z: number
) => {
  if (
    Array.isArray(params) &&
    !params.filter(({ BotZoneName }) => BotZoneName).length
  )
    return "";

  return (
    getSortedSpawnPointList(params, x, y, z).find(
      ({ BotZoneName }) => !!BotZoneName
    )?.BotZoneName || ""
  );
};

export const removeClosestSpawnsFromCustomBots = (
  CustomBots: Record<string, Ixyz[]>,
  SpawnPointParams: ISpawnPointParam[],
  map: string,
  mapConfigMap: string
) => {
  if (!CustomBots[map] || !CustomBots[map].length) {
    console.log(map, "Is empty");
    return;
  }

  const coords: Ixyz[] = CustomBots[map];

  const { mapCullingNearPointValuePlayer,
    mapCullingNearPointValuePmc,
    mapCullingNearPointValueScav } = (mapConfig[mapConfigMap] as MapConfigType)

  const mapCullingNearPointValue = (mapCullingNearPointValuePlayer +
    mapCullingNearPointValuePmc +
    mapCullingNearPointValueScav) / 3


  let filteredCoords = coords.filter(
    ({ x: X, y: Y, z: Z }) =>
      !SpawnPointParams.some(({ Position: { z, x, y } }) => {
        return mapCullingNearPointValue > getDistance(X, Y, Z, x, y, z);
      })
  );

  const okayList = new Set();

  filteredCoords = [...coords].filter(({ x: X, y: Y, z: Z }, index) => {
    const result = !coords.some(({ z, x, y }) => {
      const dist = getDistance(X, Y, Z, x, y, z);
      return (
        mapCullingNearPointValue * 1.3 > dist &&
        dist !== 0 &&
        !okayList.has("" + x + y + z)
      );
    });
    if (!result) okayList.add("" + X + Y + Z);
    return result;
  });

  console.log(
    map,
    coords.length,
    ">",
    filteredCoords.length,
    "culled",
    coords.length - filteredCoords.length,
    "spawns"
  );
  return filteredCoords;
};
