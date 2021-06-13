using System.Runtime.CompilerServices;
using CAC.Core.Application;
using CAC.Core.Domain;
using CAC.CQS.Application.TaskLists;
using CAC.CQS.Application.TaskLists.AddTaskToList;
using CAC.CQS.Application.TaskLists.CreateNewTaskList;
using CAC.CQS.Application.TaskLists.DeleteTaskList;
using CAC.CQS.Application.TaskLists.GetAllTaskLists;
using CAC.CQS.Application.TaskLists.GetAllTaskListsWithPendingEntries;
using CAC.CQS.Application.TaskLists.GetTaskListByIdQuery;
using CAC.CQS.Application.TaskLists.MarkTaskAsDone;
using CAC.CQS.Application.TaskLists.SendTaskListReminders;
using CAC.CQS.Domain.TaskListAggregate;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.CQS.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.CQS.Application
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddCommandHandler<CreateNewTaskListCommandHandler>();
            services.AddCommandHandler<AddTaskToListCommandHandler>();
            services.AddCommandHandler<MarkTaskAsDoneCommandHandler>();
            services.AddCommandHandler<DeleteTaskListCommandHandler>();
            services.AddCommandHandler<SendTaskListRemindersCommandHandler>();
            
            services.AddQueryHandler<GetAllTaskListsQueryHandler>();
            services.AddQueryHandler<GetTaskListByIdQueryHandler>();
            services.AddQueryHandler<GetAllTaskListsWithPendingEntriesQueryHandler>();

            services.AddDomainEventPublisher();
            services.AddDomainEventHandler<TaskListStatisticsDomainEventHandler>();
            services.AddDomainEventHandler<TaskListNotificationDomainEventHandler>();

            typeof(TaskListId).Assembly.AddEntityIdTypeConverterAttributes();
        }
    }
}
