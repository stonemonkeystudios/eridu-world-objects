using MessagePack;

namespace Eridu.Common {
    [MessagePackObject]
    public class EriduPlayer {
        [Key(0)]
        public int clientId;
        [Key(1)]
        public string clientEmail;
        [Key(2)]
        public bool IsRoot;
        [Key(3)]
        public bool IsAdmin;
        [Key(4)]
        public bool IsModerator;
        [Key(5)]
        public bool IsLocal;
    }
}
