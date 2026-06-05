---
title: "ORBIT — Transcrição do thread no Discord (SPT, #mods-development)"
date: 2026-06-04
status: 🔵 Em andamento
authors: Guilherme
---

# ORBIT — Transcrição do thread no Discord

> Captura **fiel** do thread `"ORBIT"` no canal **#mods-development** do servidor Discord da comunidade **SPT** (SPT Pub).
>
> - **Link:** <https://discord.com/channels/875684761291599922/1509314495019745451>
> - **Iniciado por:** Chazut — 27/05/2026 18:57 (GMT-3)
> - **Última mensagem capturada:** 04/06/2026 19:15 (GMT-3)
> - **Total de mensagens:** 699
> - **Anexos:** 30 imagens + 2 logs, salvos em [`assets/`](./assets/)
> - **Método:** navegação autenticada no Discord (extração do DOM via Chrome DevTools), capturado em 2026-06-04.
> - **Notas:** horários convertidos para **GMT-3** (como exibidos no Discord do autor da captura). Texto **preservado no idioma original** (majoritariamente inglês). `↳@X` indica resposta direcionada a X. Linhas `📎` são anexos.

---


## 27/05/2026

**Chazut** · `18:57`
Hey everyone  
  
Spent a lot of time building a mod to make bot raids feel less random: PMC squads that actually have goals instead of patrolling aimlessly, coordinate their looting, and extract when they've got what they came for.  
  
Inspired by Phobos, LootingBots and QuestingBot, but rebuilt from scratch as a single system instead of three layers fighting each other.  
  
Released today: https://forge.sp-tarkov.com/mod/2706/orbit  
  
 Strongly recommend installing Raid Review alongside it, it visualizes squad main objectives, makes it way easier to understand what's actually happening.  
  
First public release so expect rough edges. If you test it and see a bot wedged in a wall, a squad camping a random spot for 20 minutes, or anything that just feels off - please tell me.  
  
Player feedback is what helps me balance this thing.  
  
Thanks _(editado)_

**ms. r3mains** · `19:41` — definitely trying this later today

**Chazut** · `19:55` — ⁠Janky's Emporium of Jank⁠

**Fums** · `20:26` — Has it been removed on the forge or just privated? I can't access it

**Chazut** · `20:42` ↳@Fums
Yes, removed for now.  
Need explicit permission from upstream authors before republishing on the Hub (Forge guideline 6.2).  
Already cleared with Janky for Phobos ⁠Janky's Emporium of Jank⁠, waiting for LootingBots ⁠LootingBots⁠.  
Will be back up if that's resolved

**TheSunGod** · `22:20` — Following this very closely.

**TheSunGod** · `22:20` — Glad to see you got around to it @Chazut, hope my annoying cheering was at least part of the reason you decided to go for it _(editado)_

**TheSunGod** · `22:21` — So side question, ORBIT is not going to be compatible with LB, but instead it is kinda "integrated" into it, yes?


## 28/05/2026

**Cosmin** · `04:03` — is this mod more fps demanding than phobos? and another question, does it still have bots in  places phobos has? for example edges of maps

**Chazut** · `04:12`
FPS: slightly more (or same) than Phobos, not less, same core but more waypoints and I added main-objective attraction, (optional) scav home pull (to not let them roam to far from spawn) + loot logic from LootingBot (looking at the loot, looting animation), but those are light, the real cost is BSG's pathfinding.  
  
Edges: yep, still happens, same advection field as Phobos (but added some new forces, like bot main objectives and attraction to them), so bots can still drift freely toward map edges sometimes.

**Ranger4R** · `04:22` — is Orbit meant to work together with Phobos? or there will be problems?

**Chazut** · `04:24`
No, since it's more like a Phobos extended mod, it will only works with SAIN.  
No Phobos, LootingBot (some looting logic ported from it) or QuestingBot (inspired by, no code reuse but they will conflict) compatibility.

**Ranger4R** · `04:26`
I see, it is interesting to compare how it is behave..  
cause you've removed convergence logic, as I understood..

**Chazut** · `04:26` — Yes convergence to player is removed but new forces are added

**Chazut** · `04:27` — towards bot main objectives, scav spawn location (to not make scavs roam the entire map), and others. All tweakable on/off _(editado)_

**Chazut** · `04:29` — But for Phobos, you can already set the convergence to 0 in Phobos settings; that will remove the convergence to the player _(editado)_

**Cosmin** · `04:29` — i do feel this mods wins some fps, at least for me as i deactivated sain for bosses guards and scavs and also i deactivated this mod for scavs

**Vendth** · `06:04` ↳@Chazut — what are the objectives?

**Chazut** · `08:56`
For now: Roam a hotzone hunting for kills, loot a high value zone (can be marked), do some quests  
  
But planning to add more like: boss hunting, spawn rush, marked rush, extract camping

**Chazut** · `08:56` — Bot are roaming freely between main objectives, thanks to Phobos code

**Cosmin** · `08:57` — from my testing, with orbit i barely get pmc kills in customs, they actively hunt each other, with phobos each pmc roamed each corner of the map and it felt like no place was safe. Is there a setting to turn it more like phobos? i even ramped up  radius and force scale but nothing

**Cosmin** · `08:57` — also i was more on the dorms part

**Chazut** · `09:06`
No, I removed "convergence to player" from Phobos. So you will always have more action close to you using Phobos.  
  
ORBIT gives main objectives to bot, trying to simulate what a real player can have in mind when entering a map, you have few objectives to do, but do not have always a path in mind, depending on your spawn and what happen in raid. So bots will be attracted by the objectives but are roaming freely between them. Orbit don't push bot towards the player but towards their own objectives.  
  
And objectives attribution depends on SAIN personality (a Rat will be more likely to have loot objectives)

**Cosmin** · `09:18` — what happens if i disable main objective or a reduce quests from 70 % to around 20 30

**Chazut** · `09:30`
Disable main objectives: squads free-roam the whole map via the advection field (same as Phobos roaming, minus player convergence).  
Note: if you do this, the Quest/Kills/LootValue % no longer matter,  the whole system is off, so tuning those does nothing.  
Heads up though, this also strips out a big part of what makes ORBIT.  
  
Keep main objectives on, drop Quest % to 20-30: the weights are normalised, so more squads roll Kills/LootValue instead. Bump Kills % up too and you'll get more squads prowling PvP hotzones = more PMC fights. Or complety block personalities other than Wreckless/Chad/GigaChad (that are more likely to hunt for PvP).  
  
Either way ORBIT won't pull bots toward you (convergence is removed by design), so Phobos's "nowhere is safe" feel won't fully return.  
  
Honestly, if what you want is Phobos's behaviour + looting, you might be better off waiting for an official Phobos release that integrates looting.  
ORBIT and Phobos can go in totally different directions, I'm not trying to replace Phobos or any other mod, just doing my own thing

**Cosmin** · `09:33` — i see

**Cosmin** · `09:33` — yea what i do like is it has more fps and no roaming goons and cultists

**Cosmin** · `09:34` — the looting is also fine

**Chazut** · `09:37`
Glad it feel good  
On cultists though, I haven't changed anything specific for them, they roam freely by default (same code as Phobos). That said, it's a good idea, I could add a cultist on/off toggle like the scav/goon ones.  
Thanks for the suggestion _(editado)_

**Cosmin** · `09:39` — also i do think dorms are safe and has no pvp

**Cosmin** · `09:39` — 3 raids in a row and dorms where empty the whole raid

**Cosmin** · `09:39` — only around constructions the pvp took place

**Chazut** · `09:51`
That's just raid variance.  
  
