using MagicOnion;

namespace Eridu.WorldObjects {

    public interface ILivelinessService : IService<ILivelinessService> {
        UnaryResult<string> Status();
    }
}
