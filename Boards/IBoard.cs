using Processors;
using Preferences;

namespace Boards
{
    public interface IBoard
    {
        void Init(IBoardHost boardHost);
        IProcessor Processor { get; }
        string ProcessorName { get; }
        PreferencesBase Settings { get; }
        void SaveSettings(PreferencesBase settings);
        string BoardName { get; }
    }
}