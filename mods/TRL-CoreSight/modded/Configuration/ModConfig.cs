using BepInEx.Configuration;
using UnityEngine;

namespace TRLCoreSight.Configuration
{
    public static class ModConfig
    {
        // 0. Afinidade de CPU & Threads
        public static ConfigEntry<Core.ECpuAffinityMode> CpuAffinityMode { get; private set; }
        public static ConfigEntry<bool> ProcessPriorityHigh { get; private set; }
        public static ConfigEntry<bool> EnableFrametimeBenchmark { get; private set; }

        // 1. Geral
        public static ConfigEntry<bool> ModEnabled { get; private set; }
        public static ConfigEntry<bool> DebugMode { get; private set; }

        // 2. Sombras em Interiores
        public static ConfigEntry<bool> EnableInteriorShadowCulling { get; private set; }
        public static ConfigEntry<float> InteriorShadowDistance { get; private set; }
        public static ConfigEntry<float> ExteriorShadowDistance { get; private set; }

        // 3. Otimização de Bots
        public static ConfigEntry<bool> EnableBotAnimationLOD { get; private set; }
        public static ConfigEntry<float> BotOcclusionCheckInterval { get; private set; }
        public static ConfigEntry<bool> DisableCosmeticsWhenOccluded { get; private set; }

        // 4. LOD Dinâmico
        public static ConfigEntry<bool> EnableDynamicLODBias { get; private set; }
        public static ConfigEntry<float> BaseLODBias { get; private set; }
        public static ConfigEntry<float> AimLODBias { get; private set; }

        public static void Init(ConfigFile config)
        {
            // 0. Afinidade de CPU & Threads
            CpuAffinityMode = config.Bind(
                "0. Afinidade de CPU & Threads",
                "CpuAffinityMode",
                Core.ECpuAffinityMode.PhysicalCoresOnly,
                new ConfigDescription("Define a estratégia de afinidade: PhysicalCoresOnly (recomendado: desativa SMT mantendo todos os núcleos físicos), Auto (detecta P-Cores na Intel ou aplica núcleos físicos), PerformanceCores (apenas P-Cores na Intel híbrida), PrimaryCluster (modo avançado: fixa apenas a Main Thread no cluster L3 primário; deve ser testado com o benchmark antes de manter ativado) ou Disabled.", null, new ConfigurationManagerAttributes { Order = 110 })
            );

            ProcessPriorityHigh = config.Bind(
                "0. Afinidade de CPU & Threads",
                "ProcessPriorityHigh",
                true,
                new ConfigDescription("Eleva a prioridade do processo do jogo no Windows para Alta (High), reduzindo interrupções de outros programas em segundo plano.", null, new ConfigurationManagerAttributes { Order = 105 })
            );

            EnableFrametimeBenchmark = config.Bind(
                "0. Afinidade de CPU & Threads",
                "EnableFrametimeBenchmark",
                false,
                new ConfigDescription("Grava um arquivo CSV (benchmark_frametimes.csv) ao fim da raid com média de frametime e 1% lows para comparar o impacto real de cada modo de afinidade.", null, new ConfigurationManagerAttributes { Order = 102 })
            );

            CpuAffinityMode.SettingChanged += (s, e) => OnCpuSettingsChanged?.Invoke();
            ProcessPriorityHigh.SettingChanged += (s, e) => OnCpuSettingsChanged?.Invoke();

            // 1. Geral
            ModEnabled = config.Bind(
                "1. Geral",
                "ModEnabled",
                true,
                new ConfigDescription("Ativa ou desativa todas as funcionalidades de otimização do CoreSight.", null, new ConfigurationManagerAttributes { Order = 100 })
            );

            DebugMode = config.Bind(
                "1. Geral",
                "DebugMode",
                false,
                new ConfigDescription("Exibe métricas de FPS, contagem de bots ocluídos e estado de oclusão no console.", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 90 })
            );

            // 2. Sombras
            EnableInteriorShadowCulling = config.Bind(
                "2. Otimização de Sombras",
                "EnableInteriorShadowCulling",
                true,
                new ConfigDescription("Reduz dinamicamente a distância de sombras quando o jogador está dentro de quartos ou prédios fechados.", null, new ConfigurationManagerAttributes { Order = 80 })
            );

