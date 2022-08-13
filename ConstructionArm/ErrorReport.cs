using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class ErrorReport
        {
            public enum SeverityType
            {
                CRITICAL,
                WARNING,
                NONE
            };

            public readonly SeverityType Severity;
            public readonly string Message;

            private ErrorReport(SeverityType severity, string message)
            {
                this.Severity = severity;
                this.Message = message;
            }

            public bool RequiresEmergencyStop
            {
                get
                {
                    return this.Severity == SeverityType.CRITICAL;
                }
            }

            public static ErrorReport critical(string message)
            {
                return new ErrorReport(SeverityType.CRITICAL, message);
            }
            public static ErrorReport warning(string message)
            {
                return new ErrorReport(SeverityType.WARNING, message);
            }

            public static ErrorReport merge(params ErrorReport[] errorReports)
            {
                SeverityType severity = SeverityType.NONE;
                string message = "";
                foreach(ErrorReport report in errorReports)
                {
                    if (report.Severity == SeverityType.NONE)
                    {
                        continue;
                    }
                    if (severityAsNumber(report.Severity) > severityAsNumber(severity))
                    {
                        severity = report.Severity;
                    }
                    if (!String.IsNullOrEmpty(message))
                    {
                        message += "/" + report.Message;
                    } else
                    {
                        message = report.Message;
                    }
                }
                if (severity == SeverityType.NONE)
                {
                    return NONE;
                }
                return new ErrorReport(severity, message);
            }

            private static int severityAsNumber(SeverityType severity)
            {
                switch(severity)
                {
                    case SeverityType.CRITICAL:
                        return 2;
                    case SeverityType.WARNING:
                        return 1;
                    case SeverityType.NONE:
                        return 0;
                    default:
                        return -1;
                }
            }
            

            public static readonly ErrorReport NONE = new ErrorReport(SeverityType.NONE, "");
        }
    }
}
