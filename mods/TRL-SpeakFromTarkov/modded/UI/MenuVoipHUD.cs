using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using EFT;
using EFT.UI;
using Comfort.Common;
using TRL_SpeakFromTarkov.Core;
using TRL_SpeakFromTarkov.Network;

namespace TRL_SpeakFromTarkov.UI
{
    [Serializable]
    public class SftMenuChannelDto
    {
        public int channelId;
        public string channelName = string.Empty;
        public string hostProfileId = string.Empty;
        public string hostNickname = string.Empty;
        public long lastSeen;
        public string[] members = new string[0];
        public string[] bannedProfileIds = new string[0];
    }

    [Serializable]
    public class SftMenuChannelDtoListWrapper
    {
        public SftMenuChannelDto[] channels = new SftMenuChannelDto[0];
    }

    public class SftMenuChannel
    {
        public byte ChannelId { get; set; }
        public string ChannelName { get; set; } = string.Empty;
        public string HostProfileId { get; set; } = string.Empty;
        public string HostNickname { get; set; } = string.Empty;
        public DateTime LastSeen { get; set; }
        public HashSet<string> Members { get; set; } = new HashSet<string>();
        public HashSet<string> BannedProfileIds { get; set; } = new HashSet<string>();
    }

    public class MenuVoipHUD : MonoBehaviour
    {
        public static MenuVoipHUD Instance { get; private set; } = null!;

        private ConcurrentDictionary<byte, SftMenuChannel> activeChannels = new ConcurrentDictionary<byte, SftMenuChannel>();
        public byte? ConnectedChannelId { get; private set; } = null;
        public string ConnectedChannelName { get; private set; } = string.Empty;

        // Memória de Reconexão Automática Pós-Raid
        public byte? SavedMenuChannelId { get; private set; } = null;
        public string SavedMenuChannelName { get; private set; } = string.Empty;
        private bool wasInRaid = false;

        // Modal de Confirmação de Kick/Ban
        public enum PendingActionType { None, Kick, Ban }
        private PendingActionType pendingAction = PendingActionType.None;
        private string pendingTargetProfileId = string.Empty;
        private string pendingTargetNickname = string.Empty;
        private byte pendingChannelId = 0;

        private Texture2D bgTex = null!;
        private Texture2D headerTex = null!;
        private Texture2D btnGreenTex = null!;
        private Texture2D btnRedTex = null!;
        private Texture2D btnNormalTex = null!;
        private Texture2D borderTex = null!;
        private Texture2D modalBgTex = null!;

        private float lastHeartbeatTime = 0f;
        private float lastServerFetchTime = 0f;
        private Vector2 scrollPosition = Vector2.zero;

        private GUIStyle headerStyle = null!;
        private GUIStyle itemStyle = null!;
        private GUIStyle subStyle = null!;
        private GUIStyle btnGreenStyle = null!;
        private GUIStyle btnRedStyle = null!;
        private GUIStyle btnNormalStyle = null!;
        private GUIStyle modalStyle = null!;
        private bool stylesInitialized = false;

        private string GetLocalSessionId()
        {
            return SftNetwork.Instance != null ? SftNetwork.Instance.LocalSessionId : string.Empty;
        }

        private void Awake()
        {
            Instance = this;
            InitializeTextures();
        }

        private void InitializeTextures()
        {
            bgTex = MakeTex(new Color(0.07f, 0.07f, 0.07f, 0.93f));
            headerTex = MakeTex(new Color(0.12f, 0.12f, 0.12f, 0.95f));
            btnGreenTex = MakeTex(new Color(0.15f, 0.45f, 0.15f, 0.9f));
            btnRedTex = MakeTex(new Color(0.55f, 0.15f, 0.15f, 0.9f));
            btnNormalTex = MakeTex(new Color(0.22f, 0.22f, 0.22f, 0.9f));
            borderTex = MakeTex(new Color(0.28f, 0.28f, 0.28f, 0.9f));
            modalBgTex = MakeTex(new Color(0.05f, 0.05f, 0.05f, 0.98f));
        }

