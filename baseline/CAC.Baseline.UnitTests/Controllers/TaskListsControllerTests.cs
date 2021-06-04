using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CAC.Baseline.Web;
using CAC.Baseline.Web.Controllers;
using CAC.Baseline.Web.Data;
using CAC.Baseline.Web.Model;
using CAC.Core.TestUtilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Controllers
{
    [IntegrationTest]
    public sealed class TaskListsControllerTests : ControllerTestBase
    {
        private const long PremiumOwnerId = 1;
        private const long NonPremiumOwnerId = 2;

        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task CreateNewTaskList_GivenValidName_ReturnsTaskListId()
        {
            var expectedResponse = new CreateNewTaskListResponseDto(1);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = "test", OwnerId = PremiumOwnerId });

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseContent = Deserialize<CreateNewTaskListResponseDto>(responseBody);

            Assert.AreEqual(expectedResponse, responseContent);
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

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = taskList.Name, OwnerId = PremiumOwnerId });

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task CreateNewTaskList_GivenDuplicateNameForDifferentOwner_ReturnsSuccess()
        {
            var taskList = new TaskList(99, PremiumOwnerId, "test");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = taskList.Name, OwnerId = PremiumOwnerId + 1 });

            await response.AssertStatusCode(HttpStatusCode.OK);
        }

        [Test]
        public async Task CreateNewTaskList_GivenPremiumOwnerWithExistingTaskList_ReturnsSuccess()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = "new", OwnerId = PremiumOwnerId });

            await response.AssertStatusCode(HttpStatusCode.OK);
        }

        [Test]
        public async Task CreateNewTaskList_GivenNonPremiumOwnerWithExistingTaskList_ReturnsConflict()
        {
            var taskList = new TaskList(1, NonPremiumOwnerId, "test");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = "new", OwnerId = NonPremiumOwnerId });

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndValidDescription_ReturnsNoContent()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = "task" });

            await response.AssertStatusCode(HttpStatusCode.NoContent);
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public async Task AddTaskToList_GivenExistingTaskListIdAndInvalidDescription_ReturnsBadRequest(string description)
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = description });

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndDescriptionWithTooManyCharacters_ReturnsBadRequest()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            var description = string.Join(string.Empty, Enumerable.Repeat("a", AddTaskToListRequestDto.MaxTaskDescriptionLength + 1));

            await TaskListRepository.Upsert(taskList);

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
            taskList.AddEntry("task 1");
            taskList.AddEntry("task 2");
            taskList.AddEntry("task 3");
            taskList.AddEntry("task 4");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = "new" });

            await response.AssertStatusCode(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task AddTaskToList_GivenTaskListWithFiveEntriesAndNonPremiumOwner_ReturnsConflict()
        {
            var taskList = new TaskList(1, NonPremiumOwnerId, "test");
            taskList.AddEntry("task 1");
            taskList.AddEntry("task 2");
            taskList.AddEntry("task 3");
            taskList.AddEntry("task 4");
            taskList.AddEntry("task 5");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = "new" });

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndValidEntryIndex_ReturnsNoContent()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            taskList.AddEntry("task 1");
            taskList.AddEntry("task 2");

            await TaskListRepository.Upsert(taskList);

            using var content = new StringContent(string.Empty);
            var response = await HttpClient.PutAsync($"taskLists/{taskList.Id}/tasks/1/isDone", content);

            await response.AssertStatusCode(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndNonExistingEntryIndex_ReturnsBadRequest()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            taskList.AddEntry("task 1");
            taskList.AddEntry("task 2");

            await TaskListRepository.Upsert(taskList);

            using var content = new StringContent(string.Empty);
            var response = await HttpClient.PutAsync($"taskLists/{taskList.Id}/tasks/2/isDone", content);

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
        public async Task GetAll_GivenExistingTaskLists_ReturnsTaskLists()
        {
            var taskList1 = new TaskList(1, PremiumOwnerId, "test 1");
            taskList1.AddEntry("task 1");
            taskList1.AddEntry("task 2");

            var taskList2 = new TaskList(2, PremiumOwnerId, "test 2");

            await TaskListRepository.Upsert(taskList1);
            await TaskListRepository.Upsert(taskList2);

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
            taskList.AddEntry("task 1");
            taskList.AddEntry("task 2");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.GetAsync($"taskLists/{taskList.Id}");

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseContent = Deserialize<TaskListDto>(responseString);

            Assert.AreEqual(taskList.Name, responseContent!.Name);
            Assert.IsTrue(taskList.Entries.SequenceEqual(responseContent.Entries));
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
            taskList1.AddEntry("task 1");
            taskList1.AddEntry("task 2");
            taskList1.MarkEntryAsDone(0);

            var taskList2 = new TaskList(2, PremiumOwnerId, "test 2");
            taskList2.AddEntry("task 1");
            
            var taskList3 = new TaskList(3, PremiumOwnerId, "test 3");
            taskList3.AddEntry("task 1");
            taskList3.MarkEntryAsDone(0);

            await TaskListRepository.Upsert(taskList1);
            await TaskListRepository.Upsert(taskList2);
            await TaskListRepository.Upsert(taskList3);

            var response = await HttpClient.GetAsync("taskLists/withPendingEntries");

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var lists = Deserialize<IReadOnlyCollection<TaskListDto>>(responseString)!;

            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == taskList1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == taskList2.Name));
        }

        [Test]
        public async Task DeleteById_GivenExistingTaskListId_DeletesTaskList()
        {
            var taskList = new TaskList(1, PremiumOwnerId, "test");
            taskList.AddEntry("task 1");
            taskList.AddEntry("task 2");

            await TaskListRepository.Upsert(taskList);

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

        protected override void ConfigureWebHost(IWebHostBuilder webHost)
        {
            webHost.UseStartup<Startup>();
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITaskListRepository, InMemoryTaskListRepository>();
        }
    }
}
