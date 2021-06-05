using System.IO;
using CAC.Baseline.Web.Data;
using CAC.Core.Infrastructure;
using CAC.Core.TestUtilities;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace CAC.Baseline.UnitTests.Infrastructure
{
    [TestFixture]
    [IntegrationTest]
    [Parallelizable(ParallelScope.None)]
    public sealed class FileSystemTaskListEntryRepositoryTests : TaskListEntryRepositoryTests
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

        protected override ITaskListEntryRepository Testee { get; } = new FileSystemTaskListEntryRepository(Options.Create(new FileSystemStoragePersistenceOptions { BaseDir = StorageDir.FullName }));
    }
}