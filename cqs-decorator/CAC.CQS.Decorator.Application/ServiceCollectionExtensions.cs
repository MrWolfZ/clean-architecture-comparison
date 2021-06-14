using System.Runtime.CompilerServices;
using CAC.Core.Application;
using CAC.Core.Domain;
using CAC.CQS.Decorator.Application.TaskLists;
using CAC.CQS.Decorator.Application.TaskLists.AddTaskToList;
using CAC.CQS.Decorator.Application.TaskLists.CreateNewTaskList;
using CAC.CQS.Decorator.Application.TaskLists.DeleteTaskList;
using CAC.CQS.Decorator.Application.TaskLists.GetAllTaskLists;
using CAC.CQS.Decorator.Application.TaskLists.GetAllTaskListsWithPendingEntries;
using CAC.CQS.Decorator.Application.TaskLists.GetTaskListByIdQuery;
using CAC.CQS.Decorator.Application.TaskLists.MarkTaskAsDone;
using CAC.CQS.Decorator.Application.TaskLists.SendTaskListReminders;
using CAC.CQS.Decorator.Domain.TaskListAggregate;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("CAC.CQS.Decorator.UnitTests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace CAC.CQS.Decorator.Application
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