Dorms is one of the defined hotspots in Phobos, so it apply to ORBIT as well (it's a Kills objective anchor + an advection attraction zone), but whether it actually gets busy depends on what your bots rolled that raid: how many got a Kills objective vs quest/loot, their SAIN personalities (a raid full of Rats will avoid PvP and loot quietly), etc. Lots of factors.  
  
So some raids Dorms is a bloodbath, others it's dead and the action ends up around Construction instead.  
  
If you want to see why a given raid played out that way, grab last Raid Review and clic on a bot dot, it shows each squad's main objectives + movement on the post-raid map, you'd see exactly where they all went and why _(editado)_

**Cosmin** · `09:51` — thanks!

**LifeBosses (Серёга)** · `13:13` ↳@Chazut — have Is there any info anywhere on how to install mod? or mod not ready? _(editado)_

**Zybergeris** · `13:17` ↳@LifeBosses (Серёга) — its no published yet, since he needs LB authors to allow of using their code.

**LifeBosses (Серёга)** · `13:18` ↳@Zybergeris — oh ok ty

**Chazut** · `13:24` ↳@LifeBosses (Серёга)
Not installable right now, I pulled it to sort out permissions with the mods it builds on (Phobos and LootingBot).  
  
Please don't pester the authors about it though, the decision is entirely theirs and I completely understand whatever they decide, either way.  
  
I'll post here the moment there's news

**Baconism** · `13:25`
  🖼️ 📎 [`att-01-2026-05-28-Baconism.jpg`](./assets/att-01-2026-05-28-Baconism.jpg)

**LifeBosses (Серёга)** · `13:26` ↳@Chazut — Just to clarify! Thanks, we'll wait.

**Cosmin** · `16:55` — imagine a mod that can activate phobos in some raids and orbit in other raids, making raid variation even greater

**Vendth** · `18:13` — I feel like the best outcome is you all get in a room, cuddle, and unify the mods

**LAWW** · `22:23` — having trouble installing the pre release, extracted it and no bueno

**Marko** · `22:45` — i got bad news bud

**ArchaicFink** · `22:48` — It's probably the GitHub package.

**Scootis_McPootis** · `23:02` — Github only has the source code, you can build it yourself but it takes a bit of know how.

**ArchaicFink** · `23:19` — Probably best to wait for Chazut to officially release Orbit if he gets the required permissions. _(editado)_


## 29/05/2026

**LAWW** · `00:51` — what i was thinking

**TheSunGod** · `01:55` ↳@ArchaicFink — If.

**ArchaicFink** · `01:56` ↳@TheSunGod — FIFY

**TheSunGod** · `02:04` ↳@ArchaicFink
Lol.  
  
Nah, don't get me wrong. I'm rooting super hard. Questing Bots + Looting Bots combo is broken and might take a while to be fixed, and QB does not work well with TactialToaster's mods, so i switched to Phobos. But now with Phobos, i miss LootingBots a lot.  
  
ORBIT would be an absolute godsend. But i'm pretty worried how Skwizzy and Archangelway are still silent on the matter.

**ArchaicFink** · `02:11` — Phobos with a looting layer would be awesome. It's so lightweight and configurable and plays well with SAIN.

**ArchaicFink** · `02:13` — Although Orbit does sound promising. Bt the proof is in the pudding I guess. For now it's Phobos for those who are willing to test it.

**Kobe Thuy** · `02:24` — From my testing it seems like it works

**Kobe Thuy** · `02:24` — saw bots looting and all

**Pandito** · `02:27` — I don't understand the drama. If the existing mods are open source, we should have the rights to do any kind of fork or use the code to improve like ORBIT. This is exactly the purpose

**Kobe Thuy** · `02:33` ↳@Pandito — You are allowed to fork the code and do whatever as long as it personal. Publishing it on the Forge is a different story.

**Pandito** · `02:37` ↳@Kobe Thuy — Are you sure ? Publishing is part of open source loop in most of the framework as long it's credited

**Kobe Thuy** · `02:42` ↳@Pandito — Read the Forge’s guideline.

**djuice** · `03:09` — With TABs practically dead, this is the next mod I am looking forward too, hopefully thigns all go well.

**Chazut** · `03:15` ↳@Pandito — yes, I, too, thought the MIT license was sufficient, but: _(editado)_
  🖼️ 📎 [`att-02-2026-05-29-Pandito.png`](./assets/att-02-2026-05-29-Pandito.png)

**harmony 👾** · `04:01` ↳@Baconism — I've seen this in TAB

**harmony 👾** · `04:02` — I've played these games before

**harmony 👾** · `04:03` — Jokes aside I'm in for whatever frankenstein this is, no hate intended

**Fums** · `05:58` ↳@djuice — Forgot about that, what actually happened there? I know he kept saying it was going to get an alpha release in a few days and then went radio silence for a few months

**Zybergeris** · `06:23` — It was obvious that, that project will die. It was promising too much from someone random, too many soons and tomorrow

**Baconism** · `08:22` ↳@Fums — Sounded like it was a bunch of irl things that stopped progress, dunno how true that is, but thats what im going with _(editado)_

**Pandito** · `09:03` — Anyway to have your mod outside forge @Chazut  ?

**ArchaicFink** · `09:04` ↳@harmony 👾 — What's TAB?

**Baconism** · `09:06` — It stands for ThePasch's Autonomous Bots _(editado)_

**Chazut** · `09:07` ↳@Pandito
I'd rather not.  
I removed it to do things properly, and sharing it elsewhere to dodge that would defeat the whole point. I don't want to release it in any form if it puts me at odds with the modding community or the SPT staff.  
Waiting on the LootingBots permission, then it's back on the Forge the right way

**Baconism** · `09:08` — Was basically another attempt at being a successor to Looting and Questing Bots for SPT 4.0

**ArchaicFink** · `09:26` ↳@Baconism — Oh that mod. I was always sus about it.

**Fums** · `10:18` — He was open it was vibe coded from the start really

**Fums** · `10:18` — Which is fine in my eyes as long as it's made apparent that's the case

**Pandito** · `10:28` ↳@Chazut
Thank you buddy. Have you already some known issues you are trying to fix ?  
Do you believe it's a bit lighter to run than QB+LB ?

**Chazut** · `10:47`
Have you already some known issues you are trying to fix ? Not yet  
Do you believe it's a bit lighter to run than QB+LB ? Yes, thanks to Phobos, and my quest system is much less advanced than QB's, so yes

**harmony 👾** · `10:52` ↳@Fums — Naw naw, he just ran out of tokens  /j _(editado)_

**Chazut** · `11:05`
ORBIT quests system is deliberately simple, the bot routes to a quest spot and waits there a bit, and if a quest has several objectives it only takes one. It doesn't run the full questline.  
  
QuestingBots is way deeper on this, it actually does the quest (plant timers, looking at quest-item spots, favouring the player's quests, etc). So if you want real quest simulation, QB's the tool.  
  
In ORBIT, "quest" is just one of three objective types to send bots to interesting spots, not to role-play the whole task.  
  
And honestly, I'll keep it simple, going deeper would mean borrowing from QB's work, and I'd rather not kick off another code-reuse drama lol

**Chazut** · `11:09` — DanW's work on QuestingBots is seriously advanced. What he's doing right now getting bots to actually work through the Labyrinth stages looks impressive, that's a whole other level of complexity from what ORBIT does. Hats off

**MaxP0wers** · `11:15` — There's a lot to be said for lightweight, CPU friendly, well optimized. I really like QB, but play a good bit of the game in VR and stutters are really noticeable.

**Chazut** · `11:17` — Already feeling like the interaction-based extracts are gonna be a pain, the Reserve ones where you turn on switches before they open, I think that's something QB already does

**Chazut** · `11:18` — (ORBIT bot extracts are disabled in Reserve for now  )

**MaxP0wers** · `11:21` — Could always just send them down the manhole. Would be kinda fun to find discarded backpacks around there late in raid

**MaxP0wers** · `11:22` — From what I've read, I think ORBIT will be my personal preference among the 3. I'd personally usually leave convergence off or really low anyway (though always fun to have a hectic factory run every once in awhile ).

**MaxP0wers** · `11:22` — And I think I'll miss the bot looting in phobos. Though I will try this next.

**Chazut** · `11:23` — FYI, convergence is disabled in Factory and GZ _(editado)_

**Vendth** · `11:23` ↳@Chazut — Maybe have it so that bots have a compulsion to press any switch they come across? There's so few of them anyway so as long as bots roam around you have a chance to turn on the power

**Chazut** · `11:23` — with Phobos

**MaxP0wers** · `11:23` — Good to know.

**Chazut** · `11:24` — Maybe in Labs as well, I haven't checked

**Chazut** · `11:26` ↳@Vendth — I'll think of a solution and consider enabling extract that needs dropping backpacks as well _(editado)_

**Chazut** · `11:27` — I already implemented the "Car Extract" feature, where a squad member waits for their teammates before starting the timer _(editado)_

**Chazut** · `11:29` — Otherwise, sometimes the squad would extract without waiting for an injured member, who was slower _(editado)_

**Baconism** · `12:27` ↳@Chazut — Do they have certain tasks like "go to x location, open locked door and loot items above x value"?

**Baconism** · `12:27` — Like how QB had marked room looters _(editado)_

**Baconism** · `12:29` — assuming not

**Chazut** · `12:33` ↳@Baconism
Partially: ORBIT's LootValue objective sends squad to the richest zones with per-personality value thresholds (Rats grab cheap stuff, GigaChads ignore anything under ~20k), and bots can roll to open locked doors (configurable rate). I've already seen them open markeds during my tests.  
  
I do not have specific "marked room" targeting yet, it's on my roadmap. Didn't actually know QB had that though, so just to be clear it's a convergent idea, not me copying it _(editado)_
  🖼️ 📎 [`att-03-2026-05-29-Baconism.png`](./assets/att-03-2026-05-29-Baconism.png)

**Baconism** · `12:33` ↳@Chazut

**Baconism** · `12:35` — Yeah the QB marked room rushers were definitely a thing I remember running into back in like 3.8/3.9 quite alot, and when combined with looting bots the bots would wipe the room clean

**Baconism** · `12:36` — Felt very live-like which is what i strive for with my SPT instances

**LifeBosses (Серёга)** · `13:17` — I'm very curious to see how this will work in gameplay. We're waiting for the release and hope you'll get permission.

**Cosmin** · `18:23` — airdrop wasnt already in it ?

**Chazut** · `19:42` — My looting system is not exactly the same as LootingBot, LB makes bots loot when they see a loot, ORBIT tell bots "go here, loot this and check other items in proximity". _(editado)_

**Kobe Thuy** · `20:00` — Airdrops are more “Quest” than “Loot”

**Kobe Thuy** · `20:01` — iirc QB implements airdrop and not LB

**Scootis_McPootis** · `20:13` — The latest LB update does something with airdrops:

**Scootis_McPootis** · `20:13` — "Looting priority, currently implemented for corpses (Bots should prioritize looting their own kills) and airdrops"

**Scootis_McPootis** · `20:13` — Pretty sure Questing Bots can assign airdrops as a quest for bots though

**S41elite** · `22:33` ↳@Scootis_McPootis — uuu   how?

**Scootis_McPootis** · `22:50` ↳@S41elite — From it's github:

**Scootis_McPootis** · `22:50`
There are several types of quests available to each bot:  
  
EFT Quests: Bots will go to locations specified in EFT quests for placing markers, collecting/placing items, killing other bots, etc. Bots can also use quests added by other mods.  
    Spawn Rush: At the beginning of the raid, bots that are within a certain distance of you will run to your spawn point. Only a certain number of bots are allowed to perform this quest, and they won't always select it. This makes PVP-focused maps like Factory even more challenging.  
    Boss Hunter: Bots will search zones in which bosses are known to spawn. They will only be allowed to select this quest at the beginning of the raid (within the first 5 minutes by default) and if they're a high enough level.  
    Airdrop Chaser: Bots will run to the most recent airdrop if it's close to them (within 500m by default). They will be allowed to select this quest within questing.bot_quests.airdrop_bot_interest_time seconds (420s by default) of the airdrop crate landing.  
    Spawn Point Wandering: Bots will wander to different spawn points around the map. This is used as a last resort in case the bot doesn't select any other quests. This quest is currently disabled by default because it should no longer be needed with the quest variety offered in the 0.4.0 and later releases.  
    "Standard" Quests: Bots will go to specified locations around the map. They will prioritize more desirable locations for loot and locations that are closer to them. These also include some sniping and camping quests on all maps, so be careful!  
    "Custom" Quests: You can create your own quests for bots using the templates for "standard" quests. None are provided by default. _(editado)_


## 30/05/2026

**Cosmin** · `07:06` — damn, cultists are so hard to find on woods and shoreline as they roam

**Cosmin** · `11:07` — at this rate il complete the quest to kill cultist priest on shoreline in a week at best lol

**Cosmin** · `11:15` — this is infuriating

**Vendth** · `11:31` — you could just disable the mod for a bit, I assume phobos and qb would have the same issue

**Vendth** · `11:31` — i think roaming mods should have an option like SAIN does to keep vanilla behavior for bosses

**Cosmin** · `11:38` — nah, that would suck to activate and deactivate a mod just for questing...guess i will just hunt them down for days lol

**Cosmin** · `11:38` — i put their spawn at 80% for woods and shoreline

**Cosmin** · `11:39` — they know how to hide

**Cosmin** · `11:39` — 3 times they where under my nose

**S41elite** · `12:33` ↳@Scootis_McPootis — no, i mean, yeah, i read that at the time i installed it, i meant how u add those because i have never see them loot the air drops

**S41elite** · `12:34` ↳@Cosmin — they usually barely move when close to the player, unless u shoot them xD

**Cosmin** · `18:11` — they more fast _(editado)_

**Pandito** · `19:42` — I want this mode so much

**Cosmin** · `19:52` ↳@S41elite — nah they really know how to hide


## 31/05/2026

**Ika** · `08:54` — Just dropping by to say this is the best PvE AI I've experienced on SPT and I've played since 3.8. Few issues here and there but nothing worth complaining about

**RetroLogic** · `09:36` ↳@Ika — I'm so  jealous of the people who managed to get the ORBIT before it was removed from the Hub, fingers crossed it will be available again soon. The more I read about it through comments the better it sounds.

**Ika** · `09:38` ↳@RetroLogic — Oh I didn't, I just compiled it

**Cosmin** · `09:38` — we also need some pmc that only roams and ganks for kills, like the ones roaming in phobos

**Cosmin** · `09:39` — not just focusing on specific zones i mean

**RetroLogic** · `09:49` ↳@Ika — Is there a guide anywhere on the net that can teach me how to compile it from the source code? would be good to learn.

**ArchaicFink** · `10:01` — Probably best to wait this one out for Chaz to get actual permission from Arch to use the LB code in Orbit.

**ArchaicFink** · `10:02` — There's people bugging Arch to in the LB thread to say yes already. _(editado)_

**RetroLogic** · `10:09` ↳@ArchaicFink — People shouldn't be bugging mod devs at all, just let the two parties involved sort it out. Your right about waiting though, I will put my patience hat back on and wait.

**ArchaicFink** · `10:14` — It's probably for the best to wait and see how this goes. That one guy who did ping LB's dev in the thread did get a warning, but that was public. Who knows how many smoothbrains DM'ed them to say 'Just say yes to Orbit'. _(editado)_

**Vendth** · `10:23` ↳@ArchaicFink — May be unpopular but I am getting tired of the vibe of this community, I am relatively new and I have seen so many instances of people getting shot down for asking questions or doing something innocent that it made me reluctant to even speak _(editado)_

**Vendth** · `10:24` — Like I feel like if I want to ask something I am expected to weigh every word and research the entirety of discord to make sure it's okay to ask and was not asked before

**Chazut** · `10:24`
Please don't bother LB authors.  
  
Sorry, guys, I'm with my family this weekend. I'll reply to your DMs and comments once I'm back home.

**Slum_K1ng** · `10:29` ↳@Vendth
It should be common courtesy to not ping mod devs with basically “Let this other mod that ripped 20% of your code base use your code in a public release”  
  
This community is pushing all of the long-term mod devs away, if anything.

**Snoops** · `11:59` ↳@ArchaicFink — That's so annoying urgh

**Vendth** · `13:00` ↳@Slum_K1ng — Which is completely fair too, I am not saying they are wrong

**Shynd** · `13:04` ↳@Vendth
New people don't read a thread or pinned messages before bothering a mod dev with the same question asked 3 messages above theirs, asking when a mod will release when there are rules and guidelines pinned about not doing so, etc.  
  
You shouldn't feel hesitant or reluctant to converse, but the onus is on you/anyone to see what is respectful and acceptable to ask.  
  
It's not much work. Plenty of people enter the community and start conversing immediately without getting warned because they read first.

**Shynd** · `13:06` — To be clear: I'm not saying you, specifically, have done any of this. Just that the solution to feeling reluctant to ask questions is very straight forward. And if anyone has done their due diligence and asks respectfully it tends not to even matter if they ask an annoying question or something like that. They get told we don't do that here, they respectfully say something like 'oh I didn't realize thanks for letting me know', and everyone moves on

**Chazut** · `13:40` — Thank you to everyone telling people not to harass Arch and Skwizzy. That kind of pressure is exactly what would push a "maybe" toward a "no". The right move is patience, theirs entirely, and I respect either outcome. Please, anyone reading: do NOT DM or ping them about ORBIT. If the answer ends up being no I'll handle it, no drama needed

**Chazut** · `13:41` ↳@Cosmin — That's basically what the Chad/GigaChad/Wreckless personalities already do with Kills mains objetcives, they anchor PvP hotspots and roam them

**Cosmin** · `13:42` — thats the thing, hotsport, not every location

**Cosmin** · `13:42` — im referying by simply roaming everything

**Cosmin** · `13:43` — when im thinking of hotsports im thinking of dorms or sanatorium

**Chazut** · `13:43` — Noted as suggestion, could be a config

**Chazut** · `13:44` ↳@Ika
Thank you  Especially from someone who's been around since 3.8  
  
You mentioned "few issues here and there" though, I'd love to hear them when you have a sec, even the small ones. Stuff you'd shrug off still helps me

**Cosmin** · `13:47` — basically my suggestions would be like this, make cultists static and some profiles or things to make some pmc seek/ or simply roam the map/ or stationary ganks/sniping

**Cosmin** · `13:47` — if possible the ones looking to snipe, to sometime rotate so they can cover vision around them _(editado)_

**Chazut** · `13:52`
Solid list, noting all of it  
  
Static cultists: yes, toggle planned  
,  
Stationary gank/sniping: a "Camper" personality is on the roadmap for that exact thing  
,  
Rotating while watching: also on the roadmap as "smarter movement", checking corners, scanning the rear  
,  
Map-wide roamer archetype: added to the list  
,  
  
Thanks _(editado)_

**Cosmin** · `14:00` — np , we thank you !

**Cosmin** · `14:00` — you got me in luck, i got the first priest kill after 2 days of hunting them lol

**Cosmin** · `14:07` — one more to go

**Cosmin** · `14:08` — and then woods, that could be even more harder lol

**Chazut** · `14:08` — Good luck

**Cosmin** · `14:13` — thanks!

**Ika** · `14:22` ↳@Chazut
Your roadmap and known issues cover pretty much everything I've ran into and thought about so far.  
  
Other stuff is so small it's not worth talking about (imo) like AI briefly stacking inside each other on a regroup before they move.  
  
My only nitpick so far would be that bots being completely static when regrouping waiting for squad members can make them easy to 1 tap.  
  
I will continue playing and report issues I find.  
  
But really, great job!

**Cosmin** · `14:33` — the only thing i miss now is equal oportunity hater mod, basically usec bots can fight usec bots

**S41elite** · `18:00` ↳@Vendth
did u read the RULES of the discord servers related to this specific topic u are talking about? Because, i remeber there being something to prevent EVERYBODY asking for mods to be release, help with a certain mod conflict with wathever, etc etc  
(i add copy of the rules, if u forgot about them, check #2 and comunity standards)  
Pls, do understand its not a single instance of someone asking for something, its probably in the thousands (xD maybe not so many, but ....some enough to start to be bothersome after it has been answered a couple dozen times), and these people are working for free to give us such amazing mods ;D _(editado)_

**S41elite** · `18:00`
  🖼️ 📎 [`att-04-2026-05-31-S41elite.png`](./assets/att-04-2026-05-31-S41elite.png)

**S41elite** · `18:01` ↳@Vendth
there is a search bar on the top right corner that will come in handy  
I assure u, ur question has almost for sure being already asked a couple dozen times and there are probably threats with the solution _(editado)_

**Chazut** · `18:05` — That's why the SPT subreddit was great (RIP ), people are not used to the search function on Discord (myself included) _(editado)_

**S41elite** · `18:05` — the search bar has saved me at the very least 3 profiles, actually

**S41elite** · `18:05`

**RetroLogic** · `20:11` — Is ORBIT compatible with ABPS or does it use its own spawns?

**Cosmin** · `20:16` — it is compatible

**Cosmin** · `20:17` — probably in the future he could include spawns, so it could fit the mod better in terms of performance

**Cosmin** · `20:17` — idk

**Scootis_McPootis** · `20:49` — ORBIT does nothing with spawns


## 01/06/2026

**ArchaicFink** · `00:34` — How are people playing with Orbit since it's been pulled from Forge?

**Shynd** · `01:41` — They are compiling it themselves. If that's not something one already knows how to do, it's probably best to exercise patience until it is released.

**ms. r3mains** · `02:42` — maybe i just live under a rock but i keep seeing 'Phobos' being mentioned here a bunch and i have no idea what it is _(editado)_

**Chazut** · `03:31` ↳@ms. r3mains
No worries  Phobos is an AI mod by Janky (pre-release, lives in his Discord thread, not on the Hub yet, see ⁠Janky's Emporium of Jank⁠).  
It overhauls bot dispatch with an advection field that spreads bots organically across the map.  
  
ORBIT is built on top of Phobos's foundations (same dispatch + squad movement)

**bedtime** · `05:46`

**die mitze** · `07:08` — i just did a raid with orbit for the first time works very good but it would be nice if pmc squads or solo pmcs would rush to bosses like the goons or map bosses

**Ika** · `07:15` ↳@die mitze — It's planned

**TheSunGod** · `07:16` — Isn't it already a thing? I could have sworn i've seen "boss hunting" as one of the potential PMC objectives.

**Chazut** · `07:19` ↳@TheSunGod — it's on the roadmap  Bots will roam between all the boss spawn locations looking for the kill _(editado)_

**bedtime** · `07:22` — what is orbit exact to do

**bedtime** · `07:22` — i never had any experience bout this mod

**Chazut** · `07:33`
ORBIT is an AI overhaul, it doesn't change how bots fight (SAIN handles that), it changes what bots do when they're not fighting. They pick "main objectives" for the raid (loot a high-value zone, hunt kills in a hot area, complete a quest, or extract). On the way there they roam freely between objectives and opportunistically loot items, containers and corpses. The personality system reuses SAIN's archetypes (Rat / Normal / Chad / etc.) and maps them onto behaviour. Rats hoard and extract early, Chads roam aggressively and stay longer, etc.  
  
GitHub readme has the full breakdown if you want details  
https://github.com/Chazut/ORBIT  
  
Currently on hold for release, I shipped to the Hub before formally asking the authors of the mods ORBIT was built on top of for permission. Pulled it down voluntarily while I sort that out with them.  
  
Feeling pretty guilty and stupid about that one honestly, I was so happy with what I had built that I rushed to share it without thinking it through, pure excitement. I totally get how it must feel to discover a mod published with a big chunk of your own code in it (Phobos, LootingBots), I should have asked first. My bad.

**bedtime** · `07:35` ↳@Chazut

**bedtime** · `07:35` — sounds like really amazing

**Cosmin** · `07:38` — i think i will give up searching for cultists for quest...its really nerve wrecking lol

**Cosmin** · `07:39` — niddle in the haysack

**RetroLogic** · `08:16` — Hi Chazut, do you think it would be possible or even a good idea to have PMC groups stay and guard protect dorms and other high value loot areas? I think currently dorms fights are very rare and often only one pmc or scav. _(editado)_

**Chazut** · `09:00` — That's mostly raid variance. Dorms is already a high-value (and loot) zone so squads have a real shot at rolling it as their main objective, but it's weighted, not guaranteed. Same way some live Customs raids are dorms warzones and others are dead silent there. I'd rather keep that variance than force squads to camp the zone every raid _(editado)_

**bedtime** · `09:02` — @Chazut is this mod compatible with black division btw

**Chazut** · `09:04` ↳@bedtime — yes, and RUAF/UNTAR as well

**Chazut** · `09:05` — there is a toggle to keep their brain from the mod author, or to make them roam with ORBIT

**Chazut** · `09:09` — @RetroLogic If you want something less "live-like" and more action, you can try: spawn PMCs in waves via ABPS (constant pressure throughout the raid), buff the scav spawns, or in SAIN disable everything except Wreckless / Chad / GigaChad so every PMC plays aggressive. Heads up on that last one though, you lose a lot of the diversity, no more sneaky Rats hiding in a corner stripping loot off bodies

**Ozlav** · `09:09` — Just to clarify, the mod is not actually downloadable? Or am I just a bit stupid and not finding the download on the git page?

**Chazut** · `09:10` ↳@Ozlav — Not stupid, it's just not available right now. Pulled it off the Hub voluntarily while I sort out permissions with the authors of the mods ORBIT was built on top of (Phobos and LootingBots). One green light in, waiting on the other. Once that's resolved it'll be back on the Hub (fingers crossed )

**Ozlav** · `09:10` — fingers crossed indeed, this looks pretty promising

**bedtime** · `09:11` ↳@Chazut — hb ISB

**bedtime** · `09:11` — any plans?

**Chazut** · `09:14` — I don't get your message, what's ISB?

**bedtime** · `09:16` — its a mod

**bedtime** · `09:17`
  🖼️ 📎 [`att-05-2026-06-01-bedtime.png`](./assets/att-05-2026-06-01-bedtime.png)
  🖼️ 📎 [`att-06-2026-06-01-bedtime.png`](./assets/att-06-2026-06-01-bedtime.png)
  🖼️ 📎 [`att-07-2026-06-01-bedtime.png`](./assets/att-07-2026-06-01-bedtime.png)
  🖼️ 📎 [`att-08-2026-06-01-bedtime.png`](./assets/att-08-2026-06-01-bedtime.png)

**bedtime** · `09:19` — @Chazut its still beta testing but really awesome mod like bd

**Chazut** · `09:19` — Ah I didn't know about that one, because it's not on the Hub yet. Adding it to my todo list for faction support

**bedtime** · `09:20` — i really do appreciate it

**bedtime** · `09:20`

**LifeBosses (Серёга)** · `10:58` — I think this mod will help me get back into playing Tarkov.  if it comes out, of course))) Let's hope for the best)) _(editado)_

**Zybergeris** · `11:07` ↳@LifeBosses (Серёга) — tired of spt? im having quite long break atm, updating mods but not playing. i guess everything will be bork when i launch it

**LifeBosses (Серёга)** · `11:48` ↳@Zybergeris — same i dont play only update mods and play gamma now)))))))))

