using MessagePack;

namespace Eridu.WorldObjects {
    [MessagePackObject]
    public class WorldObjectData {
        [Key(0)]
        public int instanceId;
        [Key(1)]
        public string dataType;
        [Key(2)]
        public string data;
    }
}
