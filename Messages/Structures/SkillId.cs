namespace TeraPacketRetranslator.Messages
{
    public struct SkillId {
        private ulong _raw;

        public SkillId(ulong raw)
        {
            _raw = raw;
        }

        public int Id       => (int) (_raw & 0x00000000_0FFFFFFF);
        public bool isAction       => (_raw & 0x00000000_10000000) != 0;
        public bool isReaction     => (_raw & 0x00000000_20000000) != 0;
        public bool hasHuntingZone => (_raw & 0x00000001_00000000) != 0;
        public int HuntingZone => hasHuntingZone ? (int)(_raw & 0x00000000_0FFF0000) : 0;

        public override string ToString()
        {
            return Id.ToString();
        }

    }
}
