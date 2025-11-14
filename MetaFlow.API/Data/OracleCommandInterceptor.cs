using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace MetaFlow.API.Data
{
    public class OracleCommandInterceptor : DbCommandInterceptor
    {
        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            FixOracleBooleanQueries(command);
            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            FixOracleBooleanQueries(command);
            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        private static void FixOracleBooleanQueries(DbCommand command)
        {
            command.CommandText = command.CommandText
                .Replace(" = True", " = 1")
                .Replace(" = False", " = 0")
                .Replace(" True", " 1")
                .Replace(" False", " 0")
                .Replace("(True)", "(1)")
                .Replace("(False)", "(0)")
                .Replace("IS True", "= 1")
                .Replace("IS False", "= 0")
                .Replace("IS NOT True", "!= 1")
                .Replace("IS NOT False", "!= 0");
        }
    }
}