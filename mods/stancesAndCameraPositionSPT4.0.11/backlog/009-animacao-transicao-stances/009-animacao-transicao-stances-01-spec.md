# Spec: Animação Orgânica ao Trocar Stances (Wiggle)

## 1. Visão Geral
Atualmente as transições de postura da arma (Stances) são puramente lineares, subindo ou descendo reto, o que pode parecer robótico. Este item adiciona um efeito orgânico (Wiggle) simulando o personagem trazendo a arma para perto do corpo e jogando-a ligeiramente à frente durante a transição.

## 2. Requisitos Funcionais

### 2.1 Efeito Wiggle Dinâmico (Baseado no Peso/Ergonomia)
- Sempre que houver uma mudança de Stance, deve ser aplicado um pequeno "tranco" ou "balanço" na animação procedural da arma (ProceduralWeaponAnimation).
- O efeito deve durar apenas alguns milissegundos, suficiente para disfarçar o movimento linear puro e simular o peso da arma sendo reposicionada.
- **Intensidade Dinâmica:** A agressividade do "Wiggle" será calculada com base no peso total da arma e/ou na sua ergonomia. 
  - Armas leves (ex: Pistolas, SMGs leves) terão um *wiggle* mais suave e controlado.
  - Armas pesadas (ex: LMGs, Rifles com muitos mods) terão um *wiggle* mais agressivo e com maior inércia, demorando milissegundos a mais para estabilizar.

### 2.2 Configurações BepInEx (F12)
- `Enable Stance Wiggle`: Toggle global para ligar/desligar o efeito orgânico nas trocas de postura (Bool, Padrão: true).
- `Stance Wiggle Multiplier`: Multiplicador da força/intensidade desse tranco (Float, Padrão: 1.0).

## 3. Critérios de Aceite
- [ ] Ao mudar de "Pronto Baixo" para "Ativa", a arma faz um movimento orgânico suave para trás/frente ou rotação lateral, parecendo uma ação humana de puxar a arma para a mira.
- [ ] O movimento não causa enjoo ou quebra da mira caso a transição seja cancelada.
