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
        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task CreateNewTaskList_GivenValidName_ReturnsTaskListId()
        {
            var expectedResponse = new CreateNewTaskListResponseDto(1);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = "test" });

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseContent = Deserialize<CreateNewTaskListResponseDto>(responseBody);

            Assert.AreEqual(expectedResponse, responseContent);
        }

        [Test]
        public async Task CreateNewTaskList_GivenInvalidName_ReturnsBadRequest()
        {
            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = string.Empty });

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateNewTaskList_GivenDuplicateName_ReturnsBadRequest()
        {
            var taskList = new TaskList(99, "test");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new CreateNewTaskListRequestDto { Name = taskList.Name });

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndValidDescription_ReturnsNoContent()
        {
            var taskList = new TaskList(1, "test");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = "task" });

            await response.AssertStatusCode(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndInvalidDescription_ReturnsBadRequest()
        {
            var taskList = new TaskList(1, "test");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new AddTaskToListRequestDto { TaskDescription = string.Empty });

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AddTaskToList_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var response = await HttpClient.PostAsJsonAsync("taskLists/1/tasks", new AddTaskToListRequestDto { TaskDescription = "task" });

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndValidItemIndex_ReturnsNoContent()
        {
            var taskList = new TaskList(1, "test");
            taskList.AddItem("task 1");
            taskList.AddItem("task 2");

            await TaskListRepository.Upsert(taskList);

            using var content = new StringContent(string.Empty);
            var response = await HttpClient.PutAsync($"taskLists/{taskList.Id}/tasks/1/isDone", content);

            await response.AssertStatusCode(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndNonExistingItemIndex_ReturnsBadRequest()
        {
            var taskList = new TaskList(1, "test");
            taskList.AddItem("task 1");
            taskList.AddItem("task 2");

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
            var taskList1 = new TaskList(1, "test 1");
            taskList1.AddItem("task 1");
            taskList1.AddItem("task 2");

            var taskList2 = new TaskList(2, "test 2");

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
            var taskList = new TaskList(1, "test");
            taskList.AddItem("task 1");
            taskList.AddItem("task 2");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.GetAsync($"taskLists/{taskList.Id}");

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseContent = Deserialize<TaskListDto>(responseString);

            Assert.AreEqual(taskList.Name, responseContent!.Name);
            Assert.IsTrue(taskList.Items.SequenceEqual(responseContent.Items));
        }

        [Test]
        public async Task GetById_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var response = await HttpClient.GetAsync("taskLists/1");

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetAllWithPendingItems_GivenExistingTaskLists_ReturnsTaskListsWithPendingItems()
        {
            var taskList1 = new TaskList(1, "test 1");
            taskList1.AddItem("task 1");
            taskList1.AddItem("task 2");
            taskList1.MarkItemAsDone(0);

            var taskList2 = new TaskList(2, "test 2");
            taskList2.AddItem("task 1");
            
            var taskList3 = new TaskList(3, "test 3");
            taskList3.AddItem("task 1");
            taskList3.MarkItemAsDone(0);

            await TaskListRepository.Upsert(taskList1);
            await TaskListRepository.Upsert(taskList2);
            await TaskListRepository.Upsert(taskList3);

            var response = await HttpClient.GetAsync("taskLists/withPendingItems");

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var lists = Deserialize<IReadOnlyCollection<TaskListDto>>(responseString)!;

            Assert.AreEqual(2, lists.Count);
            Assert.IsTrue(lists.Any(l => l.Name == taskList1.Name));
            Assert.IsTrue(lists.Any(l => l.Name == taskList2.Name));
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
