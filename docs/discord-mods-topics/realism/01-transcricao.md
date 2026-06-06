---
title: "SPT Realism Mod — Transcrição do canal #realism-mod-development (Discord SPT)"
date: 2026-06-05
status: 🔵 Em andamento
authors: Guilherme
---

# SPT Realism Mod — Transcrição do canal de desenvolvimento (Discord)

> Captura **fiel** do canal **"Realism Mod Development"** no servidor Discord da comunidade **SPT** (SPT Pub), recorte dos **últimos ~90 dias**.
>
> - **Link:** <https://discord.com/channels/875684761291599922/1123680324254171186>
> - **Recorte capturado:** 07/03/2026 02:23 → 04/06/2026 21:03 (GMT-3)
> - **Total de mensagens:** 1132
> - **Anexos:** 18 imagens/gifs + 6 vídeos (`.mp4`), referenciados em [`assets/`](./assets/) (vídeos podem não estar versionados — ver README).
> - **Método:** navegação autenticada no Discord (extração do DOM via Chrome DevTools), seek-up até o cutoff de 90 dias + down-sweep contíguo, dedupe por ID de mensagem. Capturado em 2026-06-05.
> - **Notas:** horários derivados do **snowflake** (epoch Discord), convertidos para **GMT-3**. Texto **preservado no idioma original** (majoritariamente inglês). `↳@X` indica resposta direcionada a X. Linhas `📎` são anexos. Mensagens sem texto são gifs/stickers/embeds não capturáveis pelo DOM.

---


## 07/03/2026

**Kushlungs** · `02:23` — ah, here it is

**Kobe Thuy** · `03:14`


## 09/03/2026

**Joe** · `00:04` — diabetes when you eat too many sugar cubes and drink soda

**Joe** · `00:04`

**Dashwood Foxe** · `01:27` — @Fontaine we need bladder and bowel mechanics. _(editado)_

**t3quila** · `04:34` — Gastory

**Fontaine** · `04:53` ↳@Dashwood Foxe — Meh food poisoning sfx is close enough

**Dashwood Foxe** · `04:54` ↳@t3quila — Tfw you rip out a major one loud enough to give yourself away

**The_Gooch** · `17:26` — abiotic factor type ish


## 10/03/2026

**adishee** · `06:12` — there's an absolutely wild conflict with Realism where, if a) i have PTT installed but b) remove interactable exfils (which is not actually req, and which is better because otherwise you get no extract conditions), c) it conflicts with Realism for some reason

**Qwertyalex** · `06:16` ↳@adishee — Wait, isn't it required? Or is that a holdover from older versions?
  🖼️ 📎 [`att-01-2026-03-10-Qwertyalex.png`](./assets/att-01-2026-03-10-Qwertyalex.png)

**adishee** · `06:17` — it's not actually required

**adishee** · `06:18` — there's even a toggle to turn off the warnings in slum king's version

**adishee** · `06:19` — i've been trying to finish my PTT config and i get cockblocked both from this and from the hazard zones (which i've talked to F about already)

**Qwertyalex** · `06:20` — Huh, so it is? O_o It only uses ExfilPromptService from what I can see

**Qwertyalex** · `06:22` — Oh wait, it's also used in transit voting for fika

**adishee** · `06:58` — i really wish interactable exfils had the original extract conditions

**Qwertyalex** · `07:52` ↳@adishee — That might be a bug? Looking at the IE code it should check extract reqs
  🖼️ 📎 [`att-02-2026-03-10-Qwertyalex.png`](./assets/att-02-2026-03-10-Qwertyalex.png)

**adishee** · `09:44` — well, it does sometimes have requirements, for car transfers or flare exits for example. But the majority are not tracked. Maybe it is a bug? Like red rebel, train, backpack are not tracked.

**Fontaine** · `09:47` ↳@adishee — I mean what's the exception? Should be able to narrow it down

**Fontaine** · `09:48` — Realism modifies factory exfil requirements if gas isn't turned off, so if you're removing exfil related objects or something like that I can see that happening _(editado)_

**Yorakairos** · `09:53` ↳@Kojimbooo — Bro can’t just drop that without giving us  his modlist/ reshade/ Amands conifgs

**nat** · `13:19` ↳@Kojimbooo — What is the Hipfire mod called? Been looking for this!

**Kojimbooo** · `14:04` ↳@nat — SPT Realism Active Aim System (AAS)

**GrooveypenguinX** · `21:55` ↳@Dashwood Foxe — Little do you know.....
  🖼️ 📎 [`att-03-2026-03-11-GrooveypenguinX.png`](./assets/att-03-2026-03-11-GrooveypenguinX.png)

**Jpdarkone³** · `23:03` ↳@GrooveypenguinX — Why wasnt i informed?


## 11/03/2026

**nat** · `04:40` ↳@Kojimbooo — This a standalone thing?

**Qwertyalex** · `05:37` ↳@nat — Part of Realism, you can change the keybind for it in F12

**Fontaine** · `09:25` ↳@GrooveypenguinX — can't believe you shared this

**Fontaine** · `09:25` ↳@nat — he just means the active aim feature of Realism's stances

**GrooveypenguinX** · `09:26` ↳@Fontaine

**Fontaine** · `09:26` ↳@GrooveypenguinX

**Fontaine** · `09:29` ↳@Fontaine
Speaking of...This is a VERY early pre-pre-pre-pre-alpha porotype WIP of the new stance system. It doesn't even use animation curves or have extra procedural layers to it yet. But already things are a lot cleaner and a lot more natural looking.  
  
To get active aim to this state with the previous system took lots of jank manual work that just wasn't maintainable, it was very time consuming to adjust things. Just to make ADS not jank took hours of work. This new system just works.
  🎥 📎 [`att-04-2026-03-11-Fontaine.mp4`](./assets/att-04-2026-03-11-Fontaine.mp4)

**Fontaine** · `09:31` — The difference in speed between low ergo/high ergo will be less as can be seen; instead visually it looks like there's more effort and movement when using lower ergo weapons

**Kobe Thuy** · `09:41` ↳@Fontaine — Oh mer gawd Realism is entering testing phase, expect release next week !!!!!!

**Kobe Thuy** · `09:41`

**Kobe Thuy** · `09:42` — Can't wait to chug it into my 60+ modlist

**Fontaine** · `09:44` ↳@Kobe Thuy — you've added +2 weeks

**Kobe Thuy** · `09:44` — Make it 4

**Fontaine** · `09:44` — +2 months

**Kobe Thuy** · `09:45` — I wanna keep seeing ppl bitching about they can't move to 4.0 because "Mah RM is not updated, OMG SPT is unplayable"

**Fontaine** · `09:46` — they're true Realism patriots, unlike you

**Fontaine** · `09:46` — smh

**Kobe Thuy** · `09:47` — also the average 3.9.8 enjoyers

**Qwertyalex** · `10:06` ↳@Kobe Thuy — >Me, having the rest of the WTT team joke about how I still play 3.11 instead of 4.0

**Dashwood Foxe** · `10:14` ↳@Kobe Thuy — Vanilla tarkov is hot garbage, I want my guns to feel like guns and not toys,

**Dashwood Foxe** · `10:15` — I refuse to wet my willy with 4.0 until RM is properly updated

**Jpdarkone³** · `10:50` ↳@Fontaine — Its so smooooth unlike the insta clipping into a stance

**Jpdarkone³** · `10:51` ↳@Kobe Thuy — This is me

**riffofthegods** · `11:02` ↳@Fontaine — it looks so good man

**riffofthegods** · `11:21`

**GrooveypenguinX** · `11:27` ↳@Fontaine

**J3RN3J** · `13:12` — Goatdalf the White

**ZuluFox** · `19:55` — https://fxtwitter.com/nikgeneburn/status/2031771479784755482

**ZuluFox** · `19:55` — https://fxtwitter.com/velion/status/2031779623084667176

**ZuluFox** · `19:56` — I don't get how these people can claim to have held real guns

**PrescriptionAdderall** · `22:46` — It’s like your PMC is holding it away from his body for a moment.

**PrescriptionAdderall** · `22:47` — Like he’s adjusting the stock’s placement against his body.

**PrescriptionAdderall** · `22:51` — This only makes sense if your PMC was in low ready or something.

**PrescriptionAdderall** · `22:51` — Though it would be fucking hilarious if BSG made their own version of stances.

**DrakiaXYZ** · `22:56` ↳@PrescriptionAdderall — Nah, you don't get the advantages of low-ready like a more clear field of vision. Just the negatives, like the fact before you shoot you put your gun down

**PrescriptionAdderall** · `22:57` — I’m doing it with my airsoft M4, and that jiggle simply doesn’t exist.

**PrescriptionAdderall** · `22:58` — The most that happens is if I aim down the irons too quickly I have the slightest amount of overswing.

**PrescriptionAdderall** · `22:59` — Low ready is fine, too.

**PrescriptionAdderall** · `22:59` — Even if I short stock it’s not as bad as in the video.

**Joe** · `23:20` ↳@PrescriptionAdderall — yeah but its tacticool and swag

**Joe** · `23:23` — thats it


## 12/03/2026

**Fontaine** · `03:24` — Yeah they're animating as if the stock isn't being shouldered when aiming, which clearly is the case. It looks like as you aim the stock is being taken out of the shoulder and back into it, that's the animation. Completely unnecessary

**Fontaine** · `03:24` — They could have added a small wiggle when aiming like BF6 or whatever

**Fontaine** · `03:24` — That's what I plan to do anyway

**adishee** · `03:56` ↳@ZuluFox — does look way better ..

**adishee** · `04:01` ↳@Fontaine — looks beautiful so far. Maybe some muzzle drop during the transition would add a lot of spice

**Instructor Bugcat** · `06:43` ↳@ZuluFox — What the hell

**Instructor Bugcat** · `06:44` — Why is homie dipping his rifle before aiming when the rifle is already hip aiming at the target

**ZuluFox** · `06:45` ↳@adishee — Yeah the parts they marked as "Old" do indeed look better

**ZuluFox** · `06:45`

**Fontaine** · `07:54` ↳@adishee — Yup this is just a basic implementation using springs, just moving from 0 to target vector. I will use animation curves for everything in final version

**FazanR** · `08:03` — I mean, I like the idea that they finally started to reworking the ergonomics, but... man... they did it again. They fucked it up even more.

**Fontaine** · `09:21` — Common BSG L

**PrescriptionAdderall** · `09:27` — This pisses off both the tactical crowd and the Adderall sweats.

**PrescriptionAdderall** · `09:28` — Because the sweats won’t like that basically every gun gets a more annoying form of over-swing now.

**Fontaine** · `09:40` — It slows down the gameplay which EFT could probably use but going about it wrong

**Fontaine** · `09:41` — Realism has much faster crouch and lean but all the additional mechanics slows down the gameplay naturally

**Fontaine** · `09:41` — Making shit annoyingly slow is the project reality/squad way

**Jpdarkone³** · `09:42` ↳@Fontaine — The problen is this game having horrid pvp

**Fontaine** · `09:44` — Yeah, the "memorize spawns and timing" meta is pretty boring

**Dashwood Foxe** · `09:44` ↳@Fontaine — shudders from the first recoil rework

**Dashwood Foxe** · `09:44` — Noodle arm PMC

**Fontaine** · `09:45` — Yeah for so long they balanced meta recoil builds by making stock builds terrible. Thankfully after 10 years they realize they could just narrow the gap between meta and stock

**Jpdarkone³** · `09:45` ↳@Fontaine — Not only that theres genuinelly no reason to not run a meta m4 every raid

**Jpdarkone³** · `09:46` — Then we have fuckibg ballistics

**Fontaine** · `09:46` ↳@Jpdarkone³ — Tbf with the latest iteration of recoil a stock build is still viable IMO. Especially in semi auto

**Jpdarkone³** · `09:46` — Combine ballistics with desync to create the worst pvp on the world

**Jpdarkone³** · `09:47` ↳@Fontaine — I suppose its better but its not like you cant get the best build for a half eaten sandwich

**Jpdarkone³** · `09:47` — Also no threats aside from scavs lol

**Jpdarkone³** · `09:47` — This is why gas is cool

**Jpdarkone³** · `09:48` — It would also make higher risk areas worse as u have bad visibility

**Jpdarkone³** · `09:48` — Plus beepbeepbeep

**Fontaine** · `09:48` — BSG was supposed to add radiation to high value areas but they still haven't managed yet

**Fontaine** · `09:48` — Probably a DLC lol

**Jpdarkone³** · `09:48` ↳@Fontaine — Moments before tarkov just dies because bsg prolly doesnt care anymore

**Jpdarkone³** · `09:49` — Like they probably made their last few sales with 1.0 and now the playercount will drop

**Jpdarkone³** · `09:49` — Especially if they lock content behind dlcs

**Dashwood Foxe** · `09:49` ↳@Jpdarkone³ — Wasn't EFT just a stepping stone to that bigger vision, Russia 2024 or w.e is called

**Jpdarkone³** · `09:49` — Can they lock EOD out of DLCS again so we get more modders

**Jpdarkone³** · `09:50` ↳@Dashwood Foxe — Russia 2028 is meant to be like a singleplayer game in the same universe

**Jpdarkone³** · `09:50` — But its prolly scrapped

**Jpdarkone³** · `09:50` — Or coming out in 2038

**Jpdarkone³** · `09:50` — Realism is so satisfying dudee

**Dashwood Foxe** · `09:50` ↳@Jpdarkone³ — I can't live without it

**Fontaine** · `09:50` ↳@Dashwood Foxe — Now not-BSG is making a Tarkov in space game

**Fontaine** · `09:51` — Totally not BSG, just the same people

**Dashwood Foxe** · `09:51` ↳@Fontaine — Space https://youtu.be/pzJ6tzyg02k

**Jpdarkone³** · `09:51`
https://medal.tv/games/escape-from-tarkov/clips/lEKFDAVxeqc--UKuu/spok?invite=cr-MSxzWlMsMjA4ODc1NDUxLA  
 Yo bro HOP on ground zero

**Jpdarkone³** · `09:52` — How does this genuinelly happen why did he eat those?

**Jpdarkone³** · `09:52` ↳@Dashwood Foxe — This game looked so good

**Jpdarkone³** · `09:52` — And i rlly wanted to play it

**Dashwood Foxe** · `09:53` — Same

**Jpdarkone³** · `09:53` ↳@Fontaine — Fontaine unrelated but do you think crismon desert will be cool

**Dashwood Foxe** · `09:53` ↳@Jpdarkone³ — It was mismanaged so badly

**Jpdarkone³** · `09:53` ↳@Dashwood Foxe — I couldnt queue in the playtest due to some server problems or wtv and the game died shortly after release

**Jpdarkone³** · `09:54`

**Dashwood Foxe** · `09:54` — Ughhhh

**Dashwood Foxe** · `09:54` — Honestly while cool, it was probably a blessing, the game was rampant with cheaters

**Jpdarkone³** · `09:55` — The games concept was rlly good

**Dashwood Foxe** · `09:55` — Like unbelievably bad with cheats

**Jpdarkone³** · `09:55` — I js with they

**Jpdarkone³** · `09:55` — Cared more

**Jpdarkone³** · `09:55` — And actually made a good game

**Jpdarkone³** · `09:55` — Titanfall 3 when

