using Speckle.SNAP.API.s8iSchema;
using System.Collections.Generic;

namespace Speckle.SNAP.API
{
  public interface ISNAPCache
  {
    bool GetNatives(out List<object> snapRecords);
    bool GetNatives<T>(out List<object> snapRecords);
    bool Upsert(object record);
    bool Upsert(List<object> records);
    bool Contains<T>(string name, out object snapRecord) where T : ISnapRecordNamed;
    void Clear();
  }
}
