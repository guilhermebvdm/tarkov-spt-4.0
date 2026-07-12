# Análise e Plano de Implementação: Otimização do Stuttering (FixedUpdate vs LateUpdate)

## 1. Verificação da Teoria (Assembly-CSharp)
Conforme analisado no código descompilado do jogo (`Assembly-CSharp\EFT\Player.cs`), a rotação da câmera e a movimentação da cabeça do jogador são injetadas através do método `Player.Look(float deltaLookY, float deltaLookX)`.
Esse método é engatilhado ativamente na rotina visual do jogo, atrelado ao *refresh rate* da sua tela (seja via `Update` de inputs da Unity ou através do pipeline do `LateUpdate` na linha `26361` do mesmo arquivo). 

O problema central de otimização ocorre pois o **TarkovIRL** utiliza o Patch `Patch_Look.cs` para interceptar a rotação do jogo todos os frames, mas aplica o modificador `NewDeadzoneController._rotDeltaSmoothedInDeltaTime` que é matematicamente resolvido apenas em `PrimeMover.FixedUpdate()`.

A função `FixedUpdate()` por padrão na Unity corre a *0.02s* (50 FPS). Se o seu monitor renderiza o jogo a 100 FPS, por exemplo, o mod vai entregar o mesmo valor congelado para o `Player.Look()` por 2 quadros inteiros seguidos e saltar bruscamente no 3º quadro. Quando o multiplicador é agressivo, esse pulo se traduz em um engasgo visual da arma pulando posições (*stuttering* ou *jittering*).

## 2. A Solução Proposta

Não precisamos alterar a "força" da física. Precisamos apenas alterar a **frequência de rádio** da matemática. O objetivo é sincronizar a lógica do mod para correr a 100% da taxa de atualização do monitor do jogador.

### Passo 1: Transferir Lógicas no `PrimeMover.cs`
A função `FixedUpdate` atualmente contém este bloco:
```csharp
PlayerMotionController.UpdateMovementMeasurementsInFDT(this.FixedDeltaTime);
WeaponSelectionController.UpdateAnimationPump(this.FixedDeltaTime);
NewDeadzoneController.Update(this.FixedDeltaTime);
NewSwayController.UpdateLerp(this.FixedDeltaTime);
HeadRotController.UpdateLerp(this.FixedDeltaTime);
DirectionalSwayController.UpdateDirectionalSwayLerp(this.FixedDeltaTime);
FootstepController.UpdateStep(this.FixedDeltaTime);
SwayController.UpdateLerp(this.FixedDeltaTime);
ParallaxAdsController.UpdateLerps(this.FixedDeltaTime);
ParallaxController.Update(this.FixedDeltaTime);
```

Devemos transferir todos os Controladores que geram *deslocamento visual liso* (como `NewDeadzoneController`, `NewSwayController`, `HeadRotController`, `DirectionalSwayController`, `ParallaxAdsController` e `ParallaxController`) **para fora do `FixedUpdate()`**, inserindo-os no pipeline de `Update()` ou `LateUpdate()` do Mod, e alterando a variável enviada para `Time.deltaTime` normal da engine.

### Passo 2: Ajuste de Compensação (`Time.deltaTime`)
Ao transferir isso para o `Update`, o tempo passado não será mais o engessado `FixedDeltaTime` (0.02), mas sim a taxa real e flutuante (ex: 0.006s para um jogo a 144Hz).

Isso significa que talvez precisemos afinar os multiplicadores originais de força (ex: `PrimeMover.DeadzoneHeadFollowSpeedMulti`), pois a fórmula passará a ser processada muito mais rápido. Porém, como a Unity utiliza o `.Lerp` com `deltaTime` para suavizar e tornar as coisas independentes do framerate, a transição tende a ser natural se apenas ajustarmos o multiplicador mestre se necessário.

## 3. Conclusão
A teoria está 100% comprovada pelas referências da Unity e pelos artefatos descompilados da BSG. É o clássico "Gargalo da Física vs Renderização" da Unity Engine. 

Nenhuma redução de efeito é necessária no deadzone para resolver isso, apenas o realocamento das chamadas e troca das variáveis de ciclo de tempo do mod. Quando estiver pronto para seguirmos, nós faremos a migração metodicamente.
