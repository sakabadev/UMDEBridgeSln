using System.Collections.Generic;

namespace UMDEBridge.Editor.Repository
{
    public interface IMemoryTableRepository
    {
        void SaveAll<T>(List<T> items);
        IReadOnlyList<T> FindAll<T>();
    }
}