**Dashwood Foxe** · `09:55` ↳@Jpdarkone³ — Shattered horizon was the first to explore zero g fps _(editado)_

**Jpdarkone³** · `09:55` — So many interesting genres

**Jpdarkone³** · `09:56` — Remember that spitfire or wtv one

**Jpdarkone³** · `09:56` — The one with portals

**Jpdarkone³** · `09:56` — Too bad the portals were sorta useless like ig u could make some smart plays but yea

**Dashwood Foxe** · `09:56` — Would be cool if someone can snag Boundary assets and port it to tarkov for shits and giggles

**Jpdarkone³** · `09:56` — I js want titanfall 3 man

**Dashwood Foxe** · `09:57` ↳@Jpdarkone³ — Saaaame

**Jpdarkone³** · `09:57` — Titanfall 2 is genuinelly the most satisfying shooter ive ever played in my life

**Dashwood Foxe** · `09:57` ↳@Jpdarkone³ — The campaign was awesome

**Jpdarkone³** · `09:57` — And its like natural, youd think hitting shots at fast speed is hard

**Jpdarkone³** · `09:58` ↳@Dashwood Foxe — Instead we got apex legends _(editado)_

**Dashwood Foxe** · `09:59` — Yeaaah, extraction shooters and royales kinda ruined the gaming landscape, I know that's a spicy take. _(editado)_

**Dashwood Foxe** · `10:01` — I fucking blame that stupid novel Hunger games that gave everyone that itch for royales

**Jpdarkone³** · `10:02` ↳@Dashwood Foxe — Battle royales arent bad

**Jpdarkone³** · `10:02` — Its just that no one does anything interesting

**Jpdarkone³** · `10:02` — Atleast fortnite has building

**Jpdarkone³** · `10:02` — What does apex have different from warzone aside from abilities

**Dashwood Foxe** · `10:07` ↳@Jpdarkone³ — Interesting silly themed crossover event, this season is gundams

**Dashwood Foxe** · `10:08`
  🖼️ 📎 [`att-05-2026-03-12-DashwoodFoxe.jpg`](./assets/att-05-2026-03-12-DashwoodFoxe.jpg)

**PrescriptionAdderall** · `10:31` ↳@Fontaine
I hate how correct this is.  
Squad ICO was cancer on release, and it’s taken them to basically remove it for things to get back to somewhat feeling natural.  
That being said you still feel like a soldier who didn’t pass basic.

**Jpdarkone³** · `10:51` ↳@PrescriptionAdderall — Hot take i didnt think ico was that bad

**PrescriptionAdderall** · `10:52` — Did you play?

**PrescriptionAdderall** · `10:52` — I like the idea of the ICO, but trying to play was complete ass.

**Jpdarkone³** · `10:52` ↳@PrescriptionAdderall — Yes

**Jpdarkone³** · `10:52` — I thought it was cool how rounds went flying and go past you while you try to fight back

**PrescriptionAdderall** · `10:53` — That isn’t exclusively because of the ICO.

**PrescriptionAdderall** · `10:53` — It just made it happen more often because you couldn’t shoot straight.

**Jpdarkone³** · `10:54` — Thqts what im saying

**Jpdarkone³** · `10:54` — I didnt like how the game was run and gun

**PrescriptionAdderall** · `10:55` — That was an issue, I agree.

**PrescriptionAdderall** · `10:55` — But they overcorrected hard.

**Jpdarkone³** · `10:55` — Idk i enjoyed the tense moment of lining up a shot

**Jop the Filthy Casual** · `12:04` ↳@Dashwood Foxe — extraction shooters are still a niche, but battle royales were only kick started by fortnite imo

**adishee** · `12:16` ↳@Dashwood Foxe — that is hilarious

**Dashwood Foxe** · `12:16` ↳@adishee — Wut

**adishee** · `12:18` — that game

**adishee** · `12:18` — i mean top marks for originality

**adishee** · `12:19` — i think the funny thing is is that they are just wearing golden era space suits

**adishee** · `12:19` — looks like they were working on Hubble and, bam assault rifle in the garter

**Dashwood Foxe** · `12:20` ↳@adishee — Standard current gen EVA suits with some modest protection added and a fuck huge MMU?

**Dashwood Foxe** · `12:20` — Also Space AK, some things never change

**Dashwood Foxe** · `12:20` ↳@adishee — I like that idea

**Ser_G** · `12:28` ↳@Dashwood Foxe — Even Star Citizen did a space AK

**Ser_G** · `12:29`
  🖼️ 📎 [`att-06-2026-03-12-SerG.png`](./assets/att-06-2026-03-12-SerG.png)

**Dashwood Foxe** · `12:29` — @adishee https://youtu.be/ss1Kewdh_BE

**Ser_G** · `12:29` — It still bothers the hell out of me that the artist decided a dust cover wasn't a requirement

**Dashwood Foxe** · `12:30` — Dust cover is a suggestion on AK

**Dashwood Foxe** · `12:31` — "Cosmetic"

**Ser_G** · `12:31` — I mean I guess

**Dashwood Foxe** · `12:31` — As one guy demonstrated by cramming an entire pack of marshmallow peeps inside the receiver

**Dashwood Foxe** · `12:31` — And still firing

**Dashwood Foxe** · `12:32`
  🖼️ 📎 [`att-07-2026-03-12-DashwoodFoxe.gif`](./assets/att-07-2026-03-12-DashwoodFoxe.gif)

**Ser_G** · `12:33` — I have never heard of peeps before

**Dashwood Foxe** · `12:34` ↳@Ser_G — Same guy also replaced the entire ak furniture with bacon

**Dashwood Foxe** · `12:34` — Replace the entire handguard with slabs of bacon

**marerrey** · `12:39` ↳@Fontaine — so smooth holy

**Nosliw** · `12:55` ↳@Jop the Filthy Casual — What about PUBG

**Jop the Filthy Casual** · `12:55` ↳@Nosliw — I mean PUBG did kick start it but I credit fortnite for being F2P first

**ZuluFox** · `13:04` ↳@Jop the Filthy Casual — Fortnite wasn't even a BR yet when the BR craze started

**ZuluFox** · `13:05` — H1Z1 was the real OG

**Jop the Filthy Casual** · `13:06` ↳@ZuluFox — Although it was the biggest one in the early days after h1z1 and pubg crashed

**ZuluFox** · `13:06` — It started out with PlayerUnknown's BR mod for Arma

**Jop the Filthy Casual** · `13:06` ↳@ZuluFox — You mean for dayz the mod of arma

**ZuluFox** · `13:06` — Well PUBG didn't crash until Fortnite and Apex were already big

**ZuluFox** · `13:07` ↳@Jop the Filthy Casual — BR was for Arma not DayZ, wat

**ZuluFox** · `13:07` — Or well

**ZuluFox** · `13:07` — It was based on the DayZ mod if that's what you meant? At least that's how I remember it

**Jop the Filthy Casual** · `13:07` — I remember the battle royale mod was specifically in the arma mod version of DayZ

**Jop the Filthy Casual** · `13:08` ↳@ZuluFox — yea Arma 2 was the engine but it used the DayZ arma 2 mod

**ZuluFox** · `13:08` — Yeye okay then we're on the same page

**jsjsj** · `14:35` ↳@Fontaine — for me this one looks way better than the bsg one

**J3RN3J** · `15:42` ↳@Jpdarkone³ — man you musta been punching air on the closing announcement of the game awards this year

**Fontaine** · `19:11` ↳@Jpdarkone³ — uSe bEttEr aMmO gEt gUd

**Fontaine** · `19:11` — I wish they kept the armor hitboxes in live, made the game exciting to play for a change

**Hero of Tarkov Duwang** · `21:36` ↳@Jpdarkone³ — me when

**Ser_G** · `21:52` ↳@Fontaine — Made all the turbo sweats rage that their class 6 armor wasn't making them immune to timmies with mid tier ammo

**Ser_G** · `21:53` — I kind of see the argument that having realistic sized plates with no organs to protect was a potential issue, but I, as a casual loved it


## 13/03/2026

**Fontaine** · `03:05` — Excuse me?

**Fontaine** · `03:06` — Don't say shit like that

**Travon** · `03:33` ↳@Fontaine — good sir i wish you the bestest of health and i hope great things for your success.

**DrakiaXYZ** · `03:35` — Do not say shit like that, and do not harass mod authors for updates

**Kushlungs** · `03:38` — who's harassing , i literally cant im dead

**adishee** · `04:03` ↳@Fontaine — it's like the one thing that would have let me boot up tarkov once every two months

**adishee** · `04:04` ↳@Dashwood Foxe — nice animations

**Fontaine** · `04:13` ↳@adishee — Same

**jsjsj** · `07:40` ↳@Jpdarkone³ — if this is ap ammo thats crazy

**Fontaine** · `07:56` ↳@Kushlungs — Then stop commenting in this thread, thank you

**Jpdarkone³** · `09:37` ↳@jsjsj — Ap  6.3 yes

**jsjsj** · `10:03` ↳@Jpdarkone³ — nah man this game is fucked up

**Kushlungs** · `10:40` — Oh boo hoo

**Fontaine** · `10:42` — @DrakiaXYZ can you please remove them from the thread?

**Jpdarkone³** · `11:03` ↳@Kushlungs — This dude defo rages on 2k and cod

**Badger** · `11:13` ↳@Fontaine — done

**ZuluFox** · `11:13` — Lol trying to dodge slur filters is crazy

**Jpdarkone³** · `11:18` — Hit a nerve?

**PrescriptionAdderall** · `11:18` — Ah yes, the good ‘ol “harass the creator of the mod you like so they update their mod faster”.

**PrescriptionAdderall** · `11:18` — Works every time.

**Jpdarkone³** · `11:22` — @Badger  dont u think this warrants a bigger punishment

**Badger** · `11:22` ↳@Jpdarkone³ — where do you know that it didn't happend?

**Jpdarkone³** · `11:22` — Smh... ok cia

**Badger** · `11:23` ↳@Jpdarkone³ — huh?

**Jpdarkone³** · `11:23` ↳@Badger — I was saying youre a cia agent (mod team)

**Badger** · `11:24` ↳@Jpdarkone³ — i thought you said "cya" so i thought i said something wrong xD

**Jpdarkone³** · `11:26` — Smart mod

**BlackDeathGER** · `11:27` — you are giving me really difficult vibes xD ngl

**Jpdarkone³** · `11:30` ↳@BlackDeathGER — Im as sweet as candy wym

**BlackDeathGER** · `11:30` — i doubt that xD but we are derailing that thread here

**Jpdarkone³** · `11:31` — This theead has no rails ngl

**Joe** · `12:12` ↳@Jpdarkone³ — sour candy

**Jpdarkone³** · `13:33` — Lol
  🖼️ 📎 [`att-08-2026-03-13-Jpdarkone.jpg`](./assets/att-08-2026-03-13-Jpdarkone.jpg)

**GrooveypenguinX** · `13:35` ↳@Fontaine — You want em' nuked from WTT too?

**Jpdarkone³** · `13:35` ↳@GrooveypenguinX — He tried dming me and deleted it but i saw the f slur in there somewhere lmao

**Jpdarkone³** · `13:35` — Cus discord hadnt processed it as deleted yet

**GrooveypenguinX** · `13:35` — I didn't see the original messages but we don't need people like that in our community either

**GrooveypenguinX** · `13:39` — Nuked from WTT

**GrooveypenguinX** · `13:58`
  🖼️ 📎 [`att-09-2026-03-13-GrooveypenguinX.png`](./assets/att-09-2026-03-13-GrooveypenguinX.png)

**Dashwood Foxe** · `14:04` — What were they malding about?

**Dashwood Foxe** · `14:04` — I missed it

**Dashwood Foxe** · `14:05` — I just read somewhere that someone got upsetti sphaghetti that RM hasn't been updated for 4.x yet or something _(editado)_

**Fontaine** · `14:58` ↳@BlackDeathGER — Thanks

**DrakiaXYZ** · `14:59` ↳@Fontaine — Sorry for the delay, I was asleep

**Fontaine** · `15:00` ↳@GrooveypenguinX — I leave that go your discretion

**GrooveypenguinX** · `15:01` ↳@Fontaine
⁠Realism Mod Development⁠  
  
this was me nuking him lul

**Fontaine** · `15:01` ↳@GrooveypenguinX
  🖼️ 📎 [`att-10-2026-03-13-Fontaine.gif`](./assets/att-10-2026-03-13-Fontaine.gif)

**GrooveypenguinX** · `15:01`

**GrooveypenguinX** · `15:01`

**Fontaine** · `15:01` — No idea what their end game was, they've always been making weird comments

**J3RN3J** · `15:01` — i like that the second he gets called out on weirdo behavior he defaults to Protocol: Slur Slinger

**J3RN3J** · `15:02` — internet

**J3RN3J** · `15:02` — moment

**Archangelway** · `15:06` ↳@GrooveypenguinX — @Lacyway But when are you cooking him?

**Archangelway** · `15:06`

**Lacyway** · `15:07` ↳@Archangelway — He got unbanned at one point wasn't he

**Archangelway** · `15:08` — it's possible _(editado)_

**J3RN3J** · `15:08` — if you wanna go the full distance, go for a ban on fika aswell kekw

**GrooveypenguinX** · `15:09` ↳@J3RN3J

**J3RN3J** · `15:09` — he gone already?

**J3RN3J** · `15:09` — LMAO

**Jpdarkone³** · `15:47` ↳@J3RN3J — I think he didnt like it that i called him a person who rages at 2k

**J3RN3J** · `15:47` — who would

**J3RN3J** · `15:48` — tbf

**Jpdarkone³** · `15:48` ↳@J3RN3J — Well you wouldnt call someone a slur if there wasnt some truth to it

**J3RN3J** · `15:48` — i mean obv

**Guidot** · `17:10` ↳@Archangelway — Don't worry, papi took care of 'im


## 14/03/2026

**Joe** · `01:10` — i slept and woke up to this, what?

**Joe** · `01:11` — cocaine stim when?

**Joe** · `01:11` — works like morphine but better (also will kill you when you use too much and you get addicted)

**Joe** · `01:12` — jk, hope the dev goes well

**B468** · `14:47` ↳@Fontaine — It's so fucking hot

**Dashwood Foxe** · `20:32` ↳@Joe — (I think too much of any substance would kill you )

**Dashwood Foxe** · `20:33` ↳@B468 — poledance animation when?

**Dashwood Foxe** · `20:33`

**B468** · `20:43` ↳@Dashwood Foxe — exists in the secret fontaine sex mod

**Dashwood Foxe** · `20:43` ↳@B468 — I wanna see female tagilla poledancing at my hideout

**Dashwood Foxe** · `20:50`


## 15/03/2026

**infear** · `00:38`

**GrooveypenguinX** · `16:29` ↳@B468


## 16/03/2026

**BraveStarr** · `14:29`

**BraveStarr** · `14:29` — Huh? Did someone say sex mod?

**BraveStarr** · `14:30` — Brings back GTA SA memories.


## 17/03/2026

**Fontaine** · `05:09`

**t3quila** · `06:19`

**adishee** · `06:38` — did anybody ever do the unthinkable and just ... modify fika to run private pvp servers

**Guidot** · `07:39` — Yup, and they were banned

**adishee** · `07:45`

**BraveStarr** · `08:31`

**BraveStarr** · `08:31` — Never heard of that mod.