            InteriorShadowDistance = config.Bind(
                "2. Otimização de Sombras",
                "InteriorShadowDistance",
                25.0f,
                new ConfigDescription("Distância máxima de cálculo de sombras em ambientes fechados.", new AcceptableValueRange<float>(10.0f, 50.0f), new ConfigurationManagerAttributes { Order = 70 })
            );

            ExteriorShadowDistance = config.Bind(
                "2. Otimização de Sombras",
                "ExteriorShadowDistance",
                100.0f,
                new ConfigDescription("Distância máxima de sombras restaurada ao ar livre.", new AcceptableValueRange<float>(50.0f, 200.0f), new ConfigurationManagerAttributes { Order = 60 })
            );

            // 3. Otimização de Bots
            EnableBotAnimationLOD = config.Bind(
                "3. Otimização de Bots",
                "EnableBotAnimationLOD",
                true,
                new ConfigDescription("Reduz a taxa de atualização do esqueleto de bots que estão fora de visão ou atrás de paredes sólidas.", null, new ConfigurationManagerAttributes { Order = 50 })
            );

            BotOcclusionCheckInterval = config.Bind(
                "3. Otimização de Bots",
                "BotOcclusionCheckInterval",
                0.2f,
                new ConfigDescription("Intervalo em segundos entre as verificações de visibilidade dos bots.", new AcceptableValueRange<float>(0.05f, 1.0f), new ConfigurationManagerAttributes { IsAdvanced = true, Order = 40 })
            );

            DisableCosmeticsWhenOccluded = config.Bind(
                "3. Otimização de Bots",
                "DisableCosmeticsWhenOccluded",
                false,
                new ConfigDescription("Desativa temporariamente os renderers de equipamentos de bots a mais de 100m atrás de paredes.", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 30 })
            );

            // 4. LOD Dinâmico
            EnableDynamicLODBias = config.Bind(
                "4. LOD Dinâmico",
                "EnableDynamicLODBias",
                true,
                new ConfigDescription("Ajusta o lodBias automaticamente: reduz em áreas fechadas/correndo e aumenta ao mirar.", null, new ConfigurationManagerAttributes { Order = 20 })
            );

            BaseLODBias = config.Bind(
                "4. LOD Dinâmico",
                "BaseLODBias",
                1.0f,
                new ConfigDescription("Valor base de qualidade geométrica do cenário durante movimentação normal.", new AcceptableValueRange<float>(0.5f, 2.0f), new ConfigurationManagerAttributes { Order = 15 })
            );

            AimLODBias = config.Bind(
                "4. LOD Dinâmico",
                "AimLODBias",
                2.0f,
                new ConfigDescription("Valor aumentado de qualidade geométrica acionado ao apontar a mira da arma (ADS).", new AcceptableValueRange<float>(1.5f, 4.0f), new ConfigurationManagerAttributes { Order = 10 })
            );

            // 5. Declutter (Limpeza de Detritos Cosméticos)
            EnableDeclutter = config.Bind(
                "5. Declutter (Cosméticos)",
                "EnableDeclutter",
                true,
                new ConfigDescription("Remove detritos estáticos inúteis (papéis, latas, cacos, poças e decalques) para diminuir draw calls da GPU e aliviar a CPU.", null, new ConfigurationManagerAttributes { Order = 500 })
            );

            DeclutterMode = config.Bind(
                "5. Declutter (Cosméticos)",
                "DeclutterMode",
                EDeclutterMode.RendererOnly,
                new ConfigDescription("RendererOnly desabilita apenas a renderização visual (mais seguro, preserva física/sombras). FullGameObject desativa o objeto inteiro (mais agressivo).", null, new ConfigurationManagerAttributes { Order = 490 })
            );

            DeclutterGarbage = config.Bind(
                "5. Declutter (Cosméticos)",
                "DeclutterGarbage",
                true,
                new ConfigDescription("Remove lixo pequeno, papéis soltos, caixas de papelão vazias e sacos plásticos.", null, new ConfigurationManagerAttributes { Order = 480 })
            );

