using System.IO;
using CAC.Basic.Application.TaskLists;
using CAC.Basic.Infrastructure.TaskLists;
using CAC.Core.Infrastructure;
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

        private static readonly DirectoryInfo StorageDir = new DirectoryInfo(Path.Join(TestContext.CurrentContext.TestDirectory, nameof(FileSystemTaskListRepositoryTests)));

        protected override ITaskListRepository Testee { get; } = new FileSystemTaskListRepository(Options.Create(new FileSystemStoragePersistenceOptions { BaseDir = StorageDir.FullName }));
    }
}
