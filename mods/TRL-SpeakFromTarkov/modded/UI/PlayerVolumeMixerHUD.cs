using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using Comfort.Common;
using EFT;
using UnityEngine;
using TRL_SpeakFromTarkov.Audio;
using TRL_SpeakFromTarkov.Network;

namespace TRL_SpeakFromTarkov.UI
{
    [Serializable]
    public class PlayerVolumeData
    {
        public string ProfileId = string.Empty;
        public string Nickname = string.Empty;
        public float Volume = 1.0f;
        public bool IsMuted = false;
    }

    [Serializable]
    public class PlayerVolumeConfig
    {
        public List<PlayerVolumeData> Players = new List<PlayerVolumeData>();
    }

    public class PlayerVolumeMixerHUD : MonoBehaviour
    {
        public static PlayerVolumeMixerHUD Instance { get; private set; } = null!;
        public bool IsOpen { get; private set; } = false;

        private static readonly Dictionary<string, float> _playerVolumes = new Dictionary<string, float>();
        private static readonly HashSet<string> _mutedPlayers = new HashSet<string>();
        private static readonly Dictionary<string, string> _playerNicknames = new Dictionary<string, string>();

        private static string ConfigFilePath => Path.Combine(BepInEx.Paths.ConfigPath, "TRL-SpeakFromTarkov-PlayersVolume.json");

        private Texture2D? _bgDimTex;
        private Texture2D? _panelTex;
        private Texture2D? _headerTex;
        private Texture2D? _rowBgTex;
        private Texture2D? _btnTex;
        private Texture2D? _btnHoverTex;
        private Texture2D? _btnDangerTex;
        private Texture2D? _btnSuccessTex;

        private Vector2 _scrollPos = Vector2.zero;
        private Behaviour? _playerOwnerBehaviour;

        void Awake()
        {
            Instance = this;
            LoadConfig();
            InitTextures();
        }

        private void InitTextures()
        {
            _bgDimTex       = MakeTex(new Color(0f, 0f, 0f, 0.65f));
            _panelTex      = MakeTex(new Color(0.08f, 0.10f, 0.12f, 0.95f));
            _headerTex     = MakeTex(new Color(0.12f, 0.16f, 0.20f, 0.98f));
            _rowBgTex      = MakeTex(new Color(0.14f, 0.17f, 0.21f, 0.85f));
            _btnTex        = MakeTex(new Color(0.20f, 0.25f, 0.30f, 0.90f));
            _btnHoverTex   = MakeTex(new Color(0.30f, 0.38f, 0.45f, 0.95f));
            _btnDangerTex  = MakeTex(new Color(0.75f, 0.20f, 0.20f, 0.90f));
            _btnSuccessTex = MakeTex(new Color(0.20f, 0.70f, 0.30f, 0.90f));
        }

        private Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }

        private void OnDestroy()
        {
            DestroyTex(ref _bgDimTex);
            DestroyTex(ref _panelTex);
            DestroyTex(ref _headerTex);
            DestroyTex(ref _rowBgTex);
            DestroyTex(ref _btnTex);
            DestroyTex(ref _btnHoverTex);
            DestroyTex(ref _btnDangerTex);
            DestroyTex(ref _btnSuccessTex);
        }

        private void DestroyTex(ref Texture2D? tex)
        {
            if (tex != null)
            {
                Destroy(tex);
                tex = null;
            }
        }

        public static float GetPlayerEffectiveVolume(string profileId)
        {
            if (string.IsNullOrEmpty(profileId)) return 1.0f;
            if (_mutedPlayers.Contains(profileId)) return 0.0f;
            if (_playerVolumes.TryGetValue(profileId, out float vol)) return vol;
            return 1.0f;
        }

        public static void SetPlayerVolume(string profileId, float volume)
        {
            if (string.IsNullOrEmpty(profileId)) return;
            volume = Mathf.Clamp(volume, 0.0f, 2.0f);
            _playerVolumes[profileId] = volume;
            ApplyToSpeaker(profileId);
            SaveConfig();
        }