            DeclutterHeaps = config.Bind(
                "5. Declutter (Cosméticos)",
                "DeclutterHeaps",
                true,
                new ConfigDescription("Remove pequenos montes de entulho e pedras quebradas cosméticas no chão.", null, new ConfigurationManagerAttributes { Order = 470 })
            );

            DeclutterCartridges = config.Bind(
                "5. Declutter (Cosméticos)",
                "DeclutterCartridges",
                true,
                new ConfigDescription("Remove estojos e cartuchos de munição decorativos pré-espalhados no chão pelo mapa.", null, new ConfigurationManagerAttributes { Order = 460 })
            );

            DeclutterFakeFood = config.Bind(
                "5. Declutter (Cosméticos)",
                "DeclutterFakeFood",
                true,
                new ConfigDescription("Remove embalagens de comida cosmética estática (latas amassadas, garrafas vazias, caixas de suco).", null, new ConfigurationManagerAttributes { Order = 450 })
            );

            DeclutterDecals = config.Bind(
                "5. Declutter (Cosméticos)",
                "DeclutterDecals",
                true,
                new ConfigDescription("Remove decalques de sujeira no chão, sangue estático de cenário e grafites decorativos.", null, new ConfigurationManagerAttributes { Order = 440 })
            );

            DeclutterPuddles = config.Bind(
                "5. Declutter (Cosméticos)",
                "DeclutterPuddles",
                true,
                new ConfigDescription("Remove poças d'água decorativas de chão que consomem reflexos e draw calls.", null, new ConfigurationManagerAttributes { Order = 430 })
            );

            DeclutterShards = config.Bind(
                "5. Declutter (Cosméticos)",
                "DeclutterShards",
                true,
                new ConfigDescription("Remove cacos de vidro quebrados e fragmentos de azulejos cosméticos no chão.", null, new ConfigurationManagerAttributes { Order = 420 })
            );

            DeclutterScaleLimit = config.Bind(
                "5. Declutter (Cosméticos)",
                "DeclutterScaleLimit",
                1.5f,
                new ConfigDescription("Altura máxima vertical em metros para elegibilidade de remoção. Detritos maiores que este limite são preservados para servirem de cobertura sólida.", new AcceptableValueRange<float>(0.5f, 2.5f), new ConfigurationManagerAttributes { IsAdvanced = true, Order = 410 })
            );

            // 6. Declutter por Mapa
            DeclutterFactory = config.Bind("6. Declutter Mapas", "Factory", true, new ConfigDescription("Ativa o Declutter na Factory (dia/noite).", null, new ConfigurationManagerAttributes { Order = 390 }));
            DeclutterCustoms = config.Bind("6. Declutter Mapas", "Customs", true, new ConfigDescription("Ativa o Declutter na Customs (bigmap).", null, new ConfigurationManagerAttributes { Order = 380 }));
            DeclutterWoods = config.Bind("6. Declutter Mapas", "Woods", true, new ConfigDescription("Ativa o Declutter em Woods.", null, new ConfigurationManagerAttributes { Order = 370 }));
            DeclutterShoreline = config.Bind("6. Declutter Mapas", "Shoreline", true, new ConfigDescription("Ativa o Declutter em Shoreline.", null, new ConfigurationManagerAttributes { Order = 360 }));
            DeclutterInterchange = config.Bind("6. Declutter Mapas", "Interchange", true, new ConfigDescription("Ativa o Declutter no shopping Interchange.", null, new ConfigurationManagerAttributes { Order = 350 }));
            DeclutterReserve = config.Bind("6. Declutter Mapas", "Reserve", true, new ConfigDescription("Ativa o Declutter na base militar Reserve.", null, new ConfigurationManagerAttributes { Order = 340 }));
            DeclutterLighthouse = config.Bind("6. Declutter Mapas", "Lighthouse", true, new ConfigDescription("Ativa o Declutter no Farol (Lighthouse).", null, new ConfigurationManagerAttributes { Order = 330 }));
            DeclutterStreets = config.Bind("6. Declutter Mapas", "StreetsOfTarkov", true, new ConfigDescription("Ativa o Declutter em Streets of Tarkov (mapa de maior ganho de FPS).", null, new ConfigurationManagerAttributes { Order = 320 }));
            DeclutterGroundZero = config.Bind("6. Declutter Mapas", "GroundZero", true, new ConfigDescription("Ativa o Declutter em Ground Zero (Sandbox).", null, new ConfigurationManagerAttributes { Order = 310 }));
            DeclutterTheLab = config.Bind("6. Declutter Mapas", "TheLab", false, new ConfigDescription("Ativa o Declutter nos Laboratórios TerraGroup (Laboratório já possui alto FPS nativo).", null, new ConfigurationManagerAttributes { Order = 300 }));
            DeclutterTheLabyrinth = config.Bind("6. Declutter Mapas", "TheLabyrinth", false, new ConfigDescription("EXPERIMENTAL: Mapa fechado de evento com portas/armadilhas dinâmicas (TrapSyncable). Mantenha desativado para não quebrar a lógica procedural do evento.", null, new ConfigurationManagerAttributes { IsAdvanced = true, Order = 290 }));

