

using Hfs.Server.Core.Common;
using Hfs.Server.Core.Vfs;
using System.Threading.Tasks;

namespace Hfs.Server.HfsCore.Commands
{
    public class CommandCleanVfs : CommandBaseAdmin
    {
        public override string ActionKey { get; } = @"cleanvfs";
        public override VfsAction Action { get; } = VfsAction.Write;

        async protected override Task CommandExecute()
        {
            Utility.CleanDirectory(HfsData.TempDirUserFiles, @"*", DateTime.MaxValue, true, 0);
            Utility.CleanDirectory(HfsData.TempDirSharedFiles, @"*", DateTime.MaxValue, true, 0);
            Utility.CleanDirectory(HfsData.TempDirRemoteFiles, @"*", DateTime.MaxValue, true, 0);
        }
    }
}