        private Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        private void Update()
        {
            // Detecção de Transição Raid ➔ Menu para Reconexão Automática do Canal
            bool inRaid = Singleton<GameWorld>.Instantiated;
            if (wasInRaid && !inRaid)
            {
                wasInRaid = false;
                AutoRestoreMenuChannel();
            }
            else if (inRaid)
            {
                wasInRaid = true;
            }

            // Executa limpezas, heartbeats e busca no servidor SPT a cada 10s (sincronizado com a presenca do FIKA)
            if (!inRaid)
            {
                if (Time.time - lastServerFetchTime > 10.0f)
                {
                    lastServerFetchTime = Time.time;
                    FetchServerChannels();
                }

                if (Time.time - lastHeartbeatTime > 10.0f)
                {
                    lastHeartbeatTime = Time.time;
                    CleanupStaleChannels();
                    SendChannelHeartbeats();
                }
            }
        }

        private void FetchServerChannels()
        {
            Task.Run(() =>
            {
                try
                {
                    string json = SPT.Common.Http.RequestHandler.GetJson("/sft/channels/list");
                    if (!string.IsNullOrEmpty(json) && json != "[]")
                    {
                        string wrappedJson = "{\"channels\":" + json + "}";
                        var wrapper = UnityEngine.JsonUtility.FromJson<SftMenuChannelDtoListWrapper>(wrappedJson);
                        if (wrapper != null && wrapper.channels != null)
                        {
                            var now = DateTime.Now;
                            foreach (var dto in wrapper.channels)
                            {
                                byte cId = (byte)dto.channelId;
                                var ch = activeChannels.GetOrAdd(cId, id => new SftMenuChannel
                                {
                                    ChannelId = id,
                                    ChannelName = dto.channelName,
                                    HostProfileId = dto.hostProfileId,
                                    HostNickname = dto.hostNickname,
                                    LastSeen = now
                                });

                                ch.ChannelName = dto.channelName;
                                ch.HostProfileId = dto.hostProfileId;
                                ch.HostNickname = dto.hostNickname;
                                ch.LastSeen = now;

                                if (dto.members != null)
                                {
                                    ch.Members = new HashSet<string>(dto.members);
                                }
                                if (dto.bannedProfileIds != null)
                                {
                                    ch.BannedProfileIds = new HashSet<string>(dto.bannedProfileIds);
                                }
                            }
                        }
                    }
                }
                catch { }
            });
        }

        private void AutoRestoreMenuChannel()
        {
            if (SavedMenuChannelId.HasValue && !string.IsNullOrEmpty(SavedMenuChannelName))
            {
                byte cId = SavedMenuChannelId.Value;
                string cName = SavedMenuChannelName;

                VoIPPlugin.Log.LogInfo($"[SFT-MENU] Retornou da raid: Reconectando automaticamente ao canal de voz '{cName}' (ID {cId})...");
                JoinChannel(cId, cName);
            }
        }

        private void CleanupStaleChannels()
        {
            var now = DateTime.Now;
            string myId = GetLocalSessionId();

            foreach (var kvp in activeChannels.ToArray())
            {
                var ch = kvp.Value;

                if (ch.Members.Contains(myId) || ch.HostProfileId == myId || ConnectedChannelId == ch.ChannelId)
                {
                    ch.LastSeen = now;
                    continue;
                }

                if ((now - ch.LastSeen).TotalSeconds > 15)
                {
                    activeChannels.TryRemove(kvp.Key, out _);
                    if (ConnectedChannelId == kvp.Key)
                    {
                        LeaveChannel(userInitiated: false);
                    }
                }
            }
        }

        private void SendChannelHeartbeats()
        {
            if (!ConnectedChannelId.HasValue) return;

            byte cId = ConnectedChannelId.Value;
            string myId = GetLocalSessionId();
            if (activeChannels.TryGetValue(cId, out var channel))
            {
                bool isHost = (channel.HostProfileId == myId);
                bool shouldBroadcast = isHost;

                if (!isHost && channel.Members.Contains(myId))
                {
                    string firstActiveMember = channel.Members.OrderBy(m => m).FirstOrDefault();
                    if (firstActiveMember == myId)
                    {
                        shouldBroadcast = true;
                    }
                }

                if (shouldBroadcast)
                {
                    SftNetwork.BroadcastChannelAnnouncement(cId, channel.ChannelName, channel.HostProfileId, channel.HostNickname, 0);
                }
            }
        }