            // Inscrição nos eventos de alteração de configuração
            EnableDeclutter.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterMode.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterGarbage.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterHeaps.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterCartridges.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterFakeFood.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterDecals.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterPuddles.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterShards.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();

            // Eventos dos mapas
            DeclutterFactory.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterCustoms.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterWoods.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterShoreline.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterInterchange.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterReserve.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterLighthouse.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterStreets.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterGroundZero.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterTheLab.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();
            DeclutterTheLabyrinth.SettingChanged += (s, e) => OnDeclutterSettingChanged?.Invoke();

            // 8. Otimização de Física (Cápsulas)
            EnableShellCulling = config.Bind(
                "8. Otimização de Física (Cápsulas)",
                "EnableShellCulling",
                true,
                new ConfigDescription("Suprime a criação e física de quique de cápsulas de balas ejetadas por armas médias e distantes.", null, new ConfigurationManagerAttributes { Order = 30 })
            );

            ShellCullingDistance = config.Bind(
                "8. Otimização de Física (Cápsulas)",
                "ShellCullingDistance",
                25.0f,
                new ConfigDescription("Distância máxima em metros em relação à câmera para renderizar e simular o quique de cartuchos ejetados.", new AcceptableValueRange<float>(10.0f, 60.0f), new ConfigurationManagerAttributes { Order = 20 })
            );

            // 9. Otimização de Loot (Loose Loot Culling)
            EnableLootCulling = config.Bind(
                "9. Otimização de Loot",
                "EnableLootCulling",
                true,
                new ConfigDescription("Oculta a renderização de pequenos itens soltos no chão a média/longa distância para economizar draw calls.", null, new ConfigurationManagerAttributes { Order = 40 })
            );

            SmallLootDistance = config.Bind(
                "9. Otimização de Loot",
                "SmallLootDistance",
                25.0f,
                new ConfigDescription("Distância máxima em metros para renderizar itens pequenos de 1x1 e 1x2 (balas, parafusos, chaves).", new AcceptableValueRange<float>(15.0f, 40.0f), new ConfigurationManagerAttributes { Order = 30 })
            );

            MediumLootDistance = config.Bind(
                "9. Otimização de Loot",
                "MediumLootDistance",
                45.0f,
                new ConfigDescription("Distância máxima em metros para renderizar itens médios de 2x2 e 2x3 (medkits, capacetes, comida).", new AcceptableValueRange<float>(25.0f, 70.0f), new ConfigurationManagerAttributes { Order = 20 })
            );

            EnableADSLootBypass = config.Bind(
                "9. Otimização de Loot",
                "EnableADSLootBypass",
                true,
                new ConfigDescription("Restaura imediatamente a visibilidade de itens dentro do cone de visão ao mirar com a arma (ADS / Luneta).", null, new ConfigurationManagerAttributes { Order = 10 })
            );

            // 10. Otimização de Iluminação (Sombras Secundárias)
            EnableLocalLightShadowCulling = config.Bind(
                "10. Otimização de Iluminação",
                "EnableLocalLightShadowCulling",
                true,
                new ConfigDescription("Otimiza o cálculo de sombras de lâmpadas secundárias em corredores e interiores para aliviar a GPU.", null, new ConfigurationManagerAttributes { Order = 40 })
            );

