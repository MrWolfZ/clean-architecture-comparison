using System.ComponentModel.DataAnnotations;
using CAC.CQS.MediatR.Domain.UserAggregate;
using MediatR;

namespace CAC.CQS.MediatR.Application.TaskLists.CreateNewTaskList
{
    public sealed record CreateNewTaskListCommand : IRequest<CreateNewTaskListCommandResponse>
    {
        public const int MaxTaskListNameLength = 64;

        [Required]
        public UserId OwnerId { get; init; } = default!;

        /// <example>my task list</example>
        [Required]
        [MaxLength(MaxTaskListNameLength)]
        public string Name { get; init; } = string.Empty;
    }
}