**Jpdarkone³** · `09:46` ↳@adishee — Yeah

**FazanR** · `10:03` ↳@Guidot — Uh, fuck BSG, man...

**Guidot** · `10:04` ↳@FazanR — What, BSG had nothing to do with the banning I was talking about.

**FazanR** · `10:05` ↳@Guidot — Really? So modders censoring themselves then?

**FazanR** · `10:05` — Damn

**Guidot** · `10:06` ↳@FazanR — I don't think you understand

**Guidot** · `10:06` — And this isn't a conversation for here.

**adishee** · `10:06` — (seeing as how i created this monster...) @FazanR spt devs are very sensitive to keep spt single player only to stay off their radar

**Guidot** · `10:06` — Literally not even the point.

**FazanR** · `10:06` ↳@adishee — Fair enough

**BraveStarr** · `11:38` — Let's just put it this way Fika is somewhat considered the F word around here. Use it with caution and at your own risk. Ask too many questions and the Goons will take you to Tagilla in the laberynth never to be seen again.

**DrakiaXYZ** · `11:39`

**Jpdarkone³** · `11:57` — Everyones misunderstanding eachother this is so funny

**Jpdarkone³** · `11:57` — Everyone is talking about different things

**adishee** · `12:34` — all i know is if there is a secret modded pvp tarkov society i DONT want to know about it

**Jpdarkone³** · `12:34` ↳@adishee — No there isnt

**Fontaine** · `12:38` — The idea is that if Fika is kept to coop PvE only then BSG won't litigate, so doing PvP serves would put Fika and SPT at risk

**PrescriptionAdderall** · `12:39` — They do exist, though I can’t imagine they’re particularly large.

**PrescriptionAdderall** · `12:40` — A PvP fight would probably be a rare occurrence outside of events or something.

**Fontaine** · `13:00` ↳@PrescriptionAdderall
  🖼️ 📎 [`att-11-2026-03-17-Fontaine.gif`](./assets/att-11-2026-03-17-Fontaine.gif)

**Archangelway** · `13:09` ↳@BraveStarr — Seeing you people give these dumb takes is very interesting, I would advise you to read into it more considering Fika is allowed on the forge

**Archangelway** · `13:09` — SPT just doesn't do any support for it here

**Archangelway** · `13:09` — And PVP is something it'll never support, there have been plenty of forks of Fika that tried to do it but ended up collapsing due to the toxicity of this community

**Archangelway** · `13:09` — amongst other reasons

**Archangelway** · `13:10` — (Some also making it a lootbox filled experience afaik  )

**BraveStarr** · `13:11` ↳@Archangelway — You may have taken me a bit too serious. LOL But to be fair I suck at making jokes even when I think I'm funny.

**Archangelway** · `13:12` — idk, with people having claimed this over and over I dont see what's a joke and not anymore in regards to this topic _(editado)_

**Qwertyalex** · `13:13` — I'm just here to glaze Realism, who even are you people /s

**adishee** · `13:20` — this is my fault

**Nosliw** · `13:22` — anyone here ever gotten to kappa on a realism profile?

**PrescriptionAdderall** · `13:23` — I have. I imagine you’re having an issue with some of the required quests? _(editado)_

**Nosliw** · `13:23` — no i was just trying to change the topic

**PrescriptionAdderall** · `13:23` — Ah.

**PrescriptionAdderall** · `13:23` — Uh, it’s a bitch. What else can I say?

**Nosliw** · `13:24` — which quest(s) were the most difficult?

**GrooveypenguinX** · `13:48`

**PrescriptionAdderall** · `14:26` ↳@Nosliw — Several were impossible, so I did have to use less than legitimate means.

**PrescriptionAdderall** · `14:27` — And I got it pre Streets expansion.

**PrescriptionAdderall** · `14:27` — I don’t remember the exact version.

**PrescriptionAdderall** · `14:27` — Capturing Outposts was awful.

**PrescriptionAdderall** · `14:28` — I gave up halfway through and started counting kills while I was at the locations.

**PrescriptionAdderall** · `14:28` — (E.G: If I’m at Fortress on Customs and I kill a PMC at Crackhouse I count it.)

**PrescriptionAdderall** · `14:29` — Wait, no.

**PrescriptionAdderall** · `14:29` — It wasn’t Capturing Outposts…

**PrescriptionAdderall** · `14:34` — I can’t remember the particular quest I had to cheat the most on, but I did cheat on Huntsman Path: Secured Perimeter.

**PrescriptionAdderall** · `14:34` ↳@PrescriptionAdderall — I did cheat on Capturing Outposts, it just isn’t required for Kappa.

**J3RN3J** · `14:45` — random, but is that one lightkeeper quest where you have to kill PMCs in the mountain area on woods even possible on vanilla PvE? with AI behavior altering mods, you'd be able to do it, i guess, but without...?

**J3RN3J** · `14:47` — from what limited time i have on vanilla BSG PvE i dont recall a single encounter with a PMC in the mountain area aside from the bunker at the back of the mount

**J3RN3J** · `14:48` — bunker spawns PMCs once in a blue moon

**PrescriptionAdderall** · `14:52` ↳@J3RN3J — I don’t know, I have literally never touched Vanilla Tarkov since Tagilla was added.

**PrescriptionAdderall** · `14:52` — I know it’s possible with SWAG+DONUTS.

**PrescriptionAdderall** · `14:53` — And you can probably bait PMCs into the area. _(editado)_

**B468** · `17:45` ↳@GrooveypenguinX — I was there.


## 19/03/2026

**Jpdarkone³** · `09:30` — Hello fontaine id like to request the fishing mod thank

**adishee** · `16:13` ↳@Jpdarkone³ — Only His genius could turn spt into an actual single player game

**ZuluFox** · `16:32` — Imagine an open world survival crafting game Tarkov

**Ser_G** · `16:48` — But then it'd just turn into Exist in Tarkov

**BraveStarr** · `20:04` — So I just read the Realism mod page and I am loving where Fontaine is taking the next iteration of Realism. Breaking up the mod into individual mods sounds awesome and creating a mod that can integrate them will also be awesome. I am looking forward to seeing his magic with this mod. I'm still crossing my fingers that SPT 5.0 is still achievable someday and that mods like this one will be updated for it as well.

**PrescriptionAdderall** · `22:44` — We're all waiting for 4.0 and this dude's already planning for 5.0.


## 20/03/2026

**Bradonium** · `01:30` — I feel like the closest we'll get to SPT 5.0 is if BSG actually deliver on their word for PVE Mods

**Bradonium** · `01:30` — but im like 99.9% confident its going to be visual mods only

**Bradonium** · `01:30` — Omg guys CJ from GTA san andreas is tagilla so wacky and zany!!!

**Fontaine** · `03:38` — SPT 5.0 will release within 24 hours of Realism 4.0 update

**adishee** · `04:03` — @Fontaine which config do i have to change to stop the hazard-zone-safe-spawn action? I presume I just need to remove the defined zones but there are a bunch of config files _(editado)_

**Fontaine** · `07:37` ↳@adishee — There are no zone specific configs, unless you mean the JSON files that define the zones. I don't remember if there's some flag or value that can be used to prevent being moved out of the zone on spawn or if it's fully hard coded but in future there will be a flag that can be set to prevent that behaviour

**Fontaine** · `07:37` — For now you'll have to delete the zone or modify its size/location

**Dashwood Foxe** · `07:41` ↳@Fontaine — Has there been any headways with reverse engineering EFT's new 1.0 shenanigans that made current modding virtually impossible? _(editado)_

**Guidot** · `07:42` — No.

**Dashwood Foxe** · `07:45` — That's what I thought

**Dashwood Foxe** · `07:46` — But either way, would rather have devs flesh out their mod than to play cat and mouse with every SPT micro-revisions _(editado)_

**Fontaine** · `08:40`
As far as I'm concerned the only significant thing from 1.0 is the story missions. The story missions themselves are mostly the same boring fetch quests and grind, with one scripted raid at the end...  
  
We can back port new items, and as for new features there isn't much and either SPT already has a mod for it that's better or we can make our own version of it _(editado)_

**Fontaine** · `08:41` — IMO a hypothetical SPT 5.0 wouldnt be worth potentially having no client mods. Better off trying to reverse engineer content and new features _(editado)_

**Slum_K1ng** · `09:38` — Interchange expansion is pretty good too.

**BraveStarr** · `10:31` ↳@PrescriptionAdderall — Allow a man to dream at least. I dream of winning the lotto also. Gonna take that away too?

**BraveStarr** · `10:33` ↳@Slum_K1ng — Yea, I'd be happy if we could just get this part of the map ported. Not sure if that's even possible but would awesome if so. I would even stop mentioning 5.0 if they could. LOL

**GrooveypenguinX** · `10:48` ↳@Fontaine — Which, btw, I believe I have a way of doing emulating their "receive a quest on item pickup" and "receive a quest on entering a zone" if/when the time comes to rewrite their jank

**GrooveypenguinX** · `10:58` ↳@BraveStarr — Man you started off so strong with this but the last sentence was wack

**BraveStarr** · `10:59` ↳@GrooveypenguinX — I tend to dream a lot. Not much else to do these days.

**Fontaine** · `11:50` ↳@GrooveypenguinX — That's be amazing. Especially if it was party of common lib or something so I could use it too

**GrooveypenguinX** · `11:52` ↳@Fontaine — I'm trying to make anything that has potential use to other people a part of commonlib, so for instance the salvage zones i made are being added to commonlib as well as hiding locked quests by using the "secretQuest" prop that is unused by SPT _(editado)_

**Qwertyalex** · `11:52` ↳@GrooveypenguinX — Even Realism could have CommonLib as a dependency? The conspiracy grows.

**GrooveypenguinX** · `11:52` ↳@Qwertyalex

**Fontaine** · `11:53` ↳@Qwertyalex — Realism will be a common lib too, common lib-ception

**Fontaine** · `11:53` ↳@GrooveypenguinX — What's a salvage zone?

**GrooveypenguinX** · `11:53` ↳@Fontaine — LeaveItemAtLocation except it gives you an item on completion

**GrooveypenguinX** · `11:54` — I'm also making the consumption optional too for more inventory shenanigans

**GrooveypenguinX** · `11:54` — So like, repairing the walls of factory with a toolset except on completion it gives you a PCB and optionally consumes the toolset

**GrooveypenguinX** · `11:57` — Oh! I also implemented grouppositions for zones too, so you can have RNG decide what zone is picked

**Chocolate** · `12:16` — Does realism model unburned powder on short barrels or am I tripping and it's just bigger muzzle flash?

**Jpdarkone³** · `12:36` ↳@GrooveypenguinX — Grooveys master plan is adding a backdoor to commonlib _(editado)_

**Fontaine** · `12:42` ↳@GrooveypenguinX — That's very cool

**Fontaine** · `12:42` — Lots of possibilities there, to make quests more interactive

**Fontaine** · `12:43` ↳@Chocolate — Yes the mod uses particle physics to simulate each individual particle of unburnt powder and factors in ambient temperature and altitude _(editado)_

**Jpdarkone³** · `12:46` ↳@Fontaine — So innovative

**Jpdarkone³** · `12:46` — Is this why eft runs bad are you behind it

**Jpdarkone³** · `12:47` — fontaine dammit

**Fontaine** · `12:53` ↳@Fontaine — Shorter barrels having more muzzle blast/flash is the same net effect as modeling unburnt powder and realism does do that

**GrooveypenguinX** · `12:53` ↳@Fontaine — Realism 4.0 will actually emit micro levels of radiation from your pc if you enter a rad zone

**Chocolate** · `13:08` ↳@Fontaine — well then it gives a decent impression at least, because I noticed without being aware of it. I assume an effects mod could add some kinda particle for the "sparks"

**Chocolate** · `13:09`

**Fontaine** · `13:13` ↳@Chocolate — EFT already has a sparks effect and realism increases the amount of sparks when using shorter barrels

**Fontaine** · `13:13` — Depending on calibre

**Fontaine** · `13:37` — Also muzzle devices affect flash etc too

**Fontaine** · `13:37` — Suppressors, gas blocks and certain charging handles affect amount of gas

**GrooveypenguinX** · `13:38` — Taco Bell also affects the amount of gas

**GrooveypenguinX** · `13:38` — Alright, enough shitposting in here for me

**Chocolate** · `13:56` ↳@Fontaine — how is it every time I think of a cool detail it's already implemented?

**PrescriptionAdderall** · `13:59` — Because the mod author has a vision of what he wants in the game he is tinkering with, where the developers of said game either lack said vision, talent, or motivation to make the game they want.

**Fontaine** · `14:55` ↳@Chocolate — I'm inside your walls

**Jpdarkone³** · `15:23` ↳@Chocolate — Fontaines a damm nerd...

**IvanTheThicc** · `17:08` ↳@Fontaine — This would mean that you're really in control of 5.0 coming out

**Fontaine** · `18:19`


## 21/03/2026

**Tebz** · `07:22` — mr fountain thoughts on grayzone warfares medical system?

**Tebz** · `07:23` — https://youtu.be/0veqn0Mptls?si=FFYNRO4F2b29k-9B

**Tebz** · `07:26` — i feel like it makws more sense

**Fontaine** · `12:37` — Care to elaborate?

**Fontaine** · `12:37` — I'm not very familiar with the game

**Tebz** · `12:50` ↳@Fontaine — so in the game theres no medi packs like in tarkov

**Tebz** · `12:50` — instead you have a blood bag and blood meter

**Tebz** · `12:50` — so basically when you get shot you lose an amount of blod _(editado)_

**Tebz** · `12:51`
so you  
  
Bandage your self  
,  
2, surgery kit  
  
Blood bag  
,

**Tebz** · `12:51` — the blood bag is basically the games hp _(editado)_

**Tebz** · `12:55`
  🖼️ 📎 [`att-12-2026-03-21-Tebz.jpg`](./assets/att-12-2026-03-21-Tebz.jpg)

**The_Gooch** · `13:30` — asking the realism guy what he thinks of giving yourself a blood transfusion in the middle of a firefight?

**Fontaine** · `13:54` ↳@Tebz — Blood loss system is basically the same thing as a HP system. Blood bag sounds like it does the same thing a medkit does in Tarkov.  But it sounds like a better medical system overall than Tarkov's.

**Tebz** · `13:55` — yes

**Fontaine** · `13:55` — I do have plans for some significant changes to realism's med system, blood loss will be part of it.  I'm not 100% sure what it'll look like yet. I'll look deeper into GZ to see if there are any interesting ideas

**Tebz** · `13:55` — if i have to be honest this system reminds me of realisms

**Fontaine** · `13:56` ↳@Tebz — Yeah a little bit

**Fontaine** · `13:58` — If you imagine HP represents blood it's not far off.  The system I'm thinking of is actually quite similar the more I read about it.

**Fontaine** · `13:59` — But from watching GZ gameplay I can't help but feel it's not as deep as they make it sound, TTK still seems low

**Tebz** · `14:09` — ttk?

**Mout Duck** · `14:10` — tbh gray zone seems more like tarkov but with more of a team focus

**Mout Duck** · `14:10` — from clips ive seen

**Mout Duck** · `14:11` — dont they like let you use chars in any mode, pve/pvp in gray zone too, with shared gear _(editado)_

