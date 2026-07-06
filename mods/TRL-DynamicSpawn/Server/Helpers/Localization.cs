using System.Collections.Generic;

namespace TRLDynamicSpawnServer.Helpers
{
    public static class Localization
    {
        public static string CurrentLanguage { get; set; } = "en-US";

        public static readonly Dictionary<string, string> SupportedLanguages = new Dictionary<string, string>
        {
            { "en-US", "🇺🇸 English" },
            { "pt-BR", "🇧🇷 Português" },
            { "ru-RU", "🇷🇺 Русский" },
            { "es-ES", "🇪🇸 Español" },
            { "zh-CN", "🇨🇳 中文" }
        };

        private static readonly Dictionary<string, Dictionary<string, string>> Dictionaries = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "pt-BR", new Dictionary<string, string>
                {
                    { "Tutorial / Home Page", "Tutorial / Página Inicial" },
                    { "Global Settings", "Configurações Globais" },
                    { "Events", "Eventos" },
                    { "Difficulty Roulette", "Roleta de Dificuldade" },
                    { "Custom Spawns (MOAR)", "Spawns Personalizados (MAIS)" },
                    { "Map Configs", "Configurações de Mapa" },
                    { "Boss Config", "Configuração de Chefes" },
                    { "Global Presets (Faction Ratio)", "Predefinições Globais (Proporção de Facções)" },
                    { "Active Preset", "Predefinição Ativa" },
                    { "50% PMCs / 50% Scavs. The classic SPT balance.", "50% PMCs / 50% Scavs. O clássico balanceamento do SPT." },
                    { "Note: Elites (Bosses, Raiders, etc) always spawn first. The preset applies to the remaining slots.", "Nota: Elites (Chefes, Raiders, etc) sempre nascem primeiro. A predefinição se aplica aos espaços restantes." },
                    { "Global Timers", "Temporizadores Globais" },
                    { "Delay Before First Wave (seconds)", "Atraso antes da primeira onda (segundos)" },
                    { "Seconds Between Waves (seconds)", "Segundos entre ondas (segundos)" },
                    { "Advanced Settings", "Configurações Avançadas" },
                    { "Enable Map Overlap Culling", "Ativar Otimização de Sobreposição de Mapas" },
                    { "Disable All Bosses", "Desativar Todos os Chefes" },
                    { "Enable Boss Open Zones", "Ativar Zonas Abertas de Chefes" },
                    { "Disable Flea Market Bots", "Desativar Bots do Mercado de Pulgas" },
                    { "Enable Scav Group Spawns", "Ativar Spawns de Grupos de Scavs" },
                    { "Current Map Profile", "Perfil do Mapa Atual" },
                    { "Active Event", "Evento Ativo" },
                    { "Max Bot Count", "Contagem Máxima de Bots" },
                    { "Time Between Spawns", "Tempo Entre Spawns" },
                    { "Distance Between Spawns", "Distância Entre Spawns" },
                    { "Time to wait before checking bounds", "Tempo de espera antes de verificar limites" },
                    { "Spawn Amount Multiplier", "Multiplicador de Quantidade de Spawns" },
                    { "Allow Ghost PMCs", "Permitir PMCs Fantasmas" },
                    { "Disable Boss Open Zones", "Desativar Zonas Abertas de Chefes" },
                    { "Spawn Config", "Configuração de Spawns" },
                    { "Spawn Amount Randomizer", "Randomizador de Quantidade de Spawns" },
                    { "Spawn Chance Mult (Difficulty)", "Multiplicador de Chance (Dificuldade)" },
                    { "Enable Progressive Difficulty", "Ativar Dificuldade Progressiva" },
                    { "Maximum Limit of Bots", "Limite Máximo de Bots" },
                    { "Despawn Timer (seconds)", "Tempo de Despawn (segundos)" },
                    { "Max Bots in Game", "Máximo de Bots no Jogo" },
                    { "Minimum Distance Between Players", "Distância Mínima Entre Jogadores" },
                    { "Use Dynamic Waves", "Usar Ondas Dinâmicas" },
                    { "Disable Custom Spawns", "Desativar Spawns Personalizados" },
                    { "Settings saved successfully", "Configurações salvas com sucesso" },
                    { "Error saving settings", "Erro ao salvar configurações" },
                    { "No event active", "Nenhum evento ativo" },
                    { "Random Event", "Evento Aleatório" },
                    { "Menu", "Menu" },
                    { "Select an item...", "Selecione um item..." },
                    { "Settings", "Configurações" }
                }
            },
            {
                "ru-RU", new Dictionary<string, string>
                {
                    { "Tutorial / Home Page", "Учебник / Главная" },
                    { "Global Settings", "Глобальные настройки" },
                    { "Events", "События" },
                    { "Difficulty Roulette", "Рулетка сложности" },
                    { "Custom Spawns (MOAR)", "Пользовательские спауны" },
                    { "Map Configs", "Настройки карты" },
                    { "Boss Config", "Настройки боссов" },
                    { "Global Presets (Faction Ratio)", "Глобальные пресеты (соотношение фракций)" },
                    { "Active Preset", "Активный пресет" },
                    { "50% PMCs / 50% Scavs. The classic SPT balance.", "50% ЧВК / 50% Дикие. Классический баланс SPT." },
                    { "Note: Elites (Bosses, Raiders, etc) always spawn first. The preset applies to the remaining slots.", "Примечание: Элита всегда появляется первой." },
                    { "Global Timers", "Глобальные таймеры" },
                    { "Delay Before First Wave (seconds)", "Задержка перед первой волной (сек)" },
                    { "Seconds Between Waves (seconds)", "Секунды между волнами (сек)" },
                    { "Advanced Settings", "Дополнительные настройки" },
                    { "Enable Map Overlap Culling", "Включить отбраковку наложения карт" },
                    { "Disable All Bosses", "Отключить всех боссов" },
                    { "Enable Boss Open Zones", "Включить открытые зоны боссов" },
                    { "Disable Flea Market Bots", "Отключить ботов барахолки" },
                    { "Enable Scav Group Spawns", "Включить группы диких" },
                    { "Current Map Profile", "Текущий профиль карты" },
                    { "Active Event", "Активное событие" },
                    { "Max Bot Count", "Макс. количество ботов" },
                    { "Time Between Spawns", "Время между спаунами" },
                    { "Distance Between Spawns", "Дистанция между спаунами" },
                    { "Time to wait before checking bounds", "Время ожидания перед проверкой границ" },
                    { "Spawn Amount Multiplier", "Множитель количества" },
                    { "Allow Ghost PMCs", "Разрешить призрачных ЧВК" },
                    { "Disable Boss Open Zones", "Отключить открытые зоны боссов" },
                    { "Spawn Config", "Настройки спауна" },
                    { "Spawn Amount Randomizer", "Рандомизатор количества" },
                    { "Spawn Chance Mult (Difficulty)", "Множитель шанса (Сложность)" },
                    { "Enable Progressive Difficulty", "Прогрессивная сложность" },
                    { "Maximum Limit of Bots", "Максимальный лимит ботов" },
                    { "Despawn Timer (seconds)", "Таймер исчезновения (сек)" },
                    { "Max Bots in Game", "Макс. ботов в игре" },
                    { "Minimum Distance Between Players", "Мин. дистанция между игроками" },
                    { "Use Dynamic Waves", "Динамические волны" },
                    { "Disable Custom Spawns", "Отключить пользовательские спауны" },
                    { "Settings saved successfully", "Настройки успешно сохранены" },
                    { "Error saving settings", "Ошибка при сохранении" },
                    { "No event active", "Нет активного события" },
                    { "Random Event", "Случайное событие" },
                    { "Menu", "Меню" },
                    { "Select an item...", "Выберите элемент..." },
                    { "Settings", "Настройки" }
                }
            },
            {
                "es-ES", new Dictionary<string, string>
                {
                    { "Tutorial / Home Page", "Tutorial / Inicio" },
                    { "Global Settings", "Configuraciones Globales" },
                    { "Events", "Eventos" },
                    { "Difficulty Roulette", "Ruleta de Dificultad" },
                    { "Custom Spawns (MOAR)", "Spawns Personalizados" },
                    { "Map Configs", "Configuración de Mapa" },
                    { "Boss Config", "Configuración de Jefes" },
                    { "Global Presets (Faction Ratio)", "Ajustes Globales (Proporción de Facción)" },
                    { "Active Preset", "Ajuste Activo" },
                    { "50% PMCs / 50% Scavs. The classic SPT balance.", "50% PMCs / 50% Scavs. Balance clásico." },
                    { "Note: Elites (Bosses, Raiders, etc) always spawn first. The preset applies to the remaining slots.", "Nota: Élite (Jefes, Raiders, etc) siempre aparecen primero." },
                    { "Global Timers", "Temporizadores Globales" },
                    { "Delay Before First Wave (seconds)", "Retraso antes de la primera ola (seg)" },
                    { "Seconds Between Waves (seconds)", "Segundos entre olas (seg)" },
                    { "Advanced Settings", "Configuraciones Avanzadas" },
                    { "Enable Map Overlap Culling", "Activar optimización de superposición" },
                    { "Disable All Bosses", "Desactivar todos los jefes" },
                    { "Enable Boss Open Zones", "Activar zonas abiertas de jefes" },
                    { "Disable Flea Market Bots", "Desactivar bots del mercado" },
                    { "Enable Scav Group Spawns", "Activar grupos de scavs" },
                    { "Current Map Profile", "Perfil de Mapa Actual" },
                    { "Active Event", "Evento Activo" },
                    { "Max Bot Count", "Conteo Máx. de Bots" },
                    { "Time Between Spawns", "Tiempo entre Spawns" },
                    { "Distance Between Spawns", "Distancia entre Spawns" },
                    { "Time to wait before checking bounds", "Tiempo antes de verificar límites" },
                    { "Spawn Amount Multiplier", "Multiplicador de cantidad de spawn" },
                    { "Allow Ghost PMCs", "Permitir PMCs Fantasmas" },
                    { "Disable Boss Open Zones", "Desactivar zonas abiertas de jefes" },
                    { "Spawn Config", "Configuración de Spawn" },
                    { "Spawn Amount Randomizer", "Aleatorizador de cantidad" },
                    { "Spawn Chance Mult (Difficulty)", "Multiplicador de probabilidad (Dificultad)" },
                    { "Enable Progressive Difficulty", "Dificultad progresiva" },
                    { "Maximum Limit of Bots", "Límite máximo de bots" },
                    { "Despawn Timer (seconds)", "Tiempo de desaparición (seg)" },
                    { "Max Bots in Game", "Máx. de bots en juego" },
                    { "Minimum Distance Between Players", "Distancia mínima entre jugadores" },
                    { "Use Dynamic Waves", "Usar Olas Dinámicas" },
                    { "Disable Custom Spawns", "Desactivar Spawns Personalizados" },
                    { "Settings saved successfully", "Configuraciones guardadas" },
                    { "Error saving settings", "Error guardando" },
                    { "No event active", "Sin evento activo" },
                    { "Random Event", "Evento Aleatorio" },
                    { "Menu", "Menú" },
                    { "Select an item...", "Selecciona un elemento..." },
                    { "Settings", "Ajustes" }
                }
            },
            {
                "zh-CN", new Dictionary<string, string>
                {
                    { "Tutorial / Home Page", "教程 / 主页" },
                    { "Global Settings", "全局设置" },
                    { "Events", "事件" },
                    { "Difficulty Roulette", "难度轮盘" },
                    { "Custom Spawns (MOAR)", "自定义生成" },
                    { "Map Configs", "地图配置" },
                    { "Boss Config", "首领配置" },
                    { "Global Presets (Faction Ratio)", "全局预设 (派系比例)" },
                    { "Active Preset", "当前预设" },
                    { "50% PMCs / 50% Scavs. The classic SPT balance.", "50% PMC / 50% Scav。 经典的SPT平衡。" },
                    { "Note: Elites (Bosses, Raiders, etc) always spawn first. The preset applies to the remaining slots.", "注意：精英（Boss，Raider等）总是首先生成。" },
                    { "Global Timers", "全局计时器" },
                    { "Delay Before First Wave (seconds)", "第一波前延迟（秒）" },
                    { "Seconds Between Waves (seconds)", "波之间秒数（秒）" },
                    { "Advanced Settings", "高级设置" },
                    { "Enable Map Overlap Culling", "启用地图重叠剔除" },
                    { "Disable All Bosses", "禁用所有Boss" },
                    { "Enable Boss Open Zones", "启用Boss开放区" },
                    { "Disable Flea Market Bots", "禁用跳蚤市场机器人" },
                    { "Enable Scav Group Spawns", "启用Scav小队生成" },
                    { "Current Map Profile", "当前地图配置" },
                    { "Active Event", "当前事件" },
                    { "Max Bot Count", "最大机器人数量" },
                    { "Time Between Spawns", "生成间隔时间" },
                    { "Distance Between Spawns", "生成间隔距离" },
                    { "Time to wait before checking bounds", "检查边界前等待时间" },
                    { "Spawn Amount Multiplier", "生成数量倍数" },
                    { "Allow Ghost PMCs", "允许幽灵PMC" },
                    { "Disable Boss Open Zones", "禁用Boss开放区" },
                    { "Spawn Config", "生成配置" },
                    { "Spawn Amount Randomizer", "生成数量随机数" },
                    { "Spawn Chance Mult (Difficulty)", "生成几率倍数（难度）" },
                    { "Enable Progressive Difficulty", "启用渐进难度" },
                    { "Maximum Limit of Bots", "机器人最大限制" },
                    { "Despawn Timer (seconds)", "消失计时器（秒）" },
                    { "Max Bots in Game", "游戏内最大机器人数量" },
                    { "Minimum Distance Between Players", "玩家间最小距离" },
                    { "Use Dynamic Waves", "使用动态波次" },
                    { "Disable Custom Spawns", "禁用自定义生成" },
                    { "Settings saved successfully", "设置已成功保存" },
                    { "Error saving settings", "保存设置时出错" },
                    { "No event active", "没有活动事件" },
                    { "Random Event", "随机事件" },
                    { "Menu", "菜单" },
                    { "Select an item...", "选择一项..." },
                    { "Settings", "设置" }
                }
            }
        };

        public static string T(string englishText)
        {
            if (CurrentLanguage == "en-US") return englishText;

            if (Dictionaries.TryGetValue(CurrentLanguage, out var dict))
            {
                if (dict.TryGetValue(englishText, out var translated))
                {
                    return translated;
                }
            }

            return englishText;
        }
    }
}