            ShadowOptimizationMode = config.Bind(
                "10. Otimização de Iluminação",
                "ShadowOptimizationMode",
                LightShadowOptimizationMode.DisableWeakShadowsOnly,
                new ConfigDescription("Modo de otimização de sombras (WeakShadows protege contra vazamento de luz através de paredes).", null, new ConfigurationManagerAttributes { Order = 30 })
            );

            WeakLightIntensityThreshold = config.Bind(
                "10. Otimização de Iluminação",
                "WeakLightIntensityThreshold",
                1.2f,
                new ConfigDescription("Intensidade máxima da luz para ser considerada fraca no modo DisableWeakShadowsOnly.", new AcceptableValueRange<float>(0.2f, 5.0f), new ConfigurationManagerAttributes { Order = 20 })
            );

            WeakLightRangeThreshold = config.Bind(
                "10. Otimização de Iluminação",
                "WeakLightRangeThreshold",
                6.0f,
                new ConfigDescription("Alcance máximo em metros da luz para ser considerada secundária no modo DisableWeakShadowsOnly.", new AcceptableValueRange<float>(1.0f, 20.0f), new ConfigurationManagerAttributes { Order = 10 })
            );

            EnableLocalLightShadowCulling.SettingChanged += (s, e) => OnLightSettingsChanged?.Invoke();
            ShadowOptimizationMode.SettingChanged += (s, e) => OnLightSettingsChanged?.Invoke();
            WeakLightIntensityThreshold.SettingChanged += (s, e) => OnLightSettingsChanged?.Invoke();
            WeakLightRangeThreshold.SettingChanged += (s, e) => OnLightSettingsChanged?.Invoke();

            // 11. Anti-Stutter (Garbage Collection)
            EnableGCOptimizer = config.Bind(
                "11. Anti-Stutter (GC)",
                "EnableGCOptimizer",
                true,
                new ConfigDescription("Inibe a coleta de lixo da Unity durante combates e mira (ADS) para eliminar stutters, mantendo o GC nativo fora de risco.", null, new ConfigurationManagerAttributes { Order = 30 })
            );

            CombatGracePeriod = config.Bind(
                "11. Anti-Stutter (GC)",
                "CombatGracePeriod",
                5.0f,
                new ConfigDescription("Tempo em segundos após o último disparo de arma para manter a inibição de GC ativa.", new AcceptableValueRange<float>(2.0f, 15.0f), new ConfigurationManagerAttributes { Order = 20 })
            );

            CriticalMemoryLimitMB = config.Bind(
                "11. Anti-Stutter (GC)",
                "CriticalMemoryLimitMB",
                3500.0f,
                new ConfigDescription("Limite de segurança de memória alocada (MB) para forçar a liberação do GC nativo e evitar crash por falta de RAM.", new AcceptableValueRange<float>(2000.0f, 6000.0f), new ConfigurationManagerAttributes { IsAdvanced = true, Order = 10 })
            );

            EnableGCOptimizer.SettingChanged += (s, e) => OnGCSettingsChanged?.Invoke();
            CombatGracePeriod.SettingChanged += (s, e) => OnGCSettingsChanged?.Invoke();
            CriticalMemoryLimitMB.SettingChanged += (s, e) => OnGCSettingsChanged?.Invoke();

            // 12. Camuflagem Natural & Visão de IA
            EnableNaturalConcealment = config.Bind(
                "12. Camuflagem & Visão IA",
                "EnableNaturalConcealment",
                true,
                new ConfigDescription("Ativa ou desativa todo o sistema de camuflagem natural do jogador contra IAs em vegetação.", null, new ConfigurationManagerAttributes { Order = 100 })
            );

            ConcealmentOffsetMeters = config.Bind(
                "12. Camuflagem & Visão IA",
                "ConcealmentOffsetMeters",
                0.10f,
                new ConfigDescription("Contorno de segurança do escudo além do corpo e mochila do soldado (metros). Padrão: 10cm.", new AcceptableValueRange<float>(0.02f, 0.25f), new ConfigurationManagerAttributes { Order = 50 })
            );