**Mout Duck** · `14:12` — it seems like tarkov but without the cbt culture of making you play exactly how it wants you to and some hyper focus on making everyone follow some economy _(editado)_

**Mout Duck** · `14:22` — oh wowthis game has an ak19, ak15 and an ak12, that's already more dev progress than an entire year of tarkov

**Mout Duck** · `14:22`

**Acks** · `15:30` ↳@Tebz — Time to kill

**Dashwood Foxe** · `15:55` ↳@Fontaine — I know some games have actual IV animations for those kinds of heals, looks cool, not sure if they can be pushed into tarkov

**Dashwood Foxe** · `15:56` — they just tape a bloodpack onto their arm and jab a needle afterwards _(editado)_

**Guidot** · `17:58` — GZW has some shocking complexity if you aren't used to it.

**Jpdarkone³** · `18:51` ↳@Guidot — i cant lie, i havent played for a while but when i did i remember the game feeling really clunky

**Mout Duck** · `18:54`

**Mout Duck** · `18:54` — the base m4a1 is a norinco in it

**Travon** · `19:26` ↳@Jpdarkone³ — thats what i had remembered

**PrescriptionAdderall** · `20:24` — Blood in GZW is more akin to DAYZ, where you can be at full health, but have less than full blood.

**PrescriptionAdderall** · `20:24` — Somebody at 90% health and 100% blood will die just as fast as someone at 90% HP and 80% blood.

**PrescriptionAdderall** · `20:24` — It affects your operator in other ways, though.

**Mout Duck** · `20:25` — kinda curious do they ever put it on sale, dont think ive seen it when ive looked

**PrescriptionAdderall** · `20:25` — (I use “90%” because realistically you will never be at 100% health with some blood loss.)

**PrescriptionAdderall** · `20:25` ↳@Mout Duck — One time it went on sale in 2024.

**PrescriptionAdderall** · `20:26` — It actually increased in price after that.

**PrescriptionAdderall** · `20:27` — 34.99-39.99

**PrescriptionAdderall** · `20:28` — GZW is actually less complex to play with than Tarkov.

**PrescriptionAdderall** · `20:28` — The complexity is behind the scenes.

**PrescriptionAdderall** · `20:29` — It does model organs, and if you are hit there, you are pretty much dead instantly.

**riffofthegods** · `20:33` — it will go on sale soon according to a recent devblog

**Mout Duck** · `20:35` — yeah i was more asking as i heard there's a big update soon

**Mout Duck** · `20:35` — and wasnt sure if they did sales when they did those

**Tebz** · `21:02` ↳@Mout Duck — theyre about to go on sale on 27

**Mout Duck** · `21:02` — ill prob pick it up

**Mout Duck** · `21:02` — the pve gear shared with pvp sounds interesting

**Mout Duck** · `21:03` — it will prob make it not as bad of a sweat feast

**Mout Duck** · `21:04` — tho all the plate carriers are fictional which is odd

**Mout Duck** · `21:04` — when they use copyrighted names on other stuff

**Tebz** · `21:21` — yknow whats funny

**Tebz** · `21:21` — they have a better mosin reload

**Ser_G** · `22:19` — I just want to be able to use stripper clips for top load rifles

**Ser_G** · `22:20` — One of my larger complaints with Tarkov

**Mout Duck** · `23:24` ↳@Tebz — they actually have the full ak200 line

**Mout Duck** · `23:24` — the ak19, 12, 15 AND the 308


## 22/03/2026

**Kojimbooo** · `06:52`
  🎥 📎 [`att-13-2026-03-22-Kojimbooo.mp4`](./assets/att-13-2026-03-22-Kojimbooo.mp4)

**adishee** · `08:02` ↳@Kojimbooo — i noticed that FriendlyPMC has been taken down ...

**Fontaine** · `13:37` ↳@PrescriptionAdderall — Kind of sounds like just a different way to apply debuffs to player for having injuries/having low hp

**Fontaine** · `13:38` ↳@adishee — There's been a couple mods taken down lately...I don't get why someone would want to remove their mod. It sucks for players and the mod scene. If I ever quit SPT modding I'll leave my mods up

**PrescriptionAdderall** · `13:42` ↳@Fontaine — It is, while still giving the player similar TTK.

**PrescriptionAdderall** · `13:43` — You aren't at a health disadvantage and have all your stats cut.

**PrescriptionAdderall** · `13:43` — Which feels much better, in my opinion.

**Fontaine** · `15:19` ↳@Kojimbooo — sick

**Fontaine** · `15:20` — EFT is the dev's didn't have the maturity of 14 year old boys

**Snake** · `16:49` — Hello, any news on 4.0 release date?

**riffofthegods** · `16:58` — +2 weeks

**Fontaine** · `17:31` ↳@Snake — I'm working on it

**WTT | Eukyre 🍃** · `17:38` ↳@Fontaine — are you ok? there no fontaine sass in that message™ _(editado)_

**Jpdarkone³** · `18:36` ↳@Fontaine — no rush

**Bradonium** · `22:45` ↳@WTT | Eukyre 🍃 — Blink twice if you're being held hostage by BSG


## 23/03/2026

**Fontaine** · `04:56` ↳@WTT | Eukyre 🍃
  🖼️ 📎 [`att-14-2026-03-23-Fontaine.gif`](./assets/att-14-2026-03-23-Fontaine.gif)

**Frazzle** · `10:34` ↳@Kojimbooo — Can you share your modlist


## 24/03/2026

**pure aura 9999+** · `21:32` — Any news on realism?

**Johnathan Thiccolas's Alt** · `21:33` — scroll up like 6 messages

**pure aura 9999+** · `21:34` — Oh, thank you

**Jpdarkone³** · `21:49` ↳@pure aura 9999+ — tomorrow :hopium:


## 25/03/2026

**ash the proto** · `17:39` — realism is tomorrow ofc or maybe a tomorrow in the future

**Twank bwattewy** · `17:40` — it is deff gonna release on a "tomorrow"


## 26/03/2026

**Fontaine** · `03:23` — This I can confirm

**Vyckalino** · `04:29` — realism release confirmed!?!?!?!?!?

**adishee** · `05:12`
i mean it is technically thursday  
https://giphy.com/gifs/jim-carrey-dumb-and-dumber-so-youre-telling-me-theres-a-chance-ToMjGpKniGqRNLGBrhu _(editado)_

**Fontaine** · `06:37` — It's always a Thursday somewhere or something

**Fontaine** · `07:38`
Poll: do you prefer future modules are simply ported over as they are in 3.11 without improvements or new features until most of the other modules are ported over,  
  
Or do you prefer to wait for modules to be refactored, improved and new features added? _(editado)_

**Dyno** · `07:39`

**Fontaine** · `07:41` — This excludes work to make features standalone. In some cases that won't take much work, in other cases it'll take a lot of work (like with Stances)

**resonant** · `07:49` — Fontaine, could you clarify if #1 would need you to do wacky temporary stuff just to port over features and then redo everything in future updates because you were remaking it all from scratch anyways?

**resonant** · `07:53` — cause i genuinely refuse to play 4.0 without ballistics overhaul, so i vote 1, but if 1 impedes your work&productivity then i'd rather wait

**Fontaine** · `08:28` ↳@resonant — It would depend, case by case. Some features are already well isolated so porting them as is won't be too problematic, in other cases a lot of jank temporary stuff  would need be done

**Fontaine** · `08:30` — I suppose the major features other than stances would be ballistics, medical, hazards, attachments and recoil. Ballistics is fairly well isolated, medical and hazards will be a bit trickier to separate. Recoil and attachments would be very difficult to separate without doing lots of janky stuff

**GrooveypenguinX** · `08:32`
IMO rushing the releases is just going to cause you more headache down the road.  
  
Release them as they are finalized, either all at once or one at a time and tease the fuck out of your fanbase. This is the way

**resonant** · `08:36` ↳@Fontaine — from the votes on your patreon alone it's pretty obvious that vanilla tarkov ttk sucks, so is there a possibility you could port over ballistics as is, and then go by  philosophy from there? if not, all is well, you do you brother, i'll still gladly await the update. thank you for your amazing work.

**Robomilk** · `09:32` ↳@resonant — As a player currently waiting for realism 4.0, how does ttk and feeling change from vanilla to realism mod?

**resonant** · `10:07` ↳@Robomilk
low pen ammo stops equating to a nerf dart when you shoot at higher level plates  
of course, high pen ammo still retains its value and importance, its just that you cant survive a mag dump into the thorax anymore

**Nosliw** · `10:16` — leg meta also great in realism as leg shots cause enemies to fall to the ground

**Nosliw** · `10:17` — kedr is very good in realism

**PrescriptionAdderall** · `10:17` ↳@Robomilk — The gap between the worst (usable) ammo in Realism and the best is much narrower, and more nuanced.

**PrescriptionAdderall** · `10:18` — There is some ammunition that is just bad in Realism, most notably 5.45 US and flechettes.

**PrescriptionAdderall** · `10:18` — But aside from those, I would say every other ammo is at least usable.

**PrescriptionAdderall** · `10:19` — The moment you get FMJ rifle rounds you’re golden for the rest of the game.

**PrescriptionAdderall** · `10:19` — Sure, some rounds are suboptimal, like .45 ACP, but they can still kill.

**Fontaine** · `10:19` ↳@GrooveypenguinX — That is a distinct possibility, appreciate the encouragement

**PrescriptionAdderall** · `10:20` — And as Nosilw said, leg meta is much better. I would argue 5.56 Warmageddon is one of the best rounds in Realism.

**Fontaine** · `10:21` ↳@resonant — Yeah that's possible too, but long term a large portion of it needs to be rewritten

**BraveStarr** · `10:21` ↳@Fontaine — Think of it this way. We all waited for 4.0 with the same desperation we are waiting for Questing Bots and Realism, especially with the idea of individual mods that for one like Voltron. LOL.

**BraveStarr** · `10:21` — So as this song says: https://www.youtube.com/watch?v=RmY1TJ6B-0g

**Fontaine** · `10:22` ↳@Robomilk — As others said it closes the gap, but that's largely due to plate sizes being realistic and like how BSG first implemented it. Plates themselves can be very resistant to low pen and low mass ammo, but there's a good chance of rounds bypassing plates

**Jpdarkone³** · `10:45` ↳@Fontaine — IMO i would rather simply they are ported over as is because 1 i think it would be faster and allow you to improve them soon while still allowing us to use them and 2 it gives modders more time to familiazire themselves with the modules and implement them sooner also being able to make addons to them sooner i suppose _(editado)_

**Jpdarkone³** · `10:46` — And i dont mean rush the release but rather polish it first with a release and bug reports then add onto it and get real time feedback

**Jpdarkone³** · `10:48` — Tldr i want to play realism and this gives opportunities for the mod to be tested before things are added onto it also allows for the community to bug test new features and give feedback if they impact positively or not

**Jpdarkone³** · `10:50` — And again modders could expand the modules furthrr earlier which means even more content _(editado)_

**Jpdarkone³** · `10:51` — And i think it would be less pressure to put something out than keeping it on hold for so lobg

**Jpdarkone³** · `10:54` — Unless the new features part means like rewrite stuff and not like add new content in that case my argument might be useless so

**Jpdarkone³** · `10:54` — Well if anyone wants to write their thoughts about what i said please do

**Nosliw** · `10:55`
"it gives modders more time to familiazire themselves with the modules and implement them sooner"  
  
this arguement lends itself more to letting fontaine refactor the modules and release them in a polished state

**Jpdarkone³** · `10:56` — Well when i read 2 i thought it was more about adding new features than refactoring the mod as the mod pretty much needs to be refactored to be modules anyways

**Nosliw** · `10:56`
"i dont mean rush the release but rather polish it first with a release and bug reports then add onto it and get real time feedback"  
  
this also sounds liek you're saying he should take his time and develop the refactored, polished version and let people test the RCs to get feedback

**Nosliw** · `10:57`
"also allows for the community to bug test new features and give feedback if they impact positively or not"  
  
new features would only be included if he releases later

**Jpdarkone³** · `10:57` — New features is part of the second route

**Nosliw** · `10:58` — yes but you said to release earlier to test new features which doesn't align with what fontaine said

**Nosliw** · `10:58` — a lot of what you said aligns with the "wait and release a polished good"

**Jpdarkone³** · `10:58` — I mean when eventually new stuff is added we can actively test it

**Nosliw** · `10:58` — not with getting out a release quickly

**Jpdarkone³** · `10:58` ↳@Jpdarkone³ — Cus it will be out

**Nosliw** · `10:59` — but getting it out sooner doesn't matter in that case

**Jpdarkone³** · `10:59` — So like we dont have to rely on the release to see what got changed and how it affected gameplay

**Nosliw** · `11:00` — but whether he releases early or later, the new content doesn't come until later

**Jpdarkone³** · `11:00` ↳@Nosliw — Thats what im saying

**Nosliw** · `11:00` — so don't see how it is an arguement to release early

**Jpdarkone³** · `11:00` — If it comes later and the mod is out we can like test the changes begorehand

**Nosliw** · `11:00` — what changes?

**Jpdarkone³** · `11:00` — If he goes the wait route he will release the stuff with NEW features aleardy

**Jpdarkone³** · `11:01` — Possibly skipping the entire community testing features part

**Jpdarkone³** · `11:01` — Though maybe we can find a middle ground but yeah we need to wait for fontaine

**Nosliw** · `11:02` — i don't think fontaine has ever skipped community testing

**Nosliw** · `11:02` — there's usually like 3-4 RCs, no?

**Jpdarkone³** · `11:02` — I suppose but its a concern that like things might aleardy be implemented in ways that are a pain in the ass to maybe remove and the rcs end up as simply polish

**Jpdarkone³** · `11:02` — But idk

**Jpdarkone³** · `11:02` — Its a possibility

**Nosliw** · `11:04` — sorry i'm having a hard time understanding your point

**Jpdarkone³** · `11:13` — Im busy so my point might have came out as confusing

**Fontaine** · `15:32` ↳@Nosliw — Each module is gonna be in testing for a while

**Jpdarkone³** · `15:35` ↳@Fontaine — I have no reading comprehension and i must complain in support when testing phase  breaks

**Fontaine** · `15:39` — To clear up the confusion, the first route is doing the bare minimum to get it working and not rework any existing features or add anything new. It'll be more or less the same as it is in 3.11, copy pasting as much as possible

**Fontaine** · `15:39` — 2nd route is slowly refactoring, reworking features and adding new stuff.

