using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CAC.Core.TestUtilities;
using CAC.DDD.Domain.TaskLists;
using CAC.DDD.Web;
using CAC.DDD.Web.TaskLists;
using Microsoft.AspNetCore.Hosting;
using NUnit.Framework;

namespace CAC.DDD.UnitTests.Web.TaskLists
{
    [IntegrationTest]
    public sealed class TaskListControllerTests : ControllerTestBase
    {
        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task CreateNewTaskList_GivenValidName_ReturnsTaskListId()
        {
            var expectedResponse = new TaskListController.CreateNewTaskListResponse(1);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new TaskListController.CreateNewTaskListRequest { Name = "test" });

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseBody = await response.Content.ReadAsStringAsync();
            var responseContent = Deserialize<TaskListController.CreateNewTaskListResponse>(responseBody);

            Assert.AreEqual(expectedResponse, responseContent);
        }

        [Test]
        public async Task CreateNewTaskList_GivenInvalidName_ReturnsBadRequest()
        {
            var response = await HttpClient.PostAsJsonAsync("taskLists", new TaskListController.CreateNewTaskListRequest { Name = string.Empty });

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateNewTaskList_GivenDuplicateName_ReturnsConflict()
        {
            var taskList = TaskList.New(99, "test");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync("taskLists", new TaskListController.CreateNewTaskListRequest { Name = taskList.Name });

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndValidDescription_ReturnsNoContent()
        {
            var taskList = TaskList.New(1, "test");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new TaskListController.AddTaskToListRequest { TaskDescription = "task" });

            await response.AssertStatusCode(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndInvalidDescription_ReturnsBadRequest()
        {
            var taskList = TaskList.New(1, "test");

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{taskList.Id}/tasks", new TaskListController.AddTaskToListRequest { TaskDescription = string.Empty });

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AddTaskToList_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var id = TaskListId.Of(1);

            var response = await HttpClient.PostAsJsonAsync($"taskLists/{id}/tasks", new TaskListController.AddTaskToListRequest { TaskDescription = "task" });

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndValidItemIndex_ReturnsNoContent()
        {
            var taskList = TaskList.New(1, "test").AddItem("task 1").AddItem("task 2");

            await TaskListRepository.Upsert(taskList);

            using var content = new StringContent(string.Empty);
            var response = await HttpClient.PutAsync($"taskLists/{taskList.Id}/tasks/1/isDone", content);

            await response.AssertStatusCode(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndNonExistingItemIndex_ReturnsConflict()
        {
            var taskList = TaskList.New(1, "test").AddItem("task 1").AddItem("task 2");

            await TaskListRepository.Upsert(taskList);

            using var content = new StringContent(string.Empty);
            var response = await HttpClient.PutAsync($"taskLists/{taskList.Id}/tasks/2/isDone", content);

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var id = TaskListId.Of(1);

            using var content = new StringContent(string.Empty);
            var response = await HttpClient.PutAsync($"taskLists/{id}/tasks/1/isDone", content);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetTaskListById_GivenExistingTaskListId_ReturnsTaskList()
        {
            var taskList = TaskList.New(1, "test").AddItem("task 1").AddItem("task 2");
            var expectedResponse = new TaskListController.GetTaskListByIdQueryResponse(taskList);

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.GetAsync($"taskLists/{taskList.Id}");

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseContent = Deserialize<TaskListController.GetTaskListByIdQueryResponse>(responseString);

            Assert.AreEqual(expectedResponse, responseContent);
        }

        [Test]
        public async Task GetTaskListById_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var id = TaskListId.Of(1);

            var response = await HttpClient.GetAsync($"taskLists/{id}");

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        protected override void ConfigureWebHost(IWebHostBuilder webHost)
        {
            webHost.UseStartup<Startup>();
        }
    }
}