**Cosmin** · `19:40` — and to think cultists are not deadly enough and very hard to find on shoreline, on woods is even harder lol

**Ranger4R** · `23:53` ↳@Chazut
Hello Chazut  
I've tried Orbit, so far - so good, the idea and realization is nice, all is working.  
may I ask if you have started the work on those changes or it is on pause until situation becomes clear with the LB's creators?


## 02/06/2026

**Chazut** · `03:50`
Hey  glad you're enjoying it.  
  
Dev is still going, I'm not pausing the work itself, just the Hub release. I've kept fixing rough edges and adding small things (vanilla cultists toggle, same-floor chain-loot bias, exfil filtering, etc.), and the roadmap is still being chipped at.  
  
What I'm waiting on is permission from the LootingBots authors to put ORBIT back on the Hub. If it becomes clear I'll never get a reply, I'd consider republish on GitHub only (MIT allows it, and honestly my mistake is already done, it's in the wild, people have already cloned/forked the repo and are compiling it themselves anyway) or take a shot at rewriting the looting layer from scratch, but that second option would mean a clearly worse experience for a long time: LB has years of polish on the looting brain (corpse looting flow with smart weapon swaps comparing gun + ammo + mods, rig swap with item transfer, magazine/attachment compatibility, etc.) that I just can't reproduce overnight.  
  
I'll be honest, with all the drama and how shitty I felt about how I handled the release, I did consider just dropping the whole thing. But after sitting with it, too much work has gone into this for me to walk away from it. _(editado)_

**Ranger4R** · `04:22` — those small fixes and things you've added, it is on github already in 1st release? or you will add it later?

**Chazut** · `04:25` — It's on github on the dev branch

**Chazut** · `04:26` — Anything sitting on dev is still in testing and hasn't been promoted yet. Once I've validated the changes through enough raids without finding regressions, I merge them into main.

**Scootis_McPootis** · `04:27` — Not sure if you want bug reports, but on Interchange bots that try to exfil to "Hole in Fence" get stuck on the 2nd floor of the mall. Here's an image of where they get stuck at:

**Scootis_McPootis** · `04:27`
  🖼️ 📎 [`att-09-2026-06-02-ScootisMcPooti.png`](./assets/att-09-2026-06-02-ScootisMcPooti.png)
  🖼️ 📎 [`att-10-2026-06-02-ScootisMcPooti.png`](./assets/att-10-2026-06-02-ScootisMcPooti.png)

**Scootis_McPootis** · `04:28` — Haven't tested on the dev branch yet, so this might be fixed.

**Chazut** · `04:29` — Thanks for the report  I just disabled the "backpack" exfils on the dev branch, so bots won't pick it anymore. I'll go through the other conditional exfils more thoroughly later on _(editado)_

**Scootis_McPootis** · `04:31` — I've seen bots extract there, but if they decide on that extract while in at least Goshan they'll get stuck there. I think with SAIN/QB, if they are unable to extract at the first extract they choose, they attempt to leave at the next closest one. Not sure if that is a feature of Orbit.

**Chazut** · `04:33` — For now ORBIT keeps it simple, it picks the nearest eligible exfil and commits to that one. No fallback to the next-closest if the bot can't reach it. I'll rework the exfil logic and add that kind of retry behaviour later on, thanks

**Chazut** · `04:34` — One thing to note: ORBIT only lets bots pick exfils on the opposite side of their spawn, same restriction the player has.

**Chazut** · `04:34` — That's an ORBIT only feature _(editado)_

**Recker** · `04:45` — Still early in testing, but may be related to what’s been discussed, I’ve noticed bots trying to use transition points to exfil, not being successful then just stand there for the rest of the raid not moving _(editado)_

**Chazut** · `04:53` — Oh, thanks for the report, I'll dig into it

**Cosmin** · `07:49` — come on...only one and im done with night raids for a period
  🖼️ 📎 [`att-11-2026-06-02-Cosmin.png`](./assets/att-11-2026-06-02-Cosmin.png)

**Cosmin** · `09:08` — at last..after 3 days lol found the sneaky bastards

**Cosmin** · `09:12` — also the priest suicided himself with a grenade _(editado)_

**Cosmin** · `09:12` — when he saw that his guards died lol _(editado)_

**Chazut** · `12:54` — Hey, quick sanity check, for the lucky few of you running ORBIT  has anyone else actually seen bots walk up to a transit and just stand there? Asking because in theory ORBIT only collects Exfil waypoints, not Transit ones, so transitions shouldn't even be in the bot's pickable list. If you've witnessed it, drop me the map + transit (and/or a screenshot from RaidReview) if possible. Thanks _(editado)_

**Scootis_McPootis** · `12:56` — I'll test this right now

**Fums** · `12:56` — I'll look at compiling and testing tonight. Got another itch to load tarkov again

**Recker** · `15:39` — I'll check again, but yesterday, on Streets, two raids I found a PMC walking up to the Transit to Interchange, arriving there and just staying there, right at the transit point, and not moving again the entire raid, this being the transit near the Crash Site extract, could be a coincidence, but happening twice and that the PMC does not move again, possibly something not working as intended _(editado)_

**Vendth** · `17:28` — I am not using ORBIT but Phobos and I have the same issue in it

**Vendth** · `17:28` — Except is with all extracts and transits

**Vendth** · `17:29` — So maybe the issue is in the phobos part of the code

**Vendth** · `17:29` — or maybe i am dumb

**Shynd** · `17:30` — phobos has extract camping logic, unsure if it differentiates between transits & extracts but if it doesn't that would explain the behavior. they're not trying to extract, they're camping.

**Chazut** · `17:47`
Thanks all for the points  
  
Quick Phobos note (sorry Shynd ): no actual "extract camping" logic, just no logic at all for this, Exfil point is just picked at random like any other points of interest. Bot reaches an exfil, nothing fires, sits there until the wait timer re-picks (like any other POI). And Phobos doesn't collect transit POIs either, so what looks like a "bot on a transit" is probably just a nearby POI that visually overlaps.  
  
