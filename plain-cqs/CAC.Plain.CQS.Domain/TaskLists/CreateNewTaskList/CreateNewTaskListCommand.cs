﻿using System.ComponentModel.DataAnnotations;

namespace CAC.Plain.CQS.Domain.TaskLists.CreateNewTaskList
{
    public sealed record CreateNewTaskListCommand
    {
        /// <example>my task list</example>
        [Required]
        public string Name { get; init; } = string.Empty;
    }
}
