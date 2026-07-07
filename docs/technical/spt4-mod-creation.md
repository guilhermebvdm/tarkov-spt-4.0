---
title: Criar Mods para o SPT 4.0
date: 2026-06-04
status: 🔵 Em andamento
authors: Guilherme
---

# Como Criar Mods para o SPT 4.0

Criar mods para o Single Player Tarkov (SPT) na versão 4.0 foi unificado em uma única linguagem, facilitando o desenvolvimento cross-layer (servidor e cliente). O SPT 4.0 abandonou o Node.js/TypeScript no backend e agora utiliza **C# e .NET 9** tanto no Servidor quanto no Cliente.

Abaixo está o guia definitivo de como estruturar seu modding.

---

## 1. Entendendo a Separação

Mesmo usando a mesma linguagem (C#), a responsabilidade continua dividida:

### A. Server Mods (Backend)
Modificam regras do servidor, banco de dados (Tarkov Database), perfis, sistema de loot offline, IA spawn rates e mercado de pulgas (Flea Market).
- **Linguagem:** C# (.NET 9).
- **Onde instalar:** Na pasta `[pasta_do_spt]\user\mods\NomeDoMod\`
- **Como funciona:** Você criará uma Class Library (DLL) que será lida e injetada pelo novo servidor C# do SPT.

### B. Client Mods (In-Raid)
Modificam a jogabilidade ativa dentro do mapa da Unity (interface, mecânicas de vida, armas, câmera livre, interações no mapa e patches de encerramento de raid).
- **Linguagem:** C# (.NET Framework 4.7.2 - compatível com a Unity do Tarkov).
- **Onde instalar:** Na pasta `[pasta_do_spt]\BepInEx\plugins\`
- **Como funciona:** Você criará um plugin do **BepInEx** que injeta código diretamente no `Assembly-CSharp.dll` (DLL nativa do Tarkov) utilizando o sistema de patches **Harmony**.

---

## 2. Ferramentas Necessárias

Para começar o desenvolvimento em qualquer uma das frentes, garanta que você possui:

1. **Visual Studio 2022 (ou JetBrains Rider)**: 
   - Ao instalar o VS2022, inclua o workload *"Desenvolvimento de jogos com Unity"* e o *"Desenvolvimento para Desktop com .NET"*.
2. **.NET 9 SDK**: Essencial para compilar projetos de Servidor no SPT 4.0.
3. **dnSpy / ILSpy**: Ferramenta vital para abrir e ler o `Assembly-CSharp.dll` desobfuscado. É aqui que você vai ler o código-fonte original do Tarkov para descobrir quais métodos interceptar.
4. **Instalação Limpa do SPT 4.0**: Use a versão "limpa" estritamente para o desenvolvimento, contendo uma cópia pura dos arquivos do servidor e cliente para evitar que outros mods quebrem os seus testes.

---

## 3. Guia Rápido: Iniciando um Client Mod (BepInEx)

A maioria dos mods de gameplay se enquadra aqui.

1. Abra o Visual Studio e crie um novo projeto do tipo **Class Library (.NET Framework 4.7.2)**.
2. Adicione as Referências (Dependências) clicando com o botão direito no projeto -> Add -> Reference. Busque os seguintes arquivos na sua pasta do SPT:
   - `BepInEx\core\BepInEx.dll`
   - `BepInEx\core\0Harmony.dll`
   - `EscapeFromTarkov_Data\Managed\Assembly-CSharp.dll`
   - `EscapeFromTarkov_Data\Managed\UnityEngine.dll`
   - `EscapeFromTarkov_Data\Managed\UnityEngine.CoreModule.dll`
3. Crie a sua classe principal e herde de `BaseUnityPlugin`:

```csharp
using BepInEx;

namespace MeuPrimeiroMod
{
    [BepInPlugin("com.seu_nome.meu_primeiro_mod", "Meu Primeiro Mod", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Código executado quando o jogo carrega
            Logger.LogInfo("Meu primeiro mod carregou com sucesso!");
            
            // Aqui você registrará os seus Harmony Patches
            // new MeuPatch().Enable();
        }
    }
}
```
4. Compile o projeto e copie a DLL gerada na pasta `bin\Debug\` para a pasta `BepInEx\plugins\` do seu SPT.

---

## 4. Comunicação entre Cliente e Servidor

Se o seu mod for complexo, ele precisará de duas metades (Um Client Mod + Um Server Mod). 
- O Client Mod rodará no jogo capturando as ações do jogador.
- O Client Mod enviará requisições HTTP para as rotas customizadas que o seu Server Mod registrar no backend.

---

## 5. Referências Vitais

- **Documentação Oficial:** [dev.sp-tarkov.com](https://dev.sp-tarkov.com/) - Acesse para baixar os Templates Oficiais de BepInEx e de Server Mods em C#.
- **Exemplos de Mods de Servidor:** [github.com/sp-tarkov/server-mod-examples](https://github.com/sp-tarkov/server-mod-examples) - O repositório oficial recheado de mods exemplos básicos para você aprender a "criar" o seu primeiro Server Mod na prática.
- **SPT Hub (Forge):** [hub.sp-tarkov.com](https://hub.sp-tarkov.com/) - Baixe mods similares ao que você quer construir (sempre marcando a caixa open-source) e estude a arquitetura deles.
- **Discord Oficial:** Junte-se aos canais `#mod-development` para resolver problemas de código com a comunidade.

## Histórico

| Data | Autor | Descrição |
|---|---|---|
| 2026-07-06 | Guilherme | chore(launcher): remove empty placeholder diff.txt |
