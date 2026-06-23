namespace SPT.Launcher.Models.SPT
{
    public struct HwidRegisterRequestData
    {
        public string username;
        public string password;
        public string hwid;

        public HwidRegisterRequestData(string username, string password, string hwid)
        {
            this.username = username;
            this.password = password;
            this.hwid = hwid;
        }
    }

    public struct HwidResetPasswordRequestData
    {
        public string username;
        public string hwid;

        public HwidResetPasswordRequestData(string username, string hwid)
        {
            this.username = username;
            this.hwid = hwid;
        }
    }
}