That's why I rewrote the whole thing in ORBIT. If a bot is parked at an exfil on ORBIT and not despawning, that's a bug.  
Will keep an eye on this

**Shynd** · `18:00`
i'm not sure i'm clear on how you're correcting me and i am also unclear on how much the two projects overlap. just giving an explanation for what someone could be seeing.  
  
in phobos, there's definitely guarding behavior and extracts are definitely chosen as objectives for being guarded. if that's not a part of orbit then i have no explanation.

**Chazut** · `18:10`
Fair, I was being pedantic and that came across wrong, sorry. You're right on the net behaviour, on Phobos, exfils POI get picked (but randomly), no extract handler fires, squad guards there until the timer expires. Bots camp exfils, just emergently and randomly rather than via dedicated logic.  
  
ORBIT swaps that out, exfils are filtered out of the normal POI pick and only become reachable once the squad has formally decided to extract, and arrival triggers a proper despawn. If a bot is parked on an exfil under ORBIT, that's a bug.  
  
Overlap-wise: ORBIT inherits Phobos's dispatch core but the action layer (loot, extract, main objectives) was rewritten. _(editado)_

**Chazut** · `18:13` — And Phobos doesn't really have dedicated camping logic, it just falls out of "exfil picked + wait", like any other POI. And transits aren't pickable either, so a bot "on a transit" is likely visual overlap with a nearby POI. _(editado)_

**Recker** · `19:46`
  🖼️ 📎 [`att-12-2026-06-02-Recker.png`](./assets/att-12-2026-06-02-Recker.png)

**Recker** · `19:48` — at the vehicle extract  with Dynamic Maps enabled to see all bots and kills, check the three Transit points, bots sitting on each

**Cosmin** · `20:13` — actually, its more interesting cultists and bloodhounds to roam the map, would make more sense, let it be a option in case someone preffers to let them roam.Think il make them spawn to 100% on every big map on night time lol

**Lemoireal** · `21:00` — I try to copy the recommended personality setting,but there's no set for normal personality,so i supposed it mean to be set to 0?
  🖼️ 📎 [`att-13-2026-06-03-Lemoireal.png`](./assets/att-13-2026-06-03-Lemoireal.png)


## 03/06/2026

**Chazut** · `03:17` ↳@Recker — Sent you a DM, thanks for the investigation!

**Chazut** · `03:19` ↳@Lemoireal — Normal is the default, anything that doesn't roll one of the listed personalities defaults to it. No need to set Normal to anything

**Chazut** · `03:30` — FYI, since I doubt I'll ever get a response from LB (even a "no"...), I'm currently trying a Plan B to get back on the Hub. I hope I'll make it
  🖼️ 📎 [`att-14-2026-06-03-Chazut.png`](./assets/att-14-2026-06-03-Chazut.png)

**Chazut** · `03:33`
And this isn't a "rewrite to disguise LB code" thing  
It's a from-scratch looting layer built on top of BSG vanilla APIs. Aiming for something simpler than LB, with fewer features at first but a couple of new ones planned for the medium term.

**harmony 👾** · `03:36` — lurk

**duckhead** · `04:24` ↳@Chazut — stoked to see the results!  keep us posted and thanks for your work!

**Vendth** · `05:20` ↳@Chazut — What do you mean? You got a green frog as a response, seems like a very fair and balanced response to me

**Chazut** · `05:22`
Haha yeah, the frog reaction is the closest thing to a verdict I've had so far  
I'll take it as "we're watching" rather than a yes or no  
Thanks all for the support, will keep the thread posted on how Plan B goes

**Zybergeris** · `09:38` ↳@Chazut — Shame that its hard to even say no, just to clear things out

**Zybergeris** · `09:40`
especially when ure not asshole and trying to prove that you made a mistake.  
Just good luck with your project, many people are waiting for it!!

**Chazut** · `09:43` ↳@Zybergeris — Thanks man  yeah I tried to be respectful and transparent the whole way through, but silence is an answer too. No hard feelings on my side, I'd rather spend that energy on Plan B anyway. Appreciate the support, hoping to have something for everyone soon!

**Chazut** · `09:43` ↳@Zybergeris
No need to throw shade at them, we don't know what's happening behind the scenes. Maybe Arch is waiting on Skwizzy's call and just isn't getting one. Maybe they're not confident a mod this ambitious can be maintained long-term by someone new. I genuinely don't know.  
  
But it's fine. I know I'm motivated, and either way the LB removal is well underway, my bots are looting again, the MVP is almost done. Next up: a solid testing phase, then a few new looting features on top, and ORBIT will be officially back

**Zybergeris** · `09:58` ↳@Chazut — As longs as there is no hard feelings its good. You're right

**Fums** · `12:51` — Good to hear you still want the wheels turning on this. Just please don't burn yourself out, you have a potential gem here that could be one of the "required" SPT mods like SAIN and APBS _(editado)_

**Fums** · `12:51`
no pressure  
Spoiler

**Ika** · `14:11` ↳@Fums — Totally agree. I describe it as the glue that SPT has needed to bring AI mods together so you actually get to experience the full extent of modded AI.

**die mitze** · `14:14` — i got roaming goons on but there not roaming around is that a known bug or is there a requirment for them to start roaming also sorry if the screenshot is too bright i got HDR on
  🖼️ 📎 [`att-15-2026-06-03-diemitze.png`](./assets/att-15-2026-06-03-diemitze.png)
  🖼️ 📎 [`att-16-2026-06-03-diemitze.png`](./assets/att-16-2026-06-03-diemitze.png)

**Chazut** · `14:34` ↳@die mitze — That's weird, I haven't run into that one. Could be a conflict with another mod or weird setup/config. Can you try with only ORBIT + SAIN + BigBrain installed and see if they roam? If they do, we'll narrow it down to whatever you remove from your current setup. No worries about the screenshots

**die mitze** · `14:35` ↳@Chazut — will do the only AI mods i got is looting bots ORBIT SAIN btw

**Chazut** · `14:35` — remove looting bot

**Chazut** · `14:38` — Also worth checking: there's an "AI limit" setting in ABPS (or in SPT config, can't remember) that shuts off bots when they are too far from the player to save CPU. That's almost certainly your culprit, try turning it off and see if the Goons start roaming.

**die mitze** · `14:39` — i dont got that on i never use that

**duckhead** · `14:40` ↳@Chazut — wasn't that a feature of SAIN?

**Chazut** · `14:40` — I can't remember where that is

**die mitze** · `14:41` — sain limits AI vs AI but there is a mod adding AI limit so when there far there frozen

**die mitze** · `14:41` — pmcs are romaing its just the goons but i will test without looting bots next raid

**Chazut** · `14:44` — Yeah LB is incompatible with ORBIT, they'll fight each other for control. The reason is ORBIT originally integrated a chunk of LB's code for the looting logic. I'm in the middle of ripping that out and replacing it with a from-scratch looting system tailored to ORBIT, and it also means I'll finally be able to release ORBIT on the Hub. For now though, definitely test without LB installed.

**die mitze** · `14:44` — ah layer fight huh?

**Ika** · `14:52` ↳@die mitze — You can turn AI limits off in SAIN but I would not recommend that to anyone not running a headless because of the performance cost. Even then AI vs AI will still never be as perceptive vs a player in my experience.

**die mitze** · `14:53` ↳@Ika — but dosnt it just make AI vs AI fights a bit slower or soemthing so it dosnt kill youir CPU?

**Fums** · `14:54` — Yeah they are saying to not turn it off. Which means keep ai limit combat working _(editado)_

**Fums** · `14:55` — It just stops them from using extreme cover calculations and I think nades

**Fums** · `14:55` — Which is fine because you either walk to a corpse or.. walk to a corpse so it's not really needed at all to disable.

**Ika** · `14:55` — There are also AI vs AI vision and hearing limits which are on by default.

**Fums** · `14:56` — Yeah I would personally keep all of these limits on

**Fums** · `14:56` — Will stop bots from aggroing eachother through the wall and what not from range

**Fums** · `14:56` — That kind of thing

**Ika** · `14:56` — Even if you can run it without those limits on, you're gonna end up with bots dead faster makes the raids more dead in my experience

**Archangelway** · `14:56` ↳@Chazut — I've reached my verdict

**Archangelway** · `14:56`

**Archangelway** · `14:56`
I thought about it for a bit and read through some of the code, and I don't think I'm happy how this mod (re)implements Looting Bots (mostly with AI having heavily re-done the source files) at the moment. Though that being said I'm also giving my opinion here as a maintainer of Looting Bots and not as it's original publisher.  
  
Community wise I would be fine if the mod is re-published to the forge because of all of the random builds floating around already, but I would ideally like to see Looting Bots's code being entirely removed in a couple of months and replaced with something that isn't based off of it. Should that not be the case and should it still contain Looting Bots related code after the release of SPT 4.1, I will probably notify forge staff to request unpublishing of ORBIT due to the lack of permission for that code.  
  
In the end there's no hard feeling from me towards the mod, I hope your mod does well and creates something cool with Phobos under the hood

**Archangelway** · `14:57` — Now hopefully I can go back to doing other stuff without people nagging me

**Chazut** · `15:06`
Thanks for taking the time to actually look at it and reply  
  
Good news on my side: the full LB removal is already done. I've been working on it intensively for the past few days, completely from-scratch looting layer built on top of BSG vanilla APIs, no LB code left in the project. I even deleted my local LB clone and turned off my AI assistant's GitHub repo search tool to make sure none of it could leak in. Currently in heavy testing mode and aiming to push the LB-free build to the Hub within the next few days.  
  
So we're already on the same page, well before the SPT 4.1 deadline  
  
No hard feelings on my side either, and I genuinely appreciate that you took the time to share your verdict instead of leaving it open-ended. Thanks again

**Chazut** · `15:25` ↳@Archangelway
Quick follow-up if you don't mind, wanted to clarify where you draw the line, because I want to make sure we're on the same page.  
  
The way I see it, BSG vanilla only provides the low-level action bricks: open/close container, transfer item, find a place to put it, run a network-safe move. Any bot loot system, LB or anything else, has to call into those same gates, there's literally no alternative path BSG exposes.  
  
LB's real value-add was the decision layer above that: choosing which item to loot and when (built around the bot's vision, reactions to nearby items, etc.), distance/value gating, pathing the bot there, inventory heuristics like weapon/armor swaps, mag compatibility, etc. ORBIT works completely differently and always has, even at the first release: it's deterministic, driven by world-indexed POI and squad-level dispatch, not by per-bot vision. On my current work (not pushed yet) I wiped all LB stuff and started over with my own ideas, different architecture, different logic, different approaches, + a bunch of ORBIT-specific concepts LB doesn't have at all.  
  
So if "based off LB" means using the BSG action bricks --> unavoidable, every implementation does.  
If it means reusing LB's decision logic, any algo or any code --> zero of that in the new ORBIT.  
  
Just want to confirm we're aligned _(editado)_

**Chazut** · `15:30`
Quick update for everyone following the thread: after Arch's reply, I'd fully finish my own from-scratch looting system before releasing on the Hub, instead of rushing it out and risking any grey area later. Better safe than sorry, and honestly the result will be cleaner this way.  
  
Heavy testing continues, and ORBIT will be back on the Hub as soon as it’s truly stable

**Archangelway** · `15:32` ↳@Chazut
I cant bar anyone from using bsg’s implementations because then I would have control over everyone attempting a similar thing, so long as the code is sufficiently different and not Phobos + LB slapped on top I’m fine with it  
  
I understand that in the end some of the same methods will be called or raycasts will be done similarly and I’m not too bothered about that

**Chazut** · `15:35` ↳@Archangelway
Perfect, that's the answer I needed, totally aligned then  
Thanks again for taking the time to reply and clear this up, I know you've got better things to do than deal with this. And honestly, sorry for not handling things the right way from the start, should have been more careful with the LB code in the first release. Won't happen again

**Shynd** · `15:57` — this has been nice to observe

**harmony 👾** · `15:57` — yes yes update when /j

**die mitze** · `16:36` — is it normal that every raid is dead quite i barly find or see PMCs only scavs and lil to no gun fire

**Fums** · `16:41` — There has not been a single banned user from what I know with any of this has there? Surprising, usually you have Mr Entitled coming along

**Fums** · `16:42` ↳@die mitze — Install raid review and see what's happening and report here. I suspect bots are camping at transits and exfils.

**Chazut** · `16:49` ↳@Fums — No, that's not actually what was reported. The recent bug report was a single bot stuck at its spawn point, vanilla SPT issue where the spawn lands the bot on an isolated chunk of navmesh disconnected from the rest of the map. Hence why he appeared to be "at the transit", the spawn just happens to be next to it on Streets. Same thing happens vanilla with bots spawning inside the silo on Factory, they can't get out. Not a transit/exfil camping issue, just a vanilla spawn quirk that ORBIT will mitigate next patch (by teleporting the stucked bot to a valid location) _(editado)_

**Chazut** · `16:52` ↳@die mitze
Yeah RR is the best way to see what's actually going on, drop a raid in there and you'll see exactly what every bot was doing, where they went and why, etc.  
  
That said, ORBIT's goal is to feel like live EFT, not to turn SPT into an endless gunfight in Dorms. So some raids being quieter is expected and realistic, in live you can absolutely have raids where you barely see another player and other where it's a bloodbath. It's called raid variance and is part of the experience

**die mitze** · `16:53` ↳@Chazut — i get that i m just used to phobos 1 raid nothing 1 a few pmcs 1 raid none stop action next 2 just looitng barly any gun fight becuse the goons wiped half the raid _(editado)_

