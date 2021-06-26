using System.Threading.Tasks;
using CAC.CQS.MediatR.Application.TaskLists;
using CAC.CQS.MediatR.Application.TaskLists.DeleteTaskList;
using CAC.CQS.MediatR.Domain.TaskListAggregate;
using CAC.CQS.MediatR.UnitTests.Domain.TaskListAggregate;
using Moq;
using NUnit.Framework;

namespace CAC.CQS.MediatR.UnitTests.TaskLists.Commands.DeleteTaskList
{
    public abstract class DeleteTaskListCommandTests : CommandHandlingIntegrationTestBase<DeleteTaskListCommand>
    {
        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        private ITaskListStatisticsRepository StatisticsRepository => Resolve<ITaskListStatisticsRepository>();

        [Test]
        public async Task GivenExistingTaskListId_DeletesTaskList()
        {
            var taskList = new TaskListBuilder().WithPendingEntries(2).Build();

            taskList = await TaskListRepository.Upsert(taskList);

            await ExecuteCommand(new() { TaskListId = taskList.Id });

            var foundTaskList = await TaskListRepository.GetById(taskList.Id);

            Assert.IsNull(foundTaskList);
        }

        [Test]
        public async Task GivenNonExistingTaskListId_FailsWithEntityNotFound()
        {
            var nonExistingId = TaskListId.Of(1);
            await AssertCommandFailure(new() { TaskListId = nonExistingId }, ExpectedCommandFailure.EntityNotFound);
        }

        [Test]
        public async Task GivenSuccess_UpdatesStatistics()
        {
            var taskList = new TaskListBuilder().Build();

            taskList = await TaskListRepository.Upsert(taskList);
            
            await ExecuteCommand(new() { TaskListId = taskList.Id });

            var statistics = await StatisticsRepository.Get();
            Assert.AreEqual(1, statistics.NumberOfTaskListsDeleted);
        }

        [Test]
        public async Task GivenSuccess_PublishesNotification()
        {
            var taskList = new TaskListBuilder().Build();

            taskList = await TaskListRepository.Upsert(taskList);

            await ExecuteCommand(new() { TaskListId = taskList.Id });

            MessageQueueAdapterMock.Verify(a => a.Send(It.Is<TaskListNotificationDomainEventHandler.TaskListDeletedMessage>(m => m.TaskListId == taskList.Id)));
        }
    }
}
