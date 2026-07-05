# Ideia de Mod: FisheyeScopes

## Visão Geral
Modificação para o SPTarkov (Client Mod - BepInEx) que adiciona um efeito realista de "Fisheye" (Distorção Barril) nas bordas da lente de miras PIP (Picture-in-Picture). O objetivo é quebrar a perfeição matemática da renderização PIP original do jogo e simular as falhas óticas, distorções de borda, sujeiras e reflexos de uma lente de combate real.

## O que discutimos / Funcionalidades Planejadas

### 1. Efeito Fisheye (Distorção de Lente)
- O efeito irá distorcer apenas as extremidades da lente (esticando a imagem), mantendo o centro (retículo) intacto para não prejudicar a precisão do tiro.
- Baseado em relatos e física real, onde óticas LPVO (Low Power Variable Optics) apresentam distorção acentuada na visão periférica.

### 2. Variação Dinâmica baseada no Zoom (Integração C# -> Shader)
- O mod C# irá ler constantemente o nível de **Zoom / FOV** da câmera da luneta.
- Quando o zoom for baixo (ex: 1x), o FOV é alto e a distorção (Fisheye) será **aumentada**.
- Quando o zoom for alto (ex: 6x), o FOV é baixo e a distorção será **reduzida**.
- Isso será feito atualizando a variável `_Distortion` do Material via script em tempo real.

### 3. Efeito de Vidro (Reflexo e Sujeira)
- **Reflexo Simulado:** Usar um cubemap genérico ou textura de *glare* (brilho) que se move levemente conforme o ângulo da arma muda, simulando luz batendo na lente.
- **Sujeira/Arranhões (Smudges):** Uma textura de sujeira branca/transparente com baixa opacidade multiplicada sobre a lente, dando uma textura física ao que antes era uma tela transparente.
- **Performance:** Esses efeitos serão unidos dentro do mesmo passe do Shader na Unity, gerando impacto ZERO de performance além do que o PIP nativo já causa.

## Guia de Execução (Para Futura Referência)

### 1. A Parte da Unity (Visual)
- **Versão Exata da Unity:** `2022.3.43f1` (Versão exigida pelo Tarkov 0.16.9 / SPT 4.0.13).
- **Template:** Projeto 3D (Core / Built-in).
- **Processo:**
  1. Criar um Shader com a matemática de Fisheye, mistura de texturas (Dirt) e UV de reflexo.
  2. Criar um Material e aplicar o Shader.
  3. Usar um script de Editor (`BuildPipeline.BuildAssetBundles`) para exportar esse Material como um **AssetBundle** `.bundle`.

### 2. A Parte do Mod (C#)
- **Estrutura:** Class Library (.NET Framework 4.7.2).
- **Dependências de DLL (Apontando para `E:\TORRENT\Escape from Tarkov 4.0\...`):**
  - `BepInEx.dll`
  - `0Harmony.dll`
  - `Assembly-CSharp.dll`
  - `UnityEngine.dll`
  - `UnityEngine.CoreModule.dll`
- **Código BepInEx / Harmony:** 
  - Faremos um patch na classe que gera o PIP (como o `OpticCameraManager` ou `OpticSight`).
  - Vamos carregar o `.bundle` da Unity via código, extrair o Material, injetar o componente de pós-processamento (`OnRenderImage`) na Câmera e aplicar o material à RenderTexture da luneta.

---
*Conversa salva em 29/06/2026. A pasta inicial do mod e o arquivo `.csproj` já estão parcialmente criados em `mods/FisheyeScopes` prontos para quando você decidir retomar o projeto.*
