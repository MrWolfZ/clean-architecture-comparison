using System.Threading.Tasks;
using CAC.Baseline.Web.Model;
using CAC.Baseline.Web.Persistence;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Persistence
{
    public abstract class TaskListStatisticsRepositoryTests
    {
        protected abstract ITaskListStatisticsRepository Testee { get; }

        [Test]
        public async Task UpsertAndGet_StoreAndReturnStatistics()
        {
            var statistics = new TaskListStatistics
            {
                NumberOfTaskListsCreated = 3,
                NumberOfTimesTaskListsWereEdited = 2,
                NumberOfTaskListsDeleted = 1,
            };

            await Testee.Upsert(statistics);
            var stored = await Testee.Get();
            
            Assert.AreEqual(statistics, stored);
        }
    }
}
