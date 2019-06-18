using System;
using System.Collections.Generic;
using System.Text;

namespace CassandraBulkUpdater.Base.Contracts.Helpers
{
    interface ICommandlineArgsHelper
    {
        TValue GetCommandlineArgumentValue<TValue>(string commandlineArgumentName, bool required = true);
        TValue GetCommandlineArgumentValue<TValue>(string commandlineArgumentName, bool required = true, params TValue[] options);

        string GetCommandlineUsageText();
    }
}
