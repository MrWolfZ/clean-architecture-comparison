using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CAC.Core.TestUtilities;
using CAC.Plain.CQS.Domain.TaskLists;
using CAC.Plain.CQS.Domain.TaskLists.AddTaskToList;
using CAC.Plain.CQS.Domain.TaskLists.CreateNewTaskList;
using CAC.Plain.CQS.Domain.TaskLists.GetTaskListById;
using CAC.Plain.CQS.Domain.TaskLists.MarkTaskAsDone;
using CAC.Plain.CQS.Web;
using Microsoft.AspNetCore.Hosting;
using NUnit.Framework;

namespace CAC.Plain.CQS.UnitTests.Web.TaskLists
{
    [IntegrationTest]
    public sealed class TaskListControllerTests : ControllerTestBase
    {
        private ITaskListRepository TaskListRepository => Resolve<ITaskListRepository>();

        [Test]
        public async Task CreateNewTaskList_GivenValidName_ReturnsTaskListId()
        {
            var expectedResponse = new CreateNewTaskListCommandResponse(1);

            var command = new CreateNewTaskListCommand { Name = "test" };
            var response = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", command, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseContent = Deserialize<CreateNewTaskListCommandResponse>(responseString);

            Assert.AreEqual(expectedResponse, responseContent);
        }

        [Test]
        public async Task CreateNewTaskList_GivenInvalidName_ReturnsBadRequest()
        {
            var command = new CreateNewTaskListCommand { Name = string.Empty };
            var response = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", command, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task CreateNewTaskList_GivenDuplicateName_ReturnsConflict()
        {
            var taskList = TaskList.New(99, "test");

            await TaskListRepository.Upsert(taskList);

            var command = new CreateNewTaskListCommand { Name = taskList.Name };
            var response = await HttpClient.PostAsJsonAsync("taskLists/createNewTaskList", command, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndValidDescription_ReturnsNoContent()
        {
            var taskList = TaskList.New(1, "test");

            await TaskListRepository.Upsert(taskList);

            var command = new AddTaskToListCommand(taskList.Id) { TaskDescription = "task" };
            var response = await HttpClient.PostAsJsonAsync("taskLists/addTaskToList", command, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task AddTaskToList_GivenExistingTaskListIdAndInvalidDescription_ReturnsBadRequest()
        {
            var taskList = TaskList.New(1, "test");

            await TaskListRepository.Upsert(taskList);

            var command = new AddTaskToListCommand(taskList.Id) { TaskDescription = string.Empty };
            var response = await HttpClient.PostAsJsonAsync("taskLists/addTaskToList", command, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.BadRequest);
        }

        [Test]
        public async Task AddTaskToList_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var id = TaskListId.Of(1);

            var command = new AddTaskToListCommand(id) { TaskDescription = "task" };
            var response = await HttpClient.PostAsJsonAsync("taskLists/addTaskToList", command, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndValidItemIndex_ReturnsNoContent()
        {
            var taskList = TaskList.New(1, "test").AddItem("task 1").AddItem("task 2");

            await TaskListRepository.Upsert(taskList);

            var command = new MarkTaskAsDoneCommand(taskList.Id) { ItemIdx = 1 };
            var response = await HttpClient.PostAsJsonAsync("taskLists/markTaskAsDone", command, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.NoContent);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenExistingTaskListIdAndNonExistingItemIndex_ReturnsConflict()
        {
            var taskList = TaskList.New(1, "test").AddItem("task 1").AddItem("task 2");

            await TaskListRepository.Upsert(taskList);

            var command = new MarkTaskAsDoneCommand(taskList.Id) { ItemIdx = 2 };
            var response = await HttpClient.PostAsJsonAsync("taskLists/markTaskAsDone", command, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.Conflict);
        }

        [Test]
        public async Task MarkTaskAsDone_GivenNonExistingTaskListId_ReturnsNotFound()
        {
            var id = TaskListId.Of(1);

            var command = new MarkTaskAsDoneCommand(id) { ItemIdx = 1 };
            var response = await HttpClient.PostAsJsonAsync("taskLists/markTaskAsDone", command, JsonSerializerOptions);

            await response.AssertStatusCode(HttpStatusCode.NotFound);
        }

        [Test]
        public async Task GetTaskListById_GivenExistingTaskListId_ReturnsTaskList()
        {
            var taskList = TaskList.New(1, "test").AddItem("task 1").AddItem("task 2");
            var expectedResponse = new GetTaskListByIdQueryResponse(taskList);

            await TaskListRepository.Upsert(taskList);

            var response = await HttpClient.GetAsync($"taskLists/{taskList.Id}");

            await response.AssertStatusCode(HttpStatusCode.OK);

            var responseString = await response.Content.ReadAsStringAsync();
            var responseContent = Deserialize<GetTaskListByIdQueryResponse>(responseString);

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