            BreakProximityDistance = config.Bind(
                "12. Camuflagem & Visão IA",
                "BreakProximityDistance",
                4.0f,
                new ConfigDescription("Distância mínima em metros para um bot detectar o jogador camuflado no capim (tropeço).", new AcceptableValueRange<float>(2.0f, 8.0f), new ConfigurationManagerAttributes { Order = 40 })
            );

            EnableAimOffsetNerf = config.Bind(
                "12. Camuflagem & Visão IA",
                "EnableAimOffsetNerf",
                true,
                new ConfigDescription("Aplica dispersão e erro de mira aos bots que atirarem em jogadores parcialmente camuflados.", null, new ConfigurationManagerAttributes { Order = 30 })
            );

            AimOffsetNerfIntensity = config.Bind(
                "12. Camuflagem & Visão IA",
                "AimOffsetNerfIntensity",
                1.0f,
                new ConfigDescription("Intensidade do desvio de mira dos bots contra alvos encobertos por vegetação.", new AcceptableValueRange<float>(0.2f, 3.0f), new ConfigurationManagerAttributes { Order = 25 })
            );

            TreeCanopyOcclusion = config.Bind(
                "12. Camuflagem & Visão IA",
                "TreeCanopyOcclusion",
                true,
                new ConfigDescription("Bloqueia tiros milagrosos de bots através das folhas e copas de árvores a longa distância.", null, new ConfigurationManagerAttributes { Order = 20 })
            );

            EnableBotTimeSlicing = config.Bind(
                "12. Camuflagem & Visão IA",
                "EnableBotTimeSlicing",
                true,
                new ConfigDescription("Amortiza checagens de visão de bots pacíficos a mais de 100m para aliviar o uso de CPU.", null, new ConfigurationManagerAttributes { Order = 10 })
            );

            EnableNaturalConcealment.SettingChanged += (s, e) => OnConcealmentSettingsChanged?.Invoke();
            ConcealmentOffsetMeters.SettingChanged += (s, e) => OnConcealmentSettingsChanged?.Invoke();
            BreakProximityDistance.SettingChanged += (s, e) => OnConcealmentSettingsChanged?.Invoke();
            EnableAimOffsetNerf.SettingChanged += (s, e) => OnConcealmentSettingsChanged?.Invoke();
            AimOffsetNerfIntensity.SettingChanged += (s, e) => OnConcealmentSettingsChanged?.Invoke();
            TreeCanopyOcclusion.SettingChanged += (s, e) => OnConcealmentSettingsChanged?.Invoke();
            EnableBotTimeSlicing.SettingChanged += (s, e) => OnConcealmentSettingsChanged?.Invoke();

            // 13. Otimização de Áudio (Culling de Ambiente)
            EnableAmbientAudioCulling = config.Bind(
                "13. Otimização de Áudio",
                "EnableAmbientAudioCulling",
                true,
                new ConfigDescription("Pausa fontes de áudio ambiente contínuas inaudíveis à distância para poupar CPU de mixagem.", null, new ConfigurationManagerAttributes { Order = 30 })
            );

            AudioCullingMargin = config.Bind(
                "13. Otimização de Áudio",
                "AudioCullingMargin",
                10.0f,
                new ConfigDescription("Margem de segurança em metros além do maxDistance para pausar o áudio sem cortes sonoros.", new AcceptableValueRange<float>(2.0f, 30.0f), new ConfigurationManagerAttributes { Order = 20 })
            );

            EnableAmbientAudioCulling.SettingChanged += (s, e) => OnAudioSettingsChanged?.Invoke();
            AudioCullingMargin.SettingChanged += (s, e) => OnAudioSettingsChanged?.Invoke();

            // 14. Visualizador Debug (Escudos de Camuflagem)
            ShowConcealmentVisualizer = config.Bind(
                "14. Debug - Camuflagem",
                "ShowConcealmentVisualizer",
                false,
                new ConfigDescription("Renderiza contorno 3D translúcido em vermelho (50% transparente) nas partes do corpo onde o escudo de camuflagem estiver ativo.", null, new ConfigurationManagerAttributes { Order = 20 })
            );

