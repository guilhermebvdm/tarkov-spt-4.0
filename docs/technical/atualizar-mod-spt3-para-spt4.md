---
title: Atualizar um Mod do SPT 3.x para o SPT 4.0
date: 2026-06-04
status: 🔵 Em andamento
authors: Guilherme
---

# Como Atualizar um Mod do SPT 3.x para o SPT 4.0

A transição da versão 3.x para a versão 4.0 do Single Player Tarkov (SPT) representa a maior e mais profunda mudança arquitetural na história do projeto. O que era antes conhecido como "SPT-AKI" agora se chama apenas "SPT", e o motor base do servidor sofreu um *Rewrite* (Reescrita) completo.

Abaixo está o guia de como migrar seu conhecimento e seus projetos antigos para a nova versão.

---

## 1. A Grande Mudança: O Fim do Node.js
Na versão 3.x e anteriores, o Servidor do SPT rodava em **Node.js**, e todos os "Server Mods" eram programados em **TypeScript** ou **JavaScript**. 

**No SPT 4.0, o servidor foi totalmente reescrito em C# utilizando o .NET 9.**

### O que isso significa na prática?
- **Mods de Servidor Antigos Estão Mortos:** Você **não pode** pegar um mod de servidor 3.x, mudar o número da versão no `package.json` e esperar que ele funcione. A base de execução mudou. **Todo o código TypeScript do seu Server Mod precisa ser reescrito em C#.**
- **Curva de Aprendizado Unificada:** Antes você precisava saber TypeScript (para o Servidor) e C# (para o Cliente/BepInEx). Agora, basta dominar o ecossistema C# para ambas as tarefas.

---

## 2. Migrando Mods de Servidor (TypeScript para C#)

Se você tem um Server Mod em 3.x (que modificava preços, traders, munição, quests, etc.), você deverá portá-lo para .NET 9.

### Roteiro de Migração de Servidor:
1. **Configurar o novo Ambiente:** Baixe o Visual Studio 2022 e o SDK do .NET 9.
2. **Criar a Solução:** Utilize o novo *C# Server Mod Template* disponível na Wiki/Gitea oficial do SPT.
3. **Mapeamento de API:** A API antiga injetada pelo TSyringe (ex: `DatabaseServer`, `ProfileHelper`, `Logger`) foi convertida para as lógicas de Injeção de Dependência nativas do .NET. 
   - Em vez de rodar lógicas em um método `postDBLoad(container)`, você utilizará as novas interfaces de ciclo de vida do servidor C#.
4. **Reescrever a Lógica:** Usar LINQ do C# será o equivalente para substituir operações complexas com Arrays e Mapas que você fazia usando métodos de array do JavaScript. O acesso às tabelas do servidor (itens, loot, configs) agora se dá por classes fortamentes tipadas geradas pelo time do SPT.

---

## 3. Migrando Mods de Cliente (BepInEx - C#)

Se o seu mod era um **Client Mod (plugin BepInEx em C#)** rodando na pasta `BepInEx/plugins/`, a notícia é boa: **A linguagem é a mesma.**
No entanto, o seu mod *provavelmente* não funcionará de imediato no 4.0 devido a mudanças agressivas no código original da BSG e na estruturação do SPT.

### Roteiro de Atualização de Cliente:
1. **Nova Arquitetura de Pastas:** No SPT 4.0, a organização dos executáveis mudou. O Servidor e o Launcher agora são atalhos soltos, e as DLLs originais do jogo podem estar organizadas de forma diferente para acomodar a nova arquitetura unificada.
2. **Atualização de Referências:**
   - Abra o seu projeto no Visual Studio.
   - Remova as referências antigas da `Assembly-CSharp.dll` (e outras DLLs do jogo) da versão 3.x.
   - Adicione as DLLs correspondentes extraídas de um SPT 4.0 limpo.
3. **Refatoração de Harmony Patches:**
   - A Battlestate Games (criadora do EFT) altera o nome de classes obfuscadas (`GClass1234`, `Class567`) a quase todo patch. 
   - Se o seu Harmony Patch no 3.x usava `Reflection` ou interceptava uma classe do tipo `GClass`, **esse nome com certeza mudou no 4.0.**
   - Abra o `Assembly-CSharp.dll` do SPT 4.0 no **dnSpy**, procure pelas assinaturas dos métodos que você costumava "patchear" e reescreva seus `[HarmonyPatch]` apontando para as novas classes e métodos do jogo.
4. **Recompilar:** Gere a nova DLL compilada contra os binários da versão 4.0.

---

## 4. Onde encontrar Ajuda na Migração?

Migrar um mod grande exige entender a nova hierarquia de classes da versão atual.
- A ferramenta mais poderosa de migração é o Discord Oficial do SPT. Acesse os canais `#mod-development`.
- Leia o changelog oficial do SPT 4.0 no site deles, prestando muita atenção aos anúncios focados em "Developer Notes", onde listam as classes do jogo original que foram renomeadas.