**Chazut** · `16:55` — Phobos was tuned for chaos, ORBIT is tuned for believable squad behaviour. If you want more constant action, crank up your spawn density and tilt your SAIN personality distribution toward Chad / Wreckless / GigaChad only, those archetypes actively hunt for kills with ORBIT, so you'll see way more PMC-on-PMC fights and less Rat loot goblins PMCs

**die mitze** · `16:57` ↳@Chazut — hm alright i m starting to understand i like the way the mod goes

**Chazut** · `17:04` ↳@Chazut — Also explains why this happens more in ORBIT than vanilla: vanilla SPT has a built-in TP rescue that fires when a bot is stuck, but I had to disable it because it was teleporting bots constantly on every unreachable loot (same issue Phobos had with LB). The fix I'm shipping is a much lighter version that will only fires in real glitch cases

**Chazut** · `17:05` — and maybe it will be fixed by itself with the version of ORBIT without LB

**Recker** · `17:16` ↳@die mitze — default settings in Orbit with ABPS handling spawning, this was a nuts raid gunfire everywhere
  🖼️ 📎 [`att-17-2026-06-03-diemitze.png`](./assets/att-17-2026-06-03-diemitze.png)

**die mitze** · `17:17` ↳@Recker — hm my raids are quite i gotta change some settings

**Recker** · `17:25` — and the raid before this, same setup, was half as busy, feels good, the variety

**Cosmin** · `17:44` ↳@Recker — how was your fps doing?

**Recker** · `17:46` — looking for some if you have any for sale?

**Recker** · `17:47` — Lossless Scaling does some heavy lifting on Streets

**Vendth** · `17:51` — Yeah in my experience every dead raid I've had with phobos has been one where two or three squads had a massive fight at the beginning and killed each other

**Cosmin** · `17:59` — lossless scaling could kill you in gunfights

**Cosmin** · `18:00` — everything runs ok but those are fake fps and it runs a little late

**Cosmin** · `18:00` — if i remember it gives you fps but also lag

**Cosmin** · `18:01` — the difference s depending on the settings

**Recker** · `18:27` — LLS has come a long way, I did experience the lag you speaking of till about a year or so ago, the updates to LLS removed the lag or made it a lot less noticeable in gunfights

**Chazut** · `18:43`

**Cosmin** · `19:19` ↳@Recker — then from what it gets fps, it must take something in return

**Cosmin** · `19:19` — from where it gives

**Cosmin** · `19:20` — also please give me your setings to lss

**Scootis_McPootis** · `19:45` ↳@Cosmin — ⁠community-support⁠Lossless Scaling Quick Guide

**Recker** · `19:51` — yeah those are the settings

**Cosmin** · `19:58` — thanks

**Phoenix** · `20:27` ↳@Chazut — Very Interested in this

**Baconism** · `22:16` ↳@Chazut — thank god I have tomorrow off


## 04/06/2026

**die mitze** · `01:12` ↳@Chazut — Will there be a option to toggle extract and transfer camping?

**ms. r3mains** · `01:33` ↳@Cosmin — yeah i couldnt ever get it to a low enough latency id be comfortable with

**ms. r3mains** · `01:33` — i only really run it when im using a controller for whatever reason since it isnt as noticeable _(editado)_

**Chazut** · `02:49` ↳@die mitze — There is no "camping" feature yet

**Chazut** · `02:56` — but the fix that prevents bots from getting stuck on isolated navmeshes at spawn won't be in yet (2 street spawns next to the transits) _(editado)_

**TheSunGod** · `03:06`
Hey, does ORBIT do anything related to spawning bots? As in, touch/move their spawn points, or delay the spawns?  
  
I don't think so, but im running into a weird issue - first bots spawning with a delay, solid 20-30 seconds after the players, and bosses spawning pretty far away from their actual areas (for example Kaban and his crew spawn by the semi-truck outside the south gate, and only run inside of Lexos after).  
  
The only mod handling spawns i have, at least to my knowledge, is ABPS.

**TheSunGod** · `03:07` — I know QuestingBots had its own built-in spawn system that was automatically disabled if it detected ABPS, so im thinking maybe ORBIT has something like that too, that does not fully disable and messes things up.... _(editado)_

**TheSunGod** · `03:08` — most likely not, but the issue is weird

**Chazut** · `03:26`
Nope, ORBIT doesn't touch spawning at all, no spawn-point moves, no spawn delays. It only kicks in after a bot has spawned (registers the bot, assigns an objective, takes over movement from that point on).  
So your issue is not coming from ORBIT. Worth checking your ABPS config  
If you want to confirm, easiest test is to run a raid with only ORBIT + SAIN + BigBrain installed and see if the behaviour goes away _(editado)_

**Chazut** · `04:10`

**7Bpencil** · `04:14` — _(editado)_
  🖼️ 📎 [`att-18-2026-06-04-7Bpencil.png`](./assets/att-18-2026-06-04-7Bpencil.png)

**Enigma.** · `04:14`

**Chazut** · `04:18`
  🖼️ 📎 [`att-19-2026-06-04-Chazut.png`](./assets/att-19-2026-06-04-Chazut.png)

**Chazut** · `04:19` — BigBrain and Waypoints ARE dependencies of SAIN, but since I can't add SAIN

**7Bpencil** · `04:24` — bruh

**harmony 👾** · `04:32` ↳@Chazut — How come you can't

**harmony 👾** · `04:33` — Tried without '~'?

**Chazut** · `04:34` — Still not
  🖼️ 📎 [`att-20-2026-06-04-Chazut.png`](./assets/att-20-2026-06-04-Chazut.png)

**Chazut** · `04:35` — I've raised the bug, just have to wait for the fix

**Chazut** · `04:39` — No worries, SAIN is hardcoded as a dependency at the BepInEx level, if SAIN isn't installed, ORBIT just won't load (BepInEx skips it cleanly). So even without the Hub-level dependency button, anyone trying to run ORBIT without SAIN will see it silently disabled rather than crash.

**Chazut** · `04:58`
Now that the release is out, really curious to hear what you all think  
Drop anything weird (or anything you love) in the thread, every report helps

**MauwMa** · `05:19`
Hello!  
Reading the description the mod looks very ambitions, I'm looking forward to try it out!  
My only question is that because this is built on phobos and I heard it has a feature to keep AIs near to you for more action,  
does ORBIT has the same feature or not?

**Chazut** · `05:22` ↳@MauwMa
Hey thanks  
No, ORBIT doesn't pull bots toward you, that is Phobos thing I dropped because I felt it artificial. Squads here roam based on their own objectives (loot zones, PvP hotspots, quest triggers), not the player's position. _(editado)_

**Chazut** · `05:24` — That said, if a bunch of you ask for it back, I can re-add it as an optional toggle in the F12 config. Just let me know

**MauwMa** · `05:24` — Awesome I feel the same way.