            VisualizerShortcutKey = config.Bind(
                "14. Debug - Camuflagem",
                "VisualizerShortcutKey",
                new BepInEx.Configuration.KeyboardShortcut(KeyCode.F9),
                new ConfigDescription("Tecla de atalho para alternar a visualização dos escudos de camuflagem vermelhos durante a raid.", null, new ConfigurationManagerAttributes { Order = 10 })
            );

            ShowConcealmentVisualizer.SettingChanged += (s, e) => OnVisualizerToggled?.Invoke();

            // 15. Ferramentas de Debug (Vegetation Dumper)
            DumpVegetationAssets = config.Bind(
                "15. Debug - Vegetação",
                "DumpVegetationAssets",
                false,
                new ConfigDescription("Dispara o dump completo de texturas PNG, dimensões e colisores de vegetação da cena atual.", null, new ConfigurationManagerAttributes { Order = 20 })
            );

            DumpShortcutKey = config.Bind(
                "15. Debug - Vegetação",
                "DumpShortcutKey",
                new BepInEx.Configuration.KeyboardShortcut(KeyCode.F8),
                new ConfigDescription("Tecla de atalho para disparar o dump de vegetação em tempo real durante a raid.", null, new ConfigurationManagerAttributes { Order = 10 })
            );

            DumpVegetationAssets.SettingChanged += (s, e) =>
            {
                if (DumpVegetationAssets.Value)
                {
                    DumpVegetationAssets.Value = false;
                    OnDumpRequested?.Invoke();
                }
            };

            // 16. Ferramenta de Vegetação (Inspetor de Mira - Classificador)
            EnableVegetationInspector = config.Bind(
                "16. Ferramenta de Vegetação",
                "EnableVegetationInspector",
                true,
                new ConfigDescription("Ativa ou desativa a ferramenta de inspeção e classificação de vegetação pela mira (teclas '[' e ']').", null, new ConfigurationManagerAttributes { Order = 20 })
            );

            InspectAimVegetationKey = config.Bind(
                "16. Ferramenta de Vegetação",
                "InspectAimVegetationKey",
                new BepInEx.Configuration.KeyboardShortcut(KeyCode.LeftBracket),
                new ConfigDescription("Tecla de atalho '[' para inspecionar e registrar a vegetação mirada como CAMUFLAGEM (Whitelist).", null, new ConfigurationManagerAttributes { Order = 10 })
            );