        public void HandleChannelAnnouncement(SftChannelAnnouncementPacket packet)
        {
            string myId = GetLocalSessionId();

            if (packet.Action == 1) // Close
            {
                activeChannels.TryRemove(packet.ChannelId, out _);
                if (ConnectedChannelId == packet.ChannelId)
                {
                    LeaveChannel(userInitiated: false);
                }
                return;
            }

            var channel = activeChannels.GetOrAdd(packet.ChannelId, id => new SftMenuChannel
            {
                ChannelId = id,
                ChannelName = packet.ChannelName,
                HostProfileId = packet.HostProfileId,
                HostNickname = packet.HostNickname,
                LastSeen = DateTime.Now
            });

            channel.ChannelName = packet.ChannelName;
            channel.HostProfileId = packet.HostProfileId;
            channel.HostNickname = packet.HostNickname;
            channel.LastSeen = DateTime.Now;

            if (packet.Action == 2 && !string.IsNullOrEmpty(packet.HostProfileId)) // Join
            {
                channel.Members.Add(packet.HostProfileId);
            }
            else if (packet.Action == 3 && !string.IsNullOrEmpty(packet.HostProfileId)) // Leave
            {
                channel.Members.Remove(packet.HostProfileId);
            }
            else if (packet.Action == 4) // Kick
            {
                if (!string.IsNullOrEmpty(packet.TargetProfileId))
                {
                    channel.Members.Remove(packet.TargetProfileId);
                    if (packet.TargetProfileId == myId && ConnectedChannelId == packet.ChannelId)
                    {
                        LeaveChannel(userInitiated: true);
                        VoIPPlugin.Log.LogWarning($"[SFT-MENU] Você foi REMOVIDO do canal '{channel.ChannelName}'.");
                    }
                }
            }
            else if (packet.Action == 5) // Ban
            {
                if (!string.IsNullOrEmpty(packet.TargetProfileId))
                {
                    channel.Members.Remove(packet.TargetProfileId);
                    channel.BannedProfileIds.Add(packet.TargetProfileId);
                    if (packet.TargetProfileId == myId && ConnectedChannelId == packet.ChannelId)
                    {
                        LeaveChannel(userInitiated: true);
                        VoIPPlugin.Log.LogWarning($"[SFT-MENU] Você foi BANIDO do canal '{channel.ChannelName}'.");
                    }
                }
            }
        }

        public void CreateNewChannel()
        {
            if (ConnectedChannelId.HasValue) return;

            string myNick = GetMyNickname();
            string myProfileId = GetLocalSessionId();
            byte newChannelId = (byte)(UnityEngine.Random.Range(10, 240));

            string channelName = $"Canal de {myNick}";
            var newChannel = new SftMenuChannel
            {
                ChannelId = newChannelId,
                ChannelName = channelName,
                HostProfileId = myProfileId,
                HostNickname = myNick,
                LastSeen = DateTime.Now
            };
            newChannel.Members.Add(myProfileId);

            activeChannels[newChannelId] = newChannel;
            JoinChannel(newChannelId, channelName);

            SftNetwork.BroadcastChannelAnnouncement(newChannelId, channelName, myProfileId, myNick, 0);
            VoIPPlugin.Log.LogInfo($"[SFT-MENU] Canal de voz '{channelName}' (ID {newChannelId}) criado no menu principal.");
        }

        public void JoinChannel(byte channelId, string channelName)
        {
            string myId = GetLocalSessionId();

            if (activeChannels.TryGetValue(channelId, out var chCheck))
            {
                if (chCheck.BannedProfileIds.Contains(myId))
                {
                    VoIPPlugin.Log.LogWarning($"[SFT-MENU] Entrada recusada: você está BANIDO do canal '{channelName}'.");
                    return;
                }
            }

            ConnectedChannelId = channelId;
            ConnectedChannelName = channelName;

            SavedMenuChannelId = channelId;
            SavedMenuChannelName = channelName;

            if (activeChannels.TryGetValue(channelId, out var channel))
            {
                channel.Members.Add(myId);
            }

            if (VoipController.Instance != null)
            {
                VoipController.Instance.SetCurrentChannel(channelId);
                VoipController.Instance.EnableMenuCapture(true);
            }

            SftNetwork.BroadcastChannelAnnouncement(channelId, channelName, myId, GetMyNickname(), 2);
            VoIPPlugin.Log.LogInfo($"[SFT-MENU] Conectado ao canal de voz '{channelName}' (ID {channelId}). Microfone ativado.");
        }