**Chazut** · `05:54` — For anyone who had ORBIT installed from the first initial release (before the drama): I bumped the opportunistic corpse scan interval default from 0.5s → 2.5s for better perf (I also reworked it so it only check if LoS when there's actually a corpse in range, instead of running every tick regardless). Your old config still has 0.5s though, F12 and look for "Opportunistic corpse scan interval" to bump it manually, or just delete BepInEx/config/com.chazut.orbit.cfg to start fresh on the new defaults _(editado)_

**RetroLogic** · `06:02` ↳@Chazut — I think the toggle would be a great idea, would make those Quests like provide viewership, kill 15 scavs in an area much more manageable. Great work Chazut.

**Chazut** · `06:05` ↳@RetroLogic — Noted, will add it to the roadmap  thanks for the feedback!

**Cosmin** · `06:06` — would there be any possible way to make in the future pmc inside houses check windows for some time?

**Cosmin** · `06:06` — like hiding in a house and snipe from windows or balconies

**Cosmin** · `06:07` — or on top of buildings like sanatorium

**Chazut** · `06:09` ↳@Cosmin
Yeah I'll see if I can plan it as a "Camping" objective or a "Camper" personality. Catch is that as soon as SAIN sees an enemy, it takes over and stops the camping behaviour, so it'd mostly be pre-engagement flavour.  
  
Long-term (no promises) I might look at a PR on SAIN's side to keep the camp going through combat, but that's way down the line

**Cosmin** · `06:10` — i see

**Cosmin** · `06:10` — thanks!

**S41elite** · `06:23` ↳@Fums — fontaine's RM needs to be added to that list

**S41elite** · `06:27` ↳@Archangelway — im sorry u got pestered, arch! Much love and thank you for keeping LB alive!

**Archangelway** · `06:54` ↳@Chazut — Try setting it to ~4.4

**Archangelway** · `06:54` — It’s possible one of the many hidden versions are ruining it however

**Chazut** · `06:54` ↳@Archangelway
  🖼️ 📎 [`att-21-2026-06-04-Archangelway.png`](./assets/att-21-2026-06-04-Archangelway.png)

**Archangelway** · `06:56` — Interesting

**Chazut** · `06:56` — I also tried with *

**Archangelway** · `06:57` — Idk sain has a lot of versions that might be messing it up because it was a mod long before Forge was a thing

**Chazut** · `06:59` ↳@Archangelway — Damn ok, so no fix possible on the website side then? I'm afraid I'll have reports like "ORBIT isn't showing in F12 / not working" because new users didn't have SAIN _(editado)_

**Chazut** · `07:03` — SAIN is so popular, it should just ship with SPT at this point

**Cosmin** · `07:50` — true

**Cosmin** · `07:50` — but so should orbit sooner or later

**Cosmin** · `07:50` — !

**Vendth** · `07:58` — I mean you specified it in the mod page and of course everyone always reads them so it's fine

**Vendth** · `08:03` — Congrats for the release by the way, looking forward to try it

**bedtime** · `08:15` — any recommend setup with SAIN btw

**Recker** · `08:17` — Yeah on Oribit's page on the Forge, check the tab "Personalities  (Recommended Sain Config)"

**Vendth** · `08:41` — Do you have recommended sain setups for stuff like hearing/vision/etc sliders? Or that won't affect the mod at all?

**Vendth** · `08:41` — I am worried that lowering the hearing distance may clash with your bot behavior

**MaxP0wers** · `08:51` ↳@RetroLogic — I second this. After trying Phobos I liked it more than I thought.

**Vendth** · `08:51` ↳@Vendth — for no reason I should add

**Chazut** · `08:53` — Nope, no recommended SAIN combat sliders, ORBIT only handles objectives, dispatch and looting, all the hearing/vision/aim is 100% SAIN's domain. Tweak those however you like, it won't clash with ORBIT

**Baconism** · `09:43` — So far Orbit is working flawlessly, feels like Phobos raids with the filled backpacks of LootingBots and I couldnt ask for more than that

**Baconism** · `09:44` — cant wait to see the further progression of this mod

**Zybergeris** · `09:52` ↳@Chazut — Grab it before someone nukes it again

**Baconism** · `09:52` — it should be fine _(editado)_

**Cosmin** · `09:55` ↳@Zybergeris — meny jelly modders. instead of supporting each other mods they try and bring some of them down for their ego _(editado)_

**Chazut** · `09:57` ↳@Cosmin — Honestly the SPT modding scene is mostly really supportive, and that's what keeps me going. Let's focus on the cool stuff being built rather than the drama

**Cosmin** · `09:57` — agree

**Zybergeris** · `10:08` — @Chazut you bastard. Congrats! ORbit release is final nail to my coming back to spt again. Gonna be 8hours of modding / testing and 1 hours of actual playing . Missed this life loopn

**Chazut** · `10:12` — Welcome back to the rabbit hole my friend  enjoy the 8h of modding, that's half the fun

**Cosmin** · `10:29` — idk if this is the correct chat, but im waiting some players oppinions, so first the first time with spt whenever a boss is present on a map , even customs, i feel that fps is lower, should i deactivate sain for all bosses ? _(editado)_

**die mitze** · `10:38` ↳@Cosmin — Lower bot amount more bots= lower fps

**Cosmin** · `10:39` — but its not the problem, because even with that number of bots or lower number it does not affect my fps, i have checked number of bots and everything related to apbs, its just bosses sucks more fps out

**Cosmin** · `10:39` — so im thinking deactivating sain for bosses and guards

**Cosmin** · `10:39` — im sure  i nthat case they wont affect much like that

**Chazut** · `10:40` — Pretty sure the same thing will happen in vanilla _(editado)_

**Cosmin** · `10:41` — last year in spt i was having low fps on reserve, the same map this year i ramped up numbar of bots. Scavs where like 20 on the same time, no fps issue this time

**Cosmin** · `10:42` — but if lets say i have low number of bots, with bosses active, fps is bad

**Cosmin** · `10:42` — so im pretty sure bosses and followers affects fps because at night, the same normal number plus cultists does not lower fps

**Cosmin** · `10:43` — so im sure the number of bots is not the problem, but the bosses and their followers themselvs

**Chazut** · `10:45` — That's worth testing, try disabling SAIN for bosses + guards in SAIN's F12 (per-faction toggles) and see if your FPS recovers. If yes, you've narrowed it down, otherwise it's just vanilla SPT. Either way, not an ORBIT thing (ORBIT doesn't touch boss logic)

**Lemoireal** · `10:48` — Last time i disabled all sain bot beside pmc I don't see any fps improvement

**Baconism** · `10:49` — what can really make fps better with SAIN is disabling bot v bot SAIN handling

**Baconism** · `10:49` — Alot of bots fighting at the same time accross the map with SAIN logic can really tank fps from what ive encountered

**Cosmin** · `10:50` — hmm thats a good ideea

**Baconism** · `10:51`
  🖼️ 📎 [`att-22-2026-06-04-Baconism.png`](./assets/att-22-2026-06-04-Baconism.png)

**Cosmin** · `10:51` — i have them on already

**Cosmin** · `10:51` — but its working of when they fight

**Cosmin** · `10:52` — in the worst case scenario is bsg fault with bosses

**Baconism** · `10:52` — probably

**Cosmin** · `10:53` — i would turn performance on, but im afraid bots will be dumbed down to much when finding cover

**Chazut** · `11:09` ↳@Cosmin — That wouldn't surprise me

**Ika** · `11:10` — Biggest FPS improvements you can get is using a headless and if you're fortunate enough, putting it on a second PC, that's where the crazy gains are.

**ValFin** · `11:17` — Currently, Orbit version 1.0.0 is not compatible with ABPS. Enemies become stupid.

**Ika** · `11:20` ↳@ValFin — Haven't experienced this at all. What do you mean by stupid?

**Chazut** · `11:24` ↳@ValFin — "Stupid" how? in combat or out of combat? Need more to go on, a clip, anything. ABPS only touches spawns afaik, shouldn't interact with bot brains. You're the only one reporting this so far, probably something else on your side. Do you have other AI mods installed (LB, QB, Phobos)? because any other AI mod is incompatible (except SAIN that is required)

**ValFin** · `11:25` — Currently, there is an issue where bots completely freeze after 1 minute during combat and cannot be aimed properly, so we are conducting tests by changing the modes one by one.

**ValFin** · `11:26` — Bots keep their eyes fixed on the spot they have targeted and only fire when it comes into their line of sight.

**ValFin** · `11:27` — I confirmed that the freezing issue disappeared even after 1 minute when running with ABPS mode disabled. Further verification is required.

**Chazut** · `11:40` — That kind of freeze + locked-aim behaviour isn't something ORBIT can cause, it doesn't touch bot aiming, vision, or combat reactions, that's all SAIN / vanilla. Best bet is the general SPT support thread, more eyes there for mod interaction issues.

**Cosmin** · `11:42` — ok so i think i have won some fps

**Cosmin** · `11:42` — and i had the goons present on map

**Cosmin** · `11:43` — also total bots in customs was 50

**Cosmin** · `11:43` — most dead probably

**Cosmin** · `11:44` — peak active bots 20

**Cosmin** · `11:45` — the fps felt ok, better than last time

**Cosmin** · `11:49` — unless goons are different from reshala

**Cosmin** · `11:49` — idk

**Chazut** · `11:49` — goons are controlled by ORBIT per default

**Klinical** · `11:49`
thanks for the mod chazut!  
would you like some headless bug reports from me for the future? I run heavy modded setups all time.

**Chazut** · `11:50` — Boss are never

**Cosmin** · `11:50` — goons are unchecked in my setings

**Cosmin** · `11:50` — i hate them roaming, you cant be always prepared for goons non stop lol

**Chazut** · `11:50` ↳@Klinical — Yes sure! I will try to manage them without installing Fika

**Klinical** · `11:51` — oh while you are here did you change anything from when you ripped the mod off the site before? im still running your previous one

**Chazut** · `11:51` ↳@Klinical — yes a lot

**Filipe** · `11:52` — how can we control the spawn chance of all bosses??

**Chazut** · `11:52` ↳@Filipe — With ABPS mod

**Shynd** · `11:52` ↳@Chazut
IMO be careful saying things like "isn't something orbit can cause" since there are many, many ways these things are all interconnected. i've seen mods be the cause of some weird shit they should never be able to affect.  
  
that said, my gut is that someone has despawning active on ABPS & that interaction is causing problems. but that's just a guess.

**Filipe** · `11:53` ↳@Chazut — i unisntalled bc i tought it was incompatible with orbit

**Klinical** · `11:53`
When i used the very first build of orbit i noticed some bots would stand still a long time but they were just set in patrol mode for a long time.  
but they were not broken _(editado)_

**Chazut** · `11:54` ↳@Filipe
No it is compatible (and recommended) you can reinstall  
I will clarify on the modpage, peoples get confused _(editado)_

**Chazut** · `11:54` ↳@Klinical — This should be fixed in the last version

**Filipe** · `11:56` — i looked in the modpage, and there says "Any other “AI overhaul” mod", so i assumed its safer to not have abps, either way, tnx dude

**Chazut** · `11:57` ↳@Filipe — Yeah, this is what I need to clarify, ABPS is not an AI overhaul   (= control the spawn of bot, not the behavior)

**Chazut** · `11:58` ↳@Shynd — Fair point, never say never   Still feels unlikely on my end though, for it to freeze a bot's aim there'd have to be a pretty indirect chain. Worth keeping in mind if more reports come in _(editado)_

**Shynd** · `12:00` ↳@Chazut — i've seen mods like looting bots (in times past) have an interaction with another mod & break a bot's brain layer. users report that SAIN broke because no combat, when in reality the bot was just brainless. things like that.

**Cosmin** · `12:02` — should i buy or download free the loseless scaling soft ?

**Cosmin** · `12:02` — cuz i downloaded one frum the hub but it detected trojan and things i dont want to mess with

**Cosmin** · `12:02` — so i deleted it

**Chazut** · `12:05` ↳@Filipe — Fixed
  🖼️ 📎 [`att-23-2026-06-04-Filipe.png`](./assets/att-23-2026-06-04-Filipe.png)

**Klinical** · `12:05`
May you expose more of the config options for looting?  
I change a setting inside with ai (i know im dirty) for the rate a bot picks an item up so i can fill the backpack faster.

**Chazut** · `12:06` ↳@Shynd — Yeah, true. I'll keep an open mind on weird reports then. Thanks

**Shynd** · `12:07` — just friendly suggestions, that's all. like a few days ago i saw claims that raid review could never cause increased searching for containers, when it absolutely 100% could (and does, in my experience) if run over the internet via Fika or on multiple clients all at the same time. which obviously on the mod page you suggest against, but these are users afterall

**Chazut** · `12:07` ↳@Cosmin — Wait, you downloaded a cracked Lossless Scaling? That's not a thing here, buy it

**Klinical** · `12:09` — I cant believe I have to say this but if you are downloading a program for free that costs money you are being served a virus 99% of the time please dont steal things like come on guys.

**harmony 👾** · `12:09` — Won't help if you have a bottleneck CPU

**harmony 👾** · `12:09`

**Chazut** · `12:10` ↳@Klinical — Yes, I can expose more loot knobs in the next patch, added to my list _(editado)_

**Klinical** · `12:11`
i'm off to test your mod ill report back anything i find. thanks again for hearing me out homie  
(if you want me to test anything specific with fika just dm me or ping me) _(editado)_

**Chazut** · `12:11` ↳@Shynd — Yeah I don't have experience with Fika, so multi-client edge cases like that are blind spots for me. I'll be more careful with how I word things going forward. Thanks for the advices

**Shynd** · `12:13` — people take your word as truth

**Chazut** · `12:31` — did you guys consider interchange as a "close quarters map" or "balanced" ? With what type of weapons do you go in ?

**Chazut** · `12:32` — and did you consider Shoreline as a "long range map" or "balanced" ? I can't choose

**Chazut** · `12:32` — (building the weapon swap scoring system)

**Klinical** · `12:34`
interchange id say medium-long but short-long inside.  
Shoreline id consider a medium map since most fights are within like 40m.  
  
also just ran a labs raid and this is spammed over and over after a bot scores a kill.  
[Info   :     ORBIT] F37297: RequestNear: Squad(id: 1) bee-lining to own-kill corpse Waypoint(1000287, Corpse, Bot8) (in cell (3, 0))  this is spammed to the point of megabytes in log files.  
Running in the headless currently only for the mod. _(editado)_

**Chazut** · `12:39`
Good catch, real bug, thanks for flagging. It's harmless but will fix it in the next update.  
Sorry about the log file size in the meantime, you might have to nuke it once in a while _(editado)_

**Klinical** · `12:40` — id feel bad for the poor guys submitting those log files without nitro

**Chazut** · `12:43`
Lmaoo and that's the Release build, Debug logs are tens/hundreds of thousands of lines per raid  
thank god AI can chew through that for analysis and dashboards

**Shynd** · `12:46` ↳@Chazut — is there an ability to set behavior archetype based on weapon type? because i could totally see someone spawning on shoreline with an mp7 with the intent of engaging in CQB in resort. same with interchange. and i also could see someone spawning with a mosin on either map and doing cache runs and staying away from CQB.

**Shynd** · `12:46` — it'd have to be something like SPT/APBS generates weapon -> ORBIT sees weapon & assigns behavior archetype -> ORBIT instructs objectives that correspond with behavior archetype (mall, resort) _(editado)_

**Shynd** · `12:48`
pistol/smg/shotgun/AR without scope -> CQB  
bolt action/DMR/AR with scope -> range

**Klinical** · `12:49` — Oh i had some idea on bot looting personalities. A timmy would be more likely to miss the sound queue (of an enemy creeping up) while stuffed inside a dead players pockets. Or a chad would quickly decide to loot a secured body and disengage if the value of himself is over the limit to extract in the event of a stalemate or loosing fight. Adding some variety in how the bot interacts mid battle i feel would set you apart from other mods. Thats just crazy ideas i have though.

**Chazut** · `12:50` ↳@Shynd
Not currently, archetype is locked at spawn from SAIN personality. But that's a cool angle, a weapon-aware behaviour would actually fit well on top. Adding it to my notes  
  
I think APBS already biases loadouts toward map-appropriate weapons though, not sure if it also check SAIN brain since SAIN gets attached after spawn.  
  
Right now objectives don't bias by archetype directly (any squad can pick any POI), but with a "CQB vs range" tag I could weight POI selection (open POIs penalised for the MP7 squad, dense interiors penalised for the Mosin squad). Solid idea, keeping it on the list _(editado)_

**Klinical** · `12:52` — im curious im very new at this is there a reason you would filter that with gun types instead of calibers? I'm just genuinely curious.

**Shynd** · `12:54` — an SA-58 and an SR-25 have very different use cases (generally) _(editado)_

**Klinical** · `12:55` — oh its too broad i didnt even consider that

**Chazut** · `12:55` ↳@Klinical — Worth noting chads ignore low-value loot (they only stop for big stuff), and ORBIT doesn't touch the in-combat logic itself, that's SAIN's layer, I just pick the objectives

**Chazut** · `12:55` — So I couldn't really do the Timmy audio cue thing either, that'd have to live on SAIN's side

**Klinical** · `12:56` — the chad thing would be possible maybe?. new obtective to loot body that takes priority over combat? like emergency disengage but hes a filthy rat? _(editado)_

**Chazut** · `12:57` ↳@Klinical — Yeah Shynd nailed it, caliber alone doesn't tell you the playstyle. Weapon class + optic (or lack of) is closer to how the bot would actually fight

**Klinical** · `12:57` — I love learning this stuff. I'm trying to learn enough to publish my own mod.

**Shynd** · `12:58` — could probably even just filter by MOA

**Chazut** · `12:59` ↳@Klinical — Yeah the extract part's already there, it'd just be adding the "snatch the body on the way out" twist. Doable as an objective layer, that's my playground

**Chazut** · `13:00` ↳@Shynd — Lmao that's actually a clean shortcut! Way cleaner than a hardcoded weapon-class table

**Shynd** · `13:00`
<=2.5 MOA range objectives can be applied (>=650 RPM -> CQB can also be chosen randomly)  
>2.5 MOA CQB objectives can be applied (scope existing -> ranged can also be chosen randomly)

**Klinical** · `13:00` — this is why you are paid the big bucks shynd

**Klinical** · `13:00` — xd

**Shynd** · `13:00` — so many big bucks

**Klinical** · `13:01` — maybe the real big bucks was the friends you made along the way _(editado)_

**Baconism** · `13:01` ↳@ValFin — I use ABPS and they work fine

**Chazut** · `13:01` ↳@Shynd — Solid first-pass thresholds, the crossover rules (high RPM precise gun still viable in CQB, scoped imprecise gun still viable at range) make it more organic too. Saving this!!

**Shynd** · `13:02`
honestly the reward for my time spent is some combination of  
  
making the projects i love be the best they can be if i can help in any way  
,  
make sure the greatest amount of people get to enjoy one of my favorite games in the definitive way to enjoy it (SPT + mods + friends/co-op)  
,  
  
but that's neither here nor there. either way i'm fuckin ecstatic to be a part of these things.

**Klinical** · `13:03`
im addicted to this project i can get enough myself.  
I would not be surprised if when i can code myself if I wasnt working on spt privately in some way for my friends.  
Aint no way i could be worthy enough to code a line with the gods. jkjk  
(I already know as soon as i can im going to try to add stalker style map transitions to mimic open world. maybe hosting a headless for every map running forever and syncing between each map but I've read the fika issues over the yearson syncing players and that sounds like aids to navigate.) _(editado)_

**LifeBosses (Серёга)** · `13:04` — First thanks for the mod i try play 5 raids on custom with ORBIT I noticed one thing: all the doors are always closed everywhere... do they close the doors behind themselves? ))) when they enter buildings

**Chazut** · `13:04` ↳@Shynd — Same energy on my side too, all the mods I work on came from that exact place. Glad we're all riding the same wave

**LifeBosses (Серёга)** · `13:06` — 5 raid all door on all building close It's as if no one had been there, but in RAID review there were PMCs there before me

**Klinical** · `13:08` ↳@LifeBosses (Серёга) — this was wrong _(editado)_

**Chazut** · `13:08` — Haha they're polite I guess   jokes aside they're not actually closing them. Bots only open a door if they want something behind it (old Phobos trauma here lol). And they roll on locked doors too, don't be surprised one day if you find the Marked Room wide open