            RegisterNonConcealmentKey = config.Bind(
                "16. Ferramenta de Vegetação",
                "RegisterNonConcealmentKey",
                new BepInEx.Configuration.KeyboardShortcut(KeyCode.RightBracket),
                new ConfigDescription("Tecla de atalho ']' para registrar a vegetação mirada como NÃO-CAMUFLAGEM (Blacklist / Ignorada).", null, new ConfigurationManagerAttributes { Order = 9 })
            );
        }

        // 8. Otimização de Física (Cápsulas)
        public static ConfigEntry<bool> EnableShellCulling { get; private set; }
        public static ConfigEntry<float> ShellCullingDistance { get; private set; }

        // 9. Otimização de Loot (Loose Loot Culling)
        public static ConfigEntry<bool> EnableLootCulling { get; private set; }
        public static ConfigEntry<float> SmallLootDistance { get; private set; }
        public static ConfigEntry<float> MediumLootDistance { get; private set; }
        public static ConfigEntry<bool> EnableADSLootBypass { get; private set; }

        // 5. Declutter
        public static ConfigEntry<bool> EnableDeclutter { get; private set; }
        public static ConfigEntry<EDeclutterMode> DeclutterMode { get; private set; }
        public static ConfigEntry<bool> DeclutterGarbage { get; private set; }
        public static ConfigEntry<bool> DeclutterHeaps { get; private set; }
        public static ConfigEntry<bool> DeclutterCartridges { get; private set; }
        public static ConfigEntry<bool> DeclutterFakeFood { get; private set; }
        public static ConfigEntry<bool> DeclutterDecals { get; private set; }
        public static ConfigEntry<bool> DeclutterPuddles { get; private set; }
        public static ConfigEntry<bool> DeclutterShards { get; private set; }
        public static ConfigEntry<float> DeclutterScaleLimit { get; private set; }

        // 6. Declutter Mapas
        public static ConfigEntry<bool> DeclutterFactory { get; private set; }
        public static ConfigEntry<bool> DeclutterCustoms { get; private set; }
        public static ConfigEntry<bool> DeclutterWoods { get; private set; }
        public static ConfigEntry<bool> DeclutterShoreline { get; private set; }
        public static ConfigEntry<bool> DeclutterInterchange { get; private set; }
        public static ConfigEntry<bool> DeclutterReserve { get; private set; }
        public static ConfigEntry<bool> DeclutterLighthouse { get; private set; }
        public static ConfigEntry<bool> DeclutterStreets { get; private set; }
        public static ConfigEntry<bool> DeclutterGroundZero { get; private set; }
        public static ConfigEntry<bool> DeclutterTheLab { get; private set; }
        public static ConfigEntry<bool> DeclutterTheLabyrinth { get; private set; }

        // 10. Otimização de Iluminação (Sombras Secundárias)
        public static ConfigEntry<bool> EnableLocalLightShadowCulling { get; private set; }
        public static ConfigEntry<LightShadowOptimizationMode> ShadowOptimizationMode { get; private set; }
        public static ConfigEntry<float> WeakLightIntensityThreshold { get; private set; }
        public static ConfigEntry<float> WeakLightRangeThreshold { get; private set; }

        // 11. Anti-Stutter (Garbage Collection)
        public static ConfigEntry<bool> EnableGCOptimizer { get; private set; }
        public static ConfigEntry<float> CombatGracePeriod { get; private set; }
        public static ConfigEntry<float> CriticalMemoryLimitMB { get; private set; }

        // 12. Camuflagem Natural & Visão de IA
        public static ConfigEntry<bool> EnableNaturalConcealment { get; private set; }
        public static ConfigEntry<float> ConcealmentOffsetMeters { get; private set; }
        public static ConfigEntry<float> BreakProximityDistance { get; private set; }
        public static ConfigEntry<bool> EnableAimOffsetNerf { get; private set; }
        public static ConfigEntry<float> AimOffsetNerfIntensity { get; private set; }
        public static ConfigEntry<bool> TreeCanopyOcclusion { get; private set; }
        public static ConfigEntry<bool> EnableBotTimeSlicing { get; private set; }

        // 13. Otimização de Áudio (Culling de Ambiente)
        public static ConfigEntry<bool> EnableAmbientAudioCulling { get; private set; }
        public static ConfigEntry<float> AudioCullingMargin { get; private set; }

        // 14. Visualizador Debug (Escudos de Camuflagem)
        public static ConfigEntry<bool> ShowConcealmentVisualizer { get; private set; }
        public static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> VisualizerShortcutKey { get; private set; }

        // 15. Ferramentas de Debug (Vegetation Dumper)
        public static ConfigEntry<bool> DumpVegetationAssets { get; private set; }
        public static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> DumpShortcutKey { get; private set; }

        // 16. Ferramenta de Vegetação (Inspetor de Mira)
        public static ConfigEntry<bool> EnableVegetationInspector { get; private set; }
        public static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> InspectAimVegetationKey { get; private set; }
        public static ConfigEntry<BepInEx.Configuration.KeyboardShortcut> RegisterNonConcealmentKey { get; private set; }

        public static event System.Action OnCpuSettingsChanged;
        public static event System.Action OnDeclutterSettingChanged;
        public static event System.Action OnLightSettingsChanged;
        public static event System.Action OnGCSettingsChanged;
        public static event System.Action OnConcealmentSettingsChanged;
        public static event System.Action OnAudioSettingsChanged;
        public static event System.Action OnVisualizerToggled;
        public static event System.Action OnDumpRequested;
    }

    public enum LightShadowOptimizationMode
    {
        DisableWeakShadowsOnly,
        DowngradeSoftToHard,
        DisableAllSecondaryShadows
    }

    public enum EDeclutterMode
    {
        RendererOnly,
        FullGameObject
    }

    public class ConfigurationManagerAttributes
    {
        public bool? IsAdvanced { get; set; }
        public int? Order { get; set; }
    }
}
