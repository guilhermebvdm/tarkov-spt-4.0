using System;
using System.Collections;
using System.Reflection;
using EFT;
using Fika.Core.Main.GameMode;
using Fika.Core.Main.Players;
using Fika.Core.Main;
using UnityEngine;
using Comfort.Common;
using EFT.UI;
using EFT.AssetsManager;
using System.Threading.Tasks;

namespace TRL_PvpMode.Components
{
    public static class RespawnHandler
    {
        // Vamos guardar o Profile original no início da Raid
        private static Profile _initialProfile;

        /// <summary>
        /// Captura o estado original do perfil do jogador no primeiro spawn.
        /// Isso garante que teremos as armas e vida cheias para o próximo respawn.
        /// </summary>
        public static void SnapshotProfile(Profile profile)
        {
            _initialProfile = profile.Clone();
            BepInEx.Logging.Logger.CreateLogSource("TRL-PvpMode").LogInfo("Profile salvo em memória para restauração de loadout!");
        }

        /// <summary>
        /// Chamado pelo PlayerDeadPatch quando a tela fica preta.
        /// </summary>
        public static void StartRespawnRoutine(CoopGame gameInstance)
        {
            Plugin.Instance.StartCoroutine(RespawnCoroutine(gameInstance));
        }

        private static IEnumerator RespawnCoroutine(CoopGame gameInstance)
        {
            var logger = BepInEx.Logging.Logger.CreateLogSource("TRL-PvpMode");

            // 1 & 2. Cooldown de respawn mantendo o DeathFade desativado para assistirmos a queda
            logger.LogInfo("Aguardando cooldown de respawn (10 segundos) e bloqueando DeathFade...");
            float timer = 0f;
            while (timer < 10f)
            {
                timer += UnityEngine.Time.deltaTime;
                
                // Força o jogo a remover a "Tela Preta da Morte" nativa continuamente durante os 10s
                if (CameraClass.Instance != null && CameraClass.Instance.Camera != null)
                {
                    var deathFade = CameraClass.Instance.Camera.GetComponent("DeathFade");
                    if (deathFade != null)
                    {
                        var disableMethod = deathFade.GetType().GetMethod("DisableEffect");
                        if (disableMethod != null) disableMethod.Invoke(deathFade, null);
                    }
                }
                yield return null;
            }

            // 2. Limpeza do fantasma
            logger.LogInfo("Limpando o fantasma do jogador morto da memória...");
            var myPlayer = Singleton<GameWorld>.Instance.MainPlayer;
            if (myPlayer != null)
            {
                var oldNetId = myPlayer.Id;
                var oldProfileId = myPlayer.ProfileId;

                // Removemos o jogador antigo do Fika
                var fikaPlayer = myPlayer as Fika.Core.Main.Players.FikaPlayer;
                if (fikaPlayer != null && gameInstance.GameController.CoopHandler != null)
                {
                    if (gameInstance.GameController.CoopHandler.Players.ContainsKey(fikaPlayer.NetId))
                    {
                        gameInstance.GameController.CoopHandler.Players.Remove(fikaPlayer.NetId);
                    }
                    gameInstance.GameController.CoopHandler.HumanPlayers.Remove(fikaPlayer);
                }

                // Removemos o jogador antigo de todos os Dicionários internos do GameWorld (força bruta por Reflection para evitar Key: 1)
                var gwFields = typeof(GameWorld).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                foreach (var field in gwFields)
                {
                    if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.Dictionary<,>))
                    {
                        var genericArgs = field.FieldType.GetGenericArguments();
                        if (genericArgs[0] == typeof(int))
                        {
                            var dict = field.GetValue(Singleton<GameWorld>.Instance) as System.Collections.IDictionary;
                            if (dict != null && dict.Contains(oldNetId)) dict.Remove(oldNetId);
                        }
                        else if (genericArgs[0] == typeof(string))
                        {
                            var dict = field.GetValue(Singleton<GameWorld>.Instance) as System.Collections.IDictionary;
                            if (dict != null && dict.Contains(oldProfileId)) dict.Remove(oldProfileId);
                        }
                    }
                }

                Singleton<GameWorld>.Instance.RegisteredPlayers.Remove(myPlayer);

                // Localiza e destrói os componentes controladores do corpo morto para não roubarem input ou darem NullReference
                var components = myPlayer.GetComponentsInChildren<UnityEngine.MonoBehaviour>();
                foreach (var comp in components)
                {
                    var compName = comp.GetType().Name;
                    if (compName.Contains("GamePlayerOwner") || compName.Contains("HANB_Component") || compName.Contains("LoadAmmoComponent"))
                    {
                        logger.LogInfo($"Destruindo componente zumbi: {compName}");
                        UnityEngine.Object.Destroy(comp);
                    }
                }

