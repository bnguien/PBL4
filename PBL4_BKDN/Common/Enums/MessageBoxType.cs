using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Enums
{
    public enum MessageBoxButtonType
    {
        OK,
        OKCancel,
        YesNo,
        YesNoCancel,
        RetryCancel,
        AbortRetryIgnore
    }

    public enum MessageBoxIconType
    {
        None,
        Information,
        Warning,
        Error,
        Question
    }
}
