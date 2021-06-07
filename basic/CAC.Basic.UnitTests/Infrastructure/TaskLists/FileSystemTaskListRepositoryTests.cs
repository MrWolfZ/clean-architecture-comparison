using System.IO;
using CAC.Basic.Application.TaskLists;
using CAC.Basic.Infrastructure.TaskLists;
using CAC.Core.Infrastructure.Persistence;
using CAC.Core.TestUtilities;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace CAC.Basic.UnitTests.Infrastructure.TaskLists
{
    [TestFixture]
    [IntegrationTest]
    [Parallelizable(ParallelScope.None)]
    public sealed class FileSystemTaskListRepositoryTests : TaskListRepositoryTests
    {
        [SetUp]
        public void SetUp()
        {
            StorageDir.Create();
        }

        [TearDown]
        public void TearDown()
        {
            StorageDir.Delete(true);
        }

        private static readonly DirectoryInfo StorageDir = new(Path.Join(TestContext.CurrentContext.TestDirectory, nameof(FileSystemTaskListRepositoryTests)));

        public FileSystemTaskListRepositoryTests()
        {
            Testee = new FileSystemTaskListRepository(Options.Create(new FileSystemStoragePersistenceOptions { BaseDir = StorageDir.FullName }), DomainEventPublisher);
        }

        protected override ITaskListRepository Testee { get; }
    }
}