**Chazut** · `13:10` — That said, if you ever catch a bot actually closing one, it's definitely not intentional on my side. I won't say it's impossible (Shynd already taught me that lesson today ), but I'll dig into the door system to make sure. Thanks for the report

**Klinical** · `13:10`
did you remove the logic that closed doors? like yellow keycard room a bot would close me in  
Again you already answered it rrahhhh _(editado)_

**Chazut** · `13:13`
Nah I just stripped Phobos's compulsive door-opening habit  
ORBIT only opens one if the bot actually needs to go through or loot behind it (with a roll on locked ones). Never closes (voluntary at least)

**LifeBosses (Серёга)** · `13:13` — I ran across the entire map and they were all closed, but there were PMCs there.

**Klinical** · `13:14` — like inside the locked room?

**LifeBosses (Серёга)** · `13:15` ↳@LifeBosses (Серёга) — as if they passed through them without opening

**LifeBosses (Серёга)** · `13:16` — i try do video

**Klinical** · `13:16`
I observed bots walking through doors after the player died and started spectating too but I chalked that up to being dead.  
That was many build ago though

**Chazut** · `13:17` — They should be physically stopping to open the door, not phantom-walking through it. Sounds like the door-trigger condition (proximity + path-cross) may sometimes not work properly, gonna dig into it. Thanks for sticking with the report!

**DeW4VE** · `13:18`
Hey Chazut,  
  
I run a Fika Headless server that runs pretty constant, and so far I gotta say performance wise, this does run better than Questing + Looting bots.  
  
Keep it up!

**LifeBosses (Серёга)** · `13:18` — When you play and enter any building, doors will sometimes appear randomly if a PMC bot enters, but now all doors are closed by default. as if you were the first one to go there _(editado)_

**Klinical** · `13:18` — the customs first floor door of three story is the exact door that triggered the ghost walk for me. the side door with the metal stairs

**Chazut** · `13:19` ↳@DeW4VE — That's awesome to hear  perf was a big focus during the rewrite, glad it's paying off on headless too. Thanks for the report!

**Kobe Thuy** · `13:19` — As far as long range-cqb consideration goes, you can base it off of Progressive Bots’s default config

**Chazut** · `13:21` — Thanks @LifeBosses (Серёга) and @Klinical, that's a really useful tell. Confirms the door-open trigger may be malfunctioning. Thanks both

**Klinical** · `13:22` — back to testing sir!

**Chazut** · `13:23` ↳@Kobe Thuy — Appreciate the pointer  but I'm staying away from looking at other mods' code, won't repeat the LB drama. I'd rather just dial it in from playtesting + community feedback

**Kobe Thuy** · `13:24` ↳@Chazut — It’s a configurable config. So just by referencing the data you can tell which maps is generally considered as long range or not

**Kobe Thuy** · `13:24` — No need to copy code or anything

**Klinical** · `13:25` — they discussed already using MOA instead if thats what u guys are talking about. that sounded wicked

**Chazut** · `13:25` — Fair, I get the distinction. Just keeping the safe habit for now, community feedback + playtesting will get me there. Thanks for the clarification though

**Kobe Thuy** · `13:26` — Also can the config for certain bot types to be bypassed by Orbit be expanded to any type of bots that might come in the future, ex: ISB Special Force

**Klinical** · `13:29` — I have another crazy idea. Allowing some bots to prone  or crouch when looting a body to minimize silhouetting. Like a bot in the open proned on a body could be overlooked by a player but a bot just standing there is allot easier. _(editado)_

**Chazut** · `13:30` ↳@Kobe Thuy — Just double-checked, yeah ORBIT controls every bot type by default right now (any new bot from any mod gets picked up automatically). Honestly I'm planning to flip that logic (Phobos heritage), make it opt-IN per bot type instead of opt-OUT. Way safer. On the list, thanks!

**Chazut** · `13:31` ↳@Klinical — Solid one. ORBIT already kneels to loot (BSG's built-in pickup animation), but proning when exposed would be a nice tactical layer on top. Adding to the list as well

**Shynd** · `13:32` — would be neat to have config toggles per bot type / brain type, too. i (like some others) played around with a custom phobos version where i had removed scavs from roaming and it felt really nice to have scavs in the expected areas but run into a pmc in unexpected ways. _(editado)_

**Klinical** · `13:32`
im just gonna keep requesting features instead of making my own mod  
im going to make you build me the best mod jkjk

**Shynd** · `13:32` — sorry if that's already a thing, just thought about it while you were talking about opt in vs opt out

**Chazut** · `13:35` ↳@Shynd
Actually that one's already there   Roaming Scavs and Roaming Goons toggles in F12 + vanilla Scavs toggle.  
You haven't tested it yet? shame haha

**Klinical** · `13:36` — imo roaming scavs should be on by default

**Chazut** · `13:36` ↳@Klinical — Lmao keep them coming, that's how the mod gets better

**Shynd** · `13:37` ↳@Chazut — yeah what am i even doing here if i don't use the mod yet!?

**Chazut** · `13:37` ↳@Klinical — by default ORBIT does control scavs, but they have a home-attraction force pulling them back toward their spawn zone, so they don't sprint across the whole map like PMCs. Roaming Scavs toggle removes that pull (they roam like PMCs), Vanilla Scavs toggle drops them back to pure BSG behaviour entirely. Three tiers basically

**Chazut** · `13:38` ↳@Shynd — Lmao you bring more value to this thread

**Lemoireal** · `13:38` — Chazut,not a request,but im curious,if a mod like orbit will compatible with pit fireteam,i know they will conflict with something like group behavior,but is it possible in some way,im no developer so i didn't really understand things like this

**Chazut** · `13:39` — Honestly no idea, I haven't looked at Pit Fireteam at all. If you end up testing the combo, would love a quick report on what happens

**Klinical** · `13:40`
i dont think the bots are registered as custom bot types like the other mod Miyako Carry Service  
the carry service could be hooked in i think because they do McsBotPlayer _(editado)_

**Chazut** · `13:41` ↳@Lemoireal — My money's on chaos honestly. Place your bets on which one breaks first

**Lemoireal** · `13:41`

**Klinical** · `13:41` — if i had to guess your pmc friends will just patrol away and leave you

**Chazut** · `14:02` — Might consider proper Pit Fireteam compatibility down the line, but not anywhere near soon, got a ton of stuff queued up before that. Cross-mod compatibility is also a real maintenance burden, every time the other mod updates I'd have to re-verify nothing broke, so it's not something I'd take on lightly

**Chazut** · `14:02` — Random question, anyone got a good free online translator to recommend? (google trad isn't great, same level as me ). I burned through my free DeepL quota for the week in this thread lol. I understand English fine, just sometimes use it to polish my phrasing _(editado)_

**Lemoireal** · `14:15` ↳@Chazut — sorry I didn't mean to make you work more ,im just curious

**Lemoireal** · `14:15` — But i just tested it and watch raid review,my squad doesn't leave me,my command work,and the enemy bot still doing orbit objective

**Lemoireal** · `14:17` — But maybe there's something break that i didn't notice,must be

**Rin** · `14:38` — I haven't played any Tarkov in a hot minute, I should try out Orbit since it seems to address the (lack of) looting problem when I was using Phobos _(editado)_

**Lemoireal** · `14:46` ↳@Rin — yeah you should,like a 3 in 1 ai mod,good performance,my favorite pair with sain rn

**MauwMa** · `14:48`
I wondered if you managed to make the PMCs stay away from the minefield on lighthouse.  
The first minutes of the raid a PMC just walked through the minefield.  
Raid Review flagged as Extracted, but on the map he disappeared after getting on the minefield.
  🖼️ 📎 [`att-24-2026-06-04-MauwMa.png`](./assets/att-24-2026-06-04-MauwMa.png)

**Rin** · `14:48` — I shoud use Raid Review, seems like a pretty cool way to see how alive the raids are

**LifeBosses (Серёга)** · `14:57` ↳@Rin — me too i back now play tarkov and try ORBIT this really change raid they looting they move need more work but i belive this mod change many on raids)))

**Cosmin** · `15:12` ↳@Chazut — well i know, but i did that  in 2023 or 2024 and it was fine, in any case i was honest enough and was thinking to ask here

**Chazut** · `15:31` ↳@Lemoireal — Oh nice

**Chazut** · `15:33` ↳@MauwMa
Oops, you nailed it. Phobos was missing a lot of POIs (I suspected a bug there) so I rebuilt the POI generation, and totally forgot to exclude the minefield zones. Thanks for the report   will fix it!  
RR doesn't understand and thinks the bot was extracted (since it wasn't killed by anyone, even himself) _(editado)_

**Bourne** · `15:39` — I uninstall phobos and install orbit correct?

**Zybergeris** · `16:11`
@Chazut Orbit is BOMB!  
  
Dont overburn yourself, just keep moding at normal pace so you dont lose your mind, better to have you in long term then loss you!  
  
Mod feels amazing as for 1.0! Future update will just make it better.  
  
Good to be Back to SPT playing and enjoying!

**Chazut** · `16:39`
  🖼️ 📎 [`att-25-2026-06-04-Chazut.png`](./assets/att-25-2026-06-04-Chazut.png)

**Chazut** · `16:40` ↳@Zybergeris — Means a lot  I'll pace myself for sure, this is a long game. So happy to hear you're enjoying SPT again

**Lega** · `16:49` — Hey man, trying orbit now, do I have to activate extraction settings in SAIN for them to extract? _(editado)_

**SenTineL** · `16:50` — So I did like 5 raids using Orbit and so far it is AMAZING. Raids feels alive, not just bot shooting fest with them sitting on the "usual" places. Thanks for the amazing mod. Hope it will only grow from here onwards.

**Chazut** · `16:52` ↳@Lega
  🖼️ 📎 [`att-26-2026-06-04-Lega.png`](./assets/att-26-2026-06-04-Lega.png)

**SligarTheTiger** · `16:52` — Had a question, I guess I can just test it myself but asking if anybody else experienced this., with probos it really messed with the AI's combat making them have a lot of vanilla behaviors with how they found cover and engaged, made then pretty dumb like it was interfering with SAIN. Is that still an issue with ORBIT seeing as it uses stuff from probos. Was night in day fighting the AI with QuestingBots alpha from probos.

**Zybergeris** · `16:53` — What about AI LIMIT with this mod? Since Phobos was breaking with them is Orbit too?

**Chazut** · `16:54` ↳@SligarTheTiger
ORBIT explicitly hands the bot back to SAIN for combat (15s grace after combat ends before ORBIT picks it up again), so SAIN should run combat fully without ORBIT stepping on it.  
  
That said, the movement side (out-of-combat) is still pretty straight-line at the moment, corner checking, scanning the rear, less mindless dashing is on the roadmap. So if Phobos's dumb-looking behaviour was actually a movement thing rather than a combat one, ORBIT inherits some of that until I get to that work

**Chazut** · `16:55` ↳@Zybergeris — No idea, not aware of that one, what was the issue exactly with Phobos + AI Limit?

**Zybergeris** · `16:55` ↳@Chazut — It was breaking AI in general somehow, only SAIN Ai limiter worked

**Vendth** · `16:56` — The setting I need to change under SAIN is "Randomnly Assigned Chance" right?

**Chazut** · `16:56` ↳@Zybergeris — Ahh ok. Honestly haven't tested with AI Limit myself. If you give it a shot, would love a report

**SligarTheTiger** · `16:56` — Roger, Really like the idea it was just making fighting the AI, With probos that is, really goofy, where they'd fight eachother just walking back and forth in the open like vanilla scavs on live.

**SligarTheTiger** · `16:57` — they didn't run off and wait for an ambush etc

**Chazut** · `16:57` ↳@Vendth — Yep that's the one. Also make sure the personality itself is "Enabled", should be the line right above it iirc

**Chazut** · `16:59` — Yeah that's the movement layer being too straight-line, ORBIT still has some of that. Ambush stuff is on the roadmap as well, should feel way better once I get there

**Chazut** · `17:01` — But again, the moment a bot enters "combat" phase, meaning it detects footsteps, hears a shot, or spots something, ORBIT loses control entirely until SAIN drops it

**SligarTheTiger** · `17:02` — I'll do some trying out with it, Performance improvements and slimer mod orders are always a god send with the bots being CPU hogs

**Chazut** · `17:02` — Let me know how it goes

**Zybergeris** · `17:08` ↳@SligarTheTiger — What performance mods would you say works best?

**SligarTheTiger** · `17:09` — The only one I have is the de-clutterer or whatever it's called

**Shynd** · `17:09` ↳@Zybergeris — if you're going to test an ai limit mod, prefer Fika Dynamic AI. lacy disables bots in a way that specifically tries best to not break things for other mods. i cannot speak for AI Limit.

**Zybergeris** · `17:11` ↳@Shynd — Will try it

**Chazut** · `17:15`
General note on AI limit mods, I have never personally tested any of them with (and without) ORBIT. Just to set expectations though: ORBIT was designed assuming all bots run their full lifecycle. If a limit mod deactivates bots while they're alive, some features lose meaning, you won't find pre-looted zones or emptied corpses, extractions will be way rarer because inactive bots don't progress toward their objectives, etc. The "raid feels alive" effect depends on bots actually doing stuff in the background, even when you're not near them.  
So if it still work, just expect a different feel _(editado)_

**Archangelway** · `17:16` — AI limit mods are the worst for any AI mod

**Archangelway** · `17:17` — Just let tarkov's culling do it's thing (as shit as it is)

**Shynd** · `17:17`
yeah i personally feel like AI limit type mods are antithetical to what mods like ORBIT, Phobos, and even Questing Bots are trying to accomplish.  
  