                myPlayer.Dispose();
            }

            // 3. Novo Spawn e Restauração do Loadout
            logger.LogInfo("Gerando novo personagem e reinjetando na partida...");
            
            // Restaura o loadout inicial preservando os IDs do jogador ativo
            var profileProp = typeof(CoopGame).BaseType.GetProperty("Profile_0", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            if (profileProp != null && _initialProfile != null)
            {
                var currentProfile = profileProp.GetValue(gameInstance) as Profile;
                if (currentProfile != null)
                {
                    // ====================================================================
                    // DEEP CLONE ABSOLUTO via Sirenix Serialization
                    // ====================================================================
                    // Isso recria todos os objetos do zero na memória, sem compartilhar NADA com o cadáver.
                    var freshProfile = (Profile)Sirenix.Serialization.SerializationUtility.CreateCopy(_initialProfile);
                    
                    // Agora que os objetos são clones desvinculados, regeneramos os IDs deles
                    // para que o ItemFactoryClass do Tarkov não dê conflito de ID com os itens do cadáver.
                    var idProp = typeof(EFT.InventoryLogic.Item).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    var idField = typeof(EFT.InventoryLogic.Item).GetField("_id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    
                    if (freshProfile.Inventory != null && freshProfile.Inventory.Equipment != null)
                    {
                        foreach (var item in freshProfile.Inventory.Equipment.GetAllItems())
                        {
                            string newId = System.Guid.NewGuid().ToString("N").Substring(0, 24);
                            if (idProp != null && idProp.CanWrite) idProp.SetValue(item, newId);
                            else if (idField != null) idField.SetValue(item, newId);
                        }
                    }

                    // O Clone Sirenix mantém Id e AccountId intactos do player principal.
                    profileProp.SetValue(gameInstance, freshProfile);
                    logger.LogInfo("Loadout original restaurado com Deep Clone (Sirenix) e novos IDs gerados!");
                }
            }
            
            // Invocamos a criação do player via Reflection (pois o método CreateLocalPlayer do CoopGame é assíncrono e privado)
            MethodInfo createLocalPlayerMethod = typeof(CoopGame).GetMethod("CreateLocalPlayer", BindingFlags.Instance | BindingFlags.NonPublic);
            if (createLocalPlayerMethod != null)
            {
                // Inicia a Task assíncrona do Fika para spawnar e amarrar a câmera
                Task<LocalPlayer> spawnTask = (Task<LocalPlayer>)createLocalPlayerMethod.Invoke(gameInstance, null);
                
                // Espera a Task concluir (usando coroutine wait)
                yield return new WaitUntil(() => spawnTask.IsCompleted);
            }

            // (DeathFade já foi desativado no começo do processo)

            // 4. RECONSTRUÇÃO COMPLETA PÓS-RESPAWN
            var newPlayer = Singleton<GameWorld>.Instance.MainPlayer;
            if (newPlayer != null)
            {
                logger.LogInfo($"[ESPIÃO] Novo boneco instanciado com sucesso! (ID: {newPlayer.Id})");

                // ====================================================================
                // PEÇA EXTRA: Ponto de Spawn Aleatório
                // ====================================================================
                var spawnSystem = gameInstance.GameController.SpawnSystem;
                if (spawnSystem != null)
                {
                    logger.LogInfo("Calculando novo ponto de spawn seguro...");
                    // Passamos um GUID falso para forçar o sistema a escolher um ponto longe dos inimigos, como se fosse um bot
                    var newSpawn = spawnSystem.SelectSpawnPoint(EFT.Game.Spawning.ESpawnCategory.Player, newPlayer.Side, null, null, null, null, System.Guid.NewGuid().ToString());
                    if (newSpawn != null)
                    {
                        newPlayer.Teleport(newSpawn.Position);
                        logger.LogInfo($"Teleportado para novo spawn point aleatório: {newSpawn.Position}");
                    }
                }

                // ====================================================================
                // PEÇA 1: Criar GamePlayerOwner e GRAVAR em gparam_0
                // O gparam_0 é o "cérebro" do CoopGame — todo o código nativo lê dele.
                // Se não atualizarmos, o jogo continua pensando que o cadáver é o player.
                // ====================================================================
                logger.LogInfo("Criando GamePlayerOwner e atualizando gparam_0...");
                var funcField = typeof(CoopGame).GetField("_func_1", BindingFlags.Instance | BindingFlags.NonPublic);
                object newOwner = null;
                if (funcField != null)
                {
                    var func = funcField.GetValue(gameInstance) as System.Delegate;
                    if (func != null)
                    {
                        newOwner = func.DynamicInvoke(newPlayer);
                        if (newOwner != null)
                        {
                            // Gravar o resultado em gparam_0 (campo herdado de BaseLocalGame)
                            var gparamField = typeof(CoopGame).GetField("gparam_0", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                            if (gparamField != null)
                            {
                                gparamField.SetValue(gameInstance, newOwner);
                                logger.LogInfo("gparam_0 atualizado para o novo Owner!");
                            }
                            else
                            {
                                logger.LogWarning("[ESPIÃO] Campo gparam_0 NÃO ENCONTRADO via Reflection!");
                            }

                            // Ativar o componente
                            var ownerMono = newOwner as UnityEngine.MonoBehaviour;
                            if (ownerMono != null)
                            {
                                ownerMono.enabled = true;
                                logger.LogInfo("Controles injetados e ativados com sucesso!");
                            }
                        }
                    }
                }

                // ====================================================================
                // PEÇA 2: Adicionar o novo player ao dictionary_0 (dicionário de players do CoopGame)
                // ====================================================================
                logger.LogInfo("Registrando novo player no dictionary_0...");
                var dictField = typeof(CoopGame).GetField("dictionary_0", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                if (dictField != null)
                {
                    var dict = dictField.GetValue(gameInstance) as System.Collections.IDictionary;
                    if (dict != null)
                    {
                        var profileId = newPlayer.ProfileId;
                        if (!dict.Contains(profileId))
                        {
                            dict.Add(profileId, newPlayer);
                            logger.LogInfo($"dictionary_0: Adicionado ProfileId {profileId}");
                        }
                        else
                        {
                            dict[profileId] = newPlayer; // Substituir referência antiga
                            logger.LogInfo($"dictionary_0: ProfileId {profileId} já existia, substituído.");
                        }
                    }
                }

                // ====================================================================
                // PEÇA 3: Resetar ExitStatus para Survived (o jogo pensa que a raid acabou!)
                // ====================================================================
                logger.LogInfo("Resetando ExitStatus de Killed para Survived...");
                gameInstance.ExitStatus = ExitStatus.Survived;

                // ====================================================================
                // PEÇA 4: Reativar PacketSender (o cadáver parou de enviar pacotes de rede)
                // ====================================================================
                var fikaNewPlayer = newPlayer as FikaPlayer;
                if (fikaNewPlayer != null && fikaNewPlayer.PacketSender != null)
                {
                    logger.LogInfo("Reativando PacketSender para comunicação de rede...");
                    fikaNewPlayer.PacketSender.SendState = true;
                }

                // ====================================================================
                // PEÇA 5: Reconectar DiedEvent listeners para que a próxima morte funcione
                // ====================================================================
                logger.LogInfo("Reconectando DiedEvent listeners...");
                var diedEventMethod = typeof(CoopGame).GetMethod("HealthController_DiedEvent", BindingFlags.Instance | BindingFlags.NonPublic);
                if (diedEventMethod != null)
                {
                    var handler = (Action<EDamageType>)System.Delegate.CreateDelegate(typeof(Action<EDamageType>), gameInstance, diedEventMethod);
                    newPlayer.HealthController.DiedEvent += handler;
                    logger.LogInfo("DiedEvent reconectado!");
                }

                // ====================================================================
                // PEÇA 6: Iniciar ClientHealthController do novo player
                // ====================================================================
                var activeHC = newPlayer.ActiveHealthController;
                if (activeHC != null)
                {
                    logger.LogInfo("Iniciando ActiveHealthController...");
                    var startMethod = activeHC.GetType().GetMethod("Start", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (startMethod != null)
                    {
                        startMethod.Invoke(activeHC, null);
                    }
                }

                // ====================================================================
                // PEÇA 7: Câmera — Limpar antiga e criar nova
                // ====================================================================
                logger.LogInfo("Acoplando Câmera ao novo personagem...");
                newPlayer.PointOfView = EFT.EPointOfView.FirstPerson;

                var fpsCam = CameraClass.Instance.Camera.gameObject;
                
                // Destrói o controlador antigo da câmera
                var oldCamController = fpsCam.GetComponent<EFT.CameraControl.PlayerCameraController>();
                if (oldCamController != null)
                {
                    logger.LogInfo("Limpando PlayerCameraController antigo...");
                    UnityEngine.Object.DestroyImmediate(oldCamController);
                }

                // Destrói o Autocull sampler antigo
                foreach (var comp in fpsCam.GetComponents<UnityEngine.Component>())
                {
                    if (comp != null && comp.GetType().Name == "PerfectCullingCrossSceneSampler")
                    {
                        logger.LogInfo("Limpando PerfectCullingCrossSceneSampler antigo...");
                        UnityEngine.Object.DestroyImmediate(comp);
                    }
                }
                
                // Cria o novo controller amarrado ao boneco novo
                EFT.CameraControl.PlayerCameraController.Create(newPlayer);
                var camController = fpsCam.GetComponent<EFT.CameraControl.PlayerCameraController>();
                if (camController != null)
                {
                    camController.UpdatePointOfView();
                    logger.LogInfo("Câmera vinculada com sucesso!");
                }

                // ====================================================================
                // PEÇA 8: Atualizar FreeCameraController._gamePlayerOwner para o novo Owner
                // ====================================================================
                foreach (var mono in UnityEngine.Object.FindObjectsOfType<UnityEngine.MonoBehaviour>())
                {
                    if (mono.GetType().Name == "FreeCameraController")
                    {
                        var gpoField = mono.GetType().GetField("_gamePlayerOwner", BindingFlags.Instance | BindingFlags.NonPublic);
                        if (gpoField != null)
                        {
                            // Busca o novo GamePlayerOwner no novo player pelo nome do tipo (evita puxar Sirenix)
                            UnityEngine.Component newGPO = null;
                            foreach (var c in newPlayer.GetComponentsInChildren<UnityEngine.MonoBehaviour>())
                            {
                                if (c.GetType().Name.Contains("GamePlayerOwner"))
                                {
                                    newGPO = c;
                                    break;
                                }
                            }
                            if (newGPO != null)
                            {
                                gpoField.SetValue(mono, newGPO);
                                logger.LogInfo("FreeCameraController._gamePlayerOwner atualizado!");
                            }
                        }
                        break;
                    }
                }

                // ====================================================================
                // PEÇA 9: Restaurar BattleUI
                // ====================================================================
                UnityEngine.GameObject battleUI = null;
                var allMonos = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.MonoBehaviour>();
                foreach (var mono in allMonos)
                {
                    var name = mono.GetType().Name;
                    if (name == "EftBattleUIScreen" || name == "BattleUIScreen")
                    {
                        battleUI = mono.gameObject;
                        break;
                    }
                }

                if (battleUI != null)
                {
                    logger.LogInfo("[ESPIÃO] BattleUI encontrado e reativado!");
                    battleUI.SetActive(true);
                }
                else
                {
                    logger.LogWarning("[ESPIÃO] BattleUI NÃO FOI ENCONTRADO na cena!");
                }

                // ====================================================================
                // PEÇA 10: Travar cursor no modo FPS
                // ====================================================================
                logger.LogInfo($"[ESPIÃO] Cursor antes do reset -> Visível: {UnityEngine.Cursor.visible}, Trava: {UnityEngine.Cursor.lockState}");
                UnityEngine.Cursor.lockState = UnityEngine.CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
                logger.LogInfo("Cursor travado em modo FPS!");

                // ====================================================================
                // PEÇA 11: Chamar gparam_0.vmethod_0() — inicializa o Owner (igual ao Spawn() original)
                // ====================================================================
                if (newOwner != null)
                {
                    var vmethod0 = newOwner.GetType().GetMethod("vmethod_0", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (vmethod0 != null)
                    {
                        logger.LogInfo("Chamando vmethod_0 (inicialização do Owner)...");
                        vmethod0.Invoke(newOwner, null);
                    }
                }

                // ====================================================================
                // PEÇA 12: Re-registrar jogador nos Bots e Iniciar Godmode
                // ====================================================================
                var hostController = gameInstance.GameController as HostGameController;
                if (hostController != null && hostController.BotsController != null)
                {
                    hostController.BotsController.AddActivePLayer(newPlayer);
                    logger.LogInfo("Jogador re-registrado na lista de inimigos dos bots!");
                }

                logger.LogInfo("Aplicando Godmode inicial (3 segundos)...");
                newPlayer.ActiveHealthController.SetDamageCoeff(0f);
                
                yield return new WaitForSeconds(3f);
                
                newPlayer.ActiveHealthController.SetDamageCoeff(1f);
                logger.LogInfo("Godmode desativado. Boa caçada!");
            }
            else
            {
                logger.LogError("FALHA CRÍTICA: Novo player é NULL após CreateLocalPlayer!");
            }
        }
    }
}
