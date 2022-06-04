namespace UGameCore.Utilities
{

    public class CmdLineUtils
    {
        
        public static string[] GetCmdLineArgs()
        {
            try
            {
                string[] commandLineArgs = System.Environment.GetCommandLineArgs();
                if (commandLineArgs != null)
                    return commandLineArgs;
            }
            catch (System.Exception) {}
            
            return new string[0];
        }

        public static string GetStringArgument(string argName)
        {
            string[] commandLineArgs = GetCmdLineArgs();

            if (commandLineArgs.Length < 2) // first argument is program path
                throw new System.ArgumentException($"Command line argument '{argName}' not found");

            string search = "-" + argName + ":";
            string foundArg = System.Array.Find(commandLineArgs, arg => arg.StartsWith(search));
            if (null == foundArg)
                throw new System.ArgumentException($"Command line argument '{argName}' not found");

            // found specified argument
            // extract value

            return foundArg.Substring(search.Length);
        }

        public static bool TryGetStringArgument(string argName, out string value)
        {
            try
            {
                value = GetStringArgument(argName);
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        [System.Obsolete]
        public static bool GetArgument(string argName, ref string argValue)
        {
            string value;
            if (TryGetStringArgument(argName, out value))
            {
                argValue = value;
                return true;
            }
            
            return false;
        }

        [System.Obsolete]
        public static bool GetUshortArgument(string argName, ref ushort argValue)
        {
            if (TryGetUshortArgument(argName, out ushort value))
            {
                argValue = value;
                return true;
            }

            return false;
        }

        public static bool TryGetUshortArgument(string argName, out ushort argValue)
        {
            argValue = 0;
            return TryGetStringArgument(argName, out string stringValue)
                && ushort.TryParse(stringValue, out argValue);
        }

        public static bool HasArgument(string argName)
        {
            return TryGetStringArgument(argName, out _);
        }
    }

}