if anything, a limiter within ORBIT that turns off processing of bots further than X distance or bots that have an objective that would take them further than Y distance from a player would be better. just flatly disabling & removing the full bot object, which is what ai limiters do, sounds like a recipe for fuckin disaster  
  
i am not advocating for ORBIT to have its own limiter built in, feature creep and such, just that would be the only way i would think it could possibly cause no issues. QB has its own limiter, for instance.

**Bloody** · `17:20`
Hey! Great mod btw, has a lot of potential. Now, I had a problem when creating raids on Shoreline. Can I send the logs here? It could very well be a memory issue, an issue with another mod, or anything else. I'm not that good at reading logs and identifying the issue myself.  
Worth nothing that the crash was always pre-raid, while loading. Restarting the game didn't quite fix it nor restarting the server, but restarting my computer did

**Shynd** · `17:23` ↳@Shynd — to add to this, this is specifically why i think the convergence setting in Phobos is transformative: reduce total number of bots on the map, still feel like the raid is alive and full. gain performance without hacky disabling of AI.

**Chazut** · `17:26` ↳@Shynd
Honestly the idea makes sense on paper but I'm a bit sceptical about the gain on ORBIT.  
If I ever did anything in that direction it'd probably be a degraded tick rate for distant bots rather than a full pause, progress continues, just coarser

**Shynd** · `17:27` — yeah i personally hate it, but also i have just about the best PC on the planet so i'm not sure my opinion matters

**Chazut** · `17:28` ↳@Bloody — Sure, send them over  I can take a look. If I can't find anything, the official SPT support channel is also a great place to post, more eyes

**Bloody** · `17:30` — don't think I can add a zip folder here, hmm. I included all logs that my be relevant

**Shynd** · `17:30` — Player.log is the best log for crashes fwiw

**Trinagan's Alt** · `17:30`
@Bloody  
  
Player.log  
,  
Can you please press Win + R, paste in AppData\LocalLow\Battlestate Games\EscapeFromTarkov, and drag and drop the Player.log into Discord?  
----  
Tag: player.log - Ran by: @Shynd

**Bloody** · `17:31` — thing is, this one should be an old one, not sure if it includes the crash
  🧾 📎 [`att-27-2026-06-04-Bloody.log`](./assets/att-27-2026-06-04-Bloody.log)

**Chazut** · `17:31` ↳@Shynd — Yeah I will put convergence back as a toggle just like we discussed earlier

**Shynd** · `17:32` ↳@Bloody
yup, memory issues. i'd say like 9 out of 10 times i see this error it's due to one of two things:  
  
user has messed with their virtual memory settings / page file settings in windows, which is bad, they should be default  
,  
user has <50gb free space on their system drive, which also limits page file  
,
  🖼️ 📎 [`att-28-2026-06-04-Bloody.png`](./assets/att-28-2026-06-04-Bloody.png)

**Shynd** · `17:32` — alternatively it could be something like overwolf, which i happen to see in the dll list

**Cosmin** · `17:32` — ai limit does not work well, at least for me, used it only for streets, but even if i set limit distance to 1000-3000 on the other maps, ai still entered idle or standby even at 500m

**Bloody** · `17:33` — ok that'll be the first thing to try

**Zybergeris** · `17:33` — Hmm but i feel like ORBIT is a bit heavier then PHobos itself, possible? Performance is a bit lower, maybe cuz of looting and questing?

**Archangelway** · `17:33` — no way

**Bloody** · `17:33` — Also got these if they help, and LogOutput from BepInEx
  🖼️ 📎 [`att-29-2026-06-04-Bloody.png`](./assets/att-29-2026-06-04-Bloody.png)

**Shynd** · `17:33` — nope, Player.log covers it

**Bloody** · `17:33` — perfect

**Shynd** · `17:34` — this isn't really the place to troubleshoot a memory crash but hopefully i have given enough info to get started and, if not, ⁠spt-support-4-0

**Bloody** · `17:35` — gotta start from somewhere so I'll try it. I have 32gb of ram but for some reason pagefile was set to 10gb. no idea why I every did that but yeah

**Bloody** · `17:35` — its now system managed, lets see if it helps

**Shynd** · `17:35` — oh, well no, not ⁠spt-support-4-0 because you have fika

**Shynd** · `17:36` — either way this is not the place, we spam chazut out of house and home enough as it is

**Zybergeris** · `17:37` ↳@Shynd — hm but i believe SAINS's limiter should be kinda ok? Since it prevets AI From fighting and not moving?

**ms. r3mains** · `17:37` — these 2 raids i did with orbit+ai limiter felt pretty dead

**Shynd** · `17:37` — all SAIN's limiter does is make it so that AI don't notice other AI as often if far away from the player

**Shynd** · `17:37` — it does not disable the bot at all

**Zybergeris** · `17:38` ↳@Shynd — but them not being in comabat does make AI to be lighter on performance, no?

**Chazut** · `17:38` ↳@Zybergeris — Possible. Compared to bare Phobos, ORBIT does more per frame. Compared to LB + QB running together, idk, couple of headless users reports actually said ORBIT runs better. So depends what you were running before

**Chazut** · `17:39` — Since LB and QB are doing more complex/complete things than Orbit (in their domains) _(editado)_

**Cosmin** · `17:40` ↳@Bloody — lol now i see i have 25 gb pagefile on 32 gb ram

**Cosmin** · `17:40` — idk how

**Chazut** · `17:40` ↳@Zybergeris — Yes SAIN limiter will be ok with Orbit

**Shynd** · `17:40` ↳@Zybergeris — i mean of course, but it's just different and thus not comparable to AI Limit or Fika Dynamic AI. Chazut & Janky built the foundation for this mod on top of SAIN, i'd be blown away if any SAIN setting caused significant issues, including the Limit AI vs AI setting.

**Shynd** · `17:40` ↳@Cosmin — tarkov loves page files

**Cosmin** · `17:41` — should i change the pagefile?

**Cosmin** · `17:41` — lol

**Shynd** · `17:42` — the spt wiki has a note about page file and that is the last word on what settings someone should use https://wiki.sp-tarkov.com/en/Performance_Tuning#pagefile

**Cosmin** · `17:43` — thanks

**Chazut** · `17:43` — @Shynd really stepping in as unofficial support today, much appreciated  saving me a ton of typing haha

**Shynd** · `17:44` — tl;dr: DON'T TOUCH IT. if you did touch those settings, return them to defaults and then DON'T TOUCH IT. make sure at least 30-50gb free space on system drive. and then DON'T TOUCH IT.

**Shynd** · `17:45` ↳@Chazut — trying to get you back to where deepl free tier is enough

**ms. r3mains** · `17:45` — i had my page file on my slow ass HDD for a long time

**ms. r3mains** · `17:45` — id play something like modded H3VR, could play it fine but my whole pc would be down for a good few minutes after i closed it

**Cosmin** · `18:04` — what if i run headless client on the same pc since i have only 1 , would be worth ?

**Shynd** · `18:04` — impossible to say without just trying it

**Fums** · `18:04` — If you have the RAM and a strong CPU it can work but its not recommended.

**Cosmin** · `18:05` — AMD Ryzen 7 3700X and Kingston HyperX Fury 32 GB (2 x 16 GB) DDR4-3200 CL16 Memory

**Cosmin** · `18:05` — lol

**Cosmin** · `18:06` — and some dust i need to take out i think

**Shynd** · `18:06` — my guess is that's a recipe for more headaches, but i've seen people do local headless with 32gb ram before

**Fums** · `18:06` — You also do not need a strong secondary PC. I know in different countries and different people see money in different ways but you can get something like an optiplex 3070 with a 9th gen i5 and upgrade to 32gb of ram for fairly cheap. I have two and use one for headless.

**Fums** · `18:06` ↳@Cosmin — I would not recommend it with just 32gb of ram.

**Cosmin** · `18:06` — i see

**Cosmin** · `18:06` — then il wait untill the next upgrade

**Cosmin** · `18:07` — better

**Chazut** · `18:07` — I'm glad I upgraded my PC before RAM prices skyrocketed

**Cosmin** · `18:08` — at least you where not born in romania

**Fums** · `18:08` — I got the last DDR5 32gb kit for 180gbp back in december on amazon. Havent seen anywhere cheaper since.

**Fums** · `18:08` — 9800x3d + 5080, happy with it.

**Fums** · `18:08` — And tarkov still runs pretty piss poor with smooth motion + headless host

**Zybergeris** · `18:08` ↳@Fums — I tested it with 7800x3d and headless fps goes brrrrrrrrr and then SAIN AI just becomes too stupid to fight

**Zybergeris** · `18:09` — 32gb ram is not enough

**Cosmin** · `18:09` ↳@Zybergeris — you should go live pvp then lol

**Cosmin** · `18:09` — but the cheaters...

**Zybergeris** · `18:09` ↳@Chazut — im sad i didnt get more rams..

**Chazut** · `18:09` ↳@Fums — got the double for approx the same price last year

**Fums** · `18:10` — Yeah I impulse upgraded my PC because I was getting shit frames on customs.

**Cosmin** · `18:10` — well i got 2 of those ram pieces, and i got 2 free slots left, and idk if i will find anymore of those, im afraid i will have to buy 4 ram pieces

**Fums** · `18:11` — you dont need the exact same model of ram, especially when its in opposite channels

**Fums** · `18:11` — you are just limited by the max speed of the lowest max

**Cosmin** · `18:11` — i hope you are right, i have not checked this, trying to be safer than sorry

**Fums** · `18:11` — so if you have two 3600 and two 3200 then your max for all 4 would be 3200

**Cosmin** · `18:12` — i see

**Fums** · `18:12` — I work in IT so I would hope im decently correct but for sure do your own research before you spend the money.

**Zybergeris** · `18:13` ↳@Fums — ⁠general⁠ ?

**Fums** · `18:14` ↳@Zybergeris — Yep

**Bloody** · `18:15` ↳@Shynd — fixed the pagefile, crashed again loading shoreline
  🧾 📎 [`att-30-2026-06-04-Shynd.log`](./assets/att-30-2026-06-04-Shynd.log)

**Fums** · `18:16` — Why does my 5080 have the same amount of VRAM as a 3060ti? I hate nvidia.

**Fums** · `18:16` — Sorry seperate rant :)

**Bloody** · `18:16` — XD

**Fums** · `18:19` — out of memory again according to that ^

**Bloody** · `18:20` — thing is there's not much I can do ig, I changed pagefile, have nothing else open (Discord+Tarkov only)

**Bloody** · `18:20` — 32gb

**Chazut** · `18:20` ↳@Bloody
Reading the new log, still the same OOM ("Could not allocate memory: System out of memory!"). Page file fix unfortunately didn't free enough.  
You can maybe try a smaller map and close every other apps (Discord included), anything else in the background.  
I can't help more sorry

**Bloody** · `18:20` — and only a few selected maps give me problems. woods, for ex, is in theory bigger and gives me no issues

**Fums** · `18:20` — how many bots are you spawning in Acids

**Fums** · `18:21` — id be inclined to try and load into a raid with no mods, or at least no SAIN, orbit, placement

**Fums** · `18:21` — see if the crash happens and if not see your current memory allocation

**Bloody** · `18:22` — ok I'm in... closed discord and it worked

**Chazut** · `18:52`
So that's the two explosions I heard  
I need to fix this problem
  🖼️ 📎 [`att-31-2026-06-04-Chazut.png`](./assets/att-31-2026-06-04-Chazut.png)

**Shynd** · `18:54` — i remember janky addressing that problem & saying it was annoying because bsg

**Cosmin** · `18:54` — i never explored that zone this and labyrinth, and the woods train station

**Cosmin** · `18:55` — and i played spt since the first days

**Chazut** · `18:55` — Then, the Rat arrived and went into orbit as well _(editado)_
  🖼️ 📎 [`att-32-2026-06-04-Chazut.png`](./assets/att-32-2026-06-04-Chazut.png)

**Cosmin** · `18:55` — when all bots where just patroling their zone, ugh

**Cosmin** · `18:56` — the rat came for their corpses

**Cosmin** · `18:56` — but the rat did not know that was his trap

**Cosmin** · `18:56` — lol

**Cosmin** · `18:57` — i wish to cosplay zriachy

**Chazut** · `18:57` ↳@Shynd — Yeah, I'll need to brainstorm a solution

**Cosmin** · `18:58` — i feel that we need more of the lore on cultists and zry

**Cosmin** · `18:59` — and wtf happened to rhizy , last time he met zriachy

**Fums** · `19:13`

**Fums** · `19:13` — ⁠Janky's Emporium of Jank⁠

**Fums** · `19:13` ↳@Shynd — Yeah tis that ^

**Bourne** · `19:13` — I've been having pretty heavy frame drops whenever I load in after installing orbit. I'm using AI limit, Sain, and all dependencies. Previously I was getting like 100-170

**Fums** · `19:14` — Do you have the same with phobos?

**Fums** · `19:14` — from what I understand Orbit is lightweight like Phobos is but they arent invisible to performance.

**Bourne** · `19:15` — no, phobos I was averaging near 170

**Fums** · `19:15` — And I know Phobos has spikes when you load in as IIRC it does all the calcs at the start.

**Bourne** · `19:15` — Uninstalled looting bots and phobos and now im lucky to get 60

**Fums** · `19:15` — And youre sure its Orbit causing this?

**Fums** · `19:15` — IE you uninstalled Orbit and it shot your frames back up?

**Chazut** · `19:20` ↳@Bourne — LB and Phobos are not compatibles

---

_Fim da transcrição — 699 mensagens, de 27/05/2026 a 04/06/2026 (GMT-3)._

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-06-04 | Guilherme | docs(technical): move modding guides out of backlog, add frontmatter and index |
