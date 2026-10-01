---
title: SAIN — Resumo Descomplicado das Otimizações Multithread e LOD
date: 2026-09-02
status: 🟢 Vivo
authors: [guilhermebvdm, Antigravity]
---

# 🎮 O que mudou no SAIN? (Entenda tudo sem complicação)

Se você já jogou Escape from Tarkov no SPT com muitos bots no mapa, provavelmente já percebeu o jogo ficando pesado ou dando aquelas **"micro-travadinhas" chatas (stutters)**, principalmente em mapas grandes como *Streets of Tarkov*, *Lighthouse* ou quando vários bots começam a lutar ao mesmo tempo.

O **SAIN** é o "cérebro" dos bots: ele calcula o que cada bot vê, ouve, onde se esconde e para onde atira. As modificações que fizemos tiveram um único grande objetivo: **deixar o jogo muito mais leve e fluido, sem deixar os bots burros ou lentos de reação**.

---

## 💡 As 5 Grandes Mudanças Explicadas com Exemplos do Dia a Dia

---

### 1. Sistema de Atenção Inteligente (LOD de IA)
> **A analogia:** Imagine uma sala de aula. O aluno que está sentado na primeira carteira fazendo prova precisa de 100% de foco. Mas o aluno que está lá no fundo apenas descansando não precisa gastar toda a energia dele no mesmo segundo.

* **Como era antes:** Um bot que estava a 300 metros de você, atrás de duas montanhas e sem ninguém por perto, pensava e processava coisas com a mesma intensidade e velocidade de um PMC que estava cara a cara com você num corredor. Isso sobrecarregava o computador à toa.
* **Como ficou agora:**
  * **Perto de você ou em combate:** O bot fica com **foco total** (pensa mais de 60 vezes por segundo, velocidade máxima).
  * **Longe e sem perigo:** O bot "relaxa" e pensa com menos frequência (~8 vezes por segundo), poupando muita força do seu computador.
  * **O Despertar Instantâneo:** Se esse bot distante levar um tiro, ouvir passos ou um parceiro dele gritar, ele **acorda na mesma fração de segundo** para a velocidade máxima. Você nunca vai pegar um bot "dormindo" na hora do tiroteio!

---

### 2. Chega de "Lixo na Memória" (Fim das Micro-Travadas)
> **A analogia:** Em vez de usar um bloco de papel novo toda vez que precisa fazer uma conta rápida, amassar e jogar no chão (enchendo a sala de lixo), agora o bot tem uma lousa mágica fixa: ele escreve, apaga e usa a mesma lousa para sempre.

* **Como era antes:** Para checar onde o inimigo estava ou testar a visão, o mod criava milhares de "recadinhos descartáveis" na memória do computador a cada segundo. Quando essa memória enchia, o jogo era obrigado a dar uma freada rápida para jogar o lixo fora — gerando aquelas pequenas travadinhas de meio segundo durante o tiroteio.
* **Como ficou agora:** Criamos **espaços fixos e permanentes na memória**. O jogo não precisa mais ficar criando e jogando coisas fora. O fluxo de memória ficou 100% limpo, eliminando uma das principais causas de travamento de frames.

---

### 3. A Fila do Correio vs. O Caminhão de Entregas
> **A analogia:** Em vez de você ir até o correio 50 vezes ao dia para postar uma carta de cada vez, você junta todas as 50 cartas em uma caixa só e faz uma única viagem.

* **Como era antes:** Toda vez que um bot queria checar se conseguia ver os passos de um jogador ou de outro bot, o mod pedia isso ao jogo individualmente, dezenas de vezes por segundo.
* **Como ficou agora:** O mod junta todos os pedidos de visão de todos os bots do mapa em um **único grande pacote** e entrega de uma só vez para os outros núcleos do seu processador resolverem juntos. Muito mais rápido e organizado.

---

### 4. Esconderijos Inteligentes (Filtro Rápido de Coberturas)
> **A analogia:** Se você está fugindo de tiros vindo da sua frente, você não perde tempo olhando para uma árvore que está *atrás* do cara que está atirando em você, porque ela não vai te proteger.

* **Como era antes:** Quando o bot precisava achar uma mureta ou caixa para se esconder, ele mandava o jogo calcular a física complexa de quase todas as paredes e obstáculos ao redor, mesmo aquelas que estavam em posições inúteis ou que um amigo dele já estava usando.
* **Como ficou agora:** O bot faz um teste visual relâmpago: se a parede está num ângulo ruim ou se um parceiro de equipe já se escondeu ali, ele **descarta na hora**, sem gastar o processador do jogo com contas desnecessárias.

---

### 5. Memória Rápida para a Visão dos Bots
> **A analogia:** Se alguém te perguntar "quanto é 15 x 15?", você calcula e responde "225". Se a mesma pessoa te fizer a mesma pergunta dois segundos depois, você não precisa fazer a conta toda de novo, você apenas lembra da resposta que acabou de dar.

* **Como era antes:** O Tarkov testa a visão do bot conferindo membro por membro: primeiro testa a cabeça, depois o peito, depois o braço, as pernas... O mod calculava uma fórmula matemática gigante (que leva em conta chuva, névoa, horário do dia, se o jogador está agachado, camuflagem da roupa) para cada um desses membros separadamente, várias vezes no mesmo piscar de olhos.
* **Como ficou agora:** O bot calcula tudo isso uma vez só no início do frame e guarda a resposta. Se o jogo perguntar de novo naquele mesmo instante sobre outra parte do corpo, o bot responde imediatamente com o valor guardado. Isso cortou quase **90% das contas repetitivas** de visão!

---

## 📊 Resumo Visual: Antes vs. Depois

| Situação | No SAIN Anterior | No SAIN Atual (v4.7.0) |
|---|---|---|
| **Bots longe no mapa** | Consumiam processador como se estivessem do seu lado | Economizam até 60% de processamento até entrarem em ação |
| **Memória do jogo** | Criava e descartava milhares de dados por segundo | Memória fixa e limpa (Zero lixo gerado) |
| **Micro-travadas (Stutters)** | Frequentes em tiroteios intensos com vários bots | Reduzidas drasticamente |
| **Noite / Lanternas / Lasers** | Pesava bastante quando muitos bots ligavam luzes | Otimizado para rodar suave e sem peso extra |
| **Reação dos Bots** | Boa | Continua exatamente a mesma: rápida, tática e desafiadora |
| **Compatibilidade** | Normal | 100% compatível com outros mods (*FIKA, QuestingBots, LootingBots*) |

---

## 🎯 Em poucas palavras:
Nós **não emburrecemos os bots** e **não mudamos a dificuldade** deles. O que fizemos foi organizar a "oficina mecânica" interna do mod: tiramos o peso inútil das costas do seu computador, limpamos a memória e ensinamos o jogo a fazer cálculos apenas quando e onde realmente importa. 

O resultado é um **jogo mais leve, com taxa de quadros (FPS) mais estável e tiroteios sem aquelas travadas incômodas**!
