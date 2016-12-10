using Devious_Retention_SP;
using Devious_Retention_SP.Entities;

/// A trainer is any entity which can train units.
/// Each trainer has a specific list of units which it can
/// train. The player can create this unit by spending some
/// resources, some time after which the trained unit will
/// appear adjacent to this building (if there is room).
public interface Trainer : IEntity {
    string[] GetTrainableUnits();
    Unit[] GetTrainingQueue();
    float GetTrainingSpeed();
    void Train(string unitName);
}