using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CAC.Baseline.Web;
using CAC.Baseline.Web.Dto;
using CAC.Baseline.Web.Model;
using CAC.Baseline.Web.Persistence;
using CAC.Baseline.Web.Services;
using CAC.Core.TestUtilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Controllers
{
    [IntegrationTest]
    public sealed class TaskListsControllerTests : ControllerTestBase
    {
        private const long PremiumOwnerId = 1;
        private const long NonPremiumOwnerId = 2;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        private ITaskListEntryRepository TaskListEntryRepository => Resolve<ITaskListEntryRepository>();

        private ITaskListStatisticsService StatisticsService => Resolve<ITaskListStatisticsService>();

        [Test]
        public async Task CreateNewTaskList_GivenValidName_StoresTaskListAndReturnsTaskListId()
        {
            var expectedResponse = new CreateNewTaskListResponseDto(1);

            const string test = "test";
            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = test, OwnerId = PremiumOwnerId });

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseContent = Deserialize<CreateNewTaskListResponseDto>(responseBody);

            Assert.AreEqual(expectedResponse, responseContent);

            var storedTaskList = await TaskListRepository.GetById(responseContent!.Id);

            Assert.AreEqual(test, storedTaskList?.Name);
        }

        [Test]
        public async Task CreateNewTaskList_GivenNonExistingOwnerId_ReturnsNotFound()
        {
            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = "test", OwnerId = 99 });

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task CreateNewTaskList_GivenInvalidName_ReturnsBadRequest(string name)
        {
            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = name, OwnerId = PremiumOwnerId });

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateNewTaskList_GivenNameWithTooManyCharacters_ReturnsBadRequest()
        {
            var name = string.Join(string.Empty, Enumerable.Repeat("a", CreateNewTaskListRequestDto.MaxTaskListNameLength + 1));

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = name, OwnerId = PremiumOwnerId });

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateNewTaskList_GivenDuplicateNameForSameOwner_ReturnsConflict()
        {
            var taskList = new TaskList(99, PremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = taskList.Name, OwnerId = PremiumOwnerId });

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task CreateNewTaskList_GivenDuplicateNameForDifferentOwner_ReturnsSuccess()
        {
            var taskList = new TaskList(99, PremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = taskList.Name, OwnerId = PremiumOwnerId + 1 });

            await response.AssertStatusCode(HttpStatusCode.OK);
        }

        [Test]
        public async Task CreateNewTaskList_GivenPremiumOwnerWithExistingTaskList_ReturnsSuccess()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = "new", OwnerId = PremiumOwnerId });

            await response.AssertStatusCode(HttpStatusCode.OK);
        }

        [Test]
        public async Task CreateNewTaskList_GivenNonPremiumOwnerWithExistingTaskList_ReturnsConflict()
        {
            var taskList = new TaskList(1, NonPremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = "new", OwnerId = NonPremiumOwnerId });

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task CreateNewTaskList_GivenSuccess_UpdatesStatistics()
        {
            await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = "test", OwnerId = PremiumOwnerId });

            var statistics = await Resolve<ITaskListStatisticsService>().GetStatistics();
            Assert.AreEqual(1, statistics.NumberOfTaskListsCreated);
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndValidDescription_StoresEntryAndReturnsNoContent()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);

            const string taskDescription = "task";
            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = taskDescription });

            await response.AssertStatusCode(HttpStatusCode.NoContent);

            var storedEntries = await TaskListEntryRepository.GetEntriesForTaskList(taskList.Id);

            Assert.AreEqual(taskDescription, storedEntries.Single().Description);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task AddTaskToList_GivenExistingTaskListIdAndInvalidDescription_ReturnsBadRequest(string description)
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = description });

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndDescriptionWithTooManyCharacters_ReturnsBadRequest()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            var description = string.Join(string.Empty, Enumerable.Repeat("a", AddTaskToListRequestDto.MaxTaskDescriptionLength + 1));

            await TaskListRepository.Store(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = description });

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AddTaskToList_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var response = await HttpClient.PostAsJsonAsync("taskLists/1/tasks", new AddTaskToListRequestDto { TaskDescription = "task" });

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task AddTaskToList_GivenTaskListWithLessThanFiveEntriesAndNonPremiumOwner_ReturnsNoContent()
        {
            var taskList = new TaskList(1, NonPremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);
            
            await TaskListEntryRepository.Store(new TaskListEntry(1, taskList.Id, "task 1", false));
            await TaskListEntryRepository.Store(new TaskListEntry(2, taskList.Id, "task 2", false));
            await TaskListEntryRepository.Store(new TaskListEntry(3, taskList.Id, "task 3", false));
            await TaskListEntryRepository.Store(new TaskListEntry(4, taskList.Id, "task 4", false));

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = "new" });

            await response.AssertStatusCode(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task AddTaskToList_GivenTaskListWithFiveEntriesAndNonPremiumOwner_ReturnsConflict()
        {
            var taskList = new TaskList(1, NonPremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);
            
            await TaskListEntryRepository.Store(new TaskListEntry(1, taskList.Id, "task 1", false));
            await TaskListEntryRepository.Store(new TaskListEntry(2, taskList.Id, "task 2", false));
            await TaskListEntryRepository.Store(new TaskListEntry(3, taskList.Id, "task 3", false));
            await TaskListEntryRepository.Store(new TaskListEntry(4, taskList.Id, "task 4", false));
            await TaskListEntryRepository.Store(new TaskListEntry(5, taskList.Id, "task 5", false));

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = "new" });

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task AddTaskToList_GivenSuccess_UpdatesStatistics()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);

            await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = "task" });

            var statistics = await Resolve<ITaskListStatisticsService>().GetStatistics();
            Assert.AreEqual(1, statistics.NumberOfTimesTaskListsWereEdited);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndValidEntryIndex_UpdatesTaskListAndReturnsNoContent()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            var taskListEntry = new TaskListEntry(1, taskList.Id, "task 1", false);

            await TaskListRepository.Store(taskList);

            await TaskListEntryRepository.Store(taskListEntry);
            await TaskListEntryRepository.Store(new TaskListEntry(2, taskList.Id, "task 2", false));

            using var content = new StringContent(string.Empty);
            var response = await HttpClient.PutAsync($"taskLists/{taskList.Id}/tasks/{taskListEntry.Id}/isDone", content);

            await response.AssertStatusCode(HttpStatusCode.NoContent);

            var storedEntry = await TaskListEntryRepository.GetById(taskList.Id);

            Assert.IsTrue(storedEntry?.IsDone);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndNonExistingEntryIndex_ReturnsBadRequest()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);
            
            await TaskListEntryRepository.Store(new TaskListEntry(1, taskList.Id, "task 1", false));
            await TaskListEntryRepository.Store(new TaskListEntry(2, taskList.Id, "task 2", false));

            using var content = new StringContent(string.Empty);
            var response = await HttpClient.PutAsync($"taskLists/{taskList.Id}/tasks/3/isDone", content);

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            using var content = new StringContent(string.Empty);
            var response = await HttpClient.PutAsync("taskLists/1/tasks/1/isDone", content);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenSuccess_UpdatesStatistics()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            var entry = new TaskListEntry(1, taskList.Id, "task 1", false);

            await TaskListRepository.Store(taskList);
            await TaskListEntryRepository.Store(entry);

            using var content = new StringContent(string.Empty);
            await HttpClient.PutAsync($"taskLists/{taskList.Id}/tasks/{entry.Id}/isDone", content);

            var statistics = await Resolve<ITaskListStatisticsService>().GetStatistics();
            Assert.AreEqual(1, statistics.NumberOfTimesTaskListsWereEdited);
        }

        [Test]
        public async Task GetAll_GivenExistingTaskLists_ReturnsTaskLists()
        {
            var taskList1 = new TaskList(1, PremiumOwnerId, "test 1");
            var taskList2 = new TaskList(2, PremiumOwnerId, "test 2");

            await TaskListRepository.Store(taskList1);
            await TaskListRepository.Store(taskList2);
            
            await TaskListEntryRepository.Store(new TaskListEntry(1, taskList1.Id, "task 1", false));
            await TaskListEntryRepository.Store(new TaskListEntry(2, taskList1.Id, "task 2", false));

            var response = await HttpClient.GetAsync("taskLists");

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var lists = Deserialize<IReadOnlyCollection<TaskListDto>>(responseString)!;

            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == taskList1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == taskList2.Name));
        }

        [Test]
        public async Task GetById_GivenExistingTaskListId_ReturnsTaskList()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);
            
            await TaskListEntryRepository.Store(new TaskListEntry(1, taskList.Id, "task 1", false));
            await TaskListEntryRepository.Store(new TaskListEntry(2, taskList.Id, "task 2", false));

            var response = await HttpClient.GetAsync($"taskLists/{taskList.Id}");

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseContent = Deserialize<TaskListDto>(responseString);

            Assert.AreEqual(taskList.Name, responseContent!.Name);
            Assert.AreEqual(2, responseContent.Entries.Count);
            Assert.IsTrue(responseContent.Entries.Any(e => e.Id == 1));
            Assert.IsTrue(responseContent.Entries.Any(e => e.Id == 2));
        }

        [Test]
        public async Task GetById_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var response = await HttpClient.GetAsync("taskLists/1");

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetAllWithPendingEntries_GivenExistingTaskLists_ReturnsTaskListsWithPendingEntries()
        {
            var taskList1 = new TaskList(1, PremiumOwnerId, "test 1");
            var taskList2 = new TaskList(2, PremiumOwnerId, "test 2");
            var taskList3 = new TaskList(3, PremiumOwnerId, "test 3");

            await TaskListRepository.Store(taskList1);
            await TaskListRepository.Store(taskList2);
            await TaskListRepository.Store(taskList3);
            
            await TaskListEntryRepository.Store(new TaskListEntry(1, taskList1.Id, "task 1", true));
            await TaskListEntryRepository.Store(new TaskListEntry(2, taskList1.Id, "task 2", false));
            await TaskListEntryRepository.Store(new TaskListEntry(3, taskList2.Id, "task 1", false));
            await TaskListEntryRepository.Store(new TaskListEntry(4, taskList3.Id, "task 1", true));

            var response = await HttpClient.GetAsync("taskLists/withPendingEntries");

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var lists = Deserialize<IReadOnlyCollection<TaskListDto>>(responseString)!;

            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == taskList1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == taskList2.Name));
        }

        [Test]
        public async Task DeleteById_GivenExistingTaskListId_DeletesTaskListAndReturnsNoContent()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);
            
            await TaskListEntryRepository.Store(new TaskListEntry(1, taskList.Id, "task 1", false));
            await TaskListEntryRepository.Store(new TaskListEntry(2, taskList.Id, "task 2", false));

            var response = await HttpClient.DeleteAsync($"taskLists/{taskList.Id}");

            await response.AssertStatusCode(HttpStatusCode.NoContent);

            var foundTaskList = await TaskListRepository.GetById(taskList.Id);

            Assert.IsNull(foundTaskList);
        }

        [Test]
        public async Task DeleteById_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var response = await HttpClient.DeleteAsync("taskLists/1");

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task DeleteById_GivenSuccess_UpdatesStatistics()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await TaskListRepository.Store(taskList);

            await HttpClient.DeleteAsync($"taskLists/{taskList.Id}");

            var statistics = await StatisticsService.GetStatistics();
            Assert.AreEqual(1, statistics.NumberOfTaskListsDeleted);
        }

        protected override void ConfigureWebHost(IWebHostBuilder webHost)
        {
            webHost.UseStartup<Startup>();
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.Replace(ServiceDescriptor.Singleton<ITaskListRepository, InMemoryTaskListRepository>());
            services.Replace(ServiceDescriptor.Singleton<ITaskListEntryRepository, InMemoryTaskListEntryRepository>());
        }
    }
}
