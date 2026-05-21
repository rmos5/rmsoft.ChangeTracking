using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using rmsoft.ChangeTracking.Tests.TestModels;

namespace rmsoft.ChangeTracking.Tests
{
    [TestClass]
    public class PropertyChangesTrackerTests
    {
        [TestMethod]
        public void TrackedPropertyChange_AddsSingleChangeWithOriginalAndCurrentValues()
        {
            var model = new TrackableModel { TrackedName = "before" };
            var tracker = new PropertyChangesTracker(model);

            tracker.StartTracking();
            model.TrackedName = "after";

            Assert.IsTrue(tracker.HasChanges);
            Assert.AreEqual(1, tracker.ChangesCount);

            var change = tracker.Changes.Single();
            Assert.AreEqual(nameof(TrackableModel.TrackedName), change.PropertyName);
            Assert.AreEqual("after", change.Current);

            tracker.Undo();
            Assert.AreEqual("before", model.TrackedName);

            tracker.Redo();
            Assert.AreEqual("after", model.TrackedName);
        }

        [TestMethod]
        public void UntrackedPropertyChange_DoesNotCreateChanges()
        {
            var model = new TrackableModel { UntrackedName = "before" };
            var tracker = new PropertyChangesTracker(model);

            tracker.StartTracking();
            model.UntrackedName = "after";

            Assert.IsFalse(tracker.HasChanges);
            Assert.AreEqual(0, tracker.ChangesCount);
        }
    }
}