**Fontaine** · `15:40` — To take ballistics for example, I plan to modify and add more armor boxes (if it's even possible) and model organs and such more deeply

**JamesCheese** · `15:49` — 4.0 isn't going anywhere. whenever it's ready!

**Archangelway** · `15:51` — Yes it will

**Archangelway** · `15:51` — we're releasing 4.1 the day realism updates

**Archangelway** · `15:51`

**J3RN3J** · `16:30` — Me when I'm in an unfortunate timing competition and my opponent is the SPT modding and development community

**J3RN3J** · `16:32` — I'm pretty sure I recall an instance of SPT being updated  and, within the week, an official announcement from BSG about a big content update

**J3RN3J** · `16:32` — Might have even happened twice.

**J3RN3J** · `16:32` — There are BSG spies in our community.

**J3RN3J** · `16:33`

**Kojimbooo** · `17:39` — I swear BSG copied the faster chamber checks from Realism

**Dashwood Foxe** · `18:48` ↳@Archangelway — considering how eft 1.0 has drastically changed how clientside processes work and what not and all the C muckery, It would undo everything that realism has built. _(editado)_

**Dashwood Foxe** · `18:48` — if SPT were to update in parity with 1.0

**Dashwood Foxe** · `18:49` — realism would effectively start back to square one

**GrooveypenguinX** · `18:55` ↳@Dashwood Foxe — 4.1 is not 1.0 content

**GrooveypenguinX** · `18:56`
4.1 is the final client version BEFORE 1.0  
And also deobfuscation of the client from dumps

**Dashwood Foxe** · `18:56` ↳@GrooveypenguinX — Oh good

**Fontaine** · `19:44` ↳@Kojimbooo — they did what?


## 27/03/2026

**Kojimbooo** · `09:06` ↳@Fontaine — The AR had a long chamber check animation, but in 1.0 it got updated to be fast like Realism's faster one

**Fontaine** · `09:23` — Eh I don't think it's inspired from Realism mod. Their newer weapons all have faster/better chamber check animations

**Ser_G** · `15:26` — Old chamber checks were akin to chilling on the range with a cold one

**Frazzle** · `19:33` — all the old animations are really slow


## 28/03/2026

**Snake** · `10:12` — Is there a feature in Realism mod or some other mods to quick draw handgun without taking away your main gun? Like in ABI

**Vyckalino** · `11:01` — no

**Joe** · `12:59` ↳@Snake — No, i think it is because people would have to animate in gun slings and animations of dropping the gun and pulling out the pistols quickly.

**Snake** · `13:11` ↳@Joe — got it thx, it's a pity cause it's a cool feature there

**Fontaine** · `13:56` — It'd require a lot more than just animations, it's not feasible


## 29/03/2026

**Twank bwattewy** · `06:42` — Dunno if this is the right place to ask but. Does Realism affect your skill experience gains? or adjust anything involving leveling skills?

**AelitaLyo** · `11:34` ↳@Twank bwattewy — If my memory is correct, no. Closest thing you might get is the medical changes maybe changing what skills get xp but even that doesn't seem likely from my memory.

**JamesCheese** · `12:28` — you're going to make a lot of metabolism levels because of how xp gain is calculated over time. but that's not a very impactful skill

**Twank bwattewy** · `12:37` — nah i wanted to see if i could find a way to speed up weapon mastery and covert movement. Takes super long

**riffofthegods** · `16:09` — SVM has skill multipliers


## 30/03/2026

**Fontaine** · `11:16` — Medical changes affect how medical related skills are levelled due to changing how healing works. Same for eating and drinking

**Fontaine** · `11:17` — I also give XP points towards troubleshooting skill when manually chambering if I recall

**Fontaine** · `11:17` — Maybe a few other cases where I give skill XP

**Fontaine** · `11:17` — But yeah just use SVM

**fkndeceased** · `14:45` — lately ive been trying to replicate realisms trader economy on 4.0 thru manipulating shit in SVM and one other mod 'RZcustomEconomy'. and while i have neutered the trader stock a good bit, its still no where near as good as how realism treats it. any tips for how i may recreate realisms trader economy on 4.0? (hope im asking in the appropriate channel. and ty for all the effort u put in fontaine i love ur work)

**adishee** · `17:16` ↳@fkndeceased — Just have to wait


## 01/04/2026

**HELLSANGEL** · `16:58` — Plot twist, stances release today and it’s not a joke

**Ronin117** · `22:35` — wow you suck

**Ronin117** · `22:35` — I read that and got so excited for a milisecond

**Ronin117** · `22:35` — actually got me lmao

**23x75mmR** · `23:03`
It would have been fun if Fontaine tricked us with a release today...  
...ha


## 02/04/2026

**Joe** · `13:01` — thats the trick

**Joe** · `13:01` — he made you think that there would be one

**Nosliw** · `13:32` — i appreciate fontaine's silence on april fools _(editado)_

**Fontaine** · `15:27` — I'm on a two week vacation

**Vyckalino** · `16:09` — Vacation ?!?!?! Unacceptable, you should be ashamed of yourself.

**szade** · `18:32` ↳@Twank bwattewy — im pretty sure it also levels your endurance and strength at the same timer

**Jpdarkone³** · `19:56` — Vacation?? he needs TO WORK ON REALISM RIGHT NOW FOR 4.0

**The_Gooch** · `20:15` — lord give me the patience of an spt modder

**GrooveypenguinX** · `20:20` ↳@The_Gooch — And say unto ye, 1000 JP's was sent to test Jobtaine faith and he did not falter

**The_Gooch** · `20:20` — one must imagine fontainisyphus happy


## 03/04/2026

**Deverted** · `01:02` ↳@GrooveypenguinX — I read this as J'obtaine


## 04/04/2026

**Dashwood Foxe** · `13:17` — https://youtube.com/shorts/epQDixjXMos @Fontaine Realism 5.0

**ᴹᶜᴰᵉʷᵍˡᵉ** · `14:27` ↳@Dashwood Foxe — I remember this animation was based on something that happened to the animator in real life when shooting an mp5 xD

**Dashwood Foxe** · `15:50` ↳@ᴹᶜᴰᵉʷᵍˡᵉ — orly? that's amazing lmao

**ᴹᶜᴰᵉʷᵍˡᵉ** · `15:51` — I think he said it was a knock-off MP5

**Dashwood Foxe** · `15:52` — an SP5?

**Dashwood Foxe** · `15:53` — the most commonly used badguy weapon in hollywood apparently _(editado)_

**Twank bwattewy** · `19:02` — Probably an obvious answer to this question but does Recoil rework conflict whit Realism? And if so, is there a way to make it work ?

**Kojimbooo** · `21:39` ↳@Twank bwattewy — You can use both but it does some funky stuff


## 05/04/2026

**Fontaine** · `05:40` — Yup. Recoil rework is basically the same thing as realism's recoil just tweaked differently with additional camera recoil in top. You can mess with both configs to get it sort of working or just tweak realism's recoil with advanced config to get mostly the same thing

**BL4CK_3ST3R** · `12:23` ↳@Fontaine — Which one of Recoil Rework? There are the original one (legacy) and the rewrite. I gave both a try and found that, on the surface, they gave different results.


## 06/04/2026

**Kojimbooo** · `02:19` — Legacy with camera recoil cranked up and auto correction disabled feels really good

**Rainn** · `02:26` — are night time bot loadouts managed by realism when its installed? they aren't spawning with flaslights/nvg _(editado)_

**Fontaine** · `06:44` ↳@Rainn — Yes, if they aren't then another mod is interfering with it or you disabled bot loadout changes

**Nosliw** · `21:02` — anyone here successfully using ABPS with Realism?


## 07/04/2026

**Rainn** · `00:50` ↳@Fontaine — maybe sain?

**Qwertyalex** · `00:55` ↳@Rainn — SAIN only handles how the bot AI works, this would be something like Progressive Bots, or something else that changes their equipment

**Rainn** · `00:57` — i see, the only thing i could think of is im using the onetimezone mod and it might be interfering with the detection of night time for bot loadout selection

**Qwertyalex** · `00:59` — That could potentially, it's also worth checking if the night settings are enabled in the realism config program (should be in the realism folder)

**Rainn** · `01:05` — I have bot changes and bot loot changes if that's the same thing, I don't see any dedicated night settings

**Qwertyalex** · `01:15` — Bot changes is the setting, ye. And then Headgear Conflicts under the Realism and Ballistics tab should also be turned on

**Fontaine** · `06:18` — You don't need ballistics or headgear conflicts enabled for bots to have NVG. Time related mods most likely it. You can always just test realism on its own when having issues ...


## 08/04/2026

**Twank bwattewy** · `04:59` — is there a way to change the Comfort modifier on a specific backpack? I like the takedown sling bags (visually) but dont really see why they have -5%

**Dashwood Foxe** · `09:35` ↳@Twank bwattewy — SVM.. is that still a thing?

**Twank bwattewy** · `09:37` ↳@Dashwood Foxe — SVM does not allow me to edit any Realism related features

**Dashwood Foxe** · `09:38` — I remember SVM had some pseudo LUA interface to inject settings

**Dashwood Foxe** · `09:39` — Like launch options

**Dashwood Foxe** · `09:39` — And you can forcibly make changes to items if you knew the exact layout

**Fontaine** · `14:03` ↳@Twank bwattewy — Because bags like that suck for carrying any actual weight in real life

**Fontaine** · `14:03` — Either way just edit the stats for it in the mods server folder

**Twank bwattewy** · `14:05` ↳@Fontaine — i know but they look cool   thanks tho


## 09/04/2026

**Mout Duck** · `01:39` ↳@Tebz — off topic, just gonna throw this and say ive been ruined. Bought grayzone at sale and this just feels so much nicer to play, minus the ai, than tarkov lol. Its hard to get back into spt. _(editado)_

**Mout Duck** · `01:39`

**Mout Duck** · `01:41` — the movement and aiming just feels so much nicer than tarkov, its like comparing a sports car to a tank when it comes to reponsiveness of controls.

**Mout Duck** · `01:45` — I've never realized just how unresponsive the controls are in base tarkov till I tried this and how much the maps with extracts breakup the gameplay in a bad way.

**Tebz** · `01:46` ↳@Mout Duck — duck accepted my friend request bih

**Tebz** · `01:46` ↳@Mout Duck — Ikr

**Tebz** · `01:46`
like this game ruined tarkov for  
me

**Tebz** · `01:46` — btw what faction didnyou join

**Mout Duck** · `01:46` — tarkov DOES have some QoL i'd like of it, which is why im playing spt a bit.

**Mout Duck** · `01:46` ↳@Tebz — crimson shield of course

**Mout Duck** · `01:46`

**Tebz** · `01:47` ↳@Mout Duck — FUCK

**Tebz** · `01:47` — scum

**Tebz** · `01:47` — join Mithras

**Tebz** · `01:47`

**Mout Duck** · `01:47` — tbh faction dont matter too much given the clothing isnt faction specific yet

**Mout Duck** · `01:47` — I wouldn't mind if they did faction specific traders and made the factions distinct

**Mout Duck** · `01:48` — substance painter ass bdu shirt omg _(editado)_

**Tebz** · `01:48` ↳@Mout Duck — nah inwanna say we can play

**Tebz** · `01:48` — you fant makes squad from a different faction unlike tarkov

**Mout Duck** · `01:48` — im like level 30 boss, i aint wiping om

**Mout Duck** · `01:48`

**Tebz** · `01:49` ↳@Mout Duck — DAMN YOU HAVE BEEN PLAYING

**Tebz** · `01:49` — how much money you have?

**Mout Duck** · `01:49` — 80k

**Mout Duck** · `01:49` — i keep trying out cases

**Tebz** · `01:49` — fair

**Mout Duck** · `01:49` — and learn they're shit cause you cant preview sizes

**Tebz** · `01:49` — i have 800k

**Tebz** · `01:49`

**Mout Duck** · `01:49` — the pelcan case for weapons are like

**Mout Duck** · `01:49` — 60 slot

**Mout Duck** · `01:49` — they FUCKING SUCK

**Tebz** · `01:49` ↳@Mout Duck — yeah its not alot

**Tebz** · `01:50` — gonto midnight sapphire

**Mout Duck** · `01:50` — i might as well just use the backpacks instead haha

**Tebz** · `01:50` — you get money very quickly

**Tebz** · `01:50` — lmao

**Mout Duck** · `01:50` — honestly i just sell ak12s off one of the army checkpoints _(editado)_

**Mout Duck** · `01:50` — I can carry like 4 and its 2k a pack

**Tebz** · `01:50` ↳@Mout Duck — but fr this game needs SAIN leven of ai and it will be perfect

**Tebz** · `01:50` — what i like about GZW is that its not 90% menu

**Mout Duck** · `01:51` — supposedly the ai is mainly server resource limited

**Tebz** · `01:51` — you’re actually in the world most ofnthe time

**Tebz** · `01:51` — the game ruined tarkov for me

**Mout Duck** · `01:51` — it looks nice too generally, has tarkov like degraded in graphics recently, I booted up current spt and it just looks mega ass

**Tebz** · `01:52` — Dude

**Tebz** · `01:52` — it runs better

**Tebz** · `01:52` — even though theres shit ton of foliage

**Tebz** · `01:52` — like idk whay they did its way to smooth

**Mout Duck** · `01:52` — i swear to god, the tarkov textures and lighting didnt use to look this bad

**Tebz** · `01:52` ↳@Mout Duck — i cant imagine them having to render a rainforest

**Tebz** · `01:52` — because its way too smooth to have rhis much planys

**Tebz** · `01:53` — also you do pve or pvp?

**Mout Duck** · `01:53` — honestly mainly what it needs is jamming, that thing bsg added were headsets had unique fits per headwear so you can wear comtacs and not have it clip on the boonie and dunno, the peqs actually slot ontop of guns

**Mout Duck** · `01:53` ↳@Tebz — what do you think, i play spt haha

**Tebz** · `01:54` — pvp is fun but pnly with a group

**Tebz** · `01:54` — because the game isnt designed for pvp

**Tebz** · `01:54` — but injust wish the ai are better

**Mout Duck** · `01:54` — dont worry the streamers are all pushing to make pvp focused additions already and incentives to play pvp lol

**Mout Duck** · `01:54`

**Tebz** · `01:54` ↳@Mout Duck — lmao

**Tebz** · `01:55` — thank goodness pve inventory transfer to pvp

**Tebz** · `01:55` — unlike tarkov

**Mout Duck** · `01:55` — ive seen a few suggest that FoB control should determine whether you can use LZs _(editado)_

**Tebz** · `01:56` ↳@Mout Duck — really?

**Mout Duck** · `01:56` — as a way to enforce pvp lol

**Tebz** · `01:56` — ah

**Tebz** · `01:56` — not for pve

**Mout Duck** · `01:57` — honestly main thing i have an issue with is headsets clip with too much headwear

**Mout Duck** · `01:57` — there's actually a reason to wear a boonie hat in this game and it clips a ton with headsets

**Tebz** · `01:57` — yeah

**Mout Duck** · `01:57` — also the clothing kinda looks bad, mainly the woodland stuff and the multicam is all over the place

**Tebz** · `01:58` — and the audio sucks

**Mout Duck** · `01:58` — I think its cause they're using purchased assets for their lshz

**Mout Duck** · `01:58` ↳@Tebz — honestly it has better directional audio than tarkov when i tried it today

**Mout Duck** · `01:58` — but that's not saying much, because tarkov's audio is like mega mega fucked now from what I can tell _(editado)_

**Mout Duck** · `01:59` — I can't tell the direction ANYTHING is coming from in it at all anymore

**Tebz** · `02:00` — lmao

**Tebz** · `02:01` — but yeha

**Tebz** · `02:01` — lets hoep the updates are good

**Mout Duck** · `02:01` — well they only update every half a year so hahaha

**Mout Duck** · `02:01` — tbh what tarkov has going for it still is the headwear system and the better weapon line up

**Tebz** · `02:02` — yeah

**Mout Duck** · `02:03` — well thing is they actually are paying licensing fees FOR everything they add supposedly in grayzone, unlike bsg _(editado)_

**Mout Duck** · `02:03` — so yeah

**Tebz** · `02:03` — and better customisation

**Mout Duck** · `02:03` — id say their clothing system is a better step, just it needs more clothing and better camo swatches

**Mout Duck** · `02:04` — the multicam crye g3s are abysmal

**Mout Duck** · `02:04` — they dont even match the shirt

**Tebz** · `02:06` — k more meant weapon customisation

**Mout Duck** · `02:19` — i mean they have universal ak12/15/19 handguards unlike bsg so i give them credit there

**Mout Duck** · `02:19` — and the short vs long handguard

**Mout Duck** · `03:18` ↳@Tebz — Tbh they need some more vietnam style guns given they seem to be going for that with the tcu and xm177 muzzle

**Jpdarkone³** · `07:13` — Gzw when i played felt clunky that + not being able to squad up with diff factions is braindead

**Nosliw** · `07:56` ↳@Jpdarkone³ — Did you play 0.4?

**Mout Duck** · `08:19` — Tbh the whole squad thing is entirely due to intentions with pvp so im not sure why its active in pve

**Mout Duck** · `08:24` — Other than stuff getting messy due to faction specific quests

**Jpdarkone³** · `09:39` ↳@Nosliw — Not rlly i was kinda put off due to playing at launch

**Nosliw** · `09:39` — you should probably reserve judgement then

**Jpdarkone³** · `09:41` ↳@Nosliw — "When i played" indicating it was in the past though

**Nosliw** · `09:42` — every time you played was in the past

**Nosliw** · `09:42` — "when i played" could be yesterday for all i know _(editado)_

**Nosliw** · `09:43` — anyway this is all off topic

**Guidot** · `14:30` — Comparing 0.1 to 0.4 is impressive.

**Fontaine** · `15:00` ↳@Mout Duck — What about the aiming is unresponsive in Tarkov? Is it better with realism mod stances and fov fix?

**Mout Duck** · `15:00` — oh yeah realism feels a lot better

**Mout Duck** · `15:00` — but idk

**Mout Duck** · `15:00` — it just feels like steering a tank in vanilla

**Mout Duck** · `15:01` — like i can't do what i want to do

**Fontaine** · `15:01` — Also Jesus go to DMs lmao

**Fontaine** · `15:01` ↳@Mout Duck — Could be the slowness and inertia

**Mout Duck** · `15:02` — i cant climb what I think I should, I cant move around objects how id expect _(editado)_

**Mout Duck** · `15:03` — if that makes sense, its hard to explain. _(editado)_

**Mout Duck** · `15:03` ↳@Fontaine — lol sorry about that, I followed it up here cause the original grayzone talk was here

**Mout Duck** · `15:03`

**Mout Duck** · `15:05` — the climbing in grayzone is very nice tho, and really helps smooth out mobility for sure in a way tarkov doesn't _(editado)_

**Mout Duck** · `15:05` — you can climb basically anything within half a foot above your head _(editado)_

**Mout Duck** · `15:05` — and vault over nearly anything

**Mout Duck** · `15:06` ↳@Fontaine — Tarkov movement just feels like a constant fight to try to get my char moved where I want and around obstacles, its weird to explain.

**Nosliw** · `15:08` — inertia tings

**Mout Duck** · `15:08` — the movement just doesn't handle how it feels like it should in vanilla, even before inertia was added imo _(editado)_

**DrakiaXYZ** · `15:17` ↳@Fontaine — Fontaine wakes up, "Why are there 200 messages in my thread...?"

**Mout Duck** · `15:17`

**GrooveypenguinX** · `15:18` ↳@DrakiaXYZ — Hey guys what's your thoughts on elden ring?

**Mout Duck** · `15:20` — playing spt without realism has reminded me why I play with realism...

**Mout Duck** · `15:20`

**Mout Duck** · `15:20` — and how crappy the base med system is

**Mout Duck** · `15:23` — its kinda shocking with how weirdly specific tarkov is about certain stuff, while leaving its med system how it is at the same time. _(editado)_

**Jamjom** · `15:42` — they put more effort into placing trees in front of every possible useful sightline than they did any of the survival aspects of their tactical survival game

**Nosliw** · `15:50` — ya'll can still play realism

**Nosliw** · `15:50` — i've been playing my 3.10.5 install lately and loving it

**Mout Duck** · `16:04` — its hard to play last patch when you've seen the new headgear system

**Mout Duck** · `16:04`

**Mout Duck** · `16:04` — comtacs not clipping on my USEC hats kinda crazy

**Mout Duck** · `16:12` — or am I remembering the wrong ver and 3.11 has it huh seems _(editado)_

**Mout Duck** · `16:18` — 3.11 SHOULD have that and factory rework now that I look.

**Nosliw** · `16:25` — Yes it does

**Travon** · `16:28` ↳@GrooveypenguinX — nightreigns been lovely

**Snake** · `16:29` ↳@Jpdarkone³ — as all slaves do

**Jpdarkone³** · `16:36` ↳@Fontaine — it feels sorta slow and then you use a long range sight and sometimes turning is ass

**Travon** · `16:50` — i think thats sensitivity settings i could be wrong though

**Jpdarkone³** · `20:27` ↳@Travon — No

**Jpdarkone³** · `20:27` — I mean for the sight example

**Jpdarkone³** · `20:27` — Its the dogshit sight picture


## 10/04/2026

**Fontaine** · `13:15` — I want to rework how I handled ADS speed so that the slowest speed is much faster, but there will be more movement or sight misalignment during the ADS animation

**Nosliw** · `13:30` — that'll feel much more natural i think

**Nosliw** · `13:31` — and better for reactive shooting - at least the optic would be partially visible as it sways rather than slowing down as the sight comes up

**Jpdarkone³** · `15:38` ↳@Fontaine — I always thought ur implementation was nice and satisfying to use

**Fontaine** · `16:01` — It'll be that but better

**SplatRash** · `22:40` ↳@Fontaine — Excited to see that in action. Although from a balancing point of view it makes sense, it always felt frustrating to have it feel so slow when in the middle of a fight


## 11/04/2026

**adishee** · `16:41` ↳@Jamjom — realistic tactical something something


## 13/04/2026

**dyyl** · `15:43` ↳@Travon

**Javirare** · `21:07` — Anyone got a good alternative to Amand's graphics?

**Guidot** · `21:25` — hollywood


## 14/04/2026

**Lega** · `15:07` ↳@Javirare — Hollywood with some tweaking and a reshade got me some good results

**Javirare** · `15:21` — Yea I found a setting on it that removes the fog on maps and honestly that’s the only thing that the game needs

**Mout Duck** · `21:07` — oh so that's what removes to fog

**Mout Duck** · `21:07` — removing the fog sucks

**Mout Duck** · `21:07` — cause you can see how shit the game actually looks, actually more so since they downgraded all the textures again _(editado)_

**Mout Duck** · `21:07`

**Mout Duck** · `22:01` — no really tried newest spt and peeked at live

**Mout Duck** · `22:02` — they've REALLY changed the textures in last few big updates

**Mout Duck** · `22:02` — you can see the pixels on the paca now


## 15/04/2026

**Dashwood Foxe** · `09:06` ↳@rrrrrrrrr — Vodka goggles

**Dashwood Foxe** · `09:07` — "fog"

**rrrrrrrrr** · `12:00` — they've really messed with stuff recently tho

**rrrrrrrrr** · `12:01` — even the inventory model of your char just looks bad now

**Dashwood Foxe** · `20:57` — maybe it's time for me to try gray zone

**Dashwood Foxe** · `20:57` — friends been pestering me to buy


## 16/04/2026

**kannax** · `10:07` — i dreamt that realism was out and got dissapointed when i checked the forge

**Nosliw** · `10:08` — i've been playing my 3.10.5 install and loving it

**Nosliw** · `10:12` — sounds like many people in this thread must nuke their old install as soon as SPT updates and then wait for next realism version to drop lol

**Twank bwattewy** · `10:28` — 3.11 is available and you can just play realism whit that

**Nosliw** · `10:33` — 3.11 doesn't have That's Lit though

**S41elite** · `10:35` ↳@Fontaine — so, as in TarkovIRL mod does?

**Fontaine** · `11:44` ↳@S41elite — I'm not fully sure how TarkovIRL does it but I'll probably animate it or use some other technique

**Fontaine** · `11:45` ↳@kannax — There won't be a realism modbfor 4.0, instead the features of Realism will be standalone mods essentially, and released one at a time

**kannax** · `11:45` — Pretty neat, I always mainly used the ballistic/ammo overhaul regardless, can't really find anything like it currently

**Fontaine** · `11:46` — I'm sure someone will release some blatant half baked janky copy of those features eventually, as is the way with SPT

**Fontaine** · `11:47` — But I've big plans for those features, it'll be amazing if it works

**Javirare** · `14:12`
None of that stuff really affects bots though right? Do they operate on the same movement & ads changes as well?  
I think it does for inertia right?

**Nosliw** · `14:50` — That's why Adi Bots exists _(editado)_

**Jpdarkone³** · `15:14` ↳@Fontaine — Janky mentioned


## 17/04/2026

**Fontaine** · `06:44` ↳@Javirare — What stuff? Ballistics does of course. Bots in games aren't subject to things like player ADS animations and timing

**Dashwood Foxe** · `09:48` ↳@Fontaine — Or weapon spread and recoil, that's all determined by the defined game limits of the bot as a whole or through your favorite bot mod I think _(editado)_

**Dashwood Foxe** · `09:49` — This sometimes explains why bots can cheese

**Dashwood Foxe** · `09:49` — You can make a bot have laser accuracy with a ppsh for example

**Dashwood Foxe** · `09:51` — SPEAKING of which, @Fontaine will there be an inclusion of the ergo/handling of their kit in future updates of realism

**Dashwood Foxe** · `09:51` — For bots

**Fontaine** · `10:35` — bots aren't subject to weapon and handling because they don't operate with player inputs like a human does, it's all determined by bot behaviour and that's the realm of sain

**Dashwood Foxe** · `10:45` — I know they aren't, but can they?

**Dashwood Foxe** · `10:46`
  🖼️ 📎 [`att-15-2026-04-17-DashwoodFoxe.gif`](./assets/att-15-2026-04-17-DashwoodFoxe.gif)

**JamesCheese** · `12:37` — that sounds like an incredible pain to design bot loadouts that conform to stat requirements in a balanced way lol

**adishee** · `14:07` ↳@Dashwood Foxe — wut

**Dashwood Foxe** · `14:07` — mod the bots to account for actual gear they have

**Nosliw** · `14:23` — anything is possible


## 18/04/2026

**Twank bwattewy** · `07:06` — I  might be missing something. But 7.62x51 feels very.... underwhelming to use. slower RPM and higher recoil on most of is weapons and then pen/damage issent that much better if you consider all those factors compared to 5.56 or 5.45. Please tell me i'm doing something wrong, i genually wanne know

**S41elite** · `08:28`
7.62x51 should have overall better dmg and a lot more pen than 5.56/5.54. Of course its going to have way more recoil and less RPM, the cartridges are WAY bigger, thus the gun recoiling more and the gun needing more time to cycle, so yeah, its normal.  
Unless you present a video showcasing the same stance, distance, elevation against an AI target, its very dificult to say.  
IMO, 7.62x51 should 1 or 2 tap enemies on the chest, whereas 556/554 needs 3+ depending on ammo.

**S41elite** · `08:29` — take into account, RM implement different damage zones, as oposed of the usual arms, legs, torso and head.

**S41elite** · `08:29` — so, you might have hit a zone designated as hand, then arm, then torso, thus the damage reduction too

**S41elite** · `08:30` — If i am mistaken, i hope mod author Fontaine can enlight us both

**S41elite** · `08:31` ↳@JamesCheese — doesnt phobos or APBS do that?

**Fontaine** · `09:42` ↳@Twank bwattewy — Are you looking at the stats to decide it's underwhelming or based on gameplay? The TTK is a lot lower

**Twank bwattewy** · `09:50` ↳@Fontaine — bit of both tbh. I feel like the TTK is inconsistent, sometime is 2-3 tap a dude in the chest and sometimes i have to hit him 7-8 times whit them both having lvl 7 plates while using M80. The recoil controll needed and the firerate make it feel like i'm better of using any other full auto 5.56 or 5.45 rifle whit either BT or SoST. Wich by firerate and recoil alone would make a kill faster and eassyer. I will say it could just be in my head. I'll play around more whit 7.62x51 and see how it feels

**PrescriptionAdderall** · `11:14` — I would run it constantly in Labs without issue.

**PrescriptionAdderall** · `11:14` — On a shorty SA-58, no less.

**PrescriptionAdderall** · `11:15` — It can get absorbed by plates, but you’ll either break through or land a shot on soft armor, and then it’s game over.

**PrescriptionAdderall** · `11:17` — 5.56 SOST is debatably the best overall round in the game, but that’s no reason to rag on M80.

**Twank bwattewy** · `11:20` — i dont wanne come across as "ragging" on it but its just feels underwhelming to me. Like i said, i'll run it more and come back to it after a couple of hours of playing whit it. Thanks for the responses tho

**PrescriptionAdderall** · `11:21` — 5.56 is just very good, which is realistic.

**Fontaine** · `11:26` ↳@Twank bwattewy — You're not supposed to use .308 in full auto. M80 is the equivalent of FMJ for 5.56, it's a basic round

**Fontaine** · `11:27` — Realism has variable damage zones on the chest so shot placement is important. You won't likely pen lvl 7 plates with M80, not at all of it's steel. Also in full auto you'll have big spread and recoil bloom so you're likely not only hitting the chest

**Fontaine** · `11:30` — I appreciate feedback though, just that there are a lot of variables to consider other than raw damage

**Qwertyalex** · `11:35` — Just like real life, military plates are rated to stop a number of 7.62x51 impacts. I think it's what, 3-5 before a penetration? Would make sense that someone with plates doesn't go down on the first shot. IIRC it drops a scav in the chest though

**Fontaine** · `11:35` — Also if you're using a short barrel you'll do a lot less damage

**Fontaine** · `11:37` ↳@Qwertyalex — Depends on the rating but generally yeah _(editado)_

**Twank bwattewy** · `11:40` — Well that explains alot more to me then you realise! Thanks! i knew the barrel played a role in damage but didnt know it was that huge. Thanks again

**Twank bwattewy** · `12:00` — also this might be a conflict on my end whit mods but my shotgun shells only have 8 pellets _(editado)_

**Ser_G** · `16:15` — That's standard for most buckshot loads

**Fontaine** · `16:40` ↳@Twank bwattewy — That was done for 3.10+ since BSG introduced performance issues if there were too many pellets hitting at once

**pure aura 9999+** · `17:10` — Do we have an estimated release date for Realism? Like next month, or the end of the year?

**pure aura 9999+** · `17:12` — Every day when I wake up I anxiously wish it had been released

**KannedKielbasa** · `17:28` — Will come out after Concord 6

**Codex** · `17:52`
@pure aura 9999+  
  
When is mod going to update?  
,  
Nobody knows. Unless the mod author goes out of their way to announce it, your guess is as good as anyone's.  
Do not pester people about updates, progress about updates, or if an update is coming. Not only will you not get an answer, you also put unnecessary pressure on mod authors. They do it in their spare time, and release them for free. They don't owe anyone anything.  
----  
Tag: mod update - Ran by: @DrakiaXYZ

**guidot** · `20:06` ↳@pure aura 9999+ — When it's released, it will be released.

**pure aura 9999+** · `21:37` ↳@guidot — Okay, thanks, i was just asking because im not on discord very often and i dont see all the messages

**pure aura 9999+** · `21:37` — thanks for the answer


## 19/04/2026

**GINO** · `00:55` — Has there been any standalone mod released for SPT 4.0 that replicates Realism's randomised trader stocks and tiered flea market system? Find it really cool for a hardcore playthrough.

**guidot** · `01:08` — Something like this? https://forge.sp-tarkov.com/mod/784/hardcore-rules

**GINO** · `02:43` — Soo looking through the config of that mod, there's only an option to disable traders to barter only. Would just like to reduce stock levels so ammo/guns/parts are more scarce and subject to refreshes/random chance.

**Nosliw** · `08:04` ↳@GINO — Not as far as I know

**Nosliw** · `08:05` — The hardcore mod replicates the settings that streamers use for hardcore play throughs

**Nosliw** · `08:05` — Not realism’s systems

**Fontaine** · `10:10` ↳@pure aura 9999+ — As I have said many times here won't be a single Realism release, as per the pinned comment on the mod page. I work on it when I have time, it'll be done when it's done _(editado)_

**pure aura 9999+** · `13:45` ↳@Fontaine
Okay, thanks, i was just asking because im not on discord very often and i dont see all the messages  
thanks for the answer

**Fontaine** · `13:51` — It's on the mod page and patreon too

**Fontaine** · `13:51` — If there are any major updates it will be on either of those platforms

**S41elite** · `18:54` — I love your work, Fontaine!

**TioFreak** · `20:12` — i also love you fontaine

**Joe** · `21:52`


## 20/04/2026

**Twank bwattewy** · `05:10` — Is there a config i can change so Realism bot gear issent pulled from modded gear?

**Fontaine** · `06:15` — There's a JSON file in the mod server folder for user defined bot gear and weapon pools. It's pre-populated with WTT gear and weapons, you just need to remove those IDs

**Twank bwattewy** · `06:53` — Thanks

**Twank bwattewy** · `06:53` — Bin getting this odd bug where PMC's have armored rigs and even helmets whit no plates or soft armor.

**Twank bwattewy** · `06:58` — but most if not all of them are from Artem. So i'll just remove those from the gear pool

**Nosliw** · `20:29` — for therapist insurance, anyone known the cost to insure relative to the amount returned?

**McDewgle** · `20:31` — You should be able math that out yourself if you look at the default values in SVM (Greed.exe)


## 21/04/2026

**Bobby Renzobbi** · `17:42` ↳@Fontaine
  🖼️ 📎 [`att-16-2026-04-21-BobbyRenzobbi.png`](./assets/att-16-2026-04-21-BobbyRenzobbi.png)

**Bobby Renzobbi** · `17:48` — Aw man
  🖼️ 📎 [`att-17-2026-04-21-BobbyRenzobbi.png`](./assets/att-17-2026-04-21-BobbyRenzobbi.png)


## 22/04/2026

**Fontaine** · `09:08` — I need to fix the looping in the next version

**Twank bwattewy** · `09:15` — Dunno if it was ever posted before or if its known, or if its even a bug. But if you get adrenaline effect and it starts the heartbeat sfx while you are dehydrated. It will not stop for a very long time even when adrenaline has alrdy worn or and ur not in pain or dehydrating anymore. I've had it happen a couple of times alrdy

**SoupGod™** · `12:40` — @Fontaine

**SoupGod™** · `12:40` — Keep up the great work man

**SoupGod™** · `12:40` — I’m sure everyone appreciates it

**SoupGod™** · `12:40`

**Jpdarkone³** · `13:24` — I know in my bones.. realism modules come out tomorrow _(editado)_

**Fontaine** · `13:49`

**J3RN3J** · `14:48` — Translating...

**J3RN3J** · `14:48` — "+ 2 weeks."

**Mr.Cathoun** · `15:20` ↳@J3RN3J — + 2 months _(editado)_

**J3RN3J** · `15:25` — Possibly

**J3RN3J** · `15:25` — Im not brushed up on developerish

**Fontaine** · `15:48` — There won't be a single release like I said many times, please stop pestering me about release dates

**DrakiaXYZ** · `15:51` ↳@Fontaine — End users, amiright?

**Fontaine** · `15:52` ↳@DrakiaXYZ — Yup

**Jpdarkone³** · `20:11` ↳@Fontaine — In my defense it was satire

**Jpdarkone³** · `20:12` — Because actually it comes out today!

**DrakiaXYZ** · `20:13` — Guys, for the love of god, leave Fontaine alone

**DrakiaXYZ** · `20:13` — Otherwise I'll be forced to shut down the thread, so he can get a break from you

**Jpdarkone³** · `20:14` ↳@DrakiaXYZ — Wow rude

**Jpdarkone³** · `20:14` — Can we make this thread offtopic then if we cant talk about realism

**DrakiaXYZ** · `20:14` — This is not your thread

**Jpdarkone³** · `20:14` — It was a petition

**DrakiaXYZ** · `20:14` — This is Fontaine's thread

**Jpdarkone³** · `20:18` — Sorry drakia

**Jpdarkone³** · `20:18` — Sorry fontain

**TioFreak** · `23:13` — honestly just close the thread, give devs a break


## 23/04/2026

**PrescriptionAdderall** · `00:17` — I don’t think that’s a good idea. You can still have valuable discussion/support here.

**PrescriptionAdderall** · `00:19`
The “when update” crowd can be dealt with through moderation or possibly automation if possible.  
(Some kind of command that can be run to display a message.)  
As for pestering Fontaine, it is possible for him to mute the thread if it’s too annoying.

**PrescriptionAdderall** · `00:19` — I caveat this with the understanding that it is Fontaine’s thread and he can close it if he wishes.

**ash the proto** · `10:47` — I think the when update people should shut up and wait on a guy doing free work if you don't wanna wait try making your own stuff xb

**Fontaine** · `11:05` — When stances gets uploaded along with the common lib I will be flooded with "where/when ballistics/meds/traders/bots"

**Fontaine** · `11:05` — The common lib itself will rage bait people who don't read, they'll download and wonder why the game hasn't changed

**Archangel** · `11:05` — when common lib

**Archangel** · `11:05` — oh wait

**Qwertyalex** · `11:14` ↳@Archangel — Must resist urge to ping Groovey.

**TioFreak** · `11:26` — when groovey ping?

**TioFreak** · `11:26` — ahahaha

**Jpdarkone³** · `11:32` ↳@Fontaine — We must all report bugs without proper formatting

**GrooveypenguinX** · `22:04` — @GrooveypenguinX

**GrooveypenguinX** · `22:04`


## 25/04/2026

**J3RN3J** · `05:01` — We  fontaine

**riffofthegods** · `13:14`

**riffofthegods** · `13:14` — /j

**Nosliw** · `16:31` — anyone know how the "less loot on subsequent raids" on one map works _(editado)_

**Nosliw** · `16:32` — is it just number of raids? or dependent on how much loot you extract with

**Fontaine** · `16:38` — Based on loot xp

**Nosliw** · `17:45` — thanks


## 27/04/2026

**Furgan** · `10:20` — Is there any way to turn off just the toxic fog? its a fun extra but i'm getting bored of it 5 raids in a row now...

**Kobe Thuy** · `10:21` ↳@Furgan — Hazard Zone config in the mod app config

**Furgan** · `10:23` — you missunderstood. Just the toxic fog, not ALL the hazards

**Kobe Thuy** · `10:23` — ahhh

**PrescriptionAdderall** · `10:25` — Maybe you can edit files to make the chance to appear zero?

**Furgan** · `10:25` — Yea, just need to know wich file and where it is

**SplatRash** · `11:21` ↳@Furgan — It's there somewhere, you can find it. But I don't think you should ask how to do that, since you're modifying the mod's code. You need to figure that in your own

**Fontaine** · `11:39` ↳@Furgan — You started the hazard quest line without reading the text, which gives hints to this. You need to complete the quest line

**Furgan** · `11:58` ↳@Fontaine — yea... that sounds about right... i was kinda expecting this would be the case but guess i know what to do now   Thanks

**Fontaine** · `12:01` — You can either do that or edit the chance in the code but you can mess things up if you go that route

**Fontaine** · `12:01` — It's in the mod.ts file anyways

**Furgan** · `12:03` — Nah i'll do the quest and read it properly. Its kinda my own fault tbh. I need to learn to respect and appriciate the time the modders spend creating these things.


## 28/04/2026

**Fontaine** · `07:18`
Note that stance speeds and animations are not final, this is still WIP  
  
Stances have been reworked to use animation curves with keyframes, rather than simply transitioning from 0 to the final stance position and rotation. This results in much smoother stance animations with different enter and exit animations, and better transitions between stances.  
  
The entire stance controller and input system had to be rewritten from scratch. It was very complicated to transition to using animations while maintaining the fluid stance input system Realism is known for, but it was worth it.  
  
There were many bugs and strange quirks with the previous system that made it nearly impossible to maintain or tweak the stances in any meaningful way. This new system eliminates all of that, allowing new stances to be added or exiting stances to be tweaked easily and efficiently.  
  
This new system integrates closely with BSG's existing procedural animation system, meaning that no longer will stances fight with the existing animation systems for control. This has numerous benefits such as much less stuttering with frame drops, FPS independence, and much smoother blending between stances and sway, movement, and weapon inertia animations.  
  
It's been tough and more time consuming than anticipated but it has laid the foundation for a much better experience and the possibility of adding a lot more layers and depth to the animations and stance mechanics as a whole. _(editado)_
  🎥 📎 [`att-18-2026-04-28-Fontaine.mp4`](./assets/att-18-2026-04-28-Fontaine.mp4)

**Archangel** · `07:32`

**Fontaine** · `07:55` — To get a better look at the animations, here they are slowed down (note that a build like this won't actually be that slow, it's exaggerated for the video)
  🎥 📎 [`att-19-2026-04-28-Fontaine.mp4`](./assets/att-19-2026-04-28-Fontaine.mp4)

**Qwertyalex** · `08:33` — Man that is smoooooth

**Kojimbooo** · `09:16` — Gonna Larp so fuckin hard with that

**J3RN3J** · `09:40` — that looks so smoove

**J3RN3J** · `09:40` — very nice

**adishee** · `10:24` — beautiful job

**Furgan** · `10:44` — That looks amazing

**S41elite** · `10:48` ↳@Fontaine — honestly, for a rifle so heavy as the SR25 platform, it feels RIGHT and not sped up at all (no sarcasm) _(editado)_

**The_Gooch** · `10:59` — inshallah i will play spt again

**The_Gooch** · `11:00` — mute jp before he asks when it comes out and adds two more weeks

**Furgan** · `11:02` — Wanne see me do it

**MOONMOON** · `11:19` ↳@The_Gooch

**Scootis_McPootis** · `11:41` — Looks really good! BSG could never

**Fontaine** · `12:29` ↳@The_Gooch — Our time will come

**jsjsj** · `12:51` ↳@Fontaine — does the ergo of the gun affects the speed of the animation

**Fontaine** · `13:09` ↳@jsjsj — just like in 3.11, it will, as well as weight

**ash the proto** · `17:41` ↳@Fontaine — those are smooooth as butter nice work bitch

**ash the proto** · `17:41` — fr tho you doing gods work bro

**Jpdarkone³** · `18:04` ↳@Fontaine — im homesick because realism is my home

**riffofthegods** · `20:43` ↳@Fontaine — goodness it looks so amazing


## 29/04/2026

**Redbeard** · `01:05` ↳@Fontaine — Will the restrictions for high-ready be the same? I remember a weight or length restriction for high ready run. How about the pros/cons for different stances? Are they to remain identical to 3.11 or will you be adjusting/tweaking any of that? I.e. High ready giving accel boost, Low ready giving arm stamina regen, etc. _(editado)_

**Fontaine** · `02:56` ↳@Redbeard — You'll get what you deserve

**Qwertyalex** · `03:14` ↳@Fontaine — if (player.ProfileName == "JPDarkOne") stanceSpeed *= 0.1;

**Fontaine** · `03:22` — First objective is to add back most of the functionality from 3.11, somewhere in this process will be public test builds. Then after add new features

**Fontaine** · `03:22` — There's no reason to scrap or radically change the balance, restrictions or buffs/debuffs

**Fontaine** · `03:24` — But some things will change in how they're handled. Most likely instead of forcing low ready for heavy weapons, idle will just drain extra stamina and have an extra sway animation layer added on top _(editado)_

**Fontaine** · `03:25` — Once most stances are implemented I'll probably do a test build. It will be very bare at that stage, missing a lot of features

**Kobe Thuy** · `03:50` — I'm not in a hurry, 4.1 would be out by the time you finished the whole mod anyway

**Kobe Thuy** · `03:51` — which is a good thing tho

**Hero of Tarkov Duwang** · `03:53` ↳@Fontaine — real slick looking

**Fontaine** · `08:06` ↳@Kobe Thuy — I don't see the hype behind 4.1, but it shouldn't be particular disruptive for client mods. I learned my lesson regarding SPT server modding and will touch it as little as possible

**Kobe Thuy** · `08:10` ↳@Fontaine — Won’t be much of a change server wise, big difference will be in client modding, which will have most if not all the assemnly deobfuscated. So there will be 1 last update for mods and that’s it

**Fontaine** · `08:22` ↳@Kobe Thuy — For people who have already been modding the client for a while that won't have much of an impact, not for me anyway.

**Kobe Thuy** · `08:22` — Word

**Fontaine** · `08:23` — Just means we go from "GClassxxx" to some nearly incomprehensible BSG interns naming scheme _(editado)_

**bobinstien** · `09:25` — They'll probably have some gclass equivalent of "mod_tactical_000" and "mod_tactical000"


## 30/04/2026

**BoostedStache** · `20:04` — was there ever a difinitive conclusion to what causes the traders to break when exiting a raid? portraits being white and unclickable

**Fontaine** · `20:17` — User error

**BoostedStache** · `20:24` ↳@Fontaine
anything that i can test?  Pulling one mod out at a time is grueling considering i have to go into a raid and extract.  
Ive pulled a few that i thought could cause and issue, but to be honest i run FULL realism with very little else. Mainly WTT and artem.

**BoostedStache** · `20:25` — I also only ask because I saw a lot of talk about it in the past and wasn't sure if someone came up with a conflicting mod/setting.


## 01/05/2026

**Boss30162 (Tweaker Mobile CEO)** · `01:37` — I haven't been following realism much but i wanted to ask if you planned on making any mod weapons compatible out of the box, like the backport

**bobinstien** · `09:25` ↳@Boss30162 (Tweaker Mobile CEO) — I've got a pretty good tool for making compats based off of formulas and I've already done most of the backport so worry not (if compats are similar to 3.11)

**S41elite** · `10:11` ↳@BoostedStache — i have had this issue on 3.11, while my 3.8 and 3.9 dont happen to have this problem. Mind you, 3.8 with 40+ mods and 3.9 with almost 70 mods. My 3.11 has 35mods. _(editado)_

**Qwertyalex** · `10:15` — My 3.9.8 had 199 mods, 3.10 has 198, and 3.11 has 118, all with Realism, and I've never had that issue, what the hell did you guys do to break it lmao

**S41elite** · `10:15` ↳@Fontaine — probably this, though i have tinkered HEAVILY with my 3.9 and i didnt have that problem.

**S41elite** · `10:16` — while i have barely changed as little as possible in my 3.11

**Fontaine** · `11:04` — From what I remember it's an issue with specific trader mods that were coded in a strange way

**Fontaine** · `11:05` — Possibly Artem, and some of the cringe vibe coded cheat traders with animal/anime avatars _(editado)_

**Qwertyalex** · `11:07` — Man, I miss that MS Paint avatar replacer that was made in response to all the GenAI and/or furry portraits, that was an absolute gem

**The_Gooch** · `11:28` — i dont remember the name, but it was the three female traders mod that messed things up for me like constantly. AWS? AVS? Something like that

**BoostedStache** · `12:11` ↳@Fontaine — I have artem, painter and badger (from Epics AIO)

**Fontaine** · `12:11` — Typically to troubleshoot mod problems you remove all mods except that mod

**Fontaine** · `12:12` — I get wanting to see if anyone knows for sure but at some stage you need to do it yourself to get the answers _(editado)_

**BoostedStache** · `12:15` ↳@Fontaine — 100% understand.   I did a fresh 3.11 install last night, I think I had something corrupted because even without any traders it still happened.   If I come up with a difinitive answer I'll post it up

**Fontaine** · `12:16` — Corrupted profile in that case is more likely if truly you had no other mods. Realism doesn't do this by itself, requires other mods to cause issues _(editado)_

**SplatRash** · `16:57` ↳@BoostedStache
Do you have the fika realism sync dll (I don't remember the exact name...)  
I remember in the past it was suspected to be the cause

**Fontaine** · `17:27` — That rings a bell. It's been so long I forget these things

**Fontaine** · `17:27` — Fika sync in 4.0 will be an addon for stances, and should work much better

**SplatRash** · `17:38` ↳@Fontaine
Nice! I was hoping that fika compatibility would be focused on.  
I understand that the mod was created with a single-player experience in mind, but I noticed that the more realistic the features become, the more it encourages team play (i.e covering fire, flanking, protection while doing surgery etc.)  
  
Excited to see what the future holds, you rock!


## 02/05/2026

**The_Gooch** · `12:07` — is it safe to assume that "minor" modules like stances will not require profile restarts, but larger ones like med system will?

**Jpdarkone³** · `16:45` — I dont see why med systems would require a profile wipe?

**Jpdarkone³** · `16:46` — It literally just overhauls vanilla meds

**Jpdarkone³** · `16:46` — Same way you could toggle every realism setting off and keep playing the game

**Fontaine** · `18:42` — No realism features ever required profile wipes


## 03/05/2026

**The_Gooch** · `10:54` — i guess its really been awhile, i thought some items like food would be broken after toggling certain features

**The_Gooch** · `10:55` — memory not that good i guess

**Fontaine** · `11:28` — Nope

**Qwertyalex** · `11:37` — Nah, closest thing you had to do was just tick this box and run the server if you ever wanted to uninstall it (And the Revert HP box on the other tab as well)
  🖼️ 📎 [`att-20-2026-05-03-Qwertyalex.png`](./assets/att-20-2026-05-03-Qwertyalex.png)


## 05/05/2026

**infear** · `15:20` ↳@Fontaine

**SplatRash** · `15:35` ↳@Fontaine
Man I just saw the new animations in the official Tarkov test servers and it's crazy how much better this looks...  
  
It makes zero sense for them to add the "shouldering" animation when your gun already is shouldered. It would only make sense if they had the stance system like Realism does, and even then the way they did it just gives you motion sickness from how bad it looks...  
  
Watch yourself Fontaine, they'll come after you just because you're making them look bad

**Fontaine** · `16:16` ↳@SplatRash — They made the gun move to the camera instead of camera to the gun. Most games do this and it's much smoother. Realism added this for the 3.11 versions. The timing is a bit odd  the difference is I didn't add an awful animation to it

**SplatRash** · `16:59` ↳@Fontaine
I'm not even kidding when I say this; I don't think there's a smoother high/low-ready transition animation in any game on the market rn... Especially the quick low to high successions, it's crazy how good it looks  
  
I genuinely would be curious if someone can point to one that's better...

**SplatRash** · `17:00` — Anyway, I'll get off your d*ck now, I just had to mention it

**John Odox** · `17:46` ↳@Fontaine


## 06/05/2026

**Wolfosito** · `18:55` — Hey fontaine, did you see that bullet fragmentation mod that came out? what are your thoughts about it? i think i remember realism mod did something with that but idk how it compares

**Fontaine** · `19:05` — Didn't look too closely into it but it seems to rebalance frag chance based on realism's values and real world data, and also adds a minimum velocity to frag. Cool idea in principle but BSGs fragmentation system is still very limited and seems bugged _(editado)_

**Fontaine** · `19:07` — I plan to make some big changes to ballistics when I get around to it, I want to do away with BSGs crappy frag system _(editado)_


## 14/05/2026

**adishee** · `11:32`

**IvanTheThicc** · `12:24` — I feel like the silence means something big is coming

**Scootis_McPootis** · `13:24` — I can feel it:
  🖼️ 📎 [`att-21-2026-05-14-ScootisMcPootis.png`](./assets/att-21-2026-05-14-ScootisMcPootis.png)

**S41elite** · `13:33` — bubingus?

**The_Gooch** · `15:44` — tomorrow not thursday

**MOONMOON** · `15:47` — Powder that makes you say powder that makes you say Realism


## 15/05/2026

**Banana Pie Lord** · `15:13` — Realism tomorrow

**Fontaine** · `15:23` — Thread closure becoming more likely

**gusy_gusy** · `16:45` — salutations, this is intended feature right? i got killed like 3 times and when the rigs come back the front plates are impeccable and the back layer completely destroyed
  🖼️ 📎 [`att-22-2026-05-15-gusygusy.png`](./assets/att-22-2026-05-15-gusygusy.png)

**gusy_gusy** · `16:45` — im asking cuz it seems it only happens when using the press armour and this ANA M1

**JamesCheese** · `17:10` — looks like it penned chest armor, see the box fifth in line. I think bullets can pierce through your body and into the back plate?

**Ser_G** · `17:10` — If the shots came from above you or you were crouched its 100% possible

**Fontaine** · `17:13` — Plates don't cover the whole body, so either it's bugged or more likely you were shot from an angle where it skipped the front plate but penned body and hit rear plate _(editado)_

**Fontaine** · `19:11` — Also BSG posed players so that they lean forward and expose their left side, making this more likely to happen

**Ser_G** · `19:45` — The erroneous idea that "the tactical hunch" is what hardcore operators do _(editado)_


## 16/05/2026

**BoostedStache** · `00:03` ↳@Fontaine — Narrowed down the issue,  Fresh start with zero mods asides from Fika, FOV fix, check marks and loot value.   Was smooth sailing for lots of raids, Finishing gunsmith 1 is what breaks it. Confirmed with second profile start.  As soon as i take on gunsmith tasks it breaks the traders exiting a raid.

**BoostedStache** · `01:20` — Have confirmed that if i skip the gunsmith tasks, i have zero issues with traders breaking.  Very happy to have solved this, to anyone else having this issue i hope this helps.

**gusy_gusy** · `14:10` — thank you for answer mah dudes, god bless


## 17/05/2026

**Fontaine** · `06:44` ↳@BoostedStache — Not sure how that would happen, maybe the modification I made to GS quests corrupts the quest state in some way.  Looking at logs would help. Either way this probably won't be relevant for 4.x as server is completely different now

**Joe** · `11:33` — opens admin cmd /finish SPT-Mod Realism /Make SPT-Mod Realism Good /Bugs=0

**JamesCheese** · `11:41` — I am also skeptical that gunsmith 1 is the cause either way. everyone completes that quest very early on

**Danil Hauptmann** · `12:20` ↳@Joe — Beat him with hammers

**Fontaine** · `17:30` ↳@Joe — "Claud, update realism mod. Make no mistakes. It's serious now"

**IvanTheThicc** · `17:31` — We need to extract Fontaines brain and upload it so we can have TakovAI

**The_Gooch** · `17:44` — explain me how this doesnt work, it's literally ai, it's hecking good

**Joe** · `20:52` ↳@IvanTheThicc — It wil become very hostile towards questions, especially stupid ones. Thats the old fontaine, idk the modern one

**Joe** · `20:52` — I miss the old fontaine

**IvanTheThicc** · `21:24` — I don't tend to think stupid questions are bad but when people can ctrl f and find an answer to a question then I understand the frustration


## 18/05/2026

**Jaxson** · `00:44` ↳@Fontaine — did it work??

**Joe** · `01:32` — the old fontaine will chew you up for asking such a question

**Baconism** · `19:01`


## 20/05/2026

**Banana Pie Lord** · `00:52`

**Dashwood Foxe** · `09:38` — My 5090 has arrived

**Dashwood Foxe** · `09:38` — Now I wait for realism lmao

**Kojimbooo** · `09:45` ↳@Dashwood Foxe — You should run raytracing

**Dashwood Foxe** · `09:47` — Is that even a thing in eft

**Joe** · `12:52` ↳@Dashwood Foxe — aight bro, sleep with one eye open


## 21/05/2026

**DevilDog** · `20:28` ↳@Dashwood Foxe — real i had my 5070ti in december


## 22/05/2026

**Dashwood Foxe** · `22:08` ↳@DevilDog — I need to vertically mount it as soon as possible, need a new bench case for it

**Dashwood Foxe** · `22:08` — my risers are so old, and made for gen 3


## 23/05/2026

**Nosliw** · `14:53` — Is the GP-7 and AI-2 enough to not accumulate gas constantly on Labs?

**JamesCheese** · `16:28` — you'll probably want a stim. ai 2 works but it's slow

**Crusader** · `22:40` — Love shotguns in realism lately. More Pellets in buck plus the accurate plate placement make shotguns useable a majority of the time. Just muah


## 24/05/2026

**Nosliw** · `14:54` — today's my first time experiencing these fog raids and i'm lovin' it

**Nosliw** · `18:00` ↳@JamesCheese — Thanks

**IvanTheThicc** · `18:05` ↳@Crusader — Going to labs with mechanics custom benelli is some of the most fun I've had


## 25/05/2026

**Vycka** · `10:32`


## 26/05/2026

**DevilDog** · `20:06` ↳@Vycka — same

**IvanTheThicc** · `20:31` — We have been blessed again


## 28/05/2026

**Kojimbooo** · `16:25`
  🎥 📎 [`att-23-2026-05-28-Kojimbooo.mp4`](./assets/att-23-2026-05-28-Kojimbooo.mp4)

**Jpdarkone³** · `17:09` ↳@Kojimbooo — Why does it look like ur in extremely low fps and motion blur is carrying smoothness

**S41elite** · `20:05` — if u mean about the NVG?

**S41elite** · `20:05` — other than that, the video is absolutely a vibe! Give us more!

**S41elite** · `20:06` — (to not clutter RM threat, maybe post here: ⁠Project Fika⁠📷fika-media )


## 29/05/2026

**Borkel** · `11:30` ↳@Kojimbooo — reshade nvgs?

**The_Gooch** · `13:04` — i give you fifty bucks for your mod list and every setting you changed lol

**Kojimbooo** · `17:24` ↳@Borkel — Your mod + reshade + additional editing

**Travon** · `20:02` ↳@Fontaine — Computa make me a functional mod with zero problems


## 30/05/2026

**Frazzle** · `16:34` ↳@Kojimbooo — What’s your modlist?

**Baconism** · `21:38` ↳@S41elite

**Baconism** · `21:38` — Bro replied like hes a shitty chatbot


## 31/05/2026

**Joe** · `02:39` — maybe he is

**Joe** · `02:39` — but he is our shitty chatbot

**Baconism** · `09:56` ↳@Joe

**Nosliw** · `10:38` — Any tips where to go for Bad Omens - Part 2, with the GAMU devices?

**S41elite** · `17:51` ↳@Baconism — I answered in a polite manner, maybe u lack the manners

**S41elite** · `17:51` ↳@Joe

**Baconism** · `20:24` ↳@S41elite — I mean sure, but politeness makes it look like you're a chatbot

**Baconism** · `20:24` — I was just pointing that out

**Baconism** · `20:25` — But yes I like the politeness you so evidently have

**Nosliw** · `20:55` — 2026 where apparently being polite is "bot" behaviour

**S41elite** · `23:29` ↳@Baconism — the bot part didnt bother me at all, but the "shitty" part _(editado)_


## 01/06/2026

**Baconism** · `15:27` ↳@S41elite — My fault og

**S41elite** · `18:48` — dont sweat it, i kill it with kindness


## 03/06/2026

**Javirare** · `23:12` — Who got some good ABPS settings these scavs are eiteher overrunning me or I see none at all


## 04/06/2026

**Kojimbooo** · `06:34`
  🎥 📎 [`att-24-2026-06-04-Kojimbooo.mp4`](./assets/att-24-2026-06-04-Kojimbooo.mp4)

**Borkel** · `09:12` — oh you did the blems

**Borkel** · `09:12` — nice

**Borkel** · `09:12` ↳@Kojimbooo — they kinda grey tho

**Borkel** · `09:13` — the ones i've seen are black

**adishee** · `09:47` ↳@Kojimbooo — good ol 3.11

**Jpdarkone³** · `15:57` — Is that borkels nvgs

**Fontaine** · `16:20` ↳@Kojimbooo — What I expected live to be like

**Jpdarkone³** · `18:05` ↳@Fontaine — we can hope

**Fontaine** · `18:06` — it's too late fam

**PrescriptionAdderall** · `18:07` — BSG could never deliver something like that.

**Jpdarkone³** · `18:08` ↳@Fontaine — remember theres a alternate universe where bsg made a extraction shooter focused on realism

**Jpdarkone³** · `18:08` — and it has no perf issues

**Kojimbooo** · `18:51` — The EFT playerbase has been completely brainwashed by BSG

**Jpdarkone³** · `18:52` — true alarm

**Kojimbooo** · `18:53` — they're busy discussing their $250 and $150 editions instead of discussing the actual game, they never think what the game is like outside Live Service that BSG feeds them

**Kojimbooo** · `18:55` — the subreddit is full of crap now, the last actual meaningful content there was like 4+ years ago when people discussed the future of the game _(editado)_

**Jpdarkone³** · `18:56` ↳@Kojimbooo — its a shame cus the game genuinelly has amazing world building and the environments but like... its bsg

**Kojimbooo** · `18:57` — yeah it's a golden turd

**Kojimbooo** · `19:04` — and the game went down the sillymaxxing esport/streamer route hard since like 2022, ever since Arena

**Jpdarkone³** · `19:11` ↳@Kojimbooo — arena is so trash

**MoGumbo** · `19:44` ↳@Scootis_McPootis — tf2 palyers love waiting for major updates

**Kojimbooo** · `20:20` — BSG just teased 4 items that have been in mods for years and are getting praised for it lol

**IvanTheThicc** · `21:03` ↳@Kojimbooo — I'd argue since 2021/2022 some of their biggest qol additions have been things from mods

---

_Fim da transcrição — 1132 mensagens, de 07/03/2026 02:23 a 04/06/2026 21:03 (GMT-3)._

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-06-05 | Guilherme | feat(inventory): harden editing tool and drop UltraFika mod #0 |