        public static void SetPlayerMuted(string profileId, bool muted)
        {
            if (string.IsNullOrEmpty(profileId)) return;
            if (muted) _mutedPlayers.Add(profileId);
            else _mutedPlayers.Remove(profileId);
            ApplyToSpeaker(profileId);
            SaveConfig();
        }

        private static void ApplyToSpeaker(string profileId)
        {
            if (Singleton<GameWorld>.Instantiated && SftNetwork.Instance != null)
            {
                float effVol = GetPlayerEffectiveVolume(profileId);
                // Busca o RemoteSpeaker deste jogador se ativo
                var speakers = UnityEngine.Object.FindObjectsOfType<RemoteSpeaker>();
                foreach (var s in speakers)
                {
                    if (s != null && s.TargetProfileId == profileId)
                    {
                        s.SetVolume(effVol);
                    }
                }
            }
        }

        private static void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    var cfg = JsonUtility.FromJson<PlayerVolumeConfig>(json);
                    if (cfg != null && cfg.Players != null)
                    {
                        _playerVolumes.Clear();
                        _mutedPlayers.Clear();
                        _playerNicknames.Clear();

                        foreach (var p in cfg.Players)
                        {
                            if (!string.IsNullOrEmpty(p.ProfileId))
                            {
                                _playerVolumes[p.ProfileId] = p.Volume;
                                if (p.IsMuted) _mutedPlayers.Add(p.ProfileId);
                                if (!string.IsNullOrEmpty(p.Nickname)) _playerNicknames[p.ProfileId] = p.Nickname;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log.LogWarning($"[SFT-MIXER] Erro ao carregar configurações de volume dos jogadores: {ex.Message}");
            }
        }

        private static void SaveConfig()
        {
            try
            {
                var cfg = new PlayerVolumeConfig();
                var allIds = new HashSet<string>(_playerVolumes.Keys.Concat(_mutedPlayers).Concat(_playerNicknames.Keys));

                foreach (var id in allIds)
                {
                    float vol = _playerVolumes.TryGetValue(id, out float v) ? v : 1.0f;
                    bool muted = _mutedPlayers.Contains(id);
                    string nick = _playerNicknames.TryGetValue(id, out string n) ? n : id;

                    cfg.Players.Add(new PlayerVolumeData
                    {
                        ProfileId = id,
                        Nickname = nick,
                        Volume = vol,
                        IsMuted = muted
                    });
                }

                string json = JsonUtility.ToJson(cfg, true);
                File.WriteAllText(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                VoIPPlugin.Log.LogWarning($"[SFT-MIXER] Erro ao salvar configurações de volume dos jogadores: {ex.Message}");
            }
        }

        public void Toggle()
        {
            if (IsOpen) Close();
            else Open();
        }

        public void Open()
        {
            IsOpen = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SetGameInputBlocked(true);

            try
            {
                var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
                if (mainPlayer != null)
                {
                    var behaviours = mainPlayer.GetComponents<Behaviour>();
                    foreach (var b in behaviours)
                    {
                        if (b != null && b.GetType().Name.Contains("PlayerOwner"))
                        {
                            _playerOwnerBehaviour = b;
                            b.enabled = false;
                            break;
                        }
                    }
                }
            }
            catch { }
        }

        public void Close()
        {
            IsOpen = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SetGameInputBlocked(false);

            try
            {
                if (_playerOwnerBehaviour != null)
                {
                    _playerOwnerBehaviour.enabled = true;
                    _playerOwnerBehaviour = null;
                }
            }
            catch { }
        }

        private static void SetGameInputBlocked(bool blocked)
        {
            try
            {
                var gpoType = Type.GetType("EFT.GamePlayerOwner, Assembly-CSharp");
                if (gpoType != null)
                {
                    var flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
                    gpoType.GetMethod("SetIgnoreInput", flags)?.Invoke(null, new object[] { blocked });
                    gpoType.GetMethod("SetIgnoreInputInNPCDialog", flags)?.Invoke(null, new object[] { blocked });
                    gpoType.GetMethod("SetIgnoreInputWithKeepResetLook", flags)?.Invoke(null, new object[] { blocked });

                    var cmdType = Type.GetType("EFT.InputSystem.ECommand, Assembly-CSharp");
                    if (cmdType != null)
                    {
                        string[] cmdNames = new[] { "Escape", "ToggleInventory", "ToggleShooting", "EndShooting", "Jump", "PressThrowGrenade", "ThrowGrenade", "ToggleProne", "ToggleDuck", "ReloadWeapon" };
                        var cmdList = Array.CreateInstance(cmdType, cmdNames.Length);
                        for (int i = 0; i < cmdNames.Length; i++)
                        {
                            cmdList.SetValue(Enum.Parse(cmdType, cmdNames[i]), i);
                        }

                        string methodName = blocked ? "AddIgnoreInputCommands" : "RemoveIgnoreInputCommands";
                        gpoType.GetMethod(methodName, flags)?.Invoke(null, new object[] { cmdList });
                    }
                }
            }
            catch { }
        }

        void Update()
        {
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        void OnGUI()
        {
            if (!IsOpen) return;

            // Escurecimento de fundo
            if (_bgDimTex != null)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _bgDimTex);
            }

            float modalW = 560f;
            float modalH = 460f;
            float modalX = (Screen.width - modalW) / 2f;
            float modalY = (Screen.height - modalH) / 2f;

            // Painel Principal
            if (_panelTex != null) GUI.DrawTexture(new Rect(modalX, modalY, modalW, modalH), _panelTex);

            // Cabeçalho
            if (_headerTex != null) GUI.DrawTexture(new Rect(modalX, modalY, modalW, 45f), _headerTex);

            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.95f, 0.85f, 0.40f) }
            };
            GUI.Label(new Rect(modalX + 18f, modalY + 5f, modalW - 80f, 35f), "🎛️ SFT · IN-RAID PLAYER VOLUME MIXER", headerStyle);

            var closeBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white, background = _btnDangerTex }
            };
            if (GUI.Button(new Rect(modalX + modalW - 42f, modalY + 8f, 30f, 28f), "✕", closeBtnStyle))
            {
                Close();
                return;
            }

            var subHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Italic,
                normal = { textColor = new Color(0.70f, 0.75f, 0.80f) }
            };
            GUI.Label(new Rect(modalX + 18f, modalY + 50f, modalW - 36f, 20f), "Ajuste o volume de cada jogador para a sua audição local (0% a 200%). Salvo automaticamente.", subHeaderStyle);

            // Lista de Jogadores da Raid
            var playersList = GetRemoteRaidPlayers();

            float listX = modalX + 15f;
            float listY = modalY + 75f;
            float listW = modalW - 30f;
            float listH = modalH - 140f;

            float contentH = Mathf.Max(listH, playersList.Count * 65f + 10f);
            var viewRect = new Rect(0, 0, listW - 20f, contentH);

            _scrollPos = GUI.BeginScrollView(new Rect(listX, listY, listW, listH), _scrollPos, viewRect);

            if (playersList.Count == 0)
            {
                var emptyStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    fontStyle = FontStyle.Italic,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.gray }
                };
                GUI.Label(new Rect(0, 50f, listW - 20f, 40f), "Nenhum outro jogador remoto detectado na raid atual.", emptyStyle);
            }
            else
            {
                float rowY = 5f;
                foreach (var p in playersList)
                {
                    DrawPlayerRow(p, 0f, rowY, listW - 25f, 58f);
                    rowY += 64f;
                }
            }

            GUI.EndScrollView();

            // Rodapé com botões de ação global
            float footerY = modalY + modalH - 52f;
            var resetAllStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white, background = _btnTex }
            };

            if (GUI.Button(new Rect(modalX + 18f, footerY, 160f, 34f), "🔄 Resetar Todos (100%)", resetAllStyle))
            {
                foreach (var p in playersList)
                {
                    _playerVolumes[p.ProfileId] = 1.0f;
                    _mutedPlayers.Remove(p.ProfileId);
                    ApplyToSpeaker(p.ProfileId);
                }
                SaveConfig();
            }

            var doneStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white, background = _btnSuccessTex }
            };
            if (GUI.Button(new Rect(modalX + modalW - 138f, footerY, 120f, 34f), "Concluído", doneStyle))
            {
                Close();
            }
        }

        private void DrawPlayerRow(PlayerVolumeData player, float x, float y, float w, float h)
        {
            if (_rowBgTex != null) GUI.DrawTexture(new Rect(x, y, w, h), _rowBgTex);

            bool isMuted = _mutedPlayers.Contains(player.ProfileId);
            float currentVol = _playerVolumes.TryGetValue(player.ProfileId, out float v) ? v : 1.0f;

            // Nome do Jogador
            var nameStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = isMuted ? new Color(0.9f, 0.3f, 0.3f) : Color.white }
            };
            string displayName = !string.IsNullOrEmpty(player.Nickname) ? player.Nickname : player.ProfileId;
            GUI.Label(new Rect(x + 12f, y + 6f, 180f, 22f), displayName, nameStyle);

            // Label de Volume
            var volLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = isMuted ? new Color(0.9f, 0.3f, 0.3f) : (currentVol > 1.05f ? new Color(0.95f, 0.85f, 0.3f) : new Color(0.4f, 0.9f, 0.5f)) }
            };

            string volText = isMuted ? "MUTADO (0%)" : (currentVol > 1.05f ? $"{currentVol * 100:F0}% [BOOST]" : $"{currentVol * 100:F0}%");
            GUI.Label(new Rect(x + w - 190f, y + 6f, 100f, 22f), volText, volLabelStyle);

            // Slider de Volume (0% a 200%)
            float sliderX = x + 12f;
            float sliderY = y + 30f;
            float sliderW = w - 180f;

            GUI.enabled = !isMuted;
            float newVol = GUI.HorizontalSlider(new Rect(sliderX, sliderY, sliderW, 20f), currentVol, 0.0f, 2.0f);
            GUI.enabled = true;

            if (Math.Abs(newVol - currentVol) > 0.01f)
            {
                SetPlayerVolume(player.ProfileId, newVol);
            }

            // Botão MUTE / UNMUTE
            var muteBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white, background = isMuted ? _btnSuccessTex : _btnDangerTex }
            };

            if (GUI.Button(new Rect(x + w - 75f, y + 14f, 65f, 30f), isMuted ? "Unmute" : "Mute", muteBtnStyle))
            {
                SetPlayerMuted(player.ProfileId, !isMuted);
            }
        }

        private List<PlayerVolumeData> GetRemoteRaidPlayers()
        {
            var result = new List<PlayerVolumeData>();
            string myProfileId = string.Empty;

            if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
            {
                myProfileId = Singleton<GameWorld>.Instance.MainPlayer.ProfileId;
            }

            if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.AllAlivePlayersList != null)
            {
                foreach (var p in Singleton<GameWorld>.Instance.AllAlivePlayersList)
                {
                    if (p == null) continue;
                    string pId = p.ProfileId;
                    if (string.IsNullOrEmpty(pId) && p.Profile != null) pId = p.Profile.Id;
                    if (string.IsNullOrEmpty(pId) || pId == myProfileId) continue;

                    string nick = p.Profile != null && !string.IsNullOrEmpty(p.Profile.Nickname) ? p.Profile.Nickname : pId;
                    _playerNicknames[pId] = nick;

                    float vol = _playerVolumes.TryGetValue(pId, out float v) ? v : 1.0f;
                    bool muted = _mutedPlayers.Contains(pId);

                    result.Add(new PlayerVolumeData
                    {
                        ProfileId = pId,
                        Nickname = nick,
                        Volume = vol,
                        IsMuted = muted
                    });
                }
            }

            return result;
        }
    }
}
