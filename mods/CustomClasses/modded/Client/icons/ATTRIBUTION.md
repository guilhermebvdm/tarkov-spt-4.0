# Class icon attribution

The class icons in this folder are derived from **[game-icons.net](https://game-icons.net/)**,
licensed under **[CC BY 3.0](https://creativecommons.org/licenses/by/3.0/)**.

Each PNG is a recolor (white silhouette on transparent) + crop of the original SVG, produced by
`scripts/build-icons.mjs` from the vendored sources in `scripts/icon-sources/`. The white
silhouette is an alpha mask: the client tints it with the class color at runtime.

| Icon (file) | Class | Original icon | Author | Source |
|---|---|---|---|---|
| `armeiro.png` | Armeiro | Anvil | Lorc | https://game-icons.net/1x1/lorc/anvil.html |
| `batedor.png` | Batedor | Binoculars | Delapouite | https://game-icons.net/1x1/delapouite/binoculars.html |
| `cacador.png` | Caçador | Bullseye | Skoll | https://game-icons.net/1x1/skoll/bullseye.html |
| `fuzileiro.png` | Fuzileiro | AK47 | Skoll | https://game-icons.net/1x1/skoll/ak47.html |
| `gerenteDeOperacoes.png` | Gerente de Operações | Gears | Lorc | https://game-icons.net/1x1/lorc/gears.html |
| `medicoDeCombate.png` | Médico de Combate | Health normal | Sbed | https://game-icons.net/1x1/sbed/health-normal.html |
| `operadorFurtivo.png` | Operador Furtivo | Hooded figure | DarkZaitzev | https://game-icons.net/1x1/darkzaitzev/hooded-figure.html |
| `operadorTatico.png` | Operador Tático | Star medal | Delapouite | https://game-icons.net/1x1/delapouite/star-medal.html |
| `saqueador.png` | Saqueador | Swap bag | Lorc | https://game-icons.net/1x1/lorc/swap-bag.html |
| `sobrevivencialista.png` | Sobrevivencialista | Campfire | Lorc | https://game-icons.net/1x1/lorc/campfire.html |
| `tanque.png` | Tanque | Kevlar vest | Skoll | https://game-icons.net/1x1/skoll/kevlar-vest.html |
| `peladao.png` | Peladão (item 016) | Underwear | Delapouite | https://game-icons.net/1x1/delapouite/underwear.html |

> Under CC BY 3.0 you must keep this credit if you redistribute the icons. To swap an icon,
> replace the matching `.svg` in `scripts/icon-sources/`, update this table, and re-run
> `npm run build:icons`.