        public void LeaveChannel(bool userInitiated = true)
        {
            string myId = GetLocalSessionId();

            if (userInitiated)
            {
                SavedMenuChannelId = null;
                SavedMenuChannelName = string.Empty;
            }

            if (ConnectedChannelId.HasValue)
            {
                byte oldId = ConnectedChannelId.Value;
                if (activeChannels.TryGetValue(oldId, out var channel))
                {
                    channel.Members.Remove(myId);

                    if (channel.Members.Count > 0)
                    {
                        string nextHost = channel.Members.First();
                        channel.HostProfileId = nextHost;
                        SftNetwork.BroadcastChannelAnnouncement(oldId, channel.ChannelName, nextHost, channel.HostNickname, 0);
                        VoIPPlugin.Log.LogInfo($"[SFT-MENU] Liderança do canal '{channel.ChannelName}' transferida para {nextHost}.");
                    }
                    else
                    {
                        activeChannels.TryRemove(oldId, out _);
                        SftNetwork.BroadcastChannelAnnouncement(oldId, channel.ChannelName, myId, GetMyNickname(), 1); // Close
                    }
                }
            }

            ConnectedChannelId = null;
            ConnectedChannelName = string.Empty;

            if (VoipController.Instance != null)
            {
                VoipController.Instance.SetCurrentChannel(1);
                VoipController.Instance.EnableMenuCapture(false);
            }
            VoIPPlugin.Log.LogInfo("[SFT-MENU] Desconectado do canal de voz do menu. Microfone desligado.");
        }

        public void OpenConfirmationModal(PendingActionType type, string targetProfileId, string targetNickname, byte channelId)
        {
            pendingAction = type;
            pendingTargetProfileId = targetProfileId;
            pendingTargetNickname = targetNickname;
            pendingChannelId = channelId;
        }

        private void ExecutePendingAction()
        {
            if (pendingAction == PendingActionType.Kick)
            {
                if (activeChannels.TryGetValue(pendingChannelId, out var ch))
                {
                    ch.Members.Remove(pendingTargetProfileId);
                }
                SftNetwork.BroadcastChannelAnnouncement(pendingChannelId, ConnectedChannelName, GetLocalSessionId(), GetMyNickname(), 4, pendingTargetProfileId);
                VoIPPlugin.Log.LogInfo($"[SFT-MENU] Jogador '{pendingTargetNickname}' REMOVIDO do canal.");
            }
            else if (pendingAction == PendingActionType.Ban)
            {
                if (activeChannels.TryGetValue(pendingChannelId, out var ch))
                {
                    ch.Members.Remove(pendingTargetProfileId);
                    ch.BannedProfileIds.Add(pendingTargetProfileId);
                }
                SftNetwork.BroadcastChannelAnnouncement(pendingChannelId, ConnectedChannelName, GetLocalSessionId(), GetMyNickname(), 5, pendingTargetProfileId);
                VoIPPlugin.Log.LogInfo($"[SFT-MENU] Jogador '{pendingTargetNickname}' BANIDO do canal.");
            }

            pendingAction = PendingActionType.None;
            pendingTargetProfileId = string.Empty;
            pendingTargetNickname = string.Empty;
        }

        private string GetMyNickname()
        {
            try
            {
                if (Fika.Core.Main.Utils.FikaBackendUtils.Profile != null && !string.IsNullOrEmpty(Fika.Core.Main.Utils.FikaBackendUtils.PMCName))
                {
                    return Fika.Core.Main.Utils.FikaBackendUtils.PMCName;
                }
            }
            catch { }
            return "Jogador";
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;
            stylesInitialized = true;

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14
            };
            headerStyle.normal.textColor = new Color(0.9f, 0.77f, 0.51f);

            itemStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13
            };
            itemStyle.normal.textColor = Color.white;

            subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11
            };
            subStyle.normal.textColor = new Color(0.75f, 0.75f, 0.75f);

            btnGreenStyle = new GUIStyle(GUI.skin.button) { fontSize = 12 };
            btnGreenStyle.normal.background = btnGreenTex;
            btnGreenStyle.normal.textColor = Color.white;

            btnRedStyle = new GUIStyle(GUI.skin.button) { fontSize = 12 };
            btnRedStyle.normal.background = btnRedTex;
            btnRedStyle.normal.textColor = Color.white;

            btnNormalStyle = new GUIStyle(GUI.skin.button) { fontSize = 12 };
            btnNormalStyle.normal.background = btnNormalTex;
            btnNormalStyle.normal.textColor = Color.white;

            modalStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13
            };
            modalStyle.normal.textColor = Color.white;
        }

        private bool IsFikaHUDVisible()
        {
            if (!Fika.Core.UI.Custom.MainMenuUIScript.Exist) return false;
            var fikaScript = Fika.Core.UI.Custom.MainMenuUIScript.Instance;
            if (fikaScript == null || !fikaScript.enabled || !fikaScript.gameObject.activeInHierarchy) return false;

            try
            {
                var tr = fikaScript.transform;
                if (tr.childCount > 0)
                {
                    var child0 = tr.GetChild(0);
                    if (child0 != null && child0.childCount > 0)
                    {
                        var child00 = child0.GetChild(0);
                        if (child00 != null)
                        {
                            return child00.gameObject.activeInHierarchy;
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        private void OnGUI()
        {
            if (Singleton<GameWorld>.Instantiated) return;
            if (VoIPPlugin.EnableMod != null && !VoIPPlugin.EnableMod.Value) return;

            if (!IsFikaHUDVisible()) return;

            InitStyles();
            GUI.depth = -900;

            float width = 400f;
            float marginRight = 55f;
            float marginTop = 35f;
            float posX = Screen.width - width - marginRight;
            float posY = marginTop;

            float height = ConnectedChannelId.HasValue ? 260f : 210f;
            Rect containerRect = new Rect(posX, posY, width, height);

            GUI.DrawTexture(containerRect, bgTex);
            GUI.DrawTexture(new Rect(posX, posY, width, 1), borderTex);
            GUI.DrawTexture(new Rect(posX, posY + height - 1, width, 1), borderTex);
            GUI.DrawTexture(new Rect(posX, posY, 1, height), borderTex);
            GUI.DrawTexture(new Rect(posX + width - 1, posY, 1, height), borderTex);

            Rect headerRect = new Rect(posX + 1, posY + 1, width - 2, 30);
            GUI.DrawTexture(headerRect, headerTex);

            int channelCount = activeChannels.Count;
            GUI.Label(new Rect(posX + 10, posY + 5, 220, 22), $"CANAIS DE VOIP: {channelCount}", headerStyle);

            if (ConnectedChannelId.HasValue)
            {
                if (GUI.Button(new Rect(posX + width - 135, posY + 4, 125, 22), "SAIR DO CANAL", btnRedStyle))
                {
                    LeaveChannel(userInitiated: true);
                }
            }
            else
            {
                if (GUI.Button(new Rect(posX + width - 135, posY + 4, 125, 22), "+ CRIAR CANAL", btnGreenStyle))
                {
                    CreateNewChannel();
                }
            }

            float currentY = posY + 36;

            if (ConnectedChannelId.HasValue)
            {
                GUI.Label(new Rect(posX + 10, currentY, width - 150, 20), $"🔊 {ConnectedChannelName} (Conectado)", itemStyle);

                var proc = VoipController.Instance?.processor;
                if (proc != null)
                {
                    string micState = proc.IsMuted ? "🔇 MUTADO" : "🎙️ MIC LIGADO";
                    if (GUI.Button(new Rect(posX + width - 110, currentY, 100, 20), micState, btnNormalStyle))
                    {
                        proc.IsMuted = !proc.IsMuted;
                    }
                }
                currentY += 24;

                string myId = GetLocalSessionId();
                if (activeChannels.TryGetValue(ConnectedChannelId.Value, out var curChannel))
                {
                    bool isHost = (curChannel.HostProfileId == myId);

                    GUI.Label(new Rect(posX + 10, currentY, width - 20, 16), $"MEMBROS NO CANAL ({curChannel.Members.Count}):", subStyle);
                    currentY += 18;

                    foreach (var memberId in curChannel.Members.ToList())
                    {
                        bool isMemberHost = (memberId == curChannel.HostProfileId);
                        string nick = isMemberHost ? $"{curChannel.HostNickname} 👑 (Dono)" : "Jogador";
                        if (memberId == myId) nick += " (Você)";

                        GUI.Label(new Rect(posX + 15, currentY, 200, 20), $"🟢 {nick}", itemStyle);

                        if (isHost && memberId != myId)
                        {
                            if (GUI.Button(new Rect(posX + width - 145, currentY, 65, 18), "REMOVER", btnNormalStyle))
                            {
                                OpenConfirmationModal(PendingActionType.Kick, memberId, nick, curChannel.ChannelId);
                            }
                            if (GUI.Button(new Rect(posX + width - 75, currentY, 65, 18), "BANIR", btnRedStyle))
                            {
                                OpenConfirmationModal(PendingActionType.Ban, memberId, nick, curChannel.ChannelId);
                            }
                        }
                        currentY += 22;
                    }
                }
            }
            else
            {
                GUI.Label(new Rect(posX + 10, currentY, width - 20, 20), "Voz desligada no menu. Entre ou crie um canal.", subStyle);
                currentY += 22;
            }

            GUI.DrawTexture(new Rect(posX + 10, currentY, width - 20, 1), borderTex);
            currentY += 6;

            GUI.Label(new Rect(posX + 10, currentY, width - 20, 18), "CANAIS DISPONÍVEIS NO MENU:", subStyle);
            currentY += 20;

            var channelsList = activeChannels.Values.ToList();
            if (channelsList.Count == 0)
            {
                GUI.Label(new Rect(posX + 15, currentY, width - 30, 20), "Nenhum canal ativo no momento.", subStyle);
            }
            else
            {
                float visibleScrollHeight = (posY + height) - currentY - 6f;
                float contentHeight = channelsList.Count * 26f;

                Rect scrollOuterRect = new Rect(posX + 5, currentY, width - 10, visibleScrollHeight);
                Rect scrollContentRect = new Rect(0, 0, width - 28, Mathf.Max(visibleScrollHeight, contentHeight));

                scrollPosition = GUI.BeginScrollView(scrollOuterRect, scrollPosition, scrollContentRect);

                float itemY = 0f;
                foreach (var ch in channelsList)
                {
                    bool isCurrent = ConnectedChannelId == ch.ChannelId;
                    GUI.Label(new Rect(5, itemY, 240, 22), $"🔊 {ch.ChannelName} ({ch.Members.Count} online)", itemStyle);

                    if (!isCurrent)
                    {
                        if (GUI.Button(new Rect(scrollContentRect.width - 95, itemY, 90, 20), "ENTRAR", btnGreenStyle))
                        {
                            JoinChannel(ch.ChannelId, ch.ChannelName);
                        }
                    }
                    else
                    {
                        GUI.Label(new Rect(scrollContentRect.width - 95, itemY, 90, 20), "[CONECTADO]", subStyle);
                    }
                    itemY += 26f;
                }

                GUI.EndScrollView();
            }

            DrawConfirmationModal();
        }

        private void DrawConfirmationModal()
        {
            if (pendingAction == PendingActionType.None) return;

            GUI.depth = -1000;

            float modalWidth = 420f;
            float modalHeight = 150f;
            float mX = (Screen.width - modalWidth) / 2f;
            float mY = (Screen.height - modalHeight) / 2f;
            Rect modalRect = new Rect(mX, mY, modalWidth, modalHeight);

            GUI.DrawTexture(modalRect, modalBgTex);
            GUI.DrawTexture(new Rect(mX, mY, modalWidth, 2), borderTex);
            GUI.DrawTexture(new Rect(mX, mY + modalHeight - 2, modalWidth, 2), borderTex);
            GUI.DrawTexture(new Rect(mX, mY, 2, modalHeight), borderTex);
            GUI.DrawTexture(new Rect(mX + modalWidth - 2, mY, 2, modalHeight), borderTex);

            string title = pendingAction == PendingActionType.Kick ? "⚠️ CONFIRMAR REMOÇÃO" : "⛔ CONFIRMAR BANIMENTO";
            GUI.Label(new Rect(mX + 15, mY + 12, modalWidth - 30, 22), title, headerStyle);

            string msg = pendingAction == PendingActionType.Kick
                ? $"Tem certeza que deseja REMOVER o jogador '{pendingTargetNickname}' do canal de voz?"
                : $"Tem certeza que deseja BANIR o jogador '{pendingTargetNickname}' do canal de voz?\n(Ele não poderá entrar novamente neste canal).";

            GUI.Label(new Rect(mX + 15, mY + 42, modalWidth - 30, 45), msg, modalStyle);

            string confirmBtnText = pendingAction == PendingActionType.Kick ? "SIM, REMOVER" : "SIM, BANIR";
            if (GUI.Button(new Rect(mX + 30, mY + 100, 160, 30), confirmBtnText, btnRedStyle))
            {
                ExecutePendingAction();
            }

            if (GUI.Button(new Rect(mX + modalWidth - 190, mY + 100, 160, 30), "CANCELAR", btnNormalStyle))
            {
                pendingAction = PendingActionType.None;
            }
        }
    }
}
