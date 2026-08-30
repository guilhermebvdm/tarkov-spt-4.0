namespace RedLineRestart
{
    public static class RedLineState
    {
        // Valor default, que será atualizado no Awake() do Plugin buscando de BepInEx Config
        public static string ServerUrl = "http://127.0.0.1:6969";
        public static string ClientId = System.Guid.NewGuid().ToString();


        // Estado do Servidor
        public static int TimeLeft = 0;
        public static int CooldownLeft = 0;
        public static int YesVotes = 0;
        public static bool InProgress = false;
        public static bool IsVetoed = false;

        // Estado Local
        public static bool VotedInThisSession = false;
        public static bool ShowVoteWindow = false;
        public static bool AmITheInitiator = false;
    }
}