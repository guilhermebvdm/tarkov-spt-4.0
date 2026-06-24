namespace TarkovIRL
{
    internal class Injury
    {
        public Injury(EInjury type, float time)
        {
            this.InjuryType = type;
            TimeInflicted = time;
        }
        public enum EInjury {  LIGHT_BLEED, HEAVY_BLEED, BONE_BREAK, };
        public EInjury InjuryType { get; set; }
        public float TimeInflicted { get; set; }
        public float TimeUntilEffect
        {
            get
            {
                if (InjuryType == EInjury.LIGHT_BLEED)
                {
                    return 60f;
                }
                else if (InjuryType == EInjury.HEAVY_BLEED)
                {
                    return 30f;
                }
                else
                {
                    return 20f;
                }
            }
        }
        public float InjuryWeight
        {
            get
            {
                if (InjuryType == EInjury.LIGHT_BLEED)
                {
                    return 5f;
                }
                else if (InjuryType == EInjury.HEAVY_BLEED)
                {
                    return 20f;
                }
                else
                {
                    return 20f;
                }
            }
        }
    }
}
